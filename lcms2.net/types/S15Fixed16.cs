using System.Diagnostics;

namespace lcms2.types;

[DebuggerStepThrough]
public readonly struct S15Fixed16 : ICloneable
{
    private readonly int _value;

    public S15Fixed16(int value) =>
        _value = value;

    public static implicit operator int(S15Fixed16 v) =>
        v._value;

    public static implicit operator S15Fixed16(int v) =>
        new(v);

    object ICloneable.Clone() =>
        Clone();

    public S15Fixed16 Clone() =>
        new(_value);
}
