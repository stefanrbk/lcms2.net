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

using lcms2.state;
using lcms2.types;

using System.Numerics;

namespace lcms2.tests;
internal static class TestHelpers
{
    public static T cmsmin<T>(T a, T b) where T : IComparisonOperators<T, T, bool> =>
        (a < b) ? a : b;

    public static void CheckFixed15_16(string? title, double @in, double @out) =>
        CheckValue(title, @in, @out, 1.0 / 65535.0);

    public static void CheckValue(string? title, double @in, double @out, double max) =>
        Assert.That(@in, Is.EqualTo(@out).Within(max), title);

    public static void CheckWord(string? title, ushort @in, ushort @out) =>
        Assert.That(@in, Is.EqualTo(@out), title);

    public static void CheckWord(string? title, ushort @in, ushort @out, ushort maxErr) =>
        Assert.That(@in, Is.EqualTo(@out).Within(maxErr), title);

    public static byte[]? sRGBProfile;
    public static byte[]? aRGBProfile;
    public static byte[]? GrayProfile;
    public static byte[]? Gray3Profile;
    public static byte[]? GrayLabProfile;
    public static byte[]? LinProfile;
    public static byte[]? LimitProfile;
    public static byte[]? Labv2Profile;
    public static byte[]? Labv4Profile;
    public static byte[]? XYZProfile;
    public static byte[]? nullProfile;
    public static byte[]? BCHSProfile;
    public static byte[]? FakeCMYKProfile;
    public static byte[]? BrightnessProfile;

    public static Profile? Create_AboveRGB()
    {
        var Curve = new ToneCurve[3];
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

        Curve[0] = Curve[1] = Curve[2] = cmsBuildGamma(null, 2.19921875)!;

        var D65 = cmsWhitePointFromTemp(6504);
        var Profile = cmsCreateRGBProfileTHR(null, D65, Primaries, Curve);
        cmsFreeToneCurve(Curve[0]);

        return Profile;
    }

    public static Profile? Create_Gray22()
    {
        var Curve = cmsBuildGamma(null, 2.2);
        if (Curve is null) return null;

        var Profile = cmsCreateGrayProfileTHR(null, D50xyY, Curve);
        cmsFreeToneCurve(Curve);

        return Profile;
    }

    public static Profile? Create_Gray30()
    {
        var Curve = cmsBuildGamma(null, 3.0);
        if (Curve is null) return null;

        var Profile = cmsCreateGrayProfileTHR(null, D50xyY, Curve);
        cmsFreeToneCurve(Curve);

        return Profile;
    }

    public static Profile? Create_GrayLab()
    {
        var Curve = cmsBuildGamma(null, 1.0);
        if (Curve is null) return null;

        var Profile = cmsCreateGrayProfileTHR(null, D50xyY, Curve);
        cmsFreeToneCurve(Curve);

        cmsSetPCS(Profile, cmsSigLabData);
        return Profile;
    }

    public static Profile? Create_CMYK_DeviceLink()
    {
        var Tab = new ToneCurve[4];
        var Curve = cmsBuildGamma(null, 3.0);
        if (Curve is null) return null;

        Tab[0] = Tab[1] = Tab[2] = Tab[3] = Curve;

        var Profile = cmsCreateLinearizationDeviceLinkTHR(null, cmsSigCmykData, Tab);
        cmsFreeToneCurve(Curve);

        return Profile;
    }

    private struct FakeCMYKParams
    {
        public Transform hLab2sRGB;
        public Transform sRGB2Lab;
        public Transform hIlimit;
    }

    private static double Clip(double v) =>
        Math.Max(Math.Min(v, 1), 0);

    private static bool ForwardSampler(ReadOnlySpan<ushort> In, Span<ushort> Out, object? Cargo)
    {
        Span<double> rgb = stackalloc double[3];
        Span<double> cmyk = stackalloc double[4];

        if (Cargo is not Box<FakeCMYKParams> p)
            return false;

        cmsDoTransform(p.Value.hLab2sRGB, In, rgb, 1);

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

        cmsDoTransform(p.Value.hIlimit, cmyk, Out, 1);

        return true;
    }

    private static bool ReverseSampler(ReadOnlySpan<ushort> In, Span<ushort> Out, object? Cargo)
    {
        Span<double> rgb = stackalloc double[3];

        if (Cargo is not Box<FakeCMYKParams> p)
            return false;

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

        cmsDoTransform(p.Value.sRGB2Lab, rgb, Out, 1);

        return true;
    }

    public static Profile? CreateFakeCMYK(double InkLimit, bool lUseAboveRGB)
    {
        FakeCMYKParams p;

        var hsRGB = lUseAboveRGB
            ? Create_AboveRGB()
            : cmsCreate_sRGBProfile();

        var hLab = cmsCreateLab4Profile(null);
        var hLimit = cmsCreateInkLimitingDeviceLink(cmsSigCmykData, InkLimit);

        var cmykfrm = FLOAT_SH(1) | BYTES_SH(0) | CHANNELS_SH(4);
        p.hLab2sRGB = cmsCreateTransform(hLab, TYPE_Lab_16, hsRGB, TYPE_RGB_DBL, INTENT_PERCEPTUAL, cmsFLAGS_NOOPTIMIZE | cmsFLAGS_NOCACHE);
        p.sRGB2Lab = cmsCreateTransform(hsRGB, TYPE_RGB_DBL, hLab, TYPE_Lab_16, INTENT_PERCEPTUAL, cmsFLAGS_NOOPTIMIZE | cmsFLAGS_NOCACHE);
        p.hIlimit = cmsCreateTransform(hLimit, cmykfrm, null, TYPE_CMYK_16, INTENT_PERCEPTUAL, cmsFLAGS_NOOPTIMIZE | cmsFLAGS_NOCACHE);
        var pPtr = new Box<FakeCMYKParams>(p);

        cmsCloseProfile(hLab); cmsCloseProfile(hsRGB); cmsCloseProfile(hLimit);

        Context? ContextID = null;
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
        if (!cmsStageSampleCLut16bit(CLUT, ForwardSampler, pPtr, 0)) return null;

        cmsPipelineInsertStage(BToA0, StageLoc.AtBegin, _cmsStageAllocIdentityCurves(ContextID, 3));
        cmsPipelineInsertStage(BToA0, StageLoc.AtEnd, CLUT);
        cmsPipelineInsertStage(BToA0, StageLoc.AtEnd, _cmsStageAllocIdentityCurves(ContextID, 4));

        if (!cmsWriteTag(hICC, cmsSigBToA0Tag, BToA0)) return null;
        cmsPipelineFree(BToA0);

        var AToB0 = cmsPipelineAlloc(ContextID, 4, 3);
        if (AToB0 is null) return null;
        CLUT = cmsStageAllocCLut16bit(ContextID, 17, 4, 3, null);
        if (CLUT is null) return null;
        if (!cmsStageSampleCLut16bit(CLUT, ReverseSampler, pPtr, 0)) return null;

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
}
