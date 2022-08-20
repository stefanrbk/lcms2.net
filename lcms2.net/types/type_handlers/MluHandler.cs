using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;

public class MluHandler: TagTypeHandler
{
    public MluHandler(Signature sig, Context? context = null)
        : base(sig, context, 0) { }

    public MluHandler(Context? context = null)
        : this(default, context) { }

    public override object? Duplicate(object value, int num) =>
        (value as Mlu)?.Clone();

    public override void Free(object value) =>
        (value as Mlu)?.Dispose();

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        byte[] buf;
        char[]? block;
        uint numOfChar;

        numItems = 0;
        if (!io.ReadUInt32Number(out var count)) return null;
        if (!io.ReadUInt32Number(out var recLen)) return null;

        if (recLen != 12)
        {
            Context.SignalError(Context, ErrorCode.UnknownExtension, "multiLocalizedUnicodeType of len != 12 is not supported.");
            return null;
        }

        Mlu mlu = new(Context);

        var sizeOfHeader = (12 * count) + sizeof(TagBase);
        var largestPosition = (long)0;

        for (var i = 0; i < count; i++)
        {
            if (!io.ReadUInt16Number(out var lang)) goto Error;
            if (!io.ReadUInt16Number(out var cntry)) goto Error;

            // Now deal with len and offset.
            if (!io.ReadUInt32Number(out var len)) goto Error;
            if (!io.ReadUInt32Number(out var offset)) goto Error;

            // Check for overflow
            if (offset < (sizeOfHeader + 8)) goto Error;
            if (((offset + len) < len) || ((offset + len) > sizeOfTag + 8)) goto Error;

            // True begin of the string
            var beginOfThisString = offset - sizeOfHeader - 8;

            // To guess maximum size, add offset + len
            var endOfThisString = beginOfThisString + len;
            if (endOfThisString > largestPosition)
                largestPosition = endOfThisString;

            // Save this info into the mlu
            mlu.entries.Add(new()
            {
                Language = lang,
                Country = cntry,
                Len = len,
                OffsetToStr = offset,
            });
        }

        // Now read the remaining of tag and fill all strings. Subtract the directory
        sizeOfTag = (int)largestPosition;
        if (sizeOfTag == 0)
        {
            block = null;
            numOfChar = 0;
            buf = Array.Empty<byte>();
        } else
        {
            numOfChar = (uint)(sizeOfTag / sizeof(char));
            if (!io.ReadCharArray((int)numOfChar, out block)) goto Error;
            buf = new byte[sizeOfTag];
            Buffer.BlockCopy(block, 0, buf, 0, sizeOfTag);
        }

        mlu.memPool = buf;
        mlu.poolSize = (uint)sizeOfTag;
        mlu.poolUsed = (uint)sizeOfTag;

        numItems = 1;
        return mlu;

    Error:
        if (mlu is not null)
            mlu.Dispose();

        return null;
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        if (value is null)
        {
            // Empty placeholder
            if (!io.Write((uint)0)) return false;
            if (!io.Write((uint)12)) return false;
            return true;
        }

        var mlu = (Mlu)value;

        if (!io.Write(mlu.UsedEntries)) return false;
        if (!io.Write((uint)12)) return false;

        var headerSize = (12 * mlu.UsedEntries) + (uint)sizeof(TagBase);

        for (var i = 0; i < mlu.UsedEntries; i++)
        {
            var len = mlu.entries[i].Len;
            var offset = mlu.entries[i].OffsetToStr;

            offset += headerSize + 8;

            if (!io.Write(mlu.entries[i].Language)) return false;
            if (!io.Write(mlu.entries[i].Country)) return false;
            if (!io.Write(len)) return false;
            if (!io.Write(offset)) return false;
        }
        var buf = new char[mlu.poolUsed / sizeof(char)];
        Buffer.BlockCopy(mlu.memPool, 0, buf, 0, (int)mlu.poolUsed);

        return io.Write(buf);
    }
}
