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
using System.Runtime.CompilerServices;

namespace MoreSpans;

[DebuggerTypeProxy(typeof(UpCastingReadOnlySpan<,>.DebugView))]
[DebuggerDisplay("{ToString()}")]
public readonly ref struct UpCastingReadOnlySpan<Tfrom, Tto>
    where Tfrom : unmanaged
    where Tto : unmanaged
{
    #region Fields

    private readonly UpCastFunc<Tfrom, Tto> _funcTo;
    private readonly int _size;

    #endregion Fields

    #region Public Constructors

    public UpCastingReadOnlySpan(ReadOnlySpan<Tfrom> span, UpCastFunc<Tfrom, Tto> funcTo)
    {
        Span = span;
        _funcTo = funcTo;
        _size = Unsafe.SizeOf<Tto>() / Unsafe.SizeOf<Tfrom>();
    }

    #endregion Public Constructors

    #region Properties

    public int Length =>
        Span.Length / _size;

    public ReadOnlySpan<Tfrom> Span { get; }

    #endregion Properties

    #region Indexers

    public Tto this[int index] =>
            _funcTo(Span[(index * _size)..]);

    public UpCastingReadOnlySpan<Tfrom, Tto> this[Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(Span.Length / _size);
            return Slice(start, length);
        }
    }

    #endregion Indexers

    #region Public Methods

    public static UpCastingReadOnlySpan<Tfrom, Tto> operator +(UpCastingReadOnlySpan<Tfrom, Tto> span, int increase) =>
        span.Slice(increase);

    public static UpCastingReadOnlySpan<Tfrom, Tto> operator ++(UpCastingReadOnlySpan<Tfrom, Tto> span) =>
        span.Slice(1);

    public UpCastingReadOnlySpan<Tfrom, Tto> Slice(int start) =>
                new(Span[(start * _size)..], _funcTo);

    public UpCastingReadOnlySpan<Tfrom, Tto> Slice(int start, int length) =>
        new(Span[(start * _size)..(length * _size)], _funcTo);

    public Tto[] ToArray()
    {
        var length = Span.Length / _size;
        var array = new Tto[length];
        for (var i = 0; i < length; i++)
            array[i] = this[i];

        return array;
    }

    public override string ToString() =>
        $"MoreSpans.UpCastingReadOnlySpan<{typeof(Tfrom).Name},{typeof(Tto).Name}>[{Span.Length} -> {Length}]";

    #endregion Public Methods

    #region Classes

    internal sealed class DebugView
    {
        #region Public Constructors

        public DebugView(UpCastingReadOnlySpan<Tfrom, Tto> span)
        {
            Items = new string[span.Length];
            GetterFunction = span._funcTo;
            var size = span._size;

            for (var i = 0; i < span.Length; i++)
            {
                var temp = span.Span[(i * size)..];
                var value = temp[..size].ToArray();
                object get;
                try
                {
                    get = span._funcTo(value);
                }
                catch (Exception e)
                {
                    get = e;
                }

                Items[i] = $"{value} -Get-> {get}";
            }
        }

        #endregion Public Constructors

        #region Properties

        public UpCastFunc<Tfrom, Tto> GetterFunction { get; }

        [DebuggerDisplay("")]
        public string[] Items { get; }

        #endregion Properties
    }

    #endregion Classes
}
