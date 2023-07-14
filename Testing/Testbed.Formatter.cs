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

namespace lcms2.testbed;

internal static unsafe partial class Testbed
{
    private static bool FormatterFailed;

    private static void C16(string a)
    {
        var field = typeof(Lcms2).GetField(a);
        var value = (uint)field!.GetValue(null)!;

        CheckSingleFormatter16(null, value, a);
    }

    private static void CF(string a)
    {
        var field = typeof(Lcms2).GetField(a);
        var value = (uint)field!.GetValue(null)!;

        CheckSingleFormatterFloat(value, a);
    }

    private static void CheckSingleFormatter16(Context id, uint Type, string Text)
    {
        var Values = stackalloc ushort[cmsMAXCHANNELS];
        var Buffer = stackalloc byte[1024];

        // Already failed?
        if (FormatterFailed) return;
        //memset(&info, 0, sizeof(Transform));
        var info = new Transform()
        {
            InputFormat = Type,
            OutputFormat = Type
        };

        // Go forth and back
        var f = _cmsGetFormatter(id, Type, FormatterDirection.Input, PackFlags.Ushort);
        var b = _cmsGetFormatter(id, Type, FormatterDirection.Output, PackFlags.Ushort);

        if (f.Fmt16 is null || b.Fmt16 is null)
        {
            Fail($"no formatter for {Text}");
            FormatterFailed = true;

            // Useful for debug
            f = _cmsGetFormatter(id, Type, FormatterDirection.Input, PackFlags.Ushort);
            b = _cmsGetFormatter(id, Type, FormatterDirection.Output, PackFlags.Ushort);
            return;
        }

        var nChannels = T_CHANNELS(Type);
        var bytes = T_BYTES(Type);

        for (var j = 0; j < 5; j++)
        {
            for (var i = 0; i < nChannels; i++)
            {
                Values[i] = (ushort)(i + j);
                // For 8-bit
                if (bytes is 1)
                    Values[i] <<= 8;
            }

            b.Fmt16(info, Values, Buffer, 2);
            memset(Values, 0, sizeof(ushort) * cmsMAXCHANNELS);
            f.Fmt16(info, Values, Buffer, 2);

            for (var i = 0; i < nChannels; i++)
            {
                if (bytes is 1)
                    Values[i] >>= 8;

                if (Values[i] != i + j)
                {
                    Fail($"{Text} failed");
                    FormatterFailed = true;

                    // Useful for debug
                    for (i = 0; i < nChannels; i++)
                    {
                        Values[i] = (ushort)(i + j);
                        // For 8-bit
                        if (bytes is 1)
                            Values[i] <<= 8;
                    }
                    b.Fmt16(info, Values, Buffer, 1);
                    f.Fmt16(info, Values, Buffer, 1);
                    return;
                }
            }
        }
    }

    private static void CheckSingleFormatterFloat(uint Type, string Text)
    {
        var Values = stackalloc float[cmsMAXCHANNELS];
        var Buffer = stackalloc byte[1024];

        // Already failed?
        if (FormatterFailed) return;
        //memset(&info, 0, sizeof(Transform));
        var info = new Transform()
        {
            InputFormat = Type,
            OutputFormat = Type
        };

        // Go forth and back
        var f = _cmsGetFormatter(null, Type, FormatterDirection.Input, PackFlags.Float);
        var b = _cmsGetFormatter(null, Type, FormatterDirection.Output, PackFlags.Float);

        if (f.Fmt16 is null || b.Fmt16 is null)
        {
            Fail($"no formatter for {Text}");
            FormatterFailed = true;

            // Useful for debug
            f = _cmsGetFormatter(null, Type, FormatterDirection.Input, PackFlags.Float);
            b = _cmsGetFormatter(null, Type, FormatterDirection.Output, PackFlags.Float);
            return;
        }

        var nChannels = T_CHANNELS(Type);
        var bytes = T_BYTES(Type);

        for (var j = 0; j < 5; j++)
        {
            for (var i = 0; i < nChannels; i++)
                Values[i] = i + j;

            b.FmtFloat(info, Values, Buffer, 2);
            memset(Values, 0, sizeof(ushort) * cmsMAXCHANNELS);
            f.FmtFloat(info, Values, Buffer, 2);

            for (var i = 0; i < nChannels; i++)
            {
                var delta = Math.Abs(Values[i] - (i + j));

                if (delta > 1e-9)
                {
                    Fail($"{Text} failed");
                    FormatterFailed = true;

                    // Useful for debug
                    for (i = 0; i < nChannels; i++)
                        Values[i] = (ushort)(i + j);

                    b.FmtFloat(info, Values, Buffer, 1);
                    f.FmtFloat(info, Values, Buffer, 1);
                    return;
                }
            }
        }
    }

    public static bool CheckFormattersFloat()
    {
        FormatterFailed = false;

        CF(nameof(TYPE_XYZ_FLT));
        CF(nameof(TYPE_Lab_FLT));
        CF(nameof(TYPE_GRAY_FLT));
        CF(nameof(TYPE_RGB_FLT));
        CF(nameof(TYPE_BGR_FLT));
        CF(nameof(TYPE_CMYK_FLT));

        CF(nameof(TYPE_LabA_FLT));
        CF(nameof(TYPE_RGBA_FLT));

        CF(nameof(TYPE_ARGB_FLT));
        CF(nameof(TYPE_BGRA_FLT));
        CF(nameof(TYPE_ABGR_FLT));

        CF(nameof(TYPE_XYZ_DBL));
        CF(nameof(TYPE_Lab_DBL));
        CF(nameof(TYPE_GRAY_DBL));
        CF(nameof(TYPE_RGB_DBL));
        CF(nameof(TYPE_BGR_DBL));
        CF(nameof(TYPE_CMYK_DBL));
        CF(nameof(TYPE_XYZ_FLT));

        CF(nameof(TYPE_GRAY_HALF_FLT));
        CF(nameof(TYPE_RGB_HALF_FLT));
        CF(nameof(TYPE_CMYK_HALF_FLT));
        CF(nameof(TYPE_RGBA_HALF_FLT));

        CF(nameof(TYPE_RGBA_HALF_FLT));
        CF(nameof(TYPE_ARGB_HALF_FLT));
        CF(nameof(TYPE_BGR_HALF_FLT));
        CF(nameof(TYPE_BGRA_HALF_FLT));
        CF(nameof(TYPE_ABGR_HALF_FLT));

        return !FormatterFailed;
    }

    public static bool CheckFormatters16()
    {
        FormatterFailed = false;

        C16(nameof(TYPE_GRAY_8));
        C16(nameof(TYPE_GRAY_8_REV));
        C16(nameof(TYPE_GRAY_16));
        C16(nameof(TYPE_GRAY_16_REV));
        C16(nameof(TYPE_GRAY_16_SE));
        C16(nameof(TYPE_GRAYA_8));
        C16(nameof(TYPE_GRAYA_16));
        C16(nameof(TYPE_GRAYA_16_SE));
        C16(nameof(TYPE_GRAYA_8_PLANAR));
        C16(nameof(TYPE_GRAYA_16_PLANAR));
        C16(nameof(TYPE_RGB_8));
        C16(nameof(TYPE_RGB_8_PLANAR));
        C16(nameof(TYPE_BGR_8));
        C16(nameof(TYPE_BGR_8_PLANAR));
        C16(nameof(TYPE_RGB_16));
        C16(nameof(TYPE_RGB_16_PLANAR));
        C16(nameof(TYPE_RGB_16_SE));
        C16(nameof(TYPE_BGR_16));
        C16(nameof(TYPE_BGR_16_PLANAR));
        C16(nameof(TYPE_BGR_16_SE));
        C16(nameof(TYPE_RGBA_8));
        C16(nameof(TYPE_RGBA_8_PLANAR));
        C16(nameof(TYPE_RGBA_16));
        C16(nameof(TYPE_RGBA_16_PLANAR));
        C16(nameof(TYPE_RGBA_16_SE));
        C16(nameof(TYPE_ARGB_8));
        C16(nameof(TYPE_ARGB_8_PLANAR));
        C16(nameof(TYPE_ARGB_16));
        C16(nameof(TYPE_ABGR_8));
        C16(nameof(TYPE_ABGR_8_PLANAR));
        C16(nameof(TYPE_ABGR_16));
        C16(nameof(TYPE_ABGR_16_PLANAR));
        C16(nameof(TYPE_ABGR_16_SE));
        C16(nameof(TYPE_BGRA_8));
        C16(nameof(TYPE_BGRA_8_PLANAR));
        C16(nameof(TYPE_BGRA_16));
        C16(nameof(TYPE_BGRA_16_SE));
        C16(nameof(TYPE_CMY_8));
        C16(nameof(TYPE_CMY_8_PLANAR));
        C16(nameof(TYPE_CMY_16));
        C16(nameof(TYPE_CMY_16_PLANAR));
        C16(nameof(TYPE_CMY_16_SE));
        C16(nameof(TYPE_CMYK_8));
        C16(nameof(TYPE_CMYKA_8));
        C16(nameof(TYPE_CMYK_8_REV));
        C16(nameof(TYPE_YUVK_8));
        C16(nameof(TYPE_CMYK_8_PLANAR));
        C16(nameof(TYPE_CMYK_16));
        C16(nameof(TYPE_CMYK_16_REV));
        C16(nameof(TYPE_YUVK_16));
        C16(nameof(TYPE_CMYK_16_PLANAR));
        C16(nameof(TYPE_CMYK_16_SE));
        C16(nameof(TYPE_KYMC_8));
        C16(nameof(TYPE_KYMC_16));
        C16(nameof(TYPE_KYMC_16_SE));
        C16(nameof(TYPE_KCMY_8));
        C16(nameof(TYPE_KCMY_8_REV));
        C16(nameof(TYPE_KCMY_16));
        C16(nameof(TYPE_KCMY_16_REV));
        C16(nameof(TYPE_KCMY_16_SE));
        C16(nameof(TYPE_CMYK5_8));
        C16(nameof(TYPE_CMYK5_16));
        C16(nameof(TYPE_CMYK5_16_SE));
        C16(nameof(TYPE_KYMC5_8));
        C16(nameof(TYPE_KYMC5_16));
        C16(nameof(TYPE_KYMC5_16_SE));
        C16(nameof(TYPE_CMYK6_8));
        C16(nameof(TYPE_CMYK6_8_PLANAR));
        C16(nameof(TYPE_CMYK6_16));
        C16(nameof(TYPE_CMYK6_16_PLANAR));
        C16(nameof(TYPE_CMYK6_16_SE));
        C16(nameof(TYPE_CMYK7_8));
        C16(nameof(TYPE_CMYK7_16));
        C16(nameof(TYPE_CMYK7_16_SE));
        C16(nameof(TYPE_KYMC7_8));
        C16(nameof(TYPE_KYMC7_16));
        C16(nameof(TYPE_KYMC7_16_SE));
        C16(nameof(TYPE_CMYK8_8));
        C16(nameof(TYPE_CMYK8_16));
        C16(nameof(TYPE_CMYK8_16_SE));
        C16(nameof(TYPE_KYMC8_8));
        C16(nameof(TYPE_KYMC8_16));
        C16(nameof(TYPE_KYMC8_16_SE));
        C16(nameof(TYPE_CMYK9_8));
        C16(nameof(TYPE_CMYK9_16));
        C16(nameof(TYPE_CMYK9_16_SE));
        C16(nameof(TYPE_KYMC9_8));
        C16(nameof(TYPE_KYMC9_16));
        C16(nameof(TYPE_KYMC9_16_SE));
        C16(nameof(TYPE_CMYK10_8));
        C16(nameof(TYPE_CMYK10_16));
        C16(nameof(TYPE_CMYK10_16_SE));
        C16(nameof(TYPE_KYMC10_8));
        C16(nameof(TYPE_KYMC10_16));
        C16(nameof(TYPE_KYMC10_16_SE));
        C16(nameof(TYPE_CMYK11_8));
        C16(nameof(TYPE_CMYK11_16));
        C16(nameof(TYPE_CMYK11_16_SE));
        C16(nameof(TYPE_KYMC11_8));
        C16(nameof(TYPE_KYMC11_16));
        C16(nameof(TYPE_KYMC11_16_SE));
        C16(nameof(TYPE_CMYK12_8));
        C16(nameof(TYPE_CMYK12_16));
        C16(nameof(TYPE_CMYK12_16_SE));
        C16(nameof(TYPE_KYMC12_8));
        C16(nameof(TYPE_KYMC12_16));
        C16(nameof(TYPE_KYMC12_16_SE));
        C16(nameof(TYPE_XYZ_16));
        C16(nameof(TYPE_Lab_8));
        C16(nameof(TYPE_ALab_8));
        C16(nameof(TYPE_Lab_16));
        C16(nameof(TYPE_Yxy_16));
        C16(nameof(TYPE_YCbCr_8));
        C16(nameof(TYPE_YCbCr_8_PLANAR));
        C16(nameof(TYPE_YCbCr_16));
        C16(nameof(TYPE_YCbCr_16_PLANAR));
        C16(nameof(TYPE_YCbCr_16_SE));
        C16(nameof(TYPE_YUV_8));
        C16(nameof(TYPE_YUV_8_PLANAR));
        C16(nameof(TYPE_YUV_16));
        C16(nameof(TYPE_YUV_16_PLANAR));
        C16(nameof(TYPE_YUV_16_SE));
        C16(nameof(TYPE_HLS_8));
        C16(nameof(TYPE_HLS_8_PLANAR));
        C16(nameof(TYPE_HLS_16));
        C16(nameof(TYPE_HLS_16_PLANAR));
        C16(nameof(TYPE_HLS_16_SE));
        C16(nameof(TYPE_HSV_8));
        C16(nameof(TYPE_HSV_8_PLANAR));
        C16(nameof(TYPE_HSV_16));
        C16(nameof(TYPE_HSV_16_PLANAR));
        C16(nameof(TYPE_HSV_16_SE));

        C16(nameof(TYPE_XYZ_FLT));
        C16(nameof(TYPE_Lab_FLT));
        C16(nameof(TYPE_GRAY_FLT));
        C16(nameof(TYPE_RGB_FLT));
        C16(nameof(TYPE_BGR_FLT));
        C16(nameof(TYPE_CMYK_FLT));
        C16(nameof(TYPE_LabA_FLT));
        C16(nameof(TYPE_RGBA_FLT));
        C16(nameof(TYPE_ARGB_FLT));
        C16(nameof(TYPE_BGRA_FLT));
        C16(nameof(TYPE_ABGR_FLT));

        C16(nameof(TYPE_XYZ_DBL));
        C16(nameof(TYPE_Lab_DBL));
        C16(nameof(TYPE_GRAY_DBL));
        C16(nameof(TYPE_RGB_DBL));
        C16(nameof(TYPE_BGR_DBL));
        C16(nameof(TYPE_CMYK_DBL));

        C16(nameof(TYPE_LabV2_8));
        C16(nameof(TYPE_ALabV2_8));
        C16(nameof(TYPE_LabV2_16));

        C16(nameof(TYPE_GRAY_HALF_FLT));
        C16(nameof(TYPE_RGB_HALF_FLT));
        C16(nameof(TYPE_CMYK_HALF_FLT));
        C16(nameof(TYPE_RGBA_HALF_FLT));

        C16(nameof(TYPE_RGBA_HALF_FLT));
        C16(nameof(TYPE_ARGB_HALF_FLT));
        C16(nameof(TYPE_BGR_HALF_FLT));
        C16(nameof(TYPE_BGRA_HALF_FLT));
        C16(nameof(TYPE_ABGR_HALF_FLT));

        return !FormatterFailed;
    }

    private static bool my_isfinite(float x) =>
        x != x;

    public static bool CheckFormattersHalf()
    {
        for (var i = 0; i < 0xffff; i++)
        {
            var f = _cmsHalf2Float((ushort)i);

            if (!my_isfinite(f))
            {
                var j = _cmsFloat2Half(f);

                if (i != j)
                {
                    Fail($"{i} != {j} in Half float support!\n");
                    return false;
                }
            }
        }
        return true;
    }

    private static bool CheckOneRGB(Transform xform, ushort R, ushort G, ushort B, ushort Ro, ushort Go, ushort Bo)
    {
        var RGB = stackalloc ushort[3];
        var Out = stackalloc ushort[3];

        RGB[0] = R;
        RGB[1] = G;
        RGB[2] = B;

        cmsDoTransform(xform, RGB, Out, 1);

        return IsGoodWord("R", Ro, Out[0]) &&
               IsGoodWord("G", Go, Out[1]) &&
               IsGoodWord("B", Bo, Out[2]);
    }

    private static bool CheckOneRGB_double(Transform xform, double R, double G, double B, double Ro, double Go, double Bo)
    {
        var RGB = stackalloc double[3];
        var Out = stackalloc double[3];

        RGB[0] = R;
        RGB[1] = G;
        RGB[2] = B;

        cmsDoTransform(xform, RGB, Out, 1);

        return IsGoodVal("R", Ro, Out[0], 1e-2) &&
               IsGoodVal("G", Go, Out[1], 1e-2) &&
               IsGoodVal("B", Bo, Out[2], 1e-2);
    }

    public static bool CheckChangeBufferFormats()
    {
        var hsRGB = cmsCreate_sRGBProfile();

        var xform = cmsCreateTransform(hsRGB, TYPE_RGB_16, hsRGB, TYPE_RGB_16, INTENT_PERCEPTUAL, 0);
        cmsCloseProfile(hsRGB);
        if (xform is null) return false;

        if (!CheckOneRGB(xform, 0, 0, 0, 0, 0, 0)) return false;
        if (!CheckOneRGB(xform, 120, 0, 0, 120, 0, 0)) return false;
        if (!CheckOneRGB(xform, 0, 222, 255, 0, 222, 255)) return false;

        if (!cmsChangeBuffersFormat(xform, TYPE_BGR_16, TYPE_RGBA_16)) return false;

        if (!CheckOneRGB(xform, 0, 0, 123, 123, 0, 0)) return false;
        if (!CheckOneRGB(xform, 154, 234, 0, 0, 234, 154)) return false;

        if (!cmsChangeBuffersFormat(xform, TYPE_RGB_DBL, TYPE_RGB_DBL)) return false;

        if (!CheckOneRGB_double(xform, 0.20, 0, 0, 0.20, 0, 0)) return false;
        if (!CheckOneRGB_double(xform, 0, 0.9, 1, 0, 0.9, 1)) return false;

        cmsDeleteTransform(xform);
        return true;
    }
}
