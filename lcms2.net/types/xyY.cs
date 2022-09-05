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

public struct xyY : ICloneable
{
    #region Fields

    public double x;
    public double y;
    public double Y;

    #endregion Fields

    #region Public Constructors

    public xyY(double x, double y, double Y) =>
        (this.x, this.y, this.Y) = (x, y, Y);

    #endregion Public Constructors

    #region Public Methods

    public static explicit operator XYZ(xyY xyy) =>
        xyy.ToXYZ();

    public static implicit operator xyY((double, double, double) v) =>
            new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
           new xyY(x, y, Y);

    public XYZ ToXYZ()
    {
        var dx = x / y * Y;
        var dz = (1 - x - y) / y * Y;

        return (dx, Y, dz);
    }

    #endregion Public Methods
}

public struct xyYTripple : ICloneable
{
    #region Fields

    public xyY Blue;
    public xyY Green;
    public xyY Red;

    #endregion Fields

    #region Public Constructors

    public xyYTripple(xyY red, xyY green, xyY blue) =>
        (Red, Green, Blue) = (red, green, blue);

    #endregion Public Constructors

    #region Public Methods

    public static implicit operator xyYTripple((xyY, xyY, xyY) v) =>
        new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
        new xyYTripple(Red, Green, Blue);

    #endregion Public Methods
}
