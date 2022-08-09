using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class CurveHandler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion => 0;

    public object? Duplicate(ITagTypeHandler handler, object value, int num) =>
        (value as ToneCurve)?.Clone();

    public void Free(ITagTypeHandler handler, object value) =>
        (value as ToneCurve)?.Dispose();

    public object? Read(ITagTypeHandler handler, Stream io, int sizeOfTag, out int numItems)
    {
        double singleGamma;
        ToneCurve? newGamma;

        numItems = 0;
        if (!io.ReadUInt32Number(out var count)) return null;

        switch (count)
        {
            case 0: // Linear
                singleGamma = 1.0;

                newGamma = ToneCurve.BuildParametric(Context, 1, singleGamma);
                if (newGamma is null) return null;
                numItems = 1;
                return newGamma;
            case 1: // Specified as the exponent of gamma function
                if (!io.ReadUInt16Number(out var singleGammaFixed)) return null;
                singleGamma = IOHandler.U8Fixed8toDouble(singleGammaFixed);

                numItems = 1;
                return ToneCurve.BuildParametric(Context, 1, singleGamma);
            default: // Curve
                if (count > 0x7FFF)
                    return null; // This is to prevent bad guys for doing bad things.

                newGamma = ToneCurve.BuildTabulated16(Context, count, null);
                if (newGamma is null) return null;

                if (!io.ReadUInt16Array((int)count, out newGamma.Table16))
                {
                    newGamma.Dispose();
                    return null;
                }

                numItems = 1;
                return newGamma;
        }
    }

    public bool Write(ITagTypeHandler handler, Stream io, object value, int numItems)
    {
        var curve = (ToneCurve)value;

        if (curve.NumSegments == 1 && curve.Segments[0].Type == 1)
        {
            // Single gamma, preserve number
            var singleGammaFixed = IOHandler.DoubleToU8Fixed8(curve.Segments[0].Params![0]);

            if (!io.Write((uint)1)) return false;
            if (!io.Write(singleGammaFixed)) return false;
            return true;
        }

        if (!io.Write((uint)curve.NumEntries))return false;
        return io.Write(curve.NumEntries, curve.Table16);
    }
}
