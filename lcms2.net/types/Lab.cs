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

public struct Lab : ICloneable
{
    #region Fields

    public double a;

    public double b;

    public double L;

    #endregion Fields

    #region Public Constructors

    public Lab(double l, double a, double b) =>
        (L, this.a, this.b) = (l, a, b);

    public Lab(ReadOnlySpan<double> lab)
    {
        if (lab.Length < 3)
            L = a = b = default;
        else
            (L, a, b) = (lab[0], lab[1], lab[2]);
    }

    #endregion Public Constructors

    #region Properties

    public static Lab NaN =>
                        new(Double.NaN, Double.NaN, Double.NaN);

    public bool IsNaN =>
        Double.IsNaN(L) || Double.IsNaN(a) || Double.IsNaN(b);

    #endregion Properties

    #region Public Methods

    public static explicit operator LabEncoded(Lab lab) =>
        lab.ToLabEncoded();

    public static explicit operator LabEncodedV2(Lab lab) =>
        lab.ToLabEncodedV2();

    public static explicit operator LCh(Lab lab) =>
        lab.ToLCh();

    public static explicit operator Vec3(Lab value) =>
        value.ToVec();

    public static explicit operator XYZ(Lab lab) =>
        lab.ToXYZ();

    public static ushort[] Float2LabEncodedV2(Lab lab)
    {
        lab.L = ClampLDoubleV2(lab.L);
        lab.a = ClampabDoubleV2(lab.a);
        lab.b = ClampabDoubleV2(lab.b);

        var l = L2Fix2(lab.L);
        var a = ab2Fix2(lab.a);
        var b = ab2Fix2(lab.b);

        return new ushort[] { l, a, b };
    }

    public static implicit operator Lab((double, double, double) v) =>
                            new(v.Item1, v.Item2, v.Item3);

    public static XYZ operator %(Lab lab, XYZ whitepoint) =>
        lab.ToXYZ(whitepoint);

    public object Clone() =>
               new Lab(L, a, b);

    public double[] ToArray() =>
        new double[] { L, a, b };

    public LabEncoded ToLabEncoded()
    {
        var dl = ClampLDoubleV4(L);
        var da = ClampabDoubleV4(a);
        var db = ClampabDoubleV4(b);

        var fl = L2Fix4(dl);
        var fa = ab2Fix4(da);
        var fb = ab2Fix4(db);

        return (fl, fa, fb);
    }

    public ushort[] ToLabEncodedArray()
    {
        var dl = ClampLDoubleV4(L);
        var da = ClampabDoubleV4(a);
        var db = ClampabDoubleV4(b);

        var fl = L2Fix4(dl);
        var fa = ab2Fix4(da);
        var fb = ab2Fix4(db);

        return new[] { fl, fa, fb };
    }

    public LabEncodedV2 ToLabEncodedV2()
    {
        var dl = ClampLDoubleV2(L);
        var da = ClampabDoubleV2(a);
        var db = ClampabDoubleV2(b);

        var fl = L2Fix2(dl);
        var fa = ab2Fix2(da);
        var fb = ab2Fix2(db);

        return (fl, fa, fb);
    }

    public LCh ToLCh() =>
        new(L, Math.Pow(Sqr(a) + Sqr(b), 0.5), Atan2Deg(b, a));

    public Vec3 ToVec() =>
        new(L, a, b);

    public XYZ ToXYZ(XYZ? whitePoint = null)
    {
        whitePoint ??= WhitePoint.D50XYZ;

        var y = (L + 16.0) / 116.0;
        var x = y + (0.002 * a);
        var z = y - (0.005 * b);

        return (F1(x) * whitePoint.Value.X, F1(y) * whitePoint.Value.Y, F1(z) * whitePoint.Value.Z);
    }

    #endregion Public Methods
}
