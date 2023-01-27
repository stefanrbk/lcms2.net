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
    internal static FormattersPluginChunkType defaultFormattersPluginChunk =>
        new();

    internal static FormattersPluginChunkType globalFormattersPluginChunk = defaultFormattersPluginChunk;

    private static void DupFormatterFactoryList(Context ctx, in Context src)
    {
        var newHead = new FormattersPluginChunkType();
        var head = (FormattersPluginChunkType)src.chunks[(int)Chunks.FormattersPlugin]!;
        CmsFormattersFactoryList? Anterior = null;

        // Walk the list copying all nodes
        for (var entry = head.FactoryList;
            entry is not null;
            entry = entry.Next)
        {
            var newEntry = new CmsFormattersFactoryList();

            // We want to keep the linked list order, so this is a little bit tricky
            if (Anterior is not null)
                Anterior.Next = newEntry;

            Anterior = newEntry;

            newHead.FactoryList ??= newEntry;
        }

        ctx.chunks[(int)Chunks.FormattersPlugin] = newHead;
    }

    internal static void _cmsAllocFormattersPluginChunk(Context ctx, Context? src = null)
    {
        if (src is not null)
        {
            // Duplicate the list
            DupFormatterFactoryList(ctx, src);
        }
        else
        {
            ctx.chunks[(int)Chunks.FormattersPlugin] = defaultFormattersPluginChunk;
        }
    }

    internal static bool _cmsRegisterFormattersPlugin(Context? ContextID, PluginBase? Data)
    {
        var ctx = (FormattersPluginChunkType)_cmsContextGetClientChunk(ContextID, Chunks.FormattersPlugin)!;

        if (Data is PluginFormatters Plugin)
        {
            ctx.FactoryList = new CmsFormattersFactoryList()
            {
                Factory = Plugin.FormattersFactory,
                Next = ctx.FactoryList,
            };
        }
        else
        {
            ctx.FactoryList = null;
        }
        return true;
    }
}
