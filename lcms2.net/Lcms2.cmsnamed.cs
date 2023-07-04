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

using lcms2.state;
using lcms2.types;

using System.Diagnostics;
using System.Text;

namespace lcms2;

public static unsafe partial class Lcms2
{
    public static Mlu? cmsMLUalloc(Context? ContextID, uint nItems)
    {
        if (nItems is 0)
            nItems = 2;

        // Create the container
        var mlu = new Mlu(ContextID, nItems);
        //if (mlu is null) return null;

        //mlu->ContextID = ContextID;

        // Create entry array
        //mlu->Entries = _cmsCalloc<MluEntry>(ContextID, nItems);
        //if (mlu->Entries is null)
        //{
        //    _cmsFree(ContextID, mlu);
        //    return null;
        //}

        // Ok, keep indexes up to date
        //mlu->AllocatedEntries = nItems;
        //mlu->UsedEntries = 0;

        return mlu;
    }

    private static bool GrowMLUpool(Mlu mlu)
    {
        // Sanity check
        if (mlu is null) return false;

        var size =
            mlu.PoolSizeInBytes is 0
                ? 256u
                : mlu.PoolSizeInBytes * 2;

        // Check for overflow
        if (size < mlu.PoolSizeInBytes) return false;

        // Reallocate the pool
        var NewPtr = (mlu.MemPool is not null)
            ? _cmsRealloc(mlu.ContextID, mlu.MemPool, size)
            : _cmsCallocArray<char>(mlu.ContextID, size);
        if (NewPtr is null) return false;

        mlu.MemPool = NewPtr;
        mlu.PoolSizeInBytes = size;

        return true;
    }

    private static bool GrowMLUtable(Mlu mlu)
    {
        // I know this function is not needed as List<T> does this automatically, but I'm not ready to
        // make that kind of cleaning sweep through the code to C#ify it...

        // Sanity check
        if (mlu is null) return false;

        var AllocatedEntries = mlu.AllocatedEntries * 2;

        // Check for overflow
        if (AllocatedEntries / 2 != mlu.AllocatedEntries) return false;

        // Reallocate the memory
        //var NewPtr = _cmsRealloc<MluEntry>(mlu->ContextID, mlu->Entries, AllocatedEntries * (uint)sizeof(MluEntry));
        //if (NewPtr is null) return false;

        //mlu->Entries = NewPtr;
        mlu.AllocatedEntries = AllocatedEntries;

        return true;
    }

    private static int SearchMLUEntry(Mlu mlu, ushort LanguageCode, ushort CountryCode)
    {
        // Sanity Check
        if (mlu is null) return -1;

        // Iterate whole table
        for (var i = 0; i < mlu.UsedEntries; i++)
        {
            if (mlu.Entries[i].Country == CountryCode &&
                mlu.Entries[i].Language == LanguageCode) return i;
        }

        // Not found
        return -1;
    }

    private static bool AddMLUBlock(Mlu mlu, uint sizeInBytes, ReadOnlySpan<char> Block, ushort LanguageCode, ushort CountryCode)
    {
        // Sanity Check
        if (mlu is null) return false;

        // Is there any room available?
        if (mlu.UsedEntries >= mlu.AllocatedEntries)
        {
            if (!GrowMLUtable(mlu)) return false;
        }

        // Only one ASCII string
        if (SearchMLUEntry(mlu, LanguageCode, CountryCode) >= 0) return false;

        // Check for size
        while ((mlu.PoolSizeInBytes - mlu.PoolUsedInBytes) < sizeInBytes)
            if (!GrowMLUpool(mlu)) return false;

        var OffsetInChars = mlu.PoolUsedInBytes / _sizeof<char>();

        var Ptr = mlu.MemPool.AsSpan();
        //if (Ptr is null) return false;

        // Set the entry
        //memmove(Ptr + Offset, Block, size);
        Block[..(int)(sizeInBytes/_sizeof<char>())].CopyTo(Ptr[(int)OffsetInChars..][..(int)(sizeInBytes/_sizeof<char>())]);
        mlu.PoolUsedInBytes += sizeInBytes;

        mlu.Entries.Add(new(
            LanguageCode,
            CountryCode,
            OffsetInChars * _sizeof<char>(),
            sizeInBytes));
        //mlu->Entries[mlu->UsedEntries].StrW = Offset;
        //mlu->Entries[mlu->UsedEntries].Len = size;
        //mlu->Entries[mlu->UsedEntries].Country = CountryCode;
        //mlu->Entries[mlu->UsedEntries].Language = LanguageCode;
        //mlu->UsedEntries++;

        return true;
    }

    [DebuggerStepThrough]
    private static ushort strTo16(in byte* str)
    {
        // for non-existent strings
        if (str is null) return 0;
        return (ushort)((str[0] << 8) | str[1]);
    }

    [DebuggerStepThrough]
    private static ushort strTo16(ReadOnlySpan<byte> str)
    {
        var strPtr = stackalloc byte[2]
        {
            str[0],
            str[1]
        };
        return strTo16(strPtr);
    }

    [DebuggerStepThrough]
    private static void strFrom16(byte* str, ushort n)
    {
        str[0] = (byte)(n >> 8);
        str[1] = (byte)n;
        str[2] = 0;
    }

    private static void strFrom16(Span<byte> str, ushort n)
    {
        var buf = stackalloc byte[3];
        strFrom16(buf, n);

        str[0] = buf[0];
        str[1] = buf[1];
    }

    public static bool cmsMLUsetASCII(Mlu mlu, ReadOnlySpan<byte> LanguageCode, ReadOnlySpan<byte> CountryCode, ReadOnlySpan<byte> ASCIIString)
    {
        if (mlu is null) return false;

        //var len = (uint)strlen(ASCIIString);
        var len = (uint)ASCIIString.Length;
        var Lang = strTo16(LanguageCode);
        var Cntry = strTo16(CountryCode);

        // len == 0 would prevent operation, so we set a empty string pointing to zero
        if (len is 0)
            len = 1;

        var WStr = _cmsCallocArray<char>(mlu.ContextID, len);
        //if (WStr is null) return false;

        for (var i = 0; i < len; i++)
            //WStr[i] = (char)ASCIIString[i];
            Ascii.ToUtf16(ASCIIString[i..][..1], WStr.AsSpan()[i..][..1], out _);

        var rc = AddMLUBlock(mlu, len * _sizeof<char>(), WStr, Lang, Cntry);

        _cmsFree(mlu.ContextID, WStr);
        return rc;
    }

    private static uint mywcslen(in byte* s)
    {
        var p = s;

        while (*p is not 0)
            p++;

        return (uint)(p - s);
    }

    private static uint mywcslen(in char* s)
    {
        var p = s;

        while (*p is not '\0')
            p++;

        return (uint)(p - s);
    }

    public static bool cmsMLUsetWide(Mlu mlu, ReadOnlySpan<byte> LanguageCode, ReadOnlySpan<byte> CountryCode, ReadOnlySpan<char> WideString)
    {
        if (mlu is null) return false;
        if (WideString.Length is 0) return false;

        var Lang = strTo16(LanguageCode);
        var Cntry = strTo16(CountryCode);

        var len = (uint)WideString.Length * _sizeof<char>();
        if (len is 0)
            len = _sizeof<char>();

        return AddMLUBlock(mlu, len, WideString, Lang, Cntry);
    }

    public static Mlu? cmsMLUdup(Mlu mlu)
    {
        // Duplicating a null obtains a null
        if (mlu is null) return null;

        var NewMlu = cmsMLUalloc(mlu.ContextID, mlu.UsedEntries);
        if (NewMlu is null) return null;

        // Should never happen
        if (NewMlu.AllocatedEntries < mlu.UsedEntries)
            goto Error;

        // Sanitize...
        //if (NewMlu.Entries is null || mlu.Entries is null) goto Error;
        //memmove(NewMlu->Entries, mlu->Entries, mlu->UsedEntries * (uint)sizeof(MluEntry));
        //NewMlu->UsedEntries = mlu->UsedEntries;
        foreach (var entry in mlu.Entries)
            NewMlu.Entries.Add((MluEntry)entry.Clone());

        // The MLU may be empty
        if (mlu.PoolUsedInBytes is 0)
            NewMlu.MemPool = null;
        else
        {
            // It is not empty
            //NewMlu.MemPool = _cmsMalloc(mlu.ContextID, mlu.PoolUsedInBytes);
            NewMlu.MemPool = _cmsCallocArray<char>(mlu.ContextID, mlu.PoolUsedInBytes);
            if (NewMlu.MemPool is null) goto Error;
        }

        NewMlu.PoolSizeInBytes = mlu.PoolSizeInBytes;

        if (NewMlu.MemPool is null || mlu.MemPool is null) goto Error;

        //memmove(NewMlu->MemPool, mlu->MemPool, mlu->PoolUsedInBytes);
        Array.Copy(mlu.MemPool, NewMlu.MemPool, (int)(mlu.PoolUsedInBytes / _sizeof<char>()));
        NewMlu.PoolUsedInBytes = mlu.PoolUsedInBytes;

        return NewMlu;

    Error:
        if (NewMlu is not null) cmsMLUfree(NewMlu);
        return null;
    }

    public static void cmsMLUfree(Mlu? mlu)
    {
        if (mlu is not null)
        {
            //if (mlu.Entries is not null) _cmsFree(mlu.ContextID, mlu.Entries);
            mlu.Entries.Clear();
            if (mlu.MemPool is not null) _cmsFree(mlu.ContextID, mlu.MemPool);

            mlu.MemPool = null;
            mlu.ContextID = null;
            //_cmsFree(mlu->ContextID, mlu);
        }
    }

    internal static Span<char> _cmsMLUgetWide(
        Mlu mlu,
        ushort LanguageCode,
        ushort CountryCode,
        out ushort UsedLanguageCode,
        out ushort UsedCountryCode)
    {
        UsedCountryCode = UsedLanguageCode = 0;

        var Best = -1;
        MluEntry v;

        if (mlu is null) return null;

        if (mlu.AllocatedEntries <= 0) return null;

        for (var i = 0; i < mlu.UsedEntries; i++)
        {
            v = mlu.Entries[i];

            if (v.Language == LanguageCode)
            {
                if (Best is -1) Best = i;

                if (v.Country == CountryCode)
                    break;
                //{
                //    //if (UsedLanguageCode is not null) *UsedLanguageCode = v.Language;
                //    //if (UsedCountryCode is not null) *UsedCountryCode = v.Country;
                //    UsedLanguageCode = v.Language;
                //    UsedCountryCode = v.Country;

                //    //if (len is not null) *len = v.Len;

                //    //return (char*)((byte*)mlu.MemPool + v.StrWCharOffset);      // Found exact match
                //    return mlu.MemPool.AsSpan()[(int)v.StrWCharOffset..][..(int)(v.LenInBytes / _sizeof<char>())];
                //}
            }
        }

        // No string found. Return first one
        if (Best is -1)
            Best = 0;

        v = mlu.Entries[Best];

        //if (UsedLanguageCode is not null) *UsedLanguageCode = v.Language;
        //if (UsedCountryCode is not null) *UsedCountryCode = v.Country;
        UsedLanguageCode = v.Language;
        UsedCountryCode = v.Country;

        //if (len is not null) *len = v->Len;

        //return (char*)((byte*)mlu->MemPool + v->StrWCharOffset);
        return mlu.MemPool.AsSpan()[(int)(v.StrWByteOffset / _sizeof<char>())..][..(int)(v.LenInBytes / _sizeof<char>())];
    }

    public static uint cmsMLUgetASCII(
        Mlu mlu,
        ReadOnlySpan<byte> LanguageCode,
        ReadOnlySpan<byte> CountryCode,
        Span<byte> Buffer)
    {
        var Lang = strTo16(LanguageCode);
        var Cntry = strTo16(CountryCode);
        var BufferSize = (uint)Buffer.Length;

        // Sanitize
        if (mlu is null) return 0;

        // GetWideChar
        var Wide = _cmsMLUgetWide(mlu, Lang, Cntry, out _, out _);
        if (Wide == default) return 0;              // Don't check for null as items in a MLU CAN be zero length!

        var ASCIIlen = (uint)Encoding.ASCII.GetByteCount(Wide);

        // Maybe we want only to know the len?
        //if (Buffer is null) return ASCIIlen + 1; // Note the zero at the end
        if (Buffer.Length is 0) return ASCIIlen;    // We don't add zero at the end when working with Spans!

        // No buffer size means no data
        //if (BufferSize is 0) return 0;    // This is the same as the previous if statement. No longer needed!!

        // Some clipping may be required
        //if (BufferSize < ASCIIlen + 1)
        //    ASCIIlen = BufferSize - 1;            // No more trailing zeros!
        if (BufferSize < ASCIIlen)
            ASCIIlen = BufferSize;

        // Process each character
        for (var i = 0; i < ASCIIlen; i++)
        {
            //Buffer[i] =
            //    Wide[i] is (char)0
            //        ? (byte)0
            //        : (byte)Wide[i];
            Ascii.FromUtf16(Wide[i..][..1], Buffer[i..][..1], out _);
        }

        // We put a termination "\0"
        //Buffer[ASCIIlen] = 0;         // No we don't!!
        //return ASCIIlen + 1;
        return ASCIIlen;
    }

    public static uint cmsMLUgetWide(
        Mlu mlu,
        ReadOnlySpan<byte> LanguageCode,
        ReadOnlySpan<byte> CountryCode,
        Span<char> Buffer)
    {
        var Lang = strTo16(LanguageCode);
        var Cntry = strTo16(CountryCode);
        var BufferSize = (uint)Buffer.Length;

        // Sanitize
        if (mlu is null) return 0;

        // GetWideChar
        var Wide = _cmsMLUgetWide(mlu, Lang, Cntry, out _, out _);
        if (Wide == default) return 0;              // Don't check for null as items in a MLU CAN be zero length!

        var WideLen = (uint)Wide.Length;
        //var StrLen = WideLen * _sizeof<char>();

        // Maybe we want only to know the len?
        //if (Buffer is null) return WideLen + 1; // Note the zero at the end
        if (Buffer.Length is 0) return WideLen;    // We don't add zero at the end when working with Spans!

        // No buffer size means no data
        //if (BufferSize is 0) return 0;    // This is the same as the previous if statement. No longer needed!!

        // Some clipping may be required
        //if (BufferSize < WideLen + 1)
        //    WideLen = BufferSize - 1;            // No more trailing zeros!
        if (BufferSize < WideLen)
            WideLen = BufferSize;

        // Process each character
        //memmove(Buffer, Wide, StrLen);
        //Buffer[StrLen / sizeof(char)] = (char)0;
        Wide.CopyTo(Buffer[..(int)WideLen]);
        return WideLen;
    }

    public static bool cmsMLUgetTranslation(
        Mlu mlu,
        ReadOnlySpan<byte> LanguageCode,
        ReadOnlySpan<byte> CountryCode,
        Span<byte> ObtainedLanguage,
        Span<byte> ObtainedCountry)
    {
        var Lang = strTo16(LanguageCode);
        var Cntry = strTo16(CountryCode);

        // Sanitize
        if (mlu is null) return false;

        var Wide = _cmsMLUgetWide(mlu, Lang, Cntry, out var ObtLang, out var ObtCode);
        if (Wide == default) return false;              // Don't check for null as items in a MLU CAN be zero length!

        // Get used language and code
        strFrom16(ObtainedLanguage, ObtLang);
        strFrom16(ObtainedCountry, ObtCode);

        return true;
    }

    public static uint cmsMLUtranslationsCount(Mlu? mlu) =>
        mlu?.UsedEntries ?? 0;

    private static bool GrowNamedColorList(NamedColorList* v)
    {
        if (v is null) return false;

        var size =
            (v->Allocated is 0)
                ? 64                // Initial guess
                : v->Allocated * 2;

        // Keep a maximum color lists can grow, 100k entries seems reasonable
        if (size > 1024 * 100)
        {
            _cmsFree(v->ContextID, v->List);
            v->List = null;
            return false;
        }

        var NewPtr = _cmsRealloc<NamedColor>(v->ContextID, v->List, size * _sizeof<NamedColor>());
        if (NewPtr is null) return false;

        v->List = NewPtr;
        v->Allocated = size;
        return true;
    }

    public static NamedColorList* cmsAllocNamedColorList(Context? ContextID, uint n, uint ColorantCount, in byte* Prefix, in byte* Suffix)
    {
        var v = _cmsMallocZero<NamedColorList>(ContextID);

        if (v is null) return null;

        v->List = null;
        v->nColors = 0;
        v->ContextID = ContextID;

        while (v->Allocated < n)
        {
            if (!GrowNamedColorList(v))
            {
                cmsFreeNamedColorList(v);
                return null;
            }
        }

        strncpy(v->Prefix, Prefix, 32);
        strncpy(v->Suffix, Suffix, 32);
        v->Prefix[32] = v->Suffix[32] = 0;

        v->ColorantCount = ColorantCount;

        return v;
    }

    public static void cmsFreeNamedColorList(NamedColorList* v)
    {
        if (v is not null && v->List is not null)
            _cmsFree(v->ContextID, v->List);
        _cmsFree(v->ContextID, v);
    }

    public static NamedColorList* cmsDupNamedColorList(in NamedColorList* v)
    {
        if (v is null) return null;

        var NewNC = cmsAllocNamedColorList(v->ContextID, v->nColors, v->ColorantCount, v->Prefix, v->Suffix);
        if (NewNC is null) return null;

        // For really large tables we need this
        while (NewNC->Allocated < v->Allocated)
        {
            if (!GrowNamedColorList(NewNC))
            {
                cmsFreeNamedColorList(NewNC);
                return null;
            }
        }

        memmove(NewNC->Prefix, v->Prefix, 33);
        memmove(NewNC->Suffix, v->Suffix, 33);
        NewNC->ColorantCount = v->ColorantCount;
        memmove(NewNC->List, v->List, v->nColors * _sizeof<NamedColor>());
        NewNC->nColors = v->nColors;
        return NewNC;
    }

    public static bool cmsAppendNamedColor(
        NamedColorList* NamedColorList,
        in byte* Name,
        ushort* PCS,
        ushort* Colorant)
    {
        if (NamedColorList is null) return false;

        if (NamedColorList->nColors + 1 > NamedColorList->Allocated && !GrowNamedColorList(NamedColorList))
            return false;

        var idx = NamedColorList->nColors;
        for (var i = 0; i < NamedColorList->ColorantCount; i++)
            NamedColorList->List[idx].DeviceColorant[i] = Colorant is null ? (ushort)0 : Colorant[i];

        for (var i = 0; i < 3; i++)
            NamedColorList->List[idx].PCS[i] = PCS is null ? (ushort)0 : PCS[i];

        if (Name is not null)
        {
            strncpy(NamedColorList->List[idx].Name, Name, cmsMAX_PATH - 1);
            NamedColorList->List[idx].Name[cmsMAX_PATH - 1] = 0;
        }
        else
        {
            NamedColorList->List[idx].Name[0] = 0;
        }

        NamedColorList->nColors++;
        return true;
    }

    public static uint cmsNamedColorCount(in NamedColorList* NamedColorList) =>
        NamedColorList is not null
            ? NamedColorList->nColors
            : 0;

    public static bool cmsNamedColorInfo(
        in NamedColorList* NamedColorList,
        uint nColor,
        byte* Name,
        byte* Prefix,
        byte* Suffix,
        ushort* PCS,
        ushort* Colorant)
    {
        if (NamedColorList is null) return false;

        if (nColor >= cmsNamedColorCount(NamedColorList)) return false;

        // strcpy instead of strncpy because many apps are using small buffers
        if (Name is not null) strcpy(Name, NamedColorList->List[nColor].Name);
        if (Prefix is not null) strcpy(Prefix, NamedColorList->Prefix);
        if (Suffix is not null) strcpy(Suffix, NamedColorList->Suffix);
        if (PCS is not null)
            memmove(PCS, NamedColorList->List[nColor].PCS, 3 * _sizeof<ushort>());
        if (Colorant is not null)
            memmove(Colorant, NamedColorList->List[nColor].DeviceColorant, NamedColorList->ColorantCount * _sizeof<ushort>());

        return true;
    }

    public static int cmsNamedColorIndex(in NamedColorList* NamedColorList, in byte* Name)
    {
        if (NamedColorList is null) return -1;
        var n = cmsNamedColorCount(NamedColorList);
        for (var i = 0; i < n; i++)
        {
            if (cmsstrcasecmp(Name, NamedColorList->List[i].Name) == 0) return i;
        }

        return -1;
    }

    private static void FreeNamedColorList(Stage mpe)
    {
        if (mpe.Data is BoxPtr<NamedColorList> list)
            cmsFreeNamedColorList(list);
    }

    private static object? DupNamedColorList(Stage mpe) =>
        (mpe.Data is BoxPtr<NamedColorList> list)
            ? new BoxPtr<NamedColorList>(cmsDupNamedColorList(list))
            : null;

    private static void EvalNamedColorPCS(in float* In, float* Out, Stage mpe)
    {
        if (mpe.Data is not BoxPtr<NamedColorList> NamedColorList)
            return;
        var index = _cmsQuickSaturateWord(In[0] * 65535.0);

        if (index >= NamedColorList.Ptr->nColors)
        {
            cmsSignalError(NamedColorList.Ptr->ContextID, ErrorCode.Range, $"Color {index} out of range");
            Out[0] = Out[1] = Out[2] = 0f;
        }
        else
        {
            // Named color always uses Lab
            Out[0] = (float)(NamedColorList.Ptr->List[index].PCS[0] / 65535.0);
            Out[1] = (float)(NamedColorList.Ptr->List[index].PCS[1] / 65535.0);
            Out[2] = (float)(NamedColorList.Ptr->List[index].PCS[2] / 65535.0);
        }
    }

    private static void EvalNamedColor(in float* In, float* Out, Stage mpe)
    {
        if (mpe.Data is not BoxPtr<NamedColorList> NamedColorList)
            return;
        var index = _cmsQuickSaturateWord(In[0] * 65535.0);

        if (index >= NamedColorList.Ptr->nColors)
        {
            cmsSignalError(NamedColorList.Ptr->ContextID, ErrorCode.Range, $"Color {index} out of range");
            for (var j = 0; j < NamedColorList.Ptr->ColorantCount; j++)
                Out[j] = 0.0f;
        }
        else
        {
            for (var j = 0; j < NamedColorList.Ptr->ColorantCount; j++)
                Out[j] = (float)(NamedColorList.Ptr->List[index].DeviceColorant[j] / 65535.0);
        }
    }

    internal static Stage? _cmsStageAllocNamedColor(NamedColorList* NamedColorList, bool UsePCS) =>
        _cmsStageAllocPlaceholder(
            NamedColorList->ContextID,
            cmsSigNamedColorElemType,
            1,
            UsePCS ? 3 : NamedColorList->ColorantCount,
            UsePCS ? EvalNamedColorPCS : EvalNamedColor,
            DupNamedColorList,
            FreeNamedColorList,
            new BoxPtr<NamedColorList>(cmsDupNamedColorList(NamedColorList)));

    public static BoxPtr<NamedColorList>? cmsGetNamedColorList(Transform* xform)
    {
        var mpe = xform->Lut->Elements;

        return (mpe is not null && mpe.Type == cmsSigNamedColorElemType)
            ? mpe.Data as BoxPtr<NamedColorList>
            : null;
    }

    public static Sequence* cmsAllocProfileSequenceDescription(Context? ContextID, uint n)
    {
        // In a absolutely arbitrary way, I hereby decide to allow a maxim of 255 profiles linked
        // in a devicelink. It makes not sense anyway and may be used for exploits, so let's close the door!
        if (n is 0 or > 255) return null;

        var Seq = _cmsMallocZero<Sequence>(ContextID);
        if (Seq is null) return null;

        Seq->ContextID = ContextID;
        Seq->seq = _cmsCalloc<ProfileSequenceDescription>(ContextID, n);
        Seq->n = n;

        if (Seq->seq is null)
        {
            _cmsFree(ContextID, Seq);
            return null;
        }

        for (var i = 0; i < n; i++)
        {
            Seq->seq[i].Manufacturer = null;
            Seq->seq[i].Model = null;
            Seq->seq[i].Description = null;
        }

        return Seq;
    }

    public static void cmsFreeProfileSequenceDescription(Sequence* pseq)
    {
        if (pseq is null) return;

        for (var i = 0; i < pseq->n; i++)
        {
            if (pseq->seq[i].Manufacturer is not null)
                cmsMLUfree(pseq->seq[i].Manufacturer);
            if (pseq->seq[i].Model is not null)
                cmsMLUfree(pseq->seq[i].Model);
            if (pseq->seq[i].Description is not null)
                cmsMLUfree(pseq->seq[i].Description);
        }

        if (pseq->seq is not null)
            _cmsFree(pseq->ContextID, pseq->seq);
        _cmsFree(pseq->ContextID, pseq);
    }

    public static Sequence* cmsDupProfileSequenceDescription(in Sequence* pseq)
    {
        if (pseq is null) return null;

        var NewSeq = _cmsMalloc<Sequence>(pseq->ContextID);
        if (NewSeq is null) return null;

        NewSeq->seq = _cmsCalloc<ProfileSequenceDescription>(pseq->ContextID, pseq->n);
        if (NewSeq->seq is null) goto Error;

        NewSeq->ContextID = pseq->ContextID;
        NewSeq->n = pseq->n;

        for (var i = 0; i < pseq->n; i++)
        {
            memmove(&NewSeq->seq[i].attributes, &pseq->seq[i].attributes);

            NewSeq->seq[i].deviceMfg = pseq->seq[i].deviceMfg;
            NewSeq->seq[i].deviceModel = pseq->seq[i].deviceModel;
            memmove(&NewSeq->seq[i].ProfileID, &pseq->seq[i].ProfileID);
            NewSeq->seq[i].technology = pseq->seq[i].technology;

            NewSeq->seq[i].Manufacturer = cmsMLUdup(pseq->seq[i].Manufacturer);
            NewSeq->seq[i].Model = cmsMLUdup(pseq->seq[i].Model);
            NewSeq->seq[i].Description = cmsMLUdup(pseq->seq[i].Description);
        }

        return NewSeq;

    Error:
        cmsFreeProfileSequenceDescription(NewSeq);
        return null;
    }

    public static void* cmsDictAlloc(Context? ContextID)
    {
        var dict = _cmsMallocZero<Dictionary>(ContextID);
        if (dict is null) return null;

        dict->ContextID = ContextID;
        return dict;
    }

    public static void cmsDictFree(void* hDict)
    {
        var dict = (Dictionary*)hDict;

        _cmsAssert(dict);

        // Walk the list freeing all nodes
        var entry = dict->head;
        while (entry is not null)
        {
            if (entry->DisplayName is not null) cmsMLUfree(entry->DisplayName);
            if (entry->DisplayValue is not null) cmsMLUfree(entry->DisplayValue);
            if (entry->Name is not null) _cmsFree(dict->ContextID, entry->Name);
            if (entry->Value is not null) _cmsFree(dict->ContextID, entry->Value);

            // Don't fall in the habitual trap...
            var next = entry->Next;
            _cmsFree(dict->ContextID, entry);

            entry = next;
        }

        _cmsFree(dict->ContextID, dict);
    }

    private static char* DupWcs(Context? ContextID, in char* ptr)
    {
        if (ptr is null) return null;
        return _cmsDupMem<char>(ContextID, ptr, mywcslen(ptr) + 1);
    }

    public static bool cmsDictAddEntry(void* hDict, in char* Name, in char* Value, Mlu DisplayName, Mlu DisplayValue)
    {
        var dict = (Dictionary*)hDict;

        _cmsAssert(dict);
        _cmsAssert(Name);

        var entry = _cmsMallocZero<Dictionary.Entry>(dict->ContextID);
        if (entry is null) return false;

        entry->DisplayName = cmsMLUdup(DisplayName);
        entry->DisplayValue = cmsMLUdup(DisplayValue);
        entry->Name = DupWcs(dict->ContextID, Name);
        entry->Value = DupWcs(dict->ContextID, Value);

        entry->Next = dict->head;
        dict->head = entry;

        return true;
    }

    public static void* cmsDictDup(in void* hDict)
    {
        var old_dict = (Dictionary*)hDict;

        _cmsAssert(old_dict);

        var hNew = cmsDictAlloc(old_dict->ContextID);
        if (hNew is null) return null;

        // Walk the list
        var entry = old_dict->head;
        while (entry is not null)
        {
            if (!cmsDictAddEntry(hNew, entry->Name, entry->Value, entry->DisplayName, entry->DisplayValue))
            {
                cmsDictFree(hNew);
                return null;
            }

            entry = entry->Next;
        }

        return hNew;
    }

    public static Dictionary.Entry* cmsDictGetEntryList(void* hDict) =>
        (Dictionary*)hDict is not null
            ? ((Dictionary*)hDict)->head
            : null;

    public static Dictionary.Entry* cmsDictNextEntry(in Dictionary.Entry* e) =>
        (e is not null)
            ? e->Next
            : null;
}
