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
    private const double defaultAdaptationStateValue = 1.0;

    private static readonly AdaptationStateChunkType AdaptationStateChunk = new() { AdaptationState = defaultAdaptationStateValue };

    private static readonly AdaptationStateChunkType* globalAdaptationStateChunk;

    private static readonly AlarmCodesChunkType AlarmCodesChunk = new();

    private static readonly AlarmCodesChunkType* globalAlarmCodesChunk;

    internal static readonly TransformPluginChunkType TransformPluginChunk = new();

    internal static readonly TransformPluginChunkType* globalTransformPluginChunk;

    internal static void _cmsAllocAdaptationStateChunk(Context* ctx, in Context* src)
    {
        fixed (AdaptationStateChunkType* @default = &AdaptationStateChunk)
            AllocPluginChunk(ctx, src, Chunks.AdaptationStateContext, @default);
    }

    public static double cmsSetAdaptationStateTHR(Context* context, double d)
    {
        var ptr = _cmsContextGetClientChunk<AdaptationStateChunkType>(context, Chunks.AdaptationStateContext);

        // Get previous value for return
        var prev = ptr->AdaptationState;

        // Set the value if d is positive or zero
        if (d >= 0)
            ptr->AdaptationState = d;

        // Always return previous value
        return prev;
    }

    public static double cmsSetAdaptationState(double d) =>
        cmsSetAdaptationStateTHR(null, d);

    public static void cmsSetAlarmCodesTHR(Context* context, in ushort* AlarmCodesP)
    {
        var contextAlarmCodes = _cmsContextGetClientChunk<AlarmCodesChunkType>(context, Chunks.AlarmCodesContext);
        _cmsAssert(contextAlarmCodes is not null); // Can't happen
        memcpy(contextAlarmCodes->AlarmCodes, AlarmCodesP, (uint)sizeof(ushort) * cmsMAXCHANNELS);
    }

    public static void cmsGetAlarmCodesTHR(Context* context, ushort* AlarmCodesP)
    {
        var contextAlarmCodes = _cmsContextGetClientChunk<AlarmCodesChunkType>(context, Chunks.AlarmCodesContext);
        _cmsAssert(contextAlarmCodes is not null); // Can't happen
        memcpy(AlarmCodesP, contextAlarmCodes->AlarmCodes, (uint)sizeof(ushort) * cmsMAXCHANNELS);
    }

    public static void cmsSetAlarmCodes(in ushort* AlarmCodes) =>
        cmsSetAlarmCodesTHR(null, AlarmCodes);

    public static void cmsGetAlarmCodes(ushort* AlarmCodes) =>
        cmsGetAlarmCodesTHR(null, AlarmCodes);

    internal static void _cmsAllocAlarmCodesChunk(Context* ctx, in Context* src)
    {
        fixed (AlarmCodesChunkType* @default = &AlarmCodesChunk)
            AllocPluginChunk(ctx, src, Chunks.AlarmCodesContext, @default);
    }

    internal static void _cmsAllocTransformPluginChunk(Context* ctx, in Context* src)
    {
        fixed (TransformPluginChunkType* @default = &TransformPluginChunk)
            AllocPluginChunk(ctx, src, &DupPluginList<TransformPluginChunkType, TransformCollection>, Chunks.TransformPlugin, @default);
    }

    internal static bool _cmsRegisterTransformPlugin(Context* id, PluginBase* Data)
    {
        var Plugin = (PluginTransform*)Data;
        var ctx = _cmsContextGetClientChunk<TransformPluginChunkType>(id, Chunks.TransformPlugin);

        if (Data is null)
        {
            // Free the chain. Memory is safely freed at exit
            ctx->TransformCollection = null;
            return true;
        }

        // Factory callback is required
        if (Plugin->factories.xform is null) return false;

        var fl = _cmsPluginMalloc<TransformCollection>(id);
        if (fl is null) return false;

        // Check for full xform plug-ins previous to 2.8, we would need an adapter in that case
        fl->OldXform = Plugin->@base.ExpectedVersion < 2080;

        // Copy the parameters
        fl->Factory = Plugin->factories.xform;

        // Keep linked list
        fl->Next = ctx->TransformCollection;
        ctx->TransformCollection = fl;

        // All is ok
        return true;
    }
}
