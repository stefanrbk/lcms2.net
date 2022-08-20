using lcms2.io;
using lcms2.plugins;
using lcms2.state;

using static lcms2.Helpers;

namespace lcms2.types.type_handlers;

public class Lut8Handler: TagTypeHandler
{
    public Lut8Handler(Signature sig, Context? context = null)
        : base(sig, context, 0) { }

    public Lut8Handler(Context? context = null)
        : this(default, context) { }

    public override object? Duplicate(object value, int num) =>
        (value as Pipeline)?.Clone();

    public override void Free(object value) =>
        (value as Pipeline)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        Pipeline? newLut = null;
        var matrix = new double[9];

        numItems = 0;

        if (!io.ReadUInt8Number(out var inputChannels)) goto Error;
        if (!io.ReadUInt8Number(out var outputChannels)) goto Error;
        if (!io.ReadUInt8Number(out var clutPoints)) goto Error;

        if (clutPoints == 1) goto Error; // Impossible value, 0 for not CLUT and at least 2 for anything else

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

        // Only operates if not identity...
        if ((inputChannels == 3) && !((Mat3)matrix).IsIdentity)
        {
            if (!newLut.InsertStage(StageLoc.AtBegin, Stage.AllocMatrix(Context, 3, 3, in matrix, null)))
                goto Error;
        }

        // Get input tables
        if (!Read8bitTables(io, ref newLut, inputChannels)) goto Error;

        // Get 3D CLUT. Check the overflow...
        var numTabSize = Uipow(outputChannels, clutPoints, inputChannels);
        if (numTabSize == unchecked((uint)-1)) goto Error;
        if (numTabSize > 0)
        {
            var t = new ushort[numTabSize];
            var ptrW = t.AsSpan();

            var temp = new byte[numTabSize];

            if (io.Read(temp) != numTabSize) goto Error;

            for (var i = 0; i < numTabSize; i++)
            {
                ptrW[0] = From8to16(temp[i]);
                ptrW = ptrW[1..];
            }

            if (!newLut.InsertStage(StageLoc.AtEnd, Stage.AllocCLut16bit(Context, clutPoints, inputChannels, outputChannels, in t)))
                goto Error;
        }

        // Get output tables
        if (!Read8bitTables(io, ref newLut, outputChannels)) goto Error;

        numItems = 1;
        return newLut;

    Error:
        if (newLut is not null) newLut.Dispose();
        return null;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        Stage.ToneCurveData? preMpe = null, postMpe = null;
        Stage.MatrixData? matMpe = null;
        Stage.CLutData? clut = null;

        var newLut = (Pipeline)value;
        var mpe = newLut.elements;
        if (mpe is null) return false;
        if (mpe.type == Signature.Stage.MatrixElem)
        {
            if (mpe.inputChannels != 3 || mpe.outputChannels != 3 || mpe.data is null) return false;
            matMpe = (Stage.MatrixData)mpe.data;
            mpe = mpe.next;
        }

        if (mpe is not null && mpe.type == Signature.Stage.CurveSetElem)
        {
            if (mpe.data is null) return false;
            preMpe = (Stage.ToneCurveData)mpe.data;
            mpe = mpe.next;
        }

        if (mpe is not null && mpe.type == Signature.Stage.CLutElem)
        {
            if (mpe.data is null) return false;
            clut = (Stage.CLutData)mpe.data;
            mpe = mpe.next;
        }

        if (mpe is not null && mpe.type == Signature.Stage.CurveSetElem)
        {
            if (mpe.data is null) return false;
            postMpe = (Stage.ToneCurveData)mpe.data;
            mpe = mpe.next;
        }

        // That should be all
        if (mpe is not null)
        {
            Context.SignalError(Context, ErrorCode.UnknownExtension, "LUT is not suitable to be saved as LUT8");
            return false;
        }

        var clutPoints = (uint)(clut?.Params[0].NumSamples[0] ?? 0);

        if (!io.Write((byte)newLut.inputChannels)) return false;
        if (!io.Write((byte)newLut.outputChannels)) return false;
        if (!io.Write((byte)clutPoints)) return false;
        if (!io.Write((byte)0)) return false; // Padding

        if (matMpe is not null)
        {
            for (var i = 0; i < 9; i++)
            {
                if (!io.Write(matMpe.Double[i])) return false;
            }
        } else
        {
            var ident = (double[])Mat3.Identity;
            for (var i = 0; i < 9; i++)
            {
                if (!io.Write(ident[i])) return false;
            }
        }

        // The prelinearization table
        if (preMpe is not null && !Write8bitTables(io, newLut.inputChannels, ref preMpe))
            return false;

        var numTabSize = Uipow(newLut.outputChannels, clutPoints, newLut.inputChannels);
        if (numTabSize == unchecked((uint)-1)) return false;
        if (numTabSize > 0)
        {
            // The 3D CLUT.
            if (clut is not null)
                for (var j = 0; j < numTabSize; j++)
                {
                    var val = From16to8(clut.Table.T[j]);
                    if (!io.Write(val)) return false;
                }
        }

        // The postlinearization table
        return postMpe is not null && Write8bitTables(io, newLut.outputChannels, ref postMpe);
    }
}
