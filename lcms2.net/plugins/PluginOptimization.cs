using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Defines an additional optimization strategy. The function should return <see cref="true"/> if any
///     optimization is done on the LUT, as this terminates the optimization search. Or <see cref="false"/>
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
public sealed class PluginOptimization : Plugin
{
    public OptimizationFn Function;
    public PluginOptimization(Signature magic, uint expectedVersion, Signature type, OptimizationFn function)
        : base(magic, expectedVersion, type)
    {
        Function = function;
    }

    internal static bool RegisterPlugin(Context? context, PluginOptimization? plugin)
    {
        throw new NotImplementedException();
    }
}
public class OptimizationCollection
{
    internal OptimizationCollection? next = null;
}