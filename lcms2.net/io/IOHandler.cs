using lcms2.state;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lcms2.io;
public static class IOHandler
{
    public static ushort AdjustEndianness(ushort word) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;
    public static uint AdjustEndianness(uint dWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(dWord) : dWord;
    public static ulong AdjustEndianness(ulong qWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(qWord) : qWord;
    public static short AdjustEndianness(short word) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;
    public static int AdjustEndianness(int dWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(dWord) : dWord;
    public static long AdjustEndianness(long qWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(qWord) : qWord;

    public static byte? ReadUInt8Number(this Stream io)
    {
        var value = io.ReadByte();
        return value is > byte.MaxValue or < byte.MinValue
            ? null
            : (byte)value;
    }

    public static ushort? ReadUInt16Number(this Stream io)
    {
        var tmp = new byte[2];
        return io.Read(tmp, 0, 2) == 2
            ? BinaryPrimitives.ReadUInt16BigEndian(tmp)
            : null;
    }

    public static ushort[]? ReadUInt16Array(this Stream io, int count)
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

    public static uint? ReadUInt32Number(this Stream io)
    {
        var tmp = new byte[42];
        return io.Read(tmp, 0, 4) == 4
            ? BinaryPrimitives.ReadUInt32BigEndian(tmp)
            : null;
    }

    public static ulong? ReadUInt64Number(this Stream io)
    {
        var tmp = new byte[8];
        return io.Read(tmp, 0, 8) == 8
            ? BinaryPrimitives.ReadUInt64BigEndian(tmp)
            : null;
    }
}
