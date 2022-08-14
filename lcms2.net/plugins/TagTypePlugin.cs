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

public class TagTypeLinkedList
{
    public TagTypeHandler Handler;

    public TagTypeLinkedList? Next;

    public TagTypeLinkedList(TagTypeHandler handler, TagTypeLinkedList? next)
    {
        Handler = handler;
        Next = next;
    }

    public TagTypeLinkedList(ReadOnlySpan<TagTypeHandler> list)
    {
        Handler = list[0];
        Next = list.Length > 1 ? new(list[1..]) : null;
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
        internal static void Alloc(Context ctx, in Context? src)
        {
            if (src is not null)
                DupTagTypeList(ctx, src, Chunks.TagTypePlugin);
            else
                ctx.chunks[(int)Chunks.TagTypePlugin] = tagTypePluginChunk;
        }
        internal static bool RegisterPlugin(Context? context, Plugin? plugin) =>
            RegisterTypesPlugin(context, plugin, Chunks.TagTypePlugin);

        internal static TagTypePluginChunk global = new();
    }
    internal static class MPE
    {
        internal static void Alloc(Context ctx, in Context? src)
        {
            if (src is not null)
                DupTagTypeList(ctx, src, Chunks.MPEPlugin);
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
        new ChromaticityHandler(Signature.TagType.Chromaticity),
        new ColorantOrderHandler(Signature.TagType.ColorantOrder),
        new S15Fixed16Handler(Signature.TagType.S15Fixed16Array),
        new U16Fixed16Handler(Signature.TagType.U16Fixed16Array),
        new TextHandler(Signature.TagType.Text),
        new TextDescriptionHandler(Signature.TagType.TextDescription),
        new CurveHandler(Signature.TagType.Curve),
        new ParametricCurveHandler(Signature.TagType.ParametricCurve),
        new DateTimeHandler(Signature.TagType.DateTime),
        new Lut8Handler(Signature.TagType.Lut8),
        new Lut16Handler(Signature.TagType.Lut16),
        new ColorantTableHandler(Signature.TagType.ColorantTable),
        new NamedColorHandler(Signature.TagType.NamedColor2),
        new MluHandler(Signature.TagType.MultiLocalizedUnicode),
        new ProfileSequenceDescriptionHandler(Signature.TagType.ProfileSequenceDesc),
        new SignatureHandler(Signature.TagType.Signature),
        new MeasurementHandler(Signature.TagType.Measurement),
        new DataHandler(Signature.TagType.Data),
        new LutA2BHandler(Signature.TagType.LutAtoB),
        new LutB2AHandler(Signature.TagType.LutBtoA),
        new UcrBgHandler(Signature.TagType.UcrBg),
        new CrdInfoHandler(Signature.TagType.CrdInfo),
        new MpeHandler(Signature.TagType.MultiProcessElement),
        new ScreeningHandler(Signature.TagType.Screening),
        new ViewingConditionsHandler(Signature.TagType.ViewingConditions),
        new XYZHandler(Signature.TagType.XYZ),
        new XYZHandler(Signature.TagType.CorbisBrokenXYZ),
        new CurveHandler(Signature.TagType.MonacoBrokenCurve),
        new ProfileSequenceIdHandler(Signature.TagType.ProfileSequenceId),
        new DictionaryHandler(Signature.TagType.Dict),
        new VcgtHandler(Signature.TagType.Vcgt),
    });

    internal static readonly TagTypeLinkedList SupportedMpeTypes = new(new TagTypeHandler[] {
        // Ignore these elements for now (That's what the spec says)
        new MpeStubHandler(Signature.Stage.BAcsElem),
        new MpeStubHandler(Signature.Stage.EAcsElem),

        new MpeCurveHandler(),
        new MpeMatrixHandler(),
        new MpeClutHandler(),
    });

    private static readonly TagTypePluginChunk tagTypePluginChunk = new();
    private static void DupTagTypeList(Context ctx, in Context src, Chunks loc)
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