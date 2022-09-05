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
namespace lcms2.types;

public struct LabEncoded : ICloneable
{
    #region Fields

    public ushort a;
    public ushort b;
    public ushort L;

    #endregion Fields

    #region Public Constructors

    public LabEncoded(ushort l, ushort a, ushort b) =>
        (L, this.a, this.b) = (l, a, b);

    #endregion Public Constructors

    #region Public Methods

    public static explicit operator Lab(LabEncoded lab) =>
        lab.ToLab();

    public static implicit operator LabEncoded((ushort, ushort, ushort) v) =>
            new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
           new LabEncoded(L, a, b);

    public Lab ToLab()
    {
        var dl = L2Float(L);
        var da = ab2Float(a);
        var db = ab2Float(b);

        return (dl, da, db);
    }

    #endregion Public Methods

    #region Private Methods

    private static double ab2Float(ushort v) =>
        (v / 257.0) - 128.0;

    private static double L2Float(ushort v) =>
            v / 655.35;

    #endregion Private Methods
}
