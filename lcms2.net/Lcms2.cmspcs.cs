//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
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

using lcms2.types;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static double f(double t)
    {
        const double Limit = 24.0 / 116 * (24.0 / 116) * (24.0 / 116);

        return (t <= Limit)
            ? (841.0 / 108 * t) + (16.0 / 116)
            : Math.Pow(t, 1.0 / 3);
    }

    private static double f_1(double t)
    {
        const double Limit = 24.0 / 116;

        return (t <= Limit)
            ? 108.0 / 841 * (t - (16.0 / 116))
            : t * t * t;
    }

    public static void cmsXYZ2Lab(CIEXYZ* WhitePoint, CIELab* Lab, in CIEXYZ* xyz)
    {
        if (WhitePoint is null)
            WhitePoint = cmsD50_XYZ();

        var fx = f(xyz->X / WhitePoint->X);
        var fy = f(xyz->Y / WhitePoint->Y);
        var fz = f(xyz->Z / WhitePoint->Z);

        Lab->L = (116 * fy) - 16;
        Lab->a = 500 * (fx - fy);
        Lab->b = 200 * (fy - fz);
    }

    public static void cmsLab2XYZ(CIEXYZ* WhitePoint, CIEXYZ* xyz, in CIELab* Lab)
    {
        if (WhitePoint is null)
            WhitePoint = cmsD50_XYZ();

        var y = (Lab->L + 16.0) / 116.0;
        var x = y + (0.002 * Lab->a);
        var z = y - (0.005 * Lab->b);

        xyz->X = f_1(x) * WhitePoint->X;
        xyz->Y = f_1(y) * WhitePoint->Y;
        xyz->Z = f_1(z) * WhitePoint->Z;
    }
}
