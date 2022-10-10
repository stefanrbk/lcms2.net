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
using System.Runtime.InteropServices;

namespace lcms2.types;

/// <summary>
///     Transform factory
/// </summary>
/// <remarks>Implements the <c>_cmsTransform2Factory</c> typedef.</remarks>
public delegate void Transform2Factory(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

/// <summary>
///     Transform function
/// </summary>
/// <remarks>Implements the <c>_cmsTransform2Fn</c> typedef.</remarks>
public delegate void Transform2Fn(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

/// <summary>
///     Transform factory
/// </summary>
/// <remarks>Implements the <c>_cmsTransformFactory</c> typedef.</remarks>
public delegate void TransformFactory(TransformFn xform, object? userData, object inputBuffer, object outputBuffer, int size, int stride);

/// <summary>
///     Legacy function, handles just ONE scanline.
/// </summary>
/// <remarks>Implements the <c>_cmsTransformFn</c> typedef.</remarks>
public delegate void TransformFn(Transform cargo, object inputBuffer, object outputBuffer, int size, int stride);

public unsafe struct Cache
{
    #region Fields

    public fixed ushort CacheIn[maxChannels];
    public fixed ushort CacheOut[maxChannels];

    #endregion Fields
}

/// <summary>
///     Stride info for a transform.
/// </summary>
/// <remarks>Implements the <c>cmsStride</c> struct.</remarks>
public struct Stride
{
    #region Fields

    public int BytesPerLineIn;
    public int BytesPerLineOut;
    public int BytesPerPlaneIn;
    public int BytesPerPlaneOut;

    #endregion Fields
}

public class Transform
{
    #region Fields

    internal double adaptationState;

    internal Cache cache;

    internal Signature entryColorSpace;
    internal XYZ entryWhitePoint;
    internal Signature exitColorSpace;
    internal XYZ exitWhitePoint;
    internal FreeUserDataFn? freeUserData;
    internal Formatter16? fromInput;
    internal FormatterFloat? fromInputFloat;
    internal Pipeline gamutCheck;
    internal NamedColorList inputColorant;
    internal PixelFormat inputFormat, outputFormat;
    internal Pipeline lut;
    internal TransformFn? oldXform;
    internal NamedColorList outputColorant;
    internal Signature renderingIntent;
    internal Sequence sequence;
    internal object? state;
    internal Formatter16? toOutput;

    internal FormatterFloat? toOutputFloat;

    internal Transform2Fn? xform;

    #endregion Fields

    #region Properties

    /// <summary>
    ///     Retrieve original flags
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsGetTransformFlags</c> and <c>_cmsGetTransformUserData</c> functions.
    /// </remarks>
    public uint Flags { get; internal set; }

    /// <summary>
    ///     Retrieve 16 bit formatters
    /// </summary>
    /// <remarks>Implements the <c>_cmsGetTransformFormatters16</c> function.</remarks>
    public (Formatter16? FromInput, Formatter16? ToOutput) Formatters16 =>
        (fromInput, toOutput);

    /// <summary>
    ///     Retrieve float formatters
    /// </summary>
    /// <remarks>Implements the <c>_cmsGetTransformFormattersFloat</c> function.</remarks>
    public (FormatterFloat? FromInput, FormatterFloat? ToOutput) FormattersFloat =>
        (fromInputFloat, toOutputFloat);

    public NamedColorList? NamedColorList =>
        /** Original Code (cmsnamed.c line: 756)
         **
         ** // Retrieve the named color list from a transform. Should be first element in the LUT
         ** cmsNAMEDCOLORLIST* CMSEXPORT cmsGetNamedColorList(cmsHTRANSFORM xform)
         ** {
         **     _cmsTRANSFORM* v = (_cmsTRANSFORM*) xform;
         **     cmsStage* mpe  = v ->Lut->Elements;
         **
         **     if (mpe ->Type != cmsSigNamedColorElemType) return NULL;
         **     return (cmsNAMEDCOLORLIST*) mpe ->Data;
         ** }
         **/
        (lut.elements?.Data as Stage.NamedColorData)?.List;

    /// <summary>
    ///     User data as specified by the factory
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsSetTransformUserData</c> and <c>_cmsGetTransformUserData</c> functions.
    /// </remarks>
    public object? UserData { get; set; }

    #endregion Properties

    #region Private Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort ChangeEndian(ushort w) =>
        (ushort)((ushort)(w << 8) | (w >> 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort FromLabV2ToLabV4(ushort x)
    {
        var a = ((x << 8) | x) >> 8;
        return (a > 0xFFFF)
            ? (ushort)0xFFFF
            : (ushort)a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort FromLabV4ToLabV2(ushort x) =>
        (ushort)(((x << 8) + 0x80) / 257);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ReverseFlavor(byte x) =>
        unchecked((byte)(0xFF - x));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort ReverseFlavor(ushort x) =>
        unchecked((ushort)(0xFFFF - x));

    private Span<byte> Unroll1Byte(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = From8to16(acc[0]); acc = acc[1..]; // L

        return acc;
    }

    private Span<byte> Unroll1ByteReversed(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = ReverseFlavor(From8to16(acc[0])); acc = acc[1..]; // L

        return acc;
    }

    private Span<byte> Unroll1ByteSkip1(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = From8to16(acc[0]); acc = acc[1..]; // L
        acc = acc[1..];

        return acc;
    }

    private Span<byte> Unroll1ByteSkip2(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = From8to16(acc[0]); acc = acc[1..]; // L
        acc = acc[2..];

        return acc;
    }

    private Span<byte> Unroll1Word(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // L

        return acc;
    }

    private Span<byte> Unroll1WordReversed(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = ReverseFlavor(BitConverter.ToUInt16(acc)); acc = acc[2..]; // L

        return acc;
    }

    private Span<byte> Unroll1WordSkip3(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // L
        acc = acc[8..];

        return acc;
    }

    private Span<byte> Unroll2Bytes(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = From8to16(acc[0]); acc = acc[1..]; // ch1
        wIn[1] = From8to16(acc[0]); acc = acc[1..]; // ch2

        return acc;
    }

    private Span<byte> Unroll2Words(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = BitConverter.ToUInt16(acc); acc = acc[2..]; // ch1
        wIn[1] = BitConverter.ToUInt16(acc); acc = acc[2..]; // ch2

        return acc;
    }

    private Span<byte> Unroll3Bytes(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = From8to16(acc[0]); acc = acc[1..]; // R
        wIn[1] = From8to16(acc[0]); acc = acc[1..]; // G
        wIn[2] = From8to16(acc[0]); acc = acc[1..]; // B

        return acc;
    }

    private Span<byte> Unroll3BytesSkip1Swap(ushort[] wIn, Span<byte> acc, int _)
    {
        acc = acc[1..]; // A
        wIn[2] = From8to16(acc[0]); acc = acc[1..]; // B
        wIn[1] = From8to16(acc[0]); acc = acc[1..]; // G
        wIn[0] = From8to16(acc[0]); acc = acc[1..]; // R

        return acc;
    }

    private Span<byte> Unroll3BytesSkip1SwapFirst(ushort[] wIn, Span<byte> acc, int _)
    {
        acc = acc[1..]; // A
        wIn[0] = From8to16(acc[0]); acc = acc[1..]; // R
        wIn[1] = From8to16(acc[0]); acc = acc[1..]; // G
        wIn[2] = From8to16(acc[0]); acc = acc[1..]; // B

        return acc;
    }

    private Span<byte> Unroll3BytesSkip1SwapSwapFirst(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[2] = From8to16(acc[0]); acc = acc[1..]; // B
        wIn[1] = From8to16(acc[0]); acc = acc[1..]; // G
        wIn[0] = From8to16(acc[0]); acc = acc[1..]; // R
        acc = acc[1..]; // A

        return acc;
    }

    private Span<byte> Unroll3BytesSwap(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[2] = From8to16(acc[0]); acc = acc[1..]; // B
        wIn[1] = From8to16(acc[0]); acc = acc[1..]; // G
        wIn[0] = From8to16(acc[0]); acc = acc[1..]; // R

        return acc;
    }

    private Span<byte> Unroll3Words(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = BitConverter.ToUInt16(acc); acc = acc[2..]; // C R
        wIn[1] = BitConverter.ToUInt16(acc); acc = acc[2..]; // M G
        wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // Y B

        return acc;
    }

    private Span<byte> Unroll3WordsSkip1Swap(ushort[] wIn, Span<byte> acc, int _)
    {
        acc = acc[2..]; // A
        wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // B
        wIn[1] = BitConverter.ToUInt16(acc); acc = acc[2..]; // G
        wIn[0] = BitConverter.ToUInt16(acc); acc = acc[2..]; // R

        return acc;
    }

    private Span<byte> Unroll3WordsSkip1SwapFirst(ushort[] wIn, Span<byte> acc, int _)
    {
        acc = acc[2..]; // A
        wIn[0] = BitConverter.ToUInt16(acc); acc = acc[2..]; // R
        wIn[1] = BitConverter.ToUInt16(acc); acc = acc[2..]; // G
        wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // B

        return acc;
    }

    private Span<byte> Unroll3WordsSwap(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // Y B
        wIn[1] = BitConverter.ToUInt16(acc); acc = acc[2..]; // M G
        wIn[0] = BitConverter.ToUInt16(acc); acc = acc[2..]; // C R

        return acc;
    }

    private Span<byte> Unroll4Bytes(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = From8to16(acc[0]); acc = acc[1..]; // C
        wIn[1] = From8to16(acc[0]); acc = acc[1..]; // M
        wIn[2] = From8to16(acc[0]); acc = acc[1..]; // Y
        wIn[3] = From8to16(acc[0]); acc = acc[1..]; // K

        return acc;
    }

    private Span<byte> Unroll4BytesReverse(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = From8to16(ReverseFlavor(acc[0])); acc = acc[1..]; // C
        wIn[1] = From8to16(ReverseFlavor(acc[0])); acc = acc[1..]; // M
        wIn[2] = From8to16(ReverseFlavor(acc[0])); acc = acc[1..]; // Y
        wIn[3] = From8to16(ReverseFlavor(acc[0])); acc = acc[1..]; // K

        return acc;
    }

    private Span<byte> Unroll4BytesSwap(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[3] = From8to16(acc[0]); acc = acc[1..]; // K
        wIn[2] = From8to16(acc[0]); acc = acc[1..]; // Y
        wIn[1] = From8to16(acc[0]); acc = acc[1..]; // M
        wIn[0] = From8to16(acc[0]); acc = acc[1..]; // C

        return acc;
    }

    private Span<byte> Unroll4BytesSwapFirst(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[3] = From8to16(acc[0]); acc = acc[1..]; // K
        wIn[0] = From8to16(acc[0]); acc = acc[1..]; // C
        wIn[1] = From8to16(acc[0]); acc = acc[1..]; // M
        wIn[2] = From8to16(acc[0]); acc = acc[1..]; // Y

        return acc;
    }

    private Span<byte> Unroll4BytesSwapSwapFirst(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[2] = From8to16(acc[0]); acc = acc[1..]; // K
        wIn[1] = From8to16(acc[0]); acc = acc[1..]; // Y
        wIn[0] = From8to16(acc[0]); acc = acc[1..]; // M
        wIn[3] = From8to16(acc[0]); acc = acc[1..]; // C

        return acc;
    }

    private Span<byte> Unroll4Words(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = BitConverter.ToUInt16(acc); acc = acc[2..]; // C
        wIn[1] = BitConverter.ToUInt16(acc); acc = acc[2..]; // M
        wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // Y
        wIn[3] = BitConverter.ToUInt16(acc); acc = acc[2..]; // K

        return acc;
    }

    private Span<byte> Unroll4WordsReverse(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = ReverseFlavor(BitConverter.ToUInt16(acc)); acc = acc[2..]; // C
        wIn[1] = ReverseFlavor(BitConverter.ToUInt16(acc)); acc = acc[2..]; // M
        wIn[2] = ReverseFlavor(BitConverter.ToUInt16(acc)); acc = acc[2..]; // Y
        wIn[3] = ReverseFlavor(BitConverter.ToUInt16(acc)); acc = acc[2..]; // K

        return acc;
    }

    private Span<byte> Unroll4WordsSwap(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[3] = BitConverter.ToUInt16(acc); acc = acc[2..]; // K
        wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // Y
        wIn[1] = BitConverter.ToUInt16(acc); acc = acc[2..]; // M
        wIn[0] = BitConverter.ToUInt16(acc); acc = acc[2..]; // C

        return acc;
    }

    private Span<byte> Unroll4WordsSwapFirst(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[3] = BitConverter.ToUInt16(acc); acc = acc[2..]; // K
        wIn[0] = BitConverter.ToUInt16(acc); acc = acc[2..]; // C
        wIn[1] = BitConverter.ToUInt16(acc); acc = acc[2..]; // M
        wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // Y

        return acc;
    }

    private Span<byte> Unroll4WordsSwapSwapFirst(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // K
        wIn[1] = BitConverter.ToUInt16(acc); acc = acc[2..]; // Y
        wIn[0] = BitConverter.ToUInt16(acc); acc = acc[2..]; // M
        wIn[3] = BitConverter.ToUInt16(acc); acc = acc[2..]; // C

        return acc;
    }

    private Span<byte> UnrollALabV2_8(ushort[] wIn, Span<byte> acc, int _)
    {
        acc = acc[1..]; // A
        wIn[0] = FromLabV2ToLabV4(From8to16(acc[0])); acc = acc[1..]; // L
        wIn[1] = FromLabV2ToLabV4(From8to16(acc[0])); acc = acc[1..]; // a
        wIn[2] = FromLabV2ToLabV4(From8to16(acc[0])); acc = acc[1..]; // b

        return acc;
    }

    private Span<byte> UnrollAnyWords(ushort[] wIn, Span<byte> acc, int _)
    {
        var nChan = inputFormat.Channels;
        var swapEndian = inputFormat.EndianSwap;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;

        if (extraFirst)
            acc = acc[(extra * sizeof(ushort))..];

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            var v = BitConverter.ToUInt16(acc);

            if (swapEndian)
                v = ChangeEndian(v);

            wIn[index] = reverse is ColorFlavor.Subtractive ? ReverseFlavor(v) : v;
            acc = acc[sizeof(ushort)..];
        }

        if (!extraFirst)
            acc = acc[(extra * sizeof(ushort))..];

        if (extra is 0 && swapFirst)
        {
            var tmp = wIn[0];

            wIn.CopyTo(wIn, 1);
            wIn[nChan - 1] = tmp;
        }

        return acc;
    }

    private Span<byte> UnrollAnyWordsPremul(ushort[] wIn, Span<byte> acc, int _)
    {
        var nChan = inputFormat.Channels;
        var swapEndian = inputFormat.EndianSwap;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extraFirst = doSwap ^ swapFirst;

        var alpha = extraFirst ? acc[0] : acc[nChan - 1];
        var alpha_factor = (uint)ToFixedDomain(From8to16(alpha));

        if (extraFirst)
            acc = acc[sizeof(ushort)..];

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            var v = (uint)BitConverter.ToUInt16(acc);

            if (swapEndian)
                v = ChangeEndian((ushort)v);

            v = (v << 16) / alpha_factor;
            if (v > 0xFFFF) v = 0xFFFF;

            wIn[index] = reverse is ColorFlavor.Subtractive ? ReverseFlavor((ushort)v) : (ushort)v;
            acc = acc[sizeof(ushort)..];
        }

        if (!extraFirst)
            acc = acc[sizeof(ushort)..];

        return acc;
    }

    private Span<byte> UnrollChunkyBytes(ushort[] wIn, Span<byte> acc, int _)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var premul = inputFormat.PremultipliedAlpha;

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
            var index = doSwap ? (nChan - i - 1) : i;

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
        {
            var tmp = wIn[0];

            wIn.CopyTo(wIn, 1);
            wIn[nChan - 1] = tmp;
        }

        return acc;
    }

    private Span<byte> UnrollDouble1Chan(ushort[] wIn, Span<byte> acc, int _)
    {
        var inks = BitConverter.ToDouble(acc);

        wIn[0] = wIn[1] = wIn[2] = QuickSaturateWord(inks * 65535.0);

        return acc[sizeof(double)..];
    }

    private Span<byte> UnrollDoubleTo16(ushort[] wIn, Span<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = inputFormat.Planar;
        var maximum = inputFormat.IsInkSpace ? 655.35 : 65535.0;

        stride /= inputFormat.PixelSize;

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            var v = (float)BitConverter.ToDouble(
                acc[((i + start) * (
                    planar
                        ? stride
                        : 1) * sizeof(double))..]);

            var vi = QuickSaturateWord(v * maximum);

            vi = reverse is ColorFlavor.Subtractive ? ReverseFlavor(vi) : vi;

            wIn[index] = vi;
        }

        if (extra is 0 && swapFirst)
        {
            var tmp = wIn[0];

            wIn.CopyTo(wIn, 1);
            wIn[nChan - 1] = tmp;
        }

        return inputFormat.Planar
            ? acc[sizeof(double)..]
            : acc[((nChan + extra) * sizeof(double))..];
    }

    private Span<byte> UnrollFloatTo16(ushort[] wIn, Span<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = inputFormat.Planar;
        var maximum = inputFormat.IsInkSpace ? 655.35 : 65535.0;

        stride /= inputFormat.PixelSize;

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            var v = BitConverter.ToSingle(
                acc[((i + start) * (
                    planar
                        ? stride
                        : 1) * sizeof(float))..]);

            var vi = QuickSaturateWord(v * maximum);

            vi = reverse is ColorFlavor.Subtractive ? ReverseFlavor(vi) : vi;

            wIn[index] = vi;
        }

        if (extra is 0 && swapFirst)
        {
            var tmp = wIn[0];

            wIn.CopyTo(wIn, 1);
            wIn[nChan - 1] = tmp;
        }

        return inputFormat.Planar
            ? acc[sizeof(float)..]
            : acc[((nChan + extra) * sizeof(float))..];
    }

    private Span<byte> UnrollLabDoubleTo16(ushort[] wIn, Span<byte> acc, int stride)
    {
        Lab lab;

        if (inputFormat.Planar)
        {
            var pos_L = acc;
            var pos_a = acc[stride..];
            var pos_b = pos_a[stride..];

            lab.L = BitConverter.ToDouble(pos_L);
            lab.a = BitConverter.ToDouble(pos_a);
            lab.b = BitConverter.ToDouble(pos_b);

            lab.ToLabEncodedArray().CopyTo(wIn, 0);
            return acc[sizeof(double)..];
        }
        else
        {
            var pos_L = acc;
            var pos_a = acc[sizeof(double)..];
            var pos_b = pos_a[sizeof(double)..];

            lab.L = BitConverter.ToDouble(pos_L);
            lab.a = BitConverter.ToDouble(pos_a);
            lab.b = BitConverter.ToDouble(pos_b);

            lab.ToLabEncodedArray().CopyTo(wIn, 0);

            return acc[((sizeof(double) * 3) + (inputFormat.ExtraSamples * sizeof(double)))..];
        }
    }

    private Span<byte> UnrollLabFloatTo16(ushort[] wIn, Span<byte> acc, int stride)
    {
        Lab lab;

        if (inputFormat.Planar)
        {
            var pos_L = acc;
            var pos_a = acc[stride..];
            var pos_b = pos_a[stride..];

            lab.L = BitConverter.ToSingle(pos_L);
            lab.a = BitConverter.ToSingle(pos_a);
            lab.b = BitConverter.ToSingle(pos_b);

            lab.ToLabEncodedArray().CopyTo(wIn, 0);
            return acc[sizeof(float)..];
        }
        else
        {
            var pos_L = acc;
            var pos_a = acc[sizeof(float)..];
            var pos_b = pos_a[sizeof(float)..];

            lab.L = BitConverter.ToSingle(pos_L);
            lab.a = BitConverter.ToSingle(pos_a);
            lab.b = BitConverter.ToSingle(pos_b);

            lab.ToLabEncodedArray().CopyTo(wIn, 0);

            return acc[((3 + inputFormat.ExtraSamples) * sizeof(float))..];
        }
    }

    private Span<byte> UnrollLabV2_16(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = FromLabV2ToLabV4(BitConverter.ToUInt16(acc)); acc = acc[2..]; // L
        wIn[1] = FromLabV2ToLabV4(BitConverter.ToUInt16(acc)); acc = acc[2..]; // a
        wIn[2] = FromLabV2ToLabV4(BitConverter.ToUInt16(acc)); acc = acc[2..]; // b

        return acc;
    }

    private Span<byte> UnrollLabV2_8(ushort[] wIn, Span<byte> acc, int _)
    {
        wIn[0] = FromLabV2ToLabV4(From8to16(acc[0])); acc = acc[1..]; // L
        wIn[1] = FromLabV2ToLabV4(From8to16(acc[0])); acc = acc[1..]; // a
        wIn[2] = FromLabV2ToLabV4(From8to16(acc[0])); acc = acc[1..]; // b

        return acc;
    }

    private Span<byte> UnrollPlanarBytes(ushort[] wIn, Span<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var swapFirst = inputFormat.SwapFirst;
        var reverse = inputFormat.Flavor;
        var extraFirst = doSwap ^ swapFirst;
        var extra = inputFormat.ExtraSamples;
        var premul = inputFormat.PremultipliedAlpha;
        var init = acc;
        var alphaFactor = 1;

        if (extraFirst)
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(From8to16(acc[0]));

            acc = acc[(extra * stride)..];
        }
        else
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(From8to16(acc[nChan]));
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            uint v = From8to16(acc[0]);

            v = reverse is ColorFlavor.Subtractive ? ReverseFlavor((ushort)v) : v;

            if (premul && alphaFactor > 0)
            {
                v = (v << 16) / (uint)alphaFactor;
                if (v > 0xFFFF) v = 0xFFFF;
            }

            wIn[index] = (ushort)v;
            acc = acc[stride..];
        }

        return init[1..];
    }

    private Span<byte> UnrollPlanarWords(ushort[] wIn, Span<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapEndian = inputFormat.EndianSwap;
        var init = acc;

        if (doSwap)
            acc = acc[(inputFormat.ExtraSamples * stride)..];

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            var v = BitConverter.ToUInt16(acc);

            if (swapEndian)
                v = ChangeEndian(v);

            wIn[index] = reverse is ColorFlavor.Subtractive ? ReverseFlavor(v) : v;

            acc = acc[stride..];
        }

        return init[sizeof(ushort)..];
    }

    private Span<byte> UnrollPlanarWordsPremul(ushort[] wIn, Span<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var swapFirst = inputFormat.SwapFirst;
        var reverse = inputFormat.Flavor;
        var swapEndian = inputFormat.EndianSwap;
        var extraFirst = doSwap ^ swapFirst;
        var init = acc;

        var alpha = extraFirst ? acc[0] : acc[(nChan - 1) * stride];
        var alpha_factor = (uint)ToFixedDomain(From8to16(alpha));

        if (extraFirst)
            acc = acc[stride..];

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            var v = (uint)BitConverter.ToUInt16(acc);

            if (swapEndian)
                v = ChangeEndian((ushort)v);

            v = (v << 16) / alpha_factor;
            if (v > 0xFFFF) v = 0xFFFF;

            wIn[index] = reverse is ColorFlavor.Subtractive ? ReverseFlavor((ushort)v) : (ushort)v;

            acc = acc[stride..];
        }

        return init[sizeof(ushort)..];
    }

    private Span<byte> UnrollXYZDoubleTo16(ushort[] wIn, Span<byte> acc, int stride)
    {
        XYZ xyz;

        if (inputFormat.Planar)
        {
            var posX = acc;
            var posY = acc[stride..];
            var posZ = posY[stride..];

            xyz.X = BitConverter.ToDouble(posX);
            xyz.Y = BitConverter.ToDouble(posY);
            xyz.Z = BitConverter.ToDouble(posZ);

            xyz.ToXYZEncodedArray().CopyTo(wIn, 0);
            return acc[sizeof(double)..];
        }
        else
        {
            var posX = acc;
            var posY = acc[sizeof(double)..];
            var posZ = posY[sizeof(double)..];

            xyz.X = BitConverter.ToDouble(posX);
            xyz.Y = BitConverter.ToDouble(posY);
            xyz.Z = BitConverter.ToDouble(posZ);

            xyz.ToXYZEncodedArray().CopyTo(wIn, 0);

            return acc[((3 + inputFormat.ExtraSamples) * sizeof(double))..];
        }
    }

    private Span<byte> UnrollXYZFloatTo16(ushort[] wIn, Span<byte> acc, int stride)
    {
        XYZ xyz;

        if (inputFormat.Planar)
        {
            var posX = acc;
            var posY = acc[stride..];
            var posZ = posY[stride..];

            xyz.X = BitConverter.ToSingle(posX);
            xyz.Y = BitConverter.ToSingle(posY);
            xyz.Z = BitConverter.ToSingle(posZ);

            xyz.ToXYZEncodedArray().CopyTo(wIn, 0);
            return acc[sizeof(float)..];
        }
        else
        {
            var posX = acc;
            var posY = acc[sizeof(float)..];
            var posZ = posY[sizeof(float)..];

            xyz.X = BitConverter.ToSingle(posX);
            xyz.Y = BitConverter.ToSingle(posY);
            xyz.Z = BitConverter.ToSingle(posZ);

            xyz.ToXYZEncodedArray().CopyTo(wIn, 0);

            return acc[((3 + inputFormat.ExtraSamples) * sizeof(float))..];
        }
    }

    #endregion Private Methods

    #region Structs

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Factory
    {
        [FieldOffset(0)]
        public TransformFactory LegacyXform;

        [FieldOffset(0)]
        public Transform2Factory Xform;
    }

    #endregion Structs
}
