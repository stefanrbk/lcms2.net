
using System.Diagnostics;
using System.Runtime.CompilerServices;

using lcms2.plugins;
using lcms2.state;

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

    internal Seg[] Segments = Array.Empty<Seg>();

    // 16 bit Table-based representation follows
    internal ushort[] Table16 = Array.Empty<ushort>();

    private ToneCurve(Seg[] segments, InterpParams? interpParams = null)
    {
        InterpParams = interpParams;
        Segments = segments;
    }

    private ToneCurve(ushort[] table16, InterpParams? interpParams = null)
    {
        InterpParams = interpParams;
        Table16 = table16;
    }

    internal int NumSegments =>
        Segments.Length;

    internal int NumEntries =>
        Table16.Length;

    internal static ToneCurve? Alloc(Context? context, uint numEntries, uint numSegments, in Seg[]? segments, in ushort[]? values)
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
            ? new ToneCurve(new Seg[numSegments])
            : new ToneCurve(new ushort[numEntries]);

        // Initialize members if requested
        if (values is not null && numEntries > 0)
            values.CopyTo(p.Table16.AsSpan());

        if (segments is not null && numSegments > 0) {

            for (var i = 0; i < numSegments; i++) {

                // Type 0 is a special marker for table-based curves
                if (segments[i].Type == 0) {

                    interp = InterpParams.Compute(context, segments[i].NumGridPoints, 1, 1, Array.Empty<float>(), LerpFlag.Float);
                    if (interp is null) return null;
                    p.Segments[i].Interp = interp;
                }

                p.Segments[i] = segments[i];

                p.Segments[i].SampledPoints = segments[i].Type == 0
                    ? (float[]?)segments[i].SampledPoints?.Clone()
                    : null;

                var c = ParametricCurvesCollection.GetByType(context, segments[i].Type, out _);
                if (c is not null)
                    p.Segments[i].Eval = c.Evaluator;
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
    private double SigmoidBase(double k, double t) =>
        (1.0 / (1.0 + Math.Exp(-k * t))) - 0.5;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double InvertedSigmoidBase(double k, double t) =>
        -Math.Log((1.0 / (t + 0.5)) - 1.0) / k;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double SigmoidFactory(double k, double t)
    {
        var correction = 0.5 / SigmoidBase(k, 1);

        return (correction * SigmoidBase(k, (2.0 * t) - 1.0)) + 0.5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double InverseSigmoidFactory(double k, double t)
    {
        var correction = 0.5 / SigmoidBase(k, 1);

        return (InvertedSigmoidBase(k, (t - 0.5) / correction) + 1.0) / 2.0;
    }

    internal int ParametricType =>
        NumSegments == 1
            ? Segments[0].Type
            : 0;

    private static double DefaultEvalParametricFn(int type, in double[] @params, double r)
    {
        return 0;
    }

    internal float Eval(float v)
    {
        throw new NotImplementedException();
    }

    internal static ToneCurve? BuildParametric(Context? context, int type, params double[] @params)
    {
        throw new NotImplementedException();
    }

    internal static ToneCurve? BuildSegmented(Context? context, CurveSegment[] segments)
    {
        throw new NotImplementedException();
    }

    internal static ToneCurve? BuildTabulated16(Context? context, uint numEntries, ushort[]? values)
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

    internal record struct Seg(CurveSegment Segment, InterpParams Interp, ParametricCurveEvaluator Eval)
    {
        internal float X0
        {
            get => Segment.X0;
            set => Segment.X0 = value;
        }
        internal float X1
        {
            get => Segment.X1;
            set => Segment.X1 = value;
        }
        internal int Type
        {
            get => Segment.Type;
            set => Segment.Type = value;
        }
        internal double[] Params
        {
            get => Segment.Params;
            set => Segment.Params = value;
        }
        internal uint NumGridPoints => Segment.NumGridPoints;
        internal float[]? SampledPoints
        {
            get => Segment.SampledPoints;
            set => Segment.SampledPoints = value;
        }
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
