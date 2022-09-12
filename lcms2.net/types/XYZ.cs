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

public struct XYZ : ICloneable
{
    #region Fields

    public double X;

    public double Y;

    public double Z;

    #endregion Fields

    #region Public Constructors

    public XYZ(double x, double y, double z) =>
        (X, Y, Z) = (x, y, z);

    #endregion Public Constructors

    #region Properties

    public static XYZ NaN =>
                        new(Double.NaN, Double.NaN, Double.NaN);

    public bool IsNaN =>
        Double.IsNaN(X) || Double.IsNaN(Y) || Double.IsNaN(Z);

    #endregion Properties

    #region Public Methods

    public static explicit operator Lab(XYZ xyz) =>
        xyz.ToLab();

    public static explicit operator Vec3(XYZ xyz) =>
        xyz.ToVec3();

    public static explicit operator xyY(XYZ xyz) =>
        xyz.ToxyY();

    public static implicit operator XYZ((double, double, double) v) =>
                new(v.Item1, v.Item2, v.Item3);

    public static Lab operator %(XYZ xyz, XYZ whitepoint) =>
        xyz.ToLab(whitepoint);

    public object Clone() =>
               new XYZ(X, Y, Z);

    public Lab ToLab(XYZ? whitePoint = null)
    {
        whitePoint ??= WhitePoint.D50XYZ;

        var fx = F(X / whitePoint.Value.X);
        var fy = F(Y / whitePoint.Value.Y);
        var fz = F(Z / whitePoint.Value.Z);

        var L = (116.0 * fy) - 16.0;
        var a = 500.0 * (fx - fy);
        var b = 200.0 * (fy - fz);

        return (L, a, b);
    }

    public Vec3 ToVec() =>
                                        new(X, Y, Z);

    public Vec3 ToVec3() =>
        new(X, Y, Z);

    public xyY ToxyY()
    {
        var iSum = 1.0 / (X + Y + Z);

        return new(X * iSum, Y * iSum, Y);
    }

    #endregion Public Methods
}
