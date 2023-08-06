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

namespace lcms2.testbed;

internal static partial class Testbed
{
    private static bool CheckOneRGB_f(Transform xform, int R, int G, int B, double X, double Y, double Z, double err)
    {
        Span<float> RGB = stackalloc float[3];
        Span<double> Out = stackalloc double[3];

        RGB[0] = (float)(R / 255.0);
        RGB[1] = (float)(G / 255.0);
        RGB[2] = (float)(B / 255.0);

        cmsDoTransform(xform, RGB, Out, 1);

        return IsGoodVal("X", X, Out[0], err) &&
               IsGoodVal("Y", Y, Out[1], err) &&
               IsGoodVal("Z", Z, Out[2], err);
    }

    internal static bool Check_sRGB_Float()
    {
        var hsRGB = cmsCreate_sRGBProfileTHR(DbgThread())!;
        var hXYZ = cmsCreateXYZProfileTHR(DbgThread())!;
        var hLab = cmsCreateLab4ProfileTHR(DbgThread(), null)!;

        var xform1 = cmsCreateTransformTHR(DbgThread(), hsRGB, TYPE_RGB_FLT, hXYZ, TYPE_XYZ_DBL, INTENT_RELATIVE_COLORIMETRIC, 0)!;

        var xform2 = cmsCreateTransformTHR(DbgThread(), hsRGB, TYPE_RGB_FLT, hLab, TYPE_Lab_DBL, INTENT_RELATIVE_COLORIMETRIC, 0)!;

        cmsCloseProfile(hsRGB);
        cmsCloseProfile(hXYZ);
        cmsCloseProfile(hLab);

        MaxErr = 0;

        bool rc;

        using (logger.BeginScope("8 bits to XYZ"))
        {
            rc = CheckOneRGB_f(xform1, 1, 1, 1, 0.0002927, 0.0003035, 0.000250, 0.0001);
            rc &= CheckOneRGB_f(xform1, 127, 127, 127, 0.2046329, 0.212230, 0.175069, 0.0001);
            rc &= CheckOneRGB_f(xform1, 12, 13, 15, 0.0038364, 0.0039928, 0.003853, 0.0001);
            rc &= CheckOneRGB_f(xform1, 128, 0, 0, 0.0941240, 0.0480256, 0.003005, 0.0001);
            rc &= CheckOneRGB_f(xform1, 190, 25, 210, 0.3204592, 0.1605926, 0.468213, 0.0001);
        }

        using (logger.BeginScope("8 bits to Lab"))
        {
            rc &= CheckOneRGB_f(xform2, 1, 1, 1, 0.2741748, 0, 0, 0.01);
            rc &= CheckOneRGB_f(xform2, 127, 127, 127, 53.192776, 0, 0, 0.01);
            rc &= CheckOneRGB_f(xform2, 190, 25, 210, 47.052136, 74.565610, -56.883274, 0.01);
            rc &= CheckOneRGB_f(xform2, 128, 0, 0, 26.164701, 48.478171, 39.4384713, 0.01);
        }

        cmsDeleteTransform(xform1);
        cmsDeleteTransform(xform2);
        return rc;
    }

    private static bool CheckGray(Transform xform, byte g, double L)
    {
        var Lab = new CIELab();

        cmsDoTransform(xform, g, ref Lab, 1);

        if (!IsGoodVal("a axis on gray", 0, Lab.a, 0.001)) return false;
        if (!IsGoodVal("b axis on gray", 0, Lab.b, 0.001)) return false;

        return IsGoodVal("Gray value", L, Lab.L, 0.01);
    }

    internal static bool CheckInputGray()
    {
        var hGray = Create_Gray22();
        var hLab = cmsCreateLab4Profile(null);

        if (hGray is null || hLab is null) return false;

        var xform = cmsCreateTransform(hGray, TYPE_GRAY_8, hLab, TYPE_Lab_DBL, INTENT_RELATIVE_COLORIMETRIC, 0)!;
        cmsCloseProfile(hGray);
        cmsCloseProfile(hLab);

        if (!CheckGray(xform, 0, 0)) return false;
        if (!CheckGray(xform, 125, 52.768)) return false;
        if (!CheckGray(xform, 200, 81.069)) return false;
        if (!CheckGray(xform, 255, 100.0)) return false;

        cmsDeleteTransform(xform);
        return true;
    }

    internal static bool CheckLabInputGray()
    {
        var hGray = Create_GrayLab();
        var hLab = cmsCreateLab4Profile(null);

        if (hGray is null || hLab is null) return false;

        var xform = cmsCreateTransform(hGray, TYPE_GRAY_8, hLab, TYPE_Lab_DBL, INTENT_RELATIVE_COLORIMETRIC, 0)!;
        cmsCloseProfile(hGray);
        cmsCloseProfile(hLab);

        if (!CheckGray(xform, 0, 0)) return false;
        if (!CheckGray(xform, 125, 49.019)) return false;
        if (!CheckGray(xform, 200, 78.431)) return false;
        if (!CheckGray(xform, 255, 100.0)) return false;

        cmsDeleteTransform(xform);
        return true;
    }

    private static bool CheckOutGray(Transform xform, double L, byte g)
    {
        byte g_out = 0;
        var Lab = new CIELab(
            L: L,
            a: 0,
            b: 0);

        cmsDoTransform(xform, Lab, ref g_out, 1);

        return IsGoodVal("Gray value", g, g_out, 0.01);
    }

    internal static bool CheckOutputGray()
    {
        var hGray = Create_Gray22();
        var hLab = cmsCreateLab4Profile(null);

        if (hGray is null || hLab is null) return false;

        var xform = cmsCreateTransform(hLab, TYPE_Lab_DBL, hGray, TYPE_GRAY_8, INTENT_RELATIVE_COLORIMETRIC, 0)!;
        cmsCloseProfile(hGray);
        cmsCloseProfile(hLab);

        if (!CheckOutGray(xform, 0, 0)) return false;
        if (!CheckOutGray(xform, 100, 255)) return false;
        if (!CheckOutGray(xform, 20, 52)) return false;
        if (!CheckOutGray(xform, 50, 118)) return false;

        cmsDeleteTransform(xform);
        return true;
    }

    internal static bool CheckLabOutputGray()
    {
        var hGray = Create_GrayLab();
        var hLab = cmsCreateLab4Profile(null);

        if (hGray is null || hLab is null) return false;

        var xform = cmsCreateTransform(hLab, TYPE_Lab_DBL, hGray, TYPE_GRAY_8, INTENT_RELATIVE_COLORIMETRIC, 0)!;
        cmsCloseProfile(hGray);
        cmsCloseProfile(hLab);

        if (!CheckOutGray(xform, 0, 0)) return false;
        if (!CheckOutGray(xform, 100, 255)) return false;

        for (var i = 0; i < 100; i++)
        {
            var g = (byte)Math.Floor(i * 255.0 / 100.0 + 0.5);

            if (!CheckOutGray(xform, i, g)) return false;
        }

        cmsDeleteTransform(xform);
        return true;
    }
}
