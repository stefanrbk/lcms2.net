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
namespace lcms2;

public readonly ref struct DownCastingSpan<Tfrom, Tto>
    where Tfrom : unmanaged
    where Tto : unmanaged
{
    #region Fields

    private readonly FuncFrom _funcFrom;
    private readonly FuncTo _funcTo;
    private readonly int _size;
    private readonly Span<Tfrom> _span;

    #endregion Fields

    #region Public Constructors

    public DownCastingSpan(Span<Tfrom> span, FuncTo funcTo, FuncFrom funcFrom)
    {
        _span = span;
        _funcTo = funcTo;
        _funcFrom = funcFrom;
        unsafe
        {
            _size = span.Length * sizeof(Tfrom) / sizeof(Tto);
        }
    }

    #endregion Public Constructors

    #region Delegates

    public delegate Tfrom FuncFrom(ReadOnlySpan<Tto> span);

    public delegate Tto[] FuncTo(Tfrom value);

    #endregion Delegates

    #region Indexers

    public Tto[] this[int index]
    {
        get =>
            _funcTo(_span[index * _size]);
        set =>
            _span[index * _size] = _funcFrom(value);
    }

    #endregion Indexers
}
