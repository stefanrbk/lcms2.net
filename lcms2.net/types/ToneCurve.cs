using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    internal static ToneCurve? BuildParametric(Context? context, int type, params double[] @params)
    {
        throw new NotImplementedException();
    }

    internal static ToneCurve? BuildTabulated16(Context? context, uint numEntries, ushort[]? values)
    { 
        throw new NotImplementedException();
    }

    public object Clone() => throw new NotImplementedException();
    public void Dispose() => throw new NotImplementedException();

    internal record Seg(CurveSegment Segment, InterpParams[] Interp, ParametricCurveEvaluator Eval);
}
