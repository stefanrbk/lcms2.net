using System.Diagnostics;

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
public sealed class ParametricCurvesPlugin : Plugin
{
    public (int Types, int Count)[] Functions;

    /// <summary>
    ///     The evaluator
    /// </summary>
    public ParametricCurveEvaluator Evaluator;

    public ParametricCurvesPlugin(Signature magic, uint expectedVersion, Signature type, (int Types, int Count)[] functions, ParametricCurveEvaluator evaluator)
        : base(magic, expectedVersion, type)
    {
        Functions = functions;

        Evaluator = evaluator;
    }

    internal static bool RegisterPlugin(Context? context, ParametricCurvesPlugin? plugin)
    {
        var ctx = Context.GetCurvesPlugin(context);

        if (plugin is null) {
            ctx.parametricCurves = null;
            return true;
        }

        ctx.parametricCurves = new ParametricCurvesCollection(plugin.Functions, plugin.Evaluator, ctx.parametricCurves);

        return true;
    }
}

internal class ParametricCurvesCollection
{
    internal (int Types, int Count)[] Functions = new (int, int)[Lcms2.MaxTypesInPlugin];

    internal ParametricCurveEvaluator? Evaluator;

    internal ParametricCurvesCollection? Next;

    public const int MaxInputDimensions = 15;

    public ParametricCurvesCollection((int Types, int Count)[] functions, ParametricCurveEvaluator? evaluator, ParametricCurvesCollection? next)
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
}

internal sealed class ParametricCurvesPluginChunk
{
    internal ParametricCurvesCollection? parametricCurves;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupPluginCurvesList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.InterpPlugin] = curvesPluginChunk;
    }

    private ParametricCurvesPluginChunk()
    { }

    internal static ParametricCurvesPluginChunk global = new();
    private static readonly ParametricCurvesPluginChunk curvesPluginChunk = new();

    private static void DupPluginCurvesList(ref Context ctx, in Context src)
    {
        ParametricCurvesPluginChunk newHead = new();
        ParametricCurvesCollection? anterior = null;
        var head = (ParametricCurvesPluginChunk?)src.chunks[(int)Chunks.CurvesPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.parametricCurves; entry is not null; entry = entry.Next) {
            // We want to keep the linked list order, so this is a little bit tricky
            ParametricCurvesCollection newEntry = new(entry);

            if (anterior is not null)
                anterior.Next = newEntry;

            anterior = newEntry;

            if (newHead.parametricCurves is null)
                newHead.parametricCurves = newEntry;
        }

        ctx.chunks[(int)Chunks.CurvesPlugin] = newHead;
    }
}
