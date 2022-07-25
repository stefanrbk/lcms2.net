using System.Diagnostics;

using lcms2.plugins;

namespace lcms2.state.chunks;

internal class FormattersPlugin
{
    private FormattersFactoryList? factoryList = null;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupFormatterFactoryList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.FormattersPlugin] = curvesPluginChunk;
    }

    private FormattersPlugin() { }

    internal static FormattersPlugin global = new();
    private readonly static FormattersPlugin curvesPluginChunk = new();

    private static void DupFormatterFactoryList(ref Context ctx, in Context src)
    {
        FormattersPlugin newHead = new();
        FormattersFactoryList? anterior = null;
        var head = (FormattersPlugin?)src.chunks[(int)Chunks.FormattersPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.factoryList; entry is not null; entry = entry.next)
        {
            FormattersFactoryList newEntry = new()
            {
                // We want to keep the linked list order, so this is a little bit tricky
                next = null
            };
            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.factoryList is null)
                newHead.factoryList = newEntry;
        }

        ctx.chunks[(int)Chunks.FormattersPlugin] = newHead;
    }
}
