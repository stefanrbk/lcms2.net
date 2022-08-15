using System.Diagnostics;
using System.Runtime.CompilerServices;

using lcms2.state;

using static System.Math;
using static lcms2.Helpers;

namespace lcms2.types;

/// <summary>
///     Evaluator callback for user-supplied parametric curves. May implement more than one type
/// </summary>
/// <remarks>Implements the <c>cmsParametricCurveEvaluator</c> typedef.</remarks>
public delegate double ParametricCurveEvaluator(int type, in double[] @params, double r);

public class ToneCurve : ICloneable, IDisposable
{
    // Private optimizations for interpolation
    internal InterpParams? InterpParams;

    internal int NumSegments =>
        Segments.Length;
    internal CurveSegment[] Segments = Array.Empty<CurveSegment>();
    internal InterpParams[] SegInterp = Array.Empty<InterpParams>();
    internal ParametricCurveEvaluator[] Evals = Array.Empty<ParametricCurveEvaluator>();

    // 16 bit Table-based representation follows
    public int NumEntries =>
        Table16.Length;
    internal ushort[] Table16 = Array.Empty<ushort>();


    private ToneCurve(CurveSegment[] segments, ParametricCurveEvaluator[] evals, InterpParams? interpParams = null)
    {
        InterpParams = interpParams;
        Segments = segments;
        Evals = evals;
    }

    private ToneCurve(ushort[] table16, InterpParams? interpParams = null)
    {
        InterpParams = interpParams;
        Table16 = table16;
    }

    public ushort[] EstimatedTable =>
        Table16;

    internal int ParametricType =>
        NumSegments == 1
            ? Segments[0].Type
            : 0;

    internal static ToneCurve? Alloc(Context? context, int numEntries, int numSegments, in CurveSegment[]? segments, in ushort[]? values)
    {
        InterpParams? interp;

        // We allow huge tables, which are then restricted for smoothing operations
        if (numEntries > 65530) {
            Context.SignalError(context, ErrorCode.Range, "Couldn't create tone curve of more than 65530 entries");
            return null;
        }

        if (numEntries == 0 && numSegments == 0) {
            Context.SignalError(context, ErrorCode.Range, "Couldn't create tone curve with zero segments and no table");
            return null;
        }


        var p = numSegments != 0
            ? new ToneCurve(new CurveSegment[numSegments], new ParametricCurveEvaluator[numSegments])
            : new ToneCurve(new ushort[numEntries]);

        // Initialize members if requested
        if (values is not null && numEntries > 0)
            values.CopyTo(p.Table16.AsSpan());

        // Initialize the segments stuff. The evaluator for each segment is located and a pointer to it
        // is placed in advance to maximize performance.
        if (segments is not null && numSegments > 0) {

            for (var i = 0; i < numSegments; i++) {

                // Type 0 is a special marker for table-based curves
                if (segments[i].Type == 0) {

                    interp = InterpParams.Compute(context, segments[i].NumGridPoints, 1, 1, Array.Empty<float>(), LerpFlag.Float);
                    if (interp is null) return null;
                    p.SegInterp[i] = interp;
                }

                p.Segments[i] = segments[i];

                p.Segments[i].SampledPoints = segments[i].Type == 0
                    ? (float[]?)segments[i].SampledPoints?.Clone()
                    : null;

                var c = ParametricCurvesCollection.GetByType(context, segments[i].Type, out _);
                if (c is not null)
                    p.Evals[i] = c.Evaluator;
            }
        }

        interp = InterpParams.Compute(context, numEntries, 1, 1, p.Table16, LerpFlag.Ushort);
        if (interp is not null) {
            p.InterpParams = interp;
            return p;
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double SigmoidBase(double k, double t) =>
        (1.0 / (1.0 + Exp(-k * t))) - 0.5;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double InvertedSigmoidBase(double k, double t) =>
        -Log((1.0 / (t + 0.5)) - 1.0) / k;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double SigmoidFactory(double k, double t)
    {
        var correction = 0.5 / SigmoidBase(k, 1);

        return (correction * SigmoidBase(k, (2.0 * t) - 1.0)) + 0.5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double InverseSigmoidFactory(double k, double t)
    {
        var correction = 0.5 / SigmoidBase(k, 1);

        return (InvertedSigmoidBase(k, (t - 0.5) / correction) + 1.0) / 2.0;
    }

    private static double DefaultEvalParametricFn(int type, in double[] @params, double r)
    {
        double val, disc, e;

        var p0 = @params[0]; var ap0 = Abs(p0);
        var p1 = @params.Length <= 1 ? @params[1] : 0; var ap1 = Abs(p1);
        var p2 = @params.Length <= 1 ? @params[2] : 0; var ap2 = Abs(p2);
        var p3 = @params.Length <= 1 ? @params[3] : 0; var ap3 = Abs(p3);
        var p4 = @params.Length <= 1 ? @params[4] : 0; var ap4 = Abs(p4);
        var p5 = @params.Length <= 1 ? @params[5] : 0; var ap5 = Abs(p5);
        var p6 = @params.Length <= 1 ? @params[6] : 0; var ap6 = Abs(p6);

        switch (type) {

            // X = Y ^ Gamma
            case 1:

                val = r < 0
                    ? Abs(p0 - 1.0) < DeterminantTolerance
                        ? r
                        : 0
                    : Pow(r, p0);

                break;

            // Type 1 Reversed: X = Y ^ 1/Gamma
            case -1:

                val = r < 0
                    ? Abs(p0 - 1.0) < DeterminantTolerance
                        ? r
                        : 0
                    : ap0 < DeterminantTolerance
                        ? PlusInf
                        : Pow(r, 1 / p0);

                break;

            // CIE 122-1966
            // Y = (aX + b)^Gamma  | X >= -b/a
            // Y = 0               | else
            case 2:

                if (ap1 < DeterminantTolerance) {
                    val = 0;
                } else {
                    disc = -p2 / p1;

                    if (r >= disc) {
                        e = (p1 * r) + p2;

                        val = e > 0
                            ? Pow(e, p0)
                            : 0;
                    } else {
                        val = 0;
                    }
                }

                break;

            // Type 2 Reversed
            // X = (Y ^1/g  - b) / a
            case -2:

                val = ap0 < DeterminantTolerance || ap1 < DeterminantTolerance
                    ? 0
                    : r < 0
                        ? 0
                        : Max((Pow(r, 1.0 / p0) - p2) / p1, 0);

                break;


            // IEC 61966-3
            // Y = (aX + b)^Gamma | X <= -b/a
            // Y = c              | else
            case 3:

                if (ap1 < DeterminantTolerance) {
                    val = 0;
                } else {

                    disc = Max(-p2 / p1, 0);

                    if (r >= disc) {
                        e = (p1 * r) + p2;

                        val = e > 0
                            ? Pow(e, p0) + p3
                            : 0;
                    } else {
                        val = p3;
                    }
                }

                break;


            // Type 3 reversed
            // X=((Y-c)^1/g - b)/a      | (Y>=c)
            // X=-b/a                   | (Y<c)
            case -3:

                if (ap1 < DeterminantTolerance) {
                    val = 0;
                } else {
                    if (r >= p3) {
                        e = r - p3;

                        val = e > 0
                            ? (Pow(e, 1 / p0) - p2) / p1
                            : 0;
                    } else {
                        val = -p2 / p1;
                    }
                }

                break;

            // IEC 61966-2.1 (sRGB)
            // Y = (aX + b)^Gamma | X >= d
            // Y = cX             | X < d
            case 4:

                if (r >= p4) {
                    e = (p1 * r) + p2;

                    val = e > 0
                        ? Pow(e, p0)
                        : 0;
                } else {
                    val = r * p3;
                }

                break;

            // Type 4 reversed
            // X=((Y^1/g-b)/a)    | Y >= (ad+b)^g
            // X=Y/c              | Y< (ad+b)^g
            case -4:

                e = (p1 * p4) + p2;
                disc = e < 0
                    ? 0
                    : Pow(e, p0);

                val = r >= disc
                    ? ap0 < DeterminantTolerance || ap1 < DeterminantTolerance
                        ? 0
                        : (Pow(r, 1.0 / p0) - p2) / p1
                    : ap3 < DeterminantTolerance
                        ? 0
                        : r / p3;

                break;

            // Y = (aX + b)^Gamma + e | X >= d
            // Y = cX + f             | X < d
            case 5:

                if (r >= p4) {

                    e = (p1 * r) + p2;

                    val = e > 0
                        ? Pow(e, p0) + p5
                        : p5;
                } else {
                    val = (r * p3) + p6;
                }

                break;

            // Reversed type 5
            // X=((Y-e)1/g-b)/a   | Y >=(ad+b)^g+e), cd+f
            // X=(Y-f)/c          | else
            case -5:

                disc = (p3 * p4) + p6;
                if (r >= disc) {

                    e = r - p5;
                    val = e < 0
                        ? 0
                        : ap0 < DeterminantTolerance || ap1 < DeterminantTolerance
                            ? 0
                            : (Pow(e, 1.0 / p0) - p2) / p1;
                } else {
                    val = ap3 < DeterminantTolerance
                        ? 0
                        : (r - p6) / p3;
                }

                break;

            // Types 6,7,8 comes from segmented curves as described in ICCSpecRevision_02_11_06_Float.pdf
            // Type 6 is basically identical to type 5 without d

            // Y = (a * X + b) ^ Gamma + c
            case 6:

                e = (p1 * r) + p2;

                val = e < 0
                    ? p3
                    : Pow(e, p0) + p3;

                break;

            // ((Y - c) ^1/Gamma - b) / a
            case -6:

                if (ap1 < DeterminantTolerance) {
                    val = 0;
                } else {
                    e = r - p3;
                    val = e < 0
                        ? 0
                        : (Pow(e, 1.0 / p0) - p2) / p1;
                }

                break;

            // Y = a * log (b * X^Gamma + c) + d
            case 7:

                e = (p2 * Pow(r, p0)) + p3;
                val = e <= 0
                    ? p4
                    : (p1 * Log10(e)) + p4;

                break;
            // (Y - d) / a = log(b * X ^Gamma + c)
            // pow(10, (Y-d) / a) = b * X ^Gamma + c
            // pow((pow(10, (Y-d) / a) - c) / b, 1/g) = X
            case -7:

                val = ap0 < DeterminantTolerance || ap1 < DeterminantTolerance || ap2 < DeterminantTolerance
                    ? 0
                    : Pow((Pow(10.0, (r - p4) / p1) - p3) / p2, 1.0 / p0);

                break;

            //Y = a * b^(c*X+d) + e
            case 8:

                val = (p0 * Pow(p1, (p2 * r) + p3)) + p4;

                break;

            // Y = (log((y-e) / a) / log(b) - d ) / c
            // a=0, b=1, c=2, d=3, e=4,
            case -8:

                disc = r - p4;
                val = disc < 0
                    ? 0
                    : ap0 < DeterminantTolerance || ap2 < DeterminantTolerance
                        ? 0
                        : ((Log(disc / p0) / Log(p1)) - p3) / p2;

                break;

            // S-Shaped: (1 - (1-x)^1/g)^1/g
            case 108:

                val = ap0 < DeterminantTolerance
                    ? 0
                    : Pow(1.0 - Pow(1 - r, 1 / p0), 1 / p0);

                break;

            // y = (1 - (1-x)^1/g)^1/g
            // y^g = (1 - (1-x)^1/g)
            // 1 - y^g = (1-x)^1/g
            // (1 - y^g)^g = 1 - x
            // 1 - (1 - y^g)^g
            case -108:

                val = 1 - Pow(1 - Pow(r, p0), p0);

                break;

            // Sigmoidals
            case 109:

                val = SigmoidFactory(p0, r);

                break;

            case -109:

                val = InverseSigmoidFactory(p0, r);

                break;

            default:
                // Unsupported parametric curve. Should never reach here
                return 0;
        }

        return val;
    }

    private double EvalSegmentedFn(double r)
    {
        double result;

        for (var i = NumSegments - 1; i >= 0; i--) {

            // Check for domain
            if ((r > Segments[i].X0) && (r <= Segments[i].X1)) {

                // Type == 0 means segment is sampled
                if (Segments[i].Type == 0) {

                    var r1 = new float[] { (float)((r - Segments[i].X0) / (Segments[i].X1 - Segments[i].X0)) };
                    var out32 = new float[1];

                    // Setup the table (TODO: clean that)
                    SegInterp[i].Table = Segments[i].SampledPoints!;

                    SegInterp[i].Interpolation.LerpFloat(in r1, ref out32, SegInterp[i]);
                    result = out32[0];
                } else {
                    result = Evals[i](Segments[i].Type, Segments[i].Params, r);
                }

                return Double.IsPositiveInfinity(result)
                    ? PlusInf
                    : Double.IsNegativeInfinity(result)
                        ? MinusInf
                        : result;
            }
        }

        return MinusInf;
    }

    public static ToneCurve? BuildTabulated16(Context? context, int numEntries, ushort[]? values) =>
        Alloc(context, numEntries, 0, null, values);

    private static int EntriesByGamma(double gamma) =>
        Abs(gamma - 1.0) < 0.001
            ? 2
            : 4096;

    public static ToneCurve? BuildSegmented(Context? context, CurveSegment[] segments)
    {
        var numSegments = segments.Length;
        var numGridPoints = 4096;

        // Optimization for identity curves.
        if (numSegments == 1 && segments[0].Type == 1)
            numGridPoints = EntriesByGamma(segments[0].Params[0]);

        var g = Alloc(context, numGridPoints, numSegments, segments, null);
        if (g is null) return null;

        // Once we have the floating point version, we can approximate a 16 bit table of 4096 entries
        // for performance reasons. This table would normally not be used except on 8/16 bit transforms.
        for (var i = 0; i < numGridPoints; i++) {

            var r = (double)i / (numGridPoints - 1);

            var val = g.EvalSegmentedFn(r);

            // Round and saturate
            g.Table16[i] = QuickSaturateWord(val * 65535.0);
        }

        return g;
    }

    internal static ToneCurve? BuildTabulatedFloat(Context? context, int numEntries, float[] values)
    {
        var seg = new CurveSegment[3];

        seg[0] = new()
        {
            X0 = MinusInf,
            X1 = 0f,
            Type = 6,
        };
        seg[0].Params[0] = 1;
        seg[0].Params[1] = 0;
        seg[0].Params[2] = 0;
        seg[0].Params[3] = values[0];
        seg[0].Params[4] = 0;

        seg[1] = new()
        {
            X0 = 0f,
            X1 = 1f,
            Type = 0,
            SampledPoints = values,
        };

        seg[2] = new()
        {
            X0 = 1f,
            X1 = PlusInf,
            Type = 6,
        };
        seg[2].Params[0] = 1;
        seg[2].Params[1] = 0;
        seg[2].Params[2] = 0;
        seg[2].Params[3] = values[numEntries - 1];
        seg[2].Params[4] = 0;

        return BuildSegmented(context, seg);
    }

    internal float Eval(float v)
    {
        throw new NotImplementedException();
    }

    internal static ToneCurve? BuildParametric(Context? context, int type, params double[] @params)
    {
        throw new NotImplementedException();
    }

    public object Clone() => throw new NotImplementedException();
    public void Dispose() => throw new NotImplementedException();

    public static void DisposeTriple(ToneCurve[] curves)
    {
        Trace.Assert(curves is not null && curves.Length == 3);

        curves![0]?.Dispose();
        curves![1]?.Dispose();
        curves![2]?.Dispose();
    }

    internal static ParametricCurvesCollection DefaultCurves = new(
        new (int Types, int Count)[]
        {
            (1, 1),
            (2, 3),
            (3, 4),
            (4, 5),
            (5, 7),
            (6, 4),
            (7, 5),
            (8, 5),
            (108, 1),
            (109, 1),
        },
        DefaultEvalParametricFn,
        null);
}

internal class ParametricCurvesCollection
{
    internal (int Type, int Count)[] Functions = new (int, int)[Lcms2.MaxTypesInPlugin];

    internal ParametricCurveEvaluator Evaluator;

    internal ParametricCurvesCollection? Next;

    internal int NumFunctions =>
        Functions.Length;

    public const int MaxInputDimensions = 15;

    public ParametricCurvesCollection((int Types, int Count)[] functions, ParametricCurveEvaluator evaluator, ParametricCurvesCollection? next)
    {
        functions.CopyTo(Functions.AsSpan());
        Evaluator = evaluator;
        Next = next;
    }

    public ParametricCurvesCollection(ParametricCurvesCollection other, ParametricCurvesCollection? next = null)
    {
        other.Functions.CopyTo(Functions.AsSpan());
        Evaluator = other.Evaluator;
        Next = next;
    }

    internal int IsInSet(int type)
    {
        for (var i = 0; i < NumFunctions; i++)
            if (Math.Abs(type) == Functions[i].Type) return i;

        return -1;
    }

    internal static ParametricCurvesCollection? GetByType(Context? context, int type, out int index)
    {
        var ctx = Context.GetCurvesPlugin(context);

        for (var c = ctx.parametricCurves; c is not null; c = c.Next) {
            var pos = c.IsInSet(type);

            if (pos != -1) {
                index = pos;
                return c;
            }
        }

        // If none found, revert to defaults
        for (var c = ToneCurve.DefaultCurves; c is not null; c = c.Next) {
            var pos = c.IsInSet(type);

            if (pos != -1) {
                index = pos;
                return c;
            }
        }

        index = -1;
        return null;
    }
}
