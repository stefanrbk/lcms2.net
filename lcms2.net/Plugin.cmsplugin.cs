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
    public static ushort _cmsAdjustEndianess16(ushort Word)
    {
        Span<byte> pByte = stackalloc byte[2];
        BitConverter.TryWriteBytes(pByte, Word);

        (pByte[1], pByte[0]) = (pByte[0], pByte[1]);
        return BitConverter.ToUInt16(pByte);
    }

    [DebuggerStepThrough]
    public static uint _cmsAdjustEndianess32(uint DWord)
    {
        Span<byte> pByte = stackalloc byte[4];
        BitConverter.TryWriteBytes(pByte, DWord);

        (pByte[3], pByte[2], pByte[1], pByte[0]) = (pByte[0], pByte[1], pByte[2], pByte[3]);
        return BitConverter.ToUInt32(pByte);
    }

    [DebuggerStepThrough]
    public static ulong _cmsAdjustEndianess64(ulong QWord)
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
    public static bool _cmsReadUInt8Number(IOHandler io, out byte n)
    {
        n = 0;
        Span<byte> tmp = stackalloc byte[1];

        _cmsAssert(io);

        if (io.Read(io, tmp, sizeof(byte), 1) != 1)
            return false;

        n = tmp[0];
        return true;
    }

    [DebuggerStepThrough]
    public static bool _cmsReadUInt16Number(IOHandler io, out ushort n)
    {
        n = 0;
        Span<byte> tmp = stackalloc byte[2];

        _cmsAssert(io);

        if (io.Read(io, tmp, sizeof(ushort), 1) != 1)
            return false;

        n = _cmsAdjustEndianess16(BitConverter.ToUInt16(tmp));
        return true;
    }

    [DebuggerStepThrough]
    public static bool _cmsReadUInt16Array(IOHandler io, uint n, Span<ushort> array)
    {
        _cmsAssert(io);

        for (var i = 0; i < n; i++)
        {
            if (!_cmsReadUInt16Number(io, out array[i]))
                return false;
        }
        return true;
    }

    [DebuggerStepThrough]
    public static bool _cmsReadUInt32Number(IOHandler io, out uint n)
    {
        n = 0;
        Span<byte> tmp = stackalloc byte[4];

        _cmsAssert(io);

        if (io.Read(io, tmp, sizeof(uint), 1) != 1)
            return false;

        n = _cmsAdjustEndianess32(BitConverter.ToUInt32(tmp));
        return true;
    }

    [DebuggerStepThrough]
    public static bool _cmsReadFloat32Number(IOHandler io, out float n)
    {
        n = 0;
        Span<byte> tmp = stackalloc byte[4];

        _cmsAssert(io);

        if (io.Read(io, tmp, sizeof(uint), 1) != 1)
            return false;

        n = BitConverter.UInt32BitsToSingle(_cmsAdjustEndianess32(BitConverter.ToUInt32(tmp)));

        // Safeguard which covers against absurd values
        if (n is > 1E+20f or < -1E+20f)
            return false;

        // I guess we don't deal with subnormal values!
        return Single.IsNormal(n) || n is 0;
    }

    [DebuggerStepThrough]
    public static bool _cmsReadSignature(IOHandler io, out Signature sig)
    {
        sig = 0;

        if (!_cmsReadUInt32Number(io, out var value))
            return false;

        sig = value;
        return true;
    }

    [DebuggerStepThrough]
    public static bool _cmsReadUInt64Number(IOHandler io, out ulong n)
    {
        n = 0;
        Span<byte> tmp = stackalloc byte[8];

        _cmsAssert(io);

        if (io.Read(io, tmp, sizeof(ulong), 1) != 1)
            return false;

        n = _cmsAdjustEndianess64(BitConverter.ToUInt64(tmp));
        return true;
    }

    [DebuggerStepThrough]
    public static bool _cmsRead15Fixed16Number(IOHandler io, out double n)
    {
        n = 0;
        Span<byte> tmp = stackalloc byte[4];

        _cmsAssert(io);

        if (io.Read(io, tmp, sizeof(uint), 1) != 1)
            return false;

        n = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32(BitConverter.ToUInt32(tmp)));

        return true;
    }

    [DebuggerStepThrough]
    public static bool _cmsReadXYZNumber(IOHandler io, out CIEXYZ XYZ)
    {
        XYZ = new CIEXYZ();
        Span<byte> xyz = stackalloc byte[(sizeof(uint) * 3)];

        _cmsAssert(io);

        if (io.Read(io, xyz, (sizeof(uint) * 3), 1) != 1)
            return false;

        var ints = MemoryMarshal.Cast<byte, uint>(xyz);

        XYZ.X = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32(ints[0]));
        XYZ.Y = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32(ints[1]));
        XYZ.Z = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32(ints[2]));

        return true;
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteUInt8Number(IOHandler io, byte n)
    {
        _cmsAssert(io);

        Span<byte> tmp = stackalloc byte[1] { n };

        return io.Write(io, sizeof(byte), tmp);
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteUInt16Number(IOHandler io, ushort n)
    {
        _cmsAssert(io);

        Span<byte> tmp = stackalloc byte[2];
        BitConverter.TryWriteBytes(tmp, _cmsAdjustEndianess16(n));

        return io.Write(io, sizeof(ushort), tmp);
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteUInt16Array(IOHandler io, uint n, ReadOnlySpan<ushort> array)
    {
        _cmsAssert(io);
        _cmsAssert(array);

        for (var i = 0; i < n; i++)
        {
            if (!_cmsWriteUInt16Number(io, array[i])) return false;
        }

        return true;
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteUInt32Number(IOHandler io, uint n)
    {
        _cmsAssert(io);

        Span<byte> tmp = stackalloc byte[4];
        BitConverter.TryWriteBytes(tmp, _cmsAdjustEndianess32(n));

        return io.Write(io, sizeof(uint), tmp);
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteFloat32Number(IOHandler io, float n)
    {
        _cmsAssert(io);

        Span<byte> tmp = stackalloc byte[4];
        BitConverter.TryWriteBytes(tmp, _cmsAdjustEndianess32(BitConverter.SingleToUInt32Bits(n)));

        return io.Write(io, sizeof(uint), tmp);
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteUInt64Number(IOHandler io, ulong n)
    {
        _cmsAssert(io);

        Span<byte> tmp = stackalloc byte[8];
        BitConverter.TryWriteBytes(tmp, _cmsAdjustEndianess64(n));

        return io.Write(io, sizeof(ulong), tmp);
    }

    [DebuggerStepThrough]
    public static bool _cmsWrite15Fixed16Number(IOHandler io, double n)
    {
        _cmsAssert(io);

        Span<byte> tmp = stackalloc byte[4];
        BitConverter.TryWriteBytes(tmp, _cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(n)));

        return io.Write(io, sizeof(uint), tmp);
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteXYZNumber(IOHandler io, CIEXYZ XYZ)
    {
        Span<int> xyz = stackalloc int[3];

        _cmsAssert(io);

        xyz[0] = (S15Fixed16Number)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(XYZ.X));
        xyz[1] = (S15Fixed16Number)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(XYZ.Y));
        xyz[2] = (S15Fixed16Number)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(XYZ.Z));

        return io.Write(io, sizeof(uint) * 3, MemoryMarshal.Cast<int, byte>(xyz));
    }

    [DebuggerStepThrough]
    public static Signature _cmsReadTypeBase(IOHandler io)
    {
        Span<byte> Base = stackalloc byte[(sizeof(uint) * 2)];

        _cmsAssert(io);

        if (io.Read(io, Base, (sizeof(uint) * 2), 1) != 1)
            return default;

        return new(_cmsAdjustEndianess32(BitConverter.ToUInt32(Base)));
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteTypeBase(IOHandler io, Signature sig)
    {
        Span<byte> Base = stackalloc byte[(sizeof(uint) * 2)];

        _cmsAssert(io);

        BitConverter.TryWriteBytes(Base, _cmsAdjustEndianess32(sig));
        return io.Write(io, (sizeof(uint) * 2), Base);
    }

    [DebuggerStepThrough]
    public static bool _cmsReadAlignment(IOHandler io)
    {
        Span<byte> Buffer = stackalloc byte[4];

        _cmsAssert(io);

        var At = io.Tell(io);
        var NextAligned = _cmsALIGNLONG(At);
        var BytesToNextAlignedPos = NextAligned - At;
        if (BytesToNextAlignedPos is 0) return true;
        if (BytesToNextAlignedPos > 4) return false;

        return io.Read(io, Buffer, BytesToNextAlignedPos, 1) == 1;
    }

    [DebuggerStepThrough]
    public static bool _cmsWriteAlignment(IOHandler io)
    {
        Span<byte> Buffer = stackalloc byte[4];

        _cmsAssert(io);

        var At = io.Tell(io);
        var NextAligned = _cmsALIGNLONG(At);
        var BytesToNextAlignedPos = NextAligned - At;
        if (BytesToNextAlignedPos is 0) return true;
        if (BytesToNextAlignedPos > 4) return false;

        return io.Write(io, BytesToNextAlignedPos, Buffer);
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

        return io.Write(io, (uint)str.Length, buffer);
    }

    [DebuggerStepThrough]
    public static double _cms8Fixed8toDouble(ushort fixed8)
    {
        var lsb = (byte)(fixed8 & 0xff);
        var msb = (byte)((fixed8 >> 8) & 0xff);

        return msb + (lsb / 255.0);
    }

    [DebuggerStepThrough]
    public static ushort _cmsDoubleTo8Fixed8(double val)
    {
        var tmp = _cmsDoubleTo15Fixed16(val);
        return (ushort)((tmp >> 8) & 0xffff);
    }

    [DebuggerStepThrough]
    public static double _cms15Fixed16toDouble(S15Fixed16Number fix32)
    {
        var sign = fix32 < 0 ? -1 : 1;
        fix32 = Math.Abs(fix32);

        var whole = (ushort)((fix32 >> 16) & 0xffff);
        var fracPart = (ushort)(fix32 & 0xffff);

        var mid = fracPart / 65536.0;
        var floater = whole + mid;

        return sign * floater;
    }

    [DebuggerStepThrough]
    public static S15Fixed16Number _cmsDoubleTo15Fixed16(double v) =>
        (S15Fixed16Number)Math.Floor((v * 65536.0) + 0.5);

    [DebuggerStepThrough]
    public static void _cmsEncodeDateTimeNumber(out DateTimeNumber dest, DateTime source)
    {
        dest = new()
        {
            Seconds = _cmsAdjustEndianess16((ushort)source.Second),
            Minutes = _cmsAdjustEndianess16((ushort)source.Minute),
            Hours = _cmsAdjustEndianess16((ushort)source.Hour),
            Day = _cmsAdjustEndianess16((ushort)source.Day),
            Month = _cmsAdjustEndianess16((ushort)source.Month),
            Year = _cmsAdjustEndianess16((ushort)source.Year)
        };
    }

    [DebuggerStepThrough]
    public static void _cmsDecodeDateTimeNumber(DateTimeNumber Source, out DateTime Dest)
    {
        var sec = _cmsAdjustEndianess16(Source.Seconds);
        var min = _cmsAdjustEndianess16(Source.Minutes);
        var hour = _cmsAdjustEndianess16(Source.Hours);
        var day = _cmsAdjustEndianess16(Source.Day);
        var mon = _cmsAdjustEndianess16(Source.Month);
        var year = _cmsAdjustEndianess16(Source.Year);

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
