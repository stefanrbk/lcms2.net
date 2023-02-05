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

using lcms2.plugins;

namespace lcms2;

public static partial class Lcms2
{
    private const double defaultAdaptationStateValue = 1.0;

    private static AdaptationStateChunkType defaultAdaptationStateChunk => new()
    {
        AdaptationState = defaultAdaptationStateValue
    };

    private static readonly AdaptationStateChunkType globalAdaptationStateChunk = defaultAdaptationStateChunk;

    private static readonly ushort[] defaultAlarmCodesValue = { 0x7F00, 0x7F00, 0x7F00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    private static AlarmCodesChunkType defaultAlarmCodesChunk => new()
    {
        AlarmCodes = (ushort[])defaultAlarmCodesValue.Clone(),
    };

    private static readonly AlarmCodesChunkType globalAlarmCodesChunk = defaultAlarmCodesChunk;

    internal static TransformPluginChunkType defaultTransformPluginChunk =>
        new();

    internal static readonly TransformPluginChunkType globalTransformPluginChunk = defaultTransformPluginChunk;

    internal static void _cmsAllocAdaptationStateChunk(Context ctx, Context? src = null)
    {
        ctx.chunks[(int)Chunks.AdaptationStateContext] =
            src?.chunks[(int)Chunks.AdaptationStateContext] is AdaptationStateChunkType chunk
            ? new AdaptationStateChunkType() { AdaptationState = chunk.AdaptationState }
            : defaultAdaptationStateChunk;
    }

    public static double cmsSetAdaptationStateTHR(Context? context, double d)
    {
        var ptr = (AdaptationStateChunkType)_cmsContextGetClientChunk(context, Chunks.AdaptationStateContext)!;

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

    public static void cmsSetAlarmCodesTHR(Context? context, ReadOnlySpan<ushort> AlarmCodesP)
    {
        var contextAlarmCodes = (AlarmCodesChunkType)_cmsContextGetClientChunk(context, Chunks.AlarmCodesContext)!;
        AlarmCodesP.CopyTo(contextAlarmCodes.AlarmCodes);
    }

    public static void cmsGetAlarmCodesTHR(Context? context, Span<ushort> AlarmCodesP)
    {
        var contextAlarmCodes = (AlarmCodesChunkType)_cmsContextGetClientChunk(context, Chunks.AlarmCodesContext)!;
        contextAlarmCodes.AlarmCodes.CopyTo(AlarmCodesP);
    }

    public static void cmsSetAlarmCodes(ReadOnlySpan<ushort> AlarmCodes) =>
        cmsSetAlarmCodesTHR(null, AlarmCodes);

    public static void cmsGetAlarmCodes(Span<ushort> AlarmCodes) =>
        cmsGetAlarmCodesTHR(null, AlarmCodes);

    internal static void _cmsAllocAlarmCodesChunk(Context ctx, Context? src = null)
    {
        ctx.chunks[(int)Chunks.AlarmCodesContext] =
            src?.chunks[(int)Chunks.AlarmCodesContext] is AlarmCodesChunkType chunk
            ? new AlarmCodesChunkType() { AlarmCodes = (ushort[])chunk.AlarmCodes.Clone() }
            : defaultAlarmCodesChunk;
    }

    private static void DupPluginTransformList(Context ctx, in Context src)
    {
        var newHead = new TransformPluginChunkType();
        var head = (TransformPluginChunkType)src.chunks[(int)Chunks.TransformPlugin]!;
        CmsTransformCollection? Anterior = null;

        // Walk the list copying all nodes
        for (var entry = head.TransformCollection;
            entry is not null;
            entry = entry.Next)
        {
            var newEntry = new CmsTransformCollection
            {
                OldXform = entry.OldXform
            };

            if (entry.OldXform)
                newEntry.OldFactory = entry.OldFactory;
            else
                newEntry.Factory = entry.Factory;

            // We want to keep the linked list order, so this is a little bit tricky
            if (Anterior is not null)
                Anterior.Next = newEntry;

            Anterior = newEntry;

            newHead.TransformCollection ??= newEntry;
        }

        ctx.chunks[(int)Chunks.TransformPlugin] = newHead;
    }

    internal static void _cmsAllocTransformPluginChunk(Context ctx, Context? src)
    {
        if (src is not null)
        {
            // Dpulicate the list
            DupPluginTransformList(ctx, src);
        }
        else
        {
            ctx.chunks[(int)Chunks.TransformPlugin] = defaultTransformPluginChunk;
        }
    }

    internal static bool _cmsRegisterTransformPlugin(Context? id, PluginBase? Data)
    {
        var ctx = (TransformPluginChunkType)_cmsContextGetClientChunk(id, Chunks.TransformPlugin)!;

        if (Data is PluginTransform Plugin)
        {
            if (Plugin.xform is null && Plugin.legacy_xform is null)
                return false;

            ctx.TransformCollection = new CmsTransformCollection()
            {
                OldXform = Plugin.ExpectedVersion < 2080,
                Factory = Plugin.xform,
                OldFactory = Plugin.legacy_xform,
                Next = ctx.TransformCollection
            };

            return true;
        }
        else
        {
            ctx.TransformCollection = null;
            return true;
        }
    }
}
