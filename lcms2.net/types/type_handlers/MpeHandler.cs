using lcms2.io;
using lcms2.plugins;
using lcms2.state;

using static lcms2.Lcms2;

namespace lcms2.types.type_handlers;
public class MpeHandler : TagTypeHandler
{
    public MpeHandler(Context? context = null)
        : base(default, context, 0) { }

    public override object? Duplicate(object value, int num) =>
        (value as Pipeline)?.Clone();

    public override void Free(object value) =>
        (value as Pipeline)?.Dispose();

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        // Get actual position as a basis for element offset
        var baseOffset = (uint)(io.Tell() - sizeof(TagBase));

        // Read channels and element count
        if (!io.ReadUInt16Number(out var inputChans)) return null;
        if (!io.ReadUInt16Number(out var outputChans)) return null;

        if ((inputChans is 0 or >= MaxChannels) ||
            (outputChans is 0 or >= MaxChannels)) return null;

        // Allocate an empty LUT
        object? newLut = Pipeline.Alloc(Context, inputChans, outputChans);
        if (newLut is null) return null;

        if (!io.ReadUInt32Number(out var elementCount)) goto Error;
        if (!ReadPositionTable(io, (int)elementCount, baseOffset, ref newLut, ReadMpeElem)) goto Error;

        // Check channel count
        if (inputChans != ((Pipeline)newLut).InputChannels ||
            outputChans != ((Pipeline)newLut).OutputChannels) goto Error;

        // Success
        numItems = 1;
        return newLut;

    Error:

        ((Pipeline)newLut)?.Dispose();
        return null;
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        var lut = (Pipeline)value;
        var elem = lut.Elements;
        var mpeChunk = Context.GetMultiProcessElementPlugin(Context);

        var baseOffset = (uint)(io.Tell() - sizeof(TagBase));

        var inputChan = lut.InputChannels;
        var outputChan = lut.OutputChannels;
        var elemCount = lut.StageCount;

        var elementOffsets = new uint[elemCount];
        var elementSizes = new uint[elemCount];

        // Write the head
        if (!io.Write((ushort)inputChan)) return false;
        if (!io.Write((ushort)outputChan)) return false;
        if (!io.Write(elemCount)) return false;

        var dirPos = io.Tell();

        // Write a face directory to be filled later on
        for (var i = 0; i < elemCount; i++) {
            if (!io.Write((uint)0)) return false;   // Offset
            if (!io.Write((uint)0)) return false;   // Size
        }

        // Write each single tag. Keep track of the size as well.
        for (var i = 0; i < elemCount; i++) {

            elementOffsets[i] = (uint)io.Tell() - baseOffset;

            var elementSig = elem!.Type;

            var typeHandler = GetHandler(elementSig, mpeChunk.tagTypes);
            if (typeHandler is null) {
                Context.SignalError(Context, ErrorCode.UnknownExtension, "Found unknown MPE type '{0}'", elementSig);
                return false;
            }

            if (!io.Write((uint)elementSig)) return false;
            if (!io.Write((uint)0)) return false;
            var before = (uint)io.Tell();
            if (!typeHandler.Write(io, elem, 1)) return false;
            if (!io.WriteAlignment()) return false;

            elementSizes[i] = (uint)io.Tell() - before;

            elem = elem.Next;
        }

        // Write the directory
        var curPos = (uint)io.Tell();

        if (io.Seek(dirPos, SeekOrigin.Begin) != dirPos) return false;

        for (var i = 0; i < elemCount; i++) {
            if (!io.Write(elementOffsets[i])) return false;
            if (!io.Write(elementSizes[i])) return false;
        }

        if (io.Seek(curPos, SeekOrigin.Begin) != curPos) return false;

        return true;
    }
}
