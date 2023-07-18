//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
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
using lcms2.cgats;
using lcms2.state;

using System.Runtime.CompilerServices;

using static lcms2.cgats.CGATS;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static @string* StringAlloc(IT8* it8, int max)
    {
        var s = AllocChunk<@string>(it8);

        s->it8 = it8;
        s->max = max;
        s->len = 0;
        s->begin = (byte*)AllocChunk(it8, (uint)s->max);

        return s;
    }

    private static void StringClear(@string* s) =>
        s->len = 0;

    private static void StringAppend(@string* s, byte c)
    {
        if (s->len + 1 >= s->max)
        {
            byte* new_ptr;

            s->max *= 10;
            new_ptr = (byte*)AllocChunk(s->it8, (uint)s->max);
            memcpy(new_ptr, s->begin, s->len);
            s->begin = new_ptr;
        }

        s->begin[s->len++] = c;
        s->begin[s->len] = 0;
    }

    private static byte* StringPtr(@string* s) =>
        s->begin;

    private static void StringCat(@string* s, byte* c)
    {
        while (*c is not 0)
        {
            StringAppend(s, *c);
            c++;
        }
    }

    private static bool isseparator(int c) =>
        c is ' ' or '\t';

    private static bool ismiddle(int c) =>
        !isseparator(c) && c is not '#' and not '\"' and not '\'' and > 32 and < 127;

    private static bool isidchar(int c) =>
        char.IsAsciiLetterOrDigit((char)c) || ismiddle(c);

    private static bool isfirstidchar(int c) =>
        !char.IsDigit((char)c) && ismiddle(c);

    private static bool isabsolutepath(byte* path)
    {
        var ThreeChars = stackalloc byte[4];

        if (path is null || path[0] is 0)
            return false;

        strncpy(ThreeChars, path, 3);
        ThreeChars[3] = 0;

        if (ThreeChars[0] == DIR_CHAR)
            return true;

        if (char.IsAsciiLetter((char)ThreeChars[0]) && ThreeChars[1] is (byte)':')
            return true;

        return false;
    }

    private static bool BuildAbsolutePath(byte* relPath, byte* basePath, byte* buffer, uint MaxLen)
    {
        // Already absolute?
        if (isabsolutepath(relPath))
        {
            strncpy(buffer, relPath, MaxLen);
            buffer[MaxLen - 1] = 0;
            return true;
        }

        // No, search for last
        strncpy(buffer, basePath, MaxLen);
        buffer[MaxLen - 1] = 0;

        var tail = strrchr(buffer, DIR_CHAR);
        if (tail is null) return false;

        var len = (uint)(tail - buffer);
        if (len >= MaxLen) return false;

        // No need to assure zero terminator over here
        strncpy(tail + 1, relPath, MaxLen - len);

        return true;
    }

    private static byte* NoMeta(byte* str)
    {
        if (strchr(str, '%') is not null)
            return "**** CORRUPTED FORMAT STRING ***".ToBytePtr();

        return str;
    }

    private static bool SynError(IT8* it8, byte* Txt, params object[] args)
    {
        var Buffer = stackalloc byte[256];
        var ErrMsg = stackalloc byte[1024];

        vsnprintf(Buffer, 255, Txt, args);
        Buffer[255] = 0;

        snprintf(ErrMsg, 1023, "{0}: Line {1}, {2}".ToBytePtr(), new string((sbyte*)it8->FileStack[it8->IncludeSP]->FileName), it8->lineno, new string((sbyte*)Buffer));
        ErrMsg[1023] = 0;
        it8->sy = SYMBOL.SSYNERROR;
        cmsSignalError(it8->ContextID, cmsERROR_CORRUPTION_DETECTED, new string((sbyte*)ErrMsg));
        return false;
    }

    private static bool Check(IT8* it8, SYMBOL sy, string Err)
    {
        if (it8->sy != sy)
            return SynError(it8, NoMeta(Err.ToBytePtr()));
        return true;
    }

    private static void NextCh(IT8* it8)
    {
        if (it8->FileStack[it8->IncludeSP]->Stream is not null)
        {
            it8->ch = it8->FileStack[it8->IncludeSP]->Stream!.ReadByte();

            if (it8->ch is -1)
            {
                if (it8->IncludeSP > 0)
                {
                    it8->FileStack[it8->IncludeSP]->Stream!.Close();
                    it8->ch = ' ';      // Whitespace to be ignored
                }
                else
                    it8->ch = '\0';
            }
        }
        else
        {
            it8->ch = *it8->Source;
            if (it8->ch is not 0) it8->Source++;
        }
    }

    private static SYMBOL BinSrchKey(byte* id)
    {
        int l = 1;
        int r = NUMKEYS;

        while (r >= l)
        {
            var x = (l + r) / 2;
            var res = cmsstrcasecmp(id, TabKeys[x - 1].id);
            if (res is 0) return TabKeys[x - 1].sy;
            if (res < 0) r = x - 1;
            else l = x + 1;
        }

        return SYMBOL.SUNDEFINED;
    }

    private static double xpow10(int n) =>
        Math.Pow(10, n);

    private static void ReadReal(IT8* it8, int inum)
    {
        it8->dnum = inum;

        while (Char.IsDigit((char)it8->ch))
        {
            it8->dnum = (it8->dnum * 10.0) + (it8->ch - '0');
            NextCh(it8);
        }

        if (it8->ch is '.')     // Decimal point
        {
            var frac = 0.0;     // fraction
            var prec = 0;         // precision

            NextCh(it8);

            while (Char.IsDigit((char)it8->ch))
            {
                frac = (frac * 10.0) + (it8->ch - '0');
                prec++;
                NextCh(it8);
            }

            it8->dnum += frac / xpow10(prec);
        }

        // Exponent, example 34.00E+20
        if (Char.ToUpper((char)it8->ch) is 'E')
        {
            NextCh(it8); var sgn = 1;

            if (it8->ch is '-')
            {
                sgn = -1; NextCh(it8);
            }
            else if (it8->ch is '+')
            {
                sgn = +1; NextCh(it8);
            }

            var e = 0;
            while (Char.IsDigit((char)it8->ch))
            {
                var digit = it8->ch - '0';

                if (e * 10.0 + digit < +2147483647.0)
                    e = e * 10 + digit;

                NextCh(it8);
            }

            e *= sgn;
            it8->dnum *= xpow10(e);
        }
    }

    private static double ParseFloatNumber(byte* Buffer)
    {
        var dnum = 0.0;
        var sign = 1;

        // keep safe
        if (Buffer is null) return 0.0;

        if ((char)*Buffer is '-' or '+')
        {
            sign = ((char)*Buffer is '-') ? -1 : 1;
            Buffer++;
        }

        while (*Buffer is not 0 && Char.IsDigit((char)*Buffer))
        {
            dnum = dnum * 10.0 + (*Buffer - '0');
            if (*Buffer is not 0) Buffer++;
        }

        if ((char)*Buffer is '.')
        {
            var frac = 0.0;
            var prec = 0;

            if (*Buffer is not 0) Buffer++;

            while (Char.IsDigit((char)*Buffer))
            {
                frac = (frac * 10.0) + (*Buffer - '0');
                prec++;
                if (*Buffer is not 0) Buffer++;
            }

            dnum += frac / xpow10(prec);
        }

        // Exponent, example 34.00E+20
        if (Char.ToUpper((char)*Buffer) is 'E')
        {
            if (*Buffer is not 0) Buffer++;
            var sgn = 1;

            if ((char)*Buffer is '-')
            {
                sgn = -1;
                if (*Buffer is not 0) Buffer++;
            }
            else if ((char)*Buffer is '+')
            {
                sgn = +1;
                if (*Buffer is not 0) Buffer++;
            }

            var e = 0;
            while (Char.IsDigit((char)*Buffer))
            {
                var digit = *Buffer - '0';

                if (e * 10.0 + digit < +2147483647.0)
                    e = e * 10 + digit;

                if (*Buffer is not 0) Buffer++;
            }

            e *= sgn;
            dnum *= xpow10(e);
        }

        return sign * dnum;
    }

    private static void InStringSymbol(IT8* it8)
    {
        while (isseparator(it8->ch))
            NextCh(it8);

        if (it8->ch is '\'' or '\"')
        {
            var sng = it8->ch;
            StringClear(it8->str);

            NextCh(it8);

            while (it8->ch != sng)
            {
                if (it8->ch is '\n' or '\r' or '\0') break;
                else
                {
                    StringAppend(it8->str, (byte)it8->ch);
                    NextCh(it8);
                }
            }

            it8->sy = SYMBOL.SSTRING;
            NextCh(it8);
        }
        else
            SynError(it8, "String expected".ToBytePtr());
    }

    private static void InSymbol(IT8* it8)
    {
        var buffer = stackalloc byte[127];
        SYMBOL key;

        do
        {
            while (isseparator(it8->ch))
                NextCh(it8);

            if (isfirstidchar(it8->ch))
            {
                StringClear(it8->id);

                do
                {
                    StringAppend(it8->id, (byte)it8->ch);
                    NextCh(it8);
                } while (isidchar(it8->ch));

                key = BinSrchKey(StringPtr(it8->id));
                if (key is SYMBOL.SUNDEFINED) it8->sy = SYMBOL.SIDENT;
                else it8->sy = key;
            }
            else if (Char.IsDigit((char)it8->ch) || it8->ch is '.' or '-' or '+')
            {
                var sign = 1;

                if (it8->ch is '-')
                {
                    sign = -1;
                    NextCh(it8);
                }

                it8->inum = 0;
                it8->sy = SYMBOL.SINUM;

                if (it8->ch is '0')     // 0xnnnn (Hexa) or 0bnnnn (Binary)
                {
                    NextCh(it8);
                    if (Char.ToUpper((char)it8->ch) is 'X')
                    {
                        NextCh(it8);
                        while(Char.IsAsciiHexDigit((char)it8->ch))
                        {
                            it8->ch = Char.ToUpper((char)it8->ch);
                            var j = (it8->ch is >= 'A' and <= 'F')
                                ? it8->ch - 'A' + 10
                                : it8->ch - '0';

                            if (it8->inum * 16.0 + j > +2147483647.0)
                            {
                                SynError(it8, "Invalid hexadecimal number".ToBytePtr());
                                return;
                            }

                            it8->inum = it8->inum * 16 + j;
                            NextCh(it8);
                        }
                        return;
                    }
                    else if (Char.ToUpper((char)it8->ch) is 'B')
                    {
                        NextCh(it8);
                        while (it8->ch is '0' or '1')
                        {
                            var j = it8->ch - '0';

                            if (it8->inum * 2.0 + j > +2147483647.0)
                            {
                                SynError(it8, "Invalid binary number".ToBytePtr());
                                return;
                            }

                            it8->inum = it8->inum * 2 + j;
                            NextCh(it8);
                        }
                        return;
                    }
                }

                while (Char.IsDigit((char)it8->ch))
                {
                    var digit = it8->ch - '0';

                    if (it8->inum * 10.0 + digit > +2147483647.0)
                    {
                        ReadReal(it8, it8->inum);
                        it8->sy = SYMBOL.SDNUM;
                        it8->dnum *= sign;
                        return;
                    }

                    it8->inum = it8->inum * 10 + digit;
                    NextCh(it8);
                }

                if (it8->ch is '.')
                {
                    ReadReal(it8, it8->inum);
                    it8->sy = SYMBOL.SDNUM;
                    it8->dnum *= sign;
                    return;
                }

                it8->dnum *= sign;

                // Special case. Numbers followed by letters are taken as identifiers
                if (isidchar(it8->ch))
                {
                    if (it8->sy is SYMBOL.SINUM)
                    {
                        snprintf(buffer, 127, DEFAULT_NUM_FORMAT, it8->inum);
                    }
                    else
                    {
                        snprintf(buffer, 127, it8->DoubleFormatter, it8->dnum);
                    }

                    StringCat(it8->id, buffer);

                    do
                    {
                        StringAppend(it8->id, (byte)it8->ch);
                        NextCh(it8);
                    } while (isidchar(it8->ch));

                    it8->sy = SYMBOL.SIDENT;
                }
                return;
            }
            else
            {
                switch(it8->ch)
                {
                    // EOF marker -- ignore it
                    case '\x1a':
                        NextCh(it8);
                        break;

                    // Eof stream markers
                    case 0:
                    case -1:
                        it8->sy = SYMBOL.SEOF;
                        break;

                    // Next line
                    case '\r':
                        NextCh(it8);
                        if (it8->ch is '\n')
                            NextCh(it8);
                        it8->sy = SYMBOL.SEOLN;
                        it8->lineno++;
                        break;

                    case '\n':
                        NextCh(it8);
                        it8->sy = SYMBOL.SEOLN;
                        it8->lineno++;
                        break;

                    // Comment
                    case '#':
                        NextCh(it8);
                        while (it8->ch is not '\0' and not '\n' and not '\r')
                            NextCh(it8);

                        it8->sy = SYMBOL.SCOMMENT;
                        break;

                    // String
                    case '\'':
                    case '\"':
                        InStringSymbol(it8);
                        break;

                    default:
                        SynError(it8, "Unrecognized character: 0x{0:x}".ToBytePtr(), it8->ch);
                        return;
                }
            }

        } while (it8->sy is SYMBOL.SCOMMENT);

        // Handle the include special token

        if (it8->sy is SYMBOL.SINCLUDE)
        {
            if (it8->IncludeSP >= MAXINCLUDE - 1)
            {
                SynError(it8, "Too many recursion levels".ToBytePtr());
                return;
            }

            InStringSymbol(it8);
            if (!Check(it8, SYMBOL.SSTRING, "Filename expected")) return;

            var FileNest = it8->FileStack[it8->IncludeSP + 1];
            if (FileNest is null)
            {
                FileNest = it8->FileStack[it8->IncludeSP + 1] = AllocChunk<FILECTX>(it8);
            }

            if (BuildAbsolutePath(StringPtr(it8->str), it8->FileStack[it8->IncludeSP]->FileName, FileNest->FileName, cmsMAX_PATH - 1) is false)
            {
                SynError(it8, "File path too long".ToBytePtr());
                return;
            }

            try
            {
                FileNest->Stream = File.Open(new string((sbyte*)FileNest->FileName), FileMode.Open, FileAccess.Read);
            }
            catch
            {
                FileNest->Stream = null;
            }
            if (FileNest->Stream is null)
            {
                SynError(it8, "File {0} not found".ToBytePtr(), new string((sbyte*)FileNest->FileName));
                return;
            }
            it8->IncludeSP++;

            it8->ch = ' ';
            InSymbol(it8);
        }
    }

    private static bool CheckEOLN(IT8* it8)
    {
        if (!Check(it8, SYMBOL.SEOLN, "Expected separator")) return false;
        while (it8->sy is SYMBOL.SEOLN)
            InSymbol(it8);
        return true;
    }

    private static void Skip(IT8* it8, SYMBOL sy)
    {
        if (it8->sy == sy && it8->sy is not SYMBOL.SEOF)
            InSymbol(it8);
    }

    private static void SkipEOLN(IT8* it8)
    {
        while (it8->sy is SYMBOL.SEOLN)
            InSymbol(it8);
    }

    private static bool GetVal(IT8* it8, byte* Buffer, uint max, string ErrorTitle)
    {
        switch (it8->sy)
        {
            case SYMBOL.SEOLN:
                Buffer[0] = 0;
                break;
            case SYMBOL.SIDENT:
                strncpy(Buffer, StringPtr(it8->id), max);
                Buffer[max - 1] = 0;
                break;
            case SYMBOL.SINUM:
                snprintf(Buffer, max, DEFAULT_NUM_FORMAT, it8->inum);
                break;
            case SYMBOL.SDNUM:
                snprintf(Buffer, max, it8->DoubleFormatter, it8->dnum);
                break;
            case SYMBOL.SSTRING:
                strncpy(Buffer, StringPtr(it8->str), max);
                Buffer[max-1] = 0;
                break;
            default:
                return SynError(it8, ErrorTitle.ToBytePtr());
        }

        Buffer[max] = 0;
        return true;
    }

    private static TABLE* GetTable(IT8* it8)
    {
        if (it8->nTable >= it8->TablesCount)
        {
            SynError(it8, "Table {0} out of sequence".ToBytePtr(), it8->nTable);
            return it8->Tab;
        }

        return it8->Tab + it8->nTable;
    }

    public static void cmsIT8Free(HANDLE hIT8)
    {
        var it8 = (IT8*)hIT8;

        if (it8 is null)
            return;

        if (it8->MemorySink is not null)
        {
            for (OWNEDMEM* p = it8->MemorySink, n = null; p is not null; p = n)
            {
                n = p->Next;
                if (p->Ptr is not null) _cmsFree(it8->ContextID, p->Ptr);
                _cmsFree(it8->ContextID, p);
            }
        }

        if (it8->MemoryBlock is not null)
            _cmsFree(it8->ContextID, it8->MemoryBlock);

        _cmsFree(it8->ContextID, it8);
    }

    private static void* AllocBigBlock(IT8* it8, uint size)
    {
        var ptr = _cmsMallocZero(it8->ContextID, size, typeof(byte));

        if (ptr is not null)
        {
            var ptr1 = _cmsMallocZero<OWNEDMEM>(it8->ContextID);

            if (ptr1 is null)
            {
                _cmsFree(it8->ContextID, ptr);
                return null;
            }

            ptr1->Ptr = ptr;
            ptr1->Next = it8->MemorySink;
            it8->MemorySink = ptr1;
        }

        return ptr;
    }

    private static T** AllocChunk2<T>(IT8* it8, uint count) where T : struct =>
        (T**)AllocChunk(it8, _sizeof<nint>() * count);

    private static T* AllocChunk<T>(IT8* it8) where T : struct =>
        (T*)AllocChunk(it8, _sizeof<T>());

    private static void* AllocChunk(IT8* it8, uint size)
    {
        var Free = it8->Allocator.BlockSize - it8->Allocator.Used;

        size = _cmsALIGNMEM(size);

        if (size > Free)
        {
            it8->Allocator.BlockSize = (it8->Allocator.BlockSize is 0)
                ? 20 * 1024
                : it8->Allocator.BlockSize * 2;

            if (it8->Allocator.BlockSize < size)
                it8->Allocator.BlockSize = size;

            it8->Allocator.Used = 0;
            it8->Allocator.Block = (byte*)AllocBigBlock(it8, it8->Allocator.BlockSize);
        }

        var ptr = it8->Allocator.Block + it8->Allocator.Used;
        it8->Allocator.Used += size;

        return ptr;
    }

    private static byte* AllocString(IT8* it8, byte* str)
    {
        var Size = strlen(str) + (nint)_sizeof<byte>();

        var ptr = (byte*)AllocChunk(it8, (uint)Size);
        if (ptr is not null) memcpy(ptr, str, Size - (nint)_sizeof<byte>());

        return ptr;
    }

    private static bool IsAvailableOnList(KEYVALUE* p, byte* Key, byte* Subkey, KEYVALUE** LastPtr)
    {
        if (LastPtr is not null) *LastPtr = p;

        for (; p is not null; p = p->Next)
        {
            if (LastPtr is not null) *LastPtr = p;

            if ((char)*Key is not '#')    // Comments are ignored
            {
                if (cmsstrcasecmp(Key, p->Keyword) is 0)
                    break;
            }
        }

        if (p is null)
            return false;

        if (Subkey is null)
            return true;

        for (; p is not null; p = p->NextSubkey)
        {
            if (p->Subkey is null) continue;

            if (LastPtr is not null) *LastPtr = p;

            if (cmsstrcasecmp(Subkey, p->Subkey) is 0)
                return true;
        }

        return false;
    }

    private static KEYVALUE* AddToList(IT8* it8, KEYVALUE** Head, byte* Key, byte* Subkey, byte* xValue, WRITEMODE WriteAs)
    {
        KEYVALUE* p;

        // Check if property is already in list
        if (IsAvailableOnList(*Head, Key, Subkey, &p))
        {
            // This may work for editing properties

            //     return SynError(it8, "duplicate key <{0}>".ToCharPtr(), Key);
        }
        else
        {
            var last = p;

            // Allocate the container
            p = AllocChunk<KEYVALUE>(it8);
            if (p is null)
            {
                SynError(it8, "AddToList: out of memory".ToBytePtr());
                return null;
            }

            // Store name and value
            p->Keyword = AllocString(it8, Key);
            p->Subkey = (Subkey is null) ? null : AllocString(it8, Subkey);

            // Keep the container in our list
            if (*Head is null)
                *Head = p;
            else
            {
                if (Subkey is not null && last is not null)
                {
                    last->NextSubkey = p;

                    // If Subkey is not null, then last is the last property with the same key,
                    // but not necessarily is the last property in the list, so we need to move
                    // to the actual list end
                    while (last->Next is not null)
                        last = last->Next;
                }

                if (last is not null) last->Next = p;
            }

            p->Next = null;
            p->NextSubkey = null;
        }

        p->WriteAs = WriteAs;

        p->Value = xValue is not null
            ? AllocString(it8, xValue)
            : null;

        return p;
    }

    private static KEYVALUE* AddAvailableProperty(IT8* it8, byte* Key, WRITEMODE @as) =>
        AddToList(it8, &it8->ValidKeywords, Key, null, null, @as);

    private static KEYVALUE* AddAvailableSampleID(IT8* it8, byte* Key) =>
        AddToList(it8, &it8->ValidSampleID, Key, null, null, WRITEMODE.WRITE_UNCOOKED);

    private static void AllocTable(IT8* it8)
    {
        var t = it8->Tab + it8->TablesCount;

        t->HeaderList = null;
        t->DataFormat = null;
        t->Data = null;

        it8->TablesCount++;
    }

    public static int cmsIT8SetTable(HANDLE IT8, uint nTable)
    {
        var it8 = (IT8*)IT8;

        if (nTable >= it8->TablesCount)
        {
            if (nTable == it8->TablesCount)
            {
                AllocTable(it8);
            }
            else
            {
                SynError(it8, "Table {0} is out of sequence".ToBytePtr(), nTable);
                return -1;
            }
        }

        it8->nTable = nTable;

        return (int)nTable;
    }

    public static HANDLE cmsIT8Alloc(Context? ContextID)
    {
        var it8 = _cmsMallocZero<IT8>(ContextID);
        if (it8 is null) return null;

        it8->Tab = _cmsCalloc<TABLE>(ContextID, MAXTABLES);
        if (it8->Tab is null) goto Error;

        it8->FileStack = _cmsCalloc2<FILECTX>(ContextID, MAXINCLUDE);
        if (it8->FileStack is null) goto Error;

        AllocTable(it8);

        it8->MemoryBlock = null;
        it8->MemorySink = null;

        it8->nTable = 0;

        it8->ContextID = ContextID;
        it8->Allocator.Used = 0;
        it8->Allocator.Block = null;
        it8->Allocator.BlockSize = 0;

        it8->ValidKeywords = null;
        it8->ValidSampleID = null;

        it8->sy = SYMBOL.SUNDEFINED;
        it8->ch = ' ';
        it8->Source = null;
        it8->inum = 0;
        it8->dnum = 0;

        it8->FileStack[0] = AllocChunk<FILECTX>(it8);
        it8->IncludeSP = 0;
        it8->lineno = 1;

        it8->id = StringAlloc(it8, MAXSTR);
        it8->str = StringAlloc(it8, MAXSTR);

        strcpy(it8->DoubleFormatter, DEFAULT_DBL_FORMAT);
        cmsIT8SetSheetType(it8, "CGATS.17");

        // Initialize predefined properties & data

        for (var i = 0; i < NUMPREDEFINEDPROPS; i++)
            AddAvailableProperty(it8, PredefinedProperties[i].id, PredefinedProperties[i].@as);

        for (var i = 0; i < NUMPREDEFINEDSAMPLEID; i++)
            AddAvailableSampleID(it8, PredefinedSampleID[i]);

        return it8;

    Error:
        if (it8->Tab is not null) _cmsFree(ContextID, it8->Tab);
        if (it8 is not null) _cmsFree(ContextID, it8);

        return null;
    }

    public static byte* cmsIT8GetSheetType(HANDLE hIT8) =>
        GetTable((IT8*)hIT8)->SheetType;

    public static bool cmsIT8SetSheetType(HANDLE hIT8, string Type)
    {
        var t = GetTable((IT8*)hIT8);

        strncpy(t->SheetType, Type, MAXSTR - 1);
        t->SheetType[MAXSTR - 1] = 0;
        return true;
    }

    public static bool cmsIT8SetComment(HANDLE hIT8, string Val)
    {
        var it8 = (IT8*)hIT8;

        if (String.IsNullOrEmpty(Val)) return false;

        return AddToList(it8, &GetTable(it8)->HeaderList, COMMENT_DELIMITER, null, Val.ToBytePtr(), WRITEMODE.WRITE_UNCOOKED) is not null;
    }

    public static bool cmsIT8SetPropertyStr(HANDLE hIT8, byte* Key, string Val)
    {
        var it8 = (IT8*)hIT8;

        if (String.IsNullOrEmpty(Val)) return false;

        return AddToList(it8, &GetTable(it8)->HeaderList, Key, null, Val.ToBytePtr(), WRITEMODE.WRITE_STRINGIFY) is not null;
    }

    public static bool cmsIT8SetPropertyDbl(HANDLE hIT8, byte* cProp, double Val)
    {
        var it8 = (IT8*)hIT8;
        var Buffer = stackalloc byte[1024];

        snprintf(Buffer, 1023, it8->DoubleFormatter, Val);

        return AddToList(it8, &GetTable(it8)->HeaderList, cProp, null, Buffer, WRITEMODE.WRITE_UNCOOKED) is not null;
    }

    public static bool cmsIT8SetPropertyHex(HANDLE hIT8, byte* cProp, uint Val)
    {
        var it8 = (IT8*)hIT8;
        var Buffer = stackalloc byte[1024];

        snprintf(Buffer, 1023, DEFAULT_HEX_FORMAT, Val);

        return AddToList(it8, &GetTable(it8)->HeaderList, cProp, null, Buffer, WRITEMODE.WRITE_HEXADECIMAL) is not null;
    }

    public static bool cmsIT8SetPropertyUncooked(HANDLE hIT8, byte* Key, byte* Buffer)
    {
        var it8 = (IT8*)hIT8;

        return AddToList(it8, &GetTable(it8)->HeaderList, Key, null, Buffer, WRITEMODE.WRITE_UNCOOKED) is not null;
    }

    public static bool cmsIT8SetPropertyMulti(HANDLE hIT8, byte* Key, byte* Subkey, byte* Buffer)
    {
        var it8 = (IT8*)hIT8;

        return AddToList(it8, &GetTable(it8)->HeaderList, Key, Subkey, Buffer, WRITEMODE.WRITE_PAIR) is not null;
    }

    public static byte* cmsIT8GetProperty(HANDLE hIT8, byte* Key)
    {
        var it8 = (IT8*)hIT8;
        KEYVALUE* p;

        if (IsAvailableOnList(GetTable(it8)->HeaderList, Key, null, &p))
            return p->Value;
        return null;
    }

    public static double cmsIT8GetPropertyDbl(HANDLE hIT8, byte* cProp)
    {
        var v = cmsIT8GetProperty(hIT8, cProp);

        if (v is null) return 0;

        return ParseFloatNumber(v);
    }

    public static byte* cmsIT8GetPropertyMulti(HANDLE hIT8, byte* Key, byte* Subkey)
    {
        var it8 = (IT8*)hIT8;
        KEYVALUE* p;

        if (IsAvailableOnList(GetTable(it8)->HeaderList, Key, Subkey, &p))
            return p->Value;
        return null;
    }

    private static void AllocateDataFormat(IT8* it8)
    {
        var t = GetTable(it8);

        if (t->DataFormat is not null) return;  // Already allocated

        t->nSamples = (int)cmsIT8GetPropertyDbl(it8, "NUMBER_OF_FIELDS".ToBytePtr());

        if (t->nSamples <= 0)
        {
            SynError(it8, "AllocateDataFormat: Unknown NUMBER_OF_FIELDS".ToBytePtr());
            t->nSamples = 10;
        }

        t->DataFormat = AllocChunk2<byte>(it8, (uint)t->nSamples + 1);
        if (t->DataFormat is null)
            SynError(it8, "AllocateDataFormat: Unable to allocate dataFormat array".ToBytePtr());
    }

    private static byte* GetDataFormat(IT8* it8, int n)
    {
        var t = GetTable(it8);

        if (t->DataFormat is not null)
            return t->DataFormat[n];
        return null;
    }

    private static bool SetDataFormat(IT8* it8, int n, byte* label)
    {
        var t = GetTable(it8);

        if (t->DataFormat is null)
            AllocateDataFormat(it8);

        if (n > t->nSamples)
        {
            SynError(it8, "More than NUMBER_OF_FIELDS fields".ToBytePtr());
            return false;
        }

        if (t->DataFormat is not null)
            t->DataFormat[n] = AllocString(it8, label);

        return true;
    }

    public static bool cmsIT8SetDataFormat(HANDLE h, int n, byte* Sample)
    {
        var it8 = (IT8*)h;
        return SetDataFormat(it8, n, Sample);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int atoi(byte* str) =>
        Int32.Parse(new string((sbyte*)str));

    private static int satoi(byte* b) =>
        b is not null
            ? atoi(b)
            : 0;
    private static byte* satob(byte* v)
    {
        var buf = BinaryConversionBuffer;
        var s = buf + 33;

        if (v is null) return "0".ToBytePtr();

        var x = atoi(v);
        *--s = 0;
        if (x is 0) *--s = (byte)'0';
        for (; x is not 0; x /= 2) *--s = (byte)('0' + (x % 2));

        return s;
    }

    private static void AllocateDataSet(IT8* it8)
    {
        var t = GetTable(it8);

        if (t->Data is not null) return;    // Already allocated

        t->nSamples = satoi(cmsIT8GetProperty(it8, "NUMBER_OF_FIELDS".ToBytePtr()));
        t->nPatches = satoi(cmsIT8GetProperty(it8, "NUMBER_OF_SETS".ToBytePtr()));

        if (t->nSamples is <0 or > 0x7ffe || t->nPatches is <0 or > 0x7ffe)
        {
            SynError(it8, "AllocateDataSet: too much data".ToBytePtr());
        }
        else
        {
            t->Data = AllocChunk2<byte>(it8, ((uint)t->nSamples + 1) * ((uint)t->nPatches + 1));
            if (t->Data is null)
                SynError(it8, "AllocateDataSet: Unable to allocate data array".ToBytePtr());
        }
    }

    private static byte* GetData(IT8* it8, int nSet, int nField)
    {
        var t = GetTable(it8);
        var nSamples = t->nSamples;
        var nPatches = t->nPatches;

        if (nSet >= nPatches || nField >= nSamples)
            return null;

        if (t->Data is null) return null;
        return t->Data[nSet * nSamples + nField];
    }

    private static bool SetData(IT8* it8, int nSet, int nField, byte* Val)
    {
        var t = GetTable(it8);

        if (t->Data is null)
            AllocateDataSet(it8);

        if (t->Data is null) return false;

        if (nSet > t->nPatches || nSet < 0)
            return SynError(it8, "Patch {0} out of range, there are {1} patches".ToBytePtr(), nSet, t->nPatches);

        if (nField > t->nSamples || nField < 0)
            return SynError(it8, "Sample {0} out of range, there are {1} samples".ToBytePtr(), nSet, t->nSamples);

        t->Data[nSet * t->nSamples + nField] = AllocString(it8, Val);
        return true;
    }

    private static void WriteStr(SAVESTREAM* f, byte* str)
    {
        if (str is null)
            str = " ".ToBytePtr();

        // Length to write
        var len = (uint)strlen(str);
        f->Used += len;

        if (f->stream is not null)  // Should I write it to a file?
        {
            if (fwrite(str, 1, len, f->stream) != len)
            {
                cmsSignalError(null, cmsERROR_WRITE, "Write to file error in CGATS parser");
                return;
            }
        }
        else        // Or to a memory block?
        {
            if (f->Base is not null)    // Am I just counting the bytes?
            {
                if (f->Used > f->Max)
                {
                    cmsSignalError(null, cmsERROR_WRITE, "Write to memoty overflows in CGATS parser");
                    return;
                }

                memmove(f->Ptr, str, len);
                f->Ptr += len;
            }

        }
    }

    private static void Writef(SAVESTREAM* f, byte* frm, params object[] args)
    {
        var Buffer = stackalloc byte[4096];

        vsnprintf(Buffer, 4095, frm, args);
        Buffer[4096] = 0;
        WriteStr(f, Buffer);
    }

    private static void WriteHeader(IT8* it8, SAVESTREAM* fp)
    {
        KEYVALUE* p;
        var t = GetTable(it8);

        // Writes the type
        WriteStr(fp, t->SheetType);
        WriteStr(fp, "\n".ToBytePtr());

        for (p = t->HeaderList; p is not null; p = p->Next)
        {
            if ((char)*p->Keyword is '#')
            {
                WriteStr(fp, "#\n# ".ToBytePtr());
                for (var Pt = p->Value; *Pt is not 0; Pt++)
                {
                    Writef(fp, "{0}".ToBytePtr(), *Pt);

                    if ((char)*Pt is '\n')
                        WriteStr(fp, "# ".ToBytePtr());
                }

                WriteStr(fp, "\n#\n".ToBytePtr());
                continue;
            }

            if (!IsAvailableOnList(it8->ValidKeywords, p->Keyword, null, null))
            {
                //WriteStr(fp, "KEYWORD\t\"".ToCharPtr());
                //WriteStr(fp, p->Keyword);
                //WriteStr(fp, "\"\n".ToCharPtr());

                AddAvailableProperty(it8, p->Keyword, WRITEMODE.WRITE_UNCOOKED);
            }

            WriteStr(fp, p->Keyword);
            if (p->Value is not null)
            {
                switch (p->WriteAs)
                {
                    case WRITEMODE.WRITE_UNCOOKED:
                        Writef(fp, "\t{0}".ToBytePtr(), new string((sbyte*)p->Value));
                        break;
                    case WRITEMODE.WRITE_STRINGIFY:
                        Writef(fp, "\t\"{0}\"".ToBytePtr(), new string((sbyte*)p->Value));
                        break;
                    case WRITEMODE.WRITE_HEXADECIMAL:
                        Writef(fp, "\t0x{0:X}".ToBytePtr(), satoi(p->Value));
                        break;
                    case WRITEMODE.WRITE_BINARY:
                        Writef(fp, "\t0b{0}".ToBytePtr(), new string((sbyte*)satob(p->Value)));
                        break;
                    case WRITEMODE.WRITE_PAIR:
                        Writef(fp, "\t\"{0},{1}\"".ToBytePtr(), new string((sbyte*)p->Subkey), new string((sbyte*)p->Value));
                        break;
                    default:
                        SynError(it8, "Unknown write mode {0}".ToBytePtr(), (object?)Enum.GetName(p->WriteAs) ?? p->WriteAs);
                        return;
                }
            }

            WriteStr(fp, "\n".ToBytePtr());
        }
    }

    private static void WriteDataFormat(SAVESTREAM* fp, IT8* it8)
    {
        var t = GetTable(it8);

        if (t->DataFormat is null) return;

        WriteStr(fp, "BEGIN_DATA_FORMAT\n".ToBytePtr());
        WriteStr(fp, " ".ToBytePtr());
        var nSamples = satoi(cmsIT8GetProperty(it8, "NUMBER_OF_FIELDS".ToBytePtr()));

        for (var i = 0; i < nSamples; i++)
        {
            WriteStr(fp, t->DataFormat[i]);
            WriteStr(fp, ((i == (nSamples - 1)) ? "\n" : "\t").ToBytePtr());
        }

        WriteStr(fp, "END_DATA_FORMAT\n".ToBytePtr());
    }

    private static void WriteData(SAVESTREAM* fp, IT8* it8)
    {
        var t = GetTable(it8);

        if ( t->Data is null) return;

        WriteStr(fp, "BEGIN_DATA\n".ToBytePtr());

        t->nPatches = satoi(cmsIT8GetProperty(it8, "NUMBER_OF_SETS".ToBytePtr()));

        for (var i = 0; i < t->nPatches; i++)
        {
            WriteStr(fp, " ".ToBytePtr());

            for (var j = 0; j < t->nSamples; j++)
            {
                var ptr = t->Data[(i * t->nSamples) + j];

                if (ptr is null) WriteStr(fp, "\"\"".ToBytePtr());
                else
                {
                    // If value contains whitespace, enclose within quote

                    if (strchr(ptr, ' ') is not null)
                    {
                        WriteStr(fp, "\"".ToBytePtr());
                        WriteStr(fp, ptr);
                        WriteStr(fp, "\"".ToBytePtr());
                    }
                    else
                        WriteStr(fp, ptr);
                }

                WriteStr(fp, ((j == (t->nSamples - 1)) ? "\n" : "\t").ToBytePtr());
            }
        }

        WriteStr(fp, "END_DATA\n".ToBytePtr());
    }

    public static bool cmsIT8SaveToFile(HANDLE hIT8, char* cFileName)
    {
        SAVESTREAM sd;
        var it8 = (IT8*)hIT8;

        memset(&sd, 0);

        sd.stream = fopen(new(cFileName), "wt");
        if (sd.stream is null) return false;

        for (var i = 0u; i < it8->TablesCount; i++)
        {
            cmsIT8SetTable(hIT8, i);
            WriteHeader(it8, &sd);
            WriteDataFormat(&sd, it8);
            WriteData(&sd, it8);
        }

        if (fclose(sd.stream) is not 0) return false;

        return true;
    }

    public static bool cmsIT8SaveToMem(HANDLE hIT8, void* MemPtr, uint* BytesNeeded)
    {
        SAVESTREAM sd;
        var it8 = (IT8*)hIT8;

        memset(&sd, 0);

        sd.stream = null;
        sd.Base = (byte*)MemPtr;
        sd.Ptr = sd.Base;

        sd.Used = 0;

        sd.Max = (sd.Base is not null)
            ? *BytesNeeded      // Write to memory?
            : 0;                // Just counting the needed bytes

        for (var i = 0u; i < it8->TablesCount; i++)
        {
            cmsIT8SetTable(hIT8, i);
            WriteHeader(it8, &sd);
            WriteDataFormat(&sd, it8);
            WriteData(&sd, it8);
        }

        sd.Used++;      // the \0 at the very end

        if (sd.Base is not null)
            *sd.Ptr = 0;

        *BytesNeeded = sd.Used;

        return true;
    }

    private static bool DataFormatSection(IT8* it8)
    {
        var iField = 0;
        var t = GetTable(it8);

        InSymbol(it8);      // Eats "BEGIN_DATA_FORMAT"
        CheckEOLN(it8);

        while (it8->sy is not SYMBOL.SEND_DATA_FORMAT 
                      and not SYMBOL.SEOLN 
                      and not SYMBOL.SEOF 
                      and not SYMBOL.SSYNERROR)
        {
            if (it8->sy is not SYMBOL.SIDENT)
                return SynError(it8, "Sample type expected".ToBytePtr());

            if (!SetDataFormat(it8, iField, StringPtr(it8->id))) return false;
            iField++;

            InSymbol(it8);
            SkipEOLN(it8);
        }

        SkipEOLN(it8);
        Skip(it8, SYMBOL.SEND_DATA_FORMAT);
        SkipEOLN(it8);

        if (iField != t->nSamples)
            SynError(it8, "Count mismatch. NUMBER_OF_FIELDS was {0}, found {1}\n".ToBytePtr(), t->nSamples, iField);

        return true;
    }

    private static bool DataSection(IT8* it8)
    {
        var Buffer = stackalloc byte[256];
        var iField = 0;
        var iSet = 0;
        var t = GetTable(it8);

        InSymbol(it8);      // Eats "BEGIN_DATA"
        CheckEOLN(it8);

        if (t->Data is null)
            AllocateDataSet(it8);

        while (it8->sy is not SYMBOL.SEND_DATA and not SYMBOL.SEOF)
        {
            if (iField >= t->nSamples)
            {
                iField = 0;
                iSet++;
            }

            if (it8->sy is not SYMBOL.SEND_DATA and not SYMBOL.SEOF)
            {
                switch (it8->sy)
                {
                    // To keep very long data
                    case SYMBOL.SIDENT:
                        if (!SetData(it8, iSet, iField, StringPtr(it8->id)))
                            return false;
                        break;
                    case SYMBOL.SSTRING:
                        if (!SetData(it8, iSet, iField, StringPtr(it8->str)))
                            return false;
                        break;
                    default:
                        if (!GetVal(it8, Buffer, 255, "Sample data expected"))
                            return false;
                        if (!SetData(it8, iSet, iField, Buffer))
                            return false;
                        break;
                }

                iField++;

                InSymbol(it8);
                SkipEOLN(it8);
            }
        }

        SkipEOLN(it8);
        Skip(it8, SYMBOL.SEND_DATA);
        SkipEOLN(it8);

        // Check for data completion.

        if ((iSet + 1) != t->nPatches)
            return SynError(it8, "Count mismatch. NUMBER_OF_SETS was {0}, found {1}\n".ToBytePtr(), t->nPatches, iSet + 1);

        return true;
    }

    private static bool HeaderSection(IT8* it8)
    {
        var VarName = stackalloc byte[MAXID];
        var Buffer = stackalloc byte[MAXSTR];
        KEYVALUE* Key;

        while (it8->sy is not SYMBOL.SEOF 
                      and not SYMBOL.SSYNERROR 
                      and not SYMBOL.SBEGIN_DATA_FORMAT 
                      and not SYMBOL.SBEGIN_DATA)
        {
            switch (it8->sy)
            {
                case SYMBOL.SKEYWORD:
                    InSymbol(it8);
                    if (!GetVal(it8, Buffer, MAXSTR - 1, "Keyword expected")) return false;
                    if (AddAvailableProperty(it8, Buffer, WRITEMODE.WRITE_UNCOOKED) is null) return false;
                    InSymbol(it8);
                    break;

                case SYMBOL.SDATA_FORMAT_ID:
                    InSymbol(it8);
                    if (!GetVal(it8, Buffer, MAXSTR - 1, "Keyword expected")) return false;
                    if (AddAvailableSampleID(it8, Buffer) is null) return false;
                    InSymbol(it8);
                    break;

                case SYMBOL.SIDENT:
                    strncpy(VarName, StringPtr(it8->id), MAXID - 1);
                    VarName[MAXID - 1] = 0;

                    if (!IsAvailableOnList(it8->ValidKeywords, VarName, null, &Key))
                    {
                        //return SynError(it8, "Undefined keyword '{0}'".ToCharPtr(), new string(VarName));

                        Key = AddAvailableProperty(it8, VarName, WRITEMODE.WRITE_UNCOOKED);
                        if (Key is null) return false;
                    }

                    InSymbol(it8);
                    if (!GetVal(it8, Buffer, MAXSTR - 1, "Property data expected")) return false;

                    if (Key->WriteAs is not WRITEMODE.WRITE_PAIR)
                    {
                        AddToList(it8, &GetTable(it8)->HeaderList, VarName, null, Buffer, 
                            (it8->sy is SYMBOL.SSTRING) ? WRITEMODE.WRITE_STRINGIFY : WRITEMODE.WRITE_UNCOOKED);
                    }
                    else
                    {
                        byte* Subkey, Nextkey;

                        if (it8->sy is not SYMBOL.SSTRING)
                            return SynError(it8, "Invalid value '{0}' for property '{1}'.".ToBytePtr(), new string((sbyte*)Buffer), new string((sbyte*)VarName));

                        // chop the string as a list of "subkey, value" pairs, using ';' as a separator
                        for (Subkey = Buffer; Subkey is not null; Subkey = Nextkey)
                        {
                            byte* Value, temp;

                            // identify token pair boundary
                            Nextkey = strchr(Subkey, ';');
                            if (Nextkey is not null)
                                *Nextkey++ = 0;

                            // for each pair, split the subkey and the value
                            Value = strrchr(Subkey, (byte)',');
                            if (Value is null)
                                return SynError(it8, "Invalid value for property '{0}.".ToBytePtr(), new string((sbyte*)VarName));

                            // gobble the spaces before the comma, and the comma itself
                            temp = Value++;
                            do *temp-- = 0; while (temp >= Subkey && *temp == ' ');

                            // gobble any space at the right
                            temp = Value + strlen(Value) - 1;
                            while (*temp == ' ') *temp-- = 0;

                            // trim the strings from the left
                            Subkey += strspn(Subkey, " ".ToBytePtr());
                            Value += strspn(Value, " ".ToBytePtr());

                            if (Subkey[0] is 0 || Value[0] is 0)
                                return SynError(it8, "Invalid value for property '{0}'.".ToBytePtr(), new string((sbyte*)VarName));
                            AddToList(it8, &GetTable(it8)->HeaderList, VarName, Subkey, Value, WRITEMODE.WRITE_PAIR);
                        }
                    }

                    InSymbol(it8);
                    break;

                case SYMBOL.SEOLN: break;

                default:
                    return SynError(it8, "expected keyword or identifier".ToBytePtr());
            }

            SkipEOLN(it8);
        }

        return true;
    }

    private static void ReadType(IT8* it8, byte* SheetTypePtr)
    {
        var cnt = 0;

        // First line is a very special case.

        while (isseparator(it8->ch))
            NextCh(it8);

        while (it8->ch is not '\r' and not '\n' and not '\t' and not '\0')
        {
            if (cnt++ < MAXSTR)
                *SheetTypePtr++ = (byte)it8->ch;
            NextCh(it8);
        }

        *SheetTypePtr = 0;
    }

    private static bool ParseIT8(IT8* it8, bool nosheet)
    {
        var SheetTypePtr = it8->Tab[0].SheetType;

        if (!nosheet)
        {
            ReadType(it8, SheetTypePtr);
        }

        InSymbol(it8);

        SkipEOLN(it8);

        while (it8->sy is not SYMBOL.SEOF and not SYMBOL.SSYNERROR)
        {
            switch (it8->sy)
            {
                case SYMBOL.SBEGIN_DATA_FORMAT:
                    if (!DataFormatSection(it8)) return false;
                    break;

                case SYMBOL.SBEGIN_DATA:
                    if (!DataSection(it8)) return false;

                    if (it8->sy is not SYMBOL.SEOF)
                    {
                        AllocTable(it8);
                        it8->nTable = it8->TablesCount - 1;

                        // Read sheet type if present. We only support identifier and string
                        // <ident> <eoln> is a type string
                        // anything else, is not a type string
                        if (!nosheet)
                        {
                            if (it8->sy is SYMBOL.SIDENT)
                            {
                                // May be a type sheet or may be a prop value statement. We cannot use insymbol in
                                // this special case...
                                while (isseparator(it8->ch))
                                    NextCh(it8);

                                // If a newline is found, then this is a type string
                                if (it8->ch is '\n' or '\r')
                                {
                                    cmsIT8SetSheetType(it8, new string((sbyte*)StringPtr(it8->id)));
                                    InSymbol(it8);
                                }
                                else
                                {
                                    // It is not. Just continue
                                    cmsIT8SetSheetType(it8, "");
                                }
                            }
                            else
                            {
                                // Validate quoted strings
                                if (it8->sy is SYMBOL.SSTRING)
                                {
                                    cmsIT8SetSheetType(it8, new string((sbyte*)StringPtr(it8->str)));
                                    InSymbol(it8);
                                }
                            }
                        }
                    }

                    break;

                case SYMBOL.SEOLN:
                    SkipEOLN(it8);
                    break;

                default:
                    if (!HeaderSection(it8)) return false;
                    break;
            }
        }

        return it8->sy is not SYMBOL.SSYNERROR;
    }

    private static void CookPointers(IT8* it8)
    {
        var Buffer = stackalloc byte[256];

        var nOldTable = it8->nTable;

        for (var j = 0u; j < it8->TablesCount; j++)
        {
            var t = it8->Tab + j;

            t->SampleID = 0;
            it8->nTable = j;

            for (var idField = 0; idField < t->nSamples; idField++)
            {
                if (t->DataFormat is null)
                {
                    SynError(it8, "Undefined DATA_FORMAT".ToBytePtr());
                    return;
                }

                var Fld = t->DataFormat[idField];
                if (Fld == null) continue;

                if (cmsstrcasecmp(Fld, "SAMPLE_ID".ToBytePtr()) is 0)
                    t->SampleID = idField;

                // "LABEL" is an extension. It keeps references to forward tables
                if ((cmsstrcasecmp(Fld, "LABEL".ToBytePtr()) is 0) || Fld[0] == '$')
                {
                    // Search for table references...
                    for (var i = 0; i < t->nPatches; i++)
                    {
                        var Label = GetData(it8, i, idField);

                        if (Label is not null)
                        {
                            // This is the label, search for a table containing
                            // this property

                            for (var k = 0u; k < it8->TablesCount; k++)
                            {
                                var Table = it8->Tab + k;
                                KEYVALUE* p;

                                if (IsAvailableOnList(Table->HeaderList, Label, null, &p))
                                {
                                    // Available, keep type and table
                                    memset(Buffer, 0, 256 * _sizeof<char>());

                                    var Type = p->Value;
                                    var nTable = (int)k;

                                    snprintf(Buffer, 255, "{0} {1} {2}".ToBytePtr(), new string((sbyte*)Label), nTable, new string((sbyte*)Type));

                                    SetData(it8, i, idField, Buffer);
                                }
                            }
                        }
                    }
                }
            }
        }

        it8->nTable = nOldTable;
    }

    private static int IsMyBlock(byte* Buffer, uint n)
    {
        int words = 1, space = 0, quot = 0;

        if (n < 10) return 0;       // Too small

        if (n > 132)
            n = 132;

        for (var i = 1u; i < n; i++)
        {
            switch ((char)Buffer[i])
            {
                case '\n':
                case '\r':
                    return ((quot is 1) || (words > 2)) ? 0 : words;
                case '\t':
                case ' ':
                    if (quot is 0 && space is 0)
                        space = 1;
                    break;
                case '\"':
                    quot = quot is 1 ? 0 : 1;
                    break;
                default:
                    if (Buffer[i] is < 32 or > 127) return 0;
                    words += space;
                    space = 0;
                    break;
            }
        }

        return 0;
    }

    private static bool IsMyFile(string FileName)
    {
        var Ptr = stackalloc byte[133];

        var fp = fopen(FileName, "rt");
        if (fp is null)
        {
            cmsSignalError(null, cmsERROR_FILE, "File '{0}' not found", FileName);
            return false;
        }

        var Size = (uint)fread(Ptr, 1, 132, fp);

        if (fclose(fp) is not 0)
            return false;

        Ptr[Size] = 0;

        return IsMyBlock(Ptr, Size) is not 0;
    }

    public static HANDLE cmsIT8LoadFromMem(Context? ContextID, void* Ptr, uint len)
    {
        _cmsAssert(Ptr);
        _cmsAssert(len is not 0);

        var type = IsMyBlock((byte*)Ptr, len);
        if (type is 0) return null;

        var hIT8 = cmsIT8Alloc(ContextID);
        if (hIT8 is null) return null;

        var it8 = (IT8*)hIT8;
        it8->MemoryBlock = _cmsMalloc<byte>(ContextID, len + 1);
        if (it8->MemoryBlock is null)
        {
            cmsIT8Free(hIT8);
            return null;
        }

        strncpy(it8->MemoryBlock, (byte*)Ptr, len);
        it8->MemoryBlock[len] = 0;

        strncpy(it8->FileStack[0]->FileName, "", cmsMAX_PATH - 1);
        it8->Source = it8->MemoryBlock;

        if (!ParseIT8(it8, type is not 0))
        {
            cmsIT8Free(hIT8);
            return null;
        }

        CookPointers(it8);
        it8->nTable = 0;

        _cmsFree(ContextID, it8->MemoryBlock);
        it8->MemoryBlock = null;

        return hIT8;
    }

    public static HANDLE cmsIT8LoadFromFile(Context? ContextID, string cFileName)
    {
        _cmsAssert(cFileName);

        var type = IsMyFile(cFileName);
        if (!type) return null;

        var hIT8 = cmsIT8Alloc(ContextID);
        if (hIT8 is null) return null;

        var it8 = (IT8*)hIT8;
        var file = fopen(cFileName, "rt");

        if (file is null)
        {
            cmsIT8Free(hIT8);
            return null;
        }
        it8->FileStack[0]->Stream = (FileStream)file.Stream;

        strncpy(it8->FileStack[0]->FileName, cFileName, cmsMAX_PATH - 1);
        it8->FileStack[0]->FileName[cmsMAX_PATH - 1] = 0;

        if (!ParseIT8(it8, !type))
        {
            fclose(file);
            cmsIT8Free(hIT8);
            return null;
        }

        CookPointers(it8);
        it8->nTable = 0;

        if (fclose(file) is not 0)
        {
            cmsIT8Free(hIT8);
            return null;
        }

        return hIT8;
    }

    public static int cmsIT8EnumDataFormat(HANDLE hIT8, byte*** SampleNames)
    {
        var it8 = (IT8*)hIT8;

        _cmsAssert(hIT8);

        var t = GetTable(it8);

        if (SampleNames is not null)
            *SampleNames = t->DataFormat;
        return t->nSamples;
    }

    public static uint cmsIT8EnumProperties(HANDLE hIT8, byte*** PropertyNames)
    {
        var it8 = (IT8*)hIT8;
        _cmsAssert(hIT8);

        var t = GetTable(it8);

        // Pass#1 - count properties

        var n = 0u;
        for (var p = t->HeaderList; p is not null; p = p->Next)
            n++;

        var Props = AllocChunk2<byte>(it8, n);

        // Pass#2 - Fill pointers
        n = 0;
        for (var p = t->HeaderList; p is not null; p = p->Next)
            Props[n++] = p->Keyword;

        *PropertyNames = Props;
        return n;
    }

    public static uint cmsIT8EnumPropertyMulti(HANDLE hIT8, byte* cProp, byte*** SubpropertyNames)
    {
        KEYVALUE* p;

        var it8 = (IT8*)hIT8;
        _cmsAssert(hIT8);

        var t = GetTable(it8);

        if (!IsAvailableOnList(t->HeaderList, cProp, null, &p))
        {
            *SubpropertyNames = null;
            return 0;
        }

        // Pass#1 - count properties

        var n = 0u;
        for (var tmp = p; tmp is not null; tmp = tmp->NextSubkey)
        {
            if (tmp->Subkey is not null)
                n++;
        }

        var Props = AllocChunk2<byte>(it8, n);

        // Pass#2 - Fill pointers
        n = 0;
        for (var tmp = p; tmp is not null; tmp = tmp->NextSubkey)
            Props[n++] = p->Subkey;

        *SubpropertyNames = Props;
        return n;
    }

    private static int LocatePatch(IT8* it8, byte* cPatch)
    {
        var t = GetTable(it8);

        for (var i = 0; i < t->nPatches; i++)
        {
            var data = GetData(it8, i, t->SampleID);

            if (data is not null)
            {
                if (cmsstrcasecmp(data, cPatch) is 0)
                    return i;
            }
        }

        // SynError(it8, "Couldn't find patch '{0}'\n".ToBytePtr(), new string((sbyte*)cPatch));
        return -1;
    }

    private static int LocateEmptyPatch(IT8* it8)
    {
        var t = GetTable(it8);

        for (var i = 0; i < t->nPatches; i++)
        {
            var data = GetData(it8, i, t->SampleID);

            if (data is null)
                return i;
        }

        return -1;
    }

    private static int LocateSample(IT8* it8, byte* cSample)
    {
        var t = GetTable(it8);

        for (var i = 0; i < t->nSamples; i++)
        {
            var fld = GetDataFormat(it8, i);
            if (fld is not null)
            {
                if (cmsstrcasecmp(fld, cSample) is 0)
                    return i;
            }
        }

        return -1;
    }

    public static int cmsIT8FindDataFormat(HANDLE hIT8, byte* cSample)
    {
        var it8 = (IT8*)hIT8;

        _cmsAssert(hIT8);

        return LocateSample(it8, cSample);
    }

    public static byte* cmsIT8GetDataRowCol(HANDLE hIT8, int row, int col)
    {
        var it8 = (IT8*)hIT8;

        _cmsAssert(hIT8);

        return GetData(it8, row, col);
    }

    public static double cmsIT8GetDataRowColDbl(HANDLE hIT8, int row, int col)
    {
        var Buffer = cmsIT8GetDataRowCol(hIT8, row, col);

        if (Buffer is null) return 0.0;

        return ParseFloatNumber(Buffer);
    }

    public static bool cmsIT8SetDataRowCol(HANDLE hIT8, int row, int col, byte* Val)
    {
        var it8 = (IT8*)hIT8;

        _cmsAssert(hIT8);

        return SetData(it8, row, col, Val);
    }

    public static bool cmsIT8SetDataRowColDbl(HANDLE hIT8, int row, int col, double Val)
    {
        var it8 = (IT8*)hIT8;
        var Buff = stackalloc byte[256];

        _cmsAssert(hIT8);

        snprintf(Buff, 255, it8->DoubleFormatter, Val);

        return SetData(it8, row, col, Buff);
    }

    public static byte* cmsIT8GetData(HANDLE hIT8, byte* cPatch, byte* cSample)
    {
        var it8 = (IT8*)hIT8;

        _cmsAssert(hIT8);

        var iField = LocateSample(it8, cSample);
        if (iField < 0)
            return null;

        var iSet = LocatePatch(it8, cPatch);
        if (iSet < 0)
            return null;

        return GetData(it8, iSet, iField);
    }

    public static double cmsIT8GetDataDbl(HANDLE hIT8, byte* cPatch, byte* cSample) =>
        ParseFloatNumber(cmsIT8GetData(hIT8, cPatch, cSample));

    public static bool cmsIT8SetData(HANDLE hIT8, byte* cPatch, byte* cSample, byte* Val)
    {
        var it8 = (IT8*)hIT8;

        _cmsAssert(hIT8);

        var t = GetTable(it8);

        var iField = LocateSample(it8, cSample);
        if (iField < 0)
            return false;

        if (t->nPatches is 0)
        {
            AllocateDataFormat(it8);
            AllocateDataSet(it8);
            CookPointers(it8);
        }

        int iSet;
        if (cmsstrcasecmp(cSample, "SAMPLE_ID".ToBytePtr()) is 0)
        {
            iSet = LocateEmptyPatch(it8);
            if (iSet < 0)
                return SynError(it8, "Couldn't add more patches '{0}'\n".ToBytePtr(), new string((sbyte*)cPatch));

            iField = t->SampleID;
        }
        else
        {
            iSet = LocatePatch(it8, cPatch);
            if (iSet < 0)
                return false;
        }

        return SetData(it8, iSet, iField, Val);
    }

    public static bool cmsIT8SetDataDbl(HANDLE hIT8, byte* cPatch, byte* cSample, double Val)
    {
        var it8 = (IT8*)hIT8;
        var Buff = stackalloc byte[256];

        _cmsAssert(hIT8);

        snprintf(Buff, 255, it8->DoubleFormatter, Val);
        return cmsIT8SetData(hIT8, cPatch, cSample, Buff);
    }

    public static byte* cmsIT8GetPatchName(HANDLE hIT8, int nPatch, byte* buffer)
    {
        var it8 = (IT8*)hIT8;

        _cmsAssert(hIT8);

        var t = GetTable(it8);
        var Data = GetData(it8, nPatch, t->SampleID);

        if (Data is null) return null;
        if (buffer is null) return Data;

        strncpy(buffer, Data, MAXSTR - 1);
        buffer[MAXSTR - 1] = 0;
        return buffer;
    }

    public static int cmsIT8GetPatchByName(HANDLE hIT8, byte* cPatch)
    {
        _cmsAssert(hIT8);

        return LocatePatch((IT8*)hIT8, cPatch);
    }

    public static uint cmsIT8TableCount(HANDLE hIT8)
    {
        _cmsAssert(hIT8);

        return ((IT8*)hIT8)->TablesCount;
    }

    private static int ParseLabel(byte* buffer, byte* label, uint* tableNum, byte* type)
    {
        var str = new string((sbyte*)buffer);
        var split = str.Split(' ');
        var result = split.Length;

        if (result is 0) return 0;

        if (split[0].Length > 255)
            split[0] = split[0][..255];

        strncpy(label, split[0], 255);

        if (result is 1) return 1;

        var success = uint.TryParse(split[1], out var n);
        if (!success) return 1;

        *tableNum = n;

        if (result is 2) return 2;

        if (split[2].Length > 255)
            split[2] = split[2][..255];

        strncpy(type, split[2], 255);

        return 3;
    }

    public static int cmsIT8SetTableByLabel(HANDLE hIT8, byte* cSet, byte* cField, byte* ExpectedType)
    {
        var Type = stackalloc byte[256];
        var Label = stackalloc byte[256];

        _cmsAssert(hIT8);

        if ((cField is not null && *cField is 0) || (cField is null))
            cField = "LABEL".ToBytePtr();

        var cLabelFld = cmsIT8GetData(hIT8, cSet, cField);
        if (cLabelFld is null) return -1;

        uint nTable;
        if (ParseLabel(cLabelFld, Label, &nTable, Type) is not 3)
            return -1;

        if (ExpectedType is not null && *ExpectedType is 0)
            ExpectedType = null;

        if (ExpectedType is not null && cmsstrcasecmp(Type, ExpectedType) is not 0)
            return -1;

        return cmsIT8SetTable(hIT8, nTable);
    }

    public static bool cmsIT8SetIndexColumn(HANDLE hIT8, byte* cSample)
    {
        var it8 = (IT8*)hIT8;

        _cmsAssert(hIT8);

        var pos = LocateSample(it8, cSample);
        if (pos is -1)
            return false;

        it8->Tab[it8->nTable].SampleID = pos;
        return true;
    }

    public static void cmsIT8DefineDblFormat(HANDLE hIT8, byte* Formatter)
    {
        var it8 = (IT8*)hIT8;

        _cmsAssert(hIT8);

        if (Formatter is null)
            strcpy(it8->DoubleFormatter, DEFAULT_DBL_FORMAT);
        else
            strncpy(it8->DoubleFormatter, Formatter, MAXID);

        it8->DoubleFormatter[MAXID - 1] = 0;
    }
}
