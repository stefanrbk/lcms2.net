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

    private static readonly AdaptationStateChunkType defaultAdaptationStateChunk = new()
    {
        AdaptationState = defaultAdaptationStateValue
    };

    private static AdaptationStateChunkType globalAdaptationStateChunk = new()
    {
        AdaptationState = defaultAdaptationStateValue
    };

    private static readonly ushort[] defaultAlarmCodesValue = { 0x7F00, 0x7F00, 0x7F00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    private static readonly AlarmCodesChunkType defaultAlarmCodesChunk = new()
    {
        AlarmCodes = defaultAlarmCodesValue
    };

    private static AlarmCodesChunkType globalAlarmCodesChunk = new()
    {
        AlarmCodes = (ushort[])defaultAlarmCodesValue.Clone()
    };

    internal static void _cmsAllocAdaptationStateChunk(Context ctx, Context? src = null)
    {
        var from = (AdaptationStateChunkType?)src?.chunks[(int)Chunks.AdaptationStateContext] ?? defaultAdaptationStateChunk;

        ((AdaptationStateChunkType)ctx.chunks[(int)Chunks.AdaptationStateContext]!).AdaptationState = from.AdaptationState;
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
        var from = (AlarmCodesChunkType?)src?.chunks[(int)Chunks.AlarmCodesContext] ?? defaultAlarmCodesChunk;

        from.AlarmCodes.CopyTo(((AlarmCodesChunkType)ctx.chunks[(int)Chunks.AlarmCodesContext]!).AlarmCodes.AsSpan());
    }
}
