using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public sealed class PluginTransform : Plugin
{
    public TransformFactories Factories;

    public PluginTransform( Signature magic, uint expectedVersion, Signature type, TransformFactories factories)
        : base(magic, expectedVersion, type) =>
        Factories = factories;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct TransformFactories
{
    [FieldOffset(0)]
    internal TransformFactory legacy_xform;

    [FieldOffset(0)]
    internal Transform2Factory xform;
}

public class Cache
{
    public ushort[] CacheIn = new ushort[Lcms2.MaxChannels];
    public ushort[] CacheOut = new ushort[Lcms2.MaxChannels];
}

public struct Stride
{
    public int BytesPerLineIn;
    public int BytesPerLineOut;
    public int BytesPerPlaneIn;
    public int BytesPerPlaneOut;
}

public delegate void TransformFn(Transform cargo, object inputBuffer, object outputBuffer, int size, int stride);

public delegate void Transform2Fn(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

public delegate void TransformFactory(TransformFn xform, object? userData, object inputBuffer, object outputBuffer, int size, int stride);

public delegate void Transform2Factory(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

public class Transform
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
public class TransformCollection
{
    internal TransformCollection? next = null;
}