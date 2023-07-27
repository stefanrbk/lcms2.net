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
    internal static readonly FormattersPluginChunkType FormattersPluginChunk = new();

    internal static readonly FormattersPluginChunkType globalFormattersPluginChunk = new();

    internal static readonly Formatters16[] InputFormatters16 =
        {
            new( TYPE_Lab_DBL,                                 ANYPLANAR|ANYEXTRA,   UnrollLabDoubleTo16),
            new( TYPE_XYZ_DBL,                                 ANYPLANAR|ANYEXTRA,   UnrollXYZDoubleTo16),
            new( TYPE_Lab_FLT,                                 ANYPLANAR|ANYEXTRA,   UnrollLabFloatTo16),
            new( TYPE_XYZ_FLT,                                 ANYPLANAR|ANYEXTRA,   UnrollXYZFloatTo16),
            new( TYPE_GRAY_DBL,                                                 0,   UnrollDouble1Chan),
            new( FLOAT_SH(1)|BYTES_SH(0), ANYCHANNELS|ANYPLANAR|ANYSWAPFIRST|ANYFLAVOR|
                                                     ANYSWAP|ANYEXTRA|ANYSPACE,   UnrollDoubleTo16),
            new( FLOAT_SH(1)|BYTES_SH(4), ANYCHANNELS|ANYPLANAR|ANYSWAPFIRST|ANYFLAVOR|
                                                 ANYSWAP|ANYEXTRA|ANYSPACE,   UnrollFloatTo16),

            new ( FLOAT_SH(1)|BYTES_SH(2), ANYCHANNELS|ANYPLANAR|ANYSWAPFIRST|ANYFLAVOR|
                                                    ANYEXTRA|ANYSWAP|ANYSPACE,   UnrollHalfTo16),

            new( CHANNELS_SH(1)|BYTES_SH(1),                              ANYSPACE,  Unroll1Byte),
            new( CHANNELS_SH(1)|BYTES_SH(1)|EXTRA_SH(1),                  ANYSPACE,  Unroll1ByteSkip1),
            new( CHANNELS_SH(1)|BYTES_SH(1)|EXTRA_SH(2),                  ANYSPACE,  Unroll1ByteSkip2),
            new( CHANNELS_SH(1)|BYTES_SH(1)|FLAVOR_SH(1),                 ANYSPACE,  Unroll1ByteReversed),
            new( COLORSPACE_SH(PT_MCH2)|CHANNELS_SH(2)|BYTES_SH(1),              0,  Unroll2Bytes),

            new( TYPE_LabV2_8,                                                   0,  UnrollLabV2_8 ),
            new( TYPE_ALabV2_8,                                                  0,  UnrollALabV2_8 ),
            new( TYPE_LabV2_16,                                                  0,  UnrollLabV2_16 ),

            new( CHANNELS_SH(3)|BYTES_SH(1),                              ANYSPACE,  Unroll3Bytes),
            new( CHANNELS_SH(3)|BYTES_SH(1)|DOSWAP_SH(1),                 ANYSPACE,  Unroll3BytesSwap),
            new( CHANNELS_SH(3)|EXTRA_SH(1)|BYTES_SH(1)|DOSWAP_SH(1),     ANYSPACE,  Unroll3BytesSkip1Swap),
            new( CHANNELS_SH(3)|EXTRA_SH(1)|BYTES_SH(1)|SWAPFIRST_SH(1),  ANYSPACE,  Unroll3BytesSkip1SwapFirst),

            new( CHANNELS_SH(3)|EXTRA_SH(1)|BYTES_SH(1)|DOSWAP_SH(1)|SWAPFIRST_SH(1),
                                                                       ANYSPACE,  Unroll3BytesSkip1SwapSwapFirst),

            new( CHANNELS_SH(4)|BYTES_SH(1),                              ANYSPACE,  Unroll4Bytes),
            new( CHANNELS_SH(4)|BYTES_SH(1)|FLAVOR_SH(1),                 ANYSPACE,  Unroll4BytesReverse),
            new( CHANNELS_SH(4)|BYTES_SH(1)|SWAPFIRST_SH(1),              ANYSPACE,  Unroll4BytesSwapFirst),
            new( CHANNELS_SH(4)|BYTES_SH(1)|DOSWAP_SH(1),                 ANYSPACE,  Unroll4BytesSwap),
            new( CHANNELS_SH(4)|BYTES_SH(1)|DOSWAP_SH(1)|SWAPFIRST_SH(1), ANYSPACE,  Unroll4BytesSwapSwapFirst),

            new( BYTES_SH(1)|PLANAR_SH(1), ANYFLAVOR|ANYSWAPFIRST|ANYPREMUL|
                                           ANYSWAP|ANYEXTRA|ANYCHANNELS|ANYSPACE, UnrollPlanarBytes),

            new( BYTES_SH(1),    ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|ANYPREMUL|
                                                   ANYEXTRA|ANYCHANNELS|ANYSPACE, UnrollChunkyBytes),

            new( CHANNELS_SH(1)|BYTES_SH(2),                              ANYSPACE,  Unroll1Word),
            new( CHANNELS_SH(1)|BYTES_SH(2)|FLAVOR_SH(1),                 ANYSPACE,  Unroll1WordReversed),
            new( CHANNELS_SH(1)|BYTES_SH(2)|EXTRA_SH(3),                  ANYSPACE,  Unroll1WordSkip3),

            new( CHANNELS_SH(2)|BYTES_SH(2),                              ANYSPACE,  Unroll2Words),
            new( CHANNELS_SH(3)|BYTES_SH(2),                              ANYSPACE,  Unroll3Words),
            new( CHANNELS_SH(4)|BYTES_SH(2),                              ANYSPACE,  Unroll4Words),

            new( CHANNELS_SH(3)|BYTES_SH(2)|DOSWAP_SH(1),                 ANYSPACE,  Unroll3WordsSwap),
            new( CHANNELS_SH(3)|BYTES_SH(2)|EXTRA_SH(1)|SWAPFIRST_SH(1),  ANYSPACE,  Unroll3WordsSkip1SwapFirst),
            new( CHANNELS_SH(3)|BYTES_SH(2)|EXTRA_SH(1)|DOSWAP_SH(1),     ANYSPACE,  Unroll3WordsSkip1Swap),
            new( CHANNELS_SH(4)|BYTES_SH(2)|FLAVOR_SH(1),                 ANYSPACE,  Unroll4WordsReverse),
            new( CHANNELS_SH(4)|BYTES_SH(2)|SWAPFIRST_SH(1),              ANYSPACE,  Unroll4WordsSwapFirst),
            new( CHANNELS_SH(4)|BYTES_SH(2)|DOSWAP_SH(1),                 ANYSPACE,  Unroll4WordsSwap),
            new( CHANNELS_SH(4)|BYTES_SH(2)|DOSWAP_SH(1)|SWAPFIRST_SH(1), ANYSPACE,  Unroll4WordsSwapSwapFirst),

            new( BYTES_SH(2)|PLANAR_SH(1),  ANYFLAVOR|ANYSWAP|ANYENDIAN|ANYEXTRA|ANYCHANNELS|ANYSPACE,  UnrollPlanarWords),
            new( BYTES_SH(2),  ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|ANYENDIAN|ANYEXTRA|ANYCHANNELS|ANYSPACE,  UnrollAnyWords),

            new( BYTES_SH(2)|PLANAR_SH(1),  ANYFLAVOR|ANYSWAP|ANYENDIAN|ANYEXTRA|ANYCHANNELS|ANYSPACE|PREMUL_SH(1),  UnrollPlanarWordsPremul),
            new( BYTES_SH(2),  ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|ANYENDIAN|ANYEXTRA|ANYCHANNELS|ANYSPACE|PREMUL_SH(1),  UnrollAnyWordsPremul)
        };

    internal static readonly FormattersFloat[] InputFormattersFloat = {
    //    Type                                          Mask                  Function
    //  ----------------------------   ------------------------------------  ----------------------------
    new(     TYPE_Lab_DBL,                                ANYPLANAR|ANYEXTRA,   UnrollLabDoubleToFloat),
    new(     TYPE_Lab_FLT,                                ANYPLANAR|ANYEXTRA,   UnrollLabFloatToFloat),

    new(     TYPE_XYZ_DBL,                                ANYPLANAR|ANYEXTRA,   UnrollXYZDoubleToFloat),
    new(     TYPE_XYZ_FLT,                                ANYPLANAR|ANYEXTRA,   UnrollXYZFloatToFloat),

    new(     FLOAT_SH(1)|BYTES_SH(4), ANYPLANAR|ANYSWAPFIRST|ANYSWAP|ANYEXTRA|
                                            ANYPREMUL|ANYCHANNELS|ANYSPACE,  UnrollFloatsToFloat),

    new(     FLOAT_SH(1)|BYTES_SH(0), ANYPLANAR|ANYSWAPFIRST|ANYSWAP|ANYEXTRA|
                                              ANYCHANNELS|ANYSPACE|ANYPREMUL, UnrollDoublesToFloat),

    new(     TYPE_LabV2_8,                                                   0,  UnrollLabV2_8ToFloat ),
    new(     TYPE_ALabV2_8,                                                  0,  UnrollALabV2_8ToFloat ),
    new(     TYPE_LabV2_16,                                                  0,  UnrollLabV2_16ToFloat ),

    new(     BYTES_SH(1),              ANYPLANAR|ANYSWAPFIRST|ANYSWAP|ANYEXTRA|
                                                        ANYCHANNELS|ANYSPACE, Unroll8ToFloat),

    new(     BYTES_SH(2),              ANYPLANAR|ANYSWAPFIRST|ANYSWAP|ANYEXTRA|
                                                        ANYCHANNELS|ANYSPACE, Unroll16ToFloat),
    new(     FLOAT_SH(1)|BYTES_SH(2), ANYPLANAR|ANYSWAPFIRST|ANYSWAP|ANYEXTRA|
                                                        ANYCHANNELS|ANYSPACE, UnrollHalfToFloat),
};

    internal static readonly Formatters16[] OutputFormatters16 = {
    //    Type                                          Mask                  Function
    //  ----------------------------   ------------------------------------  ----------------------------

    new( TYPE_Lab_DBL,                                      ANYPLANAR|ANYEXTRA,  PackLabDoubleFrom16),
    new( TYPE_XYZ_DBL,                                      ANYPLANAR|ANYEXTRA,  PackXYZDoubleFrom16),

    new( TYPE_Lab_FLT,                                      ANYPLANAR|ANYEXTRA,  PackLabFloatFrom16),
    new( TYPE_XYZ_FLT,                                      ANYPLANAR|ANYEXTRA,  PackXYZFloatFrom16),

    new( FLOAT_SH(1)|BYTES_SH(0),      ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|
                                    ANYCHANNELS|ANYPLANAR|ANYEXTRA|ANYSPACE,  PackDoubleFrom16),
    new( FLOAT_SH(1)|BYTES_SH(4),      ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|
                                    ANYCHANNELS|ANYPLANAR|ANYEXTRA|ANYSPACE,  PackFloatFrom16),
    new( FLOAT_SH(1)|BYTES_SH(2),      ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|
                                    ANYCHANNELS|ANYPLANAR|ANYEXTRA|ANYSPACE,  PackHalfFrom16),

    new( CHANNELS_SH(1)|BYTES_SH(1),                                  ANYSPACE,  Pack1Byte),
    new( CHANNELS_SH(1)|BYTES_SH(1)|EXTRA_SH(1),                      ANYSPACE,  Pack1ByteSkip1),
    new( CHANNELS_SH(1)|BYTES_SH(1)|EXTRA_SH(1)|SWAPFIRST_SH(1),      ANYSPACE,  Pack1ByteSkip1SwapFirst),

    new( CHANNELS_SH(1)|BYTES_SH(1)|FLAVOR_SH(1),                     ANYSPACE,  Pack1ByteReversed),

    new( TYPE_LabV2_8,                                                       0,  PackLabV2_8 ),
    new( TYPE_ALabV2_8,                                                      0,  PackALabV2_8 ),
    new( TYPE_LabV2_16,                                                      0,  PackLabV2_16 ),

    new( CHANNELS_SH(3)|BYTES_SH(1)|OPTIMIZED_SH(1),                  ANYSPACE,  Pack3BytesOptimized),
    new( CHANNELS_SH(3)|BYTES_SH(1)|EXTRA_SH(1)|OPTIMIZED_SH(1),      ANYSPACE,  Pack3BytesAndSkip1Optimized),
    new( CHANNELS_SH(3)|BYTES_SH(1)|EXTRA_SH(1)|SWAPFIRST_SH(1)|OPTIMIZED_SH(1),
                                                                   ANYSPACE,  Pack3BytesAndSkip1SwapFirstOptimized),
    new( CHANNELS_SH(3)|BYTES_SH(1)|EXTRA_SH(1)|DOSWAP_SH(1)|SWAPFIRST_SH(1)|OPTIMIZED_SH(1),
                                                                   ANYSPACE,  Pack3BytesAndSkip1SwapSwapFirstOptimized),
    new( CHANNELS_SH(3)|BYTES_SH(1)|DOSWAP_SH(1)|EXTRA_SH(1)|OPTIMIZED_SH(1),
                                                                   ANYSPACE,  Pack3BytesAndSkip1SwapOptimized),
    new( CHANNELS_SH(3)|BYTES_SH(1)|DOSWAP_SH(1)|OPTIMIZED_SH(1),     ANYSPACE,  Pack3BytesSwapOptimized),

    new( CHANNELS_SH(3)|BYTES_SH(1),                                  ANYSPACE,  Pack3Bytes),
    new( CHANNELS_SH(3)|BYTES_SH(1)|EXTRA_SH(1),                      ANYSPACE,  Pack3BytesAndSkip1),
    new( CHANNELS_SH(3)|BYTES_SH(1)|EXTRA_SH(1)|SWAPFIRST_SH(1),      ANYSPACE,  Pack3BytesAndSkip1SwapFirst),
    new( CHANNELS_SH(3)|BYTES_SH(1)|EXTRA_SH(1)|DOSWAP_SH(1)|SWAPFIRST_SH(1),
                                                                   ANYSPACE,  Pack3BytesAndSkip1SwapSwapFirst),
    new( CHANNELS_SH(3)|BYTES_SH(1)|DOSWAP_SH(1)|EXTRA_SH(1),         ANYSPACE,  Pack3BytesAndSkip1Swap),
    new( CHANNELS_SH(3)|BYTES_SH(1)|DOSWAP_SH(1),                     ANYSPACE,  Pack3BytesSwap),
    new( CHANNELS_SH(4)|BYTES_SH(1),                                  ANYSPACE,  Pack4Bytes),
    new( CHANNELS_SH(4)|BYTES_SH(1)|FLAVOR_SH(1),                     ANYSPACE,  Pack4BytesReverse),
    new( CHANNELS_SH(4)|BYTES_SH(1)|SWAPFIRST_SH(1),                  ANYSPACE,  Pack4BytesSwapFirst),
    new( CHANNELS_SH(4)|BYTES_SH(1)|DOSWAP_SH(1),                     ANYSPACE,  Pack4BytesSwap),
    new( CHANNELS_SH(4)|BYTES_SH(1)|DOSWAP_SH(1)|SWAPFIRST_SH(1),     ANYSPACE,  Pack4BytesSwapSwapFirst),
    new( CHANNELS_SH(6)|BYTES_SH(1),                                  ANYSPACE,  Pack6Bytes),
    new( CHANNELS_SH(6)|BYTES_SH(1)|DOSWAP_SH(1),                     ANYSPACE,  Pack6BytesSwap),

    new( BYTES_SH(1),    ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|ANYEXTRA|ANYCHANNELS|
                                                          ANYSPACE|ANYPREMUL, PackChunkyBytes),

    new( BYTES_SH(1)|PLANAR_SH(1),    ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|ANYEXTRA|
                                              ANYCHANNELS|ANYSPACE|ANYPREMUL, PackPlanarBytes),

    new( CHANNELS_SH(1)|BYTES_SH(2),                                  ANYSPACE,  Pack1Word),
    new( CHANNELS_SH(1)|BYTES_SH(2)|EXTRA_SH(1),                      ANYSPACE,  Pack1WordSkip1),
    new( CHANNELS_SH(1)|BYTES_SH(2)|EXTRA_SH(1)|SWAPFIRST_SH(1),      ANYSPACE,  Pack1WordSkip1SwapFirst),
    new( CHANNELS_SH(1)|BYTES_SH(2)|FLAVOR_SH(1),                     ANYSPACE,  Pack1WordReversed),
    new( CHANNELS_SH(1)|BYTES_SH(2)|ENDIAN16_SH(1),                   ANYSPACE,  Pack1WordBigEndian),
    new( CHANNELS_SH(3)|BYTES_SH(2),                                  ANYSPACE,  Pack3Words),
    new( CHANNELS_SH(3)|BYTES_SH(2)|DOSWAP_SH(1),                     ANYSPACE,  Pack3WordsSwap),
    new( CHANNELS_SH(3)|BYTES_SH(2)|ENDIAN16_SH(1),                   ANYSPACE,  Pack3WordsBigEndian),
    new( CHANNELS_SH(3)|BYTES_SH(2)|EXTRA_SH(1),                      ANYSPACE,  Pack3WordsAndSkip1),
    new( CHANNELS_SH(3)|BYTES_SH(2)|EXTRA_SH(1)|DOSWAP_SH(1),         ANYSPACE,  Pack3WordsAndSkip1Swap),
    new( CHANNELS_SH(3)|BYTES_SH(2)|EXTRA_SH(1)|SWAPFIRST_SH(1),      ANYSPACE,  Pack3WordsAndSkip1SwapFirst),

    new( CHANNELS_SH(3)|BYTES_SH(2)|EXTRA_SH(1)|DOSWAP_SH(1)|SWAPFIRST_SH(1),
                                                                   ANYSPACE,  Pack3WordsAndSkip1SwapSwapFirst),

    new( CHANNELS_SH(4)|BYTES_SH(2),                                  ANYSPACE,  Pack4Words),
    new( CHANNELS_SH(4)|BYTES_SH(2)|FLAVOR_SH(1),                     ANYSPACE,  Pack4WordsReverse),
    new( CHANNELS_SH(4)|BYTES_SH(2)|DOSWAP_SH(1),                     ANYSPACE,  Pack4WordsSwap),
    new( CHANNELS_SH(4)|BYTES_SH(2)|ENDIAN16_SH(1),                   ANYSPACE,  Pack4WordsBigEndian),

    new( CHANNELS_SH(6)|BYTES_SH(2),                                  ANYSPACE,  Pack6Words),
    new( CHANNELS_SH(6)|BYTES_SH(2)|DOSWAP_SH(1),                     ANYSPACE,  Pack6WordsSwap),

    new( BYTES_SH(2),                  ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|ANYENDIAN|
                                     ANYEXTRA|ANYCHANNELS|ANYSPACE|ANYPREMUL, PackChunkyWords),
    new( BYTES_SH(2)|PLANAR_SH(1),     ANYFLAVOR|ANYENDIAN|ANYSWAP|ANYEXTRA|
                                     ANYCHANNELS|ANYSPACE|ANYPREMUL,          PackPlanarWords)
    };

    internal static readonly FormattersFloat[] OutputFormattersFloat = {
    //    Type                                          Mask                                 Function
    //  ----------------------------   ---------------------------------------------------  ----------------------------
    new(     TYPE_Lab_FLT,                                                ANYPLANAR|ANYEXTRA,   PackLabFloatFromFloat),
    new(     TYPE_XYZ_FLT,                                                ANYPLANAR|ANYEXTRA,   PackXYZFloatFromFloat),

    new(     TYPE_Lab_DBL,                                                ANYPLANAR|ANYEXTRA,   PackLabDoubleFromFloat),
    new(     TYPE_XYZ_DBL,                                                ANYPLANAR|ANYEXTRA,   PackXYZDoubleFromFloat),

    new(     FLOAT_SH(1)|BYTES_SH(4), ANYPLANAR|
                             ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|ANYEXTRA|ANYCHANNELS|ANYSPACE,   PackFloatsFromFloat ),
    new(     FLOAT_SH(1)|BYTES_SH(0), ANYPLANAR|
                             ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|ANYEXTRA|ANYCHANNELS|ANYSPACE,   PackDoublesFromFloat ),
    new(     FLOAT_SH(1)|BYTES_SH(2),
                             ANYFLAVOR|ANYSWAPFIRST|ANYSWAP|ANYEXTRA|ANYCHANNELS|ANYSPACE,   PackHalfFromFloat ),
};

    internal struct Formatters16
    {
        public uint Type;
        public uint Mask;
        public Formatter16 Frm;

        public Formatters16(uint Type, uint Mask, Formatter16 Frm)
        {
            this.Type = Type;
            this.Mask = Mask;
            this.Frm = Frm;
        }
    }

    internal struct FormattersFloat
    {
        public uint Type;
        public uint Mask;
        public FormatterFloat Frm;

        public FormattersFloat(uint Type, uint Mask, FormatterFloat Frm)
        {
            this.Type = Type;
            this.Mask = Mask;
            this.Frm = Frm;
        }
    }

    private static uint ANYSPACE => COLORSPACE_SH(31);
    private static uint ANYCHANNELS => CHANNELS_SH(15u);
    private static uint ANYEXTRA => EXTRA_SH(7u);
    private static uint ANYPLANAR => PLANAR_SH(1);
    private static uint ANYENDIAN => ENDIAN16_SH(1);
    private static uint ANYSWAP => DOSWAP_SH(1);
    private static uint ANYSWAPFIRST => SWAPFIRST_SH(1);
    private static uint ANYFLAVOR => FLAVOR_SH(1);
    private static uint ANYPREMUL => PREMUL_SH(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort CHANGE_ENDIAN(ushort w) =>
        (ushort)((w << 8) | (w >> 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte REVERSE_FLAVOR_8(byte x) =>
        (byte)(0xFF - x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort REVERSE_FLAVOR_16(ushort x) =>
        (ushort)(0xFFFF - x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort FomLabV2ToLabV4(ushort x)
    {
        var a = ((x << 8) | x) >> 8;
        return
            (a > 0xFFFF)
                ? (ushort)0xFFFF
                : (ushort)a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort FomLabV4ToLabV2(ushort x) =>
        (ushort)(((x << 8) + 0x80) / 257);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (uint nChan, bool DoSwap, bool Reverse, bool SwapFirst, uint Extra, bool Planar, bool Premul, bool SwapEndian) T_BREAK(uint m) =>
        (T_CHANNELS(m), T_DOSWAP(m) is not 0, T_FLAVOR(m) is not 0, T_SWAPFIRST(m) is not 0, T_EXTRA(m), T_PLANAR(m) is not 0, T_PREMUL(m) is not 0, T_ENDIAN16(m) is not 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PackSwapFirst<T>(T* swap1, uint nChan) where T : unmanaged
    {
        var tmp = swap1[nChan - 1];

        memmove(swap1 + 1, swap1, (nChan - 1) * _sizeof<T>());
        swap1[0] = tmp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UnrollSwapFirst<T>(T* wIn, uint nChan) where T : unmanaged
    {
        var tmp = wIn[0];

        memmove(&wIn[0], &wIn[1], (nChan - 1) * _sizeof<T>());
        wIn[nChan - 1] = tmp;
    }

    private static byte* UnrollChunkyBytes(Transform* info, ushort* wIn, byte* accum, uint _)
    {
        var nChan = T_CHANNELS(info->InputFormat);
        var DoSwap = T_DOSWAP(info->InputFormat) is not 0;
        var Reverse = T_FLAVOR(info->InputFormat) is not 0;
        var SwapFirst = T_SWAPFIRST(info->InputFormat) is not 0;
        var Extra = T_EXTRA(info->InputFormat);
        var Premul = T_PREMUL(info->InputFormat) is not 0;

        var ExtraFirst = DoSwap ^ SwapFirst;
        var alpha_factor = 1u;

        if (ExtraFirst)
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(FROM_8_TO_16(accum[0]));

            accum += Extra;
        }
        else
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(FROM_8_TO_16(accum[nChan]));
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = (uint)FROM_8_TO_16(*accum);
            v = Reverse ? REVERSE_FLAVOR_16((ushort)v) : v;

            if (Premul && alpha_factor > 0)
            {
                v = (v << 16) / alpha_factor;
                if (v > 0xFFFF) v = 0xFFFF;
            }

            wIn[index] = (ushort)v;
            accum++;
        }

        if (!ExtraFirst)
            accum += Extra;

        if (Extra is 0 && SwapFirst)
        {
            var tmp = wIn[0];

            memmove(&wIn[0], &wIn[1], (nChan - 1) * _sizeof<ushort>());
            wIn[nChan - 1] = tmp;
        }

        return accum;
    }

    private static byte* UnrollPlanarBytes(Transform* info, ushort* wIn, byte* accum, uint Stride)
    {
        var nChan = T_CHANNELS(info->InputFormat);
        var DoSwap = T_DOSWAP(info->InputFormat) is not 0;
        var SwapFirst = T_SWAPFIRST(info->InputFormat) is not 0;
        var Reverse = T_FLAVOR(info->InputFormat) is not 0;
        var ExtraFirst = DoSwap ^ SwapFirst;
        var Extra = T_EXTRA(info->InputFormat);
        var Premul = T_PREMUL(info->InputFormat) is not 0;
        var Init = accum;
        var alpha_factor = 1u;

        if (ExtraFirst)
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(FROM_8_TO_16(accum[0]));

            accum += Extra * Stride;
        }
        else
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(FROM_8_TO_16(accum[nChan * Stride]));
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);
            var v = (uint)FROM_8_TO_16(*accum);

            v = Reverse ? REVERSE_FLAVOR_16((ushort)v) : v;

            if (Premul && alpha_factor > 0)
            {
                v = (v << 16) / alpha_factor;
                if (v > 0xFFFF) v = 0xFFFF;
            }

            wIn[index] = (ushort)v;
            accum += Stride;
        }

        return Init + 1;
    }

    private static byte* Unroll4Bytes(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = FROM_8_TO_16(*accum); accum++; // C
        wIn[1] = FROM_8_TO_16(*accum); accum++; // M
        wIn[2] = FROM_8_TO_16(*accum); accum++; // Y
        wIn[3] = FROM_8_TO_16(*accum); accum++; // K

        return accum;
    }

    private static byte* Unroll4BytesReverse(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = FROM_8_TO_16(REVERSE_FLAVOR_8(*accum)); accum++; // C
        wIn[1] = FROM_8_TO_16(REVERSE_FLAVOR_8(*accum)); accum++; // M
        wIn[2] = FROM_8_TO_16(REVERSE_FLAVOR_8(*accum)); accum++; // Y
        wIn[3] = FROM_8_TO_16(REVERSE_FLAVOR_8(*accum)); accum++; // K

        return accum;
    }

    private static byte* Unroll4BytesSwapFirst(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[3] = FROM_8_TO_16(*accum); accum++; // K
        wIn[0] = FROM_8_TO_16(*accum); accum++; // C
        wIn[1] = FROM_8_TO_16(*accum); accum++; // M
        wIn[2] = FROM_8_TO_16(*accum); accum++; // Y

        return accum;
    }

    private static byte* Unroll4BytesSwap(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[3] = FROM_8_TO_16(*accum); accum++; // K
        wIn[2] = FROM_8_TO_16(*accum); accum++; // Y
        wIn[1] = FROM_8_TO_16(*accum); accum++; // M
        wIn[0] = FROM_8_TO_16(*accum); accum++; // C

        return accum;
    }

    private static byte* Unroll4BytesSwapSwapFirst(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[2] = FROM_8_TO_16(*accum); accum++; // Y
        wIn[1] = FROM_8_TO_16(*accum); accum++; // M
        wIn[0] = FROM_8_TO_16(*accum); accum++; // C
        wIn[3] = FROM_8_TO_16(*accum); accum++; // K

        return accum;
    }

    private static byte* Unroll3Bytes(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = FROM_8_TO_16(*accum); accum++; // R
        wIn[1] = FROM_8_TO_16(*accum); accum++; // G
        wIn[2] = FROM_8_TO_16(*accum); accum++; // B

        return accum;
    }

    private static byte* Unroll3BytesSkip1Swap(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        accum++;                                // A
        wIn[2] = FROM_8_TO_16(*accum); accum++; // B
        wIn[1] = FROM_8_TO_16(*accum); accum++; // G
        wIn[0] = FROM_8_TO_16(*accum); accum++; // R

        return accum;
    }

    private static byte* Unroll3BytesSkip1SwapSwapFirst(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[2] = FROM_8_TO_16(*accum); accum++; // B
        wIn[1] = FROM_8_TO_16(*accum); accum++; // G
        wIn[0] = FROM_8_TO_16(*accum); accum++; // R
        accum++;                                // A

        return accum;
    }

    private static byte* Unroll3BytesSkip1SwapFirst(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        accum++;                                // A
        wIn[0] = FROM_8_TO_16(*accum); accum++; // R
        wIn[1] = FROM_8_TO_16(*accum); accum++; // G
        wIn[2] = FROM_8_TO_16(*accum); accum++; // B

        return accum;
    }

    private static byte* Unroll3BytesSwap(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[2] = FROM_8_TO_16(*accum); accum++; // B
        wIn[1] = FROM_8_TO_16(*accum); accum++; // G
        wIn[0] = FROM_8_TO_16(*accum); accum++; // R

        return accum;
    }

    private static byte* UnrollLabV2_8(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++; // L
        wIn[1] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++; // a
        wIn[2] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++; // b

        return accum;
    }

    private static byte* UnrollALabV2_8(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        accum++;                                // A
        wIn[0] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++; // R
        wIn[1] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++; // G
        wIn[2] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++; // B

        return accum;
    }

    private static byte* UnrollLabV2_16(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = FomLabV2ToLabV4(*(ushort*)accum); accum += 2; // L
        wIn[1] = FomLabV2ToLabV4(*(ushort*)accum); accum += 2; // a
        wIn[2] = FomLabV2ToLabV4(*(ushort*)accum); accum += 2; // b

        return accum;
    }

    private static byte* Unroll2Bytes(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = FROM_8_TO_16(*accum); accum++; // ch1
        wIn[1] = FROM_8_TO_16(*accum); accum++; // ch2

        return accum;
    }

    private static byte* Unroll1Byte(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = wIn[1] = wIn[2] = FROM_8_TO_16(*accum); accum++; // L

        return accum;
    }

    private static byte* Unroll1ByteSkip1(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = wIn[1] = wIn[2] = FROM_8_TO_16(*accum); accum++; // L
        accum++;

        return accum;
    }

    private static byte* Unroll1ByteSkip2(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = wIn[1] = wIn[2] = FROM_8_TO_16(*accum); accum++; // L
        accum += 2;

        return accum;
    }

    private static byte* Unroll1ByteReversed(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = wIn[1] = wIn[2] = REVERSE_FLAVOR_16(FROM_8_TO_16(*accum)); accum++; // L

        return accum;
    }

    private static byte* UnrollAnyWords(Transform* info, ushort* wIn, byte* accum, uint _)
    {
        var nChan = T_CHANNELS(info->InputFormat);
        var SwapEndian = T_ENDIAN16(info->InputFormat) is not 0;
        var DoSwap = T_DOSWAP(info->InputFormat) is not 0;
        var Reverse = T_FLAVOR(info->InputFormat) is not 0;
        var SwapFirst = T_SWAPFIRST(info->InputFormat) is not 0;
        var Extra = T_EXTRA(info->InputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;

        if (ExtraFirst)
            accum += Extra * _sizeof<ushort>();

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);
            var v = (uint)*(ushort*)accum;

            if (SwapEndian)
                v = CHANGE_ENDIAN((ushort)v);

            wIn[index] = Reverse ? REVERSE_FLAVOR_16((ushort)v) : (ushort)v;

            accum += _sizeof<ushort>();
        }

        if (!ExtraFirst)
            accum += Extra * _sizeof<ushort>();

        if (Extra is 0 && SwapFirst)
        {
            var tmp = wIn[0];

            memmove(&wIn[0], &wIn[1], (nChan - 1) * _sizeof<ushort>());
            wIn[nChan - 1] = tmp;
        }

        return accum;
    }

    private static byte* UnrollAnyWordsPremul(Transform* info, ushort* wIn, byte* accum, uint _)
    {
        var nChan = T_CHANNELS(info->InputFormat);
        var SwapEndian = T_ENDIAN16(info->InputFormat) is not 0;
        var DoSwap = T_DOSWAP(info->InputFormat) is not 0;
        var Reverse = T_FLAVOR(info->InputFormat) is not 0;
        var SwapFirst = T_SWAPFIRST(info->InputFormat) is not 0;
        var ExtraFirst = DoSwap ^ SwapFirst;

        var alpha = (uint)(ExtraFirst ? accum[0] : accum[nChan - 1]);
        var alpha_factor = (uint)_cmsToFixedDomain(FROM_8_TO_16(alpha));

        if (ExtraFirst)
            accum += _sizeof<ushort>();

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);
            var v = (uint)*(ushort*)accum;

            if (SwapEndian)
                v = CHANGE_ENDIAN((ushort)v);

            v = (v << 16) / alpha_factor;
            if (v > 0xFFFF) v = 0xFFFF;

            wIn[index] = Reverse ? REVERSE_FLAVOR_16((ushort)v) : (ushort)v;

            accum += _sizeof<ushort>();
        }

        if (!ExtraFirst)
            accum += _sizeof<ushort>();

        return accum;
    }

    private static byte* UnrollPlanarWords(Transform* info, ushort* wIn, byte* accum, uint Stride)
    {
        var nChan = T_CHANNELS(info->InputFormat);
        var SwapEndian = T_ENDIAN16(info->InputFormat) is not 0;
        var DoSwap = T_DOSWAP(info->InputFormat) is not 0;
        var Reverse = T_FLAVOR(info->InputFormat) is not 0;
        var Extra = T_EXTRA(info->InputFormat);
        var Init = accum;

        if (DoSwap)
            accum += Extra * Stride;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);
            var v = *(ushort*)accum;

            if (SwapEndian)
                v = CHANGE_ENDIAN(v);

            wIn[index] = Reverse ? REVERSE_FLAVOR_16(v) : v;

            accum += Stride;
        }

        return Init + _sizeof<ushort>();
    }

    private static byte* UnrollPlanarWordsPremul(Transform* info, ushort* wIn, byte* accum, uint Stride)
    {
        var nChan = T_CHANNELS(info->InputFormat);
        var SwapEndian = T_ENDIAN16(info->InputFormat) is not 0;
        var DoSwap = T_DOSWAP(info->InputFormat) is not 0;
        var Reverse = T_FLAVOR(info->InputFormat) is not 0;
        var SwapFirst = T_SWAPFIRST(info->InputFormat) is not 0;
        var ExtraFirst = DoSwap ^ SwapFirst;
        var Init = accum;

        var alpha = (ushort)(ExtraFirst ? accum[0] : accum[(nChan - 1) * Stride]);
        var alpha_factor = (uint)_cmsToFixedDomain(FROM_8_TO_16(alpha));

        if (ExtraFirst)
            accum += Stride;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);
            var v = (uint)*(ushort*)accum;

            if (SwapEndian)
                v = CHANGE_ENDIAN((ushort)v);

            v = (v << 16) / alpha_factor;
            if (v > 0xFFFF) v = 0xFFFF;

            wIn[index] = Reverse ? REVERSE_FLAVOR_16((ushort)v) : (ushort)v;

            accum += Stride;
        }

        return Init + _sizeof<ushort>();
    }

    private static byte* Unroll4Words(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = *(ushort*)accum; accum += 2; // C
        wIn[1] = *(ushort*)accum; accum += 2; // M
        wIn[2] = *(ushort*)accum; accum += 2; // Y
        wIn[3] = *(ushort*)accum; accum += 2; // K

        return accum;
    }

    private static byte* Unroll4WordsReverse(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = REVERSE_FLAVOR_16(*(ushort*)accum); accum += 2; // C
        wIn[1] = REVERSE_FLAVOR_16(*(ushort*)accum); accum += 2; // M
        wIn[2] = REVERSE_FLAVOR_16(*(ushort*)accum); accum += 2; // Y
        wIn[3] = REVERSE_FLAVOR_16(*(ushort*)accum); accum += 2; // K

        return accum;
    }

    private static byte* Unroll4WordsSwapFirst(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[3] = *(ushort*)accum; accum += 2; // K
        wIn[0] = *(ushort*)accum; accum += 2; // C
        wIn[1] = *(ushort*)accum; accum += 2; // M
        wIn[2] = *(ushort*)accum; accum += 2; // Y

        return accum;
    }

    private static byte* Unroll4WordsSwap(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[3] = *(ushort*)accum; accum += 2; // K
        wIn[2] = *(ushort*)accum; accum += 2; // Y
        wIn[1] = *(ushort*)accum; accum += 2; // M
        wIn[0] = *(ushort*)accum; accum += 2; // C

        return accum;
    }

    private static byte* Unroll4WordsSwapSwapFirst(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[2] = *(ushort*)accum; accum += 2; // Y
        wIn[1] = *(ushort*)accum; accum += 2; // M
        wIn[0] = *(ushort*)accum; accum += 2; // C
        wIn[3] = *(ushort*)accum; accum += 2; // K

        return accum;
    }

    private static byte* Unroll3Words(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = *(ushort*)accum; accum += 2; // C R
        wIn[1] = *(ushort*)accum; accum += 2; // M G
        wIn[2] = *(ushort*)accum; accum += 2; // Y B

        return accum;
    }

    private static byte* Unroll3WordsSwap(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[2] = *(ushort*)accum; accum += 2; // Y B
        wIn[1] = *(ushort*)accum; accum += 2; // M G
        wIn[0] = *(ushort*)accum; accum += 2; // C R

        return accum;
    }

    private static byte* Unroll3WordsSkip1Swap(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        accum += 2;                           // A
        wIn[2] = *(ushort*)accum; accum += 2; // B
        wIn[1] = *(ushort*)accum; accum += 2; // G
        wIn[0] = *(ushort*)accum; accum += 2; // R

        return accum;
    }

    private static byte* Unroll3WordsSkip1SwapFirst(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        accum += 2;                           // A
        wIn[0] = *(ushort*)accum; accum += 2; // R
        wIn[1] = *(ushort*)accum; accum += 2; // G
        wIn[2] = *(ushort*)accum; accum += 2; // B

        return accum;
    }

    private static byte* Unroll1Word(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = wIn[1] = wIn[2] = *(ushort*)accum; accum += 2; // L

        return accum;
    }

    private static byte* Unroll1WordReversed(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = wIn[1] = wIn[2] = REVERSE_FLAVOR_16(*(ushort*)accum); accum += 2; // L

        return accum;
    }

    private static byte* Unroll1WordSkip3(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = wIn[1] = wIn[2] = *(ushort*)accum;

        accum += 8;

        return accum;
    }

    private static byte* Unroll2Words(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        wIn[0] = *(ushort*)accum; accum += 2; // ch1
        wIn[1] = *(ushort*)accum; accum += 2; // ch2

        return accum;
    }

    private static byte* UnrollLabDoubleTo16(Transform* info, ushort* wIn, byte* accum, uint Stride)
    {
        if (T_PLANAR(info->InputFormat) is not 0)
        {
            CIELab Lab;

            var pos_L = accum;
            var pos_a = accum + Stride;
            var pos_b = accum + (Stride * 2);

            Lab.L = *(double*)pos_L;
            Lab.a = *(double*)pos_a;
            Lab.b = *(double*)pos_b;

            cmsFloat2LabEncoded(wIn, &Lab);
            return accum + _sizeof<double>();
        }
        else
        {
            cmsFloat2LabEncoded(wIn, (CIELab*)accum);
            accum += _sizeof<CIELab>() + (T_EXTRA(info->InputFormat) * _sizeof<double>());
            return accum;
        }
    }

    private static byte* UnrollLabFloatTo16(Transform* info, ushort* wIn, byte* accum, uint Stride)
    {
        CIELab Lab;

        if (T_PLANAR(info->InputFormat) is not 0)
        {
            var pos_L = accum;
            var pos_a = accum + Stride;
            var pos_b = accum + (Stride * 2);

            Lab.L = *(float*)pos_L;
            Lab.a = *(float*)pos_a;
            Lab.b = *(float*)pos_b;

            cmsFloat2LabEncoded(wIn, &Lab);
            return accum + _sizeof<float>();
        }
        else
        {
            Lab.L = ((float*)accum)[0];
            Lab.a = ((float*)accum)[1];
            Lab.b = ((float*)accum)[2];

            cmsFloat2LabEncoded(wIn, &Lab);
            accum += (3 + T_EXTRA(info->InputFormat)) * _sizeof<float>();
            return accum;
        }
    }

    private static byte* UnrollXYZDoubleTo16(Transform* info, ushort* wIn, byte* accum, uint Stride)
    {
        if (T_PLANAR(info->InputFormat) is not 0)
        {
            CIEXYZ XYZ;

            var pos_X = accum;
            var pos_Y = accum + Stride;
            var pos_Z = accum + (Stride * 2);

            XYZ.X = *(double*)pos_X;
            XYZ.Y = *(double*)pos_Y;
            XYZ.Z = *(double*)pos_Z;

            cmsFloat2XYZEncoded(wIn, &XYZ);
            return accum + _sizeof<double>();
        }
        else
        {
            cmsFloat2XYZEncoded(wIn, (CIEXYZ*)accum);
            accum += _sizeof<CIEXYZ>() + (T_EXTRA(info->InputFormat) * _sizeof<double>());
            return accum;
        }
    }

    private static byte* UnrollXYZFloatTo16(Transform* info, ushort* wIn, byte* accum, uint Stride)
    {
        CIEXYZ XYZ;

        if (T_PLANAR(info->InputFormat) is not 0)
        {
            var pos_X = accum;
            var pos_Y = accum + Stride;
            var pos_Z = accum + (Stride * 2);

            XYZ.X = *(float*)pos_X;
            XYZ.Y = *(float*)pos_Y;
            XYZ.Z = *(float*)pos_Z;

            cmsFloat2XYZEncoded(wIn, &XYZ);
            return accum + _sizeof<float>();
        }
        else
        {
            var pt = (float*)accum;

            XYZ.X = pt[0];
            XYZ.Y = pt[1];
            XYZ.Z = pt[2];
            cmsFloat2XYZEncoded(wIn, &XYZ);
            accum += (3 + T_EXTRA(info->InputFormat)) * _sizeof<float>();
            return accum;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInkSpace(uint Type) =>
        T_COLORSPACE(Type) switch
        {
            PT_CMY or
            PT_CMYK or
            PT_MCH5 or
            PT_MCH6 or
            PT_MCH7 or
            PT_MCH8 or
            PT_MCH9 or
            PT_MCH10 or
            PT_MCH11 or
            PT_MCH12 or
            PT_MCH13 or
            PT_MCH14 or
            PT_MCH15 => true,
            _ => false,
        };

    private static uint PixelSize(uint Format)
    {
        var fmt_bytes = T_BYTES(Format);

        // For double, the T_BYTES field is zero
        if (fmt_bytes is 0)
            return _sizeof<double>();

        // Otherwise, it is already correct for all formats
        return fmt_bytes;
    }

    private static byte* UnrollDoubleTo16(Transform* info, ushort* wIn, byte* accum, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, _) = T_BREAK(info->InputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var start = 0u;
        var maximum = IsInkSpace(info->InputFormat) ? 655.35 : 65535.0;

        Stride /= PixelSize(info->InputFormat);

        if (ExtraFirst) start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = (float)(Planar
                ? ((double*)accum)[(i + start) * Stride]
                : ((double*)accum)[i + start]);

            var vi = _cmsQuickSaturateWord(v * maximum);

            if (Reverse)
                vi = REVERSE_FLAVOR_16(vi);

            wIn[index] = vi;
        }

        if (Extra is 0 && SwapFirst) UnrollSwapFirst(wIn, nChan);

        return accum + ((T_PLANAR(info->InputFormat) is not 0 ? 1 : nChan + Extra) * _sizeof<double>());
    }

    private static byte* UnrollFloatTo16(Transform* info, ushort* wIn, byte* accum, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, _) = T_BREAK(info->InputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var start = 0u;
        var maximum = IsInkSpace(info->InputFormat) ? 655.35 : 65535.0;

        Stride /= PixelSize(info->InputFormat);

        if (ExtraFirst) start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = Planar
                ? ((float*)accum)[(i + start) * Stride]
                : ((float*)accum)[i + start];

            var vi = _cmsQuickSaturateWord(v * maximum);

            if (Reverse)
                vi = REVERSE_FLAVOR_16(vi);

            wIn[index] = vi;
        }

        if (Extra is 0 && SwapFirst) UnrollSwapFirst(wIn, nChan);

        return accum + ((T_PLANAR(info->InputFormat) is not 0 ? 1 : nChan + Extra) * _sizeof<float>());
    }

    private static byte* UnrollDouble1Chan(Transform* _1, ushort* wIn, byte* accum, uint _2)
    {
        var Inks = (double*)accum;

        wIn[0] = wIn[1] = wIn[2] = _cmsQuickSaturateWord(Inks[0] * 65535.0);

        return accum + _sizeof<double>();
    }

    private static byte* Unroll8ToFloat(Transform* info, float* wIn, byte* accum, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, _) = T_BREAK(info->InputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var start = 0u;

        Stride /= PixelSize(info->InputFormat);

        if (ExtraFirst) start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = (float)(Planar
                ? accum[(i + start) * Stride]
                : accum[i + start]);

            v /= 255.0F;

            wIn[index] = Reverse ? 1 - v : v;
        }

        if (Extra is 0 && SwapFirst) UnrollSwapFirst(wIn, nChan);

        return accum + ((T_PLANAR(info->InputFormat) is not 0 ? 1 : nChan + Extra) * _sizeof<byte>());
    }

    private static byte* Unroll16ToFloat(Transform* info, float* wIn, byte* accum, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, _) = T_BREAK(info->InputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var start = 0u;

        Stride /= PixelSize(info->InputFormat);

        if (ExtraFirst) start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = (float)(Planar
                ? ((ushort*)accum)[(i + start) * Stride]
                : ((ushort*)accum)[i + start]);

            v /= 65535.0F;

            wIn[index] = Reverse ? 1 - v : v;
        }

        if (Extra is 0 && SwapFirst) UnrollSwapFirst(wIn, nChan);

        return accum + ((T_PLANAR(info->InputFormat) is not 0 ? 1 : nChan + Extra) * _sizeof<ushort>());
    }

    private static byte* UnrollFloatsToFloat(Transform* info, float* wIn, byte* accum, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, Premul, _) = T_BREAK(info->InputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var start = 0u;
        var maximum = IsInkSpace(info->InputFormat) ? 100.0F : 1.0F;
        var alpha_factor = 1.0F;
        var ptr = (float*)accum;

        Stride /= PixelSize(info->InputFormat);

        if (Premul && Extra is not 0)
            alpha_factor = (ExtraFirst ? ptr[0] : ptr[nChan * (Planar ? Stride : 1)]) / maximum;

        if (ExtraFirst) start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = ptr[(i + start) * (Planar ? Stride : 1)];

            if (Premul && alpha_factor > 0)
                v /= alpha_factor;

            v /= maximum;

            wIn[index] = Reverse ? 1 - v : v;
        }

        if (Extra is 0 && SwapFirst) UnrollSwapFirst(wIn, nChan);

        return accum + ((T_PLANAR(info->InputFormat) is not 0 ? 1 : nChan + Extra) * _sizeof<float>());
    }

    private static byte* UnrollDoublesToFloat(Transform* info, float* wIn, byte* accum, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, Premul, _) = T_BREAK(info->InputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var start = 0u;
        var maximum = IsInkSpace(info->InputFormat) ? 100.0 : 1.0;
        var alpha_factor = 1.0;
        var ptr = (double*)accum;

        Stride /= PixelSize(info->InputFormat);

        if (Premul && Extra is not 0)
            alpha_factor = (ExtraFirst ? ptr[0] : ptr[nChan * (Planar ? Stride : 1)]) / maximum;

        if (ExtraFirst) start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = ptr[(i + start) * (Planar ? Stride : 1)];

            if (Premul && alpha_factor > 0)
                v /= alpha_factor;

            v /= maximum;

            wIn[index] = (float)(Reverse ? 1 - v : v);
        }

        if (Extra is 0 && SwapFirst) UnrollSwapFirst(wIn, nChan);

        return accum + ((T_PLANAR(info->InputFormat) is not 0 ? 1 : nChan + Extra) * _sizeof<double>());
    }

    private static byte* UnrollLabDoubleToFloat(Transform* info, float* wIn, byte* accum, uint Stride)
    {
        var Pt = (double*)accum;

        if (T_PLANAR(info->InputFormat) is not 0)
        {
            Stride /= PixelSize(info->InputFormat);

            wIn[0] = (float)(Pt[0] / 100.0);
            wIn[1] = (float)((Pt[Stride] + 128) / 255.0);
            wIn[2] = (float)((Pt[Stride * 2] + 128) / 255.0);

            return accum + _sizeof<double>();
        }
        else
        {
            wIn[0] = (float)(Pt[0] / 100.0);
            wIn[1] = (float)((Pt[1] + 128) / 255.0);
            wIn[2] = (float)((Pt[2] + 128) / 255.0);

            return accum + (_sizeof<double>() * (3 + T_EXTRA(info->InputFormat)));
        }
    }

    private static byte* UnrollLabFloatToFloat(Transform* info, float* wIn, byte* accum, uint Stride)
    {
        var Pt = (float*)accum;

        if (T_PLANAR(info->InputFormat) is not 0)
        {
            Stride /= PixelSize(info->InputFormat);

            wIn[0] = (float)(Pt[0] / 100.0);
            wIn[1] = (float)((Pt[Stride] + 128) / 255.0);
            wIn[2] = (float)((Pt[Stride * 2] + 128) / 255.0);

            return accum + _sizeof<float>();
        }
        else
        {
            wIn[0] = (float)(Pt[0] / 100.0);
            wIn[1] = (float)((Pt[1] + 128) / 255.0);
            wIn[2] = (float)((Pt[2] + 128) / 255.0);

            return accum + (_sizeof<float>() * (3 + T_EXTRA(info->InputFormat)));
        }
    }

    private static byte* UnrollXYZDoubleToFloat(Transform* info, float* wIn, byte* accum, uint Stride)
    {
        var Pt = (double*)accum;

        if (T_PLANAR(info->InputFormat) is not 0)
        {
            Stride /= PixelSize(info->InputFormat);

            wIn[0] = (float)(Pt[0] / MAX_ENCODEABLE_XYZ);
            wIn[1] = (float)(Pt[Stride] / MAX_ENCODEABLE_XYZ);
            wIn[2] = (float)(Pt[Stride * 2] / MAX_ENCODEABLE_XYZ);

            return accum + _sizeof<double>();
        }
        else
        {
            wIn[0] = (float)(Pt[0] / MAX_ENCODEABLE_XYZ);
            wIn[1] = (float)(Pt[1] / MAX_ENCODEABLE_XYZ);
            wIn[2] = (float)(Pt[2] / MAX_ENCODEABLE_XYZ);

            return accum + (_sizeof<double>() * (3 + T_EXTRA(info->InputFormat)));
        }
    }

    private static byte* UnrollXYZFloatToFloat(Transform* info, float* wIn, byte* accum, uint Stride)
    {
        var Pt = (float*)accum;

        if (T_PLANAR(info->InputFormat) is not 0)
        {
            Stride /= PixelSize(info->InputFormat);

            wIn[0] = (float)(Pt[0] / MAX_ENCODEABLE_XYZ);
            wIn[1] = (float)(Pt[Stride] / MAX_ENCODEABLE_XYZ);
            wIn[2] = (float)(Pt[Stride * 2] / MAX_ENCODEABLE_XYZ);

            return accum + _sizeof<float>();
        }
        else
        {
            wIn[0] = (float)(Pt[0] / MAX_ENCODEABLE_XYZ);
            wIn[1] = (float)(Pt[1] / MAX_ENCODEABLE_XYZ);
            wIn[2] = (float)(Pt[2] / MAX_ENCODEABLE_XYZ);

            return accum + (_sizeof<float>() * (3 + T_EXTRA(info->InputFormat)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void lab4toFloat(float* wIn, ushort* lab4)
    {
        var L = lab4[0] / 655.35F;
        var a = (lab4[1] / 257.0F) - 128.0F;
        var b = (lab4[2] / 257.0F) - 128.0F;

        wIn[0] = L / 100.0F;
        wIn[1] = (a + 128.0F) / 255.0F;
        wIn[2] = (b + 128.0F) / 255.0F;
    }

    private static byte* UnrollLabV2_8ToFloat(Transform* _1, float* wIn, byte* accum, uint _2)
    {
        var lab4 = stackalloc ushort[3];

        lab4[0] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++;   // L
        lab4[1] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++;   // a
        lab4[2] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++;   // b

        lab4toFloat(wIn, lab4);

        return accum;
    }

    private static byte* UnrollALabV2_8ToFloat(Transform* _1, float* wIn, byte* accum, uint _2)
    {
        var lab4 = stackalloc ushort[3];

        accum++;                                                    // A
        lab4[0] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++;   // L
        lab4[1] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++;   // a
        lab4[2] = FomLabV2ToLabV4(FROM_8_TO_16(*accum)); accum++;   // b

        lab4toFloat(wIn, lab4);

        return accum;
    }

    private static byte* UnrollLabV2_16ToFloat(Transform* _1, float* wIn, byte* accum, uint _2)
    {
        var lab4 = stackalloc ushort[3];

        lab4[0] = FomLabV2ToLabV4(FROM_8_TO_16(*(ushort*)accum)); accum += 2;   // L
        lab4[1] = FomLabV2ToLabV4(FROM_8_TO_16(*(ushort*)accum)); accum += 2;   // a
        lab4[2] = FomLabV2ToLabV4(FROM_8_TO_16(*(ushort*)accum)); accum += 2;   // b

        lab4toFloat(wIn, lab4);

        return accum;
    }

    private static byte* PackChunkyBytes(Transform* info, ushort* wOut, byte* output, uint _)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, _, Premul, _) = T_BREAK(info->OutputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var alpha_factor = 0u;

        var swap1 = output;

        if (ExtraFirst)
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(FROM_8_TO_16(output[0]));

            output += Extra;
        }
        else
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(FROM_8_TO_16(output[nChan]));
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = wOut[index];

            if (Reverse)
                v = REVERSE_FLAVOR_16(v);

            if (Premul && alpha_factor is not 0)
                v = (ushort)(((v * alpha_factor) + 0x8000) >> 16);

            *output++ = FROM_16_TO_8(v);
        }

        if (!ExtraFirst)
            output += Extra;

        if (Extra is 0 && SwapFirst)
            PackSwapFirst(swap1, nChan);

        return output;
    }

    private static byte* PackChunkyWords(Transform* info, ushort* wOut, byte* output, uint _)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, _, Premul, SwapEndian) = T_BREAK(info->OutputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var alpha_factor = 0u;

        var swap1 = (ushort*)output;

        if (ExtraFirst)
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(*(ushort*)output);

            output += Extra * _sizeof<ushort>();
        }
        else
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(((ushort*)output)[nChan]);
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = wOut[index];

            if (SwapEndian)
                v = CHANGE_ENDIAN(v);

            if (Reverse)
                v = REVERSE_FLAVOR_16(v);

            if (Premul && alpha_factor is not 0)
                v = (ushort)(((v * alpha_factor) + 0x8000) >> 16);

            *(ushort*)output = v;

            output += _sizeof<ushort>();
        }

        if (!ExtraFirst)
            output += Extra * _sizeof<ushort>();

        if (Extra is 0 && SwapFirst)
            PackSwapFirst(swap1, nChan);

        return output;
    }

    private static byte* PackPlanarBytes(Transform* info, ushort* wOut, byte* output, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, _, Premul, _) = T_BREAK(info->OutputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var alpha_factor = 0u;
        var Init = output;

        if (ExtraFirst)
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(FROM_8_TO_16(output[0]));

            output += Extra * Stride;
        }
        else
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(FROM_8_TO_16(output[nChan * Stride]));
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = wOut[index];

            if (Reverse)
                v = REVERSE_FLAVOR_16(v);

            if (Premul && alpha_factor is not 0)
                v = (ushort)(((v * alpha_factor) + 0x8000) >> 16);

            *output = FROM_16_TO_8(v);

            output += Stride;
        }

        return Init + 1;
    }

    private static byte* PackPlanarWords(Transform* info, ushort* wOut, byte* output, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, _, Premul, SwapEndian) = T_BREAK(info->OutputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var alpha_factor = 0u;
        var Init = output;

        if (ExtraFirst)
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(*(ushort*)output);

            output += Extra * Stride;
        }
        else
        {
            if (Premul && Extra is not 0)
                alpha_factor = (uint)_cmsToFixedDomain(((ushort*)output)[nChan * Stride]);
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = wOut[index];

            if (SwapEndian)
                v = CHANGE_ENDIAN(v);

            if (Reverse)
                v = REVERSE_FLAVOR_16(v);

            if (Premul && alpha_factor is not 0)
                v = (ushort)(((v * alpha_factor) + 0x8000) >> 16);

            *(ushort*)output = v;

            output += Stride;
        }

        return Init + _sizeof<ushort>();
    }

    private static byte* Pack6Bytes(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[0]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[2]);
        *output++ = FROM_16_TO_8(wOut[3]);
        *output++ = FROM_16_TO_8(wOut[4]);
        *output++ = FROM_16_TO_8(wOut[5]);

        return output;
    }

    private static byte* Pack6BytesSwap(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[5]);
        *output++ = FROM_16_TO_8(wOut[4]);
        *output++ = FROM_16_TO_8(wOut[3]);
        *output++ = FROM_16_TO_8(wOut[2]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[0]);

        return output;
    }

    private static byte* Pack6Words(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = wOut[0];
        output += 2;
        *(ushort*)output = wOut[1];
        output += 2;
        *(ushort*)output = wOut[2];
        output += 2;
        *(ushort*)output = wOut[3];
        output += 2;
        *(ushort*)output = wOut[4];
        output += 2;
        *(ushort*)output = wOut[5];
        output += 2;

        return output;
    }

    private static byte* Pack6WordsSwap(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = wOut[5];
        output += 2;
        *(ushort*)output = wOut[4];
        output += 2;
        *(ushort*)output = wOut[3];
        output += 2;
        *(ushort*)output = wOut[2];
        output += 2;
        *(ushort*)output = wOut[1];
        output += 2;
        *(ushort*)output = wOut[0];
        output += 2;

        return output;
    }

    private static byte* Pack4Bytes(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[0]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[2]);
        *output++ = FROM_16_TO_8(wOut[3]);

        return output;
    }

    private static byte* Pack4BytesReverse(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = REVERSE_FLAVOR_8(FROM_16_TO_8(wOut[0]));
        *output++ = REVERSE_FLAVOR_8(FROM_16_TO_8(wOut[1]));
        *output++ = REVERSE_FLAVOR_8(FROM_16_TO_8(wOut[2]));
        *output++ = REVERSE_FLAVOR_8(FROM_16_TO_8(wOut[3]));

        return output;
    }

    private static byte* Pack4BytesSwapFirst(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[3]);
        *output++ = FROM_16_TO_8(wOut[0]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[2]);

        return output;
    }

    private static byte* Pack4BytesSwap(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[3]);
        *output++ = FROM_16_TO_8(wOut[2]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[0]);

        return output;
    }

    private static byte* Pack4BytesSwapSwapFirst(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[2]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[0]);
        *output++ = FROM_16_TO_8(wOut[3]);

        return output;
    }

    private static byte* Pack4Words(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = wOut[0];
        output += 2;
        *(ushort*)output = wOut[1];
        output += 2;
        *(ushort*)output = wOut[2];
        output += 2;
        *(ushort*)output = wOut[3];
        output += 2;

        return output;
    }

    private static byte* Pack4WordsReverse(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = REVERSE_FLAVOR_16(wOut[0]);
        output += 2;
        *(ushort*)output = REVERSE_FLAVOR_16(wOut[1]);
        output += 2;
        *(ushort*)output = REVERSE_FLAVOR_16(wOut[2]);
        output += 2;
        *(ushort*)output = REVERSE_FLAVOR_16(wOut[3]);
        output += 2;

        return output;
    }

    private static byte* Pack4WordsSwap(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = wOut[3];
        output += 2;
        *(ushort*)output = wOut[2];
        output += 2;
        *(ushort*)output = wOut[1];
        output += 2;
        *(ushort*)output = wOut[0];
        output += 2;

        return output;
    }

    private static byte* Pack4WordsBigEndian(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = CHANGE_ENDIAN(wOut[0]);
        output += 2;
        *(ushort*)output = CHANGE_ENDIAN(wOut[1]);
        output += 2;
        *(ushort*)output = CHANGE_ENDIAN(wOut[2]);
        output += 2;
        *(ushort*)output = CHANGE_ENDIAN(wOut[3]);
        output += 2;

        return output;
    }

    private static byte* PackLabV2_8(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(FomLabV4ToLabV2(wOut[0]));
        *output++ = FROM_16_TO_8(FomLabV4ToLabV2(wOut[1]));
        *output++ = FROM_16_TO_8(FomLabV4ToLabV2(wOut[2]));

        return output;
    }

    private static byte* PackALabV2_8(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        output++;
        *output++ = FROM_16_TO_8(FomLabV4ToLabV2(wOut[0]));
        *output++ = FROM_16_TO_8(FomLabV4ToLabV2(wOut[1]));
        *output++ = FROM_16_TO_8(FomLabV4ToLabV2(wOut[2]));

        return output;
    }

    private static byte* PackLabV2_16(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = FomLabV4ToLabV2(wOut[0]);
        output += 2;
        *(ushort*)output = FomLabV4ToLabV2(wOut[1]);
        output += 2;
        *(ushort*)output = FomLabV4ToLabV2(wOut[2]);
        output += 2;

        return output;
    }

    private static byte* Pack3Bytes(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[0]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[2]);

        return output;
    }

    private static byte* Pack3BytesOptimized(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = (byte)(wOut[0] & 0xFFu);
        *output++ = (byte)(wOut[1] & 0xFFu);
        *output++ = (byte)(wOut[2] & 0xFFu);

        return output;
    }

    private static byte* Pack3BytesSwap(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[2]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[0]);

        return output;
    }

    private static byte* Pack3BytesSwapOptimized(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = (byte)(wOut[2] & 0xFFu);
        *output++ = (byte)(wOut[1] & 0xFFu);
        *output++ = (byte)(wOut[0] & 0xFFu);

        return output;
    }

    private static byte* Pack3Words(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = wOut[0];
        output += 2;
        *(ushort*)output = wOut[1];
        output += 2;
        *(ushort*)output = wOut[2];
        output += 2;

        return output;
    }

    private static byte* Pack3WordsSwap(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = wOut[2];
        output += 2;
        *(ushort*)output = wOut[1];
        output += 2;
        *(ushort*)output = wOut[0];
        output += 2;

        return output;
    }

    private static byte* Pack3WordsBigEndian(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = CHANGE_ENDIAN(wOut[0]);
        output += 2;
        *(ushort*)output = CHANGE_ENDIAN(wOut[1]);
        output += 2;
        *(ushort*)output = CHANGE_ENDIAN(wOut[2]);
        output += 2;

        return output;
    }

    private static byte* Pack3BytesAndSkip1(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[0]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[2]);
        output++;

        return output;
    }

    private static byte* Pack3BytesAndSkip1Optimized(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = (byte)(wOut[0] & 0xFFu);
        *output++ = (byte)(wOut[1] & 0xFFu);
        *output++ = (byte)(wOut[2] & 0xFFu);
        output++;

        return output;
    }

    private static byte* Pack3BytesAndSkip1SwapFirst(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        output++;
        *output++ = FROM_16_TO_8(wOut[0]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[2]);

        return output;
    }

    private static byte* Pack3BytesAndSkip1SwapFirstOptimized(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        output++;
        *output++ = (byte)(wOut[0] & 0xFFu);
        *output++ = (byte)(wOut[1] & 0xFFu);
        *output++ = (byte)(wOut[2] & 0xFFu);

        return output;
    }

    private static byte* Pack3BytesAndSkip1Swap(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        output++;
        *output++ = FROM_16_TO_8(wOut[2]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[0]);

        return output;
    }

    private static byte* Pack3BytesAndSkip1SwapOptimized(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        output++;
        *output++ = (byte)(wOut[2] & 0xFFu);
        *output++ = (byte)(wOut[1] & 0xFFu);
        *output++ = (byte)(wOut[0] & 0xFFu);

        return output;
    }

    private static byte* Pack3BytesAndSkip1SwapSwapFirst(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[2]);
        *output++ = FROM_16_TO_8(wOut[1]);
        *output++ = FROM_16_TO_8(wOut[0]);
        output++;

        return output;
    }

    private static byte* Pack3BytesAndSkip1SwapSwapFirstOptimized(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = (byte)(wOut[2] & 0xFFu);
        *output++ = (byte)(wOut[1] & 0xFFu);
        *output++ = (byte)(wOut[0] & 0xFFu);
        output++;

        return output;
    }

    private static byte* Pack3WordsAndSkip1(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = wOut[0];
        output += 2;
        *(ushort*)output = wOut[1];
        output += 2;
        *(ushort*)output = wOut[2];
        output += 2;
        output += 2;

        return output;
    }

    private static byte* Pack3WordsAndSkip1Swap(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        output += 2;
        *(ushort*)output = wOut[2];
        output += 2;
        *(ushort*)output = wOut[1];
        output += 2;
        *(ushort*)output = wOut[0];
        output += 2;

        return output;
    }

    private static byte* Pack3WordsAndSkip1SwapFirst(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        output += 2;
        *(ushort*)output = wOut[0];
        output += 2;
        *(ushort*)output = wOut[1];
        output += 2;
        *(ushort*)output = wOut[2];
        output += 2;

        return output;
    }

    private static byte* Pack3WordsAndSkip1SwapSwapFirst(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = wOut[2];
        output += 2;
        *(ushort*)output = wOut[1];
        output += 2;
        *(ushort*)output = wOut[0];
        output += 2;
        output += 2;

        return output;
    }

    private static byte* Pack1Byte(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[0]);

        return output;
    }

    private static byte* Pack1ByteReversed(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(REVERSE_FLAVOR_16(wOut[0]));

        return output;
    }

    private static byte* Pack1ByteSkip1(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *output++ = FROM_16_TO_8(wOut[0]);
        output++;

        return output;
    }

    private static byte* Pack1ByteSkip1SwapFirst(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        output++;
        *output++ = FROM_16_TO_8(wOut[0]);

        return output;
    }

    private static byte* Pack1Word(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = wOut[0];
        output += 2;

        return output;
    }

    private static byte* Pack1WordReversed(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = REVERSE_FLAVOR_16(wOut[0]);
        output += 2;

        return output;
    }

    private static byte* Pack1WordBigEndian(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = CHANGE_ENDIAN(wOut[0]);
        output += 2;

        return output;
    }

    private static byte* Pack1WordSkip1(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        *(ushort*)output = wOut[0];
        output += 2;
        output += 2;

        return output;
    }

    private static byte* Pack1WordSkip1SwapFirst(Transform* _1, ushort* wOut, byte* output, uint _2)
    {
        output += 2;
        *(ushort*)output = wOut[0];
        output += 2;

        return output;
    }

    private static byte* PackLabDoubleFrom16(Transform* info, ushort* wOut, byte* output, uint Stride)
    {
        if (T_PLANAR(info->OutputFormat) is not 0)
        {
            CIELab Lab;
            var Out = (double*)output;
            cmsLabEncoded2Float(&Lab, wOut);

            Out[0] = Lab.L;
            Out[Stride] = Lab.a;
            Out[Stride * 2] = Lab.b;

            return output + _sizeof<double>();
        }
        else
        {
            cmsLabEncoded2Float((CIELab*)output, wOut);
            return output + (_sizeof<CIELab>() + (T_EXTRA(info->OutputFormat) * _sizeof<double>()));
        }
    }

    private static byte* PackLabFloatFrom16(Transform* info, ushort* wOut, byte* output, uint Stride)
    {
        CIELab Lab;
        cmsLabEncoded2Float(&Lab, wOut);

        var Out = (float*)output;

        if (T_PLANAR(info->OutputFormat) is not 0)
        {
            Out[0] = (float)Lab.L;
            Out[Stride] = (float)Lab.a;
            Out[Stride * 2] = (float)Lab.b;

            return output + _sizeof<float>();
        }
        else
        {
            Out[0] = (float)Lab.L;
            Out[1] = (float)Lab.a;
            Out[2] = (float)Lab.b;

            return output + ((3 + T_EXTRA(info->OutputFormat)) * _sizeof<float>());
        }
    }

    private static byte* PackXYZDoubleFrom16(Transform* info, ushort* wOut, byte* output, uint Stride)
    {
        if (T_PLANAR(info->OutputFormat) is not 0)
        {
            CIEXYZ XYZ;
            var Out = (double*)output;
            cmsXYZEncoded2Float(&XYZ, wOut);

            Out[0] = XYZ.X;
            Out[Stride] = XYZ.Y;
            Out[Stride * 2] = XYZ.Z;

            return output + _sizeof<double>();
        }
        else
        {
            cmsXYZEncoded2Float((CIEXYZ*)output, wOut);
            return output + (_sizeof<CIEXYZ>() + (T_EXTRA(info->OutputFormat) * _sizeof<double>()));
        }
    }

    private static byte* PackXYZFloatFrom16(Transform* info, ushort* wOut, byte* output, uint Stride)
    {
        CIEXYZ XYZ;
        cmsXYZEncoded2Float(&XYZ, wOut);

        var Out = (float*)output;

        if (T_PLANAR(info->OutputFormat) is not 0)
        {
            Out[0] = (float)XYZ.X;
            Out[Stride] = (float)XYZ.Y;
            Out[Stride * 2] = (float)XYZ.Z;

            return output + _sizeof<float>();
        }
        else
        {
            Out[0] = (float)XYZ.X;
            Out[1] = (float)XYZ.Y;
            Out[2] = (float)XYZ.Z;

            return output + ((3 + T_EXTRA(info->OutputFormat)) * _sizeof<float>());
        }
    }

    private static byte* PackDoubleFrom16(Transform* info, ushort* wOut, byte* output, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, SwapEndian) = T_BREAK(info->OutputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var maximum = IsInkSpace(info->OutputFormat) ? 655.35 : 65535.0;
        var swap1 = (double*)output;
        var start = 0u;

        Stride /= PixelSize(info->OutputFormat);

        if (ExtraFirst)
            start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = wOut[index] / maximum;

            if (Reverse)
                v = maximum - v;
            ((double*)output)[(i + start) * (Planar ? Stride : 1)] = v;
        }

        if (Extra is 0 && SwapFirst)
            PackSwapFirst(swap1, nChan);

        return output + (_sizeof<double>() * (Planar ? 1 : (nChan + Extra)));
    }

    private static byte* PackFloatFrom16(Transform* info, ushort* wOut, byte* output, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, SwapEndian) = T_BREAK(info->OutputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var maximum = IsInkSpace(info->OutputFormat) ? 655.35 : 65535.0;
        var swap1 = (float*)output;
        var start = 0u;

        Stride /= PixelSize(info->OutputFormat);

        if (ExtraFirst)
            start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = wOut[index] / maximum;

            if (Reverse)
                v = maximum - v;

            ((float*)output)[(i + start) * (Planar ? Stride : 1)] = (float)v;
        }

        if (Extra is 0 && SwapFirst)
            PackSwapFirst(swap1, nChan);

        return output + (_sizeof<float>() * (Planar ? 1 : (nChan + Extra)));
    }

    private static byte* PackFloatsFromFloat(Transform* info, float* wOut, byte* output, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, SwapEndian) = T_BREAK(info->OutputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var maximum = IsInkSpace(info->OutputFormat) ? 100.0 : 1.0;
        var swap1 = (float*)output;
        var start = 0u;

        Stride /= PixelSize(info->OutputFormat);

        if (ExtraFirst)
            start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = wOut[index] * maximum;

            if (Reverse)
                v = maximum - v;

            ((float*)output)[(i + start) * (Planar ? Stride : 1)] = (float)v;
        }

        if (Extra is 0 && SwapFirst)
            PackSwapFirst(swap1, nChan);

        return output + (_sizeof<float>() * (Planar ? 1 : (nChan + Extra)));
    }

    private static byte* PackDoublesFromFloat(Transform* info, float* wOut, byte* output, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, SwapEndian) = T_BREAK(info->OutputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var maximum = IsInkSpace(info->OutputFormat) ? 100.0 : 1.0;
        var swap1 = (double*)output;
        var start = 0u;

        Stride /= PixelSize(info->OutputFormat);

        if (ExtraFirst)
            start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = wOut[index] * maximum;

            if (Reverse)
                v = maximum - v;

            ((double*)output)[(i + start) * (Planar ? Stride : 1)] = v;
        }

        if (Extra is 0 && SwapFirst)
            PackSwapFirst(swap1, nChan);

        return output + (_sizeof<double>() * (Planar ? 1 : (nChan + Extra)));
    }

    private static byte* PackLabFloatFromFloat(Transform* info, float* wOut, byte* output, uint Stride)
    {
        var Out = (float*)output;

        if (T_PLANAR(info->OutputFormat) is not 0)
        {
            Stride /= PixelSize(info->OutputFormat);

            Out[0] = (float)(wOut[0] * 100.0);
            Out[Stride] = (float)((wOut[1] * 255.0) - 128.0);
            Out[Stride * 2] = (float)((wOut[2] * 255.0) - 128.0);

            return output + _sizeof<float>();
        }
        else
        {
            Out[0] = (float)(wOut[0] * 100.0);
            Out[1] = (float)((wOut[1] * 255.0) - 128.0);
            Out[2] = (float)((wOut[2] * 255.0) - 128.0);

            return output + ((3 + T_EXTRA(info->OutputFormat)) * _sizeof<float>());
        }
    }

    private static byte* PackLabDoubleFromFloat(Transform* info, float* wOut, byte* output, uint Stride)
    {
        var Out = (double*)output;

        if (T_PLANAR(info->OutputFormat) is not 0)
        {
            Stride /= PixelSize(info->OutputFormat);

            Out[0] = wOut[0] * 100.0;
            Out[Stride] = (wOut[1] * 255.0) - 128.0;
            Out[Stride * 2] = (wOut[2] * 255.0) - 128.0;

            return output + _sizeof<double>();
        }
        else
        {
            Out[0] = wOut[0] * 100.0;
            Out[1] = (wOut[1] * 255.0) - 128.0;
            Out[2] = (wOut[2] * 255.0) - 128.0;

            return output + ((3 + T_EXTRA(info->OutputFormat)) * _sizeof<double>());
        }
    }

    private static byte* PackXYZFloatFromFloat(Transform* info, float* wOut, byte* output, uint Stride)
    {
        var Out = (float*)output;

        if (T_PLANAR(info->OutputFormat) is not 0)
        {
            Stride /= PixelSize(info->OutputFormat);

            Out[0] = (float)(wOut[0] * MAX_ENCODEABLE_XYZ);
            Out[Stride] = (float)(wOut[1] * MAX_ENCODEABLE_XYZ);
            Out[Stride * 2] = (float)(wOut[2] * MAX_ENCODEABLE_XYZ);

            return output + _sizeof<float>();
        }
        else
        {
            Out[0] = (float)(wOut[0] * MAX_ENCODEABLE_XYZ);
            Out[1] = (float)(wOut[1] * MAX_ENCODEABLE_XYZ);
            Out[2] = (float)(wOut[2] * MAX_ENCODEABLE_XYZ);

            return output + ((3 + T_EXTRA(info->OutputFormat)) * _sizeof<float>());
        }
    }

    private static byte* PackXYZDoubleFromFloat(Transform* info, float* wOut, byte* output, uint Stride)
    {
        var Out = (double*)output;

        if (T_PLANAR(info->OutputFormat) is not 0)
        {
            Stride /= PixelSize(info->OutputFormat);

            Out[0] = wOut[0] * MAX_ENCODEABLE_XYZ;
            Out[Stride] = wOut[1] * MAX_ENCODEABLE_XYZ;
            Out[Stride * 2] = wOut[2] * MAX_ENCODEABLE_XYZ;

            return output + _sizeof<double>();
        }
        else
        {
            Out[0] = wOut[0] * MAX_ENCODEABLE_XYZ;
            Out[1] = wOut[1] * MAX_ENCODEABLE_XYZ;
            Out[2] = wOut[2] * MAX_ENCODEABLE_XYZ;

            return output + ((3 + T_EXTRA(info->OutputFormat)) * _sizeof<double>());
        }
    }

    private static byte* UnrollHalfTo16(Transform* info, ushort* wIn, byte* accum, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, _) = T_BREAK(info->InputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var start = 0u;
        var maximum = IsInkSpace(info->InputFormat) ? 655.35F : 65535.0F;

        Stride /= PixelSize(info->InputFormat);

        if (ExtraFirst) start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = _cmsHalf2Float(((ushort*)accum)[(i + start) * (Planar ? Stride : 1)]);

            if (Reverse)
                v = maximum - v;

            wIn[index] = _cmsQuickSaturateWord((double)v * maximum);
        }

        if (Extra is 0 && SwapFirst) UnrollSwapFirst(wIn, nChan);

        return accum + ((T_PLANAR(info->InputFormat) is not 0 ? 1 : nChan + Extra) * _sizeof<ushort>());
    }

    private static byte* UnrollHalfToFloat(Transform* info, float* wIn, byte* accum, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, _) = T_BREAK(info->InputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var start = 0u;
        var maximum = IsInkSpace(info->InputFormat) ? 100.0F : 1.0F;

        Stride /= PixelSize(info->InputFormat);

        if (ExtraFirst) start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = _cmsHalf2Float(((ushort*)accum)[(i + start) * (Planar ? Stride : 1)]);

            v /= maximum;

            wIn[index] = Reverse ? 1 - v : v;
        }

        if (Extra is 0 && SwapFirst) UnrollSwapFirst(wIn, nChan);

        return accum + ((T_PLANAR(info->InputFormat) is not 0 ? 1 : nChan + Extra) * _sizeof<ushort>());
    }

    private static byte* PackHalfFrom16(Transform* info, ushort* wOut, byte* output, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, _) = T_BREAK(info->OutputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var maximum = IsInkSpace(info->OutputFormat) ? 655.35F : 65535.0F;
        var swap1 = (ushort*)output;
        var start = 0u;

        Stride /= PixelSize(info->OutputFormat);

        if (ExtraFirst)
            start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = wOut[index] / maximum;

            if (Reverse)
                v = maximum - v;

            ((ushort*)output)[(i + start) * (Planar ? Stride : 1)] = _cmsFloat2Half(v);
        }

        if (Extra is 0 && SwapFirst)
            PackSwapFirst(swap1, nChan);

        return output + (_sizeof<ushort>() * (Planar ? 1 : (nChan + Extra)));
    }

    private static byte* PackHalfFromFloat(Transform* info, float* wOut, byte* output, uint Stride)
    {
        var (nChan, DoSwap, Reverse, SwapFirst, Extra, Planar, _, _) = T_BREAK(info->OutputFormat);
        var ExtraFirst = DoSwap ^ SwapFirst;
        var maximum = IsInkSpace(info->OutputFormat) ? 100.0F : 1.0F;
        var swap1 = (float*)output;
        var start = 0u;

        Stride /= PixelSize(info->OutputFormat);

        if (ExtraFirst)
            start = Extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = (uint)(DoSwap ? (nChan - i - 1) : i);

            var v = wOut[index] * maximum;

            if (Reverse)
                v = maximum - v;

            ((ushort*)output)[(i + start) * (Planar ? Stride : 1)] = _cmsFloat2Half(v);
        }

        if (Extra is 0 && SwapFirst)
            PackSwapFirst(swap1, nChan);

        return output + (_sizeof<ushort>() * (Planar ? 1 : (nChan + Extra)));
    }

    internal static Formatter _cmsGetStockInputFormatter(uint dwInput, PackFlags dwFlags)
    {
        Formatter fr = default;

        switch (dwFlags)
        {
            case PackFlags.Ushort:
                {
                    for (var i = 0; i < InputFormatters16.Length; i++)
                    {
                        ref var f = ref InputFormatters16[i];

                        if ((dwInput & ~f.Mask) == f.Type)
                        {
                            fr.Fmt16 = f.Frm;
                            return fr;
                        }
                    }
                    break;
                }
            case PackFlags.Float:
                {
                    for (var i = 0; i < InputFormattersFloat.Length; i++)
                    {
                        ref var f = ref InputFormattersFloat[i];

                        if ((dwInput & ~f.Mask) == f.Type)
                        {
                            fr.FmtFloat = f.Frm;
                            return fr;
                        }
                    }
                    break;
                }
        }
        return default;
    }

    internal static Formatter _cmsGetStockOutputFormatter(uint dwInput, PackFlags dwFlags)
    {
        Formatter fr = default;

        // Optimization is only a hint
        dwInput &= ~OPTIMIZED_SH(1);

        switch (dwFlags)
        {
            case PackFlags.Ushort:
                {
                    for (var i = 0; i < OutputFormatters16.Length; i++)
                    {
                        ref var f = ref OutputFormatters16[i];

                        if ((dwInput & ~f.Mask) == f.Type)
                        {
                            fr.Fmt16 = f.Frm;
                            return fr;
                        }
                    }
                    break;
                }
            case PackFlags.Float:
                {
                    for (var i = 0; i < OutputFormattersFloat.Length; i++)
                    {
                        ref var f = ref OutputFormattersFloat[i];

                        if ((dwInput & ~f.Mask) == f.Type)
                        {
                            fr.FmtFloat = f.Frm;
                            return fr;
                        }
                    }
                    break;
                }
        }
        return default;
    }

    private static void DupFormatterFactoryList(Context ctx, in Context src) =>
        // Moved to FormattersPluginChunkType.Dup

        ctx.FormattersPlugin = (FormattersPluginChunkType)src.FormattersPlugin.Dup(ctx)!;

    internal static void _cmsAllocFormattersPluginChunk(Context ctx, in Context src)
    {
        AllocPluginChunk(ctx, ref ctx.FormattersPlugin, src.FormattersPlugin, FormattersPluginChunk);
    }

    internal static bool _cmsRegisterFormattersPlugin(Context? ContextID, PluginBase* Data)
    {
        var ctx = _cmsContextGetClientChunk<FormattersPluginChunkType>(ContextID, Chunks.FormattersPlugin);
        var Plugin = (PluginFormatters*)Data;

        // Reset to build-in defaults
        if (Data is null)
        {
            ctx.FactoryList = null;
            return true;
        }

        var fl = _cmsPluginMalloc<FormattersFactoryList>(ContextID);
        if (fl is null) return false;

        fl->Factory = Plugin->FormattersFactory;

        fl->Next = ctx.FactoryList;
        ctx.FactoryList = fl;

        return true;
    }

    internal static Formatter _cmsGetFormatter(Context? ContextID, uint Type, FormatterDirection Dir, PackFlags dwFlags)
    {
        var ctx = _cmsContextGetClientChunk<FormattersPluginChunkType>(ContextID, Chunks.FormattersPlugin)!;

        for (var f = ctx.FactoryList; f is not null; f = f->Next)
        {
            var fn = f->Factory(Type, Dir, (uint)dwFlags);
            if (fn.Fmt16 is not null) return fn;
        }

        // Revert to default
        return Dir switch
        {
            FormatterDirection.Input => _cmsGetStockInputFormatter(Type, dwFlags),
            _ => _cmsGetStockOutputFormatter(Type, dwFlags),
        };
    }

    internal static bool _cmsFormatterIsFloat(uint Type) =>
        T_FLOAT(Type) is not 0;

    internal static bool _cmsFormatterIs8bit(uint Type) =>
        T_BYTES(Type) is 1;

    public static uint cmsFormatterForColorspaceOfProfile(HPROFILE hProfile, uint nBytes, bool lIsFloat)
    {
        var ColorSpace = cmsGetColorSpace(hProfile);
        var ColorSpaceBits = (uint)_cmsLCMScolorSpace(ColorSpace);
        var nOutputChans = cmsChannelsOf(ColorSpace);
        var Float = lIsFloat ? 1u : 0;

        // Create a fake formatter for result
        return FLOAT_SH(Float) | COLORSPACE_SH(ColorSpaceBits) | BYTES_SH(nBytes) | CHANNELS_SH(nOutputChans);
    }

    public static uint cmsFormatterForPCSOfProfile(Profile* hProfile, uint nBytes, bool lIsFloat)
    {
        var ColorSpace = cmsGetPCS(hProfile);

        var ColorSpaceBits = (uint)_cmsLCMScolorSpace(ColorSpace);
        var nOutputChans = cmsChannelsOf(ColorSpace);
        var Float = lIsFloat ? 1u : 0;

        // Create a fake formatter for result
        return FLOAT_SH(Float) | COLORSPACE_SH(ColorSpaceBits) | BYTES_SH(nBytes) | CHANNELS_SH(nOutputChans);
    }
}
