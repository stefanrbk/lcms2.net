using System.Buffers.Binary;
using System.Text;

using lcms2.types;

namespace lcms2.io;
public static class IOHandler
{
#if PLUGIN
    public static long Tell(
#else
    internal static long Tell(
#endif
            this Stream io) =>
            io.Seek(0, SeekOrigin.Current);

#if PLUGIN
    public static ushort AdjustEndianness(
#else
    internal static ushort AdjustEndianness(
#endif
        ushort word) =>
    BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;

#if PLUGIN
    public static uint AdjustEndianness(
#else
    internal static uint AdjustEndianness(
# endif
        uint dWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(dWord) : dWord;

#if PLUGIN
    public static ulong AdjustEndianness(
#else
    internal static ulong AdjustEndianness(
#endif
        ulong qWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(qWord) : qWord;

#if PLUGIN
    public static short AdjustEndianness(
#else
    internal static short AdjustEndianness(
#endif
        short word) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;

#if PLUGIN
    public static int AdjustEndianness(
#else
    internal static int AdjustEndianness(
#endif
        int dWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(dWord) : dWord;

#if PLUGIN
    public static long AdjustEndianness(
#else
    internal static long AdjustEndianness(
#endif
        long qWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(qWord) : qWord;

#if PLUGIN
    public static byte? ReadUInt8Number(
#else
    internal static byte? ReadUInt8Number(
#endif
        this Stream io)
    {
        var value = io.ReadByte();
        return value is > byte.MaxValue or < byte.MinValue
            ? null
            : (byte)value;
    }

#if PLUGIN
    public static ushort? ReadUInt16Number(
#else
    internal static ushort? ReadUInt16Number(
#endif
        this Stream io)
    {
        var tmp = new byte[sizeof(ushort)];
        return io.Read(tmp.AsSpan()) == sizeof(ushort)
            ? BinaryPrimitives.ReadUInt16BigEndian(tmp)
            : null;
    }

#if PLUGIN
    public static ushort[]? ReadUInt16Array(
#else
    internal static ushort[]? ReadUInt16Array(
#endif
    this Stream io, int count)
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
    public static uint? ReadUInt32Number(
#else
    internal static uint? ReadUInt32Number(
#endif
        this Stream io)
    {
        var tmp = new byte[sizeof(uint)];
        return io.Read(tmp.AsSpan()) == sizeof(uint)
            ? BinaryPrimitives.ReadUInt32BigEndian(tmp)
            : null;
    }

#if PLUGIN
    public static int? ReadInt32Number(
#else
    internal static int? ReadInt32Number(
#endif
    this Stream io)
    {
        var tmp = new byte[sizeof(int)];
        return io.Read(tmp.AsSpan()) == sizeof(int)
            ? BinaryPrimitives.ReadInt32BigEndian(tmp)
            : null;
    }

#if PLUGIN
    public static float? ReadFloat32Number(
#else
    internal static float? ReadFloat32Number(
#endif
    this Stream io)
    {
        var tmp = new byte[sizeof(float)];
        return io.Read(tmp.AsSpan()) == sizeof(float)
            ? BinaryPrimitives.ReadSingleBigEndian(tmp)
            : null;
    }

#if PLUGIN
    public static ulong? ReadUInt64Number(
#else
    internal static ulong? ReadUInt64Number(
#endif
    this Stream io)
    {
        var tmp = new byte[sizeof(ulong)];
        return io.Read(tmp.AsSpan()) == sizeof(ulong)
            ? BinaryPrimitives.ReadUInt64BigEndian(tmp)
            : null;
    }

#if PLUGIN
    public static double? Read15Fixed16Number(
#else
    internal static double? Read15Fixed16Number(
#endif
    this Stream io)
    {
        var tmp = ReadInt32Number(io);
        return tmp is not null
            ? S15Fixed16toDouble((int)tmp)
            : null;
    }

#if PLUGIN
    public static XYZ? ReadXYZNumber(
#else
    internal static XYZ? ReadXYZNumber(
#endif
    this Stream io)
    {
        var x = Read15Fixed16Number(io);
        var y = Read15Fixed16Number(io);
        var z = Read15Fixed16Number(io);
        return (x != null && y != null && z != null)
            ? ((double)x, (double)y, (double)z)
            : null;
    }

#if PLUGIN
    public static bool Write(
#else
    internal static bool Write(
#endif
    this Stream io, byte n)
    {
        try
        {
            io.WriteByte(n);
            return true;
        }
        catch
        {
            return false;
        }
    }

#if PLUGIN
    public static bool Write(
#else
    internal static bool Write(
#endif
    this Stream io, ushort n)
    {
        try
        {
            var tmp = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        }
        catch
        {
            return false;
        }
    }

#if PLUGIN
    public static bool Write(
#else
    internal static bool Write(
#endif
    this Stream io, int n, ushort[] array)
    {
        for (var i = 0; i < n; i++)
        {
            if (!io.Write(array[i]))
                return false;
        }
        return true;
    }

#if PLUGIN
    public static bool Write(
#else
    internal static bool Write(
#endif
    this Stream io, uint n)
    {
        try
        {
            var tmp = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        }
        catch
        {
            return false;
        }
    }

#if PLUGIN
    public static bool Write(
#else
    internal static bool Write(
#endif
    this Stream io, int n)
    {
        try
        {
            var tmp = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        }
        catch
        {
            return false;
        }
    }

#if PLUGIN
    public static bool Write(
#else
    internal static bool Write(
#endif
    this Stream io, float n)
    {
        try
        {
            var tmp = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        }
        catch
        {
            return false;
        }
    }

#if PLUGIN
    public static bool Write(
#else
    internal static bool Write(
#endif
    this Stream io, ulong n)
    {
        try
        {
            var tmp = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        }
        catch
        {
            return false;
        }
    }

#if PLUGIN
    public static bool Write(
#else
    internal static bool Write(
#endif
    this Stream io, double n) =>
        io.Write(DoubleToS15Fixed16(n));

#if PLUGIN
    public static bool Write(
#else
    internal static bool Write(
#endif
    this Stream io, XYZ xyz) =>
        io.Write(xyz.X) && io.Write(xyz.Y) && io.Write(xyz.Z);

#if PLUGIN
    public static double S15Fixed16toDouble(
#else
    internal static double S15Fixed16toDouble(
#endif
    int value)
    {
        var sign = value < 0 ? -1 : 1;
        value = Math.Abs(value);

        var whole = (ushort)((value >> 16) & 0xffff);
        var fracPart = (ushort)(value & 0xffff);

        var mid = fracPart / 65536.0;
        var floater = whole + mid;

        return sign * floater;
    }

#if PLUGIN
    public static int DoubleToS15Fixed16(
#else
    internal static int DoubleToS15Fixed16(
#endif
    double value) =>
        (int)Math.Floor((value * 65536.0) + 0.5);

#if PLUGIN
    public static double U8Fixed8toDouble(
#else
    internal static double U8Fixed8toDouble(
#endif
    ushort value)
    {
        var lsb = (byte)(value & 0xff);
        var msb = (byte)((value >> 8) & 0xff);

        return msb + (lsb / 256.0);
    }
#if PLUGIN

    public static ushort DoubleToU8Fixed8(
#else
    internal static ushort DoubleToU8Fixed8(
#endif
    double value) =>
        (ushort)((DoubleToS15Fixed16(value) >> 8) & 0xffff);

#if PLUGIN
    public static Signature ReadTypeBase(
#else
    internal static Signature ReadTypeBase(
#endif
    this Stream io)
    {
        var sig = io.ReadUInt32Number();
        var res = io.ReadUInt32Number();

        return sig is null || res is null
            ? new Signature(0)
            : new Signature((uint)sig);
    }

#if PLUGIN
    public static bool Write(
#else
    internal static bool Write(
#endif
    this Stream io, Signature sig) =>
        io.Write(sig) && io.Write((uint)0);

#if PLUGIN
    public static bool ReadAlignment(
#else
    internal static bool ReadAlignment(
#endif
    this Stream io)
    {
        var buffer = new byte[4];
        var at = io.Tell();
        var nextAligned = AlignLong(at);
        var bytesToNextAlignedPos = nextAligned - at;

        return bytesToNextAlignedPos == 0
            || (bytesToNextAlignedPos <= 4 && io.Read(buffer, 0, (int)bytesToNextAlignedPos) != (int)bytesToNextAlignedPos);
    }

#if PLUGIN
    public static bool WriteAlignment(
#else
    internal static bool WriteAlignment(
#endif
    this Stream io)
    {
        var buffer = new byte[4];
        var at = io.Tell();
        var nextAligned = AlignLong(at);
        var bytesToNextAlignedPos = nextAligned - at;

        if (bytesToNextAlignedPos == 0) return true;
        if (bytesToNextAlignedPos > 4) return false;

        io.Write(buffer, 0, (int)bytesToNextAlignedPos);

        return true;
    }

#if PLUGIN
    public static void IOPrintf(
#else
    internal static void IOPrintf(
#endif
    this Stream io, string frm, params object?[] args)
    {
        var resultString = string.Format(frm, args);
        var bytes = Encoding.UTF8.GetBytes(resultString);
        io.Write(bytes, 0, Math.Min(bytes.Length, 2047));
    }

    private static long AlignLong(long x) =>
        (x + (sizeof(uint) - 1)) & ~(sizeof(uint) - 1);
}
