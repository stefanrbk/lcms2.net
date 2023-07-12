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

namespace lcms2.testbed;

internal static unsafe partial class Testbed
{
    private static bool CheckGammaEstimation(ToneCurve c, double g)
    {
        var est = cmsEstimateGamma(c, 0.001);

        SubTest("Gamma estimation");
        return Math.Abs(est - g) <= 0.001;
    }

    public static bool CheckGammaCreation16()
    {
        var LinGamma = cmsBuildGamma(DbgThread(), 1.0);

        for (var i = 0; i < 0xffff; i++)
        {
            var @in = (ushort)i;
            var @out = cmsEvalToneCurve16(LinGamma, @in);
            if (@in != @out)
            {
                Fail($"(lin gamma): Must be {@in}, but is {@out} : ");
                cmsFreeToneCurve(LinGamma);
                return false;
            }
        }

        var rc = CheckGammaEstimation(LinGamma, 1.0);

        cmsFreeToneCurve(LinGamma);
        return rc;
    }

    public static bool CheckGammaCreationFlt()
    {
        var LinGamma = cmsBuildGamma(DbgThread(), 1.0);

        for (var i = 0; i < 0xffff; i++)
        {
            var @in = i / 65535f;
            var @out = cmsEvalToneCurveFloat(LinGamma, @in);
            if (MathF.Abs(@in - @out) > 1 / 65535f)
            {
                Fail($"(lin gamma): Must be {@in}, but is {@out} : ");
                cmsFreeToneCurve(LinGamma);
                return false;
            }
        }

        var rc = CheckGammaEstimation(LinGamma, 1.0);

        cmsFreeToneCurve(LinGamma);
        return rc;
    }

    private static bool CheckGammaFloat(double g)
    {
        var Curve = cmsBuildGamma(DbgThread(), g);

        MaxErr = 0.0;
        for (var i = 0; i < 0xffff; i++)
        {
            var @in = i / 65535f;
            var @out = cmsEvalToneCurveFloat(Curve, @in);
            var val = Math.Pow(@in, g);

            var Err = Math.Abs(val - @out);
            if (Err > MaxErr) MaxErr = Err;
        }

        if (MaxErr > 0) ConsoleWrite($"|Err|<{MaxErr * 65535} ");

        var rc = CheckGammaEstimation(Curve, g);
        cmsFreeToneCurve(Curve);
        return rc;
    }

    public static bool CheckGamma18() =>
        CheckGammaFloat(1.8);

    public static bool CheckGamma22() =>
        CheckGammaFloat(2.2);

    public static bool CheckGamma30() =>
        CheckGammaFloat(3.0);

    private static bool CheckGammaFloatTable(double g)
    {
        var Values = new float[1025];

        for (var i = 0; i <= 1024; i++)
        {
            var @in = i / 1024f;
            Values[i] = MathF.Pow(@in, (float)g);
        }

        var Curve = cmsBuildTabulatedToneCurveFloat(DbgThread(), 1025, Values);

        MaxErr = 0.0;
        for (var i = 0; i <= 0xffff; i++)
        {
            var @in = i / 65535f;
            var @out = cmsEvalToneCurveFloat(Curve, @in);
            var val = Math.Pow(@in, g);

            var err = Math.Abs(val - @out);
            if (err > MaxErr) MaxErr = err;
        }

        if (MaxErr > 0) ConsoleWrite($"|Err|<{MaxErr * 65535} ");

        var rc = CheckGammaEstimation(Curve, g);
        cmsFreeToneCurve(Curve);
        return rc;
    }

    public static bool CheckGamma18Table() =>
        CheckGammaFloatTable(1.8);

    public static bool CheckGamma22Table() =>
        CheckGammaFloatTable(2.2);

    public static bool CheckGamma30Table() =>
        CheckGammaFloatTable(3.0);

    private static bool CheckGammaWordTable(double g)
    {
        var Values = stackalloc ushort[1025];

        for (var i = 0; i <= 1024; i++)
        {
            var @in = i / 1024f;
            Values[i] = (ushort)Math.Floor((Math.Pow(@in, g) * 65535.0) + 0.5);
        }

        var Curve = cmsBuildTabulatedToneCurve16(DbgThread(), 1025, Values);

        MaxErr = 0.0;
        for (var i = 0; i <= 0xffff; i++)
        {
            var @in = i / 65535f;
            var @out = cmsEvalToneCurveFloat(Curve, @in);
            var val = Math.Pow(@in, g);

            var err = Math.Abs(val - @out);
            if (err > MaxErr) MaxErr = err;
        }

        if (MaxErr > 0) ConsoleWrite($"|Err|<{MaxErr * 65535} ");

        var rc = CheckGammaEstimation(Curve, g);
        cmsFreeToneCurve(Curve);
        return rc;
    }

    public static bool CheckGamma18TableWord() =>
        CheckGammaWordTable(1.8);

    public static bool CheckGamma22TableWord() =>
        CheckGammaWordTable(2.2);

    public static bool CheckGamma30TableWord() =>
        CheckGammaWordTable(3.0);

    public static bool CheckJointCurves()
    {
        var Forward = cmsBuildGamma(DbgThread(), 3.0);
        var Reverse = cmsBuildGamma(DbgThread(), 3.0);

        var Result = cmsJoinToneCurve(DbgThread(), Forward, Reverse, 256);

        cmsFreeToneCurve(Forward); cmsFreeToneCurve(Reverse);

        var rc = cmsIsToneCurveLinear(Result);
        cmsFreeToneCurve(Result);

        if (!rc) Fail("Joining same curve twice does not result in a linear ramp");

        return rc;
    }

    private static ToneCurve GammaTableLinear(uint nEntries, bool Dir)
    {
        var g = cmsBuildTabulatedToneCurve16(DbgThread(), nEntries, null);

        for (var i = 0; i < nEntries; i++)
        {
            var v = _cmsQuantizeVal(i, nEntries);

            g.Table16[i] =
                Dir
                    ? v
                    : (ushort)(0xFFFF - v);
        }

        return g;
    }

    public static bool CheckJointCurvesDescending()
    {
        var Forward = cmsBuildGamma(DbgThread(), 2.2);

        // Fake the curve to be table-based
        for (var i = 0; i < 4096; i++)
            Forward.Table16[i] = (ushort)(0xFFFF - Forward.Table16[i]);
        Forward.Segments[0].Type = 0;

        var Reverse = cmsReverseToneCurve(Forward);

        var Result = cmsJoinToneCurve(DbgThread(), Reverse, Reverse, 256);

        cmsFreeToneCurve(Forward); cmsFreeToneCurve(Reverse);

        var rc = cmsIsToneCurveLinear(Result);
        cmsFreeToneCurve(Result);

        return rc;
    }

    private static bool CheckFToneCurvePoint(ToneCurve c, ushort Point, int Value) =>
        Math.Abs(Value - cmsEvalToneCurve16(c, Point)) < 2;

    public static bool CheckReverseDegenerated()
    {
        var Tab = stackalloc ushort[16];

        Tab[0] = 0;
        Tab[1] = 0;
        Tab[2] = 0;
        Tab[3] = 0;
        Tab[4] = 0;
        Tab[5] = 0x5555;
        Tab[6] = 0x6666;
        Tab[7] = 0x7777;
        Tab[8] = 0x8888;
        Tab[9] = 0x9999;
        Tab[10] = 0xffff;
        Tab[11] = 0xffff;
        Tab[12] = 0xffff;
        Tab[13] = 0xffff;
        Tab[14] = 0xffff;
        Tab[15] = 0xffff;

        var p = cmsBuildTabulatedToneCurve16(DbgThread(), 16, Tab);
        var g = cmsReverseToneCurve(p);

        // Now let's check some points
        var rc = true;
        rc = rc && CheckFToneCurvePoint(g, 0x5555, 0x5555);
        rc = rc && CheckFToneCurvePoint(g, 0x7777, 0x7777);

        // First point for zero
        rc = rc && CheckFToneCurvePoint(g, 0x0000, 0x4444);

        // Last Point
        rc = rc && CheckFToneCurvePoint(g, 0xFFFF, 0xFFFF);

        cmsFreeToneCurve(p);
        cmsFreeToneCurve(g);

        return rc;
    }

    private static ToneCurve Build_sRGBGamma()
    {
        var Parameters = stackalloc double[5]
        {
            2.4,
            1 / 1.055,
            0.055 / 1.055,
            1 / 12.92,
            0.04045,
        };

        return cmsBuildParametricToneCurve(DbgThread(), 4, Parameters)!;
    }

    private static ToneCurve CombineGammaFloat(ToneCurve g1, ToneCurve g2)
    {
        var Tab = stackalloc ushort[256];

        for (var i = 0; i < 256; i++)
        {
            var f = i / 255f;
            f = cmsEvalToneCurveFloat(g2, cmsEvalToneCurveFloat(g1, f));

            Tab[i] = (ushort)Math.Floor((f * 65535) + 0.5);
        }

        return cmsBuildTabulatedToneCurve16(DbgThread(), 256, Tab);
    }

    private static ToneCurve CombineGamma16(ToneCurve g1, ToneCurve g2)
    {
        var Tab = stackalloc ushort[256];

        for (var i = 0; i < 256; i++)
        {
            var wValIn = _cmsQuantizeVal(i, 256);

            Tab[i] = cmsEvalToneCurve16(g2, cmsEvalToneCurve16(g1, wValIn));
        }

        return cmsBuildTabulatedToneCurve16(DbgThread(), 256, Tab);
    }

    public static bool CheckJointFloatCurves_sRGB()
    {
        var Forward = Build_sRGBGamma();
        var Reverse = cmsReverseToneCurve(Forward);
        var Result = CombineGammaFloat(Forward, Reverse);
        cmsFreeToneCurve(Forward); cmsFreeToneCurve(Reverse);

        var rc = cmsIsToneCurveLinear(Result);
        cmsFreeToneCurve(Result);
        return rc;
    }

    public static bool CheckJoint16Curves_sRGB()
    {
        var Forward = Build_sRGBGamma();
        var Reverse = cmsReverseToneCurve(Forward);
        var Result = CombineGamma16(Forward, Reverse);
        cmsFreeToneCurve(Forward); cmsFreeToneCurve(Reverse);

        var rc = cmsIsToneCurveLinear(Result);
        cmsFreeToneCurve(Result);
        return rc;
    }

    public static bool CheckJointCurvesSShaped()
    {
        var p = 3.2;
        var Forward = cmsBuildParametricToneCurve(DbgThread(), 108, &p);
        var Reverse = cmsReverseToneCurve(Forward);
        var Result = cmsJoinToneCurve(DbgThread(), Forward, Forward, 4096);
        cmsFreeToneCurve(Forward); cmsFreeToneCurve(Reverse);

        var rc = cmsIsToneCurveLinear(Result);
        cmsFreeToneCurve(Result);
        return rc;
    }

    private static float Gamma(float x, in double* Params) =>
        (float)Math.Pow(x, Params[0]);

    private static float CIE122(float x, in double* Params)
    {
        double Val;

        if (x >= -Params[2] / Params[1])
        {
            var e = (Params[1] * x) + Params[2];

            Val =
                e > 0
                    ? Math.Pow(e, Params[0])
                    : 0;
        }
        else
        {
            Val = 0;
        }

        return (float)Val;
    }

    private static float IEC61966_3(float x, in double* Params)
    {
        double Val;

        if (x >= -Params[2] / Params[1])
        {
            var e = (Params[1] * x) + Params[2];

            Val =
                e > 0
                    ? Math.Pow(e, Params[0]) + Params[3]
                    : 0;
        }
        else
        {
            Val = Params[3];
        }

        return (float)Val;
    }

    private static float IEC61966_21(float x, in double* Params)
    {
        double Val;

        if (x >= Params[4])
        {
            var e = (Params[1] * x) + Params[2];

            Val =
                e > 0
                    ? Math.Pow(e, Params[0])
                    : 0;
        }
        else
        {
            Val = x * Params[3];
        }

        return (float)Val;
    }

    private static float param_5(float x, in double* Params)
    {
        double Val;
        // Y = (aX + b)^Gamma + e | X >= d
        // Y = cX + f             | else
        if (x >= Params[4])
        {
            var e = (Params[1] * x) + Params[2];
            Val =
                e > 0
                    ? Math.Pow(e, Params[0]) + Params[5]
                    : 0;
        }
        else
        {
            Val = (x * Params[3]) + Params[6];
        }

        return (float)Val;
    }

    private static float param_6(float x, in double* Params)
    {
        double Val;

        var e = (Params[1] * x) + Params[2];
        Val =
            e > 0
                ? Math.Pow(e, Params[0]) + Params[3]
                : 0;

        return (float)Val;
    }

    private static float param_7(float x, in double* Params) =>
        (float)((Params[1] * Math.Log10((Params[2] * Math.Pow(x, Params[0])) + Params[3])) + Params[4]);

    private static float param_8(float x, in double* Params) =>
        (float)((Params[0] * Math.Pow(Params[1], (Params[2] * x) + Params[3])) + Params[4]);

    private static float sigmoidal(float x, in double* Params) =>
        (float)Math.Pow(1.0 - Math.Pow(1 - x, 1 / Params[0]), 1 / Params[0]);

    private static bool CheckSingleParametric(string Name, delegate*<float, in double*, float> fn, int Type, in double* Params)
    {
        var tc = cmsBuildParametricToneCurve(DbgThread(), Type, Params);
        var tc_1 = cmsBuildParametricToneCurve(DbgThread(), -Type, Params);

        for (var i = 0; i <= 1000; i++)
        {
            var x = i / 1000f;

            var y_fn = fn(x, Params);
            var y_param = cmsEvalToneCurveFloat(tc, x);
            var x_param = cmsEvalToneCurveFloat(tc_1, y_param);

            var y_param2 = fn(x_param, Params);

            if (!IsGoodVal(Name, y_fn, y_param, FIXED_PRECISION_15_16)) goto Error;

            if (!IsGoodVal($"Inverse {Name}", y_fn, y_param2, FIXED_PRECISION_15_16)) goto Error;
        }

        cmsFreeToneCurve(tc);
        cmsFreeToneCurve(tc_1);
        return true;

    Error:
        cmsFreeToneCurve(tc);
        cmsFreeToneCurve(tc_1);
        return false;
    }

    public static bool CheckParametricToneCurves()
    {
        var Params = stackalloc double[10];

        // 1) X = Y ^ Gamma

        Params[0] = 2.2;

        if (!CheckSingleParametric("Gamma", &Gamma, 1, Params)) return false;

        // 2) CIE 122-1966
        // Y = (aX + b)^Gamma  | X >= -b/a
        // Y = 0               | else

        Params[0] = 2.2;
        Params[1] = 1.5;
        Params[2] = -0.5;

        if (!CheckSingleParametric("CIE122-1966", &CIE122, 2, Params)) return false;

        // 3) IEC 61966-3
        // Y = (aX + b)^Gamma | X <= -b/a
        // Y = c              | else

        Params[0] = 2.2;
        Params[1] = 1.5;
        Params[2] = -0.5;
        Params[3] = 0.3;

        if (!CheckSingleParametric("IEC 61966-3", &IEC61966_3, 3, Params)) return false;

        // 4) IEC 61966-2.1 (sRGB)
        // Y = (aX + b)^Gamma | X >= d
        // Y = cX             | X < d

        Params[0] = 2.4;
        Params[1] = 1 / 1.055;
        Params[2] = 0.055 / 1.055;
        Params[3] = 1 / 12.92;
        Params[4] = 0.04045;

        if (!CheckSingleParametric("IEC 61966-2.1", &IEC61966_21, 4, Params)) return false;

        // 5) Y = (aX + b)^Gamma + e | X >= d
        // Y = cX + f             | else

        Params[0] = 2.2;
        Params[1] = 0.7;
        Params[2] = 0.2;
        Params[3] = 0.3;
        Params[4] = 0.1;
        Params[5] = 0.5;
        Params[6] = 0.2;

        if (!CheckSingleParametric("param_5", &param_5, 5, Params)) return false;

        // 6) Y = (aX + b) ^ Gamma + c

        Params[0] = 2.2;
        Params[1] = 0.7;
        Params[2] = 0.2;
        Params[3] = 0.3;

        if (!CheckSingleParametric("param_6", &param_6, 6, Params)) return false;

        // 7) Y = a * log (b * X^Gamma + c) + d

        Params[0] = 2.2;
        Params[1] = 0.9;
        Params[2] = 0.9;
        Params[3] = 0.02;
        Params[4] = 0.1;

        if (!CheckSingleParametric("param_7", &param_7, 7, Params)) return false;

        // 8) Y = a * b ^ (c*X+d) + e

        Params[0] = 0.9;
        Params[1] = 0.9;
        Params[2] = 1.02;
        Params[3] = 0.1;
        Params[4] = 0.2;

        if (!CheckSingleParametric("param_8", &param_8, 8, Params)) return false;

        // 108: S-Shaped: (1 - (1-x)^1/g)^1/g

        Params[0] = 1.9;
        if (!CheckSingleParametric("sigmoidal", &sigmoidal, 108, Params)) return false;

        // All OK

        return true;
    }
}
