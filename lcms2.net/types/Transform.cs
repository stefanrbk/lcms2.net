using System.Runtime.InteropServices;

using lcms2.state;

namespace lcms2.types;

/// <summary>
///     Transform factory
/// </summary>
/// <remarks>Implements the <c>_cmsTransform2Factory</c> typedef.</remarks>
public delegate void Transform2Factory(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

/// <summary>
///     Transform function
/// </summary>
/// <remarks>Implements the <c>_cmsTransform2Fn</c> typedef.</remarks>
public delegate void Transform2Fn(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

/// <summary>
///     Transform factory
/// </summary>
/// <remarks>Implements the <c>_cmsTransformFactory</c> typedef.</remarks>
public delegate void TransformFactory(TransformFn xform, object? userData, object inputBuffer, object outputBuffer, int size, int stride);

/// <summary>
///     Legacy function, handles just ONE scanline.
/// </summary>
/// <remarks>Implements the <c>_cmsTransformFn</c> typedef.</remarks>
public delegate void TransformFn(Transform cargo, object inputBuffer, object outputBuffer, int size, int stride);

public unsafe struct Cache
{
    public fixed ushort CacheIn[maxChannels];
    public fixed ushort CacheOut[maxChannels];
}

/// <summary>
///     Stride info for a transform.
/// </summary>
/// <remarks>Implements the <c>cmsStride</c> struct.</remarks>
public struct Stride
{
    public int BytesPerLineIn;
    public int BytesPerLineOut;
    public int BytesPerPlaneIn;
    public int BytesPerPlaneOut;
}

public class Transform
{
    internal double adaptationState;

    internal Cache cache;

    internal object? state;

    internal Signature entryColorSpace;

    internal XYZ entryWhitePoint;

    internal Signature exitColorSpace;

    internal XYZ exitWhitePoint;

    internal FreeUserDataFn? freeUserData;

    internal Formatter16? fromInput;

    internal FormatterFloat? fromInputFloat;

    internal Pipeline gamutCheck;

    internal NamedColorList inputColorant;

    internal Signature inputFormat, outputFormat;

    internal Pipeline lut;

    internal TransformFn? oldXform;

    internal NamedColorList outputColorant;

    internal Signature renderingIntent;

    internal Sequence sequence;

    internal Formatter16? toOutput;

    internal FormatterFloat? toOutputFloat;

    internal Transform2Fn? xform;

    /// <summary>
    ///     Retrieve original flags
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsGetTransformFlags</c> and <c>_cmsGetTransformUserData</c> functions.
    /// </remarks>
    public uint Flags { get; internal set; }

    /// <summary>
    ///     Retrieve 16 bit formatters
    /// </summary>
    /// <remarks>Implements the <c>_cmsGetTransformFormatters16</c> function.</remarks>
    public (Formatter16? FromInput, Formatter16? ToOutput) Formatters16 =>
        (fromInput, toOutput);

    /// <summary>
    ///     Retrieve float formatters
    /// </summary>
    /// <remarks>Implements the <c>_cmsGetTransformFormattersFloat</c> function.</remarks>
    public (FormatterFloat? FromInput, FormatterFloat? ToOutput) FormattersFloat =>
        (fromInputFloat, toOutputFloat);

    /// <summary>
    ///     User data as specified by the factory
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsSetTransformUserData</c> and <c>_cmsGetTransformUserData</c> functions.
    /// </remarks>
    public object? UserData { get; set; }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Factory
    {
        [FieldOffset(0)]
        public TransformFactory LegacyXform;

        [FieldOffset(0)]
        public Transform2Factory Xform;
    }
}
