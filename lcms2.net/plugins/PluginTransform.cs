using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;
#if PLUGIN
public sealed class PluginTransform
#else
internal sealed class PluginTransform
#endif
    : PluginBase
{
    internal TransformFactories Factories;

#if PLUGIN
    public PluginTransform(
#else
    internal PluginTransform(
#endif
        Signature magic, uint expectedVersion, Signature type, TransformFactories factories)
        : base(magic, expectedVersion, type) =>
        Factories = factories;
}

#if PLUGIN
[StructLayout(LayoutKind.Explicit)]
public unsafe struct TransformFactories
#else
[StructLayout(LayoutKind.Explicit)]
internal unsafe struct TransformFactories
#endif
{
    [FieldOffset(0)]
    internal TransformFactory legacy_xform;

    [FieldOffset(0)]
    internal Transform2Factory xform;
}

#if PLUGIN
public class Cache
#else
internal class Cache
#endif

{
    public ushort[] CacheIn = new ushort[Lcms2.MaxChannels];
    public ushort[] CacheOut = new ushort[Lcms2.MaxChannels];
}

#if PLUGIN
public struct Stride
#else
internal struct Stride
#endif

{
    public int BytesPerLineIn;
    public int BytesPerLineOut;
    public int BytesPerPlaneIn;
    public int BytesPerPlaneOut;
}

#if PLUGIN
public delegate void TransformFn(
#else
internal delegate void TransformFn(
#endif
    Transform cargo, object inputBuffer, object outputBuffer, int size, int stride);

#if PLUGIN
public delegate void Transform2Fn(
#else
internal delegate void Transform2Fn(
#endif
    Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

#if PLUGIN
public delegate void TransformFactory(
#else
internal delegate void TransformFactory(
#endif
    TransformFn xform, object? userData, object inputBuffer, object outputBuffer, int size, int stride);

#if PLUGIN
public delegate void Transform2Factory(
#else
internal delegate void Transform2Factory(
#endif
    Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

#if PLUGIN
public class Transform
#else
internal class Transform
#endif
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
