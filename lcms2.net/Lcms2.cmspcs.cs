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

public static partial class Lcms2
{
    private static readonly ushort[] RGBblack = new ushort[4];
    private static readonly ushort[] RGBwhite = new ushort[4] { 0xFFFF, 0xFFFF, 0xFFFF, 0 };
    private static readonly ushort[] CMYKblack = new ushort[4] { 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF };
    private static readonly ushort[] CMYKwhite = new ushort[4];
    private static readonly ushort[] LABblack = new ushort[4] { 0, 0x8080, 0x8080, 0 };
    private static readonly ushort[] LABwhite = new ushort[4] { 0xFFFF, 0x8080, 0x8080, 0 };
    private static readonly ushort[] CMYblack = new ushort[4] { 0xFFFF, 0xFFFF, 0xFFFF, 0 };
    private static readonly ushort[] CMYwhite = new ushort[4];
    private static readonly ushort[] GrayBlack = new ushort[4];
    private static readonly ushort[] GrayWhite = new ushort[4] { 0xFFFF, 0, 0, 0 };

    public static CIExyY cmsXYZ2xyY(CIEXYZ Source)
    {
        var ISum = 1 / (Source.X + Source.Y + Source.Z);

        return new CIExyY(
            x: Source.X * ISum,
            y: Source.Y * ISum,
            Y: Source.Y);
    }

    public static CIEXYZ cmsxyY2XYZ(CIExyY Source) =>
        new CIEXYZ(
            x: Source.x / Source.y * Source.Y,
            y: Source.Y,
            z: (1 - Source.x - Source.y) / Source.y * Source.Y);

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

    public static void cmsXYZ2Lab(CIEXYZ? WhitePoint, out CIELab Lab, CIEXYZ xyz)
    {
        WhitePoint ??= D50XYZ;

        var fx = f(xyz.X / WhitePoint.Value.X);
        var fy = f(xyz.Y / WhitePoint.Value.Y);
        var fz = f(xyz.Z / WhitePoint.Value.Z);

        Lab.L = (116 * fy) - 16;
        Lab.a = 500 * (fx - fy);
        Lab.b = 200 * (fy - fz);
    }

    public static void cmsLab2XYZ(CIEXYZ? WhitePoint, out CIEXYZ xyz, CIELab Lab)
    {
        WhitePoint ??= D50XYZ;

        var y = (Lab.L + 16.0) / 116.0;
        var x = y + (0.002 * Lab.a);
        var z = y - (0.005 * Lab.b);

        xyz.X = f_1(x) * WhitePoint.Value.X;
        xyz.Y = f_1(y) * WhitePoint.Value.Y;
        xyz.Z = f_1(z) * WhitePoint.Value.Z;
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

    public static CIELab cmsLabEncoded2FloatV2(ReadOnlySpan<ushort> wLab) =>
        new(
            L: L2float2(wLab[0]),
            a: ab2float2(wLab[1]),
            b: ab2float2(wLab[2]));

    public static CIELab cmsLabEncoded2Float(ReadOnlySpan<ushort> wLab) =>
        new(
            L: L2float4(wLab[0]),
            a: ab2float4(wLab[1]),
            b: ab2float4(wLab[2]));

    private static double Clamp_L_doubleV2(double L) =>
        Max(Min(L, 0xffff * 100.0 / 0xff00), 0);

    private static double Clamp_ab_doubleV2(double ab) =>
        Max(Min(ab, MAX_ENCODEABLE_ab2), MIN_ENCODEABLE_ab2);

    public static void cmsFloat2LabEncodedV2(Span<ushort> wLab, CIELab fLab)
    {
        CIELab Lab;

        Lab.L = Clamp_L_doubleV2(fLab.L);
        Lab.a = Clamp_ab_doubleV2(fLab.a);
        Lab.b = Clamp_ab_doubleV2(fLab.b);

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

    public static void cmsFloat2LabEncoded(Span<ushort> wLab, CIELab fLab)
    {
        CIELab Lab;

        Lab.L = Clamp_L_doubleV4(fLab.L);
        Lab.a = Clamp_ab_doubleV4(fLab.a);
        Lab.b = Clamp_ab_doubleV4(fLab.b);

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

    public static CIELCh cmsLab2LCh(CIELab Lab) =>
        new(
            L: Lab.L,
            C: Pow(Sqr(Lab.a) + Sqr(Lab.b), 0.5),
            h: atan2deg(Lab.b, Lab.a));

    public static CIELab cmsLCh2Lab(CIELCh LCh)
    {
        var h = LCh.h * M_PI / 180;

        return new(
            L: LCh.L,
            a: LCh.C * Cos(h),
            b: LCh.C * Sin(h));
    }

    private static ushort XYZ2Fix(double d) =>
        _cmsQuickSaturateWord(d * 32768);

    public static void cmsFloat2XYZEncoded(Span<ushort> XYZ, CIEXYZ xyz)
    {
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

    public static CIEXYZ cmsXYZEncoded2Float(ReadOnlySpan<ushort> XYZ) =>
        new(
            x: XYZ2float(XYZ[0]),
            y: XYZ2float(XYZ[1]),
            z: XYZ2float(XYZ[2]));

    public static double cmsDeltaE(CIELab Lab1, CIELab Lab2)
    {
        var dL = Abs(Lab1.L - Lab2.L);
        var da = Abs(Lab1.a - Lab2.a);
        var db = Abs(Lab1.b - Lab2.b);

        return Pow(Sqr(dL) + Sqr(da) + Sqr(db), 0.5);
    }

    public static double cmsCIE94DeltaE(CIELab Lab1, CIELab Lab2)
    {
        var dL = Abs(Lab1.L - Lab2.L);

        var LCh1 = cmsLab2LCh(Lab1);
        var LCh2 = cmsLab2LCh(Lab2);

        var dC = Abs(LCh1.C - LCh2.C);
        var dE = cmsDeltaE(Lab1, Lab2);

        var dhsq = Sqr(dE) - Sqr(dL) - Sqr(dC);
        var dh = dhsq < 0 ? 0 : Pow(dhsq, 0.5);

        var c12 = Sqrt(LCh1.C * LCh2.C);

        var sc = 1.0 + (0.048 * c12);
        var sh = 1.0 + (0.014 * c12);

        return Sqrt(Sqr(dL) + (Sqr(dC) / Sqr(sc)) + (Sqr(dh) / Sqr(sh)));
    }

    private static double ComputeLBFD(CIELab Lab) =>
        (54.6 * (M_LOG10E * Log((Lab.L > 7.996969 ? Sqr((Lab.L + 16) / 116) * ((Lab.L + 16) / 116) * 100 : 100 * (Lab.L / 903.3)) + 1.5))) - 9.6;

    public static double cmsBFDdeltaE(CIELab Lab1, CIELab Lab2)
    {
        var lbfd1 = ComputeLBFD(Lab1);
        var lbfd2 = ComputeLBFD(Lab2);
        var deltaL = lbfd2 - lbfd1;

        var LCh1 = cmsLab2LCh(Lab1);
        var LCh2 = cmsLab2LCh(Lab2);

        var deltaC = LCh2.C - LCh1.C;
        var AveC = (LCh1.C + LCh2.C) / 2;
        var Aveh = (LCh1.h + LCh2.h) / 2;

        var dE = cmsDeltaE(Lab1, Lab2);

        var deltah =
            Sqr(dE) > (Sqr(Lab2.L - Lab1.L) + Sqr(deltaC))
            ? Sqrt(Sqr(dE) - Sqr(Lab2.L - Lab1.L) - Sqr(deltaC))
            : 0;

        var dc = (0.035 * AveC / (1 + (0.00365 * AveC))) + 0.521;
        var g = Sqrt(Sqr(Sqr(AveC)) / (Sqr(Sqr(AveC)) + 14000));
        var t = 0.627 + ((0.055 * Cos((Aveh - 254) / (180 / M_PI))) -
               (0.040 * Cos(((2 * Aveh) - 136) / (180 / M_PI))) +
               (0.070 * Cos(((3 * Aveh) - 31) / (180 / M_PI))) +
               (0.049 * Cos(((4 * Aveh) + 114) / (180 / M_PI))) -
               (0.015 * Cos(((5 * Aveh) - 103) / (180 / M_PI))));

        var dh = dc * ((g * t) + 1 - g);
        var rh = (-0.260 * Cos((Aveh - 308) / (180 / M_PI))) -
               (0.379 * Cos(((2 * Aveh) - 160) / (180 / M_PI))) -
               (0.636 * Cos(((3 * Aveh) + 254) / (180 / M_PI))) +
               (0.226 * Cos(((4 * Aveh) + 140) / (180 / M_PI))) -
               (0.194 * Cos(((5 * Aveh) + 280) / (180 / M_PI)));

        var rc = Sqrt(AveC * AveC * AveC * AveC * AveC * AveC / ((AveC * AveC * AveC * AveC * AveC * AveC) + 70000000));
        var rt = rh * rc;

        return Sqrt(Sqr(deltaL) + Sqr(deltaC / dc) + Sqr(deltah / dh) + (rt * (deltaC / dc) * (deltah / dh)));
    }

    public static double cmsCMCdeltaE(CIELab Lab1, CIELab Lab2, double l, double c)
    {
        if (Lab1.L == 0 && Lab2.L == 0) return 0;

        var LCh1 = cmsLab2LCh(Lab1);
        var LCh2 = cmsLab2LCh(Lab2);

        var dL = Lab2.L - Lab1.L;
        var dC = LCh2.C - LCh1.C;

        var dE = cmsDeltaE(Lab1, Lab2);

        var dh =
            Sqr(dE) > (Sqr(dL) + Sqr(dC))
            ? Sqrt(Sqr(dE) - Sqr(dL) - Sqr(dC))
            : 0;

        var t = LCh1.h is > 164 and < 345
            ? 0.56 + Abs(0.2 * Cos((LCh1.h + 168) / (180 / M_PI)))
            : 0.36 + Abs(0.4 * Cos((LCh1.h + 35) / (180 / M_PI)));

        var sc = (0.0638 * LCh1.C / (1 + (0.0131 * LCh1.C))) + 0.638;
        var sl = 0.040975 * Lab1.L / (1 + (0.01765 * Lab1.L));

        if (Lab1.L < 16)
            sl = 0.511;

        var f = Sqrt(LCh1.C * LCh1.C * LCh1.C * LCh1.C / ((LCh1.C * LCh1.C * LCh1.C * LCh1.C) + 1900));
        var sh = sc * ((t * f) + 1 - f);
        return Sqrt(Sqr(dL / (l * sl)) + Sqr(dC / (c * sc)) + Sqr(dh / sh));
    }

    public static double cmsCIE2000DeltaE(CIELab Lab1, CIELab Lab2, double Kl, double Kc, double Kh)
    {
        var L1 = Lab1.L;
        var a1 = Lab1.a;
        var b1 = Lab1.b;
        var C = Sqrt(Sqr(a1) + Sqr(b1));

        var Ls = Lab2.L;
        var @as = Lab2.a;
        var bs = Lab2.b;
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

    public static uint _cmsReasonableGridpointsByColorspace(Signature Colorspace, uint dwFlags)
    {
        // Already specified?
        if ((dwFlags & 0x00FF0000) is not 0)
            return (dwFlags >> 16) & 0xFF;

        var nChannles = cmsChannelsOf(Colorspace);

        // HighResPrecalc is maximum resolution
        if ((dwFlags & cmsFLAGS_HIGHRESPRECALC) is not 0)
        {
            return nChannles switch
            {
                > 4 => 6,
                4 => 33,
                _ => 17
            };
        }

        // LowResPrecalc is lower resolution
        if ((dwFlags & cmsFLAGS_LOWRESPRECALC) is not 0)
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

    internal static bool _cmsEndPointsBySpace(Signature Space, out ushort[] White, out ushort[] Black, out uint nOutputs)
    {
        // Only most common spaces
        switch ((uint)Space)
        {
            case cmsSigGrayData:
                White = GrayWhite;
                Black = GrayBlack;
                nOutputs = 1;

                return true;

            case cmsSigRgbData:
                White = RGBwhite;
                Black = RGBblack;
                nOutputs = 3;

                return true;

            case cmsSigLabData:
                White = LABwhite;
                Black = LABblack;
                nOutputs = 3;

                return true;

            case cmsSigCmykData:
                White = CMYKwhite;
                Black = CMYKblack;
                nOutputs = 4;

                return true;

            case cmsSigCmyData:
                White = CMYwhite;
                Black = CMYblack;
                nOutputs = 3;

                return true;
        }

        White = null!;
        Black = null!;
        nOutputs = 0;

        return false;
    }

    internal static Signature _cmsICCcolorSpace(int OutNotation) =>
        OutNotation switch
        {
            1 or
            PT_GRAY => cmsSigGrayData,
            2 or
            PT_RGB => cmsSigRgbData,
            PT_CMY => cmsSigCmyData,
            PT_CMYK => cmsSigCmykData,
            PT_YCbCr => cmsSigYCbCrData,
            PT_YUV => cmsSigLuvData,
            PT_XYZ => cmsSigXYZData,
            PT_Lab or
            PT_LabV2 => cmsSigLabData,
            PT_YUVK => cmsSigLuvKData,
            PT_HSV => cmsSigHsvData,
            PT_HLS => cmsSigHlsData,
            PT_Yxy => cmsSigYxyData,
            PT_MCH1 => cmsSigMCH1Data,
            PT_MCH2 => cmsSigMCH2Data,
            PT_MCH3 => cmsSigMCH3Data,
            PT_MCH4 => cmsSigMCH4Data,
            PT_MCH5 => cmsSigMCH5Data,
            PT_MCH6 => cmsSigMCH6Data,
            PT_MCH7 => cmsSigMCH7Data,
            PT_MCH8 => cmsSigMCH8Data,
            PT_MCH9 => cmsSigMCH9Data,
            PT_MCH10 => cmsSigMCHAData,
            PT_MCH11 => cmsSigMCHBData,
            PT_MCH12 => cmsSigMCHCData,
            PT_MCH13 => cmsSigMCHDData,
            PT_MCH14 => cmsSigMCHEData,
            PT_MCH15 => cmsSigMCHFData,
            _ => default
        };

    internal static int _cmsLCMScolorSpace(Signature ProfileSpace) =>
        (uint)ProfileSpace switch
        {
            cmsSigGrayData => PT_GRAY,
            cmsSigRgbData => PT_RGB,
            cmsSigCmyData => PT_CMY,
            cmsSigCmykData => PT_CMYK,
            cmsSigYCbCrData => PT_YCbCr,
            cmsSigLuvData => PT_YUV,
            cmsSigXYZData => PT_XYZ,
            cmsSigLabData => PT_Lab,
            cmsSigLuvKData => PT_YUVK,
            cmsSigHsvData => PT_HSV,
            cmsSigHlsData => PT_HLS,
            cmsSigYxyData => PT_Yxy,
            cmsSig1colorData or
            cmsSigMCH1Data => PT_MCH1,
            cmsSig2colorData or
            cmsSigMCH2Data => PT_MCH2,
            cmsSig3colorData or
            cmsSigMCH3Data => PT_MCH3,
            cmsSig4colorData or
            cmsSigMCH4Data => PT_MCH4,
            cmsSig5colorData or
            cmsSigMCH5Data => PT_MCH5,
            cmsSig6colorData or
            cmsSigMCH6Data => PT_MCH6,
            cmsSig7colorData or
            cmsSigMCH7Data => PT_MCH7,
            cmsSig8colorData or
            cmsSigMCH8Data => PT_MCH8,
            cmsSig9colorData or
            cmsSigMCH9Data => PT_MCH9,
            cmsSig10colorData or
            cmsSigMCHAData => PT_MCH10,
            cmsSig11colorData or
            cmsSigMCHBData => PT_MCH11,
            cmsSig12colorData or
            cmsSigMCHCData => PT_MCH12,
            cmsSig13colorData or
            cmsSigMCHDData => PT_MCH13,
            cmsSig14colorData or
            cmsSigMCHEData => PT_MCH14,
            cmsSig15colorData or
            cmsSigMCHFData => PT_MCH15,
            _ => 0,
        };

    public static uint cmsChannelsOf(Signature Colorspace) =>
        (uint)Colorspace switch
        {
            cmsSigMCH1Data or
            cmsSig1colorData or
            cmsSigGrayData => 1,
            cmsSigMCH2Data or
            cmsSig2colorData => 2,
            cmsSigXYZData or
            cmsSigLabData or
            cmsSigLuvData or
            cmsSigYCbCrData or
            cmsSigYxyData or
            cmsSigRgbData or
            cmsSigHsvData or
            cmsSigHlsData or
            cmsSigCmyData or
            cmsSigMCH3Data or
            cmsSig3colorData => 3,
            cmsSigLuvKData or
            cmsSigCmykData or
            cmsSigMCH4Data or
            cmsSig4colorData => 4,
            cmsSigMCH5Data or
            cmsSig5colorData => 5,
            cmsSigMCH6Data or
            cmsSig6colorData => 6,
            cmsSigMCH7Data or
            cmsSig7colorData => 7,
            cmsSigMCH8Data or
            cmsSig8colorData => 8,
            cmsSigMCH9Data or
            cmsSig9colorData => 9,
            cmsSigMCHAData or
            cmsSig10colorData => 10,
            cmsSigMCHBData or
            cmsSig11colorData => 11,
            cmsSigMCHCData or
            cmsSig12colorData => 12,
            cmsSigMCHDData or
            cmsSig13colorData => 13,
            cmsSigMCHEData or
            cmsSig14colorData => 14,
            cmsSigMCHFData or
            cmsSig15colorData => 15,
            _ => 3,
        };
}
