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

    public const string noCountry = "\0\0";

    public const string noLanguage = "\0\0";

    internal List<MluEntry> entries;
    internal byte[] memPool = Array.Empty<byte>();
    internal uint poolSize;
    internal uint poolUsed;
    internal object? state;
    private bool _disposed;

    #endregion Fields

    #region Public Constructors

    public Mlu(object? state, int numItems = 2)
    {
        /** Original Code (cmsnamed.c line: 32)
         **
         ** // Allocates an empty multi localizad unicode object
         ** cmsMLU* CMSEXPORT cmsMLUalloc(cmsContext ContextID, cmsUInt32Number nItems)
         ** {
         **     cmsMLU* mlu;
         **
         **     // nItems should be positive if given
         **     if (nItems <= 0) nItems = 2;
         **
         **     // Create the container
         **     mlu = (cmsMLU*) _cmsMallocZero(ContextID, sizeof(cmsMLU));
         **     if (mlu == NULL) return NULL;
         **
         **     mlu ->ContextID = ContextID;
         **
         **     // Create entry array
         **     mlu ->Entries = (_cmsMLUentry*) _cmsCalloc(ContextID, nItems, sizeof(_cmsMLUentry));
         **     if (mlu ->Entries == NULL) {
         **         _cmsFree(ContextID, mlu);
         **         return NULL;
         **     }
         **
         **     // Ok, keep indexes up to date
         **     mlu ->AllocatedEntries    = nItems;
         **     mlu ->UsedEntries         = 0;
         **
         **     return mlu;
         ** }
         **/

        // numItems should be positive if given
        if (numItems <= 0)
            numItems = 2;

        this.state = state;
        entries = new List<MluEntry>(numItems);
    }

    #endregion Public Constructors

    #region Properties

    public uint TranslationsCount =>
        /** Original Code (cmsnamed.c line: 482)
         **
         ** // Get the number of translations in the MLU object
         ** cmsUInt32Number CMSEXPORT cmsMLUtranslationsCount(const cmsMLU* mlu)
         ** {
         **     if (mlu == NULL) return 0;
         **     return mlu->UsedEntries;
         ** }
         **/
        (uint)entries.Count;

    #endregion Properties

    #region Public Methods

    public static Mlu? Duplicate(Mlu? mlu)
    {
        if (mlu is null) return null;

        return (Mlu)mlu.Clone();
    }

    public object Clone()
    {
        /** Original Code (cmsnamed.c line: 267)
         **
         ** // Duplicating a MLU is as easy as copying all members
         ** cmsMLU* CMSEXPORT cmsMLUdup(const cmsMLU* mlu)
         ** {
         **     cmsMLU* NewMlu = NULL;
         **
         **     // Duplicating a NULL obtains a NULL
         **     if (mlu == NULL) return NULL;
         **
         **     NewMlu = cmsMLUalloc(mlu ->ContextID, mlu ->UsedEntries);
         **     if (NewMlu == NULL) return NULL;
         **
         **     // Should never happen
         **     if (NewMlu ->AllocatedEntries < mlu ->UsedEntries)
         **         goto Error;
         **
         **     // Sanitize...
         **     if (NewMlu ->Entries == NULL || mlu ->Entries == NULL)  goto Error;
         **
         **     memmove(NewMlu ->Entries, mlu ->Entries, mlu ->UsedEntries * sizeof(_cmsMLUentry));
         **     NewMlu ->UsedEntries = mlu ->UsedEntries;
         **
         **     // The MLU may be empty
         **     if (mlu ->PoolUsed == 0) {
         **         NewMlu ->MemPool = NULL;
         **     }
         **     else {
         **         // It is not empty
         **         NewMlu ->MemPool = _cmsMalloc(mlu ->ContextID, mlu ->PoolUsed);
         **         if (NewMlu ->MemPool == NULL) goto Error;
         **     }
         **
         **     NewMlu ->PoolSize = mlu ->PoolUsed;
         **
         **     if (NewMlu ->MemPool == NULL || mlu ->MemPool == NULL) goto Error;
         **
         **     memmove(NewMlu ->MemPool, mlu->MemPool, mlu ->PoolUsed);
         **     NewMlu ->PoolUsed = mlu ->PoolUsed;
         **
         **     return NewMlu;
         **
         ** Error:
         **
         **     if (NewMlu != NULL) cmsMLUfree(NewMlu);
         **     return NULL;
         ** }
         **/

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

    public uint GetAscii(string languageCode, string countryCode, Span<byte> buffer)
    {
        /** Original Code (cmsnamed.c line: 377)
         **
         ** // Obtain an ASCII representation of the wide string. Setting buffer to NULL returns the len
         ** cmsUInt32Number CMSEXPORT cmsMLUgetASCII(const cmsMLU* mlu,
         **                                        const char LanguageCode[3], const char CountryCode[3],
         **                                        char* Buffer, cmsUInt32Number BufferSize)
         ** {
         **     const wchar_t *Wide;
         **     cmsUInt32Number  StrLen = 0;
         **     cmsUInt32Number ASCIIlen, i;
         **
         **     cmsUInt16Number Lang  = strTo16(LanguageCode);
         **     cmsUInt16Number Cntry = strTo16(CountryCode);
         **
         **     // Sanitize
         **     if (mlu == NULL) return 0;
         **
         **     // Get WideChar
         **     Wide = _cmsMLUgetWide(mlu, &StrLen, Lang, Cntry, NULL, NULL);
         **     if (Wide == NULL) return 0;
         **
         **     ASCIIlen = StrLen / sizeof(wchar_t);
         **
         **     // Maybe we want only to know the len?
         **     if (Buffer == NULL) return ASCIIlen + 1; // Note the zero at the end
         **
         **     // No buffer size means no data
         **     if (BufferSize <= 0) return 0;
         **
         **     // Some clipping may be required
         **     if (BufferSize < ASCIIlen + 1)
         **         ASCIIlen = BufferSize - 1;
         **
         **     // Precess each character
         **     for (i=0; i < ASCIIlen; i++) {
         **
         **         if (Wide[i] == 0)
         **             Buffer[i] = 0;
         **         else
         **             Buffer[i] = (char) Wide[i];
         **     }
         **
         **     // We put a termination "\0"
         **     Buffer[ASCIIlen] = 0;
         **     return ASCIIlen + 1;
         ** }
         **/

        var lang = StrTo16(languageCode);
        var cntry = StrTo16(countryCode);

        var wide = GetUtf16(lang, cntry, out var strLen, out _, out _);
        if (wide is null) return 0;

        var asciiLen = strLen / sizeof(char);

        // Maybe we want only to know the len?
        if (buffer == default) return asciiLen + 1; // Note the zero at the end

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
        buffer[(int)asciiLen] = 0;
        return asciiLen + 1;
    }

    public bool GetTranslation(string requestedLang,
                               string requestedCode,
                               out string obtainedLang,
                               out string obtainedCode)
    {
        /** Original Code (cmsnamed.c line: 456)
         **
         ** // Get also the language and country
         ** CMSAPI cmsBool CMSEXPORT cmsMLUgetTranslation(const cmsMLU* mlu,
         **                                               const char LanguageCode[3], const char CountryCode[3],
         **                                               char ObtainedLanguage[3], char ObtainedCountry[3])
         ** {
         **     const wchar_t *Wide;
         **
         **     cmsUInt16Number Lang  = strTo16(LanguageCode);
         **     cmsUInt16Number Cntry = strTo16(CountryCode);
         **     cmsUInt16Number ObtLang, ObtCode;
         **
         **     // Sanitize
         **     if (mlu == NULL) return FALSE;
         **
         **     Wide = _cmsMLUgetWide(mlu, NULL, Lang, Cntry, &ObtLang, &ObtCode);
         **     if (Wide == NULL) return FALSE;
         **
         **     // Get used language and code
         **     strFrom16(ObtainedLanguage, ObtLang);
         **     strFrom16(ObtainedCountry, ObtCode);
         **
         **     return TRUE;
         ** }
         **/

        var lang = StrTo16(requestedLang);
        var cntry = StrTo16(requestedCode);

        var wide = GetUtf16(lang, cntry, out _, out var obtLang, out var obtCode);
        if (wide is null)
        {
            obtainedLang = "";
            obtainedCode = "";
            return false;
        }

        // Get used language and code
        obtainedLang = StrFrom16(obtLang);
        obtainedCode = StrFrom16(obtCode);

        return true;
    }

    public uint GetUtf16(string languageCode, string countryCode, Span<char> buffer)
    {
        /** Original Code (cmsnamed.c line: 422)
         **
         ** // Obtain a wide representation of the MLU, on depending on current locale settings
         ** cmsUInt32Number CMSEXPORT cmsMLUgetWide(const cmsMLU* mlu,
         **                                       const char LanguageCode[3], const char CountryCode[3],
         **                                       wchar_t* Buffer, cmsUInt32Number BufferSize)
         ** {
         **     const wchar_t *Wide;
         **     cmsUInt32Number  StrLen = 0;
         **
         **     cmsUInt16Number Lang  = strTo16(LanguageCode);
         **     cmsUInt16Number Cntry = strTo16(CountryCode);
         **
         **     // Sanitize
         **     if (mlu == NULL) return 0;
         **
         **     Wide = _cmsMLUgetWide(mlu, &StrLen, Lang, Cntry, NULL, NULL);
         **     if (Wide == NULL) return 0;
         **
         **     // Maybe we want only to know the len?
         **     if (Buffer == NULL) return StrLen + sizeof(wchar_t);
         **
         **   // No buffer size means no data
         **     if (BufferSize <= 0) return 0;
         **
         **     // Some clipping may be required
         **     if (BufferSize < StrLen + sizeof(wchar_t))
         **         StrLen = BufferSize - + sizeof(wchar_t);
         **
         **     memmove(Buffer, Wide, StrLen);
         **     Buffer[StrLen / sizeof(wchar_t)] = 0;
         **
         **     return StrLen + sizeof(wchar_t);
         ** }
         **/

        var lang = StrTo16(languageCode);
        var cntry = StrTo16(countryCode);

        var wide = GetUtf16(lang, cntry, out var strLen, out _, out _);
        if (wide is null) return 0;

        // Maybe we want only to know the len?
        if (buffer == default) return strLen + sizeof(char);

        // No buffer size means no data
        if (buffer.Length == 0) return 0;

        // Some clipping may be required
        if (buffer.Length < strLen + sizeof(char))
            strLen = (uint)(buffer.Length - sizeof(char));

        wide.CopyTo(buffer);
        buffer[(int)strLen / sizeof(char)] = (char)0;

        return strLen + sizeof(char);
    }

    public bool SetAscii(string languageCode, string countryCode, byte[] asciiString)
    {
        /** Original Code (cmsnamed.c line: 206)
         **
         ** // Add an ASCII entry. Do not add any \0 termination (ICC1v43_2010-12.pdf page 61)
         ** // In the case the user explicitly sets an empty string, we force a \0
         ** cmsBool CMSEXPORT cmsMLUsetASCII(cmsMLU* mlu, const char LanguageCode[3], const char CountryCode[3], const char* ASCIIString)
         ** {
         **     cmsUInt32Number i, len = (cmsUInt32Number) strlen(ASCIIString);
         **     wchar_t* WStr;
         **     cmsBool  rc;
         **     cmsUInt16Number Lang  = strTo16(LanguageCode);
         **     cmsUInt16Number Cntry = strTo16(CountryCode);
         **
         **     if (mlu == NULL) return FALSE;
         **
         **     // len == 0 would prevent operation, so we set a empty string pointing to zero
         **     if (len == 0)
         **     {
         **         len = 1;
         **     }
         **
         **     WStr = (wchar_t*) _cmsCalloc(mlu ->ContextID, len,  sizeof(wchar_t));
         **     if (WStr == NULL) return FALSE;
         **
         **     for (i=0; i < len; i++)
         **         WStr[i] = (wchar_t) ASCIIString[i];
         **
         **     rc = AddMLUBlock(mlu, len  * sizeof(wchar_t), WStr, Lang, Cntry);
         **
         **     _cmsFree(mlu ->ContextID, WStr);
         **     return rc;
         **
         ** }
         **/

        var len = 0;
        foreach (var a in asciiString)
        {
            if (a is 0)
                break;
            len++;
        }
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

    public bool SetUtf16(string languageCode, string countryCode, string str)
    {
        /** Original Code (cmsnamed.c line: 250)
         **
         ** // Add a wide entry. Do not add any \0 terminator (ICC1v43_2010-12.pdf page 61)
         ** cmsBool  CMSEXPORT cmsMLUsetWide(cmsMLU* mlu, const char Language[3], const char Country[3], const wchar_t* WideString)
         ** {
         **     cmsUInt16Number Lang  = strTo16(Language);
         **     cmsUInt16Number Cntry = strTo16(Country);
         **     cmsUInt32Number len;
         **
         **     if (mlu == NULL) return FALSE;
         **     if (WideString == NULL) return FALSE;
         **
         **     len = (cmsUInt32Number) (mywcslen(WideString)) * sizeof(wchar_t);
         **     if (len == 0)
         **         len = sizeof(wchar_t);
         **
         **     return AddMLUBlock(mlu, len, WideString, Lang, Cntry);
         ** }
         **/

        var lang = StrTo16(languageCode);
        var cntry = StrTo16(countryCode);

        var len = str.Length * sizeof(char);
        if (len == 0)
        {
            len = sizeof(char);
            str = "\0";
        }

        return AddBlock((uint)len, str.ToCharArray(), lang, cntry);
    }

    public bool TranslationsCodes(int index, out string langCode, out string cntryCode)
    {
        /** Original Code (cmsnamed.c line: 489)
         **
         ** // Get the language and country codes for a specific MLU index
         ** cmsBool CMSEXPORT cmsMLUtranslationsCodes(const cmsMLU* mlu,
         **                                           cmsUInt32Number idx,
         **                                           char LanguageCode[3],
         **                                           char CountryCode[3])
         ** {
         **     _cmsMLUentry *entry;
         **
         **     if (mlu == NULL) return FALSE;
         **
         **     if (idx >= mlu->UsedEntries) return FALSE;
         **
         **     entry = &mlu->Entries[idx];
         **
         **     strFrom16(LanguageCode, entry->Language);
         **     strFrom16(CountryCode, entry->Country);
         **
         **     return TRUE;
         ** }
         **/

        if (index >= TranslationsCount)
        {
            langCode = "";
            cntryCode = "";
            return false;
        }

        var entry = entries[index];
        langCode = StrFrom16(entry.Language);
        cntryCode = StrFrom16(entry.Country);

        return true;
    }

    #endregion Public Methods

    #region Internal Methods

    internal bool AddBlock(uint size, char[] block, ushort languangeCode, ushort countryCode)
    {
        /** Original Code (cmsnamed.c line: 137)
         **
         ** // Add a block of characters to the intended MLU. Language and country are specified.
         ** // Only one entry for Language/country pair is allowed.
         ** static
         ** cmsBool AddMLUBlock(cmsMLU* mlu, cmsUInt32Number size, const wchar_t *Block,
         **                      cmsUInt16Number LanguageCode, cmsUInt16Number CountryCode)
         ** {
         **     cmsUInt32Number Offset;
         **     cmsUInt8Number* Ptr;
         **
         **     // Sanity check
         **     if (mlu == NULL) return FALSE;
         **
         **     // Is there any room available?
         **     if (mlu ->UsedEntries >= mlu ->AllocatedEntries) {
         **         if (!GrowMLUtable(mlu)) return FALSE;
         **     }
         **
         **     // Only one ASCII string
         **     if (SearchMLUEntry(mlu, LanguageCode, CountryCode) >= 0) return FALSE;  // Only one  is allowed!
         **
         **     // Check for size
         **     while ((mlu ->PoolSize - mlu ->PoolUsed) < size) {
         **
         **             if (!GrowMLUpool(mlu)) return FALSE;
         **     }
         **
         **     Offset = mlu ->PoolUsed;
         **
         **     Ptr = (cmsUInt8Number*) mlu ->MemPool;
         **     if (Ptr == NULL) return FALSE;
         **
         **     // Set the entry
         **     memmove(Ptr + Offset, Block, size);
         **     mlu ->PoolUsed += size;
         **
         **     mlu ->Entries[mlu ->UsedEntries].StrW     = Offset;
         **     mlu ->Entries[mlu ->UsedEntries].Len      = size;
         **     mlu ->Entries[mlu ->UsedEntries].Country  = CountryCode;
         **     mlu ->Entries[mlu ->UsedEntries].Language = LanguageCode;
         **     mlu ->UsedEntries++;
         **
         **     return TRUE;
         ** }
         **/

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

    internal bool GrowPool()
    {
        /** Original Code (cmsnamed.c line: 61)
         **
         ** // Grows a mempool table for a MLU. Each time this function is called, mempool size is multiplied times two.
         ** static
         ** cmsBool GrowMLUpool(cmsMLU* mlu)
         ** {
         **     cmsUInt32Number size;
         **     void *NewPtr;
         **
         **     // Sanity check
         **     if (mlu == NULL) return FALSE;
         **
         **     if (mlu ->PoolSize == 0)
         **         size = 256;
         **     else
         **         size = mlu ->PoolSize * 2;
         **
         **     // Check for overflow
         **     if (size < mlu ->PoolSize) return FALSE;
         **
         **     // Reallocate the pool
         **     NewPtr = _cmsRealloc(mlu ->ContextID, mlu ->MemPool, size);
         **     if (NewPtr == NULL) return FALSE;
         **
         **
         **     mlu ->MemPool  = NewPtr;
         **     mlu ->PoolSize = size;
         **
         **     return TRUE;
         ** }
         **/

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
        /** Original Code (cmsnamed.c line: 117)
         **
         ** // Search for a specific entry in the structure. Language and Country are used.
         ** static
         ** int SearchMLUEntry(cmsMLU* mlu, cmsUInt16Number LanguageCode, cmsUInt16Number CountryCode)
         ** {
         **     cmsUInt32Number i;
         **
         **     // Sanity check
         **     if (mlu == NULL) return -1;
         **
         **     // Iterate whole table
         **     for (i=0; i < mlu ->UsedEntries; i++) {
         **
         **         if (mlu ->Entries[i].Country  == CountryCode &&
         **             mlu ->Entries[i].Language == LanguageCode) return (int) i;
         **     }
         **
         **     // Not found
         **     return -1;
         ** }
         **/

        // Iterate whole table
        for (var i = 0; i < entries.Count; i++)
            if (entries[i].Country == countryCode && entries[i].Language == languageCode) return i;

        // Not found
        return -1;
    }

    #endregion Internal Methods

    #region Protected Methods

    protected virtual void Dispose(bool disposing)
    {
        /** Original Code (cmsnamed.c line: 313)
         **
         ** // Free any used memory
         ** void CMSEXPORT cmsMLUfree(cmsMLU* mlu)
         ** {
         **     if (mlu) {
         **
         **         if (mlu -> Entries) _cmsFree(mlu ->ContextID, mlu->Entries);
         **         if (mlu -> MemPool) _cmsFree(mlu ->ContextID, mlu->MemPool);
         **
         **         _cmsFree(mlu ->ContextID, mlu);
         **     }
         ** }
         **/

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
        /** Original Code (cmsnamed.c line: 197)
         **
         ** static
         ** void strFrom16(char str[3], cmsUInt16Number n)
         ** {
         **     str[0] = (char)(n >> 8);
         **     str[1] = (char)n;
         **     str[2] = (char)0;
         **
         ** }
         **/

        var str = new char[3];
        str[0] = (char)(n >> 8);
        str[1] = (char)(n & 0xff);
        str[2] = (char)0;

        return new string(str);
    }

    private static ushort StrTo16(string str)
    {
        /** Original Code (cmsnamed.c line: 181)
         **
         ** // Convert from a 3-char code to a cmsUInt16Number. It is done in this way because some
         ** // compilers don't properly align beginning of strings
         ** static
         ** cmsUInt16Number strTo16(const char str[3])
         ** {
         **     const cmsUInt8Number* ptr8;
         **     cmsUInt16Number n;
         **
         **     // For non-existent strings
         **     if (str == NULL) return 0;
         **     ptr8 = (const cmsUInt8Number*)str;
         **     n = (cmsUInt16Number)(((cmsUInt16Number)ptr8[0] << 8) | ptr8[1]);
         **
         **     return n;
         ** }
         **/

        if (str.Length < 2) return 0;

        return (ushort)((str[0] << 8) | str[1]);
    }

    private char[]? GetUtf16(ushort languageCode, ushort countryCode, out uint len, out ushort usedLanguageCode, out ushort usedCountryCode)
    {
        /** Original Code (cmsnamed.c line: 326)
         **
         ** // The algorithm first searches for an exact match of country and language, if not found it uses
         ** // the Language. If none is found, first entry is used instead.
         ** static
         ** const wchar_t* _cmsMLUgetWide(const cmsMLU* mlu,
         **                               cmsUInt32Number *len,
         **                               cmsUInt16Number LanguageCode, cmsUInt16Number CountryCode,
         **                               cmsUInt16Number* UsedLanguageCode, cmsUInt16Number* UsedCountryCode)
         ** {
         **     cmsUInt32Number i;
         **     int Best = -1;
         **     _cmsMLUentry* v;
         **
         **     if (mlu == NULL) return NULL;
         **
         **     if (mlu -> AllocatedEntries <= 0) return NULL;
         **
         **     for (i=0; i < mlu ->UsedEntries; i++) {
         **
         **         v = mlu ->Entries + i;
         **
         **         if (v -> Language == LanguageCode) {
         **
         **             if (Best == -1) Best = (int) i;
         **
         **             if (v -> Country == CountryCode) {
         **
         **                 if (UsedLanguageCode != NULL) *UsedLanguageCode = v ->Language;
         **                 if (UsedCountryCode  != NULL) *UsedCountryCode = v ->Country;
         **
         **                 if (len != NULL) *len = v ->Len;
         **
         **                 return (wchar_t*) ((cmsUInt8Number*) mlu ->MemPool + v -> StrW);        // Found exact match
         **             }
         **         }
         **     }
         **
         **     // No string found. Return First one
         **     if (Best == -1)
         **         Best = 0;
         **
         **     v = mlu ->Entries + Best;
         **
         **     if (UsedLanguageCode != NULL) *UsedLanguageCode = v ->Language;
         **     if (UsedCountryCode  != NULL) *UsedCountryCode = v ->Country;
         **
         **     if (len != NULL) *len   = v ->Len;
         **
         **     return(wchar_t*) ((cmsUInt8Number*) mlu ->MemPool + v ->StrW);
         ** }
         **/

        int best = -1;
        MluEntry v;
        char[] result;

        if (TranslationsCount <= 0)
        {
            len = 0;
            usedLanguageCode = 0;
            usedCountryCode = 0;
            return null;
        }

        for (var i = 0; i < TranslationsCount; i++)
        {
            v = entries[i];

            if (v.Language == languageCode)
            {
                if (best == -1) best = i;
                if (v.Country == countryCode)
                    goto Found;
            }
        }

        // No string found. Return first one
        if (best == -1)
            best = 0;

        v = entries[best];
    Found:
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
