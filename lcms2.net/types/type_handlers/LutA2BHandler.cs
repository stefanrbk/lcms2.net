using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class LutA2BHandler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion => 0;

    public object? Duplicate(object value, int num) =>
        (value as Pipeline)?.Clone();

    public void Free(object value) =>
        (value as Pipeline)?.Dispose();

    public object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        unsafe {
            var baseOffset = io.Tell() - sizeof(TagBase);

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
            var newLut = Pipeline.Alloc(Context, inputChan, outputChan);
            if (newLut is null) return null;

            if (offsetA is not 0 && !newLut.InsertStage(StageLoc.AtEnd, io.ReadSetOfCurves(Context, (uint)baseOffset + offsetA, inputChan)))
                goto Error;

            if (offsetC is not 0 && !newLut.InsertStage(StageLoc.AtEnd, io.ReadClut(Context, (uint)baseOffset + offsetC, inputChan, outputChan)))
                goto Error;

            if (offsetM is not 0 && !newLut.InsertStage(StageLoc.AtEnd, io.ReadSetOfCurves(Context, (uint)baseOffset + offsetM, inputChan)))
                goto Error;

            if (offsetMat is not 0 && !newLut.InsertStage(StageLoc.AtEnd, io.ReadMatrix(Context, (uint)baseOffset + offsetM)))
                goto Error;

            if (offsetB is not 0 && !newLut.InsertStage(StageLoc.AtEnd, io.ReadSetOfCurves(Context, (uint)baseOffset + offsetB, outputChan)))
                goto Error;

            numItems = 1;
            return newLut;

        Error:
            newLut?.Dispose();
            return null;
        }
    }

    public bool Write(Stream io, object value, int numItems)
    {
        unsafe {

            Stage? a = null, clut = null, m = null, matrix = null, b = null;
            long offsetA = 0, offsetClut = 0, offsetM = 0, offsetMatrix = 0, offsetB = 0;

            var lut = (Pipeline)value;

            var baseOffset = io.Tell() - sizeof(TagBase);

            if (lut.Elements is not null &&
                !lut.CheckAndRetreiveStages(out a, out clut, out m, out matrix, out b)) {

                Context.SignalError(Context, ErrorCode.NotSuitable, "Lut is not suitable to be saved as LutAToB");
                return false;
            }

            // Get input, output channels
            var inputChan = lut.InputChannels;
            var outputChan = lut.OutputChannels;

            // Write channel count
            if (!io.Write((byte)inputChan)) return false;
            if (!io.Write((byte)outputChan)) return false;
            if (!io.Write((ushort)0)) return false;

            // Keep directory to be filled later
            var dirPos = io.Tell();

            // Write the directory
            for (var i = 0; i < 5; i++)
                if (!io.Write((uint)0)) return false;

            if (a is not null) {

                offsetA = io.Tell() - baseOffset;
                if (!io.WriteSetOfCurves(Context, Signature.TagType.ParametricCurve, a)) return false;
            }

            if (clut is not null) {

                offsetClut = io.Tell() - baseOffset;
                if (!io.WriteClut(Context, lut.SaveAs8Bits ? (byte)1 : (byte)2, clut)) return false;
            }

            if (m is not null) {

                offsetM = io.Tell() - baseOffset;
                if (!io.WriteSetOfCurves(Context, Signature.TagType.ParametricCurve, m)) return false;
            }

            if (matrix is not null) {

                offsetMatrix = io.Tell() - baseOffset;
                if (!io.WriteMatrix(matrix)) return false;
            }

            if (b is not null) {

                offsetB = io.Tell() - baseOffset;
                if (!io.WriteSetOfCurves(Context, Signature.TagType.ParametricCurve, b)) return false;
            }

            var curPos = io.Tell();

            if (io.Seek(dirPos, SeekOrigin.Begin) != dirPos) return false;

            if (!io.Write((uint)offsetB)) return false;
            if (!io.Write((uint)offsetMatrix)) return false;
            if (!io.Write((uint)offsetM)) return false;
            if (!io.Write((uint)offsetClut)) return false;
            if (!io.Write((uint)offsetB)) return false;

            if (io.Seek(curPos, SeekOrigin.Begin) != curPos) return false;

            return true;
        }
    }
}
