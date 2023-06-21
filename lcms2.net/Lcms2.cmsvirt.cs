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

using lcms2.state;
using lcms2.types;

using System.Runtime.CompilerServices;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static bool SetTextTags(Profile Profile, in char* Description)
    {
        Mlu* DescriptionMLU, CopyrightMLU;
        var rc = false;
        var ContextID = cmsGetProfileContextID(Profile);
        var en = stackalloc byte[] { (byte)'e', (byte)'n', 0 };
        var us = stackalloc byte[] { (byte)'U', (byte)'S', 0 };
        var copyright = "No copyright, use freely\0";

        DescriptionMLU = cmsMLUalloc(ContextID, 1);
        CopyrightMLU = cmsMLUalloc(ContextID, 1);

        if (DescriptionMLU is null || CopyrightMLU is null) goto Error;

        if (!cmsMLUsetWide(DescriptionMLU, en, us, Description)) goto Error;
        if (!cmsMLUsetWide(CopyrightMLU, en, us, (char*)&copyright)) goto Error;

        if (!cmsWriteTag(Profile, cmsSigProfileDescriptionTag, new BoxPtr<Mlu>(DescriptionMLU))) goto Error;
        if (!cmsWriteTag(Profile, cmsSigCopyrightTag, new BoxPtr<Mlu>(CopyrightMLU))) goto Error;

        rc = true;

    Error:
        if (DescriptionMLU is not null)
            cmsMLUfree(DescriptionMLU);
        if (CopyrightMLU is not null)
            cmsMLUfree(CopyrightMLU);
        return rc;
    }

    private static bool SetSeqDescTag(Profile Profile, in byte* Model)
    {
        var rc = false;
        var ContextID = cmsGetProfileContextID(Profile);
        var Seq = cmsAllocProfileSequenceDescription(ContextID, 1);
        var name = "Little CMS"u8;

        if (Seq is null) return false;

        Seq->seq[0].deviceMfg = 0;
        Seq->seq[0].deviceModel = 0;

        Seq->seq[0].attributes = 0;

        Seq->seq[0].technology = 0;

        fixed (byte* ptr = name)
            cmsMLUsetASCII(Seq->seq[0].Manufacturer, cmsNoLanguage, cmsNoCountry, ptr);
        cmsMLUsetASCII(Seq->seq[0].Model, cmsNoLanguage, cmsNoCountry, Model);

        if (!_cmsWriteProfileSequence(Profile, Seq))
            goto Error;

        rc = true;

    Error:
        if (Seq is not null)
            cmsFreeProfileSequenceDescription(Seq);

        return rc;
    }

    public static Profile? cmsCreateRGBProfileTHR(Context? ContextID, in CIExyY* WhitePoint, in CIExyYTRIPLE* Primaries, ToneCurve** TransferFunction)
    {
        CIEXYZ WhitePointXYZ;
        MAT3 CHAD;

        var hICC = cmsCreateProfilePlaceholder(ContextID);
        if (hICC is null)       // can't allocate
            return null;

        cmsSetProfileVersion(hICC, 4.3);

        cmsSetDeviceClass(hICC, cmsSigDisplayClass);
        cmsSetColorSpace(hICC, cmsSigRgbData);
        cmsSetPCS(hICC, cmsSigXYZData);

        cmsSetHeaderRenderingIntent(hICC, INTENT_PERCEPTUAL);

        // Implement profile using following tags:
        //
        //  1 cmsSigProfileDescriptionTag
        //  2 cmsSigMediaWhitePointTag
        //  3 cmsSigRedColorantTag
        //  4 cmsSigGreenColorantTag
        //  5 cmsSigBlueColorantTag
        //  6 cmsSigRedTRCTag
        //  7 cmsSigGreenTRCTag
        //  8 cmsSigBlueTRCTag
        //  9 Chromatic adaptation Tag
        // This conforms a standard RGB DisplayProfile as says ICC, and then I add (As per addendum II)
        // 10 cmsSigChromaticityTag

        var text = "RGB built-in";
        if (!SetTextTags(hICC, (char*)&text))
            goto Error;

        if (WhitePoint is not null)
        {
            if (!cmsWriteTag(hICC, cmsSigMediaWhitePointTag, new BoxPtr<CIEXYZ>(cmsD50_XYZ())))
                goto Error;

            cmsxyY2XYZ(&WhitePointXYZ, WhitePoint);
            _cmsAdaptationMatrix(&CHAD, null, &WhitePointXYZ, cmsD50_XYZ());

            // This is a V4 tag, but many CMM does read and understand it no matter which version
            if (!cmsWriteTag(hICC, cmsSigChromaticAdaptationTag, new BoxPtr<double>((double*)&CHAD)))
                goto Error;

            if (Primaries is not null)
            {
                CIEXYZTRIPLE Colorants;
                CIExyY MaxWhite;
                MAT3 MColorants;

                MaxWhite.x = WhitePoint->x;
                MaxWhite.y = WhitePoint->y;
                MaxWhite.Y = 1.0;

                if (!_cmsBuildRGB2XYZtransferMatrix(&MColorants, &MaxWhite, Primaries))
                    goto Error;

                Colorants.Red.X = MColorants.X.X;
                Colorants.Red.Y = MColorants.Y.X;
                Colorants.Red.Z = MColorants.Z.X;

                Colorants.Green.X = MColorants.X.Y;
                Colorants.Green.Y = MColorants.Y.Y;
                Colorants.Green.Z = MColorants.Z.Y;

                Colorants.Blue.X = MColorants.X.Z;
                Colorants.Blue.Y = MColorants.Y.Z;
                Colorants.Blue.Z = MColorants.Z.Z;

                if (!cmsWriteTag(hICC, cmsSigRedColorantTag, new BoxPtr<CIEXYZ>(&Colorants.Red))) goto Error;
                if (!cmsWriteTag(hICC, cmsSigBlueColorantTag, new BoxPtr<CIEXYZ>(&Colorants.Blue))) goto Error;
                if (!cmsWriteTag(hICC, cmsSigGreenColorantTag, new BoxPtr<CIEXYZ>(&Colorants.Green))) goto Error;
            }
        }

        if (TransferFunction is not null)
        {
            // Tries to minimize space. Thanks to Richard Hughes for this nice idea
            if (!cmsWriteTag(hICC, cmsSigRedTRCTag, new BoxPtr<ToneCurve>(TransferFunction[0])))
                goto Error;

            if (TransferFunction[1] == TransferFunction[0])
            {
                if (!cmsLinkTag(hICC, cmsSigGreenTRCTag, cmsSigRedTRCTag)) goto Error;
            }
            else
            {
                if (!cmsWriteTag(hICC, cmsSigGreenTRCTag, new BoxPtr<ToneCurve>(TransferFunction[1]))) goto Error;
            }

            if (TransferFunction[2] == TransferFunction[0])
            {
                if (!cmsLinkTag(hICC, cmsSigBlueTRCTag, cmsSigRedTRCTag)) goto Error;
            }
            else
            {
                if (!cmsWriteTag(hICC, cmsSigBlueTRCTag, new BoxPtr<ToneCurve>(TransferFunction[2]))) goto Error;
            }
        }

        if (Primaries is not null && !cmsWriteTag(hICC, cmsSigChromaticityTag, new BoxPtr<CIExyYTRIPLE>(Primaries)))
            goto Error;

        return hICC;

    Error:
        if (hICC is not null)
            cmsCloseProfile(hICC);
        return null;
    }

    public static Profile? cmsCreateRGBProfile(in CIExyY* WhitePoint, in CIExyYTRIPLE* Primaries, ToneCurve** TransferFunction) =>
        cmsCreateRGBProfileTHR(null, WhitePoint, Primaries, TransferFunction);

    public static Profile? cmsCreateGrayProfileTHR(Context? ContextID, in CIExyY* WhitePoint, ToneCurve* TransferFunction)
    {
        CIEXYZ tmp;

        var hICC = cmsCreateProfilePlaceholder(ContextID);
        if (hICC is null)       // can't allocate
            return null;

        cmsSetProfileVersion(hICC, 4.3);

        cmsSetDeviceClass(hICC, cmsSigDisplayClass);
        cmsSetColorSpace(hICC, cmsSigGrayData);
        cmsSetPCS(hICC, cmsSigXYZData);
        cmsSetHeaderRenderingIntent(hICC, INTENT_PERCEPTUAL);

        // Implement profile using following tags:
        //
        //  1 cmsSigProfileDescriptionTag
        //  2 cmsSigMediaWhitePointTag
        //  3 cmsSigGrayTRCTag

        // This conforms a standard Gray DisplayProfile

        // Fill-in the tags

        var text = "gray built-in";
        if (!SetTextTags(hICC, (char*)&text)) goto Error;

        if (WhitePoint is not null)
        {
            cmsxyY2XYZ(&tmp, WhitePoint);
            if (!cmsWriteTag(hICC, cmsSigMediaWhitePointTag, new BoxPtr<CIEXYZ>(&tmp))) goto Error;
        }

        if (TransferFunction is not null && !cmsWriteTag(hICC, cmsSigGrayTRCTag, new BoxPtr<ToneCurve>(TransferFunction))) goto Error;

        return hICC;

    Error:
        if (hICC is not null)
            cmsCloseProfile(hICC);
        return null;
    }

    public static Profile? cmsCreateGrayProfile(in CIExyY* WhitePoint, ToneCurve* TransferFunction) =>
        cmsCreateGrayProfileTHR(null, WhitePoint, TransferFunction);

    public static Profile? cmsCreateLinearizationDeviceLinkTHR(Context? ContextID, Signature ColorSpace, ToneCurve** TransferFunctions)
    {
        var hICC = cmsCreateProfilePlaceholder(ContextID);
        if (hICC is null) return null;

        cmsSetProfileVersion(hICC, 4.3);

        cmsSetDeviceClass(hICC, cmsSigLinkClass);
        cmsSetColorSpace(hICC, ColorSpace);
        cmsSetPCS(hICC, ColorSpace);

        cmsSetHeaderRenderingIntent(hICC, INTENT_PERCEPTUAL);

        // Set up channels
        var nChannels = cmsChannelsOf(ColorSpace);

        // Creates a Pipeline with prelinearization step only
        var Pipeline = new BoxPtr<Pipeline>(cmsPipelineAlloc(ContextID, nChannels, nChannels));
        if (Pipeline is null) goto Error;

        // Copy tables to Pipeline
        if (!cmsPipelineInsertStage(Pipeline, StageLoc.AtBegin, cmsStageAllocToneCurves(ContextID, nChannels, TransferFunctions)))
            goto Error;

        // Create tags
        var text = "Linearization built-in"u8;
        var text8 = "Linearization built-in";
        if (!SetTextTags(hICC, (char*)&text8)) goto Error;
        if (!cmsWriteTag(hICC, cmsSigAToB0Tag, Pipeline)) goto Error;
        if (!SetSeqDescTag(hICC, (byte*)&text)) goto Error;

        // Pipeline is already on virtual profile
        cmsPipelineFree(Pipeline);

        // Ok, done
        return hICC;

    Error:
        cmsPipelineFree(Pipeline);
        if (hICC is not null)
            cmsCloseProfile(hICC);
        return null;
    }

    public static Profile cmsCreateLinearizationDeviceLink(Signature ColorSpace, ToneCurve** TransferFunctions) =>
        cmsCreateLinearizationDeviceLinkTHR(null, ColorSpace, TransferFunctions);

    private static bool InkLimitingSampler(in ushort* In, ushort* Out, void* Cargo)
    {
        // Ink-limiting algorithm
        //
        //  Sum = C + M + Y + K
        //  If Sum > InkLimit
        //        Ratio= 1 - (Sum - InkLimit) / (C + M + Y)
        //        if Ratio <0
        //              Ratio=0
        //        endif
        //     Else
        //         Ratio=1
        //     endif
        //
        //     C = Ratio * C
        //     M = Ratio * M
        //     Y = Ratio * Y
        //     K: Does not change

        var InkLimit = *(double*)Cargo;

        InkLimit *= 655.35;

        var SumCMY = (double)In[0] + In[1] + In[2];
        var SumCMYK = SumCMY + In[3];

        var Ratio = Math.Max(0, (SumCMYK > InkLimit) ? 1 - ((SumCMYK - InkLimit) / SumCMY) : 1);

        Out[0] = _cmsQuickSaturateWord(In[0] * Ratio);  // C
        Out[1] = _cmsQuickSaturateWord(In[1] * Ratio);  // M
        Out[2] = _cmsQuickSaturateWord(In[2] * Ratio);  // Y

        Out[3] = In[3];                                 // K (untouched)

        return true;
    }

    public static Profile? cmsCreateInkLimitingDeviceLinkTHR(Context? ContextID, Signature ColorSpace, double Limit)
    {
        if ((uint)ColorSpace is not cmsSigCmykData)
        {
            cmsSignalError(ContextID, cmsERROR_COLORSPACE_CHECK, "InkLimiting: Only CMYK currently supported");
            return null;
        }

        if (Limit is < 0 or > 400)
        {
            cmsSignalError(ContextID, cmsERROR_RANGE, "InkLimiting: Limit should be between 0..400");
            Limit = Math.Max(0, Math.Min(400, Limit));
        }

        var hICC = cmsCreateProfilePlaceholder(ContextID);
        if (hICC is null)          // can't allocate
            return null;

        cmsSetProfileVersion(hICC, 4.3);

        cmsSetDeviceClass(hICC, cmsSigLinkClass);
        cmsSetColorSpace(hICC, ColorSpace);
        cmsSetPCS(hICC, ColorSpace);

        cmsSetHeaderRenderingIntent(hICC, INTENT_PERCEPTUAL);

        // Creates a Pipeline with 3D grid only
        var LUT = new BoxPtr<Pipeline>(cmsPipelineAlloc(ContextID, 4, 4));
        if (LUT is null) goto Error;

        var nChannels = cmsChannelsOf(ColorSpace);

        var CLUT = cmsStageAllocCLut16bit(ContextID, 17, nChannels, nChannels, null);
        if (CLUT is null) goto Error;

        if (!cmsStageSampleCLut16bit(CLUT, InkLimitingSampler, &Limit, 0)) goto Error;

        if (!cmsPipelineInsertStage(LUT, StageLoc.AtBegin, _cmsStageAllocIdentityCurves(ContextID, nChannels)) ||
            !cmsPipelineInsertStage(LUT, StageLoc.AtEnd, CLUT) ||
            !cmsPipelineInsertStage(LUT, StageLoc.AtEnd, _cmsStageAllocIdentityCurves(ContextID, nChannels)))
        {
            goto Error;
        }

        // Create tags
        var text = "ink-limiting built-in"u8;
        var text8 = "ink-limiting built-in";
        if (!SetTextTags(hICC, (char*)&text8)) goto Error;
        if (!cmsWriteTag(hICC, cmsSigAToB0Tag, LUT)) goto Error;
        if (!SetSeqDescTag(hICC, (byte*)&text)) goto Error;

        // Pipeline is already on virtual profile
        cmsPipelineFree(LUT);

        // Ok, done
        return hICC;

    Error:
        if (LUT is not null)
            cmsPipelineFree(LUT);

        if (hICC is not null)
            cmsCloseProfile(hICC);

        return null;
    }

    public static Profile? cmsCreateInkLimitingDeviceLink(Signature ColorSpace, double Limit) =>
        cmsCreateInkLimitingDeviceLinkTHR(null, ColorSpace, Limit);

    public static Profile? cmsCreateLab2ProfileTHR(Context? ContextID, in CIExyY* WhitePoint)
    {
        BoxPtr<Pipeline>? LUT = null;

        var Profile = cmsCreateRGBProfileTHR(ContextID, WhitePoint is null ? cmsD50_xyY() : WhitePoint, null, null);
        if (Profile is null) return null;

        cmsSetProfileVersion(Profile, 2.1);

        cmsSetDeviceClass(Profile, cmsSigAbstractClass);
        cmsSetColorSpace(Profile, cmsSigLabData);
        cmsSetPCS(Profile, cmsSigLabData);

        var text = "Lab identity build-in";
        if (!SetTextTags(Profile, (char*)&text)) goto Error;

        // An identity LUT is all we need
        LUT = new(cmsPipelineAlloc(ContextID, 3, 3));
        if (LUT is null) goto Error;

        if (!cmsPipelineInsertStage(LUT, StageLoc.AtBegin, _cmsStageAllocIdentityCLut(ContextID, 3)))
            goto Error;

        if (!cmsWriteTag(Profile, cmsSigAToB0Tag, LUT)) goto Error;
        cmsPipelineFree(LUT);

        return Profile;

    Error:
        if (LUT is not null)
            cmsPipelineFree(LUT);
        if (Profile is not null)
            cmsCloseProfile(Profile);

        return null;
    }

    public static Profile? cmsCreateLab2Profile(in CIExyY* WhitePoint) =>
        cmsCreateLab2ProfileTHR(null, WhitePoint);

    public static Profile? cmsCreateLab4ProfileTHR(Context? ContextID, in CIExyY* WhitePoint)
    {
        BoxPtr<Pipeline>? LUT = null;

        var Profile = cmsCreateRGBProfileTHR(ContextID, WhitePoint is null ? cmsD50_xyY() : WhitePoint, null, null);
        if (Profile is null) return null;

        cmsSetProfileVersion(Profile, 4.3);

        cmsSetDeviceClass(Profile, cmsSigAbstractClass);
        cmsSetColorSpace(Profile, cmsSigLabData);
        cmsSetPCS(Profile, cmsSigLabData);

        var text = "Lab identity build-in";
        if (!SetTextTags(Profile, (char*)&text)) goto Error;

        // An empty LUT is all we need
        LUT = new(cmsPipelineAlloc(ContextID, 3, 3));
        if (LUT is null) goto Error;

        if (!cmsPipelineInsertStage(LUT, StageLoc.AtBegin, _cmsStageAllocIdentityCurves(ContextID, 3)))
            goto Error;

        if (!cmsWriteTag(Profile, cmsSigAToB0Tag, LUT)) goto Error;
        cmsPipelineFree(LUT);

        return Profile;

    Error:
        if (LUT is not null)
            cmsPipelineFree(LUT);
        if (Profile is not null)
            cmsCloseProfile(Profile);

        return null;
    }

    public static Profile? cmsCreateLab4Profile(in CIExyY* WhitePoint) =>
        cmsCreateLab4ProfileTHR(null, WhitePoint);

    public static Profile? cmsCreateXYZProfileTHR(Context? ContextID)
    {
        BoxPtr<Pipeline>? LUT = null;

        var Profile = cmsCreateRGBProfileTHR(ContextID, cmsD50_xyY(), null, null);
        if (Profile is null) return null;

        cmsSetProfileVersion(Profile, 4.3);

        cmsSetDeviceClass(Profile, cmsSigAbstractClass);
        cmsSetColorSpace(Profile, cmsSigXYZData);
        cmsSetPCS(Profile, cmsSigXYZData);

        var text = "XYZ identity build-in";
        if (!SetTextTags(Profile, (char*)&text)) goto Error;

        // An identity LUT is all we need
        LUT = new(cmsPipelineAlloc(ContextID, 3, 3));
        if (LUT is null) goto Error;

        if (!cmsPipelineInsertStage(LUT, StageLoc.AtBegin, _cmsStageAllocIdentityCurves(ContextID, 3)))
            goto Error;

        if (!cmsWriteTag(Profile, cmsSigAToB0Tag, LUT)) goto Error;
        cmsPipelineFree(LUT);

        return Profile;

    Error:
        if (LUT is not null)
            cmsPipelineFree(LUT);
        if (Profile is not null)
            cmsCloseProfile(Profile);

        return null;
    }

    public static Profile? cmsCreateXYZProfile() =>
        cmsCreateXYZProfileTHR(null);

    private static ToneCurve* Build_sRGBGamma(Context? ContextID)
    {
        var Parameters = stackalloc double[5]
        {
            2.4,
            1 / 1.055,
            0.055 / 1.055,
            1 / 12.92,
            0.04045,
        };

        return cmsBuildParametricToneCurve(ContextID, 4, Parameters);
    }

    public static Profile? cmsCreate_sRGBProfileTHR(Context? ContextID)
    {
        var D65 = new CIExyY() { x = 0.3127, y = 0.3290, Y = 1.0 };
        var Rec709Primaries = new CIExyYTRIPLE()
        {
            Red = new() { x = 0.6400, y = 0.3300, Y = 1.0 },
            Green = new() { x = 0.3000, y = 0.6000, Y = 1.0 },
            Blue = new() { x = 0.1500, y = 0.0600, Y = 1.0 },
        };
        var Gamma22 = stackalloc ToneCurve*[3];

        // cmsWhitePointFromTemp(&D65, 6504);
        Gamma22[0] = Gamma22[1] = Gamma22[2] = Build_sRGBGamma(ContextID);
        if (Gamma22[0] is null) return null;

        var hsRGB = cmsCreateRGBProfileTHR(ContextID, &D65, &Rec709Primaries, Gamma22);
        cmsFreeToneCurve(Gamma22[0]);
        if (hsRGB is null) return null;

        var text = "sRGB build-in";
        if (!SetTextTags(hsRGB, (char*)&text))
        {
            cmsCloseProfile(hsRGB);
            return null;
        }

        return hsRGB;
    }

    public static Profile? cmsCreate_sRGBProfile() =>
        cmsCreate_sRGBProfileTHR(null);

    private struct BCHSWADJUSTS
    {
        public double Brightness, Contrast, Hue, Saturation;
        public bool lAdjustWP;
        public CIEXYZ WPsrc, WPdest;
    }

    private static bool bchswSampler(in ushort* In, ushort* Out, void* Cargo)
    {
        CIELab LabIn, LabOut;
        CIELCh LChIn, LChOut;
        CIEXYZ XYZ;
        var bchsw = (BCHSWADJUSTS*)Cargo;

        cmsLabEncoded2Float(&LabIn, In);

        cmsLab2LCh(&LChIn, &LabIn);

        // Do some adjusts on LCh

        LChOut.L = (LChIn.L * bchsw->Contrast) + bchsw->Brightness;
        LChOut.C = LChIn.C + bchsw->Saturation;
        LChOut.h = LChIn.h + bchsw->Hue;

        cmsLCh2Lab(&LabOut, &LChOut);

        // Move white point in Lab
        if (bchsw->lAdjustWP)
        {
            cmsLab2XYZ(&bchsw->WPsrc, &XYZ, &LabOut);
            cmsXYZ2Lab(&bchsw->WPdest, &LabOut, &XYZ);
        }

        // Back to encoded
        cmsFloat2LabEncoded(Out, &LabOut);
        return true;
    }

    public static Profile? cmsCreateBCHSWabstractProfileTHR(
        Context? ContextID,
        uint nLUTPoints,
        double Bright,
        double Contrast,
        double Hue,
        double Saturation,
        uint TempSrc,
        uint TempDest)
    {
        var Dimensions = stackalloc uint[MAX_INPUT_DIMENSIONS];
        BCHSWADJUSTS bchsw;
        CIExyY WhitePnt;
        BoxPtr<Pipeline>? Pipeline = null;

        bchsw.Brightness = Bright;
        bchsw.Contrast = Contrast;
        bchsw.Hue = Hue;
        bchsw.Saturation = Saturation;
        if (TempSrc == TempDest)
        {
            bchsw.lAdjustWP = false;
        }
        else
        {
            bchsw.lAdjustWP = true;
            cmsWhitePointFromTemp(&WhitePnt, TempSrc);
            cmsxyY2XYZ(&bchsw.WPsrc, &WhitePnt);
            cmsWhitePointFromTemp(&WhitePnt, TempDest);
            cmsxyY2XYZ(&bchsw.WPdest, &WhitePnt);
        }

        var hICC = cmsCreateProfilePlaceholder(ContextID);
        if (hICC is null) return null;

        cmsSetDeviceClass(hICC, cmsSigAbstractClass);
        cmsSetColorSpace(hICC, cmsSigLabData);
        cmsSetPCS(hICC, cmsSigLabData);

        cmsSetHeaderRenderingIntent(hICC, INTENT_PERCEPTUAL);

        // Creates a Pipeline with 3D grid only
        Pipeline = new(cmsPipelineAlloc(ContextID, 3, 3));
        if (Pipeline is null)
        {
            cmsCloseProfile(hICC);
            return null;
        }

        for (var i = 0; i < MAX_INPUT_DIMENSIONS; i++) Dimensions[i] = nLUTPoints;
        var CLUT = cmsStageAllocCLut16bitGranular(ContextID, Dimensions, 3, 3, null);
        if (CLUT is null) goto Error;

        if (!cmsStageSampleCLut16bit(CLUT, bchswSampler, &bchsw, SamplerFlag.None))
            goto Error;

        if (!cmsPipelineInsertStage(Pipeline, StageLoc.AtEnd, CLUT))
            goto Error;

        // Create tags
        var text = "BCHS build-in";
        if (!SetTextTags(hICC, (char*)&text)) goto Error;

        if (!cmsWriteTag(hICC, cmsSigMediaWhitePointTag, new BoxPtr<CIEXYZ>(cmsD50_XYZ()))) goto Error;
        if (!cmsWriteTag(hICC, cmsSigAToB0Tag, Pipeline)) goto Error;

        // Pipeline is already on virtual profile
        cmsPipelineFree(Pipeline);

        // Ok, done
        return hICC;

    Error:
        cmsPipelineFree(Pipeline);
        cmsCloseProfile(hICC);

        return null;
    }

    public static Profile? cmsCreateBCHSWabstractProfile(
        uint nLUTPoints,
        double Bright,
        double Contrast,
        double Hue,
        double Saturation,
        uint TempSrc,
        uint TempDest) =>
        cmsCreateBCHSWabstractProfileTHR(null, nLUTPoints, Bright, Contrast, Hue, Saturation, TempSrc, TempDest);

    public static Profile? cmsCreateNULLProfileTHR(Context? ContextID)
    {
        var EmptyTab = stackalloc ToneCurve*[3];
        var Zero = stackalloc ushort[2] { 0, 0 };
        var PickLstarMatrix = stackalloc double[] { 1, 0, 0 };
        BoxPtr<Pipeline>? LUT = null;

        var Profile = cmsCreateProfilePlaceholder(ContextID);
        if (Profile is null) return null;

        cmsSetProfileVersion(Profile, 4.3);

        var text = "NULL profile build-in";
        if (!SetTextTags(Profile, (char*)&text)) goto Error;

        cmsSetDeviceClass(Profile, cmsSigOutputClass);
        cmsSetColorSpace(Profile, cmsSigGrayData);
        cmsSetPCS(Profile, cmsSigLabData);

        // Create a valid ICC 4 structure
        LUT = new(cmsPipelineAlloc(ContextID, 3, 1));
        if (LUT is null) goto Error;

        EmptyTab[0] = EmptyTab[1] = EmptyTab[2] = cmsBuildTabulatedToneCurve16(ContextID, 2, Zero);
        var PostLin = cmsStageAllocToneCurves(ContextID, 3, EmptyTab);
        var OutLin = cmsStageAllocToneCurves(ContextID, 1, EmptyTab);
        cmsFreeToneCurve(EmptyTab[0]);

        if (!cmsPipelineInsertStage(LUT, StageLoc.AtEnd, PostLin))
            goto Error;

        if (!cmsPipelineInsertStage(LUT, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 1, 3, PickLstarMatrix, null)))
            goto Error;

        if (!cmsPipelineInsertStage(LUT, StageLoc.AtEnd, OutLin))
            goto Error;

        if (!cmsWriteTag(Profile, cmsSigBToA0Tag, LUT)) goto Error;
        if (!cmsWriteTag(Profile, cmsSigMediaWhitePointTag, new BoxPtr<CIEXYZ>(cmsD50_XYZ()))) goto Error;

        cmsPipelineFree(LUT);
        return Profile;

    Error:
        if (LUT is not null)
            cmsPipelineFree(LUT);
        if (Profile is not null)
            cmsCloseProfile(Profile);

        return null;
    }

    public static Profile? cmsCreateNULLProfile() =>
        cmsCreateNULLProfileTHR(null);

    private static bool IsPCS(Signature ColorSpace) =>
        (uint)ColorSpace is cmsSigXYZData or cmsSigLabData;

    private static void FixColorSpaces(Profile Profile, Signature ColorSpace, Signature PCS, uint dwFlags)
    {
        var (cls, cp, pcs) = ((dwFlags & cmsFLAGS_GUESSDEVICECLASS) is not 0, IsPCS(ColorSpace), IsPCS(PCS)) switch
        {
            (true, true, true) => (cmsSigAbstractClass, ColorSpace, PCS),
            (true, true, false) => (cmsSigOutputClass, PCS, ColorSpace),
            (true, false, true) => (cmsSigInputClass, ColorSpace, PCS),
            (false, _, _) => (cmsSigLinkClass, ColorSpace, PCS),
        };

        cmsSetDeviceClass(Profile, cls);
        cmsSetColorSpace(Profile, cp);
        cmsSetPCS(Profile, pcs);
    }

    private static Profile? CreateNamedColorDevicelink(Transform* xform)
    {
        var v = xform;
        Profile? hICC = null;
        BoxPtr<NamedColorList>? nc2 = null, Original = null;

        // Create an empty placeholder
        hICC = cmsCreateProfilePlaceholder(v->ContextID);
        if (hICC is null) return null;

        // Critical information
        cmsSetDeviceClass(hICC, cmsSigNamedColorClass);
        cmsSetColorSpace(hICC, v->ExitColorSpace);
        cmsSetPCS(hICC, cmsSigLabData);

        // Tag profile with information
        var text = "Named color devicelink";
        if (!SetTextTags(hICC, (char*)&text)) goto Error;

        Original = cmsGetNamedColorList(xform);
        if (Original is null) goto Error;

        var nColors = cmsNamedColorCount(Original);
        nc2 = new(cmsDupNamedColorList(Original));
        if (nc2 is null) goto Error;

        // Colorant count now depends on the output space
        nc2.Ptr->ColorantCount = cmsPipelineOutputChannels(v->Lut);

        // Make sure we have proper formatters
        cmsChangeBuffersFormat(xform, TYPE_NAMED_COLOR_INDEX,
            FLOAT_SH(0) | COLORSPACE_SH((uint)_cmsLCMScolorSpace(v->ExitColorSpace)) | BYTES_SH(2) | CHANNELS_SH(cmsChannelsOf(v->ExitColorSpace)));

        // Apply the transform to colorants.
        for (var i = 0; i < nColors; i++)
            cmsDoTransform(xform, &i, nc2.Ptr->List[i].DeviceColorant, 1);

        if (!cmsWriteTag(hICC, cmsSigNamedColor2Tag, nc2)) goto Error;
        cmsFreeNamedColorList(nc2);

        return hICC;

    Error:
        if (hICC is not null) cmsCloseProfile(hICC);
        return null;
    }

    private struct AllowedLUT
    {
        public bool IsV4;
        public Signature RequiredTag;
        public Signature LutType;
        public int nTypes;
        public fixed uint MpeTypes[5];

        public AllowedLUT(bool isV4, Signature requiredTag, Signature lutType, params uint[] mpeTypes)
        {
            IsV4 = isV4;
            RequiredTag = requiredTag;
            LutType = lutType;
            nTypes = mpeTypes.Length;
            for (var i = 0; i < mpeTypes.Length && i < 5; i++)
            {
                MpeTypes[i] = mpeTypes[i];
            }
        }
    }

    private static readonly AllowedLUT* AllowedLUTTypes;

    private const uint SIZE_OF_ALLOWED_LUT = 11;

    private static bool CheckOne(in AllowedLUT* Tab, in Pipeline* Lut)
    {
        Stage? mpe;
        int n;

        for (n = 0, mpe = Lut->Elements; mpe is not null; mpe = mpe.Next, n++)
        {
            if (n > Tab->nTypes) return false;
            if (cmsStageType(mpe) != Tab->MpeTypes[n]) return false;
        }

        return n == Tab->nTypes;
    }

    private static AllowedLUT* FindCombination(in Pipeline* Lut, bool IsV4, Signature DestinationTag)
    {
        for (var n = 0u; n < SIZE_OF_ALLOWED_LUT; n++)
        {
            var Tab = (AllowedLUT*)Unsafe.AsPointer(ref AllowedLUTTypes[n]);

            if (IsV4 ^ Tab->IsV4) continue;
            if (((uint)Tab->RequiredTag is not 0) && (Tab->RequiredTag != DestinationTag)) continue;

            if (CheckOne(Tab, Lut)) return Tab;
        }

        return null;
    }

    public static Profile cmsTransform2DeviceLink(Transform* hTransform, double Version, uint dwFlags)
    {
        Profile? Profile = null;
        var xform = hTransform;
        BoxPtr<Pipeline>? LUT = null;
        var ContextID = cmsGetTransformContextID(hTransform);

        _cmsAssert(hTransform);

        // Get the first mpe to check for named color
        var mpe = cmsPipelineGetPtrToFirstStage(xform->Lut);

        // Check if it is a named color transform
        if (mpe is not null)
        {
            if ((uint)cmsStageType(mpe) is cmsSigNamedColorElemType)
                return CreateNamedColorDevicelink(hTransform);
        }

        // First thing to do is to get a copy of the transformation
        LUT = new(cmsPipelineDup(xform->Lut));
        if (LUT is null) return null;

        // Time to fix the Lab2/Lab4 issue.
        if (((uint)xform->EntryColorSpace is cmsSigLabData) && (Version < 4.0))
        {
            if (!cmsPipelineInsertStage(LUT, StageLoc.AtBegin, _cmsStageAllocLabV2ToV4curves(ContextID)))
                goto Error;
        }

        // On the output side too. Note that due to V2/V4 PCS encoding on lab we cannot fix white misalignments
        if (((uint)xform->ExitColorSpace) is cmsSigLabData && (Version < 4.0))
        {
            dwFlags |= cmsFLAGS_NOWHITEONWHITEFIXUP;
            if (!cmsPipelineInsertStage(LUT, StageLoc.AtEnd, _cmsStageAllocLabV4ToV2(ContextID)))
                goto Error;
        }

        Profile = cmsCreateProfilePlaceholder(ContextID);
        if (Profile is null) goto Error;       // Can't allocate

        cmsSetProfileVersion(Profile, Version);

        FixColorSpaces(Profile, xform->EntryColorSpace, xform->ExitColorSpace, dwFlags);

        // Optimize the LUT and precalculate a devicelink
        var ChansIn = cmsChannelsOf(xform->EntryColorSpace);
        var ChansOut = cmsChannelsOf(xform->ExitColorSpace);

        var ColorSpaceBitsIn = _cmsLCMScolorSpace(xform->EntryColorSpace);
        var ColorSpaceBitsOut = _cmsLCMScolorSpace(xform->ExitColorSpace);

        var FrmIn = COLORSPACE_SH((uint)ColorSpaceBitsIn) | CHANNELS_SH(ChansIn) | BYTES_SH(2);
        var FrmOut = COLORSPACE_SH((uint)ColorSpaceBitsOut) | CHANNELS_SH(ChansOut) | BYTES_SH(2);

        var deviceClass = cmsGetDeviceClass(Profile);

        var DestinationTag = (Signature)(((uint)deviceClass is cmsSigOutputClass)
            ? cmsSigBToA0Tag
            : cmsSigAToB0Tag);

        // Check if the profile/version can store the result
        var AllowedLUT = ((dwFlags & cmsFLAGS_FORCE_CLUT) is 0)
            ? FindCombination(LUT, Version >= 4.0, DestinationTag)
            : null;

        if (AllowedLUT is null)
        {
            // Try to optimize
            fixed (Pipeline** ptr = &LUT.Ptr)
                _cmsOptimizePipeline(ContextID, ptr, xform->RenderingIntent, &FrmIn, &FrmOut, &dwFlags);
            AllowedLUT = FindCombination(LUT, Version >= 4.0, DestinationTag);
        }

        // If no way, then force CLUT that for sure can be written
        if (AllowedLUT is null)
        {
            dwFlags |= cmsFLAGS_FORCE_CLUT;
            fixed (Pipeline** ptr = &LUT.Ptr)
                _cmsOptimizePipeline(ContextID, ptr, xform->RenderingIntent, &FrmIn, &FrmOut, &dwFlags);

            // Put identity curves if needed
            var FirstStage = cmsPipelineGetPtrToFirstStage(LUT);
            if (FirstStage is not null && (uint)FirstStage.Type is cmsSigCurveSetElemType)
                if (!cmsPipelineInsertStage(LUT, StageLoc.AtBegin, _cmsStageAllocIdentityCurves(ContextID, ChansIn)))
                    goto Error;

            var LastStage = cmsPipelineGetPtrToLastStage(LUT);
            if (LastStage is not null && (uint)LastStage.Type is cmsSigCurveSetElemType)
                if (!cmsPipelineInsertStage(LUT, StageLoc.AtEnd, _cmsStageAllocIdentityCurves(ContextID, ChansOut)))
                    goto Error;

            AllowedLUT = FindCombination(LUT, Version >= 4.0, DestinationTag);
        }

        // Something is wrong...
        if (AllowedLUT is null)
            goto Error;

        if ((dwFlags & cmsFLAGS_8BITS_DEVICELINK) is not 0)
            cmsPipelineSetSaveAs8bitsFlag(LUT, true);

        // Tag profile with information
        var devicelink = "devicelink";
        if (!SetTextTags(Profile, (char*)&devicelink))
            goto Error;

        // Store result
        if (!cmsWriteTag(Profile, DestinationTag, LUT))
            goto Error;

        if (xform->InputColorant is not null)
            if (!cmsWriteTag(Profile, cmsSigColorantTableTag, new BoxPtr<NamedColorList>(xform->InputColorant))) goto Error;

        if (xform->OutputColorant is not null)
            if (!cmsWriteTag(Profile, cmsSigColorantTableOutTag, new BoxPtr<NamedColorList>(xform->OutputColorant))) goto Error;

        if (((uint)deviceClass is cmsSigLinkClass) && (xform->Sequence is not null))
            if (!_cmsWriteProfileSequence(Profile, xform->Sequence)) goto Error;

        // Set the white point
        if ((uint)deviceClass is cmsSigInputClass)
        {
            if (!cmsWriteTag(Profile, cmsSigMediaWhitePointTag, new BoxPtr<CIEXYZ>(&xform->EntryWhitePoint))) goto Error;
        }
        else
        {
            if (!cmsWriteTag(Profile, cmsSigMediaWhitePointTag, new BoxPtr<CIEXYZ>(&xform->ExitWhitePoint))) goto Error;
        }

        // Per 7.2.14 in spec 4.3
        cmsSetHeaderRenderingIntent(Profile, xform->RenderingIntent);

        cmsPipelineFree(LUT);
        return Profile;

    Error:
        if (LUT is not null) cmsPipelineFree(LUT);
        cmsCloseProfile(Profile);
        return null;
    }
}
