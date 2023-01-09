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
using lcms2.types;

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace lcms2.io;

public static class IOHandler
{
    #region Enums

    public enum AccessMode
    {
        Read,
        Write,
    }

    #endregion Enums

    #region Public Methods

    public static DateTimeNumber AdjustEndianness(DateTimeNumber date)
    {
        date.Seconds = AdjustEndianness(date.Seconds);
        date.Minutes = AdjustEndianness(date.Minutes);
        date.Hours = AdjustEndianness(date.Hours);
        date.Day = AdjustEndianness(date.Day);
        date.Month = AdjustEndianness(date.Month);
        date.Year = AdjustEndianness(date.Year);

        return date;
    }

    /// <summary>
    ///     Swaps the endianness of a <see cref="ushort"/> on little endian machines. ICC Profiles
    ///     are stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="word">Word value to be swapped</param>
    /// <remarks>Implements the <c>_cmsAdjustEndianess16</c> function.</remarks>
    public static ushort AdjustEndianness(ushort word) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;

    /// <summary>
    ///     Swaps the endianness of a <see cref="uint"/> on little endian machines. ICC Profiles are
    ///     stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="dWord">dWord value to be swapped</param>
    /// <remarks>Implements the <c>_cmsAdjustEndianess32</c> function.</remarks>
    public static uint AdjustEndianness(uint dWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(dWord) : dWord;

    /// <summary>
    ///     Swaps the endianness of a <see cref="ulong"/> on little endian machines. ICC Profiles
    ///     are stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="qWord">qWord value to be swapped</param>
    /// <remarks>Implements the <c>_cmsAdjustEndianess64</c> function.</remarks>
    public static ulong AdjustEndianness(ulong qWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(qWord) : qWord;

    /// <summary>
    ///     Swaps the endianness of a <see cref="short"/> on little endian machines. ICC Profiles
    ///     are stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="word">Word value to be swapped</param>
    public static short AdjustEndianness(short word) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(word) : word;

    /// <summary>
    ///     Swaps the endianness of a <see cref="int"/> on little endian machines. ICC Profiles are
    ///     stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="dWord">dWord value to be swapped</param>
    public static int AdjustEndianness(int dWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(dWord) : dWord;

    /// <summary>
    ///     Swaps the endianness of a <see cref="long"/> on little endian machines. ICC Profiles are
    ///     stored in big endian and requires "adjustment".
    /// </summary>
    /// <param name="qWord">qWord value to be swapped</param>
    public static long AdjustEndianness(long qWord) =>
        BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(qWord) : qWord;

    /// <summary>
    ///     Writes a <see cref="string"/> to the <see cref="Stream"/> (up to 2K).
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

    /// <summary>
    ///     Reads a signed fixed point Q15.16 value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsRead15Fixed16Number</c> function.</remarks>
    /// <returns>The fixed point value represented as a <see cref="double"/> in native endian.</returns>
    public static bool Read15Fixed16Number(this Stream io, out double value)
    {
        value = 0;
        if (!io.ReadInt32Number(out var tmp)) return false;

        value = S15Fixed16toDouble(tmp);
        return true;
    }

    /// <summary>
    ///     Aligns the <see cref="Stream"/> on the next 4-byte boundary for reading.
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

    public static bool ReadAsciiString(this Stream io, int n, out string str)
    {
        str = String.Empty;
        var buf = new byte[n];
        var chars = new char[n];

        if (io.Read(buf) != n) return false;

        for (var i = 0; i < n; i++)
            chars[i] = (char)buf[i];

        str = new string(chars);
        return true;
    }

    public static bool ReadCharArray(this Stream io, int n, out char[] str)
    {
        str = new char[n];

        for (var i = 0; i < n; i++)
        {
            if (!io.ReadUInt16Number(out var value)) return false;
            str[i] = (char)value;
        }

        return true;
    }

    /// <summary>
    ///     Reads a <see cref="float"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadFloat32Number</c> function.</remarks>
    /// <returns>The <see cref="float"/> value converted from big endian into native endian.</returns>
    public static bool ReadFloat32Number(this Stream io, out float value)
    {
        var tmp = new byte[sizeof(float)];
        var len = io.Read(tmp.AsSpan());
        value = BinaryPrimitives.ReadSingleBigEndian(tmp);

        return len == sizeof(float);
    }

    /// <summary>
    ///     Reads a <see cref="int"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <returns>The <see cref="int"/> value converted from big endian into native endian.</returns>
    public static bool ReadInt32Number(this Stream io, out int value)
    {
        var tmp = new byte[sizeof(int)];
        var len = io.Read(tmp.AsSpan());
        value = BinaryPrimitives.ReadInt32BigEndian(tmp);

        return len == sizeof(int);
    }

    /// <summary>
    ///     Reads a <see cref="TagBase"/> from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadTypeBase</c> function.</remarks>
    /// <returns>The <see cref="TagBase"/> converted from big endian into native endian.</returns>
    public static unsafe TagBase ReadTypeBase(this Stream io)
    {
        try
        {
            var buf = new byte[sizeof(TagBase)];
            if (io.Read(buf) != sizeof(TagBase))
                return default;
            var tb = MemoryMarshal.Read<TagBase>(buf);

            return tb;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    ///     Reads a <see cref="ushort"/> array from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <param name="count">The length of the array to read.</param>
    /// <remarks>Implements the <c>_cmsReadUInt16Array</c> function.</remarks>
    /// <returns>The <see cref="ushort"/> array converted from big endian into native endian.</returns>
    public static bool ReadUInt16Array(this Stream io, int count, out ushort[] array)
    {
        array = new ushort[count];
        for (var i = 0; i < count; i++)
            if (!io.ReadUInt16Number(out array[i])) return false;

        return true;
    }

    /// <summary>
    ///     Reads a <see cref="ushort"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadUInt16Number</c> function.</remarks>
    /// <returns>The <see cref="ushort"/> value converted from big endian into native endian.</returns>
    public static bool ReadUInt16Number(this Stream io, out ushort value)
    {
        var tmp = new byte[sizeof(ushort)];
        var len = io.Read(tmp.AsSpan());
        value = BinaryPrimitives.ReadUInt16BigEndian(tmp);

        return len == sizeof(ushort);
    }

    /// <summary>
    ///     Reads a <see cref="uint"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadUInt32Number</c> function.</remarks>
    /// <returns>The <see cref="uint"/> value converted from big endian into native endian.</returns>
    public static bool ReadUInt32Number(this Stream io, out uint value)
    {
        var tmp = new byte[sizeof(uint)];
        var len = io.Read(tmp.AsSpan());
        value = BinaryPrimitives.ReadUInt32BigEndian(tmp);

        return len == sizeof(uint);
    }

    /// <summary>
    ///     Reads a <see cref="ulong"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadUInt64Number</c> function.</remarks>
    /// <returns>The <see cref="ulong"/> value converted from big endian into native endian.</returns>
    public static bool ReadUInt64Number(this Stream io, out ulong value)
    {
        var tmp = new byte[sizeof(ulong)];
        var len = io.Read(tmp.AsSpan());
        value = BinaryPrimitives.ReadUInt64BigEndian(tmp);

        return len == sizeof(ulong);
    }

    /// <summary>
    ///     Reads a <see cref="byte"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadUInt8Number</c> function.</remarks>
    public static bool ReadUInt8Number(this Stream io, out byte value)
    {
        var x = io.ReadByte();
        value = (byte)x;

        return x is not (> Byte.MaxValue or < Byte.MinValue);
    }

    /// <summary>
    ///     Reads a <see cref="string"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <param name="n">Length of the string to read.</param>
    /// <returns>
    ///     The <see cref="string"/> value converted from UTF16 big endian into a native endian
    ///     UTF16 string or <see langword="null"/> if there was a problem.
    /// </returns>
    public static bool ReadUtf16String(this Stream io, int n, out string str)
    {
        var result = ReadCharArray(io, n, out var value);
        str = new string(value);

        return result;
    }

    /// <summary>
    ///     Reads a <see cref="XYZ"/> value from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io"><see cref="Stream"/> to read from</param>
    /// <remarks>Implements the <c>_cmsReadXYZNumber</c> function.</remarks>
    /// <returns>The <see cref="XYZ"/> value converted from big endian into native endian.</returns>
    public static bool ReadXYZNumber(this Stream io, out XYZ value)
    {
        value = default;

        if (!io.Read15Fixed16Number(out var x)) return false;
        if (!io.Read15Fixed16Number(out var y)) return false;
        if (!io.Read15Fixed16Number(out var z)) return false;

        value = (x, y, z);
        return true;
    }

    public static long Tell(this Stream io) =>
                                            io.Seek(0, SeekOrigin.Current);

    /// <summary>
    ///     Writes a <see cref="byte"/> value to the <see cref="Stream"/>.
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
    ///     Writes a <see cref="ushort"/> value to the <see cref="Stream"/>.
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
    ///     Writes a <see cref="ushort"/> array to the <see cref="Stream"/>.
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
    ///     Writes a <see cref="uint"/> value to the <see cref="Stream"/>.
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
    ///     Writes a <see cref="int"/> value to the <see cref="Stream"/>.
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
    ///     Writes a <see cref="float"/> value to the <see cref="Stream"/>.
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
    ///     Writes a <see cref="ulong"/> value to the <see cref="Stream"/>.
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
    ///     Writes a signed fixed point Q15.16 value represented as a <see cref="double"/> to the
    ///     <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="n">The value to write</param>
    /// <remarks>Implements the <c>_cmsWrite15Fixed16Number</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, double n) =>
        io.Write(DoubleToS15Fixed16(n));

    /// <summary>
    ///     Writes a <see cref="XYZ"/> value to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="xyz">The value to write</param>
    /// <remarks>Implements the <c>_cmsWriteXYZNumber</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, XYZ xyz) =>
        io.Write(xyz.X) && io.Write(xyz.Y) && io.Write(xyz.Z);

    public static bool Write(this Stream io, char[] str)
    {
        for (var i = 0; i < str.Length; i++)
        {
            if (!io.Write(str[i]))
                return false;
        }
        return true;
    }

    /// <summary>
    ///     Writes a <see cref="TagBase"/> to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="tagBase">The <see cref="TagBase"/> to write</param>
    /// <remarks>Implements the <c>_cmsWriteTypeBase</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static unsafe bool Write(this Stream io, TagBase tagBase)
    {
        try
        {
            tagBase.Signature = new Signature(AdjustEndianness(tagBase.Signature));
            var buf = new byte[sizeof(TagBase)];
            MemoryMarshal.Write(buf, ref tagBase);
            io.Write(buf);
        }
        catch
        {
            return false;
        }
        return true;
    }

    /// <summary>
    ///     Aligns the <see cref="Stream"/> on the next 4-byte boundary for reading.
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

    public static bool WriteAsciiString(this Stream io, string str, int len = -1)
    {
        if (len == -1) len = str.Length;

        try
        {
            var buf = new byte[len];
            for (var i = 0; i < str.Length; i++)
                buf[i] = (byte)str[i];

            io.Write(buf);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Writes a <see cref="string"/> to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="str">The string to write</param>
    /// <returns>Whether the write operation was successful</returns>
    public static bool WriteUtf16String(this Stream io, string str) =>
        io.Write(str.ToCharArray());

    #endregion Public Methods
}
