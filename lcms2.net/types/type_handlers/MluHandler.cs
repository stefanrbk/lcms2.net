using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class MluHandler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion => 0;

    public object? Duplicate(object value, int num) => throw new NotImplementedException();
    public void Free(object value) => throw new NotImplementedException();
    public object? Read(Stream io, int sizeOfTag, out int numItems)
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
        
        unsafe
        {
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
                mlu.Entries.Add(new()
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

            mlu.MemPool = buf;
            mlu.PoolSize = (uint)sizeOfTag;
            mlu.PoolUsed = (uint)sizeOfTag;

            numItems = 1;
            return mlu;
        }

    Error:
        if (mlu is not null)
            mlu.Dispose();

        return null;
    }

    public bool Write(Stream io, object value, int numItems) => throw new NotImplementedException();
}
