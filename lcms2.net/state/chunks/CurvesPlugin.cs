using System.Diagnostics;

using lcms2.plugins;

namespace lcms2.state.chunks;

internal class CurvesPlugin
{
    private ParametricCurvesCollection? parametricCurves = null;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupPluginCurvesList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.InterpPlugin] = curvesPluginChunk;
    }

    private CurvesPlugin()
    { }

    internal static CurvesPlugin global = new();
    private static readonly CurvesPlugin curvesPluginChunk = new();

    private static void DupPluginCurvesList(ref Context ctx, in Context src)
    {
        CurvesPlugin newHead = new();
        ParametricCurvesCollection? anterior = null;
        var head = (CurvesPlugin?)src.chunks[(int)Chunks.CurvesPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.parametricCurves; entry is not null; entry = entry.next)
        {
            ParametricCurvesCollection newEntry = new()
            {
                // We want to keep the linked list order, so this is a little bit tricky
                next = null
            };
            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.parametricCurves is null)
                newHead.parametricCurves = newEntry;
        }

        ctx.chunks[(int)Chunks.CurvesPlugin] = newHead;
    }
}
