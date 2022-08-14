using lcms2.io;
using lcms2.plugins;
using lcms2.state;

using static lcms2.Helpers;

namespace lcms2.types.type_handlers;
public class VcgtHandler : TagTypeHandler
{
    public VcgtHandler(Context? context = null)
        : base(default, context, 0) { }

    public override object? Duplicate(object value, int num) =>
        (value as ToneCurve[])?.Select(v => v.Clone()).ToArray();

    public override void Free(object value)
    {
        if (value is ToneCurve[] curves)
            ToneCurve.DisposeTriple(curves);
    }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        // Read tag type
        if (!io.ReadUInt32Number(out var temp)) return null;
        var tagType = (VCGType)temp;

        // Allocate space for the array;
        var curves = new ToneCurve[3];

        // There are two possible flavors
        switch (tagType) {

            // Gamma is stored as a table
            case VCGType.Table:

                // Check channel count, which should be 3 (we don't support monochrome at this time)
                if (!io.ReadUInt16Number(out var numChannels)) goto Error;

                if (numChannels != 3) {
                    Context.SignalError(Context, ErrorCode.UnknownExtension, "Unsupported number of channels for VCGT '{0}'", numChannels);
                    return null;
                }

                // Get Table element count and bytes per element
                if (!io.ReadUInt16Number(out var numElems)) goto Error;
                if (!io.ReadUInt16Number(out var numBytes)) goto Error;

                // Adobe's quirk fixup. Fixing broken profiles...
                if (numElems == 256 && numBytes == 1 && sizeOfTag == 1576)
                    numBytes = 2;

                // Populate tone curves
                for (var n = 0; n < 3; n++) {

                    var tempCurve = ToneCurve.BuildTabulated16(Context, numElems, null);
                    if (tempCurve is null) goto Error;

                    curves[n] = tempCurve;

                    // On depending on byte depth
                    switch (numBytes) {

                        // One byte, 0..255
                        case 1:
                            for (var i = 0; i < numElems; i++) {

                                if (!io.ReadUInt8Number(out var v)) goto Error;
                                curves[n].Table16[i] = From8to16(v);
                            }
                            break;

                        // One word 0..65535
                        case 2:

                            if (!io.ReadUInt16Array(numElems, out curves[n].Table16)) goto Error;

                            break;

                        // Unsupported
                        default:

                            Context.SignalError(Context, ErrorCode.UnknownExtension, "Unsupported bit depth for VCGT '{0}'", numBytes * 8);
                            goto Error;
                    }
                } // For all 3 channels

                break;

            // In this case, gamma is stored as a formula
            case VCGType.Formula:

                var colorant = new VCGTGAMMA[3];

                // populate tone curves
                for (var n = 0; n < 3; n++) {

                    if (!io.Read15Fixed16Number(out colorant[n].Gamma)) goto Error;
                    if (!io.Read15Fixed16Number(out colorant[n].Min)) goto Error;
                    if (!io.Read15Fixed16Number(out colorant[n].Max)) goto Error;

                    // Parametric curve type 5 is:
                    // Y = (aX + b)^Gamma + e | X >= d
                    // Y = cX + f             | X < d

                    // vcgt formula is:
                    // Y = (Max - Min) * (X ^ Gamma) + Min

                    // So, the translation is
                    // a = (Max - Min) ^ ( 1 / Gamma)
                    // e = Min
                    // b=c=d=f=0

                    var tempCurve = ToneCurve.BuildParametric(Context, 5, colorant[n].Gamma, Math.Pow(colorant[n].Max - colorant[n].Min, 1.0 / colorant[n].Gamma), 0, 0, 0, colorant[n].Min, 0);
                    if (tempCurve is null) goto Error;

                    curves[n] = tempCurve;
                }

                break;

            // Unsupported
            default:

                Context.SignalError(Context, ErrorCode.UnknownExtension, "Unsupported tag type for VCGT '{0}'", tagType);
                goto Error;
        }

        numItems = 1;
        return curves;

    Error:

        ToneCurve.DisposeTriple(curves);

        return null;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var curves = (ToneCurve[])value;

        if (curves[0].ParametricType == 5 &&
            curves[1].ParametricType == 5 &&
            curves[2].ParametricType == 5) {

            if (!io.Write((uint)VCGType.Formula)) return false;

            // Save parameters
            for (var i = 0; i < 3; i++) {

                VCGTGAMMA v;

                v.Gamma = curves[i].Segments[0].Params[0];
                v.Min = curves[i].Segments[0].Params[5];
                v.Max = Math.Pow(curves[i].Segments[0].Params[1], v.Gamma) + v.Min;

                if (!io.Write(v.Gamma)) return false;
                if (!io.Write(v.Min)) return false;
                if (!io.Write(v.Max)) return false;
            }
        } else {

            // Always store as a table of 256 words
            if (!io.Write((uint)VCGType.Table)) return false;
            if (!io.Write((ushort)3)) return false;
            if (!io.Write((ushort)256)) return false;
            if (!io.Write((ushort)2)) return false;

            for (var i = 0; i < 3; i++)
                for (var j = 0; j < 256; j++) {

                    var v = curves[i].Eval(j / 255.0f);
                    var n = QuickSaturateWord(v * 65535.0);

                    if (!io.Write(n)) return false;
                }
        }

        return true;
    }

    private struct VCGTGAMMA
    {
        public double Gamma;
        public double Min;
        public double Max;
    }

    private enum VCGType
    {
        Table = 0,
        Formula = 1,
    }
}
