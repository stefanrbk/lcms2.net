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

using lcms2.io;
using lcms2.state;
using lcms2.types;

using System.Runtime.CompilerServices;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private const uint cmsCorbisBrokenXYZtype = 0x17A505B8;
    private const uint cmsMonacoBrokenCurveType = 0x9478EE00;

    internal static readonly TagTypeLinkedList supportedMPEtypes = TagTypeLinkedList.Build(
        new(cmsSigBAcsElemType, null, null, null, null, null, 0),
        new(cmsSigEAcsElemType, null, null, null, null, null, 0),
        new(cmsSigCurveSetElemType, Type_MPEcurve_Read, Type_MPEcurve_Write, GenericMPEdup, GenericMPEfree, null, 0),
        new(cmsSigMatrixElemType, Type_MPEmatrix_Read, Type_MPEmatrix_Write, GenericMPEdup, GenericMPEfree, null, 0),
        new(cmsSigCLutElemType, Type_MPEclut_Read, Type_MPEclut_Write, GenericMPEdup, GenericMPEfree, null, 0));

    internal static readonly TagTypePluginChunkType MPETypePluginChunk = new();

    internal static readonly TagTypePluginChunkType globalMPETypePluginChunk = new();

    internal static readonly TagTypeLinkedList supportedTagTypes = TagTypeLinkedList.Build(
        new(cmsSigChromaticityType, Type_Chromaticity_Read, Type_Chromaticity_Write, Type_Chromaticity_Dup, Type_Chromaticity_Free, null, 0),
        new(cmsSigColorantOrderType, Type_ColorantOrderType_Read, Type_ColorantOrderType_Write, Type_ColorantOrderType_Dup, Type_ColorantOrderType_Free, null, 0),
        new(cmsSigS15Fixed16ArrayType, Type_S15Fixed16_Read, Type_S15Fixed16_Write, Type_S15Fixed16_Dup, Type_S15Fixed16_Free, null, 0),
        new(cmsSigU16Fixed16ArrayType, Type_U16Fixed16_Read, Type_U16Fixed16_Write, Type_U16Fixed16_Dup, Type_U16Fixed16_Free, null, 0),
        new(cmsSigTextType, Type_Text_Read, Type_Text_Write, Type_Text_Dup, Type_Text_Free, null, 0),
        new(cmsSigTextDescriptionType, Type_Text_Description_Read, Type_Text_Description_Write, Type_Text_Description_Dup, Type_Text_Description_Free, null, 0),
        new(cmsSigCurveType, Type_Curve_Read, Type_Curve_Write, ToneCurve_Dup, ToneCurve_Free, null, 0),
        new(cmsSigParametricCurveType, Type_ParametricCurve_Read, Type_ParametricCurve_Write, ToneCurve_Dup, ToneCurve_Free, null, 0),
        new(cmsSigDateTimeType, Type_DateTime_Read, Type_DateTime_Write, Type_DateTime_Dup, Type_DateTime_Free, null, 0),
        new(cmsSigLut8Type, Type_LUT8_Read, Type_LUT8_Write, Type_LUT8_Dup, Type_LUT8_Free, null, 0),
        new(cmsSigLut16Type, Type_LUT16_Read, Type_LUT16_Write, Type_LUT16_Dup, Type_LUT16_Free, null, 0),
        new(cmsSigColorantTableType, Type_ColorantTable_Read, Type_ColorantTable_Write, NamedColor_Dup, NamedColor_Free, null, 0),
        new(cmsSigNamedColor2Type, Type_NamedColor_Read, Type_NamedColor_Write, NamedColor_Dup, NamedColor_Free, null, 0),
        new(cmsSigMultiLocalizedUnicodeType, Type_MLU_Read, Type_MLU_Write, Type_MLU_Dup, Type_MLU_Free, null, 0),
        new(cmsSigProfileSequenceDescType, Type_ProfileSequenceDesc_Read, Type_ProfileSequenceDesc_Write, Sequence_Dup, Sequence_Free, null, 0),
        new(cmsSigSignatureType, Type_Signature_Read, Type_Signature_Write, Type_Signature_Dup, Type_Signature_Free, null, 0),
        new(cmsSigMeasurementType, Type_Measurement_Read, Type_Measurement_Write, Type_Measurement_Dup, Type_Measurement_Free, null, 0),
        new(cmsSigDataType, Type_Data_Read, Type_Data_Write, Type_Data_Dup, Type_Data_Free, null, 0),
        new(cmsSigLutAtoBType, Type_LUTA2B_Read, Type_LUTA2B_Write, Type_LUTA2B_Dup, Type_LUTA2B_Free, null, 0),
        new(cmsSigLutBtoAType, Type_LUTB2A_Read, Type_LUTB2A_Write, Type_LUTB2A_Dup, Type_LUTB2A_Free, null, 0),
        new(cmsSigUcrBgType, Type_UcrBg_Read, Type_UcrBg_Write, Type_UcrBg_Dup, Type_UcrBg_Free, null, 0),
        new(cmsSigCrdInfoType, Type_CrdInfo_Read, Type_CrdInfo_Write, Type_CrdInfo_Dup, Type_CrdInfo_Free, null, 0),
        new(cmsSigMultiProcessElementType, Type_MPE_Read, Type_MPE_Write, Type_MPE_Dup, Type_MPE_Free, null, 0),
        new(cmsSigScreeningType, Type_Screening_Read, Type_Screening_Write, Type_Screening_Dup, Type_Screening_Free, null, 0),
        new(cmsSigViewingConditionsType, Type_ViewingConditions_Read, Type_ViewingConditions_Write, Type_ViewingConditions_Dup, Type_ViewingConditions_Free, null, 0),
        new(cmsSigXYZType, Type_XYZ_Read, Type_XYZ_Write, Type_XYZ_Dup, Type_XYZ_Free, null, 0),
        new(cmsCorbisBrokenXYZtype, Type_XYZ_Read, Type_XYZ_Write, Type_XYZ_Dup, Type_XYZ_Free, null, 0),
        new(cmsMonacoBrokenCurveType, Type_Curve_Read, Type_Curve_Write, ToneCurve_Dup, ToneCurve_Free, null, 0),
        new(cmsSigProfileSequenceIdType, Type_ProfileSequenceId_Read, Type_ProfileSequenceId_Write, Sequence_Dup, Sequence_Free, null, 0),
        new(cmsSigDictType, Type_Dictionary_Read, Type_Dictionary_Write, Type_Dictionary_Dup, Type_Dictionary_Free, null, 0),
        new(cmsSigVcgtType, Type_vcgt_Read, Type_vcgt_Write, Type_vcgt_Dup, Type_vcgt_Free, null, 0));

    internal static readonly TagTypePluginChunkType TagTypePluginChunk = new();

    internal static readonly TagTypePluginChunkType globalTagTypePluginChunk = new();

    internal static readonly TagLinkedList supportedTags = TagLinkedList.Build(
        new(cmsSigAToB0Tag, 1, new Signature[] { cmsSigLut16Type, cmsSigLutAtoBType, cmsSigLut8Type, }, DecideLUTtypeA2B),
        new(cmsSigAToB1Tag, 1, new Signature[] { cmsSigLut16Type, cmsSigLutAtoBType, cmsSigLut8Type, }, DecideLUTtypeA2B),
        new(cmsSigAToB2Tag, 1, new Signature[] { cmsSigLut16Type, cmsSigLutAtoBType, cmsSigLut8Type, }, DecideLUTtypeA2B),
        new(cmsSigBToA0Tag, 1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A),
        new(cmsSigBToA1Tag, 1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A),
        new(cmsSigBToA2Tag, 1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A),
        new(cmsSigRedColorantTag, 1, new Signature[] { cmsSigXYZType, cmsCorbisBrokenXYZtype, }, DecideXYZtype),
        new(cmsSigGreenColorantTag, 1, new Signature[] { cmsSigXYZType, cmsCorbisBrokenXYZtype, }, DecideXYZtype),
        new(cmsSigBlueColorantTag, 1, new Signature[] { cmsSigXYZType, cmsCorbisBrokenXYZtype, }, DecideXYZtype),
        new(cmsSigRedTRCTag, 1, new Signature[] { cmsSigCurveType, cmsSigParametricCurveType, cmsMonacoBrokenCurveType, }, DecideCurveType),
        new(cmsSigGreenTRCTag, 1, new Signature[] { cmsSigCurveType, cmsSigParametricCurveType, cmsMonacoBrokenCurveType, }, DecideCurveType),
        new(cmsSigBlueTRCTag, 1, new Signature[] { cmsSigCurveType, cmsSigParametricCurveType, cmsMonacoBrokenCurveType, }, DecideCurveType),
        new(cmsSigCalibrationDateTimeTag, 1, new Signature[] { cmsSigDateTimeType, }, null),
        new(cmsSigCharTargetTag, 1, new Signature[] { cmsSigTextType, }, null),
        new(cmsSigChromaticAdaptationTag, 9, new Signature[] { cmsSigS15Fixed16ArrayType, }, null),
        new(cmsSigChromaticityTag, 1, new Signature[] { cmsSigChromaticityType, }, null),
        new(cmsSigColorantOrderTag, 1, new Signature[] { cmsSigColorantOrderType, }, null),
        new(cmsSigColorantTableTag, 1, new Signature[] { cmsSigColorantTableType, }, null),
        new(cmsSigColorantTableOutTag, 1, new Signature[] { cmsSigColorantTableType, }, null),
        new(cmsSigCopyrightTag, 1, new Signature[] { cmsSigTextType, cmsSigMultiLocalizedUnicodeType, cmsSigTextDescriptionType, }, DecideTextType),
        new(cmsSigDateTimeTag, 1, new Signature[] { cmsSigDateTimeType, }, null),
        new(cmsSigDeviceMfgDescTag, 1, new Signature[] { cmsSigTextDescriptionType, cmsSigMultiLocalizedUnicodeType, cmsSigTextType, }, DecideTextDescType),
        new(cmsSigDeviceModelDescTag, 1, new Signature[] { cmsSigTextDescriptionType, cmsSigMultiLocalizedUnicodeType, cmsSigTextType, }, DecideTextDescType),
        new(cmsSigGamutTag, 1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A),
        new(cmsSigGrayTRCTag, 1, new Signature[] { cmsSigCurveType, cmsSigParametricCurveType, }, DecideCurveType),
        new(cmsSigLuminanceTag, 1, new Signature[] { cmsSigXYZType, }, null),
        new(cmsSigMediaBlackPointTag, 1, new Signature[] { cmsSigXYZType, cmsCorbisBrokenXYZtype, }, null),
        new(cmsSigMediaWhitePointTag, 1, new Signature[] { cmsSigXYZType, cmsCorbisBrokenXYZtype, }, null),
        new(cmsSigNamedColor2Tag, 1, new Signature[] { cmsSigNamedColor2Type, }, null),
        new(cmsSigPreview0Tag, 1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A),
        new(cmsSigPreview1Tag, 1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A),
        new(cmsSigPreview2Tag, 1, new Signature[] { cmsSigLut16Type, cmsSigLutBtoAType, cmsSigLut8Type, }, DecideLUTtypeB2A),
        new(cmsSigProfileDescriptionTag, 1, new Signature[] { cmsSigTextDescriptionType, cmsSigMultiLocalizedUnicodeType, cmsSigTextType, }, DecideTextDescType),
        new(cmsSigProfileSequenceDescTag, 1, new Signature[] { cmsSigProfileSequenceDescType, }, null),
        new(cmsSigTechnologyTag, 1, new Signature[] { cmsSigSignatureType, }, null),
        new(cmsSigColorimetricIntentImageStateTag, 1, new Signature[] { cmsSigSignatureType, }, null),
        new(cmsSigPerceptualRenderingIntentGamutTag, 1, new Signature[] { cmsSigSignatureType, }, null),
        new(cmsSigSaturationRenderingIntentGamutTag, 1, new Signature[] { cmsSigSignatureType, }, null),
        new(cmsSigMeasurementTag, 1, new Signature[] { cmsSigMeasurementType, }, null),
        new(cmsSigPs2CRD0Tag, 1, new Signature[] { cmsSigDataType, }, null),
        new(cmsSigPs2CRD1Tag, 1, new Signature[] { cmsSigDataType, }, null),
        new(cmsSigPs2CRD2Tag, 1, new Signature[] { cmsSigDataType, }, null),
        new(cmsSigPs2CRD3Tag, 1, new Signature[] { cmsSigDataType, }, null),
        new(cmsSigPs2CSATag, 1, new Signature[] { cmsSigDataType, }, null),
        new(cmsSigPs2RenderingIntentTag, 1, new Signature[] { cmsSigDataType, }, null),
        new(cmsSigViewingCondDescTag, 1, new Signature[] { cmsSigTextDescriptionType, cmsSigMultiLocalizedUnicodeType, cmsSigTextType, }, DecideTextDescType),
        new(cmsSigUcrBgTag, 1, new Signature[] { cmsSigUcrBgType, }, null),
        new(cmsSigCrdInfoTag, 1, new Signature[] { cmsSigCrdInfoType, }, null),
        new(cmsSigDToB0Tag, 1, new Signature[] { cmsSigMultiProcessElementType, }, null),
        new(cmsSigDToB1Tag, 1, new Signature[] { cmsSigMultiProcessElementType, }, null),
        new(cmsSigDToB2Tag, 1, new Signature[] { cmsSigMultiProcessElementType, }, null),
        new(cmsSigDToB3Tag, 1, new Signature[] { cmsSigMultiProcessElementType, }, null),
        new(cmsSigBToD0Tag, 1, new Signature[] { cmsSigMultiProcessElementType, }, null),
        new(cmsSigBToD1Tag, 1, new Signature[] { cmsSigMultiProcessElementType, }, null),
        new(cmsSigBToD2Tag, 1, new Signature[] { cmsSigMultiProcessElementType, }, null),
        new(cmsSigBToD3Tag, 1, new Signature[] { cmsSigMultiProcessElementType, }, null),
        new(cmsSigScreeningDescTag, 1, new Signature[] { cmsSigTextDescriptionType, }, null),
        new(cmsSigViewingConditionsTag, 1, new Signature[] { cmsSigViewingConditionsType, }, null),
        new(cmsSigScreeningTag, 1, new Signature[] { cmsSigScreeningType, }, null),
        new(cmsSigVcgtTag, 1, new Signature[] { cmsSigVcgtType, }, null),
        new(cmsSigMetaTag, 1, new Signature[] { cmsSigDictType, }, null),
        new(cmsSigProfileSequenceIdTag, 1, new Signature[] { cmsSigProfileSequenceIdType, }, null),
        new(cmsSigProfileDescriptionMLTag, 1, new Signature[] { cmsSigMultiLocalizedUnicodeType, }, null),
        new(cmsSigArgyllArtsTag, 9, new Signature[] { cmsSigS15Fixed16ArrayType, }, null));

    internal static readonly TagPluginChunkType TagPluginChunk = new();

    internal static readonly TagPluginChunkType globalTagPluginChunk = new();

    private static bool RegisterTypesPlugin(Context? id, PluginBase? Data, Chunks pos)
    {
        var Plugin = (PluginTagType?)Data;
        var ctx = pos is Chunks.MPEPlugin
            ? _cmsGetContext(id).MPEPlugin
            : _cmsGetContext(id).TagTypePlugin;

        // Calling the function with NULL as plug-in would unregister the plug in
        if (Data is null)
        {
            // There is no need to set free the memory, as pool is destroyed as a whole.
            ctx.TagTypes = null;
            return true;
        }

        // Registering happens in plug-in memory pool.
        //var pt = _cmsPluginMalloc<TagTypeLinkedList>(id);
        //if (pt is null) return false;
        var pt = new TagTypeLinkedList(Plugin!.Handler, ctx.TagTypes);

        //pt->Handler = Plugin!.Handler;
        //pt->Next = ctx.TagTypes;

        ctx.TagTypes = pt;

        return true;
    }

    private static TagTypeHandler? GetHandler(Signature sig, TagTypeLinkedList? PluginLinkedList, TagTypeLinkedList DefaultLinkedList)
    {
        for (var pt = PluginLinkedList; pt is not null; pt = pt.Next)
            if (sig == pt.Handler.Signature) return pt.Handler;

        for (var pt = DefaultLinkedList; pt is not null; pt = pt.Next)
            if (sig == pt.Handler.Signature) return pt.Handler;

        return null;
    }

    #region IO

    internal static bool _cmsWriteWCharArray(IOHandler io, uint n, in string str)
    {
        _cmsAssert(io);
        _cmsAssert(!(str is null && n > 0));

        for (var i = 0; i < n; i++)
            if (!_cmsWriteUInt16Number(io, str![i])) return false;

        return true;
    }

    internal static bool _cmsWriteWCharArray(IOHandler io, uint n, in char* Array)
    {
        _cmsAssert(io);
        _cmsAssert(!(Array is null && n > 0));

        for (var i = 0; i < n; i++)
            if (!_cmsWriteUInt16Number(io, Array[i])) return false;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool is_surrogate(uint uc) =>
        (uc - 0xD800u) < 2048u;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool is_high_surrogate(uint uc) =>
        (uc & 0xFFFFFC00) == 0xD800;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool is_low_surrogate(uint uc) =>
        (uc & 0xFFFFFC00) == 0xDC00;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint surrogate_to_utf32(uint high, uint low) =>
        (high << 10) + low - 0x35FDC00;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool convert_utf16_to_utf32(IOHandler io, int n, char* output)
    {
        ushort uc;

        while (n > 0)
        {
            if (!_cmsReadUInt16Number(io, &uc)) return false;
            n--;

            if (!is_surrogate(uc))
            {
                *output++ = (char)uc;
            }
            else
            {
                ushort low;

                if (!_cmsReadUInt16Number(io, &low)) return false;
                n--;

                if (is_high_surrogate(uc) && is_low_surrogate(low))
                {
                    *output++ = (char)surrogate_to_utf32(uc, low);
                }
                else
                {
                    return false;       // Corrupted string, just ignore
                }
            }
        }

        return true;
    }

    internal static bool _cmsReadWCharArray(IOHandler io, uint n, out string str)
    {
        var buffer = stackalloc char[(int)n+1];
        var rc = _cmsReadWCharArray(io, n, buffer);
        buffer[(int)n] = '\0';
        str = new string(buffer);

        return rc;
    }

    internal static bool _cmsReadWCharArray(IOHandler io, uint n, char* Array)
    {
        ushort tmp;
        const bool is32 = sizeof(char) > sizeof(ushort);

        _cmsAssert(io);

        if (is32 && Array is not null)
            return convert_utf16_to_utf32(io, (int)n, Array);

        for (var i = 0; i < n; i++)
        {
            if (Array is not null)
            {
                if (!_cmsReadUInt16Number(io, &tmp)) return false;
                Array[i] = (char)tmp;
            }
            else
            {
                if (!_cmsReadUInt16Number(io, null)) return false;
            }
        }
        return true;
    }

    private static bool ReadPositionTable(
        TagTypeHandler self,
        IOHandler io,
        uint Count,
        uint BaseOffset,
        object? Cargo,
        delegate*<TagTypeHandler, IOHandler, object?, uint, uint, bool> ElementFn)
    {
        uint* ElementOffsets = null, ElementSizes = null;

        var currentPosition = io.Tell(io);

        // Verify there is enough space left to read at least two uint items for Count items
        if (((io.reportedSize - currentPosition) / (2 * _sizeof<uint>())) < Count)
            return false;

        // Let's take the offsets to each element
        ElementOffsets = _cmsCalloc<uint>(io.ContextID, Count);
        if (ElementOffsets is null) goto Error;

        ElementSizes = _cmsCalloc<uint>(io.ContextID, Count);
        if (ElementSizes is null) goto Error;

        for (var i = 0; i < Count; i++)
        {
            if (!_cmsReadUInt32Number(io, &ElementOffsets[i])) goto Error;
            if (!_cmsReadUInt32Number(io, &ElementSizes[i])) goto Error;

            ElementOffsets[i] += BaseOffset;
        }

        // Seek to each element and read it
        for (var i = 0; i < Count; i++)
        {
            if (!io.Seek(io, ElementOffsets[i])) goto Error;

            // This is the reader callback
            if (!ElementFn(self, io, Cargo, (uint)i, ElementSizes[i])) goto Error;
        }

        //Success
        if (ElementOffsets is not null) _cmsFree(io.ContextID, ElementOffsets);
        if (ElementSizes is not null) _cmsFree(io.ContextID, ElementSizes);
        return true;

    Error:
        if (ElementOffsets is not null) _cmsFree(io.ContextID, ElementOffsets);
        if (ElementSizes is not null) _cmsFree(io.ContextID, ElementSizes);
        return false;
    }

    private static bool WritePositionTable(
        TagTypeHandler self,
        IOHandler io,
        uint SizeOfTag,
        uint Count,
        uint BaseOffset,
        object? Cargo,
        PositionTableEntryFn ElementFn)
    {
        uint* ElementOffsets = null, ElementSizes = null;

        // Create table
        ElementOffsets = _cmsCalloc<uint>(io.ContextID, Count);
        if (ElementOffsets is null) goto Error;

        ElementSizes = _cmsCalloc<uint>(io.ContextID, Count);
        if (ElementSizes is null) goto Error;

        // Keep starting position of curve offsets
        var DirectoryPos = io.Tell(io);

        // Write a fake directory to be filled later on
        for (var i = 0; i < Count; i++)
        {
            if (!_cmsWriteUInt32Number(io, 0)) goto Error;  // Offset
            if (!_cmsWriteUInt32Number(io, 0)) goto Error;  // size
        }

        // Write each element. Keep track of the size as well.
        for (var i = 0; i < Count; i++)
        {
            var Before = io.Tell(io);
            ElementOffsets[i] = Before - BaseOffset;

            // Callback to write...
            if (!ElementFn(self, io, Cargo, (uint)i, SizeOfTag)) goto Error;

            // Now the size
            ElementSizes[i] = io.Tell(io) - Before;
        }

        // Write the directory
        var CurrentPos = io.Tell(io);
        if (!io.Seek(io, DirectoryPos)) goto Error;

        for (var i = 0; i < Count; i++)
        {
            if (!_cmsWriteUInt32Number(io, ElementOffsets[i])) goto Error;
            if (!_cmsWriteUInt32Number(io, ElementSizes[i])) goto Error;
        }

        if (!io.Seek(io, CurrentPos)) goto Error;

        //Success
        if (ElementOffsets is not null) _cmsFree(io.ContextID, ElementOffsets);
        if (ElementSizes is not null) _cmsFree(io.ContextID, ElementSizes);
        return true;

    Error:
        if (ElementOffsets is not null) _cmsFree(io.ContextID, ElementOffsets);
        if (ElementSizes is not null) _cmsFree(io.ContextID, ElementSizes);
        return false;
    }

    #endregion IO

    #region TypeHandlers

    #region XYZ

    private static BoxPtr<CIEXYZ>? Type_XYZ_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        CIEXYZ* xyz;

        *nItems = 0;
        xyz = _cmsMallocZero<CIEXYZ>(self.ContextID);
        if (xyz is null) return null;

        if (!_cmsReadXYZNumber(io, xyz))
        {
            _cmsFree(self.ContextID, xyz);
            return null;
        }

        *nItems = 1;
        return new(xyz);
    }

    private static bool Type_XYZ_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2) =>
        Ptr is BoxPtr<CIEXYZ> xyz && _cmsWriteXYZNumber(io, xyz);

    private static BoxPtr<CIEXYZ>? Type_XYZ_Dup(TagTypeHandler self, object? Ptr, uint _) =>
        Ptr is BoxPtr<CIEXYZ> xyz
            ? new(_cmsDupMem<CIEXYZ>(self.ContextID, xyz))
            : null;

    private static void Type_XYZ_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<CIEXYZ> xyz)
            _cmsFree(self.ContextID, xyz);
    }

    private static Signature DecideXYZtype(double _1, object? _2) =>
        cmsSigXYZType;

    #endregion XYZ

    #region Chromaticity

    private static BoxPtr<CIExyYTRIPLE>? Type_Chromaticity_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        CIExyYTRIPLE* chrm;
        ushort nChans, Table;

        *nItems = 0;
        chrm = _cmsMallocZero<CIExyYTRIPLE>(self.ContextID);
        if (chrm is null) return null;

        if (!_cmsReadUInt16Number(io, &nChans)) goto Error;

        // Let's recover from a bug interoduced in early versions of lcms1
        if (nChans is 0 && SizeOfTag is 32)
        {
            if (!_cmsReadUInt16Number(io, null)) goto Error;
            if (!_cmsReadUInt16Number(io, &nChans)) goto Error;
        }

        if (nChans is not 3) goto Error;

        if (!_cmsReadUInt16Number(io, &Table)) goto Error;

        if (!_cmsRead15Fixed16Number(io, out chrm->Red.x)) goto Error;
        if (!_cmsRead15Fixed16Number(io, out chrm->Red.y)) goto Error;

        chrm->Red.Y = 1.0;

        if (!_cmsRead15Fixed16Number(io, out chrm->Green.x)) goto Error;
        if (!_cmsRead15Fixed16Number(io, out chrm->Green.y)) goto Error;

        chrm->Green.Y = 1.0;

        if (!_cmsRead15Fixed16Number(io, out chrm->Blue.x)) goto Error;
        if (!_cmsRead15Fixed16Number(io, out chrm->Blue.y)) goto Error;

        chrm->Blue.Y = 1.0;

        *nItems = 1;
        return new(chrm);

    Error:
        _cmsFree(self.ContextID, chrm);
        return null;
    }

    private static bool SaveOneChromaticity(double x, double y, IOHandler io)
    {
        if (!_cmsWriteUInt32Number(io, (uint)_cmsDoubleTo15Fixed16(x))) return false;
        if (!_cmsWriteUInt32Number(io, (uint)_cmsDoubleTo15Fixed16(y))) return false;

        return true;
    }

    private static bool Type_Chromaticity_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        var chrm = Ptr as BoxPtr<CIExyYTRIPLE>;
        if (chrm is null) return false;

        if (!_cmsWriteUInt16Number(io, 3)) return false;    // nChannels
        if (!_cmsWriteUInt16Number(io, 0)) return false;    // Table

        if (!SaveOneChromaticity(chrm.Ptr->Red.x, chrm.Ptr->Red.y, io)) return false;
        if (!SaveOneChromaticity(chrm.Ptr->Green.x, chrm.Ptr->Green.y, io)) return false;
        if (!SaveOneChromaticity(chrm.Ptr->Blue.x, chrm.Ptr->Blue.y, io)) return false;

        return true;
    }

    private static BoxPtr<CIExyYTRIPLE>? Type_Chromaticity_Dup(TagTypeHandler self, object? Ptr, uint _) =>
        Ptr is BoxPtr<CIExyYTRIPLE> value
            ? new(_cmsDupMem<CIExyYTRIPLE>(self.ContextID, value))
            : null;

    private static void Type_Chromaticity_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<CIExyYTRIPLE> value)
            _cmsFree(self.ContextID, value);
    }

    #endregion Chromaticity

    #region ColorantOrder

    private static BoxPtr<byte>? Type_ColorantOrderType_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        byte* ColorantOrder;
        uint Count;

        *nItems = 0;
        if (!_cmsReadUInt32Number(io, &Count)) return null;
        if (Count > cmsMAXCHANNELS) return null;

        ColorantOrder = _cmsCalloc<byte>(self.ContextID, cmsMAXCHANNELS);
        if (ColorantOrder is null) return null;

        // We use FF as end marker
        memset(ColorantOrder, 0xFF, cmsMAXCHANNELS * _sizeof<byte>());

        if (io.Read(io, ColorantOrder, _sizeof<byte>(), Count) != Count)
        {
            _cmsFree(self.ContextID, ColorantOrder);
            return null;
        }

        *nItems = 1;
        return new(ColorantOrder);
    }

    private static bool Type_ColorantOrderType_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        if (Ptr is not BoxPtr<byte> ColorantOrder) return false;
        uint i, Count;


        // Get the length
        for (Count = i = 0; i < cmsMAXCHANNELS; i++)
            if (ColorantOrder[i] is not 0xFF) Count++;

        if (!_cmsWriteUInt32Number(io, Count)) return false;

        var sz = Count * _sizeof<byte>();
        if (!io.Write(io, sz, ColorantOrder)) return false;

        return true;
    }

    private static BoxPtr<byte>? Type_ColorantOrderType_Dup(TagTypeHandler self, object? Ptr, uint _) =>
        Ptr is BoxPtr<byte> value
            ? new(_cmsDupMem<byte>(self.ContextID, value, cmsMAXCHANNELS))
            : null;

    private static void Type_ColorantOrderType_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<byte> value)
            _cmsFree(self.ContextID, value);
    }

    #endregion ColorantOrder

    #region S15Fixed16

    private static BoxPtr<double>? Type_S15Fixed16_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        double* array_double;

        *nItems = 0;
        var n = SizeOfTag / _sizeof<uint>();
        array_double = _cmsCalloc<double>(self.ContextID, n);
        if (array_double is null) return null;

        for (var i = 0; i < n; i++)
        {
            if (!_cmsRead15Fixed16Number(io, out array_double[i]))
            {
                _cmsFree(self.ContextID, array_double);
                return null;
            }
        }

        *nItems = n;
        return new(array_double);
    }

    private static bool Type_S15Fixed16_Write(TagTypeHandler _, IOHandler io, object? Ptr, uint nItems)
    {
        if (Ptr is not BoxPtr<double> Value) return false;

        for (var i = 0; i < nItems; i++)
            if (!_cmsWrite15Fixed16Number(io, Value[i])) return false;

        return true;
    }

    private static BoxPtr<double>? Type_S15Fixed16_Dup(TagTypeHandler self, object? Ptr, uint n) =>
        Ptr is BoxPtr<double> Value
            ? new(_cmsDupMem<double>(self.ContextID, Value, n))
            : null;

    private static void Type_S15Fixed16_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<double> value)
            _cmsFree(self.ContextID, value);
    }

    #endregion S15Fixed16

    #region U16Fixed16

    private static BoxPtr<double>? Type_U16Fixed16_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        double* array_double;
        uint v;

        *nItems = 0;
        var n = SizeOfTag / _sizeof<uint>();
        array_double = _cmsCalloc<double>(self.ContextID, n);
        if (array_double is null) return null;

        for (var i = 0; i < n; i++)
        {
            if (!_cmsReadUInt32Number(io, &v))
            {
                _cmsFree(self.ContextID, array_double);
                return null;
            }

            // Convert to double
            array_double[i] = v / 65536.0;
        }

        *nItems = n;
        return new(array_double);
    }

    private static bool Type_U16Fixed16_Write(TagTypeHandler _, IOHandler io, object? Ptr, uint nItems)
    {
        if (Ptr is not BoxPtr<double> Value) return false;

        for (var i = 0; i < nItems; i++)
        {
            var v = (uint)Math.Floor((Value[i] * 65536.0) + 0.5);
            if (!_cmsWriteUInt32Number(io, v)) return false;
        }

        return true;
    }

    private static BoxPtr<double>? Type_U16Fixed16_Dup(TagTypeHandler self, object? Ptr, uint n) =>
        Ptr is BoxPtr<double> value
            ? new(_cmsDupMem<double>(self.ContextID, value, n))
            : null;

    private static void Type_U16Fixed16_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<double> value)
            _cmsFree(self.ContextID, value);
    }

    #endregion U16Fixed16

    #region Signature

    private static BoxPtr<Signature>? Type_Signature_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        *nItems = 0;
        var SigPtr = _cmsMalloc<Signature>(self.ContextID);
        if (SigPtr is null) return null;

        if (!_cmsReadUInt32Number(io, (uint*)SigPtr))
        {
            _cmsFree(self.ContextID, SigPtr);
            return null;
        }

        *nItems = 1;
        return new BoxPtr<Signature>(SigPtr);
    }

    private static bool Type_Signature_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2) =>
        Ptr is BoxPtr<Signature> sig && _cmsWriteUInt32Number(io, *sig.Ptr);

    private static BoxPtr<Signature>? Type_Signature_Dup(TagTypeHandler self, object? Ptr, uint n) =>
        Ptr is BoxPtr<Signature> sig
            ? new BoxPtr<Signature>(_cmsDupMem<Signature>(self.ContextID, sig, n))
            : null;

    private static void Type_Signature_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<Signature> sig)
            _cmsFree(self.ContextID, sig);
    }

    #endregion Signature

    #region Text

    private static Mlu? Type_Text_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        Mlu? mlu = null;
        byte[]? Text = null;

        *nItems = 0;
        mlu = cmsMLUalloc(self.ContextID, 1);
        if (mlu is null) return null;

        // We need to store the "\0" at the end, so +1
        if (SizeOfTag is UInt32.MaxValue) goto Error;

        //Text = _cmsMalloc<byte>(self.ContextID, SizeOfTag + 1);
        Text = _cmsCallocArray<byte>(self.ContextID, SizeOfTag);       // No trailing zero required!
        if (Text is null) goto Error;

        var tmpText = stackalloc byte[(int)SizeOfTag];
        if (io.Read(io, tmpText, _sizeof<byte>(), SizeOfTag) != SizeOfTag) goto Error;
        new ReadOnlySpan<byte>(tmpText, (int)SizeOfTag).CopyTo(Text);

        // Make sure text is properly ended
        //Text[SizeOfTag] = 0;              // Nope!!
        *nItems = 1;

        // Keep the result
        if (!cmsMLUsetASCII(mlu, cmsNoLanguage, cmsNoCountry, Text)) goto Error;

        _cmsFree(self.ContextID, Text);
        return mlu;

    Error:
        if (mlu is not null) cmsMLUfree(mlu);
        if (Text is not null) _cmsFree(self.ContextID, Text);

        return null;
    }

    private static bool Type_Text_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Mlu mlu) return false;

        // Get the size of the string. Note there is an extra "\0" at the end       // No extra "\0"
        var size = cmsMLUgetASCII(mlu, cmsNoLanguage, cmsNoCountry, null);
        if (size is 0) return false;    // Cannot be zero!

        // Create memory
        var Text = _cmsCallocArray<byte>(self.ContextID, size);
        if (Text is null) return false;

        cmsMLUgetASCII(mlu, cmsNoLanguage, cmsNoCountry, Text);
        var tmpText = stackalloc byte[(int)size];
        Text.AsSpan()[..(int)size].CopyTo(new(tmpText, (int)size));

        // Write it, including separators
        var rc = io.Write(io, size, tmpText);

        _cmsFree(self.ContextID, Text);
        return rc;
    }

    private static Mlu? Type_Text_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        Ptr is Mlu value
            ? cmsMLUdup(value)
            : null;

    private static void Type_Text_Free(TagTypeHandler _, object? Ptr) =>
        cmsMLUfree(Ptr as Mlu);

    private static Signature DecideTextType(double ICCVersion, object? _) =>
        ICCVersion >= 4.0
            ? cmsSigMultiLocalizedUnicodeType
            : cmsSigTextType;

    #endregion Text

    #region Data

    private static BoxPtr<IccData>? Type_Data_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        IccData* BinData;
        uint LenOfData;

        *nItems = 0;

        if (SizeOfTag < _sizeof<uint>()) return null;

        LenOfData = SizeOfTag - _sizeof<uint>();
        if (LenOfData > Int32.MaxValue) return null;

        BinData = _cmsMalloc<IccData>(self.ContextID, _sizeof<IccData>() + LenOfData - 1);
        if (BinData is null) return null;

        BinData->len = LenOfData;
        if (!_cmsReadUInt32Number(io, &BinData->flag)) goto Error;

        if (io.Read(io, BinData->data, _sizeof<byte>(), LenOfData) != LenOfData) goto Error;

        *nItems = 1;

        return new(BinData);

    Error:
        _cmsFree(self.ContextID, BinData);
        return null;
    }

    private static bool Type_Data_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        if (Ptr is not BoxPtr<IccData> BinData) return false;

        if (!_cmsWriteUInt32Number(io, BinData.Ptr->flag)) return false;

        return io.Write(io, BinData.Ptr->len, BinData.Ptr->data);
    }

    private static BoxPtr<IccData>? Type_Data_Dup(TagTypeHandler self, object? Ptr, uint _) =>
        Ptr is BoxPtr<IccData> data
            ? new((IccData*)_cmsDupMem(self.ContextID, data, _sizeof<IccData>() + data.Ptr->len - 1, typeof(IccData)))
            : null;

    private static void Type_Data_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<IccData> data)
            _cmsFree(self.ContextID, data);
    }

    #endregion Data

    #region Text Description

    private static Mlu? Type_Text_Description_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        Mlu? mlu = null;
        byte[]? Text = null;
        uint AsciiCount, UnicodeCode, UnicodeCount;
        ushort ScriptCodeCode, Dummy;
        byte ScriptCodeCount;

        *nItems = 0;

        // Ond dword should be there
        if (SizeOfTag < _sizeof<uint>()) return null;

        // Read len of ASCII
        if (!_cmsReadUInt32Number(io, &AsciiCount)) return null;
        SizeOfTag -= _sizeof<uint>();

        // Check for size
        if (SizeOfTag < AsciiCount) return null;

        // All seems Ok, allocate the container
        mlu = cmsMLUalloc(self.ContextID, 1);
        if (mlu is null) return null;

        // As much memory as size of tag
        //Text = _cmsMalloc<byte>(self.ContextID, AsciiCount + 1);
        Text = _cmsCallocArray<byte>(self.ContextID, AsciiCount);
        if (Text is null) goto Error;

        // Read it
        var tmpText = stackalloc byte[(int)AsciiCount];
        if (io.Read(io, tmpText, _sizeof<byte>(), AsciiCount) != AsciiCount) goto Error;
        new Span<byte>(tmpText, (int)AsciiCount).CopyTo(Text);
        SizeOfTag -= AsciiCount;

        // Make sure there is a terminator
        //Text[AsciiCount] = 0;

        // Set the MLU entry. From here we can be tolerant to wrong types
        if (!cmsMLUsetASCII(mlu, cmsNoLanguage, cmsNoCountry, Text)) goto Error;
        _cmsFree(self.ContextID, Text);
        Text = null;

        // Skip Unicode code
        if (SizeOfTag < 2 * _sizeof<uint>()) goto Done;
        if (!_cmsReadUInt32Number(io, &UnicodeCode)) goto Done;
        if (!_cmsReadUInt32Number(io, &UnicodeCount)) goto Done;
        SizeOfTag -= 2u * _sizeof<uint>();

        if (SizeOfTag < UnicodeCount * _sizeof<ushort>()) goto Done;

        for (var i = 0; i < UnicodeCount; i++)
            if (io.Read(io, &Dummy, _sizeof<ushort>(), 1) is 0) goto Done;
        SizeOfTag -= UnicodeCount * _sizeof<ushort>();

        // Skip ScriptCode code if present. Some buggy profiles does have less
        // data that stricttly required. We need to skip it as this type may come
        // embedded in other types.

        if (SizeOfTag >= _sizeof<ushort>() + _sizeof<byte>() + 67)
        {
            if (!_cmsReadUInt16Number(io, &ScriptCodeCode)) goto Done;
            if (!_cmsReadUInt8Number(io, &ScriptCodeCount)) goto Done;

            // Skip rest of tag
            for (var i = 0; i < 67; i++)
                if (io.Read(io, &Dummy, _sizeof<byte>(), 1) is 0) goto Error;
        }

    Done:
        *nItems = 1;
        return mlu;

    Error:
        if (mlu is not null) cmsMLUfree(mlu);
        if (Text is not null) _cmsFree(self.ContextID, Text);

        return null;
    }

    private static bool Type_Text_Description_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Mlu mlu) return false;
        byte[]? Text = null;
        char[]? Wide = null;
        uint len, len_text, len_tag_requirement, len_aligned;
        var rc = false;
        var Filler = stackalloc byte[68];

        // Used below for writing zeros
        memset(Filler, 0, 68);

        // Get the len of string
        len = cmsMLUgetASCII(mlu, cmsNoLanguage, cmsNoCountry, null);

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
        if (len <= 0)
        {
            //Text = _cmsDupMem<byte>(self.ContextID, Filler, _sizeof<byte>());
            //Wide = _cmsDupMem<char>(self.ContextID, Filler, _sizeof<char>());
            Text = Array.Empty<byte>();
            Wide = Array.Empty<char>();
        }
        else
        {
            // Create independent buffers
            Text = _cmsCallocArray<byte>(self.ContextID, len);
            if (Text is null) goto Error;
            Wide = _cmsCallocArray<char>(self.ContextID, len);
            if (Wide is null) goto Error;

            // Get both representations.
            cmsMLUgetASCII(mlu, cmsNoLanguage, cmsNoCountry, Text);
            cmsMLUgetWide(mlu, cmsNoLanguage, cmsNoCountry, Wide);
        }

        // Tell the real text len including the null terminator and padding
        //len_text = (uint)strlen(Text) + 1;
        len_text = (uint)Text.Length;
        // Compute a total tag size requirement
        len_tag_requirement =
            8                   // Alignment
            + 4                 // count
            + len_text          // desc[count]
            + 4                 // ucLangCode
            + 4                 // ucCount
            + (2 * len_text)    // ucDesc[ucCount]
            + 2                 // scCode
            + 1                 // scCount
            + 67;               // scDesc[67]
        len_aligned = _cmsALIGNLONG(len_tag_requirement);

        if (!_cmsWriteUInt32Number(io, len_text)) goto Error;
        if (len_text > 0)
        {
            var tmpText = stackalloc byte[(int)len_text];
            Text.AsSpan()[..(int)len_text].CopyTo(new(tmpText, (int)len_text));
            if (!io.Write(io, len_text, tmpText)) goto Error;
        }

        if (!_cmsWriteUInt32Number(io, 0)) goto Error;  // ucLangCode

        if (!_cmsWriteUInt32Number(io, len_text)) goto Error;
        if (len_text > 0)
        {
            var tmpWide = stackalloc char[(int)len_text];
            Wide.AsSpan()[..(int)len_text].CopyTo(new(tmpWide, (int)len_text));
            if (!_cmsWriteWCharArray(io, len_text, tmpWide)) goto Error;
        }

        // ScriptCode Code & Count (unused)
        if (!_cmsWriteUInt16Number(io, 0)) goto Error;
        if (!_cmsWriteUInt8Number(io, 0)) goto Error;

        if (!io.Write(io, 67, Filler)) goto Error;

        // possibly add pad at the end of tag
        if (len_aligned - len_tag_requirement > 0)
            if (!io.Write(io, len_aligned - len_tag_requirement, Filler)) goto Error;

        rc = true;

    Error:
        if (Text is not null) _cmsFree(self.ContextID, Text);
        if (Wide is not null) _cmsFree(self.ContextID, Wide);

        return rc;
    }

    private static Mlu? Type_Text_Description_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        Ptr is Mlu mlu
            ? cmsMLUdup(mlu)
            : null;

    private static void Type_Text_Description_Free(TagTypeHandler _, object? Ptr) =>
        cmsMLUfree(Ptr as Mlu);

    private static Signature DecideTextDescType(double ICCVersion, object? _) =>
        ICCVersion >= 4.0
            ? cmsSigMultiLocalizedUnicodeType
            : cmsSigTextDescriptionType;

    #endregion Text Description

    #region Curve

    private static ToneCurve? Type_Curve_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        uint Count;
        ToneCurve NewGamma;

        *nItems = 0;
        if (!_cmsReadUInt32Number(io, &Count)) return null;

        switch (Count)
        {
            case 0:     // Linear
                {
                    var SingleGamma = 1.0;

                    NewGamma = cmsBuildParametricToneCurve(self.ContextID, 1, &SingleGamma);
                    if (NewGamma is null) return null;
                    *nItems = 1;
                    return NewGamma;
                }
            case 1:     // Specified as the exponent of gamma function
                {
                    double SingleGamma;
                    ushort SingleGammaFixed;

                    if (!_cmsReadUInt16Number(io, &SingleGammaFixed)) return null;
                    SingleGamma = _cms8Fixed8toDouble(SingleGammaFixed);

                    *nItems = 1;
                    return cmsBuildParametricToneCurve(self.ContextID, 1, &SingleGamma);
                }
            default:

                if (Count > 0x7FFF)
                    return null;    // This is to prevent bad guys from doing bad things

                NewGamma = cmsBuildTabulatedToneCurve16(self.ContextID, Count, null);
                if (NewGamma is null) return null;

                fixed (ushort* t = &NewGamma.Table16[0])
                {
                    if (!_cmsReadUInt16Array(io, Count, t))
                    {
                        cmsFreeToneCurve(NewGamma);
                        return null;
                    }
                }

                *nItems = 1;
                return NewGamma;
        }
    }

    private static bool Type_Curve_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        if (Ptr is not ToneCurve Curve)
            return false;

        if (Curve.nSegments is 1 && Curve.Segments[0].Type is 1)
        {
            // Single gamma, preserve number
            var SingleGammaFixed = _cmsDoubleTo8Fixed8(Curve.Segments[0].Params[0]);

            if (!_cmsWriteUInt32Number(io, 1)) return false;
            if (!_cmsWriteUInt16Number(io, SingleGammaFixed)) return false;
            return true;
        }

        if (!_cmsWriteUInt32Number(io, Curve.nEntries)) return false;
        fixed (ushort* t = &Curve.Table16[0])
            return _cmsWriteUInt16Array(io, Curve.nEntries, t);
    }

    #endregion Curve

    #region ParametricCurve

    private static Signature DecideCurveType(double ICCVersion, object? Data)
    {
        if (ICCVersion < 4.0) return cmsSigCurveType;

        if (Data is ToneCurve Curve)
        {
            if (Curve.nSegments is not 1) return cmsSigCurveType;
            if (Curve.Segments[0].Type is < 0 or > 5) return cmsSigCurveType;
        }

        return cmsSigParametricCurveType;
    }

    private static ToneCurve? Type_ParametricCurve_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        var ParamsByType = stackalloc int[] { 1, 3, 4, 5, 7 };
        var Params = stackalloc double[10];
        ushort Type;

        *nItems = 0;
        if (!_cmsReadUInt16Number(io, &Type)) return null;
        if (!_cmsReadUInt16Number(io, null)) return null;   // Reserved

        if (Type > 4)
        {
            cmsSignalError(self.ContextID, ErrorCode.UnknownExtension, $"Unknown parametric curve type '{Type}'");
            return null;
        }

        memset(Params, 0, _sizeof<double>() * 10);
        var n = ParamsByType[Type];

        for (var i = 0; i < n; i++)
            if (!_cmsRead15Fixed16Number(io, out Params[i])) return null;

        var NewGamma = cmsBuildParametricToneCurve(self.ContextID, Type + 1, Params);

        *nItems = 1;
        return NewGamma;
    }

    private static bool Type_ParametricCurve_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _2)
    {
        if (Ptr is not ToneCurve Curve)
            return false;

        var ParamsByType = stackalloc int[] { 0, 1, 3, 4, 5, 7 };

        var typen = Curve.Segments[0].Type;

        if (Curve.nSegments is 1 && typen < 1)
        {
            cmsSignalError(self.ContextID, ErrorCode.UnknownExtension, "Multisegment or Inverted parametric curves cannot be written");
            return false;
        }

        if (typen > 5)
        {
            cmsSignalError(self.ContextID, ErrorCode.UnknownExtension, "Unsupported parametric curve");
            return false;
        }

        var nParam = ParamsByType[typen];

        if (!_cmsWriteUInt16Number(io, (ushort)(Curve.Segments[0].Type - 1))) return false;
        if (!_cmsWriteUInt16Number(io, 0)) return false;    // Reserved

        for (var i = 0; i < nParam; i++)
        {
            if (!_cmsWrite15Fixed16Number(io, Curve.Segments[0].Params[i])) return false;
        }

        return true;
    }

    #endregion ParametricCurve

    #region DateTime

    private static BoxPtr<DateTime>? Type_DateTime_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        DateTimeNumber timestamp;
        DateTime* NewDateTime;

        *nItems = 0;

        if (io.Read(io, &timestamp, _sizeof<DateTimeNumber>(), 1) is not 1) return null;

        NewDateTime = _cmsMalloc<DateTime>(self.ContextID);
        if (NewDateTime is null) return null;

        _cmsDecodeDateTimeNumber(&timestamp, NewDateTime);

        *nItems = 1;
        return new(NewDateTime);
    }

    private static bool Type_DateTime_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        if (Ptr is not BoxPtr<DateTime> DateTime) return false;
        DateTimeNumber timestamp;

        _cmsEncodeDateTimeNumber(&timestamp, DateTime);
        if (!io.Write(io, _sizeof<DateTimeNumber>(), &timestamp)) return false;
        return true;
    }

    private static BoxPtr<DateTime>? Type_DateTime_Dup(TagTypeHandler self, object? Ptr, uint _) =>
        Ptr is BoxPtr<DateTime> dt
            ? new(_cmsDupMem<DateTime>(self.ContextID, dt))
            : null;

    private static void Type_DateTime_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<DateTime> dt)
            _cmsFree(self.ContextID, dt);
    }

    #endregion DateTime

    #region Measurement

    private static BoxPtr<IccMeasurementConditions>? Type_Measurement_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        IccMeasurementConditions mc;

        *nItems = 0;
        memset(&mc, 0, _sizeof<IccMeasurementConditions>());

        if (!_cmsReadUInt32Number(io, &mc.Observer)) return null;
        if (!_cmsReadXYZNumber(io, &mc.Backing)) return null;
        if (!_cmsReadUInt32Number(io, &mc.Geometry)) return null;
        if (!_cmsRead15Fixed16Number(io, out mc.Flare)) return null;
        if (!_cmsReadUInt32Number(io, (uint*)&mc.IlluminantType)) return null;

        var result = _cmsDupMem<IccMeasurementConditions>(self.ContextID, &mc);
        if (result is null) return null;

        *nItems = 1;
        return new(result);
    }

    private static bool Type_Measurement_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        if (Ptr is not BoxPtr<IccMeasurementConditions> mc) return false;

        if (!_cmsWriteUInt32Number(io, mc.Ptr->Observer)) return false;
        if (!_cmsWriteXYZNumber(io, &mc.Ptr->Backing)) return false;
        if (!_cmsWriteUInt32Number(io, mc.Ptr->Geometry)) return false;
        if (!_cmsWrite15Fixed16Number(io, mc.Ptr->Flare)) return false;
        if (!_cmsWriteUInt32Number(io, (uint)mc.Ptr->IlluminantType)) return false;

        return true;
    }

    private static BoxPtr<IccMeasurementConditions>? Type_Measurement_Dup(TagTypeHandler self, object? Ptr, uint _) =>
        Ptr is BoxPtr<IccMeasurementConditions> mc
            ? new(_cmsDupMem<IccMeasurementConditions>(self.ContextID, mc))
            : null;

    private static void Type_Measurement_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<IccMeasurementConditions> mc)
            _cmsFree(self.ContextID, mc);
    }

    #endregion Measurement

    #region MLU

    private static Mlu? Type_MLU_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        Mlu? mlu = null;
        uint Count, RecLen, NumOfWchar, SizeOfHeader, Len, Offset, BeginOfThisString, EndOfThisString, LargestPosition;
        char[]? Block;

        *nItems = 0;
        if (!_cmsReadUInt32Number(io, &Count)) return null;
        if (!_cmsReadUInt32Number(io, &RecLen)) return null;

        if (RecLen is not 12)
        {
            cmsSignalError(self.ContextID, ErrorCode.UnknownExtension, "multiLocalizedUnicodeType of len != 12 is not supported");
            return null;
        }

        mlu = cmsMLUalloc(self.ContextID, Count);
        if (mlu is null) return null;

        //mlu->UsedEntries = Count;

        SizeOfHeader = (12 * Count) + _sizeof<TagBase>();
        LargestPosition = 0;

        for (var i = 0; i < Count; i++)
        {
            ushort Language, Country;
            if (!_cmsReadUInt16Number(io, &Language)) goto Error;
            if (!_cmsReadUInt16Number(io, &Country)) goto Error;

            // Now deal with Len and offset
            if (!_cmsReadUInt32Number(io, &Len)) goto Error;
            if (!_cmsReadUInt32Number(io, &Offset)) goto Error;

            // Check for overflow
            if (Offset < (SizeOfHeader + 8)) goto Error;
            if (((Offset + Len) < Len) || ((Offset + Len) > SizeOfTag + 8)) goto Error;

            // True begin of the string
            BeginOfThisString = Offset - SizeOfHeader - 8;

            // Adjust to char elements
            Len *= (uint)(_sizeof<char>() / _sizeof<ushort>());
            BeginOfThisString *= (uint)(_sizeof<char>() / _sizeof<ushort>());

            mlu.Entries.Add(new(Language, Country, BeginOfThisString, Len));

            // To guess maximum size, add offset + len
            EndOfThisString = BeginOfThisString + Len;
            if (EndOfThisString > LargestPosition)
                LargestPosition = EndOfThisString;
        }

        // Now read the remaining of tag and fill all strings. Subtract the directory
        SizeOfTag = LargestPosition * _sizeof<char>() / _sizeof<ushort>();
        if (SizeOfTag is 0)
        {
            Block = null;
            NumOfWchar = 0;
        }
        else
        {
            //Block = _cmsMalloc<char>(self.ContextID, SizeOfTag);
            NumOfWchar = SizeOfTag / _sizeof<char>();
            Block = _cmsCallocArray<char>(self.ContextID, NumOfWchar);
            if (Block is null) goto Error;
            var tmpBlock = stackalloc char[(int)NumOfWchar];
            if (!_cmsReadWCharArray(io, NumOfWchar, tmpBlock)) goto Error;
            new Span<char>(tmpBlock, (int)NumOfWchar).CopyTo(Block.AsSpan()[..(int)NumOfWchar]);
        }

        mlu.MemPool = Block;
        mlu.PoolSizeInBytes = SizeOfTag;
        mlu.PoolUsedInBytes = SizeOfTag;

        *nItems = 1;
        return mlu;

    Error:
        if (mlu is not null) cmsMLUfree(mlu);

        return null;
    }

    private static bool Type_MLU_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        uint HeaderSize, Len, Offset;

        if (Ptr is null)
        {
            // Empty placeholder
            if (!_cmsWriteUInt32Number(io, 0)) return false;
            if (!_cmsWriteUInt32Number(io, 12)) return false;
            return true;
        }

        if (Ptr is not Mlu mlu) return false;

        if (!_cmsWriteUInt32Number(io, mlu.UsedEntries)) return false;
        if (!_cmsWriteUInt32Number(io, 12)) return false;

        HeaderSize = (12 * mlu.UsedEntries) + _sizeof<TagBase>();

        for (var i = 0; i < mlu.UsedEntries; i++)
        {
            Len = mlu.Entries[i].LenInBytes;
            Offset = mlu.Entries[i].StrWByteOffset;

            Len = Len * _sizeof<ushort>() / _sizeof<char>();
            Offset = (Offset * _sizeof<ushort>() / _sizeof<char>()) + HeaderSize + 8;

            if (!_cmsWriteUInt16Number(io, mlu.Entries[i].Language)) return false;
            if (!_cmsWriteUInt16Number(io, mlu.Entries[i].Country)) return false;
            if (!_cmsWriteUInt32Number(io, Len)) return false;
            if (!_cmsWriteUInt32Number(io, Offset)) return false;
        }

        var sizeInChars = (int)(mlu.PoolUsedInBytes / _sizeof<char>());
        var tmp = stackalloc char[sizeInChars];
        mlu.MemPool.AsSpan()[..sizeInChars].CopyTo(new(tmp, sizeInChars));
        if (!_cmsWriteWCharArray(io, (uint)sizeInChars, tmp)) return false;

        return true;
    }

    private static Mlu? Type_MLU_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        Ptr is Mlu mlu
            ? cmsMLUdup(mlu)
            : null;

    private static void Type_MLU_Free(TagTypeHandler _, object? Ptr) =>
        cmsMLUfree(Ptr as Mlu);

    #endregion MLU

    #region LUT8

    /*
    This structure represents a colour transform using tables of 8-bit precision.
    This type contains four processing elements: a 3 by 3 matrix (which shall be
    the identity matrix unless the input colour space is XYZ), a set of one dimensional
    input tables, a multidimensional lookup table, and a set of one dimensional output
    tables. Data is processed using these elements via the following sequence:
    (matrix) -> (1d input tables)  -> (multidimensional lookup table - CLUT) -> (1d output tables)

    Byte Position   Field Length (bytes)  Content Encoded as...
    8                  1          Number of Input Channels (i)    uInt8Number
    9                  1          Number of Output Channels (o)   uInt8Number
    10                 1          Number of CLUT grid points (identical for each side) (g) uInt8Number
    11                 1          Reserved for padding (fill with 00h)

    12..15             4          Encoded e00 parameter   s15Fixed16Number
    */

    private static Signature DecideLUTtypeA2B(double ICCVersion, object? Data) =>
        (ICCVersion < 4.0)
            ? Data is Pipeline p
                ? p.SaveAs8Bits
                    ? cmsSigLut8Type
                    : cmsSigLut16Type
                : cmsSigLutAtoBType
            : cmsSigLutAtoBType;

    private static Signature DecideLUTtypeB2A(double ICCVersion, object? Data) =>
        (ICCVersion < 4.0)
            ? Data is Pipeline p
                ? p.SaveAs8Bits
                    ? cmsSigLut8Type
                    : cmsSigLut16Type
                : cmsSigLutBtoAType
            : cmsSigLutBtoAType;

    private static bool Read8bitTables(Context? ContextID, IOHandler io, Pipeline lut, uint nChannels)
    {
        byte* Temp = null;

        var pool = Context.GetPool<ToneCurve>(ContextID);
        ToneCurve[] Tables = pool.Rent(cmsMAXCHANNELS);

        if (nChannels is > cmsMAXCHANNELS or <= 0) goto Error2;

        //memset(Tables, 0, _sizeof<nint>() * cmsMAXCHANNELS);

        Temp = _cmsMalloc<byte>(ContextID, 256);
        if (Temp is null) goto Error2;

        for (var i = 0; i < nChannels; i++)
        {
            Tables[i] = cmsBuildTabulatedToneCurve16(ContextID, 256, null)!;
            if (Tables[i] is null) goto Error;
        }

        for (var i = 0; i < nChannels; i++)
        {
            if (io.Read(io, Temp, 256, 1) is not 1) goto Error;

            for (var j = 0; j < 256; j++)
                if (Tables[i].Table16 is not null) Tables[i].Table16![j] = FROM_8_TO_16(Temp[j]);
        }

        _cmsFree(ContextID, Temp);
        Temp = null;

        if (!cmsPipelineInsertStage(lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, nChannels, Tables)))
            goto Error;

        for (var i = 0; i < nChannels; i++)
            cmsFreeToneCurve(Tables[i]);

        return true;

    Error:
        for (var i = 0; i < nChannels; i++)
            if (Tables[i] is not null) cmsFreeToneCurve(Tables[i]);

        if (Temp is not null) _cmsFree(ContextID, Temp);

    Error2:
        pool.Return(Tables);
        return false;
    }

    private static bool Write8bitTables(Context? ContextID, IOHandler io, uint n, StageToneCurvesData? Tables)
    {
        if (Tables is not null)
        {
            for (var i = 0; i < n; i++)
            {
                // Usual case of identity curves
                if ((Tables.TheCurves[i].nEntries is 2) &&
                    (Tables.TheCurves[i].Table16 is not null) &&
                    (Tables.TheCurves[i].Table16![0] is 0) &&
                    (Tables.TheCurves[i].Table16![1] is 65535))
                {
                    for (var j = 0; j < 256; j++)
                        if (!_cmsWriteUInt8Number(io, (byte)j)) return false;
                }
                else
                {
                    if (Tables.TheCurves[i].nEntries is not 256)
                    {
                        cmsSignalError(ContextID, ErrorCode.Range, "LUT8 needs 256 entries on prelinearization");
                        return false;
                    }
                    else
                    {
                        for (var j = 0; j < 256; j++)
                        {
                            var val = FROM_16_TO_8(Tables.TheCurves[i].Table16[j]);
                            if (!_cmsWriteUInt8Number(io, val)) return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private static uint uipow(uint n, uint a, uint b)
    {
        var rv = 1u;

        if (a is 0) return 0;
        if (n is 0) return 0;

        for (; b > 0; b--)
        {
            rv *= a;

            // Check for overflow
            if (rv > UInt32.MaxValue / a) return unchecked((uint)-1);
        }

        var rc = rv * n;

        if (rv != rc / n) return unchecked((uint)-1);
        return rc;
    }

    private static Pipeline? Type_LUT8_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        byte InputChannels, OutputChannels, CLUTpoints;
        byte* Temp = null;
        Pipeline? NewLUT = null;
        Span<double> Matrix = stackalloc double[3 * 3];

        *nItems = 0;

        if (!_cmsReadUInt8Number(io, &InputChannels)) goto Error;
        if (!_cmsReadUInt8Number(io, &OutputChannels)) goto Error;
        if (!_cmsReadUInt8Number(io, &CLUTpoints)) goto Error;

        if (CLUTpoints is 1) goto Error;    // Impossible value, 0 for no CLUT and then 2 at least

        // Padding
        if (!_cmsReadUInt8Number(io, null)) goto Error;

        // Do some checking
        if (InputChannels is 0 or > cmsMAXCHANNELS) goto Error;
        if (OutputChannels is 0 or > cmsMAXCHANNELS) goto Error;

        // Allocates an empty Pipeline
        NewLUT = cmsPipelineAlloc(self.ContextID, InputChannels, OutputChannels);
        if (NewLUT is null) goto Error;

        // Read the Matrix
        for (var i = 0; i < 9; i++)
            if (!_cmsRead15Fixed16Number(io, out Matrix[i])) goto Error;

        // Only operates if not identity...
        if ((InputChannels is 3) &&
            !_cmsMAT3isIdentity(new(Matrix)) &&
            !cmsPipelineInsertStage(NewLUT, StageLoc.AtBegin, cmsStageAllocMatrix(self.ContextID, 3, 3, Matrix, null)))
        {
            goto Error;
        }

        // Get input tables
        if (!Read8bitTables(self.ContextID, io, NewLUT, InputChannels)) goto Error;

        // Get 3D CLUT. Check the overflow...
        var nTabSize = uipow(OutputChannels, CLUTpoints, InputChannels);
        if (nTabSize == unchecked((uint)-1)) goto Error;
        if (nTabSize > 0)
        {
            var PtrW = _cmsCalloc<ushort>(self.ContextID, nTabSize);
            var T = PtrW;
            if (T is null) goto Error;

            Temp = _cmsMalloc<byte>(self.ContextID, nTabSize);
            if (Temp is null)
            {
                _cmsFree(self.ContextID, T);
                goto Error;
            }

            if (io.Read(io, Temp, nTabSize, 1) is not 1)
            {
                _cmsFree(self.ContextID, T);
                _cmsFree(self.ContextID, Temp);
                goto Error;
            }

            for (var i = 0; i < nTabSize; i++)
                *PtrW++ = FROM_8_TO_16(Temp[i]);
            _cmsFree(self.ContextID, Temp);
            Temp = null;

            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, cmsStageAllocCLut16bit(self.ContextID, CLUTpoints, InputChannels, OutputChannels, T)))
            {
                _cmsFree(self.ContextID, T);
                goto Error;
            }
            _cmsFree(self.ContextID, T);
        }

        // Get output tables
        if (!Read8bitTables(self.ContextID, io, NewLUT, OutputChannels)) goto Error;

        *nItems = 1;
        return NewLUT;

    Error:
        if (NewLUT is not null) cmsPipelineFree(NewLUT);

        return null;
    }

    private static bool Type_LUT8_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Pipeline NewLut)
            return false;

        ReadOnlySpan<double> ident = stackalloc double[9] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        StageToneCurvesData? PreMPE = null, PostMPE = null;
        StageMatrixData? MatMPE = null;
        StageCLutData<ushort>? clut = null;

        // Disassemble the LUT into components.
        var mpe = NewLut.Elements;
        if (mpe.Type == cmsSigMatrixElemType)
        {
            if (mpe.InputChannels is not 3 || mpe.OutputChannels is not 3) return false;
            MatMPE = mpe.Data as StageMatrixData;
            mpe = mpe.Next;
        }

        if (mpe?.Type == cmsSigCurveSetElemType)
        {
            PreMPE = mpe.Data as StageToneCurvesData;
            mpe = mpe.Next;
        }

        if (mpe?.Type == cmsSigCLutElemType)
        {
            clut = mpe.Data as StageCLutData<ushort>;
            mpe = mpe.Next;
        }

        if (mpe?.Type == cmsSigCurveSetElemType)
        {
            PostMPE = mpe.Data as StageToneCurvesData;
            mpe = mpe.Next;
        }

        // That should be all
        if (mpe is not null)
        {
            cmsSignalError(mpe.ContextID, ErrorCode.UnknownExtension, "LUT is not suitable to be saved as LUT8");
            return false;
        }

        var clutPoints = (clut?.Params.nSamples[0]) ?? 0u;

        if (!_cmsWriteUInt8Number(io, (byte)NewLut.InputChannels)) return false;
        if (!_cmsWriteUInt8Number(io, (byte)NewLut.OutputChannels)) return false;
        if (!_cmsWriteUInt8Number(io, (byte)clutPoints)) return false;
        if (!_cmsWriteUInt8Number(io, 0)) return false; // Padding

        var mat = MatMPE is not null ? MatMPE.Double : ident;
        for (var i = 0; i < 9; i++)
            if (!_cmsWrite15Fixed16Number(io, mat[i])) return false;

        // The prelinearization table
        if (!Write8bitTables(self.ContextID, io, NewLut.InputChannels, PreMPE)) return false;

        var nTabSize = uipow(NewLut.OutputChannels, clutPoints, NewLut.InputChannels);
        if (nTabSize == unchecked((uint)-1)) return false;
        if (nTabSize > 0)
        {
            // The 3D CLUT
            if (clut is not null)
            {
                for (var j = 0; j < nTabSize; j++)
                {
                    var val = FROM_16_TO_8(clut.TUshort[j]);
                    if (!_cmsWriteUInt8Number(io, val)) return false;
                }
            }
        }

        // The postlinearization table
        return Write8bitTables(self.ContextID, io, NewLut.OutputChannels, PostMPE);
    }

    private static Pipeline? Type_LUT8_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        cmsPipelineDup(Ptr as Pipeline);

    private static void Type_LUT8_Free(TagTypeHandler _, object? Ptr) =>
        cmsPipelineFree(Ptr as Pipeline);

    #endregion LUT8

    #region LUT16

    private static bool Read16bitTables(Context? ContextID, IOHandler io, Pipeline lut, uint nChannels, uint nEntries)
    {
        var pool = Context.GetPool<ToneCurve>(ContextID);

        // Maybe an empty table? (this is a lcms extension)
        if (nEntries <= 0) return true;

        if (nEntries < 2 || nChannels > cmsMAXCHANNELS) return false;

        // Init table to zero
        //memset(Tables, 0, _sizeof<nint>() * cmsMAXCHANNELS);
        ToneCurve[] Tables = pool.Rent(cmsMAXCHANNELS);

        for (var i = 0; i < nChannels; i++)
        {
            Tables[i] = cmsBuildTabulatedToneCurve16(ContextID, nEntries, null)!;
            if (Tables[i] is null) goto Error;

            fixed (ushort* t = &Tables[i].Table16[0])
                if (!_cmsReadUInt16Array(io, nEntries, t)) goto Error;
        }

        // Add the table (which may certainly be an identity, but this is up to the optimizer, not the reading code)
        if (!cmsPipelineInsertStage(lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, nChannels, Tables)))
            goto Error;

        for (var i = 0; i < nChannels; i++)
            cmsFreeToneCurve(Tables[i]);

        pool.Return(Tables);
        return true;

    Error:
        for (var i = 0; i < nChannels; i++)
            if (Tables[i] is not null) cmsFreeToneCurve(Tables[i]);

        pool.Return(Tables);
        return false;
    }

    private static bool Write16bitTables(Context? ContextID, IOHandler io, StageToneCurvesData Tables)
    {
        _cmsAssert(Tables);

        var nEntries = Tables.TheCurves[0].nEntries;

        for (var i = 0; i < nEntries; i++)
        {
            // Usual case of identity curves
            for (var j = 0; j < 256; j++)
                if (!_cmsWriteUInt16Number(io, Tables.TheCurves[i].Table16[j])) return false;
        }
        return true;
    }

    private static Pipeline? Type_LUT16_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        ushort InputEntries, OutputEntries;
        byte InputChannels, OutputChannels, CLUTpoints;
        Pipeline? NewLUT = null;
        Span<double> Matrix = stackalloc double[3 * 3];

        *nItems = 0;

        if (!_cmsReadUInt8Number(io, &InputChannels)) goto Error;
        if (!_cmsReadUInt8Number(io, &OutputChannels)) goto Error;
        if (!_cmsReadUInt8Number(io, &CLUTpoints)) goto Error;  // 255 maximum

        // Padding
        if (!_cmsReadUInt8Number(io, null)) goto Error;

        // Do some checking
        if (InputChannels is 0 or > cmsMAXCHANNELS) goto Error;
        if (OutputChannels is 0 or > cmsMAXCHANNELS) goto Error;

        // Allocates an empty LUT
        NewLUT = cmsPipelineAlloc(self.ContextID, InputChannels, OutputChannels);
        if (NewLUT is null) goto Error;

        // Read the Matrix
        for (var i = 0; i < 9; i++)
            if (!_cmsRead15Fixed16Number(io, out Matrix[i])) goto Error;

        // Only operates on 3 channels
        if ((InputChannels is 3) &&
            !_cmsMAT3isIdentity(new(Matrix)) &&
            !cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, cmsStageAllocMatrix(self.ContextID, 3, 3, Matrix, null)))
        {
            goto Error;
        }

        if (!_cmsReadUInt16Number(io, &InputEntries)) goto Error;
        if (!_cmsReadUInt16Number(io, &OutputEntries)) goto Error;

        if (InputEntries > 0x7FFF || OutputEntries > 0x7FFF) goto Error;
        if (CLUTpoints is 1) goto Error;    // Impossible value, 0 for no CLUT and then 2 at least

        // Get input tables
        if (!Read16bitTables(self.ContextID, io, NewLUT, InputChannels, InputEntries)) goto Error;

        // Get 3D CLUT
        var nTabSize = uipow(OutputChannels, CLUTpoints, InputChannels);
        if (nTabSize == unchecked((uint)-1)) goto Error;
        if (nTabSize > 0)
        {
            var T = _cmsCalloc<ushort>(self.ContextID, nTabSize);
            if (T is null) goto Error;

            if (!_cmsReadUInt16Array(io, nTabSize, T))
            {
                _cmsFree(self.ContextID, T);
                goto Error;
            }

            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, cmsStageAllocCLut16bit(self.ContextID, CLUTpoints, InputChannels, OutputChannels, T)))
            {
                _cmsFree(self.ContextID, T);
                goto Error;
            }
            _cmsFree(self.ContextID, T);
        }

        // Get output tables
        if (!Read16bitTables(self.ContextID, io, NewLUT, OutputChannels, OutputEntries)) goto Error;

        *nItems = 1;
        return NewLUT;

    Error:
        if (NewLUT is not null) cmsPipelineFree(NewLUT);

        return null;
    }

    private static bool Type_LUT16_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Pipeline NewLut)
            return false;

        ReadOnlySpan<double> ident = stackalloc double[9] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        StageToneCurvesData? PreMPE = null, PostMPE = null;
        StageMatrixData? MatMPE = null;
        StageCLutData<ushort>? clut = null;

        // Disassemble the LUT into components.
        var mpe = NewLut.Elements;
        if (mpe?.Type == cmsSigMatrixElemType)
        {
            if (mpe.InputChannels is not 3 || mpe.OutputChannels is not 3) return false;
            MatMPE = mpe.Data as StageMatrixData;
            mpe = mpe.Next;
        }

        if (mpe?.Type == cmsSigCurveSetElemType)
        {
            PreMPE = mpe.Data as StageToneCurvesData;
            mpe = mpe.Next;
        }

        if (mpe?.Type == cmsSigCLutElemType)
        {
            clut = mpe.Data as StageCLutData<ushort>;
            mpe = mpe.Next;
        }

        if (mpe?.Type == cmsSigCurveSetElemType)
        {
            PostMPE = mpe.Data as StageToneCurvesData;
            mpe = mpe.Next;
        }

        // That should be all
        if (mpe is not null)
        {
            cmsSignalError(mpe.ContextID, ErrorCode.UnknownExtension, "LUT is not suitable to be saved as LUT16");
            return false;
        }

        var InputChannels = cmsPipelineInputChannels(NewLut);
        var OutputChannels = cmsPipelineOutputChannels(NewLut);

        var clutPoints = (clut?.Params.nSamples[0]) ?? 0u;

        if (!_cmsWriteUInt8Number(io, (byte)NewLut.InputChannels)) return false;
        if (!_cmsWriteUInt8Number(io, (byte)NewLut.OutputChannels)) return false;
        if (!_cmsWriteUInt8Number(io, (byte)clutPoints)) return false;
        if (!_cmsWriteUInt8Number(io, 0)) return false; // Padding

        var mat = MatMPE is not null ? MatMPE.Double : ident;
        for (var i = 0; i < 9; i++)
            if (!_cmsWrite15Fixed16Number(io, mat[i])) return false;

        if (!_cmsWriteUInt16Number(io, (ushort)(PreMPE is not null ? PreMPE.TheCurves[0].nEntries : 2))) return false;
        if (!_cmsWriteUInt16Number(io, (ushort)(PostMPE is not null ? PostMPE.TheCurves[0].nEntries : 2))) return false;

        // The prelinearization table
        if (PreMPE is not null)
        {
            if (!Write16bitTables(self.ContextID, io, PreMPE)) return false;
        }
        else
        {
            for (var i = 0; i < InputChannels; i++)
            {
                if (!_cmsWriteUInt16Number(io, 0)) return false;
                if (!_cmsWriteUInt16Number(io, 0xFFFF)) return false;
            }
        }

        var nTabSize = uipow(NewLut.OutputChannels, clutPoints, NewLut.InputChannels);
        if (nTabSize == unchecked((uint)-1)) return false;
        if (nTabSize > 0)
        {
            // The 3D CLUT
            if (clut is not null && !_cmsWriteUInt16Array(io, nTabSize, (ushort*)Unsafe.AsPointer(ref clut.TUshort.GetPinnableReference()))) return false;
        }

        // The postlinearization table
        if (PostMPE is not null)
        {
            if (!Write16bitTables(self.ContextID, io, PostMPE)) return false;
        }
        else
        {
            for (var i = 0; i < InputChannels; i++)
            {
                if (!_cmsWriteUInt16Number(io, 0)) return false;
                if (!_cmsWriteUInt16Number(io, 0xFFFF)) return false;
            }
        }

        return true;
    }

    private static Pipeline? Type_LUT16_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        cmsPipelineDup(Ptr as Pipeline);

    private static void Type_LUT16_Free(TagTypeHandler _, object? Ptr) =>
        cmsPipelineFree(Ptr as Pipeline);

    #endregion LUT16

    #region LUTA2B

    /* LutAtoB type

        This structure represents a colour transform. The type contains up to five processing
        elements which are stored in the AtoBTag tag in the following order: a set of one
        dimensional curves, a 3 by 3 matrix with offset terms, a set of one dimensional curves,
        a multidimensional lookup table, and a set of one dimensional output curves.
        Data are processed using these elements via the following sequence:

    ("A" curves) -> (multidimensional lookup table - CLUT) -> ("M" curves) -> (matrix) -> ("B" curves).

    It is possible to use any or all of these processing elements. At least one processing element
    must be included.Only the following combinations are allowed:

    B
    M - Matrix - B
    A - CLUT - B
    A - CLUT - M - Matrix - B

    */

    private static Stage? ReadMatrix(TagTypeHandler self, IOHandler io, uint Offset)
    {
        Span<double> dMat = stackalloc double[3 * 3];
        Span<double> dOff = stackalloc double[3];

        // Go to address
        if (!io.Seek(io, Offset)) return null;

        // Read the Matrix and Offsets
        for (var i = 0; i < 3 * 3; i++)
            if (!_cmsRead15Fixed16Number(io, out dMat[i])) return null;
        for (var i = 0; i < 3; i++)
            if (!_cmsRead15Fixed16Number(io, out dOff[i])) return null;

        return cmsStageAllocMatrix(self.ContextID, 3, 3, dMat, dOff);
    }

    private static Stage? ReadCLUT(TagTypeHandler self, IOHandler io, uint Offset, uint InputChannels, uint OutputChannels)
    {
        var gridPoints8 = stackalloc byte[cmsMAXCHANNELS];  // Number of grid points in each dimension
        Span<uint> GridPoints = stackalloc uint[cmsMAXCHANNELS];
        byte Precision;

        if (!io.Seek(io, Offset)) return null;
        if (io.Read(io, gridPoints8, cmsMAXCHANNELS, 1) is not 1) return null;

        for (var i = 0; i < cmsMAXCHANNELS; i++)
        {
            if (gridPoints8[i] is 1) return null; // Imposible value, 0 for no CLUT and then 2 at least
            GridPoints[i] = gridPoints8[i];
        }

        if (!_cmsReadUInt8Number(io, &Precision)) return null;

        if (!_cmsReadUInt8Number(io, null)) return null;
        if (!_cmsReadUInt8Number(io, null)) return null;
        if (!_cmsReadUInt8Number(io, null)) return null;

        var CLUT = cmsStageAllocCLut16bitGranular(self.ContextID, GridPoints, InputChannels, OutputChannels, null);
        if (CLUT is null || CLUT.Data is not StageCLutData<ushort> Data)
            return null;

        // Predcision can be 1 or 2 bytes
        switch (Precision)
        {
            case 1:
                byte v;

                for (var i = 0; i < Data.nEntries; i++)
                {
                    if (io.Read(io, &v, _sizeof<byte>(), 1) is not 1)
                    {
                        cmsStageFree(CLUT);
                        return null;
                    }
                    Data.TUshort[i] = FROM_8_TO_16(v);
                }
                break;

            case 2:
                if (!_cmsReadUInt16Array(io, Data.nEntries, (ushort*)Unsafe.AsPointer(ref Data.TUshort.GetPinnableReference())))
                {
                    cmsStageFree(CLUT);
                    return null;
                }
                break;

            default:
                cmsStageFree(CLUT);
                cmsSignalError(self.ContextID, ErrorCode.UnknownExtension, $"Unknown precision of '{Precision}'");
                return null;
        }

        return CLUT;
    }

    private static ToneCurve? ReadEmbeddedCurve(TagTypeHandler self, IOHandler io)
    {
        uint nItems;

        var BaseType = _cmsReadTypeBase(io);
        if (BaseType == cmsSigCurveType)
        {
            return Type_Curve_Read(self, io, &nItems, 0);
        }
        else if (BaseType == cmsSigParametricCurveType)
        {
            return Type_ParametricCurve_Read(self, io, &nItems, 0);
        }
        else
        {
            var buf = stackalloc byte[5];
            _cmsTagSignature2String(buf, BaseType);
            cmsSignalError(self.ContextID, ErrorCode.UnknownExtension, $"Unknown curve type '{new string((sbyte*)buf)}'");
            return null;
        }
    }

    private static Stage? ReadSetOfCurves(TagTypeHandler self, IOHandler io, uint Offset, uint nCurves)
    {
        var pool = Context.GetPool<ToneCurve>(self.ContextID);
        Stage? Lin = null;

        if (nCurves > cmsMAXCHANNELS) return null;

        if (!io.Seek(io, Offset)) return null;

        ToneCurve[] Curves = pool.Rent(cmsMAXCHANNELS);
        for (var i = 0; i < nCurves; i++)
            Curves[i] = null!;

        for (var i = 0; i < nCurves; i++)
        {
            Curves[i] = ReadEmbeddedCurve(self, io)!;
            if (Curves[i] is null) goto Error;
            if (!_cmsReadAlignment(io)) goto Error;
        }

        Lin = cmsStageAllocToneCurves(self.ContextID, nCurves, Curves);

    Error:
        for (var i = 0; i < nCurves; i++)
            cmsFreeToneCurve(Curves[i]);

        pool.Return(Curves);
        return Lin;
    }

    private static Pipeline? Type_LUTA2B_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        byte inputChan;     // Number of input channels
        byte outputChan;    // Number of output channels
        uint offsetB;       // Offset to first "B" curve
        uint offsetMat;     // Offset to matrix
        uint offsetM;       // Offset to first "M" curve
        uint offsetC;       // Offset to CLUT
        uint offsetA;       // Offset to first "A" curve
        Pipeline? NewLUT = null;

        *nItems = 0;

        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        if (!_cmsReadUInt8Number(io, &inputChan)) goto Error;
        if (!_cmsReadUInt8Number(io, &outputChan)) goto Error;

        if (!_cmsReadUInt16Number(io, null)) goto Error;

        if (!_cmsReadUInt32Number(io, &offsetB)) goto Error;
        if (!_cmsReadUInt32Number(io, &offsetMat)) goto Error;
        if (!_cmsReadUInt32Number(io, &offsetM)) goto Error;
        if (!_cmsReadUInt32Number(io, &offsetC)) goto Error;
        if (!_cmsReadUInt32Number(io, &offsetA)) goto Error;

        // Do some checking
        if (inputChan is 0 or > cmsMAXCHANNELS) goto Error;
        if (outputChan is 0 or > cmsMAXCHANNELS) goto Error;

        // Allocates an empty LUT
        NewLUT = cmsPipelineAlloc(self.ContextID, inputChan, outputChan);
        if (NewLUT is null) goto Error;

        if (offsetA is not 0)
        {
            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, ReadSetOfCurves(self, io, BaseOffset + offsetA, inputChan)))
                goto Error;
        }

        if (offsetC is not 0)
        {
            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, ReadCLUT(self, io, BaseOffset + offsetC, inputChan, outputChan)))
                goto Error;
        }

        if (offsetM is not 0)
        {
            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, ReadSetOfCurves(self, io, BaseOffset + offsetM, outputChan)))
                goto Error;
        }

        if (offsetMat is not 0)
        {
            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, ReadMatrix(self, io, BaseOffset + offsetMat)))
                goto Error;
        }

        if (offsetB is not 0)
        {
            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, ReadSetOfCurves(self, io, BaseOffset + offsetB, outputChan)))
                goto Error;
        }

        *nItems = 1;
        return NewLUT;

    Error:
        if (NewLUT is not null) cmsPipelineFree(NewLUT);

        return null;
    }

    private static bool WriteMatrix(TagTypeHandler _, IOHandler io, Stage mpe)
    {
        Span<double> zeros = stackalloc double[(int)mpe.OutputChannels];
        if (mpe.Data is not StageMatrixData m)
            return false;

        //memset(zeros, 0, (int)mpe.OutputChannels * sizeof(double));
        zeros.Clear();

        var n = mpe.InputChannels * mpe.OutputChannels;

        // Write the Matrix
        for (var i = 0; i < n; i++)
            if (!_cmsWrite15Fixed16Number(io, m.Double[i])) return false;

        var offsets = m.Offset is not null ? m.Offset : zeros;
        for (var i = 0; i < mpe.OutputChannels; i++)
            if (!_cmsWrite15Fixed16Number(io, offsets[i])) return false;

        return true;
    }

    private static bool WriteSetOfCurves(TagTypeHandler self, IOHandler io, Signature Type, Stage mpe)
    {
        var n = cmsStageOutputChannels(mpe);
        var Curves = _cmsStageGetPtrToCurveSet(mpe);

        for (var i = 0; i < n; i++)
        {
            // If this is a table-based curve, use curve type even on V4
            var CurrentType = Type;

            if ((Curves[i].nSegments is 0) ||
                ((Curves[i].nSegments is 2) && (Curves[i].Segments[1].Type is 0)) ||
                Curves[i].Segments[0].Type < 0)
            {
                CurrentType = cmsSigCurveType;
            }

            if (!_cmsWriteTypeBase(io, CurrentType)) return false;

            if (CurrentType == cmsSigCurveType)
            {
                if (!Type_Curve_Write(self, io, Curves[i], 1)) return false;
            }
            else if (CurrentType == cmsSigParametricCurveType)
            {
                if (!Type_ParametricCurve_Write(self, io, Curves[i], 1)) return false;
            }
            else
            {
                var buf = stackalloc byte[5];
                _cmsTagSignature2String(buf, Type);
                cmsSignalError(self.ContextID, ErrorCode.UnknownExtension, $"Unknown curve type '{new string((sbyte*)buf)}'");
                return false;
            }

            if (!_cmsWriteAlignment(io)) return false;
        }
        return true;
    }

    private static bool WriteCLUT(TagTypeHandler self, IOHandler io, byte Precision, Stage mpe)
    {
        var gridPoints = stackalloc byte[cmsMAXCHANNELS]; // Number of grid points in each dimension.

        if (mpe.Data is StageCLutData<float>)
        {
            cmsSignalError(self.ContextID, ErrorCode.NotSuitable, "Cannot save floating point data, CLUT are 8 or 16 bit only");
            return false;
        }

        if (mpe.Data is not StageCLutData<ushort> CLUT)
            return false;

        memset(gridPoints, 0, _sizeof<byte>() * cmsMAXCHANNELS);
        for (var i = 0; i < CLUT.Params.nInputs; i++)
            gridPoints[i] = (byte)CLUT.Params.nSamples[i];

        if (!io.Write(io, (uint)(cmsMAXCHANNELS * _sizeof<byte>()), gridPoints)) return false;

        if (!_cmsWriteUInt8Number(io, Precision)) return false;
        if (!_cmsWriteUInt8Number(io, 0)) return false;
        if (!_cmsWriteUInt8Number(io, 0)) return false;
        if (!_cmsWriteUInt8Number(io, 0)) return false;

        // Precision can be 1 or 2 bytes
        switch (Precision)
        {
            case 1:
                for (var i = 0; i < CLUT.nEntries; i++)
                    if (!_cmsWriteUInt8Number(io, FROM_16_TO_8(CLUT.TUshort[i]))) return false;
                break;

            case 2:
                if (!_cmsWriteUInt16Array(io, CLUT.nEntries, (ushort*)Unsafe.AsPointer(ref CLUT.TUshort.GetPinnableReference()))) return false;
                break;

            default:
                cmsSignalError(self.ContextID, ErrorCode.UnknownExtension, $"Unknown precision of '{Precision}'");
                return false;
        }

        return _cmsWriteAlignment(io);
    }

    private static bool Type_LUTA2B_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Pipeline Lut)
            return false;

        Stage? A = null, B = null, M = null, Matrix = null, CLUT = null;
        uint offsetB = 0, offsetMat = 0, offsetM = 0, offsetC = 0, offsetA = 0;

        // Get the base for all offsets
        var BassOffset = io.Tell(io) - _sizeof<TagBase>();

        if (Lut.Elements is not null &&
            !cmsPipelineCheckAndRetrieveStages(Lut, cmsSigCurveSetElemType, out B) &&
            !cmsPipelineCheckAndRetrieveStages(Lut, cmsSigCurveSetElemType, out M, cmsSigMatrixElemType, out Matrix, cmsSigCurveSetElemType, out B) &&
            !cmsPipelineCheckAndRetrieveStages(Lut, cmsSigCurveSetElemType, out A, cmsSigCLutElemType, out CLUT, cmsSigCurveSetElemType, out B) &&
            !cmsPipelineCheckAndRetrieveStages(Lut, cmsSigCurveSetElemType, out A, cmsSigCLutElemType, out CLUT, cmsSigCurveSetElemType, out M,
                                                    cmsSigMatrixElemType, out Matrix, cmsSigCurveSetElemType, out B))
        {
            cmsSignalError(self.ContextID, ErrorCode.NotSuitable, "LUT is not suitable to be saved as LutAToB");
            return false;
        }

        // Get input/output channels
        var inputChan = cmsPipelineInputChannels(Lut);
        var outputChan = cmsPipelineOutputChannels(Lut);

        // Write channel count
        if (!_cmsWriteUInt8Number(io, (byte)inputChan)) return false;
        if (!_cmsWriteUInt8Number(io, (byte)outputChan)) return false;
        if (!_cmsWriteUInt16Number(io, 0)) return false;

        // Keep directory location to be filled later
        var DirectoryPos = io.Tell(io);

        // Write the directory
        if (!_cmsWriteUInt32Number(io, 0)) return false;
        if (!_cmsWriteUInt32Number(io, 0)) return false;
        if (!_cmsWriteUInt32Number(io, 0)) return false;
        if (!_cmsWriteUInt32Number(io, 0)) return false;
        if (!_cmsWriteUInt32Number(io, 0)) return false;

        if (A is not null)
        {
            offsetA = io.Tell(io) - BassOffset;
            if (!WriteSetOfCurves(self, io, cmsSigParametricCurveType, A)) return false;
        }

        if (CLUT is not null)
        {
            offsetC = io.Tell(io) - BassOffset;
            if (!WriteCLUT(self, io, (byte)(Lut.SaveAs8Bits ? 1 : 2), CLUT)) return false;
        }

        if (M is not null)
        {
            offsetM = io.Tell(io) - BassOffset;
            if (!WriteSetOfCurves(self, io, cmsSigParametricCurveType, M)) return false;
        }

        if (Matrix is not null)
        {
            offsetMat = io.Tell(io) - BassOffset;
            if (!WriteMatrix(self, io, Matrix)) return false;
        }

        if (B is not null)
        {
            offsetB = io.Tell(io) - BassOffset;
            if (!WriteSetOfCurves(self, io, cmsSigParametricCurveType, B)) return false;
        }

        var CurrentPos = io.Tell(io);

        if (!io.Seek(io, DirectoryPos)) return false;

        if (!_cmsWriteUInt32Number(io, offsetB)) return false;
        if (!_cmsWriteUInt32Number(io, offsetMat)) return false;
        if (!_cmsWriteUInt32Number(io, offsetM)) return false;
        if (!_cmsWriteUInt32Number(io, offsetC)) return false;
        if (!_cmsWriteUInt32Number(io, offsetA)) return false;

        return io.Seek(io, CurrentPos);
    }

    private static Pipeline? Type_LUTA2B_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        cmsPipelineDup(Ptr as Pipeline);

    private static void Type_LUTA2B_Free(TagTypeHandler _, object? Ptr) =>
        cmsPipelineFree(Ptr as Pipeline);

    #endregion LUTA2B

    #region LUTB2A

    private static Pipeline? Type_LUTB2A_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        byte inputChan;     // Number of input channels
        byte outputChan;    // Number of output channels
        uint offsetB;       // Offset to first "B" curve
        uint offsetMat;     // Offset to matrix
        uint offsetM;       // Offset to first "M" curve
        uint offsetC;       // Offset to CLUT
        uint offsetA;       // Offset to first "A" curve
        Pipeline? NewLUT = null;

        *nItems = 0;

        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        if (!_cmsReadUInt8Number(io, &inputChan)) goto Error;
        if (!_cmsReadUInt8Number(io, &outputChan)) goto Error;

        if (!_cmsReadUInt16Number(io, null)) goto Error;

        if (!_cmsReadUInt32Number(io, &offsetB)) goto Error;
        if (!_cmsReadUInt32Number(io, &offsetMat)) goto Error;
        if (!_cmsReadUInt32Number(io, &offsetM)) goto Error;
        if (!_cmsReadUInt32Number(io, &offsetC)) goto Error;
        if (!_cmsReadUInt32Number(io, &offsetA)) goto Error;

        // Do some checking
        if (inputChan is 0 or > cmsMAXCHANNELS) goto Error;
        if (outputChan is 0 or > cmsMAXCHANNELS) goto Error;

        // Allocates an empty LUT
        NewLUT = cmsPipelineAlloc(self.ContextID, inputChan, outputChan);
        if (NewLUT is null) goto Error;

        if (offsetB is not 0)
        {
            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, ReadSetOfCurves(self, io, BaseOffset + offsetB, outputChan)))
                goto Error;
        }

        if (offsetC is not 0)
        {
            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, ReadCLUT(self, io, BaseOffset + offsetC, inputChan, outputChan)))
                goto Error;
        }

        if (offsetM is not 0)
        {
            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, ReadSetOfCurves(self, io, BaseOffset + offsetM, outputChan)))
                goto Error;
        }

        if (offsetMat is not 0)
        {
            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, ReadMatrix(self, io, BaseOffset + offsetMat)))
                goto Error;
        }

        if (offsetA is not 0)
        {
            if (!cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, ReadSetOfCurves(self, io, BaseOffset + offsetA, inputChan)))
                goto Error;
        }

        *nItems = 1;
        return NewLUT;

    Error:
        if (NewLUT is not null) cmsPipelineFree(NewLUT);

        return null;
    }

    private static bool Type_LUTB2A_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Pipeline Lut)
            return false;

        Stage? A = null, B = null, M = null, Matrix = null, CLUT = null;
        uint offsetB = 0, offsetMat = 0, offsetM = 0, offsetC = 0, offsetA = 0;

        // Get the base for all offsets
        var BassOffset = io.Tell(io) - _sizeof<TagBase>();

        if (Lut.Elements is not null &&
            !cmsPipelineCheckAndRetrieveStages(Lut, cmsSigCurveSetElemType, out B) &&
            !cmsPipelineCheckAndRetrieveStages(Lut, cmsSigCurveSetElemType, out B, cmsSigMatrixElemType, out Matrix, cmsSigCurveSetElemType, out M) &&
            !cmsPipelineCheckAndRetrieveStages(Lut, cmsSigCurveSetElemType, out B, cmsSigCLutElemType, out CLUT, cmsSigCurveSetElemType, out A) &&
            !cmsPipelineCheckAndRetrieveStages(Lut, cmsSigCurveSetElemType, out B, cmsSigMatrixElemType, out Matrix, cmsSigCurveSetElemType, out M,
                                                    cmsSigCLutElemType, out CLUT, cmsSigCurveSetElemType, out A))
        {
            cmsSignalError(self.ContextID, ErrorCode.NotSuitable, "LUT is not suitable to be saved as LutAToB");
            return false;
        }

        // Get input/output channels
        var inputChan = cmsPipelineInputChannels(Lut);
        var outputChan = cmsPipelineOutputChannels(Lut);

        // Write channel count
        if (!_cmsWriteUInt8Number(io, (byte)inputChan)) return false;
        if (!_cmsWriteUInt8Number(io, (byte)outputChan)) return false;
        if (!_cmsWriteUInt16Number(io, 0)) return false;

        // Keep directory location to be filled later
        var DirectoryPos = io.Tell(io);

        // Write the directory
        if (!_cmsWriteUInt32Number(io, 0)) return false;
        if (!_cmsWriteUInt32Number(io, 0)) return false;
        if (!_cmsWriteUInt32Number(io, 0)) return false;
        if (!_cmsWriteUInt32Number(io, 0)) return false;
        if (!_cmsWriteUInt32Number(io, 0)) return false;

        if (A is not null)
        {
            offsetA = io.Tell(io) - BassOffset;
            if (!WriteSetOfCurves(self, io, cmsSigParametricCurveType, A)) return false;
        }

        if (CLUT is not null)
        {
            offsetC = io.Tell(io) - BassOffset;
            if (!WriteCLUT(self, io, (byte)(Lut.SaveAs8Bits ? 1 : 2), CLUT)) return false;
        }

        if (M is not null)
        {
            offsetM = io.Tell(io) - BassOffset;
            if (!WriteSetOfCurves(self, io, cmsSigParametricCurveType, M)) return false;
        }

        if (Matrix is not null)
        {
            offsetMat = io.Tell(io) - BassOffset;
            if (!WriteMatrix(self, io, Matrix)) return false;
        }

        if (B is not null)
        {
            offsetB = io.Tell(io) - BassOffset;
            if (!WriteSetOfCurves(self, io, cmsSigParametricCurveType, B)) return false;
        }

        var CurrentPos = io.Tell(io);

        if (!io.Seek(io, DirectoryPos)) return false;

        if (!_cmsWriteUInt32Number(io, offsetB)) return false;
        if (!_cmsWriteUInt32Number(io, offsetMat)) return false;
        if (!_cmsWriteUInt32Number(io, offsetM)) return false;
        if (!_cmsWriteUInt32Number(io, offsetC)) return false;
        if (!_cmsWriteUInt32Number(io, offsetA)) return false;

        return io.Seek(io, CurrentPos);
    }

    private static Pipeline? Type_LUTB2A_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        cmsPipelineDup(Ptr as Pipeline);

    private static void Type_LUTB2A_Free(TagTypeHandler _, object? Ptr) =>
        cmsPipelineFree(Ptr as Pipeline);

    #endregion LUTB2A

    #region ColorantTable

    private static NamedColorList? Type_ColorantTable_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        var Name = stackalloc byte[34];
        var PCS = stackalloc ushort[3];
        NamedColorList? List = null;
        uint Count;
        Span<byte> preSufix = stackalloc byte[1] { 0 };

        *nItems = 0;
        if (!_cmsReadUInt32Number(io, &Count)) return null;

        if (Count > cmsMAXCHANNELS)
        {
            cmsSignalError(self.ContextID, ErrorCode.Range, $"Too many colorants '{Count}'");
            return null;
        }

        List = cmsAllocNamedColorList(self.ContextID, Count, 0, preSufix, preSufix);
        if (List is null) return null;

        for (var i = 0; i < Count; i++)
        {
            if (io.Read(io, Name, 32, 1) is not 1) goto Error;
            Name[32] = 0;

            if (!_cmsReadUInt16Array(io, 3, PCS)) goto Error;

            if (!cmsAppendNamedColor(List, new(Name, 34), new(PCS, 3), null)) goto Error;
        }

        *nItems = 1;

        return List;

    Error:
        cmsFreeNamedColorList(List);
        return null;
    }

    private static bool Type_ColorantTable_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        var root = stackalloc byte[cmsMAX_PATH];
        var PCS = stackalloc ushort[3];

        if (Ptr is not NamedColorList NamedColorList)
            return false;

        var nColors = cmsNamedColorCount(NamedColorList);

        if (!_cmsWriteUInt32Number(io, nColors)) return false;

        for (var i = 0u; i < nColors; i++)
        {
            memset(root, 0, cmsMAX_PATH * _sizeof<byte>());

            if (!cmsNamedColorInfo(NamedColorList, i, new(root, cmsMAX_PATH), null, null, new(PCS, 3), null)) return false;
            root[32] = 0;

            if (!io.Write(io, 32, root)) return false;
            if (!_cmsWriteUInt16Array(io, 3, PCS)) return false;
        }

        return true;
    }

    #endregion ColorantTable

    #region NamedColor

    private static NamedColorList? Type_NamedColor_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        uint vendorFlag;                      // Bottom 16 bits for ICC use
        uint count;                           // Count of named colors
        uint nDeviceCoords;                   // Num of device coordinates
        byte* prefix = stackalloc byte[32];   // Prefix for each color name
        byte* suffix = stackalloc byte[32];   // Suffix for each color name
        NamedColorList? v = null;
        ushort* PCS = stackalloc ushort[3];
        ushort* Colorant = stackalloc ushort[cmsMAXCHANNELS];
        byte* Root = stackalloc byte[33];

        *nItems = 0;
        if (!_cmsReadUInt32Number(io, &vendorFlag)) return null;
        if (!_cmsReadUInt32Number(io, &count)) return null;
        if (!_cmsReadUInt32Number(io, &nDeviceCoords)) return null;

        if (io.Read(io, prefix, 32, 1) is not 1) return null;
        if (io.Read(io, suffix, 32, 1) is not 1) return null;

        prefix[31] = suffix[31] = 0;

        v = cmsAllocNamedColorList(self.ContextID, count, nDeviceCoords, new(prefix, 32), new(suffix, 32));
        if (v is null)
        {
            cmsSignalError(self.ContextID, ErrorCode.Range, $"Too many named colors '{count}'");
            return null;
        }

        if (nDeviceCoords > cmsMAXCHANNELS)
        {
            cmsSignalError(self.ContextID, ErrorCode.Range, $"Too many device coordinates '{nDeviceCoords}'");
            goto Error;
        }

        for (var i = 0; i < count; i++)
        {
            memset(Colorant, 0, _sizeof<ushort>() * cmsMAXCHANNELS);
            if (io.Read(io, Root, 32, 1) is not 1) goto Error;
            Root[32] = 0;

            if (!_cmsReadUInt16Array(io, 3, PCS)) goto Error;
            if (!_cmsReadUInt16Array(io, nDeviceCoords, Colorant)) goto Error;

            if (!cmsAppendNamedColor(v, new(Root, 33), new(PCS, 3), new(Colorant, cmsMAXCHANNELS))) goto Error;
        }

        *nItems = 1;

        return v;

    Error:
        cmsFreeNamedColorList(v);
        return null;
    }

    private static bool Type_NamedColor_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        var prefix = stackalloc byte[33];
        var suffix = stackalloc byte[33];
        var Root = stackalloc byte[cmsMAX_PATH];
        var PCS = stackalloc ushort[3];
        var Colorant = stackalloc ushort[cmsMAXCHANNELS];

        if (Ptr is not NamedColorList NamedColorList)
            return false;

        var nColors = cmsNamedColorCount(NamedColorList);

        if (!_cmsWriteUInt32Number(io, 0)) return false;
        if (!_cmsWriteUInt32Number(io, nColors)) return false;
        if (!_cmsWriteUInt32Number(io, NamedColorList.ColorantCount)) return false;

        memcpy(new Span<byte>(prefix, 33), NamedColorList.Prefix, 33);
        memcpy(new Span<byte>(suffix, 33), NamedColorList.Suffix, 33);

        suffix[32] = prefix[32] = 0;

        if (!io.Write(io, 32, prefix)) return false;
        if (!io.Write(io, 32, suffix)) return false;

        for (var i = 0u; i < nColors; i++)
        {
            memset(Root, 0, cmsMAX_PATH * _sizeof<byte>());
            memset(PCS, 0, 3 * _sizeof<ushort>());
            memset(Colorant, 0, cmsMAXCHANNELS * _sizeof<ushort>());

            if (!cmsNamedColorInfo(NamedColorList, i, new(Root, cmsMAX_PATH), null, null, new(PCS, 3), new(Colorant, cmsMAXCHANNELS))) return false;
            Root[32] = 0;
            if (!io.Write(io, 32, Root)) return false;
            if (!_cmsWriteUInt16Array(io, 3, PCS)) return false;
            if (!_cmsWriteUInt16Array(io, NamedColorList.ColorantCount, Colorant)) return false;
        }

        return true;
    }

    #endregion NamedColor

    #region ProfileSequenceDesc

    private static bool ReadEmbeddedText(TagTypeHandler self, IOHandler io, out Mlu? mlu, uint SizeOfTag)
    {
        uint nItems;

        var BaseType = _cmsReadTypeBase(io);

        switch (BaseType)
        {
            case cmsSigTextType:
                //if (*mlu is not null) cmsMLUfree(*mlu);
                mlu = Type_Text_Read(self, io, &nItems, SizeOfTag);
                return mlu is not null;

            case cmsSigTextDescriptionType:
                //if (*mlu is not null) cmsMLUfree(*mlu);
                mlu = Type_Text_Description_Read(self, io, &nItems, SizeOfTag);
                return mlu is not null;

            /*
            TBD: Size is needed for MLU, and we have no idea on which is the available size
            */

            case cmsSigMultiLocalizedUnicodeType:
                //if (*mlu is not null) cmsMLUfree(*mlu);
                mlu = Type_MLU_Read(self, io, &nItems, SizeOfTag);
                return mlu is not null;

            default:
                mlu = null;
                return false;
        }
    }

    private static Sequence? Type_ProfileSequenceDesc_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        Sequence? OutSeq = null;
        uint Count;

        *nItems = 0;
        if (!_cmsReadUInt32Number(io, &Count)) return null;

        if (SizeOfTag < _sizeof<uint>()) return null;
        SizeOfTag -= _sizeof<uint>();

        OutSeq = cmsAllocProfileSequenceDescription(self.ContextID, Count);
        if (OutSeq is null) return null;

        OutSeq.n = Count;

        // Get structures as well

        for (var i = 0; i < Count; i++)
        {
            var sec = OutSeq.seq[i];

            fixed (void* ptr = &sec.deviceMfg)
                if (!_cmsReadUInt32Number(io, (uint*)ptr)) goto Error;
            if (SizeOfTag < _sizeof<uint>()) goto Error;
            SizeOfTag -= _sizeof<uint>();

            fixed (void* ptr = &sec.deviceModel)
                if (!_cmsReadUInt32Number(io, (uint*)ptr)) goto Error;
            if (SizeOfTag < _sizeof<uint>()) goto Error;
            SizeOfTag -= _sizeof<uint>();

            fixed (ulong* ptr = &sec.attributes)
                if (!_cmsReadUInt64Number(io, ptr)) goto Error;
            if (SizeOfTag < _sizeof<ulong>()) goto Error;
            SizeOfTag -= _sizeof<ulong>();

            fixed (void* ptr = &sec.technology)
                if (!_cmsReadUInt32Number(io, (uint*)ptr)) goto Error;
            if (SizeOfTag < _sizeof<uint>()) goto Error;
            SizeOfTag -= _sizeof<uint>();

            if (!ReadEmbeddedText(self, io, out sec.Manufacturer, SizeOfTag)) goto Error;
            if (!ReadEmbeddedText(self, io, out sec.Model, SizeOfTag)) goto Error;
        }

        *nItems = 1;

        return OutSeq;

    Error:
        cmsFreeProfileSequenceDescription(OutSeq);
        return null;
    }

    private static bool SaveDescription(TagTypeHandler self, IOHandler io, Mlu Text)
    {
        if (self.ICCVersion < 0x04000000)
        {
            if (!_cmsWriteTypeBase(io, cmsSigTextDescriptionType)) return false;
            return Type_Text_Description_Write(self, io, Text, 1);
        }
        else
        {
            if (!_cmsWriteTypeBase(io, cmsSigMultiLocalizedUnicodeType)) return false;
            return Type_MLU_Write(self, io, Text, 1);
        }
    }

    private static bool Type_ProfileSequenceDesc_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Sequence Seq)
            return false;

        if (!_cmsWriteUInt32Number(io, Seq.n)) return false;

        for (var i = 0u; i < Seq.n; i++)
        {
            var sec = Seq.seq[i];

            if (!_cmsWriteUInt32Number(io, sec.deviceMfg)) return false;
            if (!_cmsWriteUInt32Number(io, sec.deviceModel)) return false;
            if (!_cmsWriteUInt64Number(io, sec.attributes)) return false;
            if (!_cmsWriteUInt32Number(io, sec.technology)) return false;

            if (!SaveDescription(self, io, sec.Manufacturer)) return false;
            if (!SaveDescription(self, io, sec.Model)) return false;
        }

        return true;
    }

    #endregion ProfileSequenceDesc

    #region ProfileSequenceId

    private static bool ReadSeqID(TagTypeHandler self, IOHandler io, object? Cargo, uint n, uint SizeOfTag)
    {
        if (Cargo is not Sequence OutSeq)
            return false;

        var seq = OutSeq.seq[n];
        fixed (void* id8 = seq.ProfileID.id8)
            if (io.Read(io, id8, 16, 1) is not 1) return false;

        return ReadEmbeddedText(self, io, out seq.Description, SizeOfTag);
    }

    private static Sequence? Type_ProfileSequenceId_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        Sequence? OutSeq = null;
        uint Count;

        *nItems = 0;

        // Get actual position as a basis for element offsets
        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        // Get table count
        if (!_cmsReadUInt32Number(io, &Count)) return null;
        SizeOfTag -= _sizeof<uint>();

        // Allocate an empty structure
        OutSeq = cmsAllocProfileSequenceDescription(self.ContextID, Count);
        if (OutSeq is null) return null;

        // Read the position table
        if (!ReadPositionTable(self, io, Count, BaseOffset, OutSeq, &ReadSeqID))
        {
            cmsFreeProfileSequenceDescription(OutSeq);
            return null;
        }

        // Success
        *nItems = 1;
        return OutSeq;
    }

    private static bool WriteSeqID(TagTypeHandler self, IOHandler io, object? Cargo, uint n, uint _)
    {
        if (Cargo is not Sequence Seq)
            return false;

        fixed (void* id8 = Seq.seq[n].ProfileID.id8)
            if (!io.Write(io, 16, id8)) return false;

        // Store MLU here
        if (!SaveDescription(self, io, Seq.seq[n].Description)) return false;

        return true;
    }

    private static bool Type_ProfileSequenceId_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Sequence Seq) return false;

        // Keep the base offset
        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        // This is the table count
        if (!_cmsWriteUInt32Number(io, Seq.n)) return false;

        // This is the position table and content
        if (!WritePositionTable(self, io, 0, Seq.n, BaseOffset, Seq, WriteSeqID)) return false;

        return true;
    }

    #endregion ProfileSequenceId

    #region UcrBg

    private static Box<UcrBg>? Type_UcrBg_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        uint CountUcr, CountBg;
        int SignedSizeOfTag = (int)SizeOfTag;

        *nItems = 0;
        var n = new UcrBg();
        //if (n is null) return null;

        // First curve is Under color removal

        if (SignedSizeOfTag < _sizeof<uint>()) return null;
        if (!_cmsReadUInt32Number(io, &CountUcr)) return null;
        SignedSizeOfTag -= _sizeof<uint>();

        n.Ucr = cmsBuildTabulatedToneCurve16(self.ContextID, CountUcr, null)!;
        if (n.Ucr is null) goto Error;

        if (SignedSizeOfTag < (int)(CountUcr * _sizeof<ushort>())) goto Error;
        fixed (ushort* t = &n.Ucr.Table16[0])
            if (!_cmsReadUInt16Array(io, CountUcr, t)) goto Error;

        SignedSizeOfTag -= (int)(CountUcr * _sizeof<ushort>());

        // Secong curve is Black generation

        if (SignedSizeOfTag < _sizeof<uint>()) goto Error;
        if (!_cmsReadUInt32Number(io, &CountBg)) goto Error;
        SignedSizeOfTag -= _sizeof<uint>();

        n.Bg = cmsBuildTabulatedToneCurve16(self.ContextID, CountBg, null)!;
        if (n.Bg is null) goto Error;

        if (SignedSizeOfTag < (int)(CountBg * _sizeof<ushort>())) goto Error;
        fixed (ushort* t = &n.Bg.Table16[0])
            if (!_cmsReadUInt16Array(io, CountBg, t)) goto Error;
        SignedSizeOfTag -= (int)(CountBg * _sizeof<ushort>());

        if (SignedSizeOfTag is < 0 or > 32000) goto Error;

        // Now comes the text. The length is specified by the tag size
        var desc = cmsMLUalloc(self.ContextID, 1);
        if (desc is null) goto Error;
        n.Desc = desc;

        //ASCIIString = _cmsMalloc<byte>(self.ContextID, (uint)SignedSizeOfTag + 1);
        var ASCIIString = _cmsCallocArray<byte>(self.ContextID, (uint)SignedSizeOfTag);
        var tmpASCIIString = stackalloc byte[SignedSizeOfTag];
        if (io.Read(io, tmpASCIIString, _sizeof<byte>(), (uint)SignedSizeOfTag) != SignedSizeOfTag)
        {
            _cmsFree(self.ContextID, ASCIIString);
            goto Error;
        }
        new Span<byte>(tmpASCIIString, SignedSizeOfTag).CopyTo(ASCIIString.AsSpan(..SignedSizeOfTag));

        //ASCIIString[SignedSizeOfTag] = 0;
        cmsMLUsetASCII(n.Desc, cmsNoLanguage, cmsNoCountry, ASCIIString);
        _cmsFree(self.ContextID, ASCIIString);

        *nItems = 1;
        return new(n);

    Error:
        if (n.Ucr is not null) cmsFreeToneCurve(n.Ucr);
        if (n.Bg is not null) cmsFreeToneCurve(n.Bg);
        if (n.Desc is not null) cmsMLUfree(n.Desc);
        //_cmsFree(self.ContextID, n);
        return null;
    }

    private static bool Type_UcrBg_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Box<UcrBg> Value) return false;

        // First curve is Under color removal
        if (!_cmsWriteUInt32Number(io, Value.Value.Ucr.nEntries)) return false;
        fixed (ushort* t = &Value.Value.Ucr.Table16[0])
            if (!_cmsWriteUInt16Array(io, Value.Value.Ucr.nEntries, t)) return false;

        // Then black generation
        if (!_cmsWriteUInt32Number(io, Value.Value.Bg.nEntries)) return false;
        fixed (ushort* t = &Value.Value.Bg.Table16[0])
            if (!_cmsWriteUInt16Array(io, Value.Value.Bg.nEntries, t)) return false;

        // Now comes the text. The length is specified by the tag size
        var TextSize = cmsMLUgetASCII(Value.Value.Desc, cmsNoLanguage, cmsNoCountry, null);
        //Text = _cmsMalloc<byte>(self.ContextID, TextSize);
        var Text = _cmsCallocArray<byte>(self.ContextID, TextSize);
        if (cmsMLUgetASCII(Value.Value.Desc, cmsNoLanguage, cmsNoCountry, Text.AsSpan()[..(int)TextSize]) != TextSize) return false;

        var tmp = stackalloc char[(int)TextSize];
        Text.AsSpan()[..(int)TextSize].CopyTo(new(tmp, (int)TextSize));
        if (!io.Write(io, TextSize, tmp)) return false;
        _cmsFree(self.ContextID, Text);

        return true;
    }

    private static Box<UcrBg>? Type_UcrBg_Dup(TagTypeHandler self, object? Ptr, uint _)
    {
        if (Ptr is not Box<UcrBg> Src) return null;
        var NewUcrBg = new UcrBg();

        //if (NewUcrBg is null) return null;

        NewUcrBg.Bg = cmsDupToneCurve(Src.Value.Bg)!;
        if (NewUcrBg.Bg is null) goto Error;
        NewUcrBg.Ucr = cmsDupToneCurve(Src.Value.Ucr)!;
        if (NewUcrBg.Ucr is null) goto Error;
        var desc = cmsMLUdup(Src.Value.Desc);
        if (desc is null) goto Error;
        NewUcrBg.Desc = desc;

        return new(NewUcrBg);

    Error:
        Type_UcrBg_Free(self, new Box<UcrBg>(NewUcrBg));
        return null;
    }

    private static void Type_UcrBg_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is not Box<UcrBg> Src) return;

        if (Src.Value.Bg is not null) cmsFreeToneCurve(Src.Value.Bg);
        if (Src.Value.Ucr is not null) cmsFreeToneCurve(Src.Value.Ucr);
        if (Src.Value.Desc is not null) cmsMLUfree(Src.Value.Desc);

        //_cmsFree(self.ContextID, Src);
    }

    #endregion UcrBg

    #region CrdInfo

    private static bool ReadCountAndSting(TagTypeHandler self, IOHandler io, Mlu mlu, uint* SizeOfTag, ReadOnlySpan<byte> Section)
    {
        uint Count;
        var ps = "PS"u8;

        if (*SizeOfTag < _sizeof<uint>()) return false;

        if (!_cmsReadUInt32Number(io, &Count)) return false;

        if (Count > UInt32.MaxValue - _sizeof<uint>()) return false;
        if (*SizeOfTag < Count + _sizeof<uint>()) return false;

        //var Text = _cmsMalloc<byte>(self.ContextID, Count + 1);
        var Text = _cmsCallocArray<byte>(self.ContextID, Count);
        if (Text is null) return false;

        var tmp = stackalloc byte[(int)Count];
        if (io.Read(io, tmp, _sizeof<byte>(), Count) != Count)
        {
            _cmsFree(self.ContextID, Text);
            return false;
        }
        new Span<byte>(tmp, (int)Count).CopyTo(Text.AsSpan()[..(int)Count]);

        //Text[Count] = 0;

        cmsMLUsetASCII(mlu, ps, Section, Text[..(int)Count]);
        _cmsFree(self.ContextID, Text);

        *SizeOfTag -= Count + _sizeof<uint>();
        return true;
    }

    private static bool WriteCountAndSting(TagTypeHandler self, IOHandler io, Mlu mlu, ReadOnlySpan<byte> Section)
    {
        var ps = "PS"u8;

        var TextSize = cmsMLUgetASCII(mlu, ps, Section, null);
        //var Text = _cmsMalloc<byte>(self.ContextID, TextSize);
        var Text = _cmsCallocArray<byte>(self.ContextID, TextSize);
        if (Text is null) return false;

        if (!_cmsWriteUInt32Number(io, TextSize)) goto Error;

        if (cmsMLUgetASCII(mlu, ps, Section, Text.AsSpan()[..(int)TextSize]) is 0) goto Error;

        var tmp = stackalloc byte[(int)TextSize];
        Text.AsSpan()[..(int)TextSize].CopyTo(new(tmp, (int)TextSize));
        if (!io.Write(io, TextSize, tmp)) goto Error;
        _cmsFree(self.ContextID, Text);

        return true;

    Error:
        _cmsFree(self.ContextID, Text);
        return false;
    }

    private static Mlu? Type_CrdInfo_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        var nm = "nm"u8;
        var n0 = "#0"u8;
        var n1 = "#1"u8;
        var n2 = "#2"u8;
        var n3 = "#3"u8;

        *nItems = 0;
        var mlu = cmsMLUalloc(self.ContextID, 5);
        if (mlu is null) return null;

        if (!ReadCountAndSting(self, io, mlu, &SizeOfTag, nm)) goto Error;
        if (!ReadCountAndSting(self, io, mlu, &SizeOfTag, n0)) goto Error;
        if (!ReadCountAndSting(self, io, mlu, &SizeOfTag, n1)) goto Error;
        if (!ReadCountAndSting(self, io, mlu, &SizeOfTag, n2)) goto Error;
        if (!ReadCountAndSting(self, io, mlu, &SizeOfTag, n3)) goto Error;

        *nItems = 1;
        return mlu;

    Error:
        cmsMLUfree(mlu);
        return null;
    }

    private static bool Type_CrdInfo_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        var nm = "nm"u8;
        var n0 = "#0"u8;
        var n1 = "#1"u8;
        var n2 = "#2"u8;
        var n3 = "#3"u8;

        if (Ptr is not Mlu mlu) return false;

        if (!WriteCountAndSting(self, io, mlu, nm)) return false;
        if (!WriteCountAndSting(self, io, mlu, n0)) return false;
        if (!WriteCountAndSting(self, io, mlu, n1)) return false;
        if (!WriteCountAndSting(self, io, mlu, n2)) return false;
        if (!WriteCountAndSting(self, io, mlu, n3)) return false;

        return true;
    }

    private static Mlu? Type_CrdInfo_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        Ptr is Mlu mlu
            ? cmsMLUdup(mlu)
            : null;

    private static void Type_CrdInfo_Free(TagTypeHandler _, object? Ptr) =>
        cmsMLUfree(Ptr as Mlu);

    #endregion CrdInfo

    #region Screening

    private static BoxPtr<Screening>? Type_Screening_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        *nItems = 0;

        var sc = _cmsMallocZero<Screening>(self.ContextID);
        if (sc is null) return null;

        if (!_cmsReadUInt32Number(io, &sc->Flag)) goto Error;
        if (!_cmsReadUInt32Number(io, &sc->nChannels)) goto Error;

        if (sc->nChannels > cmsMAXCHANNELS - 1)
            sc->nChannels = cmsMAXCHANNELS - 1;

        for (var i = 0; i < sc->nChannels; i++)
        {
            if (!_cmsRead15Fixed16Number(io, out ((ScreeningChannel*)sc->Channels)[i].Frequency)) goto Error;
            if (!_cmsRead15Fixed16Number(io, out ((ScreeningChannel*)sc->Channels)[i].ScreenAngle)) goto Error;
            if (!_cmsReadUInt32Number(io, &((ScreeningChannel*)sc->Channels)[i].SpotShape)) goto Error;
        }

        *nItems = 1;
        return new(sc);

    Error:
        _cmsFree(self.ContextID, sc);

        return null;
    }

    private static bool Type_Screening_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        if (Ptr is not BoxPtr<Screening> sc) return false;

        if (!_cmsWriteUInt32Number(io, sc.Ptr->Flag)) return false;
        if (!_cmsWriteUInt32Number(io, sc.Ptr->nChannels)) return false;

        if (sc.Ptr->nChannels > cmsMAXCHANNELS - 1)
            sc.Ptr->nChannels = cmsMAXCHANNELS - 1;

        for (var i = 0; i < sc.Ptr->nChannels; i++)
        {
            if (!_cmsWrite15Fixed16Number(io, ((ScreeningChannel*)sc.Ptr->Channels)[i].Frequency)) return false;
            if (!_cmsWrite15Fixed16Number(io, ((ScreeningChannel*)sc.Ptr->Channels)[i].ScreenAngle)) return false;
            if (!_cmsWriteUInt32Number(io, ((ScreeningChannel*)sc.Ptr->Channels)[i].SpotShape)) return false;
        }

        return true;
    }

    private static BoxPtr<Screening>? Type_Screening_Dup(TagTypeHandler self, object? Ptr, uint _) =>
        Ptr is BoxPtr<Screening> s
            ? new(_cmsDupMem<Screening>(self.ContextID, s))
            : null;

    private static void Type_Screening_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<Screening> s)
            _cmsFree(self.ContextID, s);
    }

    #endregion Screening

    #region ViewingConditions

    private static BoxPtr<IccViewingConditions>? Type_ViewingConditions_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        *nItems = 0;

        var vc = _cmsMallocZero<IccViewingConditions>(self.ContextID);
        if (vc is null) return null;

        if (!_cmsReadXYZNumber(io, &vc->IlluminantXYZ)) goto Error;
        if (!_cmsReadXYZNumber(io, &vc->SurroundXYZ)) goto Error;
        if (!_cmsReadUInt32Number(io, (uint*)(void*)&vc->IlluminantType)) goto Error;

        *nItems = 1;
        return new(vc);

    Error:
        _cmsFree(self.ContextID, vc);

        return null;
    }

    private static bool Type_ViewingConditions_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        if (Ptr is not BoxPtr<IccViewingConditions> sc) return false;

        if (!_cmsWriteXYZNumber(io, &sc.Ptr->IlluminantXYZ)) return false;
        if (!_cmsWriteXYZNumber(io, &sc.Ptr->SurroundXYZ)) return false;
        if (!_cmsWriteUInt32Number(io, (uint)sc.Ptr->IlluminantType)) return false;

        return true;
    }

    private static BoxPtr<IccViewingConditions>? Type_ViewingConditions_Dup(TagTypeHandler self, object? Ptr, uint _) =>
        Ptr is BoxPtr<IccViewingConditions> sc
            ? new(_cmsDupMem<IccViewingConditions>(self.ContextID, sc))
            : null;

    private static void Type_ViewingConditions_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is BoxPtr<IccViewingConditions> sc)
            _cmsFree(self.ContextID, sc);
    }

    #endregion ViewingConditions

    #region MPE

    private static bool ReadMPEElem(TagTypeHandler self, IOHandler io, object? Cargo, uint _, uint SizeOfTag)
    {
        Signature ElementSig;
        uint nItems;

        if (Cargo is not Pipeline NewLUT)
            return false;

        var MPETypePluginChunk = _cmsGetContext(self.ContextID).MPEPlugin;

        // Take signature and channels for each element.
        if (!_cmsReadUInt32Number(io, (uint*)&ElementSig)) return false;

        // The reserved placeholder
        if (!_cmsReadUInt32Number(io, null)) return false;

        // Read diverse MPE types
        var TypeHandler = GetHandler(ElementSig, MPETypePluginChunk.TagTypes, supportedMPEtypes);
        if (TypeHandler is null)
        {
            var str = stackalloc byte[5];

            _cmsTagSignature2String(str, ElementSig);

            // An unknown element was found.
            cmsSignalError(self.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Unknown MPE type '{new string((sbyte*)str)}' found.");
            return false;
        }

        // If no read method, just ignore the element (valid for cmsSigBAcsElemType and cmsSigEAcsElemType)
        // Read the MPE. No size is given
        if (TypeHandler.ReadPtr is not null)
        {
            // This is a real element which should be read and processed
            var stage = (Stage?)TypeHandler.ReadPtr(self, io, &nItems, SizeOfTag);
            if (stage is null || !cmsPipelineInsertStage(NewLUT, StageLoc.AtEnd, stage))
                return false;
        }

        return true;
    }

    private static Pipeline? Type_MPE_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        ushort InputChans, OutputChans;
        uint ElementCount;
        Pipeline? NewLUT = null;

        *nItems = 0;

        // Get actual position as a basis for element offsets
        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        // Read channels and element count
        if (!_cmsReadUInt16Number(io, &InputChans)) return null;
        if (!_cmsReadUInt16Number(io, &OutputChans)) return null;

        if (InputChans is 0 or >= cmsMAXCHANNELS) return null;
        if (OutputChans is 0 or >= cmsMAXCHANNELS) return null;

        // Allocates an empty LUT
        NewLUT = cmsPipelineAlloc(self.ContextID, InputChans, OutputChans);
        if (NewLUT is null) return null;

        if (!_cmsReadUInt32Number(io, &ElementCount)) goto Error;
        if (!ReadPositionTable(self, io, ElementCount, BaseOffset, NewLUT, &ReadMPEElem)) goto Error;

        // Check channel count
        if (InputChans != NewLUT.InputChannels ||
            OutputChans != NewLUT.OutputChannels) goto Error;

        // Success
        *nItems = 1;
        return NewLUT;

    // Error
    Error:
        if (NewLUT is not null) cmsPipelineFree(NewLUT);

        return null;
    }

    private static bool Type_MPE_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Pipeline Lut)
            return false;

        var Elem = Lut.Elements;
        uint* ElementOffsets = null, ElementSizes = null;
        var MPETypePluginChunk = _cmsGetContext(self.ContextID).MPEPlugin;
        var str = stackalloc byte[5];

        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        var inputChan = cmsPipelineInputChannels(Lut);
        var outputChan = cmsPipelineOutputChannels(Lut);
        var ElemCount = cmsPipelineStageCount(Lut);

        ElementOffsets = _cmsCalloc<uint>(self.ContextID, ElemCount);
        if (ElementOffsets is null) goto Error;

        ElementSizes = _cmsCalloc<uint>(self.ContextID, ElemCount);
        if (ElementSizes is null) goto Error;

        // Write the head
        if (!_cmsWriteUInt16Number(io, (ushort)inputChan)) goto Error;
        if (!_cmsWriteUInt16Number(io, (ushort)outputChan)) goto Error;
        if (!_cmsWriteUInt32Number(io, (ushort)ElemCount)) goto Error;

        var DirectoryPos = io.Tell(io);

        // Write a fake directory to be filled later on
        for (var i = 0; i < ElemCount; i++)
        {
            if (!_cmsWriteUInt32Number(io, 0)) goto Error;  // Offset
            if (!_cmsWriteUInt32Number(io, 0)) goto Error;  // Size
        }

        // Write each single tag. Keep track of the size as well.
        for (var i = 0; i < ElemCount; i++)
        {
            ElementOffsets[i] = io.Tell(io) - BaseOffset;

            var ElementSig = Elem.Type;

            var TypeHandler = GetHandler(ElementSig, MPETypePluginChunk.TagTypes, supportedMPEtypes);
            if (TypeHandler is null)
            {
                _cmsTagSignature2String(str, ElementSig);

                // An unknown element was found.
                cmsSignalError(self.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Found unknown MPE type '{new string((sbyte*)str)}'");
                goto Error;
            }

            if (!_cmsWriteUInt32Number(io, ElementSig)) goto Error;
            if (!_cmsWriteUInt32Number(io, 0)) goto Error;
            var Before = io.Tell(io);
            if (!TypeHandler.WritePtr(self, io, Elem, 1)) goto Error;
            if (!_cmsWriteAlignment(io)) goto Error;

            ElementSizes[i] = io.Tell(io) - Before;

            Elem = Elem.Next;
        }

        // Write the directory
        var CurrentPos = io.Tell(io);

        if (!io.Seek(io, DirectoryPos)) goto Error;

        for (var i = 0; i < ElemCount; i++)
        {
            if (!_cmsWriteUInt32Number(io, ElementOffsets[i])) goto Error;
            if (!_cmsWriteUInt32Number(io, ElementSizes[i])) goto Error;
        }

        if (!io.Seek(io, CurrentPos)) goto Error;

        if (ElementOffsets is not null) _cmsFree(self.ContextID, ElementOffsets);
        if (ElementSizes is not null) _cmsFree(self.ContextID, ElementSizes);
        return true;

    Error:
        if (ElementOffsets is not null) _cmsFree(self.ContextID, ElementOffsets);
        if (ElementSizes is not null) _cmsFree(self.ContextID, ElementSizes);
        return false;
    }

    private static Pipeline? Type_MPE_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        cmsPipelineDup(Ptr as Pipeline);

    private static void Type_MPE_Free(TagTypeHandler _, object? Ptr) =>
        cmsPipelineFree(Ptr as Pipeline);

    #endregion MPE

    #region vcgt

    private const byte cmsVideoCardGammaTableType = 0;
    private const byte cmsVideoCardGammaFormulaType = 1;

    private struct VCGTGAMMA()
    {
        public double Gamma, Min, Max;
    }

    private static ToneCurve[]? Type_vcgt_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        uint TagType;

        *nItems = 0;

        // Read tag type
        if (!_cmsReadUInt32Number(io, &TagType)) return null;

        // Allocate space for the array
        //Curves = (ToneCurve**)_cmsCalloc(self.ContextID, 3, _sizeof<nint>());
        //if (Curves is null) return null;
        var Curves = Context.GetPool<ToneCurve>(self.ContextID).Rent(3);

        // There are two possible flavors
        switch (TagType)
        {
            // Gamma is stored as a table
            case cmsVideoCardGammaTableType:
                {
                    ushort nChannels, nElems, nBytes;

                    // Check channel count, which should be 3 (we don't support monochrome this time)
                    if (!_cmsReadUInt16Number(io, &nChannels)) goto Error;

                    if (nChannels is not 3)
                    {
                        cmsSignalError(self.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Unsupported number of channels for VCGT '{nChannels}'");
                        goto Error;
                    }

                    // Get Table element count and bytes per element
                    if (!_cmsReadUInt16Number(io, &nElems)) goto Error;
                    if (!_cmsReadUInt16Number(io, &nBytes)) goto Error;

                    // Adobe's quirk fixup. Fixing broken profiles...
                    if (nElems is 256 && nBytes is 1 && SizeOfTag is 1576)
                        nBytes = 2;

                    // Populate tone curves
                    for (var n = 0; n < 3; n++)
                    {
                        Curves[n] = cmsBuildTabulatedToneCurve16(self.ContextID, nElems, null);
                        if (Curves[n] is null) goto Error;

                        // Depending on byte depth...
                        switch (nBytes)
                        {
                            // One byte, 0..255
                            case 1:
                                for (var i = 0; i < nElems; i++)
                                {
                                    byte v;
                                    if (!_cmsReadUInt8Number(io, &v)) goto Error;
                                    Curves[n].Table16[i] = FROM_8_TO_16(v);
                                }
                                break;

                            // One word 0..65535
                            case 2:
                                fixed (ushort* t = &Curves[n].Table16[0])
                                    if (!_cmsReadUInt16Array(io, nElems, t)) goto Error;
                                break;

                            // Unsupported
                            default:
                                cmsSignalError(self.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Unsupported bit depth for VCGT '{nBytes * 8}'");
                                goto Error;
                        }
                    } // For all 3 channels
                }
                break;

            // In this case, gamma is stored as a formula
            case 1:
                {
                    var Colorant = stackalloc VCGTGAMMA[3];
                    var Params = stackalloc double[10];

                    memset(Params, 0, 10 * _sizeof<double>());

                    // Populate tone curves
                    for (var n = 0; n < 3; n++)
                    {
                        if (!_cmsRead15Fixed16Number(io, out Colorant[n].Gamma)) goto Error;
                        if (!_cmsRead15Fixed16Number(io, out Colorant[n].Min)) goto Error;
                        if (!_cmsRead15Fixed16Number(io, out Colorant[n].Max)) goto Error;

                        // Parametric curve type 5 is:
                        // Y = (aX + b)^Gamma + e | X >= d
                        // Y = cX + f             | X < d

                        // vcgt formula is:
                        // Y = (Max - Min) * (X ^ Gamma) + Min

                        // So, the translation is
                        // a = (Max - Min) ^ ( 1 / Gamma)
                        // e = Min
                        // b=c=d=f=0

                        Params[0] = Colorant[n].Gamma;
                        Params[1] = Math.Pow(Colorant[n].Max - Colorant[n].Min, 1.0 / Colorant[n].Gamma);
                        Params[5] = Colorant[n].Min;
                        // Params[2,3,4,6] are 0 and will stay 0

                        Curves[n] = cmsBuildParametricToneCurve(self.ContextID, 5, Params);
                        if (Curves[n] is null) goto Error;
                    }
                }
                break;

            // Unsupported
            default:
                cmsSignalError(self.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Unsupported tag type for VCGT '{TagType}'");
                goto Error;
        }

        *nItems = 1;
        return Curves;

    // Regret, free all resources
    Error:
        cmsFreeToneCurveTriple(Curves);
        _cmsFree(self.ContextID, Curves);
        return null;
    }

    private static bool Type_vcgt_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        if (Ptr is not ToneCurve[] Curves) return false;

        if (cmsGetToneCurveParametricType(Curves[0]) is 5 &&
            cmsGetToneCurveParametricType(Curves[1]) is 5 &&
            cmsGetToneCurveParametricType(Curves[2]) is 5)
        {
            if (!_cmsWriteUInt32Number(io, cmsVideoCardGammaFormulaType)) return false;

            // Save parameters
            for (var i = 0; i < 3; i++)
            {
                VCGTGAMMA v;

                v.Gamma = Curves[i].Segments[0].Params[0];
                v.Min = Curves[i].Segments[0].Params[5];
                v.Max = Math.Pow(Curves[i].Segments[0].Params[1], v.Gamma) + v.Min;

                if (!_cmsWrite15Fixed16Number(io, v.Gamma)) return false;
                if (!_cmsWrite15Fixed16Number(io, v.Min)) return false;
                if (!_cmsWrite15Fixed16Number(io, v.Max)) return false;
            }
        }
        else
        {
            // Always store as a table of 256 words
            if (!_cmsWriteUInt32Number(io, cmsVideoCardGammaTableType)) return false;
            if (!_cmsWriteUInt16Number(io, 3)) return false;
            if (!_cmsWriteUInt16Number(io, 256)) return false;
            if (!_cmsWriteUInt16Number(io, 2)) return false;

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 256; j++)
                {
                    var v = cmsEvalToneCurveFloat(Curves[i], (float)(j / 255.0));
                    var n = _cmsQuickSaturateWord(v * 65535.0);

                    if (!_cmsWriteUInt16Number(io, n)) return false;
                }
            }
        }

        return true;
    }

    private static ToneCurve[]? Type_vcgt_Dup(TagTypeHandler self, object? Ptr, uint _)
    {
        if (Ptr is not ToneCurve[] OldCurves)
            return null;

        //var NewCurves = _cmsCalloc2<ToneCurve>(self.ContextID, 3);
        //if (NewCurves is null) return null;

        var NewCurves = Context.GetPool<ToneCurve>(self.ContextID).Rent(3);

        NewCurves[0] = cmsDupToneCurve(OldCurves[0]);
        NewCurves[1] = cmsDupToneCurve(OldCurves[1]);
        NewCurves[2] = cmsDupToneCurve(OldCurves[2]);

        return NewCurves;
    }

    private static void Type_vcgt_Free(TagTypeHandler self, object? Ptr)
    {
        if (Ptr is ToneCurve[] curves)
        {
            cmsFreeToneCurveTriple(curves);
            _cmsFree(self.ContextID, curves);
        }
    }

    #endregion vcgt

    #region Dictionary

    private struct DICelem
    {
        public Context? ContextID;
        public uint[] Offsets;
        public uint[] Sizes;
    }

    private struct DICarray
    {
        public DICelem Name, Value, DisplayName, DisplayValue;
    }

    private static bool AllocElem(Context? ContextID, out DICelem e, uint Count)
    {
        var pool = _cmsGetContext(ContextID).GetBufferPool<uint>();

        e = new DICelem
        {
            ContextID = ContextID,
            Offsets = pool.Rent((int)Count),
            Sizes = pool.Rent((int)Count)
        };
        Array.Clear(e.Offsets);
        Array.Clear(e.Sizes);

        //e.Offsets = _cmsCalloc<uint>(ContextID, Count);
        //if (e->Offsets is null) return false;

        //e->Sizes = _cmsCalloc<uint>(ContextID, Count);
        //if (e->Sizes is null)
        //{
        //    _cmsFree(ContextID, e->Offsets);
        //    return false;
        //}

        return true;
    }

    private static void FreeElem(ref DICelem e)
    {
        if (e.Offsets is not null) _cmsFree(e.ContextID, e.Offsets);
        if (e.Sizes is not null) _cmsFree(e.ContextID, e.Sizes);
        e.Offsets = e.Sizes = null!;
    }

    private static void FreeArray(ref DICarray a)
    {
        if (a.Name.Offsets is not null || a.Name.Sizes is not null) FreeElem(ref a.Name);
        if (a.Value.Offsets is not null || a.Value.Sizes is not null) FreeElem(ref a.Value);
        if (a.DisplayName.Offsets is not null || a.DisplayName.Sizes is not null) FreeElem(ref a.DisplayName);
        if (a.DisplayValue.Offsets is not null || a.DisplayValue.Sizes is not null) FreeElem(ref a.DisplayValue);
    }

    private static bool AllocArray(Context? ContextID, out DICarray a, uint Count, uint Length)
    {
        a = new();
        // Empty values
        //memset(a, 0, _sizeof<DICarray>());

        // Depending on record size, create column arrays
        if (!AllocElem(ContextID, out a.Name, Count)) goto Error;
        if (!AllocElem(ContextID, out a.Value, Count)) goto Error;

        if (Length > 16 && !AllocElem(ContextID, out a.DisplayName, Count))
            goto Error;

        if (Length > 24 && !AllocElem(ContextID, out a.DisplayValue, Count))
            goto Error;

        return true;

    Error:
        FreeArray(ref a);
        return false;
    }

    private static bool ReadOneElem(IOHandler io, ref DICelem e, uint i, uint BaseOffset)
    {
        fixed (uint* ptr = &e.Offsets[i])
            if (!_cmsReadUInt32Number(io, ptr)) return false;
        fixed (uint* ptr = &e.Sizes[i])
            if (!_cmsReadUInt32Number(io, ptr)) return false;

        // An offset of zero has special meaning and shall be preserved
        if (e.Offsets[i] > 0)
            e.Offsets[i] += BaseOffset;

        return true;
    }

    private static bool ReadOffsetArray(IOHandler io, ref DICarray a, uint Count, uint Length, uint BaseOffset, int* SignedSizeOfTagPtr)
    {
        var SignedSizeOfTag = *SignedSizeOfTagPtr;

        // Read column arrays
        for (var i = 0u; i < Count; i++)
        {
            if (SignedSizeOfTag < 4 * _sizeof<uint>()) return false;
            SignedSizeOfTag -= 4 * _sizeof<uint>();

            if (!ReadOneElem(io, ref a.Name, i, BaseOffset)) return false;
            if (!ReadOneElem(io, ref a.Value, i, BaseOffset)) return false;

            if (Length > 16)
            {
                if (SignedSizeOfTag < 2 * _sizeof<uint>()) return false;
                SignedSizeOfTag -= 2 * _sizeof<uint>();

                if (!ReadOneElem(io, ref a.DisplayName, i, BaseOffset)) return false;
            }

            if (Length > 24)
            {
                if (SignedSizeOfTag < 2 * _sizeof<uint>()) return false;
                SignedSizeOfTag -= 2 * _sizeof<uint>();

                if (!ReadOneElem(io, ref a.DisplayValue, i, BaseOffset)) return false;
            }
        }

        *SignedSizeOfTagPtr = SignedSizeOfTag;
        return true;
    }

    private static bool WriteOneElem(IOHandler io, ref DICelem e, uint i)
    {
        if (!_cmsWriteUInt32Number(io, e.Offsets[i])) return false;
        if (!_cmsWriteUInt32Number(io, e.Sizes[i])) return false;

        return true;
    }

    private static bool WriteOffsetArray(IOHandler io, ref DICarray a, uint Count, uint Length)
    {
        for (var i = 0u; i < Count; i++)
        {
            if (!WriteOneElem(io, ref a.Name, i)) return false;
            if (!WriteOneElem(io, ref a.Value, i)) return false;

            if (Length > 16)
                if (!WriteOneElem(io, ref a.DisplayName, i)) return false;

            if (Length > 24)
                if (!WriteOneElem(io, ref a.DisplayValue, i)) return false;
        }

        return true;
    }

    private static bool ReadOneWChar(IOHandler io, ref DICelem e, uint i, out string? wcstr)
    {
        wcstr = null;

        // Special case for undefined strings (see ICC Votable
        // Proposal Submission, Dictionary Type and Metadata TAG Definition)
        if (e.Offsets[i] is 0)
            return true;

        if (!io.Seek(io, e.Offsets[i]))
            return false;

        var nChars = e.Sizes[i] / _sizeof<ushort>();

        //wcstr = _cmsMallocZero<char>(e.ContextID, nChars + 1);
        //if (*wcstr is null) return false;

        if (!_cmsReadWCharArray(io, nChars, out wcstr))
        {
            //_cmsFree(e->ContextID, *wcstr);
            return false;
        }

        // End of string marker
        //(*wcstr)[nChars] = '\0';
        return true;
    }

    private static bool WriteOneWChar(IOHandler io, ref DICelem e, uint i, in string? wcstr, uint BaseOffset)
    {
        var Before = io.Tell(io);

        e.Offsets[i] = Before - BaseOffset;

        if (wcstr is null)
        {
            e.Sizes[i] = 0;
            e.Offsets[i] = 0;
            return true;
        }

        //var n = mywcslen(wcstr);
        var n = (uint)wcstr.Length;
        if (!_cmsWriteWCharArray(io, n, wcstr)) return false;

        e.Sizes[i] = io.Tell(io) - Before;
        return true;
    }

    private static bool ReadOneMLUC(TagTypeHandler self, IOHandler io, ref DICelem e, uint i, out Mlu? mlu)
    {
        var nItems = 0u;

        // A way to get nul MLUCs
        if (e.Offsets[i] is 0 || e.Sizes[i] is 0)
        {
            mlu = null;
            return true;
        }

        if (!io.Seek(io, e.Offsets[i]))
        {
            mlu = null;
            return false;
        }

        mlu = Type_MLU_Read(self, io, &nItems, e.Sizes[i]);
        return mlu is not null;
    }

    private static bool WriteOneMLUC(TagTypeHandler self, IOHandler io, ref DICelem e, uint i, Mlu? mlu, uint BaseOffset)
    {
        // Special case for undefined strings (see ICC Votable
        // Proposal Submission, Dictionary Type and Metadata TAG Definition)
        if (mlu is null)
        {
            e.Sizes[i] = 0;
            e.Offsets[i] = 0;
            return true;
        }

        var Before = io.Tell(io);
        e.Offsets[i] = Before - BaseOffset;

        if (!Type_MLU_Write(self, io, mlu, 1)) return false;

        e.Sizes[i] = io.Tell(io) - Before;
        return true;
    }

    private static Dictionary? Type_Dictionary_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint SizeOfTag)
    {
        Dictionary? hDict = null;
        string? NameWCS = null, ValueWCS = null;
        Mlu? DisplayNameMLU = null, DisplayValueMLU = null;
        DICarray a = default;
        uint Count, Length;
        var SignedSizeOfTag = (int)SizeOfTag;

        *nItems = 0;
        //memset(&a, 0, _sizeof<DICarray>());

        // Get actual position as a basis for element offsets
        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        // Get name-value record count
        SignedSizeOfTag -= _sizeof<uint>();
        if (SignedSizeOfTag < 0) goto Error;
        if (!_cmsReadUInt32Number(io, &Count)) goto Error;

        // Get rec length
        SignedSizeOfTag -= _sizeof<uint>();
        if (SignedSizeOfTag < 0) goto Error;
        if (!_cmsReadUInt32Number(io, &Length)) goto Error;

        // Check for valid lengths
        if (Length is not 16 and not 24 and not 32)
        {
            cmsSignalError(self.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Unknown record length in dictionry '{Length}'");
            return null;
        }

        // Creates an empty dictionary
        hDict = cmsDictAlloc(self.ContextID);
        if (hDict is null) goto Error;

        // Depending on record size, create column arrays
        if (!AllocArray(self.ContextID, out a, Count, Length)) goto Error;

        // Read column arrays
        if (!ReadOffsetArray(io, ref a, Count, Length, BaseOffset, &SignedSizeOfTag)) goto Error;

        // Seek to each element and read it
        for (var i = 0u; i < Count; i++)
        {
            var rc = true;

            if (!ReadOneWChar(io, ref a.Name, i, out NameWCS)) goto Error;
            if (!ReadOneWChar(io, ref a.Value, i, out ValueWCS)) goto Error;

            if (Length > 16)
                if (!ReadOneMLUC(self, io, ref a.DisplayName, i, out DisplayNameMLU)) goto Error;

            if (Length > 24)
                if (!ReadOneMLUC(self, io, ref a.DisplayValue, i, out DisplayValueMLU)) goto Error;

            if (NameWCS is null || ValueWCS is null)
            {
                cmsSignalError(self.ContextID, cmsERROR_CORRUPTION_DETECTED, "Bad dictionary Name/Value");
                rc = false;
            }
            else
            {
                rc = cmsDictAddEntry(hDict, NameWCS, ValueWCS, DisplayNameMLU, DisplayValueMLU);
            }

            //if (NameWCS is not null) _cmsFree(self.ContextID, NameWCS);
            //if (ValueWCS is not null) _cmsFree(self.ContextID, ValueWCS);
            if (DisplayNameMLU is not null) cmsMLUfree(DisplayNameMLU);
            if (DisplayValueMLU is not null) cmsMLUfree(DisplayValueMLU);

            if (!rc) goto Error;
        }

        FreeArray(ref a);
        *nItems = 1;
        return hDict;

    Error:
        FreeArray(ref a);
        if (hDict is not null) cmsDictFree(hDict);
        return null;
    }

    private static bool Type_Dictionary_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Dictionary hDict)
            return false;

        Dictionary.Entry? p;

        if (hDict is null)
            return false;

        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        // Let's inspect the dictionary
        var Count = 0u; var AnyName = false; var AnyValue = false;
        for (p = cmsDictGetEntryList(hDict); p is not null; p = cmsDictNextEntry(p))
        {
            if (p.DisplayName is not null) AnyName = true;
            if (p.DisplayValue is not null) AnyValue = true;
            Count++;
        }

        var Length = 16u;
        if (AnyName) Length += 8;
        if (AnyValue) Length += 8;

        if (!_cmsWriteUInt32Number(io, Count)) return false;
        if (!_cmsWriteUInt32Number(io, Length)) return false;

        // Keep starting posiotion of offsets table
        var DirectoryPos = io.Tell(io);

        // Allocate offsets array
        if (!AllocArray(self.ContextID, out var a, Count, Length)) goto Error;

        // Write a fake directory to be filled later on
        if (!WriteOffsetArray(io, ref a, Count, Length)) goto Error;

        // Write each element. Keep track of the size as well.
        p = cmsDictGetEntryList(hDict);
        for (var i = 0u; i < Count; i++)
        {
            if (!WriteOneWChar(io, ref a.Name, i, p.Name, BaseOffset)) goto Error;
            if (!WriteOneWChar(io, ref a.Value, i, p.Value, BaseOffset)) goto Error;

            if (p.DisplayName is not null)
                if (!WriteOneMLUC(self, io, ref a.DisplayName, i, p.DisplayName, BaseOffset)) goto Error;

            if (p.DisplayValue is not null)
                if (!WriteOneMLUC(self, io, ref a.DisplayValue, i, p.DisplayValue, BaseOffset)) goto Error;

            p = cmsDictNextEntry(p);
        }

        // Write the directory
        var CurrentPos = io.Tell(io);
        if (!io.Seek(io, DirectoryPos)) goto Error;

        if (!WriteOffsetArray(io, ref a, Count, Length)) goto Error;

        if (!io.Seek(io, CurrentPos)) goto Error;

        FreeArray(ref a);
        return true;

    Error:
        FreeArray(ref a);
        return false;
    }

    private static Dictionary? Type_Dictionary_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        cmsDictDup(Ptr as Dictionary);

    private static void Type_Dictionary_Free(TagTypeHandler _, object? Ptr) =>
        cmsDictFree(Ptr as Dictionary);

    #endregion Dictionary

    #region Generic

    #region ToneCurve types

    private static ToneCurve? ToneCurve_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        cmsDupToneCurve(Ptr as ToneCurve);

    private static void ToneCurve_Free(TagTypeHandler _, object? Ptr) =>
        cmsFreeToneCurve(Ptr as ToneCurve);

    #endregion ToneCurve types

    #region NamedColorList types

    private static NamedColorList? NamedColor_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        cmsDupNamedColorList(Ptr as NamedColorList);

    private static void NamedColor_Free(TagTypeHandler _, object? Ptr) =>
        cmsFreeNamedColorList(Ptr as NamedColorList);

    #endregion NamedColorList types

    #region Sequence types

    private static Sequence? Sequence_Dup(TagTypeHandler _1, object? Ptr, uint _2) =>
        cmsDupProfileSequenceDescription(Ptr as Sequence);

    private static void Sequence_Free(TagTypeHandler _, object? Ptr) =>
        cmsFreeProfileSequenceDescription(Ptr as Sequence);

    #endregion Sequence types

    #endregion Generic

    #endregion TypeHandlers

    #region MPE Handlers

    #region Generic

    private static Stage? GenericMPEdup(TagTypeHandler _1, object? Ptr, uint _2) =>
        Ptr is Stage stage
            ? cmsStageDup(stage)
            : null;

    private static void GenericMPEfree(TagTypeHandler _1, object? Ptr)
    {
        if (Ptr is Stage stage)
            cmsStageFree(stage);
    }

    #endregion Generic

    #region MPEcurve

    private static ToneCurve? ReadSegmentedCurve(TagTypeHandler self, IOHandler io)
    {
        var str = stackalloc byte[5];
        var ParamsByType = stackalloc uint[] { 4, 5, 5 };
        Signature ElementSig;
        ushort nSegments;
        float PrevBreak = MINUS_INF;

        // Take signature and channels for each element.
        if (!_cmsReadUInt32Number(io, (uint*)&ElementSig)) return null;

        // That should be a segmented curve
        if ((uint)ElementSig is not cmsSigSegmentedCurve) return null;

        if (!_cmsReadUInt32Number(io, null)) return null;
        if (!_cmsReadUInt16Number(io, &nSegments)) return null;
        if (!_cmsReadUInt16Number(io, null)) return null;

        if (nSegments < 1) return null;
        var Segments = _cmsCallocArray<CurveSegment>(self.ContextID, nSegments);
        if (Segments is null) return null;

        // Read breakpoints
        for (var i = 0; i < nSegments - 1; i++)
        {
            Segments[i].x0 = PrevBreak;
            fixed (float* ptr = &Segments[i].x1)
                if (!_cmsReadFloat32Number(io, ptr)) goto Error;
            PrevBreak = Segments[i].x1;
        }

        Segments[nSegments - 1].x0 = PrevBreak;
        Segments[nSegments - 1].x1 = PLUS_INF;

        // Read segments
        for (var i = 0; i < nSegments; i++)
        {
            if (!_cmsReadUInt32Number(io, (uint*)&ElementSig)) goto Error;
            if (!_cmsReadUInt32Number(io, null)) goto Error;

            switch ((uint)ElementSig)
            {
                case cmsSigFormulaCurveSeg:
                    {
                        ushort Type;

                        if (!_cmsReadUInt16Number(io, &Type)) goto Error;
                        if (!_cmsReadUInt16Number(io, null)) goto Error;

                        Segments[i].Type = Type + 6;
                        if (Type > 2) goto Error;

                        Segments[i].Params = Context.GetPool<double>(self.ContextID).Rent(10);

                        for (var j = 0; j < ParamsByType[Type]; j++)
                        {
                            float f;
                            if (!_cmsReadFloat32Number(io, &f)) goto Error;
                            Segments[i].Params[j] = f;
                        }
                    }
                    break;

                case cmsSigSampledCurveSeg:
                    {
                        uint Count;

                        if (!_cmsReadUInt32Number(io, &Count)) goto Error;

                        // This first point is implicit in the last stage, we allocate an extra note to be populated later on
                        Count++;
                        Segments[i].nGridPoints = Count;
                        //Segments[i].SampledPoints = _cmsCalloc<float>(self.ContextID, Count);
                        //if (Segments[i].SampledPoints is null) goto Error;
                        Segments[i].SampledPoints = Context.GetPool<float>(self.ContextID).Rent((int)Count);

                        Segments[i].SampledPoints[0] = 0;
                        for (var j = 1; j < Count; j++)
                            fixed (float* sp = &Segments[i].SampledPoints[j]) if (!_cmsReadFloat32Number(io, sp)) goto Error;
                    }
                    break;

                default:
                    {
                        _cmsTagSignature2String(str, ElementSig);
                        cmsSignalError(self.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Unknown curve element type '{new string((sbyte*)str)}' found.");
                    }
                    goto Error;
            }
        }

        var Curve = cmsBuildSegmentedToneCurve(self.ContextID, nSegments, Segments);

        for (var i = 0; i < nSegments; i++)
            if (Segments[i].SampledPoints is not null) _cmsFree(self.ContextID, Segments[i].SampledPoints);
        _cmsFree(self.ContextID, Segments);

        // Explore for missing implicit points
        for (var i = 0; i < nSegments; i++)
        {
            // If sampled curve, fix it
            if (Curve.Segments[i].Type is 0)
                Curve.Segments[i].SampledPoints[0] = cmsEvalToneCurveFloat(Curve, Curve.Segments[i].x0);
        }

        return Curve;

    Error:
        for (var i = 0; i < nSegments; i++)
            if (Segments[i].SampledPoints is not null) _cmsFree(self.ContextID, Segments[i].SampledPoints);
        _cmsFree(self.ContextID, Segments);
        return null;
    }

    private static bool ReadMPECurve(TagTypeHandler self, IOHandler io, object? Cargo, uint n, uint _)
    {
        if (Cargo is not ToneCurve[] GammaTables)
            return false;

        GammaTables[n] = ReadSegmentedCurve(self, io)!;
        return GammaTables[n] is not null;
    }

    private static Stage? Type_MPEcurve_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        ushort InputChans, OutputChans;

        *nItems = 0;

        // Get actual position as a basis for element offsets
        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        if (!_cmsReadUInt16Number(io, &InputChans)) return null;
        if (!_cmsReadUInt16Number(io, &OutputChans)) return null;

        if (InputChans != OutputChans) return null;

        //var GammaTables = (ToneCurve**)_cmsCalloc(self.ContextID, InputChans, _sizeof<nint>());
        //if (GammaTables is null) return null;
        var pool = Context.GetPool<ToneCurve>(self.ContextID);
        var GammaTables = pool.Rent(InputChans);

        var mpe = ReadPositionTable(self, io, InputChans, BaseOffset, GammaTables, &ReadMPECurve)
            ? cmsStageAllocToneCurves(self.ContextID, InputChans, GammaTables)
            : null;

        for (var i = 0; i < InputChans; i++)
            if (GammaTables[i] is not null) cmsFreeToneCurve(GammaTables[i]);

        _cmsFree(self.ContextID, GammaTables);
        *nItems = mpe is not null ? 1u : 0;
        return mpe;
    }

    private static bool WriteSegmentedCurve(IOHandler io, ToneCurve g)
    {
        var ParamsByType = stackalloc uint[] { 4, 5, 5 };

        var Segments = g.Segments;
        var nSegments = g.nSegments;

        if (!_cmsWriteUInt32Number(io, cmsSigSegmentedCurve)) return false;
        if (!_cmsWriteUInt32Number(io, 0)) return false;
        if (!_cmsWriteUInt16Number(io, (ushort)nSegments)) return false;
        if (!_cmsWriteUInt16Number(io, 0)) return false;

        // Write the break points
        for (var i = 0; i < nSegments - 1; i++)
            if (!_cmsWriteFloat32Number(io, Segments[i].x1)) return false;

        // Write the segments
        for (var i = 0; i < nSegments; i++)
        {
            var ActualSeg = Segments[i];

            if (ActualSeg.Type is 0)
            {
                // This is a sampled curve. First point is implicit in the ICC format, but not in our representation
                if (!_cmsWriteUInt32Number(io, cmsSigSampledCurveSeg)) return false;
                if (!_cmsWriteUInt32Number(io, 0)) return false;
                if (!_cmsWriteUInt32Number(io, ActualSeg.nGridPoints - 1)) return false;

                for (var j = 1; j < g.Segments[i].nGridPoints; j++)
                    if (!_cmsWriteFloat32Number(io, ActualSeg.SampledPoints[j])) return false;
            }
            else
            {
                // This is a formula-based
                if (!_cmsWriteUInt32Number(io, cmsSigFormulaCurveSeg)) return false;
                if (!_cmsWriteUInt32Number(io, 0)) return false;

                // We only allow 1, 2, and 3 as types
                var Type = ActualSeg.Type - 6;
                if (Type is > 2 or < 0) return false;

                if (!_cmsWriteUInt16Number(io, (ushort)Type)) return false;
                if (!_cmsWriteUInt16Number(io, 0)) return false;

                for (var j = 0; j < ParamsByType[Type]; j++)
                    if (!_cmsWriteFloat32Number(io, (float)ActualSeg.Params[j])) return false;
            }

            // It seems there is no need to align. Code is here, and for safety commented out
            // if (!_cmsWriteAlignment(io)) goto Error;
        }

        return true;
    }

    private static bool WriteMPECurve(TagTypeHandler _1, IOHandler io, object? Cargo, uint n, uint _2) =>
        Cargo is StageToneCurvesData Curves && WriteSegmentedCurve(io, Curves.TheCurves[n]);

    private static bool Type_MPEcurve_Write(TagTypeHandler self, IOHandler io, object? Ptr, uint _)
    {
        if (Ptr is not Stage mpe ||
            mpe.Data is not StageToneCurvesData Curves) return false;

        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        // Write the header. Since those are curves, input and output channels are the same
        if (!_cmsWriteUInt16Number(io, (ushort)mpe.InputChannels)) return false;
        if (!_cmsWriteUInt16Number(io, (ushort)mpe.InputChannels)) return false;

        if (!WritePositionTable(self, io, 0, mpe.InputChannels, BaseOffset, Curves, WriteMPECurve)) return false;

        return true;
    }

    #endregion MPEcurve

    #region MPEmatrix

    private static Stage? Type_MPEmatrix_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        ushort InputChans, OutputChans;

        *nItems = 0;

        if (!_cmsReadUInt16Number(io, &InputChans)) return null;
        if (!_cmsReadUInt16Number(io, &OutputChans)) return null;

        if (InputChans >= cmsMAXCHANNELS) return null;
        if (OutputChans >= cmsMAXCHANNELS) return null;

        var nElems = (uint)InputChans * OutputChans;

        //var Matrix = _cmsCalloc<double>(self.ContextID, nElems);
        //if (Matrix is null) return null;

        //var Offsets = _cmsCalloc<double>(self.ContextID, OutputChans);
        //if (Offsets is null)
        //{
        //    _cmsFree(self.ContextID, Matrix);
        //    return null;
        //}
        Span<double> Matrix = stackalloc double[(int)nElems];
        Span<double> Offsets = stackalloc double[OutputChans];

        for (var i = 0; i < nElems; i++)
        {
            float v;

            if (!_cmsReadFloat32Number(io, &v))
            {
                //_cmsFree(self.ContextID, Matrix);
                //_cmsFree(self.ContextID, Offsets);
                return null;
            }
            Matrix[i] = v;
        }

        for (var i = 0; i < OutputChans; i++)
        {
            float v;

            if (!_cmsReadFloat32Number(io, &v))
            {
                //_cmsFree(self.ContextID, Matrix);
                //_cmsFree(self.ContextID, Offsets);
                return null;
            }
            Offsets[i] = v;
        }

        var mpe = cmsStageAllocMatrix(self.ContextID, OutputChans, InputChans, Matrix, Offsets);
        //_cmsFree(self.ContextID, Matrix);
        //_cmsFree(self.ContextID, Offsets);
        *nItems = mpe is not null ? 1u : 0;
        return mpe;
    }

    private static bool Type_MPEmatrix_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        if (Ptr is not Stage mpe ||
            mpe.Data is not StageMatrixData Matrix) return false;

        if (!_cmsWriteUInt16Number(io, (ushort)mpe.InputChannels)) return false;
        if (!_cmsWriteUInt16Number(io, (ushort)mpe.OutputChannels)) return false;

        var nElems = mpe.InputChannels * mpe.OutputChannels;

        for (var i = 0; i < nElems; i++)
            if (!_cmsWriteFloat32Number(io, (float)Matrix.Double[i])) return false;

        for (var i = 0; i < mpe.OutputChannels; i++)
        {
            if (!_cmsWriteFloat32Number(io, Matrix.Offset is null ? 0 : (float)Matrix.Offset[i])) return false;
        }

        return true;
    }

    #endregion MPEmatrix

    #region MPEclut

    private static Stage? Type_MPEclut_Read(TagTypeHandler self, IOHandler io, uint* nItems, uint _)
    {
        ushort InputChans, OutputChans;
        var Dimensions8 = stackalloc byte[16];
        Span<uint> GridPoints = stackalloc uint[MAX_INPUT_DIMENSIONS];
        Stage? mpe = null;

        *nItems = 0;

        // Get actual position as a basis for element offsets
        var BaseOffset = io.Tell(io) - _sizeof<TagBase>();

        if (!_cmsReadUInt16Number(io, &InputChans)) return null;
        if (!_cmsReadUInt16Number(io, &OutputChans)) return null;

        if (InputChans is 0) goto Error;
        if (OutputChans is 0) goto Error;

        if (io.Read(io, Dimensions8, _sizeof<byte>(), 16) is not 16) goto Error;

        // Copy MAX_INPUT_DIMENSIONS at most. Expact to uint
        var nMaxGrids = Math.Min(InputChans, MAX_INPUT_DIMENSIONS);

        for (var i = 0; i < nMaxGrids; i++)
        {
            if (Dimensions8[i] is 1) goto Error;    // Impossible value, 0 for no CLUT and at least 2 otherwise
            GridPoints[i] = Dimensions8[i];
        }

        // Allocate the true CLUT
        mpe = cmsStageAllocCLutFloatGranular(self.ContextID, GridPoints, InputChans, OutputChans, null);
        if (mpe is null) goto Error;

        // Read and sanitize the data
        var clut = (StageCLutData<float>)mpe.Data;
        for (var i = 0; i < clut.nEntries; i++)
            fixed (float* t = &clut.TFloat[i]) if (!_cmsReadFloat32Number(io, t)) goto Error;

        *nItems = 1;
        return mpe;

    Error:
        if (mpe is not null) cmsStageFree(mpe);
        return null;
    }

    private static bool Type_MPEclut_Write(TagTypeHandler _1, IOHandler io, object? Ptr, uint _2)
    {
        var Dimensions8 = stackalloc byte[16];

        if (Ptr is not Stage mpe || mpe.Data is not StageCLutData<float> clut)
            return false;

        // Check for maximum number of channels supported by lcms
        if (mpe.InputChannels > MAX_INPUT_DIMENSIONS) return false;

        //// Only floats are supported in MPE
        //if (!clut.HasFloatValues) return false;
        //
        // We already check for HasFloatValues by making sure we are working with
        // a StageCLutData<float> from the beginning.

        if (!_cmsWriteUInt16Number(io, (ushort)mpe.InputChannels)) return false;
        if (!_cmsWriteUInt16Number(io, (ushort)mpe.OutputChannels)) return false;

        memset(Dimensions8, 0, _sizeof<byte>() * 16);

        for (var i = 0; i < mpe.InputChannels; i++)
            Dimensions8[i] = (byte)clut.Params.nSamples[i];

        if (!io.Write(io, 16, Dimensions8)) return false;

        for (var i = 0; i < clut.nEntries; i++)
            if (!_cmsWriteFloat32Number(io, clut.TFloat[i])) return false;

        return true;
    }

    #endregion MPEclut

    #endregion MPE Handlers

    #region Plugin

    internal static void DupTagTypeList(Context ctx, in Context src, Chunks loc)
    {
        TagTypePluginChunkType head = loc is Chunks.MPEPlugin
            ? src.MPEPlugin
            : src.TagTypePlugin;
        TagTypeLinkedList? Anterior = null, entry;
        TagTypePluginChunkType newHead = new();

        _cmsAssert(ctx);
        _cmsAssert(head);

        // Walk the list copying all nodes
        for (entry = head.TagTypes;
             entry is not null;
             entry = entry.Next)
        {
            //var newEntry = _cmsSubAllocDup<TagTypeLinkedList>(ctx.MemPool, entry);
            var newEntry = (TagTypeLinkedList)entry.Clone();

            if (newEntry is null)
                return;

            // We want to keep the linked list order, so this is a little bit tricky
            newEntry.Next = null;
            if (Anterior is not null)
                Anterior.Next = newEntry;

            Anterior = newEntry;

            newHead.TagTypes ??= newEntry;
        }
        if (loc is Chunks.MPEPlugin)
            ctx.MPEPlugin = newHead;
        else
            ctx.TagTypePlugin = newHead;
    }

    internal static void _cmsAllocTagTypePluginChunk(Context ctx, in Context? src)
    {
        _cmsAssert(ctx);

        var from = src is not null
            ? src.TagTypePlugin
            : TagTypePluginChunk;

        _cmsAssert(from);

        ctx.TagTypePlugin = (TagTypePluginChunkType)from.Dup(ctx);

        //fixed (TagTypePluginChunkType* @default = &TagTypePluginChunk)
        //    AllocPluginChunk(ctx, src, (Context c, in Context s) => DupTagTypeList(c, s, Chunks.TagTypePlugin), Chunks.TagTypePlugin, @default);
    }

    internal static void _cmsAllocMPETypePluginChunk(Context ctx, in Context? src)
    {
        _cmsAssert(ctx);

        var from = src is not null
            ? src.MPEPlugin
            : MPETypePluginChunk;

        _cmsAssert(from);

        ctx.MPEPlugin = (TagTypePluginChunkType)from.Dup(ctx);

        //fixed (TagTypePluginChunkType* @default = &MPETypePluginChunk)
        //    AllocPluginChunk(ctx, src, (Context c, in Context s) => DupTagTypeList(c, s, Chunks.MPEPlugin), Chunks.MPEPlugin, @default);
    }

    internal static bool _cmsRegisterTagTypePlugin(Context? id, PluginBase? Data) =>
        RegisterTypesPlugin(id, Data, Chunks.TagTypePlugin);

    internal static bool _cmsRegisterMultiProcessElementPlugin(Context? id, PluginBase? Data) =>
        RegisterTypesPlugin(id, Data, Chunks.MPEPlugin);

    internal static void DupTagList(Context ctx, in Context? src)
    {
        TagPluginChunkType head = src.TagPlugin;
        TagLinkedList? Anterior = null, entry;
        TagPluginChunkType newHead = new();

        _cmsAssert(ctx);
        _cmsAssert(head);

        // Walk the list copying all nodes
        for (entry = head.Tag;
             entry is not null;
             entry = entry.Next)
        {
            //var newEntry = _cmsSubAllocDup<TagLinkedList>(ctx.MemPool, entry);

            //if (newEntry is null)
            //    return;

            var newEntry = (TagLinkedList)entry.Clone();

            // We want to keep the linked list order, so this is a little bit tricky
            newEntry.Next = null;
            if (Anterior is not null)
                Anterior.Next = newEntry;

            Anterior = newEntry;

            newHead.Tag ??= newEntry;
        }

        ctx.TagPlugin = newHead;
    }

    internal static void _cmsAllocTagPluginChunk(Context ctx, in Context? src)
    {
        _cmsAssert(ctx);

        var from = src is not null
            ? src.TagPlugin
            : TagPluginChunk;

        _cmsAssert(from);

        ctx.TagPlugin = (TagPluginChunkType)from.Dup(ctx);

        //fixed (TagPluginChunkType* @default = &TagPluginChunk)
        //    AllocPluginChunk(ctx, src, DupTagList, Chunks.TagPlugin, @default);
    }

    internal static bool _cmsRegisterTagPlugin(Context? id, PluginBase? Data)
    {
        var Plugin = (PluginTag?)Data;
        var TagPluginChunk = _cmsGetContext(id).TagPlugin;

        if (Data is null)
        {
            TagPluginChunk.Tag = null;
            return true;
        }

        //var pt = _cmsPluginMalloc<TagLinkedList>(id);
        //if (pt == null) return false;

        //pt->Signature = Plugin!.Signature;
        //pt->Descriptor = Plugin.Descriptor;
        //pt->Next = TagPluginChunk.Tag;

        var pt = new TagLinkedList(Plugin!.Signature, Plugin.Descriptor, TagPluginChunk.Tag);

        TagPluginChunk.Tag = pt;

        return true;
    }

    #endregion Plugin

    internal static TagTypeHandler? _cmsGetTagTypeHandler(Context? ContextID, Signature sig)
    {
        var ctx = _cmsGetContext(ContextID).TagTypePlugin;

        return GetHandler(sig, ctx.TagTypes, supportedTagTypes);
    }

    internal static TagDescriptor? _cmsGetTagDescriptor(Context? ContextID, Signature sig)
    {
        var TagPluginChunk = _cmsGetContext(ContextID).TagPlugin;

        for (var pt = TagPluginChunk.Tag; pt is not null; pt = pt.Next)
            if (sig == pt.Signature) return pt.Descriptor;

        for (var pt = supportedTags; pt is not null; pt = pt.Next)
            if (sig == pt.Signature) return pt.Descriptor;

        return null;
    }
}
