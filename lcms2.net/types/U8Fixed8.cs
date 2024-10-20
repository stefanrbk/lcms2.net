using System.Diagnostics;

namespace lcms2.types;

[DebuggerStepThrough]
public readonly struct U8Fixed8 : ICloneable
{
    private readonly ushort _value;

    public U8Fixed8(ushort value) =>
        _value = value;

    public static implicit operator ushort(U8Fixed8 v) =>
        v._value;

    public static implicit operator U8Fixed8(ushort v) =>
        new(v);

    object ICloneable.Clone() =>
        Clone();

    public U8Fixed8 Clone() =>
        new(_value);
}
