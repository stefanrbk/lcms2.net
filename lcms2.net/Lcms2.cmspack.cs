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
    internal static readonly FormattersPluginChunkType FormattersPluginChunk;

    internal static readonly FormattersPluginChunkType* globalFormattersPluginChunk;

    internal static void _cmsAllocFormattersPluginChunk(Context* ctx, in Context* src)
    {
        fixed (FormattersPluginChunkType* @default = &FormattersPluginChunk)
            AllocPluginChunk(ctx, src, &DupPluginList<FormattersPluginChunkType, FormattersFactoryList>, Chunks.FormattersPlugin, @default);
    }

    internal static bool _cmsRegisterFormattersPlugin(Context* ContextID, PluginBase* Data)
    {
        var ctx = _cmsContextGetClientChunk<FormattersPluginChunkType>(ContextID, Chunks.FormattersPlugin);
        var Plugin = (PluginFormatters*)Data;

        // Reset to build-in defaults
        if (Data is null)
        {
            ctx->FactoryList = null;
            return true;
        }

        var fl = _cmsPluginMalloc<FormattersFactoryList>(ContextID);
        if (fl is null) return false;

        fl->Factory = Plugin->FormattersFactory;

        fl->Next = ctx->FactoryList;
        ctx->FactoryList = fl;

        return true;
    }
}
