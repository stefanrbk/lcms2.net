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

internal static unsafe partial class Testbed
{
    private static HPROFILE Create_AboveRGB()
    {
        var Curve = stackalloc ToneCurve*[3];
        CIExyYTRIPLE Primaries = new()
        {
            Red = new()
            {
                x = 0.64,
                y = 0.33,
                Y = 1
            },
            Green = new()
            {
                x = 0.21,
                y = 0.71,
                Y = 1
            },
            Blue = new()
            {
                x = 0.15,
                y = 0.06,
                Y = 1
            },
        };
        CIExyY D65;

        Curve[0] = Curve[1] = Curve[2] = cmsBuildGamma(DbgThread(), 2.19921875);

        cmsWhitePointFromTemp(&D65, 6504);
        var hProfile = cmsCreateRGBProfileTHR(DbgThread(), &D65, &Primaries, Curve);
        cmsFreeToneCurve(Curve[0]);

        return hProfile;
    }

    private static HPROFILE Create_Gray22()
    {
        var Curve = cmsBuildGamma(DbgThread(), 2.2);
        if (Curve is null) return null;

        var hProfile = cmsCreateGrayProfileTHR(DbgThread(), cmsD50_xyY(), Curve);
        cmsFreeToneCurve(Curve);

        return hProfile;
    }

    private static HPROFILE Create_Gray30()
    {
        var Curve = cmsBuildGamma(DbgThread(), 3.0);
        if (Curve is null) return null;

        var hProfile = cmsCreateGrayProfileTHR(DbgThread(), cmsD50_xyY(), Curve);
        cmsFreeToneCurve(Curve);

        return hProfile;
    }

    private static HPROFILE Create_GrayLab()
    {
        var Curve = cmsBuildGamma(DbgThread(), 1.0);
        if (Curve is null) return null;

        var hProfile = cmsCreateGrayProfileTHR(DbgThread(), cmsD50_xyY(), Curve);
        cmsFreeToneCurve(Curve);

        cmsSetPCS(hProfile, cmsSigLabData);
        return hProfile;
    }

    private static HPROFILE Create_CMYK_DeviceLink()
    {
        var Tab = stackalloc ToneCurve*[4];
        var Curve = cmsBuildGamma(DbgThread(), 3.0);
        if (Curve is null) return null;

        Tab[0] = Tab[1] = Tab[2] = Tab[3] = Curve;

        var hProfile = cmsCreateLinearizationDeviceLinkTHR(DbgThread(), cmsSigCmykData, Tab);
        cmsFreeToneCurve(Curve);

        return hProfile;
    }

    private struct FakeCMYKParams
    {
        public Transform* hLab2sRGB;
        public Transform* sRGB2Lab;
        public Transform* hIlimit;
    }

    private static double Clip(double v) =>
        Math.Max(Math.Min(v, 1), 0);

    private static bool ForwardSampler(in ushort* In, ushort* Out, void* Cargo)
    {
        var rgb = stackalloc double[3];
        var cmyk = stackalloc double[4];

        var p = (FakeCMYKParams*)Cargo;

        cmsDoTransform(p->hLab2sRGB, In, rgb, 1);

        var c = 1 - rgb[0];
        var m = 1 - rgb[1];
        var y = 1 - rgb[2];

        var k = c < m ? cmsmin(c, y) : cmsmin(m, y);

        // NONSENSE WARNING!: I'm doing this just because this is a test
        // profile that may have ink limit up to 400%. There is no UCR here
        // so the profile is basically useless for anything but testing.

        cmyk[0] = c;
        cmyk[1] = m;
        cmyk[2] = y;
        cmyk[3] = k;

        cmsDoTransform(p->hIlimit, cmyk, Out, 1);

        return true;
    }

    private static bool ReverseSampler(in ushort* In, ushort* Out, void* Cargo)
    {
        var rgb = stackalloc double[3];

        var p = (FakeCMYKParams*)Cargo;

        var c = In[0] / 65535.0;
        var m = In[1] / 65535.0;
        var y = In[2] / 65535.0;
        var k = In[3] / 65535.0;

        if (k is 0)
        {
            rgb[0] = Clip(1 - c);
            rgb[1] = Clip(1 - m);
            rgb[2] = Clip(1 - y);
        }
        else
        {
            if (k is 1)
            {
                rgb[0] = rgb[1] = rgb[2] = 0;
            }
            else
            {
                rgb[0] = Clip((1 - c) * (1 - k));
                rgb[1] = Clip((1 - m) * (1 - k));
                rgb[2] = Clip((1 - y) * (1 - k));
            }
        }

        cmsDoTransform(p->sRGB2Lab, rgb, Out, 1);

        return true;
    }

    private static HPROFILE CreateFakeCMYK(double InkLimit, bool lUseAboveRGB)
    {
        FakeCMYKParams p;

        var hsRGB = lUseAboveRGB
            ? Create_AboveRGB()
            : cmsCreate_sRGBProfile();

        var hLab = cmsCreateLab4Profile(null);
        var hLimit = cmsCreateInkLimitingDeviceLink(cmsSigCmykData, InkLimit);

        var cmykfrm = FLOAT_SH(1) | BYTES_SH(0) | CHANNELS_SH(4);
        p.hLab2sRGB = cmsCreateTransform(hLab, TYPE_Lab_16, hsRGB, TYPE_RGB_DBL, INTENT_PERCEPTUAL, cmsFLAGS_NOOPTIMIZE | cmsFLAGS_NOCACHE);
        p.sRGB2Lab = cmsCreateTransform(hsRGB, TYPE_RGB_DBL, hsRGB, TYPE_Lab_16, INTENT_PERCEPTUAL, cmsFLAGS_NOOPTIMIZE | cmsFLAGS_NOCACHE);
        p.hIlimit = cmsCreateTransform(hLimit, cmykfrm, null, TYPE_CMYK_16, INTENT_PERCEPTUAL, cmsFLAGS_NOOPTIMIZE | cmsFLAGS_NOCACHE);

        cmsCloseProfile(hLab); cmsCloseProfile(hsRGB); cmsCloseProfile(hLimit);

        var ContextID = DbgThread();
        var hICC = cmsCreateProfilePlaceholder(ContextID);
        if (hICC is null) return null;

        cmsSetProfileVersion(hICC, 4.3);

        cmsSetDeviceClass(hICC, cmsSigOutputClass);
        cmsSetColorSpace(hICC, cmsSigCmykData);
        cmsSetPCS(hICC, cmsSigLabData);

        var BToA0 = cmsPipelineAlloc(ContextID, 3, 4);
        if (BToA0 is null) return null;
        var CLUT = cmsStageAllocCLut16bit(ContextID, 17, 3, 4, null);
        if (CLUT is null) return null;
        if (!cmsStageSampleCLut16bit(CLUT, ForwardSampler, &p, 0)) return null;

        cmsPipelineInsertStage(BToA0, StageLoc.AtBegin, _cmsStageAllocIdentityCurves(ContextID, 3));
        cmsPipelineInsertStage(BToA0, StageLoc.AtEnd, CLUT);
        cmsPipelineInsertStage(BToA0, StageLoc.AtEnd, _cmsStageAllocIdentityCurves(ContextID, 4));

        if (!cmsWriteTag(hICC, cmsSigBToA0Tag, BToA0)) return null;
        cmsPipelineFree(BToA0);

        var AToB0 = cmsPipelineAlloc(ContextID, 4, 3);
        if (AToB0 is null) return null;
        CLUT = cmsStageAllocCLut16bit(ContextID, 17, 4, 3, null);
        if (CLUT is null) return null;
        if (!cmsStageSampleCLut16bit(CLUT, ReverseSampler, &p, 0)) return null;

        cmsPipelineInsertStage(AToB0, StageLoc.AtBegin, _cmsStageAllocIdentityCurves(ContextID, 4));
        cmsPipelineInsertStage(AToB0, StageLoc.AtEnd, CLUT);
        cmsPipelineInsertStage(AToB0, StageLoc.AtEnd, _cmsStageAllocIdentityCurves(ContextID, 3));

        if (!cmsWriteTag(hICC, cmsSigAToB0Tag, AToB0)) return null;
        cmsPipelineFree(AToB0);

        cmsDeleteTransform(p.hLab2sRGB);
        cmsDeleteTransform(p.sRGB2Lab);
        cmsDeleteTransform(p.hIlimit);

        cmsLinkTag(hICC, cmsSigAToB1Tag, cmsSigAToB0Tag);
        cmsLinkTag(hICC, cmsSigAToB2Tag, cmsSigAToB0Tag);
        cmsLinkTag(hICC, cmsSigBToA1Tag, cmsSigBToA0Tag);
        cmsLinkTag(hICC, cmsSigBToA2Tag, cmsSigBToA0Tag);

        return hICC;
    }

    private static bool OneVirtual(HPROFILE h, string SubTestTxt, string FileName)
    {
        SubTest(SubTestTxt);
        if (h is null) return false;

        if (!cmsSaveProfileToFile(h, FileName)) return false;
        cmsCloseProfile(h);

        h = cmsOpenProfileFromFile(FileName, "r");
        if (h is null) return false;

        cmsCloseProfile(h);
        return true;
    }

    internal static bool CreateTestProfiles()
    {
        var h = cmsCreate_sRGBProfileTHR(DbgThread());
        if (!OneVirtual(h, "sRGB profile", "sRGBlcms2.icc")) return false;

        // ----

        h = Create_AboveRGB();
        if (!OneVirtual(h, "aRGB profile", "aRGBlcms2.icc")) return false;

        // ----

        h = Create_Gray22();
        if (!OneVirtual(h, "Gray profile", "graylcms2.icc")) return false;

        // ----

        h = Create_Gray30();
        if (!OneVirtual(h, "Gray 3.0 profile", "gray3lcms2.icc")) return false;

        // ----

        h = Create_GrayLab();
        if (!OneVirtual(h, "Gray Lab profile", "glablcms2.icc")) return false;

        // ----

        h = Create_CMYK_DeviceLink();
        if (!OneVirtual(h, "Linearization profile", "linlcms2.icc")) return false;

        // ----

        h = cmsCreateInkLimitingDeviceLinkTHR(DbgThread(), cmsSigCmykData, 150);
        if (!OneVirtual(h, "Ink-limiting profile", "limitlcms2.icc")) return false;

        // ----

        h = cmsCreateLab2ProfileTHR(DbgThread(), null);
        if (!OneVirtual(h, "Lab 2 identity profile", "labv2lcms2.icc")) return false;

        // ----

        h = cmsCreateLab4ProfileTHR(DbgThread(), null);
        if (!OneVirtual(h, "Lab 4 identity profile", "labv4lcms2.icc")) return false;

        // ----

        h = cmsCreateXYZProfileTHR(DbgThread());
        if (!OneVirtual(h, "XYZ identity profile", "xyzlcms2.icc")) return false;

        // ----

        h = cmsCreateNULLProfileTHR(DbgThread());
        if (!OneVirtual(h, "NULL profile", "nulllcms2.icc")) return false;

        // ----

        h = cmsCreateBCHSWabstractProfileTHR(DbgThread(), 17, 0, 0, 0, 0, 5000, 6000);
        if (!OneVirtual(h, "BCHS profile", "bchslcms2.icc")) return false;

        // ----

        h = CreateFakeCMYK(300, false);
        if (!OneVirtual(h, "Fake CMYK profile", "lcms2cmyk.icc")) return false;

        // ----

        h = cmsCreateBCHSWabstractProfileTHR(DbgThread(), 17, 0, 1.2, 0, 3, 5000, 5000);
        if (!OneVirtual(h, "Brightness", "brightness.icc")) return false;

        return true;
    }

    internal static void RemoveTestProfiles()
    {
        File.Delete("sRGBlcms2.icc");
        File.Delete("aRGBlcms2.icc");
        File.Delete("graylcms2.icc");
        File.Delete("gray3lcms2.icc");
        File.Delete("linlcms2.icc");
        File.Delete("limitlcms2.icc");
        File.Delete("labv2lcms2.icc");
        File.Delete("labv4lcms2.icc");
        File.Delete("xyzlcms2.icc");
        File.Delete("nulllcms2.icc");
        File.Delete("bchslcms2.icc");
        File.Delete("lcms2cmyk.icc");
        File.Delete("glablcms2.icc");
        File.Delete("lcms2link.icc");
        File.Delete("lcms2link2.icc");
        File.Delete("brightness.icc");
    }
}
