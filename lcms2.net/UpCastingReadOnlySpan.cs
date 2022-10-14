﻿//---------------------------------------------------------------------------------
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
namespace lcms2;

public readonly ref struct UpCastingReadOnlySpan<Tfrom, Tto>
    where Tfrom : unmanaged
    where Tto : unmanaged
{
    #region Fields

    private readonly FuncTo _funcTo;
    private readonly int _size;

    #endregion Fields

    #region Public Constructors

    public UpCastingReadOnlySpan(ReadOnlySpan<Tfrom> span, FuncTo funcTo)
    {
        Span = span;
        _funcTo = funcTo;
        unsafe
        {
            _size = span.Length * sizeof(Tfrom) / sizeof(Tto);
        }
    }

    #endregion Public Constructors

    #region Delegates

    public delegate Tto FuncTo(ReadOnlySpan<Tfrom> span);

    #endregion Delegates

    #region Properties

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

    #endregion Public Methods
}
