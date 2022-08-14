
using System.Diagnostics;

using lcms2.plugins;
using lcms2.state;

namespace lcms2.types;

public class ToneCurve : ICloneable, IDisposable
{
    // Private optimizations for interpolation
    internal InterpParams? InterpParams;

    internal Seg[] Segments;

    // 16 bit Table-based representation follows
    internal ushort[] Table16;

    internal int NumSegments =>
        Segments.Length;

    internal int NumEntries =>
        Table16.Length;

    internal int ParametricType =>
        NumSegments == 1
            ? Segments[0].Type
            : 0;

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

    internal record Seg(CurveSegment Segment, InterpParams[] Interp, ParametricCurveEvaluator Eval)
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
        internal float[] SampledPoints
        {
            get => Segment.SampledPoints;
            set => Segment.SampledPoints = value;
        }
    }
}
