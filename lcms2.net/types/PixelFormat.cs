//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
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

using System.Runtime.CompilerServices;

namespace lcms2.types;

// Format of pixel is defined by one cmsUInt32Number, using bit fields as follows
//
//                               2                1          0
//                        4 3 2 10987 6 5 4 3 2 1 098 7654 321
//                        M A O TTTTT U Y F P X S EEE CCCC BBB
//
//            M: Premultiplied alpha (only works when extra samples is 1)
//            A: Floating point -- With this flag we can differentiate 16 bits as float and as int
//            O: Optimized -- previous optimization already returns the final 8-bit value
//            T: Pixeltype
//            F: Flavor  0=MinIsBlack(Chocolate) 1=MinIsWhite(Vanilla)
//            P: Planar? 0=Chunky, 1=Planar
//            X: swap 16 bps endianness?
//            S: Do swap? ie, BGR, KYMC
//            E: Extra samples
//            C: Channels (Samples per pixel)
//            B: bytes per sample
//            Y: Swap first - changes ABGR to BGRA and KCMY to CMYK

public enum Colorspace : byte
{
    Any = 0,
    Gray = 3,
    RGB = 4,
    CMY = 5,
    CMYK = 6,
    YCbCr = 7,
    YUV = 8,
    XYZ = 9,
    Lab = 10,
    YUVK = 11,
    HSV = 12,
    HLS = 13,
    Yxy = 14,
    MCH1 = 15,
    MCH2 = 16,
    MCH3 = 17,
    MCH4 = 18,
    MCH5 = 19,
    MCH6 = 20,
    MCH7 = 21,
    MCH8 = 22,
    MCH9 = 23,
    MCH10 = 24,
    MCH11 = 25,
    MCH12 = 26,
    MCH13 = 27,
    MCH14 = 28,
    MCH15 = 29,
    LabV2 = 30,
}

public readonly struct PixelFormat
{
    #region Fields

    public static readonly PixelFormat ABGR_16 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat ABGR_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat ABGR_16_PREMUL =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat ABGR_16_SE =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat ABGR_8 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            DOSWAP_SH(1));

    public static readonly PixelFormat ABGR_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            DOSWAP_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat ABGR_8_PREMUL =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            DOSWAP_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat ABGR_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(4) |
            DOSWAP_SH(1));

    public static readonly PixelFormat ABGR_FLT_PREMUL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(4) |
            DOSWAP_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat ABGR_HALF_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat ALab_8 =
        new(
            COLORSPACE_SH(Colorspace.Lab) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            EXTRA_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat ALabV2_8 =
        new(
            COLORSPACE_SH(Colorspace.LabV2) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            EXTRA_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat ARGB_16 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat ARGB_16_PREMUL =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            SWAPFIRST_SH(1) | PREMUL_SH(1));

    public static readonly PixelFormat ARGB_8 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat ARGB_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            SWAPFIRST_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat ARGB_8_PREMUL =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            SWAPFIRST_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat ARGB_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(4) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat ARGB_FLT_PREMUL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(4) |
            SWAPFIRST_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat ARGB_HALF_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) | SWAPFIRST_SH(1));

    public static readonly PixelFormat BGR_16 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat BGR_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat BGR_16_SE =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat BGR_8 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            DOSWAP_SH(1));

    public static readonly PixelFormat BGR_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            DOSWAP_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat BGR_DBL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(0) |
            DOSWAP_SH(1));

    public static readonly PixelFormat BGR_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(4) |
            DOSWAP_SH(1));

    public static readonly PixelFormat BGR_HALF_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat BGRA_16 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat BGRA_16_PREMUL =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            SWAPFIRST_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat BGRA_16_SE =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            ENDIAN16_SH(1) |
            DOSWAP_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat BGRA_8 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            DOSWAP_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat BGRA_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            DOSWAP_SH(1) |
            SWAPFIRST_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat BGRA_8_PREMUL =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            DOSWAP_SH(1) |
            SWAPFIRST_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat BGRA_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(4) |
            DOSWAP_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat BGRA_FLT_PREMUL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(4) |
            DOSWAP_SH(1) |
            SWAPFIRST_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat BGRA_HALF_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat CMY_16 =
        new(
            COLORSPACE_SH(Colorspace.CMY) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat CMY_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.CMY) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            PLANAR_SH(1));

    public static readonly PixelFormat CMY_16_SE =
        new(
            COLORSPACE_SH(Colorspace.CMY) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat CMY_8 =
        new(
            COLORSPACE_SH(Colorspace.CMY) |
            CHANNELS_SH(3) |
            BYTES_SH(1));

    public static readonly PixelFormat CMY_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.CMY) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat CMYK_16 =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(2));

    public static readonly PixelFormat CMYK_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(2) |
            PLANAR_SH(1));

    public static readonly PixelFormat CMYK_16_REV =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(2) |
            FLAVOR_SH(1));

    public static readonly PixelFormat CMYK_16_SE =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat CMYK_8 =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(1));

    public static readonly PixelFormat CMYK_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat CMYK_8_REV =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(1) |
            FLAVOR_SH(1));

    public static readonly PixelFormat CMYK_DBL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(0));

    public static readonly PixelFormat CMYK_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(4));

    public static readonly PixelFormat CMYK_HALF_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(2));

    public static readonly PixelFormat CMYK10_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH10) |
            CHANNELS_SH(10) |
            BYTES_SH(2));

    public static readonly PixelFormat CMYK10_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH10) |
            CHANNELS_SH(10) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat CMYK10_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH10) |
            CHANNELS_SH(10) |
            BYTES_SH(1));

    public static readonly PixelFormat CMYK11_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH11) |
            CHANNELS_SH(11) |
            BYTES_SH(2));

    public static readonly PixelFormat CMYK11_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH11) |
            CHANNELS_SH(11) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat CMYK11_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH11) |
            CHANNELS_SH(11) |
            BYTES_SH(1));

    public static readonly PixelFormat CMYK12_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH12) |
            CHANNELS_SH(12) |
            BYTES_SH(2));

    public static readonly PixelFormat CMYK12_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH12) |
            CHANNELS_SH(12) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat CMYK12_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH12) |
            CHANNELS_SH(12) |
            BYTES_SH(1));

    public static readonly PixelFormat CMYK5_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH5) |
            CHANNELS_SH(5) |
            BYTES_SH(2));

    public static readonly PixelFormat CMYK5_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH5) |
            CHANNELS_SH(5) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat CMYK5_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH5) |
            CHANNELS_SH(5) |
            BYTES_SH(1));

    public static readonly PixelFormat CMYK6_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH6) |
            CHANNELS_SH(6) |
            BYTES_SH(2));

    public static readonly PixelFormat CMYK6_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.MCH6) |
            CHANNELS_SH(6) |
            BYTES_SH(2) |
            PLANAR_SH(1));

    public static readonly PixelFormat CMYK6_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH6) |
            CHANNELS_SH(6) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat CMYK6_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH6) |
            CHANNELS_SH(6) |
            BYTES_SH(1));

    public static readonly PixelFormat CMYK6_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.MCH6) |
            CHANNELS_SH(6) |
            BYTES_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat CMYK7_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH7) |
            CHANNELS_SH(7) |
            BYTES_SH(2));

    public static readonly PixelFormat CMYK7_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH7) |
            CHANNELS_SH(7) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat CMYK7_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH7) |
            CHANNELS_SH(7) |
            BYTES_SH(1));

    public static readonly PixelFormat CMYK8_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH8) |
            CHANNELS_SH(8) |
            BYTES_SH(2));

    public static readonly PixelFormat CMYK8_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH8) |
            CHANNELS_SH(8) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat CMYK8_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH8) |
            CHANNELS_SH(8) |
            BYTES_SH(1));

    public static readonly PixelFormat CMYK9_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH9) |
            CHANNELS_SH(9) |
            BYTES_SH(2));

    public static readonly PixelFormat CMYK9_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH9) |
            CHANNELS_SH(9) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat CMYK9_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH9) |
            CHANNELS_SH(9) |
            BYTES_SH(1));

    public static readonly PixelFormat CMYKA_8 =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            EXTRA_SH(1) |
            CHANNELS_SH(4) |
            BYTES_SH(1));

    public static readonly PixelFormat GRAY_16 =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            CHANNELS_SH(1) |
            BYTES_SH(2));

    public static readonly PixelFormat GRAY_16_REV =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            CHANNELS_SH(1) |
            BYTES_SH(2) |
            FLAVOR_SH(1));

    public static readonly PixelFormat GRAY_16_SE =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            CHANNELS_SH(1) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat GRAY_8 =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            CHANNELS_SH(1) |
            BYTES_SH(1));

    public static readonly PixelFormat GRAY_8_REV =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            CHANNELS_SH(1) |
            BYTES_SH(1) |
            FLAVOR_SH(1));

    public static readonly PixelFormat GRAY_DBL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.Gray) |
            CHANNELS_SH(1) |
            BYTES_SH(0));

    public static readonly PixelFormat GRAY_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.Gray) |
            CHANNELS_SH(1) |
            BYTES_SH(4));

    public static readonly PixelFormat GRAY_HALF_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.Gray) |
            CHANNELS_SH(1) |
            BYTES_SH(2));

    public static readonly PixelFormat GRAYA_16 =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            EXTRA_SH(1) |
            CHANNELS_SH(1) |
            BYTES_SH(2));

    public static readonly PixelFormat GRAYA_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            EXTRA_SH(1) |
            CHANNELS_SH(1) |
            BYTES_SH(2) |
            PLANAR_SH(1));

    public static readonly PixelFormat GRAYA_16_PREMUL =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            EXTRA_SH(1) |
            CHANNELS_SH(1) |
            BYTES_SH(2) |
            PREMUL_SH(1));

    public static readonly PixelFormat GRAYA_16_SE =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            EXTRA_SH(1) |
            CHANNELS_SH(1) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat GRAYA_8 =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            EXTRA_SH(1) |
            CHANNELS_SH(1) |
            BYTES_SH(1));

    public static readonly PixelFormat GRAYA_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            EXTRA_SH(1) |
            CHANNELS_SH(1) |
            BYTES_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat GRAYA_8_PREMUL =
        new(
            COLORSPACE_SH(Colorspace.Gray) |
            EXTRA_SH(1) |
            CHANNELS_SH(1) |
            BYTES_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat GRAYA_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.Gray) |
            CHANNELS_SH(1) |
            BYTES_SH(4) |
            EXTRA_SH(1));

    public static readonly PixelFormat GRAYA_FLT_PREMUL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.Gray) |
            CHANNELS_SH(1) |
            BYTES_SH(4) |
            EXTRA_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat HLS_16 =
        new(
            COLORSPACE_SH(Colorspace.HLS) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat HLS_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.HLS) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            PLANAR_SH(1));

    public static readonly PixelFormat HLS_16_SE =
        new(
            COLORSPACE_SH(Colorspace.HLS) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat HLS_8 =
        new(
            COLORSPACE_SH(Colorspace.HLS) |
            CHANNELS_SH(3) |
            BYTES_SH(1));

    public static readonly PixelFormat HLS_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.HLS) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat HSV_16 =
        new(
            COLORSPACE_SH(Colorspace.HSV) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat HSV_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.HSV) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            PLANAR_SH(1));

    public static readonly PixelFormat HSV_16_SE =
        new(
            COLORSPACE_SH(Colorspace.HSV) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat HSV_8 =
        new(
            COLORSPACE_SH(Colorspace.HSV) |
            CHANNELS_SH(3) |
            BYTES_SH(1));

    public static readonly PixelFormat HSV_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.HSV) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat KCMY_16 =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(2) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat KCMY_16_REV =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(2) |
            FLAVOR_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat KCMY_16_SE =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(2) |
            ENDIAN16_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat KCMY_8 =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat KCMY_8_REV =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(1) |
            FLAVOR_SH(1) |
            SWAPFIRST_SH(1));

    public static readonly PixelFormat KYMC_16 =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC_16_SE =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat KYMC_8 =
        new(
            COLORSPACE_SH(Colorspace.CMYK) |
            CHANNELS_SH(4) |
            BYTES_SH(1) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC10_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH10) |
            CHANNELS_SH(10) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC10_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH10) |
            CHANNELS_SH(10) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat KYMC10_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH10) |
            CHANNELS_SH(10) |
            BYTES_SH(1) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC11_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH11) |
            CHANNELS_SH(11) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC11_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH11) |
            CHANNELS_SH(11) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat KYMC11_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH11) |
            CHANNELS_SH(11) |
            BYTES_SH(1) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC12_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH12) |
            CHANNELS_SH(12) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC12_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH12) |
            CHANNELS_SH(12) |
            BYTES_SH(2) |
            DOSWAP_SH(1) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat KYMC12_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH12) |
            CHANNELS_SH(12) |
            BYTES_SH(1) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC5_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH5) |
            CHANNELS_SH(5) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC5_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH5) |
            CHANNELS_SH(5) |
            BYTES_SH(2) |
            DOSWAP_SH(1) | ENDIAN16_SH(1));

    public static readonly PixelFormat KYMC5_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH5) |
            CHANNELS_SH(5) |
            BYTES_SH(1) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC7_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH7) |
            CHANNELS_SH(7) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC7_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH7) |
            CHANNELS_SH(7) |
            BYTES_SH(2) |
            DOSWAP_SH(1) | ENDIAN16_SH(1));

    public static readonly PixelFormat KYMC7_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH7) |
            CHANNELS_SH(7) |
            BYTES_SH(1) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC8_16 =
        new(
            COLORSPACE_SH(Colorspace.MCH8) |
            CHANNELS_SH(8) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC8_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH8) |
            CHANNELS_SH(8) |
            BYTES_SH(2) |
            DOSWAP_SH(1) | ENDIAN16_SH(1));

    public static readonly PixelFormat KYMC8_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH8) |
            CHANNELS_SH(8) |
            BYTES_SH(1) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC9_16 =
        new(COLORSPACE_SH(Colorspace.MCH9) | CHANNELS_SH(9) |
            BYTES_SH(2) |
            DOSWAP_SH(1));

    public static readonly PixelFormat KYMC9_16_SE =
        new(
            COLORSPACE_SH(Colorspace.MCH9) |
            CHANNELS_SH(9) |
            BYTES_SH(2) |
            DOSWAP_SH(1) | ENDIAN16_SH(1));

    public static readonly PixelFormat KYMC9_8 =
        new(
            COLORSPACE_SH(Colorspace.MCH9) |
            CHANNELS_SH(9) |
            BYTES_SH(1) |
            DOSWAP_SH(1));

    public static readonly PixelFormat Lab_16 =
        new(
            COLORSPACE_SH(Colorspace.Lab) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat Lab_8 =
        new(
            COLORSPACE_SH(Colorspace.Lab) |
            CHANNELS_SH(3) |
            BYTES_SH(1));

    public static readonly PixelFormat Lab_DBL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.Lab) |
            CHANNELS_SH(3) |
            BYTES_SH(0));

    public static readonly PixelFormat Lab_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.Lab) |
            CHANNELS_SH(3) |
            BYTES_SH(4));

    public static readonly PixelFormat LabA_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.Lab) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(4));

    public static readonly PixelFormat LabV2_16 =
        new(
            COLORSPACE_SH(Colorspace.LabV2) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat LabV2_8 =
        new(
            COLORSPACE_SH(Colorspace.LabV2) |
            CHANNELS_SH(3) |
            BYTES_SH(1));

    public static readonly PixelFormat NAMED_COLOR_INDEX =
        new(
            CHANNELS_SH(1) |
            BYTES_SH(2));

    public static readonly PixelFormat RGB_16 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat RGB_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            PLANAR_SH(1));

    public static readonly PixelFormat RGB_16_SE =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat RGB_8 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(1));

    public static readonly PixelFormat RGB_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat RGB_DBL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(0));

    public static readonly PixelFormat RGB_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(4));

    public static readonly PixelFormat RGB_HALF_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat RGBA_16 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat RGBA_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            PLANAR_SH(1));

    public static readonly PixelFormat RGBA_16_PREMUL =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            PREMUL_SH(1));

    public static readonly PixelFormat RGBA_16_SE =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat RGBA_8 =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1));

    public static readonly PixelFormat RGBA_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat RGBA_8_PREMUL =
        new(
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            PREMUL_SH(1));

    public static readonly PixelFormat RGBA_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(4));

    public static readonly PixelFormat RGBA_FLT_PREMUL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(4) |
            PREMUL_SH(1));

    public static readonly PixelFormat RGBA_HALF_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.RGB) |
            EXTRA_SH(1) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat XYZ_16 =
        new(
            COLORSPACE_SH(Colorspace.XYZ) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat XYZ_DBL =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.XYZ) |
            CHANNELS_SH(3) |
            BYTES_SH(0));

    public static readonly PixelFormat XYZ_FLT =
        new(
            FLOAT_SH(1) |
            COLORSPACE_SH(Colorspace.XYZ) |
            CHANNELS_SH(3) |
            BYTES_SH(4));

    public static readonly PixelFormat YCbCr_16 =
        new(
            COLORSPACE_SH(Colorspace.YCbCr) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat YCbCr_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.YCbCr) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            PLANAR_SH(1));

    public static readonly PixelFormat YCbCr_16_SE =
        new(
            COLORSPACE_SH(Colorspace.YCbCr) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat YCbCr_8 =
        new(
            COLORSPACE_SH(Colorspace.YCbCr) |
            CHANNELS_SH(3) |
            BYTES_SH(1));

    public static readonly PixelFormat YCbCr_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.YCbCr) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat YUV_16 =
        new(
            COLORSPACE_SH(Colorspace.YUV) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    public static readonly PixelFormat YUV_16_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.YUV) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            PLANAR_SH(1));

    public static readonly PixelFormat YUV_16_SE =
        new(
            COLORSPACE_SH(Colorspace.YUV) |
            CHANNELS_SH(3) |
            BYTES_SH(2) |
            ENDIAN16_SH(1));

    public static readonly PixelFormat YUV_8 =
        new(
            COLORSPACE_SH(Colorspace.YUV) |
            CHANNELS_SH(3) |
            BYTES_SH(1));

    public static readonly PixelFormat YUV_8_PLANAR =
        new(
            COLORSPACE_SH(Colorspace.YUV) |
            CHANNELS_SH(3) |
            BYTES_SH(1) |
            PLANAR_SH(1));

    public static readonly PixelFormat YUVK_16 = CMYK_16_REV;

    public static readonly PixelFormat YUVK_8 = CMYK_8_REV;

    public static readonly PixelFormat Yxy_16 =
        new(
            COLORSPACE_SH(Colorspace.Yxy) |
            CHANNELS_SH(3) |
            BYTES_SH(2));

    private readonly uint value;

    #endregion Fields

    #region Internal Constructors

    internal PixelFormat(uint value) =>
        this.value = value;

    #endregion Internal Constructors

    #region Properties

    public static uint AnyChannels =>
        CHANNELS_SH(15);

    public static uint AnyEndian =>
        ENDIAN16_SH(1);

    public static uint AnyExtra =>
        EXTRA_SH(7);

    public static uint AnyFlavor =>
        FLAVOR_SH(1);

    public static uint AnyPlanar =>
        PLANAR_SH(1);

    public static uint AnyPremul =>
        PREMUL_SH(1);

    public static uint AnySpace =>
        COLORSPACE_SH((Colorspace)31);

    public static uint AnySwap =>
        DOSWAP_SH(1);

    public static uint AnySwapFirst =>
        SWAPFIRST_SH(1);

    public byte Bytes =>
        BYTES(this);

    public byte Channels =>
        CHANNELS(this);

    public bool Chunky =>
        !Planar;

    public Colorspace Colorspace =>
        COLORSPACE(this);

    public bool EndianSwap =>
        ENDIAN16(this);

    public byte ExtraSamples =>
        EXTRA(this);

    public ColorFlavor Flavor =>
        FLAVOR(this)
            ? ColorFlavor.Subtractive
            : ColorFlavor.Additive;

    public bool Float =>
        FLOAT(this);

    public bool Int =>
        !Float;

    public bool IsInkSpace =>
        Colorspace is
            Colorspace.CMY or
            Colorspace.CMYK or
            Colorspace.MCH5 or
            Colorspace.MCH6 or
            Colorspace.MCH7 or
            Colorspace.MCH8 or
            Colorspace.MCH9 or
            Colorspace.MCH10 or
            Colorspace.MCH11 or
            Colorspace.MCH12 or
            Colorspace.MCH13 or
            Colorspace.MCH14 or
            Colorspace.MCH15;

    public bool Optimized =>
            OPTIMIZED(this);

    public ushort PixelSize =>
        Bytes is 0
            ? (ushort)sizeof(ushort)
            : Bytes;

    public bool Planar =>
            PLANAR(this);

    public bool PremultipliedAlpha =>
        PREMUL(this);

    public bool SwapAll =>
        DOSWAP(this);

    public bool SwapFirst =>
        SWAPFIRST(this);

    #endregion Properties

    #region Public Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte BYTES(uint value) =>
        (byte)(value & 7);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BYTES_SH(uint value) =>
        value & 7u;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte CHANNELS(uint value) =>
        (byte)((value >> 3) & 15);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CHANNELS_SH(uint value) =>
        value << 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Colorspace COLORSPACE(uint value) =>
        (Colorspace)((value >> 16) & 15);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint COLORSPACE_SH(Colorspace value) =>
        (uint)((byte)value << 16);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DOSWAP(uint value) =>
        ((value >> 10) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint DOSWAP_SH(uint value) =>
        value << 10;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ENDIAN16(uint value) =>
        ((value >> 11) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ENDIAN16_SH(uint value) =>
        value << 11;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PixelFormat(uint value) =>
        new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte EXTRA(uint value) =>
        (byte)((value >> 7) & 7);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint EXTRA_SH(uint value) =>
        value << 7;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FLAVOR(uint value) =>
        ((value >> 13) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint FLAVOR_SH(uint value) =>
        value << 13;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FLOAT(uint value) =>
        ((value >> 22) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint FLOAT_SH(uint value) =>
        value << 22;

    public static implicit operator uint(PixelFormat format) =>
            format.value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool OPTIMIZED(uint value) =>
        ((value >> 21) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint OPTIMIZED_SH(uint value) =>
        value << 21;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PLANAR(uint value) =>
        ((value >> 12) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PLANAR_SH(uint value) =>
        value << 12;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PREMUL(uint value) =>
        ((value >> 23) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PREMUL_SH(uint value) =>
        value << 23;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SWAPFIRST(uint value) =>
        ((value >> 14) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SWAPFIRST_SH(uint value) =>
        value << 14;

    #endregion Public Methods
}

public enum ColorFlavor
{
    Additive,
    Subtractive,
}
