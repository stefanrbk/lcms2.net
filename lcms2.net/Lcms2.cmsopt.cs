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
    internal static CmsOptimizationCollection defaultOptimization = null!;

    internal static OptimizationPluginChunkType defaultOptimizationPluginChunk =>
        new();

    internal static readonly OptimizationPluginChunkType globalOptimizationPluginChunk = defaultOptimizationPluginChunk;

    private static void DupPluginOptimizationList(Context ctx, in Context src)
    {
        var newHead = new OptimizationPluginChunkType();
        var head = (OptimizationPluginChunkType)src.chunks[(int)Chunks.OptimizationPlugin]!;
        CmsOptimizationCollection? Anterior = null;

        // Walk the list copying all nodes
        for (var entry = head.OptimizationCollection;
            entry is not null;
            entry = entry.Next)
        {
            var newEntry = new CmsOptimizationCollection(entry.OptimizePtr);

            // We want to keep the linked list order, so this is a little bit tricky
            if (Anterior is not null)
                Anterior.Next = newEntry;

            Anterior = newEntry;

            newHead.OptimizationCollection ??= newEntry;
        }

        ctx.chunks[(int)Chunks.OptimizationPlugin] = newHead;
    }

    internal static void _cmsAllocOptimizationPluginChunk(Context ctx, Context? src = null)
    {
        if (src is not null)
        {
            // Dpulicate the list
            DupPluginOptimizationList(ctx, src);
        }
        else
        {
            ctx.chunks[(int)Chunks.OptimizationPlugin] = defaultOptimizationPluginChunk;
        }
    }

    internal static bool _cmsRegisterOptimizationPlugin(Context? id, PluginBase? Data)
    {
        var ctx = (OptimizationPluginChunkType)_cmsContextGetClientChunk(id, Chunks.OptimizationPlugin)!;

        if (Data is PluginOptimization Plugin)
        {
            ctx.OptimizationCollection = new CmsOptimizationCollection(Plugin.OptimizePtr, ctx.OptimizationCollection);

            return true;
        }
        else
        {
            ctx.OptimizationCollection = null;
            return true;
        }
    }
}
