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

    public bool IsLinear
    {
        get
        {
            for (var i = 0; i < NumEntries; i++) {

                var diff = Abs(Table16[i] - QuantizeValue(i, NumEntries));
                if (diff > 0x0F)
                    return false;
            }

            return true;
        }
    }

    public bool IsMonotonic
    {
        get
        {
            // Degenerated curves are monotonic? Ok, let's pass them
            var n = NumEntries;
            if (n < 2) return true;

            // Curve direction
            var lDesc = IsDescending;

            if (lDesc) {

                var last = Table16[0];

                for (var i = 1; i < n; i++) {
                    if (Table16[i] - last > 2) // We allow some ripple
                        return false;
                    else
                        last = Table16[i];
                }
            } else {
                var last = Table16[n - 1];

                for (var i = n - 2; i >= 0; i--) {
                    if (Table16[i] - last > 2)
                        return false;
                    else
                        last = Table16[i];
                }

            }
            return true;
        }
    }

    public bool IsDescending =>
        Table16[0] > Table16[NumEntries - 1];

    public bool IsMultisegment =>
        NumSegments > 1;

    public int ParametricType =>
        NumSegments == 1
            ? Segments[0].Type
            : 0;

    public double[] Params =>
        Segments[0].Params;

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

    public static ToneCurve? BuildTabulatedFloat(Context? context, int numEntries, float[] values)
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

    public static ToneCurve? BuildParametric(Context? context, int type, params double[] @params)
    {
        var c = ParametricCurvesCollection.GetByType(context, type, out var pos);

        if (c is null) {
            Context.SignalError(context, ErrorCode.UnknownExtension, "Invalid parametric curve type {0}", type);
            return null;
        }

        var seg0 = new CurveSegment()
        {
            X0 = MinusInf,
            X1 = PlusInf,
            Type = type,
        };

        var size = c.Functions[pos].Count * sizeof(double);
        @params[..size].CopyTo(seg0.Params, 0);

        return BuildSegmented(context, new CurveSegment[] { seg0 });
    }

    public static ToneCurve? BuildGamma(Context? context, double gamma) =>
        BuildParametric(context, 1, gamma);

    private bool disposed = false;
    public void Dispose()
    {
        if (!disposed) {
            Table16 = null!;

            if (Segments is not null) {
                for (var i = 0; i < NumSegments; i++) {
                    Segments[i].SampledPoints = null;
                    SegInterp[i] = null!;
                }
            }
            Evals = null!;

            GC.SuppressFinalize(this);
            disposed = true;
        }
    }

    public static void DisposeTriple(ToneCurve[] curves)
    {
        Debug.Assert(curves is not null && curves.Length == 3);

        curves![0]?.Dispose();
        curves![1]?.Dispose();
        curves![2]?.Dispose();
    }

    public object Clone() =>
        Alloc(InterpParams?.Context, NumEntries, NumSegments, Segments, Table16)!;

    public ToneCurve? Join(Context? context, ToneCurve Y, int numResultingPoints)
    {
        var X = this;
        ToneCurve? result = null;

        var Yreversed = Y.Reverse(numResultingPoints);
        if (Yreversed is null) goto Error;

        var res = new float[numResultingPoints];

        // Iterate
        for (var i = 0; i < numResultingPoints; i++) {
            var t = (float)i / (numResultingPoints - 1);
            var x = X.Eval(t);
            res[i] = Yreversed.Eval(x);
        }

        result = BuildTabulatedFloat(context, numResultingPoints, res);

    Error:
        Yreversed?.Dispose();

        return result;
    }

    private static int GetInterval(double @in, ushort[] lutTable, InterpParams p)
    {
        // A 1 point table is not allowed
        if (p.Domain[0] < 1) return -1;

        // Let's see if ascending or descending
        if (lutTable[0] < lutTable[p.Domain[0]]) {

            // Table is overall ascending
            for (var i = p.Domain[0] - 1; i >= 0; i--) {

                var y0 = lutTable[i];
                var y1 = lutTable[i + 1];

                if (y0 <= y1) { // Increasing
                    if (@in >= y0 && @in <= y1) return i;
                } else { // Decreasing
                    if (@in >= y1 && @in <= y0) return i;
                }
            }
        } else {
            // Table is overall descending
            for (var i = 0; i < p.Domain[0]; i++) {

                var y0 = lutTable[i];
                var y1 = lutTable[i + 1];

                if (y0 <= y1) {
                    if (@in >= y0 && @in <= y1) return i;
                } else {
                    if (@in >= y1 && @in <= y0) return i;
                }
            }
        }

        return -1;
    }

    public ToneCurve? Reverse(int numResultSamples)
    {
        var a = 0.0;
        var b = 0.0;

        // Try to reverse it analytically whatever possible

        if (NumSegments == 1 && Segments[0].Type > 0 &&
            /* Segments[0].Type <= 5 */
            ParametricCurvesCollection.GetByType(InterpParams?.Context, Segments[0].Type, out _) is not null) {

            return BuildParametric(InterpParams?.Context, -Segments[0].Type, Segments[0].Params);
        }

        var result = BuildTabulated16(InterpParams?.Context, numResultSamples, null);
        if (result is null)
            return null;

        // We want to know if this is an ascending or descending table
        var ascending = !IsDescending;

        // Iterate across Y axis
        for (var i = 0; i < numResultSamples; i++) {
            var y = i * 65535.0 / (numResultSamples - 1);

            // Find interval in which y is within.
            var j = GetInterval(y, Table16, InterpParams!);
            if (j >= 0) {

                // Get limits of interval
                var x1 = Table16[j];
                var x2 = Table16[j + 1];

                var y1 = j * 65535.0 / (NumEntries - 1);
                var y2 = (j + 1) * 65535.0 / (NumEntries - 1);

                // if collapsed, then use any
                if (x1 == x2) {

                    result.Table16[i] = QuickSaturateWord(ascending ? y2 : y1);
                    continue;

                } else {

                    // Interpolate
                    a = (y2 - y1) / (x2 - x1);
                    b = y2 - (a * x2);

                }
            }

            result.Table16[i] = QuickSaturateWord((a * y) + b);
        }

        return result;
    }

    public ToneCurve? Reverse() =>
        Reverse(4096);

    private static bool Smooth2(float[] w, float[] y, float[] z, float lambda, int m)
    {
        int i, i1, i2;

        var c = new float[MaxNodesInCurve];
        var d = new float[MaxNodesInCurve];
        var e = new float[MaxNodesInCurve];

        d[1] = w[1] + lambda;
        c[1] = -2 * lambda / d[1];
        e[1] = lambda / d[1];
        z[1] = w[1] * y[1];
        d[2] = w[2] + (5 * lambda) - (d[1] * c[1] * c[1]);
        c[2] = ((-4 * lambda) - (d[1] * c[1] * e[1])) / d[2];
        e[2] = lambda / d[2];
        z[2] = (w[2] * y[2]) - (c[1] * z[1]);

        for (i = 3; i < m - 1; i++) {
            i1 = i - 1; i2 = i - 2;
            d[i] = w[i] + (6 * lambda) - (c[i1] * c[i1] * d[i1]) - (e[i2] * e[i2] * d[i2]);
            c[i] = ((-4 * lambda) - (d[i1] * c[i1] * e[i1])) / d[i];
            e[i] = lambda / d[i];
            z[i] = (w[i] * y[i]) - (c[i1] * z[i1]) - (e[i2] * z[i2]);
        }

        i1 = m - 2; i2 = m - 3;

        d[m - 1] = w[m - 1] + (5 * lambda) - (c[i1] * c[i1] * d[i1]) - (e[i2] * e[i2] * d[i2]);
        c[m - 1] = ((-2 * lambda) - (d[i1] * c[i1] * e[i1])) / d[m - 1];
        z[m - 1] = (w[m - 1] * y[m - 1]) - (c[i1] * z[i1]) - (e[i2] * z[i2]);
        i1 = m - 1; i2 = m - 2;

        d[m] = w[m] + lambda - (c[i1] * c[i1] * d[i1]) - (e[i2] * e[i2] * d[i2]);
        z[m] = ((w[m] * y[m]) - (c[i1] * z[i1]) - (e[i2] * z[i2])) / d[m];
        z[m - 1] = (z[m - 1] / d[m - 1]) - (c[m - 1] * z[m]);

        for (i = m - 2; 1 <= i; i--)
            z[i] = (z[i] / d[i]) - (c[i] * z[i + 1]) - (e[i] * z[i + 2]);

        return true;
    }

    public bool Smooth(double lambda)
    {
        var successStatus = true;

        if (InterpParams is not null) {

            var context = InterpParams.Context;

            if (!IsLinear) { // Only non-linear curves need smoothing

                var numItems = NumEntries;
                if (numItems < MaxNodesInCurve) {

                    // Allocate one more item than needed
                    var w = new float[numItems + 1];
                    var y = new float[numItems + 1];
                    var z = new float[numItems + 1];

                    {
                        for (var i = 0; i < numItems; i++) {
                            y[i + 1] = Table16[i];
                            w[i + 1] = 1f;
                        }

                        var notCheck = false;

                        if (lambda < 0) {
                            notCheck = true;
                            lambda = -lambda;
                        }

                        if (Smooth2(w, y, z, (float)lambda, numItems)) {

                            // Do some reality checking...

                            var zeros = 0;
                            var poles = 0;
                            for (var i = numItems; i > 1; i--) {

                                if (z[i] == 0f) zeros++;
                                if (z[i] >= 65535f) poles++;
                                if (z[i] < z[i - 1]) {
                                    Context.SignalError(context, ErrorCode.Range, "ToneCurve.Smooth: Non-Monotonic.");
                                    successStatus = notCheck;
                                    break;
                                }
                            }

                            if (successStatus && zeros > (numItems / 3)) {
                                Context.SignalError(context, ErrorCode.Range, "ToneCurve.Smooth: Degenerated, mostly zeros.");
                                successStatus = notCheck;
                            }

                            if (successStatus && poles > (numItems / 3)) {
                                Context.SignalError(context, ErrorCode.Range, "ToneCurve.Smooth: Degenerated, mostly poles.");
                                successStatus = notCheck;
                            }

                            if (successStatus) { // Seems ok
                                for (var i = 0; i < numItems; i++)
                                    // Clamp to ushort
                                    Table16[i] = QuickSaturateWord(z[i + 1]);
                            }
                        } else { // Could not smooth
                            Context.SignalError(context, ErrorCode.Range, "ToneCurve.Smooth: Function Smooth2 failed.");
                            successStatus = false;
                        }
                    }
                } else {
                    Context.SignalError(context, ErrorCode.Range, "ToneCurve.Smooth: Too many points.");
                    successStatus = false;
                }
            }
        }

        return successStatus;
    }

    public float Eval(float v)
    {
        // Check for 16 bit table. If so, this is a limited-precision tone curve
        if (NumSegments == 0) {
            var i = QuickSaturateWord(v * 65535.0);
            var o = Eval(i);

            return o / 65535.0f;
        }

        return (float)EvalSegmentedFn(v);
    }

    public ushort Eval(ushort v)
    {
        var i = new ushort[] { v };
        var o = new ushort[1];

        InterpParams!.Interpolation.Lerp16(i, ref o, InterpParams!);

        return o[0];
    }

    public double EstimateGamma(double precision)
    {
        var sum = 0d;
        var sum2 = 0d;
        var n = 0d;

        // Excluding endpoints
        for (var i = 1; i < MaxNodesInCurve - 1; i++) {

            var x = (double)i / (MaxNodesInCurve - 1);
            var y = (double)Eval((float)x);

            // Avoid 7% on lower part to prevent
            // artifacts due to linear ramps
            if (y > 0 && y < 1 && x > 0.07) {
                var gamma = Log(y) / Log(x);
                sum += gamma;
                sum2 += gamma * gamma;
                n++;
            }
        }

        // Take a look on SD to see if gamma isn't exponential at all
        var std = Sqrt(((n * sum2) - (sum * sum)) / (n * (n - 1)));

        return (std > precision)
            ? -1d
            : sum / n; // The mean
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
