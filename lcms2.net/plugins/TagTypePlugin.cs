using System.Diagnostics;

using lcms2.state;
using lcms2.types;
using lcms2.types.type_handlers;

namespace lcms2.plugins;

/// <summary>
///     Tag type handler
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginTagType</c> struct.</remarks>
public sealed class TagTypePlugin : Plugin
{
    public TagTypeHandler handler;
    public TagTypePlugin(Signature magic, uint expectedVersion, Signature type, TagTypeHandler handler)
        : base(magic, expectedVersion, type) =>
        this.handler = handler;

    internal static bool RegisterPlugin(Context? context, TagTypePlugin? plugin) =>
        TagTypePluginChunk.TagType.RegisterPlugin(context, plugin);
}

internal class TagTypeLinkedList
{
    internal TagTypeHandler Handler;

    internal TagTypeLinkedList? Next;

    internal TagTypeLinkedList(TagTypeHandler handler, TagTypeLinkedList? next)
    {
        Handler = handler;
        Next = next;
    }

    internal TagTypeLinkedList(ReadOnlySpan<TagTypeHandler> list)
    {
        Handler = list[0];
        Next = list.Length > 1 ? new(list[1..]) : null;
    }

    public static TagTypeHandler? GetHandler(Signature sig, TagTypeLinkedList pluginList, TagTypeLinkedList defaultList)
    {
        for (var pt = pluginList; pt is not null; pt = pt.Next) {
            if (sig == pt.Handler.Signature)
                return pt.Handler;
        }
        for (var pt = defaultList; pt is not null; pt = pt.Next) {
            if (sig == pt.Handler.Signature)
                return pt.Handler;
        }

        return null;
    }
}

internal sealed class TagTypePluginChunk
{
    internal TagTypeLinkedList? tagTypes;

    private static bool RegisterTypesPlugin(Context? context, Plugin? data, Chunks type)
    {
        var ctx = (TagTypePluginChunk)Context.GetClientChunk(context, type)!;

        if (data is null) {
            ctx.tagTypes = null;
            return true;
        }

        var pt = new TagTypeLinkedList(data switch
        {
            TagTypePlugin p => p.handler,
            MultiProcessElementPlugin p => p.Handler,
            _ => null
        }, ctx.tagTypes);

        if (pt.Handler is null)
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

    internal static readonly TagTypeLinkedList SupportedTagTypes = new(new TagTypeHandler[] {
        new ChromaticityHandler(),
        new ColorantOrderHandler(),
        new ColorantTableHandler(),
        new CrdInfoHandler(),
        new CurveHandler(),
        new DataHandler(),
        new DateTimeHandler(),
        new Lut16Handler(),
        new Lut8Handler(),
        new LutA2BHandler(),
        new LutB2AHandler(),
        new MeasurementHandler(),
        new MluHandler(),
        new NamedColorHandler(),
        new ParametricCurveHandler(),
        new ProfileSequenceDescriptionHandler(),
        new ProfileSequenceIdHandler(),
        new S15Fixed16Handler(),
        new ScreeningHandler(),
        new SignatureHandler(),
        new TextDescriptionHandler(),
        new TextHandler(),
        new U16Fixed16Handler(),
        new UcrBgHandler(),
        new ViewingConditionsHandler(),
        new XYZHandler(),
    });

    internal static readonly TagTypeLinkedList SupportedMpeTypes = new(new TagTypeHandler[] {
        new MpeCurveHandler(),
        new MpeMatrixHandler(),
        new MpeClutHandler(),
    });

    private static readonly TagTypePluginChunk tagTypePluginChunk = new();
    private static void DupTagTypeList(ref Context ctx, in Context src, Chunks loc)
    {
        TagTypePluginChunk newHead = new();
        TagTypeLinkedList? anterior = null;
        var head = (TagTypePluginChunk?)src.chunks[(int)loc];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.tagTypes; entry is not null; entry = entry.Next) {
            TagTypeLinkedList newEntry = new(entry.Handler, null);

            if (anterior is not null)
                anterior.Next = newEntry;

            anterior = newEntry;

            if (newHead.tagTypes is null)
                newHead.tagTypes = newEntry;
        }

        ctx.chunks[(int)loc] = newHead;
    }
}

public delegate bool PositionTableEntryFn(TagTypeHandler self, Stream io, ref object cargo, int n, int sizeOfTag);