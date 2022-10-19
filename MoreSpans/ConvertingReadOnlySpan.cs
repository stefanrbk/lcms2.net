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
namespace MoreSpans;

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
        new(Span[start..length], _funcTo);

    #endregion Public Methods
}
