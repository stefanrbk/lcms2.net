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

using static System.Math;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static readonly ushort* RGBblack;
    private static readonly ushort* RGBwhite;
    private static readonly ushort* CMYKblack;
    private static readonly ushort* CMYKwhite;
    private static readonly ushort* LABblack;
    private static readonly ushort* LABwhite;
    private static readonly ushort* CMYblack;
    private static readonly ushort* CMYwhite;
    private static readonly ushort* GrayBlack;
    private static readonly ushort* GrayWhite;

    public static void cmsXYZ2xyY(CIExyY* Dest, in CIEXYZ* Source)
    {
        var ISum = 1 / (Source->X + Source->Y + Source->Z);

        Dest->x = Source->X * ISum;
        Dest->y = Source->Y * ISum;
        Dest->Y = Source->Y;
    }

    public static void cmsxyY2XYZ(CIEXYZ* Dest, in CIExyY* Source)
    {
        Dest->X = Source->x / Source->y * Source->Y;
        Dest->Y = Source->Y;
        Dest->Z = (1 - Source->x - Source->y) / Source->y * Source->Y;
    }

    private static double f(double t)
    {
        const double Limit = 24.0 / 116 * (24.0 / 116) * (24.0 / 116);

        return (t <= Limit)
            ? (841.0 / 108 * t) + (16.0 / 116)
            : Pow(t, 1.0 / 3);
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

    private static double L2float2(ushort v) =>
        v / 652.8;

    private static double ab2float2(ushort v) =>
        (v / 256.0) - 128;

    private static ushort L2Fix2(double L) =>
        _cmsQuickSaturateWord(L * 652.8);

    private static ushort ab2Fix2(double ab) =>
        _cmsQuickSaturateWord((ab + 128) * 256);

    private static double L2float4(ushort v) =>
        v / 655.35;

    private static double ab2float4(ushort v) =>
        (v / 257.0) - 128;

    public static void cmsLabEncoded2FloatV2(CIELab* Lab, in ushort* wLab)
    {
        Lab->L = L2float2(wLab[0]);
        Lab->a = ab2float2(wLab[1]);
        Lab->b = ab2float2(wLab[2]);
    }

    public static void cmsLabEncoded2Float(CIELab* Lab, in ushort* wLab)
    {
        Lab->L = L2float4(wLab[0]);
        Lab->a = ab2float4(wLab[1]);
        Lab->b = ab2float4(wLab[2]);
    }

    private static double Clamp_L_doubleV2(double L) =>
        Max(Min(L, 0xffff * 100.0 / 0xff00), 0);

    private static double Clamp_ab_doubleV2(double ab) =>
        Max(Min(ab, MAX_ENCODEABLE_ab2), MIN_ENCODEABLE_ab2);

    public static void cmsFloat2LabEncodedV2(ushort* wLab, in CIELab* fLab)
    {
        CIELab Lab;

        Lab.L = Clamp_L_doubleV2(fLab->L);
        Lab.a = Clamp_ab_doubleV2(fLab->a);
        Lab.b = Clamp_ab_doubleV2(fLab->b);

        wLab[0] = L2Fix2(Lab.L);
        wLab[1] = ab2Fix2(Lab.a);
        wLab[2] = ab2Fix2(Lab.b);
    }

    private static double Clamp_L_doubleV4(double L) =>
        Max(Min(L, 100), 0);

    private static double Clamp_ab_doubleV4(double ab) =>
        Max(Min(ab, MAX_ENCODEABLE_ab4), MIN_ENCODEABLE_ab4);

    private static ushort L2Fix4(double L) =>
        _cmsQuickSaturateWord(L * 655.35);

    private static ushort ab2Fix4(double ab) =>
        _cmsQuickSaturateWord((ab + 128) * 257);

    public static void cmsFloat2LabEncoded(ushort* wLab, in CIELab* fLab)
    {
        CIELab Lab;

        Lab.L = Clamp_L_doubleV4(fLab->L);
        Lab.a = Clamp_ab_doubleV4(fLab->a);
        Lab.b = Clamp_ab_doubleV4(fLab->b);

        wLab[0] = L2Fix4(Lab.L);
        wLab[1] = ab2Fix4(Lab.a);
        wLab[2] = ab2Fix4(Lab.b);
    }

    private static double RADIANS(double deg) =>
        deg * M_PI / 180;

    private static double atan2deg(double a, double b)
    {
        var h = a is 0 && b is 0
            ? 0
            : Atan2(a, b);

        h *= 180 / M_PI;

        while (h > 360)
            h -= 360;
        while (h < 0)
            h += 360;

        return h;
    }

    private static double Sqr(double v) =>
        v * v;

    public static void cmsLab2LCh(CIELCh* LCh, in CIELab* Lab)
    {
        LCh->L = Lab->L;
        LCh->C = Pow(Sqr(Lab->a) + Sqr(Lab->b), 0.5);
        LCh->h = atan2deg(Lab->b, Lab->a);
    }

    public static void cmsLCh2Lab(CIELab* Lab, in CIELCh* LCh)
    {
        var h = LCh->h * M_PI / 180;

        Lab->L = LCh->L;
        Lab->a = LCh->C * Cos(h);
        Lab->b = LCh->C * Sin(h);
    }

    private static ushort XYZ2Fix(double d) =>
        _cmsQuickSaturateWord(d * 32768);

    public static void cmsFloat2XYZEncoded(ushort* XYZ, in CIEXYZ* fXYZ)
    {
        CIEXYZ xyz;

        xyz.X = fXYZ->X;
        xyz.Y = fXYZ->Y;
        xyz.Z = fXYZ->Z;

        // Clamp to encodeable values.
        if (xyz.Y <= 0)
        {
            xyz.X = 0;
            xyz.Y = 0;
            xyz.Z = 0;
        }

        if (xyz.X > MAX_ENCODEABLE_XYZ)
            xyz.X = MAX_ENCODEABLE_XYZ;

        if (xyz.X < 0)
            xyz.X = 0;

        if (xyz.Y > MAX_ENCODEABLE_XYZ)
            xyz.Y = MAX_ENCODEABLE_XYZ;

        if (xyz.Y < 0)
            xyz.Y = 0;

        if (xyz.Z > MAX_ENCODEABLE_XYZ)
            xyz.Z = MAX_ENCODEABLE_XYZ;

        if (xyz.Z < 0)
            xyz.Z = 0;

        XYZ[0] = XYZ2Fix(xyz.X);
        XYZ[1] = XYZ2Fix(xyz.Y);
        XYZ[2] = XYZ2Fix(xyz.Z);
    }

    private static double XYZ2float(ushort v) =>
        _cms15Fixed16toDouble(v << 1);

    public static void cmsXYZEncoded2Float(CIEXYZ* fXYZ, in ushort* XYZ)
    {
        fXYZ->X = XYZ2float(XYZ[0]);
        fXYZ->Y = XYZ2float(XYZ[1]);
        fXYZ->Z = XYZ2float(XYZ[2]);
    }

    public static double cmsDeltaE(in CIELab* Lab1, in CIELab* Lab2)
    {
        var dL = Abs(Lab1->L - Lab2->L);
        var da = Abs(Lab1->a - Lab2->a);
        var db = Abs(Lab1->b - Lab2->b);

        return Pow(Sqr(dL) + Sqr(da) + Sqr(db), 0.5);
    }

    public static double cmsCIE94DeltaE(in CIELab* Lab1, in CIELab* Lab2)
    {
        CIELCh LCh1, LCh2;
        double dE, dL, dC, dh, dhsq;
        double c12, sc, sh;

        dL = Abs(Lab1->L - Lab2->L);

        cmsLab2LCh(&LCh1, Lab1);
        cmsLab2LCh(&LCh2, Lab2);

        dC = Abs(LCh1.C - LCh2.C);
        dE = cmsDeltaE(Lab1, Lab2);

        dhsq = Sqr(dE) - Sqr(dL) - Sqr(dC);
        if (dhsq < 0)
            dh = 0;
        else
            dh = Pow(dhsq, 0.5);

        c12 = Sqrt(LCh1.C * LCh2.C);

        sc = 1.0 + (0.048 * c12);
        sh = 1.0 + (0.014 * c12);

        return Sqrt(Sqr(dL) + (Sqr(dC) / Sqr(sc)) + (Sqr(dh) / Sqr(sh)));
    }

    private static double ComputeLBFD(in CIELab* Lab) =>
        (54.6 * (M_LOG10E * Log((Lab->L > 7.996969 ? Sqr((Lab->L + 16) / 116) * ((Lab->L + 16) / 116) * 100 : 100 * (Lab->L / 903.3)) + 1.5))) - 9.6;

    public static double cmsBFDdeltaE(in CIELab* Lab1, in CIELab* Lab2)
    {
        double lbfd1, lbfd2, AveC, Aveh, dE, deltaL,
            deltaC, deltah, dc, t, g, dh, rh, rc, rt;
        CIELCh LCh1, LCh2;

        lbfd1 = ComputeLBFD(Lab1);
        lbfd2 = ComputeLBFD(Lab2);
        deltaL = lbfd2 - lbfd1;

        cmsLab2LCh(&LCh1, Lab1);
        cmsLab2LCh(&LCh2, Lab2);

        deltaC = LCh2.C - LCh1.C;
        AveC = (LCh1.C + LCh2.C) / 2;
        Aveh = (LCh1.h + LCh2.h) / 2;

        dE = cmsDeltaE(Lab1, Lab2);

        deltah =
            Sqr(dE) > (Sqr(Lab2->L - Lab1->L) + Sqr(deltaC))
            ? Sqrt(Sqr(dE) - Sqr(Lab2->L - Lab1->L) - Sqr(deltaC))
            : 0;

        dc = (0.035 * AveC / (1 + (0.00365 * AveC))) + 0.521;
        g = Sqrt(Sqr(Sqr(AveC)) / (Sqr(Sqr(AveC)) + 14000));
        t = 0.627 + ((0.055 * Cos((Aveh - 254) / (180 / M_PI))) -
               (0.040 * Cos(((2 * Aveh) - 136) / (180 / M_PI))) +
               (0.070 * Cos(((3 * Aveh) - 31) / (180 / M_PI))) +
               (0.049 * Cos(((4 * Aveh) + 114) / (180 / M_PI))) -
               (0.015 * Cos(((5 * Aveh) - 103) / (180 / M_PI))));

        dh = dc * ((g * t) + 1 - g);
        rh = (-0.260 * Cos((Aveh - 308) / (180 / M_PI))) -
               (0.379 * Cos(((2 * Aveh) - 160) / (180 / M_PI))) -
               (0.636 * Cos(((3 * Aveh) + 254) / (180 / M_PI))) +
               (0.226 * Cos(((4 * Aveh) + 140) / (180 / M_PI))) -
               (0.194 * Cos(((5 * Aveh) + 280) / (180 / M_PI)));

        rc = Sqrt(AveC * AveC * AveC * AveC * AveC * AveC / ((AveC * AveC * AveC * AveC * AveC * AveC) + 70000000));
        rt = rh * rc;

        return Sqrt(Sqr(deltaL) + Sqr(deltaC / dc) + Sqr(deltah / dh) + (rt * (deltaC / dc) * (deltah / dh)));
    }

    public static double cmsCMCdeltaE(in CIELab* Lab1, in CIELab* Lab2, double l, double c)
    {
        double dE, dL, dC, dh, sl, sc, sh, t, f;
        CIELCh LCh1, LCh2;

        if (Lab1->L == 0 && Lab2->L == 0) return 0;

        cmsLab2LCh(&LCh1, Lab1);
        cmsLab2LCh(&LCh2, Lab2);

        dL = Lab2->L - Lab1->L;
        dC = LCh2.C - LCh1.C;

        dE = cmsDeltaE(Lab1, Lab2);

        dh =
            Sqr(dE) > (Sqr(dL) + Sqr(dC))
            ? Sqrt(Sqr(dE) - Sqr(dL) - Sqr(dC))
            : 0;

        t = LCh1.h is > 164 and < 345
            ? 0.56 + Abs(0.2 * Cos((LCh1.h + 168) / (180 / M_PI)))
            : 0.36 + Abs(0.4 * Cos((LCh1.h + 35) / (180 / M_PI)));

        sc = (0.0638 * LCh1.C / (1 + (0.0131 * LCh1.C))) + 0.638;
        sl = 0.040975 * Lab1->L / (1 + (0.01765 * Lab1->L));

        if (Lab1->L < 16)
            sl = 0.511;

        f = Sqrt(LCh1.C * LCh1.C * LCh1.C * LCh1.C / ((LCh1.C * LCh1.C * LCh1.C * LCh1.C) + 1900));
        sh = sc * ((t * f) + 1 - f);
        return Sqrt(Sqr(dL / (l * sl)) + Sqr(dC / (c * sc)) + Sqr(dh / sh));
    }

    public static double cmsCIE2000DeltaE(in CIELab* Lab1, in CIELab* Lab2, double Kl, double Kc, double Kh)
    {
        var L1 = Lab1->L;
        var a1 = Lab1->a;
        var b1 = Lab1->b;
        var C = Sqrt(Sqr(a1) + Sqr(b1));

        var Ls = Lab2->L;
        var @as = Lab2->a;
        var bs = Lab2->b;
        var Cs = Sqrt(Sqr(@as) + Sqr(bs));

        var G = 0.5 * (1 - Sqrt(Pow((C + Cs) / 2, 7.0) / (Pow((C + Cs) / 2, 7.0) + Pow(25.0, 7.0))));

        var a_p = (1 + G) * a1;
        var b_p = b1;
        var C_p = Sqrt(Sqr(a_p) + Sqr(b_p));
        var h_p = atan2deg(b_p, a_p);

        var a_ps = (1 + G) * @as;
        var b_ps = bs;
        var C_ps = Sqrt(Sqr(a_ps) + Sqr(b_ps));
        var h_ps = atan2deg(b_ps, a_ps);

        var meanC_p = (C_p + C_ps) / 2;

        var hps_plus_hp = h_ps + h_p;
        var hps_minus_hp = h_ps - h_p;

        var meanh_p = Abs(hps_minus_hp) <= 180.000001 ? hps_plus_hp / 2 :
                                hps_plus_hp < 360 ? (hps_plus_hp + 360) / 2 :
                                                     (hps_plus_hp - 360) / 2;

        var delta_h = hps_minus_hp <= -180.000001 ? (hps_minus_hp + 360) :
                                hps_minus_hp > 180 ? (hps_minus_hp - 360) :
                                                        hps_minus_hp;
        var delta_L = Ls - L1;
        var delta_C = C_ps - C_p;

        var delta_H = 2 * Sqrt(C_ps * C_p) * Sin(RADIANS(delta_h) / 2);

        var T = 1 - (0.17 * Cos(RADIANS(meanh_p - 30)))
                     + (0.24 * Cos(RADIANS(2 * meanh_p)))
                     + (0.32 * Cos(RADIANS((3 * meanh_p) + 6)))
                     - (0.2 * Cos(RADIANS((4 * meanh_p) - 63)));

        var Sl = 1 + (0.015 * Sqr(((Ls + L1) / 2) - 50) / Sqrt(20 + Sqr(((Ls + L1) / 2) - 50)));

        var Sc = 1 + (0.045 * (C_p + C_ps) / 2);
        var Sh = 1 + (0.015 * ((C_ps + C_p) / 2) * T);

        var delta_ro = 30 * Exp(-Sqr((meanh_p - 275) / 25));

        var Rc = 2 * Sqrt(Pow(meanC_p, 7.0) / (Pow(meanC_p, 7.0) + Pow(25.0, 7.0)));

        var Rt = -Sin(2 * RADIANS(delta_ro)) * Rc;

        return (double)Sqrt(Sqr(delta_L / (Sl * Kl)) +
                            Sqr(delta_C / (Sc * Kc)) +
                            Sqr(delta_H / (Sh * Kh)) +
                            (Rt * (delta_C / (Sc * Kc)) * (delta_H / (Sh * Kh))));
    }

    internal static uint _cmsReasonableGridpointsByColorspace(Signature Colorspace, uint dwFlags)
    {
        // Already specified?
        if ((dwFlags & 0x00FF0000) is not 0)
            return (dwFlags >> 16) & 0xFF;

        var nChannles = cmsChannelsOf(Colorspace);

        // HighResPrecalc is maximum resolution
        if ((dwFlags & FLAGS_HIGHRESPRECALC) is not 0)
        {
            return nChannles switch
            {
                > 4 => 6,
                4 => 33,
                _ => 17
            };
        }

        // LowResPrecalc is lower resolution
        if ((dwFlags & FLAGS_LOWRESPRECALC) is not 0)
        {
            return nChannles switch
            {
                > 4 => 7,
                4 => 23,
                _ => 49
            };
        }

        // Default values
        return nChannles switch
        {
            > 4 => 7,
            4 => 17,
            _ => 33,
        };
    }

    internal static bool _cmsEndPointsBySpace(Signature Space, ushort** White, ushort** Black, uint* nOutputs)
    {
        // Only most common spaces
        if (Space == Signature.Colorspace.Gray)
        {
            if (White is not null) *White = GrayWhite;
            if (Black is not null) *Black = GrayBlack;
            if (nOutputs is not null) *nOutputs = 1;

            return true;
        }
        else if (Space == Signature.Colorspace.Rgb)
        {
            if (White is not null) *White = RGBwhite;
            if (Black is not null) *Black = RGBblack;
            if (nOutputs is not null) *nOutputs = 3;

            return true;
        }
        else if (Space == Signature.Colorspace.Lab)
        {
            if (White is not null) *White = LABwhite;
            if (Black is not null) *Black = LABblack;
            if (nOutputs is not null) *nOutputs = 3;

            return true;
        }
        else if (Space == Signature.Colorspace.Cmyk)
        {
            if (White is not null) *White = CMYKwhite;
            if (Black is not null) *Black = CMYKblack;
            if (nOutputs is not null) *nOutputs = 4;

            return true;
        }
        else if (Space == Signature.Colorspace.Cmy)
        {
            if (White is not null) *White = CMYwhite;
            if (Black is not null) *Black = CMYblack;
            if (nOutputs is not null) *nOutputs = 3;

            return true;
        }

        return false;
    }

    internal static Signature _cmsICCcolorSpace(int OutNotation) =>
        OutNotation switch
        {
            1 or PT_GRAY => Signature.Colorspace.Gray,
            2 or PT_RGB => Signature.Colorspace.Rgb,
            PT_CMY => Signature.Colorspace.Cmy,
            PT_CMYK => Signature.Colorspace.Cmyk,
            PT_YCbCr => Signature.Colorspace.YCbCr,
            PT_YUV => Signature.Colorspace.Luv,
            PT_XYZ => Signature.Colorspace.XYZ,
            PT_Lab or PT_LabV2 => Signature.Colorspace.Lab,
            PT_YUVK => Signature.Colorspace.LuvK,
            PT_HSV => Signature.Colorspace.Hsv,
            PT_HLS => Signature.Colorspace.Hls,
            PT_Yxy => Signature.Colorspace.Yxy,
            PT_MCH1 => Signature.Colorspace.MCH1,
            PT_MCH2 => Signature.Colorspace.MCH2,
            PT_MCH3 => Signature.Colorspace.MCH3,
            PT_MCH4 => Signature.Colorspace.MCH4,
            PT_MCH5 => Signature.Colorspace.MCH5,
            PT_MCH6 => Signature.Colorspace.MCH6,
            PT_MCH7 => Signature.Colorspace.MCH7,
            PT_MCH8 => Signature.Colorspace.MCH8,
            PT_MCH9 => Signature.Colorspace.MCH9,
            PT_MCH10 => Signature.Colorspace.MCHA,
            PT_MCH11 => Signature.Colorspace.MCHB,
            PT_MCH12 => Signature.Colorspace.MCHC,
            PT_MCH13 => Signature.Colorspace.MCHD,
            PT_MCH14 => Signature.Colorspace.MCHE,
            PT_MCH15 => Signature.Colorspace.MCHF,
            _ => default
        };

    internal static int _cmsLCMScolorSpace(Signature ProfileSpace)
    {
        if (ProfileSpace == Signature.Colorspace.Gray)
        {
            return PT_GRAY;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Rgb)
        {
            return PT_RGB;
        }
        else if (ProfileSpace == Signature.Colorspace.Color1 ||
                         ProfileSpace == Signature.Colorspace.MCH1)
        {
            return PT_MCH1;
        }
        else if (ProfileSpace == Signature.Colorspace.Color2 ||
                         ProfileSpace == Signature.Colorspace.MCH2)
        {
            return PT_MCH2;
        }
        else if (ProfileSpace == Signature.Colorspace.Color3 ||
                         ProfileSpace == Signature.Colorspace.MCH3)
        {
            return PT_MCH3;
        }
        else if (ProfileSpace == Signature.Colorspace.Color4 ||
                         ProfileSpace == Signature.Colorspace.MCH4)
        {
            return PT_MCH4;
        }
        else if (ProfileSpace == Signature.Colorspace.Color5 ||
                         ProfileSpace == Signature.Colorspace.MCH5)
        {
            return PT_MCH5;
        }
        else if (ProfileSpace == Signature.Colorspace.Color6 ||
                         ProfileSpace == Signature.Colorspace.MCH6)
        {
            return PT_MCH6;
        }
        else if (ProfileSpace == Signature.Colorspace.Color7 ||
                         ProfileSpace == Signature.Colorspace.MCH7)
        {
            return PT_MCH7;
        }
        else if (ProfileSpace == Signature.Colorspace.Color8 ||
                         ProfileSpace == Signature.Colorspace.MCH8)
        {
            return PT_MCH8;
        }
        else if (ProfileSpace == Signature.Colorspace.Color9 ||
                         ProfileSpace == Signature.Colorspace.MCH9)
        {
            return PT_MCH9;
        }
        else if (ProfileSpace == Signature.Colorspace.Color10 ||
                         ProfileSpace == Signature.Colorspace.MCHA)
        {
            return PT_MCH10;
        }
        else if (ProfileSpace == Signature.Colorspace.Color11 ||
                         ProfileSpace == Signature.Colorspace.MCHB)
        {
            return PT_MCH11;
        }
        else if (ProfileSpace == Signature.Colorspace.Color12 ||
                         ProfileSpace == Signature.Colorspace.MCHC)
        {
            return PT_MCH12;
        }
        else if (ProfileSpace == Signature.Colorspace.Color13 ||
                         ProfileSpace == Signature.Colorspace.MCHD)
        {
            return PT_MCH13;
        }
        else if (ProfileSpace == Signature.Colorspace.Color14 ||
                         ProfileSpace == Signature.Colorspace.MCHE)
        {
            return PT_MCH14;
        }
        else if (ProfileSpace == Signature.Colorspace.Color15 ||
                         ProfileSpace == Signature.Colorspace.MCHF)
        {
            return PT_MCH15;
        }
        else
        {
            return default;
        }
    }

    public static uint cmsChannelsOf(Signature Colorspace)
    {
        if (Colorspace == Signature.Colorspace.MCH1 ||
            Colorspace == Signature.Colorspace.Color1 ||
            Colorspace == Signature.Colorspace.Gray)
        {
            return 1;
        }
        else if (Colorspace == Signature.Colorspace.MCH2 ||
                 Colorspace == Signature.Colorspace.Color2)
        {
            return 2;
        }
        else if (Colorspace == Signature.Colorspace.XYZ ||
                 Colorspace == Signature.Colorspace.Lab ||
                 Colorspace == Signature.Colorspace.Luv ||
                 Colorspace == Signature.Colorspace.YCbCr ||
                 Colorspace == Signature.Colorspace.Yxy ||
                 Colorspace == Signature.Colorspace.Rgb ||
                 Colorspace == Signature.Colorspace.Hsv ||
                 Colorspace == Signature.Colorspace.Hls ||
                 Colorspace == Signature.Colorspace.Cmy ||
                 Colorspace == Signature.Colorspace.MCH3 ||
                 Colorspace == Signature.Colorspace.Color3)
        {
            return 3;
        }
        else if (Colorspace == Signature.Colorspace.LuvK ||
                 Colorspace == Signature.Colorspace.Cmyk ||
                 Colorspace == Signature.Colorspace.MCH4 ||
                 Colorspace == Signature.Colorspace.Color4)
        {
            return 4;
        }
        else if (Colorspace == Signature.Colorspace.MCH5 ||
                 Colorspace == Signature.Colorspace.Color5)
        {
            return 5;
        }
        else if (Colorspace == Signature.Colorspace.MCH6 ||
                 Colorspace == Signature.Colorspace.Color6)
        {
            return 6;
        }
        else if (Colorspace == Signature.Colorspace.MCH7 ||
                 Colorspace == Signature.Colorspace.Color7)
        {
            return 7;
        }
        else if (Colorspace == Signature.Colorspace.MCH8 ||
                 Colorspace == Signature.Colorspace.Color8)
        {
            return 8;
        }
        else if (Colorspace == Signature.Colorspace.MCH9 ||
                 Colorspace == Signature.Colorspace.Color9)
        {
            return 9;
        }
        else if (Colorspace == Signature.Colorspace.MCHA ||
                 Colorspace == Signature.Colorspace.Color10)
        {
            return 10;
        }
        else if (Colorspace == Signature.Colorspace.MCHB ||
                 Colorspace == Signature.Colorspace.Color11)
        {
            return 11;
        }
        else if (Colorspace == Signature.Colorspace.MCHC ||
                 Colorspace == Signature.Colorspace.Color12)
        {
            return 12;
        }
        else if (Colorspace == Signature.Colorspace.MCHD ||
                 Colorspace == Signature.Colorspace.Color13)
        {
            return 13;
        }
        else if (Colorspace == Signature.Colorspace.MCHE ||
                 Colorspace == Signature.Colorspace.Color14)
        {
            return 14;
        }
        else if (Colorspace == Signature.Colorspace.MCHF ||
                 Colorspace == Signature.Colorspace.Color15)
        {
            return 15;
        }
        else
        {
            return 3;
        }
    }
}
