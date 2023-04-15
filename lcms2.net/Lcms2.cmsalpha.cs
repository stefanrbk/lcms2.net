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
    // CHANGE_ENDIAN already defined in Lcms2.cmspack.cs

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte _cmsQuickSaturateByte(double d) =>
        (byte)_cmsQuickSaturateWord(Math.Max(Math.Min(d + 0.5, 255), 0));

    private static uint trueBytesSize(uint Format) => PixelSize(Format);

    private static void copy8(void* dst, in void* src) =>
        memmove(dst, src, 1);

    private static void from8to16(void* dst, in void* src) =>
        *(ushort*)dst = FROM_8_TO_16(*(byte*)src);

    private static void from8to16SE(void* dst, in void* src) =>
        *(ushort*)dst = CHANGE_ENDIAN(FROM_8_TO_16(*(byte*)src));

    private static void from8toFLT(void* dst, in void* src) =>
        *(float*)dst = *(byte*)src / 255f;

    private static void from8toDBL(void* dst, in void* src) =>
        *(double*)dst = *(byte*)src / 255d;

    private static void from8toHLF(void* dst, in void* src) =>
        *(ushort*)dst = _cmsFloat2Half(*(byte*)src / 255f);

    private static void from16to8(void* dst, in void* src) =>
        *(byte*)dst = FROM_16_TO_8(*(ushort*)src);

    private static void from16SEto8(void* dst, in void* src) =>
        *(byte*)dst = FROM_16_TO_8(CHANGE_ENDIAN(*(ushort*)src));

    private static void copy16(void* dst, in void* src) =>
        memmove(dst, src, 2);

    private static void from16to16(void* dst, in void* src) =>
        *(ushort*)dst = CHANGE_ENDIAN(*(ushort*)src);

    private static void from16toFLT(void* dst, in void* src) =>
        *(float*)dst = *(ushort*)src / 255f;

    private static void from16SEtoFLT(void* dst, in void* src) =>
        *(float*)dst = CHANGE_ENDIAN(*(ushort*)src) / 255f;

    private static void from16toDBL(void* dst, in void* src) =>
        *(double*)dst = *(ushort*)src / 255d;

    private static void from16SEtoDBL(void* dst, in void* src) =>
        *(double*)dst = CHANGE_ENDIAN(*(ushort*)src) / 255d;

    private static void from16toHLF(void* dst, in void* src) =>
        *(ushort*)dst = _cmsFloat2Half(*(ushort*)src / 255f);

    private static void from16SEtoHLF(void* dst, in void* src) =>
        *(ushort*)dst = _cmsFloat2Half(CHANGE_ENDIAN(*(ushort*)src) / 255f);

    private static void fromFLTto8(void* dst, in void* src) =>
        *(byte*)dst = _cmsQuickSaturateByte(*(float*)src * 255.0);

    private static void fromFLTto16(void* dst, in void* src) =>
        *(ushort*)dst = _cmsQuickSaturateWord(*(float*)src * 65535f);

    private static void fromFLTto16SE(void* dst, in void* src) =>
        *(ushort*)dst = CHANGE_ENDIAN(_cmsQuickSaturateWord(*(float*)src * 65535f));

    private static void copy32(void* dst, in void* src) =>
        memmove(dst, src, _sizeof<float>());

    private static void fromFLTtoDBL(void* dst, in void* src) =>
        *(double*)dst = *(float*)src;

    private static void fromFLTtoHLF(void* dst, in void* src) =>
        *(ushort*)dst = _cmsFloat2Half(*(float*)src);

    private static void fromHLFto8(void* dst, in void* src) =>
        *(byte*)dst = _cmsQuickSaturateByte(_cmsHalf2Float(*(ushort*)src) * 255.0);

    private static void fromHLFto16(void* dst, in void* src) =>
        *(ushort*)dst = _cmsQuickSaturateWord(_cmsHalf2Float(*(ushort*)src) * 65535f);

    private static void fromHLFto16SE(void* dst, in void* src) =>
        *(ushort*)dst = CHANGE_ENDIAN(_cmsQuickSaturateWord(_cmsHalf2Float(*(ushort*)src) * 65535f));

    private static void fromHLFtoFLT(void* dst, in void* src) =>
        *(double*)dst = _cmsHalf2Float(*(ushort*)src);

    private static void fromHLFtoDBL(void* dst, in void* src) =>
        *(double*)dst = _cmsHalf2Float(*(ushort*)src);

    private static void fromDBLto8(void* dst, in void* src) =>
        *(byte*)dst = _cmsQuickSaturateByte(*(double*)src * 255.0);

    private static void fromDBLto16(void* dst, in void* src) =>
        *(ushort*)dst = _cmsQuickSaturateWord(*(double*)src * 65535f);

    private static void fromDBLto16SE(void* dst, in void* src) =>
        *(ushort*)dst = CHANGE_ENDIAN(_cmsQuickSaturateWord(*(double*)src * 65535f));

    private static void fromDBLtoFLT(void* dst, in void* src) =>
        *(float*)dst = (float)*(double*)src;

    private static void fromDBLtoHLF(void* dst, in void* src) =>
        *(ushort*)dst = _cmsFloat2Half((float)*(double*)src);

    private static void copy64(void* dst, in void* src) =>
        memmove(dst, src, _sizeof<double>());

    private static int FormatterPos(uint frm) =>
        (T_BYTES(frm), T_FLOAT(frm) is not 0) switch
        {
            (0, true) => 5,     // DBL
            (2, true) => 3,     // HLF
            (4, true) => 4,     // FLT
            (2, false) =>
                T_ENDIAN16(frm) is not 0
                    ? 2         // 16SE
                    : 1,        // 16
            (1, false) => 0,    // 8
            _ => -1             // not recognized
        };

    private static readonly delegate*<void*, in void*, void>[,] FormattersAlpha = new delegate*<void*, in void*, void>[,]
    {
        { &copy8, &from8to16, &from8to16SE, &from8toHLF, &from8toFLT, &from8toDBL },
        { &from16to8, &copy16, &from16to16, &from16toHLF, &from16toFLT, &from16toDBL },
        { &from16SEto8, &from16to16, &copy16, &from16SEtoHLF, &from16SEtoFLT, &from16SEtoDBL },
        { &fromHLFto8, &fromHLFto16, &fromHLFto16SE, &copy16, &fromHLFtoFLT, &fromHLFtoDBL },
        { &fromFLTto8, &fromFLTto16, &fromFLTto16SE, &fromFLTtoHLF, &copy32, &fromFLTtoDBL },
        { &fromDBLto8, &fromDBLto16, &fromDBLto16SE, &fromDBLtoHLF, &fromDBLtoFLT, &copy64 },
    };

    internal static delegate*<void*, in void*, void> _cmsGetFormatterAlpha(Context* id, uint @in, uint @out)
    {
        var in_n = FormatterPos(@in);
        var out_n = FormatterPos(@out);

        if (in_n is < 0 or > 5 || out_n is < 0 or > 5)
        {
            cmsSignalError(id, cmsERROR_UNKNOWN_EXTENSION, "Unrecognized alpha channel width");
            return null;
        }

        return FormattersAlpha[in_n, out_n];
    }

    private static void ComputeIncrementsForChunky(uint Format, uint* ComponentStartingOrder, uint* ComponentPointerIncrements)
    {
        var channels = stackalloc uint[cmsMAXCHANNELS];
        var extra = T_EXTRA(Format);
        var nchannels = T_CHANNELS(Format);
        var total_chans = nchannels + extra;
        var channelSize = PixelSize(Format);
        var pixelSize = channelSize * total_chans;

        // Sanity check
        if (total_chans is <= 0 or >= cmsMAXCHANNELS)
            return;

        memset(channels, 0, cmsMAXCHANNELS * _sizeof<uint>());

        // Separation is independent of starting point and only depends on channel size
        for (var i = 0; i < extra; i++)
            ComponentPointerIncrements[i] = pixelSize;

        // Handle do swap
        for (var i = 0u; i < total_chans; i++)
            channels[i] = T_DOSWAP(Format) is not 0 ? total_chans - i - 1 : i;

        // Handle swap first (ROL of positions), example CMYK -> KCMY | 0123 ->3012
        if (T_SWAPFIRST(Format) is not 0 && total_chans > 1)
        {
            var tmp = channels[0];
            for (var i = 0; i < total_chans - 1; i++)
                channels[i] = channels[i + 1];
            channels[total_chans - 1] = tmp;
        }

        // Handle size
        if (channelSize > 1)
            for (var i = 0; i < total_chans; i++)
                channels[i] *= channelSize;

        for (var i = 0; i < extra; i++)
            ComponentStartingOrder[i] = channels[i + nchannels];
    }

    private static void ComputeIncrementsForPlanar(uint Format, uint BytesPerPlane, uint* ComponentStartingOrder, uint* ComponentPointerIncrements)
    {
        var channels = stackalloc uint[cmsMAXCHANNELS];
        var extra = T_EXTRA(Format);
        var nchannels = T_CHANNELS(Format);
        var total_chans = nchannels + extra;
        var channelSize = PixelSize(Format);

        // Sanity check
        if (total_chans is <= 0 or >= cmsMAXCHANNELS)
            return;

        memset(channels, 0, cmsMAXCHANNELS * _sizeof<uint>());

        // Separation is independent of starting point and only depends on channel size
        for (var i = 0; i < extra; i++)
            ComponentPointerIncrements[i] = channelSize;

        // Handle do swap
        for (var i = 0u; i < total_chans; i++)
            channels[i] = T_DOSWAP(Format) is not 0 ? total_chans - i - 1 : i;

        // Handle swap first (ROL of positions), example CMYK -> KCMY | 0123 ->3012
        if (T_SWAPFIRST(Format) is not 0 && total_chans > 1)
        {
            var tmp = channels[0];
            for (var i = 0; i < total_chans - 1; i++)
                channels[i] = channels[i + 1];
            channels[total_chans - 1] = tmp;
        }

        // Handle size
        for (var i = 0; i < total_chans; i++)
            channels[i] *= BytesPerPlane;

        for (var i = 0; i < extra; i++)
            ComponentStartingOrder[i] = channels[i + nchannels];
    }

    private static void ComputeComponentIncrements(uint Format, uint BytesPerPlane, uint* ComponentStartingOrder, uint* ComponentPointerIncrements)
    {
        if (T_PLANAR(Format) is not 0)
        {
            ComputeIncrementsForPlanar(Format, BytesPerPlane, ComponentStartingOrder, ComponentPointerIncrements);
        }
        else
        {
            ComputeIncrementsForChunky(Format, ComponentStartingOrder, ComponentPointerIncrements);
        }
    }

    internal static void _cmsHandleExtraChannels(Transform* p, in void* @in, void* @out, uint PixelsPerLine, uint LineCount, Stride* Stride)
    {
        var SourceStartingOrder = stackalloc uint[cmsMAXCHANNELS];
        var SourceIncrements = stackalloc uint[cmsMAXCHANNELS];
        var DestStartingOrder = stackalloc uint[cmsMAXCHANNELS];
        var DestIncrements = stackalloc uint[cmsMAXCHANNELS];

        // Make sure we need some copy
        if ((p->dwOriginalFlags & cmsFLAGS_COPY_ALPHA) is 0)
            return;

        // Exit early if in-place color-management is occurring - no need to copy extra channels to themselves.
        if (p->InputFormat == p->OutputFormat && @in == @out)
            return;

        // Make sure we have same number of alpha channels. If not, just return as this should be checked at transform creation time.
        var nExtra = T_EXTRA(p->InputFormat);
        if (nExtra != T_EXTRA(p->OutputFormat))
            return;

        // Anything to do?
        if (nExtra is 0) return;

        // Compute the increments
        ComputeComponentIncrements(p->InputFormat, Stride->BytesPerPlaneIn, SourceStartingOrder, SourceIncrements);
        ComputeComponentIncrements(p->OutputFormat, Stride->BytesPerPlaneOut, DestStartingOrder, DestIncrements);

        // Check for conversions 8, 16, half, float, double
        var copyValueFn = _cmsGetFormatterAlpha(p->ContextID, p->InputFormat, p->OutputFormat);
        if (copyValueFn is null) return;

        if (nExtra is 1)    // Optivized routine for copying a single extra channel quickly
        {
            var SourceStrideIncrement = 0u;
            var DestStrideIncrement = 0u;

            // The loop itself
            for (var i = 0; i < LineCount; i++)
            {
                // Prepare pointers for the loop
                var SourcePtr = (byte*)@in + SourceStartingOrder[0] + SourceStrideIncrement;
                var DestPtr = (byte*)@out + DestStartingOrder[0] + DestStrideIncrement;

                for (var j = 0; j < PixelsPerLine; j++)
                {
                    copyValueFn(DestPtr, SourcePtr);

                    SourcePtr += SourceIncrements[0];
                    DestPtr += DestIncrements[0];
                }

                SourceStrideIncrement += Stride->BytesPerLineIn;
                DestStrideIncrement += Stride->BytesPerPlaneOut;
            }
        }
        else            // General case with more than one extra channel
        {
            var SourcePtr = stackalloc byte*[cmsMAXCHANNELS];
            var DestPtr = stackalloc byte*[cmsMAXCHANNELS];
            var SourceStrideIncrements = stackalloc uint[cmsMAXCHANNELS];
            var DestStrideIncrements = stackalloc uint[cmsMAXCHANNELS];

            memset(SourceStrideIncrements, 0, _sizeof<uint>() * cmsMAXCHANNELS);
            memset(DestStrideIncrements, 0, _sizeof<uint>() * cmsMAXCHANNELS);

            // The loop itself
            for (var i = 0; i < LineCount; i++)
            {
                // Prepare pointers for the loop
                for (var j = 0; j < nExtra; j++)
                {
                    SourcePtr[j] = (byte*)@in + SourceStartingOrder[j] + SourceStrideIncrements[j];
                    DestPtr[j] = (byte*)@out + DestStartingOrder[j] + DestStrideIncrements[j];
                }

                for (var j = 0; j < PixelsPerLine; j++)
                {
                    for (var k = 0; k < nExtra; k++)
                    {
                        copyValueFn(DestPtr[k], SourcePtr[k]);

                        SourcePtr[k] += SourceIncrements[k];
                        DestPtr[k] += DestIncrements[k];
                    }
                }

                for (var j = 0; j < nExtra; j++)
                {
                    SourceStrideIncrements[j] += Stride->BytesPerLineIn;
                    DestStrideIncrements[j] += Stride->BytesPerLineOut;
                }
            }
        }
    }
}
