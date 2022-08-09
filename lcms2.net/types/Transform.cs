using System.Runtime.InteropServices;

using lcms2.state;

namespace lcms2.types;

public unsafe struct Cache
{
    public fixed ushort CacheIn[Lcms2.MaxChannels];
    public fixed ushort CacheOut[Lcms2.MaxChannels];
}

/// <summary>
///     Stride info for a transform.
/// </summary>
/// <remarks>
///     Implements the <c>cmsStride</c> struct.</remarks>
public struct Stride
{
    public int BytesPerLineIn;
    public int BytesPerLineOut;
    public int BytesPerPlaneIn;
    public int BytesPerPlaneOut;
}

/// <summary>
///     Legacy function, handles just ONE scanline.
/// </summary>
/// <remarks>
///     Implements the <c>_cmsTransformFn</c> typedef.</remarks>
public delegate void TransformFn(Transform cargo, object inputBuffer, object outputBuffer, int size, int stride);

/// <summary>
///     Transform function
/// </summary>
/// <remarks>
///     Implements the <c>_cmsTransform2Fn</c> typedef.</remarks>
public delegate void Transform2Fn(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

/// <summary>
///     Transform factory
/// </summary>
/// <remarks>
///     Implements the <c>_cmsTransformFactory</c> typedef.</remarks>
public delegate void TransformFactory(TransformFn xform, object? userData, object inputBuffer, object outputBuffer, int size, int stride);

/// <summary>
///     Transform factory
/// </summary>
/// <remarks>
///     Implements the <c>_cmsTransform2Factory</c> typedef.</remarks>
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

    /// <summary>
    ///     Retrieve original flags
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsGetTransformFlags</c> and <c>_cmsGetTransformUserData</c> functions.</remarks>
    public uint Flags { get; internal set; }
    internal double AdaptationState;

    internal Signature RenderingIntent;

    internal Context Context;

    /// <summary>
    ///     User data as specified by the factory
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsSetTransformUserData</c> and <c>_cmsGetTransformUserData</c> functions.</remarks>
    public object? UserData { get; set; }
    internal FreeUserDataFn? FreeUserData;

    internal TransformFn? OldXform;

    /// <summary>
    ///     Retrieve 16 bit formatters
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsGetTransformFormatters16</c> function.</remarks>
    public (Formatter16? FromInput, Formatter16? ToOutput) Formatters16 =>
        (FromInput, ToOutput);

    /// <summary>
    ///     Retrieve float formatters
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsGetTransformFormattersFloat</c> function.</remarks>
    public (FormatterFloat? FromInput, FormatterFloat? ToOutput) FormattersFloat =>
        (FromInputFloat, ToOutputFloat);


    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Factory
    {
        [FieldOffset(0)]
        public TransformFactory LegacyXform;

        [FieldOffset(0)]
        public Transform2Factory Xform;
    }
}
