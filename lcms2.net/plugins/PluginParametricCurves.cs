using lcms2.types;

namespace lcms2.plugins;

public delegate double ParametricCurveEvaluator(Signature type, in double[] @params, double r);
public sealed class PluginParametricCurves : PluginBase
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

public class ParametricCurvesCollection
{
    internal int numFunctions;
    internal int[] functionTypes = new int[Lcms2.MaxTypesInPlugin];
    internal int[] parameterCount = new int[Lcms2.MaxTypesInPlugin];

    internal ParametricCurveEvaluator? evaluator = null;

    internal ParametricCurvesCollection? next = null;

    public const int MaxInputDimensions = 15;
}
