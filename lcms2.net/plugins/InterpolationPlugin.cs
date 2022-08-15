using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

using static lcms2.Helpers;

namespace lcms2.plugins;

/// <summary>
/// The plugin representing an interpolation
/// </summary>
/// <remarks>Implements the <c>cmsPluginInterpolation</c> struct.</remarks>
public sealed class InterpolationPlugin
    : Plugin
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
    public InterpFn16? Lerp16;
    [FieldOffset(0)]
    public InterpFnFloat? LerpFloat;
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
    public Context? Context;

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
    public uint[] NumSamples = new uint[MaxInputDimensions];

    /// <summary>
    /// Domain = numSamples - 1
    /// </summary>
    public int[] Domain = new int[MaxInputDimensions];

    /// <summary>
    /// Optimization for 3D CLUT. This is the number of nodes premultiplied for each
    /// dimension. For example, in 7 nodes, 7, 7^2 , 7^3, 7^4, etc. On non-regular
    /// Samplings may vary according of the number of nodes for each dimension.
    /// </summary>
    public int[] Opta = new int[MaxInputDimensions];

    /// <summary>
    /// "Points" to the actual interpolation table.
    /// </summary>
    public object Table;

    /// <summary>
    /// Points to the function to do the interpolation
    /// </summary>
    public InterpFunction Interpolation;

    public const int MaxInputDimensions = 15;

    public InterpParams(Context? context, LerpFlag flags, int numInputs, int numOutputs, object table)
    {
        Context = context;
        Flags = flags;
        NumInputs = numInputs;
        NumOutputs = numOutputs;
        Table = table;
        Interpolation = default;
    }

    internal bool SetInterpolationRoutine(Context? context)
    {
        var ptr = Context.GetInterpolationPlugin(context);

        Interpolation.Lerp16 = null;

        // Invoke factory, possibly in the Plugin
        if (ptr.interpolators is not null)
            Interpolation = ptr.interpolators(NumInputs, NumOutputs, Flags);

        // If unsupported by the plugin, go for the default.
        // It happens only if an extern plugin is being used
        if (Interpolation.Lerp16 is null)
            Interpolation = InterpolationPluginChunk.DefaultInterpolatorsFactory(NumInputs, NumOutputs, Flags);

        // Check for valid interpolator (we just check one member of the union
        return Interpolation.Lerp16 is not null;
    }

    internal static InterpParams? Compute(Context? context, in uint[] numSamples, int inputChan, int outputChan, object table, LerpFlag flags)
    {
        // Check for maximum inputs
        if (inputChan > MaxInputDimensions) {
            Context.SignalError(context, ErrorCode.Range, "Too many input channels ({0} channels, max={1})", inputChan, MaxInputDimensions);
            return null;
        }

        // Creates an empty object and keep original parameters
        var p = new InterpParams(context, flags, inputChan, outputChan, table);

        // Fill samples per input direction and domain (which is number of nodes minus one)
        for (var i = 0; i < inputChan; i++) {

            p.NumSamples[i] = numSamples[i];
            p.Domain[i] = (int)numSamples[i] - 1;
        }

        // Compute factors to apply to each component to index the grid array
        p.Opta[0] = p.NumOutputs;
        for (var i = 1; i < inputChan; i++)
            p.Opta[i] = p.Opta[i - 1] * (int)numSamples[inputChan - i];

        if (!p.SetInterpolationRoutine(context)) {
            Context.SignalError(context, ErrorCode.UnknownExtension, "Unsupported interpolation ({0}->{1} channels)", inputChan, outputChan);
            return null;
        }

        // All seems ok
        return p;
    }

    internal static InterpParams? Compute(Context? context, uint numSamples, int inputChan, int outputChan, object table, LerpFlag flags)
    {
        var samples = new uint[MaxInputDimensions];

        for (var i = 0; i < MaxInputDimensions; i++)
            samples[i] = numSamples;

        return Compute(context, samples, inputChan, outputChan, table, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort LinearInterp(int a, int l, int h)
    {
        uint dif = ((uint)(h - l) * (uint)a) + 0x8000u;
        dif = (dif >> 16) + (uint)l;
        return (ushort)dif;
    }

    private void LinLerp1D(in ushort[] value, ref ushort[] output)
    {
        var lutTable = (ushort[])Table;

        // if last value or just one point
        if (value[0] == 0xFFFF || Domain[0] == 0) {
            output[0] = lutTable[Domain[0]];
        } else {
            var val3 = Domain[0] * value[0];
            val3 = ToFixedDomain(val3);

            var cell0 = FixedToInt(val3);
            var rest = FixedRestToInt(val3);

            var y0 = lutTable[cell0];
            var y1 = lutTable[cell0 + 1];

            output[0] = LinearInterp(rest, y0, y1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float fclamp(float v) =>
        (v < 1.0e-9f) || Single.IsNaN(v)
            ? 0.0f
            : (v > 1.0f ? 1.0f : v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float lerp(float a, float l, float h) =>
        l + ((h - l) * a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort lerp(int a, ushort l, ushort h) =>
        (ushort)(l + RoundFixedToInt((h - l) * a));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float dens(float[] table, int i, int j, int outChan) =>
        table[i + j + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort dens(ushort[] table, int i, int j, int outChan) =>
        table[i + j + outChan];

    private void LinLerp1D(in float[] value, ref float[] output)
    {
        var lutTable = (float[])Table;

        var val2 = fclamp(value[0]);

        if (val2 == 1.0 || Domain[0] == 0) {
            output[0] = lutTable[Domain[0]];
        } else {
            val2 *= Domain[0];

            var cell0 = (int)MathF.Floor(val2);
            var cell1 = (int)MathF.Ceiling(val2);

            // rest is 16 LSB bits
            var rest = val2 - cell0;

            var y0 = lutTable[cell0];
            var y1 = lutTable[cell1];

            output[0] = y0 + ((y1 - y0) * rest);
        }
    }

    private void Eval1Input(in ushort[] input, ref ushort[] output)
    {
        var p16 = this;
        var lutTable = (ushort[])p16.Table;

        if (input[0] == 0xFFFF || p16.Domain[0] == 0) {

            var y0 = p16.Domain[0] * p16.Opta[0];

            for (var outChan = 0; outChan < p16.NumOutputs; outChan++)
                output[outChan] = lutTable[y0 + outChan];
        } else {

            var v = input[0] * p16.Domain[0];
            var fk = ToFixedDomain(v);

            var k0 = FixedToInt(fk);
            var rk = (ushort)FixedRestToInt(fk);

            var k1 = k0 + (input[0] != 0xFFFF ? 1 : 0);

            k0 *= p16.Opta[0];
            k1 *= p16.Opta[0];

            for (var outChan = 0; outChan < p16.NumOutputs; outChan++)
                output[outChan] = LinearInterp(rk, lutTable[k0 + outChan], lutTable[k1 + outChan]);
        }
    }

    private void Eval1Input(in float[] value, ref float[] output)
    {
        var p = this;
        var lutTable = (float[])p.Table;

        var val2 = fclamp(value[0]);

        if (val2 == 1.0 || p.Domain[0] == 0) {

            var start = (uint)(p.Domain[0] * p.Opta[0]);

            for (var outChan = 0; outChan < p.NumOutputs; outChan++)
                output[outChan] = lutTable[start + outChan];
        } else {

            val2 *= p.Domain[0];

            var cell0 = (int)MathF.Floor(val2);
            var cell1 = (int)MathF.Ceiling(val2);

            var rest = val2 - cell0;

            cell0 *= p.Opta[0];
            cell1 *= p.Opta[0];

            for (var outChan = 0; outChan < p.NumOutputs; outChan++) {

                var y0 = lutTable[cell0 + outChan];
                var y1 = lutTable[cell1 + outChan];

                output[outChan] = y0 + ((y1 - y0) * rest);
            }
        }
    }

    private void BilinearInterp(in float[] input, ref float[] output)
    {
        var p = this;
        var lutTable = (float[])Table;

        var totalOut = p.NumOutputs;
        var px = fclamp(input[0]) * p.Domain[0];
        var py = fclamp(input[1]) * p.Domain[1];

        var x0 = QuickFloor(px); var fx = px - x0;
        var y0 = QuickFloor(py); var fy = py - y0;

        x0 *= p.Opta[1];
        var x1 = x0 + (fclamp(input[0]) >= 1.0 ? 0 : p.Opta[1]);

        y0 *= p.Opta[0];
        var y1 = y0 + (fclamp(input[1]) >= 1.0 ? 0 : p.Opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++) {

            var d00 = dens(lutTable, x0, y0, outChan);
            var d01 = dens(lutTable, x0, y1, outChan);
            var d10 = dens(lutTable, x1, y0, outChan);
            var d11 = dens(lutTable, x1, y1, outChan);

            var dx0 = lerp(fx, d00, d10);
            var dx1 = lerp(fx, d01, d11);

            output[outChan] = lerp(fy, dx0, dx1);
        }
    }

    private void BilinearInterp(in ushort[] input, ref ushort[] output)
    {
        var p = this;
        var lutTable = (ushort[])Table;

        var totalOut = p.NumOutputs;

        var fx = ToFixedDomain(input[0] * p.Domain[0]);
        var x0 = FixedToInt(fx);
        var rx = FixedRestToInt(fx);

        var fy = ToFixedDomain(input[1] * p.Domain[1]);
        var y0 = FixedToInt(fy);
        var ry = FixedRestToInt(fy);

        x0 *= p.Opta[1];
        var x1 = x0 + (input[0] == 0xFFFF ? 0 : p.Opta[1]);

        y0 *= p.Opta[0];
        var y1 = y0 + (input[1] == 0xFFFF ? 0 : p.Opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++) {

            var d00 = dens(lutTable, x0, y0, outChan);
            var d01 = dens(lutTable, x0, y1, outChan);
            var d10 = dens(lutTable, x1, y0, outChan);
            var d11 = dens(lutTable, x1, y1, outChan);

            var dx0 = lerp(rx, d00, d10);
            var dx1 = lerp(rx, d01, d11);

            output[outChan] = lerp(ry, dx0, dx1);
        }
    }
}

internal sealed class InterpolationPluginChunk
{
    internal InterpFnFactory? interpolators;

    internal static void Alloc(ref Context ctx, in Context? src) =>
        ctx.chunks[(int)Chunks.InterpPlugin] =
            (InterpolationPluginChunk?)src?.chunks[(int)Chunks.InterpPlugin] ?? new InterpolationPluginChunk();

    private InterpolationPluginChunk()
    { }

    internal static InterpolationPluginChunk global = new() { interpolators = null };

    internal static InterpFunction DefaultInterpolatorsFactory(int _numInputChannels, int _numOutputChannels, LerpFlag _flags) =>
        throw new NotImplementedException();
}