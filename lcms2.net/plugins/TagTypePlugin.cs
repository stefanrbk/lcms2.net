using System.Diagnostics;

using lcms2.state;
using lcms2.types;
using lcms2.types.type_handlers;

namespace lcms2.plugins;

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

/// <summary>
///     Tag type handler
/// </summary>
/// <remarks>Implements the <c>cmsPluginTagType</c> struct.</remarks>
public sealed class TagTypePlugin: Plugin
{
    public TagTypeHandler Handler;

    public TagTypePlugin(Signature magic, uint expectedVersion, Signature type, TagTypeHandler handler)
        : base(magic, expectedVersion, type) =>
        Handler = handler;

    internal static bool RegisterPlugin(object? state, TagTypePlugin? plugin) =>
        TagTypePluginChunk.TagType.RegisterPlugin(state, plugin);
}

internal sealed class TagTypePluginChunk
{
    internal static readonly TagTypeLinkedList supportedMpeTypes = new(new TagTypeHandler[]
    {
        // Ignore these elements for now (That's what the spec says)
        new MpeStubHandler(Signature.Stage.BAcsElem),
        new MpeStubHandler(Signature.Stage.EAcsElem),

        new MpeCurveHandler(Signature.Stage.CurveSetElem),
        new MpeMatrixHandler(Signature.Stage.MatrixElem),
        new MpeClutHandler(Signature.Stage.CLutElem),
    });

    internal static readonly TagTypeLinkedList supportedTagTypes = new(new TagTypeHandler[]
    {
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

    internal TagTypeLinkedList? tagTypes;

    internal static TagTypePluginChunk Default => new();

    private TagTypePluginChunk()
    { }

    private static bool RegisterTypesPlugin(object? state, Plugin? data, bool isMpePlugin)
    {
        TagTypePluginChunk sta;
        TagTypeHandler handler;

        if (data is null) return false;

        if (isMpePlugin)
        {
                sta = State.GetMultiProcessElementPlugin(state);
                handler = ((MultiProcessElementPlugin)data).Handler;
        } else
        {
                sta = State.GetTagTypePlugin(state);
                handler = ((TagTypePlugin)data).Handler;
        }

        if (data is null)
        {
            sta.tagTypes = null;
            return true;
        }

        var pt = new TagTypeLinkedList(handler, sta.tagTypes);

        if (pt.Handler is null)
            return false;

        sta.tagTypes = pt;
        return true;
    }

    internal static class MPE
    {
        internal static TagTypePluginChunk global = new();

        internal static bool RegisterPlugin(object? context, Plugin? plugin) =>
            RegisterTypesPlugin(context, plugin, isMpePlugin: true);
    }

    internal static class TagType
    {
        internal static TagTypePluginChunk global = new();

        internal static bool RegisterPlugin(object? context, Plugin? plugin) =>
            RegisterTypesPlugin(context, plugin, isMpePlugin: false);
    }
}

public delegate bool PositionTableEntryFn(TagTypeHandler self, Stream io, ref object cargo, int n, int sizeOfTag);
