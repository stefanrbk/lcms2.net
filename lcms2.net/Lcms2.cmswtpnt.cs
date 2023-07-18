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

using System.Diagnostics;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static readonly CIEXYZ D50XYZ = new() { X = cmsD50X, Y = cmsD50Y, Z = cmsD50Z };
    private static readonly CIExyY D50xyY;

    [DebuggerStepThrough]
    public static CIEXYZ* cmsD50_XYZ()
    {
        fixed (CIEXYZ* xyz = &D50XYZ)
            return xyz;
    }

    [DebuggerStepThrough]
    public static CIExyY* cmsD50_xyY()
    {
        fixed (CIExyY* xyy = &D50xyY)
            return xyy;
    }

    public static bool cmsWhitePointFromTemp(CIExyY* WhitePoint, double TempK)

    {
        double x;
        _cmsAssert(WhitePoint);

        var T = TempK;
        var T2 = T * T;            // Square
        var T3 = T2 * T;           // Cube

        // For correlated color temperature (T) between 4000K and 7000K:

        if (T is >= 4000 and <= 7000)
        {
            x = (-4.6070 * (1E9 / T3)) + (2.9678 * (1E6 / T2)) + (0.09911 * (1E3 / T)) + 0.244063;
        }
        else
            // or for correlated color temperature (T) between 7000K and 25000K:

            if (T is > 7000.0 and <= 25000.0)
        {
            x = (-2.0064 * (1E9 / T3)) + (1.9018 * (1E6 / T2)) + (0.24748 * (1E3 / T)) + 0.237040;
        }
        else
        {
            cmsSignalError(null, ErrorCode.Range, "cmsWhitePointFromTemp: invalid temp");
            return false;
        }

        // Obtain y(x)
        var y = (-3.000 * (x * x)) + (2.870 * x) - 0.275;

        // wave factors (not used, but here for futures extensions)

        // M1 = (-1.3515 - 1.7703*x + 5.9114 *y)/(0.0241 + 0.2562*x - 0.7341*y);
        // M2 = (0.0300 - 31.4424*x + 30.0717*y)/(0.0241 + 0.2562*x - 0.7341*y);

        WhitePoint->x = x;
        WhitePoint->y = y;
        WhitePoint->Y = 1.0;

        return true;
    }

    private struct ISOTEMPERATURE
    {
        public double mirek;
        public double ut;
        public double vt;
        public double tt;
    }

    private static readonly ISOTEMPERATURE[] isotempdata = {
            new() {mirek = 0,     ut = 0.18006,  vt = 0.26352,  tt = -0.24341},
            new() {mirek = 10,    ut = 0.18066,  vt = 0.26589,  tt = -0.25479},
            new() {mirek = 20,    ut = 0.18133,  vt = 0.26846,  tt = -0.26876},
            new() {mirek = 30,    ut = 0.18208,  vt = 0.27119,  tt = -0.28539},
            new() {mirek = 40,    ut = 0.18293,  vt = 0.27407,  tt = -0.30470},
            new() {mirek = 50,    ut = 0.18388,  vt = 0.27709,  tt = -0.32675},
            new() {mirek = 60,    ut = 0.18494,  vt = 0.28021,  tt = -0.35156},
            new() {mirek = 70,    ut = 0.18611,  vt = 0.28342,  tt = -0.37915},
            new() {mirek = 80,    ut = 0.18740,  vt = 0.28668,  tt = -0.40955},
            new() {mirek = 90,    ut = 0.18880,  vt = 0.28997,  tt = -0.44278},
            new() {mirek = 100,   ut = 0.19032,  vt = 0.29326,  tt = -0.47888},
            new() {mirek = 125,   ut = 0.19462,  vt = 0.30141,  tt = -0.58204},
            new() {mirek = 150,   ut = 0.19962,  vt = 0.30921,  tt = -0.70471},
            new() {mirek = 175,   ut = 0.20525,  vt = 0.31647,  tt = -0.84901},
            new() {mirek = 200,   ut = 0.21142,  vt = 0.32312,  tt = -1.0182 },
            new() {mirek = 225,   ut = 0.21807,  vt = 0.32909,  tt = -1.2168 },
            new() {mirek = 250,   ut = 0.22511,  vt = 0.33439,  tt = -1.4512 },
            new() {mirek = 275,   ut = 0.23247,  vt = 0.33904,  tt = -1.7298 },
            new() {mirek = 300,   ut = 0.24010,  vt = 0.34308,  tt = -2.0637 },
            new() {mirek = 325,   ut = 0.24702,  vt = 0.34655,  tt = -2.4681 },
            new() {mirek = 350,   ut = 0.25591,  vt = 0.34951,  tt = -2.9641 },
            new() {mirek = 375,   ut = 0.26400,  vt = 0.35200,  tt = -3.5814 },
            new() {mirek = 400,   ut = 0.27218,  vt = 0.35407,  tt = -4.3633 },
            new() {mirek = 425,   ut = 0.28039,  vt = 0.35577,  tt = -5.3762 },
            new() {mirek = 450,   ut = 0.28863,  vt = 0.35714,  tt = -6.7262 },
            new() {mirek = 475,   ut = 0.29685,  vt = 0.35823,  tt = -8.5955 },
            new() {mirek = 500,   ut = 0.30505,  vt = 0.35907,  tt = -11.324 },
            new() {mirek = 525,   ut = 0.31320,  vt = 0.35968,  tt = -15.628 },
            new() {mirek = 550,   ut = 0.32129,  vt = 0.36011,  tt = -23.325 },
            new() {mirek = 575,   ut = 0.32931,  vt = 0.36038,  tt = -40.770 },
            new() {mirek = 600,   ut = 0.33724,  vt = 0.36051,  tt = -116.45 },
    };

    private static uint NISO =>
        (uint)isotempdata.Length;

    public static bool cmsTempFromWhitePoint(double* TempK, in CIExyY* WhitePoint)
    {
        _cmsAssert(WhitePoint);
        _cmsAssert(TempK);

        var di = 0.0;
        var mi = 0.0;
        var xs = WhitePoint->x;
        var ys = WhitePoint->y;

        // convert (x,y) to CIE 1960 (u,WhitePoint)

        var us = 2 * xs / (-xs + (6 * ys) + 1.5);
        var vs = 3 * ys / (-xs + (6 * ys) + 1.5);

        for (var j = 0; j < NISO; j++)
        {
            var uj = isotempdata[j].ut;
            var vj = isotempdata[j].vt;
            var tj = isotempdata[j].tt;
            var mj = isotempdata[j].mirek;

            var dj = (vs - vj - (tj * (us - uj))) / Math.Sqrt(1.0 + (tj * tj));

            if ((j != 0) && (di / dj < 0.0))
            {
                // Found a match
                *TempK = 1000000.0 / (mi + (di / (di - dj) * (mj - mi)));
                return true;
            }

            di = dj;
            mi = mj;
        }

        // Not found
        return false;
    }

    private static bool ComputeChromaticAdaptation(
        MAT3* Conversion,
        in CIEXYZ* SourceWhitePoint,
        in CIEXYZ* DestWhitePoint,
        in MAT3* Chad)
    {
        MAT3 Chad_Inv, Cone;
        VEC3 ConeSourceXYZ, ConeSourceRGB, ConeDestXYZ, ConeDestRGB;

        var Tmp = *Chad;
        if (!_cmsMAT3inverse(&Tmp, &Chad_Inv)) return false;

        _cmsVEC3init(&ConeSourceXYZ, SourceWhitePoint->X, SourceWhitePoint->Y, SourceWhitePoint->Z);

        _cmsVEC3init(&ConeDestXYZ, DestWhitePoint->X, DestWhitePoint->Y, DestWhitePoint->Z);

        _cmsMAT3eval(&ConeSourceRGB, Chad, &ConeSourceXYZ);
        _cmsMAT3eval(&ConeDestRGB, Chad, &ConeDestXYZ);

        // Build matrix
        _cmsVEC3init(&Cone.X, ConeDestRGB.X / ConeSourceRGB.X, 0.0, 0.0);
        _cmsVEC3init(&Cone.Y, 0.0, ConeDestRGB.Y / ConeSourceRGB.Y, 0.0);
        _cmsVEC3init(&Cone.Z, 0.0, 0.0, ConeDestRGB.Z / ConeSourceRGB.Z);

        // Normalize
        _cmsMAT3per(&Tmp, &Cone, Chad);
        _cmsMAT3per(Conversion, &Chad_Inv, &Tmp);

        return true;
    }

    internal static bool _cmsAdaptationMatrix(MAT3* r, in MAT3* ConeMatrix, in CIEXYZ* FromIll, in CIEXYZ* ToIll)
    {
        var LamRigg = stackalloc double[]
        {
            0.8951,
            0.2664,
            -0.1614,
            -0.7502,
            1.7135,
            0.0367,
            0.0389,
            -0.0685,
            1.0296,
        };

        var _coneMatrix = (ConeMatrix is null) ? (MAT3*)LamRigg : ConeMatrix;

        return ComputeChromaticAdaptation(r, FromIll, ToIll, _coneMatrix);
    }

    internal static bool _cmsAdaptMatrixToD50(MAT3* r, in CIExyY* SourceWhitePt)
    {
        CIEXYZ Dn;
        MAT3 Bradford;

        cmsxyY2XYZ(&Dn, SourceWhitePt);

        if (!_cmsAdaptationMatrix(&Bradford, null, &Dn, cmsD50_XYZ())) return false;

        var Tmp = *r;
        _cmsMAT3per(r, &Bradford, &Tmp);

        return true;
    }

    internal static bool _cmsBuildRGB2XYZtransferMatrix(MAT3* r, in CIExyY* WhitePt, in CIExyYTRIPLE* Primrs)
    {
        VEC3* rv = &r->X;
        VEC3 WhitePoint, Coef;
        double* Coefn = &Coef.X;
        MAT3 Result, Primaries;

        var xn = WhitePt->x;
        var yn = WhitePt->y;
        var xr = Primrs->Red.x;
        var yr = Primrs->Red.y;
        var xg = Primrs->Green.x;
        var yg = Primrs->Green.y;
        var xb = Primrs->Blue.x;
        var yb = Primrs->Blue.y;

        // Build Primaries matrix
        _cmsVEC3init(&Primaries.X, xr, xg, xb);
        _cmsVEC3init(&Primaries.Y, yr, yg, yb);
        _cmsVEC3init(&Primaries.Z, 1 - xr - yr, 1 - xg - yg, 1 - xb - yb);

        // Result = Primaries ^ (-1) inverse matrix
        if (!_cmsMAT3inverse(&Primaries, &Result)) return false;

        _cmsVEC3init(&WhitePoint, xn / yn, 1.0, (1.0 - xn - yn) / yn);

        // Across inverse primaries...
        _cmsMAT3eval(&Coef, &Result, &WhitePoint);

        // Give us the Coefs, then I build transformation matrix
        _cmsVEC3init(&rv[0], Coefn[VX] * xr, Coefn[VY] * xg, Coefn[VZ] * xb);
        _cmsVEC3init(&rv[1], Coefn[VX] * yr, Coefn[VY] * yg, Coefn[VZ] * yb);
        _cmsVEC3init(&rv[2], Coefn[VX] * (1.0 - xr - yr), Coefn[VY] * (1.0 - xg - yg), Coefn[VZ] * (1.0 - xb - yb));

        return _cmsAdaptMatrixToD50(r, WhitePt);
    }

    public static bool cmsAdaptToIlluminant(
        CIEXYZ* Result,
        in CIEXYZ* SourceWhitePt,
        in CIEXYZ* Illuminant,
        in CIEXYZ* Value)
    {
        MAT3 Bradford;
        VEC3 In, Out;
        double* Outn = &Out.X;

        _cmsAssert(Result);
        _cmsAssert(SourceWhitePt);
        _cmsAssert(Illuminant);
        _cmsAssert(Value);

        if (!_cmsAdaptationMatrix(&Bradford, null, SourceWhitePt, Illuminant)) return false;

        _cmsVEC3init(&In, Value->X, Value->Y, Value->Z);
        _cmsMAT3eval(&Out, &Bradford, &In);

        Result->X = Outn[0];
        Result->Y = Outn[1];
        Result->Z = Outn[2];

        return true;
    }
}
