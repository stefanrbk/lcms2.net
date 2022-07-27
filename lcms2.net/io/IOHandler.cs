using lcms2.state;
using lcms2.types;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lcms2.io;
public static class IOHandler
{
#if PLUGIN
    public
#else
    internal
#endif
        static ushort AdjustEndianness(ushort word) =>
    BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;
#if PLUGIN
    public
#else
    internal
#endif
        static uint AdjustEndianness(uint dWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(dWord) : dWord;
#if PLUGIN
    public
#else
    internal
#endif
        static ulong AdjustEndianness(ulong qWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(qWord) : qWord;
#if PLUGIN
    public
#else
    internal
#endif
        static short AdjustEndianness(short word) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;
#if PLUGIN
    public
#else
    internal
#endif
        static int AdjustEndianness(int dWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(dWord) : dWord;
#if PLUGIN
    public
#else
    internal
#endif
        static long AdjustEndianness(long qWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(qWord) : qWord;

#if PLUGIN
    public
#else
    internal
#endif
        static byte? ReadUInt8Number(this Stream io)
    {
        var value = io.ReadByte();
        return value is > byte.MaxValue or < byte.MinValue
            ? null
            : (byte)value;
    }

#if PLUGIN
    public
#else
    internal
#endif
        static ushort? ReadUInt16Number(this Stream io)
    {
        var tmp = new byte[sizeof(ushort)];
        return io.Read(tmp.AsSpan()) == sizeof(ushort)
            ? BinaryPrimitives.ReadUInt16BigEndian(tmp)
            : null;
    }

#if PLUGIN
    public
#else
    internal
#endif
        static ushort[]? ReadUInt16Array(this Stream io, int count)
    {
        var tmp = new ushort[count];
        for (var i = 0; i < count; i++)
        {
            var value = ReadUInt16Number(io);
            if (value is null)
                return null;
            tmp[i] = (ushort)value;
        }
        return tmp;
    }

#if PLUGIN
    public
#else
    internal
#endif
        static uint? ReadUInt32Number(this Stream io)
    {
        var tmp = new byte[sizeof(uint)];
        return io.Read(tmp.AsSpan()) == sizeof(uint)
            ? BinaryPrimitives.ReadUInt32BigEndian(tmp)
            : null;
    }

#if PLUGIN
    public
#else
    internal
#endif
        static int? ReadInt32Number(this Stream io)
    {
        var tmp = new byte[sizeof(int)];
        return io.Read(tmp.AsSpan()) == sizeof(int)
            ? BinaryPrimitives.ReadInt32BigEndian(tmp)
            : null;
    }

#if PLUGIN
    public
#else
    internal
#endif
        static float? ReadFloat32Number(this Stream io)
    {
        var tmp = new byte[sizeof(float)];
        return io.Read(tmp.AsSpan()) == sizeof(float)
            ? BinaryPrimitives.ReadSingleBigEndian(tmp)
            : null;
    }

#if PLUGIN
    public
#else
    internal
#endif
        static ulong? ReadUInt64Number(this Stream io)
    {
        var tmp = new byte[sizeof(ulong)];
        return io.Read(tmp.AsSpan()) == sizeof(ulong)
            ? BinaryPrimitives.ReadUInt64BigEndian(tmp)
            : null;
    }

#if PLUGIN
    public
#else
    internal
#endif
        static double? Read15Fixed16Number(this Stream io)
    {
        var tmp = ReadInt32Number(io);
        return tmp is not null
            ? S15Fixed16toDouble((int)tmp)
            : null;
    }

#if PLUGIN
    public
#else
    internal
#endif
        static XYZ? ReadXYZNumber(this Stream io)
    {
        var x = Read15Fixed16Number(io);
        var y = Read15Fixed16Number(io);
        var z = Read15Fixed16Number(io);
        return (x != null && y != null && z != null)
            ? ((double)x, (double)y, (double)z)
            : null;
    }

#if PLUGIN
    public
#else
    internal
#endif
        static bool Write(this Stream io, byte n)
    {
        try
        {
            io.WriteByte(n);
            return true;
        } catch
        {
            return false;
        }
    }

#if PLUGIN
    public
#else
    internal
#endif
        static bool Write(this Stream io, ushort n)
    {
        try
        {
            var tmp = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        } catch
        {
            return false;
        }
    }

#if PLUGIN
    public
#else
    internal
#endif
        static bool Write(this Stream io, int n, ushort[] array)
    {
        for (var i = 0; i < n; i++)
        {
            if (!io.Write(array[i]))
                return false;
        }
        return true;
    }

#if PLUGIN
    public
#else
    internal
#endif
        static bool Write(this Stream io, uint n)
    {
        try
        {
            var tmp = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        } catch
        {
            return false;
        }
    }

#if PLUGIN
    public
#else
    internal
#endif
        static bool Write(this Stream io, int n)
    {
        try
        {
            var tmp = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        } catch
        {
            return false;
        }
    }

#if PLUGIN
    public
#else
    internal
#endif
        static bool Write(this Stream io, float n)
    {
        try
        {
            var tmp = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        } catch
        {
            return false;
        }
    }

#if PLUGIN
    public
#else
    internal
#endif
        static bool Write(this Stream io, ulong n)
    {
        try
        {
            var tmp = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        } catch
        {
            return false;
        }
    }

#if PLUGIN
    public
#else
    internal
#endif
        static bool Write(this Stream io, double n) =>
        io.Write(DoubleToS15Fixed16(n));

#if PLUGIN
    public
#else
    internal
#endif
        static bool Write(this Stream io, XYZ xyz) =>
        io.Write(xyz.X) && io.Write(xyz.Y) && io.Write(xyz.Z);

    private static double S15Fixed16toDouble(int value)
    {
        var sign = value < 0 ? -1 : 1;
        value = Math.Abs(value);

        var whole = (ushort)((value >> 16) & 0xffff);
        var fracPart = (ushort)(value & 0xffff);

        var mid = fracPart / 65536.0;
        var floater = whole + mid;

        return sign * floater;
    }
    private static int DoubleToS15Fixed16(double value) =>
        (int)Math.Floor((value * 65536.0) + 0.5);
}
