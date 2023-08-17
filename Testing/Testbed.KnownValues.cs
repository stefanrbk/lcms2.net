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

using Microsoft.Extensions.Logging;

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
        cmsDoTransform(xform, g, out CIELab Lab, 1);

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
        var Lab = new CIELab(
            L: L,
            a: 0,
            b: 0);

        cmsDoTransform(xform, Lab, out byte g_out, 1);

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

    private static bool CheckCMYK(int Intent, byte[] Profile1, byte[] Profile2)
    {
        var hSWOP = cmsOpenProfileFromMemTHR(DbgThread(), Profile1)!;
        var hFOGRA = cmsOpenProfileFromMemTHR(DbgThread(), Profile2)!;

        Span<float> CMYK1 = stackalloc float[4];
        Span<float> CMYK2 = stackalloc float[4];

        var hLab = cmsCreateLab4ProfileTHR(DbgThread(), null)!;

        var xform = cmsCreateTransformTHR(DbgThread(), hSWOP, TYPE_CMYK_FLT, hFOGRA, TYPE_CMYK_FLT, (uint)Intent, 0)!;

        var swop_lab = cmsCreateTransformTHR(DbgThread(), hSWOP, TYPE_CMYK_FLT, hLab, TYPE_Lab_DBL, (uint)Intent, 0)!;
        var fogra_lab = cmsCreateTransformTHR(DbgThread(), hFOGRA, TYPE_CMYK_FLT, hLab, TYPE_Lab_DBL, (uint)Intent, 0)!;

        var Max = 0.0;
        for (var i = 0; i <= 100; i++)
        {
            CMYK1[0] = 10;
            CMYK1[1] = 20;
            CMYK1[2] = 30;
            CMYK1[3] = i;

            cmsDoTransform(swop_lab, CMYK1, out CIELab Lab1, 1);
            cmsDoTransform(xform, CMYK1, CMYK2, 1);
            cmsDoTransform(fogra_lab, CMYK2, out CIELab Lab2, 1);

            var DeltaL = Math.Abs(Lab1.L - Lab2.L);

            Max = Math.Max(Max, DeltaL);
        }

        cmsDeleteTransform(xform);

        xform = cmsCreateTransformTHR(DbgThread(), hFOGRA, TYPE_CMYK_FLT, hSWOP, TYPE_CMYK_FLT, (uint)Intent, 0)!;

        for (var i = 0; i <= 100; i++)
        {
            CMYK1[0] = 10;
            CMYK1[1] = 20;
            CMYK1[2] = 30;
            CMYK1[3] = i;

            cmsDoTransform(fogra_lab, CMYK1, out CIELab Lab1, 1);
            cmsDoTransform(xform, CMYK1, CMYK2, 1);
            cmsDoTransform(swop_lab, CMYK2, out CIELab Lab2, 1);

            var DeltaL = Math.Abs(Lab1.L - Lab2.L);

            Max = Math.Max(Max, DeltaL);
        }

        cmsCloseProfile(hSWOP);
        cmsCloseProfile(hFOGRA);
        cmsCloseProfile(hLab);

        cmsDeleteTransform(xform);
        cmsDeleteTransform(swop_lab);
        cmsDeleteTransform(fogra_lab);

        return Max < 3.0;
    }

    internal static bool CheckCMYKRoundtrip() =>
        CheckCMYK(INTENT_RELATIVE_COLORIMETRIC, TestProfiles.test1, TestProfiles.test1);

    internal static bool CheckCMYKPerceptual() =>
        CheckCMYK(INTENT_PERCEPTUAL, TestProfiles.test1, TestProfiles.test2);

    internal static bool CheckCMYKRelCol() =>
        CheckCMYK(INTENT_RELATIVE_COLORIMETRIC, TestProfiles.test1, TestProfiles.test2);

    internal static bool CheckProofingXFORMFloat()
    {
        var hAbove = Create_AboveRGB()!;
        var xform = cmsCreateProofingTransformTHR(DbgThread(), hAbove, TYPE_RGB_FLT, hAbove, TYPE_RGB_FLT, hAbove,
            INTENT_RELATIVE_COLORIMETRIC, INTENT_RELATIVE_COLORIMETRIC, cmsFLAGS_SOFTPROOFING)!;

        cmsCloseProfile(hAbove);
        var rc = CheckFloatLinearXFORM(xform, 3);
        cmsDeleteTransform(xform);
        return rc;
    }

    internal static bool CheckProofingXFORM16()
    {
        var hAbove = Create_AboveRGB()!;
        var xform = cmsCreateProofingTransformTHR(DbgThread(), hAbove, TYPE_RGB_16, hAbove, TYPE_RGB_16, hAbove,
            INTENT_RELATIVE_COLORIMETRIC, INTENT_RELATIVE_COLORIMETRIC, cmsFLAGS_SOFTPROOFING|cmsFLAGS_NOCACHE)!;

        cmsCloseProfile(hAbove);
        var rc = Check16linearXFORM(xform, 3);
        cmsDeleteTransform(xform);
        return rc;
    }

    internal static bool CheckGamutCheck()
    {
        var Alarm = new ushort[16];
        Alarm[0] = 0xDEAD;
        Alarm[1] = 0xBABE;
        Alarm[2] = 0xFACE;

        // Set alarm codes to fancy values so we could check the out of gamut condition
        cmsSetAlarmCodes(Alarm);

        // Create the profiles
        var hSRGB = cmsCreate_sRGBProfileTHR(DbgThread());
        var hAbove = Create_AboveRGB();

        if (hSRGB is null || hAbove is null)
            return false;   // Failed

        using (logger.BeginScope("Gamut check on floating point"))
        {
            // Create a gamut checker in the same space. No value should be out of gamut
            var xform = cmsCreateProofingTransformTHR(DbgThread(), hAbove, TYPE_RGB_FLT, hAbove, TYPE_RGB_FLT, hAbove,
                INTENT_RELATIVE_COLORIMETRIC, INTENT_RELATIVE_COLORIMETRIC, cmsFLAGS_GAMUTCHECK);

            if (!CheckFloatLinearXFORM(xform, 3))
            {
                cmsCloseProfile(hSRGB);
                cmsCloseProfile(hAbove);
                cmsDeleteTransform(xform);
                logger.LogWarning("Gamut check on same profile failed");
                return false;
            }

            cmsDeleteTransform(xform);
        }

        using (logger.BeginScope("Gamut check on 16 bits"))
        {
            var xform = cmsCreateProofingTransformTHR(DbgThread(), hAbove, TYPE_RGB_16, hAbove, TYPE_RGB_16, hSRGB,
                INTENT_RELATIVE_COLORIMETRIC, INTENT_RELATIVE_COLORIMETRIC, cmsFLAGS_GAMUTCHECK);

            cmsCloseProfile(hSRGB);
            cmsCloseProfile(hAbove);

            if (!Check16linearXFORM(xform, 3))
            {
                cmsDeleteTransform(xform);
                logger.LogWarning("Gamut check on 16 bits failed");
                return false;
            }

            cmsDeleteTransform(xform);
        }

        return true;
    }

    internal static bool CheckKOnlyBlackPreserving()
    {
        var hSWOP = cmsOpenProfileFromMemTHR(DbgThread(), TestProfiles.test1)!;
        var hFOGRA = cmsOpenProfileFromMemTHR(DbgThread(), TestProfiles.test2)!;
        var CMYK1 = new float[4];
        var CMYK2 = new float[4];

        var hLab = cmsCreateLab4ProfileTHR(DbgThread(), null)!;

        var xform = cmsCreateTransformTHR(DbgThread(), hSWOP, TYPE_CMYK_FLT, hFOGRA, TYPE_CMYK_FLT, INTENT_PRESERVE_K_ONLY_PERCEPTUAL, 0)!;

        var swop_lab = cmsCreateTransformTHR(DbgThread(), hSWOP, TYPE_CMYK_FLT, hLab, TYPE_Lab_DBL, INTENT_PERCEPTUAL, 0)!;
        var fogra_lab = cmsCreateTransformTHR(DbgThread(), hFOGRA, TYPE_CMYK_FLT, hLab, TYPE_Lab_DBL, INTENT_PERCEPTUAL, 0)!;

        var Max = 0.0;

        using (logger.BeginScope("SWOP to FOGRA"))
        {
            for (var i = 0; i <= 100; i++)
            {
                CMYK1[0] = 0;
                CMYK1[1] = 0;
                CMYK1[2] = 0;
                CMYK1[3] = i;

                // SWOP CMYK to Lab1
                cmsDoTransform(swop_lab, CMYK1, out CIELab Lab1, 1);

                // SWOP To FOGRA using black preservation
                cmsDoTransform(xform, CMYK1, CMYK2, 1);

                // Obtained FOGRA CMYK to Lab2
                cmsDoTransform(fogra_lab, CMYK2, out CIELab Lab2, 1);

                // We care only on L*
                var DeltaL = Math.Abs(Lab1.L - Lab2.L);

                if (DeltaL > Max) Max = DeltaL;
            }

            cmsDeleteTransform(xform);

            if (Max >= 3.0)
            {
                logger.LogWarning("Delta was >= 3.0 ({delta})", Max);

                cmsCloseProfile(hSWOP);
                cmsCloseProfile(hFOGRA);
                cmsCloseProfile(hLab);

                cmsDeleteTransform(swop_lab);
                cmsDeleteTransform(fogra_lab);

                return false;
            }
        }

        using (logger.BeginScope("FOGRA to SWOP"))
        {
            Max = 0;

            xform = cmsCreateTransformTHR(DbgThread(), hFOGRA, TYPE_CMYK_FLT, hSWOP, TYPE_CMYK_FLT, INTENT_PRESERVE_K_ONLY_PERCEPTUAL, 0)!;

            for (var i = 0; i <= 100; i++)
            {
                CMYK1[0] = 0;
                CMYK1[1] = 0;
                CMYK1[2] = 0;
                CMYK1[3] = i;

                cmsDoTransform(fogra_lab, CMYK1, out CIELab Lab1, 1);
                cmsDoTransform(xform, CMYK1, CMYK2, 1);
                cmsDoTransform(swop_lab, CMYK2, out CIELab Lab2, 1);

                var DeltaL = Math.Abs(Lab1.L - Lab2.L);

                if (DeltaL > Max) Max = DeltaL;
            }

            cmsDeleteTransform(xform);

            if (Max >= 3.0)
            {
                logger.LogWarning("Delta was >= 3.0 ({delta})", Max);

                cmsCloseProfile(hSWOP);
                cmsCloseProfile(hFOGRA);
                cmsCloseProfile(hLab);

                cmsDeleteTransform(swop_lab);
                cmsDeleteTransform(fogra_lab);

                return false;
            }
        }

        return true;
    }

    internal static bool CheckKPlaneBlackPreserving()
    {
        var hSWOP = cmsOpenProfileFromMemTHR(DbgThread(), TestProfiles.test1)!;
        var hFOGRA = cmsOpenProfileFromMemTHR(DbgThread(), TestProfiles.test2)!;
        var CMYK1 = new float[4];
        var CMYK2 = new float[4];

        var hLab = cmsCreateLab4ProfileTHR(DbgThread(), null)!;

        var xform = cmsCreateTransformTHR(DbgThread(), hSWOP, TYPE_CMYK_FLT, hFOGRA, TYPE_CMYK_FLT, INTENT_PERCEPTUAL, 0)!;

        var swop_lab = cmsCreateTransformTHR(DbgThread(), hSWOP, TYPE_CMYK_FLT, hLab, TYPE_Lab_DBL, INTENT_PERCEPTUAL, 0)!;
        var fogra_lab = cmsCreateTransformTHR(DbgThread(), hFOGRA, TYPE_CMYK_FLT, hLab, TYPE_Lab_DBL, INTENT_PERCEPTUAL, 0)!;

        var Max = 0.0;

        using (logger.BeginScope("SWOP to FOGRA"))
        {
            for (var i = 0; i <= 100; i++)
            {
                CMYK1[0] = 0;
                CMYK1[1] = 0;
                CMYK1[2] = 0;
                CMYK1[3] = i;

                cmsDoTransform(swop_lab, CMYK1, out CIELab Lab1, 1);
                cmsDoTransform(xform, CMYK1, CMYK2, 1);
                cmsDoTransform(fogra_lab, CMYK2, out CIELab Lab2, 1);

                var DeltaE = cmsDeltaE(Lab1, Lab2);

                if (DeltaE > Max) Max = DeltaE;
            }

            cmsDeleteTransform(xform);

            if (Max >= 30.0)
            {
                logger.LogWarning("Delta was >= 30.0 ({delta})", Max);

                cmsCloseProfile(hSWOP);
                cmsCloseProfile(hFOGRA);
                cmsCloseProfile(hLab);

                cmsDeleteTransform(swop_lab);
                cmsDeleteTransform(fogra_lab);

                return false;
            }
        }

        using (logger.BeginScope("FOGRA to SWOP"))
        {
            Max = 0;

            xform = cmsCreateTransformTHR(DbgThread(), hFOGRA, TYPE_CMYK_FLT, hSWOP, TYPE_CMYK_FLT, INTENT_PRESERVE_K_PLANE_PERCEPTUAL, 0)!;

            for (var i = 0; i <= 100; i++)
            {
                CMYK1[0] = 30;
                CMYK1[1] = 20;
                CMYK1[2] = 10;
                CMYK1[3] = i;

                cmsDoTransform(fogra_lab, CMYK1, out CIELab Lab1, 1);
                cmsDoTransform(xform, CMYK1, CMYK2, 1);
                cmsDoTransform(swop_lab, CMYK2, out CIELab Lab2, 1);

                var DeltaE = cmsDeltaE(Lab1, Lab2);

                if (DeltaE > Max) Max = DeltaE;
            }

            cmsDeleteTransform(xform);

            if (Max >= 30.0)
            {
                logger.LogWarning("Delta was >= 30.0 ({delta})", Max);

                cmsCloseProfile(hSWOP);
                cmsCloseProfile(hFOGRA);
                cmsCloseProfile(hLab);

                cmsDeleteTransform(swop_lab);
                cmsDeleteTransform(fogra_lab);

                return false;
            }
        }

        return true;
    }

    internal static bool CheckV4gamma()
    {
        var Lin = new ushort[] { 0, 0xFFFF };
        var g = cmsBuildTabulatedToneCurve16(DbgThread(), 2, Lin);

        var h = cmsOpenProfileFromFileTHR(DbgThread(), "v4gamma.icc", "w");
        if (h is null)
            return false;

        cmsSetProfileVersion(h, 4.3);

        if (!cmsWriteTag(h, cmsSigGrayTRCTag, g))
            return false;
        cmsCloseProfile(h);

        cmsFreeToneCurve(g);
        remove("v4gamma.icc");
        return true;
    }

    internal static bool CheckBlackPoint()
    {
        var hProfile = cmsOpenProfileFromMemTHR(DbgThread(), TestProfiles.test5)!;
        var Black = cmsDetectDestinationBlackPoint(hProfile, INTENT_RELATIVE_COLORIMETRIC);
        cmsCloseProfile(hProfile);

        hProfile = cmsOpenProfileFromMemTHR(DbgThread(), TestProfiles.test1)!;
        Black = cmsDetectDestinationBlackPoint(hProfile, INTENT_RELATIVE_COLORIMETRIC);
        cmsXYZ2Lab(null, out var Lab, Black);
        cmsCloseProfile(hProfile);

        hProfile = cmsOpenProfileFromFileTHR(DbgThread(), "lcms2cmyk.icc", "r")!;
        Black = cmsDetectDestinationBlackPoint(hProfile, INTENT_RELATIVE_COLORIMETRIC);
        cmsXYZ2Lab(null, out Lab, Black);
        cmsCloseProfile(hProfile);

        hProfile = cmsOpenProfileFromMemTHR(DbgThread(), TestProfiles.test2)!;
        Black = cmsDetectDestinationBlackPoint(hProfile, INTENT_RELATIVE_COLORIMETRIC);
        cmsXYZ2Lab(null, out Lab, Black);
        cmsCloseProfile(hProfile);

        hProfile = cmsOpenProfileFromMemTHR(DbgThread(), TestProfiles.test1)!;
        Black = cmsDetectDestinationBlackPoint(hProfile, INTENT_PERCEPTUAL);
        cmsXYZ2Lab(null, out Lab, Black);
        cmsCloseProfile(hProfile);

        return true;
    }
    
    internal static bool CheckOneTAC(double InkLimit)
    {
        var h = CreateFakeCMYK(InkLimit, true)!;
        cmsSaveProfileToFile(h, "lcmstac.icc");
        cmsCloseProfile(h);

        h = cmsOpenProfileFromFile("lcmstac.icc", "r")!;
        var d = cmsDetectTAC(h);
        cmsCloseProfile(h);

        remove("lcmstac.icc");

        return Math.Abs(d - InkLimit) <= 5;
    }


    internal static bool CheckTAC()
    {
        if (!CheckOneTAC(180))
            return false;
        if (!CheckOneTAC(220))
            return false;
        if (!CheckOneTAC(286))
            return false;
        if (!CheckOneTAC(310))
            return false;
        if (!CheckOneTAC(330))
            return false;
        return true;
    }
}
