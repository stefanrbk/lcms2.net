using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public delegate float ParametricCurveEvaluator(Signature type, in double[] @params, double r);

public class ParametricCurvesCollection
{
    internal int numFunctions;
    internal int[] functionTypes = new int[Lcms2.MaxTypesInPlugin];
    internal int[] parameterCount = new int[Lcms2.MaxTypesInPlugin];

    internal ParametricCurveEvaluator? evaluator = null;

    internal ParametricCurvesCollection? next = null;

    public const int MaxInputDimensions = 15;
}
