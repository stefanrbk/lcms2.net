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
    private static bool Check8linearXFORM(Transform xform, int nChan)
    {
        Span<byte> Inw = stackalloc byte[cmsMAXCHANNELS];
        Span<byte> Outw = stackalloc byte[cmsMAXCHANNELS];

        var n2 = 0;

        for (var j = 0; j < 0xFF; j++)
        {
            Inw.Fill((byte)j);
            cmsDoTransform(xform, Inw, Outw, 1);

            for (var i = 0; i < nChan; i++)
            {
                var dif = Math.Abs(Outw[i] - j);
                if (dif > n2) n2 = dif;
            }
        }

        // We allow 2 instances of difference on 8 bits
        if (n2 > 2)
        {
            logger.LogWarning("Differences too big ({dif})", n2);
            return false;
        }

        return true;
    }

    private static bool Check8bitXFORM(Transform xform1, Transform xform2, int nChan)
    {
        Span<byte> Inw = stackalloc byte[cmsMAXCHANNELS];
        Span<byte> Outw1 = stackalloc byte[cmsMAXCHANNELS];
        Span<byte> Outw2 = stackalloc byte[cmsMAXCHANNELS];

        var n2 = 0;

        for (var j = 0; j < 0xFF; j++)
        {
            Inw.Fill((byte)j);
            cmsDoTransform(xform1, Inw, Outw1, 1);
            cmsDoTransform(xform2, Inw, Outw2, 1);

            for (var i = 0; i < nChan; i++)
            {
                var dif = Math.Abs(Outw2[i] - Outw1[i]);
                if (dif > n2) n2 = dif;
            }
        }

        // We allow 2 instances of difference on 8 bits
        if (n2 > 2)
        {
            logger.LogWarning("Differences too big ({dif})", n2);
            return false;
        }

        return true;
    }

    private static bool Check16linearXFORM(Transform xform, int nChan)
    {
        Span<ushort> Inw = stackalloc ushort[cmsMAXCHANNELS];
        Span<ushort> Outw = stackalloc ushort[cmsMAXCHANNELS];

        var n2 = 0;

        for (var j = 0; j < 0xFFFF; j++)
        {
            Inw.Fill((ushort)j);
            cmsDoTransform(xform, Inw, Outw, 1);

            for (var i = 0; i < nChan; i++)
            {
                var dif = Math.Abs(Outw[i] - j);
                if (dif > n2) n2 = dif;
            }
        }

        // We allow 2 instances of difference on 16 bits
        if (n2 > 0x200)
        {
            logger.LogWarning("Differences too big ({dif})", n2);
            return false;
        }

        return true;
    }

    private static bool Check16bitXFORM(Transform xform1, Transform xform2, int nChan)
    {
        Span<ushort> Inw = stackalloc ushort[cmsMAXCHANNELS];
        Span<ushort> Outw1 = stackalloc ushort[cmsMAXCHANNELS];
        Span<ushort> Outw2 = stackalloc ushort[cmsMAXCHANNELS];

        var n2 = 0;

        for (var j = 0; j < 0xFFFF; j++)
        {
            Inw.Fill((ushort)j);
            cmsDoTransform(xform1, Inw, Outw1, 1);
            cmsDoTransform(xform2, Inw, Outw2, 1);

            for (var i = 0; i < nChan; i++)
            {
                var dif = Math.Abs(Outw2[i] - Outw1[i]);
                if (dif > n2) n2 = dif;
            }
        }

        // We allow 2 instances of difference on 16 bits
        if (n2 > 0x200)
        {
            logger.LogWarning("Differences too big ({dif})", n2);
            return false;
        }

        return true;
    }

    private static bool CheckFloatLinearXFORM(Transform xform, int nChan)
    {
        Span<float> In = stackalloc float[cmsMAXCHANNELS];
        Span<float> Out = stackalloc float[cmsMAXCHANNELS];

        for (var j = 0; j < 0xFFFF; j++)
        {
            In.Fill((float)(j / 65535.0));
            cmsDoTransform(xform, In, Out, 1);

            for (var i = 0; i < nChan; i++)
            {
                // We allow no difference in floating point
                if (!IsGoodFixed15_16("linear xform float", Out[i], (float)(j / 65535.0)))
                    return false;
            }
        }

        return true;
    }

    private static bool CheckFloatXFORM(Transform xform1, Transform xform2, int nChan)
    {
        Span<float> In = stackalloc float[cmsMAXCHANNELS];
        Span<float> Out1 = stackalloc float[cmsMAXCHANNELS];
        Span<float> Out2 = stackalloc float[cmsMAXCHANNELS];

        for (var j = 0; j < 0xFFFF; j++)
        {
            In.Fill((float)(j / 65535.0));
            cmsDoTransform(xform1, In, Out1, 1);
            cmsDoTransform(xform2, In, Out2, 1);

            for (var i = 0; i < nChan; i++)
            {
                // We allow no difference in floating point
                if (!IsGoodFixed15_16("linear xform float", Out1[i], Out2[i]))
                    return false;
            }
        }

        return true;
    }

    internal static bool CheckCurvesOnlyTransforms()
    {
        var rc = true;

        ToneCurve[] c1 = new ToneCurve[] { cmsBuildGamma(DbgThread(), 2.2)! };
        ToneCurve[] c2 = new ToneCurve[] { cmsBuildGamma(DbgThread(), 1 / 2.2)! };
        ToneCurve[] c3 = new ToneCurve[] { cmsBuildGamma(DbgThread(), 4.84)! };

        var h1 = cmsCreateLinearizationDeviceLinkTHR(DbgThread(), cmsSigGrayData, c1)!;
        var h2 = cmsCreateLinearizationDeviceLinkTHR(DbgThread(), cmsSigGrayData, c2)!;
        var h3 = cmsCreateLinearizationDeviceLinkTHR(DbgThread(), cmsSigGrayData, c3)!;

        using (logger.BeginScope("Gray float optimizable transform"))
        {
            var xform1 = cmsCreateTransform(h1, TYPE_GRAY_FLT, h2, TYPE_GRAY_FLT, INTENT_PERCEPTUAL, 0)!;
            rc &= CheckFloatLinearXFORM(xform1, 1);
            cmsDeleteTransform(xform1);
            if (!rc) goto Error;
        }

        using (logger.BeginScope("Gray 16 optimizable transform"))
        {
            var xform1 = cmsCreateTransform(h1, TYPE_GRAY_16, h2, TYPE_GRAY_16, INTENT_PERCEPTUAL, 0)!;
            rc &= Check16linearXFORM(xform1, 1);
            cmsDeleteTransform(xform1);
            if (!rc) goto Error;
        }

        using (logger.BeginScope("Gray 16 optimizable transform"))
        {
            var xform1 = cmsCreateTransform(h1, TYPE_GRAY_16, h2, TYPE_GRAY_16, INTENT_PERCEPTUAL, 0)!;
            rc &= Check16linearXFORM(xform1, 1);
            cmsDeleteTransform(xform1);
            if (!rc) goto Error;
        }

        using (logger.BeginScope("Gray float non-optimizable transform"))
        {
            var xform1 = cmsCreateTransform(h1, TYPE_GRAY_FLT, h1, TYPE_GRAY_FLT, INTENT_PERCEPTUAL, 0)!;
            var xform2 = cmsCreateTransform(h3, TYPE_GRAY_FLT, null, TYPE_GRAY_FLT, INTENT_PERCEPTUAL, 0)!;

            rc &= CheckFloatXFORM(xform1, xform2, 1);
            cmsDeleteTransform(xform1);
            cmsDeleteTransform(xform2);
            if (!rc) goto Error;
        }

        using (logger.BeginScope("Gray 8 non-optimizable transform"))
        {
            var xform1 = cmsCreateTransform(h1, TYPE_GRAY_8, h1, TYPE_GRAY_8, INTENT_PERCEPTUAL, 0)!;
            var xform2 = cmsCreateTransform(h3, TYPE_GRAY_8, null, TYPE_GRAY_8, INTENT_PERCEPTUAL, 0)!;

            rc &= Check8bitXFORM(xform1, xform2, 1);
            cmsDeleteTransform(xform1);
            cmsDeleteTransform(xform2);
            if (!rc) goto Error;
        }

        using (logger.BeginScope("Gray 16 non-optimizable transform"))
        {
            var xform1 = cmsCreateTransform(h1, TYPE_GRAY_16, h1, TYPE_GRAY_16, INTENT_PERCEPTUAL, 0)!;
            var xform2 = cmsCreateTransform(h3, TYPE_GRAY_16, null, TYPE_GRAY_16, INTENT_PERCEPTUAL, 0)!;

            rc &= Check8bitXFORM(xform1, xform2, 1);
            cmsDeleteTransform(xform1);
            cmsDeleteTransform(xform2);
            if (!rc) goto Error;
        }

    Error:
        cmsCloseProfile(h1);
        cmsCloseProfile(h2);
        cmsCloseProfile(h3);

        cmsFreeToneCurve(c1[0]);
        cmsFreeToneCurve(c2[0]);
        cmsFreeToneCurve(c3[0]);

        return rc;
    }

    private static double MaxDE;

    private static bool CheckOneLab(Transform xform, double L, double a, double b)
    {
        Span<CIELab> In = stackalloc CIELab[1] { new CIELab(L, a, b) };
        Span<CIELab> Out = stackalloc CIELab[1];

        cmsDoTransform(xform, In, Out, 1);

        var dE = cmsDeltaE(In[0], Out[0]);

        if (dE > MaxDE) MaxDE = dE;

        if (MaxDE > 0.003)
        {
            logger.LogWarning("dE={dE}", MaxDE);
            logger.LogWarning("Lab1=({L}, {a}, {b})", In[0].L, In[0].a, In[0].b);
            logger.LogWarning("Lab2=({L}, {a}, {b})", Out[0].L, Out[0].a, Out[0].b);
            cmsDoTransform(xform, In, Out, 1);
            return false;
        }

        return true;
    }

    private static bool CheckSeveralLab(Transform xform)
    {
        MaxDE = 0;

        for (var L = 0; L < 65536; L += 1311)
        {
            for (var a = 0; a < 65536; a += 1232)
            {
                for (var b = 0; b < 65536; b += 1111)
                {
                    if (!CheckOneLab(xform, (L * 100.0) / 65535.0,
                                            (a / 257.0) - 128, (b / 257.0) - 128))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static bool OneTrivialLab(Profile hLab1, Profile hLab2, string txt)
    {
        using (logger.BeginScope("{txt}", txt))
        {
            var xform = cmsCreateTransformTHR(DbgThread(), hLab1, TYPE_Lab_DBL, hLab2, TYPE_Lab_DBL, INTENT_RELATIVE_COLORIMETRIC, 0)!;
            cmsCloseProfile(hLab1);
            cmsCloseProfile(hLab2);

            var rc = CheckSeveralLab(xform);
            cmsDeleteTransform(xform);
            return rc;
        }
    }

    internal static bool CheckFloatLabTransforms() =>
        OneTrivialLab(cmsCreateLab4ProfileTHR(DbgThread(), null)!, cmsCreateLab4ProfileTHR(DbgThread(), null)!, "Lab4/Lab4") &&
        OneTrivialLab(cmsCreateLab2ProfileTHR(DbgThread(), null)!, cmsCreateLab2ProfileTHR(DbgThread(), null)!, "Lab2/Lab2") &&
        OneTrivialLab(cmsCreateLab4ProfileTHR(DbgThread(), null)!, cmsCreateLab2ProfileTHR(DbgThread(), null)!, "Lab4/Lab2") &&
        OneTrivialLab(cmsCreateLab2ProfileTHR(DbgThread(), null)!, cmsCreateLab4ProfileTHR(DbgThread(), null)!, "Lab2/Lab4");
}
