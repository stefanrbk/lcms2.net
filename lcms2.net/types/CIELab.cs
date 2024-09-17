//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright ©️ 1998-2024 Marti Maria Saguer
//              2022-2024 Stefan Kewatt
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

namespace lcms2.types;

public struct CIELab(double L, double a, double b)
{
    public double L = L;
    public double a = a;
    public double b = b;

    public static CIELab NaN =>
        new(double.NaN, double.NaN, double.NaN);

    public readonly bool IsNaN =>
        double.IsNaN(L) || double.IsNaN(a) || double.IsNaN(b);

    public readonly CIELCh AsLCh =>
        new(L, Math.Pow(Sqr(a) + Sqr(b), 0.5), atan2deg(b, a));

    public static explicit operator CIELCh(CIELab lab) =>
        lab.AsLCh;

    public readonly CIEXYZ AsXYZ(CIEXYZ? WhitePoint = null)
    {
        var wp = WhitePoint ?? CIEXYZ.D50;

        var y = (L + 16.0) / 116.0;
        var x = y + (0.002 * a);
        var z = y - (0.005 * b);

        return new(f_1(x) * wp.X, f_1(y) * wp.Y, f_1(z) * wp.Z);
    }
}
