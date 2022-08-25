using System.Diagnostics;

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

    internal static bool RegisterPlugin(Context? context, ParametricCurvesPlugin? plugin)
    {
        var ctx = Context.GetCurvesPlugin(context);

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

    private ParametricCurvesPluginChunk()
    { }

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupPluginCurvesList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.CurvesPlugin] = new ParametricCurvesPluginChunk();
    }

    private static void DupPluginCurvesList(ref Context ctx, in Context src)
    {
        ParametricCurvesPluginChunk newHead = new();
        ParametricCurvesCollection? anterior = null;
        var head = (ParametricCurvesPluginChunk?)src.chunks[(int)Chunks.CurvesPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.parametricCurves; entry is not null; entry = entry.next)
        {
            // We want to keep the linked list order, so this is a little bit tricky
            ParametricCurvesCollection newEntry = new(entry);

            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.parametricCurves is null)
                newHead.parametricCurves = newEntry;
        }

        ctx.chunks[(int)Chunks.CurvesPlugin] = newHead;
    }
}
