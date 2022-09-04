using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using lcms2.state;

using static lcms2.Helpers;

namespace lcms2.types;

/// <summary>
///     16 bits forward interpolation. This function performs precision-limited linear interpolation
///     and is supposed to be quite fast. Implementation may be tetrahedral or trilinear, and
///     plug-ins may choose to implement any other interpolation algorithm.
/// </summary>
/// <remarks>Implements the <c>_cmsInterpFn16</c> typedef.</remarks>
public delegate void InterpFn16(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p);

/// <summary>
///     Interpolators factory
/// </summary>
/// <remarks>Implements the <c>cmsInterpFnFactory</c> typedef.</remarks>
public delegate InterpFunction InterpFnFactory(uint numInputChannels, uint numOutputChannels, LerpFlag flags);

/// <summary>
///     Floating point forward interpolation. Full precision interpolation using floats. This is not
///     a time critical function. Implementation may be tetrahedral or trilinear, and plug-ins may
///     choose to implement any other interpolation algorithm.
/// </summary>
/// <remarks>Implements the <c>_cmsInterpFnFloat</c> typedef.</remarks>
public delegate void InterpFnFloat(ReadOnlySpan<float> input, Span<float> output, InterpParams p);

[Flags]
public enum LerpFlag
{
    Ushort = 0,
    Float = 1,
    Trilinear = 4
}

/// <summary>
///     Used on all interpolations. Supplied by lcms2 when calling the interpolation function
/// </summary>
/// <remarks>Implements the <c>cmsInterpParams</c> struct.</remarks>
public partial class InterpParams: ICloneable
{
    public const int MaxInputDimensions = 15;

    /// <summary>
    ///     The calling thread
    /// </summary>
    public object? StateContainer;

    /// <summary>
    ///     Domain = numSamples - 1
    /// </summary>
    public int[] Domain = new int[MaxInputDimensions];

    /// <summary>
    ///     Keep original flags
    /// </summary>
    public LerpFlag Flags;

    /// <summary>
    ///     Points to the function to do the interpolation
    /// </summary>
    public InterpFunction Interpolation;

    /// <summary>
    ///     != 1 only in 3D interpolation
    /// </summary>
    public uint NumInputs;

    /// <summary>
    ///     != 1 only in 3D interpolation
    /// </summary>
    public uint NumOutputs;

    /// <summary>
    ///     Valid on all kinds of tables
    /// </summary>
    public uint[] NumSamples = new uint[MaxInputDimensions];

    /// <summary>
    ///     Optimization for 3D CLUT. This is the number of nodes premultiplied for each dimension.
    ///     For example, in 7 nodes, 7, 7^2 , 7^3, 7^4, etc. On non-regular Samplings may vary
    ///     according of the number of nodes for each dimension.
    /// </summary>
    public int[] Opta = new int[MaxInputDimensions];

    /// <summary>
    ///     "Points" to the actual interpolation table.
    /// </summary>
    public object Table;

    public InterpParams(object? state, LerpFlag flags, uint numInputs, uint numOutputs, object table)
    {
        StateContainer = state;
        Flags = flags;
        NumInputs = numInputs;
        NumOutputs = numOutputs;
        Table = table;
        Interpolation = default;
    }

    public ushort[] Table16 =>
        (Flags & LerpFlag.Float) == 0
            ?(ushort[])Table
            : throw new InvalidOperationException();

    public float[] TableFloat =>
        (Flags & LerpFlag.Float) != 0
            ? (float[])Table
            : throw new InvalidOperationException();

    public void LerpFloat(ReadOnlySpan<float> input, Span<float> output)
    {
        if ((Flags & LerpFlag.Float) != 0)
            Interpolation.LerpFloat(input, output, this);
        else
            throw new InvalidOperationException();
    }

    public void Lerp16(ReadOnlySpan<ushort> input, Span<ushort> output)
    {
        if ((Flags & LerpFlag.Float) == 0)
            Interpolation.Lerp16(input, output, this);
        else
            throw new InvalidOperationException();
    }

    internal static InterpParams? Compute(object? state, in uint[] numSamples, uint inputChan, uint outputChan, object table, LerpFlag flags)
    {
        // Check for maximum inputs
        if (inputChan > MaxInputDimensions)
        {
            State.SignalError(state, ErrorCode.Range, "Too many input channels ({0} channels, max={1})", inputChan, MaxInputDimensions);
            return null;
        }

        // Creates an empty object and keep original parameters
        var p = new InterpParams(state, flags, inputChan, outputChan, table);

        // Fill samples per input direction and domain (which is number of nodes minus one)
        for (var i = 0; i < inputChan; i++)
        {
            p.NumSamples[i] = numSamples[i];
            p.Domain[i] = (int)numSamples[i] - 1;
        }

        // Compute factors to apply to each component to index the grid array
        p.Opta[0] = (int)p.NumOutputs;
        for (var i = 1; i < inputChan; i++)
            p.Opta[i] = (int)(p.Opta[i - 1] * numSamples[inputChan - i]);

        if (!p.SetInterpolationRoutine(state))
        {
            State.SignalError(state, ErrorCode.UnknownExtension, "Unsupported interpolation ({0}->{1} channels)", inputChan, outputChan);
            return null;
        }

        // All seems ok
        return p;
    }

    internal static InterpParams? Compute(object? state, uint numSamples, uint inputChan, uint outputChan, object table, LerpFlag flags)
    {
        var samples = new uint[MaxInputDimensions];

        for (var i = 0; i < MaxInputDimensions; i++)
            samples[i] = numSamples;

        return Compute(state, samples, inputChan, outputChan, table, flags);
    }

    internal static InterpFunction DefaultInterpolatorsFactory(uint numInputChannels, uint numOutputChannels, LerpFlag flags)
    {
        InterpFunction interpolation = default;
        var isFloat = (flags & LerpFlag.Float) != 0;
        var isTriliniar = (flags & LerpFlag.Trilinear) != 0;

        // Safety check
        if (numInputChannels >= 4 && numOutputChannels >= maxStageChannels)
            return default;

        switch (numInputChannels)
        {
            case 1: // Gray Lut / linear

                if (numOutputChannels == 1)
                {
                    if (isFloat)
                        interpolation.LerpFloat = LinLerp1D;
                    else
                        interpolation.Lerp16 = LinLerp1D;
                } else
                {
                    if (isFloat)
                        interpolation.LerpFloat = Eval1Input;
                    else
                        interpolation.Lerp16 = Eval1Input;
                }
                break;

            case 2: // Duotone

                if (isFloat)
                    interpolation.LerpFloat = BilinearInterp;
                else
                    interpolation.Lerp16 = BilinearInterp;

                break;

            case 3: // RGB et al

                if (isTriliniar)
                {
                    if (isFloat)
                        interpolation.LerpFloat = TrilinearInterp;
                    else
                        interpolation.Lerp16 = TrilinearInterp;
                } else
                {
                    if (isFloat)
                        interpolation.LerpFloat = TetrahedralInterp;
                    else
                        interpolation.Lerp16 = TetrahedralInterp;
                }
                break;

            case 4: // CMYK lut

                if (isFloat)
                    interpolation.LerpFloat = Eval4Inputs;
                else
                    interpolation.Lerp16 = Eval4Inputs;

                break;

            case 5:

                if (isFloat)
                    interpolation.LerpFloat = Eval5Inputs;
                else
                    interpolation.Lerp16 = Eval5Inputs;

                break;

            case 6:

                if (isFloat)
                    interpolation.LerpFloat = Eval6Inputs;
                else
                    interpolation.Lerp16 = Eval6Inputs;

                break;

            case 7:

                if (isFloat)
                    interpolation.LerpFloat = Eval7Inputs;
                else
                    interpolation.Lerp16 = Eval7Inputs;

                break;

            case 8:

                if (isFloat)
                    interpolation.LerpFloat = Eval8Inputs;
                else
                    interpolation.Lerp16 = Eval8Inputs;

                break;

            case 9:

                if (isFloat)
                    interpolation.LerpFloat = Eval9Inputs;
                else
                    interpolation.Lerp16 = Eval9Inputs;

                break;

            case 10:

                if (isFloat)
                    interpolation.LerpFloat = Eval10Inputs;
                else
                    interpolation.Lerp16 = Eval10Inputs;

                break;

            case 11:

                if (isFloat)
                    interpolation.LerpFloat = Eval11Inputs;
                else
                    interpolation.Lerp16 = Eval11Inputs;

                break;

            case 12:

                if (isFloat)
                    interpolation.LerpFloat = Eval12Inputs;
                else
                    interpolation.Lerp16 = Eval12Inputs;

                break;

            case 13:

                if (isFloat)
                    interpolation.LerpFloat = Eval13Inputs;
                else
                    interpolation.Lerp16 = Eval13Inputs;

                break;

            case 14:

                if (isFloat)
                    interpolation.LerpFloat = Eval14Inputs;
                else
                    interpolation.Lerp16 = Eval14Inputs;

                break;

            case 15:

                if (isFloat)
                    interpolation.LerpFloat = Eval15Inputs;
                else
                    interpolation.Lerp16 = Eval15Inputs;

                break;

            default:

                interpolation.Lerp16 = null!;
                break;
        }
        return interpolation;
    }

    internal bool SetInterpolationRoutine(object? state)
    {
        var ptr = State.GetInterpolationPlugin(state);

        Interpolation.Lerp16 = null;

        // Invoke factory, possibly in the Plugin
        if (ptr.interpolators is not null)
            Interpolation = ptr.interpolators(NumInputs, NumOutputs, Flags);

        // If unsupported by the plugin, go for the default. It happens only if an extern plugin is
        // being used
        if (Interpolation.Lerp16 is null)
            Interpolation = DefaultInterpolatorsFactory(NumInputs, NumOutputs, Flags);

        // Check for valid interpolator (we just check one member of the union
        return Interpolation.Lerp16 is not null;
    }

    private static void BilinearInterp(ReadOnlySpan<float> input, Span<float> output, InterpParams p)
    {
        var lutTable = p.TableFloat;

        var totalOut = p.NumOutputs;
        var px = Fclamp(input[0]) * p.Domain[0];
        var py = Fclamp(input[1]) * p.Domain[1];

        var x0 = QuickFloor(px); var fx = px - x0;
        var y0 = QuickFloor(py); var fy = py - y0;

        x0 *= p.Opta[1];
        var x1 = x0 + (Fclamp(input[0]) >= 1.0 ? 0 : p.Opta[1]);

        y0 *= p.Opta[0];
        var y1 = y0 + (Fclamp(input[1]) >= 1.0 ? 0 : p.Opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var d00 = Dens(lutTable, x0, y0, outChan);
            var d01 = Dens(lutTable, x0, y1, outChan);
            var d10 = Dens(lutTable, x1, y0, outChan, outChan);
            var d11 = Dens(lutTable, x1, y1, outChan, outChan);

            var dx0 = Lerp(fx, d00, d10);
            var dx1 = Lerp(fx, d01, d11);

            output[outChan] = Lerp(fy, dx0, dx1);
        }
    }

    private static void BilinearInterp(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p)
    {
        var lutTable = p.Table16;

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

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var d00 = Dens(lutTable, x0, y0, outChan);
            var d01 = Dens(lutTable, x0, y1, outChan);
            var d10 = Dens(lutTable, x1, y0, outChan, outChan);
            var d11 = Dens(lutTable, x1, y1, outChan, outChan);

            var dx0 = Lerp(rx, d00, d10);
            var dx1 = Lerp(rx, d01, d11);

            output[outChan] = Lerp(ry, dx0, dx1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Dens(Span<float> table, int i, int j, int outChan) =>
        table[i + j + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort Dens(Span<ushort> table, int i, int j, int outChan) =>
        table[i + j + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Dens(Span<float> table, int i, int j, int k, int outChan) =>
        table[i + j + k + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort Dens(Span<ushort> table, int i, int j, int k, int outChan) =>
        table[i + j + k + outChan];

    private static void Eval1Input(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p16)
    {
        var lutTable = p16.Table16;

        if (input[0] == 0xFFFF || p16.Domain[0] == 0)
        {
            var y0 = p16.Domain[0] * p16.Opta[0];

            for (var outChan = 0; outChan < p16.NumOutputs; outChan++)
                output[outChan] = lutTable[y0 + outChan];
        } else
        {
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

    private static void Eval1Input(ReadOnlySpan<float> value, Span<float> output, InterpParams p)
    {
        var lutTable = p.TableFloat;

        var val2 = Fclamp(value[0]);

        if (val2 == 1.0 || p.Domain[0] == 0)
        {
            var start = (uint)(p.Domain[0] * p.Opta[0]);

            for (var outChan = 0; outChan < p.NumOutputs; outChan++)
                output[outChan] = lutTable[start + outChan];
        } else
        {
            val2 *= p.Domain[0];

            var cell0 = (int)MathF.Floor(val2);
            var cell1 = (int)MathF.Ceiling(val2);

            var rest = val2 - cell0;

            cell0 *= p.Opta[0];
            cell1 *= p.Opta[0];

            for (var outChan = 0; outChan < p.NumOutputs; outChan++)
            {
                var y0 = lutTable[cell0 + outChan];
                var y1 = lutTable[cell1 + outChan];

                output[outChan] = y0 + ((y1 - y0) * rest);
            }
        }
    }

    private static void Eval4Inputs(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p)
    {
        var tmp1 = new ushort[maxStageChannels];
        var tmp2 = new ushort[maxStageChannels];

        int c0, rest, c1 = 0, c2 = 0, c3 = 0;

        var totalOut = p.NumOutputs;

        var fk = ToFixedDomain(input[0] * p.Domain[0]);
        var fx = ToFixedDomain(input[1] * p.Domain[1]);
        var fy = ToFixedDomain(input[2] * p.Domain[2]);
        var fz = ToFixedDomain(input[3] * p.Domain[3]);

        var k0 = FixedToInt(fk);
        var x0 = FixedToInt(fx);
        var y0 = FixedToInt(fy);
        var z0 = FixedToInt(fz);

        var rk = FixedRestToInt(fk);
        var rx = FixedRestToInt(fx);
        var ry = FixedRestToInt(fy);
        var rz = FixedRestToInt(fz);

        k0 *= p.Opta[3];
        var k1 = k0 + (input[0] == 0xFFFF ? 0 : p.Opta[3]);

        x0 *= p.Opta[2];
        var x1 = x0 + (input[1] == 0xFFFF ? 0 : p.Opta[2]);

        y0 *= p.Opta[1];
        var y1 = y0 + (input[2] == 0xFFFF ? 0 : p.Opta[1]);

        z0 *= p.Opta[0];
        var z1 = z0 + (input[3] == 0xFFFF ? 0 : p.Opta[0]);

        var lutTable = p.Table16.AsSpan()[k0..];

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            c0 = Dens(lutTable, x0, y0, z0, outChan);

            if (rx >= ry && ry >= rz)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x1, y0, z0, outChan);
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            } else if (rx >= rz && rz >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x1, y0, z0, outChan);
            } else if (rz >= rx && rx >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            } else if (ry >= rx && rx >= rz)
            {
                c1 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x0, y1, z0, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            } else if (ry >= rz && rz >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y1, z0, outChan);
            } else if (rz >= ry && ry >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            } else
            {
                c1 = c2 = c3 = 0;
            }

            rest = (c1 * rx) + (c2 * ry) + (c3 * rz);
            tmp1[outChan] = (ushort)(c0 + RoundFixedToInt(ToFixedDomain(rest)));
        }

        lutTable = p.Table16.AsSpan()[k1..];

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            c0 = Dens(lutTable, x0, y0, z0, outChan);

            if (rx >= ry && ry >= rz)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x1, y0, z0, outChan);
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            } else if (rx >= rz && rz >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x1, y0, z0, outChan);
            } else if (rz >= rx && rx >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            } else if (ry >= rx && rx >= rz)
            {
                c1 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x0, y1, z0, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            } else if (ry >= rz && rz >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y1, z0, outChan);
            } else if (rz >= ry && ry >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            } else
            {
                c1 = c2 = c3 = 0;
            }

            rest = (c1 * rx) + (c2 * ry) + (c3 * rz);
            tmp2[outChan] = (ushort)(c0 + RoundFixedToInt(ToFixedDomain(rest)));
        }

        for (var i = 0; i < p.NumOutputs; i++)
            output[i] = LinearInterp(rk, tmp1[i], tmp2[i]);
    }

    private static void Eval4Inputs(ReadOnlySpan<float> input, Span<float> output, InterpParams p)
    {
        var lutTable = p.TableFloat;
        var tmp1 = new float[maxStageChannels];
        var tmp2 = new float[maxStageChannels];

        var pk = Fclamp(input[0]) * p.Domain[0];
        var k0 = QuickFloor(pk);
        var rest = pk - k0;

        k0 *= p.Opta[3];
        var k1 = k0 + (Fclamp(input[0]) >= 1.0 ? 0 : p.Opta[3]);

        var t = lutTable[k0..];
        var p1 = new InterpParams(p.StateContainer, p.Flags, p.NumInputs, p.NumOutputs, t);

        p.Domain[1..3].CopyTo(p1.Domain.AsSpan());

        var inp = input[1..];
        TetrahedralInterp(inp, tmp1, p1);

        t = lutTable[k1..];
        p1.Table = t;

        TetrahedralInterp(inp, tmp2, p1);

        for (var i = 0; i < p.NumOutputs; i++)
        {
            var y0 = tmp1[i];
            var y1 = tmp2[i];

            output[i] = Lerp(rest, y0, y1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Fclamp(float v) =>
        (v < 1.0e-9f) || Single.IsNaN(v)
            ? 0.0f
            : (v > 1.0f ? 1.0f : v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float a, float l, float h) =>
        l + ((h - l) * a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort Lerp(int a, ushort l, ushort h) =>
        (ushort)(l + RoundFixedToInt((h - l) * a));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort LinearInterp(int a, int l, int h)
    {
        uint dif = ((uint)(h - l) * (uint)a) + 0x8000u;
        dif = (dif >> 16) + (uint)l;
        return (ushort)dif;
    }

    private static void LinLerp1D(ReadOnlySpan<ushort> value, Span<ushort> output, InterpParams p)
    {
        var lutTable = p.Table16;

        // if last value or just one point
        if (value[0] == 0xFFFF || p.Domain[0] == 0)
        {
            output[0] = lutTable[p.Domain[0]];
        } else
        {
            var val3 = p.Domain[0] * value[0];
            val3 = ToFixedDomain(val3);

            var cell0 = FixedToInt(val3);
            var rest = FixedRestToInt(val3);

            var y0 = lutTable[cell0];
            var y1 = lutTable[cell0 + 1];

            output[0] = LinearInterp(rest, y0, y1);
        }
    }

    private static void LinLerp1D(ReadOnlySpan<float> value, Span<float> output, InterpParams p)
    {
        var lutTable = p.TableFloat;

        var val2 = Fclamp(value[0]);

        if (val2 == 1.0 || p.Domain[0] == 0)
        {
            output[0] = lutTable[p.Domain[0]];
        } else
        {
            val2 *= p.Domain[0];

            var cell0 = (int)MathF.Floor(val2);
            var cell1 = (int)MathF.Ceiling(val2);

            // rest is 16 LSB bits
            var rest = val2 - cell0;

            var y0 = lutTable[cell0];
            var y1 = lutTable[cell1];

            output[0] = y0 + ((y1 - y0) * rest);
        }
    }

    // Tetrahedral interpolation, using Sakamoto algorithm.
    private static void TetrahedralInterp(ReadOnlySpan<float> input, Span<float> output, InterpParams p)
    {
        var lutTable = p.TableFloat;
        float c1 = 0, c2 = 0, c3 = 0;

        var totalOut = p.NumOutputs;

        var px = Fclamp(input[0]) * p.Domain[0];
        var py = Fclamp(input[1]) * p.Domain[1];
        var pz = Fclamp(input[2]) * p.Domain[2];

        // We need full floor functionality here
        var x0 = (int)MathF.Floor(px); var rx = px - x0;
        var y0 = (int)MathF.Floor(py); var ry = py - y0;
        var z0 = (int)MathF.Floor(pz); var rz = pz - z0;

        x0 *= p.Opta[2];
        var x1 = x0 + (Fclamp(input[0]) >= 1.0 ? 0 : p.Opta[2]);

        y0 *= p.Opta[1];
        var y1 = y0 + (Fclamp(input[1]) >= 1.0 ? 0 : p.Opta[1]);

        z0 *= p.Opta[0];
        var z1 = z0 + (Fclamp(input[2]) >= 1.0 ? 0 : p.Opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var c0 = Dens(lutTable, x0, y0, z0, outChan);

            if (rx >= ry && ry >= rz)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x1, y0, z0, outChan);
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            } else if (rx >= rz && rz >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x1, y0, z0, outChan);
            } else if (rz >= rx && rx >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            } else if (ry >= rx && rx >= rz)
            {
                c1 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x0, y1, z0, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            } else if (ry >= rz && rz >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y1, z0, outChan);
            } else if (rz >= ry && ry >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            } else
            {
                c1 = c2 = c3 = 0;
            }

            output[outChan] = c0 + (c1 * rx) + (c2 * ry) + (c3 * rz);
        }
    }

    private static void TetrahedralInterp(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p)
    {
        var lutTable = p.Table16;
        int rest, c0, c1 = 0, c2 = 0, c3 = 0;
        var o = output;

        var totalOut = p.NumOutputs;

        var fx = ToFixedDomain(input[0] * p.Domain[0]);
        var fy = ToFixedDomain(input[1] * p.Domain[1]);
        var fz = ToFixedDomain(input[2] * p.Domain[2]);

        // We need full floor functionality here
        var x0 = FixedToInt(fx);
        var y0 = FixedToInt(fy);
        var z0 = FixedToInt(fz);

        var rx = FixedRestToInt(fx);
        var ry = FixedRestToInt(fy);
        var rz = FixedRestToInt(fz);

        x0 *= p.Opta[2];
        var x1 = input[0] == 0xFFFF ? 0 : p.Opta[2];

        y0 *= p.Opta[1];
        var y1 = input[1] == 0xFFFF ? 0 : p.Opta[1];

        z0 *= p.Opta[0];
        var z1 = input[2] == 0xFFFF ? 0 : p.Opta[0];

        lutTable = lutTable[(x0 + y0 + z0)..];
        if (rx >= ry)
        {
            if (ry >= rz)
            {
                y1 += x1;
                z1 += y1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = lutTable[0];
                    c3 -= c2;
                    c2 -= c1;
                    c1 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
                }
            } else if (rz >= rx)
            {
                x1 += z1;
                y1 += x1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = lutTable[0];
                    c2 -= c1;
                    c1 -= c3;
                    c3 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
                }
            } else
            {
                z1 += x1;
                y1 += z1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = lutTable[0];
                    c2 -= c3;
                    c3 -= c1;
                    c1 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
                }
            }
        } else
        {
            if (rx >= rz)
            {
                x1 += y1;
                z1 += x1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = lutTable[0];
                    c3 -= c1;
                    c1 -= c2;
                    c2 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
                }
            } else if (ry >= rz)
            {
                z1 += y1;
                x1 += z1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = lutTable[0];
                    c1 -= c3;
                    c3 -= c2;
                    c2 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
                }
            } else
            {
                y1 += z1;
                x1 += y1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = lutTable[0];
                    c1 -= c2;
                    c2 -= c3;
                    c3 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
                }
            }
        }
    }

    private static void TrilinearInterp(ReadOnlySpan<float> input, Span<float> output, InterpParams p)
    {
        var lutTable = p.TableFloat;

        var totalOut = p.NumOutputs;

        var px = Fclamp(input[0]) * p.Domain[0];
        var py = Fclamp(input[1]) * p.Domain[1];
        var pz = Fclamp(input[2]) * p.Domain[2];

        // We need full floor functionality here
        var x0 = (int)MathF.Floor(px); var fx = px - x0;
        var y0 = (int)MathF.Floor(py); var fy = py - y0;
        var z0 = (int)MathF.Floor(pz); var fz = pz - z0;

        x0 *= p.Opta[2];
        var x1 = x0 + (Fclamp(input[0]) >= 1.0 ? 0 : p.Opta[2]);

        y0 *= p.Opta[1];
        var y1 = y0 + (Fclamp(input[1]) >= 1.0 ? 0 : p.Opta[1]);

        z0 *= p.Opta[0];
        var z1 = z0 + (Fclamp(input[2]) >= 1.0 ? 0 : p.Opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var d000 = Dens(lutTable, x0, y0, z0, outChan);
            var d001 = Dens(lutTable, x0, y0, z1, outChan);
            var d010 = Dens(lutTable, x0, y1, z0, outChan);
            var d011 = Dens(lutTable, x0, y1, z1, outChan);

            var d100 = Dens(lutTable, x1, y0, z0, outChan);
            var d101 = Dens(lutTable, x1, y0, z1, outChan);
            var d110 = Dens(lutTable, x1, y1, z0, outChan);
            var d111 = Dens(lutTable, x1, y1, z1, outChan);

            var dx00 = Lerp(fx, d000, d100);
            var dx01 = Lerp(fx, d001, d101);
            var dx10 = Lerp(fx, d010, d110);
            var dx11 = Lerp(fx, d011, d111);

            var dxy0 = Lerp(fy, dx00, dx10);
            var dxy1 = Lerp(fy, dx01, dx11);

            output[outChan] = Lerp(fz, dxy0, dxy1);
        }
    }

    private static void TrilinearInterp(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p)
    {
        var lutTable = p.Table16;

        var totalOut = p.NumOutputs;

        var fx = ToFixedDomain(input[0]) * p.Domain[0];
        var x0 = FixedToInt(fx);
        var rx = FixedRestToInt(fx);

        var fy = ToFixedDomain(input[1]) * p.Domain[1];
        var y0 = FixedToInt(fy);
        var ry = FixedRestToInt(fy);

        var fz = ToFixedDomain(input[2]) * p.Domain[2];
        var z0 = FixedToInt(fz);
        var rz = FixedRestToInt(fz);

        x0 *= p.Opta[2];
        var x1 = x0 + (input[0] == 0xFFFF ? 0 : p.Opta[2]);

        y0 *= p.Opta[1];
        var y1 = y0 + (input[1] == 0xFFFF ? 0 : p.Opta[1]);

        z0 *= p.Opta[0];
        var z1 = z0 + (input[2] == 0xFFFF ? 0 : p.Opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var d000 = Dens(lutTable, x0, y0, z0, outChan);
            var d001 = Dens(lutTable, x0, y0, z1, outChan);
            var d010 = Dens(lutTable, x0, y1, z0, outChan);
            var d011 = Dens(lutTable, x0, y1, z1, outChan);

            var d100 = Dens(lutTable, x1, y0, z0, outChan);
            var d101 = Dens(lutTable, x1, y0, z1, outChan);
            var d110 = Dens(lutTable, x1, y1, z0, outChan);
            var d111 = Dens(lutTable, x1, y1, z1, outChan);

            var dx00 = Lerp(rx, d000, d100);
            var dx01 = Lerp(rx, d001, d101);
            var dx10 = Lerp(rx, d010, d110);
            var dx11 = Lerp(rx, d011, d111);

            var dxy0 = Lerp(ry, dx00, dx10);
            var dxy1 = Lerp(ry, dx01, dx11);

            output[outChan] = Lerp(rz, dxy0, dxy1);
        }
    }

    public object Clone() =>
        new InterpParams(StateContainer, Flags, NumInputs, NumOutputs, Table)
        {
            NumSamples = (uint[])NumSamples.Clone(),
            Domain = (int[])Domain.Clone(),
            Opta = (int[])Opta.Clone(),
            Interpolation = Interpolation,
        };
}

/// <summary>
///     This type holds a pointer to an interpolator that can be either 16 bits or float
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
