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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort ChangeEndian(ReadOnlySpan<byte> span) =>
        ChangeEndian(BitConverter.ToUInt16(span));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort From8to16Reversed(byte value) =>
        From8to16(ReverseFlavor(value));

    private void Lab4toFloat(Span<float> wIn, ReadOnlySpan<ushort> lab4)
    {
        var L = lab4[0] / 655.35f;
        var a = (lab4[1] / 257f) - 128f;
        var b = (lab4[2] / 257f) - 128f;

        wIn[0] = L / 100f;
        wIn[1] = (a + 128f) / 255f;
        wIn[2] = (b + 128f) / 255f;
    }

    private Span<byte> Pack1Byte(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];

        return ptr[1..].Span;
    }

    private Span<byte> Pack1ByteReversed(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, s => From16to8(ReverseFlavor(s)));
        ptr[0] = wOut[0];

        return ptr[1..].Span;
    }

    private Span<byte> Pack1ByteSkip1(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];

        return ptr[2..].Span;
    }

    private Span<byte> Pack1ByteSkip1SwapFirst(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[1] = wOut[0];

        return ptr[2..].Span;
    }

    private Span<byte> Pack1Word(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];

        return ptr[1..].Span;
    }

    private Span<byte> Pack1WordBigEndian(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(ChangeEndian(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[3..].Span;
    }

    private Span<byte> Pack1WordReversed(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(ReverseFlavor(s)));
        ptr[0] = wOut[0];

        return ptr[1..].Span;
    }

    private Span<byte> Pack1WordSkip1(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];

        return ptr[2..].Span;
    }

    private Span<byte> Pack1WordSkip1SwapFirst(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[1] = wOut[0];

        return ptr[2..].Span;
    }

    private Span<byte> Pack3Bytes(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[3..].Span;
    }

    private Span<byte> Pack3BytesAndSkip1(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[4..].Span;
    }

    private Span<byte> Pack3BytesAndSkip1Optimized(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[0] = (byte)(wOut[0] & 0xFF);
        output[1] = (byte)(wOut[1] & 0xFF);
        output[2] = (byte)(wOut[2] & 0xFF);

        return output[4..];
    }

    private Span<byte> Pack3BytesAndSkip1Swap(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[1] = wOut[2];
        ptr[2] = wOut[1];
        ptr[3] = wOut[0];

        return ptr[4..].Span;
    }

    private Span<byte> Pack3BytesAndSkip1SwapFirst(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[1] = wOut[0];
        ptr[2] = wOut[1];
        ptr[3] = wOut[2];

        return ptr[4..].Span;
    }

    private Span<byte> Pack3BytesAndSkip1SwapFirstOptimized(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[1] = (byte)(wOut[0] & 0xFF);
        output[2] = (byte)(wOut[1] & 0xFF);
        output[3] = (byte)(wOut[2] & 0xFF);

        return output[4..];
    }

    private Span<byte> Pack3BytesAndSkip1SwapOptimized(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[1] = (byte)(wOut[2] & 0xFF);
        output[2] = (byte)(wOut[1] & 0xFF);
        output[3] = (byte)(wOut[0] & 0xFF);

        return output[4..];
    }

    private Span<byte> Pack3BytesAndSkip1SwapSwapFirst(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[2];
        ptr[1] = wOut[1];
        ptr[2] = wOut[0];

        return ptr[4..].Span;
    }

    private Span<byte> Pack3BytesAndSkip1SwapSwapFirstOptimized(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[0] = (byte)(wOut[2] & 0xFF);
        output[1] = (byte)(wOut[1] & 0xFF);
        output[2] = (byte)(wOut[0] & 0xFF);

        return output[4..];
    }

    private Span<byte> Pack3BytesOptimized(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[0] = (byte)(wOut[0] & 0xFF);
        output[1] = (byte)(wOut[1] & 0xFF);
        output[2] = (byte)(wOut[2] & 0xFF);

        return output[3..];
    }

    private Span<byte> Pack3BytesSwap(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[2];
        ptr[1] = wOut[1];
        ptr[2] = wOut[0];

        return ptr[3..].Span;
    }

    private Span<byte> Pack3BytesSwapOptimized(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        output[0] = (byte)(wOut[2] & 0xFF);
        output[1] = (byte)(wOut[1] & 0xFF);
        output[2] = (byte)(wOut[0] & 0xFF);

        return output[3..];
    }

    private Span<byte> Pack3Words(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[3..].Span;
    }

    private Span<byte> Pack3WordsAndSkip1(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[4..].Span;
    }

    private Span<byte> Pack3WordsAndSkip1Swap(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[1] = wOut[2];
        ptr[2] = wOut[1];
        ptr[3] = wOut[0];

        return ptr[4..].Span;
    }

    private Span<byte> Pack3WordsAndSkip1SwapFirst(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[1] = wOut[0];
        ptr[2] = wOut[1];
        ptr[3] = wOut[2];

        return ptr[4..].Span;
    }

    private Span<byte> Pack3WordsAndSkip1SwapSwapFirst(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[2];
        ptr[1] = wOut[1];
        ptr[2] = wOut[0];

        return ptr[4..].Span;
    }

    private Span<byte> Pack3WordsBigEndian(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(ChangeEndian(s)));
        ptr[0] = wOut[0];

        return ptr[1..].Span;
    }

    private Span<byte> Pack3WordsSwap(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[2];
        ptr[1] = wOut[1];
        ptr[2] = wOut[0];

        return ptr[3..].Span;
    }

    private Span<byte> Pack4Bytes(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private Span<byte> Pack4BytesReverse(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, s => ReverseFlavor(From16to8(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private Span<byte> Pack4BytesSwap(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[3];
        ptr[1] = wOut[2];
        ptr[2] = wOut[1];
        ptr[3] = wOut[0];

        return ptr[4..].Span;
    }

    private Span<byte> Pack4BytesSwapFirst(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[3];
        ptr[1] = wOut[0];
        ptr[2] = wOut[1];
        ptr[3] = wOut[2];

        return ptr[4..].Span;
    }

    private Span<byte> Pack4BytesSwapSwapFirst(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, From16to8);
        ptr[0] = wOut[2];
        ptr[1] = wOut[1];
        ptr[2] = wOut[0];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private Span<byte> Pack4Words(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private Span<byte> Pack4WordsBigEndian(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(ChangeEndian(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private Span<byte> Pack4WordsReverse(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(ReverseFlavor(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];
        ptr[3] = wOut[3];

        return ptr[4..].Span;
    }

    private Span<byte> Pack4WordsSwap(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, BitConverter.GetBytes);
        ptr[0] = wOut[3];
        ptr[1] = wOut[2];
        ptr[2] = wOut[1];
        ptr[3] = wOut[0];

        return ptr[4..].Span;
    }

    private Span<byte> Pack6Bytes(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
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

    private Span<byte> Pack6BytesSwap(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
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

    private Span<byte> Pack6Words(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
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

    private Span<byte> Pack6WordsSwap(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
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

    private Span<byte> PackALabV2_8(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, s => From16to8(FromLabV4ToLabV2(s)));
        ptr[1] = wOut[0];
        ptr[2] = wOut[1];
        ptr[3] = wOut[2];

        return ptr[4..].Span;
    }

    private Span<byte> PackChunkyBytes(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var nChan = outputFormat.Channels;
        var doSwap = outputFormat.SwapAll;
        var reverse = outputFormat.Flavor;
        var extra = outputFormat.ExtraSamples;
        var swapFirst = outputFormat.SwapFirst;
        var premul = outputFormat.PremultipliedAlpha;
        var extraFirst = doSwap ^ swapFirst;
        var v = (ushort)0;
        var alphaFactor = 0;

        var swap1 = output;
        var ptr = output.Converter(From8to16, From16to8);

        if (extraFirst)
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[0]);

            output = output[extra..];
        }
        else
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[nChan]);
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;

            v = wOut[index];

            if (reverse is ColorFlavor.Subtractive)
                v = ReverseFlavor(v);

            if (premul && alphaFactor != 0)
                v = (ushort)(((v * alphaFactor) + 0x8000) >> 16);

            ptr[0] = v;
            ptr++;
        }

        if (!extraFirst)
            ptr += extra;

        if (extra is 0 && swapFirst)
            RollingShift(swap1[..nChan]);

        return ptr.Span;
    }

    private Span<byte> PackChunkyWords(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var nChan = outputFormat.Channels;
        var swapEndian = outputFormat.EndianSwap;
        var doSwap = outputFormat.SwapAll;
        var reverse = outputFormat.Flavor;
        var extra = outputFormat.ExtraSamples;
        var swapFirst = outputFormat.SwapFirst;
        var premul = outputFormat.PremultipliedAlpha;
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
            var index = doSwap ? (nChan - i - 1) : i;

            v = wOut[index];

            if (swapEndian)
                v = ChangeEndian(v);

            if (reverse is ColorFlavor.Subtractive)
                v = ReverseFlavor(v);

            if (premul && alphaFactor != 0)
                v = (ushort)(((v * alphaFactor) + 0x8000) >> 16);

            ptr[0] = v;
            ptr++;
        }

        if (!extraFirst)
            ptr += extra;

        if (extra is 0 && swapFirst)
            RollingShift(swap1[..nChan]);

        return ptr.Span;
    }

    private Span<byte> PackDoubleFrom16(ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var nChan = outputFormat.Channels;
        var doSwap = outputFormat.SwapAll;
        var reverse = outputFormat.Flavor;
        var extra = outputFormat.ExtraSamples;
        var swapFirst = outputFormat.SwapFirst;
        var planar = outputFormat.Planar;
        var extraFirst = doSwap ^ swapFirst;
        var maximum = outputFormat.IsInkSpace ? 655.35 : 65535.0;
        var v = 0d;

        var ptr = output.UpCaster(BitConverter.ToDouble, BitConverter.GetBytes);
        var swap1 = ptr;

        if (planar)
            stride /= outputFormat.PixelSize;
        else
            stride = 1;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;

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

    private Span<byte> PackDoublesFromFloat(ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var nChan = outputFormat.Channels;
        var doSwap = outputFormat.SwapAll;
        var reverse = outputFormat.Flavor;
        var extra = outputFormat.ExtraSamples;
        var swapFirst = outputFormat.SwapFirst;
        var planar = outputFormat.Planar;
        var extraFirst = doSwap ^ swapFirst;
        var maximum = outputFormat.IsInkSpace ? 100.0 : 1.0;
        var v = 0d;

        var ptr = output.UpCaster(BitConverter.ToDouble, BitConverter.GetBytes);
        var swap1 = ptr;

        if (planar)
            stride /= outputFormat.PixelSize;
        else
            stride = 1;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;

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

    private Span<byte> PackFloatFrom16(ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var nChan = outputFormat.Channels;
        var doSwap = outputFormat.SwapAll;
        var reverse = outputFormat.Flavor;
        var extra = outputFormat.ExtraSamples;
        var swapFirst = outputFormat.SwapFirst;
        var planar = outputFormat.Planar;
        var extraFirst = doSwap ^ swapFirst;
        var maximum = outputFormat.IsInkSpace ? 655.35 : 65535.0;
        var v = 0d;

        var ptr = output.UpCaster(BitConverter.ToSingle, BitConverter.GetBytes);
        var swap1 = ptr;

        if (planar)
            stride /= outputFormat.PixelSize;
        else
            stride = 1;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;

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

    private Span<byte> PackFloatsFromFloat(ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var nChan = outputFormat.Channels;
        var doSwap = outputFormat.SwapAll;
        var reverse = outputFormat.Flavor;
        var extra = outputFormat.ExtraSamples;
        var swapFirst = outputFormat.SwapFirst;
        var planar = outputFormat.Planar;
        var extraFirst = doSwap ^ swapFirst;
        var maximum = outputFormat.IsInkSpace ? 100.0 : 1.0;
        var v = 0d;

        var ptr = output.UpCaster(BitConverter.ToSingle, BitConverter.GetBytes);
        var swap1 = ptr;

        if (planar)
            stride /= outputFormat.PixelSize;
        else
            stride = 1;

        var start = extraFirst ? extra : 0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;

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

    private Span<byte> PackLabDoubleFrom16(ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, double>(null!, BitConverter.GetBytes);
        var lab = new LabEncoded(wOut).ToLab();

        if (outputFormat.Planar)
        {
            @out[0] = lab.L;
            @out[stride] = lab.a;
            @out[stride * 2] = lab.b;

            return @out[1..].Span;
        }
        else
        {
            lab.ToArray().CopyTo(@out);
            return @out[(3 + outputFormat.ExtraSamples)..].Span;
        }
    }

    private Span<byte> PackLabDoubleFromFloat(ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, double>(null!, BitConverter.GetBytes);

        if (outputFormat.Planar)
        {
            stride /= outputFormat.PixelSize;

            @out[0] = wOut[0] * 100.0;
            @out[stride] = (wOut[1] * 255.0) - 128.0;
            @out[stride * 2] = (wOut[2] * 255.0) - 128.0;

            return @out[1..].Span;
        }
        else
        {
            @out[0] = wOut[0] * 100.0;
            @out[1] = (wOut[1] * 255.0) - 128.0;
            @out[2] = (wOut[2] * 255.0) - 128.0;

            return @out[(3 + outputFormat.ExtraSamples)..].Span;
        }
    }

    private Span<byte> PackLabFloatFrom16(ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, float>(null!, BitConverter.GetBytes);
        var lab = new LabEncoded(wOut).ToLab();

        if (outputFormat.Planar)
        {
            stride /= outputFormat.PixelSize;

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
            return @out[(3 + outputFormat.ExtraSamples)..].Span;
        }
    }

    private Span<byte> PackLabFloatFromFloat(ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, float>(null!, BitConverter.GetBytes);

        if (outputFormat.Planar)
        {
            stride /= outputFormat.PixelSize;

            @out[0] = (float)(wOut[0] * 100.0);
            @out[stride] = (float)((wOut[1] * 255.0) - 128.0);
            @out[stride * 2] = (float)((wOut[2] * 255.0) - 128.0);

            return @out[1..].Span;
        }
        else
        {
            @out[0] = (float)(wOut[0] * 100.0);
            @out[1] = (float)((wOut[1] * 255.0) - 128.0);
            @out[2] = (float)((wOut[2] * 255.0) - 128.0);

            return @out[(3 + outputFormat.ExtraSamples)..].Span;
        }
    }

    private Span<byte> PackLabV2_16(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.UpCaster<byte, ushort>(null!, s => BitConverter.GetBytes(FromLabV4ToLabV2(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[3..].Span;
    }

    private Span<byte> PackLabV2_8(ReadOnlySpan<ushort> wOut, Span<byte> output, int _)
    {
        var ptr = output.Converter<byte, ushort>(null!, s => From16to8(FromLabV4ToLabV2(s)));
        ptr[0] = wOut[0];
        ptr[1] = wOut[1];
        ptr[2] = wOut[2];

        return ptr[3..].Span;
    }

    private Span<byte> PackPlanarBytes(ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var nChan = outputFormat.Channels;
        var doSwap = outputFormat.SwapAll;
        var swapFirst = outputFormat.SwapFirst;
        var reverse = outputFormat.Flavor;
        var extra = outputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var premul = outputFormat.PremultipliedAlpha;
        var alphaFactor = 0;

        var init = output;
        var ptr = output.Converter(From8to16, From16to8);

        if (extraFirst)
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[0]);

            output = output[(extra * stride)..];
        }
        else
        {
            if (premul && extra != 0)
                alphaFactor = ToFixedDomain(ptr[nChan]);
        }

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;

            var v = wOut[index];

            if (reverse is ColorFlavor.Subtractive)
                v = ReverseFlavor(v);

            if (premul && alphaFactor != 0)
                v = (ushort)(((v * alphaFactor) + 0x8000) >> 16);

            ptr[0] = v;
            ptr += stride;
        }

        return init[1..];
    }

    private Span<byte> PackPlanarWords(ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var nChan = outputFormat.Channels;
        var doSwap = outputFormat.SwapAll;
        var swapFirst = outputFormat.SwapFirst;
        var reverse = outputFormat.Flavor;
        var extra = outputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var premul = outputFormat.PremultipliedAlpha;
        var swapEndian = outputFormat.EndianSwap;
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
            var index = doSwap ? (nChan - i - 1) : i;

            var v = wOut[index];

            if (swapEndian)
                v = ChangeEndian(v);

            if (reverse is ColorFlavor.Subtractive)
                v = ReverseFlavor(v);

            if (premul && alphaFactor != 0)
                v = (ushort)(((v * alphaFactor) + 0x8000) >> 16);

            ptr[0] = v;
            ptr += stride;
        }

        return init[1..].Span;
    }

    private Span<byte> PackXYZDoubleFrom16(ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, double>(null!, BitConverter.GetBytes);
        var xyz = XYZ.FromEncodedArray(wOut);

        if (outputFormat.Planar)
        {
            stride /= outputFormat.PixelSize;

            @out[0] = xyz.X;
            @out[stride] = xyz.Y;
            @out[stride * 2] = xyz.Z;

            return @out[1..].Span;
        }
        else
        {
            xyz.ToArray().CopyTo(@out);
            return @out[(3 + outputFormat.ExtraSamples)..].Span;
        }
    }

    private Span<byte> PackXYZDoubleFromFloat(ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, double>(null!, s => BitConverter.GetBytes(s * maxEncodableXYZ));

        if (outputFormat.Planar)
        {
            stride /= outputFormat.PixelSize;

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

            return @out[(3 + outputFormat.ExtraSamples)..].Span;
        }
    }

    private Span<byte> PackXYZFloatFrom16(ReadOnlySpan<ushort> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, float>(null!, s => BitConverter.GetBytes((float)(s * maxEncodableXYZ)));
        var xyz = XYZ.FromEncodedArray(wOut);

        if (outputFormat.Planar)
        {
            stride /= outputFormat.PixelSize;

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

            return @out[(3 + outputFormat.ExtraSamples)..].Span;
        }
    }

    private Span<byte> PackXYZFloatFromFloat(ReadOnlySpan<float> wOut, Span<byte> output, int stride)
    {
        var @out = output.UpCaster<byte, float>(null!, s => BitConverter.GetBytes((float)(s * maxEncodableXYZ)));

        if (outputFormat.Planar)
        {
            stride /= outputFormat.PixelSize;

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

            return @out[(3 + outputFormat.ExtraSamples)..].Span;
        }
    }

    private ushort ReverseFlavor(ReadOnlySpan<byte> span) =>
            ReverseFlavor(BitConverter.ToUInt16(span));

    private ReadOnlySpan<byte> Unroll16ToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = inputFormat.Planar;
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        stride /= inputFormat.PixelSize;

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            var v =
                (float)(planar
                    ? ptr[(i + start) * stride]
                    : ptr[i + start]);

            v /= 65535.0f;

            wIn[index] = reverse is ColorFlavor.Subtractive ? 1 - v : v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return inputFormat.Planar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private ReadOnlySpan<byte> Unroll1Byte(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = From8to16(acc[0]); acc = acc[1..]; // L

        return acc;
    }

    private ReadOnlySpan<byte> Unroll1ByteReversed(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = ReverseFlavor(From8to16(acc[0])); acc = acc[1..]; // L

        return acc;
    }

    private ReadOnlySpan<byte> Unroll1ByteSkip1(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = From8to16(acc[0]); acc = acc[1..]; // L
        acc = acc[1..];

        return acc;
    }

    private ReadOnlySpan<byte> Unroll1ByteSkip2(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = From8to16(acc[0]); acc = acc[1..]; // L
        acc = acc[2..];

        return acc;
    }

    private ReadOnlySpan<byte> Unroll1Word(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // L

        return acc;
    }

    private ReadOnlySpan<byte> Unroll1WordReversed(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = ReverseFlavor(BitConverter.ToUInt16(acc)); acc = acc[2..]; // L

        return acc;
    }

    private ReadOnlySpan<byte> Unroll1WordSkip3(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        wIn[0] = wIn[1] = wIn[2] = BitConverter.ToUInt16(acc); acc = acc[2..]; // L
        acc = acc[8..];

        return acc;
    }

    private ReadOnlySpan<byte> Unroll2Bytes(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[0] = ptr[0]; // ch1
        wIn[1] = ptr[1]; // ch2

        return ptr[2..].Span;
    }

    private ReadOnlySpan<byte> Unroll2Words(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[0] = ptr[0]; // ch1
        wIn[1] = ptr[1]; // ch2

        return ptr[2..].Span;
    }

    private ReadOnlySpan<byte> Unroll3Bytes(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[0] = ptr[0]; // R
        wIn[1] = ptr[1]; // G
        wIn[2] = ptr[2]; // B

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> Unroll3BytesSkip1Swap(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        ptr++; // A
        wIn[2] = ptr[0]; // B
        wIn[1] = ptr[1]; // G
        wIn[0] = ptr[2]; // R

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> Unroll3BytesSkip1SwapFirst(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        ptr++; // A
        wIn[0] = ptr[0]; // R
        wIn[1] = ptr[1]; // G
        wIn[2] = ptr[2]; // B

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> Unroll3BytesSkip1SwapSwapFirst(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[2] = ptr[0]; // B
        wIn[1] = ptr[1]; // G
        wIn[0] = ptr[2]; // R
        ptr++; // A

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> Unroll3BytesSwap(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[2] = ptr[0]; // B
        wIn[1] = ptr[1]; // G
        wIn[0] = ptr[2]; // R

        return acc;
    }

    private ReadOnlySpan<byte> Unroll3Words(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[0] = ptr[0]; // C R
        wIn[1] = ptr[1]; // M G
        wIn[2] = ptr[2]; // Y B

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> Unroll3WordsSkip1Swap(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        ptr++; // A
        wIn[2] = ptr[0]; // B
        wIn[1] = ptr[1]; // G
        wIn[0] = ptr[2]; // R

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> Unroll3WordsSkip1SwapFirst(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        ptr++; // A
        wIn[0] = ptr[0]; // R
        wIn[1] = ptr[1]; // G
        wIn[2] = ptr[2]; // B

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> Unroll3WordsSwap(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[2] = ptr[0]; // Y B
        wIn[1] = ptr[1]; // M G
        wIn[0] = ptr[2]; // C R

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> Unroll4Bytes(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[0] = ptr[0]; // C
        wIn[1] = ptr[1]; // M
        wIn[2] = ptr[2]; // Y
        wIn[3] = ptr[3]; // K

        return ptr[4..].Span;
    }

    private ReadOnlySpan<byte> Unroll4BytesReverse(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16Reversed);

        wIn[0] = ptr[0]; // C
        wIn[1] = ptr[1]; // M
        wIn[2] = ptr[2]; // Y
        wIn[3] = ptr[3]; // K

        return ptr[4..].Span;
    }

    private ReadOnlySpan<byte> Unroll4BytesSwap(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[3] = ptr[0]; // K
        wIn[2] = ptr[1]; // Y
        wIn[1] = ptr[2]; // M
        wIn[0] = ptr[3]; // C

        return ptr[4..].Span;
    }

    private ReadOnlySpan<byte> Unroll4BytesSwapFirst(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[3] = ptr[0]; // K
        wIn[0] = ptr[1]; // C
        wIn[1] = ptr[2]; // M
        wIn[2] = ptr[3]; // Y

        return ptr[4..].Span;
    }

    private ReadOnlySpan<byte> Unroll4BytesSwapSwapFirst(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[2] = ptr[0]; // K
        wIn[1] = ptr[1]; // Y
        wIn[0] = ptr[2]; // M
        wIn[3] = ptr[3]; // C

        return ptr[4..].Span;
    }

    private ReadOnlySpan<byte> Unroll4Words(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[0] = ptr[0]; // C
        wIn[1] = ptr[1]; // M
        wIn[2] = ptr[2]; // Y
        wIn[3] = ptr[3]; // K

        return ptr[4..].Span;
    }

    private ReadOnlySpan<byte> Unroll4WordsReverse(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, ReverseFlavor);

        wIn[0] = ptr[0]; // C
        wIn[1] = ptr[1]; // M
        wIn[2] = ptr[2]; // Y
        wIn[3] = ptr[3]; // K

        return ptr[4..].Span;
    }

    private ReadOnlySpan<byte> Unroll4WordsSwap(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[3] = ptr[0]; // K
        wIn[2] = ptr[1]; // Y
        wIn[1] = ptr[2]; // M
        wIn[0] = ptr[3]; // C

        return ptr[4..].Span;
    }

    private ReadOnlySpan<byte> Unroll4WordsSwapFirst(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[3] = ptr[0]; // K
        wIn[0] = ptr[1]; // C
        wIn[1] = ptr[2]; // M
        wIn[2] = ptr[3]; // Y

        return ptr[4..].Span;
    }

    private ReadOnlySpan<byte> Unroll4WordsSwapSwapFirst(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[2] = ptr[0]; // K
        wIn[1] = ptr[1]; // Y
        wIn[0] = ptr[2]; // M
        wIn[3] = ptr[3]; // C

        return ptr[4..].Span;
    }

    private ReadOnlySpan<byte> Unroll8ToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = inputFormat.Planar;

        stride /= inputFormat.PixelSize;

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            var v = (float)acc[(i + start) * (planar ? stride : 1)];

            v /= 255.0f;

            wIn[index] = reverse is ColorFlavor.Subtractive ? 1 - v : v;
        }

        if (extra is 0 && swapFirst)
            RollingShift(wIn[..nChan]);

        return inputFormat.Planar
            ? acc[sizeof(byte)..]
            : acc[((nChan + extra) * sizeof(byte))..];
    }

    private ReadOnlySpan<byte> UnrollALabV2_8(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        ptr++; // A
        wIn[0] = FromLabV2ToLabV4(ptr[0]); // L
        wIn[1] = FromLabV2ToLabV4(ptr[1]); // a
        wIn[2] = FromLabV2ToLabV4(ptr[2]); // b

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> UnrollALabV2_8ToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int _)
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

    private ReadOnlySpan<byte> UnrollAnyWords(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var nChan = inputFormat.Channels;
        var swapEndian = inputFormat.EndianSwap;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc,
            swapEndian
                ? ChangeEndian
                : BitConverter.ToUInt16);

        if (extraFirst)
            ptr += extra;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
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

    private ReadOnlySpan<byte> UnrollAnyWordsPremul(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var nChan = inputFormat.Channels;
        var swapEndian = inputFormat.EndianSwap;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extraFirst = doSwap ^ swapFirst;

        var alpha = extraFirst ? acc[0] : acc[nChan - 1];
        var alpha_factor = (uint)ToFixedDomain(From8to16(alpha));
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        if (extraFirst)
            ptr++;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
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

    private ReadOnlySpan<byte> UnrollChunkyBytes(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
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
            RollingShift(wIn[..nChan]);

        return acc;
    }

    private ReadOnlySpan<byte> UnrollDouble1Chan(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);

        var inks = ptr[0];

        wIn[0] = wIn[1] = wIn[2] = QuickSaturateWord(inks * 65535.0);

        return ptr[1..].Span;
    }

    private ReadOnlySpan<byte> UnrollDoublesToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = inputFormat.Planar;
        var premul = inputFormat.PremultipliedAlpha;
        var maximum = inputFormat.IsInkSpace ? 100d : 1d;
        var alphaFactor = 1d;
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);

        stride /= inputFormat.PixelSize;

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
            var index = doSwap ? (nChan - i - 1) : i;
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

        return inputFormat.Planar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private ReadOnlySpan<byte> UnrollDoubleTo16(Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = inputFormat.Planar;
        var maximum = inputFormat.IsInkSpace ? 655.35 : 65535.0;
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);

        stride /= inputFormat.PixelSize;

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
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

        return inputFormat.Planar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private ReadOnlySpan<byte> UnrollFloatsToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = inputFormat.Planar;
        var premul = inputFormat.PremultipliedAlpha;
        var maximum = inputFormat.IsInkSpace ? 100f : 1f;
        var alphaFactor = 1f;
        var ptr = new UpCastingReadOnlySpan<byte, float>(acc, BitConverter.ToSingle);

        stride /= inputFormat.PixelSize;

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
            var index = doSwap ? (nChan - i - 1) : i;
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

        return inputFormat.Planar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private ReadOnlySpan<byte> UnrollFloatTo16(Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapFirst = inputFormat.SwapFirst;
        var extra = inputFormat.ExtraSamples;
        var extraFirst = doSwap ^ swapFirst;
        var planar = inputFormat.Planar;
        var maximum = inputFormat.IsInkSpace ? 655.35 : 65535.0;
        var ptr = new UpCastingReadOnlySpan<byte, float>(acc, BitConverter.ToSingle);

        stride /= inputFormat.PixelSize;

        var start = extraFirst
            ? extra
            : (byte)0;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            var v =
                planar
                    ? ptr[(i + start) * stride]
                    : ptr[i + start];

            var vi = QuickSaturateWord(v * maximum);

            vi = reverse is ColorFlavor.Subtractive ? ReverseFlavor(vi) : vi;

            wIn[index] = vi;
        }

        if (extra is 0 && swapFirst)
        {
            var tmp = wIn[0];

            wIn.CopyTo(wIn[1..]);
            wIn[nChan - 1] = tmp;
        }

            RollingShift(wIn[..nChan]);

        return inputFormat.Planar
            ? ptr[1..].Span
            : ptr[(nChan + extra)..].Span;
    }

    private ReadOnlySpan<byte> UnrollLabDoubleTo16(Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        Lab lab;
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);
        stride /= sizeof(double);

        if (inputFormat.Planar)
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

            return ptr[(3 + inputFormat.ExtraSamples)..].Span;
        }
    }

    private ReadOnlySpan<byte> UnrollLabDoubleToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);

        if (inputFormat.Planar)
        {
            stride /= inputFormat.PixelSize;

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

            return ptr[(3 + inputFormat.ExtraSamples)..].Span;
        }
    }

    private ReadOnlySpan<byte> UnrollLabFloatTo16(Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        Lab lab;
        var ptr = new UpCastingReadOnlySpan<byte, float>(acc, BitConverter.ToSingle);

        if (inputFormat.Planar)
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

            return ptr[(3 + inputFormat.ExtraSamples)..].Span;
        }
    }

    private ReadOnlySpan<byte> UnrollLabFloatToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var ptr = new UpCastingReadOnlySpan<byte, float>(acc, BitConverter.ToSingle);

        if (inputFormat.Planar)
        {
            stride /= inputFormat.PixelSize;

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

            return ptr[(3 + inputFormat.ExtraSamples)..].Span;
        }
    }

    private ReadOnlySpan<byte> UnrollLabV2_16(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);

        wIn[0] = FromLabV2ToLabV4(ptr[0]); // L
        wIn[1] = FromLabV2ToLabV4(ptr[1]); // a
        wIn[2] = FromLabV2ToLabV4(ptr[2]); // b

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> UnrollLabV2_16ToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int _)
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

    private ReadOnlySpan<byte> UnrollLabV2_8(Span<ushort> wIn, ReadOnlySpan<byte> acc, int _)
    {
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        wIn[0] = FromLabV2ToLabV4(ptr[0]); // L
        wIn[1] = FromLabV2ToLabV4(ptr[1]); // a
        wIn[2] = FromLabV2ToLabV4(ptr[2]); // b

        return ptr[3..].Span;
    }

    private ReadOnlySpan<byte> UnrollLabV2_8ToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int _)
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

    private ReadOnlySpan<byte> UnrollPlanarBytes(Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var swapFirst = inputFormat.SwapFirst;
        var reverse = inputFormat.Flavor;
        var extraFirst = doSwap ^ swapFirst;
        var extra = inputFormat.ExtraSamples;
        var premul = inputFormat.PremultipliedAlpha;
        var init = acc[1..];
        var alphaFactor = 1;
        var ptr = new ConvertingReadOnlySpan<byte, ushort>(acc, From8to16);

        stride /= sizeof(ushort);

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
            var index = doSwap ? (nChan - i - 1) : i;
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

    private ReadOnlySpan<byte> UnrollPlanarWords(Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var reverse = inputFormat.Flavor;
        var swapEndian = inputFormat.EndianSwap;
        var ptr = new UpCastingReadOnlySpan<byte, ushort>(acc, BitConverter.ToUInt16);
        var init = ptr[1..].Span;

        stride /= sizeof(ushort);

        if (doSwap)
            ptr += inputFormat.ExtraSamples * stride;

        for (var i = 0; i < nChan; i++)
        {
            var index = doSwap ? (nChan - i - 1) : i;
            var v = ptr[0];

            if (swapEndian)
                v = ChangeEndian(v);

            wIn[index] = reverse is ColorFlavor.Subtractive ? ReverseFlavor(v) : v;

            ptr += stride;
        }

        return init;
    }

    private ReadOnlySpan<byte> UnrollPlanarWordsPremul(Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var nChan = inputFormat.Channels;
        var doSwap = inputFormat.SwapAll;
        var swapFirst = inputFormat.SwapFirst;
        var reverse = inputFormat.Flavor;
        var swapEndian = inputFormat.EndianSwap;
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
            var index = doSwap ? (nChan - i - 1) : i;
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

    private ReadOnlySpan<byte> UnrollXYZDoubleTo16(Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        XYZ xyz;
        var ptr = new UpCastingReadOnlySpan<byte, double>(acc, BitConverter.ToDouble);
        stride /= sizeof(double);

        if (inputFormat.Planar)
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

            return ptr[(3 + inputFormat.ExtraSamples)..].Span;
        }
    }

    private ReadOnlySpan<byte> UnrollXYZDoubleToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var ptr = new UpCastingReadOnlySpan<byte, double>(
            acc,
            s =>
                BitConverter.ToDouble(s) / maxEncodableXYZ);

        if (inputFormat.Planar)
        {
            stride /= inputFormat.PixelSize;

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

            return ptr[(3 + inputFormat.ExtraSamples)..].Span;
        }
    }

    private ReadOnlySpan<byte> UnrollXYZFloatTo16(Span<ushort> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        XYZ xyz;
        var ptr = new UpCastingReadOnlySpan<byte, float>(acc, BitConverter.ToSingle);
        stride /= sizeof(float);

        if (inputFormat.Planar)
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

            return ptr[(3 + inputFormat.ExtraSamples)..].Span;
        }
    }

    private ReadOnlySpan<byte> UnrollXYZFloatToFloat(Span<float> wIn, ReadOnlySpan<byte> acc, int stride)
    {
        var ptr = new UpCastingReadOnlySpan<byte, float>(
            acc,
            s =>
                (float)(BitConverter.ToSingle(s) / maxEncodableXYZ));

        if (inputFormat.Planar)
        {
            stride /= inputFormat.PixelSize;

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

            return ptr[(3 + inputFormat.ExtraSamples)..].Span;
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
