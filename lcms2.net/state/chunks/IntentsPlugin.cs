using System.Diagnostics;

using lcms2.plugins;

namespace lcms2.state.chunks;

internal class IntentsPlugin
{
    private IntentsList? intents = null;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupIntentsList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.IntentPlugin] = intentsPluginChunk;
    }

    private IntentsPlugin()
    { }

    internal static IntentsPlugin global = new();
    private static readonly IntentsPlugin intentsPluginChunk = new();

    private static void DupIntentsList(ref Context ctx, in Context src)
    {
        IntentsPlugin newHead = new();
        IntentsList? anterior = null;
        var head = (IntentsPlugin?)src.chunks[(int)Chunks.IntentPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.intents; entry is not null; entry = entry.next)
        {
            IntentsList newEntry = new()
            {
                // We want to keep the linked list order, so this is a little bit tricky
                next = null
            };
            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.intents is null)
                newHead.intents = newEntry;
        }

        ctx.chunks[(int)Chunks.IntentPlugin] = newHead;
    }
}
