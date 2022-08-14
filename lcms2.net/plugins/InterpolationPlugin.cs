using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
/// The plugin representing an interpolation
/// </summary>
/// <remarks>Implements the <c>cmsPluginInterpolation</c> struct.</remarks>
public sealed class InterpolationPlugin : Plugin
{
    public InterpFnFactory? InterpolatorsFactory;

    public InterpolationPlugin(Signature magic, uint expectedVersion, Signature type, InterpFnFactory? interpolatorsFactory)
        : base(magic, expectedVersion, type) =>
        InterpolatorsFactory = interpolatorsFactory;

    internal static bool RegisterPlugin(Context? context, InterpolationPlugin? plugin)
    {
        var ptr = (InterpolationPluginChunk)Context.GetClientChunk(context, Chunks.InterpPlugin)!;

        if (plugin is null) {
            ptr.interpolators = null;
            return true;
        }

        // Set replacement functions
        ptr.interpolators = plugin.InterpolatorsFactory;
        return true;
    }
}

/// <summary>
/// 16 bits forward interpolation. This function performs precision-limited linear interpolation
/// and is supposed to be quite fast. Implementation may be tetrahedral or trilinear, and plug-ins may
/// choose to implement any other interpolation algorithm.
/// </summary>
/// <remarks>Implements the <c>_cmsInterpFn16</c> typedef.</remarks>
public delegate void InterpFn16(in ushort[] input, ushort[] output, in InterpParams p);

/// <summary>
/// Floating point forward interpolation. Full precision interpolation using floats. This is not a
/// time critical function. Implementation may be tetrahedral or trilinear, and plug-ins may
/// choose to implement any other interpolation algorithm.
/// </summary>
/// <remarks>Implements the <c>_cmsInterpFnFloat</c> typedef.</remarks>
public delegate void InterpFnFloat(in float[] input, float[] output, in InterpParams p);

/// <summary>
/// Interpolators factory
/// </summary>
/// <remarks>Implements the <c>cmsInterpFnFactory</c> typedef.</remarks>
public delegate InterpFunction InterpFnFactory(int numInputChannels, int numOutputChannels, LerpFlag flags);

[Flags]
public enum LerpFlag
{
    Ushort = 0,
    Float = 1,
    Trilinear = 4
}

/// <summary>
/// This type holds a pointer to an interpolator that can be either 16 bits or float
/// </summary>
/// <remarks>Implements the <c>cmsInterpFunction</c> union.</remarks>
[StructLayout(LayoutKind.Explicit)]
public struct InterpFunction
{
    [FieldOffset(0)]
    public InterpFn16 Lerp16;
    [FieldOffset(0)]
    public InterpFnFloat LerpFloat;
}

/// <summary>
/// Used on all interpolations. Supplied by lcms2 when calling the interpolation function
/// </summary>
/// <remarks>Implements the <c>cmsInterpParams</c> struct.</remarks>
public class InterpParams
{
    /// <summary>
    /// The calling thread
    /// </summary>
    public Context Context;

    /// <summary>
    /// Keep original flags
    /// </summary>
    public LerpFlag Flags;
    /// <summary>
    /// != 1 only in 3D interpolation
    /// </summary>
    public int NumInputs;
    /// <summary>
    /// != 1 only in 3D interpolation
    /// </summary>
    public int NumOutputs;

    /// <summary>
    /// Valid on all kinds of tables
    /// </summary>
    public uint[] NumSamples;
    /// <summary>
    /// Domain = numSamples - 1
    /// </summary>
    public int[] Domain;

    /// <summary>
    /// Optimization for 3D CLUT. This is the number of nodes premultiplied for each
    /// dimension. For example, in 7 nodes, 7, 7^2 , 7^3, 7^4, etc. On non-regular
    /// Samplings may vary according of the number of nodes for each dimension.
    /// </summary>
    public int[] Opta;

    /// <summary>
    /// "Points" to the actual interpolation table.
    /// </summary>
    public object Table;
    /// <summary>
    /// Points to the function to do the interpolation
    /// </summary>
    public InterpFunction Interpolation;

    public const int MaxInputDimensions = 15;

    public InterpParams(Context context, LerpFlag flags, int numInputs, int numOutputs, uint[] numSamples, int[] domain, int[] opta, object table, InterpFunction interpolation)
    {
        Context = context;
        Flags = flags;
        NumInputs = numInputs;
        NumOutputs = numOutputs;
        NumSamples = numSamples;
        Domain = domain;
        Opta = opta;
        Table = table;
        Interpolation = interpolation;
    }
}

internal sealed class InterpolationPluginChunk
{
    internal InterpFnFactory? interpolators;

    internal static void Alloc(ref Context ctx, in Context? src) =>
        ctx.chunks[(int)Chunks.InterpPlugin] =
            (InterpolationPluginChunk?)src?.chunks[(int)Chunks.InterpPlugin] ?? interpPluginChunk;

    private InterpolationPluginChunk()
    { }

    internal static InterpolationPluginChunk global = new() { interpolators = null };
    private static readonly InterpolationPluginChunk interpPluginChunk = new() { interpolators = null };

    internal static InterpFunction DefaultInterpolatorsFactory(int _numInputChannels, int _numOutputChannels, LerpFlag _flags) =>
        throw new NotImplementedException();
}