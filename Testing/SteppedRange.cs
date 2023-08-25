using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Numerics;
using System.Collections;

namespace lcms2.testbed;
/// <summary>Construct a Range object using the start and end indexes.</summary>
/// <param name="start">Represent the inclusive start index of the range.</param>
/// <param name="end">Represent the exclusive end index of the range.</param>
/// <param name="step">Represent the step distance through the range.</param>
public readonly struct SteppedRange<T> : IEquatable<SteppedRange<T>>, IEnumerable<T> where T : IAdditionOperators<T, T, T>, IComparisonOperators<T, T, bool>, IMinMaxValue<T>, ISpanFormattable
{
    /// <summary>Represent the inclusive start index of the SteppedRange.</summary>
    public T Start { get; }

    /// <summary>Represent the inclusive end index of the SteppedRange.</summary>
    public T End { get; }

    /// <summary>Represent the step distance through the SteppedRange.</summary>
    public T Step { get; }

    /// <summary>Represent whether the End value is inclusive or exclusive.</summary>
    public bool Inclusive { get; }

    private SteppedRange(T start, T end, T step, bool inclusive) =>
        (Start, End, Step, Inclusive) = (start, end, step, inclusive);

    public static SteppedRange<T> CreateInclusive(T start, T end, T step) =>
        new(start, end, step, true);

    public static SteppedRange<T> CreateExclusive(T start, T end, T step) =>
        new(start, end, step, false);

    /// <summary>Indicates whether the current SteppedRange object is equal to another object of the same type.</summary>
    /// <param name="value">An object to compare with this object</param>
    public override bool Equals([NotNullWhen(true)] object? value) =>
        value is SteppedRange<T> r &&
        r.Start.Equals(Start) &&
        r.End.Equals(End) &&
        r.Step.Equals(Step);

    /// <summary>Indicates whether the current Range object is equal to another SteppedRange object.</summary>
    /// <param name="other">An object to compare with this object</param>
    public bool Equals(SteppedRange<T> other) =>
        other.Start.Equals(Start) && other.End.Equals(End) && other.Step.Equals(Step);

    /// <summary>Returns the hash code for this instance.</summary>
    public override int GetHashCode() =>
        HashCode.Combine(Start.GetHashCode(), End.GetHashCode(), Step.GetHashCode());

    /// <summary>Converts the value of the current Range object to its equivalent string representation.</summary>
    public override string ToString()
    {
        var min = T.MinValue.ToString()!.Length;
        var max = T.MaxValue.ToString()!.Length;
        Span<char> span = stackalloc char[(2 * 2) + (3 * Math.Max(min, max))]; // 2 for ".." x2, then for each index for longest possible T
        int pos = 0;

        bool formatted = Start.TryFormat(span[pos..], out int charsWritten, default, null);
        Debug.Assert(formatted);
        pos += charsWritten;

        span[pos++] = '.';
        span[pos++] = '.';

        formatted = End.TryFormat(span[pos..], out charsWritten, default, null);
        Debug.Assert(formatted);
        pos += charsWritten;

        span[pos++] = '.';
        span[pos++] = '.';

        formatted = Step.TryFormat(span[pos..], out charsWritten, default, null);
        Debug.Assert(formatted);
        pos += charsWritten;

        return new string(span[..pos]);
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (Inclusive)
            for (var i = Start; i <= End; i += Step) yield return i;
        else
            for (var i = Start; i < End; i += Step) yield return i;
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public static bool operator ==(SteppedRange<T> left, SteppedRange<T> right) =>
        left.Equals(right);

    public static bool operator !=(SteppedRange<T> left, SteppedRange<T> right) =>
        !(left == right);
}
