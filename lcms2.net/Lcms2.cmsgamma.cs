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

using lcms2.plugins;

namespace lcms2;

public static partial class Lcms2
{
    private const ushort maxNodesInCurve = 4097;
    private const float minusInf = -1e22f;
    private const float plusInf = 1e22f;

    private static CmsParametricCurvesCollection defaultCurves =>
        new(
            10,
            new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 108, 109 },
            new uint[] { 1, 3, 4, 5, 7, 4, 5, 5, 1, 1 },
            DefaultEvalParametricFn);

    internal static CurvesPluginChunkType defaultCurvePluginChunk =>
        new()
        {
            ParametricCurves = defaultCurves,
        };

    internal static readonly CurvesPluginChunkType globalCurvePluginChunk = defaultCurvePluginChunk;

    /// <summary>
    ///     Duplicates the plug-in in the new context.
    /// </summary>
    private static void DupPluginCurvesList(Context ctx, in Context src)
    {
        var newHead = new CurvesPluginChunkType();
        var head = (CurvesPluginChunkType)src.chunks[(int)Chunks.CurvesPlugin]!;
        CmsParametricCurvesCollection? Anterior = null;

        // Walk the list copying all nodes
        for (var entry = head.ParametricCurves;
            entry is not null;
            entry = entry.Next)
        {
            var newEntry = new CmsParametricCurvesCollection(
                entry.NumFunctions,
                entry.FunctionTypes,
                entry.ParameterCount,
                entry.Evaluator);

            // We want to keep the linked list order, so this is a little bit tricky
            if (Anterior is not null)
                Anterior.Next = newEntry;

            Anterior = newEntry;

            if (newHead.ParametricCurves is null)
                newHead.ParametricCurves = newEntry;
        }

        ctx.chunks[(int)Chunks.CurvesPlugin] = newHead;
    }

    internal static void _cmsAllocCurvesPluginChunk(Context ctx, Context? src = null)
    {
        if (src is not null)
        {
            // Copy all linked list
            DupPluginCurvesList(ctx, src);
        }
        else
        {
            ctx.chunks[(int)Chunks.CurvesPlugin] = defaultCurvePluginChunk;
        }
    }

    internal static bool _cmsRegisterParametricCurvesPlugin(Context? ContextID, PluginBase? Data)
    {
        var ctx = (CurvesPluginChunkType)_cmsContextGetClientChunk(ContextID, Chunks.CurvesPlugin)!;

        if (Data is PluginParametricCurves Plugin)
        {
            // Copy the parameters
            var fl = new CmsParametricCurvesCollection(
                Plugin.NumFunctions,
                Plugin.FunctionTypes,
                Plugin.ParameterCount,
                Plugin.Evaluator);

            // Make sure we don't have too many functions
            if (fl.NumFunctions > MaxTypesInPlugin)
                fl.NumFunctions = MaxTypesInPlugin;

            // Keep linked list
            fl.Next = ctx.ParametricCurves;
            ctx.ParametricCurves = fl;

            return true;
        }
        else
        {
            ctx.ParametricCurves = null;
            return true;
        }
    }

    private static double DefaultEvalParametricFn(int Type, ReadOnlySpan<double> Params, double R)
    {
        return 0;
    }
}
