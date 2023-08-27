//---------------------------------------------------------------------------------
//
//  Little Color Management System, fast floating point extensions
//  Copyright (c) 1998-2022 Marti Maria Saguer, all rights reserved
//                     2023 Stefan Kewatt, all rights reserved
//
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//---------------------------------------------------------------------------------
namespace lcms2.FastFloatPlugin;
public static partial class FastFloat
{
    private const uint cmsFLAGS_CAN_CHANGE_FORMATTER = 0x02000000;   // Allow change buffer format
    internal const ushort MAX_NODES_IN_CURVE = 0x8001;

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
