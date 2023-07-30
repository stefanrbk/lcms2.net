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

namespace lcms2.state;

public readonly unsafe struct Context
{
    private readonly static Dictionary<int, Context_class> repo = new();
    internal readonly static Context_class global = new();
    private Context(int? id) =>
        this.id = id;

    private readonly int? id;

    internal Context_class Get
    {
        get
        {
            lock (repo)
                return id.HasValue ? repo.TryGetValue(id.Value, out var value) ? value : global : global;
        }
    }

    internal Context(Context_class ctx) : this(ctx.GetHashCode())
    {
        lock (repo)
            repo.Add(id!.Value, ctx);
    }

    internal static void Remove(Context ctx)
    {
        if (ctx.id.HasValue)
        {
            lock (repo)
                repo.Remove(ctx.id.Value);
        }
    }

    public static implicit operator Context(int? id) =>
        new(id);

    public static bool operator !=(Context ctx, int? id) =>
        ctx.id != id;

    public static bool operator ==(Context ctx, int? id) =>
        ctx.id == id;

    public static bool operator !=(Context ctx1, Context ctx2) =>
        ctx1.id != ctx2.id;

    public static bool operator ==(Context ctx1, Context ctx2) =>
        ctx1.id == ctx2.id;

    public override bool Equals(object obj)
    {
        return obj switch
        {
            null => !id.HasValue,
            int val => this == val,
            Context ctx => this == ctx,
            _ => false
        };
    }

    public override int GetHashCode() =>
        HashCode.Combine(id, 69);
}

internal unsafe class Context_class
{
    internal SubAllocator* MemPool;
    internal MemPluginChunkType DefaultMemoryManager;
    internal void* UserData;
    internal LogErrorChunkType ErrorLogger;
    internal AdaptationStateChunkType AdaptationState;
    internal AlarmCodesChunkType AlarmCodes;
    internal MemPluginChunkType MemPlugin;
    internal InterpPluginChunkType InterpPlugin;
    internal CurvesPluginChunkType CurvesPlugin;
    internal FormattersPluginChunkType FormattersPlugin;
    internal TagTypePluginChunkType TagTypePlugin;
    internal TagPluginChunkType TagPlugin;
    internal IntentsPluginChunkType IntentsPlugin;
    internal TagTypePluginChunkType MPEPlugin;
    internal OptimizationPluginChunkType OptimizationPlugin;
    internal TransformPluginChunkType TransformPlugin;
    internal MutexPluginChunkType MutexPlugin;
}
