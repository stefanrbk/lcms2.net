using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public delegate bool OptimizationFn(Pipeline lut, Signature intent, Signature[] inputFormat, Signature[] outputFormat, uint[] flags);

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