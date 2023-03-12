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
    internal static readonly OptimizationPluginChunkType OptimizationPluginChunk = new();

    internal static readonly OptimizationPluginChunkType* globalOptimizationPluginChunk;

    internal static void _cmsAllocOptimizationPluginChunk(Context* ctx, in Context* src)
    {
        fixed (OptimizationPluginChunkType* @default = &OptimizationPluginChunk)
            AllocPluginChunk(ctx, src, &DupPluginList<OptimizationPluginChunkType, OptimizationCollection>, Chunks.OptimizationPlugin, @default);
    }

    internal static bool _cmsRegisterOptimizationPlugin(Context* id, PluginBase* Data)
    {
        var Plugin = (PluginOptimization*)Data;
        var ctx = _cmsContextGetClientChunk<OptimizationPluginChunkType>(id, Chunks.OptimizationPlugin);

        if (Data is null)
        {
            ctx->OptimizationCollection = null;
            return true;
        }

        // Optimizer callback is required
        if (Plugin->OptimizePtr is null) return false;

        var fl = _cmsPluginMalloc<OptimizationCollection>(id);
        if (fl is null) return false;

        // Copy the parameters
        fl->OptimizePtr = Plugin->OptimizePtr;

        // Keep linked list
        fl->Next = ctx->OptimizationCollection;

        // Set the head
        ctx->OptimizationCollection = fl;

        // All is ok
        return true;
    }
}
