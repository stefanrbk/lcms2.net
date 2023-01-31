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
    internal static CmsTagTypeLinkedList supportedMPEtypes = null!;

    internal static TagTypePluginChunkType defaultMPETypePluginChunk =>
        new();

    internal static readonly TagTypePluginChunkType globalMPETypePluginChunk = defaultMPETypePluginChunk;

    internal static CmsTagTypeLinkedList supportedTagTypes = null!;

    internal static TagTypePluginChunkType defaultTagTypePluginChunk =>
        new();

    internal static readonly TagTypePluginChunkType globalTagTypePluginChunk = defaultTagTypePluginChunk;

    private static bool RegisterTypesPlugin(Context? id, PluginBase? Data, Chunks pos)
    {
        var ctx = (TagTypePluginChunkType)_cmsContextGetClientChunk(id, pos)!;

        if (Data is PluginTagType Plugin)
        {
            ctx.TagTypes = new CmsTagTypeLinkedList(Plugin.Handler, ctx.TagTypes);
            return true;
        }
        else
        {
            ctx.TagTypes = null;
            return false;
        }
    }

    private static void DupTagTypeList(Context ctx, in Context src, Chunks loc)
    {
        var newHead = new TagTypePluginChunkType();
        var head = (TagTypePluginChunkType)src.chunks[(int)loc]!;
        CmsTagTypeLinkedList? Anterior = null;

        // Walk the list copying all nodes
        for (var entry = head.TagTypes;
            entry is not null;
            entry = entry.Next)
        {
            var newEntry = new CmsTagTypeLinkedList(entry.Handler);

            // We want to keep the linked list order, so this is a little bit tricky
            if (Anterior is not null)
                Anterior.Next = newEntry;

            Anterior = newEntry;

            newHead.TagTypes ??= newEntry;
        }

        ctx.chunks[(int)Chunks.TagTypePlugin] = newHead;
    }

    internal static void _cmsAllocTagTypePluginChunk(Context ctx, Context? src = null)
    {
        if (src is not null)
        {
            // Dpulicate the list
            DupTagTypeList(ctx, src, Chunks.TagTypePlugin);
        }
        else
        {
            ctx.chunks[(int)Chunks.TagTypePlugin] = defaultTagTypePluginChunk;
        }
    }

    internal static void _cmsAllocMPETypePluginChunk(Context ctx, Context? src = null)
    {
        if (src is not null)
        {
            // Dpulicate the list
            DupTagTypeList(ctx, src, Chunks.MPEPlugin);
        }
        else
        {
            ctx.chunks[(int)Chunks.MPEPlugin] = defaultMPETypePluginChunk;
        }
    }

    internal static bool _cmsRegisterTagTypePlugin(Context? id, PluginBase? Data) =>
        RegisterTypesPlugin(id, Data, Chunks.TagTypePlugin);

    internal static bool _cmsRegisterMultiProcessElementPlugin(Context? id, PluginBase? Data) =>
        RegisterTypesPlugin(id, Data, Chunks.MPEPlugin);
}
