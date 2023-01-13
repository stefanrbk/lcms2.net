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
public readonly ref struct DownCastingReadOnlySpan<Tfrom, Tto>
    where Tfrom : unmanaged
    where Tto : unmanaged
{
    #region Fields

    private readonly DownCastFunc<Tto, Tfrom> _funcTo;
    private readonly int _size;

    #endregion Fields

    #region Public Constructors

    public DownCastingReadOnlySpan(ReadOnlySpan<Tfrom> span, DownCastFunc<Tto, Tfrom> funcTo)
    {
        Span = span;
        _funcTo = funcTo;
        unsafe
        {
            _size = span.Length * sizeof(Tfrom) / sizeof(Tto);
        }
    }

    #endregion Public Constructors

    #region Properties

    public ReadOnlySpan<Tfrom> Span { get; }

    #endregion Properties

    #region Indexers

    public Tto[] this[int index] =>
        _funcTo(Span[index * _size]);

    #endregion Indexers

    #region Public Methods

    public Tto[] ToArray()
    {
        var length = Span.Length * _size;
        var array = new Tto[length];
        for (var i = 0; i < Span.Length; i++)
            this[i].CopyTo(array, i * _size);

        return array;
    }

    #endregion Public Methods
}
