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
    internal static readonly TagTypeLinkedList* supportedMPEtypes;

    internal static readonly TagTypePluginChunkType MPETypePluginChunk = new();

    internal static readonly TagTypePluginChunkType* globalMPETypePluginChunk;

    internal static readonly TagTypeLinkedList* supportedTagTypes;

    internal static readonly TagTypePluginChunkType TagTypePluginChunk = new();

    internal static readonly TagTypePluginChunkType* globalTagTypePluginChunk;

    internal static readonly TagLinkedList* supportedTags;

    internal static readonly TagPluginChunkType TagPluginChunk = new();

    internal static readonly TagPluginChunkType* globalTagPluginChunk;

    private static bool RegisterTypesPlugin(Context* id, PluginBase* Data, Chunks pos)
    {
        var Plugin = (PluginTagType*)Data;
        var ctx = _cmsContextGetClientChunk<TagTypePluginChunkType>(id, pos);

        // Calling the function with NULL as plug-in would unregister the plug in
        if (Data is null)
        {
            // There is no need to set free the memory, as pool is destroyed as a whole.
            ctx->TagTypes = null;
            return true;
        }

        // Registering happens in plug-in memory pool.
        var pt = _cmsPluginMalloc<TagTypeLinkedList>(id);
        if (pt is null) return false;

        pt->Handler = Plugin->Handler;
        pt->Next = ctx->TagTypes;

        ctx->TagTypes = pt;

        return true;
    }

    internal static void _cmsAllocTagTypePluginChunk(Context* ctx, in Context* src)
    {
        fixed (TagTypePluginChunkType* @default = &TagTypePluginChunk)
            AllocPluginChunk(ctx, src, &DupPluginList<TagTypePluginChunkType, TagTypeLinkedList>, Chunks.TagTypePlugin, @default);
    }

    internal static void _cmsAllocMPETypePluginChunk(Context* ctx, in Context* src)
    {
        fixed (TagTypePluginChunkType* @default = &MPETypePluginChunk)
            AllocPluginChunk(ctx, src, &DupPluginList<TagTypePluginChunkType, TagTypeLinkedList>, Chunks.MPEPlugin, @default);
    }

    internal static bool _cmsRegisterTagTypePlugin(Context* id, PluginBase* Data) =>
        RegisterTypesPlugin(id, Data, Chunks.TagTypePlugin);

    internal static bool _cmsRegisterMultiProcessElementPlugin(Context* id, PluginBase* Data) =>
        RegisterTypesPlugin(id, Data, Chunks.MPEPlugin);

    internal static void _cmsAllocTagPluginChunk(Context* ctx, in Context* src)
    {
        fixed (TagPluginChunkType* @default = &TagPluginChunk)
            AllocPluginChunk(ctx, src, &DupPluginList<TagPluginChunkType, TagLinkedList>, Chunks.TagPlugin, @default);
    }

    internal static bool _cmsRegisterTagPlugin(Context* id, PluginBase* Data)
    {
        var Plugin = (PluginTag*)Data;
        var TagPluginChunk = _cmsContextGetClientChunk<TagPluginChunkType>(id, Chunks.TagPlugin);

        if (Data is null)
        {
            TagPluginChunk->Tag = null;
            return true;
        }

        var pt = _cmsPluginMalloc<TagLinkedList>(id);
        if (pt == null) return false;

        pt->Signature = Plugin->Signature;
        pt->Descriptor = Plugin->Descriptor;
        pt->Next = TagPluginChunk->Tag;

        TagPluginChunk->Tag = pt;

        return true;
    }
}
