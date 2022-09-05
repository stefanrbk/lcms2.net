//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
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
public sealed class OptimizationPlugin : Plugin
{
    #region Fields

    public OptimizationFn Function;

    #endregion Fields

    #region Public Constructors

    public OptimizationPlugin(Signature magic, uint expectedVersion, Signature type, OptimizationFn function)
        : base(magic, expectedVersion, type)
    {
        Function = function;
    }

    #endregion Public Constructors

    #region Internal Methods

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

    #endregion Internal Methods
}

internal class OptimizationCollection
{
    #region Fields

    internal OptimizationCollection? next;

    internal OptimizationFn optimizePtr;

    #endregion Fields

    #region Public Constructors

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

    #endregion Public Constructors
}

internal sealed class OptimizationPluginChunk
{
    #region Fields

    internal static OptimizationPluginChunk global = new();
    internal OptimizationCollection? optimizationCollection;

    #endregion Fields

    #region Private Constructors

    private OptimizationPluginChunk()
    { }

    #endregion Private Constructors

    #region Properties

    internal static OptimizationPluginChunk Default => new();

    #endregion Properties
}
