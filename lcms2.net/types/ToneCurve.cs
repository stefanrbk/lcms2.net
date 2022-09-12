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

using System.Diagnostics;
using System.Runtime.CompilerServices;

using static System.Math;

namespace lcms2.types;

public delegate double ParametricCurveEvaluator(int type, in double[] @params, double r);

public sealed class ToneCurve : ICloneable, IDisposable
{
    #region Fields

    internal static ParametricCurvesCollection defaultCurves =
        /** Original Code (cmsgamma.c line: 60)
         **
         ** // The built-in list
         ** static _cmsParametricCurvesCollection DefaultCurves = {
         **     10,                                      // # of curve types
         **     { 1, 2, 3, 4, 5, 6, 7, 8, 108, 109 },    // Parametric curve ID
         **     { 1, 3, 4, 5, 7, 4, 5, 5,   1,   1 },    // Parameters by type
         **     DefaultEvalParametricFn,                 // Evaluator
         **     NULL                                     // Next in chain
         ** };
         **/

        new(
            new (int Types, int Count)[]
            {
                (1, 1),
                (2, 3),
                (3, 4),
                (4, 5),
                (5, 7),
                (6, 4),
                (7, 5),
                (8, 5),
                (108, 1),
                (109, 1),
            },
            DefaultEvalParametricFn,
            null);

    internal ParametricCurveEvaluator[] evals = Array.Empty<ParametricCurveEvaluator>();

    // Private optimizations for interpolation
    internal InterpParams? interpParams;

    internal InterpParams[] segInterp = Array.Empty<InterpParams>();

    internal CurveSegment[] segments = Array.Empty<CurveSegment>();

    internal ushort[] table16 = Array.Empty<ushort>();

    private bool _disposed;

    #endregion Fields

    #region Private Constructors

    private ToneCurve(CurveSegment[] segments, ParametricCurveEvaluator[] evals, InterpParams? interpParams = null)
    {
        this.interpParams = interpParams;
        this.segments = segments;
        this.evals = evals;
    }

    private ToneCurve(ushort[] table16, InterpParams? interpParams = null)
    {
        this.interpParams = interpParams;
        this.table16 = table16;
    }

    #endregion Private Constructors

    #region Properties

    public ushort[] EstimatedTable =>
                        table16;

    public bool IsDescending =>
           table16[0] > table16[NumEntries - 1];

    public bool IsLinear
    {
        get
        {
            for (var i = 0; i < NumEntries; i++)
            {
                var diff = Abs(table16[i] - QuantizeValue(i, NumEntries));
                if (diff > 0x0F)
                    return false;
            }

            return true;
        }
    }

    public bool IsMonotonic
    {
        get
        {
            // Degenerated curves are monotonic? Ok, let's pass them
            var n = NumEntries;
            if (n < 2) return true;

            // Curve direction
            var lDesc = IsDescending;

            if (lDesc)
            {
                var last = table16[0];

                for (var i = 1; i < n; i++)
                {
                    if (table16[i] - last > 2) // We allow some ripple
                        return false;
                    else
                        last = table16[i];
                }
            }
            else
            {
                var last = table16[n - 1];

                for (var i = n - 2; i >= 0; i--)
                {
                    if (table16[i] - last > 2)
                        return false;
                    else
                        last = table16[i];
                }
            }
            return true;
        }
    }

    public bool IsMultisegment =>
           NumSegments > 1;

    // 16 bit Table-based representation follows
    public uint NumEntries =>
        (uint)table16.Length;

    public int ParametricType =>
           NumSegments == 1
               ? segments[0].Type
                 : 0;

    public double[] Params =>
           segments[0].Params;

    internal int NumSegments =>
                     segments.Length;

    #endregion Properties

    #region Public Methods

    public static ToneCurve? BuildGamma(object? state, double gamma) =>
               BuildParametric(state, 1, gamma);

    public static ToneCurve? BuildParametric(object? state, int type, params double[] @params)
    {
        var c = ParametricCurvesCollection.GetByType(state, type, out var pos);

        if (c is null)
        {
            Errors.InvalidParametricCurveType(state, type);
            return null;
        }

        var seg0 = new CurveSegment()
        {
            X0 = minusInf,
            X1 = plusInf,
            Type = type,
        };

        var size = c.functions[pos].Count;
        if (@params.Length < size || @params.Length >= 10 /* seg0.Params.Length always = 10 */) return null;

        @params.AsSpan(..size).CopyTo(seg0.Params);

        return BuildSegmented(state, new CurveSegment[] { seg0 });
    }

    public static ToneCurve? BuildSegmented(object? state, CurveSegment[] segments)
    {
        var numSegments = segments.Length;
        var numGridPoints = 4096u;

        // Optimization for identity curves.
        if (numSegments == 1 && segments[0].Type == 1)
            numGridPoints = EntriesByGamma(segments[0].Params[0]);

        var g = Alloc(state, segments);
        if (g is null) return null;

        // Once we have the floating point version, we can approximate a 16 bit table of 4096
        // entries for performance reasons. This table would normally not be used except on 8/16 bit transforms.
        for (var i = 0; i < numGridPoints; i++)
        {
            var r = (double)i / (numGridPoints - 1);

            var val = g.EvalSegmentedFn(r);

            // Round and saturate
            g.table16[i] = QuickSaturateWord(val * 65535.0);
        }

        return g;
    }

    public static ToneCurve? BuildEmptyTabulated16(object? state, uint numEntries) =>
        BuildTabulated(state, new ushort[numEntries]);

    public static ToneCurve? BuildEmptyTabulatedFloat(object? state, uint numSamples) =>
        BuildTabulated(state, new float[numSamples]);

    public static ToneCurve? BuildTabulated(object? state, ReadOnlySpan<ushort> values) =>
        Alloc(state, values);

    public static ToneCurve? BuildTabulated(object? state, ReadOnlySpan<float> values)
    {
        var seg = new CurveSegment[3];

        // A segmented tone curve should have function segments in the first and last positions
        // Initialize segmented curve part up to 0 to constant value = samples[0]
        seg[0] = new()
        {
            X0 = minusInf,
            X1 = 0f,
            Type = 6,
        };
        seg[0].Params[0] = 1;
        seg[0].Params[1] = 0;
        seg[0].Params[2] = 0;
        seg[0].Params[3] = values[0];
        seg[0].Params[4] = 0;

        // From zero to 1
        seg[1] = new()
        {
            X0 = 0f,
            X1 = 1f,
            Type = 0,
            SampledPoints = values.ToArray(),
        };

        // Final segment is constant = lastsample
        seg[2] = new()
        {
            X0 = 1f,
            X1 = plusInf,
            Type = 6,
        };
        seg[2].Params[0] = 1;
        seg[2].Params[1] = 0;
        seg[2].Params[2] = 0;
        seg[2].Params[3] = values[^1];
        seg[2].Params[4] = 0;

        return BuildSegmented(state, seg);
    }

    public static void DisposeTriple(ToneCurve?[] curves)
    {
        Debug.Assert(curves.Length == 3);

        curves[0]?.Dispose();
        curves[1]?.Dispose();
        curves[2]?.Dispose();
    }

    public object Clone() =>
        table16.Length > 0
            ? Alloc(interpParams?.StateContainer, table16)!
            : segments.Length > 0
                ? Alloc(interpParams?.StateContainer, segments)!
                : null!;

    public void Dispose()
    {
        if (!_disposed)
        {
            table16 = null!;

            if (segments is not null)
            {
                for (var i = 0; i < NumSegments; i++)
                {
                    segments[i].SampledPoints = null;
                    segInterp[i] = null!;
                }
            }
            evals = null!;

            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }

    public double EstimateGamma(double precision)
    {
        var sum = 0d;
        var sum2 = 0d;
        var n = 0d;

        // Excluding endpoints
        for (var i = 1; i < maxNodesInCurve - 1; i++)
        {
            var x = (double)i / (maxNodesInCurve - 1);
            var y = (double)Eval((float)x);

            // Avoid 7% on lower part to prevent artifacts due to linear ramps
            if (y > 0 && y < 1 && x > 0.07)
            {
                var gamma = Log(y) / Log(x);
                sum += gamma;
                sum2 += gamma * gamma;
                n++;
            }
        }

        // Take a look on SD to see if gamma isn't exponential at all
        var std = Sqrt(((n * sum2) - (sum * sum)) / (n * (n - 1)));

        return (std > precision)
            ? -1d
            : sum / n; // The mean
    }

    public float Eval(float v)
    {
        // Check for 16 bit table. If so, this is a limited-precision tone curve
        if (NumSegments == 0)
        {
            var i = QuickSaturateWord(v * 65535.0);
            var o = Eval(i);

            return o / 65535.0f;
        }

        return (float)EvalSegmentedFn(v);
    }

    public ushort Eval(ushort v)
    {
        var i = new ushort[] { v };
        var o = new ushort[1];

        interpParams!.Interpolation?.Lerp(i, o, interpParams!);

        return o[0];
    }

    public ToneCurve? Join(object? state, ToneCurve Y, uint numResultingPoints)
    {
        var X = this;
        ToneCurve? result = null;

        var Yreversed = Y.Reverse(numResultingPoints);
        if (Yreversed is null) goto Error;

        var res = new float[numResultingPoints];

        // Iterate
        for (var i = 0; i < numResultingPoints; i++)
        {
            var t = (float)i / (numResultingPoints - 1);
            var x = X.Eval(t);
            res[i] = Yreversed.Eval(x);
        }

        result = BuildTabulated(state, res);

    Error:
        Yreversed?.Dispose();

        return result;
    }

    public ToneCurve? Reverse(uint numResultSamples)
    {
        var a = 0.0;
        var b = 0.0;

        // Try to reverse it analytically whatever possible

        if (NumSegments == 1 && segments[0].Type > 0 &&
            /* Segments[0].Type <= 5 */
            ParametricCurvesCollection.GetByType(interpParams?.StateContainer, segments[0].Type, out _) is not null)
        {
            return BuildParametric(interpParams?.StateContainer, -segments[0].Type, segments[0].Params);
        }

        var result = BuildEmptyTabulated16(interpParams?.StateContainer, numResultSamples);
        if (result is null)
            return null;

        // We want to know if this is an ascending or descending table
        var ascending = !IsDescending;

        // Iterate across Y axis
        for (var i = 0; i < numResultSamples; i++)
        {
            var y = i * 65535.0 / (numResultSamples - 1);

            // Find interval in which y is within.
            var j = GetInterval(y, table16, interpParams!);
            if (j >= 0)
            {
                // Get limits of interval
                var x1 = table16[j];
                var x2 = table16[j + 1];

                var y1 = j * 65535.0 / (NumEntries - 1);
                var y2 = (j + 1) * 65535.0 / (NumEntries - 1);

                // if collapsed, then use any
                if (x1 == x2)
                {
                    result.table16[i] = QuickSaturateWord(ascending ? y2 : y1);
                    continue;
                }
                else
                {
                    // Interpolate
                    a = (y2 - y1) / (x2 - x1);
                    b = y2 - (a * x2);
                }
            }

            result.table16[i] = QuickSaturateWord((a * y) + b);
        }

        return result;
    }

    public ToneCurve? Reverse() =>
           Reverse(4096);

    public bool Smooth(double lambda)
    {
        var successStatus = true;

        if (interpParams is not null)
        {
            var context = interpParams.StateContainer;

            if (!IsLinear)
            { // Only non-linear curves need smoothing
                var numItems = NumEntries;
                if (numItems < maxNodesInCurve)
                {
                    // Allocate one more item than needed
                    var w = new float[numItems + 1];
                    var y = new float[numItems + 1];
                    var z = new float[numItems + 1];

                    {
                        for (var i = 0; i < numItems; i++)
                        {
                            y[i + 1] = table16[i];
                            w[i + 1] = 1f;
                        }

                        var notCheck = false;

                        if (lambda < 0)
                        {
                            notCheck = true;
                            lambda = -lambda;
                        }

                        if (Smooth2(w, y, z, (float)lambda, (int)numItems))
                        {
                            // Do some reality checking...

                            var zeros = 0;
                            var poles = 0;
                            for (var i = numItems; i > 1; i--)
                            {
                                if (z[i] == 0f) zeros++;
                                if (z[i] >= 65535f) poles++;
                                if (z[i] < z[i - 1])
                                {
                                    Errors.ToneCurveSmoothNonMonotonic(context);
                                    successStatus = notCheck;
                                    break;
                                }
                            }

                            if (successStatus && zeros > (numItems / 3))
                            {
                                Errors.ToneCurveSmoothMostlyZeros(context);
                                successStatus = notCheck;
                            }

                            if (successStatus && poles > (numItems / 3))
                            {
                                Errors.ToneCurveSmoothMostlyPoles(context);
                                successStatus = notCheck;
                            }

                            if (successStatus)
                            { // Seems ok
                                for (var i = 0; i < numItems; i++)
                                    // Clamp to ushort
                                    table16[i] = QuickSaturateWord(z[i + 1]);
                            }
                        }
                        else
                        { // Could not smooth
                            Errors.ToneCurveSmoothFailed(context);
                            successStatus = false;
                        }
                    }
                }
                else
                {
                    Errors.ToneCurveSmoothTooManyPoints(context);
                    successStatus = false;
                }
            }
        }

        return successStatus;
    }

    #endregion Public Methods

    #region Internal Methods

    internal static ToneCurve? Alloc(object? state, ReadOnlySpan<CurveSegment> segments)
    {
        /** Original Code (cmsgamma.c line: 209)
         **
         ** // Low level allocate, which takes care of memory details. nEntries may be zero, and in this case
         ** // no optimization curve is computed. nSegments may also be zero in the inverse case, where only the
         ** // optimization curve is given. Both features simultaneously is an error
         ** static
         ** cmsToneCurve* AllocateToneCurveStruct(cmsContext ContextID, cmsUInt32Number nEntries,
         **                                       cmsUInt32Number nSegments, const cmsCurveSegment* Segments,
         **                                       const cmsUInt16Number* Values)
         ** {
         **     cmsToneCurve* p;
         **     cmsUInt32Number i;
         **
         **     // We allow huge tables, which are then restricted for smoothing operations
         **     if (nEntries > 65530) {
         **         cmsSignalError(ContextID, cmsERROR_RANGE, "Couldn't create tone curve of more than 65530 entries");
         **         return NULL;
         **     }
         **
         **     if (nEntries == 0 && nSegments == 0) {
         **         cmsSignalError(ContextID, cmsERROR_RANGE, "Couldn't create tone curve with zero segments and no table");
         **         return NULL;
         **     }
         **
         **     // Allocate all required pointers, etc.
         **     p = (cmsToneCurve*) _cmsMallocZero(ContextID, sizeof(cmsToneCurve));
         **     if (!p) return NULL;
         **
         **     // In this case, there are no segments
         **     if (nSegments == 0) {
         **         p ->Segments = NULL;
         **         p ->Evals = NULL;
         **     }
         **     else {
         **         p ->Segments = (cmsCurveSegment*) _cmsCalloc(ContextID, nSegments, sizeof(cmsCurveSegment));
         **         if (p ->Segments == NULL) goto Error;
         **
         **         p ->Evals    = (cmsParametricCurveEvaluator*) _cmsCalloc(ContextID, nSegments, sizeof(cmsParametricCurveEvaluator));
         **         if (p ->Evals == NULL) goto Error;
         **     }
         **
         **     p -> nSegments = nSegments;
         **
         **     // This 16-bit table contains a limited precision representation of the whole curve and is kept for
         **     // increasing xput on certain operations.
         **     if (nEntries == 0) {
         **         p ->Table16 = NULL;
         **     }
         **     else {
         **        p ->Table16 = (cmsUInt16Number*)  _cmsCalloc(ContextID, nEntries, sizeof(cmsUInt16Number));
         **        if (p ->Table16 == NULL) goto Error;
         **     }
         **
         **     p -> nEntries  = nEntries;
         **
         **     // Initialize members if requested
         **     if (Values != NULL && (nEntries > 0)) {
         **
         **         for (i=0; i < nEntries; i++)
         **             p ->Table16[i] = Values[i];
         **     }
         **
         **     // Initialize the segments stuff. The evaluator for each segment is located and a pointer to it
         **     // is placed in advance to maximize performance.
         **     if (Segments != NULL && (nSegments > 0)) {
         **
         **         _cmsParametricCurvesCollection *c;
         **
         **         p ->SegInterp = (cmsInterpParams**) _cmsCalloc(ContextID, nSegments, sizeof(cmsInterpParams*));
         **         if (p ->SegInterp == NULL) goto Error;
         **
         **         for (i=0; i < nSegments; i++) {
         **
         **             // Type 0 is a special marker for table-based curves
         **             if (Segments[i].Type == 0)
         **                 p ->SegInterp[i] = _cmsComputeInterpParams(ContextID, Segments[i].nGridPoints, 1, 1, NULL, CMS_LERP_FLAGS_FLOAT);
         **
         **             memmove(&p ->Segments[i], &Segments[i], sizeof(cmsCurveSegment));
         **
         **             if (Segments[i].Type == 0 && Segments[i].SampledPoints != NULL)
         **                 p ->Segments[i].SampledPoints = (cmsFloat32Number*) _cmsDupMem(ContextID, Segments[i].SampledPoints, sizeof(cmsFloat32Number) * Segments[i].nGridPoints);
         **             else
         **                 p ->Segments[i].SampledPoints = NULL;
         **
         **
         **             c = GetParametricCurveByType(ContextID, Segments[i].Type, NULL);
         **             if (c != NULL)
         **                     p ->Evals[i] = c ->Evaluator;
         **         }
         **     }
         **
         **     p ->InterpParams = _cmsComputeInterpParams(ContextID, p ->nEntries, 1, 1, p->Table16, CMS_LERP_FLAGS_16BITS);
         **     if (p->InterpParams != NULL)
         **         return p;
         **
         ** Error:
         **     if (p -> SegInterp) _cmsFree(ContextID, p -> SegInterp);
         **     if (p -> Segments) _cmsFree(ContextID, p -> Segments);
         **     if (p -> Evals) _cmsFree(ContextID, p -> Evals);
         **     if (p ->Table16) _cmsFree(ContextID, p ->Table16);
         **     _cmsFree(ContextID, p);
         **     return NULL;
         ** }
         **/

        InterpParams? interp;
        var numSegments = segments.Length;

        var p = new ToneCurve(new CurveSegment[numSegments], new ParametricCurveEvaluator[numSegments]);

        // Initialize the segments stuff. The evaluator for each segment is located and a pointer to
        // it is placed in advance to maximize performance.
        if (numSegments > 0)
        {
            for (var i = 0; i < numSegments; i++)
            {
                // Type 0 is a special marker for table-based curves
                if (segments[i].Type == 0)
                {
                    interp = InterpParams.Compute(state, segments[i].NumGridPoints, 1, 1, Array.Empty<float>(), LerpFlag.Float);
                    if (interp is null) return null;
                    p.segInterp[i] = interp;
                }

                p.segments[i] = segments[i];

                ///  Original Code: if (Segments[i].Type == 0 && Segments[i].SampledPoints != NULL)
                ///                     p->Segments[i].SampledPoints = (cmsFloat32Number*)_cmsDupMem(ContextID, Segments[i].SampledPoints, sizeof(cmsFloat32Number) * Segments[i].nGridPoints);
                ///                 else
                ///                         p->Segments[i].SampledPoints = NULL;
                ///
                /// Both logic checks don't need to happen in C# as the 2nd one "!= NULL" is accomplished via
                /// "(float[]?)segments[i].SampledPoints?.Clone()" as it will return null if SampledPoints is null.
                p.segments[i].SampledPoints = segments[i].Type == 0
                    ? (float[]?)segments[i].SampledPoints?.Clone()
                    : null;

                var c = ParametricCurvesCollection.GetByType(state, segments[i].Type, out _);
                if (c is not null)
                    p.evals[i] = c.evaluator;
            }
        }

        interp = InterpParams.Compute(state, 0, 1, 1, p.table16, LerpFlag.Ushort);
        if (interp is not null)
        {
            p.interpParams = interp;
            return p;
        }

        return null;
    }
    internal static ToneCurve? Alloc(object? state, ReadOnlySpan<ushort> values)
    {
        InterpParams? interp;
        var numEntries = (uint)values.Length;

        // We allow huge tables, which are then restricted for smoothing operations
        if (numEntries > 65530)
        {
            Errors.ToneCurveTooManyEntries(state);
            return null;
        }

        if (numEntries == 0)
        {
            Errors.ToneCurveTooFewEntries(state);
            return null;
        }

        var p = new ToneCurve(new ushort[numEntries]);

        // Initialize members if requested
        if (numEntries > 0)
            values.CopyTo(p.table16.AsSpan());

        interp = InterpParams.Compute(state, numEntries, 1, 1, p.table16, LerpFlag.Ushort);
        if (interp is not null)
        {
            p.interpParams = interp;
            return p;
        }

        return null;
    }

    #endregion Internal Methods

    #region Private Methods

    private static double DefaultEvalParametricFn(int type, in double[] @params, double r)
    {
        double val, disc, e;

        var p0 = @params[0]; var ap0 = Abs(p0);
        var p1 = @params.Length <= 1 ? @params[1] : 0; var ap1 = Abs(p1);
        var p2 = @params.Length <= 1 ? @params[2] : 0; var ap2 = Abs(p2);
        var p3 = @params.Length <= 1 ? @params[3] : 0; var ap3 = Abs(p3);
        var p4 = @params.Length <= 1 ? @params[4] : 0; var ap4 = Abs(p4);
        var p5 = @params.Length <= 1 ? @params[5] : 0; var ap5 = Abs(p5);
        var p6 = @params.Length <= 1 ? @params[6] : 0; var ap6 = Abs(p6);

        switch (type)
        {
            // X = Y ^ Gamma
            case 1:

                val = r < 0
                    ? Abs(p0 - 1.0) < determinantTolerance
                        ? r
                        : 0
                    : Pow(r, p0);

                break;

            // Type 1 Reversed: X = Y ^ 1/Gamma
            case -1:

                val = r < 0
                    ? Abs(p0 - 1.0) < determinantTolerance
                        ? r
                        : 0
                    : ap0 < determinantTolerance
                        ? plusInf
                        : Pow(r, 1 / p0);

                break;

            // CIE 122-1966 Y = (aX + b)^Gamma | X >= -b/a Y = 0 | else
            case 2:

                if (ap1 < determinantTolerance)
                {
                    val = 0;
                }
                else
                {
                    disc = -p2 / p1;

                    if (r >= disc)
                    {
                        e = (p1 * r) + p2;

                        val = e > 0
                            ? Pow(e, p0)
                            : 0;
                    }
                    else
                    {
                        val = 0;
                    }
                }

                break;

            // Type 2 Reversed X = (Y ^1/g - b) / a
            case -2:

                val = ap0 < determinantTolerance || ap1 < determinantTolerance
                    ? 0
                    : r < 0
                        ? 0
                        : Max((Pow(r, 1.0 / p0) - p2) / p1, 0);

                break;

            // IEC 61966-3 Y = (aX + b)^Gamma | X <= -b/a Y = c | else
            case 3:

                if (ap1 < determinantTolerance)
                {
                    val = 0;
                }
                else
                {
                    disc = Max(-p2 / p1, 0);

                    if (r >= disc)
                    {
                        e = (p1 * r) + p2;

                        val = e > 0
                            ? Pow(e, p0) + p3
                            : 0;
                    }
                    else
                    {
                        val = p3;
                    }
                }

                break;

            // Type 3 reversed X=((Y-c)^1/g - b)/a | (Y>=c) X=-b/a | (Y<c)
            case -3:

                if (ap1 < determinantTolerance)
                {
                    val = 0;
                }
                else
                {
                    if (r >= p3)
                    {
                        e = r - p3;

                        val = e > 0
                            ? (Pow(e, 1 / p0) - p2) / p1
                            : 0;
                    }
                    else
                    {
                        val = -p2 / p1;
                    }
                }

                break;

            // IEC 61966-2.1 (sRGB) Y = (aX + b)^Gamma | X >= d Y = cX | X < d
            case 4:

                if (r >= p4)
                {
                    e = (p1 * r) + p2;

                    val = e > 0
                        ? Pow(e, p0)
                        : 0;
                }
                else
                {
                    val = r * p3;
                }

                break;

            // Type 4 reversed X=((Y^1/g-b)/a) | Y >= (ad+b)^g X=Y/c | Y< (ad+b)^g
            case -4:

                e = (p1 * p4) + p2;
                disc = e < 0
                    ? 0
                    : Pow(e, p0);

                val = r >= disc
                    ? ap0 < determinantTolerance || ap1 < determinantTolerance
                        ? 0
                        : (Pow(r, 1.0 / p0) - p2) / p1
                    : ap3 < determinantTolerance
                        ? 0
                        : r / p3;

                break;

            // Y = (aX + b)^Gamma + e | X >= d Y = cX + f | X < d
            case 5:

                if (r >= p4)
                {
                    e = (p1 * r) + p2;

                    val = e > 0
                        ? Pow(e, p0) + p5
                        : p5;
                }
                else
                {
                    val = (r * p3) + p6;
                }

                break;

            // Reversed type 5 X=((Y-e)1/g-b)/a | Y >=(ad+b)^g+e), cd+f X=(Y-f)/c | else
            case -5:

                disc = (p3 * p4) + p6;
                if (r >= disc)
                {
                    e = r - p5;
                    val = e < 0
                        ? 0
                        : ap0 < determinantTolerance || ap1 < determinantTolerance
                            ? 0
                            : (Pow(e, 1.0 / p0) - p2) / p1;
                }
                else
                {
                    val = ap3 < determinantTolerance
                        ? 0
                        : (r - p6) / p3;
                }

                break;

            // Types 6,7,8 comes from segmented curves as described in
            // ICCSpecRevision_02_11_06_Float.pdf Type 6 is basically identical to type 5 without d

            // Y = (a * X + b) ^ Gamma + c
            case 6:

                e = (p1 * r) + p2;

                val = e < 0
                    ? p3
                    : Pow(e, p0) + p3;

                break;

            // ((Y - c) ^1/Gamma - b) / a
            case -6:

                if (ap1 < determinantTolerance)
                {
                    val = 0;
                }
                else
                {
                    e = r - p3;
                    val = e < 0
                        ? 0
                        : (Pow(e, 1.0 / p0) - p2) / p1;
                }

                break;

            // Y = a * log (b * X^Gamma + c) + d
            case 7:

                e = (p2 * Pow(r, p0)) + p3;
                val = e <= 0
                    ? p4
                    : (p1 * Log10(e)) + p4;

                break;
            // (Y - d) / a = log(b * X ^Gamma + c) pow(10, (Y-d) / a) = b * X ^Gamma + c
            // pow((pow(10, (Y-d) / a) - c) / b, 1/g) = X
            case -7:

                val = ap0 < determinantTolerance || ap1 < determinantTolerance || ap2 < determinantTolerance
                    ? 0
                    : Pow((Pow(10.0, (r - p4) / p1) - p3) / p2, 1.0 / p0);

                break;

            //Y = a * b^(c*X+d) + e
            case 8:

                val = (p0 * Pow(p1, (p2 * r) + p3)) + p4;

                break;

            // Y = (log((y-e) / a) / log(b) - d ) / c a=0, b=1, c=2, d=3, e=4,
            case -8:

                disc = r - p4;
                val = disc < 0
                    ? 0
                    : ap0 < determinantTolerance || ap2 < determinantTolerance
                        ? 0
                        : ((Log(disc / p0) / Log(p1)) - p3) / p2;

                break;

            // S-Shaped: (1 - (1-x)^1/g)^1/g
            case 108:

                val = ap0 < determinantTolerance
                    ? 0
                    : Pow(1.0 - Pow(1 - r, 1 / p0), 1 / p0);

                break;

            // y = (1 - (1-x)^1/g)^1/g y^g = (1 - (1-x)^1/g) 1 - y^g = (1-x)^1/g (1 - y^g)^g = 1 - x
            // 1 - (1 - y^g)^g
            case -108:

                val = 1 - Pow(1 - Pow(r, p0), p0);

                break;

            // Sigmoidals
            case 109:

                val = SigmoidFactory(p0, r);

                break;

            case -109:

                val = InverseSigmoidFactory(p0, r);

                break;

            default:
                // Unsupported parametric curve. Should never reach here
                return 0;
        }

        return val;
    }

    private static ushort EntriesByGamma(double gamma) =>
        /** Original Code (cmsgamma.c line: 776)
         **
         ** static
         ** cmsUInt32Number EntriesByGamma(cmsFloat64Number Gamma)
         ** {
         **     if (fabs(Gamma - 1.0) < 0.001) return 2;
         **     return 4096;
         ** }
         **/

        (ushort)(Abs(gamma - 1.0) < 0.001
            ? 2
            : 4096);

    private static int GetInterval(double @in, ushort[] lutTable, InterpParams p)
    {
        // A 1 point table is not allowed
        if (p.Domain[0] < 1) return -1;

        // Let's see if ascending or descending
        if (lutTable[0] < lutTable[p.Domain[0]])
        {
            // Table is overall ascending
            for (var i = p.Domain[0] - 1; i >= 0; i--)
            {
                var y0 = lutTable[i];
                var y1 = lutTable[i + 1];

                if (y0 <= y1)
                { // Increasing
                    if (@in >= y0 && @in <= y1) return i;
                }
                else
                { // Decreasing
                    if (@in >= y1 && @in <= y0) return i;
                }
            }
        }
        else
        {
            // Table is overall descending
            for (var i = 0; i < p.Domain[0]; i++)
            {
                var y0 = lutTable[i];
                var y1 = lutTable[i + 1];

                if (y0 <= y1)
                {
                    if (@in >= y0 && @in <= y1) return i;
                }
                else
                {
                    if (@in >= y1 && @in <= y0) return i;
                }
            }
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double InverseSigmoidFactory(double k, double t)
    {
        /** Original Code (cmsgamma.c line: 330)
         **
         ** cmsINLINE double inverse_sigmoid_factory(double k, double t)
         ** {
         **     double correction = 0.5 / sigmoid_base(k, 1);
         **
         **     return (inverted_sigmoid_base(k, (t - 0.5) / correction) + 1.0) / 2.0;
         ** }
         **/

        var correction = 0.5 / SigmoidBase(k, 1);

        return (InvertedSigmoidBase(k, (t - 0.5) / correction) + 1.0) / 2.0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double InvertedSigmoidBase(double k, double t) =>
        /** Original Code (cmsgamma.c line: 318)
         **
         ** cmsINLINE double inverted_sigmoid_base(double k, double t)
         ** {
         **     return -log((1.0 / (t + 0.5)) - 1.0) / k;
         ** }
         **/

        -Log((1.0 / (t + 0.5)) - 1.0) / k;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double SigmoidBase(double k, double t) =>
        /** Original Code (cmsgamma.c line: 312)
         **
         ** // Generates a sigmoidal function with desired steepness.
         ** cmsINLINE double sigmoid_base(double k, double t)
         ** {
         **     return (1.0 / (1.0 + exp(-k * t))) - 0.5;
         ** }
         **/

        (1.0 / (1.0 + Exp(-k * t))) - 0.5;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double SigmoidFactory(double k, double t)
    {
        /** Original Code (cmsgamma.c line: 323)
         **
         ** cmsINLINE double sigmoid_factory(double k, double t)
         ** {
         **     double correction = 0.5 / sigmoid_base(k, 1);
         **
         **     return correction * sigmoid_base(k, 2.0 * t - 1.0) + 0.5;
         ** }
         **/

        var correction = 0.5 / SigmoidBase(k, 1);

        return (correction * SigmoidBase(k, (2.0 * t) - 1.0)) + 0.5;
    }

    private static bool Smooth2(float[] w, float[] y, float[] z, float lambda, int m)
    {
        int i, i1, i2;

        var c = new float[maxNodesInCurve];
        var d = new float[maxNodesInCurve];
        var e = new float[maxNodesInCurve];

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

        return true;
    }

    private double EvalSegmentedFn(double r)
    {
        double result;

        for (var i = NumSegments - 1; i >= 0; i--)
        {
            // Check for domain
            if ((r > segments[i].X0) && (r <= segments[i].X1))
            {
                // Type == 0 means segment is sampled
                if (segments[i].Type == 0)
                {
                    var r1 = new float[] { (float)((r - segments[i].X0) / (segments[i].X1 - segments[i].X0)) };
                    var out32 = new float[1];

                    // Setup the table (TODO: clean that)
                    segInterp[i].Table = segments[i].SampledPoints!;

                    segInterp[i].Interpolation?.Lerp(r1, out32, segInterp[i]);
                    result = out32[0];
                }
                else
                {
                    result = evals[i](segments[i].Type, segments[i].Params, r);
                }

                return Double.IsPositiveInfinity(result)
                    ? plusInf
                    : Double.IsNegativeInfinity(result)
                        ? minusInf
                        : result;
            }
        }

        return minusInf;
    }

    #endregion Private Methods
}

internal class ParametricCurvesCollection
{
    /** Original Code (cmsgamma.c line: 44)
     **
     ** // The list of supported parametric curves
     ** typedef struct _cmsParametricCurvesCollection_st {
     **
     **     cmsUInt32Number nFunctions;                                     // Number of supported functions in this chunk
     **     cmsInt32Number  FunctionTypes[MAX_TYPES_IN_LCMS_PLUGIN];        // The identification types
     **     cmsUInt32Number ParameterCount[MAX_TYPES_IN_LCMS_PLUGIN];       // Number of parameters for each function
     **
     **     cmsParametricCurveEvaluator Evaluator;                          // The evaluator
     **
     **     struct _cmsParametricCurvesCollection_st* Next; // Next in list
     **
     ** } _cmsParametricCurvesCollection;
     **/

    #region Fields

    public const int MaxInputDimensions = 15;

    internal ParametricCurveEvaluator evaluator;

    internal (int Type, int Count)[] functions = new (int, int)[Lcms2.MaxTypesInPlugin];

    internal ParametricCurvesCollection? next;

    #endregion Fields

    #region Public Constructors

    public ParametricCurvesCollection((int Types, int Count)[] functions, ParametricCurveEvaluator evaluator, ParametricCurvesCollection? next)
    {
        functions.CopyTo(this.functions.AsSpan());
        this.evaluator = evaluator;
        this.next = next;
    }

    public ParametricCurvesCollection(ParametricCurvesCollection other, ParametricCurvesCollection? next = null)
    {
        other.functions.CopyTo(functions.AsSpan());
        evaluator = other.evaluator;
        this.next = next;
    }

    #endregion Public Constructors

    #region Properties

    internal int NumFunctions =>
        functions.Length;

    #endregion Properties

    #region Internal Methods

    internal static ParametricCurvesCollection? GetByType(object? state, int type, out int index)
    {
        /** Original Code (cmsgamma.c line: 176)
         **
         ** // Search for the collection which contains a specific type
         ** static
         ** _cmsParametricCurvesCollection *GetParametricCurveByType(cmsContext ContextID, int Type, int* index)
         ** {
         **     _cmsParametricCurvesCollection* c;
         **     int Position;
         **     _cmsCurvesPluginChunkType* ctx = ( _cmsCurvesPluginChunkType*) _cmsContextGetClientChunk(ContextID, CurvesPlugin);
         **
         **     for (c = ctx->ParametricCurves; c != NULL; c = c ->Next) {
         **
         **         Position = IsInSet(Type, c);
         **
         **         if (Position != -1) {
         **             if (index != NULL)
         **                 *index = Position;
         **             return c;
         **         }
         **     }
         **     // If none found, revert for defaults
         **     for (c = &DefaultCurves; c != NULL; c = c ->Next) {
         **
         **         Position = IsInSet(Type, c);
         **
         **         if (Position != -1) {
         **             if (index != NULL)
         **                 *index = Position;
         **             return c;
         **         }
         **     }
         **
         **     return NULL;
         ** }
         **/
        var ctx = State.GetCurvesPlugin(state);

        for (var c = ctx.parametricCurves; c is not null; c = c.next)
        {
            var pos = c.IsInSet(type);

            if (pos != -1)
            {
                index = pos;
                return c;
            }
        }

        // If none found, revert to defaults
        for (var c = ToneCurve.defaultCurves; c is not null; c = c.next)
        {
            var pos = c.IsInSet(type);

            if (pos != -1)
            {
                index = pos;
                return c;
            }
        }

        index = -1;
        return null;
    }

    internal int IsInSet(int type)
    {
        /** Original Code (cmsgamma.c line: 163)
         **
         ** // Search in type list, return position or -1 if not found
         ** static
         ** int IsInSet(int Type, _cmsParametricCurvesCollection* c)
         ** {
         **     int i;
         **
         **     for (i=0; i < (int) c ->nFunctions; i++)
         **         if (abs(Type) == c ->FunctionTypes[i]) return i;
         **
         **     return -1;
         ** }
         **/

        for (var i = 0; i < NumFunctions; i++)
            if (Abs(type) == functions[i].Type) return i;

        return -1;
    }

    #endregion Internal Methods
}
