using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class TextDescriptionHandler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion { get; }

    public object? Duplicate(object value, int num) =>
        ((Mlu)value).Clone();

    public void Free(object value) =>
        ((Mlu)value).Dispose();

    public object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        // One dword should be there
        if (sizeOfTag < sizeof(uint)) return null;

        // Read len of ASCII
        if (!io.ReadUInt32Number(out var asciiCount)) return null;
        sizeOfTag -= sizeof(uint);

        // Check for size
        if (sizeOfTag < asciiCount) return null;

        // All seems Ok, create the container
        Mlu mlu = new(Context);

        // As many memory as size of tag
        var text = new byte[asciiCount + 1];

        // Read it
        if (io.Read(text, 0, (int)asciiCount) != asciiCount) goto Error;
        sizeOfTag -= (int)asciiCount;

        // Make sure there is a terminator
        text[asciiCount] = 0;

        // Set the MLU entry. From here we can be tolerant to wrong types
        if (!mlu.SetAscii(Mlu.NoLanguage, Mlu.NoCountry, text)) goto Error;
        text = null;

        // Skip Unicode code
        if (sizeOfTag < 2 * sizeof(uint)) goto Done;
        if (!io.ReadUInt32Number(out _)) goto Done;
        if (!io.ReadUInt32Number(out var unicodeCount)) goto Done;
        sizeOfTag -= 2 * sizeof(uint);

        if (sizeOfTag < unicodeCount * sizeof(ushort)) goto Done;

        var dummy = new byte[sizeof(ushort)];
        for (var i = 0; i < unicodeCount; i++)
            if (io.Read(dummy, 0, sizeof(ushort)) != sizeof(ushort)) goto Done;
        sizeOfTag -= (int)unicodeCount * sizeof(ushort);

        // Skip ScriptCode code if present. Some buggy profiles does nave less
        // data that is strictly required. We need to skip it as this type may come
        // embedded in other types.

        if (sizeOfTag >= sizeof(ushort) + sizeof(byte) + 67) {
            if (!io.ReadUInt16Number(out _)) goto Done;
            if (!io.ReadUInt8Number(out _)) goto Done;

            // Skip rest of tag
            for (var i = 0; i < 67; i++)
                if (io.Read(dummy, 0, sizeof(byte)) != sizeof(byte)) goto Error;
        }

    Done:
        numItems = 1;
        return mlu;

    Error:
        mlu.Dispose();
        return null;
    }
    public bool Write(Stream io, object value, int numItems)
    {
        byte[]? text;
        char[]? wide;
        var result = false;

        var mlu = (Mlu)value;
        var filler = new byte[68]; // Used for writing zeros
        byte[]? nullArray = null;

        // Get the len of string
        var len = mlu.GetAscii(Mlu.NoLanguage, Mlu.NoCountry, ref nullArray);

        // Specification ICC.1:2001-04 (v2.4.0): It has been found that textDescriptionType can contain misaligned data
        //(see clause 4.1 for the definition of 'aligned'). Because the Unicode language
        // code and Unicode count immediately follow the ASCII description, their
        // alignment is not correct if the ASCII count is not a multiple of four. The
        // ScriptCode code is misaligned when the ASCII count is odd. Profile reading and
        // writing software must be written carefully in order to handle these alignment
        // problems.
        //
        // The above last sentence suggest to handle alignment issues in the
        // parser. The provided example (Table 69 on Page 60) makes this clear. 
        // The padding only in the ASCII count is not sufficient for a aligned tag
        // size, with the same text size in ASCII and Unicode.

        // Null strings
        if (len <= 0) {
            text = new byte[1];
            wide = new char[1];
        } else {
            // Create independent buffers
            text = new byte[len];
            wide = new char[len];

            // Get both representations
            mlu.GetAscii(Mlu.NoLanguage, Mlu.NoCountry, ref text);
            mlu.GetUtf16(Mlu.NoLanguage, Mlu.NoCountry, ref wide);
        }

        // Tell the real text len including the null terminator and padding
        var lenText = text!.Length + 1;
        // Compute a total tag size requirement
        var lenTagRequirement = 8 + 4 + lenText + 4 + 4 + (2 * lenText) + 2 + 1 + 67;
        var lenAligned = (uint)IOHandler.AlignLong(lenTagRequirement);

        // * uint          count;          * Description length
        // * sbyte         desc[count]     * NULL terminated ascii string
        // * uint          ucLangCode;     * UniCode language code
        // * uint          ucCount;        * UniCode description length
        // * short         ucDesc[ucCount];* The UniCode description
        // * ushort        scCode;         * ScriptCode code
        // * byte          scCount;        * ScriptCode count
        // * sbyte         scDesc[67];     * ScriptCode Description

        if (!io.Write(lenText)) goto Error;
        // BUG? lenText might be longer than text.Length
        io.Write(text, 0, lenText);

        if (!io.Write((uint)0)) goto Error; // ucLanguageCode

        if (!io.Write(lenText)) goto Error;
        // BUG? lenText might be longer than wide.Length
        if (!io.Write(wide![..lenText])) goto Error;

        // ScriptCode Code & count (unused)
        if (!io.Write((ushort)0)) goto Error;
        if (!io.Write((byte)0)) goto Error;

        io.Write(filler, 0, 67);

        // possibly add pad at the end of tag
        if (lenAligned - lenTagRequirement > 0)
            io.Write(filler, 0, (int)lenAligned - lenTagRequirement);

        result = true;

    Error:
        if (text is not null)
            text = null;
        if (wide is not null)
            wide = null;

        return result;
    }
}
