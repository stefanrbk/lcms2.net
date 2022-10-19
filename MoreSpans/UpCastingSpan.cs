//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//---------------------------------------------------------------------------------
//

using System.Diagnostics;

namespace MoreSpans;

[DebuggerTypeProxy("UpDownCastingSpanDebugView<,>")]
public readonly ref struct UpCastingSpan<Tfrom, Tto>
    where Tfrom : unmanaged
    where Tto : unmanaged
{
    #region Fields

    private readonly DownCastFunc<Tfrom, Tto> _funcFrom;
    private readonly UpCastFunc<Tfrom, Tto> _funcTo;
    private readonly int _size;

    #endregion Fields

    #region Public Constructors

    public UpCastingSpan(Span<Tfrom> span, UpCastFunc<Tfrom, Tto> funcTo, DownCastFunc<Tfrom, Tto> funcFrom)
    {
        Span = span;
        _funcTo = funcTo;
        _funcFrom = funcFrom;
        unsafe
        {
            _size = span.Length * sizeof(Tfrom) / sizeof(Tto);
        }
    }

    #endregion Public Constructors

    #region Properties

    public int Length =>
        Span.Length / _size;

    public Span<Tfrom> Span { get; }

    #endregion Properties

    #region Indexers

    public Tto this[int index]
    {
        get =>
            _funcTo(Span[(index * _size)..]);
        set =>
            _funcFrom(value).CopyTo(Span[(index * _size)..]);
    }

    public Tto this[Index index]
    {
        get =>
            this[index.GetOffset(Length)];
        set =>
            this[index.GetOffset(Length)] = value;
    }

    public UpCastingSpan<Tfrom, Tto> this[Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(Span.Length / _size);
            return Slice(start, length);
        }
    }

    #endregion Indexers

    #region Public Methods

    public static implicit operator UpCastingReadOnlySpan<Tfrom, Tto>(UpCastingSpan<Tfrom, Tto> span) =>
        new(span.Span, span._funcTo);

    public static UpCastingSpan<Tfrom, Tto> operator +(UpCastingSpan<Tfrom, Tto> span, int increase) =>
        span.Slice(increase);

    public static UpCastingSpan<Tfrom, Tto> operator ++(UpCastingSpan<Tfrom, Tto> span) =>
        span.Slice(1);

    public void CopyTo(Span<Tto> destination)
    {
        var length = Span.Length / _size;
        for (var i = 0; i < length; i++)
            destination[i] = this[i];
    }

    public void CopyTo<T>(UpCastingSpan<T, Tto> destination)
        where T : unmanaged
    {
        var length = Span.Length / _size;
        for (var i = 0; i < length; i++)
            destination[i] = this[i];
    }

    public UpCastingSpan<Tfrom, Tto> Slice(int start) =>
                        new(Span[(start * _size)..], _funcTo, _funcFrom);

    public UpCastingSpan<Tfrom, Tto> Slice(int start, int length) =>
        new(Span[(start * _size)..(length * _size)], _funcTo, _funcFrom);

    public Tto[] ToArray()
    {
        var length = Span.Length / _size;
        var array = new Tto[length];
        for (var i = 0; i < length; i++)
            array[i] = this[i];

        return array;
    }

    #endregion Public Methods

    //public void CopyTo<T>(DownCastingSpan<T, Tto> destination)
    //    where T : unmanaged
    //{
    //    var length = Span.Length / _size;
    //    for (var i = 0; i < length; i++)
    //        destination[i];
    //}
}
