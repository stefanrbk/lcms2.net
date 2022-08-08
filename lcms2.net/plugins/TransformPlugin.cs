using System.Diagnostics;
using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Transform plugin
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginTransform</c> typedef.</remarks>
public sealed class TransformPlugin : Plugin
{
    public Transform.Factory Factories;

    public TransformPlugin(Signature magic, uint expectedVersion, Signature type, Transform.Factory factories)
        : base(magic, expectedVersion, type) =>
        Factories = factories;

    internal static bool RegisterPlugin(Context? context, TransformPlugin? plugin)
    {
        var ctx = Context.GetTransformPlugin(context);

        if (plugin is null)
        {
            ctx.transformCollection = null;
            return true;
        }

        // Check for full xform plugins previous to 2.8, we would need an adapter in that case
        var old = plugin.ExpectedVersion < 2080;

        ctx.transformCollection = new(plugin.Factories, old, ctx.transformCollection);

        return true;
    }
}

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

internal class TransformCollection
{
    internal Transform.Factory Factory;
    internal bool OldXform;

    internal TransformCollection? Next;

    public TransformCollection(Transform.Factory factory, bool oldXform, TransformCollection? next)
    {
        Factory = factory;
        OldXform = oldXform;
        Next = next;
    }

    public TransformCollection(TransformCollection other, TransformCollection? next = null)
    {
        Factory = other.Factory;
        OldXform = other.OldXform;
        Next = next;
    }
}

internal sealed class TransformPluginChunk
{
    internal TransformCollection? transformCollection;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupTransformList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.TransformPlugin] = transformChunk;
    }

    private TransformPluginChunk()
    { }

    internal static TransformPluginChunk global = new();
    private static readonly TransformPluginChunk transformChunk = new();

    private static void DupTransformList(ref Context ctx, in Context src)
    {
        TransformPluginChunk newHead = new();
        TransformCollection? anterior = null;
        var head = (TransformPluginChunk?)src.chunks[(int)Chunks.TransformPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.transformCollection; entry is not null; entry = entry.Next)
        {
            // We want to keep the linked list order, so this is a little bit tricky
            TransformCollection newEntry = new(entry);

            if (anterior is not null)
                anterior.Next = newEntry;

            anterior = newEntry;

            if (newHead.transformCollection is null)
                newHead.transformCollection = newEntry;
        }

        ctx.chunks[(int)Chunks.TransformPlugin] = newHead;
    }
}
