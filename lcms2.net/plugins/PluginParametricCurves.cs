using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Evaluator callback for user-supplied parametric curves. May implement more than one type
/// </summary>
/// <remarks>Implements the <c>cmsParametricCurveEvaluator</c> typedef.</remarks>
public delegate double ParametricCurveEvaluator(Signature type, in double[] @params, double r);

/// <summary>
///     Parametric Curves
/// </summary>
/// <remarks>
///     A plugin may implement an arbitrary number of parametric curves.<br />
///     Implements the <c>cmsPluginParametricCurves</c> struct.
/// </remarks>
public sealed class PluginParametricCurves : Plugin
{
    /// <summary>
    ///     Number of supported functions
    /// </summary>
    public int NumFunctions;
    /// <summary>
    ///     The identification types
    /// </summary>
    /// <remarks>A negative type means same function but analytically inverted.</remarks>
    public int[] FunctionTypes;
    /// <summary>
    ///     The number of parameters for each function
    /// </summary>
    /// <remarks>Max of 10</remarks>
    public int[] ParameterCount;

    /// <summary>
    ///     The evaluator
    /// </summary>
    public ParametricCurveEvaluator Evaluator;

    public PluginParametricCurves(Signature magic, uint expectedVersion, Signature type, int numFunctions, int[] functionTypes, int[] parameterCount, ParametricCurveEvaluator evaluator)
        : base(magic, expectedVersion, type)
    {
        NumFunctions = numFunctions;
        FunctionTypes = functionTypes;
        ParameterCount = parameterCount;

        Evaluator = evaluator;
    }

    internal static bool RegisterPlugin(Context? context, PluginParametricCurves? plugin)
    {
        throw new NotImplementedException();
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
