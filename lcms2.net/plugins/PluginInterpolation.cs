using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

#if PLUGIN
public sealed class PluginInterpolation
#else
internal sealed class PluginInterpolation
#endif
: PluginBase
{
    public InterpFnFactory? InterpolatorsFactory;

    public PluginInterpolation(Signature magic, uint expectedVersion, Signature type, InterpFnFactory? interpolatorsFactory)
        : base(magic, expectedVersion, type) =>
        InterpolatorsFactory = interpolatorsFactory;
}

#if PLUGIN
public delegate void InterpFn16(
#else
internal delegate void InterpFn16(
#endif
    in ushort[] input, ushort[] output, in InterpParams p);

#if PLUGIN
public delegate void InterpFnFloat(
#else
internal delegate void InterpFnFloat(
#endif
    in float[] input, float[] output, in InterpParams p);

#if PLUGIN
public delegate InterpFunction InterpFnFactory(
#else
internal delegate InterpFunction InterpFnFactory(
#endif
    int numInputChannels, int numOutputChannels, LerpFlag flags);

#if PLUGIN
[Flags]
public enum LerpFlag
#else
[Flags]
internal enum LerpFlag
#endif
{
    Ushort = 0,
    Float = 1,
    Trilinear = 4
}

#if PLUGIN
[StructLayout(LayoutKind.Explicit)]
public struct InterpFunction
#else
[StructLayout(LayoutKind.Explicit)]
internal struct InterpFunction
#endif

{
    [FieldOffset(0)]
    public InterpFn16 Lerp16;
    [FieldOffset(0)]
    public InterpFnFloat LerpFloat;
}

#if PLUGIN
public class InterpParams
#else
internal class InterpParams
#endif

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
