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

using lcms2.io;
using lcms2.state;
using lcms2.types;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace lcms2;

public static unsafe partial class Lcms2
{
    internal static readonly List<(FILE file, int count)> OpenFiles = new();
    internal static readonly List<(nuint, nuint)> AllocList = new();
    internal static bool PrintAllocs;

    #region lcms2.h

    internal const ushort LCMS_VERSION = 2131;

    internal const ushort cmsMAX_PATH = 256;

    internal const double cmsD50X = 0.9642;
    internal const double cmsD50Y = 1.0;
    internal const double cmsD50Z = 0.8249;

    internal const double cmsPERCEPTUAL_BLACK_X = 0.00336;
    internal const double cmsPERCEPTUAL_BLACK_Y = 0.0034731;
    internal const double cmsPERCEPTUAL_BLACK_Z = 0.00287;

    internal const uint cmsMagicNumber = 0x61637370;
    internal const uint lcmsSignature = 0x6c636d73;

    internal const uint cmsSigChromaticityType = 0x6368726D;
    internal const uint cmsSigColorantOrderType = 0x636C726F;
    internal const uint cmsSigColorantTableType = 0x636C7274;
    internal const uint cmsSigCrdInfoType = 0x63726469;
    internal const uint cmsSigCurveType = 0x63757276;
    internal const uint cmsSigDataType = 0x64617461;
    internal const uint cmsSigDictType = 0x64696374;
    internal const uint cmsSigDateTimeType = 0x6474696D;
    internal const uint cmsSigDeviceSettingsType = 0x64657673;
    internal const uint cmsSigLut16Type = 0x6d667432;
    internal const uint cmsSigLut8Type = 0x6d667431;
    internal const uint cmsSigLutAtoBType = 0x6d414220;
    internal const uint cmsSigLutBtoAType = 0x6d424120;
    internal const uint cmsSigMeasurementType = 0x6D656173;
    internal const uint cmsSigMultiLocalizedUnicodeType = 0x6D6C7563;
    internal const uint cmsSigMultiProcessElementType = 0x6D706574;

    [Obsolete]
    internal const uint cmsSigNamedColorType = 0x6E636f6C;

    internal const uint cmsSigNamedColor2Type = 0x6E636C32;
    internal const uint cmsSigParametricCurveType = 0x70617261;
    internal const uint cmsSigProfileSequenceDescType = 0x70736571;
    internal const uint cmsSigProfileSequenceIdType = 0x70736964;
    internal const uint cmsSigResponseCurveSet16Type = 0x72637332;
    internal const uint cmsSigS15Fixed16ArrayType = 0x73663332;
    internal const uint cmsSigScreeningType = 0x7363726E;
    internal const uint cmsSigSignatureType = 0x73696720;
    internal const uint cmsSigTextType = 0x74657874;
    internal const uint cmsSigTextDescriptionType = 0x64657363;
    internal const uint cmsSigU16Fixed16ArrayType = 0x75663332;
    internal const uint cmsSigUcrBgType = 0x62666420;
    internal const uint cmsSigUInt16ArrayType = 0x75693136;
    internal const uint cmsSigUInt32ArrayType = 0x75693332;
    internal const uint cmsSigUInt64ArrayType = 0x75693634;
    internal const uint cmsSigUInt8ArrayType = 0x75693038;
    internal const uint cmsSigVcgtType = 0x76636774;
    internal const uint cmsSigViewingConditionsType = 0x76696577;
    internal const uint cmsSigXYZType = 0x58595A20;

    internal const uint cmsSigAToB0Tag = 0x41324230;
    internal const uint cmsSigAToB1Tag = 0x41324231;
    internal const uint cmsSigAToB2Tag = 0x41324232;
    internal const uint cmsSigBlueColorantTag = 0x6258595A;
    internal const uint cmsSigBlueMatrixColumnTag = 0x6258595A;
    internal const uint cmsSigBlueTRCTag = 0x62545243;
    internal const uint cmsSigBToA0Tag = 0x42324130;
    internal const uint cmsSigBToA1Tag = 0x42324131;
    internal const uint cmsSigBToA2Tag = 0x42324132;
    internal const uint cmsSigCalibrationDateTimeTag = 0x63616C74;
    internal const uint cmsSigCharTargetTag = 0x74617267;
    internal const uint cmsSigChromaticAdaptationTag = 0x63686164;
    internal const uint cmsSigChromaticityTag = 0x6368726D;
    internal const uint cmsSigColorantOrderTag = 0x636C726F;
    internal const uint cmsSigColorantTableTag = 0x636C7274;
    internal const uint cmsSigColorantTableOutTag = 0x636C6F74;
    internal const uint cmsSigColorimetricIntentImageStateTag = 0x63696973;
    internal const uint cmsSigCopyrightTag = 0x63707274;
    internal const uint cmsSigCrdInfoTag = 0x63726469;
    internal const uint cmsSigDataTag = 0x64617461;
    internal const uint cmsSigDateTimeTag = 0x6474696D;
    internal const uint cmsSigDeviceMfgDescTag = 0x646D6E64;
    internal const uint cmsSigDeviceModelDescTag = 0x646D6464;
    internal const uint cmsSigDeviceSettingsTag = 0x64657673;
    internal const uint cmsSigDToB0Tag = 0x44324230;
    internal const uint cmsSigDToB1Tag = 0x44324231;
    internal const uint cmsSigDToB2Tag = 0x44324232;
    internal const uint cmsSigDToB3Tag = 0x44324233;
    internal const uint cmsSigBToD0Tag = 0x42324430;
    internal const uint cmsSigBToD1Tag = 0x42324431;
    internal const uint cmsSigBToD2Tag = 0x42324432;
    internal const uint cmsSigBToD3Tag = 0x42324433;
    internal const uint cmsSigGamutTag = 0x67616D74;
    internal const uint cmsSigGrayTRCTag = 0x6b545243;
    internal const uint cmsSigGreenColorantTag = 0x6758595A;
    internal const uint cmsSigGreenMatrixColumnTag = 0x6758595A;
    internal const uint cmsSigGreenTRCTag = 0x67545243;
    internal const uint cmsSigLuminanceTag = 0x6C756d69;
    internal const uint cmsSigMeasurementTag = 0x6D656173;
    internal const uint cmsSigMediaBlackPointTag = 0x626B7074;
    internal const uint cmsSigMediaWhitePointTag = 0x77747074;
    internal const uint cmsSigNamedColorTag = 0x6E636f6C;
    internal const uint cmsSigNamedColor2Tag = 0x6E636C32;
    internal const uint cmsSigOutputResponseTag = 0x72657370;
    internal const uint cmsSigPerceptualRenderingIntentGamutTag = 0x72696730;
    internal const uint cmsSigPreview0Tag = 0x70726530;
    internal const uint cmsSigPreview1Tag = 0x70726531;
    internal const uint cmsSigPreview2Tag = 0x70726532;
    internal const uint cmsSigProfileDescriptionTag = 0x64657363;
    internal const uint cmsSigProfileDescriptionMLTag = 0x6473636d;
    internal const uint cmsSigProfileSequenceDescTag = 0x70736571;
    internal const uint cmsSigProfileSequenceIdTag = 0x70736964;
    internal const uint cmsSigPs2CRD0Tag = 0x70736430;
    internal const uint cmsSigPs2CRD1Tag = 0x70736431;
    internal const uint cmsSigPs2CRD2Tag = 0x70736432;
    internal const uint cmsSigPs2CRD3Tag = 0x70736433;
    internal const uint cmsSigPs2CSATag = 0x70733273;
    internal const uint cmsSigPs2RenderingIntentTag = 0x70733269;
    internal const uint cmsSigRedColorantTag = 0x7258595A;
    internal const uint cmsSigRedMatrixColumnTag = 0x7258595A;
    internal const uint cmsSigRedTRCTag = 0x72545243;
    internal const uint cmsSigSaturationRenderingIntentGamutTag = 0x72696732;
    internal const uint cmsSigScreeningDescTag = 0x73637264;
    internal const uint cmsSigScreeningTag = 0x7363726E;
    internal const uint cmsSigTechnologyTag = 0x74656368;
    internal const uint cmsSigUcrBgTag = 0x62666420;
    internal const uint cmsSigViewingCondDescTag = 0x76756564;
    internal const uint cmsSigViewingConditionsTag = 0x76696577;
    internal const uint cmsSigVcgtTag = 0x76636774;
    internal const uint cmsSigMetaTag = 0x6D657461;
    internal const uint cmsSigArgyllArtsTag = 0x61727473;

    internal const uint cmsSigDigitalCamera = 0x6463616D;
    internal const uint cmsSigFilmScanner = 0x6673636E;
    internal const uint cmsSigReflectiveScanner = 0x7273636E;
    internal const uint cmsSigInkJetPrinter = 0x696A6574;
    internal const uint cmsSigThermalWaxPrinter = 0x74776178;
    internal const uint cmsSigElectrophotographicPrinter = 0x6570686F;
    internal const uint cmsSigElectrostaticPrinter = 0x65737461;
    internal const uint cmsSigDyeSublimationPrinter = 0x64737562;
    internal const uint cmsSigPhotographicPaperPrinter = 0x7270686F;
    internal const uint cmsSigFilmWriter = 0x6670726E;
    internal const uint cmsSigVideoMonitor = 0x7669646D;
    internal const uint cmsSigVideoCamera = 0x76696463;
    internal const uint cmsSigProjectionTelevision = 0x706A7476;
    internal const uint cmsSigCRTDisplay = 0x43525420;
    internal const uint cmsSigPMDisplay = 0x504D4420;
    internal const uint cmsSigAMDisplay = 0x414D4420;
    internal const uint cmsSigPhotoCD = 0x4B504344;
    internal const uint cmsSigPhotoImageSetter = 0x696D6773;
    internal const uint cmsSigGravure = 0x67726176;
    internal const uint cmsSigOffsetLithography = 0x6F666673;
    internal const uint cmsSigSilkscreen = 0x73696C6B;
    internal const uint cmsSigFlexography = 0x666C6578;
    internal const uint cmsSigMotionPictureFilmScanner = 0x6D706673;
    internal const uint cmsSigMotionPictureFilmRecorder = 0x6D706672;
    internal const uint cmsSigDigitalMotionPictureCamera = 0x646D7063;
    internal const uint cmsSigDigitalCinemaProjector = 0x64636A70;

    internal const uint cmsSigXYZData = 0x58595A20;
    internal const uint cmsSigLabData = 0x4C616220;
    internal const uint cmsSigLuvData = 0x4C757620;
    internal const uint cmsSigYCbCrData = 0x59436272;
    internal const uint cmsSigYxyData = 0x59787920;
    internal const uint cmsSigRgbData = 0x52474220;
    internal const uint cmsSigGrayData = 0x47524159;
    internal const uint cmsSigHsvData = 0x48535620;
    internal const uint cmsSigHlsData = 0x484C5320;
    internal const uint cmsSigCmykData = 0x434D594B;
    internal const uint cmsSigCmyData = 0x434D5920;
    internal const uint cmsSigMCH1Data = 0x4D434831;
    internal const uint cmsSigMCH2Data = 0x4D434832;
    internal const uint cmsSigMCH3Data = 0x4D434833;
    internal const uint cmsSigMCH4Data = 0x4D434834;
    internal const uint cmsSigMCH5Data = 0x4D434835;
    internal const uint cmsSigMCH6Data = 0x4D434836;
    internal const uint cmsSigMCH7Data = 0x4D434837;
    internal const uint cmsSigMCH8Data = 0x4D434838;
    internal const uint cmsSigMCH9Data = 0x4D434839;
    internal const uint cmsSigMCHAData = 0x4D434841;
    internal const uint cmsSigMCHBData = 0x4D434842;
    internal const uint cmsSigMCHCData = 0x4D434843;
    internal const uint cmsSigMCHDData = 0x4D434844;
    internal const uint cmsSigMCHEData = 0x4D434845;
    internal const uint cmsSigMCHFData = 0x4D434846;
    internal const uint cmsSigNamedData = 0x6e6d636c;
    internal const uint cmsSig1colorData = 0x31434C52;
    internal const uint cmsSig2colorData = 0x32434C52;
    internal const uint cmsSig3colorData = 0x33434C52;
    internal const uint cmsSig4colorData = 0x34434C52;
    internal const uint cmsSig5colorData = 0x35434C52;
    internal const uint cmsSig6colorData = 0x36434C52;
    internal const uint cmsSig7colorData = 0x37434C52;
    internal const uint cmsSig8colorData = 0x38434C52;
    internal const uint cmsSig9colorData = 0x39434C52;
    internal const uint cmsSig10colorData = 0x41434C52;
    internal const uint cmsSig11colorData = 0x42434C52;
    internal const uint cmsSig12colorData = 0x43434C52;
    internal const uint cmsSig13colorData = 0x44434C52;
    internal const uint cmsSig14colorData = 0x45434C52;
    internal const uint cmsSig15colorData = 0x46434C52;
    internal const uint cmsSigLuvKData = 0x4C75764B;

    internal const uint cmsSigInputClass = 0x73636E72;
    internal const uint cmsSigDisplayClass = 0x6D6E7472;
    internal const uint cmsSigOutputClass = 0x70727472;
    internal const uint cmsSigLinkClass = 0x6C696E6B;
    internal const uint cmsSigAbstractClass = 0x61627374;
    internal const uint cmsSigColorSpaceClass = 0x73706163;
    internal const uint cmsSigNamedColorClass = 0x6e6d636c;

    internal const uint cmsSigMacintosh = 0x4150504C;
    internal const uint cmsSigMicrosoft = 0x4D534654;
    internal const uint cmsSigSolaris = 0x53554E57;
    internal const uint cmsSigSGI = 0x53474920;
    internal const uint cmsSigTaligent = 0x54474E54;
    internal const uint cmsSigUnices = 0x2A6E6978;

    internal const uint cmsSigPerceptualReferenceMediumGamut = 0x70726d67;

    internal const uint cmsSigSceneColorimetryEstimates = 0x73636F65;
    internal const uint cmsSigSceneAppearanceEstimates = 0x73617065;
    internal const uint cmsSigFocalPlaneColorimetryEstimates = 0x66706365;
    internal const uint cmsSigReflectionHardcopyOriginalColorimetry = 0x72686F63;
    internal const uint cmsSigReflectionPrintOutputColorimetry = 0x72706F63;

    internal const uint cmsSigCurveSetElemType = 0x63767374;
    internal const uint cmsSigMatrixElemType = 0x6D617466;
    internal const uint cmsSigCLutElemType = 0x636C7574;

    internal const uint cmsSigBAcsElemType = 0x62414353;
    internal const uint cmsSigEAcsElemType = 0x65414353;

    // Custom from here, not in the ICC Spec
    internal const uint cmsSigXYZ2LabElemType = 0x6C327820;

    internal const uint cmsSigLab2XYZElemType = 0x78326C20;
    internal const uint cmsSigNamedColorElemType = 0x6E636C20;
    internal const uint cmsSigLabV2toV4 = 0x32203420;
    internal const uint cmsSigLabV4toV2 = 0x34203220;

    // Identities
    internal const uint cmsSigIdentityElemType = 0x69646E20;

    // Float to floatPCS
    internal const uint cmsSigLab2FloatPCS = 0x64326C20;

    internal const uint cmsSigFloatPCS2Lab = 0x6C326420;
    internal const uint cmsSigXYZ2FloatPCS = 0x64327820;
    internal const uint cmsSigFloatPCS2XYZ = 0x78326420;
    internal const uint cmsSigClipNegativesElemType = 0x636c7020;

    internal const uint cmsSigFormulaCurveSeg = 0x70617266;
    internal const uint cmsSigSampledCurveSeg = 0x73616D66;
    internal const uint cmsSigSegmentedCurve = 0x63757266;

    internal const uint cmsSigStatusA = 0x53746141;
    internal const uint cmsSigStatusE = 0x53746145;
    internal const uint cmsSigStatusI = 0x53746149;
    internal const uint cmsSigStatusT = 0x53746154;
    internal const uint cmsSigStatusM = 0x5374614D;
    internal const uint cmsSigDN = 0x444E2020;
    internal const uint cmsSigDNP = 0x444E2050;
    internal const uint cmsSigDNN = 0x444E4E20;
    internal const uint cmsSigDNNP = 0x444E4E50;

    internal const uint cmsReflective = 0;
    internal const uint cmsTransparency = 1;
    internal const uint cmsGlossy = 0;
    internal const uint cmsMatte = 2;

    internal const byte cmsMAXCHANNELS = 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint PREMUL_SH(uint m) => m << 23;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint FLOAT_SH(uint m) => m << 22;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint OPTIMIZED_SH(uint m) => m << 21;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint COLORSPACE_SH(uint m) => m << 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint SWAPFIRST_SH(uint m) => m << 14;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint FLAVOR_SH(uint m) => m << 13;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint PLANAR_SH(uint m) => m << 12;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ENDIAN16_SH(uint m) => m << 11;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint DOSWAP_SH(uint m) => m << 10;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint EXTRA_SH(uint m) => m << 7;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint CHANNELS_SH(uint m) => m << 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint BYTES_SH(uint m) => m << 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_PREMUL(uint m) => (m >> 23) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_FLOAT(uint m) => (m >> 22) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_OPTIMIZED(uint m) => (m >> 21) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_COLORSPACE(uint m) => (m >> 16) & 31;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_SWAPFIRST(uint m) => (m >> 14) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_FLAVOR(uint m) => (m >> 13) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_PLANAR(uint m) => (m >> 12) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_ENDIAN16(uint m) => (m >> 11) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_DOSWAP(uint m) => (m >> 10) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_EXTRA(uint m) => (m >> 7) & 7;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_CHANNELS(uint m) => (m >> 3) & 15;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint T_BYTES(uint m) => (m >> 0) & 7;

    internal const ushort PT_ANY = 0;
    internal const ushort PT_GRAY = 3;
    internal const ushort PT_RGB = 4;
    internal const ushort PT_CMY = 5;
    internal const ushort PT_CMYK = 6;
    internal const ushort PT_YCbCr = 7;
    internal const ushort PT_YUV = 8;
    internal const ushort PT_XYZ = 9;
    internal const ushort PT_Lab = 10;
    internal const ushort PT_YUVK = 11;
    internal const ushort PT_HSV = 12;
    internal const ushort PT_HLS = 13;
    internal const ushort PT_Yxy = 14;
    internal const ushort PT_MCH1 = 15;
    internal const ushort PT_MCH2 = 16;
    internal const ushort PT_MCH3 = 17;
    internal const ushort PT_MCH4 = 18;
    internal const ushort PT_MCH5 = 19;
    internal const ushort PT_MCH6 = 20;
    internal const ushort PT_MCH7 = 21;
    internal const ushort PT_MCH8 = 22;
    internal const ushort PT_MCH9 = 23;
    internal const ushort PT_MCH10 = 24;
    internal const ushort PT_MCH11 = 25;
    internal const ushort PT_MCH12 = 26;
    internal const ushort PT_MCH13 = 27;
    internal const ushort PT_MCH14 = 28;
    internal const ushort PT_MCH15 = 29;
    internal const ushort PT_LabV2 = 30;

    public static uint TYPE_GRAY_8 = COLORSPACE_SH(PT_GRAY) | CHANNELS_SH(1) | BYTES_SH(1);
    public static uint TYPE_GRAY_8_REV = COLORSPACE_SH(PT_GRAY) | CHANNELS_SH(1) | BYTES_SH(1) | FLAVOR_SH(1);
    public static uint TYPE_GRAY_16 = COLORSPACE_SH(PT_GRAY) | CHANNELS_SH(1) | BYTES_SH(2);
    public static uint TYPE_GRAY_16_REV = COLORSPACE_SH(PT_GRAY) | CHANNELS_SH(1) | BYTES_SH(2) | FLAVOR_SH(1);
    public static uint TYPE_GRAY_16_SE = COLORSPACE_SH(PT_GRAY) | CHANNELS_SH(1) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_GRAYA_8 = COLORSPACE_SH(PT_GRAY) | EXTRA_SH(1) | CHANNELS_SH(1) | BYTES_SH(1);
    public static uint TYPE_GRAYA_8_PREMUL = COLORSPACE_SH(PT_GRAY) | EXTRA_SH(1) | CHANNELS_SH(1) | BYTES_SH(1) | PREMUL_SH(1);
    public static uint TYPE_GRAYA_16 = COLORSPACE_SH(PT_GRAY) | EXTRA_SH(1) | CHANNELS_SH(1) | BYTES_SH(2);
    public static uint TYPE_GRAYA_16_PREMUL = COLORSPACE_SH(PT_GRAY) | EXTRA_SH(1) | CHANNELS_SH(1) | BYTES_SH(2) | PREMUL_SH(1);
    public static uint TYPE_GRAYA_16_SE = COLORSPACE_SH(PT_GRAY) | EXTRA_SH(1) | CHANNELS_SH(1) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_GRAYA_8_PLANAR = COLORSPACE_SH(PT_GRAY) | EXTRA_SH(1) | CHANNELS_SH(1) | BYTES_SH(1) | PLANAR_SH(1);
    public static uint TYPE_GRAYA_16_PLANAR = COLORSPACE_SH(PT_GRAY) | EXTRA_SH(1) | CHANNELS_SH(1) | BYTES_SH(2) | PLANAR_SH(1);

    public static uint TYPE_RGB_8 = COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(1);
    public static uint TYPE_RGB_8_PLANAR = COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(1) | PLANAR_SH(1);
    public static uint TYPE_BGR_8 = COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1);
    public static uint TYPE_BGR_8_PLANAR = COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1) | PLANAR_SH(1);
    public static uint TYPE_RGB_16 = COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_RGB_16_PLANAR = COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(2) | PLANAR_SH(1);
    public static uint TYPE_RGB_16_SE = COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_BGR_16 = COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_BGR_16_PLANAR = COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1) | PLANAR_SH(1);
    public static uint TYPE_BGR_16_SE = COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1) | ENDIAN16_SH(1);

    public static uint TYPE_RGBA_8 = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1);
    public static uint TYPE_RGBA_8_PREMUL = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | PREMUL_SH(1);
    public static uint TYPE_RGBA_8_PLANAR = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | PLANAR_SH(1);
    public static uint TYPE_RGBA_16 = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_RGBA_16_PREMUL = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | PREMUL_SH(1);
    public static uint TYPE_RGBA_16_PLANAR = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | PLANAR_SH(1);
    public static uint TYPE_RGBA_16_SE = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | ENDIAN16_SH(1);

    public static uint TYPE_ARGB_8 = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | SWAPFIRST_SH(1);
    public static uint TYPE_ARGB_8_PREMUL = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | SWAPFIRST_SH(1) | PREMUL_SH(1);
    public static uint TYPE_ARGB_8_PLANAR = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | SWAPFIRST_SH(1) | PLANAR_SH(1);
    public static uint TYPE_ARGB_16 = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | SWAPFIRST_SH(1);
    public static uint TYPE_ARGB_16_PREMUL = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | SWAPFIRST_SH(1) | PREMUL_SH(1);

    public static uint TYPE_ABGR_8 = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1);
    public static uint TYPE_ABGR_8_PREMUL = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1) | PREMUL_SH(1);
    public static uint TYPE_ABGR_8_PLANAR = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1) | PLANAR_SH(1);
    public static uint TYPE_ABGR_16 = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_ABGR_16_PREMUL = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1) | PREMUL_SH(1);
    public static uint TYPE_ABGR_16_PLANAR = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1) | PLANAR_SH(1);
    public static uint TYPE_ABGR_16_SE = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1) | ENDIAN16_SH(1);

    public static uint TYPE_BGRA_8 = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1) | SWAPFIRST_SH(1);
    public static uint TYPE_BGRA_8_PREMUL = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1) | SWAPFIRST_SH(1) | PREMUL_SH(1);
    public static uint TYPE_BGRA_8_PLANAR = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1) | SWAPFIRST_SH(1) | PLANAR_SH(1);
    public static uint TYPE_BGRA_16 = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1) | SWAPFIRST_SH(1);
    public static uint TYPE_BGRA_16_PREMUL = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1) | SWAPFIRST_SH(1) | PREMUL_SH(1);
    public static uint TYPE_BGRA_16_SE = COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | ENDIAN16_SH(1) | DOSWAP_SH(1) | SWAPFIRST_SH(1);

    public static uint TYPE_CMY_8 = COLORSPACE_SH(PT_CMY) | CHANNELS_SH(3) | BYTES_SH(1);
    public static uint TYPE_CMY_8_PLANAR = COLORSPACE_SH(PT_CMY) | CHANNELS_SH(3) | BYTES_SH(1) | PLANAR_SH(1);
    public static uint TYPE_CMY_16 = COLORSPACE_SH(PT_CMY) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_CMY_16_PLANAR = COLORSPACE_SH(PT_CMY) | CHANNELS_SH(3) | BYTES_SH(2) | PLANAR_SH(1);
    public static uint TYPE_CMY_16_SE = COLORSPACE_SH(PT_CMY) | CHANNELS_SH(3) | BYTES_SH(2) | ENDIAN16_SH(1);

    public static uint TYPE_CMYK_8 = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(1);
    public static uint TYPE_CMYKA_8 = COLORSPACE_SH(PT_CMYK) | EXTRA_SH(1) | CHANNELS_SH(4) | BYTES_SH(1);
    public static uint TYPE_CMYK_8_REV = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(1) | FLAVOR_SH(1);
    public static uint TYPE_YUVK_8 = TYPE_CMYK_8_REV;
    public static uint TYPE_CMYK_8_PLANAR = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(1) | PLANAR_SH(1);
    public static uint TYPE_CMYK_16 = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(2);
    public static uint TYPE_CMYK_16_REV = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(2) | FLAVOR_SH(1);
    public static uint TYPE_YUVK_16 = TYPE_CMYK_16_REV;
    public static uint TYPE_CMYK_16_PLANAR = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(2) | PLANAR_SH(1);
    public static uint TYPE_CMYK_16_SE = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(2) | ENDIAN16_SH(1);

    public static uint TYPE_KYMC_8 = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(1) | DOSWAP_SH(1);
    public static uint TYPE_KYMC_16 = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_KYMC_16_SE = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(2) | DOSWAP_SH(1) | ENDIAN16_SH(1);

    public static uint TYPE_KCMY_8 = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(1) | SWAPFIRST_SH(1);
    public static uint TYPE_KCMY_8_REV = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(1) | FLAVOR_SH(1) | SWAPFIRST_SH(1);
    public static uint TYPE_KCMY_16 = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(2) | SWAPFIRST_SH(1);
    public static uint TYPE_KCMY_16_REV = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(2) | FLAVOR_SH(1) | SWAPFIRST_SH(1);
    public static uint TYPE_KCMY_16_SE = COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(2) | ENDIAN16_SH(1) | SWAPFIRST_SH(1);

    public static uint TYPE_CMYK5_8 = COLORSPACE_SH(PT_MCH5) | CHANNELS_SH(5) | BYTES_SH(1);
    public static uint TYPE_CMYK5_16 = COLORSPACE_SH(PT_MCH5) | CHANNELS_SH(5) | BYTES_SH(2);
    public static uint TYPE_CMYK5_16_SE = COLORSPACE_SH(PT_MCH5) | CHANNELS_SH(5) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_KYMC5_8 = COLORSPACE_SH(PT_MCH5) | CHANNELS_SH(5) | BYTES_SH(1) | DOSWAP_SH(1);
    public static uint TYPE_KYMC5_16 = COLORSPACE_SH(PT_MCH5) | CHANNELS_SH(5) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_KYMC5_16_SE = COLORSPACE_SH(PT_MCH5) | CHANNELS_SH(5) | BYTES_SH(2) | DOSWAP_SH(1) | ENDIAN16_SH(1);
    public static uint TYPE_CMYK6_8 = COLORSPACE_SH(PT_MCH6) | CHANNELS_SH(6) | BYTES_SH(1);
    public static uint TYPE_CMYK6_8_PLANAR = COLORSPACE_SH(PT_MCH6) | CHANNELS_SH(6) | BYTES_SH(1) | PLANAR_SH(1);
    public static uint TYPE_CMYK6_16 = COLORSPACE_SH(PT_MCH6) | CHANNELS_SH(6) | BYTES_SH(2);
    public static uint TYPE_CMYK6_16_PLANAR = COLORSPACE_SH(PT_MCH6) | CHANNELS_SH(6) | BYTES_SH(2) | PLANAR_SH(1);
    public static uint TYPE_CMYK6_16_SE = COLORSPACE_SH(PT_MCH6) | CHANNELS_SH(6) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_CMYK7_8 = COLORSPACE_SH(PT_MCH7) | CHANNELS_SH(7) | BYTES_SH(1);
    public static uint TYPE_CMYK7_16 = COLORSPACE_SH(PT_MCH7) | CHANNELS_SH(7) | BYTES_SH(2);
    public static uint TYPE_CMYK7_16_SE = COLORSPACE_SH(PT_MCH7) | CHANNELS_SH(7) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_KYMC7_8 = COLORSPACE_SH(PT_MCH7) | CHANNELS_SH(7) | BYTES_SH(1) | DOSWAP_SH(1);
    public static uint TYPE_KYMC7_16 = COLORSPACE_SH(PT_MCH7) | CHANNELS_SH(7) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_KYMC7_16_SE = COLORSPACE_SH(PT_MCH7) | CHANNELS_SH(7) | BYTES_SH(2) | DOSWAP_SH(1) | ENDIAN16_SH(1);
    public static uint TYPE_CMYK8_8 = COLORSPACE_SH(PT_MCH8) | CHANNELS_SH(8) | BYTES_SH(1);
    public static uint TYPE_CMYK8_16 = COLORSPACE_SH(PT_MCH8) | CHANNELS_SH(8) | BYTES_SH(2);
    public static uint TYPE_CMYK8_16_SE = COLORSPACE_SH(PT_MCH8) | CHANNELS_SH(8) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_KYMC8_8 = COLORSPACE_SH(PT_MCH8) | CHANNELS_SH(8) | BYTES_SH(1) | DOSWAP_SH(1);
    public static uint TYPE_KYMC8_16 = COLORSPACE_SH(PT_MCH8) | CHANNELS_SH(8) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_KYMC8_16_SE = COLORSPACE_SH(PT_MCH8) | CHANNELS_SH(8) | BYTES_SH(2) | DOSWAP_SH(1) | ENDIAN16_SH(1);
    public static uint TYPE_CMYK9_8 = COLORSPACE_SH(PT_MCH9) | CHANNELS_SH(9) | BYTES_SH(1);
    public static uint TYPE_CMYK9_16 = COLORSPACE_SH(PT_MCH9) | CHANNELS_SH(9) | BYTES_SH(2);
    public static uint TYPE_CMYK9_16_SE = COLORSPACE_SH(PT_MCH9) | CHANNELS_SH(9) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_KYMC9_8 = COLORSPACE_SH(PT_MCH9) | CHANNELS_SH(9) | BYTES_SH(1) | DOSWAP_SH(1);
    public static uint TYPE_KYMC9_16 = COLORSPACE_SH(PT_MCH9) | CHANNELS_SH(9) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_KYMC9_16_SE = COLORSPACE_SH(PT_MCH9) | CHANNELS_SH(9) | BYTES_SH(2) | DOSWAP_SH(1) | ENDIAN16_SH(1);
    public static uint TYPE_CMYK10_8 = COLORSPACE_SH(PT_MCH10) | CHANNELS_SH(10) | BYTES_SH(1);
    public static uint TYPE_CMYK10_16 = COLORSPACE_SH(PT_MCH10) | CHANNELS_SH(10) | BYTES_SH(2);
    public static uint TYPE_CMYK10_16_SE = COLORSPACE_SH(PT_MCH10) | CHANNELS_SH(10) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_KYMC10_8 = COLORSPACE_SH(PT_MCH10) | CHANNELS_SH(10) | BYTES_SH(1) | DOSWAP_SH(1);
    public static uint TYPE_KYMC10_16 = COLORSPACE_SH(PT_MCH10) | CHANNELS_SH(10) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_KYMC10_16_SE = COLORSPACE_SH(PT_MCH10) | CHANNELS_SH(10) | BYTES_SH(2) | DOSWAP_SH(1) | ENDIAN16_SH(1);
    public static uint TYPE_CMYK11_8 = COLORSPACE_SH(PT_MCH11) | CHANNELS_SH(11) | BYTES_SH(1);
    public static uint TYPE_CMYK11_16 = COLORSPACE_SH(PT_MCH11) | CHANNELS_SH(11) | BYTES_SH(2);
    public static uint TYPE_CMYK11_16_SE = COLORSPACE_SH(PT_MCH11) | CHANNELS_SH(11) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_KYMC11_8 = COLORSPACE_SH(PT_MCH11) | CHANNELS_SH(11) | BYTES_SH(1) | DOSWAP_SH(1);
    public static uint TYPE_KYMC11_16 = COLORSPACE_SH(PT_MCH11) | CHANNELS_SH(11) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_KYMC11_16_SE = COLORSPACE_SH(PT_MCH11) | CHANNELS_SH(11) | BYTES_SH(2) | DOSWAP_SH(1) | ENDIAN16_SH(1);
    public static uint TYPE_CMYK12_8 = COLORSPACE_SH(PT_MCH12) | CHANNELS_SH(12) | BYTES_SH(1);
    public static uint TYPE_CMYK12_16 = COLORSPACE_SH(PT_MCH12) | CHANNELS_SH(12) | BYTES_SH(2);
    public static uint TYPE_CMYK12_16_SE = COLORSPACE_SH(PT_MCH12) | CHANNELS_SH(12) | BYTES_SH(2) | ENDIAN16_SH(1);
    public static uint TYPE_KYMC12_8 = COLORSPACE_SH(PT_MCH12) | CHANNELS_SH(12) | BYTES_SH(1) | DOSWAP_SH(1);
    public static uint TYPE_KYMC12_16 = COLORSPACE_SH(PT_MCH12) | CHANNELS_SH(12) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_KYMC12_16_SE = COLORSPACE_SH(PT_MCH12) | CHANNELS_SH(12) | BYTES_SH(2) | DOSWAP_SH(1) | ENDIAN16_SH(1);

    // Colorimetric
    public static uint TYPE_XYZ_16 = COLORSPACE_SH(PT_XYZ) | CHANNELS_SH(3) | BYTES_SH(2);

    public static uint TYPE_Lab_8 = COLORSPACE_SH(PT_Lab) | CHANNELS_SH(3) | BYTES_SH(1);
    public static uint TYPE_LabV2_8 = COLORSPACE_SH(PT_LabV2) | CHANNELS_SH(3) | BYTES_SH(1);

    public static uint TYPE_ALab_8 = COLORSPACE_SH(PT_Lab) | CHANNELS_SH(3) | BYTES_SH(1) | EXTRA_SH(1) | SWAPFIRST_SH(1);
    public static uint TYPE_ALabV2_8 = COLORSPACE_SH(PT_LabV2) | CHANNELS_SH(3) | BYTES_SH(1) | EXTRA_SH(1) | SWAPFIRST_SH(1);
    public static uint TYPE_Lab_16 = COLORSPACE_SH(PT_Lab) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_LabV2_16 = COLORSPACE_SH(PT_LabV2) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_Yxy_16 = COLORSPACE_SH(PT_Yxy) | CHANNELS_SH(3) | BYTES_SH(2);

    // YCbCr
    public static uint TYPE_YCbCr_8 = COLORSPACE_SH(PT_YCbCr) | CHANNELS_SH(3) | BYTES_SH(1);

    public static uint TYPE_YCbCr_8_PLANAR = COLORSPACE_SH(PT_YCbCr) | CHANNELS_SH(3) | BYTES_SH(1) | PLANAR_SH(1);
    public static uint TYPE_YCbCr_16 = COLORSPACE_SH(PT_YCbCr) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_YCbCr_16_PLANAR = COLORSPACE_SH(PT_YCbCr) | CHANNELS_SH(3) | BYTES_SH(2) | PLANAR_SH(1);
    public static uint TYPE_YCbCr_16_SE = COLORSPACE_SH(PT_YCbCr) | CHANNELS_SH(3) | BYTES_SH(2) | ENDIAN16_SH(1);

    // YUV
    public static uint TYPE_YUV_8 = COLORSPACE_SH(PT_YUV) | CHANNELS_SH(3) | BYTES_SH(1);

    public static uint TYPE_YUV_8_PLANAR = COLORSPACE_SH(PT_YUV) | CHANNELS_SH(3) | BYTES_SH(1) | PLANAR_SH(1);
    public static uint TYPE_YUV_16 = COLORSPACE_SH(PT_YUV) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_YUV_16_PLANAR = COLORSPACE_SH(PT_YUV) | CHANNELS_SH(3) | BYTES_SH(2) | PLANAR_SH(1);
    public static uint TYPE_YUV_16_SE = COLORSPACE_SH(PT_YUV) | CHANNELS_SH(3) | BYTES_SH(2) | ENDIAN16_SH(1);

    // HLS
    public static uint TYPE_HLS_8 = COLORSPACE_SH(PT_HLS) | CHANNELS_SH(3) | BYTES_SH(1);

    public static uint TYPE_HLS_8_PLANAR = COLORSPACE_SH(PT_HLS) | CHANNELS_SH(3) | BYTES_SH(1) | PLANAR_SH(1);
    public static uint TYPE_HLS_16 = COLORSPACE_SH(PT_HLS) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_HLS_16_PLANAR = COLORSPACE_SH(PT_HLS) | CHANNELS_SH(3) | BYTES_SH(2) | PLANAR_SH(1);
    public static uint TYPE_HLS_16_SE = COLORSPACE_SH(PT_HLS) | CHANNELS_SH(3) | BYTES_SH(2) | ENDIAN16_SH(1);

    // HSV
    public static uint TYPE_HSV_8 = COLORSPACE_SH(PT_HSV) | CHANNELS_SH(3) | BYTES_SH(1);

    public static uint TYPE_HSV_8_PLANAR = COLORSPACE_SH(PT_HSV) | CHANNELS_SH(3) | BYTES_SH(1) | PLANAR_SH(1);
    public static uint TYPE_HSV_16 = COLORSPACE_SH(PT_HSV) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_HSV_16_PLANAR = COLORSPACE_SH(PT_HSV) | CHANNELS_SH(3) | BYTES_SH(2) | PLANAR_SH(1);
    public static uint TYPE_HSV_16_SE = COLORSPACE_SH(PT_HSV) | CHANNELS_SH(3) | BYTES_SH(2) | ENDIAN16_SH(1);

    // Named color index. Only 16 bits is allowed (don't check colorspace)
    public static uint TYPE_NAMED_COLOR_INDEX = CHANNELS_SH(1) | BYTES_SH(2);

    // Float formatters.
    public static uint TYPE_XYZ_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_XYZ) | CHANNELS_SH(3) | BYTES_SH(4);

    public static uint TYPE_Lab_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_Lab) | CHANNELS_SH(3) | BYTES_SH(4);
    public static uint TYPE_LabA_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_Lab) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(4);
    public static uint TYPE_GRAY_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_GRAY) | CHANNELS_SH(1) | BYTES_SH(4);
    public static uint TYPE_GRAYA_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_GRAY) | CHANNELS_SH(1) | BYTES_SH(4) | EXTRA_SH(1);
    public static uint TYPE_GRAYA_FLT_PREMUL = FLOAT_SH(1) | COLORSPACE_SH(PT_GRAY) | CHANNELS_SH(1) | BYTES_SH(4) | EXTRA_SH(1) | PREMUL_SH(1);
    public static uint TYPE_RGB_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(4);

    public static uint TYPE_RGBA_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(4);
    public static uint TYPE_RGBA_FLT_PREMUL = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(4) | PREMUL_SH(1);
    public static uint TYPE_ARGB_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(4) | SWAPFIRST_SH(1);
    public static uint TYPE_ARGB_FLT_PREMUL = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(4) | SWAPFIRST_SH(1) | PREMUL_SH(1);
    public static uint TYPE_BGR_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(4) | DOSWAP_SH(1);
    public static uint TYPE_BGRA_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(4) | DOSWAP_SH(1) | SWAPFIRST_SH(1);
    public static uint TYPE_BGRA_FLT_PREMUL = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(4) | DOSWAP_SH(1) | SWAPFIRST_SH(1) | PREMUL_SH(1);
    public static uint TYPE_ABGR_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(4) | DOSWAP_SH(1);
    public static uint TYPE_ABGR_FLT_PREMUL = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(4) | DOSWAP_SH(1) | PREMUL_SH(1);

    public static uint TYPE_CMYK_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(4);

    // Floating point formatters.
    // NOTE THAT 'BYTES' FIELD IS SET TO ZERO ON DLB because 8 bytes overflows the bitfield
    public static uint TYPE_XYZ_DBL = FLOAT_SH(1) | COLORSPACE_SH(PT_XYZ) | CHANNELS_SH(3) | BYTES_SH(0);

    public static uint TYPE_Lab_DBL = FLOAT_SH(1) | COLORSPACE_SH(PT_Lab) | CHANNELS_SH(3) | BYTES_SH(0);
    public static uint TYPE_GRAY_DBL = FLOAT_SH(1) | COLORSPACE_SH(PT_GRAY) | CHANNELS_SH(1) | BYTES_SH(0);
    public static uint TYPE_RGB_DBL = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(0);
    public static uint TYPE_BGR_DBL = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(0) | DOSWAP_SH(1);
    public static uint TYPE_CMYK_DBL = FLOAT_SH(1) | COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(0);

    // IEEE 754-2008 "half"
    public static uint TYPE_GRAY_HALF_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_GRAY) | CHANNELS_SH(1) | BYTES_SH(2);

    public static uint TYPE_RGB_HALF_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_RGBA_HALF_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2);
    public static uint TYPE_CMYK_HALF_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_CMYK) | CHANNELS_SH(4) | BYTES_SH(2);

    public static uint TYPE_ARGB_HALF_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | SWAPFIRST_SH(1);
    public static uint TYPE_BGR_HALF_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1);
    public static uint TYPE_BGRA_HALF_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | EXTRA_SH(1) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1) | SWAPFIRST_SH(1);
    public static uint TYPE_ABGR_HALF_FLT = FLOAT_SH(1) | COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1);

    internal const uint cmsILLUMINANT_TYPE_UNKNOWN = 0x0000000;
    internal const uint cmsILLUMINANT_TYPE_D50 = 0x0000001;
    internal const uint cmsILLUMINANT_TYPE_D65 = 0x0000002;
    internal const uint cmsILLUMINANT_TYPE_D93 = 0x0000003;
    internal const uint cmsILLUMINANT_TYPE_F2 = 0x0000004;
    internal const uint cmsILLUMINANT_TYPE_D55 = 0x0000005;
    internal const uint cmsILLUMINANT_TYPE_A = 0x0000006;
    internal const uint cmsILLUMINANT_TYPE_E = 0x0000007;
    internal const uint cmsILLUMINANT_TYPE_F8 = 0x0000008;

    internal const ErrorCode cmsERROR_UNDEFINED = ErrorCode.Undefined;
    internal const ErrorCode cmsERROR_FILE = ErrorCode.File;
    internal const ErrorCode cmsERROR_RANGE = ErrorCode.Range;
    internal const ErrorCode cmsERROR_INTERNAL = ErrorCode.Internal;
    internal const ErrorCode cmsERROR_NULL = ErrorCode.Null;
    internal const ErrorCode cmsERROR_READ = ErrorCode.Read;
    internal const ErrorCode cmsERROR_SEEK = ErrorCode.Seek;
    internal const ErrorCode cmsERROR_WRITE = ErrorCode.Write;
    internal const ErrorCode cmsERROR_UNKNOWN_EXTENSION = ErrorCode.UnknownExtension;
    internal const ErrorCode cmsERROR_COLORSPACE_CHECK = ErrorCode.ColorspaceCheck;
    internal const ErrorCode cmsERROR_ALREADY_DEFINED = ErrorCode.AlreadyDefined;
    internal const ErrorCode cmsERROR_BAD_SIGNATURE = ErrorCode.BadSignature;
    internal const ErrorCode cmsERROR_CORRUPTION_DETECTED = ErrorCode.CorruptionDetected;
    internal const ErrorCode cmsERROR_NOT_SUITABLE = ErrorCode.NotSuitable;

    internal const uint AVG_SURROUND = 1;
    internal const uint DIM_SURROUND = 2;
    internal const uint DARK_SURROUND = 3;
    internal const uint CUTSHEET_SURROUND = 4;

    internal const double D_CALCULATE = -1.0;

    internal const uint SAMPLER_INSPECT = 0x01000000;

    internal static readonly byte* cmsNoLanguage;
    internal static readonly byte* cmsNoCountry;

    internal const ushort cmsPRINTER_DEFAULT_SCREENS = 0x0001;
    internal const ushort cmsFREQUENCE_UNITS_LINES_CM = 0x0000;
    internal const ushort cmsFREQUENCE_UNITS_LINES_INCH = 0x0002;

    internal const byte cmsSPOT_UNKNOWN = 0;
    internal const byte cmsSPOT_PRINTER_DEFAULT = 1;
    internal const byte cmsSPOT_ROUND = 2;
    internal const byte cmsSPOT_DIAMOND = 3;
    internal const byte cmsSPOT_ELLIPSE = 4;
    internal const byte cmsSPOT_LINE = 5;
    internal const byte cmsSPOT_SQUARE = 6;
    internal const byte cmsSPOT_CROSS = 7;

    internal const uint cmsEmbeddedProfileFalse = 0x00000000;
    internal const uint cmsEmbeddedProfileTrue = 0x00000001;
    internal const uint cmsUseAnywhere = 0x00000000;
    internal const uint cmsUseWithEmbeddedDataOnly = 0x00000002;

    internal const byte LCMS_USED_AS_INPUT = 0;
    internal const byte LCMS_USED_AS_OUTPUT = 1;
    internal const byte LCMS_USED_AS_PROOF = 2;

    internal const byte cmsInfoDescription = 0;
    internal const byte cmsInfoManufacturer = 1;
    internal const byte cmsInfoModel = 2;
    internal const byte cmsInfoCopyright = 3;

    // ICC Intents
    internal const byte INTENT_PERCEPTUAL = 0;

    internal const byte INTENT_RELATIVE_COLORIMETRIC = 1;
    internal const byte INTENT_SATURATION = 2;
    internal const byte INTENT_ABSOLUTE_COLORIMETRIC = 3;

    // Non-ICC intents
    internal const byte INTENT_PRESERVE_K_ONLY_PERCEPTUAL = 10;

    internal const byte INTENT_PRESERVE_K_ONLY_RELATIVE_COLORIMETRIC = 11;
    internal const byte INTENT_PRESERVE_K_ONLY_SATURATION = 12;
    internal const byte INTENT_PRESERVE_K_PLANE_PERCEPTUAL = 13;
    internal const byte INTENT_PRESERVE_K_PLANE_RELATIVE_COLORIMETRIC = 14;
    internal const byte INTENT_PRESERVE_K_PLANE_SATURATION = 15;

    // Flags

    internal const ushort cmsFLAGS_NOCACHE = 0x0040;
    internal const ushort cmsFLAGS_NOOPTIMIZE = 0x0100;
    internal const ushort cmsFLAGS_NULLTRANSFORM = 0x0200;

    // Proofing flags
    internal const ushort cmsFLAGS_GAMUTCHECK = 0x1000;

    internal const ushort cmsFLAGS_SOFTPROOFING = 0x4000;

    // Misc
    internal const ushort cmsFLAGS_BLACKPOINTCOMPENSATION = 0x2000;

    internal const ushort cmsFLAGS_NOWHITEONWHITEFIXUP = 0x0004;
    internal const ushort cmsFLAGS_HIGHRESPRECALC = 0x0400;
    internal const ushort cmsFLAGS_LOWRESPRECALC = 0x0800;

    // For devicelink creation
    internal const ushort cmsFLAGS_8BITS_DEVICELINK = 0x0008;

    internal const ushort cmsFLAGS_GUESSDEVICECLASS = 0x0020;
    internal const ushort cmsFLAGS_KEEP_SEQUENCE = 0x0080;

    // Specific to a particular optimizations
    internal const ushort cmsFLAGS_FORCE_CLUT = 0x0002;

    internal const ushort cmsFLAGS_CLUT_POST_LINEARIZATION = 0x0001;
    internal const ushort cmsFLAGS_CLUT_PRE_LINEARIZATION = 0x0010;

    // Specific to unbounded mode
    internal const ushort cmsFLAGS_NONEGATIVES = 0x8000;

    // Copy alpha channels when transforming
    internal const uint cmsFLAGS_COPY_ALPHA = 0x04000000;

    // Fine-tune control over number of gridpoints
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint cmsFLAGS_GRIDPOINTS(int n) =>
        (uint)(n & 0xFF) << 16;

    // CRD special
    internal const uint cmsFLAGS_NODEFAULTRESOURCEDEF = 0x01000000;

    #endregion lcms2.h

    #region lcms2_plugin.h

    internal const int VX = 0;
    internal const int VY = 1;
    internal const int VZ = 2;

    internal const uint cmsPluginMagicNumber = 0x61637070;              // 'acpp'

    internal const uint cmsPluginMemHandlerSig = 0x6D656D48;            // 'memH'
    internal const uint cmsPluginInterpolationSig = 0x696E7048;         // 'inpH'
    internal const uint cmsPluginParametricCurveSig = 0x70617248;       // 'parH'
    internal const uint cmsPluginFormattersSig = 0x66726D48;            // 'frmH
    internal const uint cmsPluginTagTypeSig = 0x74797048;               // 'typH'
    internal const uint cmsPluginTagSig = 0x74616748;                   // 'tagH'
    internal const uint cmsPluginRenderingIntentSig = 0x696E7448;       // 'intH'
    internal const uint cmsPluginMultiProcessElementSig = 0x6D706548;   // 'mpeH'
    internal const uint cmsPluginOptimizationSig = 0x6F707448;          // 'optH'
    internal const uint cmsPluginTransformSig = 0x7A666D48;             // 'xfmH'
    internal const uint cmsPluginMutexSig = 0x6D747A48;                 // 'mtxH'

    internal const byte MAX_TYPES_IN_LCMS_PLUGIN = 20;

    internal const byte MAX_INPUT_DIMENSIONS = 15;

    #endregion lcms2_plugin.h

    #region lcms2_internal.h

    internal const double M_PI = 3.14159265358979323846;
    internal const double M_LOG10E = 0.434294481903251827651;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T _cmsALIGNLONG<T>(T x) where T : IBitwiseOperators<T, uint, T>, IAdditionOperators<T, uint, T> =>
        (x + (_sizeof<uint>() - 1u)) & ~(_sizeof<uint>() - 1u);

    internal static ushort CMS_PTR_ALIGNMENT = _sizeof<nint>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T _cmsALIGNMEM<T>(T x) where T : IBitwiseOperators<T, uint, T>, IAdditionOperators<T, uint, T> =>
        (x + ((uint)CMS_PTR_ALIGNMENT - 1)) & ~((uint)CMS_PTR_ALIGNMENT - 1);

    internal const double MAX_ENCODEABLE_XYZ = 1 + (32767.0 / 32768);
    internal const double MIN_ENCODEABLE_ab2 = -128.0;
    internal const double MAX_ENCODEABLE_ab2 = (65535.0 / 256) - 128;
    internal const double MIN_ENCODEABLE_ab4 = -128.0;
    internal const double MAX_ENCODEABLE_ab4 = 127.0;

    internal const byte MAX_STAGE_CHANNELS = 128;

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static ushort FROM_8_TO_16(uint rgb) => (ushort)((rgb << 8) | rgb);

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static byte FROM_16_TO_8(uint rgb) => (byte)((((rgb * 65281u) + 8388608u) >> 24) & 0xFFu);

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static void _cmsAssert(bool condition) =>
        Debug.Assert(condition);

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static void _cmsAssert(void* ptr) =>
        Debug.Assert(ptr is not null);

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static void _cmsAssert(object? obj) =>
        Debug.Assert(obj is not null);

    internal const double MATRIX_DET_TOLERANCE = 1e-4;

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static int FIXED_TO_INT(int x) => x >> 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static int FIXED_REST_TO_INT(int x) => x & 0xFFFF;

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static int ROUND_FIXED_TO_INT(int x) => (x + 0x8000) >> 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static int _cmsToFixedDomain(int a) => a + ((a + 0x7fff) / 0xffff);

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static int _cmsFromFixedDomain(int a) => a - ((a + 0x7fff) >> 16);

    [StructLayout(LayoutKind.Explicit)]
    internal struct _temp
    {
        [FieldOffset(0)]
        public double val;

        [FieldOffset(0)]
        public fixed int halves[2];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static int _cmsQuickFloor(double val)
    {
#if CMS_DONT_USE_FAST_FLOOR
        (int)Math.Floor(val);
#else
        const double _lcms_double2fixmagic = 68719476736.0 * 1.5;
        _temp temp;
        temp.val = val + _lcms_double2fixmagic;

        return temp.halves[0] >> 16;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static ushort _cmsQuickFloorWord(double d) =>
        (ushort)(_cmsQuickFloor(d - 32767) + 32767);

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static ushort _cmsQuickSaturateWord(double d)
    {
        d += 0.5;
        return d switch
        {
            <= 0 => 0,
            >= 65535.0 => 0xffff,
            _ => _cmsQuickFloorWord(d),
        };
    }

    internal const byte MAX_TABLE_TAG = 100;

    internal const uint cmsFLAGS_CAN_CHANGE_FORMATTER = 0x02000000;

    #endregion lcms2_internal.h

    private static readonly Destructor Finalise = new();

    private sealed class Destructor
    {
        ~Destructor()
        {
            // Context and plugins
            free(globalContext);
            free(globalLogErrorChunk);
            free(globalAlarmCodesChunk);
            free(globalAdaptationStateChunk);
            free(globalMemPluginChunk);
            free(globalInterpPluginChunk);
            free(globalCurvePluginChunk);
            free(globalFormattersPluginChunk);
            free(supportedTagTypes);
            free(globalTagTypePluginChunk);
            free(supportedTags);
            free(globalTagPluginChunk);
            free(globalIntentsPluginChunk);
            free(supportedMPEtypes);
            free(globalMPETypePluginChunk);
            free(globalOptimizationPluginChunk);
            free(globalTransformPluginChunk);
            free(globalMutexPluginChunk);

            // WhitePoint defaults
            free(D50XYZ);
            free(D50xyY);

            // Colorspace endpoints
            free(RGBblack);
            free(RGBwhite);
            free(CMYKblack);
            free(CMYKwhite);
            free(LABblack);
            free(LABwhite);
            free(CMYblack);
            free(CMYwhite);
            free(GrayBlack);
            free(GrayWhite);

            // MLU defaults
            free(cmsNoLanguage);
            free(cmsNoCountry);

            // io1 "const"s
            free(Device2PCS16);
            free(Device2PCSFloat);
            free(PCS2Device16);
            free(PCS2DeviceFloat);
            free(GrayInputMatrix);
            free(OneToThreeInputMatrix);
            free(PickYMatrix);
            free(PickLstarMatrix);

            // Optimization defaults
            free(DefaultOptimization);

            // Intents defaults
            free(defaultIntents);

            // Lut defaults
            free(AllowedLUTTypes);
        }
    }

    static Lcms2()
    {
        #region Context and plugins

        var defaultTag = default(TagLinkedList);
        var tagNextOffset = (nuint)(&defaultTag.Next) - (nuint)(&defaultTag);

        var defaultTagType = default(TagTypeLinkedList);
        var tagTypeNextOffset = (nuint)(&defaultTagType.Next) - (nuint)(&defaultTagType);

        // Error logger
        fixed (LogErrorChunkType* plugin = &LogErrorChunk)
            globalLogErrorChunk = dup<LogErrorChunkType>(plugin);

        // Alarm Codes
        fixed (AlarmCodesChunkType* plugin = &AlarmCodesChunk)
        {
            plugin->AlarmCodes[0] = plugin->AlarmCodes[1] = plugin->AlarmCodes[2] = 0x7F00;

            globalAlarmCodesChunk = dup<AlarmCodesChunkType>(plugin);
        }

        // Adaptation State
        fixed (AdaptationStateChunkType* plugin = &AdaptationStateChunk)
            globalAdaptationStateChunk = dup<AdaptationStateChunkType>(plugin);

        // Memory Handler
        globalMemPluginChunk = alloc<MemPluginChunkType>();
        *globalMemPluginChunk = new()
        {
            MallocPtr = _cmsMallocDefaultFn,
            MallocZeroPtr = _cmsMallocZeroDefaultFn,
            FreePtr = _cmsFreeDefaultFn,
            ReallocPtr = _cmsReallocDefaultFn,
            CallocPtr = _cmsCallocDefaultFn,
            DupPtr = _cmsDupDefaultFn
        };

        // Interpolation Plugin
        fixed (InterpPluginChunkType* plugin = &InterpPluginChunk)
            globalInterpPluginChunk = dup<InterpPluginChunkType>(plugin);

        // Curves Plugin
        fixed (ParametricCurvesCollection* curves = &defaultCurves)
        {
            fixed (int* defaultFunctionTypes = defaultCurvesFunctionTypes)
                memcpy(curves->FunctionTypes, defaultFunctionTypes, 10 * _sizeof<int>());
            fixed (uint* defaultParameterCount = defaultCurvesParameterCounts)
                memcpy(curves->ParameterCount, defaultParameterCount, 10 * _sizeof<uint>());
        }
        fixed (CurvesPluginChunkType* plugin = &CurvesPluginChunk)
            globalCurvePluginChunk = dup<CurvesPluginChunkType>(plugin);

        // Formatters Plugin
        globalFormattersPluginChunk = alloc<FormattersPluginChunkType>();
        *globalFormattersPluginChunk = new();

        // Tag Type Plugin
        supportedTagTypes = calloc<TagTypeLinkedList>(31);

        supportedTagTypes[0] = new(new(cmsSigChromaticityType, Type_Chromaticity_Read, Type_Chromaticity_Write, Type_Chromaticity_Dup, Type_Chromaticity_Free, null, 0), &supportedTagTypes[1]);
        supportedTagTypes[1] = new(new(cmsSigColorantOrderType, Type_ColorantOrderType_Read, Type_ColorantOrderType_Write, Type_ColorantOrderType_Dup, Type_ColorantOrderType_Free, null, 0), &supportedTagTypes[2]);
        supportedTagTypes[2] = new(new(cmsSigS15Fixed16ArrayType, Type_S15Fixed16_Read, Type_S15Fixed16_Write, Type_S15Fixed16_Dup, Type_S15Fixed16_Free, null, 0), &supportedTagTypes[3]);
        supportedTagTypes[3] = new(new(cmsSigU16Fixed16ArrayType, Type_U16Fixed16_Read, Type_U16Fixed16_Write, Type_U16Fixed16_Dup, Type_U16Fixed16_Free, null, 0), &supportedTagTypes[4]);
        supportedTagTypes[4] = new(new(cmsSigTextType, Type_Text_Read, Type_Text_Write, Type_Text_Dup, Type_Text_Free, null, 0), &supportedTagTypes[5]);
        supportedTagTypes[5] = new(new(cmsSigTextDescriptionType, Type_Text_Description_Read, Type_Text_Description_Write, Type_Text_Description_Dup, Type_Text_Description_Free, null, 0), &supportedTagTypes[6]);
        supportedTagTypes[6] = new(new(cmsSigCurveType, Type_Curve_Read, Type_Curve_Write, Type_Curve_Dup, Type_Curve_Free, null, 0), &supportedTagTypes[7]);
        supportedTagTypes[7] = new(new(cmsSigParametricCurveType, Type_ParametricCurve_Read, Type_ParametricCurve_Write, Type_ParametricCurve_Dup, Type_ParametricCurve_Free, null, 0), &supportedTagTypes[8]);
        supportedTagTypes[8] = new(new(cmsSigDateTimeType, Type_DateTime_Read, Type_DateTime_Write, Type_DateTime_Dup, Type_DateTime_Free, null, 0), &supportedTagTypes[9]);
        supportedTagTypes[9] = new(new(cmsSigLut8Type, Type_LUT8_Read, Type_LUT8_Write, Type_LUT8_Dup, Type_LUT8_Free, null, 0), &supportedTagTypes[10]);
        supportedTagTypes[10] = new(new(cmsSigLut16Type, Type_LUT16_Read, Type_LUT16_Write, Type_LUT16_Dup, Type_LUT16_Free, null, 0), &supportedTagTypes[11]);
        supportedTagTypes[11] = new(new(cmsSigColorantTableType, Type_ColorantTable_Read, Type_ColorantTable_Write, Type_ColorantTable_Dup, Type_ColorantTable_Free, null, 0), &supportedTagTypes[12]);
        supportedTagTypes[12] = new(new(cmsSigNamedColor2Type, Type_NamedColor_Read, Type_NamedColor_Write, Type_NamedColor_Dup, Type_NamedColor_Free, null, 0), &supportedTagTypes[13]);
        supportedTagTypes[13] = new(new(cmsSigMultiLocalizedUnicodeType, Type_MLU_Read, Type_MLU_Write, Type_MLU_Dup, Type_MLU_Free, null, 0), &supportedTagTypes[14]);
        supportedTagTypes[14] = new(new(cmsSigProfileSequenceDescType, Type_ProfileSequenceDesc_Read, Type_ProfileSequenceDesc_Write, Type_ProfileSequenceDesc_Dup, Type_ProfileSequenceDesc_Free, null, 0), &supportedTagTypes[15]);
        supportedTagTypes[15] = new(new(cmsSigSignatureType, Type_Signature_Read, Type_Signature_Write, Type_Signature_Dup, Type_Signature_Free, null, 0), &supportedTagTypes[16]);
        supportedTagTypes[16] = new(new(cmsSigMeasurementType, Type_Measurement_Read, Type_Measurement_Write, Type_Measurement_Dup, Type_Measurement_Free, null, 0), &supportedTagTypes[17]);
        supportedTagTypes[17] = new(new(cmsSigDataType, Type_Data_Read, Type_Data_Write, Type_Data_Dup, Type_Data_Free, null, 0), &supportedTagTypes[18]);
        supportedTagTypes[18] = new(new(cmsSigLutAtoBType, Type_LUTA2B_Read, Type_LUTA2B_Write, Type_LUTA2B_Dup, Type_LUTA2B_Free, null, 0), &supportedTagTypes[19]);
        supportedTagTypes[19] = new(new(cmsSigLutBtoAType, Type_LUTB2A_Read, Type_LUTB2A_Write, Type_LUTB2A_Dup, Type_LUTB2A_Free, null, 0), &supportedTagTypes[20]);
        supportedTagTypes[20] = new(new(cmsSigUcrBgType, Type_UcrBg_Read, Type_UcrBg_Write, Type_UcrBg_Dup, Type_UcrBg_Free, null, 0), &supportedTagTypes[21]);
        supportedTagTypes[21] = new(new(cmsSigCrdInfoType, Type_CrdInfo_Read, Type_CrdInfo_Write, Type_CrdInfo_Dup, Type_CrdInfo_Free, null, 0), &supportedTagTypes[22]);
        supportedTagTypes[22] = new(new(cmsSigMultiProcessElementType, Type_MPE_Read, Type_MPE_Write, Type_MPE_Dup, Type_MPE_Free, null, 0), &supportedTagTypes[23]);
        supportedTagTypes[23] = new(new(cmsSigScreeningType, Type_Screening_Read, Type_Screening_Write, Type_Screening_Dup, Type_Screening_Free, null, 0), &supportedTagTypes[24]);
        supportedTagTypes[24] = new(new(cmsSigViewingConditionsType, Type_ViewingConditions_Read, Type_ViewingConditions_Write, Type_ViewingConditions_Dup, Type_ViewingConditions_Free, null, 0), &supportedTagTypes[25]);
        supportedTagTypes[25] = new(new(cmsSigXYZType, Type_XYZ_Read, Type_XYZ_Write, Type_XYZ_Dup, Type_XYZ_Free, null, 0), &supportedTagTypes[26]);
        supportedTagTypes[26] = new(new(cmsCorbisBrokenXYZtype, Type_XYZ_Read, Type_XYZ_Write, Type_XYZ_Dup, Type_XYZ_Free, null, 0), &supportedTagTypes[27]);
        supportedTagTypes[27] = new(new(cmsMonacoBrokenCurveType, Type_Curve_Read, Type_Curve_Write, Type_Curve_Dup, Type_Curve_Free, null, 0), &supportedTagTypes[28]);
        supportedTagTypes[28] = new(new(cmsSigProfileSequenceIdType, Type_ProfileSequenceId_Read, Type_ProfileSequenceId_Write, Type_ProfileSequenceId_Dup, Type_ProfileSequenceId_Free, null, 0), &supportedTagTypes[29]);
        supportedTagTypes[29] = new(new(cmsSigDictType, Type_Dictionary_Read, Type_Dictionary_Write, Type_Dictionary_Dup, Type_Dictionary_Free, null, 0), &supportedTagTypes[30]);
        supportedTagTypes[30] = new(new(cmsSigVcgtType, Type_vcgt_Read, Type_vcgt_Write, Type_vcgt_Dup, Type_vcgt_Free, null, 0), null);

        fixed (TagTypePluginChunkType* plugin = &TagTypePluginChunk)
            globalTagTypePluginChunk = dup(plugin);

        // Tag Plugin
        supportedTags = calloc<TagLinkedList>(64);
        supportedTags[0] = new(cmsSigAToB0Tag, new(1, new Signature[] { cmsSigLut16Type, cmsSigLutAtoBType, cmsSigLut8Type, }, DecideLUTtypeA2B), &supportedTags[1]);
        supportedTags[1] = new(cmsSigAToB1Tag, new(1, new Signature[] { cmsSigLut16Type, cmsSigLutAtoBType, cmsSigLut8Type, }, DecideLUTtypeA2B), &supportedTags[2]);
        supportedTags[2] = new(cmsSigAToB2Tag, new(1, new Signature[] { cmsSigLut16Type, cmsSigLutAtoBType, cmsSigLut8Type, }, DecideLUTtypeA2B), &supportedTags[3]);
        supportedTags[3] = new(cmsSigBToA0Tag, new(1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A), &supportedTags[4]);
        supportedTags[4] = new(cmsSigBToA1Tag, new(1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A), &supportedTags[5]);
        supportedTags[5] = new(cmsSigBToA2Tag, new(1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A), &supportedTags[6]);
        supportedTags[6] = new(cmsSigRedColorantTag, new(1, new Signature[] { cmsSigXYZType, cmsCorbisBrokenXYZtype, }, DecideXYZtype), &supportedTags[7]);
        supportedTags[7] = new(cmsSigGreenColorantTag, new(1, new Signature[] { cmsSigXYZType, cmsCorbisBrokenXYZtype, }, DecideXYZtype), &supportedTags[8]);
        supportedTags[8] = new(cmsSigBlueColorantTag, new(1, new Signature[] { cmsSigXYZType, cmsCorbisBrokenXYZtype, }, DecideXYZtype), &supportedTags[9]);
        supportedTags[9] = new(cmsSigRedTRCTag, new(1, new Signature[] { cmsSigCurveType, cmsSigParametricCurveType, cmsMonacoBrokenCurveType, }, DecideCurveType), &supportedTags[10]);
        supportedTags[10] = new(cmsSigGreenTRCTag, new(1, new Signature[] { cmsSigCurveType, cmsSigParametricCurveType, cmsMonacoBrokenCurveType, }, DecideCurveType), &supportedTags[11]);
        supportedTags[11] = new(cmsSigBlueTRCTag, new(1, new Signature[] { cmsSigCurveType, cmsSigParametricCurveType, cmsMonacoBrokenCurveType, }, DecideCurveType), &supportedTags[12]);
        supportedTags[12] = new(cmsSigCalibrationDateTimeTag, new(1, new Signature[] { cmsSigDateTimeType, }, null), &supportedTags[13]);
        supportedTags[13] = new(cmsSigCharTargetTag, new(1, new Signature[] { cmsSigTextType, }, null), &supportedTags[14]);
        supportedTags[14] = new(cmsSigChromaticAdaptationTag, new(9, new Signature[] { cmsSigS15Fixed16ArrayType, }, null), &supportedTags[15]);
        supportedTags[15] = new(cmsSigChromaticityTag, new(1, new Signature[] { cmsSigChromaticityType, }, null), &supportedTags[16]);
        supportedTags[16] = new(cmsSigColorantOrderTag, new(1, new Signature[] { cmsSigColorantOrderType, }, null), &supportedTags[17]);
        supportedTags[17] = new(cmsSigColorantTableTag, new(1, new Signature[] { cmsSigColorantTableType, }, null), &supportedTags[18]);
        supportedTags[18] = new(cmsSigColorantTableOutTag, new(1, new Signature[] { cmsSigColorantTableType, }, null), &supportedTags[19]);
        supportedTags[19] = new(cmsSigCopyrightTag, new(1, new Signature[] { cmsSigTextType, cmsSigMultiLocalizedUnicodeType, cmsSigTextDescriptionType, }, DecideTextType), &supportedTags[20]);
        supportedTags[20] = new(cmsSigDateTimeTag, new(1, new Signature[] { cmsSigDateTimeType, }, null), &supportedTags[21]);
        supportedTags[21] = new(cmsSigDeviceMfgDescTag, new(1, new Signature[] { cmsSigTextDescriptionType, cmsSigMultiLocalizedUnicodeType, cmsSigTextType, }, DecideTextDescType), &supportedTags[22]);
        supportedTags[22] = new(cmsSigDeviceModelDescTag, new(1, new Signature[] { cmsSigTextDescriptionType, cmsSigMultiLocalizedUnicodeType, cmsSigTextType, }, DecideTextDescType), &supportedTags[23]);
        supportedTags[23] = new(cmsSigGamutTag, new(1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A), &supportedTags[24]);
        supportedTags[24] = new(cmsSigGrayTRCTag, new(1, new Signature[] { cmsSigCurveType, cmsSigParametricCurveType, }, DecideCurveType), &supportedTags[25]);
        supportedTags[25] = new(cmsSigLuminanceTag, new(1, new Signature[] { cmsSigXYZType, }, null), &supportedTags[26]);
        supportedTags[26] = new(cmsSigMediaBlackPointTag, new(1, new Signature[] { cmsSigXYZType, cmsCorbisBrokenXYZtype, }, null), &supportedTags[27]);
        supportedTags[27] = new(cmsSigMediaWhitePointTag, new(1, new Signature[] { cmsSigXYZType, cmsCorbisBrokenXYZtype, }, null), &supportedTags[28]);
        supportedTags[28] = new(cmsSigNamedColor2Tag, new(1, new Signature[] { cmsSigNamedColor2Type, }, null), &supportedTags[29]);
        supportedTags[29] = new(cmsSigPreview0Tag, new(1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A), &supportedTags[30]);
        supportedTags[30] = new(cmsSigPreview1Tag, new(1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A), &supportedTags[31]);
        supportedTags[31] = new(cmsSigPreview2Tag, new(1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A), &supportedTags[32]);
        supportedTags[32] = new(cmsSigProfileDescriptionTag, new(1, new Signature[] { cmsSigTextDescriptionType, cmsSigMultiLocalizedUnicodeType, cmsSigTextType, }, DecideTextDescType), &supportedTags[33]);
        supportedTags[33] = new(cmsSigProfileSequenceDescTag, new(1, new Signature[] { cmsSigProfileSequenceDescType, }, null), &supportedTags[34]);
        supportedTags[34] = new(cmsSigTechnologyTag, new(1, new Signature[] { cmsSigSignatureType, }, null), &supportedTags[35]);
        supportedTags[35] = new(cmsSigColorimetricIntentImageStateTag, new(1, new Signature[] { cmsSigSignatureType, }, null), &supportedTags[36]);
        supportedTags[36] = new(cmsSigPerceptualRenderingIntentGamutTag, new(1, new Signature[] { cmsSigSignatureType, }, null), &supportedTags[37]);
        supportedTags[37] = new(cmsSigSaturationRenderingIntentGamutTag, new(1, new Signature[] { cmsSigSignatureType, }, null), &supportedTags[38]);
        supportedTags[38] = new(cmsSigMeasurementTag, new(1, new Signature[] { cmsSigMeasurementType, }, null), &supportedTags[39]);
        supportedTags[39] = new(cmsSigPs2CRD0Tag, new(1, new Signature[] { cmsSigDataType, }, null), &supportedTags[40]);
        supportedTags[40] = new(cmsSigPs2CRD1Tag, new(1, new Signature[] { cmsSigDataType, }, null), &supportedTags[41]);
        supportedTags[41] = new(cmsSigPs2CRD2Tag, new(1, new Signature[] { cmsSigDataType, }, null), &supportedTags[42]);
        supportedTags[42] = new(cmsSigPs2CRD3Tag, new(1, new Signature[] { cmsSigDataType, }, null), &supportedTags[43]);
        supportedTags[43] = new(cmsSigPs2CSATag, new(1, new Signature[] { cmsSigDataType, }, null), &supportedTags[44]);
        supportedTags[44] = new(cmsSigPs2RenderingIntentTag, new(1, new Signature[] { cmsSigDataType, }, null), &supportedTags[45]);
        supportedTags[45] = new(cmsSigViewingCondDescTag, new(1, new Signature[] { cmsSigTextDescriptionType, cmsSigMultiLocalizedUnicodeType, cmsSigTextType, }, DecideTextDescType), &supportedTags[46]);
        supportedTags[46] = new(cmsSigUcrBgTag, new(1, new Signature[] { cmsSigUcrBgType, }, null), &supportedTags[47]);
        supportedTags[47] = new(cmsSigCrdInfoTag, new(1, new Signature[] { cmsSigCrdInfoType, }, null), &supportedTags[48]);
        supportedTags[48] = new(cmsSigDToB0Tag, new(1, new Signature[] { cmsSigMultiProcessElementType, }, null), &supportedTags[49]);
        supportedTags[49] = new(cmsSigDToB1Tag, new(1, new Signature[] { cmsSigMultiProcessElementType, }, null), &supportedTags[50]);
        supportedTags[50] = new(cmsSigDToB2Tag, new(1, new Signature[] { cmsSigMultiProcessElementType, }, null), &supportedTags[51]);
        supportedTags[51] = new(cmsSigDToB3Tag, new(1, new Signature[] { cmsSigMultiProcessElementType, }, null), &supportedTags[52]);
        supportedTags[52] = new(cmsSigBToD0Tag, new(1, new Signature[] { cmsSigMultiProcessElementType, }, null), &supportedTags[53]);
        supportedTags[53] = new(cmsSigBToD1Tag, new(1, new Signature[] { cmsSigMultiProcessElementType, }, null), &supportedTags[54]);
        supportedTags[54] = new(cmsSigBToD2Tag, new(1, new Signature[] { cmsSigMultiProcessElementType, }, null), &supportedTags[55]);
        supportedTags[55] = new(cmsSigBToD3Tag, new(1, new Signature[] { cmsSigMultiProcessElementType, }, null), &supportedTags[56]);
        supportedTags[56] = new(cmsSigScreeningDescTag, new(1, new Signature[] { cmsSigTextDescriptionType, }, null), &supportedTags[57]);
        supportedTags[57] = new(cmsSigViewingConditionsTag, new(1, new Signature[] { cmsSigViewingConditionsType, }, null), &supportedTags[58]);
        supportedTags[58] = new(cmsSigScreeningTag, new(1, new Signature[] { cmsSigScreeningType, }, null), &supportedTags[59]);
        supportedTags[59] = new(cmsSigVcgtTag, new(1, new Signature[] { cmsSigVcgtType, }, null), &supportedTags[60]);
        supportedTags[60] = new(cmsSigMetaTag, new(1, new Signature[] { cmsSigDictType, }, null), &supportedTags[61]);
        supportedTags[61] = new(cmsSigProfileSequenceIdTag, new(1, new Signature[] { cmsSigProfileSequenceIdType, }, null), &supportedTags[62]);
        supportedTags[62] = new(cmsSigProfileDescriptionMLTag, new(1, new Signature[] { cmsSigMultiLocalizedUnicodeType, }, null), &supportedTags[63]);
        supportedTags[63] = new(cmsSigArgyllArtsTag, new(9, new Signature[] { cmsSigS15Fixed16ArrayType, }, null), null);

        fixed (TagPluginChunkType* plugin = &TagPluginChunk)
            globalTagPluginChunk = dup(plugin);

        // Intents Plugin
        fixed (IntentsPluginChunkType* plugin = &IntentsPluginChunk)
            globalIntentsPluginChunk = dup(plugin);

        // MPE Type Plugin
        supportedMPEtypes = calloc<TagTypeLinkedList>(5);
        supportedMPEtypes[0] = new(new(cmsSigBAcsElemType, null, null, null, null, null, 0), &supportedMPEtypes[1]);
        supportedMPEtypes[1] = new(new(cmsSigEAcsElemType, null, null, null, null, null, 0), &supportedMPEtypes[2]);
        supportedMPEtypes[2] = new(new(cmsSigCurveSetElemType, Type_MPEcurve_Read, Type_MPEcurve_Write, GenericMPEdup, GenericMPEfree, null, 0), &supportedMPEtypes[3]);
        supportedMPEtypes[3] = new(new(cmsSigMatrixElemType, Type_MPEmatrix_Read, Type_MPEmatrix_Write, GenericMPEdup, GenericMPEfree, null, 0), &supportedMPEtypes[4]);
        supportedMPEtypes[4] = new(new(cmsSigCLutElemType, Type_MPEclut_Read, Type_MPEclut_Write, GenericMPEdup, GenericMPEfree, null, 0), null);

        fixed (TagTypePluginChunkType* plugin = &MPETypePluginChunk)
            globalMPETypePluginChunk = dup(plugin);

        // Optimization Plugin
        fixed (OptimizationPluginChunkType* plugin = &OptimizationPluginChunk)
            globalOptimizationPluginChunk = dup(plugin);

        // Transform Plugin
        fixed (TransformPluginChunkType* plugin = &TransformPluginChunk)
            globalTransformPluginChunk = dup(plugin);

        // Mutex Plugin
        fixed (MutexPluginChunkType* plugin = &MutexChunk)
            globalMutexPluginChunk = dup(plugin);

        // Global Context
        globalContext = (Context)alloc(_sizeof<Context_struct>());
        *globalContext = new()
        {
            Next = null,
            MemPool = null,
            DefaultMemoryManager = default,
        };
        globalContext->chunks.parent = globalContext;

        globalContext->chunks[Chunks.UserPtr] = null;
        globalContext->chunks[Chunks.Logger] = globalLogErrorChunk;
        globalContext->chunks[Chunks.AlarmCodesContext] = globalAlarmCodesChunk;
        globalContext->chunks[Chunks.AdaptationStateContext] = globalAdaptationStateChunk;
        globalContext->chunks[Chunks.MemPlugin] = globalMemPluginChunk;
        globalContext->chunks[Chunks.InterpPlugin] = globalInterpPluginChunk;
        globalContext->chunks[Chunks.CurvesPlugin] = globalCurvePluginChunk;
        globalContext->chunks[Chunks.FormattersPlugin] = globalFormattersPluginChunk;
        globalContext->chunks[Chunks.TagTypePlugin] = globalTagTypePluginChunk;
        globalContext->chunks[Chunks.TagPlugin] = globalTagPluginChunk;
        globalContext->chunks[Chunks.IntentPlugin] = globalIntentsPluginChunk;
        globalContext->chunks[Chunks.MPEPlugin] = globalMPETypePluginChunk;
        globalContext->chunks[Chunks.OptimizationPlugin] = globalOptimizationPluginChunk;
        globalContext->chunks[Chunks.TransformPlugin] = globalTransformPluginChunk;
        globalContext->chunks[Chunks.MutexPlugin] = globalMutexPluginChunk;

        #endregion Context and plugins

        #region WhitePoint defaults

        D50XYZ = alloc<CIEXYZ>();
        *D50XYZ = new() { X = cmsD50X, Y = cmsD50Y, Z = cmsD50Z };
        D50xyY = alloc<CIExyY>();
        cmsXYZ2xyY(D50xyY, D50XYZ);

        #endregion WhitePoint defaults

        #region Colorspace endpoints

        RGBblack = calloc<ushort>(4);
        RGBwhite = calloc<ushort>(4);
        CMYKblack = calloc<ushort>(4);
        CMYKwhite = calloc<ushort>(4);
        LABblack = calloc<ushort>(4);
        LABwhite = calloc<ushort>(4);
        CMYblack = calloc<ushort>(4);
        CMYwhite = calloc<ushort>(4);
        GrayBlack = calloc<ushort>(4);
        GrayWhite = calloc<ushort>(4);

        RGBwhite[0] = RGBwhite[1] = RGBwhite[2] = 0xffff;
        CMYKblack[0] = CMYKblack[1] = CMYKblack[2] = CMYKblack[3] = 0xffff;
        LABblack[1] = LABblack[2] = LABwhite[1] = LABwhite[2] = 0x8080;
        LABwhite[0] = 0xffff;
        CMYblack[0] = CMYblack[1] = CMYblack[2] = 0xffff;
        GrayWhite[0] = 0xffff;

        #endregion Colorspace endpoints

        #region Mlu defaults

        cmsNoLanguage = calloc<byte>(3);
        cmsNoCountry = calloc<byte>(3);

        #endregion Mlu defaults

        #region io1 "const"s

        {
            var temp1 = stackalloc Signature[4]
            {
                cmsSigAToB0Tag,     // Perceptual
                cmsSigAToB1Tag,     // Relative colorimetric
                cmsSigAToB2Tag,     // Saturation
                cmsSigAToB1Tag,     // Absolute colorimetric
            };
            var temp2 = stackalloc Signature[4]
            {
                cmsSigDToB0Tag,     // Perceptual
                cmsSigDToB1Tag,     // Relative colorimetric
                cmsSigDToB2Tag,     // Saturation
                cmsSigDToB3Tag,     // Absolute colorimetric
            };
            var temp3 = stackalloc Signature[4]
            {
                cmsSigBToA0Tag,     // Perceptual
                cmsSigBToA1Tag,     // Relative colorimetric
                cmsSigBToA2Tag,     // Saturation
                cmsSigBToA1Tag,     // Absolute colorimetric
            };
            var temp4 = stackalloc Signature[4]
            {
                cmsSigBToD0Tag,     // Perceptual
                cmsSigBToD1Tag,     // Relative colorimetric
                cmsSigBToD2Tag,     // Saturation
                cmsSigBToD3Tag,     // Absolute colorimetric
            };
            var temp5 = stackalloc double[3]
            {
                InpAdj * cmsD50X,
                InpAdj * cmsD50Y,
                InpAdj * cmsD50Z,
            };
            var temp6 = stackalloc double[3] { 1, 1, 1, };
            var temp7 = stackalloc double[3] { 0, OutpAdj * cmsD50Y, 0, };
            var temp8 = stackalloc double[3] { 1, 0, 0, };

            Device2PCS16 = (Signature*)dup(temp1, 4 * _sizeof<Signature>());
            Device2PCSFloat = (Signature*)dup(temp2, 4 * _sizeof<Signature>());
            PCS2Device16 = (Signature*)dup(temp3, 4 * _sizeof<Signature>());
            PCS2DeviceFloat = (Signature*)dup(temp4, 4 * _sizeof<Signature>());

            GrayInputMatrix = (double*)dup(temp5, 4 * _sizeof<double>());
            OneToThreeInputMatrix = (double*)dup(temp6, 4 * _sizeof<double>());
            PickYMatrix = (double*)dup(temp7, 4 * _sizeof<double>());
            PickLstarMatrix = (double*)dup(temp8, 4 * _sizeof<double>());
        }

        #endregion io1 "const"s

        #region Optimization defaults

        DefaultOptimization = calloc<OptimizationCollection>(4);
        DefaultOptimization[0] = new() { OptimizePtr = OptimizeByJoiningCurves, Next = &DefaultOptimization[1] };
        DefaultOptimization[1] = new() { OptimizePtr = OptimizeMatrixShaper, Next = &DefaultOptimization[2] };
        DefaultOptimization[2] = new() { OptimizePtr = OptimizeByComputingLinearization, Next = &DefaultOptimization[3] };
        DefaultOptimization[3] = new() { OptimizePtr = OptimizeByResampling, Next = null };

        #endregion Optimization defaults

        #region Intents defaults

        defaultIntents = calloc<IntentsList>(10);
        defaultIntents[0] = new()
        {
            Intent = INTENT_PERCEPTUAL,
            Description = "Perceptual",
            Link = DefaultICCintents,
            Next = &defaultIntents[1]
        };
        defaultIntents[1] = new()
        {
            Intent = INTENT_RELATIVE_COLORIMETRIC,
            Description = "Relative colorimetric",
            Link = DefaultICCintents,
            Next = &defaultIntents[2]
        };
        defaultIntents[2] = new()
        {
            Intent = INTENT_SATURATION,
            Description = "Saturation",
            Link = DefaultICCintents,
            Next = &defaultIntents[3]
        };
        defaultIntents[3] = new()
        {
            Intent = INTENT_ABSOLUTE_COLORIMETRIC,
            Description = "Absolute colorimetric",
            Link = DefaultICCintents,
            Next = &defaultIntents[4]
        };
        defaultIntents[4] = new()
        {
            Intent = INTENT_PRESERVE_K_ONLY_PERCEPTUAL,
            Description = "Perceptual preserving black ink",
            Link = DefaultICCintents,
            Next = &defaultIntents[5]
        };
        defaultIntents[5] = new()
        {
            Intent = INTENT_PRESERVE_K_ONLY_RELATIVE_COLORIMETRIC,
            Description = "Relative colorimetric preserving black ink",
            Link = DefaultICCintents,
            Next = &defaultIntents[6]
        };
        defaultIntents[6] = new()
        {
            Intent = INTENT_PRESERVE_K_ONLY_SATURATION,
            Description = "Saturation preserving black ink",
            Link = DefaultICCintents,
            Next = &defaultIntents[7]
        };
        defaultIntents[7] = new()
        {
            Intent = INTENT_PRESERVE_K_PLANE_PERCEPTUAL,
            Description = "Perceptual preserving black plane",
            Link = DefaultICCintents,
            Next = &defaultIntents[8]
        };
        defaultIntents[8] = new()
        {
            Intent = INTENT_PRESERVE_K_PLANE_RELATIVE_COLORIMETRIC,
            Description = "Relative colorimetric preserving black plane",
            Link = DefaultICCintents,
            Next = &defaultIntents[9]
        };
        defaultIntents[9] = new()
        {
            Intent = INTENT_PRESERVE_K_PLANE_SATURATION,
            Description = "Saturation preserving black plane",
            Link = DefaultICCintents,
            Next = null
        };

        #endregion Intents defaults

        #region Lut defaults

        var luts = new AllowedLUT[]
        {
            new(false, default, cmsSigLut16Type, cmsSigMatrixElemType, cmsSigCurveSetElemType, cmsSigCLutElemType, cmsSigCurveSetElemType),
            new(false, default, cmsSigLut16Type, cmsSigCurveSetElemType, cmsSigCLutElemType, cmsSigCurveSetElemType),
            new(false, default, cmsSigLut16Type, cmsSigCurveSetElemType, cmsSigCLutElemType),
            new(true, default, cmsSigLutAtoBType, cmsSigCurveSetElemType),
            new(true, cmsSigAToB0Tag, cmsSigLutAtoBType, cmsSigCurveSetElemType, cmsSigMatrixElemType, cmsSigCurveSetElemType),
            new(true, cmsSigAToB0Tag, cmsSigLutAtoBType, cmsSigCurveSetElemType, cmsSigCLutElemType, cmsSigCurveSetElemType),
            new(true, cmsSigAToB0Tag, cmsSigLutAtoBType, cmsSigCurveSetElemType, cmsSigCLutElemType, cmsSigCurveSetElemType, cmsSigMatrixElemType, cmsSigCurveSetElemType),
            new(true, cmsSigBToA0Tag, cmsSigLutBtoAType, cmsSigCurveSetElemType),
            new(true, cmsSigBToA0Tag, cmsSigLutBtoAType, cmsSigCurveSetElemType, cmsSigMatrixElemType, cmsSigCurveSetElemType),
            new(true, cmsSigBToA0Tag, cmsSigLutBtoAType, cmsSigCurveSetElemType, cmsSigCLutElemType, cmsSigCurveSetElemType),
            new(true, cmsSigBToA0Tag, cmsSigLutBtoAType, cmsSigCurveSetElemType, cmsSigMatrixElemType, cmsSigCurveSetElemType, cmsSigCLutElemType, cmsSigCurveSetElemType),
        };
        AllowedLUTTypes = calloc<AllowedLUT>((uint)luts.Length);
        fixed (AllowedLUT* ptr = luts)
            for (int i = 0; i < luts.Length; i++)
                memcpy(&AllowedLUTTypes[i], &ptr[i]);

        #endregion Lut defaults
    }

    private static void WriteAllocs(nuint ptr, nuint size, string label)
    {
        Console.ResetColor();

        Console.Write($"{label}\t{size}\t at 0x{ptr >> 16:X12}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"{ptr & 0xFFFF:X4}");

        Console.ResetColor();
    }

    [DebuggerStepThrough]
    internal static ushort _sizeof<T>() where T : struct
    {
        if (typeof(T) == typeof(Screening))
            return (ushort)(sizeof(Screening) - 1 + (sizeof(ScreeningChannel) * cmsMAXCHANNELS));

        if (typeof(T) == typeof(Context_struct))
            return (ushort)(sizeof(Context_struct) - 1);

        return (ushort)sizeof(T);
    }

    [DebuggerStepThrough]
    internal static void* alloc(nuint size)
    {
        var result = NativeMemory.Alloc(size);
        AllocList.Add(((nuint)result, size));
        if (PrintAllocs)
            WriteAllocs((nuint)result, size, "Alloc\t");

        return result;
    }

    [DebuggerStepThrough]
    internal static void* alloc(nint size) =>
        alloc((nuint)size);

    [DebuggerStepThrough]
    internal static T* alloc<T>() where T : struct =>
        (T*)alloc(_sizeof<T>());

    [DebuggerStepThrough]
    internal static void* allocZeroed(nuint size)
    {
        var result = NativeMemory.AllocZeroed(size);
        AllocList.Add(((nuint)result, size));
        if (PrintAllocs)
            WriteAllocs((nuint)result, size, "AllocZero");

        return result;
    }

    [DebuggerStepThrough]
    internal static void* allocZeroed(nint size) =>
        allocZeroed((nuint)size);

    [DebuggerStepThrough]
    internal static T* allocZeroed<T>() where T : struct =>
        (T*)allocZeroed(_sizeof<T>());

    [DebuggerStepThrough]
    internal static void* dup(in void* org, nint size) =>
        dup(org, (nuint)size);

    [DebuggerStepThrough]
    internal static void* dup(in void* org, nuint size)
    {
        var value = alloc(size);
        memcpy(value, org, size);

        return value;
    }

    [DebuggerStepThrough]
    internal static T* dup<T>(in T* org) where T : struct
    {
        var value = alloc<T>();
        memcpy(value, org);

        return value;
    }

    [DebuggerStepThrough]
    internal static void memset<T>(T* dst, int val) where T : struct =>
        memset(dst, val, _sizeof<T>());

    [DebuggerStepThrough]
    internal static void memset(void* dst, int val, nint size) =>
        NativeMemory.Fill(dst, (uint)size, (byte)val);

    [DebuggerStepThrough]
    internal static void memset(void* dst, int val, nuint size) =>
        NativeMemory.Fill(dst, size, (byte)val);

    [DebuggerStepThrough]
    internal static void memmove<T>(T* dst, in T* src) where T : struct =>
        memcpy(dst, src);

    [DebuggerStepThrough]
    internal static void memmove(void* dst, in void* src, nuint size) =>
        memcpy(dst, src, size);

    [DebuggerStepThrough]
    internal static void memmove(void* dst, in void* src, nint size) =>
        memcpy(dst, src, size);

    [DebuggerStepThrough]
    internal static void memcpy<T>(T* dst, in T* src) where T : struct =>
        memcpy(dst, src, _sizeof<T>());

    [DebuggerStepThrough]
    internal static void memcpy(void* dst, in void* src, nuint size) =>
        NativeMemory.Copy(src, dst, size);

    [DebuggerStepThrough]
    internal static void memcpy(void* dst, in void* src, nint size) =>
        NativeMemory.Copy(src, dst, (nuint)size);

    [DebuggerStepThrough]
    internal static int memcmp(in void* buf1, in void* buf2, nint count)
    {
        nint counter = 0;
        while (counter < count)
        {
            var val = ((byte*)buf1)[counter] - ((byte*)buf2)[counter++];
            if (val is not 0)
                return val;
        }
        return 0;
    }

    [DebuggerStepThrough]
    internal static void free(void* ptr)
    {
        var item = AllocList.Find(p => p.Item1 == ((nuint)ptr));
        _cmsAssert(item);
        AllocList.Remove(item);
        if (PrintAllocs)
            WriteAllocs(item.Item1, item.Item2, "Free\t");
        NativeMemory.Free(ptr);
    }

    [DebuggerStepThrough]
    internal static void* calloc(uint num, nuint size)
    {
        var result = NativeMemory.AllocZeroed(num, size);
        AllocList.Add(((nuint)result, num * size));

        return result;
    }

    [DebuggerStepThrough]
    internal static void* calloc(uint num, nint size) =>
        calloc(num, (nuint)size);

    [DebuggerStepThrough]
    internal static T* calloc<T>(uint num) where T : struct =>
        (T*)calloc(num, _sizeof<T>());

    internal static nint strlen(in byte* str)
    {
        var ptr = str;

        while (*ptr is not 0)
            ptr++;

        return (nint)(ptr - str);
    }

    internal static byte* strncpy(byte* dest, in byte* src, nuint n)
    {
        var strSrc = src;
        var strDest = dest;

        while (strDest < dest + n)
            *strDest++ = (*strSrc is not 0) ? *strSrc++ : (byte)0;

        return dest;
    }

    internal static byte* strcpy(byte* dest, in byte* src)
    {
        var strSrc = src;
        var strDest = dest;

        do
        {
            *strDest++ = *strSrc;
        } while (*strSrc++ is not 0);

        return dest;
    }

    internal static int strcmp(byte* sLeft, ReadOnlySpan<byte> sRight) =>
        strcmp(new ReadOnlySpan<byte>(sLeft, (int)mywcslen(sLeft)), sRight);

    internal static int strcmp(ReadOnlySpan<byte> sLeft, ReadOnlySpan<byte> sRight)
    {
        var end = cmsmin(sLeft.Length, sRight.Length);

        for (var i = 0; i < end; i++)
        {
            var val = sRight[i] - sLeft[i];

            if (val is not 0)
                return val;
        }

        if (sLeft.Length > sRight.Length)
            return -sLeft[end];
        if (sRight.Length > sLeft.Length)
            return sRight[end];
        return 0;
    }

    internal static int strcmp(byte* sLeft, byte* sRight)
    {
        int val;
        do
        {
            val = *sLeft - *sRight;
        } while (val is 0 && *sLeft++ is not 0 && *sRight++ is not 0);

        return val;
    }

    internal static int sprintf(byte* buffer, string format, params object[] args)
    {
        var str = String.Format(format, args).AsSpan();
        var result = str.Length;

        while (str.Length > 0)
        {
            *buffer++ = (byte)str[0];
            str = str[1..];
        }
        *buffer = 0;

        return result;
    }

    internal static nuint fread(void* Buffer, nuint ElementSize, nuint ElementCount, FILE* Stream)
    {
        var stream = Stream->Stream;

        _cmsAssert(Buffer);
        _cmsAssert(stream);

        for (nuint i = 0; i < ElementCount; i++)
        {
            try
            {
                if (stream.Read(new(Buffer, (int)ElementSize)) != (int)ElementSize)
                    return i;
            }
            catch (Exception)
            {
                return i;
            }

            Buffer = (byte*)Buffer + ElementSize;
        }

        return ElementCount;
    }

    internal const byte SEEK_CUR = 1;
    internal const byte SEEK_END = 2;
    internal const byte SEEK_SET = 0;

    internal static int fseek(FILE* stream, long offset, int origin)
    {
        var file = stream->Stream;

        try
        {
            file.Seek(offset, origin is SEEK_CUR ? SeekOrigin.Current : origin is SEEK_END ? SeekOrigin.End : SeekOrigin.Begin);
            return 0;
        }
        catch
        {
            return -1;
        }
    }

    internal static long ftell(FILE* stream)
    {
        var file = stream->Stream;

        try
        {
            return file.Position;
        }
        catch (Exception)
        {
            return -1;
        }
    }

    internal static nuint fwrite(in void* Buffer, nuint ElementSize, nuint ElementCount, FILE* Stream)
    {
        var stream = Stream->Stream;
        var buffer = (byte*)Buffer;

        _cmsAssert(Buffer);
        _cmsAssert(stream);

        for (nuint i = 0; i < ElementCount; i++)
        {
            try
            {
                stream.Write(new(buffer, (int)ElementSize));
            }
            catch (Exception)
            {
                return i;
            }

            buffer = buffer + ElementSize;
        }

        return ElementCount;
    }

    internal static int fclose(FILE* stream)
    {
        var file = stream->Stream;
        var filename = stream->Filename;
        free(stream);

        var index = OpenFiles.FindIndex(i => i.file.Filename == filename);
        var f = OpenFiles[index];
        f.count--;
        if (f.count == 0)
        {
            OpenFiles.RemoveAt(index);
            try
            {
                file.Close();
            }
            catch (Exception)
            {
                return -1;
            }
        }
        else
        {
            OpenFiles[index] = f;
        }

        return 0;
    }
    
    internal static FILE* fopen(string filename, string mode)
    {
        Stream stream;
        int index = OpenFiles.FindIndex(i => i.file.Filename == filename);
        if (index is not -1)
        {
            var f = OpenFiles[index];
            f.count++;
            OpenFiles[index] = f;
            stream = f.file.Stream;
        }
        else
        {
            try
            {
                var options = new FileStreamOptions();
                if (mode.Contains('r'))
                {
                    options.Mode = FileMode.Open;
                    options.Access = FileAccess.Read;
                }
                else if (mode.Contains('w'))
                {
                    options.Mode = FileMode.Create;
                    options.Access = FileAccess.ReadWrite;
                }
                else
                {
                    return null;
                }
                stream = File.Open(filename, options);
            }
            catch
            {
                return null;
            }
        }
        var file = alloc<FILE>();
        file->Stream = stream;
        file->Filename = filename;

        if (index is -1)
        {
            OpenFiles.Add((*file, 1));
        }

        return file;
    }
}
