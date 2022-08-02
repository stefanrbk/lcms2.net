using lcms2.types;

namespace lcms2.plugins;

#if PLUGIN
public delegate double ParametricCurveEvaluator(
#else
internal delegate double ParametricCurveEvaluator(
#endif
    Signature type, in double[] @params, double r);
#if PLUGIN
public sealed class PluginParametricCurves
#else
internal sealed class PluginParametricCurves
#endif
    : PluginBase
{
    public int NumFunctions;
    public int[] FunctionTypes;
    public int[] ParameterCount;

    public PluginParametricCurves(Signature magic, uint expectedVersion, Signature type, int numFunctions, int[] functionTypes, int[] parameterCount)
        : base(magic, expectedVersion, type)
    {
        NumFunctions = numFunctions;
        FunctionTypes = functionTypes;
        ParameterCount = parameterCount;
    }
}

#if PLUGIN
public class ParametricCurvesCollection
#else
internal class ParametricCurvesCollection
#endif
{
    internal int numFunctions;
    internal int[] functionTypes = new int[Lcms2.MaxTypesInPlugin];
    internal int[] parameterCount = new int[Lcms2.MaxTypesInPlugin];

    internal ParametricCurveEvaluator? evaluator = null;

    internal ParametricCurvesCollection? next = null;

    public const int MaxInputDimensions = 15;
}
