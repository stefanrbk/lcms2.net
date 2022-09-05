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
namespace lcms2.types;

public class Mlu : ICloneable, IDisposable
{
    #region Fields

    internal const string noCountry = "\0\0";

    internal const string noLanguage = "\0\0";

    internal List<MluEntry> entries = new();
    internal byte[] memPool = Array.Empty<byte>();
    internal uint poolSize;
    internal uint poolUsed;
    internal object? state;
    private bool _disposed;

    #endregion Fields

    #region Internal Constructors

    internal Mlu(object? state) =>
        this.state = state;

    #endregion Internal Constructors

    #region Properties

    internal uint UsedEntries => (uint)entries.Count;

    #endregion Properties

    #region Public Methods

    public object Clone()
    {
        var mlu = this;
        Mlu newMlu = new(state);

        newMlu.entries.AddRange(mlu.entries);

        // The MLU may be empty
        if (mlu.poolUsed != 0)
            newMlu.memPool = new byte[mlu.poolUsed];

        newMlu.poolSize = mlu.poolUsed;

        Buffer.BlockCopy(mlu.memPool, 0, newMlu.memPool, 0, (int)mlu.poolUsed);
        newMlu.poolUsed = mlu.poolUsed;

        return newMlu;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion Public Methods

    #region Internal Methods

    internal static Mlu? Duplicate(Mlu? mlu)
    {
        if (mlu is null) return null;

        return (Mlu)mlu.Clone();
    }

    internal bool AddBlock(uint size, char[] block, ushort languangeCode, ushort countryCode)
    {
        // Only one ASCII string
        if (SearchEntry(languangeCode, countryCode) >= 0) return false; // Only one is allowed

        // Check for size
        while ((poolSize - poolUsed) < size)
            if (!GrowPool()) return false;

        var offset = poolUsed;

        var ptr = memPool;
        Buffer.BlockCopy(block, 0, ptr, (int)offset, (int)size);

        poolUsed += size;

        var entry = new MluEntry()
        {
            OffsetToStr = offset,
            Len = size,
            Country = countryCode,
            Language = languangeCode
        };
        entries.Add(entry);

        return true;
    }

    internal uint GetAscii(string languageCode, string countryCode)
    {
        byte[]? nullBuff = null;

        return GetAscii(languageCode, countryCode, ref nullBuff);
    }

    internal uint GetAscii(string languageCode, string countryCode, ref byte[]? buffer)
    {
        var lang = StrTo16(languageCode);
        var cntry = StrTo16(countryCode);

        var wide = GetUtf16(lang, cntry, out var strLen, out _, out _);

        var asciiLen = strLen / sizeof(char);

        // Maybe we want only to know the len?
        if (buffer is null) return asciiLen + 1; // Note the zero at the end

        // No buffer means no data
        if (buffer.Length == 0) return 0;

        // Some clipping may be required
        if (buffer.Length < asciiLen + 1)
            asciiLen = (uint)(buffer.Length - 1);

        // Process each character
        for (var i = 0; i < asciiLen; i++)
        {
            buffer[i] = wide[i] == 0
                ? (byte)0
                : (byte)wide[i];
        }

        // We put a termination "\0"
        buffer[asciiLen] = 0;
        return asciiLen + 1;
    }

    internal uint GetUtf16(string languageCode, string countryCode, ref char[]? buffer)
    {
        var lang = StrTo16(languageCode);
        var cntry = StrTo16(countryCode);

        var wide = GetUtf16(lang, cntry, out var strLen, out _, out _);

        // Maybe we want only to know the len?
        if (buffer is null) return strLen + sizeof(char);

        // No buffer size means no data
        if (buffer.Length == 0) return 0;

        // Some clipping may be required
        if (buffer.Length < strLen + sizeof(char))
            strLen = (uint)(buffer.Length - sizeof(char));

        Buffer.BlockCopy(wide, 0, buffer, 0, (int)strLen);
        buffer[strLen / sizeof(char)] = (char)0;

        return strLen + sizeof(char);
    }

    internal bool GrowPool()
    {
        var size = poolSize == 0
            ? 256
            : poolSize * 2;

        // Check for overflow
        if (size < poolSize) return false;

        // Reallocate the pool
        var newPool = new byte[size];
        Buffer.BlockCopy(memPool, 0, newPool, 0, memPool.Length);

        memPool = newPool;
        poolSize = size;

        return true;
    }

    internal int SearchEntry(ushort languageCode, ushort countryCode)
    {
        // Iterate whole table
        for (var i = 0; i < entries.Count; i++)
            if (entries[i].Country == countryCode && entries[i].Language == languageCode) return i;

        // Not found
        return -1;
    }

    internal bool SetAscii(string languageCode, string countryCode, byte[] asciiString)
    {
        var len = asciiString.Length;
        var lang = StrTo16(languageCode);
        var cntry = StrTo16(countryCode);

        // len == 0 would prevent operation, so we set a empty string pointing to zero
        if (len == 0)
            len = 1;

        var wStr = new char[len];

        for (var i = 0; i < len; i++)
            wStr[i] = (char)asciiString[i];

        return AddBlock((uint)(len * sizeof(char)), wStr, lang, cntry);
    }

    internal bool SetUtf16(string languageCode, string countryCode, string str)
    {
        var lang = StrTo16(languageCode);
        var cntry = StrTo16(countryCode);

        var len = str.Length * sizeof(char);
        if (len == 0)
            len = sizeof(char);

        return AddBlock((uint)len, str.ToCharArray(), lang, cntry);
    }

    #endregion Internal Methods

    #region Protected Methods

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            entries = null!;
            memPool = null!;

            _disposed = true;
        }
    }

    #endregion Protected Methods

    #region Private Methods

    private static string StrFrom16(ushort n)
    {
        var str = new char[3];
        str[0] = (char)(n >> 8);
        str[1] = (char)(n & 0xff);
        str[2] = (char)0;

        return new string(str);
    }

    private static ushort StrTo16(string str)
    {
        if (str.Length < 2) return 0;

        return (ushort)((str[0] << 8) | str[1]);
    }

    private char[] GetUtf16(ushort languageCode, ushort countryCode, out uint len, out ushort usedLanguageCode, out ushort usedCountryCode)
    {
        int best = -1;
        MluEntry v;
        char[] result;

        for (var i = 0; i < entries.Count; i++)
        {
            v = entries[i];

            if (v.Language == languageCode)
            {
                if (best == -1) best = i;
                if (v.Country == countryCode)
                {
                    usedLanguageCode = v.Language;
                    usedCountryCode = v.Country;

                    len = v.Len;

                    result = new char[len / sizeof(char)];
                    Buffer.BlockCopy(memPool, (int)v.OffsetToStr, result, 0, (int)len);

                    // Found exact match
                    return result;
                }
            }
        }

        // No string found. Return first one
        if (best == -1)
            best = 0;

        v = entries[best];
        usedLanguageCode = v.Language;
        usedCountryCode = v.Country;

        len = v.Len;

        result = new char[len / sizeof(char)];
        Buffer.BlockCopy(memPool, (int)v.OffsetToStr, result, 0, (int)len);

        // Found exact match
        return result;
    }

    #endregion Private Methods
}

internal struct MluEntry
{
    #region Fields

    public ushort Country;
    public ushort Language;
    public uint Len;
    public uint OffsetToStr;

    #endregion Fields
}
