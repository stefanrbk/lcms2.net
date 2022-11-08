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
using MoreSpans;

using System.Runtime.CompilerServices;

using static lcms2.types.PixelFormat;

namespace lcms2.types.defaults;

internal static class DefaultFormatters
{
    #region Fields

    private static readonly Formatters16Input[] InputFormatters16 = new Formatters16Input[]
    {
        new(
            Lab_DBL,
            AnyPlanar | AnyExtra,
            UnrollLabDoubleTo16),
        new(
            XYZ_DBL,
            AnyPlanar | AnyExtra,
            UnrollXYZDoubleTo16),
        new(
            Lab_FLT,
            AnyPlanar | AnyExtra,
            UnrollLabFloatTo16),
        new(
            XYZ_FLT,
            AnyPlanar | AnyExtra,
            UnrollXYZFloatTo16),
        new(
            GRAY_DBL,
            0,
            UnrollDouble1Chan),
        new(
            FLOAT_SH(1) | BYTES_SH(0),
            AnyChannels | AnyPlanar | AnySwapFirst | AnyFlavor | AnySwap | AnyExtra | AnySpace,
            UnrollDoubleTo16),
        new(
            FLOAT_SH(1) | BYTES_SH(4),
            AnyChannels | AnyPlanar | AnySwapFirst | AnyFlavor | AnySwap | AnyExtra | AnySpace,
            UnrollFloatTo16),
        new(FLOAT_SH(1) | BYTES_SH(2),
            AnyChannels | AnyPlanar | AnySwapFirst | AnyFlavor | AnySwap | AnyExtra | AnySpace,
            UnrollHalfTo16),

        // --------------------------------
        new(
            CHANNELS_SH(1) | BYTES_SH(1),
            AnySpace,
            Unroll1Byte),
        new(
            CHANNELS_SH(1) | BYTES_SH(1) | EXTRA_SH(1),
            AnySpace,
            Unroll1ByteSkip1),
        new(
            CHANNELS_SH(1) | BYTES_SH(1) | EXTRA_SH(2),
            AnySpace,
            Unroll1ByteSkip2),
        new(
            CHANNELS_SH(1) | BYTES_SH(1) | FLAVOR_SH(1),
            AnySpace,
            Unroll1ByteReversed),
        new(
            COLORSPACE_SH(Colorspace.MCH2) | CHANNELS_SH(2) | BYTES_SH(1),
            0,
            Unroll2Bytes),

        // --------------------------------
        new(
            LabV2_8,
            0,
            UnrollLabV2_8),
        new(
            ALabV2_8,
            0,
            UnrollALabV2_8),
        new(
            LabV2_16,
            0,
            UnrollLabV2_16),

        // --------------------------------
        new(
            CHANNELS_SH(3) | BYTES_SH(1),
            AnySpace,
            Unroll3Bytes),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1),
            AnySpace,
            Unroll3BytesSwap),
        new(
            CHANNELS_SH(3) | EXTRA_SH(1) | BYTES_SH(1) | DOSWAP_SH(1),
            AnySpace,
            Unroll3BytesSkip1Swap),
        new(
            CHANNELS_SH(3) | EXTRA_SH(1) | BYTES_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Unroll3BytesSkip1SwapFirst),
        new(
            CHANNELS_SH(3) | EXTRA_SH(1) | BYTES_SH(1) | DOSWAP_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Unroll3BytesSkip1SwapSwapFirst),

        // --------------------------------
        new(
            CHANNELS_SH(4) | BYTES_SH(1),
            AnySpace,
            Unroll4Bytes),
        new(
            CHANNELS_SH(4) | BYTES_SH(1) | FLAVOR_SH(1),
            AnySpace,
            Unroll4BytesReverse),
        new(
            CHANNELS_SH(4) | BYTES_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Unroll4BytesSwapFirst),
        new(
            CHANNELS_SH(4) | BYTES_SH(1) | DOSWAP_SH(1),
            AnySpace,
            Unroll4BytesSwap),
        new(
            CHANNELS_SH(4) | BYTES_SH(1) | DOSWAP_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Unroll4BytesSwapSwapFirst),

        // --------------------------------
        new(
            BYTES_SH(1) | PLANAR_SH(1),
            AnyFlavor | AnySwapFirst | AnyPremul | AnySwap | AnyExtra | AnyChannels | AnySpace,
            UnrollPlanarBytes),
        new(
            BYTES_SH(1),
            AnyFlavor | AnySwapFirst | AnySwap | AnyPremul | AnyExtra | AnyChannels | AnySpace,
            UnrollChunkyBytes),

        // --------------------------------
        new(
            CHANNELS_SH(1) | BYTES_SH(2),
            AnySpace,
            Unroll1Word),
        new(
            CHANNELS_SH(1) | BYTES_SH(2) | FLAVOR_SH(1),
            AnySpace,
            Unroll1WordReversed),
        new(
            CHANNELS_SH(1) | BYTES_SH(1) | EXTRA_SH(3),
            AnySpace,
            Unroll1WordSkip3),

        // --------------------------------
        new(
            CHANNELS_SH(2) | BYTES_SH(2),
            0,
            Unroll2Words),
        new(
            CHANNELS_SH(3) | BYTES_SH(2),
            AnySpace,
            Unroll3Words),
        new(
            CHANNELS_SH(4) | BYTES_SH(2),
            AnySpace,
            Unroll4Words),

        // --------------------------------
        new(
            CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1),
            AnySpace,
            Unroll3WordsSwap),
        new(
            CHANNELS_SH(3) | BYTES_SH(2) | EXTRA_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Unroll3WordsSkip1SwapFirst),
        new(
            CHANNELS_SH(3) | BYTES_SH(2) | EXTRA_SH(1) | DOSWAP_SH(1),
            AnySpace,
            Unroll3WordsSkip1Swap),
        new(
            CHANNELS_SH(4) | BYTES_SH(2) | FLAVOR_SH(1),
            AnySpace,
            Unroll4WordsReverse),
        new(
            CHANNELS_SH(4) | BYTES_SH(2) | SWAPFIRST_SH(1),
            AnySpace,
            Unroll4WordsSwapFirst),
        new(
            CHANNELS_SH(4) | BYTES_SH(2) | DOSWAP_SH(1),
            AnySpace,
            Unroll4WordsSwap),
        new(
            CHANNELS_SH(4) | BYTES_SH(2) | DOSWAP_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Unroll4WordsSwapSwapFirst),

        // --------------------------------
        new(
            BYTES_SH(2) | PLANAR_SH(1),
            AnyFlavor | AnySwap | AnyEndian | AnyExtra | AnyChannels | AnySpace,
            UnrollPlanarWords),
        new(
            BYTES_SH(2),
            AnyFlavor | AnySwapFirst | AnySwap | AnyEndian | AnyExtra | AnyChannels | AnySpace,
            UnrollAnyWords),

        // --------------------------------
        new(
            BYTES_SH(2) | PLANAR_SH(1),
            AnyFlavor | AnySwap | AnyEndian | AnyExtra | AnyChannels | AnySpace | PREMUL_SH(1),
            UnrollPlanarWordsPremul),
        new(
            BYTES_SH(2),
            AnyFlavor | AnySwapFirst | AnySwap | AnyEndian | AnyExtra | AnyChannels | AnySpace | PREMUL_SH(1),
            UnrollAnyWordsPremul),
    };

    private static readonly FormattersFloatInput[] InputFormattersFloat = new FormattersFloatInput[]
    {
        new(
            Lab_DBL,
            AnyPlanar | AnyExtra,
            UnrollLabDoubleToFloat),
        new(
            Lab_FLT,
            AnyPlanar | AnyExtra,
            UnrollLabFloatToFloat),

        // --------------------------------
        new(
            XYZ_DBL,
            AnyPlanar | AnyExtra,
            UnrollXYZDoubleToFloat),
        new(
            XYZ_FLT,
            AnyPlanar | AnyExtra,
            UnrollXYZFloatToFloat),

        // --------------------------------
        new(
            FLOAT_SH(1) | BYTES_SH(4),
            AnyPlanar | AnySwapFirst | AnySwap | AnyExtra | AnyPremul | AnyChannels | AnySpace,
            UnrollFloatsToFloat),
        new(
            FLOAT_SH(1) | BYTES_SH(0),
            AnyPlanar | AnySwapFirst | AnySwap | AnyExtra | AnyChannels | AnySpace | AnyPremul,
            UnrollDoublesToFloat),

        // --------------------------------
        new(
            LabV2_8,
            0,
            UnrollLabV2_8ToFloat),
        new(
            ALabV2_8,
            0,
            UnrollALabV2_8ToFloat),
        new(
            LabV2_16,
            0,
            UnrollLabV2_16ToFloat),

        // --------------------------------
        new(
            BYTES_SH(1),
            AnyPlanar | AnySwapFirst | AnySwap | AnyExtra | AnyChannels | AnySpace,
            Unroll8ToFloat),

        // --------------------------------
        new(
            BYTES_SH(2),
            AnyPlanar | AnySwapFirst | AnySwap | AnyExtra | AnyChannels | AnySpace,
            Unroll16ToFloat),

        // --------------------------------
        new(FLOAT_SH(1) | BYTES_SH(2),
            AnyPlanar | AnySwapFirst | AnySwap | AnyExtra | AnyChannels | AnySpace,
            UnrollHalfToFloat),
    };

    private static readonly Formatters16Output[] OutputFormatters16 = new Formatters16Output[]
    {
        new(
            Lab_DBL,
            AnyPlanar | AnyExtra,
            PackLabDoubleFrom16),
        new(
            XYZ_DBL,
            AnyPlanar | AnyExtra,
            PackXYZDoubleFrom16),

        // --------------------------------
        new(
            Lab_FLT,
            AnyPlanar | AnyExtra,
            PackLabFloatFrom16),
        new(
            XYZ_FLT,
            AnyPlanar | AnyExtra,
            PackXYZFloatFrom16),

        // --------------------------------
        new(
            FLOAT_SH(1) | BYTES_SH(0),
            AnyFlavor | AnySwapFirst | AnySwap | AnyChannels | AnyPlanar | AnyExtra | AnySpace,
            PackDoubleFrom16),
        new(
            FLOAT_SH(1) | BYTES_SH(4),
            AnyFlavor | AnySwapFirst | AnySwap | AnyChannels | AnyPlanar | AnyExtra | AnySpace,
            PackFloatFrom16),
        new(FLOAT_SH(1) | BYTES_SH(2),
            AnyFlavor | AnySwapFirst | AnySwap | AnyChannels | AnyPlanar | AnyExtra | AnySpace,
            PackHalfFrom16),

        // --------------------------------
        new(
            CHANNELS_SH(1) | BYTES_SH(1),
            AnySpace,
            Pack1Byte),
        new(
            CHANNELS_SH(1) | BYTES_SH(1) | EXTRA_SH(1),
            AnySpace,
            Pack1ByteSkip1),
        new(
            CHANNELS_SH(1) | BYTES_SH(1) | EXTRA_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Pack1ByteSkip1SwapFirst),
        new(
            CHANNELS_SH(1) | BYTES_SH(1) | FLAVOR_SH(1),
            AnySpace,
            Pack1ByteReversed),

        // --------------------------------
        new(
            LabV2_8,
            0,
            PackLabV2_8),
        new(
            ALabV2_8,
            0,
            PackALabV2_8),
        new(
            LabV2_16,
            0,
            PackLabV2_16),

        // --------------------------------
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | OPTIMIZED_SH(1),
            AnySpace,
            Pack3BytesOptimized),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | EXTRA_SH(1) | OPTIMIZED_SH(1),
            AnySpace,
            Pack3BytesAndSkip1Optimized),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | EXTRA_SH(1) | SWAPFIRST_SH(1) | OPTIMIZED_SH(1),
            AnySpace,
            Pack3BytesAndSkip1SwapFirstOptimized),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | EXTRA_SH(1) | DOSWAP_SH(1) | SWAPFIRST_SH(1) | OPTIMIZED_SH(1),
            AnySpace,
            Pack3BytesAndSkip1SwapSwapFirstOptimized),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1) | EXTRA_SH(1) | OPTIMIZED_SH(1),
            AnySpace,
            Pack3BytesAndSkip1SwapOptimized),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1) | OPTIMIZED_SH(1),
            AnySpace,
            Pack3BytesSwapOptimized),

        // --------------------------------
        new(
            CHANNELS_SH(3) | BYTES_SH(1),
            AnySpace,
            Pack3Bytes),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | EXTRA_SH(1),
            AnySpace,
            Pack3BytesAndSkip1),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | EXTRA_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Pack3BytesAndSkip1SwapFirst),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | EXTRA_SH(1) | DOSWAP_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Pack3BytesAndSkip1SwapSwapFirst),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1) | EXTRA_SH(1),
            AnySpace,
            Pack3BytesAndSkip1Swap),
        new(
            CHANNELS_SH(3) | BYTES_SH(1) | DOSWAP_SH(1),
            AnySpace,
            Pack3BytesSwap),
        new(
            CHANNELS_SH(4) | BYTES_SH(1),
            AnySpace,
            Pack4Bytes),
        new(
            CHANNELS_SH(4) | BYTES_SH(1) | FLAVOR_SH(1),
            AnySpace,
            Pack4BytesReverse),
        new(
            CHANNELS_SH(4) | BYTES_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Pack4BytesSwapFirst),
        new(
            CHANNELS_SH(4) | BYTES_SH(1) | DOSWAP_SH(1),
            AnySpace,
            Pack4BytesSwap),
        new(
            CHANNELS_SH(4) | BYTES_SH(1) | DOSWAP_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Pack4BytesSwapSwapFirst),
        new(
            CHANNELS_SH(6) | BYTES_SH(1),
            AnySpace,
            Pack6Bytes),
        new(
            CHANNELS_SH(6) | BYTES_SH(1) | DOSWAP_SH(1),
            AnySpace,
            Pack6BytesSwap),

        // --------------------------------
        new(
            BYTES_SH(1),
            AnyFlavor | AnySwapFirst | AnySwap | AnyExtra | AnyChannels | AnySpace | AnyPremul,
            PackChunkyBytes),

        // --------------------------------
        new(
            BYTES_SH(1) | PLANAR_SH(1),
            AnyFlavor | AnySwapFirst | AnySwap | AnyExtra | AnyChannels | AnySpace | AnyPremul,
            PackPlanarBytes),

        // --------------------------------
        new(
            CHANNELS_SH(1) | BYTES_SH(2),
            AnySpace,
            Pack1Word),
        new(
            CHANNELS_SH(1) | BYTES_SH(2) | EXTRA_SH(1),
            AnySpace,
            Pack1WordSkip1),
        new(
            CHANNELS_SH(1) | BYTES_SH(2) | EXTRA_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Pack1WordSkip1SwapFirst),
        new(
            CHANNELS_SH(1) | BYTES_SH(2) | FLAVOR_SH(1),
            AnySpace,
            Pack1WordReversed),
        new(
            CHANNELS_SH(1) | BYTES_SH(2) | ENDIAN16_SH(1),
            AnySpace,
            Pack1WordBigEndian),
        new(
            CHANNELS_SH(3) | BYTES_SH(2),
            AnySpace,
            Pack3Words),
        new(
            CHANNELS_SH(3) | BYTES_SH(2) | DOSWAP_SH(1),
            AnySpace,
            Pack3WordsSwap),
        new(
            CHANNELS_SH(3) | BYTES_SH(2) | ENDIAN16_SH(1),
            AnySpace,
            Pack3WordsBigEndian),
        new(
            CHANNELS_SH(3) | BYTES_SH(2) | EXTRA_SH(1),
            AnySpace,
            Pack3WordsAndSkip1),
        new(
            CHANNELS_SH(3) | BYTES_SH(2) | EXTRA_SH(1) | DOSWAP_SH(1),
            AnySpace,
            Pack3WordsAndSkip1Swap),
        new(
            CHANNELS_SH(3) | BYTES_SH(2) | EXTRA_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Pack3WordsAndSkip1SwapFirst),
        new(
            CHANNELS_SH(3) | BYTES_SH(2) | EXTRA_SH(1) | DOSWAP_SH(1) | SWAPFIRST_SH(1),
            AnySpace,
            Pack3WordsAndSkip1SwapSwapFirst),

        // --------------------------------
        new(
            CHANNELS_SH(4) | BYTES_SH(2),
            AnySpace,
            Pack4Words),
        new(
            CHANNELS_SH(4) | BYTES_SH(2) | FLAVOR_SH(1),
            AnySpace,
            Pack4WordsReverse),
        new(
            CHANNELS_SH(4) | BYTES_SH(2) | DOSWAP_SH(1),
            AnySpace,
            Pack4WordsSwap),
        new(
            CHANNELS_SH(4) | BYTES_SH(2) | ENDIAN16_SH(1),
            AnySpace,
            Pack4WordsBigEndian),

        // --------------------------------
        new(
            CHANNELS_SH(6) | BYTES_SH(2),
            AnySpace,
            Pack6Words),
        new(
            CHANNELS_SH(6) | BYTES_SH(2) | DOSWAP_SH(1),
            AnySpace,
            Pack6WordsSwap),

        // --------------------------------
        new(
            BYTES_SH(2),
            AnyFlavor | AnySwapFirst | AnySwap | AnyEndian | AnyExtra | AnyChannels | AnySpace | AnyPremul,
            PackChunkyWords),
        new(
            BYTES_SH(2) | PLANAR_SH(1),
            AnyFlavor | AnyEndian | AnySwap | AnyExtra | AnyChannels | AnySpace | AnyPremul,
            PackPlanarWords),
    };

    private static readonly FormattersFloatOutput[] OutputFormattersFloat = new FormattersFloatOutput[]
    {
        new(
            Lab_FLT,
            AnyPlanar | AnyExtra,
            PackLabFloatFromFloat),
        new(
            XYZ_FLT,
            AnyPlanar | AnyExtra,
            PackXYZFloatFromFloat),

        // --------------------------------
        new(
            Lab_DBL,
            AnyPlanar | AnyExtra,
            PackLabDoubleFromFloat),
        new(
            XYZ_DBL,
            AnyPlanar | AnyExtra,
            PackXYZDoubleFromFloat),

        // --------------------------------
        new(
            FLOAT_SH(1) | BYTES_SH(4),
            AnyPlanar | AnyFlavor | AnySwapFirst | AnySwap | AnyExtra | AnyChannels | AnySpace,
            PackFloatsFromFloat),
        new(
            FLOAT_SH(1) | BYTES_SH(0),
            AnyPlanar | AnyFlavor | AnySwapFirst | AnySwap | AnyExtra | AnyChannels | AnySpace,
            PackDoublesFromFloat),
        new(FLOAT_SH(1) | BYTES_SH(2),
            AnyFlavor | AnySwapFirst | AnySwap | AnyExtra | AnyChannels | AnySpace,
            PackHalfFromFloat),
    };

    #endregion Fields

    #region Internal Methods

    internal static Formatter GetInput(uint input, PackFlag flags)
    {
        Formatter fr = default;

        switch (flags)
        {
            case PackFlag.Ushort:
                {
                    foreach (var f in InputFormatters16)
                    {
                        if ((input & ~f.Mask) == f.Type)
                        {
                            fr.Fmt16In = f.Frm;
                            break;
                        }
                    }
                }
                break;

            case PackFlag.Float:
                {
                    foreach (var f in InputFormattersFloat)
                    {
                        if ((input & ~f.Mask) == f.Type)
                        {
                            fr.FmtFloatIn = f.Frm;
                            break;
                        }
                    }
                }
                break;
        }

        return fr;
    }

    internal static Formatter GetOutput(uint input, PackFlag flags)
    {
        Formatter fr = default;

        // Optimization is only a hint
        input &= ~OPTIMIZED_SH(1);

        switch (flags)
        {
            case PackFlag.Ushort:
                {
                    foreach (var f in OutputFormatters16)
                    {
                        if ((input & ~f.Mask) == f.Type)
                        {
                            fr.Fmt16Out = f.Frm;
                            break;
                        }
                    }
                }
                break;

            case PackFlag.Float:
                {
                    foreach (var f in OutputFormattersFloat)
                    {
                        if ((input & ~f.Mask) == f.Type)
                        {
                            fr.FmtFloatOut = f.Frm;
                            break;
                        }
                    }
                }
                break;
        }

        return fr;
    }

    #endregion Internal Methods

    #region Private Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort ChangeEndian(ushort w) =>
        (ushort)((ushort)(w << 8) | w >> 8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort From8to16Reversed(byte value) =>
        From8to16(ReverseFlavor(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort FromLabV2ToLabV4(ushort x)
    {
        var a = (x << 8 | x) >> 8;
        return a > 0xFFFF
            ? (ushort)0xFFFF
            : (ushort)a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort FromLabV4ToLabV2(ushort x) =>
        (ushort)(((x << 8) + 0x80) / 257);

    private static void Lab4toFloat(Span<float> wIn, ReadOnlySpan<ushort> lab4)
    {
        var L = lab4[0] / 655.35f;
        var a = lab4[1] / 257f - 128f;
        var b = lab4[2] / 257f - 128f;

        wIn[0] = L / 100f;
        wIn[1] = (a + 128f) / 255f;
        wIn[2] = (b + 128f) / 255f;
    }

    private static Span<byte> Pack1Byte(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];

        return ptr[1..].Span;
    }

    private static Span<byte> Pack1ByteReversed(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, s => From16to8(ReverseFlavor(s)));
        ptr[0] = wOut[0];

        return ptr[1..].Span;
    }

    private static Span<byte> Pack1ByteSkip1(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];

        return ptr[2..].Span;
    }

    private static Span<byte> Pack1ByteSkip1SwapFirst(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[1] = wOut[0];

        return ptr[2..].Span;
    }

    private static Span<byte> Pack1Word(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];

        return ptr[1..].Span;
    }

    private static Span<byte> Pack1WordBigEndian(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(ChangeEndian(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[3..].Span;
    }

    private static Span<byte> Pack1WordReversed(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(ReverseFlavor(s)));
        ptr[0] = wOut[0];

        return ptr[1..].Span;
    }

    private static Span<byte> Pack1WordSkip1(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];

        return ptr[2..].Span;
    }

    private static Span<byte> Pack1WordSkip1SwapFirst(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[1] = wOut[0];

        return ptr[2..].Span;
    }

    private static Span<byte> Pack3Bytes(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[3..].Span;
    }

    private static Span<byte> Pack3BytesAndSkip1(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack3BytesAndSkip1Optimized(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[0] = (byte)(wOut[0] & 0xFF);
        output[1] = (byte)(wOut[1] & 0xFF);
        output[2] = (byte)(wOut[2] & 0xFF);

        return output[4..];
    }

    private static Span<byte> Pack3BytesAndSkip1Swap(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[1] = wOut[2];
        ptr[2] = wOut[1];
        ptr[3] = wOut[0];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack3BytesAndSkip1SwapFirst(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[1] = wOut[0];
        ptr[2] = wOut[1];
        ptr[3] = wOut[2];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack3BytesAndSkip1SwapFirstOptimized(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[1] = (byte)(wOut[0] & 0xFF);
        output[2] = (byte)(wOut[1] & 0xFF);
        output[3] = (byte)(wOut[2] & 0xFF);

        return output[4..];
    }

    private static Span<byte> Pack3BytesAndSkip1SwapOptimized(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[1] = (byte)(wOut[2] & 0xFF);
        output[2] = (byte)(wOut[1] & 0xFF);
        output[3] = (byte)(wOut[0] & 0xFF);

        return output[4..];
    }

    private static Span<byte> Pack3BytesAndSkip1SwapSwapFirst(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[2];
        ptr[1] = wOut[1];
        ptr[2] = wOut[0];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack3BytesAndSkip1SwapSwapFirstOptimized(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[0] = (byte)(wOut[2] & 0xFF);
        output[1] = (byte)(wOut[1] & 0xFF);
        output[2] = (byte)(wOut[0] & 0xFF);

        return output[4..];
    }

    private static Span<byte> Pack3BytesOptimized(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[0] = (byte)(wOut[0] & 0xFF);
        output[1] = (byte)(wOut[1] & 0xFF);
        output[2] = (byte)(wOut[2] & 0xFF);

        return output[3..];
    }

    private static Span<byte> Pack3BytesSwap(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[2];
        ptr[1] = wOut[1];
        ptr[2] = wOut[0];

        return ptr[3..].Span;
    }

    private static Span<byte> Pack3BytesSwapOptimized(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[0] = (byte)(wOut[2] & 0xFF);
        output[1] = (byte)(wOut[1] & 0xFF);
        output[2] = (byte)(wOut[0] & 0xFF);

        return output[3..];
    }

    private static Span<byte> Pack3Words(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[3..].Span;
    }

    private static Span<byte> Pack3WordsAndSkip1(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack3WordsAndSkip1Swap(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[1] = wOut[2];
        ptr[2] = wOut[1];
        ptr[3] = wOut[0];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack3WordsAndSkip1SwapFirst(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[1] = wOut[0];
        ptr[2] = wOut[1];
        ptr[3] = wOut[2];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack3WordsAndSkip1SwapSwapFirst(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[2];
        ptr[1] = wOut[1];
        ptr[2] = wOut[0];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack3WordsBigEndian(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(ChangeEndian(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[1..].Span;
    }

    private static Span<byte> Pack3WordsSwap(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[2];
        ptr[1] = wOut[1];
        ptr[2] = wOut[0];

        return ptr[3..].Span;
    }

    private static Span<byte> Pack4Bytes(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack4BytesReverse(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, s => ReverseFlavor(From16to8(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack4BytesSwap(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[3];
        ptr[1] = wOut[2];
        ptr[2] = wOut[1];
        ptr[3] = wOut[0];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack4BytesSwapFirst(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[3];
        ptr[1] = wOut[0];
        ptr[2] = wOut[1];
        ptr[3] = wOut[2];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack4BytesSwapSwapFirst(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[2];
        ptr[1] = wOut[1];
        ptr[2] = wOut[0];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack4Words(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack4WordsBigEndian(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(ChangeEndian(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack4WordsReverse(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(ReverseFlavor(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack4WordsSwap(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[3];
        ptr[1] = wOut[2];
        ptr[2] = wOut[1];
        ptr[3] = wOut[0];

        return ptr[4..].Span;
    }

    private static Span<byte> Pack6Bytes(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];
        ptr[4] = wOut[4];
        ptr[5] = wOut[5];

        return ptr[6..].Span;
    }

    private static Span<byte> Pack6BytesSwap(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[5];
        ptr[1] = wOut[4];
        ptr[2] = wOut[3];
        ptr[3] = wOut[2];
        ptr[4] = wOut[1];
        ptr[5] = wOut[0];

        return ptr[6..].Span;
    }

    private static Span<byte> Pack6Words(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];
        ptr[4] = wOut[4];
        ptr[5] = wOut[5];

        return ptr[6..].Span;
    }

    private static Span<byte> Pack6WordsSwap(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[5];
        ptr[1] = wOut[4];
        ptr[2] = wOut[3];
        ptr[3] = wOut[2];
        ptr[4] = wOut[1];
        ptr[5] = wOut[0];

        return ptr[6..].Span;
    }

    private static Span<byte> PackALabV2_8(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, s => From16to8(FromLabV4ToLabV2(s)));
        ptr[1] = wOut[0];
        ptr[2] = wOut[1];
        ptr[3] = wOut[2];

        return ptr[4..].Span;
    }

    private static Span<byte> PackChunkyBytes(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var nChan = info.outputFormat.Channels;
        var doSwap = info.outputFormat.HasSwapAll;
        var reverse = info.outputFormat.Flavor;
        var extra = info.outputFormat.ExtraSamples;
        var swapFirst = info.outputFormat.HasSwapFirst;
        var premul = info.outputFormat.HasPremultipliedAlpha;
        var extraFirst = doSwap ^ swapFirst;
        var v = (ushort)0;
        var alphaFactor = 0;

        var swap1 = output;
        var ptr = output.Converter(From8to16, From16to8);

        if (extraFirst)
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[0]);

            ptr = ptr[extra..];
        }
        else
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[nChan]);
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            v = wOut[index];

            if (reverse is ColorFlavor.Subtractive)
                v = ReverseFlavor(v);

            if (premul && alphaFactor != 0)
                v = (ushort)(v * alphaFactor + 0x8000 >> 16);

            ptr[0] = v;
            ptr++;
        }

        if (!extraFirst)
            ptr += extra;

        if (extra is 0 && swapFirst)
            RollingShift(swap1[..nChan]);

        return ptr.Span;
    }

    private static Span<byte> PackChunkyWords(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var nChan = info.outputFormat.Channels;
        var swapEndian = info.outputFormat.HasEndianSwap;
        var doSwap = info.outputFormat.HasSwapAll;
        var reverse = info.outputFormat.Flavor;
        var extra = info.outputFormat.ExtraSamples;
        var swapFirst = info.outputFormat.HasSwapFirst;
        var premul = info.outputFormat.HasPremultipliedAlpha;
        var extraFirst = doSwap ^ swapFirst;
        var v = (ushort)0;
        var alphaFactor = 0;

        var ptr = output.UpCaster(BitConverter.ToUInt16, BitConverter.GetBytes);
        var swap1 = ptr;

        if (extraFirst)
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[0]);

            ptr += extra;
        }
        else
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[nChan]);
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            v = wOut[index];

            if (swapEndian)
                v = ChangeEndian(v);

            if (reverse is ColorFlavor.Subtractive)
                v = ReverseFlavor(v);

            if (premul && alphaFactor != 0)
                v = (ushort)(v * alphaFactor + 0x8000 >> 16);

            ptr[0] = v;
            ptr++;
        }

        if (!extraFirst)
            ptr += extra;

        if (extra is 0 && swapFirst)
            RollingShift(swap1[..nChan]);

        return ptr.Span;
    }

    private static Span<byte> PackDoubleFrom16(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var nChan = info.outputFormat.Channels;
        var doSwap = info.outputFormat.HasSwapAll;
        var reverse = info.outputFormat.Flavor;
        var extra = info.outputFormat.ExtraSamples;
        var swapFirst = info.outputFormat.HasSwapFirst;
        var planar = info.outputFormat.IsPlanar;
        var extraFirst = doSwap ^ swapFirst;
        var maximum = info.outputFormat.IsInkSpace ? 655.35 : 65535.0;
        var v = 0d;

        var ptr = output.UpCaster(BitConverter.ToDouble, BitConverter.GetBytes);
        var swap1 = ptr;

        if (planar)
            stride /= info.outputFormat.PixelSize;
        else
            stride = 1;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            v = wOut[index] / maximum;

            if (reverse is ColorFlavor.Subtractive)
                v = maximum - v;

            ptr[(i + start) * stride] = v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(swap1[..nChan]);

        return planar
            ? swap1[1..].Span
            : swap1[(nChan + extra)..].Span;
    }

    private static Span<byte> PackDoublesFromFloat(Transform info, ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var nChan = info.outputFormat.Channels;
        var doSwap = info.outputFormat.HasSwapAll;
        var reverse = info.outputFormat.Flavor;
        var extra = info.outputFormat.ExtraSamples;
        var swapFirst = info.outputFormat.HasSwapFirst;
        var planar = info.outputFormat.IsPlanar;
        var extraFirst = doSwap ^ swapFirst;
        var maximum = info.outputFormat.IsInkSpace ? 100.0 : 1.0;
        var v = 0d;

        var ptr = output.UpCaster(BitConverter.ToDouble, BitConverter.GetBytes);
        var swap1 = ptr;

        if (planar)
            stride /= info.outputFormat.PixelSize;
        else
            stride = 1;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            v = wOut[index] * maximum;

            if (reverse is ColorFlavor.Subtractive)
                v = maximum - v;

            ptr[(i + start) * stride] = v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(swap1[..nChan]);

        return planar
            ? swap1[1..].Span
            : swap1[(nChan + extra)..].Span;
    }

    private static Span<byte> PackFloatFrom16(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var nChan = info.outputFormat.Channels;
        var doSwap = info.outputFormat.HasSwapAll;
        var reverse = info.outputFormat.Flavor;
        var extra = info.outputFormat.ExtraSamples;
        var swapFirst = info.outputFormat.HasSwapFirst;
        var planar = info.outputFormat.IsPlanar;
        var extraFirst = doSwap ^ swapFirst;
        var maximum = info.outputFormat.IsInkSpace ? 655.35 : 65535.0;
        var v = 0d;

        var ptr = output.UpCaster(BitConverter.ToSingle, BitConverter.GetBytes);
        var swap1 = ptr;

        if (planar)
            stride /= info.outputFormat.PixelSize;
        else
            stride = 1;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            v = wOut[index] / maximum;

            if (reverse is ColorFlavor.Subtractive)
                v = maximum - v;

            ptr[(i + start) * stride] = (float)v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(swap1[..nChan]);

        return planar
            ? swap1[1..].Span
            : swap1[(nChan + extra)..].Span;
    }

    private static Span<byte> PackFloatsFromFloat(Transform info, ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var nChan = info.outputFormat.Channels;
        var doSwap = info.outputFormat.HasSwapAll;
        var reverse = info.outputFormat.Flavor;
        var extra = info.outputFormat.ExtraSamples;
        var swapFirst = info.outputFormat.HasSwapFirst;
        var planar = info.outputFormat.IsPlanar;
        var extraFirst = doSwap ^ swapFirst;
        var maximum = info.outputFormat.IsInkSpace ? 100.0 : 1.0;
        var v = 0d;

        var ptr = output.UpCaster(BitConverter.ToSingle, BitConverter.GetBytes);
        var swap1 = ptr;

        if (planar)
            stride /= info.outputFormat.PixelSize;
        else
            stride = 1;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            v = wOut[index] * maximum;

            if (reverse is ColorFlavor.Subtractive)
                v = maximum - v;

            ptr[(i + start) * stride] = (float)v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(swap1[..nChan]);

        return planar
            ? swap1[1..].Span
            : swap1[(nChan + extra)..].Span;
    }

    private static Span<byte> PackHalfFrom16(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var nChan = info.outputFormat.Channels;
        var doSwap = info.outputFormat.HasSwapAll;
        var reverse = info.outputFormat.Flavor;
        var extra = info.outputFormat.ExtraSamples;
        var swapFirst = info.outputFormat.HasSwapFirst;
        var planar = info.outputFormat.IsPlanar;
        var extraFirst = doSwap ^ swapFirst;
        var maximum = info.outputFormat.IsInkSpace ? 655.35f : 65535.0f;

        var ptr = output.UpCaster(BitConverter.ToHalf, BitConverter.GetBytes);
        var swap1 = ptr;

        if (planar)
            stride /= info.outputFormat.PixelSize;
        else
            stride = 1;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            var v = wOut[index] / maximum;

            if (reverse is ColorFlavor.Subtractive)
                v = maximum - v;

            ptr[(i + start) * stride] = (Half)v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(swap1[..nChan]);

        return planar
            ? swap1[1..].Span
            : swap1[(nChan + extra)..].Span;
    }

    private static Span<byte> PackHalfFromFloat(Transform info, ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var nChan = info.outputFormat.Channels;
        var doSwap = info.outputFormat.HasSwapAll;
        var reverse = info.outputFormat.Flavor;
        var extra = info.outputFormat.ExtraSamples;
        var swapFirst = info.outputFormat.HasSwapFirst;
        var planar = info.outputFormat.IsPlanar;
        var extraFirst = doSwap ^ swapFirst;
        var maximum = info.outputFormat.IsInkSpace ? 100f : 1f;

        var ptr = output.UpCaster(BitConverter.ToHalf, BitConverter.GetBytes);
        var swap1 = ptr;

        if (planar)
            stride /= info.outputFormat.PixelSize;
        else
            stride = 1;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            var v = wOut[index] * maximum;

            if (reverse is ColorFlavor.Subtractive)
                v = maximum - v;

            ptr[(i + start) * stride] = (Half)v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(swap1[..nChan]);

        return planar
            ? swap1[1..].Span
            : swap1[(nChan + extra)..].Span;
    }

    private static Span<byte> PackLabDoubleFrom16(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, double>(null!, BitConverter.GetBytes);
        var lab = new LabEncoded(wOut).ToLab();

        if (info.outputFormat.IsPlanar)
        {
            @out[0] = lab.L;
            @out[stride] = lab.a;
            @out[stride * 2] = lab.b;

            return @out[1..].Span;
        }
        else
        {
            lab.ToArray().CopyTo(@out);
            return @out[(3 + info.outputFormat.ExtraSamples)..].Span;
        }
    }

    private static Span<byte> PackLabDoubleFromFloat(Transform info, ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, double>(null!, BitConverter.GetBytes);

        if (info.outputFormat.IsPlanar)
        {
            stride /= info.outputFormat.PixelSize;

            @out[0] = wOut[0] * 100.0;
            @out[stride] = wOut[1] * 255.0 - 128.0;
            @out[stride * 2] = wOut[2] * 255.0 - 128.0;

            return @out[1..].Span;
        }
        else
        {
            @out[0] = wOut[0] * 100.0;
            @out[1] = wOut[1] * 255.0 - 128.0;
            @out[2] = wOut[2] * 255.0 - 128.0;

            return @out[(3 + info.outputFormat.ExtraSamples)..].Span;
        }
    }

    private static Span<byte> PackLabFloatFrom16(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, float>(null!, BitConverter.GetBytes);
        var lab = new LabEncoded(wOut).ToLab();

        if (info.outputFormat.IsPlanar)
        {
            stride /= info.outputFormat.PixelSize;

            @out[0] = (float)lab.L;
            @out[stride] = (float)lab.a;
            @out[stride * 2] = (float)lab.b;

            return @out[1..].Span;
        }
        else
        {
            @out[0] = (float)lab.L;
            @out[1] = (float)lab.a;
            @out[2] = (float)lab.b;
            return @out[(3 + info.outputFormat.ExtraSamples)..].Span;
        }
    }

    private static Span<byte> PackLabFloatFromFloat(Transform info, ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, float>(null!, BitConverter.GetBytes);

        if (info.outputFormat.IsPlanar)
        {
            stride /= info.outputFormat.PixelSize;

            @out[0] = (float)(wOut[0] * 100.0);
            @out[stride] = (float)(wOut[1] * 255.0 - 128.0);
            @out[stride * 2] = (float)(wOut[2] * 255.0 - 128.0);

            return @out[1..].Span;
        }
        else
        {
            @out[0] = (float)(wOut[0] * 100.0);
            @out[1] = (float)(wOut[1] * 255.0 - 128.0);
            @out[2] = (float)(wOut[2] * 255.0 - 128.0);

            return @out[(3 + info.outputFormat.ExtraSamples)..].Span;
        }
    }

    private static Span<byte> PackLabV2_16(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(FromLabV4ToLabV2(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[3..].Span;
    }

    private static Span<byte> PackLabV2_8(Transform _1, ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, s => From16to8(FromLabV4ToLabV2(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[3..].Span;
    }

    private static Span<byte> PackPlanarBytes(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var nChan = info.outputFormat.Channels;
        var doSwap = info.outputFormat.HasSwapAll;
        var swapFirst = info.outputFormat.HasSwapFirst;
        var reverse = info.outputFormat.Flavor;
        var extra = info.outputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var premul = info.outputFormat.HasPremultipliedAlpha;
        var alphaFactor = 0;

        var init = output;
        var ptr = output.Converter(From8to16, From16to8);

        if (extraFirst)
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[0]);

            ptr = ptr[(extra * stride)..];
        }
        else
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[nChan]);
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            var v = wOut[index];

            if (reverse is ColorFlavor.Subtractive)
                v = ReverseFlavor(v);

            if (premul && alphaFactor != 0)
                v = (ushort)(v * alphaFactor + 0x8000 >> 16);

            ptr[0] = v;
            ptr += stride;
        }

        return init[1..];
    }

    private static Span<byte> PackPlanarWords(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var nChan = info.outputFormat.Channels;
        var doSwap = info.outputFormat.HasSwapAll;
        var swapFirst = info.outputFormat.HasSwapFirst;
        var reverse = info.outputFormat.Flavor;
        var extra = info.outputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var premul = info.outputFormat.HasPremultipliedAlpha;
        var swapEndian = info.outputFormat.HasEndianSwap;
        var alphaFactor = 0;

        var ptr = output.UpCaster(BitConverter.ToUInt16, BitConverter.GetBytes);
        var init = ptr;

        stride /= sizeof(ushort);

        if (extraFirst)
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[0]);

            ptr += extra;
        }
        else
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[nChan * stride]);
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            var v = wOut[index];

            if (swapEndian)
                v = ChangeEndian(v);

            if (reverse is ColorFlavor.Subtractive)
                v = ReverseFlavor(v);

            if (premul && alphaFactor != 0)
                v = (ushort)(v * alphaFactor + 0x8000 >> 16);

            ptr[0] = v;
            ptr += stride;
        }

        return init[1..].Span;
    }

    private static Span<byte> PackXYZDoubleFrom16(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, double>(null!, BitConverter.GetBytes);
        var xyz = XYZ.FromEncodedArray(wOut);

        if (info.outputFormat.IsPlanar)
        {
            stride /= info.outputFormat.PixelSize;

            @out[0] = xyz.X;
            @out[stride] = xyz.Y;
            @out[stride * 2] = xyz.Z;

            return @out[1..].Span;
        }
        else
        {
            xyz.ToArray().CopyTo(@out);
            return @out[(3 + info.outputFormat.ExtraSamples)..].Span;
        }
    }

    private static Span<byte> PackXYZDoubleFromFloat(Transform info, ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, double>(null!, s => BitConverter.GetBytes(s * maxEncodableXYZ));

        if (info.outputFormat.IsPlanar)
        {
            stride /= info.outputFormat.PixelSize;

            @out[0] = wOut[0];
            @out[stride] = wOut[1];
            @out[stride * 2] = wOut[2];

            return @out[1..].Span;
        }
        else
        {
            @out[0] = wOut[0];
            @out[1] = wOut[1];
            @out[2] = wOut[2];

            return @out[(3 + info.outputFormat.ExtraSamples)..].Span;
        }
    }

    private static Span<byte> PackXYZFloatFrom16(Transform info, ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, float>(null!, s => BitConverter.GetBytes((float)(s * maxEncodableXYZ)));
        var xyz = XYZ.FromEncodedArray(wOut);

        if (info.outputFormat.IsPlanar)
        {
            stride /= info.outputFormat.PixelSize;

            @out[0] = (float)xyz.X;
            @out[stride] = (float)xyz.Y;
            @out[stride * 2] = (float)xyz.Z;

            return @out[1..].Span;
        }
        else
        {
            @out[0] = (float)xyz.X;
            @out[1] = (float)xyz.Y;
            @out[2] = (float)xyz.Z;

            return @out[(3 + info.outputFormat.ExtraSamples)..].Span;
        }
    }

    private static Span<byte> PackXYZFloatFromFloat(Transform info, ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, float>(null!, s => BitConverter.GetBytes((float)(s * maxEncodableXYZ)));

        if (info.outputFormat.IsPlanar)
        {
            stride /= info.outputFormat.PixelSize;

            @out[0] = wOut[0];
            @out[stride] = wOut[1];
            @out[stride * 2] = wOut[2];

            return @out[1..].Span;
        }
        else
        {
            @out[0] = wOut[0];
            @out[1] = wOut[1];
            @out[2] = wOut[2];

            return @out[(3 + info.outputFormat.ExtraSamples)..].Span;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ReverseFlavor(byte x) =>
        unchecked((byte)(0xFF - x));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort ReverseFlavor(ushort x) =>
        unchecked((ushort)(0xFFFF - x));

    private static ushort ReverseFlavor(ReadOnlySpan<byte> span) =>
        ReverseFlavor(BitConverter.ToUInt16(span));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RollingShift<T>(Span<T> span)
    {
        var tmp = span[^1];
        span[..^1].CopyTo(span[1..]);
        span[0] = tmp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RollingShift<T1, T2>(UpCastingSpan<T1, T2> span)
        where T1 : unmanaged
        where T2 : unmanaged
    {
        var tmp = span[^1];
        span[..^1].CopyTo(span[1..]);
        span[0] = tmp;
    }

    private static ReadOnlySpan<byte> Unroll16ToFloat(Transform info, Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = info.inputFormat.IsPlanar;
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        stride /= info.inputFormat.PixelSize;

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v =
                (float)(planar
                    ? ptr[(i + start) * stride]
                    : ptr[i + start]);

            v /= 65535.0f;

            wIn[index] = reverse is ColorFlavor.Subtractive ? 1 - v : v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return info.inputFormat.IsPlanar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private static ReadOnlySpan<byte> Unroll1Byte(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = From8to16(acc[0]); acc = acc[1..]; // L

        return acc;
    }

    private static ReadOnlySpan<byte> Unroll1ByteReversed(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = ReverseFlavor(From8to16(acc[0])); acc = acc[1..]; // L

        return acc;
    }

    private static ReadOnlySpan<byte> Unroll1ByteSkip1(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = From8to16(acc[0]); acc = acc[1..]; // L
        acc = acc[1..];

        return acc;
    }

    private static ReadOnlySpan<byte> Unroll1ByteSkip2(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = From8to16(acc[0]); acc = acc[1..]; // L
        acc = acc[2..];

        return acc;
    }

    private static ReadOnlySpan<byte> Unroll1Word(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // L

        return acc;
    }

    private static ReadOnlySpan<byte> Unroll1WordReversed(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = ReverseFlavor(BitConverter.ToUInt16(acc)); acc = acc[2..]; // L

        return acc;
    }

    private static ReadOnlySpan<byte> Unroll1WordSkip3(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // L
        acc = acc[8..];

        return acc;
    }

    private static ReadOnlySpan<byte> Unroll2Bytes(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[0] = ptr[0]; // ch1
        wIn[1] = ptr[1]; // ch2

        return ptr[2..].Span;
    }

    private static ReadOnlySpan<byte> Unroll2Words(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[0] = ptr[0]; // ch1
        wIn[1] = ptr[1]; // ch2

        return ptr[2..].Span;
    }

    private static ReadOnlySpan<byte> Unroll3Bytes(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[0] = ptr[0]; // R
        wIn[1] = ptr[1]; // G
        wIn[2] = ptr[2]; // B

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> Unroll3BytesSkip1Swap(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        ptr++; // A
        wIn[2] = ptr[0]; // B
        wIn[1] = ptr[1]; // G
        wIn[0] = ptr[2]; // R

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> Unroll3BytesSkip1SwapFirst(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        ptr++; // A
        wIn[0] = ptr[0]; // R
        wIn[1] = ptr[1]; // G
        wIn[2] = ptr[2]; // B

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> Unroll3BytesSkip1SwapSwapFirst(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[2] = ptr[0]; // B
        wIn[1] = ptr[1]; // G
        wIn[0] = ptr[2]; // R
        ptr++; // A

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> Unroll3BytesSwap(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[2] = ptr[0]; // B
        wIn[1] = ptr[1]; // G
        wIn[0] = ptr[2]; // R

        return acc;
    }

    private static ReadOnlySpan<byte> Unroll3Words(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[0] = ptr[0]; // C R
        wIn[1] = ptr[1]; // M G
        wIn[2] = ptr[2]; // Y B

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> Unroll3WordsSkip1Swap(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        ptr++; // A
        wIn[2] = ptr[0]; // B
        wIn[1] = ptr[1]; // G
        wIn[0] = ptr[2]; // R

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> Unroll3WordsSkip1SwapFirst(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        ptr++; // A
        wIn[0] = ptr[0]; // R
        wIn[1] = ptr[1]; // G
        wIn[2] = ptr[2]; // B

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> Unroll3WordsSwap(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[2] = ptr[0]; // Y B
        wIn[1] = ptr[1]; // M G
        wIn[0] = ptr[2]; // C R

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> Unroll4Bytes(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[0] = ptr[0]; // C
        wIn[1] = ptr[1]; // M
        wIn[2] = ptr[2]; // Y
        wIn[3] = ptr[3]; // K

        return ptr[4..].Span;
    }

    private static ReadOnlySpan<byte> Unroll4BytesReverse(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16Reversed);

        wIn[0] = ptr[0]; // C
        wIn[1] = ptr[1]; // M
        wIn[2] = ptr[2]; // Y
        wIn[3] = ptr[3]; // K

        return ptr[4..].Span;
    }

    private static ReadOnlySpan<byte> Unroll4BytesSwap(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[3] = ptr[0]; // K
        wIn[2] = ptr[1]; // Y
        wIn[1] = ptr[2]; // M
        wIn[0] = ptr[3]; // C

        return ptr[4..].Span;
    }

    private static ReadOnlySpan<byte> Unroll4BytesSwapFirst(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[3] = ptr[0]; // K
        wIn[0] = ptr[1]; // C
        wIn[1] = ptr[2]; // M
        wIn[2] = ptr[3]; // Y

        return ptr[4..].Span;
    }

    private static ReadOnlySpan<byte> Unroll4BytesSwapSwapFirst(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[2] = ptr[0]; // K
        wIn[1] = ptr[1]; // Y
        wIn[0] = ptr[2]; // M
        wIn[3] = ptr[3]; // C

        return ptr[4..].Span;
    }

    private static ReadOnlySpan<byte> Unroll4Words(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[0] = ptr[0]; // C
        wIn[1] = ptr[1]; // M
        wIn[2] = ptr[2]; // Y
        wIn[3] = ptr[3]; // K

        return ptr[4..].Span;
    }

    private static ReadOnlySpan<byte> Unroll4WordsReverse(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, ReverseFlavor);

        wIn[0] = ptr[0]; // C
        wIn[1] = ptr[1]; // M
        wIn[2] = ptr[2]; // Y
        wIn[3] = ptr[3]; // K

        return ptr[4..].Span;
    }

    private static ReadOnlySpan<byte> Unroll4WordsSwap(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[3] = ptr[0]; // K
        wIn[2] = ptr[1]; // Y
        wIn[1] = ptr[2]; // M
        wIn[0] = ptr[3]; // C

        return ptr[4..].Span;
    }

    private static ReadOnlySpan<byte> Unroll4WordsSwapFirst(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[3] = ptr[0]; // K
        wIn[0] = ptr[1]; // C
        wIn[1] = ptr[2]; // M
        wIn[2] = ptr[3]; // Y

        return ptr[4..].Span;
    }

    private static ReadOnlySpan<byte> Unroll4WordsSwapSwapFirst(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[2] = ptr[0]; // K
        wIn[1] = ptr[1]; // Y
        wIn[0] = ptr[2]; // M
        wIn[3] = ptr[3]; // C

        return ptr[4..].Span;
    }

    private static ReadOnlySpan<byte> Unroll8ToFloat(Transform info, Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = info.inputFormat.IsPlanar;

        stride /= info.inputFormat.PixelSize;

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v = (float)acc[(i + start) * (planar ? stride : 1)];

            v /= 255.0f;

            wIn[index] = reverse is ColorFlavor.Subtractive ? 1 - v : v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return info.inputFormat.IsPlanar
            ? acc[sizeof(byte)..]
            : acc[((nChan + extra) * sizeof(byte))..];
    }

    private static ReadOnlySpan<byte> UnrollALabV2_8(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        ptr++; // A
        wIn[0] = FromLabV2ToLabV4(ptr[0]); // L
        wIn[1] = FromLabV2ToLabV4(ptr[1]); // a
        wIn[2] = FromLabV2ToLabV4(ptr[2]); // b

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> UnrollALabV2_8ToFloat(Transform _1, Span<float> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, s => FromLabV2ToLabV4(From8to16(s)));

        ptr++;  // A
        var lab4 = new ushort[]
        {
            ptr[0], // L
            ptr[1], // a
            ptr[2], // b
        };

        Lab4toFloat(wIn, lab4);

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> UnrollAnyWords(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var nChan = info.inputFormat.Channels;
        var swapEndian = info.inputFormat.HasEndianSwap;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc,
            swapEndian
                ? s =>
                    ChangeEndian(BitConverter.ToUInt16(s))
                : BitConverter.ToUInt16);

        if (extraFirst)
            ptr += extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v = ptr[0];

            wIn[index] = reverse is ColorFlavor.Subtractive ? ReverseFlavor(v) : v;
            ptr++;
        }

        if (!extraFirst)
            ptr += extra;

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return ptr.Span;
    }

    private static ReadOnlySpan<byte> UnrollAnyWordsPremul(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var nChan = info.inputFormat.Channels;
        var swapEndian = info.inputFormat.HasEndianSwap;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extraFirst = doSwap ^ swapFirst;

        var alpha = extraFirst ? acc[0] : acc[nChan - 1];
        var alpha_factor = (uint)ToFixedDomain(From8to16(alpha));
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        if (extraFirst)
            ptr++;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v = (uint)ptr[0];

            if (swapEndian)
                v = ChangeEndian((ushort)v);

            v = (v << 16) / alpha_factor;
            if (v > 0xFFFF) v = 0xFFFF;

            wIn[index] = reverse is ColorFlavor.Subtractive ? ReverseFlavor((ushort)v) : (ushort)v;
            ptr++;
        }

        if (!extraFirst)
            ptr++;

        return ptr.Span;
    }

    private static ReadOnlySpan<byte> UnrollChunkyBytes(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var premul = info.inputFormat.HasPremultipliedAlpha;

        var extraFirst = doSwap ^ swapFirst;
        var alphaFactor = 1;

        if (extraFirst)
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(From8to16(acc[0]));

            acc = acc[extra..];
        }
        else
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(From8to16(acc[nChan]));
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;

            uint v = From8to16(acc[0]);
            v = reverse is ColorFlavor.Subtractive ? ReverseFlavor((ushort)v) : v;

            if (premul && alphaFactor > 0)
            {
                v = (v << 16) / (uint)alphaFactor;
                if (v > 0xFFFF) v = 0xFFFF;
            }

            wIn[index] = (ushort)v;
            acc = acc[1..];
        }

        if (!extraFirst)
            acc = acc[extra..];

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return acc;
    }

    private static ReadOnlySpan<byte> UnrollDouble1Chan(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);

        var inks = ptr[0];

        wIn[0] = wIn[1] = wIn[2] = QuickSaturateWord(inks * 65535.0);

        return ptr[1..].Span;
    }

    private static ReadOnlySpan<byte> UnrollDoublesToFloat(Transform info, Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = info.inputFormat.IsPlanar;
        var premul = info.inputFormat.HasPremultipliedAlpha;
        var maximum = info.inputFormat.IsInkSpace ? 100d : 1d;
        var alphaFactor = 1d;
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);

        stride /= info.inputFormat.PixelSize;

        if (premul && extra != 0)
        {
            alphaFactor =
                planar
                    ? (extraFirst ? ptr[0] : ptr[nChan * stride]) / maximum
                    : (extraFirst ? ptr[0] : ptr[nChan]) / maximum;
        }

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v =
                planar
                    ? ptr[(i + start) * stride]
                    : ptr[i + start];

            if (premul && alphaFactor > 0)
                v /= alphaFactor;

            v /= maximum;

            wIn[index] = (float)(reverse is ColorFlavor.Subtractive ? 1 - v : v);
        }

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return info.inputFormat.IsPlanar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private static ReadOnlySpan<byte> UnrollDoubleTo16(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = info.inputFormat.IsPlanar;
        var maximum = info.inputFormat.IsInkSpace ? 655.35 : 65535.0;
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);

        stride /= info.inputFormat.PixelSize;

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v =
                (float)(planar
                    ? ptr[(i + start) * stride]
                    : ptr[i + start]);

            var vi = QuickSaturateWord(v * maximum);

            vi = reverse is ColorFlavor.Subtractive ? ReverseFlavor(vi) : vi;

            wIn[index] = vi;
        }

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return info.inputFormat.IsPlanar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private static ReadOnlySpan<byte> UnrollFloatsToFloat(Transform info, Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = info.inputFormat.IsPlanar;
        var premul = info.inputFormat.HasPremultipliedAlpha;
        var maximum = info.inputFormat.IsInkSpace ? 100f : 1f;
        var alphaFactor = 1f;
        var ptr = new UpCastingReadOnlySpan<byte, float>(acc, BitConverter.ToSingle);

        stride /= info.inputFormat.PixelSize;

        if (premul && extra != 0)
        {
            alphaFactor =
                planar
                    ? (extraFirst ? ptr[0] : ptr[nChan * stride]) / maximum
                    : (extraFirst ? ptr[0] : ptr[nChan]) / maximum;
        }

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v =
                planar
                    ? ptr[(i + start) * stride]
                    : ptr[i + start];

            if (premul && alphaFactor > 0)
                v /= alphaFactor;

            v /= maximum;

            wIn[index] = reverse is ColorFlavor.Subtractive ? 1 - v : v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return info.inputFormat.IsPlanar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private static ReadOnlySpan<byte> UnrollFloatTo16(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = info.inputFormat.IsPlanar;
        var maximum = info.inputFormat.IsInkSpace ? 655.35 : 65535.0;
        var ptr = new UpCastingReadOnlySpan<byte, float>(acc, BitConverter.ToSingle);

        stride /= info.inputFormat.PixelSize;

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v =
                planar
                    ? ptr[(i + start) * stride]
                    : ptr[i + start];

            var vi = QuickSaturateWord(v * maximum);

            vi = reverse is ColorFlavor.Subtractive ? ReverseFlavor(vi) : vi;

            wIn[index] = vi;
        }

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return info.inputFormat.IsPlanar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private static ReadOnlySpan<byte> UnrollHalfTo16(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = info.inputFormat.IsPlanar;
        var maximum = info.inputFormat.IsInkSpace ? 655.35f : 65535.0f;
        var ptr = new UpCastingReadOnlySpan<byte, Half>(acc, BitConverter.ToHalf);

        stride /= info.inputFormat.PixelSize;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v =
                planar
                    ? (float)ptr[(i + start) * stride]
                    : (float)ptr[i + start];

            if (reverse is ColorFlavor.Subtractive)
                v = maximum - v;

            wIn[index] = QuickSaturateWord((double)v * maximum);
        }

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return info.inputFormat.IsPlanar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private static ReadOnlySpan<byte> UnrollHalfToFloat(Transform info, Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = info.inputFormat.IsPlanar;
        var maximum = info.inputFormat.IsInkSpace ? 100f : 1f;
        var ptr = new UpCastingReadOnlySpan<byte, Half>(acc, BitConverter.ToHalf);

        stride /= info.inputFormat.PixelSize;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v =
                planar
                    ? (float)ptr[(i + start) * stride]
                    : (float)ptr[i + start];

            v /= maximum;

            wIn[index] = reverse is ColorFlavor.Subtractive ? 1 - v : v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return info.inputFormat.IsPlanar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private static ReadOnlySpan<byte> UnrollLabDoubleTo16(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        Lab lab;
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);
        stride /= sizeof(double);

        if (info.inputFormat.IsPlanar)
        {
            var pos_L = ptr;
            var pos_a = ptr[stride..];
            var pos_b = pos_a[stride..];

            lab.L = pos_L[0];
            lab.a = pos_a[0];
            lab.b = pos_b[0];

            lab.ToLabEncodedArray().CopyTo(wIn);
            return ptr[1..].Span;
        }
        else
        {
            lab.L = ptr[0];
            lab.a = ptr[1];
            lab.b = ptr[2];

            lab.ToLabEncodedArray().CopyTo(wIn);

            return ptr[(3 + info.inputFormat.ExtraSamples)..].Span;
        }
    }

    private static ReadOnlySpan<byte> UnrollLabDoubleToFloat(Transform info, Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);

        if (info.inputFormat.IsPlanar)
        {
            stride /= info.inputFormat.PixelSize;

            wIn[0] = (float)(ptr[0] / 100);
            wIn[1] = (float)((ptr[stride] + 128) / 255);
            wIn[2] = (float)((ptr[stride * 2] + 128) / 255);

            return ptr[1..].Span;
        }
        else
        {
            wIn[0] = (float)(ptr[0] / 100);
            wIn[1] = (float)((ptr[1] + 128) / 255);
            wIn[2] = (float)((ptr[2] + 128) / 255);

            return ptr[(3 + info.inputFormat.ExtraSamples)..].Span;
        }
    }

    private static ReadOnlySpan<byte> UnrollLabFloatTo16(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        Lab lab;
        var ptr = new UpCastingReadOnlySpan<byte, float>(acc, BitConverter.ToSingle);

        if (info.inputFormat.IsPlanar)
        {
            var pos_L = ptr;
            var pos_a = ptr[stride..];
            var pos_b = pos_a[stride..];

            lab.L = pos_L[0];
            lab.a = pos_a[0];
            lab.b = pos_b[0];

            lab.ToLabEncodedArray().CopyTo(wIn);
            return acc[sizeof(float)..];
        }
        else
        {
            lab.L = ptr[0];
            lab.a = ptr[1];
            lab.b = ptr[2];

            lab.ToLabEncodedArray().CopyTo(wIn);

            return ptr[(3 + info.inputFormat.ExtraSamples)..].Span;
        }
    }

    private static ReadOnlySpan<byte> UnrollLabFloatToFloat(Transform info, Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var ptr = new UpCastingReadOnlySpan<byte, float>(acc, BitConverter.ToSingle);

        if (info.inputFormat.IsPlanar)
        {
            stride /= info.inputFormat.PixelSize;

            wIn[0] = (float)(ptr[0] / 100.0);
            wIn[1] = (float)((ptr[stride] + 128.0) / 255);
            wIn[2] = (float)((ptr[stride * 2] + 128.0) / 255);

            return ptr[1..].Span;
        }
        else
        {
            wIn[0] = (float)(ptr[0] / 100.0);
            wIn[1] = (float)((ptr[1] + 128.0) / 255);
            wIn[2] = (float)((ptr[2] + 128.0) / 255);

            return ptr[(3 + info.inputFormat.ExtraSamples)..].Span;
        }
    }

    private static ReadOnlySpan<byte> UnrollLabV2_16(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[0] = FromLabV2ToLabV4(ptr[0]); // L
        wIn[1] = FromLabV2ToLabV4(ptr[1]); // a
        wIn[2] = FromLabV2ToLabV4(ptr[2]); // b

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> UnrollLabV2_16ToFloat(Transform _1, Span<float> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(
            acc,
            s =>
                FromLabV2ToLabV4(BitConverter.ToUInt16(s)));

        var lab4 = new ushort[]
        {
            ptr[0], // L
            ptr[1], // a
            ptr[2], // b
        };

        Lab4toFloat(wIn, lab4);

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> UnrollLabV2_8(Transform _1, Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[0] = FromLabV2ToLabV4(ptr[0]); // L
        wIn[1] = FromLabV2ToLabV4(ptr[1]); // a
        wIn[2] = FromLabV2ToLabV4(ptr[2]); // b

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> UnrollLabV2_8ToFloat(Transform _1, Span<float> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, s => FromLabV2ToLabV4(From8to16(s)));

        var lab4 = new ushort[]
        {
            ptr[0], // L
            ptr[1], // a
            ptr[2], // b
        };

        Lab4toFloat(wIn, lab4);

        return ptr[3..].Span;
    }

    private static ReadOnlySpan<byte> UnrollPlanarBytes(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var reverse = info.inputFormat.Flavor;
        var extraFirst = doSwap ^ swapFirst;
        var extra = info.inputFormat.ExtraSamples;
        var premul = info.inputFormat.HasPremultipliedAlpha;
        var init = acc[1..];
        var alphaFactor = 1;
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        if (extraFirst)
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[0]);

            ptr += extra * stride;
        }
        else
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[nChan]);
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            uint v = ptr[0];

            v = reverse is ColorFlavor.Subtractive ? ReverseFlavor((ushort)v) : v;

            if (premul && alphaFactor > 0)
            {
                v = (v << 16) / (uint)alphaFactor;
                if (v > 0xFFFF) v = 0xFFFF;
            }

            wIn[index] = (ushort)v;
            ptr += stride;
        }

        return init;
    }

    private static ReadOnlySpan<byte> UnrollPlanarWords(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var reverse = info.inputFormat.Flavor;
        var swapEndian = info.inputFormat.HasEndianSwap;
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);
        var init = ptr[1..].Span;

        stride /= sizeof(ushort);

        if (doSwap)
            ptr += info.inputFormat.ExtraSamples * stride;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v = ptr[0];

            if (swapEndian)
                v = ChangeEndian(v);

            wIn[index] = reverse is ColorFlavor.Subtractive ? ReverseFlavor(v) : v;

            ptr += stride;
        }

        return init;
    }

    private static ReadOnlySpan<byte> UnrollPlanarWordsPremul(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = info.inputFormat.Channels;
        var doSwap = info.inputFormat.HasSwapAll;
        var swapFirst = info.inputFormat.HasSwapFirst;
        var reverse = info.inputFormat.Flavor;
        var swapEndian = info.inputFormat.HasEndianSwap;
        var extraFirst = doSwap ^ swapFirst;

        var alpha = extraFirst ? acc[0] : acc[(nChan - 1) * stride];
        var alpha_factor = (uint)ToFixedDomain(From8to16(alpha));

        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);
        var init = ptr[1..].Span;

        stride /= sizeof(ushort);

        if (extraFirst)
            ptr += stride;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? nChan - i - 1 : i;
            var v = (uint)ptr[0];

            if (swapEndian)
                v = ChangeEndian((ushort)v);

            v = (v << 16) / alpha_factor;
            if (v > 0xFFFF) v = 0xFFFF;

            wIn[index] = reverse is ColorFlavor.Subtractive ? ReverseFlavor((ushort)v) : (ushort)v;

            ptr += stride;
        }

        return init;
    }

    private static ReadOnlySpan<byte> UnrollXYZDoubleTo16(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        XYZ xyz;
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);
        stride /= sizeof(double);

        if (info.inputFormat.IsPlanar)
        {
            var posX = ptr;
            var posY = ptr[stride..];
            var posZ = posY[stride..];

            xyz.X = posX[0];
            xyz.Y = posY[0];
            xyz.Z = posZ[0];

            xyz.ToXYZEncodedArray().CopyTo(wIn);
            return acc[sizeof(double)..];
        }
        else
        {
            xyz.X = ptr[0];
            xyz.Y = ptr[1];
            xyz.Z = ptr[2];

            xyz.ToXYZEncodedArray().CopyTo(wIn);

            return ptr[(3 + info.inputFormat.ExtraSamples)..].Span;
        }
    }

    private static ReadOnlySpan<byte> UnrollXYZDoubleToFloat(Transform info, Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var ptr = new UpCastingReadOnlySpan<byte, double>(
            acc,
            s =>
                BitConverter.ToDouble(s) / maxEncodableXYZ);

        if (info.inputFormat.IsPlanar)
        {
            stride /= info.inputFormat.PixelSize;

            wIn[0] = (float)ptr[0];
            wIn[1] = (float)ptr[stride];
            wIn[2] = (float)ptr[stride * 2];

            return ptr[1..].Span;
        }
        else
        {
            wIn[0] = (float)ptr[0];
            wIn[1] = (float)ptr[1];
            wIn[2] = (float)ptr[2];

            return ptr[(3 + info.inputFormat.ExtraSamples)..].Span;
        }
    }

    private static ReadOnlySpan<byte> UnrollXYZFloatTo16(Transform info, Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        XYZ xyz;
        var ptr = new UpCastingReadOnlySpan<byte, float>(acc, BitConverter.ToSingle);
        stride /= sizeof(float);

        if (info.inputFormat.IsPlanar)
        {
            var posX = ptr;
            var posY = ptr[stride..];
            var posZ = posY[stride..];

            xyz.X = posX[0];
            xyz.Y = posY[0];
            xyz.Z = posZ[0];

            xyz.ToXYZEncodedArray().CopyTo(wIn);
            return acc[sizeof(float)..];
        }
        else
        {
            xyz.X = ptr[0];
            xyz.Y = ptr[1];
            xyz.Z = ptr[2];

            xyz.ToXYZEncodedArray().CopyTo(wIn);

            return ptr[(3 + info.inputFormat.ExtraSamples)..].Span;
        }
    }

    private static ReadOnlySpan<byte> UnrollXYZFloatToFloat(Transform info, Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var ptr = new UpCastingReadOnlySpan<byte, float>(
            acc,
            s =>
                (float)(BitConverter.ToSingle(s) / maxEncodableXYZ));

        if (info.inputFormat.IsPlanar)
        {
            stride /= info.inputFormat.PixelSize;

            wIn[0] = (float)ptr[0];
            wIn[1] = (float)ptr[stride];
            wIn[2] = (float)ptr[stride * 2];

            return ptr[1..].Span;
        }
        else
        {
            wIn[0] = (float)ptr[0];
            wIn[1] = (float)ptr[1];
            wIn[2] = (float)ptr[2];

            return ptr[(3 + info.inputFormat.ExtraSamples)..].Span;
        }
    }

    #endregion Private Methods
}
