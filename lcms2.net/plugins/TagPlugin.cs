//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//---------------------------------------------------------------------------------
//
using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Tag identification plugin descriptor
/// </summary>
/// <remarks>Implements the <c>cmsTagDescriptor</c> struct.</remarks>
public class TagDescriptor
{
    #region Fields

    /// <summary>
    ///     For writing
    /// </summary>
    public TagTypeDecider? DecideType;

    /// <summary>
    ///     If this tag needs an array, how many elements should be kept
    /// </summary>
    public int ElementCount;

    /// <summary>
    ///     For reading
    /// </summary>
    public Signature[] SupportedTypes;

    #endregion Fields

    #region Public Constructors

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

    #endregion Public Constructors
}

public class TagLinkedList
{
    #region Fields

    public TagDescriptor Descriptor;
    public TagLinkedList? Next;
    public Signature Signature;

    #endregion Fields

    #region Public Constructors

    public TagLinkedList(Signature signature, TagDescriptor descriptor, TagLinkedList? next)
    {
        Signature = signature;
        Descriptor = descriptor;
        Next = next;
    }

    public TagLinkedList(ReadOnlySpan<(Signature sig, TagDescriptor desc)> list)
    {
        Signature = list[0].sig;
        Descriptor = list[0].desc;
        Next = list.Length > 1 ? new(list[1..]) : null;
    }

    #endregion Public Constructors
}

/// <summary>
///     Tag identification plugin
/// </summary>
/// <remarks>Plugin implements a single tag <br/> Implements the <c>cmsPluginTag</c> struct.</remarks>
public sealed class TagPlugin
    : Plugin
{
    #region Fields

    public TagDescriptor Descriptor;
    public Signature Signature;

    #endregion Fields

    #region Public Constructors

    public TagPlugin(Signature magic, uint expectedVersion, Signature type, Signature signature, TagDescriptor descriptor)
        : base(magic, expectedVersion, type)
    {
        Signature = signature;
        Descriptor = descriptor;
    }

    #endregion Public Constructors

    #region Internal Methods

    internal static bool RegisterPlugin(object? context, TagPlugin? plugin)
    {
        var chunk = State.GetTagPlugin(context);

        if (plugin is null)
        {
            chunk.tags = null;
            return true;
        }

        chunk.tags =
            new TagLinkedList(plugin.Signature, plugin.Descriptor, chunk.tags);

        return true;
    }

    #endregion Internal Methods
}

internal sealed class TagPluginChunk
{
    #region Fields

    internal static readonly TagLinkedList supportedTags = new(new (Signature sig, TagDescriptor desc)[]
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
    internal TagLinkedList? tags;

    #endregion Fields

    #region Private Constructors

    private TagPluginChunk()
    { }

    #endregion Private Constructors

    #region Properties

    internal static TagPluginChunk Default => new();

    #endregion Properties

    #region Internal Methods

    internal TagDescriptor GetTagDescriptor(object? state, Signature sig)
    {
        var chunk = State.GetTagPlugin(state);

        for (var pt = chunk.tags; pt is not null; pt = pt.Next)

            if (sig == pt.Signature) return pt.Descriptor;

        for (var pt = supportedTags; pt is not null; pt = pt.Next)

            if (sig == pt.Signature) return pt.Descriptor;

        return null;
    }

    #endregion Internal Methods
}
