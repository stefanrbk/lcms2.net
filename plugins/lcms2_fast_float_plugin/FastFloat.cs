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

namespace lcms2_fast_float_plugin;
public static partial class FastFloat
{
    internal static uint BIT15_SH(uint a) =>           ((a) << 26);
    internal static uint T_BIT15(uint a) =>            (((a)>>26)&1);
    internal static uint DITHER_SH(uint a) =>          ((a) << 27);
    internal static uint T_DITHER(uint a)  =>          (((a)>>27)&1);

    internal static uint TYPE_GRAY_15 =           (COLORSPACE_SH(PT_GRAY)|CHANNELS_SH(1)|BYTES_SH(2)|BIT15_SH(1));
    internal static uint TYPE_GRAY_15_REV =       (COLORSPACE_SH(PT_GRAY)|CHANNELS_SH(1)|BYTES_SH(2)|FLAVOR_SH(1)|BIT15_SH(1));
    internal static uint TYPE_GRAY_15_SE =        (COLORSPACE_SH(PT_GRAY)|CHANNELS_SH(1)|BYTES_SH(2)|ENDIAN16_SH(1)|BIT15_SH(1));
    internal static uint TYPE_GRAYA_15 =          (COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|BYTES_SH(2)|BIT15_SH(1));
    internal static uint TYPE_GRAYA_15_SE =       (COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|BYTES_SH(2)|ENDIAN16_SH(1)|BIT15_SH(1));
    internal static uint TYPE_GRAYA_15_PLANAR =   (COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|BYTES_SH(2)|PLANAR_SH(1)|BIT15_SH(1));

    internal static uint TYPE_RGB_15 =            (COLORSPACE_SH(PT_RGB)|CHANNELS_SH(3)|BYTES_SH(2)|BIT15_SH(1));
    internal static uint TYPE_RGB_15_PLANAR =     (COLORSPACE_SH(PT_RGB)|CHANNELS_SH(3)|BYTES_SH(2)|PLANAR_SH(1)|BIT15_SH(1));
    internal static uint TYPE_RGB_15_SE =         (COLORSPACE_SH(PT_RGB)|CHANNELS_SH(3)|BYTES_SH(2)|ENDIAN16_SH(1)|BIT15_SH(1));

    internal static uint TYPE_BGR_15 =            (COLORSPACE_SH(PT_RGB)|CHANNELS_SH(3)|BYTES_SH(2)|DOSWAP_SH(1)|BIT15_SH(1));
    internal static uint TYPE_BGR_15_PLANAR =     (COLORSPACE_SH(PT_RGB)|CHANNELS_SH(3)|BYTES_SH(2)|DOSWAP_SH(1)|PLANAR_SH(1)|BIT15_SH(1));
    internal static uint TYPE_BGR_15_SE =         (COLORSPACE_SH(PT_RGB)|CHANNELS_SH(3)|BYTES_SH(2)|DOSWAP_SH(1)|ENDIAN16_SH(1)|BIT15_SH(1));

    internal static uint TYPE_RGBA_15 =           (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|BIT15_SH(1));
    internal static uint TYPE_RGBA_15_PLANAR =    (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|PLANAR_SH(1)|BIT15_SH(1));
    internal static uint TYPE_RGBA_15_SE =        (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|ENDIAN16_SH(1)|BIT15_SH(1));

    internal static uint TYPE_ARGB_15 =           (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|SWAPFIRST_SH(1)|BIT15_SH(1));

    internal static uint TYPE_ABGR_15 =           (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|DOSWAP_SH(1)|BIT15_SH(1));
    internal static uint TYPE_ABGR_15_PLANAR =    (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|DOSWAP_SH(1)|PLANAR_SH(1)|BIT15_SH(1));
    internal static uint TYPE_ABGR_15_SE =        (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|DOSWAP_SH(1)|ENDIAN16_SH(1)|BIT15_SH(1));

    internal static uint TYPE_BGRA_15 =           (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|DOSWAP_SH(1)|SWAPFIRST_SH(1)|BIT15_SH(1));
    internal static uint TYPE_BGRA_15_SE =        (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|ENDIAN16_SH(1)|DOSWAP_SH(1)|SWAPFIRST_SH(1)|BIT15_SH(1));

    internal static uint TYPE_CMY_15 =            (COLORSPACE_SH(PT_CMY)|CHANNELS_SH(3)|BYTES_SH(2)|BIT15_SH(1));
    internal static uint TYPE_YMC_15 =            (COLORSPACE_SH(PT_CMY)|CHANNELS_SH(3)|BYTES_SH(2)|DOSWAP_SH(1)|BIT15_SH(1));
    internal static uint TYPE_CMY_15_PLANAR =     (COLORSPACE_SH(PT_CMY)|CHANNELS_SH(3)|BYTES_SH(2)|PLANAR_SH(1)|BIT15_SH(1));
    internal static uint TYPE_CMY_15_SE =         (COLORSPACE_SH(PT_CMY)|CHANNELS_SH(3)|BYTES_SH(2)|ENDIAN16_SH(1)|BIT15_SH(1));

    internal static uint TYPE_CMYK_15 =           (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(2)|BIT15_SH(1));
    internal static uint TYPE_CMYK_15_REV =       (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(2)|FLAVOR_SH(1)|BIT15_SH(1));
    internal static uint TYPE_CMYK_15_PLANAR =    (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(2)|PLANAR_SH(1)|BIT15_SH(1));
    internal static uint TYPE_CMYK_15_SE =        (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(2)|ENDIAN16_SH(1)|BIT15_SH(1));

    internal static uint TYPE_KYMC_15 =           (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(2)|DOSWAP_SH(1)|BIT15_SH(1));
    internal static uint TYPE_KYMC_15_SE =        (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(2)|DOSWAP_SH(1)|ENDIAN16_SH(1)|BIT15_SH(1));

    internal static uint TYPE_KCMY_15 =           (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(2)|SWAPFIRST_SH(1)|BIT15_SH(1));
    internal static uint TYPE_KCMY_15_REV =       (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(2)|FLAVOR_SH(1)|SWAPFIRST_SH(1)|BIT15_SH(1));
    internal static uint TYPE_KCMY_15_SE =        (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(2)|ENDIAN16_SH(1)|SWAPFIRST_SH(1)|BIT15_SH(1));

    internal static uint TYPE_GRAY_8_DITHER =     (COLORSPACE_SH(PT_GRAY)|CHANNELS_SH(1)|BYTES_SH(1)|DITHER_SH(1));
    internal static uint TYPE_RGB_8_DITHER =      (COLORSPACE_SH(PT_RGB)|CHANNELS_SH(3)|BYTES_SH(1)|DITHER_SH(1));
    internal static uint TYPE_RGBA_8_DITHER =      (COLORSPACE_SH(PT_RGB)|CHANNELS_SH(3)|BYTES_SH(1)|EXTRA_SH(1)|DITHER_SH(1));
    internal static uint TYPE_BGR_8_DITHER =      (COLORSPACE_SH(PT_RGB)|CHANNELS_SH(3)|BYTES_SH(1)|DOSWAP_SH(1)|DITHER_SH(1));
    internal static uint TYPE_ABGR_8_DITHER =      (COLORSPACE_SH(PT_RGB)|CHANNELS_SH(3)|BYTES_SH(1)|DOSWAP_SH(1)|EXTRA_SH(1)|DITHER_SH(1));
    internal static uint TYPE_CMYK_8_DITHER =     (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(1)|DITHER_SH(1));
    internal static uint TYPE_KYMC_8_DITHER =     (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|BYTES_SH(1)|DOSWAP_SH(1)|DITHER_SH(1));


    internal static uint TYPE_AGRAY_8 =           (COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|DOSWAP_SH(1)|BYTES_SH(1));
    internal static uint TYPE_AGRAY_16 =          (COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|DOSWAP_SH(1)|BYTES_SH(2));
    internal static uint TYPE_AGRAY_FLT =         (FLOAT_SH(1)|COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|DOSWAP_SH(1)|BYTES_SH(4));
    internal static uint TYPE_AGRAY_DBL =         (FLOAT_SH(1)|COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|DOSWAP_SH(1)|BYTES_SH(0));

    internal static uint TYPE_ACMYK_8 =           (COLORSPACE_SH(PT_CMYK)|EXTRA_SH(1)|CHANNELS_SH(4)|BYTES_SH(1)|SWAPFIRST_SH(1));
    internal static uint TYPE_KYMCA_8 =           (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|EXTRA_SH(1)|BYTES_SH(1)|DOSWAP_SH(1)|SWAPFIRST_SH(1));
    internal static uint TYPE_AKYMC_8 =           (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|EXTRA_SH(1)|BYTES_SH(1)|DOSWAP_SH(1));

    internal static uint TYPE_CMYKA_16 =           (COLORSPACE_SH(PT_CMYK)|EXTRA_SH(1)|CHANNELS_SH(4)|BYTES_SH(2));
    internal static uint TYPE_ACMYK_16 =           (COLORSPACE_SH(PT_CMYK)|EXTRA_SH(1)|CHANNELS_SH(4)|BYTES_SH(2)|SWAPFIRST_SH(1));
    internal static uint TYPE_KYMCA_16 =           (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|EXTRA_SH(1)|BYTES_SH(2)|DOSWAP_SH(1)|SWAPFIRST_SH(1));
    internal static uint TYPE_AKYMC_16 =           (COLORSPACE_SH(PT_CMYK)|CHANNELS_SH(4)|EXTRA_SH(1)|BYTES_SH(2)|DOSWAP_SH(1));


    internal static uint TYPE_AGRAY_8_PLANAR =    (COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|BYTES_SH(1)|PLANAR_SH(1)|SWAPFIRST_SH(1));
    internal static uint TYPE_AGRAY_16_PLANAR =   (COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|BYTES_SH(2)|PLANAR_SH(1)|SWAPFIRST_SH(1));

    internal static uint TYPE_GRAYA_FLT_PLANAR =  (FLOAT_SH(1)|COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|BYTES_SH(4)|PLANAR_SH(1));
    internal static uint TYPE_AGRAY_FLT_PLANAR =  (FLOAT_SH(1)|COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|BYTES_SH(4)|PLANAR_SH(1)|SWAPFIRST_SH(1));

    internal static uint TYPE_GRAYA_DBL_PLANAR =  (FLOAT_SH(1)|COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|BYTES_SH(0)|PLANAR_SH(1));
    internal static uint TYPE_AGRAY_DBL_PLANAR =  (FLOAT_SH(1)|COLORSPACE_SH(PT_GRAY)|EXTRA_SH(1)|CHANNELS_SH(1)|BYTES_SH(0)|PLANAR_SH(1)|SWAPFIRST_SH(1));

    internal static uint TYPE_ARGB_16_PLANAR =    (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|SWAPFIRST_SH(1)|PLANAR_SH(1));
    internal static uint TYPE_BGRA_16_PLANAR =    (COLORSPACE_SH(PT_RGB)|EXTRA_SH(1)|CHANNELS_SH(3)|BYTES_SH(2)|SWAPFIRST_SH(1)|DOSWAP_SH(1)|PLANAR_SH(1));

}
