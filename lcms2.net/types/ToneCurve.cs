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
        /** Original Code (cmsgamma.c line: 762)
         **
         ** const cmsUInt16Number* CMSEXPORT cmsGetToneCurveEstimatedTable(const cmsToneCurve* t)
         ** {
         **     _cmsAssert(t != NULL);
         **     return t ->Table16;
         ** }
         **/

        table16;

    public bool IsDescending =>
        /** Original Code (cmsgamma.c line: 1375)
         **
         ** // Same, but for descending tables
         ** cmsBool  CMSEXPORT cmsIsToneCurveDescending(const cmsToneCurve* t)
         ** {
         **     _cmsAssert(t != NULL);
         **
         **     return t ->Table16[0] > t ->Table16[t ->nEntries-1];
         ** }
         **/

        table16[0] > table16[^1];

    public bool IsLinear
    {
        /** Original Code (cmsgamma.c line: 1310)
         **
         ** // Is a table linear? Do not use parametric since we cannot guarantee some weird parameters resulting
         ** // in a linear table. This way assures it is linear in 12 bits, which should be enough in most cases.
         ** cmsBool CMSEXPORT cmsIsToneCurveLinear(const cmsToneCurve* Curve)
         ** {
         **     int i;
         **     int diff;
         **
         **     _cmsAssert(Curve != NULL);
         **
         **     for (i=0; i < (int) Curve ->nEntries; i++) {
         **
         **         diff = abs((int) Curve->Table16[i] - (int) _cmsQuantizeVal(i, Curve ->nEntries));
         **         if (diff > 0x0f)
         **             return FALSE;
         **     }
         **
         **     return TRUE;
         ** }
         **/

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
        /** Original Code (cmsgamma.c line: 1329)
         **
         ** // Same, but for monotonicity
         ** cmsBool  CMSEXPORT cmsIsToneCurveMonotonic(const cmsToneCurve* t)
         ** {
         **     cmsUInt32Number n;
         **     int i, last;
         **     cmsBool lDescending;
         **
         **     _cmsAssert(t != NULL);
         **
         **     // Degenerated curves are monotonic? Ok, let's pass them
         **     n = t ->nEntries;
         **     if (n < 2) return TRUE;
         **
         **     // Curve direction
         **     lDescending = cmsIsToneCurveDescending(t);
         **
         **     if (lDescending) {
         **
         **         last = t ->Table16[0];
         **
         **         for (i = 1; i < (int) n; i++) {
         **
         **             if (t ->Table16[i] - last > 2) // We allow some ripple
         **                 return FALSE;
         **             else
         **                 last = t ->Table16[i];
         **
         **         }
         **     }
         **     else {
         **
         **         last = t ->Table16[n-1];
         **
         **         for (i = (int) n - 2; i >= 0; --i) {
         **
         **             if (t ->Table16[i] - last > 2)
         **                 return FALSE;
         **             else
         **                 last = t ->Table16[i];
         **
         **         }
         **     }
         **
         **     return TRUE;
         ** }
         **/

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
                var last = table16[^1];

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
       /** Original Code (cmsgamma.c line: 1384)
       **
       ** // Another info fn: is out gamma table multisegment?
       **  cmsBool  CMSEXPORT cmsIsToneCurveMultisegment(const cmsToneCurve* t)
       **  {
       **  _cmsAssert(t != NULL);
       **
       **  return t -> nSegments > 1;
       **  }
       **/

       NumSegments > 1;

    public uint NumEntries =>
        /** Original Code (cmsgamma.c line: 755)
         **
         ** // Access to estimated low-res table
         ** cmsUInt32Number CMSEXPORT cmsGetToneCurveEstimatedTableEntries(const cmsToneCurve* t)
         ** {
         **     _cmsAssert(t != NULL);
         **     return t ->nEntries;
         ** }
         **/

        (uint)table16.Length;

    public int ParametricType =>
        /** Original Code (cmsgamma.c line: 1392)
         **
         ** cmsInt32Number  CMSEXPORT cmsGetToneCurveParametricType(const cmsToneCurve* t)
         ** {
         **     _cmsAssert(t != NULL);
         **
         **     if (t -> nSegments != 1) return 0;
         **     return t ->Segments[0].Type;
         ** }
         **/

        NumSegments == 1
            ? segments[0].Type
              : 0;

    public double[]? Params =>
        /** Original Code (cmsgamma.c line: 1486)
         **
         ** // Retrieve parameters on one-segment tone curves
         **
         ** cmsFloat64Number* CMSEXPORT cmsGetToneCurveParams(const cmsToneCurve* t)
         ** {
         **     _cmsAssert(t != NULL);
         **
         **     if (t->nSegments != 1) return NULL;
         **     return t->Segments[0].Params;
         ** }
         **/

        NumSegments is 1
            ? segments[0].Params
            : null;

    internal int NumSegments =>
                     segments.Length;

    #endregion Properties

    #region Public Methods

    public static ToneCurve? BuildEmptyTabulated16(object? state, uint numEntries) =>
        BuildTabulated(state, new ushort[numEntries]);

    public static ToneCurve? BuildEmptyTabulatedFloat(object? state, uint numSamples) =>
        BuildTabulated(state, new float[numSamples]);

    public static ToneCurve? BuildGamma(object? state, double gamma) =>
                /** Original Code (cmsgamma.c line: 892)
                 **
                 ** // Build a gamma table based on gamma constant
                 ** cmsToneCurve* CMSEXPORT cmsBuildGamma(cmsContext ContextID, cmsFloat64Number Gamma)
                 ** {
                 **     return cmsBuildParametricToneCurve(ContextID, 1, &Gamma);
                 ** }
                 **/

                BuildParametric(state, 1, gamma);

    public static ToneCurve? BuildParametric(object? state, int type, params double[] @params)
    {
        /** Original Code (cmsgamma.c line: 859)
         **
         ** // Parametric curves
         ** //
         ** // Parameters goes as: Curve, a, b, c, d, e, f
         ** // Type is the ICC type +1
         ** // if type is negative, then the curve is analytically inverted
         ** cmsToneCurve* CMSEXPORT cmsBuildParametricToneCurve(cmsContext ContextID, cmsInt32Number Type, const cmsFloat64Number Params[])
         ** {
         **     cmsCurveSegment Seg0;
         **     int Pos = 0;
         **     cmsUInt32Number size;
         **     _cmsParametricCurvesCollection* c = GetParametricCurveByType(ContextID, Type, &Pos);
         **
         **     _cmsAssert(Params != NULL);
         **
         **     if (c == NULL) {
         **         cmsSignalError(ContextID, cmsERROR_UNKNOWN_EXTENSION, "Invalid parametric curve type %d", Type);
         **         return NULL;
         **     }
         **
         **     memset(&Seg0, 0, sizeof(Seg0));
         **
         **     Seg0.x0   = MINUS_INF;
         **     Seg0.x1   = PLUS_INF;
         **     Seg0.Type = Type;
         **
         **     size = c->ParameterCount[Pos] * sizeof(cmsFloat64Number);
         **     memmove(Seg0.Params, Params, size);
         **
         **     return cmsBuildSegmentedToneCurve(ContextID, 1, &Seg0);
         ** }
         **/

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

    public static ToneCurve? BuildSegmented(object? state, ReadOnlySpan<CurveSegment> segments)
    {
        /** Original Code (cmsgamma.c line: 784)
         **
         ** // Create a segmented gamma, fill the table
         ** cmsToneCurve* CMSEXPORT cmsBuildSegmentedToneCurve(cmsContext ContextID,
         **                                                    cmsUInt32Number nSegments, const cmsCurveSegment Segments[])
         ** {
         **     cmsUInt32Number i;
         **     cmsFloat64Number R, Val;
         **     cmsToneCurve* g;
         **     cmsUInt32Number nGridPoints = 4096;
         **
         **     _cmsAssert(Segments != NULL);
         **
         **     // Optimizatin for identity curves.
         **     if (nSegments == 1 && Segments[0].Type == 1) {
         **
         **         nGridPoints = EntriesByGamma(Segments[0].Params[0]);
         **     }
         **
         **     g = AllocateToneCurveStruct(ContextID, nGridPoints, nSegments, Segments, NULL);
         **     if (g == NULL) return NULL;
         **
         **     // Once we have the floating point version, we can approximate a 16 bit table of 4096 entries
         **     // for performance reasons. This table would normally not be used except on 8/16 bits transforms.
         **     for (i = 0; i < nGridPoints; i++) {
         **
         **         R   = (cmsFloat64Number) i / (nGridPoints-1);
         **
         **         Val = EvalSegmentedFn(g, R);
         **
         **         // Round and saturate
         **         g ->Table16[i] = _cmsQuickSaturateWord(Val * 65535.0);
         **     }
         **
         **     return g;
         ** }
         **/

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

    public static ToneCurve? BuildTabulated(object? state, ReadOnlySpan<ushort> values) =>
        /** Original Code (cmsgamma.c line: 769)
         **
         ** // Create an empty gamma curve, by using tables. This specifies only the limited-precision part, and leaves the
         ** // floating point description empty.
         ** cmsToneCurve* CMSEXPORT cmsBuildTabulatedToneCurve16(cmsContext ContextID, cmsUInt32Number nEntries, const cmsUInt16Number Values[])
         ** {
         **     return AllocateToneCurveStruct(ContextID, nEntries, 0, NULL, Values);
         ** }
         **/

        Alloc(state, values);

    public static ToneCurve? BuildTabulated(object? state, ReadOnlySpan<float> values)
    {
        /** Original Code (cmsgamma.c line: )
         **
         ** // Use a segmented curve to store the floating point table
         ** cmsToneCurve* CMSEXPORT cmsBuildTabulatedToneCurveFloat(cmsContext ContextID, cmsUInt32Number nEntries, const cmsFloat32Number values[])
         ** {
         **     cmsCurveSegment Seg[3];
         **
         **     // A segmented tone curve should have function segments in the first and last positions
         **     // Initialize segmented curve part up to 0 to constant value = samples[0]
         **     Seg[0].x0 = MINUS_INF;
         **     Seg[0].x1 = 0;
         **     Seg[0].Type = 6;
         **
         **     Seg[0].Params[0] = 1;
         **     Seg[0].Params[1] = 0;
         **     Seg[0].Params[2] = 0;
         **     Seg[0].Params[3] = values[0];
         **     Seg[0].Params[4] = 0;
         **
         **     // From zero to 1
         **     Seg[1].x0 = 0;
         **     Seg[1].x1 = 1.0;
         **     Seg[1].Type = 0;
         **
         **     Seg[1].nGridPoints = nEntries;
         **     Seg[1].SampledPoints = (cmsFloat32Number*) values;
         **
         **     // Final segment is constant = lastsample
         **     Seg[2].x0 = 1.0;
         **     Seg[2].x1 = PLUS_INF;
         **     Seg[2].Type = 6;
         **
         **     Seg[2].Params[0] = 1;
         **     Seg[2].Params[1] = 0;
         **     Seg[2].Params[2] = 0;
         **     Seg[2].Params[3] = values[nEntries-1];
         **     Seg[2].Params[4] = 0;
         **
         **
         **     return cmsBuildSegmentedToneCurve(ContextID, 3, Seg);
         ** }
         **/

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
        /** Original Code (cmsgamma.c line: 937)
         **
         ** // Utility function, free 3 gamma tables
         ** void CMSEXPORT cmsFreeToneCurveTriple(cmsToneCurve* Curve[3])
         ** {
         **
         **     _cmsAssert(Curve != NULL);
         **
         **     if (Curve[0] != NULL) cmsFreeToneCurve(Curve[0]);
         **     if (Curve[1] != NULL) cmsFreeToneCurve(Curve[1]);
         **     if (Curve[2] != NULL) cmsFreeToneCurve(Curve[2]);
         **
         **     Curve[0] = Curve[1] = Curve[2] = NULL;
         ** }
         **/

        Debug.Assert(curves.Length == 3);

        curves[0]?.Dispose();
        curves[1]?.Dispose();
        curves[2]?.Dispose();
    }

    public object Clone() =>
        /** Original Code (cmsgamma.c line: )
         **
         ** // Duplicate a gamma table
         ** cmsToneCurve* CMSEXPORT cmsDupToneCurve(const cmsToneCurve* In)
         ** {
         **     if (In == NULL) return NULL;
         **
         **     return  AllocateToneCurveStruct(In ->InterpParams ->ContextID, In ->nEntries, In ->nSegments, In ->Segments, In ->Table16);
         ** }
         **/

        table16.Length > 0
            ? Alloc(interpParams?.StateContainer, table16)!
            : segments.Length > 0
                ? Alloc(interpParams?.StateContainer, segments)!
                : null!;

    public void Dispose()
    {
        /** Original Code (cmsgamma.c line: 899)
         **
         ** // Free all memory taken by the gamma curve
         ** void CMSEXPORT cmsFreeToneCurve(cmsToneCurve* Curve)
         ** {
         **     cmsContext ContextID;
         **
         **     if (Curve == NULL) return;
         **
         **     ContextID = Curve ->InterpParams->ContextID;
         **
         **     _cmsFreeInterpParams(Curve ->InterpParams);
         **
         **     if (Curve -> Table16)
         **         _cmsFree(ContextID, Curve ->Table16);
         **
         **     if (Curve ->Segments) {
         **
         **         cmsUInt32Number i;
         **
         **         for (i=0; i < Curve ->nSegments; i++) {
         **
         **             if (Curve ->Segments[i].SampledPoints) {
         **                 _cmsFree(ContextID, Curve ->Segments[i].SampledPoints);
         **             }
         **
         **             if (Curve ->SegInterp[i] != 0)
         **                 _cmsFreeInterpParams(Curve->SegInterp[i]);
         **         }
         **
         **         _cmsFree(ContextID, Curve ->Segments);
         **         _cmsFree(ContextID, Curve ->SegInterp);
         **     }
         **
         **     if (Curve -> Evals)
         **         _cmsFree(ContextID, Curve -> Evals);
         **
         **     _cmsFree(ContextID, Curve);
         ** }
         **/

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
        /** Original Code (cmsgamma.c line: 1431)
         **
         ** // Least squares fitting.
         ** // A mathematical procedure for finding the best-fitting curve to a given set of points by
         ** // minimizing the sum of the squares of the offsets ("the residuals") of the points from the curve.
         ** // The sum of the squares of the offsets is used instead of the offset absolute values because
         ** // this allows the residuals to be treated as a continuous differentiable quantity.
         ** //
         ** // y = f(x) = x ^ g
         ** //
         ** // R  = (yi - (xi^g))
         ** // R2 = (yi - (xi^g))2
         ** // SUM R2 = SUM (yi - (xi^g))2
         ** //
         ** // dR2/dg = -2 SUM x^g log(x)(y - x^g)
         ** // solving for dR2/dg = 0
         ** //
         ** // g = 1/n * SUM(log(y) / log(x))
         **
         ** cmsFloat64Number CMSEXPORT cmsEstimateGamma(const cmsToneCurve* t, cmsFloat64Number Precision)
         ** {
         **     cmsFloat64Number gamma, sum, sum2;
         **     cmsFloat64Number n, x, y, Std;
         **     cmsUInt32Number i;
         **
         **     _cmsAssert(t != NULL);
         **
         **     sum = sum2 = n = 0;
         **
         **     // Excluding endpoints
         **     for (i=1; i < (MAX_NODES_IN_CURVE-1); i++) {
         **
         **         x = (cmsFloat64Number) i / (MAX_NODES_IN_CURVE-1);
         **         y = (cmsFloat64Number) cmsEvalToneCurveFloat(t, (cmsFloat32Number) x);
         **
         **         // Avoid 7% on lower part to prevent
         **         // artifacts due to linear ramps
         **
         **         if (y > 0. && y < 1. && x > 0.07) {
         **
         **             gamma = log(y) / log(x);
         **             sum  += gamma;
         **             sum2 += gamma * gamma;
         **             n++;
         **         }
         **     }
         **
         **     // Take a look on SD to see if gamma isn't exponential at all
         **     Std = sqrt((n * sum2 - sum * sum) / (n*(n-1)));
         **
         **     if (Std > Precision)
         **         return -1.0;
         **
         **     return (sum / n);   // The mean
         ** }
         **/

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
        /** Original Code (cmsgamma.c line: 1400)
         **
         ** // We need accuracy this time
         ** cmsFloat32Number CMSEXPORT cmsEvalToneCurveFloat(const cmsToneCurve* Curve, cmsFloat32Number v)
         ** {
         **     _cmsAssert(Curve != NULL);
         **
         **     // Check for 16 bits table. If so, this is a limited-precision tone curve
         **     if (Curve ->nSegments == 0) {
         **
         **         cmsUInt16Number In, Out;
         **
         **         In = (cmsUInt16Number) _cmsQuickSaturateWord(v * 65535.0);
         **         Out = cmsEvalToneCurve16(Curve, In);
         **
         **         return (cmsFloat32Number) (Out / 65535.0);
         **     }
         **
         **     return (cmsFloat32Number) EvalSegmentedFn(Curve, v);
         ** }
         **/

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
        /** Original Code (cmsgamma.c line: 1419)
         **
         ** // We need xput over here
         ** cmsUInt16Number CMSEXPORT cmsEvalToneCurve16(const cmsToneCurve* Curve, cmsUInt16Number v)
         ** {
         **     cmsUInt16Number out;
         **
         **     _cmsAssert(Curve != NULL);
         **
         **     Curve ->InterpParams ->Interpolation.Lerp16(&v, &out, Curve ->InterpParams);
         **     return out;
         ** }
         **/

        var i = new ushort[] { v };
        var o = new ushort[1];

        interpParams!.Interpolation?.Lerp(i, o, interpParams!);

        return o[0];
    }

    public ToneCurve? Join(object? state, ToneCurve Y, uint numResultingPoints)
    {
        /** Original Code (cmsgamma.c line: )
         **
         ** // Joins two curves for X and Y. Curves should be monotonic.
         ** // We want to get
         ** //
         ** //      y = Y^-1(X(t))
         ** //
         ** cmsToneCurve* CMSEXPORT cmsJoinToneCurve(cmsContext ContextID,
         **                                       const cmsToneCurve* X,
         **                                       const cmsToneCurve* Y, cmsUInt32Number nResultingPoints)
         ** {
         **     cmsToneCurve* out = NULL;
         **     cmsToneCurve* Yreversed = NULL;
         **     cmsFloat32Number t, x;
         **     cmsFloat32Number* Res = NULL;
         **     cmsUInt32Number i;
         **
         **
         **     _cmsAssert(X != NULL);
         **     _cmsAssert(Y != NULL);
         **
         **     Yreversed = cmsReverseToneCurveEx(nResultingPoints, Y);
         **     if (Yreversed == NULL) goto Error;
         **
         **     Res = (cmsFloat32Number*) _cmsCalloc(ContextID, nResultingPoints, sizeof(cmsFloat32Number));
         **     if (Res == NULL) goto Error;
         **
         **     //Iterate
         **     for (i=0; i <  nResultingPoints; i++) {
         **
         **         t = (cmsFloat32Number) i / (cmsFloat32Number)(nResultingPoints-1);
         **         x = cmsEvalToneCurveFloat(X,  t);
         **         Res[i] = cmsEvalToneCurveFloat(Yreversed, x);
         **     }
         **
         **     // Allocate space for output
         **     out = cmsBuildTabulatedToneCurveFloat(ContextID, nResultingPoints, Res);
         **
         ** Error:
         **
         **     if (Res != NULL) _cmsFree(ContextID, Res);
         **     if (Yreversed != NULL) cmsFreeToneCurve(Yreversed);
         **
         **     return out;
         ** }
         **/

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

    public ToneCurve? Reverse(uint numResultSamples = 4096)
    {
        /** Original Code (cmsgamma.c line: 1053)
         **
         ** // Reverse a gamma table
         ** cmsToneCurve* CMSEXPORT cmsReverseToneCurveEx(cmsUInt32Number nResultSamples, const cmsToneCurve* InCurve)
         ** {
         **     cmsToneCurve *out;
         **     cmsFloat64Number a = 0, b = 0, y, x1, y1, x2, y2;
         **     int i, j;
         **     int Ascending;
         **
         **     _cmsAssert(InCurve != NULL);
         **
         **     // Try to reverse it analytically whatever possible
         **
         **     if (InCurve ->nSegments == 1 && InCurve ->Segments[0].Type > 0 &&
         **         /* InCurve -> Segments[0].Type <= 5 *\
         **         GetParametricCurveByType(InCurve ->InterpParams->ContextID, InCurve ->Segments[0].Type, NULL) != NULL) {
         **
         **         return cmsBuildParametricToneCurve(InCurve ->InterpParams->ContextID,
         **                                        -(InCurve -> Segments[0].Type),
         **                                        InCurve -> Segments[0].Params);
         **     }
         **
         **     // Nope, reverse the table.
         **     out = cmsBuildTabulatedToneCurve16(InCurve ->InterpParams->ContextID, nResultSamples, NULL);
         **     if (out == NULL)
         **         return NULL;
         **
         **     // We want to know if this is an ascending or descending table
         **     Ascending = !cmsIsToneCurveDescending(InCurve);
         **
         **     // Iterate across Y axis
         **     for (i=0; i < (int) nResultSamples; i++) {
         **
         **         y = (cmsFloat64Number) i * 65535.0 / (nResultSamples - 1);
         **
         **         // Find interval in which y is within.
         **         j = GetInterval(y, InCurve->Table16, InCurve->InterpParams);
         **         if (j >= 0) {
         **
         **
         **             // Get limits of interval
         **             x1 = InCurve ->Table16[j];
         **             x2 = InCurve ->Table16[j+1];
         **
         **             y1 = (cmsFloat64Number) (j * 65535.0) / (InCurve ->nEntries - 1);
         **             y2 = (cmsFloat64Number) ((j+1) * 65535.0 ) / (InCurve ->nEntries - 1);
         **
         **             // If collapsed, then use any
         **             if (x1 == x2) {
         **
         **                 out ->Table16[i] = _cmsQuickSaturateWord(Ascending ? y2 : y1);
         **                 continue;
         **
         **             } else {
         **
         **                 // Interpolate
         **                 a = (y2 - y1) / (x2 - x1);
         **                 b = y2 - a * x2;
         **             }
         **         }
         **
         **         out ->Table16[i] = _cmsQuickSaturateWord(a* y + b);
         **     }
         **
         **
         **     return out;
         ** }
         **
         ** // Reverse a gamma table
         ** cmsToneCurve* CMSEXPORT cmsReverseToneCurve(const cmsToneCurve* InGamma)
         ** {
         **     _cmsAssert(InGamma != NULL);
         **
         **     return cmsReverseToneCurveEx(4096, InGamma);
         ** }
         **/

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

    public bool Smooth(double lambda)
    {
        /** Original Code (cmsgamma.c line: 1195)
         **
         ** // Smooths a curve sampled at regular intervals.
         ** cmsBool  CMSEXPORT cmsSmoothToneCurve(cmsToneCurve* Tab, cmsFloat64Number lambda)
         ** {
         **     cmsBool SuccessStatus = TRUE;
         **     cmsFloat32Number *w, *y, *z;
         **     cmsUInt32Number i, nItems, Zeros, Poles;
         **     cmsBool notCheck = FALSE;
         **
         **     if (Tab != NULL && Tab->InterpParams != NULL)
         **     {
         **         cmsContext ContextID = Tab->InterpParams->ContextID;
         **
         **         if (!cmsIsToneCurveLinear(Tab)) // Only non-linear curves need smoothing
         **         {
         **             nItems = Tab->nEntries;
         **             if (nItems < MAX_NODES_IN_CURVE)
         **             {
         **                 // Allocate one more item than needed
         **                 w = (cmsFloat32Number *)_cmsCalloc(ContextID, nItems + 1, sizeof(cmsFloat32Number));
         **                 y = (cmsFloat32Number *)_cmsCalloc(ContextID, nItems + 1, sizeof(cmsFloat32Number));
         **                 z = (cmsFloat32Number *)_cmsCalloc(ContextID, nItems + 1, sizeof(cmsFloat32Number));
         **
         **                 if (w != NULL && y != NULL && z != NULL) // Ensure no memory allocation failure
         **                 {
         **                     memset(w, 0, (nItems + 1) * sizeof(cmsFloat32Number));
         **                     memset(y, 0, (nItems + 1) * sizeof(cmsFloat32Number));
         **                     memset(z, 0, (nItems + 1) * sizeof(cmsFloat32Number));
         **
         **                     for (i = 0; i < nItems; i++)
         **                     {
         **                         y[i + 1] = (cmsFloat32Number)Tab->Table16[i];
         **                         w[i + 1] = 1.0;
         **                     }
         **
         **                     if (lambda < 0)
         **                     {
         **                         notCheck = TRUE;
         **                         lambda = -lambda;
         **                     }
         **
         **                     if (smooth2(ContextID, w, y, z, (cmsFloat32Number)lambda, (int)nItems))
         **                     {
         **                         // Do some reality - checking...
         **
         **                         Zeros = Poles = 0;
         **                         for (i = nItems; i > 1; --i)
         **                         {
         **                             if (z[i] == 0.) Zeros++;
         **                             if (z[i] >= 65535.) Poles++;
         **                             if (z[i] < z[i - 1])
         **                             {
         **                                 cmsSignalError(ContextID, cmsERROR_RANGE, "cmsSmoothToneCurve: Non-Monotonic.");
         **                                 SuccessStatus = notCheck;
         **                                 break;
         **                             }
         **                         }
         **
         **                         if (SuccessStatus && Zeros > (nItems / 3))
         **                         {
         **                             cmsSignalError(ContextID, cmsERROR_RANGE, "cmsSmoothToneCurve: Degenerated, mostly zeros.");
         **                             SuccessStatus = notCheck;
         **                         }
         **
         **                         if (SuccessStatus && Poles > (nItems / 3))
         **                         {
         **                             cmsSignalError(ContextID, cmsERROR_RANGE, "cmsSmoothToneCurve: Degenerated, mostly poles.");
         **                             SuccessStatus = notCheck;
         **                         }
         **
         **                         if (SuccessStatus) // Seems ok
         **                         {
         **                             for (i = 0; i < nItems; i++)
         **                             {
         **                                 // Clamp to cmsUInt16Number
         **                                 Tab->Table16[i] = _cmsQuickSaturateWord(z[i + 1]);
         **                             }
         **                         }
         **                     }
         **                     else // Could not smooth
         **                     {
         **                         cmsSignalError(ContextID, cmsERROR_RANGE, "cmsSmoothToneCurve: Function smooth2 failed.");
         **                         SuccessStatus = FALSE;
         **                     }
         **                 }
         **                 else // One or more buffers could not be allocated
         **                 {
         **                     cmsSignalError(ContextID, cmsERROR_RANGE, "cmsSmoothToneCurve: Could not allocate memory.");
         **                     SuccessStatus = FALSE;
         **                 }
         **
         **                 if (z != NULL)
         **                     _cmsFree(ContextID, z);
         **
         **                 if (y != NULL)
         **                     _cmsFree(ContextID, y);
         **
         **                 if (w != NULL)
         **                     _cmsFree(ContextID, w);
         **             }
         **             else // too many items in the table
         **             {
         **                 cmsSignalError(ContextID, cmsERROR_RANGE, "cmsSmoothToneCurve: Too many points.");
         **                 SuccessStatus = FALSE;
         **             }
         **         }
         **     }
         **     else // Tab parameter or Tab->InterpParams is NULL
         **     {
         **         // Can't signal an error here since the ContextID is not known at this point
         **         SuccessStatus = FALSE;
         **     }
         **
         **     return SuccessStatus;
         ** }
         **/

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

                        Smooth(w, y, z, (float)lambda, (int)numItems);

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
                            {
                                // Clamp to ushort
                                table16[i] = QuickSaturateWord(z[i + 1]);
                            }
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
        /** Original Code (cmsgamma.c line: 338)
         **
         ** // Parametric Fn using floating point
         ** static
         ** cmsFloat64Number DefaultEvalParametricFn(cmsInt32Number Type, const cmsFloat64Number Params[], cmsFloat64Number R)
         ** {
         **     cmsFloat64Number e, Val, disc;
         **
         **     switch (Type) {
         **
         **    // X = Y ^ Gamma
         **     case 1:
         **         if (R < 0) {
         **
         **             if (fabs(Params[0] - 1.0) < MATRIX_DET_TOLERANCE)
         **                 Val = R;
         **             else
         **                 Val = 0;
         **         }
         **         else
         **             Val = pow(R, Params[0]);
         **         break;
         **
         **     // Type 1 Reversed: X = Y ^1/gamma
         **     case -1:
         **         if (R < 0) {
         **
         **             if (fabs(Params[0] - 1.0) < MATRIX_DET_TOLERANCE)
         **                 Val = R;
         **             else
         **                 Val = 0;
         **         }
         **         else
         **         {
         **             if (fabs(Params[0]) < MATRIX_DET_TOLERANCE)
         **                 Val = PLUS_INF;
         **             else
         **                 Val = pow(R, 1 / Params[0]);
         **         }
         **         break;
         **
         **     // CIE 122-1966
         **     // Y = (aX + b)^Gamma  | X >= -b/a
         **     // Y = 0               | else
         **     case 2:
         **     {
         **
         **         if (fabs(Params[1]) < MATRIX_DET_TOLERANCE)
         **         {
         **             Val = 0;
         **         }
         **         else
         **         {
         **             disc = -Params[2] / Params[1];
         **
         **             if (R >= disc) {
         **
         **                 e = Params[1] * R + Params[2];
         **
         **                 if (e > 0)
         **                     Val = pow(e, Params[0]);
         **                 else
         **                     Val = 0;
         **             }
         **             else
         **                 Val = 0;
         **         }
         **     }
         **     break;
         **
         **      // Type 2 Reversed
         **      // X = (Y ^1/g  - b) / a
         **      case -2:
         **      {
         **          if (fabs(Params[0]) < MATRIX_DET_TOLERANCE ||
         **              fabs(Params[1]) < MATRIX_DET_TOLERANCE)
         **          {
         **              Val = 0;
         **          }
         **          else
         **          {
         **              if (R < 0)
         **                  Val = 0;
         **              else
         **                  Val = (pow(R, 1.0 / Params[0]) - Params[2]) / Params[1];
         **
         **              if (Val < 0)
         **                  Val = 0;
         **          }
         **      }
         **      break;
         **
         **
         **     // IEC 61966-3
         **     // Y = (aX + b)^Gamma | X <= -b/a
         **     // Y = c              | else
         **     case 3:
         **     {
         **         if (fabs(Params[1]) < MATRIX_DET_TOLERANCE)
         **         {
         **             Val = 0;
         **         }
         **         else
         **         {
         **             disc = -Params[2] / Params[1];
         **             if (disc < 0)
         **                 disc = 0;
         **
         **             if (R >= disc) {
         **
         **                 e = Params[1] * R + Params[2];
         **
         **                 if (e > 0)
         **                     Val = pow(e, Params[0]) + Params[3];
         **                 else
         **                     Val = 0;
         **             }
         **             else
         **                 Val = Params[3];
         **         }
         **     }
         **     break;
         **
         **
         **     // Type 3 reversed
         **     // X=((Y-c)^1/g - b)/a      | (Y>=c)
         **     // X=-b/a                   | (Y<c)
         **     case -3:
         **     {
         **         if (fabs(Params[1]) < MATRIX_DET_TOLERANCE)
         **         {
         **             Val = 0;
         **         }
         **         else
         **         {
         **             if (R >= Params[3]) {
         **
         **                 e = R - Params[3];
         **
         **                 if (e > 0)
         **                     Val = (pow(e, 1 / Params[0]) - Params[2]) / Params[1];
         **                 else
         **                     Val = 0;
         **             }
         **             else {
         **                 Val = -Params[2] / Params[1];
         **             }
         **         }
         **     }
         **     break;
         **
         **
         **     // IEC 61966-2.1 (sRGB)
         **     // Y = (aX + b)^Gamma | X >= d
         **     // Y = cX             | X < d
         **     case 4:
         **         if (R >= Params[4]) {
         **
         **             e = Params[1]*R + Params[2];
         **
         **             if (e > 0)
         **                 Val = pow(e, Params[0]);
         **             else
         **                 Val = 0;
         **         }
         **         else
         **             Val = R * Params[3];
         **         break;
         **
         **     // Type 4 reversed
         **     // X=((Y^1/g-b)/a)    | Y >= (ad+b)^g
         **     // X=Y/c              | Y< (ad+b)^g
         **     case -4:
         **     {
         **
         **         e = Params[1] * Params[4] + Params[2];
         **         if (e < 0)
         **             disc = 0;
         **         else
         **             disc = pow(e, Params[0]);
         **
         **         if (R >= disc) {
         **
         **             if (fabs(Params[0]) < MATRIX_DET_TOLERANCE ||
         **                 fabs(Params[1]) < MATRIX_DET_TOLERANCE)
         **
         **                 Val = 0;
         **
         **             else
         **                 Val = (pow(R, 1.0 / Params[0]) - Params[2]) / Params[1];
         **         }
         **         else {
         **
         **             if (fabs(Params[3]) < MATRIX_DET_TOLERANCE)
         **                 Val = 0;
         **             else
         **                 Val = R / Params[3];
         **         }
         **
         **     }
         **     break;
         **
         **
         **     // Y = (aX + b)^Gamma + e | X >= d
         **     // Y = cX + f             | X < d
         **     case 5:
         **         if (R >= Params[4]) {
         **
         **             e = Params[1]*R + Params[2];
         **
         **             if (e > 0)
         **                 Val = pow(e, Params[0]) + Params[5];
         **             else
         **                 Val = Params[5];
         **         }
         **         else
         **             Val = R*Params[3] + Params[6];
         **         break;
         **
         **
         **     // Reversed type 5
         **     // X=((Y-e)1/g-b)/a   | Y >=(ad+b)^g+e), cd+f
         **     // X=(Y-f)/c          | else
         **     case -5:
         **     {
         **         disc = Params[3] * Params[4] + Params[6];
         **         if (R >= disc) {
         **
         **             e = R - Params[5];
         **             if (e < 0)
         **                 Val = 0;
         **             else
         **             {
         **                 if (fabs(Params[0]) < MATRIX_DET_TOLERANCE ||
         **                     fabs(Params[1]) < MATRIX_DET_TOLERANCE)
         **
         **                     Val = 0;
         **                 else
         **                     Val = (pow(e, 1.0 / Params[0]) - Params[2]) / Params[1];
         **             }
         **         }
         **         else {
         **             if (fabs(Params[3]) < MATRIX_DET_TOLERANCE)
         **                 Val = 0;
         **             else
         **                 Val = (R - Params[6]) / Params[3];
         **         }
         **
         **     }
         **     break;
         **
         **
         **     // Types 6,7,8 comes from segmented curves as described in ICCSpecRevision_02_11_06_Float.pdf
         **     // Type 6 is basically identical to type 5 without d
         **
         **     // Y = (a * X + b) ^ Gamma + c
         **     case 6:
         **         e = Params[1]*R + Params[2];
         **
         **         if (e < 0)
         **             Val = Params[3];
         **         else
         **             Val = pow(e, Params[0]) + Params[3];
         **         break;
         **
         **     // ((Y - c) ^1/Gamma - b) / a
         **     case -6:
         **     {
         **         if (fabs(Params[1]) < MATRIX_DET_TOLERANCE)
         **         {
         **             Val = 0;
         **         }
         **         else
         **         {
         **             e = R - Params[3];
         **             if (e < 0)
         **                 Val = 0;
         **             else
         **                 Val = (pow(e, 1.0 / Params[0]) - Params[2]) / Params[1];
         **         }
         **     }
         **     break;
         **
         **
         **     // Y = a * log (b * X^Gamma + c) + d
         **     case 7:
         **
         **        e = Params[2] * pow(R, Params[0]) + Params[3];
         **        if (e <= 0)
         **            Val = Params[4];
         **        else
         **            Val = Params[1]*log10(e) + Params[4];
         **        break;
         **
         **     // (Y - d) / a = log(b * X ^Gamma + c)
         **     // pow(10, (Y-d) / a) = b * X ^Gamma + c
         **     // pow((pow(10, (Y-d) / a) - c) / b, 1/g) = X
         **     case -7:
         **     {
         **         if (fabs(Params[0]) < MATRIX_DET_TOLERANCE ||
         **             fabs(Params[1]) < MATRIX_DET_TOLERANCE ||
         **             fabs(Params[2]) < MATRIX_DET_TOLERANCE)
         **         {
         **             Val = 0;
         **         }
         **         else
         **         {
         **             Val = pow((pow(10.0, (R - Params[4]) / Params[1]) - Params[3]) / Params[2], 1.0 / Params[0]);
         **         }
         **     }
         **     break;
         **
         **
         **    //Y = a * b^(c*X+d) + e
         **    case 8:
         **        Val = (Params[0] * pow(Params[1], Params[2] * R + Params[3]) + Params[4]);
         **        break;
         **
         **
         **    // Y = (log((y-e) / a) / log(b) - d ) / c
         **    // a=0, b=1, c=2, d=3, e=4,
         **    case -8:
         **
         **        disc = R - Params[4];
         **        if (disc < 0) Val = 0;
         **        else
         **        {
         **            if (fabs(Params[0]) < MATRIX_DET_TOLERANCE ||
         **                fabs(Params[2]) < MATRIX_DET_TOLERANCE)
         **            {
         **                Val = 0;
         **            }
         **            else
         **            {
         **                Val = (log(disc / Params[0]) / log(Params[1]) - Params[3]) / Params[2];
         **            }
         **        }
         **        break;
         **
         **
         **    // S-Shaped: (1 - (1-x)^1/g)^1/g
         **    case 108:
         **        if (fabs(Params[0]) < MATRIX_DET_TOLERANCE)
         **            Val = 0;
         **        else
         **            Val = pow(1.0 - pow(1 - R, 1/Params[0]), 1/Params[0]);
         **       break;
         **
         **     // y = (1 - (1-x)^1/g)^1/g
         **     // y^g = (1 - (1-x)^1/g)
         **     // 1 - y^g = (1-x)^1/g
         **     // (1 - y^g)^g = 1 - x
         **     // 1 - (1 - y^g)^g
         **     case -108:
         **         Val = 1 - pow(1 - pow(R, Params[0]), Params[0]);
         **         break;
         **
         **     // Sigmoidals
         **     case 109:
         **         Val = sigmoid_factory(Params[0], R);
         **         break;
         **
         **     case -109:
         **         Val = inverse_sigmoid_factory(Params[0], R);
         **         break;
         **
         **     default:
         **         // Unsupported parametric curve. Should never reach here
         **         return 0;
         **     }
         **
         **     return Val;
         ** }
         **/

        double val, disc, e;

        var (p0, p1, p2, p3, p4, p5, p6) = @params.Length switch
        {
            0 => (0d, 0d, 0d, 0d, 0d, 0d, 0d),
            1 => (@params[0], 0d, 0d, 0d, 0d, 0d, 0d),
            2 => (@params[0], @params[1], 0d, 0d, 0d, 0d, 0d),
            3 => (@params[0], @params[1], @params[2], 0d, 0d, 0d, 0d),
            4 => (@params[0], @params[1], @params[2], @params[3], 0d, 0d, 0d),
            5 => (@params[0], @params[1], @params[2], @params[3], @params[4], 0d, 0d),
            6 => (@params[0], @params[1], @params[2], @params[3], @params[4], @params[5], 0d),
            _ => (@params[0], @params[1], @params[2], @params[3], @params[4], @params[5], @params[6]),
        };

        switch (type)
        {
            // X = Y ^ Gamma
            case 1:

                val = r < 0
                    ? (double)Abs(p0 - 1) < determinantTolerance
                        ? r
                        : 0
                    : Pow(r, p0);

                break;

            // Type 1 Reversed
            // X = Y ^ 1/Gamma
            case -1:

                val = r < 0
                    ? (double)Abs(p0 - 1) < determinantTolerance
                        ? r
                        : 0
                    : (double)Abs(p0) < determinantTolerance
                        ? plusInf
                        : Pow(r, 1 / p0);

                break;

            // CIE 122-1966
            // Y = (aX + b)^Gamma | X ≥ -b/a
            // Y = 0              | else
            case 2:

                if ((double)Abs(p1) < determinantTolerance)
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

            // Type 2 Reversed
            // X = (Y ^1/g - b) / a
            case -2:

                val = (double)Abs(p0) < determinantTolerance || (double)Abs(p1) < determinantTolerance
                    ? 0
                    : r < 0
                        ? 0
                        : Max((Pow(r, 1.0 / p0) - p2) / p1, 0); // Max is the same as "if (val < 0)" check

                break;

            // IEC 61966-3
            // Y = (aX + b)^Gamma | X ≤ -b/a
            // Y = c              | else
            case 3:

                if ((double)Abs(p1) < determinantTolerance)
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

            // Type 3 reversed
            // X = ((Y-c)^1/g - b)/a | Y ≥ c
            // X = -b/a              | Y < c
            case -3:

                if ((double)Abs(p1) < determinantTolerance)
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

            // IEC 61966-2.1 (sRGB)
            // Y = (aX + b)^Gamma | X ≥ d
            // Y = cX             | X < d
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

            // Type 4 reversed
            // X = ((Y^1/g-b)/a) | Y ≥ (ad+b)^g
            // X = Y/c           | Y < (ad+b)^g
            case -4:

                e = (p1 * p4) + p2;
                disc = e < 0
                    ? 0
                    : Pow(e, p0);

                val = r >= disc
                    ? (double)Abs(p0) < determinantTolerance || (double)Abs(p1) < determinantTolerance
                        ? 0
                        : (Pow(r, 1.0 / p0) - p2) / p1
                    : (double)Abs(p3) < determinantTolerance
                        ? 0
                        : r / p3;

                break;

            // Y = (aX + b)^Gamma + e | X ≥ d
            // Y = cX + f             | X < d
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

            // Reversed type 5
            // X = ((Y-e)1/g-b)/a | Y ≥ (ad+b)^g+e), cd+f
            // X = (Y-f)/c        | else
            case -5:

                disc = (p3 * p4) + p6;
                if (r >= disc)
                {
                    e = r - p5;
                    val = e < 0
                        ? 0
                        : (double)Abs(p0) < determinantTolerance || (double)Abs(p1) < determinantTolerance
                            ? 0
                            : (Pow(e, 1.0 / p0) - p2) / p1;
                }
                else
                {
                    val = (double)Abs(p3) < determinantTolerance
                        ? 0
                        : (r - p6) / p3;
                }

                break;

            // Types 6,7,8 comes from segmented curves as described in ICCSpecRevision_02_11_06_Float.pdf
            // Type 6 is basically identical to type 5 without d

            // Y = (a * X + b) ^ Gamma + c
            case 6:

                e = (p1 * r) + p2;

                val = e < 0
                    ? p3
                    : Pow(e, p0) + p3;

                break;

            // X = ((Y - c) ^1/Gamma - b) / a
            case -6:

                if ((double)Abs(p1) < determinantTolerance)
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

            //                Y = a * log (b * X^Gamma + c) + d
            // b * X ^Gamma + c = (Y - d) / a = log(b * X ^Gamma + c) pow(10, (Y-d) / a)
            //                X = pow((pow(10, (Y-d) / a) - c) / b, 1/g)
            case -7:

                val = (double)Abs(p0) < determinantTolerance || (double)Abs(p1) < determinantTolerance || (double)Abs(p2) < determinantTolerance
                    ? 0
                    : Pow((Pow(10.0, (r - p4) / p1) - p3) / p2, 1.0 / p0);

                break;

            //Y = a * b^(c*X+d) + e
            case 8:

                val = (p0 * Pow(p1, (p2 * r) + p3)) + p4;

                break;

            // Y = (log((y-e) / a) / log(b) - d ) / c
            // a = p0, b = p1, c = p2, d = p3, e = p4,
            case -8:

                disc = r - p4;
                val = disc < 0
                    ? 0
                    : (double)Abs(p0) < determinantTolerance || (double)Abs(p2) < determinantTolerance
                        ? 0
                        : ((Log(disc / p0) / Log(p1)) - p3) / p2;

                break;

            // S-Shaped: (1 - (1-x)^1/g)^1/g
            case 108:

                val = (double)Abs(p0) < determinantTolerance
                    ? 0
                    : Pow(1.0 - Pow(1 - r, 1 / p0), 1 / p0);

                break;

            //         Y = (1 - (1-X)^1/g)^1/g
            //       Y^g = (1 - (1-X)^1/g)
            //   1 - Y^g = (1-X)^1/g
            // (1-X)^1/g = 1 - Y^g
            //     1 - X = (1 - Y^g)^g
            //         X = 1 - (1 - Y^g)^g
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
        /** Original Code (cmsgamma.c line: )
         **
         ** // Get the surrounding nodes. This is tricky on non-monotonic tables
         ** static
         ** int GetInterval(cmsFloat64Number In, const cmsUInt16Number LutTable[], const struct _cms_interp_struc* p)
         ** {
         **     int i;
         **     int y0, y1;
         **
         **     // A 1 point table is not allowed
         **     if (p -> Domain[0] < 1) return -1;
         **
         **     // Let's see if ascending or descending.
         **     if (LutTable[0] < LutTable[p ->Domain[0]]) {
         **
         **         // Table is overall ascending
         **         for (i = (int) p->Domain[0] - 1; i >= 0; --i) {
         **
         **             y0 = LutTable[i];
         **             y1 = LutTable[i+1];
         **
         **             if (y0 <= y1) { // Increasing
         **                 if (In >= y0 && In <= y1) return i;
         **             }
         **             else
         **                 if (y1 < y0) { // Decreasing
         **                     if (In >= y1 && In <= y0) return i;
         **                 }
         **         }
         **     }
         **     else {
         **         // Table is overall descending
         **         for (i=0; i < (int) p -> Domain[0]; i++) {
         **
         **             y0 = LutTable[i];
         **             y1 = LutTable[i+1];
         **
         **             if (y0 <= y1) { // Increasing
         **                 if (In >= y0 && In <= y1) return i;
         **             }
         **             else
         **                 if (y1 < y0) { // Decreasing
         **                     if (In >= y1 && In <= y0) return i;
         **                 }
         **         }
         **     }
         **
         **     return -1;
         ** }
         **/

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

    private static void Smooth(float[] w, float[] y, float[] z, float lambda, int m)
    {
        /** Original Code (cmsgamma.c line: 1128)
         **
         ** // From: Eilers, P.H.C. (1994) Smoothing and interpolation with finite
         ** // differences. in: Graphic Gems IV, Heckbert, P.S. (ed.), Academic press.
         ** //
         ** // Smoothing and interpolation with second differences.
         ** //
         ** //   Input:  weights (w), data (y): vector from 1 to m.
         ** //   Input:  smoothing parameter (lambda), length (m).
         ** //   Output: smoothed vector (z): vector from 1 to m.
         **
         ** static
         ** cmsBool smooth2(cmsContext ContextID, cmsFloat32Number w[], cmsFloat32Number y[],
         **                 cmsFloat32Number z[], cmsFloat32Number lambda, int m)
         ** {
         **     int i, i1, i2;
         **     cmsFloat32Number *c, *d, *e;
         **     cmsBool st;
         **
         **
         **     c = (cmsFloat32Number*) _cmsCalloc(ContextID, MAX_NODES_IN_CURVE, sizeof(cmsFloat32Number));
         **     d = (cmsFloat32Number*) _cmsCalloc(ContextID, MAX_NODES_IN_CURVE, sizeof(cmsFloat32Number));
         **     e = (cmsFloat32Number*) _cmsCalloc(ContextID, MAX_NODES_IN_CURVE, sizeof(cmsFloat32Number));
         **
         **     if (c != NULL && d != NULL && e != NULL) {
         **
         **
         **     d[1] = w[1] + lambda;
         **     c[1] = -2 * lambda / d[1];
         **     e[1] = lambda /d[1];
         **     z[1] = w[1] * y[1];
         **     d[2] = w[2] + 5 * lambda - d[1] * c[1] *  c[1];
         **     c[2] = (-4 * lambda - d[1] * c[1] * e[1]) / d[2];
         **     e[2] = lambda / d[2];
         **     z[2] = w[2] * y[2] - c[1] * z[1];
         **
         **     for (i = 3; i < m - 1; i++) {
         **         i1 = i - 1; i2 = i - 2;
         **         d[i]= w[i] + 6 * lambda - c[i1] * c[i1] * d[i1] - e[i2] * e[i2] * d[i2];
         **         c[i] = (-4 * lambda -d[i1] * c[i1] * e[i1])/ d[i];
         **         e[i] = lambda / d[i];
         **         z[i] = w[i] * y[i] - c[i1] * z[i1] - e[i2] * z[i2];
         **     }
         **
         **     i1 = m - 2; i2 = m - 3;
         **
         **     d[m - 1] = w[m - 1] + 5 * lambda -c[i1] * c[i1] * d[i1] - e[i2] * e[i2] * d[i2];
         **     c[m - 1] = (-2 * lambda - d[i1] * c[i1] * e[i1]) / d[m - 1];
         **     z[m - 1] = w[m - 1] * y[m - 1] - c[i1] * z[i1] - e[i2] * z[i2];
         **     i1 = m - 1; i2 = m - 2;
         **
         **     d[m] = w[m] + lambda - c[i1] * c[i1] * d[i1] - e[i2] * e[i2] * d[i2];
         **     z[m] = (w[m] * y[m] - c[i1] * z[i1] - e[i2] * z[i2]) / d[m];
         **     z[m - 1] = z[m - 1] / d[m - 1] - c[m - 1] * z[m];
         **
         **     for (i = m - 2; 1<= i; i--)
         **         z[i] = z[i] / d[i] - c[i] * z[i + 1] - e[i] * z[i + 2];
         **
         **       st = TRUE;
         **     }
         **     else st = FALSE;
         **
         **     if (c != NULL) _cmsFree(ContextID, c);
         **     if (d != NULL) _cmsFree(ContextID, d);
         **     if (e != NULL) _cmsFree(ContextID, e);
         **
         **     return st;
         ** }
         **/

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
    }

    private double EvalSegmentedFn(double r)
    {
        /** Original Code (cmsgamma.c line: 710)
         **
         ** // Evaluate a segmented function for a single value. Return -Inf if no valid segment found .
         ** // If fn type is 0, perform an interpolation on the table
         ** static
         ** cmsFloat64Number EvalSegmentedFn(const cmsToneCurve *g, cmsFloat64Number R)
         ** {
         **     int i;
         **     cmsFloat32Number Out32;
         **     cmsFloat64Number Out;
         **
         **     for (i = (int) g->nSegments - 1; i >= 0; --i) {
         **
         **         // Check for domain
         **         if ((R > g->Segments[i].x0) && (R <= g->Segments[i].x1)) {
         **
         **             // Type == 0 means segment is sampled
         **             if (g->Segments[i].Type == 0) {
         **
         **                 cmsFloat32Number R1 = (cmsFloat32Number)(R - g->Segments[i].x0) / (g->Segments[i].x1 - g->Segments[i].x0);
         **
         **                 // Setup the table (TODO: clean that)
         **                 g->SegInterp[i]->Table = g->Segments[i].SampledPoints;
         **
         **                 g->SegInterp[i]->Interpolation.LerpFloat(&R1, &Out32, g->SegInterp[i]);
         **                 Out = (cmsFloat64Number) Out32;
         **
         **             }
         **             else {
         **                 Out = g->Evals[i](g->Segments[i].Type, g->Segments[i].Params, R);
         **             }
         **
         **             if (isinf(Out))
         **                 return PLUS_INF;
         **             else
         **             {
         **                 if (isinf(-Out))
         **                     return MINUS_INF;
         **             }
         **
         **             return Out;
         **         }
         **     }
         **
         **     return MINUS_INF;
         ** }
         **/

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
