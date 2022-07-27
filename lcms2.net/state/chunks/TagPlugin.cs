using System.Diagnostics;

using lcms2.plugins;

namespace lcms2.state.chunks;

internal class TagPlugin
{
    private TagLinkedList? tags = null;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupTagList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.TagPlugin] = tagsPluginChunk;
    }

    private TagPlugin() { }

    internal static TagPlugin global = new();
    private readonly static TagPlugin tagsPluginChunk = new();

    private static void DupTagList(ref Context ctx, in Context src)
    {
        TagPlugin newHead = new();
        TagLinkedList? anterior = null;
        var head = (TagPlugin?)src.chunks[(int)Chunks.TagPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.tags; entry is not null; entry = entry.next)
        {
            TagLinkedList newEntry = new()
            {
                // We want to keep the linked list order, so this is a little bit tricky
                next = null
            };
            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.tags is null)
                newHead.tags = newEntry;
        }

        ctx.chunks[(int)Chunks.TagPlugin] = newHead;
    }
}
