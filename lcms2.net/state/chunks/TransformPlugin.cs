using System.Diagnostics;

using lcms2.plugins;

namespace lcms2.state.chunks;

internal class TransformPlugin
{
    private TransformCollection? transformCollection = null;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupTransformList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.TransformPlugin] = transformChunk;
    }

    private TransformPlugin()
    { }

    internal static TransformPlugin global = new();
    private static readonly TransformPlugin transformChunk = new();

    private static void DupTransformList(ref Context ctx, in Context src)
    {
        TransformPlugin newHead = new();
        TransformCollection? anterior = null;
        var head = (TransformPlugin?)src.chunks[(int)Chunks.TransformPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.transformCollection; entry is not null; entry = entry.next)
        {
            TransformCollection newEntry = new()
            {
                // We want to keep the linked list order, so this is a little bit tricky
                next = null
            };
            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.transformCollection is null)
                newHead.transformCollection = newEntry;
        }

        ctx.chunks[(int)Chunks.TransformPlugin] = newHead;
    }
}
