using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

using lcms2.plugins;
using lcms2.state;
using lcms2.types;
using lcms2.types.type_handlers;

using static lcms2.Helpers;

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
    public static bool ReadUInt8Number(this Stream io, out byte value)
    {
        var x = io.ReadByte();
        value = (byte)x;

        return x is not (> Byte.MaxValue or < Byte.MinValue);
    }

    /// <summary>
    /// Reads a <see cref="ushort"/> value from the <see cref="Stream"/>.
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
    /// Reads a <see cref="ushort"/> array from the <see cref="Stream"/>.
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
    /// Reads a <see cref="uint"/> value from the <see cref="Stream"/>.
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
    /// Reads a <see cref="int"/> value from the <see cref="Stream"/>.
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
    /// Reads a <see cref="float"/> value from the <see cref="Stream"/>.
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
    /// Reads a <see cref="ulong"/> value from the <see cref="Stream"/>.
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
    /// Reads a signed fixed point Q15.16 value from the <see cref="Stream"/>.
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
    /// Reads a <see cref="XYZ"/> value from the <see cref="Stream"/>.
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
    public static bool ReadUtf16String(this Stream io, int n, out string str)
    {
        var result = ReadCharArray(io, n, out var value);
        str = new string(value);

        return result;
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

        for (var i = 0; i < n; i++) {
            if (!io.ReadUInt16Number(out var value)) return false;
            str[i] = (char)value;
        }

        return true;
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

        for (var i = 0; i < count; i++) {
            if (!io.ReadUInt32Number(out var offset)) return false;
            if (!io.ReadUInt32Number(out var size)) return false;

            offsets[i] = offset + baseOffset;
            sizes[i] = size;
        }

        // Seek to each element and read it
        for (var i = 0; i < count; i++) {
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
        try {
            io.WriteByte(n);
            return true;
        } catch {
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
        try {
            var tmp = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        } catch {
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
        for (var i = 0; i < n; i++) {
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
        try {
            var tmp = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        } catch {
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
        try {
            var tmp = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        } catch {
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
        try {
            var tmp = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        } catch {
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
        try {
            var tmp = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(tmp, n);
            io.Write(tmp.AsSpan());
            return true;
        } catch {
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
    public static bool WriteUtf16String(this Stream io, string str) =>
        io.Write(str.ToCharArray());

    public static bool WriteAsciiString(this Stream io, string str, int len = -1)
    {
        if (len == -1) len = str.Length;

        try {
            var buf = new byte[len];
            for (var i = 0; i < str.Length; i++)
                buf[i] = (byte)str[i];

            io.Write(buf);

            return true;
        } catch {
            return false;
        }
    }

    public static bool Write(this Stream io, char[] str)
    {
        for (var i = 0; i < str.Length; i++) {
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
        for (var i = 0; i < count; i++) {
            if (!io.Write((uint)0)) return false; // Offset
            if (!io.Write((uint)0)) return false; // Size
        }

        // Write each element. Keep track of the size as well.
        for (var i = 0; i < count; i++) {
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

        for (var i = 0; i < count; i++) {
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
    /// <returns>The <see cref="TagBase"/> converted from big endian into native endian.</returns>
    public static TagBase ReadTypeBase(this Stream io)
    {
        try {
            unsafe {
                var buf = new byte[sizeof(TagBase)];
                if (io.Read(buf) != sizeof(TagBase))
                    return default;
                var tb = MemoryMarshal.Read<TagBase>(buf);

                return tb;
            }
        } catch {
            return default;
        }
    }

    /// <summary>
    /// Writes a <see cref="TagBase"/> to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="io">The <see cref="Stream"/> to write to</param>
    /// <param name="tagBase">The <see cref="TagBase"/> to write</param>
    /// <remarks>Implements the <c>_cmsWriteTypeBase</c> function.</remarks>
    /// <returns>Whether the write operation was successful</returns>
    public static bool Write(this Stream io, TagBase tagBase)
    {
        try {
            unsafe {
                tagBase.Signature = new Signature(AdjustEndianness(tagBase.Signature));
                var buf = new byte[sizeof(TagBase)];
                MemoryMarshal.Write(buf, ref tagBase);
                io.Write(buf);
            }
        } catch {
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
        try {
            var resultString = string.Format(frm, args);
            var bytes = Encoding.UTF8.GetBytes(resultString);
            io.Write(bytes, 0, Math.Min(bytes.Length, 2047));
        } catch {
            return false;
        }
        return true;
    }

    internal static bool Read8bitTables(this Stream io, Context? context, ref Pipeline lut, uint numChannels)
    {
        var tables = new ToneCurve[Lcms2.MaxChannels];

        if (numChannels is > Lcms2.MaxChannels or <= 0) return false;

        var temp = new byte[256];

        for (var i = 0; i < numChannels; i++) {
            var tab = ToneCurve.BuildTabulated16(context, 256, null);
            if (tab is null) goto Error;
            tables[i] = tab;
        }

        for (var i = 0; i < numChannels; i++) {
            if (io.Read(temp) != 256) goto Error;

            for (var j = 0; j < 256; j++)
                tables[i].Table16[j] = From8to16(temp[j]);
        }
        temp = null;

        if (!lut.InsertStage(StageLoc.AtEnd, Stage.AllocToneCurves(context, numChannels, tables)))
            goto Error;

        for (var i = 0; i < numChannels; i++)
            tables[i].Dispose();

        return true;

    Error:
        return false;
    }

    internal static bool Write8bitTables(this Stream io, Context? context, uint num, ref Stage.ToneCurveData tables)
    {
        for (var i = 0; i < num; i++) {
            // Usual case of identity curves
            if ((tables.TheCurves[i].NumEntries == 2) &&
                (tables.TheCurves[i].Table16[0] == 0) &&
                (tables.TheCurves[i].Table16[1] == 65535)) {
                for (var j = 0; j < 256; j++)
                    if (!io.Write((byte)j)) return false;
            } else {
                if (tables.TheCurves[i].NumEntries != 256) {
                    Context.SignalError(context, ErrorCode.Range, "LUT8 needs 256 entries on prelinearization");
                    return false;
                } else
                    for (var j = 0; j < 256; j++) {
                        var val = From16to8(tables.TheCurves[i].Table16[j]);

                        if (!io.Write(val)) return false;
                    }
            }
        }

        return true;
    }

    internal static bool Read16bitTables(this Stream io, Context? context, ref Pipeline lut, uint numChannels, uint numEntries)
    {
        var tables = new ToneCurve[Lcms2.MaxChannels];

        // Maybe an empty table? (this is a lcms extension)
        if (numEntries == 0) return true;

        // Check for malicious profiles
        if (numChannels is > Lcms2.MaxChannels || numEntries < 2) return false;

        for (var i = 0; i < numChannels; i++) {
            var tab = ToneCurve.BuildTabulated16(context, numEntries, null);
            if (tab is null) goto Error;
            tables[i] = tab;

            if (!io.ReadUInt16Array((int)numEntries, out tables[i].Table16)) goto Error;
        }

        // Add the table (which may certainly be an identity, but this is up to the optimizer, not the reading code)
        if (!lut.InsertStage(StageLoc.AtEnd, Stage.AllocToneCurves(context, numChannels, tables)))
            goto Error;

        for (var i = 0; i < numChannels; i++)
            tables[i].Dispose();

        return true;

    Error:
        for (var i = 0; i < numChannels; i++)
            if (tables[i] is not null)
                tables[i].Dispose();

        return false;
    }

    internal static bool Write16bitTables(this Stream io, ref Stage.ToneCurveData tables)
    {
        var numEntries = tables.TheCurves[0].NumEntries;

        for (var i = 0; i < tables.NumCurves; i++) {
            for (var j = 0; j < numEntries; j++) {
                var val = tables.TheCurves[i].Table16[j];
                if (!io.Write(val)) return false;
            }
        }

        return true;
    }

    internal static Stage? ReadMatrix(this Stream io, Context? context, uint offset)
    {
        var dMat = new double[9];
        var dOff = new double[3];

        // Go to address
        if (io.Seek(offset, SeekOrigin.Begin) != offset) return null;

        // Read the matrix
        for (var i = 0; i < 9; i++)
            if (!io.Read15Fixed16Number(out dMat[i])) return null;

        return Stage.AllocMatrix(context, 3, 3, in dMat, dOff);
    }

    internal static Stage? ReadClut(this Stream io, Context? context, uint offset, uint inputChannels, uint outputChannels)
    {
        var gridPoints8 = new byte[Lcms2.MaxChannels]; // Number of grid points in each dimension.
        var gridPoints = new uint[Lcms2.MaxChannels];
        ushort[]? nullTab = null;

        if (io.Seek(offset, SeekOrigin.Begin) != offset) return null;
        if (io.Read(gridPoints8, 0, Lcms2.MaxChannels) != Lcms2.MaxChannels) return null;

        for (var i = 0; i < Lcms2.MaxChannels; i++) {
            if (gridPoints[i] == 1) return null; // Impossible value, 0 for not CLUT and at least 2 for anything else
            gridPoints[i] = gridPoints8[i];
        }

        if (!io.ReadUInt8Number(out var precision)) return null;

        if (!io.ReadUInt8Number(out _)) return null;
        if (!io.ReadUInt8Number(out _)) return null;
        if (!io.ReadUInt8Number(out _)) return null;

        var clut = Stage.AllocCLut16bitGranular(context, gridPoints, inputChannels, outputChannels, in nullTab);
        if (clut is null || clut.Data is null) return null;

        var data = (Stage.CLutData)clut.Data;

        // Precision can be 1 or 2 bytes
        switch (precision) {
            case 1:
                for (var i = 0; i < data.NumEntries; i++) {
                    if (!io.ReadUInt8Number(out var v)) {
                        clut.Dispose();
                        return null;
                    }
                    data.Table.T[i] = From8to16(v);
                }
                break;
            case 2:
                if (!io.ReadUInt16Array(data.NumEntries, out data.Table.T)) {
                    clut.Dispose();
                    return null;
                }
                break;
            default:
                clut.Dispose();
                Context.SignalError(context, ErrorCode.UnknownExtension, "Unknown precision of '{0}'", precision);
                return null;
        }

        return clut;
    }

    internal static ToneCurve? ReadEmbeddedCurve(this Stream io, Context? context)
    {
        var baseType = io.ReadTypeBase();
        if (baseType.Signature == Signature.TagType.Curve) {
            var h = new CurveHandler();
            return (ToneCurve?)h.Read(io, 0, out _);
        } else if (baseType.Signature == Signature.TagType.ParametricCurve) {
            var h = new ParametricCurveHandler();
            return (ToneCurve?)h.Read(io, 0, out _);
        } else
            Context.SignalError(context, ErrorCode.UnknownExtension, "Unknown curve type '{0}'", baseType.Signature);
        return null;
    }

    internal static Stage? ReadSetOfCurves(this Stream io, Context? context, uint offset, uint numCurves)
    {
        Stage? lin = null;
        var curves = new ToneCurve?[Lcms2.MaxChannels];

        if (numCurves > Lcms2.MaxChannels) return null;

        if (io.Seek(offset, SeekOrigin.Begin) != offset) return null;

        for (var i = 0; i< numCurves; i++) {
            curves[i] = ReadEmbeddedCurve(io, context);
            if (curves[i] is null) goto Error;
            if (!io.ReadAlignment()) goto Error;
        }

        lin = Stage.AllocToneCurves(context, numCurves, curves);

    Error:
        for (var i = 0; i < numCurves; i++)
            curves[i]?.Dispose();
        return lin;
    }

    internal static bool WriteMatrix(this Stream io, Stage mpe)
    {
        var m = (Stage.MatrixData)mpe.Data;

        var num = mpe.InputChannels * mpe.OutputChannels;

        // Write the matrix
        for (var i = 0; i < num; i++)
            if (!io.Write(m.Double[i])) return false;

        for (var i = 0; i < mpe.OutputChannels; i++)
            if (!io.Write(m.Offset?[i] ?? 0)) return false;

        return true;
    }

    internal static bool WriteSetOfCurves(this Stream io, Context? context, Signature type, Stage mpe)
    {
        var num = mpe.OutputChannels;
        var curves = mpe.CurveSet;

        for (var i = 0; i < num; i++) {
            // If this is a table-based curve, use curve type even on V4
            var currentType = type;

            if ((curves[i].NumSegments == 0) ||
                ((curves[i].NumSegments == 2) &&
                (curves[i].Segments[i].Type == 0)) ||
                (curves[i].Segments[0].Type < 0))
                currentType = Signature.TagType.Curve;

            if (!io.Write(new TagBase(currentType))) return false;

            if (currentType == Signature.TagType.Curve) {
                var h = new CurveHandler(context);
                if (!h.Write(io, curves[i], 1)) return false;
            } else if (currentType == Signature.TagType.ParametricCurve) {
                var h = new ParametricCurveHandler(context);
                if (!h.Write(io, curves[i], 1)) return false;
            } else {
                Context.SignalError(context, ErrorCode.UnknownExtension, "Unknown curve type '{0}'", type);
                return false;
            }

            if (!io.WriteAlignment()) return false;
        }

        return true;
    }

    internal static bool WriteClut(this Stream io, Context? context, byte precision, Stage mpe)
    {
        var gridPoints = new byte[Lcms2.MaxChannels]; // Number of grid points in each dimension.
        var clut = (Stage.CLutData)mpe.Data;

        if (clut.HasFloatValues) {
            Context.SignalError(context, ErrorCode.NotSuitable, "Cannot save floating point data, CLUT are 8 or 16 bit only");
            return false;
        }

        for (var i = 0; i < clut.Params[0].NumInputs; i++)
            gridPoints[i] = (byte)clut.Params[0].NumSamples[i];

        io.Write(gridPoints, 0, Lcms2.MaxChannels * sizeof(byte));

        if (!io.Write(precision)) return false;
        if (!io.Write((byte)0)) return false;
        if (!io.Write((byte)0)) return false;
        if (!io.Write((byte)0)) return false;

        // Precision can be 1 or 2 bytes
        switch (precision) {
            case 1:

                for (var i = 0; i < clut.NumEntries; i++)
                    if (!io.Write(From16to8(clut.Table.T[i]))) return false;

                break;

            case 2:

                if (!io.Write(clut.NumEntries, clut.Table.T)) return false;

                break;

            default:

                Context.SignalError(context, ErrorCode.UnknownExtension, "Unknown precision of '{0}'", precision);
                return false;
        }

        return io.WriteAlignment();
    }
}
