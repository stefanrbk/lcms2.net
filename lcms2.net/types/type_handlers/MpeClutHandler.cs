using lcms2.io;
using lcms2.plugins;
using lcms2.state;

using static lcms2.Helpers;

namespace lcms2.types.type_handlers;
public class MpeClutHandler : TagTypeHandler
{
    public MpeClutHandler(Signature sig, Context? context = null)
        : base(sig, context, 0) { }

    public MpeClutHandler(Context? context = null)
        : this(default, context) { }

    public override object? Duplicate(object value, int num) =>
        (value as Stage)?.Clone();

    public override void Free(object value) =>
        (value as Stage)?.Dispose();

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        var dimensions8 = new byte[16];

        numItems = 0;

        if (!io.ReadUInt16Number(out var inputChans)) return null;
        if (!io.ReadUInt16Number(out var outputChans)) return null;

        if (inputChans == 0 || outputChans == 0) return null;

        if (io.Read(dimensions8) != 16) return null;

        // Copy MaxInputDimensions at most. Expand to uint
        var numMaxGrids = (uint)Math.Min((ushort)MaxInputDimensions, inputChans);
        var gridPoints = new uint[numMaxGrids];

        for (var i = 0; i < numMaxGrids; i++) {

            if (dimensions8[i] == 1) return null; // Impossible value, 0 for no CLUT or at least 2
            gridPoints[i] = dimensions8[i];
        }

        // Allocate the true CLUT
        var mpe = Stage.AllocCLutFloatGranular(Context, gridPoints, inputChans, outputChans, null);
        if (mpe is null) goto Error;

        // Read and sanitize the data
        var clut = (Stage.CLutData)mpe.Data;
        for (var i = 0; i < clut.NumEntries; i++)
            if (!io.ReadFloat32Number(out clut.Table.TFloat[i])) goto Error;

        numItems = 1;
        return mpe;

    Error:

        mpe?.Dispose();
        return null;
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        var dimensions8 = new byte[16];
        var mpe = (Stage)value;
        var clut = (Stage.CLutData)mpe.Data;

        // Check for maximum number of channels supported by lcms
        if (mpe.InputChannels > MaxInputDimensions) return false;

        // Only floats are supported in MPE
        if (!clut.HasFloatValues) return false;

        if (!io.Write((ushort)mpe.InputChannels)) return false;
        if (!io.Write((ushort)mpe.OutputChannels)) return false;

        for (var i = 0; i < mpe.InputChannels; i++)
            dimensions8[i] = (byte)clut.Params[0].NumSamples[i];

        io.Write(dimensions8);

        for (var i = 0; i < clut.NumEntries; i++)
            if (!io.Write(clut.Table.TFloat[i])) return false;

        return true;
    }
}
