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
//using lcms2.old.types;

//namespace lcms2.testbed;

//public static class ToneCurveTests
//{
//    #region Public Methods

//    public static bool CheckGamma18() =>
//        CheckGamma(1.8);

//    public static bool CheckGamma18Table() =>
//        CheckGammaFloatTable(1.8);

//    public static bool CheckGamma18TableWord() =>
//        CheckGammaWordTable(1.8);

//    public static bool CheckGamma22() =>
//            CheckGamma(2.2);

//    public static bool CheckGamma22Table() =>
//        CheckGammaFloatTable(2.2);

//    public static bool CheckGamma22TableWord() =>
//        CheckGammaWordTable(2.2);

//    public static bool CheckGamma30() =>
//            CheckGamma(3.0);

//    public static bool CheckGamma30Table() =>
//        CheckGammaFloatTable(3.0);

//    public static bool CheckGamma30TableWord() =>
//        CheckGammaWordTable(3.0);

//    public static bool CheckGammaCreation16()
//    {
//        using var linGamma = ToneCurve.BuildGamma(null, 1.0);
//        if (linGamma is null) return false;

//        for (var i = 0; i < 0xFFFF; i++)
//        {
//            var @in = (ushort)i;
//            var @out = linGamma.Eval(@in);
//            if (@in != @out)
//            {
//                Fail($"(lin gamma): Must be {@in}, but was {@out} : ");
//                return false;
//            }
//        }

//        return CheckGammaEstimation(linGamma, 1.0);
//    }

//    public static bool CheckGammaCreationFloat()
//    {
//        using var linGamma = ToneCurve.BuildGamma(null, 1.0);
//        if (linGamma is null) return false;

//        for (var i = 0; i < 0xFFFF; i++)
//        {
//            var @in = i / 65535f;
//            var @out = linGamma.Eval(@in);
//            if (Math.Abs(@in - @out) > (1 / 65535f))
//            {
//                Fail($"(lin gamma): Must be {@in}, but was {@out} : ");
//                return false;
//            }
//        }

//        return CheckGammaEstimation(linGamma, 1.0);
//    }

//    public static bool CheckJoint16CurvesSrgb()
//    {
//        using var forward = BuildSrgbGamma();
//        if (forward is null)
//            return false;

//        using var reverse = forward.Reverse();
//        if (reverse is null)
//            return false;

//        using var result = CombineGamma16(forward, reverse);
//        if (result is null)
//            return false;

//        return result.IsLinear;
//    }

//    public static bool CheckJointCurves()
//    {
//        using var forward = ToneCurve.BuildGamma(null, 3.0);
//        using var reverse = ToneCurve.BuildGamma(null, 3.0);

//        if (forward is null || reverse is null)
//            return false;

//        using var result = forward.Join(null, reverse, 256);

//        if (result is null)
//            return false;

//        var rc = result.IsLinear;

//        if (!rc)
//            Fail("Joining same curve twice does not result in a linear ramp");

//        return rc;
//    }

//    public static bool CheckJointCurvesDescending()
//    {
//        using var forward = ToneCurve.BuildGamma(null, 2.2);
//        if (forward is null)
//            return false;

//        // Fake the curve to be table-based

//        for (var i = 0; i < 4096; i++)
//            forward.table16[i] = (ushort)(0xFFFF - forward.table16[i]);
//        forward.segments[0].Type = 0;

//        using var reverse = forward.Reverse();
//        if (reverse is null)
//            return false;

//        using var result = reverse.Join(null, reverse, 256);

//        if (result is null)
//            return false;

//        return result.IsLinear;
//    }

//    public static bool CheckJointCurvesSShaped()
//    {
//        using var forward = ToneCurve.BuildParametric(null, 108, 3.2);
//        if (forward is null) return false;

//        using var reverse = forward.Reverse();
//        if (reverse is null) return false;

//        using var result = forward.Join(null, forward, 4096);
//        if (result is null) return false;

//        return result.IsLinear;
//    }

//    public static bool CheckJointFloatCurvesSrgb()
//    {
//        using var forward = BuildSrgbGamma();
//        if (forward is null)
//            return false;

//        using var reverse = forward.Reverse();
//        if (reverse is null)
//            return false;

//        using var result = CombineGammaFloat(forward, reverse);
//        if (result is null)
//            return false;

//        return result.IsLinear;
//    }

//    public static bool CheckParametricToneCurves()
//    {
//        var p = new double[10];

//        // 1) X = Y ^ Gamma

//        p[0] = 2.2;

//        if (!CheckSingleParametric("Gamma", Gamma, 1, p))
//            return false;

//        // 2) CIE 122-1966
//        // Y = (aX + b)^Gamma  | X >= -b/a
//        // Y = 0               | else

//        p[0] = 2.2;
//        p[1] = 1.5;
//        p[2] = -0.5;

//        if (!CheckSingleParametric("CIE122-1966", CIE122, 2, p))
//            return false;

//        // 3) IEC 61966-3
//        // Y = (aX + b)^Gamma | X <= -b/a
//        // Y = c              | else

//        p[0] = 2.2;
//        p[1] = 1.5;
//        p[2] = -0.5;
//        p[3] = 0.3;

//        if (!CheckSingleParametric("IEC 61966-3", IEC61966_3, 3, p))
//            return false;

//        // 4) IEC 61966-2.1 (sRGB)
//        // Y = (aX + b)^Gamma | X >= d
//        // Y = cX             | X < d

//        p[0] = 2.4;
//        p[1] = 1.0 / 1.055;
//        p[2] = 0.055 / 1.055;
//        p[3] = 1.0 / 12.92;
//        p[4] = 0.04045;

//        if (!CheckSingleParametric("IEC 61966-2.1", IEC61966_21, 4, p))
//            return false;

//        // 5) Y = (aX + b)^Gamma + e | X >= d
//        // Y = cX + f             | else

//        p[0] = 2.2;
//        p[1] = 0.7;
//        p[2] = 0.2;
//        p[3] = 0.3;
//        p[4] = 0.1;
//        p[5] = 0.5;
//        p[6] = 0.2;

//        if (!CheckSingleParametric("param_5", Param5, 5, p))
//            return false;

//        // 6) Y = (aX + b) ^ Gamma + c

//        p[0] = 2.2;
//        p[1] = 0.7;
//        p[2] = 0.2;
//        p[3] = 0.3;

//        if (!CheckSingleParametric("param_6", Param6, 6, p))
//            return false;

//        // 7) Y = a * log (b * X^Gamma + c) + d

//        p[0] = 2.2;
//        p[1] = 0.9;
//        p[2] = 0.9;
//        p[3] = 0.02;
//        p[4] = 0.1;

//        if (!CheckSingleParametric("param_7", Param7, 7, p))
//            return false;

//        // 8) Y = a * b ^ (c*X+d) + e

//        p[0] = 0.9;
//        p[1] = 0.9;
//        p[2] = 1.02;
//        p[3] = 0.1;
//        p[4] = 0.2;

//        if (!CheckSingleParametric("param_8", Param8, 8, p))
//            return false;

//        // 108: S-Shaped: (1 - (1-x)^1/g)^1/g

//        p[0] = 1.9;
//        if (!CheckSingleParametric("sigmoidal", Sigmoidal, 108, p))
//            return false;

//        // All OK

//        return true;
//    }

//    public static bool CheckReverseDegenerated()
//    {
//        var tab = new ushort[]
//        {
//            0,
//            0,
//            0,
//            0,
//            0,
//            0x5555,
//            0x6666,
//            0x7777,
//            0x8888,
//            0x9999,
//            0xFFFF,
//            0xFFFF,
//            0xFFFF,
//            0xFFFF,
//            0xFFFF,
//            0xFFFF
//        };

//        using var p = ToneCurve.BuildTabulated(null, tab);
//        if (p is null)
//            return false;

//        using var g = p.Reverse();
//        if (g is null)
//            return false;

//        // Now let's check some points
//        if (!CheckFToneCurvePoint(g, 0x5555, 0x5555)) return false;
//        if (!CheckFToneCurvePoint(g, 0x7777, 0x7777)) return false;

//        // First point for zero
//        if (!CheckFToneCurvePoint(g, 0x0000, 0x4444)) return false;

//        // Last point
//        if (!CheckFToneCurvePoint(g, 0xFFFF, 0xFFFF)) return false;

//        return true;
//    }

//    #endregion Public Methods

//    #region Private Methods

//    private static ToneCurve? BuildSrgbGamma() =>
//        ToneCurve.BuildParametric(null, 4, new double[5]
//        {
//            2.4,
//            1 / 1.055,
//            0.055 / 1.055,
//            1 / 12.92,
//            0.04045
//        });

//    private static bool CheckFToneCurvePoint(ToneCurve c, ushort point, int value) =>
//            Math.Abs(value - c.Eval(point)) < 2;

//    private static bool CheckGamma(double g)
//    {
//        using var curve = ToneCurve.BuildGamma(null, g);
//        if (curve is null) return false;

//        MaxErr = 0;
//        for (var i = 0; i < 0xFFFF; i++)
//        {
//            var @in = i / 65535f;
//            var @out = curve.Eval(@in);
//            var val = Math.Pow(@in, g);

//            var err = Math.Abs(val - @out);
//            if (err > MaxErr) MaxErr = err;
//        }

//        if (MaxErr > 0) Console.Write($"|Err|<{MaxErr * 65535.0:F6} ");

//        if (!CheckGammaEstimation(curve, g)) return false;

//        curve.Dispose();
//        return true;
//    }

//    private static bool CheckGammaFloatTable(double g)
//    {
//        var values = new float[1025];

//        for (var i = 0; i <= 1024; i++)
//        {
//            var @in = i / 1024f;
//            values[i] = MathF.Pow(@in, (float)g);
//        }

//        using var curve = ToneCurve.BuildTabulated(null, values);
//        if (curve is null) return false;

//        MaxErr = 0;
//        for (var i = 0; i < 0xFFFF; i++)
//        {
//            var @in = i / 65535f;
//            var @out = curve.Eval(@in);
//            var val = Math.Pow(@in, g);

//            var err = Math.Abs(val - @out);
//            if (err > MaxErr) MaxErr = err;
//        }

//        if (MaxErr > 0) Console.Write($"|Err|<{MaxErr * 65535.0:F6} ");

//        if (!CheckGammaEstimation(curve, g)) return false;

//        curve.Dispose();
//        return true;
//    }

//    private static bool CheckGammaWordTable(double g)
//    {
//        var values = new ushort[1025];

//        for (var i = 0; i <= 1024; i++)
//        {
//            var @in = i / 1024f;
//            values[i] = (ushort)Math.Floor((Math.Pow(@in, g) * 65535.0) + 0.5);
//        }

//        using var curve = ToneCurve.BuildTabulated(null, values);
//        if (curve is null) return false;

//        MaxErr = 0.0;
//        for (var i = 0; i <= 0xFFFF; i++)
//        {
//            var @in = i / 65535f;
//            var @out = curve.Eval(@in);
//            var val = Math.Pow(@in, g);

//            var err = Math.Abs(val - @out);
//            if (err > MaxErr) MaxErr = err;
//        }

//        if (MaxErr > 0) Console.Write($"|Err|<{MaxErr * 65535.0:F6} ");

//        if (!CheckGammaEstimation(curve, g)) return false;

//        curve.Dispose();
//        return true;
//    }

//    private static bool CheckSingleParametric(string name, Func<float, double[], float> fn, int type, double[] p)
//    {
//        using var tc = ToneCurve.BuildParametric(null, type, p);
//        using var tc1 = ToneCurve.BuildParametric(null, -type, p);

//        bool End(bool result)
//        {
//            tc?.Dispose();
//            tc1?.Dispose();

//            return result;
//        }

//        if (tc is null || tc1 is null) return End(false);

//        for (var i = 0; i <= 1000; i++)
//        {
//            var x = i / 1000f;

//            var yFn = fn(x, p);
//            var yParam = tc.Eval(x);
//            var xParam = tc1.Eval(yParam);

//            var yParam2 = fn(xParam, p);

//            if (!IsGoodVal(name, yFn, yParam, FixedPrecision15_16))
//                return End(false);

//            if (!IsGoodVal($"Inverse {name}", yFn, yParam2, FixedPrecision15_16))
//                return End(false);
//        }

//        return End(true);
//    }

//    private static float CIE122(float x, double[] p)
//    {
//        if (x >= -p[2] / p[1])
//        {
//            var e = (p[1] * x) + p[2];

//            if (e > 0)
//                return (float)Math.Pow(e, p[0]);
//            else
//                return 0f;
//        }
//        else
//        {
//            return 0f;
//        }
//    }

//    private static ToneCurve? CombineGamma16(ToneCurve g1, ToneCurve g2)
//    {
//        var tab = new ushort[256];

//        for (var i = 0; i < 256; i++)
//        {
//            var val = QuantizeValue(i, 256);

//            tab[i] = g2.Eval(g1.Eval(val));
//        }

//        return ToneCurve.BuildTabulated(null, tab);
//    }

//    private static ToneCurve? CombineGammaFloat(ToneCurve g1, ToneCurve g2)
//    {
//        var tab = new ushort[256];

//        for (var i = 0; i < 256; i++)
//        {
//            var f = i / 255f;
//            f = g2.Eval(g1.Eval(f));

//            tab[i] = (ushort)Math.Floor((f * 65535.0) + 0.5);
//        }

//        return ToneCurve.BuildTabulated(null, tab);
//    }

//    private static float Gamma(float x, double[] p) =>
//            (float)Math.Pow(x, p[0]);

//    private static float IEC61966_21(float x, double[] p)
//    {
//        if (x >= p[4])
//        {
//            var e = (p[1] * x) + p[2];

//            if (e > 0)
//                return (float)Math.Pow(e, p[0]);
//            else
//                return 0f;
//        }
//        else
//        {
//            return (float)(x * p[3]);
//        }
//    }

//    private static float IEC61966_3(float x, double[] p)
//    {
//        if (x >= -p[2] / p[1])
//        {
//            var e = (p[1] * x) + p[2];

//            if (e > 0)
//                return (float)(Math.Pow(e, p[0]) + p[3]);
//            else
//                return 0f;
//        }
//        else
//        {
//            return (float)p[3];
//        }
//    }

//    private static float Param5(float x, double[] p)
//    {
//        // Y = (aX + b)^Gamma + e | X >= d
//        // Y = cX + f             | else

//        if (x >= p[4])
//        {
//            var e = (p[1] * x) + p[2];

//            if (e > 0)
//                return (float)(Math.Pow(e, p[0]) + p[5]);
//            else
//                return 0f;
//        }
//        else
//        {
//            return (float)((x * p[3]) + p[6]);
//        }
//    }

//    private static float Param6(float x, double[] p)
//    {
//        var e = (p[1] * x) + p[2];

//        if (e > 0)
//            return (float)(Math.Pow(e, p[0]) + p[3]);
//        else
//            return 0f;
//    }

//    private static float Param7(float x, double[] p) =>
//        (float)((p[1] * Math.Log10((p[2] * Math.Pow(x, p[0])) + p[3])) + p[4]);

//    private static float Param8(float x, double[] p) =>
//        (float)((p[0] * Math.Pow(p[1], (p[2] * x) + p[3])) + p[4]);

//    private static float Sigmoidal(float x, double[] p) =>
//        (float)Math.Pow(1.0 - Math.Pow(1.0 - x, 1 / p[0]), 1 / p[0]);

//    #endregion Private Methods
//}
