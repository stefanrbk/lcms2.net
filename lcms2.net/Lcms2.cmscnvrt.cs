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
    internal static CmsIntentsList defaultIntents = null!;

    internal static IntentsPluginChunkType defaultIntentsPluginChunk =>
        new();

    internal static readonly IntentsPluginChunkType globalIntentsPluginChunk = defaultIntentsPluginChunk;

    private static void DupPluginIntentsList(Context ctx, in Context src)
    {
        var newHead = new IntentsPluginChunkType();
        var head = (IntentsPluginChunkType)src.chunks[(int)Chunks.IntentPlugin]!;
        CmsIntentsList? Anterior = null;

        // Walk the list copying all nodes
        for (var entry = head.Intents;
            entry is not null;
            entry = entry.Next)
        {
            var newEntry = new CmsIntentsList(entry.Intent, entry.Description, entry.Link);

            // We want to keep the linked list order, so this is a little bit tricky
            if (Anterior is not null)
                Anterior.Next = newEntry;

            Anterior = newEntry;

            newHead.Intents ??= newEntry;
        }

        ctx.chunks[(int)Chunks.IntentPlugin] = newHead;
    }

    internal static void _cmsAllocIntentsPluginChunk(Context ctx, Context? src = null)
    {
        if (src is not null)
        {
            // Dpulicate the list
            DupPluginIntentsList(ctx, src);
        }
        else
        {
            ctx.chunks[(int)Chunks.IntentPlugin] = defaultIntentsPluginChunk;
        }
    }

    public static unsafe uint cmsGetSupportedIntents(uint nMax, uint* Codes, string?[] Descriptions) =>
        cmsGetSupportedIntentsTHR(null, nMax, Codes, Descriptions);

    public static unsafe uint cmsGetSupportedIntentsTHR(Context? ContextID, uint nMax, uint* Codes, string?[] Descriptions)
    {
        var ctx = (IntentsPluginChunkType)_cmsContextGetClientChunk(ContextID, Chunks.IntentPlugin)!;
        uint nIntents;
        CmsIntentsList? pt;

        for (nIntents = 0, pt = ctx.Intents; pt is not null; pt = pt.Next)
        {
            if (nIntents < nMax)
            {
                if (Codes is not null)
                    Codes[nIntents] = pt.Intent;

                if (nIntents < Descriptions.Length)
                    Descriptions[nIntents] = pt.Description;
            }

            nIntents++;
        }

        for (pt = defaultIntents; pt is not null; pt = pt.Next)
        {
            if (nIntents < nMax)
            {
                if (Codes is not null)
                    Codes[nIntents] = pt.Intent;

                if (nIntents < Descriptions.Length)
                    Descriptions[nIntents] = pt.Description;
            }

            nIntents++;
        }

        return nIntents;
    }

    internal static bool _cmsRegisterRenderingIntentPlugin(Context? id, PluginBase? Data)
    {
        var ctx = (IntentsPluginChunkType)_cmsContextGetClientChunk(id, Chunks.IntentPlugin)!;

        if (Data is PluginRenderingIntent Plugin)
        {
            var pt = new CmsIntentsList(Plugin.Intent, Plugin.Description, Plugin.Link, ctx.Intents);

            ctx.Intents = pt;

            return true;
        }
        else
        {
            ctx.Intents = null;
            return true;
        }
    }
}
