using System.Diagnostics;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

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
    public TagTypeDecider? DecideType;

    public TagDescriptor(int elementCount, int numSupportedTypes, TagTypeDecider? decider)
    {
        ElementCount = elementCount;
        SupportedTypes = new Signature[numSupportedTypes];
        DecideType = decider;
    }

    public TagDescriptor(int elementCount, Signature[] supportedTypes, TagTypeDecider? decider)
    {
        ElementCount = elementCount;
        SupportedTypes = supportedTypes;
        DecideType = decider;
    }
}
public class TagLinkedList
{
    public Signature Signature;
    public TagDescriptor Descriptor;

    public TagLinkedList? Next;

    public TagLinkedList(Signature signature, TagDescriptor descriptor, TagLinkedList? next)
    {
        this.Signature = signature;
        this.Descriptor = descriptor;
        this.Next = next;
    }

    public TagLinkedList(ReadOnlySpan<(Signature sig, TagDescriptor desc)> list)
    {
        Signature = list[0].sig;
        Descriptor = list[0].desc;
        Next = list.Length > 1 ? new(list[1..]) : null;
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
            ctx.chunks[(int)Chunks.TagPlugin] = new TagPluginChunk();
    }

    private TagPluginChunk()
    { }

    internal static readonly TagLinkedList SupportedTags = new(new (Signature sig, TagDescriptor desc)[]
    {
        (Signature.Tag.AToB0, TagHandlers.AToB),
        (Signature.Tag.AToB1, TagHandlers.AToB),
        (Signature.Tag.AToB2, TagHandlers.AToB),
        (Signature.Tag.BToA0, TagHandlers.BToA),
        (Signature.Tag.BToA1, TagHandlers.BToA),
        (Signature.Tag.BToA2, TagHandlers.BToA),

        (Signature.Tag.RedColorant, TagHandlers.XyzEx),
        (Signature.Tag.GreenColorant, TagHandlers.XyzEx),
        (Signature.Tag.BlueColorant, TagHandlers.XyzEx),

        (Signature.Tag.RedTRC, TagHandlers.CurveEx),
        (Signature.Tag.GreenTRC, TagHandlers.CurveEx),
        (Signature.Tag.BlueTRC, TagHandlers.CurveEx),

        (Signature.Tag.CalibrationDateTime, TagHandlers.DateTime),
        (Signature.Tag.CharTarget, new TagDescriptor(1, new Signature[] { Signature.TagType.Text }, null)),

        (Signature.Tag.ChromaticAdaptation, TagHandlers.S15Fixed16Array),
        (Signature.Tag.Chromaticity, new TagDescriptor(1, new Signature[] { Signature.TagType.Chromaticity }, null)),
        (Signature.Tag.ColorantOrder, new TagDescriptor(1, new Signature[] { Signature.TagType.ColorantOrder }, null)),
        (Signature.Tag.ColorantTable, TagHandlers.ColorantTable),
        (Signature.Tag.ColorantTableOut, TagHandlers.ColorantTable),

        (Signature.Tag.Copyright, TagHandlers.Text),
        (Signature.Tag.DateTime, TagHandlers.DateTime),

        (Signature.Tag.DeviceMfgDesc, TagHandlers.TextDescription),
        (Signature.Tag.DeviceModelDesc, TagHandlers.TextDescription),

        (Signature.Tag.Gamut, TagHandlers.BToA),

        (Signature.Tag.GrayTRC, TagHandlers.Curve),
        (Signature.Tag.Luminance, TagHandlers.Xyz),

        (Signature.Tag.MediaBlackPoint, TagHandlers.XyzEx),
        (Signature.Tag.MediaWhitePoint, TagHandlers.XyzEx),

        (Signature.Tag.NamedColor2, new TagDescriptor(1, new Signature[] { Signature.TagType.NamedColor2 }, null)),

        (Signature.Tag.Preview0, TagHandlers.BToA),
        (Signature.Tag.Preview1, TagHandlers.BToA),
        (Signature.Tag.Preview2, TagHandlers.BToA),

        (Signature.Tag.ProfileDescription, TagHandlers.TextDescription),
        (Signature.Tag.ProfileSequenceDesc, new TagDescriptor(1, new Signature[] { Signature.TagType.ProfileSequenceDesc }, null)),
        (Signature.Tag.Technology, TagHandlers.Signature),

        (Signature.Tag.ColorimetricIntentImageState, TagHandlers.Signature),
        (Signature.Tag.PerceptualRenderingIntentGamut, TagHandlers.Signature),
        (Signature.Tag.SaturationRenderingIntentGamut, TagHandlers.Signature),

        (Signature.Tag.Measurement, new TagDescriptor(1, new Signature[] { Signature.TagType.Measurement }, null)),

        (Signature.Tag.Ps2CRD0, TagHandlers.Signature),
        (Signature.Tag.Ps2CRD1, TagHandlers.Signature),
        (Signature.Tag.Ps2CRD2, TagHandlers.Signature),
        (Signature.Tag.Ps2CRD3, TagHandlers.Signature),
        (Signature.Tag.Ps2CSA, TagHandlers.Signature),
        (Signature.Tag.Ps2RenderingIntent, TagHandlers.Signature),

        (Signature.Tag.ViewingCondDesc, TagHandlers.TextDescription),

        (Signature.Tag.UcrBg, new TagDescriptor(1, new Signature[] { Signature.TagType.UcrBg }, null)),
        (Signature.Tag.CrdInfo, new TagDescriptor(1, new Signature[] { Signature.TagType.CrdInfo }, null)),

        (Signature.Tag.DToB0, TagHandlers.MultiProcessElement),
        (Signature.Tag.DToB1, TagHandlers.MultiProcessElement),
        (Signature.Tag.DToB2, TagHandlers.MultiProcessElement),
        (Signature.Tag.DToB3, TagHandlers.MultiProcessElement),
        (Signature.Tag.BToD0, TagHandlers.MultiProcessElement),
        (Signature.Tag.BToD1, TagHandlers.MultiProcessElement),
        (Signature.Tag.BToD2, TagHandlers.MultiProcessElement),
        (Signature.Tag.BToD3, TagHandlers.MultiProcessElement),

        (Signature.Tag.ScreeningDesc, new TagDescriptor(1, new Signature[] { Signature.TagType.TextDescription }, null)),
        (Signature.Tag.ViewingConditions, new TagDescriptor(1, new Signature[] { Signature.TagType.ViewingConditions }, null)),

        (Signature.Tag.Screening, new TagDescriptor(1, new Signature[] { Signature.TagType.Screening }, null)),
        (Signature.Tag.Vcgt, new TagDescriptor(1, new Signature[] { Signature.TagType.Vcgt }, null)),
        (Signature.Tag.Meta, new TagDescriptor(1, new Signature[] { Signature.TagType.Dict }, null)),
        (Signature.Tag.ProfileSequenceId, new TagDescriptor(1, new Signature[] { Signature.TagType.ProfileSequenceId }, null)),

        (Signature.Tag.ProfileDescriptionML, new TagDescriptor(1, new Signature[] { Signature.TagType.MultiLocalizedUnicode }, null)),
        (Signature.Tag.ArgyllArts, TagHandlers.S15Fixed16Array),

        /*
            Not supported                 Why
            =======================       =========================================
            cmsSigOutputResponseTag   ==> WARNING, POSSIBLE PATENT ON THIS SUBJECT!
            cmsSigNamedColorTag       ==> Deprecated
            cmsSigDataTag             ==> Ancient, unused
            cmsSigDeviceSettingsTag   ==> Deprecated, useless
        */
    });

    internal static TagPluginChunk global = new();

    private static void DupTagList(ref Context ctx, in Context src)
    {
        TagPluginChunk newHead = new();
        TagLinkedList? anterior = null;
        var head = (TagPluginChunk?)src.chunks[(int)Chunks.TagPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.tags; entry is not null; entry = entry.Next) {
            // We want to keep the linked list order, so this is a little bit tricky
            TagLinkedList newEntry = new(entry.Signature, entry.Descriptor, null);

            if (anterior is not null)
                anterior.Next = newEntry;

            anterior = newEntry;

            if (newHead.tags is null)
                newHead.tags = newEntry;
        }

        ctx.chunks[(int)Chunks.TagPlugin] = newHead;
    }

    internal TagDescriptor GetTagDescriptor(Context? context, Signature sig)
    {
        var chunk = Context.GetTagPlugin(context);

        for (var pt = chunk.tags; pt is not null; pt = pt.Next)

            if (sig == pt.Signature) return pt.Descriptor;

        for (var pt = SupportedTags; pt is not null; pt = pt.Next)

            if (sig == pt.Signature) return pt.Descriptor;

        return null;
    }
}
