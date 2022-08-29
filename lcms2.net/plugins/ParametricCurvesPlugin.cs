using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Parametric Curves
/// </summary>
/// <remarks>
///     A plugin may implement an arbitrary number of parametric curves. <br/> Implements the
///     <c>cmsPluginParametricCurves</c> struct.
/// </remarks>
public sealed class ParametricCurvesPlugin: Plugin
{
    /// <summary>
    ///     The evaluator
    /// </summary>
    public ParametricCurveEvaluator Evaluator;

    public (int Types, int Count)[] Functions;

    public ParametricCurvesPlugin(Signature magic, uint expectedVersion, Signature type, (int Types, int Count)[] functions, ParametricCurveEvaluator evaluator)
        : base(magic, expectedVersion, type)
    {
        Functions = functions;

        Evaluator = evaluator;
    }

    internal static bool RegisterPlugin(object? state, ParametricCurvesPlugin? plugin)
    {
        var ctx = State.GetCurvesPlugin(state);

        if (plugin is null)
        {
            ctx.parametricCurves = null;
            return true;
        }

        ctx.parametricCurves = new ParametricCurvesCollection(plugin.Functions, plugin.Evaluator, ctx.parametricCurves);

        return true;
    }
}

internal sealed class ParametricCurvesPluginChunk
{
    internal static ParametricCurvesPluginChunk global = new();
    internal ParametricCurvesCollection? parametricCurves;

    internal static ParametricCurvesPluginChunk Default => new();

    private ParametricCurvesPluginChunk()
    { }
}
