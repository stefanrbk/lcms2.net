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

using lcms2.state;
using lcms2.types;

using System.Runtime.CompilerServices;

using static System.Math;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private const ushort maxNodesInCurve = 4097;
    private const float MINUS_INF = -1e22f;
    private const float PLUS_INF = 1e22f;

    private static readonly int[] defaultCurvesFunctionTypes = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 108, 109 };
    private static readonly uint[] defaultCurvesParameterCounts = new uint[] { 1, 3, 4, 5, 7, 4, 5, 5, 1, 1 };

    private static readonly ParametricCurvesCollection defaultCurves = new()
    {
        Evaluator = DefaultEvalParametricFn,
        nFunctions = 10,
    };

    internal static readonly CurvesPluginChunkType CurvesPluginChunk = new();

    internal static readonly CurvesPluginChunkType globalCurvePluginChunk = new();

    /// <summary>
    ///     Duplicates the plug-in in the new context.
    /// </summary>
    private static void DupPluginCurvesList(Context ctx, in Context src)
    {
        var head = src.CurvesPlugin;
        ParametricCurvesCollection? Anterior = null;
        var newHead = new CurvesPluginChunkType();

        // Walk the list copying all nodes
        for (var entry = head.ParametricCurves;
            entry is not null;
            entry = entry.Next)
        {
            var newEntry = (ParametricCurvesCollection?)entry.Clone();

            if (newEntry is null)
                return;

            // We want to keep the linked list order, so this is a little bit tricky
            newEntry.Next = null;
            if (Anterior is not null)
                Anterior.Next = newEntry;

            Anterior = newEntry;

            newHead.ParametricCurves ??= newEntry;
        }

        ctx.CurvesPlugin = newHead;
    }

    internal static void _cmsAllocCurvesPluginChunk(Context ctx, in Context? src)
    {
        _cmsAssert(ctx);

        var from = src is not null
            ? src.CurvesPlugin
            : CurvesPluginChunk;

        ctx.CurvesPlugin = (CurvesPluginChunkType)from.Dup(ctx);

        //fixed (CurvesPluginChunkType* @default = &CurvesPluginChunk)
        //    AllocPluginChunk(ctx, src, DupPluginCurvesList, Chunks.CurvesPlugin, @default);
    }

    internal static bool _cmsRegisterParametricCurvesPlugin(Context? ContextID, PluginBase? Data)
    {
        var ctx = _cmsGetContext(ContextID).CurvesPlugin;
        var Plugin = (PluginParametricCurves?)Data;

        if (Data is null)
        {
            ctx.ParametricCurves = null;
            return true;
        }

        var fl = new ParametricCurvesCollection();
        if (fl == null) return false;

        // Copy the parameters
        fl.Evaluator = Plugin!.Evaluator;
        fl.nFunctions = Plugin.NumFunctions;

        // Make sure no mem overwrites
        if (fl.nFunctions > MAX_TYPES_IN_LCMS_PLUGIN)
        {
            fl.nFunctions = MAX_TYPES_IN_LCMS_PLUGIN;
        }

        // Copy the data
        for (var i = 0; i < fl.nFunctions; i++)
            fl.FunctionTypes[i] = (int)Plugin.FunctionTypes[i];
        for (var i = 0; i < fl.nFunctions; i++)
            fl.ParameterCount[i] = Plugin.ParameterCount[i];
        //memcpy(fl->FunctionTypes, Plugin.FunctionTypes, fl->nFunctions * sizeof(uint));
        //memcpy(fl->ParameterCount, Plugin.ParameterCount, fl->nFunctions * sizeof(uint));

        // Keep linked list
        fl.Next = ctx.ParametricCurves;
        ctx.ParametricCurves = fl;

        // All is ok
        return true;
    }

    private static int IsInSet(int Type, ParametricCurvesCollection c)
    {
        for (var i = 0; i < c.nFunctions; i++)
            if (Abs(Type) == c.FunctionTypes[i]) return i;

        return -1;
    }

    private static ParametricCurvesCollection? GetParametricCurveByType(Context? ContextID, int Type, int* index)
    {
        int Position;
        var ctx = _cmsGetContext(ContextID).CurvesPlugin;

        for (var c = ctx.ParametricCurves; c is not null; c = c.Next)
        {
            Position = IsInSet(Type, c);

            if (Position is not -1)
            {
                if (index is not null)
                    *index = Position;
                return c;
            }
        }

        // If none found, revert for defaults
        for (var c = defaultCurves; c is not null; c = c.Next)
        {
            Position = IsInSet(Type, c);

            if (Position is not -1)
            {
                if (index is not null)
                    *index = Position;
                return c;
            }
        }
        return null;
    }

    private static ToneCurve* AllocateToneCurveStruct(
        Context? ContextID, uint nEntries,
        uint nSegments, in CurveSegment* Segments,
        in ushort* Values)
    {
        // We allow huge tables, which are then restricted for smoothing operations
        if (nEntries > 65530)
        {
            cmsSignalError(ContextID, ErrorCode.Range, "Couldn't create tone curve of more than 65530 entries");
            return null;
        }

        if (nEntries == 0 && nSegments == 0)
        {
            cmsSignalError(ContextID, ErrorCode.Range, "Couldn't create tone curve with zero segments and no table");
            return null;
        }

        // Allocate all required pointers, etc
        var p = _cmsMallocZero<ToneCurve>(ContextID);
        if (p is null) return null;

        // In this case, there are no segments
        if (nSegments is 0)
        {
            p->Segments = null;
            p->Evals = null;
        }
        else
        {
            p->Segments = _cmsCalloc<CurveSegment>(ContextID, nSegments);
            if (p->Segments is null) goto Error;

            p->Evals = (ParametricCurveEvaluator*)_cmsCalloc<nint>(ContextID, nSegments);
            if (p->Evals is null) goto Error;
        }

        p->nSegments = nSegments;

        // This 16-bit table contains a limited precision representation of the whole curve and is kept for
        // increasing xput on certain operations.
        if (nEntries is 0)
        {
            p->Table16 = null;
        }
        else
        {
            p->Table16 = _cmsCalloc<ushort>(ContextID, nEntries);
            if (p->Table16 is null) goto Error;
        }

        p->nEntries = nEntries;

        // Initialize members if requested
        if (Values is not null && (nEntries > 0))
        {
            for (var i = 0; i < nEntries; i++)
            {
                p->Table16[i] = Values[i];
            }
        }

        // Initialize the segments stuff. The evaluator for each segment is located and a pointer to it
        // is placed in advance to maximize performance.
        if (Segments is not null && (nSegments > 0))
        {
            p->SegInterp = _cmsCalloc2<InterpParams>(ContextID, nSegments);

            for (var i = 0; i < nSegments; i++)
            {
                // Type 0 is a special marker for table-based curves
                if (Segments[i].Type == 0)
                    p->SegInterp[i] = _cmsComputeInterpParams(ContextID, Segments[i].nGridPoints, 1, 1, null, LerpFlag.Float);

                memcpy(&p->Segments[i], &Segments[i], _sizeof<CurveSegment>());

                p->Segments[i].SampledPoints = Segments[i].Type == 0 && Segments[i].SampledPoints is not null
                    ? _cmsDupMem<float>(ContextID, Segments[i].SampledPoints, Segments[i].nGridPoints)
                    : (float*)null;

                var c = GetParametricCurveByType(ContextID, Segments[i].Type, null);
                if (c is not null)
                    p->Evals[i] = c.Evaluator;
            }
        }

        p->InterpParams = _cmsComputeInterpParams(ContextID, p->nEntries, 1, 1, p->Table16, LerpFlag.Ushort);
        if (p->InterpParams is not null)
            return p;
        Error:
        if (p->SegInterp is not null) _cmsFree(ContextID, p->SegInterp);
        if (p->Segments is not null) _cmsFree(ContextID, p->Segments);
        if (p->Evals is not null) _cmsFree(ContextID, p->Evals);
        if (p->Table16 is not null) _cmsFree(ContextID, p->Table16);
        _cmsFree(ContextID, p);
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double sigmoid_base(double k, double t) =>
        (1.0 / (1.0 + Exp(-k * t))) - 0.5;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double inverted_sigmoid_base(double k, double t) =>
        -Log((1.0 / (t + 0.5)) - 1.0) / k;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double sigmoid_factory(double k, double t)
    {
        var correction = 0.5 / sigmoid_base(k, 1);

        return (correction * sigmoid_base(k, (2.0 * t) - 1.0)) + 0.5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double inverse_sigmoid_factory(double k, double t)
    {
        var correction = 0.5 / sigmoid_base(k, 1);

        return (inverted_sigmoid_base(k, (t - 0.5) / correction) + 1.0) / 2.0;
    }

    private static double DefaultEvalParametricFn(int Type, in double* Params, double R)
    {
        double val, disc, e;

        switch (Type)
        {
            // X = Y ^ Gamma
            case 1:

                val = R < 0
                    ? Abs(Params[0] - 1) < MATRIX_DET_TOLERANCE
                        ? R
                        : 0
                    : Pow(R, Params[0]);

                break;

            // Type 1 Reversed
            // X = Y ^ 1/Gamma
            case -1:

                val = R < 0
                    ? Abs(Params[0] - 1) < MATRIX_DET_TOLERANCE
                        ? R
                        : 0
                    : Abs(Params[0]) < MATRIX_DET_TOLERANCE
                        ? PLUS_INF
                        : Pow(R, 1 / Params[0]);

                break;

            // CIE 122-1966
            // Y = (aX + b)^Gamma | X ≥ -b/a
            // Y = 0              | else
            case 2:

                if (Abs(Params[1]) < MATRIX_DET_TOLERANCE)
                {
                    val = 0;
                }
                else
                {
                    disc = -Params[2] / Params[1];

                    if (R >= disc)
                    {
                        e = (Params[1] * R) + Params[2];

                        val = e > 0
                            ? Pow(e, Params[0])
                            : 0;
                    }
                    else
                    {
                        val = 0;
                    }
                }

                break;

            // Type 2 Reversed
            // X = (Y ^1/g - b) / a
            case -2:

                val = Abs(Params[0]) < MATRIX_DET_TOLERANCE || Abs(Params[1]) < MATRIX_DET_TOLERANCE
                    ? 0
                    : R < 0
                        ? 0
                        : Max((Pow(R, 1.0 / Params[0]) - Params[2]) / Params[1], 0); // Max is the same as "if (val < 0)" check

                break;

            // IEC 61966-3
            // Y = (aX + b)^Gamma | X ≤ -b/a
            // Y = c              | else
            case 3:

                if (Abs(Params[1]) < MATRIX_DET_TOLERANCE)
                {
                    val = 0;
                }
                else
                {
                    disc = Max(-Params[2] / Params[1], 0);

                    if (R >= disc)
                    {
                        e = (Params[1] * R) + Params[2];

                        val = e > 0
                            ? Pow(e, Params[0]) + Params[3]
                            : 0;
                    }
                    else
                    {
                        val = Params[3];
                    }
                }

                break;

            // Type 3 reversed
            // X = ((Y-c)^1/g - b)/a | Y ≥ c
            // X = -b/a              | Y < c
            case -3:

                if (Abs(Params[1]) < MATRIX_DET_TOLERANCE)
                {
                    val = 0;
                }
                else
                {
                    if (R >= Params[3])
                    {
                        e = R - Params[3];

                        val = e > 0
                            ? (Pow(e, 1 / Params[0]) - Params[2]) / Params[1]
                            : 0;
                    }
                    else
                    {
                        val = -Params[2] / Params[1];
                    }
                }

                break;

            // IEC 61966-2.1 (sRGB)
            // Y = (aX + b)^Gamma | X ≥ d
            // Y = cX             | X < d
            case 4:

                if (R >= Params[4])
                {
                    e = (Params[1] * R) + Params[2];

                    val = e > 0
                        ? Pow(e, Params[0])
                        : 0;
                }
                else
                {
                    val = R * Params[3];
                }

                break;

            // Type 4 reversed
            // X = ((Y^1/g-b)/a) | Y ≥ (ad+b)^g
            // X = Y/c           | Y < (ad+b)^g
            case -4:

                e = (Params[1] * Params[4]) + Params[2];
                disc = e < 0
                    ? 0
                    : Pow(e, Params[0]);

                val = R >= disc
                    ? Abs(Params[0]) < MATRIX_DET_TOLERANCE || Abs(Params[1]) < MATRIX_DET_TOLERANCE
                        ? 0
                        : (Pow(R, 1.0 / Params[0]) - Params[2]) / Params[1]
                    : Abs(Params[3]) < MATRIX_DET_TOLERANCE
                        ? 0
                        : R / Params[3];

                break;

            // Y = (aX + b)^Gamma + e | X ≥ d
            // Y = cX + f             | X < d
            case 5:

                if (R >= Params[4])
                {
                    e = (Params[1] * R) + Params[2];

                    val = e > 0
                        ? Pow(e, Params[0]) + Params[5]
                        : Params[5];
                }
                else
                {
                    val = (R * Params[3]) + Params[6];
                }

                break;

            // Reversed type 5
            // X = ((Y-e)1/g-b)/a | Y ≥ (ad+b)^g+e), cd+f
            // X = (Y-f)/c        | else
            case -5:

                disc = (Params[3] * Params[4]) + Params[6];
                if (R >= disc)
                {
                    e = R - Params[5];
                    val = e < 0
                        ? 0
                        : Abs(Params[0]) < MATRIX_DET_TOLERANCE || Abs(Params[1]) < MATRIX_DET_TOLERANCE
                            ? 0
                            : (Pow(e, 1.0 / Params[0]) - Params[2]) / Params[1];
                }
                else
                {
                    val = Abs(Params[3]) < MATRIX_DET_TOLERANCE
                        ? 0
                        : (R - Params[6]) / Params[3];
                }

                break;

            // Types 6,7,8 comes from segmented curves as described in ICCSpecRevision_02_11_06_Float.pdf
            // Type 6 is basically identical to type 5 without d

            // Y = (a * X + b) ^ Gamma + c
            case 6:

                e = (Params[1] * R) + Params[2];

                val = e < 0
                    ? Params[3]
                    : Pow(e, Params[0]) + Params[3];

                break;

            // X = ((Y - c) ^1/Gamma - b) / a
            case -6:

                if (Abs(Params[1]) < MATRIX_DET_TOLERANCE)
                {
                    val = 0;
                }
                else
                {
                    e = R - Params[3];
                    val = e < 0
                        ? 0
                        : (Pow(e, 1.0 / Params[0]) - Params[2]) / Params[1];
                }

                break;

            // Y = a * log (b * X^Gamma + c) + d
            case 7:

                e = (Params[2] * Pow(R, Params[0])) + Params[3];
                val = e <= 0
                    ? Params[4]
                    : (Params[1] * Log10(e)) + Params[4];

                break;

            //                Y = a * log (b * X^Gamma + c) + d
            // b * X ^Gamma + c = (Y - d) / a = log(b * X ^Gamma + c) pow(10, (Y-d) / a)
            //                X = pow((pow(10, (Y-d) / a) - c) / b, 1/g)
            case -7:

                val = Abs(Params[0]) < MATRIX_DET_TOLERANCE || Abs(Params[1]) < MATRIX_DET_TOLERANCE || Abs(Params[2]) < MATRIX_DET_TOLERANCE
                    ? 0
                    : Pow((Pow(10.0, (R - Params[4]) / Params[1]) - Params[3]) / Params[2], 1.0 / Params[0]);

                break;

            //Y = a * b^(c*X+d) + e
            case 8:

                val = (Params[0] * Pow(Params[1], (Params[2] * R) + Params[3])) + Params[4];

                break;

            // Y = (log((y-e) / a) / log(b) - d ) / c
            // a = p0, b = p1, c = p2, d = p3, e = p4,
            case -8:

                disc = R - Params[4];
                val = disc < 0
                    ? 0
                    : Abs(Params[0]) < MATRIX_DET_TOLERANCE || Abs(Params[2]) < MATRIX_DET_TOLERANCE
                        ? 0
                        : ((Log(disc / Params[0]) / Log(Params[1])) - Params[3]) / Params[2];

                break;

            // S-Shaped: (1 - (1-x)^1/g)^1/g
            case 108:

                val = Abs(Params[0]) < MATRIX_DET_TOLERANCE
                    ? 0
                    : Pow(1.0 - Pow(1 - R, 1 / Params[0]), 1 / Params[0]);

                break;

            //         Y = (1 - (1-X)^1/g)^1/g
            //       Y^g = (1 - (1-X)^1/g)
            //   1 - Y^g = (1-X)^1/g
            // (1-X)^1/g = 1 - Y^g
            //     1 - X = (1 - Y^g)^g
            //         X = 1 - (1 - Y^g)^g
            case -108:

                val = 1 - Pow(1 - Pow(R, Params[0]), Params[0]);

                break;

            // Sigmoidals
            case 109:

                val = sigmoid_factory(Params[0], R);

                break;

            case -109:

                val = inverse_sigmoid_factory(Params[0], R);

                break;

            default:
                // Unsupported parametric curve. Should never reach here
                return 0;
        }

        return val;
    }

    private static double EvalSegmentedFn(in ToneCurve* g, double R)
    {
        float Out32;
        double Out;

        for (var i = (int)g->nSegments - 1; i >= 0; i--)
        {
            // Check for domain
            if ((R > g->Segments[i].x0) && (R <= g->Segments[i].x1))
            {
                // Type == 0 means segment is sampled
                if (g->Segments[i].Type == 0)
                {
                    var R1 = (float)(R - g->Segments[i].x0) / (g->Segments[i].x1 - g->Segments[i].x0);

                    // Setup the table (TODO: clean that)
                    g->SegInterp[i]->Table = g->Segments[i].SampledPoints;

                    g->SegInterp[i]->Interpolation.LerpFloat(&R1, &Out32, g->SegInterp[i]);
                    Out = Out32;
                }
                else
                {
                    Out = g->Evals[i](g->Segments[i].Type, g->Segments[i].Params, R);
                }

                if (Double.IsPositiveInfinity(Out))
                    return PLUS_INF;
                else if (Double.IsNegativeInfinity(Out))
                    return MINUS_INF;

                return Out;
            }
        }

        return MINUS_INF;
    }

    public static uint cmsGetToneCurveEstimatedTableEntries(in ToneCurve* t)
    {
        _cmsAssert(t);
        return t->nEntries;
    }

    public static ushort* cmsGetToneCurveEstimatedTable(in ToneCurve* t)
    {
        _cmsAssert(t);
        return t->Table16;
    }

    public static ToneCurve* cmsBuildTabulatedToneCurve16(Context? ContextID, uint nEntries, in ushort* Values) =>
        AllocateToneCurveStruct(ContextID, nEntries, 0, null, Values);

    private static uint EntriesByGamma(double Gamma) =>
        (Abs(Gamma - 1.0) < 0.001) ? 2u : 4096u;

    public static ToneCurve* cmsBuildSegmentedToneCurve(Context? ContextID, uint nSegments, in CurveSegment* Segments)
    {
        var nGridPoints = 4096u;

        _cmsAssert(Segments);

        // Optimization for identity curves.
        if (nSegments is 1 && Segments[0].Type is 1)
            nGridPoints = EntriesByGamma(Segments[0].Params[0]);

        var g = AllocateToneCurveStruct(ContextID, nGridPoints, nSegments, Segments, null);
        if (g is null) return null;

        // Once we have the floating point version, we can approximate a 16 bit table of 4096 entries
        // for performance reasons. This table would normally not be used except on 8/16 bit transforms.
        for (var i = 0; i < nGridPoints; i++)
        {
            var R = (double)i / (nGridPoints - 1);

            var Val = EvalSegmentedFn(g, R);

            // Round and saturate
            g->Table16[i] = _cmsQuickSaturateWord(Val * 65535.0);
        }

        return g;
    }

    public static ToneCurve* cmsBuildTabulatedToneCurveFloat(Context? ContextID, uint nEntries, in float* values)
    {
        var Seg = stackalloc CurveSegment[3];

        // A segmented tone curve should have function segments in the first and last positions
        // Initialize segmented curve part up to 0 to constant value = samples[0]
        Seg[0].x0 = MINUS_INF;
        Seg[0].x1 = 0f;
        Seg[0].Type = 6;

        Seg[0].Params[0] = 1;
        Seg[0].Params[1] = 0;
        Seg[0].Params[2] = 0;
        Seg[0].Params[3] = values[0];
        Seg[0].Params[4] = 0;

        // From zero to 1
        Seg[1].x0 = 0f;
        Seg[1].x1 = 1f;
        Seg[1].Type = 0;

        Seg[1].nGridPoints = nEntries;
        Seg[1].SampledPoints = values;

        // Final segment is constant = lastsample
        Seg[2].x0 = 1f;
        Seg[2].x1 = PLUS_INF;
        Seg[2].Type = 6;

        Seg[2].Params[0] = 1;
        Seg[2].Params[1] = 0;
        Seg[2].Params[2] = 0;
        Seg[2].Params[3] = values[nEntries - 1];
        Seg[2].Params[4] = 0;

        return cmsBuildSegmentedToneCurve(ContextID, 3, Seg);
    }

    public static ToneCurve* cmsBuildParametricToneCurve(Context? ContextID, int Type, in double* Params)
    {
        CurveSegment Seg0;
        int Pos = 0;
        var c = GetParametricCurveByType(ContextID, Type, &Pos);

        _cmsAssert(Params);

        if (c is null)
        {
            cmsSignalError(ContextID, ErrorCode.UnknownExtension, $"Invalid parametric curve type {Type}");
            return null;
        }

        Seg0.x0 = MINUS_INF;
        Seg0.x1 = PLUS_INF;
        Seg0.Type = Type;

        var size = c.ParameterCount[Pos] * _sizeof<double>();
        memcpy(Seg0.Params, Params, size);

        return cmsBuildSegmentedToneCurve(ContextID, 1, &Seg0);
    }

    public static ToneCurve* cmsBuildGamma(Context? ContextID, double Gamma) =>
        cmsBuildParametricToneCurve(ContextID, 1, &Gamma);

    public static void cmsFreeToneCurve(ToneCurve* Curve)
    {
        if (Curve is null) return;

        var ContextID = Curve->InterpParams->ContextID;

        _cmsFreeInterpParams(Curve->InterpParams);

        if (Curve->Table16 is not null)
            _cmsFree(ContextID, Curve->Table16);

        if (Curve->Segments is not null)
        {
            for (var i = 0; i < Curve->nSegments; i++)
            {
                if (Curve->Segments[i].SampledPoints is not null)
                    _cmsFree(ContextID, Curve->Segments[i].SampledPoints);

                if (Curve->SegInterp[i] is not null)
                    _cmsFreeInterpParams(Curve->SegInterp[i]);
            }

            _cmsFree(ContextID, Curve->Segments);
            _cmsFree(ContextID, Curve->SegInterp);
        }

        if (Curve->Evals is not null)
            _cmsFree(ContextID, Curve->Evals);

        _cmsFree(ContextID, Curve);
    }

    public static void cmsFreeToneCurveTriple(ToneCurve** Curve)
    {
        _cmsAssert(Curve);

        if (Curve[0] is not null) cmsFreeToneCurve(Curve[0]);
        if (Curve[1] is not null) cmsFreeToneCurve(Curve[1]);
        if (Curve[2] is not null) cmsFreeToneCurve(Curve[2]);

        Curve[0] = Curve[1] = Curve[2] = null;
    }

    public static ToneCurve* cmsDupToneCurve(in ToneCurve* In)
    {
        if (In is null) return null;

        return AllocateToneCurveStruct(In->InterpParams->ContextID, In->nEntries, In->nSegments, In->Segments, In->Table16);
    }

    public static ToneCurve* cmsJoinToneCurve(Context? ContextID, in ToneCurve* X, in ToneCurve* Y, uint nResultingPoints)
    {
        ToneCurve* @out = null;
        ToneCurve* Yreversed = null;
        float* Res = null;

        _cmsAssert(X);
        _cmsAssert(Y);

        Yreversed = cmsReverseToneCurveEx(nResultingPoints, Y);
        if (Yreversed is null) goto Error;

        Res = _cmsCalloc<float>(ContextID, nResultingPoints);
        if (Res is null) goto Error;

        // Iterate
        for (var i = 0; i < nResultingPoints; i++)
        {
            var t = (float)i / (nResultingPoints - 1);
            var x = cmsEvalToneCurveFloat(X, t);
            Res[i] = cmsEvalToneCurveFloat(Yreversed, x);
        }

        // Allocate space for output
        @out = cmsBuildTabulatedToneCurveFloat(ContextID, nResultingPoints, Res);

    Error:
        if (Res is not null) _cmsFree(ContextID, Res);
        if (Yreversed is not null) cmsFreeToneCurve(Yreversed);

        return @out;
    }

    private static int GetInterval(double In, in ushort* LutTable, in InterpParams* p)
    {
        // A 1 point table is not allowed
        if (p->Domain[0] < 1) return -1;

        // Let's see if ascending or descending.
        if (LutTable[0] < LutTable[p->Domain[0]])
        {
            // Table is overall ascending
            for (var i = (int)p->Domain[0] - 1; i >= 0; --i)
            {
                var y0 = LutTable[i];
                var y1 = LutTable[i + 1];

                if (y0 <= y1)       // Increasing
                {
                    if (In >= y0 && In <= y1) return i;
                }
                else                // Decreasing
                    if (y1 < y0)
                {
                    if (In >= y1 && In <= y0) return i;
                }
            }
        }
        else
        {
            // Table is overall descending
            for (var i = 0; i < p->Domain[0]; i++)
            {
                var y0 = LutTable[i];
                var y1 = LutTable[i + 1];

                if (y0 <= y1)       // Increasing
                {
                    if (In >= y0 && In <= y1) return i;
                }
                else                // Decreasing
                    if (y1 < y0)
                {
                    if (In >= y1 && In <= y0) return i;
                }
            }
        }

        return -1;
    }

    public static ToneCurve* cmsReverseToneCurveEx(uint nResultingSamples, in ToneCurve* InCurve)
    {
        double a = 0, b = 0, y, x1, y1, x2, y2;

        _cmsAssert(InCurve);

        // Try to reverse it analytically whatever possible

        if (InCurve->nSegments is 1 && InCurve->Segments[0].Type > 0 &&
            /* InCurve->Segments[0].Type <= 5 */
            GetParametricCurveByType(InCurve->InterpParams->ContextID, InCurve->Segments[0].Type, null) is not null)
        {
            return cmsBuildParametricToneCurve(InCurve->InterpParams->ContextID,
                -InCurve->Segments[0].Type,
                InCurve->Segments[0].Params);
        }

        // Nope, reverse the table.
        var @out = cmsBuildTabulatedToneCurve16(InCurve->InterpParams->ContextID, nResultingSamples, null);
        if (@out is null) return null;

        // We want to know if this is an ascending or descending table
        var Ascending = !cmsIsToneCurveDescending(InCurve);

        // Iterate across Y axis
        for (var i = 0; i < nResultingSamples; i++)
        {
            y = i * 65535.0 / (nResultingSamples - 1);

            // Find interval in which y is within.
            var j = GetInterval(y, InCurve->Table16, InCurve->InterpParams);
            if (j >= 0)
            {
                // Get limits of interval
                x1 = InCurve->Table16[j];
                x2 = InCurve->Table16[j + 1];

                y1 = j * 65535.0 / (InCurve->nEntries - 1);
                y2 = (j + 1) * 65535.0 / (InCurve->nEntries - 1);

                // If collapsed, then use any
                if (x1 == x2)
                {
                    @out->Table16[i] = _cmsQuickSaturateWord(Ascending ? y2 : y1);
                    continue;
                }
                else
                {
                    // Interpolate
                    a = (y2 - y1) / (x2 - x1);
                    b = y2 - (a * x2);
                }
            }

            @out->Table16[i] = _cmsQuickSaturateWord((a * y) + b);
        }

        return @out;
    }

    public static ToneCurve* cmsReverseToneCurve(in ToneCurve* InGamma)
    {
        _cmsAssert(InGamma);

        return cmsReverseToneCurveEx(4096, InGamma);
    }

    private static bool smooth2(Context? ContextID, float* w, float* y,
                    float* z, float lambda, int m)
    {
        int i, i1, i2;
        float* c, d, e;
        bool st;

        c = (float*)_cmsCalloc(ContextID, maxNodesInCurve, _sizeof<float>());
        d = (float*)_cmsCalloc(ContextID, maxNodesInCurve, _sizeof<float>());
        e = (float*)_cmsCalloc(ContextID, maxNodesInCurve, _sizeof<float>());

        if (c is not null && d is not null && e is not null)
        {
            d[1] = w[1] + lambda;
            c[1] = -2 * lambda / d[1];
            e[1] = lambda / d[1];
            z[1] = w[1] * y[1];
            d[2] = w[2] + (5 * lambda) - (d[1] * c[1] * c[1]);
            c[2] = ((-4 * lambda) - (d[1] * c[1] * e[1])) / d[2];
            e[2] = lambda / d[2];
            z[2] = (w[2] * y[2]) - (c[1] * z[1]);

            for (i = 3; i < m - 1; i++)
            {
                i1 = i - 1; i2 = i - 2;
                d[i] = w[i] + (6 * lambda) - (c[i1] * c[i1] * d[i1]) - (e[i2] * e[i2] * d[i2]);
                c[i] = ((-4 * lambda) - (d[i1] * c[i1] * e[i1])) / d[i];
                e[i] = lambda / d[i];
                z[i] = (w[i] * y[i]) - (c[i1] * z[i1]) - (e[i2] * z[i2]);
            }

            i1 = m - 2; i2 = m - 3;

            d[m - 1] = w[m - 1] + (5 * lambda) - (c[i1] * c[i1] * d[i1]) - (e[i2] * e[i2] * d[i2]);
            c[m - 1] = ((-2 * lambda) - (d[i1] * c[i1] * e[i1])) / d[m - 1];
            z[m - 1] = (w[m - 1] * y[m - 1]) - (c[i1] * z[i1]) - (e[i2] * z[i2]);
            i1 = m - 1; i2 = m - 2;

            d[m] = w[m] + lambda - (c[i1] * c[i1] * d[i1]) - (e[i2] * e[i2] * d[i2]);
            z[m] = ((w[m] * y[m]) - (c[i1] * z[i1]) - (e[i2] * z[i2])) / d[m];
            z[m - 1] = (z[m - 1] / d[m - 1]) - (c[m - 1] * z[m]);

            for (i = m - 2; 1 <= i; i--)
                z[i] = (z[i] / d[i]) - (c[i] * z[i + 1]) - (e[i] * z[i + 2]);

            st = true;
        }
        else
        {
            st = false;
        }

        if (c is not null) _cmsFree(ContextID, c);
        if (d is not null) _cmsFree(ContextID, d);
        if (e is not null) _cmsFree(ContextID, e);

        return st;
    }

    public static bool cmsSmoothToneCurve(ToneCurve* Tab, double lambda)
    {
        bool SuccessStatus = true, notCheck = false;
        float* w, y, z;
        uint i, nItems, Zeros, Poles;

        if (Tab is null || Tab->InterpParams is null)
        {
            // Can't signal an error here since ContextID is not known at this point
            return false;
        }

        var ContextID = Tab->InterpParams->ContextID;

        if (cmsIsToneCurveLinear(Tab)) // Only non-linear curves need smoothing
            return SuccessStatus;

        nItems = Tab->nEntries;
        if (nItems >= maxNodesInCurve)    // too many items in the table
        {
            cmsSignalError(ContextID, ErrorCode.Range, "cmsSmoothToneCurve: Too many points.");
            return false;
        }

        // Allocate one more item than needed
        w = _cmsCalloc<float>(ContextID, nItems + 1);
        y = _cmsCalloc<float>(ContextID, nItems + 1);
        z = _cmsCalloc<float>(ContextID, nItems + 1);

        if (w is null || y is null || z is null) // Ensure no memory allocation failure
        {
            cmsSignalError(ContextID, ErrorCode.Range, "cmsSmoothToneCurve: Could not allocate memory.");
            return false;
        }

        memset(w, 0, (nItems + 1) * _sizeof<float>());
        memset(y, 0, (nItems + 1) * _sizeof<float>());
        memset(z, 0, (nItems + 1) * _sizeof<float>());

        for (i = 0; i < nItems; i++)
        {
            y[i + 1] = Tab->Table16[i];
            w[i + 1] = 1f;
        }

        if (lambda < 0)
        {
            notCheck = true;
            lambda = -lambda;
        }

        if (!smooth2(ContextID, w, y, z, (float)lambda, (int)nItems))    // Could not smooth
        {
            cmsSignalError(ContextID, ErrorCode.Range, "cmsSmoothToneCurve: Function smooth2 failed.");
            return false;
        }

        // Do some reality - checking...

        Zeros = Poles = 0;
        for (i = nItems; i > 1; --i)
        {
            if (z[i] == 0) Zeros++;
            if (z[i] >= 65535) Poles++;
            if (z[i] < z[i - 1])
            {
                cmsSignalError(ContextID, ErrorCode.Range, "cmsSmoothToneCurve: Non-Monotonic.");
                SuccessStatus = notCheck;
                break;
            }
        }

        if (!SuccessStatus)
            goto Done;

        if (Zeros > (nItems / 3))
        {
            cmsSignalError(ContextID, ErrorCode.Range, "cmsSmoothToneCurve: Degenerated, mostly zeros.");
            SuccessStatus = notCheck;
            goto Done;
        }

        if (Poles > (nItems / 3))
        {
            cmsSignalError(ContextID, ErrorCode.Range, "cmsSmoothToneCurve: Degenerated, mostly poles.");
            SuccessStatus = notCheck;
            goto Done;
        }

        for (i = 0; i < nItems; ++i)
        {
            // Clamp to ushort
            Tab->Table16[i] = _cmsQuickSaturateWord(z[i + 1]);
        }

    Done:
        if (z is not null) _cmsFree(ContextID, z);
        if (y is not null) _cmsFree(ContextID, y);

        if (w is not null) _cmsFree(ContextID, w);

        return SuccessStatus;
    }

    public static bool cmsIsToneCurveLinear(in ToneCurve* Curve)
    {
        _cmsAssert(Curve);

        for (var i = 0; i < Curve->nEntries; i++)
        {
            var diff = Abs(Curve->Table16[i] - _cmsQuantizeVal(i, Curve->nEntries));
            if (diff > 0x0f)
                return false;
        }

        return true;
    }

    public static bool cmsIsToneCurveMonotonic(in ToneCurve* t)
    {
        _cmsAssert(t);

        // Degenerated curves are monotonic? Ok, let's pass them
        var n = t->nEntries;
        if (n < 2) return true;

        // Curve direction
        var lDescending = cmsIsToneCurveDescending(t);

        if (lDescending)
        {
            var last = t->Table16[0];

            for (var i = 1; i < n; i++)
            {
                if (t->Table16[i] - last > 2)   // We allow some ripple
                    return false;
                else
                    last = t->Table16[i];
            }
        }
        else
        {
            var last = t->Table16[n - 1];

            for (var i = (int)n - 2; i >= 0; --i)
            {
                if (t->Table16[i] - last > 2)
                    return false;
                else
                    last = t->Table16[i];
            }
        }

        return true;
    }

    public static bool cmsIsToneCurveDescending(in ToneCurve* t)
    {
        _cmsAssert(t);

        return t->Table16[0] > t->Table16[t->nEntries - 1];
    }

    public static bool cmsIsToneMultisegment(in ToneCurve* t)
    {
        _cmsAssert(t);

        return t->nSegments > 1;
    }

    public static int cmsGetToneCurveParametricType(in ToneCurve* t)
    {
        _cmsAssert(t);

        if (t->nSegments is not 1) return 0;
        return t->Segments[0].Type;
    }

    public static float cmsEvalToneCurveFloat(in ToneCurve* Curve, float v)
    {
        _cmsAssert(Curve);

        // Check for 16 bit table. If so, this is a limited-precision tone curve
        if (Curve->nSegments is 0)
        {
            var In = _cmsQuickSaturateWord(v * 65535.0);
            var Out = cmsEvalToneCurve16(Curve, In);

            return (float)(Out / 65535.0);
        }

        return (float)EvalSegmentedFn(Curve, v);
    }

    public static ushort cmsEvalToneCurve16(in ToneCurve* Curve, ushort v)
    {
        ushort @out;

        _cmsAssert(Curve);

        Curve->InterpParams->Interpolation.Lerp16(&v, &@out, Curve->InterpParams);
        return @out;
    }

    public static double cmsEstimateGamma(in ToneCurve* t, double Precision)
    {
        _cmsAssert(t);

        double sum = 0, sum2 = 0, n = 0;

        // Excluding endpoints
        for (var i = 1; i < (maxNodesInCurve - 1); i++)
        {
            var x = (double)i / (maxNodesInCurve - 1);
            var y = (double)cmsEvalToneCurveFloat(t, (float)x);

            // Avoid 7% on lower part to prevent
            // artifacts due to linear ramps

            if (y > 0 && y < 1 && x > 0.07)
            {
                var gamma = Log(y) / Log(x);
                sum += gamma;
                sum2 += gamma * gamma;
                n++;
            }
        }

        // Take a look on SD to see if gamma isn't exponential at all
        var Std = Sqrt(((n * sum2) - (sum * sum)) / (n * (n - 1)));

        return (Std > Precision)
            ? -1.0
            : sum / n;   // The mean
    }

    public static double* cmsGetToneCurveParams(in ToneCurve* t)
    {
        _cmsAssert(t);

        return (t->nSegments is not 1)
            ? null
            : t->Segments[0].Params;
    }
}
