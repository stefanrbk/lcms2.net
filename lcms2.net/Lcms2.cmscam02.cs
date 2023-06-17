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

using static lcms2.Lcms2;
using static System.Math;
using System.Runtime.ConstrainedExecution;
using lcms2.state;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static double compute_n(CIECAM02* pMod) =>
        pMod->Yb / pMod->adoptedWhite.XYZ[1];

    private static double compute_z(CIECAM02* pMod) =>
        1.48 + Pow(1.0 / pMod->n, 0.5);

    private static double computeNbb(CIECAM02* pMod) =>
        0.725 * Pow(1.0 / pMod->n, 0.2);

    private static double computeFL(CIECAM02* pMod)
    {
        var k = 1.0 / ((5.0 * pMod->LA) + 1.0);
        var FL = (0.2 * Pow(k, 4.0) * (5.0 * pMod->LA)) + 
            (0.1 * Pow(1.0 - Pow(k, 4.0), 2.0) * Pow(5.0 * pMod->LA, 1.0 / 3.0));

        return FL;
    }

    private static double computeD(CIECAM02* pMod) =>
        pMod->F - (1.0 / 3.6 * Exp((-pMod->LA - 42) / 92.0));

    private static CAM02COLOR XYZtoCAT02(CAM02COLOR clr)
    {
        clr.RGB[0] = (clr.XYZ[0] * 0.7328) + (clr.XYZ[1] * 0.4296) + (clr.XYZ[2] * -0.1624);
        clr.RGB[1] = (clr.XYZ[0] * -0.7036) + (clr.XYZ[1] * 1.6975) + (clr.XYZ[2] * 0.0061);
        clr.RGB[2] = (clr.XYZ[0] * 0.0030) + (clr.XYZ[1] * 0.0136) + (clr.XYZ[2] * 0.9834);

        return clr;
    }

    private static CAM02COLOR ChromaticAdaptation(CAM02COLOR clr, CIECAM02* pMod)
    {
        for(var i = 0; i < 3; i++)
        {
            clr.RGBc[i] = ((pMod->adoptedWhite.XYZ[1] *
                (pMod->D / pMod->adoptedWhite.RGB[i])) +
                (1.0 - pMod->D)) * clr.RGB[i];
        }

        return clr;
    }

    private static CAM02COLOR CAT02toHPE(CAM02COLOR clr)
    {
        var M = stackalloc double[9];

        M[0] = ((0.38971 * 1.096124) + (0.68898 * 0.454369) + (-0.07868 * -0.009628));
        M[1] = ((0.38971 * -0.278869) + (0.68898 * 0.473533) + (-0.07868 * -0.005698));
        M[2] = ((0.38971 * 0.182745) + (0.68898 * 0.072098) + (-0.07868 * 1.015326));
        M[3] = ((-0.22981 * 1.096124) + (1.18340 * 0.454369) + (0.04641 * -0.009628));
        M[4] = ((-0.22981 * -0.278869) + (1.18340 * 0.473533) + (0.04641 * -0.005698));
        M[5] = ((-0.22981 * 0.182745) + (1.18340 * 0.072098) + (0.04641 * 1.015326));
        M[6] = (-0.009628);
        M[7] = (-0.005698);
        M[8] = (1.015326);

        clr.RGBp[0] = (clr.RGBc[0] * M[0]) + (clr.RGBc[1] * M[1]) + (clr.RGBc[2] * M[2]);
        clr.RGBp[1] = (clr.RGBc[0] * M[3]) + (clr.RGBc[1] * M[4]) + (clr.RGBc[2] * M[5]);
        clr.RGBp[2] = (clr.RGBc[0] * M[6]) + (clr.RGBc[1] * M[7]) + (clr.RGBc[2] * M[8]);

        return clr;
    }

    private static CAM02COLOR NonlinearCompression(CAM02COLOR clr, CIECAM02* pMod)
    {
        for(var i = 0; i < 3; i++)
        {
            if (clr.RGBp[i] < 0)
            {

                var temp = Pow((-1.0 * pMod->FL * clr.RGBp[i] / 100.0), 0.42);
                clr.RGBpa[i] = (-1.0 * 400.0 * temp) / (temp + 27.13) + 0.1;
            }
            else
            {
                var temp = Pow((pMod->FL * clr.RGBp[i] / 100.0), 0.42);
                clr.RGBpa[i] = (400.0 * temp) / (temp + 27.13) + 0.1;
            }
        }

        clr.A = (((2.0 * clr.RGBpa[0]) + clr.RGBpa[1] +
            (clr.RGBpa[2] / 20.0)) - 0.305) * pMod->Nbb;

        return clr;
    }

    private static CAM02COLOR ComputeCorrelates(CAM02COLOR clr, CIECAM02* pMod)
    {
        double a, b, temp, e, t, r2d, d2r;

        a = clr.RGBpa[0] - (12.0 * clr.RGBpa[1] / 11.0) + (clr.RGBpa[2] / 11.0);
        b = (clr.RGBpa[0] + clr.RGBpa[1] - (2.0 * clr.RGBpa[2])) / 9.0;

        r2d = (180.0 / 3.141592654);
        if (a == 0)
        {
            if (b == 0) clr.h = 0;
            else if (b > 0) clr.h = 90;
            else clr.h = 270;
        }
        else if (a > 0)
        {
            temp = b / a;
            if (b > 0) clr.h = (r2d * Atan(temp));
            else if (b == 0) clr.h = 0;
            else clr.h = (r2d * Atan(temp)) + 360;
        }
        else
        {
            temp = b / a;
            clr.h = (r2d * Atan(temp)) + 180;
        }

        d2r = (3.141592654 / 180.0);
        e = ((12500.0 / 13.0) * pMod->Nc * pMod->Ncb) *
            (Cos((clr.h * d2r + 2.0)) + 3.8);

        if (clr.h < 20.14)
        {
            temp = ((clr.h + 122.47) / 1.2) + ((20.14 - clr.h) / 0.8);
            clr.H = 300 + (100 * ((clr.h + 122.47) / 1.2)) / temp;
        }
        else if (clr.h < 90.0)
        {
            temp = ((clr.h - 20.14) / 0.8) + ((90.00 - clr.h) / 0.7);
            clr.H = (100 * ((clr.h - 20.14) / 0.8)) / temp;
        }
        else if (clr.h < 164.25)
        {
            temp = ((clr.h - 90.00) / 0.7) + ((164.25 - clr.h) / 1.0);
            clr.H = 100 + ((100 * ((clr.h - 90.00) / 0.7)) / temp);
        }
        else if (clr.h < 237.53)
        {
            temp = ((clr.h - 164.25) / 1.0) + ((237.53 - clr.h) / 1.2);
            clr.H = 200 + ((100 * ((clr.h - 164.25) / 1.0)) / temp);
        }
        else
        {
            temp = ((clr.h - 237.53) / 1.2) + ((360 - clr.h + 20.14) / 0.8);
            clr.H = 300 + ((100 * ((clr.h - 237.53) / 1.2)) / temp);
        }

        clr.J = 100.0 * Pow((clr.A / pMod->adoptedWhite.A),
            (pMod->c * pMod->z));

        clr.Q = (4.0 / pMod->c) * Pow((clr.J / 100.0), 0.5) *
            (pMod->adoptedWhite.A + 4.0) * Pow(pMod->FL, 0.25);

        t = (e * Pow(((a * a) + (b * b)), 0.5)) /
            (clr.RGBpa[0] + clr.RGBpa[1] +
            ((21.0 / 20.0) * clr.RGBpa[2]));

        clr.C = Pow(t, 0.9) * Pow((clr.J / 100.0), 0.5) *
            Pow((1.64 - Pow(0.29, pMod->n)), 0.73);

        clr.M = clr.C * Pow(pMod->FL, 0.25);
        clr.s = 100.0 * Pow((clr.M / clr.Q), 0.5);

        return clr;
    }

    private static CAM02COLOR InverseCorrelates(CAM02COLOR clr, CIECAM02* pMod)
    {
        double t, e, p1, p2, p3, p4, p5, hr, d2r;
        d2r = 3.141592654 / 180.0;

        t = Pow((clr.C / (Pow((clr.J / 100.0), 0.5) *
            (Pow((1.64 - Pow(0.29, pMod->n)), 0.73)))),
            (1.0 / 0.9));
        e = ((12500.0 / 13.0) * pMod->Nc * pMod->Ncb) *
            (Cos((clr.h * d2r + 2.0)) + 3.8);

        clr.A = pMod->adoptedWhite.A * Pow(
               (clr.J / 100.0),
               (1.0 / (pMod->c * pMod->z)));

        p1 = e / t;
        p2 = (clr.A / pMod->Nbb) + 0.305;
        p3 = 21.0 / 20.0;

        hr = clr.h * d2r;

        if (Abs(Sin(hr)) >= Abs(Cos(hr)))
        {
            p4 = p1 / Sin(hr);
            clr.b = (p2 * (2.0 + p3) * (460.0 / 1403.0)) /
                (p4 + (2.0 + p3) * (220.0 / 1403.0) *
                (Cos(hr) / Sin(hr)) - (27.0 / 1403.0) +
                p3 * (6300.0 / 1403.0));
            clr.a = clr.b * (Cos(hr) / Sin(hr));
        }
        else
        {
            p5 = p1 / Cos(hr);
            clr.a = (p2 * (2.0 + p3) * (460.0 / 1403.0)) /
                (p5 + (2.0 + p3) * (220.0 / 1403.0) -
                ((27.0 / 1403.0) - p3 * (6300.0 / 1403.0)) *
                (Sin(hr) / Cos(hr)));
            clr.b = clr.a * (Sin(hr) / Cos(hr));
        }

        clr.RGBpa[0] = ((460.0 / 1403.0) * p2) +
                  ((451.0 / 1403.0) * clr.a) +
                  ((288.0 / 1403.0) * clr.b);
        clr.RGBpa[1] = ((460.0 / 1403.0) * p2) -
                  ((891.0 / 1403.0) * clr.a) -
                  ((261.0 / 1403.0) * clr.b);
        clr.RGBpa[2] = ((460.0 / 1403.0) * p2) -
                  ((220.0 / 1403.0) * clr.a) -
                  ((6300.0 / 1403.0) * clr.b);

        return clr;
    }

    private static CAM02COLOR InverseNonlinearity(CAM02COLOR clr, CIECAM02* pMod)
    {
        for (var i = 0; i < 3; i++)
        {
            var c1 = (clr.RGBpa[i] - 0.1) < 0 ? -1 : 1;
            clr.RGBp[i] = c1 * (100.0 / pMod->FL) *
                Pow(((27.13 * Abs(clr.RGBpa[i] - 0.1)) /
                (400.0 - Abs(clr.RGBpa[i] - 0.1))),
                (1.0 / 0.42));
        }

        return clr;
    }

    private static CAM02COLOR HPEtoCAT02(CAM02COLOR clr)
    {
        var M = stackalloc double[9];

        M[0] = ((0.7328 * 1.910197) + (0.4296 * 0.370950));
        M[1] = ((0.7328 * -1.112124) + (0.4296 * 0.629054));
        M[2] = ((0.7328 * 0.201908) + (0.4296 * 0.000008) - 0.1624);
        M[3] = ((-0.7036 * 1.910197) + (1.6975 * 0.370950));
        M[4] = ((-0.7036 * -1.112124) + (1.6975 * 0.629054));
        M[5] = ((-0.7036 * 0.201908) + (1.6975 * 0.000008) + 0.0061);
        M[6] = ((0.0030 * 1.910197) + (0.0136 * 0.370950));
        M[7] = ((0.0030 * -1.112124) + (0.0136 * 0.629054));
        M[8] = ((0.0030 * 0.201908) + (0.0136 * 0.000008) + 0.9834); ;

        clr.RGBc[0] = (clr.RGBp[0] * M[0]) + (clr.RGBp[1] * M[1]) + (clr.RGBp[2] * M[2]);
        clr.RGBc[1] = (clr.RGBp[0] * M[3]) + (clr.RGBp[1] * M[4]) + (clr.RGBp[2] * M[5]);
        clr.RGBc[2] = (clr.RGBp[0] * M[6]) + (clr.RGBp[1] * M[7]) + (clr.RGBp[2] * M[8]);
        return clr;
    }

    private static CAM02COLOR InverseChromaticAdaptation(CAM02COLOR clr, CIECAM02* pMod)
    {
        for (var i = 0; i < 3; i++)
        {
            clr.RGB[i] = clr.RGBc[i] /
                ((pMod->adoptedWhite.XYZ[1] * pMod->D / pMod->adoptedWhite.RGB[i]) + 1.0 - pMod->D);
        }

        return clr;
    }

    private static CAM02COLOR CAT02toXYZ(CAM02COLOR clr)
    {
        clr.XYZ[0] = (clr.RGB[0] * 1.096124) + (clr.RGB[1] * -0.278869) + (clr.RGB[2] * 0.182745);
        clr.XYZ[1] = (clr.RGB[0] * 0.454369) + (clr.RGB[1] * 0.473533) + (clr.RGB[2] * 0.072098);
        clr.XYZ[2] = (clr.RGB[0] * -0.009628) + (clr.RGB[1] * -0.005698) + (clr.RGB[2] * 1.015326);

        return clr;
    }

    public static HANDLE cmsCIECAM02INIT(Context? ContextID, in ViewingConditions* pVC)
    {
        CIECAM02* lpMod;

        _cmsAssert(pVC);

        if ((lpMod = _cmsMallocZero<CIECAM02>(ContextID)) is null)
            return null;

        lpMod->ContextID = ContextID;

        lpMod->adoptedWhite.XYZ[0] = pVC->whitePoint.X;
        lpMod->adoptedWhite.XYZ[1] = pVC->whitePoint.Y;
        lpMod->adoptedWhite.XYZ[2] = pVC->whitePoint.Z;

        lpMod->LA = pVC->La;
        lpMod->Yb = pVC->Yb;
        lpMod->D = pVC->D_value;
        lpMod->surround = pVC->surround;

        switch (lpMod->surround)
        {


            case CUTSHEET_SURROUND:
                lpMod->F = 0.8;
                lpMod->c = 0.41;
                lpMod->Nc = 0.8;
                break;

            case DARK_SURROUND:
                lpMod->F = 0.8;
                lpMod->c = 0.525;
                lpMod->Nc = 0.8;
                break;

            case DIM_SURROUND:
                lpMod->F = 0.9;
                lpMod->c = 0.59;
                lpMod->Nc = 0.95;
                break;

            default:
                // Average surround
                lpMod->F = 1.0;
                lpMod->c = 0.69;
                lpMod->Nc = 1.0;
                break;
        }

        lpMod->n = compute_n(lpMod);
        lpMod->z = compute_z(lpMod);
        lpMod->Nbb = computeNbb(lpMod);
        lpMod->FL = computeFL(lpMod);

        if (lpMod->D == D_CALCULATE)
        {
            lpMod->D = computeD(lpMod);
        }

        lpMod->Ncb = lpMod->Nbb;

        lpMod->adoptedWhite = XYZtoCAT02(lpMod->adoptedWhite);
        lpMod->adoptedWhite = ChromaticAdaptation(lpMod->adoptedWhite, lpMod);
        lpMod->adoptedWhite = CAT02toHPE(lpMod->adoptedWhite);
        lpMod->adoptedWhite = NonlinearCompression(lpMod->adoptedWhite, lpMod);

        return lpMod;
    }

    public static void cmsCIECAM02Done(HANDLE hModel)
    {
        var lpMod = (CIECAM02*)hModel;

        if (lpMod is not null)
            _cmsFree(lpMod->ContextID, lpMod);
    }

    public static void cmsCIECAM02Forward(HANDLE hModel, in CIEXYZ* pIn, JCh* pOut)
    {
        CAM02COLOR clr;
        CIECAM02* lpMod = (CIECAM02*)hModel;

        _cmsAssert(lpMod);
        _cmsAssert(pIn);
        _cmsAssert(pOut);

        memset(&clr, 0);

        clr.XYZ[0] = pIn->X;
        clr.XYZ[1] = pIn->Y;
        clr.XYZ[2] = pIn->Z;

        clr = XYZtoCAT02(clr);
        clr = ChromaticAdaptation(clr, lpMod);
        clr = CAT02toHPE(clr);
        clr = NonlinearCompression(clr, lpMod);
        clr = ComputeCorrelates(clr, lpMod);

        pOut->J = clr.J;
        pOut->C = clr.C;
        pOut->h = clr.h;
    }

    public static void cmsCIECAM02Reverse(HANDLE hModel, in JCh* pIn, CIEXYZ* pOut)
    {
        CAM02COLOR clr;
        CIECAM02* lpMod = (CIECAM02*)hModel;

        _cmsAssert(lpMod);
        _cmsAssert(pIn);
        _cmsAssert(pOut);

        memset(&clr, 0);

        clr.J = pIn->J;
        clr.C = pIn->C;
        clr.h = pIn->h;

        clr = InverseCorrelates(clr, lpMod);
        clr = InverseNonlinearity(clr, lpMod);
        clr = HPEtoCAT02(clr);
        clr = InverseChromaticAdaptation(clr, lpMod);
        clr = CAT02toXYZ(clr);

        pOut->X = clr.XYZ[0];
        pOut->Y = clr.XYZ[1];
        pOut->Z = clr.XYZ[2];
    }
}
