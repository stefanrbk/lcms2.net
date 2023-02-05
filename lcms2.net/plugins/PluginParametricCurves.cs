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
using lcms2.types;

namespace lcms2.plugins;

public delegate double CmsParametricCurveEvaluator(int Type, ReadOnlySpan<double> Params, double R);

public class PluginParametricCurves : PluginBase
{
    public uint NumFunctions { get; internal set; }
    public int[] FunctionTypes { get; } = new int[MaxTypesInPlugin];
    public uint[] ParameterCount { get; } = new uint[MaxTypesInPlugin];

    public CmsParametricCurveEvaluator Evaluator { get; internal set; }

    public PluginParametricCurves(
        uint expectedVersion,
        Signature magic,
        Signature type,
        uint numFunctions,
        ReadOnlySpan<int> functionTypes,
        ReadOnlySpan<uint> parameterCount,
        CmsParametricCurveEvaluator evaluator)

        : base(expectedVersion, magic, type)
    {
        NumFunctions = numFunctions;
        functionTypes.CopyTo(FunctionTypes);
        parameterCount.CopyTo(ParameterCount);
        Evaluator = evaluator;
    }
}

internal class CmsParametricCurvesCollection
{
    public uint NumFunctions;
    public int[] FunctionTypes = new int[MaxTypesInPlugin];
    public uint[] ParameterCount = new uint[MaxTypesInPlugin];
    public CmsParametricCurveEvaluator Evaluator;

    public CmsParametricCurvesCollection? Next;

    public CmsParametricCurvesCollection(
        uint numFunctions,
        ReadOnlySpan<int> functionTypes,
        ReadOnlySpan<uint> parameterCount,
        CmsParametricCurveEvaluator evaluator)
    {
        NumFunctions = numFunctions;
        functionTypes.CopyTo(FunctionTypes);
        parameterCount.CopyTo(ParameterCount);
        Evaluator = evaluator;
    }
}
