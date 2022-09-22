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
using lcms2.io;
using lcms2.plugins;

namespace lcms2.types.type_handlers;

public class MluHandler : TagTypeHandler
{
    #region Public Constructors

    public MluHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public MluHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

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
            Errors.NotSupportedMluLength(StateContainer);
            return null;
        }

        Mlu mlu = new(StateContainer);

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
        }
        else
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

    #endregion Public Methods
}
