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

namespace lcms2;

public static unsafe partial class Lcms2
{
    public static Mlu* cmsMLUalloc(Context? ContextID, uint nItems)
    {
        if (nItems is 0)
            nItems = 2;

        // Create the container
        var mlu = _cmsMallocZero<Mlu>(ContextID);
        if (mlu is null) return null;

        mlu->ContextID = ContextID;

        // Create entry array
        mlu->Entries = _cmsCalloc<MluEntry>(ContextID, nItems);
        if (mlu->Entries is null)
        {
            _cmsFree(ContextID, mlu);
            return null;
        }

        // Ok, keep indexes up to date
        mlu->AllocatedEntries = nItems;
        mlu->UsedEntries = 0;

        return mlu;
    }

    private static bool GrowMLUpool(Mlu* mlu)
    {
        // Sanity check
        if (mlu is null) return false;

        var size =
            mlu->PoolSize is 0
                ? 256u
                : mlu->PoolSize * 2;

        // Check for overflow
        if (size < mlu->PoolSize) return false;

        // Reallocate the pool
        var NewPtr = _cmsRealloc(mlu->ContextID, mlu->MemPool, size);
        if (NewPtr is null) return false;

        mlu->MemPool = NewPtr;
        mlu->PoolSize = size;

        return true;
    }

    private static bool GrowMLUtable(Mlu* mlu)
    {
        // Sanity check
        if (mlu is null) return false;

        var AllocatedEntries = mlu->AllocatedEntries * 2;

        // Check for overflow
        if (AllocatedEntries / 2 != mlu->AllocatedEntries) return false;

        // Reallocate the memory
        var NewPtr = _cmsRealloc<MluEntry>(mlu->ContextID, mlu->Entries, AllocatedEntries * (uint)sizeof(MluEntry));
        if (NewPtr is null) return false;

        mlu->Entries = NewPtr;
        mlu->AllocatedEntries = AllocatedEntries;

        return true;
    }

    private static int SearchMLUEntry(Mlu* mlu, ushort LanguageCode, ushort CountryCode)
    {
        // Sanity Check
        if (mlu is null) return -1;

        // Iterate whole table
        for (var i = 0; i < mlu->UsedEntries; i++)
        {
            if (mlu->Entries[i].Country == CountryCode &&
                mlu->Entries[i].Language == LanguageCode) return i;
        }

        // Not found
        return -1;
    }

    private static bool AddMLUBlock(Mlu* mlu, uint size, in char* Block, ushort LanguageCode, ushort CountryCode)
    {
        // Sanity Check
        if (mlu is null) return false;

        // Is there any room available?
        if (mlu->UsedEntries >= mlu->AllocatedEntries)
        {
            if (!GrowMLUtable(mlu)) return false;
        }

        // Only one ASCII string
        if (SearchMLUEntry(mlu, LanguageCode, CountryCode) >= 0) return false;

        // Check for size
        while ((mlu->PoolSize - mlu->PoolUsed) < size)
            if (!GrowMLUpool(mlu)) return false;

        var Offset = mlu->PoolUsed;

        var Ptr = (byte*)mlu->MemPool;
        if (Ptr is null) return false;

        // Set the entry
        memmove(Ptr + Offset, Block, size);
        mlu->PoolUsed += size;

        mlu->Entries[mlu->UsedEntries].StrW = Offset;
        mlu->Entries[mlu->UsedEntries].Len = size;
        mlu->Entries[mlu->UsedEntries].Country = CountryCode;
        mlu->Entries[mlu->UsedEntries].Language = LanguageCode;
        mlu->UsedEntries++;

        return true;
    }

    private static ushort strTo16(in byte* str)
    {
        // for non-existent strings
        if (str is null) return 0;
        return (ushort)((str[0] << 8) | str[1]);
    }

    private static ushort strTo16(ReadOnlySpan<byte> str)
    {
        fixed (byte* strPtr = str)
            return strTo16(str);
    }

    private static void strFrom16(byte* str, ushort n)
    {
        str[0] = (byte)(n >> 8);
        str[1] = (byte)n;
        str[2] = 0;
    }

    public static bool cmsMLUsetASCII(Mlu* mlu, in byte* LanguageCode, in byte* CountryCode, in byte* ASCIIString)
    {
        var len = (uint)strlen(ASCIIString);
        var Lang = strTo16(LanguageCode);
        var Cntry = strTo16(CountryCode);

        if (mlu is null) return false;

        // len == 0 would prevent operation, so we set a empty string pointing to zero
        if (len is 0)
            len = 1;

        var WStr = _cmsCalloc<char>(mlu->ContextID, len);
        if (WStr is null) return false;

        for (var i = 0; i < len; i++)
            WStr[i] = (char)ASCIIString[i];

        var rc = AddMLUBlock(mlu, len * sizeof(char), WStr, Lang, Cntry);

        _cmsFree(mlu->ContextID, WStr);
        return rc;
    }

    private static uint mywcslen(in char* s)
    {
        var p = s;

        while (*p is not '\0')
            p++;

        return (uint)(p - s);
    }

    public static bool cmsMLUsetWide(Mlu* mlu, in byte* LanguageCode, in byte* CountryCode, in char* WideString)
    {
        var Lang = strTo16(LanguageCode);
        var Cntry = strTo16(CountryCode);

        if (mlu is null) return false;
        if (WideString is null) return false;

        var len = mywcslen(WideString) * sizeof(char);
        if (len is 0)
            len = sizeof(char);

        return AddMLUBlock(mlu, len, WideString, Lang, Cntry);
    }

    public static Mlu* cmsMLUdup(in Mlu* mlu)
    {
        // Duplicating a null obtains a null
        if (mlu is null) return null;

        var NewMlu = cmsMLUalloc(mlu->ContextID, mlu->UsedEntries);
        if (NewMlu is null) return null;

        // Should never happen
        if (NewMlu->AllocatedEntries < mlu->UsedEntries)
            goto Error;

        // Sanitize...
        if (NewMlu->Entries is null || mlu->Entries is null) goto Error;
        memmove(NewMlu->Entries, mlu->Entries, mlu->UsedEntries * (uint)sizeof(MluEntry));
        NewMlu->UsedEntries = mlu->UsedEntries;

        // Thye MLU may be empty
        if (mlu->PoolUsed is 0)
            NewMlu->MemPool = null;
        else
        {
            // It is not empty
            NewMlu->MemPool = _cmsMalloc(mlu->ContextID, mlu->PoolUsed);
            if (NewMlu->MemPool is null) goto Error;
        }

        NewMlu->PoolSize = mlu->PoolSize;

        if (NewMlu->MemPool is null || mlu->MemPool is null) goto Error;

        memmove(NewMlu->MemPool, mlu->MemPool, mlu->PoolUsed);
        NewMlu->PoolUsed = mlu->PoolUsed;

        return NewMlu;

    Error:
        if (NewMlu is not null) cmsMLUfree(NewMlu);
        return null;
    }

    public static void cmsMLUfree(Mlu* mlu)
    {
        if (mlu is not null)
        {
            if (mlu->Entries is not null) _cmsFree(mlu->ContextID, mlu->Entries);
            if (mlu->MemPool is not null) _cmsFree(mlu->ContextID, mlu->MemPool);

            _cmsFree(mlu->ContextID, mlu);
        }
    }

    internal static char* _cmsMLUgetWide(
        in Mlu* mlu,
        uint* len,
        ushort LanguageCode,
        ushort CountryCode,
        ushort* UsedLanguageCode,
        ushort* UsedCountryCode)
    {
        MluEntry* v;
        var Best = -1;

        if (mlu is null) return null;

        if (mlu->AllocatedEntries <= 0) return null;

        for (var i = 0; i < mlu->UsedEntries; i++)
        {
            v = mlu->Entries + i;

            if (v->Language == LanguageCode)
            {
                if (Best is -1) Best = i;

                if (v->Country == CountryCode)
                {
                    if (UsedLanguageCode is not null) *UsedLanguageCode = v->Language;
                    if (UsedCountryCode is not null) *UsedCountryCode = v->Country;

                    if (len is not null) *len = v->Len;

                    return (char*)((byte*)mlu->MemPool + v->StrW);      // Found exact match
                }
            }
        }

        // No string found. Return first one
        if (Best is -1)
            Best = 0;

        v = mlu->Entries + Best;

        if (UsedLanguageCode is not null) *UsedLanguageCode = v->Language;
        if (UsedCountryCode is not null) *UsedCountryCode = v->Country;

        if (len is not null) *len = v->Len;

        return (char*)((byte*)mlu->MemPool + v->StrW);
    }

    public static uint cmsMLUgetASCII(
        in Mlu* mlu,
        in byte* LanguageCode,
        in byte* CountryCode,
        byte* Buffer,
        uint BufferSize)
    {
        var StrLen = 0u;
        var Lang = strTo16(LanguageCode);
        var Cntry = strTo16(CountryCode);

        // Sanitize
        if (mlu is null) return 0;

        // GetWideChar
        var Wide = _cmsMLUgetWide(mlu, &StrLen, Lang, Cntry, null, null);
        if (Wide is null) return 0;

        var ASCIIlen = StrLen / sizeof(char);

        // Maybe we want only to know the len?
        if (Buffer is null) return ASCIIlen + 1; // Note the zero at the end

        // No buffer size means no data
        if (BufferSize is 0) return 0;

        // Some clipping may be required
        if (BufferSize < ASCIIlen + 1)
            ASCIIlen = BufferSize - 1;

        // Process each character
        for (var i = 0; i < ASCIIlen; i++)
        {
            Buffer[i] =
                Wide[i] is (char)0
                    ? (byte)0
                    : (byte)Wide[i];
        }

        // We put a termination "\0"
        Buffer[ASCIIlen] = 0;
        return ASCIIlen + 1;
    }

    public static uint cmsMLUgetWide(
        in Mlu* mlu,
        in byte* LanguageCode,
        in byte* CountryCode,
        char* Buffer,
        uint BufferSize)
    {
        var StrLen = 0u;
        var Lang = strTo16(LanguageCode);
        var Cntry = strTo16(CountryCode);

        // Sanitize
        if (mlu is null) return 0;

        // GetWideChar
        var Wide = _cmsMLUgetWide(mlu, &StrLen, Lang, Cntry, null, null);
        if (Wide is null) return 0;

        var WideLen = StrLen / sizeof(char);

        // Maybe we want only to know the len?
        if (Buffer is null) return WideLen; // Note the zero at the end

        // No buffer size means no data
        if (BufferSize is 0) return 0;

        // Some clipping may be required
        if (BufferSize < WideLen)
            WideLen = BufferSize - sizeof(char);

        // Process each character
        memmove(Buffer, Wide, StrLen);
        Buffer[StrLen / sizeof(char)] = (char)0;
        return WideLen;
    }

    public static bool cmsMLUgetTranslation(
        in Mlu* mlu,
        byte* LanguageCode,
        byte* CountryCode,
        byte* ObtainedLanguage,
        byte* ObtainedCountry)
    {
        var Lang = strTo16(LanguageCode);
        var Cntry = strTo16(CountryCode);
        ushort ObtLang, ObtCode;

        // Sanitize
        if (mlu is null) return false;

        var Wide = _cmsMLUgetWide(mlu, null, Lang, Cntry, &ObtLang, &ObtCode);
        if (Wide is null) return false;

        // Get used language and code
        strFrom16(ObtainedLanguage, ObtLang);
        strFrom16(ObtainedCountry, ObtCode);

        return true;
    }

    public static uint cmsMLUtranslationsCount(in Mlu* mlu) =>
        (mlu is null)
            ? 0
            : mlu->UsedEntries;

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

        var NewPtr = _cmsRealloc<NamedColor>(v->ContextID, v->List, size * (uint)sizeof(NamedColor));
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
        memmove(NewNC->List, v->List, v->nColors * (uint)sizeof(NamedColor));
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
            memmove(PCS, NamedColorList->List[nColor].PCS, 3 * sizeof(ushort));
        if (Colorant is not null)
            memmove(Colorant, NamedColorList->List[nColor].DeviceColorant, NamedColorList->ColorantCount * sizeof(ushort));

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

    private static void FreeNamedColorList(Stage* mpe)
    {
        var list = (BoxPtr<NamedColorList>)mpe->Data;
        cmsFreeNamedColorList(list);
    }

    private static object? DupNamedColorList(Stage* mpe)
    {
        var list = (BoxPtr<NamedColorList>)mpe->Data;
        return new BoxPtr<NamedColorList>(cmsDupNamedColorList(list));
    }

    private static void EvalNamedColorPCS(in float* In, float* Out, in Stage* mpe)
    {
        var NamedColorList = (BoxPtr<NamedColorList>)mpe->Data;
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

    private static void EvalNamedColor(in float* In, float* Out, in Stage* mpe)
    {
        var NamedColorList = (BoxPtr<NamedColorList>)mpe->Data;
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

    internal static Stage* _cmsStageAllocNamedColor(NamedColorList* NamedColorList, bool UsePCS) =>
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

        if (mpe->Type != cmsSigNamedColorElemType) return null;
        return mpe->Data as BoxPtr<NamedColorList>;
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

    public static bool cmsDictAddEntry(void* hDict, in char* Name, in char* Value, in Mlu* DisplayName, in Mlu* DisplayValue)
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
