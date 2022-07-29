using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;
#if PLUGIN
    public
#else
internal
#endif
sealed class PluginTransform : PluginBase
{
    internal TransformFactories Factories;

#if PLUGIN
    public
#else
    internal
#endif
        PluginTransform(Signature magic, uint expectedVersion, Signature type, TransformFactories factories)
        : base(magic, expectedVersion, type) =>
        Factories = factories;
}
[StructLayout(LayoutKind.Explicit)]
#if PLUGIN
    public
#else
internal
#endif
    unsafe struct TransformFactories
{
    [FieldOffset(0)]
    internal TransformFactory legacy_xform;
    [FieldOffset(0)]
    internal Transform2Factory xform;
}
#if PLUGIN
    public
#else
internal
#endif
    class Cache
{
    public ushort[] CacheIn = new ushort[Lcms2.MaxChannels];
    public ushort[] CacheOut = new ushort[Lcms2.MaxChannels];
}
#if PLUGIN
    public
#else
internal
#endif
    struct Stride
{
    public int BytesPerLineIn;
    public int BytesPerLineOut;
    public int BytesPerPlaneIn;
    public int BytesPerPlaneOut;
}
#if PLUGIN
    public
#else
internal
#endif
    delegate void TransformFn(Transform cargo, object inputBuffer, object outputBuffer, int size, int stride);
#if PLUGIN
    public
#else
internal
#endif
    delegate void Transform2Fn(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);
#if PLUGIN
    public
#else
internal
#endif
    delegate void TransformFactory(TransformFn xform, object? userData, object inputBuffer, object outputBuffer, int size, int stride);
#if PLUGIN
    public
#else
internal
#endif
    delegate void Transform2Factory(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

#if PLUGIN
    public
#else
internal
#endif
    class Transform
{
    internal Signature InputFormat, OutputFormat;

    internal Transform2Fn? xform;

    internal Formatter16? FromInput;
    internal Formatter16? ToOutput;

    internal FormatterFloat? FromInputFloat;
    internal FormatterFloat? ToOutputFloat;

    internal Cache Cache;

    internal Pipeline Lut;

    internal Pipeline GamutCheck;

    internal NamedColorList InputColorant;
    internal NamedColorList OutputColorant;

    internal Signature EntryColorSpace;
    internal Signature ExitColorSpace;

    internal XYZ EntryWhitePoint;
    internal XYZ ExitWhitePoint;

    internal Sequence Sequence;

    internal uint OriginalFlags;
    internal double AdaptationState;

    internal Signature RenderingIntent;

    internal Context Context;

    internal object? UserData;
    internal FreeUserDataFn? FreeUserData;

    internal TransformFn? OldXform;
}
