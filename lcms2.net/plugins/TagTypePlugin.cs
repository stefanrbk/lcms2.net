using System.Diagnostics;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Tag type handler
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginTagType</c> struct.</remarks>
public sealed class TagTypePlugin : Plugin
{
    public ITagTypeHandler handler;
    public TagTypePlugin(Signature magic, uint expectedVersion, Signature type, ITagTypeHandler handler)
        : base(magic, expectedVersion, type)
    {
        this.handler = handler;
    }

    internal static bool RegisterPlugin(Context? context, TagTypePlugin? plugin) =>
        TagTypePluginChunk.TagType.RegisterPlugin(context, plugin);
}

/// <summary>
///     Tag type handler
/// </summary>
/// <remarks>
///     Each type is free to return anything it wants, and it is up to the caller to
///     know in advance what is the type contained in the tag.<br />
///     Implements the <c>cmsTagTypeHandler</c> struct.</remarks>
public interface ITagTypeHandler
{
    /// <summary>
    ///     Signature of the type
    /// </summary>
    Signature Signature { get; }

    /// <summary>
    ///     Additional parameter used by the calling thread
    /// </summary>
    Context Context { get; }

    /// <summary>
    ///     Additional parameter used by the calling thread
    /// </summary>
    uint ICCVersion { get; }

    /// <summary>
    ///     Allocates and reads items.
    /// </summary>
    (object Value, int Count)? Read(ITagTypeHandler handler, Stream io, int sizeOfTag);

    /// <summary>
    ///     Writes n Items
    /// </summary>
    bool Write(ITagTypeHandler handler, Stream io, object value, int numItems);

    /// <summary>
    ///     Duplicate an item or array of items
    /// </summary>
    object? Duplicate(ITagTypeHandler handler, object value, int num);

    /// <summary>
    ///     Free all resources
    /// </summary>
    void Free(ITagTypeHandler handler, object value);
}

public class TagTypeLinkedList
{
    internal ITagTypeHandler? factory;

    internal TagTypeLinkedList? next;

    internal TagTypeLinkedList(ITagTypeHandler? factory, TagTypeLinkedList? next)
    {
        this.factory = factory;
        this.next = next;
    }
}

internal sealed class TagTypePluginChunk
{
    internal TagTypeLinkedList? tagTypes;

    private static bool RegisterTypesPlugin(Context? context, Plugin? data, Chunks type)
    {
        var ctx = (TagTypePluginChunk)Context.GetClientChunk(context, type)!;

        if (data is null)
        {
            ctx.tagTypes = null;
            return true;
        }

        var pt = new TagTypeLinkedList(data switch
        {
            TagTypePlugin p => p.handler,
            MultiProcessElementPlugin p => p.Handler,
            _ => null
        }, ctx.tagTypes);

        if (pt.factory is null)
            return false;

        ctx.tagTypes = pt;
        return true;
    }

    internal static class TagType
    {
        internal static void Alloc(ref Context ctx, in Context? src)
        {
            if (src is not null)
                DupTagTypeList(ref ctx, src, Chunks.TagTypePlugin);
            else
                ctx.chunks[(int)Chunks.TagTypePlugin] = tagTypePluginChunk;
        }
        internal static bool RegisterPlugin(Context? context, Plugin? plugin) =>
            RegisterTypesPlugin(context, plugin, Chunks.TagTypePlugin);

        internal static TagTypePluginChunk global = new();
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
        internal static bool RegisterPlugin(Context? context, Plugin? plugin) =>
            RegisterTypesPlugin(context, plugin, Chunks.MPEPlugin);

        internal static TagTypePluginChunk global = new();
    }

    private TagTypePluginChunk()
    { }

    private static readonly TagTypePluginChunk tagTypePluginChunk = new();
    private static void DupTagTypeList(ref Context ctx, in Context src, Chunks loc)
    {
        TagTypePluginChunk newHead = new();
        TagTypeLinkedList? anterior = null;
        var head = (TagTypePluginChunk?)src.chunks[(int)loc];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.tagTypes; entry is not null; entry = entry.next)
        {
            TagTypeLinkedList newEntry = new(entry.factory, null);

            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.tagTypes is null)
                newHead.tagTypes = newEntry;
        }

        ctx.chunks[(int)loc] = newHead;
    }
}