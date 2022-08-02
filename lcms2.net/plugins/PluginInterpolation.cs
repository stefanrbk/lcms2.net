using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public sealed class PluginInterpolation : PluginBase
{
    public InterpFnFactory? InterpolatorsFactory;

    public PluginInterpolation(Signature magic, uint expectedVersion, Signature type, InterpFnFactory? interpolatorsFactory)
        : base(magic, expectedVersion, type) =>
        InterpolatorsFactory = interpolatorsFactory;
}

public delegate void InterpFn16(in ushort[] input, ushort[] output, in InterpParams p);

public delegate void InterpFnFloat(in float[] input, float[] output, in InterpParams p);

public delegate InterpFunction InterpFnFactory(int numInputChannels, int numOutputChannels, LerpFlag flags);

[Flags]
public enum LerpFlag
{
    Ushort = 0,
    Float = 1,
    Trilinear = 4
}

[StructLayout(LayoutKind.Explicit)]
public struct InterpFunction
{
    [FieldOffset(0)]
    public InterpFn16 Lerp16;
    [FieldOffset(0)]
    public InterpFnFloat LerpFloat;
}

public class InterpParams
{
    internal Context context;

    internal LerpFlag flags;
    internal int numInputs;
    internal int numOutputs;

    internal int[] numSamples;
    internal int[] domain;

    internal int[] opta;

    internal object table;
    internal InterpFunction interpolation;

    public const int MaxInputDimensions = 15;
}
