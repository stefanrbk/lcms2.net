using System.Diagnostics;

namespace lcms2.types;

[DebuggerStepThrough]
public readonly struct U16Fixed16 : ICloneable
{
    private readonly uint _value;

    public U16Fixed16(uint value) =>
        _value = value;

    public static implicit operator uint(U16Fixed16 v) =>
        v._value;

    public static implicit operator U16Fixed16(uint v) =>
        new(v);

    object ICloneable.Clone() =>
        Clone();

    public U16Fixed16 Clone() =>
        new(_value);
}
