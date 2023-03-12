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

namespace lcms2;

public static unsafe partial class Lcms2
{
    internal static IntentsList defaultIntents;

    internal static readonly IntentsPluginChunkType IntentsPluginChunk = new();

    internal static readonly IntentsPluginChunkType* globalIntentsPluginChunk;

    internal static void _cmsAllocIntentsPluginChunk(Context* ctx, in Context* src)
    {
        fixed (IntentsPluginChunkType* @default = &IntentsPluginChunk)
            AllocPluginChunk(ctx, src, &DupPluginList<IntentsPluginChunkType, IntentsList>, Chunks.IntentPlugin, @default);
    }

    public static uint cmsGetSupportedIntents(uint nMax, uint* Codes, string?[]? Descriptions) =>
        cmsGetSupportedIntentsTHR(null, nMax, Codes, Descriptions);

    public static uint cmsGetSupportedIntentsTHR(Context* ContextID, uint nMax, uint* Codes, string?[]? Descriptions)
    {
        var ctx = _cmsContextGetClientChunk<IntentsPluginChunkType>(ContextID, Chunks.IntentPlugin);
        uint nIntents;
        IntentsList* pt;

        for (nIntents = 0, pt = ctx->Intents; pt is not null; pt = pt->Next)
        {
            if (nIntents < nMax)
            {
                if (Codes is not null)
                    Codes[nIntents] = pt->Intent;

                if (nIntents < Descriptions?.Length)
                    Descriptions[nIntents] = pt->Description;
            }

            nIntents++;
        }
        fixed (IntentsList* defIntents = &defaultIntents)
        {
            for (pt = defIntents; pt is not null; pt = pt->Next)
            {
                if (nIntents < nMax)
                {
                    if (Codes is not null)
                        Codes[nIntents] = pt->Intent;

                    if (nIntents < Descriptions?.Length)
                        Descriptions[nIntents] = pt->Description;
                }

                nIntents++;
            }
        }

        return nIntents;
    }

    internal static bool _cmsRegisterRenderingIntentPlugin(Context* id, PluginBase* Data)
    {
        var ctx = _cmsContextGetClientChunk<IntentsPluginChunkType>(id, Chunks.IntentPlugin);
        var Plugin = (PluginRenderingIntent*)Data;

        // Do we have to reset the custom intents?
        if (Data is null)
        {
            ctx->Intents = null;
            return true;
        }

        var fl = _cmsPluginMalloc<IntentsList>(id);
        if (fl is null) return false;

        fl->Intent = Plugin->Intent;
        fl->Description = Plugin->Description;

        fl->Link = Plugin->Link;

        fl->Next = ctx->Intents;
        ctx->Intents = fl;

        return true;
    }
}
