﻿using lcms2.state;
using lcms2.types;

namespace lcms2.testbed;

public class InterpolationTests : ITest
{
    #region Fields

    private object _state;

    #endregion Fields

    #region Public Methods
    public static bool Check1DLerp2() =>
        Check1D(2, false, 0);
    public static bool Check1DLerp3() =>
        Check1D(3, false, 1);
    public static bool Check1DLerp4() =>
        Check1D(4, false, 0);
    public static bool Check1DLerp6() =>
        Check1D(6, false, 0);
    public static bool Check1DLerp18() =>
        Check1D(18, false, 0);
    public static bool Check1DLerp2Down() =>
        Check1D(2, true, 0);
    public static bool Check1DLerp3Down() =>
        Check1D(3, true, 1);
    public static bool Check1DLerp4Down() =>
        Check1D(4, true, 0);
    public static bool Check1DLerp6Down() =>
        Check1D(6, true, 0);
    public static bool Check1DLerp18Down() =>
        Check1D(18, true, 0);

    // A single function that does check 1D interpolation
    // nNodesToCheck = number on nodes to check
    // Down = Create decreasing tables
    // Reverse = Check reverse interpolation
    // max_err = max allowed error
    public static bool Check1D(uint numNodesToCheck, bool down, int maxErr)
    {
        var tab = new ushort[numNodesToCheck];

        var p = InterpParams.Compute(null , numNodesToCheck, 1, 1, tab, LerpFlag.Ushort);
        if (p is null || p.Interpolation is null) return false;

        BuildTable(numNodesToCheck, ref tab, down);

        for (var i = 0; i <= 0xFFFF; i++)
        {
            var @in = new ushort[] { (ushort)i };
            var @out = new ushort[1];

            p.Interpolation.Lerp(@in, @out, p);

            if (down) @out[0] = (ushort)(0xFFFF - @out[0]);

            if (Math.Abs(@out[0] - @in[0]) > maxErr)
            {
                Fail($"({numNodesToCheck}): Must be {@in}, but was {@out} : ");
                return false;
            }
        }

        return true;
    }

    public static bool ExhaustiveCheck1DLerp()
    {
        if (HasConsole)
            Console.Write("10 - 4096");
        for (uint i = 0, j = 1; i < 16; i++, j++)
        {
            if (!ExhaustiveCheck1D(i, j)) return false;
        }

        if (HasConsole)
            Console.Write("\nThe result is ");

        return true;
    }

    public static bool ExhaustiveCheck1DLerpDown()
    {
        if (HasConsole)
            Console.Write("10 - 4096");
        for (uint i = 0, j = 1; i < 16; i++, j++)
        {
            if (!ExhaustiveCheck1DDown(i, j)) return false;
        }

        if (HasConsole)
            Console.Write("\nThe result is ");

        return true;
    }

    public static bool Check3DInterpolationTetrahedral16()
    {
        var table = new ushort[]
        {
            0, 0, 0,                // B=0, G=0, R=0
            0, 0, 0xFFFF,           // B=1, G=0, R=0
            
            0, 0xFFFF, 0,           // B=0, G=1, R=0
            0, 0xFFFF, 0xFFFF,      // B=1, G=1, R=0
            
            0xFFFF, 0, 0,           // B=0, G=0, R=1
            0xFFFF, 0, 0xFFFF,      // B=1, G=0, R=1
            
            0xFFFF, 0xFFFF, 0,      // B=0, G=1, R=1
            0xFFFF, 0xFFFF, 0xFFFF, // B=1, G=1, R=1
        };

        var p = InterpParams.Compute(null, 2, 3, 3, table, LerpFlag.Ushort);
        if (p is null || p.Interpolation is null) return false;

        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new ushort[] { (ushort)i, (ushort)i, (ushort)i };
            var @out = new ushort[3];

            p.Interpolation.Lerp(@in, @out, p);

            if (!IsGoodFixed15_16("Channel 1", @out[0], @in[0]) ||
                !IsGoodFixed15_16("Channel 2", @out[1], @in[1] / 2f) ||
                !IsGoodFixed15_16("Channel 2", @out[2], @in[2] / 4f))
            {
                return false;
            }
        }

        if (MaxErr > 0)
            WriteLineRed($"|Err|<{MaxErr}");

        return true;
    }

    public static bool Check3DInterpolationTrilinear16()
    {
        var table = new ushort[]
        {
            0, 0, 0,                // B=0, G=0, R=0
            0, 0, 0xFFFF,           // B=1, G=0, R=0
            
            0, 0xFFFF, 0,           // B=0, G=1, R=0
            0, 0xFFFF, 0xFFFF,      // B=1, G=1, R=0
            
            0xFFFF, 0, 0,           // B=0, G=0, R=1
            0xFFFF, 0, 0xFFFF,      // B=1, G=0, R=1
            
            0xFFFF, 0xFFFF, 0,      // B=0, G=1, R=1
            0xFFFF, 0xFFFF, 0xFFFF, // B=1, G=1, R=1
        };

        var p = InterpParams.Compute(null, 2, 3, 3, table, LerpFlag.Trilinear);
        if (p is null || p.Interpolation is null) return false;

        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new ushort[] { (ushort)i, (ushort)i, (ushort)i };
            var @out = new ushort[3];

            p.Interpolation.Lerp(@in, @out, p);

            if (!IsGoodFixed15_16("Channel 1", @out[0], @in[0]) ||
                !IsGoodFixed15_16("Channel 2", @out[1], @in[1] / 2f) ||
                !IsGoodFixed15_16("Channel 2", @out[2], @in[2] / 4f))
            {
                return false;
            }
        }

        if (MaxErr > 0)
            WriteLineRed($"|Err|<{MaxErr}");

        return true;
    }

    public static bool Check3DInterpolationFloatTetrahedral()
    {
        var floatTable = new float[]
        {
            0, 0, 0,            // B=0, G=0, R=0
            0, 0, .25f,         // B=1, G=0, R=0
            
            0, .5f, 0,          // B=0, G=1, R=0
            0, .5f, .25f,       // B=1, G=1, R=0
            
            1, 0, 0,            // B=0, G=0, R=1
            1, 0, .25f,         // B=1, G=0, R=1
            
            1, .5f, 0,          // B=0, G=1, R=1
            1, .5f, .25f,       // B=1, G=1, R=1
        };

        var p = InterpParams.Compute(null, 2, 3, 3, floatTable, LerpFlag.Float);
        if (p is null || p.Interpolation is null) return false;

        MaxErr = 0.0;
        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new float[] { i / 65535f, i / 65535f, i / 65535f };
            var @out = new float[3];

            p.Interpolation.Lerp(@in, @out, p);

            if (!IsGoodFixed15_16("Channel 1", @out[0], @in[0]) ||
                !IsGoodFixed15_16("Channel 2", @out[1], @in[1] / 2f) ||
                !IsGoodFixed15_16("Channel 2", @out[2], @in[2] / 4f))
            {
                return false;
            }
        }

        if (MaxErr > 0)
            WriteLineRed($"|Err|<{MaxErr}");

        return true;
    }

    public static bool Check3DInterpolationFloatTrilinear()
    {
        var floatTable = new float[]
        {
            0, 0, 0,            // B=0, G=0, R=0
            0, 0, .25f,         // B=1, G=0, R=0
            
            0, .5f, 0,          // B=0, G=1, R=0
            0, .5f, .25f,       // B=1, G=1, R=0
            
            1, 0, 0,            // B=0, G=0, R=1
            1, 0, .25f,         // B=1, G=0, R=1
            
            1, .5f, 0,          // B=0, G=1, R=1
            1, .5f, .25f,       // B=1, G=1, R=1
        };

        var p = InterpParams.Compute(null, 2, 3, 3, floatTable, LerpFlag.Float | LerpFlag.Trilinear);

        if (p is null || p.Interpolation is null) return false;

        MaxErr = 0.0;
        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new float[] { i / 65535f, i / 65535f, i / 65535f };
            var @out = new float[3];

            p.Interpolation.Lerp(@in, @out, p);

            if (!IsGoodFixed15_16("Channel 1", @out[0], @in[0]) ||
                !IsGoodFixed15_16("Channel 2", @out[1], @in[1] / 2f) ||
                !IsGoodFixed15_16("Channel 2", @out[2], @in[2] / 4f))
            {
                return false;
            }
        }

        if (MaxErr > 0)
            WriteLineRed($"|Err|<{MaxErr}");

        return true;
    }

    [Test]
    public void CheckReverseInterpolation3x3Test()
    {
        var target = new float[4];
        var result = new float[4];
        var hint = new float[4];

        var table = new ushort[]
        {
            0, 0, 0,                // B=0, G=0, R=0
            0, 0, 0xFFFF,           // B=1, G=0, R=0
            
            0, 0xFFFF, 0,           // B=0, G=1, R=0
            0, 0xFFFF, 0xFFFF,      // B=1, G=1, R=0
            
            0xFFFF, 0, 0,           // B=0, G=0, R=1
            0xFFFF, 0, 0xFFFF,      // B=1, G=0, R=1
            
            0xFFFF, 0xFFFF, 0,      // B=0, G=1, R=1
            0xFFFF, 0xFFFF, 0xFFFF, // B=1, G=1, R=1
        };

        var lut = Pipeline.Alloc(_state, 3, 3);
        Assert.That(lut, Is.Not.Null);

        var clut = Stage.AllocCLut16bit(_state, 2, 3, 3, table);
        lut.InsertStage(StageLoc.AtBegin, clut);

        lut.EvalReverse(target, result, null);
        Assert.That(result.Take(3).ToArray(), Contains.Item(0), "Reverse interpolation didn't find zero");

        // Transverse identity
        var max = 0f;
        for (var i = 0; i <= 100; i++)
        {
            var @in = i / 100f;

            target[0] = @in; target[1] = 0; target[2] = 0;
            lut.EvalReverse(target, result, hint);

            var err = MathF.Abs(@in - result[0]);
            if (err > max) max = err;

            result.CopyTo(hint, 0);
        }

        lut.Dispose();
        Assert.That(max, Is.LessThanOrEqualTo(FloatPrecision));
    }

    [Test]
    public void CheckReverseInterpolation4x3Test()
    {
        var target = new float[4];
        var result = new float[4];
        var hint = new float[4];

        var table = new ushort[]
        {
            0, 0, 0,                // 0 0 0 0 = ( 0, 0, 0)
            0, 0, 0,                // 0 0 0 1 = ( 0, 0, 0)

            0, 0, 0xFFFF,           // 0 0 1 0 = ( 0, 0, 1)
            0, 0, 0xFFFF,           // 0 0 1 1 = ( 0, 0, 1)
            
            0, 0xFFFF, 0,           // 0 1 0 0 = ( 0, 1, 0)
            0, 0xFFFF, 0,           // 0 1 0 1 = ( 0, 1, 0)

            0, 0xFFFF, 0xFFFF,      // 0 1 1 0 = ( 0, 1, 1)
            0, 0xFFFF, 0xFFFF,      // 0 1 1 1 = ( 0, 1, 1)
            
            0xFFFF, 0, 0,           // 1 0 0 0 = ( 1, 0, 0)
            0xFFFF, 0, 0,           // 1 0 0 1 = ( 1, 0, 0)

            0xFFFF, 0, 0xFFFF,      // 1 0 1 0 = ( 1, 0, 1)
            0xFFFF, 0, 0xFFFF,      // 1 0 1 1 = ( 1, 0, 1)
            
            0xFFFF, 0xFFFF, 0,      // 1 1 0 0 = ( 1, 1, 0)
            0xFFFF, 0xFFFF, 0,      // 1 1 0 1 = ( 1, 1, 0)

            0xFFFF, 0xFFFF, 0xFFFF, // 1 1 1 0 = ( 1, 1, 1)
            0xFFFF, 0xFFFF, 0xFFFF, // 1 1 1 1 = ( 1, 1, 1)
        };

        var lut = Pipeline.Alloc(_state, 4, 3);
        Assert.That(lut, Is.Not.Null);

        var clut = Stage.AllocCLut16bit(_state, 2, 4, 3, table);
        lut.InsertStage(StageLoc.AtBegin, clut);

        // Check if the LUT is behaving as expected
        SubTest("4->3 feasibility");
        for (var i = 0; i <= 100; i++)
        {
            target[0] = i / 100f;
            target[1] = target[0];
            target[2] = 0;
            target[3] = 12;

            lut.Eval(target, result);

            Assert.Multiple(() =>
            {
                ClearAssert();

                IsGoodFixed15_16("0", target[0], result[0]);
                IsGoodFixed15_16("1", target[1], result[1]);
                IsGoodFixed15_16("2", target[2], result[2]);
            });
        }

        SubTest("4->3 zero");
        target[0] = target[1] = target[2] = 0;

        // This one holds the fixed k
        target[3] = 0;

        // This is our hint (which is a big lie in this case)
        Enumerable.Repeat(0.1f, 3).ToArray().CopyTo(hint, 0);

        lut.EvalReverse(target, result, hint);

        Assert.That(result, Contains.Item(0), "Reverse interpolation didn't find zero");

        SubTest("4->3 find CMY");

        var max = 0f;
        for (var i = 0; i <= 100; i++)
        {
            var @in = i / 100f;

            target[0] = @in; target[1] = 0; target[2] = 0;
            lut.EvalReverse(target, result, hint);

            var err = MathF.Abs(@in - result[0]);
            if (err > max) max = err;

            result.CopyTo(hint, 0);
        }

        lut.Dispose();
        Assert.That(max, Is.LessThanOrEqualTo(FloatPrecision));
    }

    [TestCaseSource(typeof(TestDataGenerator), nameof(TestDataGenerator.CheckXDGranular), new object[] { 3 })]
    [TestCaseSource(typeof(TestDataGenerator), nameof(TestDataGenerator.CheckXDGranular), new object[] { 4 })]
    [TestCaseSource(typeof(TestDataGenerator), nameof(TestDataGenerator.CheckXDGranular), new object[] { 5 })]
    [TestCaseSource(typeof(TestDataGenerator), nameof(TestDataGenerator.CheckXDGranular), new object[] { 6 })]
    [TestCaseSource(typeof(TestDataGenerator), nameof(TestDataGenerator.CheckXDGranular), new object[] { 7 })]
    [TestCaseSource(typeof(TestDataGenerator), nameof(TestDataGenerator.CheckXDGranular), new object[] { 8 })]
    public void CheckXDInterpGranularTest(uint[] dims, uint inputChans, ushort[] a)
    {
        var lut = Pipeline.Alloc(_state, inputChans, 3);
        var mpe = Stage.AllocCLut16bit(_state, dims, inputChans, 3, null);
        Assert.Multiple(() =>
        {
            ClearAssert();

            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(SamplerXD, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        var out1 = new ushort[3];
        var out2 = new ushort[3];

        // This is the interpolated value
        lut.Eval(a, out1);

        // This is the real value
        SamplerXD(a.Concat(Enumerable.Repeat<ushort>(0, 5)).ToArray(), out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            IsGoodWord("Channel 1", out1[0], out2[0], 2);
            IsGoodWord("Channel 2", out1[1], out2[1], 2);
            IsGoodWord("Channel 3", out1[2], out2[2], 2);
        });
    }

    [TestCaseSource(typeof(TestDataGenerator), nameof(TestDataGenerator.CheckXD), new object[] { 3 })]
    [TestCaseSource(typeof(TestDataGenerator), nameof(TestDataGenerator.CheckXD), new object[] { 4 })]
    public void CheckXDInterpTest(uint inputChans, ushort[] a)
    {
        var lut = Pipeline.Alloc(_state, inputChans, 3);
        var mpe = Stage.AllocCLut16bit(_state, 9, inputChans, 3, null);
        Assert.Multiple(() =>
        {
            ClearAssert();

            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(SamplerXD, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        var out1 = new ushort[3];
        var out2 = new ushort[3];

        // This is the interpolated value
        lut.Eval(a, out1);

        // This is the real value
        SamplerXD(a.Concat(Enumerable.Repeat<ushort>(0, 5)).ToArray(), out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            IsGoodWord("Channel 1", out1[0], out2[0], 2);
            IsGoodWord("Channel 2", out1[1], out2[1], 2);
            IsGoodWord("Channel 3", out1[2], out2[2], 2);
        });
    }

    public static bool ExhaustiveCheck3DInterpolationTetrahedral16()
    {
        if (HasConsole)
            Console.Write("0 - 256");
        for (uint i = 0, j = 1; i < 16; i++, j++)
        {
            if (!TestExhaustiveCheck3DInterpolationTetrahedral16(i, j)) return false;
        }

        if (HasConsole)
            Console.Write("\nThe result is ");

        return true;
    }

    public static bool TestExhaustiveCheck3DInterpolationTetrahedral16(uint start, uint stop)
    {
        if (HasConsole)
        {
            Console.WriteLine();
            Console.WriteLine($"\tTesting {start} - {stop}");
        }
        var startStr = start.ToString();
        var stopStr = stop.ToString();
        var widthSubtract = 16 + startStr.Length + stopStr.Length;

        var table = new ushort[]
        {
            0, 0, 0,                // B=0, G=0, R=0
            0, 0, 0xFFFF,           // B=1, G=0, R=0
            
            0, 0xFFFF, 0,           // B=0, G=1, R=0
            0, 0xFFFF, 0xFFFF,      // B=1, G=1, R=0
            
            0xFFFF, 0, 0,           // B=0, G=0, R=1
            0xFFFF, 0, 0xFFFF,      // B=1, G=0, R=1
            
            0xFFFF, 0xFFFF, 0,      // B=0, G=1, R=1
            0xFFFF, 0xFFFF, 0xFFFF, // B=1, G=1, R=1
        };

        var p = InterpParams.Compute(null, 2, 3, 3, table, LerpFlag.Ushort);
        if (p is null || p.Interpolation is null) return false;

        for (var r = start; r < stop; r++)
        {
            for (var g = 0; g < 0xFF; g++)
            {
                for (var b = 0; b < 0xFF; b++)
                {
                    var @in = new ushort[] { (ushort)r, (ushort)g, (ushort)b };
                    var @out = new ushort[3];

                    p.Interpolation.Lerp(@in, @out, p);

                    if (!IsGoodWord("Channel 1", @out[0], @in[0]) ||
                        !IsGoodWord("Channel 2", @out[1], @in[1]) ||
                        !IsGoodWord("Channel 2", @out[2], @in[2]))
                    {
                        return false;
                    }
                }
            }

            ProgressBar(start, stop, widthSubtract, r);
        }

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");

        return true;
    }

    public static bool ExhaustiveCheck3DInterpolationTrilinear16()
    {
        if (HasConsole)
            Console.Write("0 - 256");
        for (uint i = 0, j = 1; i < 16; i++, j++)
        {
            if (!TestExhaustiveCheck3DInterpolationTrilinear16(i, j)) return false;
        }

        if (HasConsole)
            Console.Write("\nThe result is ");

        return true;
    }

    public static bool TestExhaustiveCheck3DInterpolationTrilinear16(uint start, uint stop)
    {
        if (HasConsole)
        {
            Console.WriteLine();
            Console.WriteLine($"\tTesting {start} - {stop}");
        }
        var startStr = start.ToString();
        var stopStr = stop.ToString();
        var widthSubtract = 16 + startStr.Length + stopStr.Length;

        var table = new ushort[]
        {
            0, 0, 0,                // B=0, G=0, R=0
            0, 0, 0xFFFF,           // B=1, G=0, R=0
            
            0, 0xFFFF, 0,           // B=0, G=1, R=0
            0, 0xFFFF, 0xFFFF,      // B=1, G=1, R=0
            
            0xFFFF, 0, 0,           // B=0, G=0, R=1
            0xFFFF, 0, 0xFFFF,      // B=1, G=0, R=1
            
            0xFFFF, 0xFFFF, 0,      // B=0, G=1, R=1
            0xFFFF, 0xFFFF, 0xFFFF, // B=1, G=1, R=1
        };

        var p = InterpParams.Compute(null, 2, 3, 3, table, LerpFlag.Trilinear);
        if (p is null || p.Interpolation is null) return false;

        for (var r = start; r < stop; r++)
        {
            for (var g = 0; g < 0xFF; g++)
            {
                for (var b = 0; b < 0xFF; b++)
                {
                    var @in = new ushort[] { (ushort)r, (ushort)g, (ushort)b };
                    var @out = new ushort[3];

                    p.Interpolation.Lerp(@in, @out, p);

                    if (!IsGoodWord("Channel 1", @out[0], @in[0]) ||
                        !IsGoodWord("Channel 2", @out[1], @in[1]) ||
                        !IsGoodWord("Channel 2", @out[2], @in[2]))
                    {
                        return false;
                    }
                }
            }

            ProgressBar(start, stop, widthSubtract, r);
        }

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");

        return true;
    }

    public static bool ExhaustiveCheck3DInterpolationTetrahedralFloat()
    {
        if (HasConsole)
            Console.Write("0 - 256");
        for (uint i = 0, j = 1; i < 16; i++, j++)
        {
            if (!TestExhaustiveCheck3DInterpolationTetrahedralFloat(i, j)) return false;
        }

        if (HasConsole)
            Console.Write("\nThe result is ");

        return true;
    }

    public static bool TestExhaustiveCheck3DInterpolationTetrahedralFloat(uint start, uint stop)
    {
        if (HasConsole)
        {
            Console.WriteLine();
            Console.WriteLine($"\tTesting {start} - {stop}");
        }
        var startStr = start.ToString();
        var stopStr = stop.ToString();
        var widthSubtract = 16 + startStr.Length + stopStr.Length;

        var floatTable = new float[]
        {
            0, 0, 0,            // B=0, G=0, R=0
            0, 0, .25f,         // B=1, G=0, R=0
            
            0, .5f, 0,          // B=0, G=1, R=0
            0, .5f, .25f,       // B=1, G=1, R=0
            
            1, 0, 0,            // B=0, G=0, R=1
            1, 0, .25f,         // B=1, G=0, R=1
            
            1, .5f, 0,          // B=0, G=1, R=1
            1, .5f, .25f,       // B=1, G=1, R=1
        };

        var p = InterpParams.Compute(null, 2, 3, 3, floatTable, LerpFlag.Float);
        if (p is null || p.Interpolation is null) return false;

        MaxErr = 0.0;
        for (var r = start; r < stop; r++)
        {
            for (var g = 0; g < 0xFF; g++)
            {
                for (var b = 0; b < 0xFF; b++)
                {
                    var @in = new ushort[] { (ushort)r, (ushort)g, (ushort)b };
                    var @out = new ushort[3];

                    p.Interpolation.Lerp(@in, @out, p);

                    if (!IsGoodWord("Channel 1", @out[0], @in[0]) ||
                        !IsGoodWord("Channel 2", @out[1], @in[1]) ||
                        !IsGoodWord("Channel 2", @out[2], @in[2]))
                    {
                        return false;
                    }
                }
            }

            ProgressBar(start, stop, widthSubtract, r);
        }

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");

        return true;
    }

    public static bool ExhaustiveCheck3DInterpolationTrilinearFloat()
    {
        if (HasConsole)
            Console.Write("0 - 256");
        for (uint i = 0, j = 1; i < 16; i++, j++)
        {
            if (!TestExhaustiveCheck3DInterpolationTrilinearFloat(i, j)) return false;
        }

        if (HasConsole)
            Console.Write("\nThe result is ");

        return true;
    }

    public static bool TestExhaustiveCheck3DInterpolationTrilinearFloat(uint start, uint stop)
    {
        if (HasConsole)
        {
            Console.WriteLine();
            Console.WriteLine($"\tTesting {start} - {stop}");
        }
        var startStr = start.ToString();
        var stopStr = stop.ToString();
        var widthSubtract = 16 + startStr.Length + stopStr.Length;

        var floatTable = new float[]
        {
            0, 0, 0,            // B=0, G=0, R=0
            0, 0, .25f,         // B=1, G=0, R=0
            
            0, .5f, 0,          // B=0, G=1, R=0
            0, .5f, .25f,       // B=1, G=1, R=0
            
            1, 0, 0,            // B=0, G=0, R=1
            1, 0, .25f,         // B=1, G=0, R=1
            
            1, .5f, 0,          // B=0, G=1, R=1
            1, .5f, .25f,       // B=1, G=1, R=1
        };

        var p = InterpParams.Compute(null, 2, 3, 3, floatTable, LerpFlag.Float | LerpFlag.Trilinear);
        if (p is null || p.Interpolation is null) return false;

        MaxErr = 0.0;
        for (var r = start; r < stop; r++)
        {
            for (var g = 0; g < 0xFF; g++)
            {
                for (var b = 0; b < 0xFF; b++)
                {
                    var @in = new ushort[] { (ushort)r, (ushort)g, (ushort)b };
                    var @out = new ushort[3];

                    p.Interpolation.Lerp(@in, @out, p);

                    if (!IsGoodWord("Channel 1", @out[0], @in[0]) ||
                        !IsGoodWord("Channel 2", @out[1], @in[1]) ||
                        !IsGoodWord("Channel 2", @out[2], @in[2]))
                    {
                        return false;
                    }
                }
            }

            ProgressBar(start, stop, widthSubtract, r);
        }

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");

        return true;
    }

    [SetUp]
    public void Setup()
    {
        _state = State.CreateStateContainer()!;
        try
        {
            _ = Console.BufferWidth;
        }
        catch
        {
            HasConsole = false;
        }
    }

    [TearDown]
    public void Teardown() =>
        State.DeleteStateContainer(_state);

    #endregion Public Methods

    #region Private Methods

    // Since prime factors of 65535 (FFFF) are,
    //
    //            0xFFFF = 3 * 5 * 17 * 257
    //
    // I test tables of 2, 4, 6, and 18 points, that will be exact.
    private static void BuildTable(uint n, ref ushort[] tab, bool descending)
    {
        for (var i = 0; i < n; i++)
        {
            var v = 65535.0 * i / (n - 1);

            tab[descending ? n - i - 1 : i] = (ushort)Math.Floor(v + 0.5);
        }
    }

    private static bool ExhaustiveCheck1D(uint start, uint stop)
    {
        if (HasConsole)
        {
            Console.WriteLine();
            Console.WriteLine($"\tTesting {start} - {stop}");
        }
        var startStr = start.ToString();
        var stopStr = stop.ToString();
        var widthSubtract = 16 + startStr.Length + stopStr.Length;
        for (var j = start; j <= stop; j++)
        {
            ProgressBar(start, stop, widthSubtract, j);

            if (!Check1D(j, false, 1)) return false;
        }

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");

        return true;
    }

    private static bool ExhaustiveCheck1DDown(uint start, uint stop)
    {
        if (HasConsole)
        {
            Console.WriteLine();
            Console.WriteLine($"\tTesting {start} - {stop}");
        }
        var startStr = start.ToString();
        var stopStr = stop.ToString();
        var widthSubtract = 16 + startStr.Length + stopStr.Length;
        for (var j = start; j <= stop; j++)
        {
            ProgressBar(start, stop, widthSubtract, j);

            if (!Check1D(j, true, 1)) return false;
        }

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");

        return true;
    }
    private static ushort Fn8D1(ReadOnlySpan<ushort> a) =>
        (ushort)a.ToArray()
                 .Average(i => i);

    private static ushort Fn8D2(ReadOnlySpan<ushort> a) =>
        (ushort)a.ToArray()
                 .Concat(Enumerable.Repeat<ushort>(0, 4))
                 .Select((i, j) => j is 2 or 3 ? 3 * i : i)
                 .Average(i => i);

    private static ushort Fn8D3(ReadOnlySpan<ushort> a) =>
        (ushort)a.ToArray()
                 .Concat(Enumerable.Repeat<ushort>(0, 5))
                 .Select((i, j) => j is 1 or 3 ? 3 * i : j is 2 ? 2 * i : i)
                 .Average(i => i);

    private static bool SamplerXD(ReadOnlySpan<ushort> @in, Span<ushort> @out, in object? cargo)
    {
        @out![0] = Fn8D1(@in[..8]);
        @out![1] = Fn8D2(@in[..8]);
        @out![1] = Fn8D3(@in[..8]);

        return true;
    }

    #endregion Private Methods
}
