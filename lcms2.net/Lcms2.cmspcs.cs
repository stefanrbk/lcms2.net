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

using lcms2.types;

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

    public static CIExyY cmsXYZ2xyY(CIEXYZ Source) =>
        // See CIEXYZ.As_xyY
        Source.As_xyY;

    public static CIEXYZ cmsxyY2XYZ(CIExyY Source) =>
        // See CIExyY.AsXYZ
        Source.AsXYZ;

    public static void cmsXYZ2Lab(CIEXYZ? WhitePoint, out CIELab Lab, CIEXYZ xyz) =>
        // See CIEXYZ.AsLab()
        Lab = xyz.AsLab(WhitePoint);

    public static void cmsLab2XYZ(CIEXYZ? WhitePoint, out CIEXYZ xyz, CIELab Lab) =>
        // See CIELab.AsXYZ()
        xyz = Lab.AsXYZ(WhitePoint);

    public static CIELab cmsLabEncoded2FloatV2(ReadOnlySpan<ushort> wLab) =>
        // See CIELab.FromLabEncodedV2
        CIELab.FromLabEncodedV2(wLab);

    public static CIELab cmsLabEncoded2Float(ReadOnlySpan<ushort> wLab) =>
        // See CIELab.FromLabEncoded
        CIELab.FromLabEncoded(wLab);

    public static void cmsFloat2LabEncodedV2(Span<ushort> wLab, CIELab fLab) =>
        // See CIELab.ToLabEncodedV2
        fLab.ToLabEncodedV2(wLab);

    public static void cmsFloat2LabEncoded(Span<ushort> wLab, CIELab fLab) =>
        // See CIELab.ToLabEncoded
        fLab.ToLabEncoded(wLab);

    public static CIELCh cmsLab2LCh(CIELab Lab) =>
        // See CIELab.AsLCh
        Lab.AsLCh;

    public static CIELab cmsLCh2Lab(CIELCh LCh) =>
        // See CIELCh.AsLab
        LCh.AsLab;

    public static void cmsFloat2XYZEncoded(Span<ushort> XYZ, CIEXYZ xyz) =>
        // See CIEXYZ.ToXYZEncoded()
        xyz.ToXYZEncoded(XYZ);

    public static CIEXYZ cmsXYZEncoded2Float(ReadOnlySpan<ushort> XYZ) =>
        // See CIEXYZ.FromXYZEncoded()
        CIEXYZ.FromXYZEncoded(XYZ);

    public static double cmsDeltaE(CIELab Lab1, CIELab Lab2) =>
        // See DeltaE.De76()
        DeltaE.De76(Lab1, Lab2);

    public static double cmsCIE94DeltaE(CIELab Lab1, CIELab Lab2) =>
        // See DeltaE.CIE94()
        DeltaE.CIE94(Lab1, Lab2);

    public static double cmsBFDdeltaE(CIELab Lab1, CIELab Lab2) =>
        // See DeltaE.BFD()
        DeltaE.BFD(Lab1, Lab2);

    public static double cmsCMCdeltaE(CIELab Lab1, CIELab Lab2, double l, double c) =>
        // See DeltaE.CMC()
        DeltaE.CMC(Lab1, Lab2, l, c);

    public static double cmsCIE2000DeltaE(CIELab Lab1, CIELab Lab2, double Kl, double Kc, double Kh) =>
        // See DeltaE.CIE2000
        DeltaE.CIE2000(Lab1, Lab2, Kl, Kc, Kh);

    internal static uint _cmsReasonableGridpointsByColorspace(Signature Colorspace, uint dwFlags)
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

    [Obsolete("Deprecated, use cmsChannelsOfColorSpace instead")]
    public static uint cmsChannelsOf(Signature Colorspace)
    {
        var n = cmsChannelsOfColorSpace(Colorspace);
        if (n < 0)
            return 3;
        return (uint)n;
    }

    public static int cmsChannelsOfColorSpace(Signature Colorspace) =>
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
            _ => -1,
        };
}
