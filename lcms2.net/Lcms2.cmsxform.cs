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
    private const double DEFAULT_OBSERVER_ADAPTATION_STATE = 1.0;

    private static readonly AdaptationStateChunkType AdaptationStateChunk = new() { AdaptationState = DEFAULT_OBSERVER_ADAPTATION_STATE };

    private static readonly AdaptationStateChunkType globalAdaptationStateChunk = new() { AdaptationState = DEFAULT_OBSERVER_ADAPTATION_STATE };

    private static readonly AlarmCodesChunkType AlarmCodesChunk = new();

    private static readonly AlarmCodesChunkType globalAlarmCodesChunk = new();

    internal static readonly TransformPluginChunkType TransformPluginChunk = new();

    internal static readonly TransformPluginChunkType globalTransformPluginChunk = new();

    internal static void _cmsAllocAdaptationStateChunk(Context ctx, in Context? src)
    {
        _cmsAssert(ctx);

        var from = src is not null
            ? src.AdaptationState
            : AdaptationStateChunk;

        _cmsAssert(from);

        ctx.AdaptationState = (AdaptationStateChunkType)from.Dup(ctx);

        //fixed (AdaptationStateChunkType* @default = &AdaptationStateChunk)
        //    AllocPluginChunk(ctx, src, Chunks.AdaptationStateContext, @default);
    }

    public static double cmsSetAdaptationStateTHR(Context? context, double d)
    {
        var ptr = _cmsGetContext(context).AdaptationState;

        // Get previous value for return
        var prev = ptr.AdaptationState;

        // Set the value if d is positive or zero
        if (d >= 0)
            ptr.AdaptationState = d;

        // Always return previous value
        return prev;
    }

    public static double cmsSetAdaptationState(double d) =>
        cmsSetAdaptationStateTHR(null, d);

    public static void cmsSetAlarmCodesTHR(Context? context, in ushort* AlarmCodesP)
    {
        var contextAlarmCodes = _cmsGetContext(context).AlarmCodes;
        _cmsAssert(contextAlarmCodes); // Can't happen
        for (var i = 0; i < cmsMAXCHANNELS; i++)
            contextAlarmCodes.AlarmCodes[i] = AlarmCodesP[i];
    }

    public static void cmsGetAlarmCodesTHR(Context? context, ushort* AlarmCodesP)
    {
        var contextAlarmCodes = _cmsGetContext(context).AlarmCodes;
        _cmsAssert(contextAlarmCodes); // Can't happen
        for (var i = 0; i < cmsMAXCHANNELS; i++)
            AlarmCodesP[i] = contextAlarmCodes.AlarmCodes[i];
    }

    public static void cmsSetAlarmCodes(in ushort* AlarmCodes) =>
        cmsSetAlarmCodesTHR(null, AlarmCodes);

    public static void cmsGetAlarmCodes(ushort* AlarmCodes) =>
        cmsGetAlarmCodesTHR(null, AlarmCodes);

    internal static void _cmsAllocAlarmCodesChunk(Context ctx, in Context? src)
    {
        _cmsAssert(ctx);

        var from = src is not null
            ? src.AlarmCodes
            : AlarmCodesChunk;

        _cmsAssert(from);

        ctx.AlarmCodes = (AlarmCodesChunkType)from.Dup(ctx);

        //fixed (AlarmCodesChunkType* @default = &AlarmCodesChunk)
        //    AllocPluginChunk(ctx, src, Chunks.AlarmCodesContext, @default);
    }

    public static void cmsDeleteTransform(HTRANSFORM hTransform)
    {
        var p = (Transform*)hTransform;
        _cmsAssert(p);

        if (p->GamutCheck is not null)
            cmsPipelineFree(p->GamutCheck);

        if (p->Lut is not null)
            cmsPipelineFree(p->Lut);

        if (p->InputColorant is not null)
            cmsFreeNamedColorList(p->InputColorant);

        if (p->OutputColorant is not null)
            cmsFreeNamedColorList(p->OutputColorant);

        if (p->Sequence is not null)
            cmsFreeProfileSequenceDescription(p->Sequence);

        if (p->UserData is not null)
            p->FreeUserData!(p->ContextID, p->UserData);

        _cmsFree(p->ContextID, p);
    }

    // PixelSize already defined in Lcms2.cmspack.cs

    public static void cmsDoTransform(
        HTRANSFORM Transform,
        in void* InputBuffer,
        void* OutputBuffer,
        uint Size)
    {
        var p = (Transform*)Transform;
        Stride stride;

        stride.BytesPerLineIn = 0;  // Not used
        stride.BytesPerLineOut = 0;
        stride.BytesPerPlaneIn = Size * PixelSize(p->InputFormat);
        stride.BytesPerPlaneOut = Size * PixelSize(p->OutputFormat);

        p->xform(p, InputBuffer, OutputBuffer, Size, 1, &stride);
    }

    public static void cmsDoTransformStride(
        HTRANSFORM Transform,
        in void* InputBuffer,
        void* OutputBuffer,
        uint Size,
        uint Stride)
    {
        var p = (Transform*)Transform;
        Stride stride;

        stride.BytesPerLineIn = 0;  // Not used
        stride.BytesPerLineOut = 0;
        stride.BytesPerPlaneIn = Stride;
        stride.BytesPerPlaneOut = Stride;

        p->xform(p, InputBuffer, OutputBuffer, Size, 1, &stride);
    }

    public static void cmsDoTransformLineStride(
        HTRANSFORM Transform,
        in void* InputBuffer,
        void* OutputBuffer,
        uint PixelsPerLine,
        uint LineCount,
        uint BytesPerLineIn,
        uint BytesPerLineOut,
        uint BytesPerPlaneIn,
        uint BytesPerPlaneOut)
    {
        var p = (Transform*)Transform;
        Stride stride;

        stride.BytesPerLineIn = BytesPerLineIn;
        stride.BytesPerLineOut = BytesPerLineOut;
        stride.BytesPerPlaneIn = BytesPerPlaneIn;
        stride.BytesPerPlaneOut = BytesPerPlaneOut;

        p->xform(p, InputBuffer, OutputBuffer, PixelsPerLine, LineCount, &stride);
    }

    private static void FloatXFORM(
        Transform* p,
        in void* @in,
        void* @out,
        uint PixelsPerLine,
        uint LineCount,
        in Stride* Stride)
    {
        var fIn = stackalloc float[cmsMAXCHANNELS];
        var fOut = stackalloc float[cmsMAXCHANNELS];
        float OutOfGamut;

        _cmsHandleExtraChannels(p, @in, @out, PixelsPerLine, LineCount, Stride);

        var strideIn = 0u;
        var strideOut = 0u;
        memset(fIn, 0, sizeof(float) * cmsMAXCHANNELS);
        memset(fOut, 0, sizeof(float) * cmsMAXCHANNELS);

        for (var i = 0; i < LineCount; i++)
        {
            var accum = (byte*)@in + strideIn;
            var output = (byte*)@out + strideOut;

            for (var j = 0; j < PixelsPerLine; j++)
            {
                accum = p->FromInputFloat(p, fIn, accum, Stride->BytesPerPlaneIn);

                // Any gamut check to do?
                if (p->GamutCheck is not null)
                {
                    // Evaluate gamut marker.
                    cmsPipelineEvalFloat(fIn, &OutOfGamut, p->GamutCheck);

                    // Is current color out of gamut?
                    if (OutOfGamut > 0.0)
                    {
                        // Certainly, out of gamut
                        for (var c = 0; c < cmsMAXCHANNELS; c++)
                            fOut[c] = -1.0f;
                    }
                    else
                    {
                        // No, proceed normally
                        cmsPipelineEvalFloat(fIn, fOut, p->Lut);
                    }
                }
                else
                {
                    // No gamut check at all
                    cmsPipelineEvalFloat(fIn, fOut, p->Lut);
                }

                output = p->ToOutputFloat(p, fOut, output, Stride->BytesPerPlaneOut);
            }

            strideIn += Stride->BytesPerLineIn;
            strideOut += Stride->BytesPerLineOut;
        }
    }

    private static void NullFloatXFORM(
        Transform* p,
        in void* @in,
        void* @out,
        uint PixelsPerLine,
        uint LineCount,
        in Stride* Stride)
    {
        var fIn = stackalloc float[cmsMAXCHANNELS];

        _cmsHandleExtraChannels(p, @in, @out, PixelsPerLine, LineCount, Stride);

        var strideIn = 0u;
        var strideOut = 0u;
        memset(fIn, 0, sizeof(float) * cmsMAXCHANNELS);

        for (var i = 0; i < LineCount; i++)
        {
            var accum = (byte*)@in + strideIn;
            var output = (byte*)@out + strideOut;

            for (var j = 0; j < PixelsPerLine; j++)
            {
                accum = p->FromInputFloat(p, fIn, accum, Stride->BytesPerPlaneIn);
                output = p->ToOutputFloat(p, fIn, output, Stride->BytesPerPlaneOut);
            }

            strideIn += Stride->BytesPerLineIn;
            strideOut += Stride->BytesPerLineOut;
        }
    }

    private static void NullXFORM(
        Transform* p,
        in void* @in,
        void* @out,
        uint PixelsPerLine,
        uint LineCount,
        in Stride* Stride)
    {
        var wIn = stackalloc ushort[cmsMAXCHANNELS];

        _cmsHandleExtraChannels(p, @in, @out, PixelsPerLine, LineCount, Stride);

        var strideIn = 0u;
        var strideOut = 0u;
        memset(wIn, 0, sizeof(ushort) * cmsMAXCHANNELS);

        for (var i = 0; i < LineCount; i++)
        {
            var accum = (byte*)@in + strideIn;
            var output = (byte*)@out + strideOut;

            for (var j = 0; j < PixelsPerLine; j++)
            {
                accum = p->FromInput(p, wIn, accum, Stride->BytesPerPlaneIn);
                output = p->ToOutput(p, wIn, output, Stride->BytesPerPlaneOut);
            }

            strideIn += Stride->BytesPerLineIn;
            strideOut += Stride->BytesPerLineOut;
        }
    }

    private static void PrecalculatedXFORM(
        Transform* p,
        in void* @in,
        void* @out,
        uint PixelsPerLine,
        uint LineCount,
        in Stride* Stride)
    {
        var wIn = stackalloc ushort[cmsMAXCHANNELS];
        var wOut = stackalloc ushort[cmsMAXCHANNELS];

        _cmsHandleExtraChannels(p, @in, @out, PixelsPerLine, LineCount, Stride);

        var strideIn = 0u;
        var strideOut = 0u;
        memset(wIn, 0, sizeof(ushort) * cmsMAXCHANNELS);
        memset(wOut, 0, sizeof(ushort) * cmsMAXCHANNELS);

        for (var i = 0; i < LineCount; i++)
        {
            var accum = (byte*)@in + strideIn;
            var output = (byte*)@out + strideOut;

            for (var j = 0; j < PixelsPerLine; j++)
            {
                accum = p->FromInput(p, wIn, accum, Stride->BytesPerPlaneIn);
                p->Lut->Eval16Fn(wIn, wOut, p->Lut->Data);
                output = p->ToOutput(p, wIn, output, Stride->BytesPerPlaneOut);
            }

            strideIn += Stride->BytesPerLineIn;
            strideOut += Stride->BytesPerLineOut;
        }
    }

    private static void TransformOnePixelWithGamutCheck(Transform* p, in ushort* wIn, ushort* wOut)
    {
        ushort wOutOfGamut;

        p->GamutCheck->Eval16Fn(wIn, &wOutOfGamut, p->GamutCheck->Data);
        if (wOutOfGamut >= 1)
        {
            var ContextAlarmCodes = _cmsGetContext(p->ContextID).AlarmCodes;

            for (var i = 0; i < p->Lut->OutputChannels; i++)
                wOut[i] = ContextAlarmCodes.AlarmCodes[i];
        }
        else
        {
            p->Lut->Eval16Fn(wIn, wOut, p->Lut->Data);
        }
    }

    private static void PrecalculatedXFORMGamutCheck(
        Transform* p,
        in void* @in,
        void* @out,
        uint PixelsPerLine,
        uint LineCount,
        in Stride* Stride)
    {
        var wIn = stackalloc ushort[cmsMAXCHANNELS];
        var wOut = stackalloc ushort[cmsMAXCHANNELS];

        _cmsHandleExtraChannels(p, @in, @out, PixelsPerLine, LineCount, Stride);

        var strideIn = 0u;
        var strideOut = 0u;
        memset(wIn, 0, sizeof(ushort) * cmsMAXCHANNELS);
        memset(wOut, 0, sizeof(ushort) * cmsMAXCHANNELS);

        for (var i = 0; i < LineCount; i++)
        {
            var accum = (byte*)@in + strideIn;
            var output = (byte*)@out + strideOut;

            for (var j = 0; j < PixelsPerLine; j++)
            {
                accum = p->FromInput(p, wIn, accum, Stride->BytesPerPlaneIn);
                TransformOnePixelWithGamutCheck(p, wIn, wOut);
                output = p->ToOutput(p, wIn, output, Stride->BytesPerPlaneOut);
            }

            strideIn += Stride->BytesPerLineIn;
            strideOut += Stride->BytesPerLineOut;
        }
    }

    private static void CachedXFORM(
        Transform* p,
        in void* @in,
        void* @out,
        uint PixelsPerLine,
        uint LineCount,
        in Stride* Stride)
    {
        var wIn = stackalloc ushort[cmsMAXCHANNELS];
        var wOut = stackalloc ushort[cmsMAXCHANNELS];
        Cache Cache;

        _cmsHandleExtraChannels(p, @in, @out, PixelsPerLine, LineCount, Stride);

        // Empty buffers for quick memcmp
        memset(wIn, 0, sizeof(ushort) * cmsMAXCHANNELS);
        memset(wOut, 0, sizeof(ushort) * cmsMAXCHANNELS);

        // Get copy of zero cache
        memcpy(&Cache, &p->Cache);

        var strideIn = 0u;
        var strideOut = 0u;

        for (var i = 0; i < LineCount; i++)
        {
            var accum = (byte*)@in + strideIn;
            var output = (byte*)@out + strideOut;

            for (var j = 0; j < PixelsPerLine; j++)
            {
                accum = p->FromInput(p, wIn, accum, Stride->BytesPerPlaneIn);

                if (memcmp(wIn, Cache.CacheIn, sizeof(ushort) * cmsMAXCHANNELS) is 0)
                {
                    memcpy(wOut, Cache.CacheOut, sizeof(ushort) * cmsMAXCHANNELS);
                }
                else
                {
                    p->Lut->Eval16Fn(wIn, wOut, p->Lut->Data);

                    memcpy(Cache.CacheIn, wIn, sizeof(ushort) * cmsMAXCHANNELS);
                    memcpy(Cache.CacheOut, wOut, sizeof(ushort) * cmsMAXCHANNELS);
                }

                output = p->ToOutput(p, wIn, output, Stride->BytesPerPlaneOut);
            }

            strideIn += Stride->BytesPerLineIn;
            strideOut += Stride->BytesPerLineOut;
        }
    }

    private static void CachedXFORMGamutCheck(
        Transform* p,
        in void* @in,
        void* @out,
        uint PixelsPerLine,
        uint LineCount,
        in Stride* Stride)
    {
        var wIn = stackalloc ushort[cmsMAXCHANNELS];
        var wOut = stackalloc ushort[cmsMAXCHANNELS];
        Cache Cache;

        _cmsHandleExtraChannels(p, @in, @out, PixelsPerLine, LineCount, Stride);

        // Empty buffers for quick memcmp
        memset(wIn, 0, sizeof(ushort) * cmsMAXCHANNELS);
        memset(wOut, 0, sizeof(ushort) * cmsMAXCHANNELS);

        // Get copy of zero cache
        memcpy(&Cache, &p->Cache);

        var strideIn = 0u;
        var strideOut = 0u;

        for (var i = 0; i < LineCount; i++)
        {
            var accum = (byte*)@in + strideIn;
            var output = (byte*)@out + strideOut;

            for (var j = 0; j < PixelsPerLine; j++)
            {
                accum = p->FromInput(p, wIn, accum, Stride->BytesPerPlaneIn);

                if (memcmp(wIn, Cache.CacheIn, sizeof(ushort) * cmsMAXCHANNELS) is 0)
                {
                    memcpy(wOut, Cache.CacheOut, sizeof(ushort) * cmsMAXCHANNELS);
                }
                else
                {
                    TransformOnePixelWithGamutCheck(p, wIn, wOut);

                    memcpy(Cache.CacheIn, wIn, sizeof(ushort) * cmsMAXCHANNELS);
                    memcpy(Cache.CacheOut, wOut, sizeof(ushort) * cmsMAXCHANNELS);
                }

                output = p->ToOutput(p, wIn, output, Stride->BytesPerPlaneOut);
            }

            strideIn += Stride->BytesPerLineIn;
            strideOut += Stride->BytesPerLineOut;
        }
    }

    internal static void DupPluginTransformList(Context ctx, in Context src)
    {
        TransformPluginChunkType head = src.TransformPlugin;
        TransformCollection* Anterior = null, entry;
        TransformPluginChunkType newHead = new();

        _cmsAssert(ctx);
        _cmsAssert(head);

        // Walk the list copying all nodes
        for (entry = head.TransformCollection;
             entry is not null;
             entry = entry->Next)
        {
            var newEntry = _cmsSubAllocDup<TransformCollection>(ctx.MemPool, entry);

            if (newEntry is null)
                return;

            // We want to keep the linked list order, so this is a little bit tricky
            newEntry->Next = null;
            if (Anterior is not null)
                Anterior->Next = newEntry;

            Anterior = newEntry;

            if (newHead.TransformCollection is null)
                newHead.TransformCollection = newEntry;
        }

        ctx.TransformPlugin = newHead;
    }

    internal static void _cmsAllocTransformPluginChunk(Context ctx, in Context? src)
    {
        _cmsAssert(ctx);

        var from = src is not null
            ? src.TransformPlugin
            : TransformPluginChunk;

        _cmsAssert(from);

        ctx.TransformPlugin = (TransformPluginChunkType)from.Dup(ctx);

        //fixed (TransformPluginChunkType* @default = &TransformPluginChunk)
        //    AllocPluginChunk(ctx, src, DupPluginTransformList, Chunks.TransformPlugin, @default);
    }

    internal static void _cmsTransform2toTransformAdaptor(
        Transform* CMMcargo,
        in void* InputBuffer,
        void* OutputBuffer,
        uint PixelsPerLine,
        uint LineCount,
        in Stride* Stride)
    {
        _cmsHandleExtraChannels(CMMcargo, InputBuffer, OutputBuffer, PixelsPerLine, LineCount, Stride);

        var strideIn = 0u;
        var strideOut = 0u;

        for (var i = 0; i < LineCount; i++)
        {
            void* accum = (byte*)InputBuffer + strideIn;
            void* output = (byte*)OutputBuffer + strideOut;

            CMMcargo->OldXform!(CMMcargo, accum, output, PixelsPerLine, Stride->BytesPerPlaneIn);

            strideIn += Stride->BytesPerLineIn;
            strideOut += Stride->BytesPerLineOut;
        }
    }

    internal static bool _cmsRegisterTransformPlugin(Context? id, PluginBase? Data)
    {
        var Plugin = (PluginTransform?)Data;
        var ctx = _cmsGetContext(id).TransformPlugin;

        if (Data is null)
        {
            // Free the chain. Memory is safely freed at exit
            ctx.TransformCollection = null;
            return true;
        }

        // Factory callback is required
        if (Plugin!.factories.xform is null) return false;

        var fl = _cmsPluginMalloc<TransformCollection>(id);
        if (fl is null) return false;

        // Check for full xform plug-ins previous to 2.8, we would need an adapter in that case
        fl->OldXform = Plugin.ExpectedVersion < 2080;

        // Copy the parameters
        fl->Factory = Plugin.factories.xform;

        // Keep linked list
        fl->Next = ctx.TransformCollection;
        ctx.TransformCollection = fl;

        // All is ok
        return true;
    }

    internal static void _cmsSetTransformUserData(Transform* CMMcargo, void* ptr, FreeUserDataFn? FreePrivateDataFn)
    {
        _cmsAssert(CMMcargo);
        CMMcargo->UserData = ptr;
        CMMcargo->FreeUserData = FreePrivateDataFn;
    }

    internal static void* _cmsGetTransformUserData(Transform* CMMcargo)
    {
        _cmsAssert(CMMcargo);
        return CMMcargo->UserData;
    }

    internal static void _cmsGetTransformFormatters16(
        Transform* CMMcargo,
        Formatter16* FromInput,
        Formatter16* ToOutput)
    {
        _cmsAssert(CMMcargo);
        if (FromInput is not null) *FromInput = CMMcargo->FromInput;
        if (ToOutput is not null) *ToOutput = CMMcargo->ToOutput;
    }

    internal static void _cmsGetTransformFormattersFloat(
        Transform* CMMcargo,
        FormatterFloat* FromInput,
        FormatterFloat* ToOutput)
    {
        _cmsAssert(CMMcargo);
        if (FromInput is not null) *FromInput = CMMcargo->FromInputFloat;
        if (ToOutput is not null) *ToOutput = CMMcargo->ToOutputFloat;
    }

    internal static uint _cmsGetTransformFlags(Transform* CMMcargo)
    {
        _cmsAssert(CMMcargo);
        return CMMcargo->dwOriginalFlags;
    }

    private static Transform* AllocEmptyTransform(
        Context? ContextID,
        Pipeline* lut,
        uint Intent,
        uint* InputFormat,
        uint* OutputFormat,
        uint* dwFlags)
    {
        var ctx = _cmsGetContext(ContextID).TransformPlugin;

        // Allocate needed memory
        var p = _cmsMallocZero<Transform>(ContextID);

        // Store the proposed pipeline
        p->Lut = lut;

        // Let's see if any plug-in wants to do the transform by itself
        if (p->Lut is not null)
        {
            if ((*dwFlags & cmsFLAGS_NOOPTIMIZE) is 0)
            {
                for (var Plugin = ctx.TransformCollection;
                    Plugin is not null;
                    Plugin = Plugin->Next)
                {
                    if (Plugin->Factory(p->xform, &p->UserData, p->FreeUserData, &p->Lut, InputFormat, OutputFormat, dwFlags))
                    {
                        // Last plugin in the declaration order takes control. We just keep
                        // the original parameters as a logging.
                        // Note that cmsFLAGS_CAN_CHANGE_FORMATTER is not set, so by default
                        // an optimized transform is not reusable. The plug-in can, however, change
                        // the flags and make it suitable.

                        p->ContextID = ContextID;
                        p->InputFormat = *InputFormat;
                        p->OutputFormat = *OutputFormat;
                        p->dwOriginalFlags = *dwFlags;

                        // Fill the formatters just in case the optimized routine is interested.
                        // No error is thrown if the formatter doesn't exist. It is up to the optimization
                        // factory to decide what to do in those cases.
                        p->FromInput = _cmsGetFormatter(ContextID, *InputFormat, FormatterDirection.Input, PackFlags.Ushort).Fmt16;
                        p->ToOutput = _cmsGetFormatter(ContextID, *OutputFormat, FormatterDirection.Output, PackFlags.Ushort).Fmt16;
                        p->FromInputFloat = _cmsGetFormatter(ContextID, *InputFormat, FormatterDirection.Input, PackFlags.Float).FmtFloat;
                        p->ToOutputFloat = _cmsGetFormatter(ContextID, *OutputFormat, FormatterDirection.Output, PackFlags.Float).FmtFloat;

                        // Save the day? (Ignore the warning)
                        if (Plugin->OldXform)
                        {
                            p->OldXform = *(TransformFn*)&p->xform;
                            p->xform = _cmsTransform2toTransformAdaptor;
                        }

                        return p;
                    }
                }
            }

            // Not suitable for the transform plug-in, let's check the pipeline plug-in
            //_cmsOptimizePipeline(ContextID, &p->Lut, Intent, InputFormat, OutputFormat, dwFlags);
        }

        // Check whether this is a true floating point transform
        if (_cmsFormatterIsFloat(*OutputFormat))
        {
            // Get formatter function always return a valid union, but the context of this union may be null.
            p->FromInputFloat = _cmsGetFormatter(ContextID, *InputFormat, FormatterDirection.Input, PackFlags.Float).FmtFloat;
            p->ToOutputFloat = _cmsGetFormatter(ContextID, *OutputFormat, FormatterDirection.Output, PackFlags.Float).FmtFloat;
            *dwFlags |= cmsFLAGS_CAN_CHANGE_FORMATTER;

            if (p->FromInputFloat is null || p->ToOutputFloat is null)
            {
                cmsSignalError(ContextID, cmsERROR_UNKNOWN_EXTENSION, "Unsupported raster format");
                cmsDeleteTransform(p);
                return null;
            }

            p->xform = ((*dwFlags & cmsFLAGS_NULLTRANSFORM) is not 0)
                ? NullFloatXFORM
                // Float transforms don't use cache, always are non-null
                : FloatXFORM;
        }
        else
        {
            if (*InputFormat is 0 && *OutputFormat is 0)
            {
                p->FromInput = p->ToOutput = null!;
                *dwFlags |= cmsFLAGS_CAN_CHANGE_FORMATTER;
            }
            else
            {
                p->FromInput = _cmsGetFormatter(ContextID, *InputFormat, FormatterDirection.Input, PackFlags.Ushort).Fmt16;
                p->ToOutput = _cmsGetFormatter(ContextID, *OutputFormat, FormatterDirection.Output, PackFlags.Ushort).Fmt16;

                if (p->FromInput is null || p->ToOutput is null)
                {
                    cmsSignalError(ContextID, cmsERROR_UNKNOWN_EXTENSION, "Unsupported raster format");
                    cmsDeleteTransform(p);
                    return null;
                }

                var BytesPerPixelInput = T_BYTES(p->InputFormat);
                if (BytesPerPixelInput is 0 or >= 2)
                    *dwFlags |= cmsFLAGS_CAN_CHANGE_FORMATTER;
            }

            p->xform = (*dwFlags & cmsFLAGS_NULLTRANSFORM, *dwFlags & cmsFLAGS_NOCACHE, *dwFlags & cmsFLAGS_GAMUTCHECK) switch
            {
                (not 0, _, _) => NullXFORM,
                (_, not 0, not 0) => PrecalculatedXFORMGamutCheck,
                (_, not 0, _) => PrecalculatedXFORM,
                (_, _, not 0) => CachedXFORMGamutCheck,
                _ => CachedXFORM,
            };
        }

        p->InputFormat = *InputFormat;
        p->OutputFormat = *OutputFormat;
        p->dwOriginalFlags = *dwFlags;
        p->ContextID = ContextID;
        p->UserData = null;
        return p;
    }

    private static bool GetXFormColorSpaces(uint nProfiles, Profile[] Profiles, Signature* Input, Signature* Output)
    {
        if (nProfiles is 0) return false;
        if (Profiles[0] is null) return false;

        var PostColorSpace = *Input = cmsGetColorSpace(Profiles[0]);

        for (var i = 0; i < nProfiles; i++)
        {
            var Profile = Profiles[i];

            var lIsInput = (uint)PostColorSpace is not cmsSigXYZData and not cmsSigLabData;

            if (Profile is null) return false;

            var cls = (uint)cmsGetDeviceClass(Profile);

            var (ColorSpaceIn, ColorSpaceOut) = (lIsInput, cls) switch
            {
                (_, cmsSigNamedColorClass) => ((Signature)cmsSig1colorData, (nProfiles > 1) ? cmsGetPCS(Profile) : cmsGetColorSpace(Profile)),
                (true, _) or (_, cmsSigLinkClass) => (cmsGetColorSpace(Profile), cmsGetPCS(Profile)),
                _ => (cmsGetPCS(Profile), cmsGetColorSpace(Profile)),
            };

            if (i is 0)
                *Input = ColorSpaceIn;

            PostColorSpace = ColorSpaceOut;
        }

        *Output = PostColorSpace;

        return true;
    }

    private static bool IsProperColorSpace(Signature Check, uint dwFormat)
    {
        var Space1 = T_COLORSPACE(dwFormat);
        var Space2 = _cmsLCMScolorSpace(Check);

        if (Space1 is PT_ANY) return true;
        if (Space1 == Space2) return true;

        if (Space1 is PT_LabV2 && Space2 is PT_Lab) return true;
        if (Space1 is PT_Lab && Space2 is PT_LabV2) return true;

        return false;
    }

    private static void NormalizeXYZ(CIEXYZ* Dest)
    {
        while (Dest->X > 2 && Dest->Y > 2 && Dest->Z > 2)
        {
            Dest->X /= 10;
            Dest->Y /= 10;
            Dest->Z /= 10;
        }
    }

    private static void SetWhitePoint(CIEXYZ* wtPt, in CIEXYZ* src)
    {
        if (src is null)
        {
            wtPt->X = cmsD50X;
            wtPt->Y = cmsD50Y;
            wtPt->Z = cmsD50Z;
        }
        else
        {
            wtPt->X = src->X;
            wtPt->Y = src->Y;
            wtPt->Z = src->Z;

            NormalizeXYZ(wtPt);
        }
    }

    public static Transform* cmsCreateExtendedTransform(
        Context? ContextID,
        uint nProfiles,
        Profile[] Profiles,
        bool* BPC,
        uint* Intents,
        double* AdaptationStates,
        Profile? hGamutProfile,
        uint nGamutPCSposition,
        uint InputFormat,
        uint OutputFormat,
        uint dwFlags)
    {
        Signature EntryColorSpace, ExitColorSpace;
        var LastIntent = Intents[nProfiles - 1];

        // If it is a fake transform
        if ((dwFlags & cmsFLAGS_NULLTRANSFORM) is not 0)
            return AllocEmptyTransform(ContextID, null, INTENT_PERCEPTUAL, &InputFormat, &OutputFormat, &dwFlags);

        // If gamut check is requested, make sure we have a gamut profile
        if ((dwFlags & cmsFLAGS_GAMUTCHECK) is not 0 && hGamutProfile is null)
            dwFlags &= ~(uint)cmsFLAGS_GAMUTCHECK;

        // On floating point transforms, inhibit cache
        if (_cmsFormatterIsFloat(InputFormat) || _cmsFormatterIsFloat(OutputFormat))
            dwFlags |= cmsFLAGS_NOCACHE;

        // Mark entry/exit spaces
        if (!GetXFormColorSpaces(nProfiles, Profiles, &EntryColorSpace, &ExitColorSpace))
        {
            cmsSignalError(ContextID, cmsERROR_NULL, "NULL input profiles on transform");
            return null;
        }

        // Check if proper colorspaces
        if (!IsProperColorSpace(EntryColorSpace, InputFormat))
        {
            cmsSignalError(ContextID, cmsERROR_COLORSPACE_CHECK, "Wrong input color space on transform");
            return null;
        }
        if (!IsProperColorSpace(ExitColorSpace, OutputFormat))
        {
            cmsSignalError(ContextID, cmsERROR_COLORSPACE_CHECK, "Wrong output color space on transform");
            return null;
        }

        // Check whether the transform is 16 bits and involves linear RGB in first profile. If so, disable optimizations
        if ((uint)EntryColorSpace is cmsSigRgbData && T_BYTES(InputFormat) is 2 && (dwFlags & cmsFLAGS_NOOPTIMIZE) is 0)
        {
            var gamma = cmsDetectRGBProfileGamma(Profiles[0], 0.1);

            if (gamma is > 0 and < 1.6)
                dwFlags |= cmsFLAGS_NOOPTIMIZE;
        }

        // Create a pipeline with all transformations
        var Lut = _cmsLinkProfiles(ContextID, nProfiles, Intents, Profiles, BPC, AdaptationStates, dwFlags);
        if (Lut is null)
        {
            cmsSignalError(ContextID, cmsERROR_NOT_SUITABLE, "Couldn't link the profiles");
            return null;
        }

        // Check channel count
        if ((cmsChannelsOf(EntryColorSpace) != cmsPipelineInputChannels(Lut)) ||
            (cmsChannelsOf(ExitColorSpace) != cmsPipelineOutputChannels(Lut)))
        {
            cmsPipelineFree(Lut);
            cmsSignalError(ContextID, cmsERROR_NOT_SUITABLE, "Channel count douesn't match. Profile is corrupted");
            return null;
        }

        // All seems ok
        var xform = AllocEmptyTransform(ContextID, Lut, LastIntent, &InputFormat, &OutputFormat, &dwFlags);
        if (xform is null)
            goto Error;

        // Keep values
        xform->EntryColorSpace = EntryColorSpace;
        xform->ExitColorSpace = ExitColorSpace;
        xform->RenderingIntent = Intents[nProfiles - 1];

        // Take white points
        SetWhitePoint(&xform->EntryWhitePoint, (BoxPtr<CIEXYZ>)cmsReadTag(Profiles[0], cmsSigMediaWhitePointTag));
        SetWhitePoint(&xform->ExitWhitePoint, (BoxPtr<CIEXYZ>)cmsReadTag(Profiles[nProfiles - 1], cmsSigMediaWhitePointTag));

        // Create a gamut check LUT if requested
        if (hGamutProfile is not null && ((dwFlags & cmsFLAGS_GAMUTCHECK) is not 0))
            xform->GamutCheck = _cmsCreateGamutCheckPipeline(ContextID, Profiles, BPC, Intents, AdaptationStates, nGamutPCSposition, hGamutProfile);

        // Try to read input and output colorant table
        if (cmsIsTag(Profiles[0], cmsSigColorantTableTag))
        {
            // Input table can only come in this way.
            xform->InputColorant = cmsDupNamedColorList((BoxPtr<NamedColorList>)cmsReadTag(Profiles[0], cmsSigColorantTableTag));
        }

        // Output is a little bit more complex.
        if ((uint)cmsGetDeviceClass(Profiles[nProfiles - 1]) is cmsSigLinkClass)
        {
            // This tag may exist only on devicelink profiles.
            if (cmsIsTag(Profiles[nProfiles - 1], cmsSigColorantTableOutTag))
            {
                // It may be null if error
                xform->OutputColorant = cmsDupNamedColorList((BoxPtr<NamedColorList>)cmsReadTag(Profiles[nProfiles - 1], cmsSigColorantTableOutTag));
            }
        }
        else
        {
            if (cmsIsTag(Profiles[nProfiles - 1], cmsSigColorantTableTag))
                xform->OutputColorant = cmsDupNamedColorList((BoxPtr<NamedColorList>)cmsReadTag(Profiles[nProfiles - 1], cmsSigColorantTableTag));
        }

        // Store the sequence of profiles
        xform->Sequence = ((dwFlags & cmsFLAGS_KEEP_SEQUENCE) is not 0)
            ? _cmsCompileProfileSequence(ContextID, nProfiles, Profiles)
            : null;

        // If this is a cached transform, init first value, which is zero (16 bits only)
        if ((dwFlags & cmsFLAGS_NOCACHE) is 0)
        {
            memset(&xform->Cache.CacheIn, 0, sizeof(ushort) * cmsMAXCHANNELS);

            if (xform->GamutCheck is not null)
            {
                TransformOnePixelWithGamutCheck(xform, xform->Cache.CacheIn, xform->Cache.CacheOut);
            }
            else
            {
                xform->Lut->Eval16Fn(xform->Cache.CacheIn, xform->Cache.CacheOut, xform->Lut->Data);
            }
        }

        return xform;

    Error:
        cmsPipelineFree(Lut);
        return null;
    }

    public static Transform* cmsCreateMultiprofileTransformTHR(
        Context? ContextID,
        Profile[] Profiles,
        uint InputFormat,
        uint OutputFormat,
        uint nProfiles,
        uint Intent,
        uint dwFlags)
    {
        var BPC = stackalloc bool[256];
        var Intents = stackalloc uint[256];
        var AdaptationStates = stackalloc double[256];

        if (nProfiles is <= 0 or > 255)
        {
            cmsSignalError(ContextID, cmsERROR_RANGE, $"Wrong number of profiles. 1..255 expected, {nProfiles} found.");
            return null;
        }

        for (var i = 0; i < nProfiles; i++)
        {
            BPC[i] = (dwFlags & cmsFLAGS_BLACKPOINTCOMPENSATION) is not 0;
            Intents[i] = Intent;
            AdaptationStates[i] = cmsSetAdaptationStateTHR(ContextID, -1);
        }

        return cmsCreateExtendedTransform(ContextID, nProfiles, Profiles, BPC, Intents, AdaptationStates, null, 0, InputFormat, OutputFormat, dwFlags);
    }

    public static Transform* cmsCreateMultiprofileTransform(
        Profile[] Profiles,
        uint InputFormat,
        uint OutputFormat,
        uint nProfiles,
        uint Intent,
        uint dwFlags) =>
        cmsCreateMultiprofileTransformTHR(null, Profiles, InputFormat, OutputFormat, nProfiles, Intent, dwFlags);

    public static Transform* cmsCreateTransformTHR(
        Context? ContextID,
        Profile Input,
        uint InputFormat,
        Profile Output,
        uint OutputFormat,
        uint Intent,
        uint dwFlags)
    {
        var hArray = new Profile[2] { Input, Output };

        return cmsCreateMultiprofileTransformTHR(ContextID, hArray, InputFormat, OutputFormat, Output is null ? 1u : 2u, Intent, dwFlags);
    }

    public static Transform* cmsCreateTransform(
        Profile Input,
        uint InputFormat,
        Profile Output,
        uint OutputFormat,
        uint Intent,
        uint dwFlags) =>
        cmsCreateTransformTHR(null, Input, InputFormat, Output, OutputFormat, Intent, dwFlags);

    public static Transform* cmsCreateProofingTransformTHR(
        Context? ContextID,
        Profile InputProfile,
        uint InputFormat,
        Profile OutputProfile,
        uint OutputFormat,
        Profile ProofingProfile,
        uint nIntent,
        uint ProofingIntent,
        uint dwFlags)
    {
        var DoBPC = (dwFlags & cmsFLAGS_BLACKPOINTCOMPENSATION) is not 0;
        var hArray = new Profile[4] { InputProfile, ProofingProfile, ProofingProfile, OutputProfile };
        var Intents = stackalloc uint[4] { nIntent, nIntent, INTENT_RELATIVE_COLORIMETRIC, ProofingIntent };
        var BPC = stackalloc bool[4] { DoBPC, DoBPC, false, false };
        var Adaptation = stackalloc double[4];

        Adaptation[0] = Adaptation[1] = Adaptation[2] = Adaptation[3] = cmsSetAdaptationStateTHR(ContextID, -1);

        if ((dwFlags & (cmsFLAGS_SOFTPROOFING | cmsFLAGS_GAMUTCHECK)) is 0)
            return cmsCreateTransformTHR(ContextID, InputProfile, InputFormat, OutputProfile, OutputFormat, nIntent, dwFlags);

        return cmsCreateExtendedTransform(ContextID, 4, hArray, BPC, Intents, Adaptation, ProofingProfile, 1, InputFormat, OutputFormat, dwFlags);
    }

    public static Transform* cmsCreateProofingTransform(
        Profile InputProfile,
        uint InputFormat,
        Profile OutputProfile,
        uint OutputFormat,
        Profile ProofingProfile,
        uint nIntent,
        uint ProofingIntent,
        uint dwFlags) =>
        cmsCreateProofingTransformTHR(null, InputProfile, InputFormat, OutputProfile, OutputFormat, ProofingProfile, nIntent, ProofingIntent, dwFlags);

    public static Context? cmsGetTransformContextID(HTRANSFORM xform) =>
        xform is not null
            ? ((Transform*)xform)->ContextID
            : null;

    public static uint cmsGetTransformInputFormat(HTRANSFORM xform) =>
        xform is not null
            ? ((Transform*)xform)->InputFormat
            : 0;

    public static uint cmsGetTransformOutputFormat(HTRANSFORM xform) =>
        xform is not null
            ? ((Transform*)xform)->OutputFormat
            : 0;

    public static bool cmsChangeBuffersFormat(HTRANSFORM hTransform, uint InputFormat, uint OutputFormat)
    {
        var xform = (Transform*)hTransform;
        // We only can afford to change formatters if previous transform is at least 16 bits
        if ((xform->dwOriginalFlags & cmsFLAGS_CAN_CHANGE_FORMATTER) is 0)
        {
            cmsSignalError(xform->ContextID, cmsERROR_NOT_SUITABLE, "cmsChangeBuffersFormat works only on transforms created originally with at least 16 bits of precision");
            return false;
        }

        var FromInput = _cmsGetFormatter(xform->ContextID, InputFormat, FormatterDirection.Input, PackFlags.Ushort).Fmt16;
        var ToOutput = _cmsGetFormatter(xform->ContextID, OutputFormat, FormatterDirection.Output, PackFlags.Ushort).Fmt16;

        if (FromInput is null || ToOutput is null)
        {
            cmsSignalError(xform->ContextID, cmsERROR_UNKNOWN_EXTENSION, "Unsupported raster format");
            return false;
        }

        xform->InputFormat = InputFormat;
        xform->OutputFormat = OutputFormat;
        xform->FromInput = FromInput;
        xform->ToOutput = ToOutput;
        return true;
    }
}
