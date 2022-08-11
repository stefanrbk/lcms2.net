using lcms2.io;
using lcms2.plugins;
using lcms2.state;

using static lcms2.Helpers;

namespace lcms2.types.type_handlers;
public class Lut16Handler : ITagTypeHandler
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
        Pipeline? newLut = null;
        var matrix = new double[9];

        numItems = 0;

        if (!io.ReadUInt8Number(out var inputChannels)) goto Error;
        if (!io.ReadUInt8Number(out var outputChannels)) goto Error;
        if (!io.ReadUInt8Number(out var clutPoints)) goto Error; // 255 maximum

        // Padding
        if (!io.ReadUInt8Number(out _)) goto Error;

        // Do some checking
        if (inputChannels == 0 || inputChannels > Lcms2.MaxChannels) goto Error;
        if (outputChannels == 0 || outputChannels > Lcms2.MaxChannels) goto Error;

        // Allocates an empty Pipeline
        newLut = Pipeline.Alloc(Context, inputChannels, outputChannels);
        if (newLut is null) goto Error;

        // Read the Matrix
        for (var i = 0; i < 9; i++)
            if (!io.Read15Fixed16Number(out matrix[i])) goto Error;

        // Only operates on 3 channels
        if ((inputChannels == 3) && !((Mat3)matrix).IsIdentity) {
            if (!newLut.InsertStage(StageLoc.AtEnd, Stage.AllocMatrix(Context, 3, 3, in matrix, null)))
                goto Error;
        }

        if (!io.ReadUInt16Number(out var inputEntries)) goto Error;
        if (!io.ReadUInt16Number(out var outputEntries)) goto Error;

        if (inputEntries > 0x7FFF || outputEntries > 0x7FFF) goto Error;
        if (clutPoints == 1) goto Error; // Impossible value, 0 for not CLUT and at least 2 for anything else

        // Get input tables
        if (!io.Read16bitTables(Context, ref newLut, inputChannels, inputEntries)) goto Error;

        // Get 3D CLUT. Check the overflow...
        var numTabSize = Uipow(outputChannels, clutPoints, inputChannels);
        if (numTabSize == unchecked((uint)-1)) goto Error;
        if (numTabSize > 0) {
            if (!io.ReadUInt16Array((int)numTabSize, out var t)) goto Error;

            if (!newLut.InsertStage(StageLoc.AtEnd, Stage.AllocCLut16bit(Context, clutPoints, inputChannels, outputChannels, in t)))
                goto Error;
        }

        // Get output tables
        if (!io.Read16bitTables(Context, ref newLut, outputChannels, outputEntries)) goto Error;

        numItems = 1;
        return newLut;

    Error:
        if (newLut is not null) newLut.Dispose();
        return null;
    }

    public bool Write(Stream io, object value, int numItems)
    {
        Stage.ToneCurveData? preMpe = null, postMpe = null;
        Stage.MatrixData? matMpe = null;
        Stage.CLutData? clut = null;

        var newLut = (Pipeline)value;
        var mpe = newLut.Elements;
        if (mpe is not null && mpe.Type == Signature.Stage.MatrixElem) {
            if (mpe.InputChannels != 3 || mpe.OutputChannels != 3 || mpe.Data is null) return false;
            matMpe = (Stage.MatrixData)mpe.Data;
            mpe = mpe.Next;
        }

        if (mpe is not null && mpe.Type == Signature.Stage.CurveSetElem) {
            if (mpe.Data is null) return false;
            preMpe = (Stage.ToneCurveData)mpe.Data;
            mpe = mpe.Next;
        }

        if (mpe is not null && mpe.Type == Signature.Stage.CLutElem) {
            if (mpe.Data is null) return false;
            clut = (Stage.CLutData)mpe.Data;
            mpe = mpe.Next;
        }

        if (mpe is not null && mpe.Type == Signature.Stage.CurveSetElem) {
            if (mpe.Data is null) return false;
            postMpe = (Stage.ToneCurveData)mpe.Data;
            mpe = mpe.Next;
        }

        // That should be all
        if (mpe is not null) {
            Context.SignalError(Context, ErrorCode.UnknownExtension, "LUT is not suitable to be saved as LUT16");
            return false;
        }

        var clutPoints = clut?.Params[0].NumSamples[0] ?? 0;

        if (!io.Write((byte)newLut.InputChannels)) return false;
        if (!io.Write((byte)newLut.OutputChannels)) return false;
        if (!io.Write((byte)clutPoints)) return false;
        if (!io.Write((byte)0)) return false; // Padding

        if (matMpe is not null) {
            for (var i = 0; i < 9; i++) {
                if (!io.Write(matMpe.Double[i])) return false;
            }
        } else {
            var ident = (double[])Mat3.Identity;
            for (var i = 0; i < 9; i++) {
                if (!io.Write(ident[i])) return false;
            }
        }

        if (preMpe is not null) {
            if (!io.Write((ushort)preMpe.TheCurves[0].NumEntries)) return false;
        } else {
            if (!io.Write((ushort)2)) return false;
        }

        if (postMpe is not null) {
            if (!io.Write((ushort)postMpe.TheCurves[0].NumEntries)) return false;
        } else {
            if (!io.Write((ushort)2)) return false;
        }

        // The prelinearization table
        if (preMpe is not null) {
            if (!io.Write16bitTables(ref preMpe)) return false;
        } else {
            for (var i = 0; i < newLut.InputChannels; i++) {
                if (!io.Write((ushort)0)) return false;
                if (!io.Write((ushort)0xFFFF)) return false;
            }
        }

        var numTabSize = Uipow(newLut.OutputChannels, clutPoints, newLut.InputChannels);
        if (numTabSize == unchecked((uint)-1)) return false;
        if (numTabSize > 0) {
            // The 3D CLUT.
            if (clut is not null && !io.Write((int)numTabSize, clut.Table.T))
                return false;
        }

        // The postlinearization table
        if (postMpe is not null) {
            if (!io.Write16bitTables(ref postMpe)) return false;
        } else {
            for (var i = 0; i < newLut.OutputChannels; i++) {
                if (!io.Write((ushort)0)) return false;
                if (!io.Write((ushort)0xFFFF)) return false;
            }
        }

        return true;
    }
}
