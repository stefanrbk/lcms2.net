using System.Diagnostics;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public delegate Signature TagTypeDecider(double iccVersion, ref object data);

/// <summary>
///     Tag identification plugin
/// </summary>
/// <remarks>
///     Plugin implements a single tag<br />
///     Implements the <c>cmsPluginTag</c> struct.</remarks>
public sealed class TagPlugin
    : Plugin
{
    public Signature Signature;
    public TagDescriptor Descriptor;

    public TagPlugin(Signature magic, uint expectedVersion, Signature type, Signature signature, TagDescriptor descriptor)
        : base(magic, expectedVersion, type)
    {
        Signature = signature;
        Descriptor = descriptor;
    }

    internal static bool RegisterPlugin(Context? context, TagPlugin? plugin)
    {
        var chunk = (TagPluginChunk)Context.GetClientChunk(context, Chunks.TagPlugin)!;

        if (plugin is null) {
            chunk.tags = null;
            return true;
        }

        chunk.tags =
            new TagLinkedList(plugin.Signature, plugin.Descriptor, chunk.tags);

        return true;
    }
}

/// <summary>
///     Tag identification plugin descriptor
/// </summary>
/// <remarks>
///     Implements the <c>cmsTagDescriptor</c> struct.</remarks>
public class TagDescriptor
{
    /// <summary>
    ///     If this tag needs an array, how many elements should be kept
    /// </summary>
    public int ElementCount;
    /// <summary>
    ///     For reading
    /// </summary>
    public Signature[] SupportedTypes;
    /// <summary>
    ///     For writing
    /// </summary>
    public TagTypeDecider DecideType;

    public TagDescriptor(int elementCount, int numSupportedTypes, TagTypeDecider decider)
    {
        ElementCount = elementCount;
        SupportedTypes = new Signature[numSupportedTypes];
        DecideType = decider;
    }
}
public class TagLinkedList
{
    internal Signature signature;
    internal TagDescriptor descriptor;

    internal TagLinkedList? next;

    internal TagLinkedList(Signature signature, TagDescriptor descriptor, TagLinkedList? next)
    {
        this.signature = signature;
        this.descriptor = descriptor;
        this.next = next;
    }
}

internal sealed class TagPluginChunk
{
    internal TagLinkedList? tags;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupTagList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.TagPlugin] = tagsPluginChunk;
    }

    private TagPluginChunk()
    { }

    internal static TagPluginChunk global = new();
    private static readonly TagPluginChunk tagsPluginChunk = new();

    private static void DupTagList(ref Context ctx, in Context src)
    {
        TagPluginChunk newHead = new();
        TagLinkedList? anterior = null;
        var head = (TagPluginChunk?)src.chunks[(int)Chunks.TagPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.tags; entry is not null; entry = entry.next) {
            // We want to keep the linked list order, so this is a little bit tricky
            TagLinkedList newEntry = new(entry.signature, entry.descriptor, null);

            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.tags is null)
                newHead.tags = newEntry;
        }

        ctx.chunks[(int)Chunks.TagPlugin] = newHead;
    }
}
