using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;

public class LutB2AHandler: TagTypeHandler
{
    public LutB2AHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public LutB2AHandler(object? state = null)
        : this(default, state) { }

    public override object? Duplicate(object value, int num) =>
        (value as Pipeline)?.Clone();

    public override void Free(object value) =>
        (value as Pipeline)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        var baseOffset = io.Tell() - TagBase.SizeOf;

        if (!io.ReadUInt8Number(out var inputChan)) return null;
        if (!io.ReadUInt8Number(out var outputChan)) return null;

        if (!io.ReadUInt16Number(out _)) return null;

        if (!io.ReadUInt32Number(out var offsetB)) return null;
        if (!io.ReadUInt32Number(out var offsetMat)) return null;
        if (!io.ReadUInt32Number(out var offsetM)) return null;
        if (!io.ReadUInt32Number(out var offsetC)) return null;
        if (!io.ReadUInt32Number(out var offsetA)) return null;

        if (inputChan is 0 or >= Lcms2.MaxChannels) return null;
        if (outputChan is 0 or >= Lcms2.MaxChannels) return null;

        // Allocates an empty LUT
        var newLut = Pipeline.Alloc(StateContainer, inputChan, outputChan);
        if (newLut is null) return null;

        if (offsetB is not 0 && !newLut.InsertStage(StageLoc.AtEnd, ReadSetOfCurves(io, (uint)baseOffset + offsetB, outputChan)))
            goto Error;

        if (offsetC is not 0 && !newLut.InsertStage(StageLoc.AtEnd, ReadClut(io, (uint)baseOffset + offsetC, inputChan, outputChan)))
            goto Error;

        if (offsetM is not 0 && !newLut.InsertStage(StageLoc.AtEnd, ReadSetOfCurves(io, (uint)baseOffset + offsetM, inputChan)))
            goto Error;

        if (offsetMat is not 0 && !newLut.InsertStage(StageLoc.AtEnd, ReadMatrix(io, (uint)baseOffset + offsetM)))
            goto Error;

        if (offsetA is not 0 && !newLut.InsertStage(StageLoc.AtEnd, ReadSetOfCurves(io, (uint)baseOffset + offsetA, inputChan)))
            goto Error;

        numItems = 1;
        return newLut;

    Error:
        newLut?.Dispose();
        return null;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        Stage? a = null, clut = null, m = null, matrix = null, b = null;
        long offsetA = 0, offsetClut = 0, offsetM = 0, offsetMatrix = 0, offsetB = 0;

        var lut = (Pipeline)value;

        var baseOffset = io.Tell() - TagBase.SizeOf;

        if (lut.elements is not null &&
            !lut.CheckAndRetrieveStagesBtoA(out b, out matrix, out m, out clut, out a))
        {
            State.SignalError(StateContainer, ErrorCode.NotSuitable, "Lut is not suitable to be saved as LutBToA");
            return false;
        }

        // Get input, output channels
        var inputChan = lut.inputChannels;
        var outputChan = lut.outputChannels;

        // Write channel count
        if (!io.Write((byte)inputChan)) return false;
        if (!io.Write((byte)outputChan)) return false;
        if (!io.Write((ushort)0)) return false;

        // Keep directory to be filled later
        var dirPos = io.Tell();

        // Write the directory
        for (var i = 0; i < 5; i++)
            if (!io.Write((uint)0)) return false;

        if (a is not null)
        {
            offsetA = io.Tell() - baseOffset;
            if (!WriteSetOfCurves(io, Signature.TagType.ParametricCurve, a)) return false;
        }

        if (clut is not null)
        {
            offsetClut = io.Tell() - baseOffset;
            if (!WriteClut(io, lut.saveAs8Bits ? (byte)1 : (byte)2, clut)) return false;
        }

        if (m is not null)
        {
            offsetM = io.Tell() - baseOffset;
            if (!WriteSetOfCurves(io, Signature.TagType.ParametricCurve, m)) return false;
        }

        if (matrix is not null)
        {
            offsetMatrix = io.Tell() - baseOffset;
            if (!WriteMatrix(io, matrix)) return false;
        }

        if (b is not null)
        {
            offsetB = io.Tell() - baseOffset;
            if (!WriteSetOfCurves(io, Signature.TagType.ParametricCurve, b)) return false;
        }

        var curPos = io.Tell();

        if (io.Seek(dirPos, SeekOrigin.Begin) != dirPos) return false;

        if (!io.Write((uint)offsetB)) return false;
        if (!io.Write((uint)offsetMatrix)) return false;
        if (!io.Write((uint)offsetM)) return false;
        if (!io.Write((uint)offsetClut)) return false;
        if (!io.Write((uint)offsetA)) return false;

        if (io.Seek(curPos, SeekOrigin.Begin) != curPos) return false;

        return true;
    }
}
