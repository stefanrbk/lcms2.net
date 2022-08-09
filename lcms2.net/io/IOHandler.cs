﻿using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

using lcms2.plugins;
using lcms2.types;

namespace lcms2.io;
public static class IOHandler
{
    public static long Tell(this Stream io) =>
        io.Seek(0, SeekOrigin.Current);

    /// <summary>
    /// Swaps the endianness of a <see cref="ushort"/> on little endian machines.
    /// ICC Profiles are stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="word">Word value to be swapped</param>
    /// <remarks>Implements the <c>_cmsAdjustEndianess16</c> function.</remarks>
    public static ushort AdjustEndianness(ushort word) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;

    /// <summary>
    /// Swaps the endianness of a <see cref="uint"/> on little endian machines.
    /// ICC Profiles are stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="dWord">dWord value to be swapped</param>
    /// <remarks>Implements the <c>_cmsAdjustEndianess32</c> function.</remarks>
    public static uint AdjustEndianness(uint dWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(dWord) : dWord;

    /// <summary>
    /// Swaps the endianness of a <see cref="ulong"/> on little endian machines.
    /// ICC Profiles are stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="qWord">qWord value to be swapped</param>
    /// <remarks>Implements the <c>_cmsAdjustEndianess64</c> function.</remarks>
    public static ulong AdjustEndianness(ulong qWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(qWord) : qWord;

    /// <summary>
    /// Swaps the endianness of a <see cref="short"/> on little endian machines.
    /// ICC Profiles are stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="word">Word value to be swapped</param>
    public static short AdjustEndianness(short word) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;

    /// <summary>
    /// Swaps the endianness of a <see cref="int"/> on little endian machines.
    /// ICC Profiles are stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="dWord">dWord value to be swapped</param>
    public static int AdjustEndianness(int dWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(dWord) : dWord;

    /// <summary>
    /// Swaps the endianness of a <see cref="long"/> on little endian machines.
    /// ICC Profiles are stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="qWord">qWord value to be swapped</param>
    public static long AdjustEndianness(long qWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(qWord) : qWord;

    /// <summary>
    /// Reads a <see cref="byte"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadUInt8Number</c> function.</remarks>
    public static byte? ReadUInt8Number(this Stream io)
    {
        var value = io.ReadByte();
        return value is > byte.MaxValue or < byte.MinValue
            ? null
            : (byte)value;
    }

    /// <summary>
    /// Reads a <see cref="ushort"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadUInt16Number</c> function.</remarks>
    /// <returns>The <see cref="ushort"/> value converted from big endian into native endian.</returns>
    public static ushort? ReadUInt16Number(this Stream io)
    {
        var tmp = new byte[sizeof(ushort)];
        return io.Read(tmp.AsSpan()) == sizeof(ushort)
            ? BinaryPrimitives.ReadUInt16BigEndian(tmp)
            : null;
    }

    /// <summary>
    /// Reads a <see cref="ushort"/> array from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <param name="count">The length of the array to read.</param>
    /// <remarks>Implements the <c>_cmsReadUInt16Array</c> function.</remarks>
    /// <returns>The <see cref="ushort"/> array converted from big endian into native endian.</returns>
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

    /// <summary>
    /// Reads a <see cref="uint"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadUInt32Number</c> function.</remarks>
    /// <returns>The <see cref="uint"/> value converted from big endian into native endian.</returns>
    public static uint? ReadUInt32Number(this Stream io)
    {
        var tmp = new byte[sizeof(uint)];
        return io.Read(tmp.AsSpan()) == sizeof(uint)
            ? BinaryPrimitives.ReadUInt32BigEndian(tmp)
            : null;
    }

    /// <summary>
    /// Reads a <see cref="int"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <returns>The <see cref="int"/> value converted from big endian into native endian.</returns>
    public static int? ReadInt32Number(this Stream io)
    {
        var tmp = new byte[sizeof(int)];
        return io.Read(tmp.AsSpan()) == sizeof(int)
            ? BinaryPrimitives.ReadInt32BigEndian(tmp)
            : null;
    }

    /// <summary>
    /// Reads a <see cref="float"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadFloat32Number</c> function.</remarks>
    /// <returns>The <see cref="float"/> value converted from big endian into native endian.</returns>
    public static float? ReadFloat32Number(this Stream io)
    {
        var tmp = new byte[sizeof(float)];
        return io.Read(tmp.AsSpan()) == sizeof(float)
            ? BinaryPrimitives.ReadSingleBigEndian(tmp)
            : null;
    }

    /// <summary>
    /// Reads a <see cref="ulong"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadUInt64Number</c> function.</remarks>
    /// <returns>The <see cref="ulong"/> value converted from big endian into native endian.</returns>
    public static ulong? ReadUInt64Number(this Stream io)
    {
        var tmp = new byte[sizeof(ulong)];
        return io.Read(tmp.AsSpan()) == sizeof(ulong)
            ? BinaryPrimitives.ReadUInt64BigEndian(tmp)
            : null;
    }

    /// <summary>
    /// Reads a signed fixed point Q15.16 value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsRead15Fixed16Number</c> function.</remarks>
    /// <returns>The fixed point value represented as a <see cref="double"/> in native endian.</returns>
    public static double? Read15Fixed16Number(this Stream io)
    {
        var tmp = ReadInt32Number(io);
        return tmp is not null
            ? S15Fixed16toDouble((int)tmp)
            : null;
    }

    /// <summary>
    /// Reads a <see cref="XYZ"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadXYZNumber</c> function.</remarks>
    /// <returns>The <see cref="XYZ"/> value converted from big endian into native endian.</returns>
    public static XYZ? ReadXYZNumber(this Stream io)
    {
        var x = Read15Fixed16Number(io);
        var y = Read15Fixed16Number(io);
        var z = Read15Fixed16Number(io);
        return (x != null && y != null && z != null)
            ? ((double)x, (double)y, (double)z)
            : null;
    }

    /// <summary>
    ///     Reads a <see cref="string"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">
    ///     <see cref="Stream"/> to read from</param>
    /// <param name="n">
    ///     Length of the string to read.</param>
    /// <returns>
    ///     The <see cref="string"/> value converted from UTF16 big endian into a native endian UTF16 string or
    ///     <see langword="null"/> if there was a problem.</returns>
    public static string? ReadString(this Stream io, int n)
    {
        var sb = new StringBuilder(n);

        for (var i = 0; i < n; i++)
        {
            var value = io.ReadUInt16Number();
            if (value is null)
                return null;
            sb.Append((char)value);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Reads a position table as described in ICC spec 4.3.<br />
    ///     A table of n elements is read, where first comes n records containing offsets and sizes and
    ///     then a block containing the data itself. This allows to reuse same data in more than one entry.
    /// </summary>
    /// <param name="io">
    ///     <see cref="Stream"/> to read from</param>
    /// <returns>
    ///     Whether the read operation was successful.</returns>
    public static bool ReadPositionTable(this Stream io, ITagTypeHandler self, int count, uint baseOffset, ref object cargo, PositionTableEntryFn elementFn)
    {
        var currentPos = io.Tell();

        // Verify there is enough space left to read at least two int items for count items.
        if (((io.Length - currentPos) / (2 * sizeof(uint))) < count)
            return false;

        // Let's take the offsets to each element
        var offsets = new uint[count];
        var sizes = new uint[count];

        for (var i = 0; i < count; i++)
        {
            var offset = io.ReadUInt32Number();
            if (offset is null) return false;

            var size = io.ReadUInt32Number();
            if (size is null) return false;

            offsets[i] = (uint)offset + baseOffset;
            sizes[i] = (uint)size;
        }

        // Seek to each element and read it
        for (var i = 0; i < count; i++)
        {
            if (io.Seek(offsets[i], SeekOrigin.Begin) != offsets[i])
                return false;

            // This is the reader callback
            if (!elementFn(self, io, ref cargo, i, (int)sizes[i])) return false;
        }

        return true;
    }

    /// <summary>
    /// Writes a <see cref="byte"/> value to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="n">The value to write</param>
    /// <remarks>Implements the <c>_cmsWriteUInt8Number</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, byte n)
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

    /// <summary>
    /// Writes a <see cref="ushort"/> value to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="n">The value to write</param>
    /// <remarks>Implements the <c>_cmsWriteUInt16Number</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, ushort n)
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

    /// <summary>
    /// Writes a <see cref="ushort"/> array to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="n">The array length</param>
    /// <param name="array">The array to write</param>
    /// <remarks>Implements the <c>_cmsWriteUInt16Array</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, int n, ushort[] array)
    {
        for (var i = 0; i < n; i++)
        {
            if (!io.Write(array[i]))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Writes a <see cref="uint"/> value to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="n">The value to write</param>
    /// <remarks>Implements the <c>_cmsWriteUInt32Number</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, uint n)
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

    /// <summary>
    /// Writes a <see cref="int"/> value to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="n">The value to write</param>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, int n)
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

    /// <summary>
    /// Writes a <see cref="float"/> value to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="n">The value to write</param>
    /// <remarks>Implements the <c>_cmsWriteFloat32Number</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, float n)
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

    /// <summary>
    /// Writes a <see cref="ulong"/> value to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="n">The value to write</param>
    /// <remarks>Implements the <c>_cmsWriteUInt64Number</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, ulong n)
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

    /// <summary>
    /// Writes a signed fixed point Q15.16 value represented as a <see cref="double"/> to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="n">The value to write</param>
    /// <remarks>Implements the <c>_cmsWrite15Fixed16Number</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, double n) =>
        io.Write(DoubleToS15Fixed16(n));

    /// <summary>
    /// Writes a <see cref="XYZ"/> value to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="xyz">The value to write</param>
    /// <remarks>Implements the <c>_cmsWriteXYZNumber</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, XYZ xyz) =>
        io.Write(xyz.X) && io.Write(xyz.Y) && io.Write(xyz.Z);

    /// <summary>
    ///     Writes a <see cref="string"/> to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">
    ///     The <see cref="Stream"/> to write to</param>
    /// <param name="str">
    ///     The string to write</param>
    /// <returns>
    ///     Whether the write operation was successful</returns>
    public static bool Write(this Stream io, string str)
    {
        for (var i = 0; i < str.Length; i++)
        {
            if (!io.Write(str[i]))
                return false;
        }
        return true;
    }

    /// <summary>
    ///     Writes a position table as described in ICC spec 4.3.<br />
    ///     A table of n elements is read, where first comes n records containing offsets and sizes and
    ///     then a block containing the data itself. This allows to reuse same data in more than one entry.
    /// </summary>
    /// <param name="io">
    ///     <see cref="Stream"/> to write to</param>
    /// <returns>
    ///     Whether the read operation was successful.</returns>
    public static bool Write(this Stream io, ITagTypeHandler self, int sizeOfTag, int count, uint baseOffset, ref object cargo, PositionTableEntryFn elementFn)
    {
        // Create table
        var offsets = new uint[count];
        var sizes = new uint[count];

        // Keep starting position of curve offsets
        var dirPos = io.Tell();

        // Write a fake directory to be filled later on
        for (var i = 0; i < count; i++)
        {
            if (!io.Write((uint)0)) return false; // Offset
            if (!io.Write((uint)0)) return false; // Size
        }

        // Write each element. Keep track of the size as well.
        for (var i = 0; i < count; i++)
        {
            var before = (uint)io.Tell();
            offsets[i] = before - baseOffset;

            // Callback to write...
            if (!elementFn(self, io, ref cargo, i, sizeOfTag)) return false;

            // Now the size
            sizes[i] = (uint)io.Tell() - before;
        }

        // Write the directory
        var curPos = io.Tell();
        if (io.Seek(dirPos, SeekOrigin.Begin) != dirPos) return false;

        for (var i = 0; i < count; i++)
        {
            if (!io.Write(offsets[i])) return false;
            if (!io.Write(sizes[i])) return false;
        }

        return io.Seek(curPos, SeekOrigin.Begin) == curPos; // Make sure we end up at the end of the table
    }

    /// <summary>
    /// Converts a Q15.16 signed fixed-point number into a double-precision floating-point number.
    /// </summary>
    /// <remarks>Implements the <c>_cms15Fixed16toDouble</c> function.</remarks>
    public static double S15Fixed16toDouble(int value)
    {
        var sign = value < 0 ? -1 : 1;
        value = Math.Abs(value);

        var whole = (ushort)((value >> 16) & 0xffff);
        var fracPart = (ushort)(value & 0xffff);

        var mid = fracPart / 65536.0;
        var floater = whole + mid;

        return sign * floater;
    }

    /// <summary>
    /// Converts a double-precision floating-point number into a Q15.16 signed fixed-point number.
    /// </summary>
    /// <remarks>Implements the <c>_cmsDoubleTo15Fixed16</c> function.</remarks>
    public static int DoubleToS15Fixed16(double value) =>
        (int)Math.Floor((value * 65536.0) + 0.5);

    /// <summary>
    /// Converts a Q8.8 unsigned fixed-point number into a double-precision floating-point number.
    /// </summary>
    /// <remarks>Implements the <c>_cms8Fixed8toDouble</c> function.</remarks>
    public static double U8Fixed8toDouble(ushort value)
    {
        var lsb = (byte)(value & 0xff);
        var msb = (byte)((value >> 8) & 0xff);

        return msb + (lsb / 256.0);
    }

    /// <summary>
    /// Converts a double-precision floating-point number into a Q8.8 unsigned fixed-point number.
    /// </summary>
    /// <remarks>Implements the <c>_cmsDoubleTo8Fixed8</c> function.</remarks>
    public static ushort DoubleToU8Fixed8(double value) =>
        (ushort)((DoubleToS15Fixed16(value) >> 8) & 0xffff);

    /// <summary>
    /// Reads a <see cref="TagBase"/> from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadTypeBase</c> function.</remarks>
    /// <returns>The <see cref="Signature"/> part converted from big endian into native endian.</returns>
    public static Signature ReadTypeBase(this Stream io)
    {
        try
        {
            unsafe
            {
                var buf = new byte[sizeof(TagBase)];
                if (io.Read(buf) != sizeof(TagBase))
                    return new Signature(0);
                var tb = MemoryMarshal.Read<TagBase>(buf);

                return new Signature(AdjustEndianness((uint)tb.Signature));
            }
        }
        catch
        {
            return new Signature(0);
        }
    }

    /// <summary>
    /// Writes a <see cref="TagBase"/> to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="sig">The <see cref="Signature"/> part to write</param>
    /// <remarks>Implements the <c>_cmsWriteTypeBase</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, Signature sig)
    {
        try
        {
            unsafe
            {
                TagBase tagBase;

                tagBase.Signature = new Signature(AdjustEndianness(sig));
                var buf = new byte[sizeof(TagBase)];
                MemoryMarshal.Write(buf, ref tagBase);
                io.Write(buf);
            }
        }
        catch
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Aligns the <see cref="Stream"/> on the next 4-byte boundary for reading.
    /// </summary>
    /// <remarks>Implements the <c>_cmsReadAlignment</c> function.</remarks>
    /// <returns>Whether the alignment operation was successful</returns>
    public static bool ReadAlignment(this Stream io)
    {
        var buffer = new byte[4];
        var at = io.Tell();
        var nextAligned = AlignLong(at);
        var bytesToNextAlignedPos = nextAligned - at;

        return bytesToNextAlignedPos == 0
            || (bytesToNextAlignedPos <= 4 && io.Read(buffer, 0, (int)bytesToNextAlignedPos) != (int)bytesToNextAlignedPos);
    }

    /// <summary>
    /// Aligns the <see cref="Stream"/> on the next 4-byte boundary for reading.
    /// </summary>
    /// <remarks>Implements the <c>_cmsReadAlignment</c> function.</remarks>
    /// <returns>Whether the alignment operation was successful</returns>
    public static bool WriteAlignment(this Stream io)
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

    /// <summary>
    /// Writes a <see cref="string"/> to the <see cref="Stream"/> (up to 2K).
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to write to</param>
    /// <remarks>Implements the <c>_cmsIOPrintf</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool IOPrintf(this Stream io, string frm, params object?[] args)
    {
        try
        {
            var resultString = string.Format(frm, args);
            var bytes = Encoding.UTF8.GetBytes(resultString);
            io.Write(bytes, 0, Math.Min(bytes.Length, 2047));
        }
        catch
        {
            return false;
        }
        return true;
    }

    private static long AlignLong(long x) =>
        (x + (sizeof(uint) - 1)) & ~(sizeof(uint) - 1);
}
