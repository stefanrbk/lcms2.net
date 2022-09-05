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
///     Parametric Curves
/// </summary>
/// <remarks>
///     A plugin may implement an arbitrary number of parametric curves. <br/> Implements the
///     <c>cmsPluginParametricCurves</c> struct.
/// </remarks>
public sealed class ParametricCurvesPlugin : Plugin
{
    #region Fields

    /// <summary>
    ///     The evaluator
    /// </summary>
    public ParametricCurveEvaluator Evaluator;

    public (int Types, int Count)[] Functions;

    #endregion Fields

    #region Public Constructors

    public ParametricCurvesPlugin(Signature magic, uint expectedVersion, Signature type, (int Types, int Count)[] functions, ParametricCurveEvaluator evaluator)
        : base(magic, expectedVersion, type)
    {
        Functions = functions;

        Evaluator = evaluator;
    }

    #endregion Public Constructors

    #region Internal Methods

    internal static bool RegisterPlugin(object? state, ParametricCurvesPlugin? plugin)
    {
        var ctx = State.GetCurvesPlugin(state);

        if (plugin is null)
        {
            ctx.parametricCurves = null;
            return true;
        }

        ctx.parametricCurves = new ParametricCurvesCollection(plugin.Functions, plugin.Evaluator, ctx.parametricCurves);

        return true;
    }

    #endregion Internal Methods
}

internal sealed class ParametricCurvesPluginChunk
{
    #region Fields

    internal static ParametricCurvesPluginChunk global = new();
    internal ParametricCurvesCollection? parametricCurves;

    #endregion Fields

    #region Private Constructors

    private ParametricCurvesPluginChunk()
    { }

    #endregion Private Constructors

    #region Properties

    internal static ParametricCurvesPluginChunk Default => new();

    #endregion Properties
}
