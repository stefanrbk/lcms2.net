using System.Diagnostics;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Defines an additional optimization strategy. The function should return <see
///     langword="true"/> if any optimization is done on the LUT, as this terminates the
///     optimization search. Or <see langword="false"/> if it is unable to optimize and wants to
///     give a chance to the rest of the optimizers.
/// </summary>
/// <remarks>Implements the <c>_cmsOPToptimizeFn</c> typedef.</remarks>
public delegate bool OptimizationFn(Pipeline lut, Signature intent, Signature[] inputFormat, Signature[] outputFormat, uint[] flags);

/// <summary>
///     Optimization plugin
/// </summary>
/// <remarks>Implements the <c>cmsPluginOptimization</c> struct.</remarks>
public sealed class OptimizationPlugin: Plugin
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

        if (plugin is null)
        {
            ctx.optimizationCollection = null;
            return true;
        }

        if (plugin.Function is null)
            return false;

        ctx.optimizationCollection = new OptimizationCollection(plugin.Function, ctx.optimizationCollection);

        return true;
    }
}

internal class OptimizationCollection
{
    internal OptimizationCollection? next;

    internal OptimizationFn optimizePtr;

    public OptimizationCollection(OptimizationFn optimizePtr, OptimizationCollection? next)
    {
        this.optimizePtr = optimizePtr;
        this.next = next;
    }

    public OptimizationCollection(OptimizationCollection other, OptimizationCollection? next = null)
    {
        optimizePtr = other.optimizePtr;
        this.next = next;
    }
}

internal sealed class OptimizationPluginChunk
{
    internal static OptimizationPluginChunk global = new();
    internal OptimizationCollection? optimizationCollection;

    private static readonly OptimizationPluginChunk _tagsPluginChunk = new();

    private OptimizationPluginChunk()
    { }

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupOptimizationList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.OptimizationPlugin] = _tagsPluginChunk;
    }

    private static void DupOptimizationList(ref Context ctx, in Context src)
    {
        OptimizationPluginChunk newHead = new();
        OptimizationCollection? anterior = null;
        var head = (OptimizationPluginChunk?)src.chunks[(int)Chunks.OptimizationPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.optimizationCollection; entry is not null; entry = entry.next)
        {
            // We want to keep the linked list order, so this is a little bit tricky
            OptimizationCollection newEntry = new(entry);

            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.optimizationCollection is null)
                newHead.optimizationCollection = newEntry;
        }

        ctx.chunks[(int)Chunks.OptimizationPlugin] = newHead;
    }
}
