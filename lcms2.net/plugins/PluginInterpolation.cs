using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

#if PLUGIN
    public
#else
internal
#endif
sealed class PluginInterpolation : PluginBase
{
    public InterpFnFactory? InterpolatorsFactory;

    public PluginInterpolation(Signature magic, uint expectedVersion, Signature type, InterpFnFactory? interpolatorsFactory)
        : base(magic, expectedVersion, type) =>
        InterpolatorsFactory = interpolatorsFactory;
}
#if PLUGIN
    public
#else
internal
#endif
    delegate void InterpFn16(in ushort[] input, ushort[] output, in InterpParams p);
#if PLUGIN
    public
#else
internal
#endif
    delegate void InterpFnFloat(in float[] input, float[] output, in InterpParams p);

#if PLUGIN
    public
#else
internal
#endif
    delegate InterpFunction InterpFnFactory(int numInputChannels, int numOutputChannels, LerpFlag flags);

[Flags]
#if PLUGIN
    public
#else
internal
#endif
    enum LerpFlag
{
    Ushort = 0,
    Float = 1,
    Trilinear = 4
}

[StructLayout(LayoutKind.Explicit)]
#if PLUGIN
    public
#else
internal
#endif
    struct InterpFunction
{
    [FieldOffset(0)]
    public InterpFn16 Lerp16;
    [FieldOffset(0)]
    public InterpFnFloat LerpFloat;
}

#if PLUGIN
    public
#else
internal
#endif
    class InterpParams
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
