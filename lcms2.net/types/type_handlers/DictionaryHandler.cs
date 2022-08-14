using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class DictionaryHandler : TagTypeHandler
{
    public DictionaryHandler(Context? context = null)
        : base(default, context, 0) { }

    public override object? Duplicate(object value, int num) =>
        (value as Dictionary)?.Clone();

    public override void Free(object value) =>
        (value as Dictionary)?.Dispose();

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        Dictionary? hDict = null;
        Mlu? dispNameMlu = null, dispValueMlu = null;
        bool rc;
        DicArray a;

        numItems = 0;

        // Get actual position as a basis for element offsets
        var baseOffset = (uint)(io.Tell() - sizeof(TagBase));

        // Get name-value record count
        sizeOfTag -= sizeof(uint);
        if (sizeOfTag < 0) goto Error;
        if (!io.ReadUInt32Number(out var count)) goto Error;

        // Get rec length
        sizeOfTag -= sizeof(uint);
        if (sizeOfTag < 0) goto Error;
        if (!io.ReadUInt32Number(out var length)) goto Error;

        // Check for valid lengths
        if (length is not 16 and not 24 and not 32) {
            Context.SignalError(Context, ErrorCode.UnknownExtension, "Unknown record length in dictionary '{0}'", length);
            goto Error;
        }

        // Creates an empty dictionary
        hDict = new Dictionary(Context);

        // Depending on record size, create column arrays
        a = new DicArray(Context, count, length);

        // Read column arrays
        if (a.ReadOffset(io, count, length, baseOffset, ref sizeOfTag)) goto Error;

        // Seek to each element and read it
        for (var i = 0u; i < count; i++) {

            if (!a.Name.ReadOneChar(io, i, out var nameStr)) goto Error;
            if (!a.Name.ReadOneChar(io, i, out var valueStr)) goto Error;

            if (length > 16 &&
                !a.Value.ReadOneMluC(this, io, i, out dispNameMlu))

                goto Error;

            if (length > 24 &&
                !a.Value.ReadOneMluC(this, io, i, out dispValueMlu))

                goto Error;

            if (String.IsNullOrEmpty(nameStr) || String.IsNullOrEmpty(valueStr)) {
                Context.SignalError(Context, ErrorCode.CorruptionDetected, "Bad dictionary Name/Value");
                rc = false;
            } else {
                hDict.AddEntry(nameStr, valueStr, dispNameMlu, dispValueMlu);
                rc = true;
            }

            if (!rc) goto Error;
        }

        numItems = 1;
        return hDict;

    Error:

        hDict?.Dispose();
        return null;
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        DictionaryEntry? p;
        var dict = (Dictionary)value;

        if (dict is null) return false;

        var baseOffset = (uint)(io.Tell() - sizeof(TagBase));

        // Let's inspect the dictionary
        var count = 0u;
        var anyName = false;
        var anyValue = false;
        for (p = dict.Head; p is not null; p = p.Next) {

            if (p.DisplayName is not null) anyName = true;
            if (p.DisplayValue is not null) anyValue = true;
            count++;
        }

        var length = 16u;
        if (anyName) length += 8;
        if (anyValue) length += 8;

        if (!io.Write(count)) return false;
        if (!io.Write(length)) return false;

        // Keep starting position of offsets table
        var dirPos = (uint)io.Tell();

        // Allocate offsets array
        var a = new DicArray(Context, count, length);

        // Write a fake directory to be filled later on
        if (!a.WriteOffset(io, count, length)) return false;

        // Write each element. Keep track of the size as well.
        p = dict.Head;
        for (var i = 0u; i < count; i++) {

            if (p is null) return false;

            if (!a.Name.WriteOneChar(io, i, p.Name, baseOffset)) return false;
            if (!a.Name.WriteOneChar(io, i, p.Value, baseOffset)) return false;

            if (p.DisplayName is not null &&
                !a.DisplayName!.Value.WriteOneMluC(this, io, i, p.DisplayName, baseOffset)) return false;

            if (p.DisplayValue is not null &&
                !a.DisplayValue!.Value.WriteOneMluC(this, io, i, p.DisplayValue, baseOffset)) return false;

            p = p.Next;
        }

        // Write the directory
        var curPos = (uint)io.Tell();
        if (io.Seek(dirPos, SeekOrigin.Begin) != dirPos) return false;

        if (!a.WriteOffset(io, count, length)) return false;

        if (io.Seek(curPos, SeekOrigin.Begin) != curPos) return false;

        return true;
    }

    internal struct DicElem
    {
        public Context? Context;
        public uint[] Offsets;
        public uint[] Sizes;

        public DicElem(Context? context, uint count)
        {
            Offsets = new uint[count];
            Sizes = new uint[count];
            Context = context;
        }

        public bool ReadOneElem(Stream io, uint index, uint baseOffset)
        {
            if (!io.ReadUInt32Number(out Offsets[index])) return false;
            if (!io.ReadUInt32Number(out Sizes[index])) return false;

            // An offset of zero has special meaning and shall be preserved
            if (Offsets[index] > 0)
                Offsets[index] += baseOffset;

            return true;
        }

        public bool WriteOneElem(Stream io, uint index)
        {
            if (!io.Write(Offsets[index])) return false;
            if (!io.Write(Sizes[index])) return false;

            return true;
        }

        public bool ReadOneChar(Stream io, uint index, out string? str)
        {
            str = null;

            // Special case for undefined strings (see ICC Votable
            // Proposal Submission, Dictionary Type and Metadata TAG Definition)
            if (Offsets[index] == 0)
                return true;

            if (io.Seek(Offsets[index], SeekOrigin.Begin) != Offsets[index]) return false;

            var numChars = Sizes[index] / sizeof(char);

            if (!io.ReadCharArray((int)numChars, out var chars)) return false;

            str = new string(chars);
            return true;
        }

        public bool WriteOneChar(Stream io, uint index, string str, uint baseOffset)
        {
            var before = (uint)io.Tell();

            Offsets[index] = before - baseOffset;

            if (String.IsNullOrEmpty(str)) {
                Sizes[index] = 0;
                Offsets[index] = 0;
                return true;
            }

            var n = str.Length;
            if (!io.Write(str.ToCharArray())) return false;

            Sizes[index] = (uint)io.Tell() - before;
            return true;
        }

        public bool ReadOneMluC(TagTypeHandler handler, Stream io, uint index, out Mlu? mlu)
        {
            mlu = null;

            // A way to get null MLUCs
            if (Offsets[index] == 0 || Sizes[index] == 0)
                return true;

            if (io.Seek(Offsets[index], SeekOrigin.Begin) != Offsets[index]) return false;

            mlu = (Mlu?)handler.Read(io, (int)Sizes[index], out _);
            return mlu is not null;
        }

        public bool WriteOneMluC(TagTypeHandler handler, Stream io, uint index, in Mlu? mlu, uint baseOffset)
        {
            // Special case for undefined strings (see ICC Votable
            // Proposal Submission, Dictionary Type and Metadata TAG Definition)
            if (mlu is null) {
                Sizes[index] = 0;
                Offsets[index] = 0;
                return true;
            }

            var before = (uint)io.Tell();
            Offsets[index] = before - baseOffset;

            if (!handler.Write(io, mlu, 1)) return false;

            Sizes[index] = (uint)io.Tell() - before;
            return true;
        }
    }

    internal struct DicArray
    {
        public DicElem Name, Value;
        public DicElem? DisplayName, DisplayValue;

        public DicArray(Context? context, uint count, uint length)
        {
            Name = new DicElem(context, count);
            Value = new DicElem(context, count);

            DisplayName = length > 16
                ? new DicElem(context, count)
                : null;

            DisplayValue = length > 24
                ? new DicElem(context, count)
                : null;
        }

        public bool ReadOffset(Stream io, uint count, uint length, uint baseOffset, ref int signedSizeOfTagPtr)
        {
            var signedSizeOfTag = signedSizeOfTagPtr;

            // Read column arrays
            for (var i = 0u; i < count; i++) {

                if (signedSizeOfTag < 4 * sizeof(uint)) return false;
                signedSizeOfTag -= 4 * sizeof(uint);

                if (!Name.ReadOneElem(io, i, baseOffset)) return false;
                if (!Value.ReadOneElem(io, i, baseOffset)) return false;

                if (length > 16) {

                    if (signedSizeOfTag < 2 * sizeof(uint)) return false;
                    signedSizeOfTag -= 2 * sizeof(uint);

                    if (!DisplayName!.Value.ReadOneElem(io, i, baseOffset)) return false;
                }

                if (length > 24) {

                    if (signedSizeOfTag < 2 * sizeof(uint)) return false;
                    signedSizeOfTag -= 2 * sizeof(uint);

                    if (!DisplayValue!.Value.ReadOneElem(io, i, baseOffset)) return false;
                }
            }

            signedSizeOfTagPtr = signedSizeOfTag;
            return true;
        }

        public bool WriteOffset(Stream io, uint count, uint length)
        {
            for (var i = 0u; i < count; i++) {

                if (!Name.WriteOneElem(io, i)) return false;
                if (!Value.WriteOneElem(io, i)) return false;

                if (length > 16 &&
                    !DisplayName!.Value.WriteOneElem(io, i))

                    return false;

                if (length > 24 &&
                    !DisplayValue!.Value.WriteOneElem(io, i))

                    return false;
            }

            return true;
        }
    }
}
