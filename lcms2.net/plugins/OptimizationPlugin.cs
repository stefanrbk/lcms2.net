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

    internal static bool RegisterPlugin(object? state, OptimizationPlugin? plugin)
    {
        var ctx = State.GetOptimizationPlugin(state);

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

    internal static OptimizationPluginChunk Default => new();

    private OptimizationPluginChunk()
    { }
}
