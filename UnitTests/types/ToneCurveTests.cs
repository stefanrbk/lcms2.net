//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright ©️ 1998-2024 Marti Maria Saguer
//              2022-2024 Stefan Kewatt
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

//[TestFixture(TestOf = typeof(ToneCurve))]
//public static class ToneCurveTests
//{
//    #region Private Methods

//    private static void CheckGammaEstimation(ToneCurve c, double g)
//    {
//        var est = c.EstimateGamma(1E-3);
//        Assert.That(est, Is.EqualTo(g).Within(1E-3));
//    }

//    #endregion Private Methods

//    #region Classes

//    public class JointCurvesTests : TestBase
//    {
//        #region Fields

//        ToneCurve? Forward;
//        ToneCurve? Result;
//        ToneCurve? Reverse;

//        #endregion Fields

//        #region Public Methods

//        [Test, Category(FixedTest)]
//        public void CheckJoint16CurvesSrgb()
//        {
//            Forward = BuildSrgbGamma();
//            Assert.That(Forward, Is.Not.Null);

//            Reverse = Forward.Reverse();
//            Assert.That(Reverse, Is.Not.Null);

//            Result = CombineGamma16(Forward, Reverse);
//            Assert.That(Result, Is.Not.Null);

//            Assert.That(Result.IsLinear);
//        }

//        [Test, Category(FixedTest)]
//        public void CheckJointCurves()
//        {
//            Forward = ToneCurve.BuildGamma(null, 3.0);
//            Reverse = ToneCurve.BuildGamma(null, 3.0);
//            Assert.Multiple(() =>
//            {
//                Assert.That(Forward, Is.Not.Null);
//                Assert.That(Reverse, Is.Not.Null);
//            });

//            Result = Forward!.Join(null, Reverse!, 256);
//            Assert.That(Result, Is.Not.Null);

//            Assert.That(Result.IsLinear, "Joining same curve twice does not result in a linear ramp");
//        }

//        [Test, Category(FixedTest)]
//        public void CheckJointCurvesDescending()
//        {
//            Forward = ToneCurve.BuildGamma(null, 2.2);
//            Assert.That(Forward, Is.Not.Null);

//            // Fake the curve to be table-based

//            for (var i = 0; i < 4096; i++)
//                Forward!.table16[i] = (ushort)(0xFFFF - Forward.table16[i]);
//            Forward.segments[0].Type = 0;

//            Reverse = Forward.Reverse();
//            Assert.That(Reverse, Is.Not.Null);

//            Result = Reverse.Join(null, Reverse, 256);
//            Assert.That(Result, Is.Not.Null);

//            Assert.That(Result.IsLinear);
//        }

//        [Test, Category(FixedTest)]
//        public void CheckJointCurvesSShaped()
//        {
//            Forward = ToneCurve.BuildParametric(null, 108, 3.2);
//            Assert.That(Forward, Is.Not.Null);

//            Reverse = Forward.Reverse();
//            Assert.That(Reverse, Is.Not.Null);

//            Result = Forward.Join(null, Forward, 4096);
//            Assert.That(Result, Is.Not.Null);

//            Assert.That(Result.IsLinear);
//        }

//        [Test, Category(FixedTest)]
//        public void CheckJointFloatCurvesSrgb()
//        {
//            Forward = BuildSrgbGamma();
//            Assert.That(Forward, Is.Not.Null);

//            Reverse = Forward.Reverse();
//            Assert.That(Reverse, Is.Not.Null);

//            Result = CombineGammaFloat(Forward, Reverse);
//            Assert.That(Result, Is.Not.Null);

//            Assert.That(Result.IsLinear);
//        }

//        [Test, Category(FixedTest)]
//        public void CheckReverseDegenerated()
//        {
//            var tab = new ushort[]
//            {
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
//            };

//            Forward = ToneCurve.BuildTabulated(null, tab);
//            Assert.That(Forward, Is.Not.Null);

//            Reverse = Forward.Reverse();
//            Assert.That(Reverse, Is.Not.Null);

//            Assert.Multiple(() =>
//            {
//                // Now let's check some points
//                CheckFToneCurvePoint(Reverse, 0x5555, 0x5555);
//                CheckFToneCurvePoint(Reverse, 0x7777, 0x7777);

//                // First point for zero
//                CheckFToneCurvePoint(Reverse, 0x0000, 0x4444);

//                // Last point
//                CheckFToneCurvePoint(Reverse, 0xFFFF, 0xFFFF);
//            });
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            Forward?.Dispose();
//            Reverse?.Dispose();
//            Result?.Dispose();
//        }

//        #endregion Public Methods

//        #region Private Methods

//        private static ToneCurve? BuildSrgbGamma() =>
//            ToneCurve.BuildParametric(null, 4, new double[5]
//            {
//                2.4,
//                1 / 1.055,
//                0.055 / 1.055,
//                1 / 12.92,
//                0.04045
//            });

//        private static void CheckFToneCurvePoint(ToneCurve c, ushort point, int value) =>
//                Assert.That(Math.Abs(value - c.Eval(point)), Is.LessThan(2));

//        private static ToneCurve? CombineGamma16(ToneCurve g1, ToneCurve g2)
//        {
//            var tab = new ushort[256];

//            for (var i = 0; i < 256; i++)
//            {
//                var val = Helpers.QuantizeValue(i, 256);

//                tab[i] = g2.Eval(g1.Eval(val));
//            }

//            return ToneCurve.BuildTabulated(null, tab);
//        }

//        private static ToneCurve? CombineGammaFloat(ToneCurve g1, ToneCurve g2)
//        {
//            var tab = new ushort[256];

//            for (var i = 0; i < 256; i++)
//            {
//                var f = i / 255f;
//                f = g2.Eval(g1.Eval(f));

//                tab[i] = (ushort)Math.Floor((f * 65535.0) + 0.5);
//            }

//            return ToneCurve.BuildTabulated(null, tab);
//        }

//        #endregion Private Methods
//    }

//    public class LinearCurveTests : TestBase
//    {
//        #region Fields

//        ToneCurve LinGamma;

//        #endregion Fields

//        #region Public Methods

//        [Test, Category(RandomTest)]
//        public void CheckGammaCreation16(
//            [Random(0, 0xFFFF, 20)]
//                int i)
//        {
//            var @in = (ushort)i;
//            var @out = LinGamma.Eval(@in);
//            Assert.That(@out, Is.EqualTo(@in));
//        }

//        [Test, Category(RandomTest)]
//        public void CheckGammaCreationFloat(
//            [Random(0, 0xFFFF, 20)]
//                int i)
//        {
//            var @in = i / 65535f;
//            var @out = LinGamma.Eval(@in);
//            Assert.That(@out, Is.EqualTo(@in).Within(1 / 65535f));
//        }

//        [Test, Category(FixedTest)]
//        public void CheckGammaEstimation10() =>
//            CheckGammaEstimation(LinGamma, 1.0);

//        [SetUp]
//        public override void SetUp()
//        {
//            base.SetUp();

//            var linGamma = ToneCurve.BuildGamma(null, 1.0);
//            Assert.That(linGamma, Is.Not.Null);
//            LinGamma = linGamma;
//        }

//        [TearDown]
//        public void TearDown() =>
//            LinGamma.Dispose();

//        #endregion Public Methods
//    }

//    public class ParametricCurveTests : TestBase
//    {
//        #region Public Methods

//        [Test, Category(FixedTest)]
//        public void CheckParametricToneCurves(
//        [Range(1, 8)]
//            [Values(108)]
//                int type)
//        {
//            switch (type)
//            {
//                // 1) X = Y ^ Gamma
//                case 1:

//                    CheckSingleParametric("Gamma", Gamma, 1, 2.2);
//                    break;

//                // 2) CIE 122-1966
//                // Y = (aX + b)^Gamma  | X >= -b/a
//                // Y = 0               | else
//                case 2:

//                    CheckSingleParametric("CIE122-1966", CIE122, 2, 2.2, 1.5, -0.5);
//                    break;

//                // 3) IEC 61966-3
//                // Y = (aX + b)^Gamma | X <= -b/a
//                // Y = c              | else
//                case 3:

//                    CheckSingleParametric("IEC 61966-3", IEC61966_3, 3, 2.2, 1.5, -0.5, 0.3);
//                    break;

//                // 4) IEC 61966-2.1 (sRGB)
//                // Y = (aX + b)^Gamma | X >= d
//                // Y = cX             | X < d
//                case 4:

//                    CheckSingleParametric("IEC 61966-2.1", IEC61966_21, 4, 2.4, 1 / 1.055, 0.055 / 1.055, 1 / 12.92, 0.04045);
//                    break;

//                // 5) Y = (aX + b)^Gamma + e | X >= d
//                // Y = cX + f             | else
//                case 5:

//                    CheckSingleParametric("param_5", Param5, 5, 2.2, 0.7, 0.2, 0.3, 0.1, 0.5, 0.2);
//                    break;

//                // 6) Y = (aX + b) ^ Gamma + c
//                case 6:

//                    CheckSingleParametric("param_6", Param6, 6, 2.2, 0.7, 0.2, 0.3);
//                    break;

//                // 7) Y = a * log (b * X^Gamma + c) + d
//                case 7:

//                    CheckSingleParametric("param_7", Param7, 7, 2.2, 0.9, 0.9, 0.02, 0.1);
//                    break;

//                // 8) Y = a * b ^ (c*X+d) + e
//                case 8:

//                    CheckSingleParametric("param_8", Param8, 8, 0.9, 0.9, 1.02, 0.1, 0.2);
//                    break;

//                // 108: S-Shaped: (1 - (1-x)^1/g)^1/g
//                case 108:

//                    CheckSingleParametric("sigmoidal", Sigmoidal, 108, 1.9);
//                    break;
//            }

//            // All OK

//            Assert.Pass();
//        }

//        #endregion Public Methods

//        #region Private Methods

//        private static void CheckSingleParametric(string name, Func<float, double[], float> fn, int type, params double[] p)
//        {
//            var tc = ToneCurve.BuildParametric(null, type, p);
//            var tc1 = ToneCurve.BuildParametric(null, -type, p);
//            Assert.Multiple(() =>
//            {
//                Assert.That(tc, Is.Not.Null);
//                Assert.That(tc1, Is.Not.Null);
//            });

//            Assert.Multiple(() =>
//            {
//                for (var i = 0; i <= 1000; i++)
//                {
//                    var x = i / 1000f;

//                    var yFn = fn(x, p);
//                    var yParam = tc!.Eval(x);
//                    var xParam = tc1!.Eval(yParam);

//                    var yParam2 = fn(xParam, p);

//                    Assert.Multiple(() =>
//                    {
//                        IsGoodVal(name, yFn, yParam, S15Fixed16Precision);
//                        IsGoodVal($"Inverse {name}", yFn, yParam2, S15Fixed16Precision);
//                    });
//                }
//                tc!.Dispose();
//                tc1!.Dispose();
//            });
//        }

//        private static float CIE122(float x, double[] p)
//        {
//            if (x >= -p[2] / p[1])
//            {
//                var e = (p[1] * x) + p[2];

//                if (e > 0)
//                    return (float)Math.Pow(e, p[0]);
//                else
//                    return 0f;
//            }
//            else
//            {
//                return 0f;
//            }
//        }

//        private static float Gamma(float x, double[] p) =>
//            (float)Math.Pow(x, p[0]);

//        private static float IEC61966_21(float x, double[] p)
//        {
//            if (x >= p[4])
//            {
//                var e = (p[1] * x) + p[2];

//                if (e > 0)
//                    return (float)Math.Pow(e, p[0]);
//                else
//                    return 0f;
//            }
//            else
//            {
//                return (float)(x * p[3]);
//            }
//        }

//        private static float IEC61966_3(float x, double[] p)
//        {
//            if (x >= -p[2] / p[1])
//            {
//                var e = (p[1] * x) + p[2];

//                if (e > 0)
//                    return (float)(Math.Pow(e, p[0]) + p[3]);
//                else
//                    return 0f;
//            }
//            else
//            {
//                return (float)p[3];
//            }
//        }

//        private static float Param5(float x, double[] p)
//        {
//            // Y = (aX + b)^Gamma + e | X >= d
//            // Y = cX + f             | else

//            if (x >= p[4])
//            {
//                var e = (p[1] * x) + p[2];

//                if (e > 0)
//                    return (float)(Math.Pow(e, p[0]) + p[5]);
//                else
//                    return 0f;
//            }
//            else
//            {
//                return (float)((x * p[3]) + p[6]);
//            }
//        }

//        private static float Param6(float x, double[] p)
//        {
//            var e = (p[1] * x) + p[2];

//            if (e > 0)
//                return (float)(Math.Pow(e, p[0]) + p[3]);
//            else
//                return 0f;
//        }

//        private static float Param7(float x, double[] p) =>
//            (float)((p[1] * Math.Log10((p[2] * Math.Pow(x, p[0])) + p[3])) + p[4]);

//        private static float Param8(float x, double[] p) =>
//            (float)((p[0] * Math.Pow(p[1], (p[2] * x) + p[3])) + p[4]);

//        private static float Sigmoidal(float x, double[] p) =>
//            (float)Math.Pow(1.0 - Math.Pow(1.0 - x, 1 / p[0]), 1 / p[0]);

//        #endregion Private Methods
//    }

//    public class SingleCurveTests : TestBase
//    {
//        #region Fields

//        ToneCurve? Curve;

//        #endregion Fields

//        #region Public Methods

//        [Test, Category(FixedTest)]
//        public void CheckGamma(
//            [Values(1.8, 2.2, 3.0)]
//                double g)
//        {
//            Curve = ToneCurve.BuildGamma(null, g);
//            Assert.That(Curve, Is.Not.Null);

//            var MaxErr = 0.0;
//            for (var i = 0; i < 0xFFFF; i++)
//            {
//                var @in = i / 65535f;
//                var @out = Curve.Eval(@in);
//                var val = Math.Pow(@in, g);

//                var err = Math.Abs(val - @out);
//                if (err > MaxErr) MaxErr = err;
//            }

//            if (MaxErr > 0) Console.Write($"|Err|<{MaxErr * 65535.0:F6}");

//            CheckGammaEstimation(Curve, g);
//        }

//        [Test, Category(FixedTest)]
//        public void CheckGammaFloatTable(
//            [Values(1.8, 2.2, 3.0)]
//                double g)
//        {
//            var values = new float[1025];

//            for (var i = 0; i <= 1024; i++)
//            {
//                var @in = i / 1024f;
//                values[i] = MathF.Pow(@in, (float)g);
//            }

//            var curve = ToneCurve.BuildTabulated(null, values);
//            Assert.That(curve, Is.Not.Null);

//            var MaxErr = 0.0;
//            for (var i = 0; i < 0xFFFF; i++)
//            {
//                var @in = i / 65535f;
//                var @out = curve.Eval(@in);
//                var val = Math.Pow(@in, g);

//                var err = Math.Abs(val - @out);
//                if (err > MaxErr) MaxErr = err;
//            }

//            if (MaxErr > 0) Console.Write($"|Err|<{MaxErr * 65535.0:F6}");

//            CheckGammaEstimation(curve, g);

//            curve.Dispose();
//        }

//        [Test, Category(FixedTest)]
//        public void CheckGammaWordTable(
//            [Values(1.8, 2.2, 3.0)]
//                double g)
//        {
//            var values = new ushort[1025];

//            for (var i = 0; i <= 1024; i++)
//            {
//                var @in = i / 1024f;
//                values[i] = (ushort)Math.Floor((Math.Pow(@in, g) * 65535.0) + 0.5);
//            }

//            var curve = ToneCurve.BuildTabulated(null, values);
//            Assert.That(curve, Is.Not.Null);

//            var MaxErr = 0.0;
//            for (var i = 0; i < 0xFFFF; i++)
//            {
//                var @in = i / 65535f;
//                var @out = curve.Eval(@in);
//                var val = Math.Pow(@in, g);

//                var err = Math.Abs(val - @out);
//                if (err > MaxErr) MaxErr = err;
//            }

//            if (MaxErr > 0) Console.Write($"|Err|<{MaxErr * 65535.0:F6}");

//            CheckGammaEstimation(curve, g);

//            curve.Dispose();
//        }

//        [TearDown]
//        public void TearDown() =>
//            Curve?.Dispose();

//        #endregion Public Methods
//    }

//    #endregion Classes
//}
