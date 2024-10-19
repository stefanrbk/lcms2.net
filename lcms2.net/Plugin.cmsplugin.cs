//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright ©️ 1998-2024 Marti Maria Saguer
//              2022-2024 Stefan Kewatt
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

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using lcms2.io;
using lcms2.types;

using S15Fixed16Number = System.Int32;

namespace lcms2;
public static partial class Plugin
{
    [DebuggerStepThrough]
    public static ushort AdjustEndianess(ushort Word)   // _cmsAdjustEndianess16
    {
        Span<byte> pByte = stackalloc byte[2];
        BitConverter.TryWriteBytes(pByte, Word);

        (pByte[1], pByte[0]) = (pByte[0], pByte[1]);
        return BitConverter.ToUInt16(pByte);
    }

    [DebuggerStepThrough]
    public static uint AdjustEndianess(uint DWord)  // _cmsAdjustEndianess32
    {
        Span<byte> pByte = stackalloc byte[4];
        BitConverter.TryWriteBytes(pByte, DWord);

        (pByte[3], pByte[2], pByte[1], pByte[0]) = (pByte[0], pByte[1], pByte[2], pByte[3]);
        return BitConverter.ToUInt32(pByte);
    }

    [DebuggerStepThrough]
    public static ulong AdjustEndianess(ulong QWord)    // _cmsAdjustEndianess64
    {
        Span<byte> pByte = stackalloc byte[8];
        BitConverter.TryWriteBytes(pByte, QWord);

        (pByte[7], pByte[0]) = (pByte[0], pByte[7]);
        (pByte[6], pByte[1]) = (pByte[1], pByte[6]);
        (pByte[5], pByte[2]) = (pByte[2], pByte[5]);
        (pByte[4], pByte[3]) = (pByte[3], pByte[4]);

        return BitConverter.ToUInt64(pByte);
    }

    [DebuggerStepThrough]
    public static bool _cmsReadSignature(IOHandler io, out Signature sig)
    {
        sig = 0;

        if (!io.ReadUint(out var value))
            return false;

        sig = value;
        return true;
    }

    [DebuggerStepThrough]
    public static Signature _cmsReadTypeBase(IOHandler io)
    {
        Span<byte> Base = stackalloc byte[(sizeof(uint) * 2)];

        _cmsAssert(io);

        if (io.ReadFunc(io, Base, (sizeof(uint) * 2), 1) != 1)
            return default;

        return new(AdjustEndianess(BitConverter.ToUInt32(Base)));
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteTypeBase(IOHandler io, Signature sig)
    {
        Span<byte> Base = stackalloc byte[(sizeof(uint) * 2)];

        _cmsAssert(io);

        BitConverter.TryWriteBytes(Base, AdjustEndianess(sig));
        return io.WriteFunc(io, (sizeof(uint) * 2), Base);
    }

    [DebuggerStepThrough]
    public static bool _cmsReadAlignment(IOHandler io)
    {
        Span<byte> Buffer = stackalloc byte[4];

        _cmsAssert(io);

        var At = io.TellFunc(io);
        var NextAligned = _cmsALIGNLONG(At);
        var BytesToNextAlignedPos = NextAligned - At;
        if (BytesToNextAlignedPos is 0) return true;
        if (BytesToNextAlignedPos > 4) return false;

        return io.ReadFunc(io, Buffer, BytesToNextAlignedPos, 1) == 1;
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteAlignment(IOHandler io)
    {
        Span<byte> Buffer = stackalloc byte[4];

        _cmsAssert(io);

        var At = io.TellFunc(io);
        var NextAligned = _cmsALIGNLONG(At);
        var BytesToNextAlignedPos = NextAligned - At;
        if (BytesToNextAlignedPos is 0) return true;
        if (BytesToNextAlignedPos > 4) return false;

        return io.WriteFunc(io, BytesToNextAlignedPos, Buffer);
    }

    [DebuggerStepThrough]
    public static bool _cmsIOPrintf(IOHandler io, ReadOnlySpan<byte> frm, params object[] args) =>
        _cmsIOPrintf(io, SpanToString(frm), args);

    [DebuggerStepThrough]
    public static bool _cmsIOPrintf(IOHandler io, string frm, params object[] args)
    {
        _cmsAssert(io);
        _cmsAssert(frm);

        var str = new StringBuilder(String.Format(frm, args));
        str.Replace(',', '.');
        if (str.Length > 2047) return false;
        var buffer = Encoding.UTF8.GetBytes(str.ToString());

        return io.WriteFunc(io, (uint)str.Length, buffer);
    }

    [DebuggerStepThrough]
    public static double _cms8Fixed8toDouble(ushort fixed8) =>
        fixed8 / 256.0;

    [DebuggerStepThrough]
    public static ushort _cmsDoubleTo8Fixed8(double val)
    {
        var tmp = _cmsDoubleTo15Fixed16(val);
        return (ushort)((tmp >> 8) & 0xffff);
    }

    [DebuggerStepThrough]
    public static double _cms15Fixed16toDouble(S15Fixed16Number fix32) =>
        fix32 / 65536.0;

    [DebuggerStepThrough]
    public static S15Fixed16Number _cmsDoubleTo15Fixed16(double v) =>
        (S15Fixed16Number)Math.Floor((v * 65536.0) + 0.5);

    [DebuggerStepThrough]
    public static void _cmsEncodeDateTimeNumber(out DateTimeNumber dest, DateTime source)
    {
        dest = new()
        {
            Seconds = AdjustEndianess((ushort)source.Second),
            Minutes = AdjustEndianess((ushort)source.Minute),
            Hours = AdjustEndianess((ushort)source.Hour),
            Day = AdjustEndianess((ushort)source.Day),
            Month = AdjustEndianess((ushort)source.Month),
            Year = AdjustEndianess((ushort)source.Year)
        };
    }

    [DebuggerStepThrough]
    public static void _cmsDecodeDateTimeNumber(DateTimeNumber Source, out DateTime Dest)
    {
        var sec = AdjustEndianess(Source.Seconds);
        var min = AdjustEndianess(Source.Minutes);
        var hour = AdjustEndianess(Source.Hours);
        var day = AdjustEndianess(Source.Day);
        var mon = AdjustEndianess(Source.Month);
        var year = AdjustEndianess(Source.Year);

        try
        {
            Dest = new(year, mon, day, hour, min, sec);
        }
        catch (ArgumentOutOfRangeException)
        {
            Dest = default;
        }
    }
}
