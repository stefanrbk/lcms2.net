using System.Diagnostics;

using lcms2.plugins;

namespace lcms2.state.chunks;

internal class TagTypePlugin
{
    private TagTypeLinkedList? tagTypes = null;

    internal static class TagType
    {
        internal static void Alloc(ref Context ctx, in Context? src)
        {
            if (src is not null)
                DupTagTypeList(ref ctx, src, Chunks.TagTypePlugin);
            else
                ctx.chunks[(int)Chunks.TagTypePlugin] = tagTypePluginChunk;
        }

        internal static TagTypePlugin global = new();
    }
    internal static class MPE
    {
        internal static void Alloc(ref Context ctx, in Context? src)
        {
            if (src is not null)
                DupTagTypeList(ref ctx, src, Chunks.MPEPlugin);
            else
                ctx.chunks[(int)Chunks.MPEPlugin] = tagTypePluginChunk;
        }

        internal static TagTypePlugin global = new();
    }

    private TagTypePlugin()
    { }

    private static readonly TagTypePlugin tagTypePluginChunk = new();
    private static void DupTagTypeList(ref Context ctx, in Context src, Chunks loc)
    {
        TagTypePlugin newHead = new();
        TagTypeLinkedList? anterior = null;
        var head = (TagTypePlugin?)src.chunks[(int)loc];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.tagTypes; entry is not null; entry = entry.next)
        {
            TagTypeLinkedList newEntry = new()
            {
                // We want to keep the linked list order, so this is a little bit tricky
                next = null
            };
            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.tagTypes is null)
                newHead.tagTypes = newEntry;
        }

        ctx.chunks[(int)loc] = newHead;
    }
}
