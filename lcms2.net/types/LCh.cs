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

public struct LCh : ICloneable
{
    #region Fields

    public double C;

    public double h;

    public double L;

    #endregion Fields

    #region Public Constructors

    public LCh(double l, double c, double h) =>
        (L, C, this.h) = (l, c, h);

    #endregion Public Constructors

    #region Properties

    public static LCh NaN =>
                        new(Double.NaN, Double.NaN, Double.NaN);

    public bool IsNaN =>
        Double.IsNaN(L) || Double.IsNaN(C) || Double.IsNaN(h);

    #endregion Properties

    #region Public Methods

    public static explicit operator Lab(LCh lch) =>
        lch.ToLab();

    public static implicit operator LCh((double, double, double) v) =>
            new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
           new LCh(L, C, h);

    public Lab ToLab() =>
        new(L, C * Math.Cos(h * Math.PI / 180.0), C * Math.Sin(h * Math.PI / 180.0));

    #endregion Public Methods
}
