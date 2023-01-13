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

[DebuggerTypeProxy(typeof(ConvertingReadOnlySpan<,>.DebugView))]
[DebuggerDisplay("{ToString()}")]
public readonly ref struct ConvertingReadOnlySpan<Tfrom, Tto>
    where Tfrom : unmanaged
    where Tto : unmanaged
{
    #region Fields

    private readonly ConvertFunc<Tfrom, Tto> _funcTo;

    #endregion Fields

    #region Public Constructors

    public ConvertingReadOnlySpan(ReadOnlySpan<Tfrom> span, ConvertFunc<Tfrom, Tto> funcTo)
    {
        Span = span;
        _funcTo = funcTo;
    }

    #endregion Public Constructors

    #region Properties

    public int Length =>
        Span.Length;

    public ReadOnlySpan<Tfrom> Span { get; }

    #endregion Properties

    #region Indexers

    public Tto this[int index] =>
        _funcTo(Span[index]);

    public ConvertingReadOnlySpan<Tfrom, Tto> this[Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(Span.Length);
            return Slice(start, length);
        }
    }

    #endregion Indexers

    #region Public Methods

    public static ConvertingReadOnlySpan<Tfrom, Tto> operator +(ConvertingReadOnlySpan<Tfrom, Tto> span, int increase) =>
        span.Slice(increase);

    public static ConvertingReadOnlySpan<Tfrom, Tto> operator ++(ConvertingReadOnlySpan<Tfrom, Tto> span) =>
        span.Slice(1);

    public ConvertingReadOnlySpan<Tfrom, Tto> Slice(int start) =>
                new(Span[start..], _funcTo);

    public ConvertingReadOnlySpan<Tfrom, Tto> Slice(int start, int length) =>
        new(Span.Slice(start, length), _funcTo);

    public override string ToString() =>
        $"MoreSpans.ConvertingSpan<{typeof(Tfrom).Name},{typeof(Tto).Name}>[{Length}]";

    #endregion Public Methods

    #region Classes

    internal sealed class DebugView
    {
        #region Public Constructors

        public DebugView(ConvertingReadOnlySpan<Tfrom, Tto> span)
        {
            Items = new string[span.Length];
            GetterFunction = span._funcTo;

            for (var i = 0; i < span.Length; i++)
            {
                var value = span.Span[i];
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

        public ConvertFunc<Tfrom, Tto> GetterFunction { get; }

        [DebuggerDisplay("")]
        public string[] Items { get; }

        #endregion Properties
    }

    #endregion Classes
}
