using System.Diagnostics;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Defines an additional optimization strategy. The function should return <see langword="true"/> if any
///     optimization is done on the LUT, as this terminates the optimization search. Or <see langword="false"/>
///     if it is unable to optimize and wants to give a chance to the rest of the optimizers.
/// </summary>
/// <remarks>
///     Implements the <c>_cmsOPToptimizeFn</c> typedef.</remarks>
public delegate bool OptimizationFn(Pipeline lut, Signature intent, Signature[] inputFormat, Signature[] outputFormat, uint[] flags);

/// <summary>
///     Optimization plugin
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginOptimization</c> struct.</remarks>
public sealed class OptimizationPlugin : Plugin
{
    public OptimizationFn Function;
    public OptimizationPlugin(Signature magic, uint expectedVersion, Signature type, OptimizationFn function)
        : base(magic, expectedVersion, type)
    {
        Function = function;
    }

    internal static bool RegisterPlugin(Context? context, OptimizationPlugin? plugin)
    {
        var ctx = Context.GetOptimizationPlugin(context);

        if (plugin is null) {
            ctx.OptimizationCollection = null;
            return true;
        }

        if (plugin.Function is null)
            return false;

        ctx.OptimizationCollection = new OptimizationCollection(plugin.Function, ctx.OptimizationCollection);

        return true;
    }
}

internal class OptimizationCollection
{
    internal OptimizationFn OptimizePtr;

    internal OptimizationCollection? Next;

    public OptimizationCollection(OptimizationFn optimizePtr, OptimizationCollection? next)
    {
        OptimizePtr = optimizePtr;
        Next = next;
    }

    public OptimizationCollection(OptimizationCollection other, OptimizationCollection? next = null)
    {
        OptimizePtr = other.OptimizePtr;
        Next = next;
    }
}

internal sealed class OptimizationPluginChunk
{
    internal OptimizationCollection? OptimizationCollection;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupOptimizationList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.OptimizationPlugin] = tagsPluginChunk;
    }

    private OptimizationPluginChunk()
    { }

    internal static OptimizationPluginChunk global = new();
    private static readonly OptimizationPluginChunk tagsPluginChunk = new();

    private static void DupOptimizationList(ref Context ctx, in Context src)
    {
        OptimizationPluginChunk newHead = new();
        OptimizationCollection? anterior = null;
        var head = (OptimizationPluginChunk?)src.chunks[(int)Chunks.OptimizationPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.OptimizationCollection; entry is not null; entry = entry.Next) {
            // We want to keep the linked list order, so this is a little bit tricky
            OptimizationCollection newEntry = new(entry);

            if (anterior is not null)
                anterior.Next = newEntry;

            anterior = newEntry;

            if (newHead.OptimizationCollection is null)
                newHead.OptimizationCollection = newEntry;
        }

        ctx.chunks[(int)Chunks.OptimizationPlugin] = newHead;
    }
}
