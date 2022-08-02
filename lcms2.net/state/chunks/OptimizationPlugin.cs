using System.Diagnostics;

using lcms2.plugins;

namespace lcms2.state.chunks;

internal class OptimizationPlugin
{
    private OptimizationCollection? optimizationCollection = null;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupOptimizationList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.OptimizationPlugin] = tagsPluginChunk;
    }

    private OptimizationPlugin()
    { }

    internal static OptimizationPlugin global = new();
    private static readonly OptimizationPlugin tagsPluginChunk = new();

    private static void DupOptimizationList(ref Context ctx, in Context src)
    {
        OptimizationPlugin newHead = new();
        OptimizationCollection? anterior = null;
        var head = (OptimizationPlugin?)src.chunks[(int)Chunks.OptimizationPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.optimizationCollection; entry is not null; entry = entry.next)
        {
            OptimizationCollection newEntry = new()
            {
                // We want to keep the linked list order, so this is a little bit tricky
                next = null
            };
            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.optimizationCollection is null)
                newHead.optimizationCollection = newEntry;
        }

        ctx.chunks[(int)Chunks.OptimizationPlugin] = newHead;
    }
}
