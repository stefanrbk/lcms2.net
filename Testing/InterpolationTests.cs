using lcms2.plugins;
using lcms2.state;
using lcms2.types;

namespace lcms2.testing;

[TestFixture(TestOf = typeof(InterpolationPlugin))]
public class InterpolationTests: ITest
{
    private object _state;

    [SetUp]
    public void Setup()
    {
        _state = State.CreateStateContainer()!;
        try
        {
            _ = Console.BufferWidth;
        } catch
        {
            HasConsole = false;
        }
    }

    [TearDown]
    public void Teardown() =>
        State.DeleteStateContainer(_state);

    // A single function that does check 1D interpolation
    // nNodesToCheck = number on nodes to check
    // Down = Create decreasing tables
    // Reverse = Check reverse interpolation
    // max_err = max allowed error
    [TestCase(2u, false, 0)]
    [TestCase(3u, false, 1)]
    [TestCase(4u, false, 0)]
    [TestCase(6u, false, 0)]
    [TestCase(18u, false, 0)]
    [TestCase(2u, true, 0)]
    [TestCase(3u, true, 1)]
    [TestCase(6u, true, 0)]
    [TestCase(18u, true, 0)]
    public void Check1DTest(uint numNodesToCheck, bool down, int maxErr)
    {
        var tab = new ushort[numNodesToCheck];

        var p = InterpParams.Compute(_state, numNodesToCheck, 1, 1, tab, LerpFlag.Ushort);
        Assert.That(p, Is.Not.Null);

        BuildTable(numNodesToCheck, ref tab, down);

        for (var i = 0; i <= 0xFFFF; i++)
        {
            var @in = new ushort[] { (ushort)i };
            var @out = new ushort[1];

            p.Interpolation.Lerp16(in @in, ref @out, p);

            if (down) @out[0] = (ushort)(0xFFFF - @out[0]);

            Assert.That(@out[0], Is.EqualTo(@in[0]).Within(maxErr), $"{numNodesToCheck} nodes");
        }
    }

    [Explicit("Exhaustive test")]
    [TestCase(10u, 256 * 1u)]
    [TestCase(256 * 1u, 256 * 2u)]
    [TestCase(256 * 2u, 256 * 3u)]
    [TestCase(256 * 3u, 256 * 4u)]
    [TestCase(256 * 4u, 256 * 5u)]
    [TestCase(256 * 5u, 256 * 6u)]
    [TestCase(256 * 6u, 256 * 7u)]
    [TestCase(256 * 7u, 256 * 8u)]
    [TestCase(256 * 8u, 256 * 9u)]
    [TestCase(256 * 9u, 256 * 10u)]
    [TestCase(256 * 10u, 256 * 11u)]
    [TestCase(256 * 11u, 256 * 12u)]
    [TestCase(256 * 12u, 256 * 13u)]
    [TestCase(256 * 13u, 256 * 14u)]
    [TestCase(256 * 14u, 256 * 15u)]
    [TestCase(256 * 15u, 256 * 16u)]
    public void ExhaustiveCheck1DTest(uint start, uint stop) =>
        ExhaustiveCheck1D(Check1DTest, false, start, stop);

    [Explicit("Exhaustive test")]
    [TestCase(10u, 256 * 1u)]
    [TestCase(256 * 1u, 256 * 2u)]
    [TestCase(256 * 2u, 256 * 3u)]
    [TestCase(256 * 3u, 256 * 4u)]
    [TestCase(256 * 4u, 256 * 5u)]
    [TestCase(256 * 5u, 256 * 6u)]
    [TestCase(256 * 6u, 256 * 7u)]
    [TestCase(256 * 7u, 256 * 8u)]
    [TestCase(256 * 8u, 256 * 9u)]
    [TestCase(256 * 9u, 256 * 10u)]
    [TestCase(256 * 10u, 256 * 11u)]
    [TestCase(256 * 11u, 256 * 12u)]
    [TestCase(256 * 12u, 256 * 13u)]
    [TestCase(256 * 13u, 256 * 14u)]
    [TestCase(256 * 14u, 256 * 15u)]
    [TestCase(256 * 15u, 256 * 16u)]
    public void ExhaustiveCheck1DDownTest(uint start, uint stop) =>
        ExhaustiveCheck1D(Check1DTest, true, start, stop);

    private static void ExhaustiveCheck1D(Action<uint, bool, int> fn, bool down, uint start, uint stop)
    {
        if (HasConsole)
        {
            Console.WriteLine();
            Console.WriteLine($"\tTesting {start} - {stop}");
        }
        var startStr = start.ToString();
        var stopStr = stop.ToString();
        var widthSubtract = 16 + startStr.Length + stopStr.Length;

        Assert.Multiple(() =>
        {
            ClearAssert();

            for (var j = start; j <= stop; j++)
            {
                ProgressBar(start, stop, widthSubtract, j);

                fn(j, down, 1);
            }
        });

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");
    }

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

            tab[descending ? (n - i - 1) : i] = (ushort)Math.Floor(v + 0.5);
        }
    }

    [Test]
    public void Check3DInterpolationFloatTetrahedralTest()
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

        var p = InterpParams.Compute(_state, 2, 3, 3, floatTable, LerpFlag.Float);
        Assert.That(p, Is.Not.Null);

        MaxErr = 0.0;
        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new float[] { i / 65535f, i / 65535f , i / 65535f };
            var @out = new float[3];

            p.Interpolation.LerpFloat(@in, ref @out, p);
            Assert.Multiple(() =>
            {
                ClearAssert();

                IsGoodFixed15_16("Channel 1", @out[0], @in[0]);
                IsGoodFixed15_16("Channel 2", @out[1], @in[1]/ 2f);
                IsGoodFixed15_16("Channel 2", @out[2], @in[2]/ 4f);

                if (HasAssertions)
                    WriteLineRed($"|Err|<{MaxErr}");
            });
        }
    }

    [Test]
    public void Check3DInterpolationFloatTrilinearTest()
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

        var p = InterpParams.Compute(_state, 2, 3, 3, floatTable, LerpFlag.Float|LerpFlag.Trilinear);
        Assert.That(p, Is.Not.Null);

        MaxErr = 0.0;
        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new float[] { i / 65535f, i / 65535f , i / 65535f };
            var @out = new float[3];

            p.Interpolation.LerpFloat(@in, ref @out, p);
            Assert.Multiple(() =>
            {
                ClearAssert();

                IsGoodFixed15_16("Channel 1", @out[0], @in[0]);
                IsGoodFixed15_16("Channel 2", @out[1], @in[1] / 2f);
                IsGoodFixed15_16("Channel 2", @out[2], @in[2] / 4f);

                if (HasAssertions)
                    WriteLineRed($"|Err|<{MaxErr}");
            });
        }
    }

    [Test]
    public void Check3DInterpolation16TetrahedralTest()
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

        var p = InterpParams.Compute(_state, 2, 3, 3, table, LerpFlag.Ushort);
        Assert.That(p, Is.Not.Null);

        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new ushort[] { (ushort)i, (ushort)i, (ushort)i };
            var @out = new ushort[3];

            p.Interpolation.Lerp16(@in, ref @out, p);
            Assert.Multiple(() =>
            {
                ClearAssert();

                IsGoodWord("Channel 1", @out[0], @in[0]);
                IsGoodWord("Channel 2", @out[1], @in[1]);
                IsGoodWord("Channel 2", @out[2], @in[2]);
            });
        }
    }

    [Test]
    public void Check3DInterpolation16TrilinearTest()
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

        var p = InterpParams.Compute(_state, 2, 3, 3, table, LerpFlag.Trilinear);
        Assert.That(p, Is.Not.Null);

        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new ushort[] { (ushort)i, (ushort)i, (ushort)i };
            var @out = new ushort[3];

            p.Interpolation.Lerp16(@in, ref @out, p);
            Assert.Multiple(() =>
            {
                ClearAssert();

                IsGoodWord("Channel 1", @out[0], @in[0]);
                IsGoodWord("Channel 2", @out[1], @in[1]);
                IsGoodWord("Channel 2", @out[2], @in[2]);
            });
        }
    }

    [Explicit]
    [TestCase(16*0u, 16*1u)]
    [TestCase(16*1u, 16*2u)]
    [TestCase(16*2u, 16*3u)]
    [TestCase(16*3u, 16*4u)]
    [TestCase(16*4u, 16*5u)]
    [TestCase(16*5u, 16*6u)]
    [TestCase(16*6u, 16*7u)]
    [TestCase(16*7u, 16*8u)]
    [TestCase(16*8u, 16*9u)]
    [TestCase(16*9u, 16*10u)]
    [TestCase(16*10u, 16*11u)]
    [TestCase(16*11u, 16*12u)]
    [TestCase(16*12u, 16*13u)]
    [TestCase(16*13u, 16*14u)]
    [TestCase(16*14u, 16*15u)]
    [TestCase(16*15u, 16*16u)]
    public void ExhaustiveCheck3DInterpolationFloatTetrahedralTest(uint start, uint stop)
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

        var p = InterpParams.Compute(_state, 2, 3, 3, floatTable, LerpFlag.Float);
        Assert.That(p, Is.Not.Null);

        MaxErr = 0.0;
        Assert.Multiple(() =>
        {
            for (var r = start; r < stop; r++)
            {
                for (var g = 0; g < 0xFF; g++)
                {
                    for (var b = 0; b < 0xFF; b++)
                    {
                        var @in = new float[] { r / 255f, g / 255f , b / 255f };
                        var @out = new float[3];

                        p.Interpolation.LerpFloat(@in, ref @out, p);
                        Assert.Multiple(() =>
                        {
                            ClearAssert();

                            IsGoodFixed15_16("Channel 1", @out[0], @in[0]);
                            IsGoodFixed15_16("Channel 2", @out[1], @in[1] / 2f);
                            IsGoodFixed15_16("Channel 2", @out[2], @in[2] / 4f);

                            if (HasAssertions)
                                WriteLineRed($"|Err|<{MaxErr}");
                        });
                    }
                }

                ProgressBar(start, stop, widthSubtract, r);
            }
        });

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");
    }

    [Explicit]
    [TestCase(16 * 0u, 16 * 1u)]
    [TestCase(16 * 1u, 16 * 2u)]
    [TestCase(16 * 2u, 16 * 3u)]
    [TestCase(16 * 3u, 16 * 4u)]
    [TestCase(16 * 4u, 16 * 5u)]
    [TestCase(16 * 5u, 16 * 6u)]
    [TestCase(16 * 6u, 16 * 7u)]
    [TestCase(16 * 7u, 16 * 8u)]
    [TestCase(16 * 8u, 16 * 9u)]
    [TestCase(16 * 9u, 16 * 10u)]
    [TestCase(16 * 10u, 16 * 11u)]
    [TestCase(16 * 11u, 16 * 12u)]
    [TestCase(16 * 12u, 16 * 13u)]
    [TestCase(16 * 13u, 16 * 14u)]
    [TestCase(16 * 14u, 16 * 15u)]
    [TestCase(16 * 15u, 16 * 16u)]
    public void ExhaustiveCheck3DInterpolationFloatTrilinearTest(uint start, uint stop)
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

        var p = InterpParams.Compute(_state, 2, 3, 3, floatTable, LerpFlag.Float|LerpFlag.Trilinear);
        Assert.That(p, Is.Not.Null);

        MaxErr = 0.0;
        Assert.Multiple(() =>
        {
            for (var r = start; r < stop; r++)
            {
                for (var g = 0; g < 0xFF; g++)
                {
                    for (var b = 0; b < 0xFF; b++)
                    {
                        var @in = new float[] { r / 255f, g / 255f , b / 255f };
                        var @out = new float[3];

                        p.Interpolation.LerpFloat(@in, ref @out, p);
                        Assert.Multiple(() =>
                        {
                            ClearAssert();

                            IsGoodFixed15_16("Channel 1", @out[0], @in[0]);
                            IsGoodFixed15_16("Channel 2", @out[1], @in[1] / 2f);
                            IsGoodFixed15_16("Channel 2", @out[2], @in[2] / 4f);

                            if (HasAssertions)
                                WriteLineRed($"|Err|<{MaxErr}");
                        });
                    }
                }

                ProgressBar(start, stop, widthSubtract, r);
            }
        });

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");
    }

    [Explicit]
    [TestCase(16 * 0u, 16 * 1u)]
    [TestCase(16 * 1u, 16 * 2u)]
    [TestCase(16 * 2u, 16 * 3u)]
    [TestCase(16 * 3u, 16 * 4u)]
    [TestCase(16 * 4u, 16 * 5u)]
    [TestCase(16 * 5u, 16 * 6u)]
    [TestCase(16 * 6u, 16 * 7u)]
    [TestCase(16 * 7u, 16 * 8u)]
    [TestCase(16 * 8u, 16 * 9u)]
    [TestCase(16 * 9u, 16 * 10u)]
    [TestCase(16 * 10u, 16 * 11u)]
    [TestCase(16 * 11u, 16 * 12u)]
    [TestCase(16 * 12u, 16 * 13u)]
    [TestCase(16 * 13u, 16 * 14u)]
    [TestCase(16 * 14u, 16 * 15u)]
    [TestCase(16 * 15u, 16 * 16u)]
    public void ExhaustiveCheck3DInterpolation16TetrahedralTest(uint start, uint stop)
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

        var p = InterpParams.Compute(_state, 2, 3, 3, table, LerpFlag.Ushort);
        Assert.That(p, Is.Not.Null);

        Assert.Multiple(() =>
        {
            for (var r = start; r < stop; r++)
            {
                for (var g = 0; g < 0xFF; g++)
                {
                    for (var b = 0; b < 0xFF; b++)
                    {
                        var @in = new ushort[] { (ushort)r, (ushort)g, (ushort)b };
                        var @out = new ushort[3];

                        p.Interpolation.Lerp16(@in, ref @out, p);
                        Assert.Multiple(() =>
                        {
                            ClearAssert();

                            IsGoodWord("Channel 1", @out[0], @in[0]);
                            IsGoodWord("Channel 2", @out[1], @in[1]);
                            IsGoodWord("Channel 2", @out[2], @in[2]);
                        });
                    }
                }

                ProgressBar(start, stop, widthSubtract, r);
            }
        });

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");
    }

    [Explicit]
    [TestCase(16 * 0u, 16 * 1u)]
    [TestCase(16 * 1u, 16 * 2u)]
    [TestCase(16 * 2u, 16 * 3u)]
    [TestCase(16 * 3u, 16 * 4u)]
    [TestCase(16 * 4u, 16 * 5u)]
    [TestCase(16 * 5u, 16 * 6u)]
    [TestCase(16 * 6u, 16 * 7u)]
    [TestCase(16 * 7u, 16 * 8u)]
    [TestCase(16 * 8u, 16 * 9u)]
    [TestCase(16 * 9u, 16 * 10u)]
    [TestCase(16 * 10u, 16 * 11u)]
    [TestCase(16 * 11u, 16 * 12u)]
    [TestCase(16 * 12u, 16 * 13u)]
    [TestCase(16 * 13u, 16 * 14u)]
    [TestCase(16 * 14u, 16 * 15u)]
    [TestCase(16 * 15u, 16 * 16u)]
    public void ExhaustiveCheck3DInterpolation16TrilinearTest(uint start, uint stop)
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

        var p = InterpParams.Compute(_state, 2, 3, 3, table, LerpFlag.Trilinear);
        Assert.That(p, Is.Not.Null);

        Assert.Multiple(() =>
        {
            for (var r = start; r < stop; r++)
            {
                for (var g = 0; g < 0xFF; g++)
                {
                    for (var b = 0; b < 0xFF; b++)
                    {
                        var @in = new ushort[] { (ushort)r, (ushort)g, (ushort)b };
                        var @out = new ushort[3];

                        p.Interpolation.Lerp16(@in, ref @out, p);
                        Assert.Multiple(() =>
                        {
                            ClearAssert();

                            IsGoodWord("Channel 1", @out[0], @in[0]);
                            IsGoodWord("Channel 2", @out[1], @in[1]);
                            IsGoodWord("Channel 2", @out[2], @in[2]);
                        });
                    }
                }

                ProgressBar(start, stop, widthSubtract, r);
            }
        });

        if (HasConsole)
            Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");
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

    [Test]
    public void Check3DInterpTest()
    {
        var lut = Pipeline.Alloc(_state, 3, 3);
        var mpe = Stage.AllocCLut16bit(_state, 9, 3, 3, null);
        Assert.Multiple(() =>
        {
            ClearAssert();

            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(Sampler3D, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        Assert.Multiple(() =>
        {
            ClearAssert();

            CheckOne3D(lut, 0x0000, 0x0000, 0x0000);
            CheckOne3D(lut, 0xFFFF, 0xFFFF, 0xFFFF);

            CheckOne3D(lut, 0x8080, 0x8080, 0x8080);
            CheckOne3D(lut, 0x0000, 0xFE00, 0x80FF);
            CheckOne3D(lut, 0x1111, 0x2222, 0x3333);
            CheckOne3D(lut, 0x0000, 0x0012, 0x0013);
            CheckOne3D(lut, 0x3141, 0x1415, 0x1592);
            CheckOne3D(lut, 0xFF00, 0xFF01, 0xFF12);
        });
    }

    [Test]
    public void Check3DInterpGranularTest()
    {
        var dims = new uint[] { 7, 8, 9 };

        var lut = Pipeline.Alloc(_state, 3, 3);
        var mpe = Stage.AllocCLut16bit(_state, dims, 3, 3, null);
        Assert.Multiple(() =>
        {
            ClearAssert();

            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(Sampler3D, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        Assert.Multiple(() =>
        {
            ClearAssert();

            CheckOne3D(lut, 0x0000, 0x0000, 0x0000);
            CheckOne3D(lut, 0xFFFF, 0xFFFF, 0xFFFF);

            CheckOne3D(lut, 0x8080, 0x8080, 0x8080);
            CheckOne3D(lut, 0x0000, 0xFE00, 0x80FF);
            CheckOne3D(lut, 0x1111, 0x2222, 0x3333);
            CheckOne3D(lut, 0x0000, 0x0012, 0x0013);
            CheckOne3D(lut, 0x3141, 0x1415, 0x1592);
            CheckOne3D(lut, 0xFF00, 0xFF01, 0xFF12);
        });
    }

    [Test]
    public void Check4DInterpTest()
    {
        var lut = Pipeline.Alloc(_state, 4, 3);
        var mpe = Stage.AllocCLut16bit(_state, 9, 4, 3, null);
        Assert.Multiple(() =>
        {
            ClearAssert();

            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(Sampler4D, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        Assert.Multiple(() =>
        {
            ClearAssert();

            CheckOne4D(lut, 0x0000, 0x0000, 0x0000, 0x0000);
            CheckOne4D(lut, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF);

            CheckOne4D(lut, 0x8080, 0x8080, 0x8080, 0x8080);
            CheckOne4D(lut, 0x0000, 0xFE00, 0x80FF, 0x8888);
            CheckOne4D(lut, 0x1111, 0x2222, 0x3333, 0x4444);
            CheckOne4D(lut, 0x0000, 0x0012, 0x0013, 0x0014);
            CheckOne4D(lut, 0x3141, 0x1415, 0x1592, 0x9261);
            CheckOne4D(lut, 0xFF00, 0xFF01, 0xFF12, 0xFF13);
        });
    }

    [Test]
    public void Check4DInterpGranularTest()
    {
        var dims = new uint[] { 9, 8, 7, 6 };

        var lut = Pipeline.Alloc(_state, 4, 3);
        var mpe = Stage.AllocCLut16bit(_state, dims, 4, 3, null);
        Assert.Multiple(() =>
        {
            ClearAssert();

            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(Sampler4D, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        Assert.Multiple(() =>
        {
            ClearAssert();

            CheckOne4D(lut, 0x0000, 0x0000, 0x0000, 0x0000);
            CheckOne4D(lut, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF);

            CheckOne4D(lut, 0x8080, 0x8080, 0x8080, 0x8080);
            CheckOne4D(lut, 0x0000, 0xFE00, 0x80FF, 0x8888);
            CheckOne4D(lut, 0x1111, 0x2222, 0x3333, 0x4444);
            CheckOne4D(lut, 0x0000, 0x0012, 0x0013, 0x0014);
            CheckOne4D(lut, 0x3141, 0x1415, 0x1592, 0x9261);
            CheckOne4D(lut, 0xFF00, 0xFF01, 0xFF12, 0xFF13);
        });
    }

    [Test]
    public void Check5DInterpGranularTest()
    {
        var dims = new uint[] { 3, 2, 2, 2, 2 };

        var lut = Pipeline.Alloc(_state, 5, 3);
        var mpe = Stage.AllocCLut16bit(_state, dims, 5, 3, null);
        Assert.Multiple(() =>
        {
            ClearAssert();

            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(Sampler5D, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        Assert.Multiple(() =>
        {
            ClearAssert();

            CheckOne5D(lut, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000);
            CheckOne5D(lut, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF);

            CheckOne5D(lut, 0x8080, 0x8080, 0x8080, 0x8080, 0x1234);
            CheckOne5D(lut, 0x0000, 0xFE00, 0x80FF, 0x8888, 0x8878);
            CheckOne5D(lut, 0x1111, 0x2222, 0x3333, 0x4444, 0x1455);
            CheckOne5D(lut, 0x0000, 0x0012, 0x0013, 0x0014, 0x2333);
            CheckOne5D(lut, 0x3141, 0x1415, 0x1592, 0x9261, 0x4567);
            CheckOne5D(lut, 0xFF00, 0xFF01, 0xFF12, 0xFF13, 0xF344);
        });
    }

    [Test]
    public void Check6DInterpGranularTest()
    {
        var dims = new uint[] { 4, 3, 3, 2, 2, 2 };

        var lut = Pipeline.Alloc(_state, 6, 3);
        var mpe = Stage.AllocCLut16bit(_state, dims, 6, 3, null);
        Assert.Multiple(() =>
        {
            ClearAssert();

            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(Sampler6D, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        Assert.Multiple(() =>
        {
            ClearAssert();

            CheckOne6D(lut, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000);
            CheckOne6D(lut, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0x0000, 0xFFFF);

            CheckOne6D(lut, 0x8080, 0x8080, 0x8080, 0x8080, 0x1234, 0x1122);
            CheckOne6D(lut, 0x0000, 0xFE00, 0x80FF, 0x8888, 0x8878, 0x2233);
            CheckOne6D(lut, 0x1111, 0x2222, 0x3333, 0x4444, 0x1455, 0x3344);
            CheckOne6D(lut, 0x0000, 0x0012, 0x0013, 0x0014, 0x2333, 0x4455);
            CheckOne6D(lut, 0x3141, 0x1415, 0x1592, 0x9261, 0x4567, 0x5566);
            CheckOne6D(lut, 0xFF00, 0xFF01, 0xFF12, 0xFF13, 0xF344, 0x6677);
        });
    }

    [Test]
    public void Check7DInterpGranularTest()
    {
        var dims = new uint[] { 4, 3, 3, 2, 2, 2, 2 };

        var lut = Pipeline.Alloc(_state, 7, 3);
        var mpe = Stage.AllocCLut16bit(_state, dims, 7, 3, null);
        Assert.Multiple(() =>
        {
            ClearAssert();

            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(Sampler7D, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        Assert.Multiple(() =>
        {
            ClearAssert();

            CheckOne7D(lut, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000);
            CheckOne7D(lut, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0x0000, 0xFFFF, 0xFFFF);

            CheckOne7D(lut, 0x8080, 0x8080, 0x8080, 0x8080, 0x1234, 0x1122, 0x0056);
            CheckOne7D(lut, 0x0000, 0xFE00, 0x80FF, 0x8888, 0x8878, 0x2233, 0x0088);
            CheckOne7D(lut, 0x1111, 0x2222, 0x3333, 0x4444, 0x1455, 0x3344, 0x1987);
            CheckOne7D(lut, 0x0000, 0x0012, 0x0013, 0x0014, 0x2333, 0x4455, 0x9988);
            CheckOne7D(lut, 0x3141, 0x1415, 0x1592, 0x9261, 0x4567, 0x5566, 0xFE56);
            CheckOne7D(lut, 0xFF00, 0xFF01, 0xFF12, 0xFF13, 0xF344, 0x6677, 0xBABE);
        });
    }

    [Test]
    public void Check8DInterpGranularTest()
    {
        var dims = new uint[] { 4, 3, 3, 2, 2, 2, 2, 2 };

        var lut = Pipeline.Alloc(_state, 8, 3);
        var mpe = Stage.AllocCLut16bit(_state, dims, 8, 3, null);
        Assert.Multiple(() =>
        {
            ClearAssert();

            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(Sampler8D, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        Assert.Multiple(() =>
        {
            ClearAssert();

            CheckOne8D(lut, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000);
            CheckOne8D(lut, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0x0000, 0xFFFF, 0xFFFF, 0xFFFF);

            CheckOne8D(lut, 0x8080, 0x8080, 0x8080, 0x8080, 0x1234, 0x1122, 0x0056, 0x0011);
            CheckOne8D(lut, 0x0000, 0xFE00, 0x80FF, 0x8888, 0x8878, 0x2233, 0x0088, 0x2020);
            CheckOne8D(lut, 0x1111, 0x2222, 0x3333, 0x4444, 0x1455, 0x3344, 0x1987, 0x4532);
            CheckOne8D(lut, 0x0000, 0x0012, 0x0013, 0x0014, 0x2333, 0x4455, 0x9988, 0x1200);
            CheckOne8D(lut, 0x3141, 0x1415, 0x1592, 0x9261, 0x4567, 0x5566, 0xFE56, 0x6666);
            CheckOne8D(lut, 0xFF00, 0xFF01, 0xFF12, 0xFF13, 0xF344, 0x6677, 0xBABE, 0xFACE);
        });
    }

    private static void CheckOne3D(Pipeline lut, ushort a1, ushort a2, ushort a3)
    {
        var @in = new ushort[3]
        {
            a1, a2, a3
        };
        var out1 = new ushort[3];
        var out2 = new ushort[3];

        // This is the interpolated value
        lut.Eval(@in, out1);

        // This is the real value
        Sampler3D(@in, out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            IsGoodWord("Channel 1", out1[0], out2[0], 2);
            IsGoodWord("Channel 2", out1[1], out2[1], 2);
            IsGoodWord("Channel 3", out1[2], out2[2], 2);
        });
    }

    private static void CheckOne4D(Pipeline lut, ushort a1, ushort a2, ushort a3, ushort a4)
    {
        var @in = new ushort[4]
        {
            a1, a2, a3, a4
        };
        var out1 = new ushort[3];
        var out2 = new ushort[3];

        // This is the interpolated value
        lut.Eval(@in, out1);

        // This is the real value
        Sampler4D(@in, out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            IsGoodWord("Channel 1", out1[0], out2[0], 2);
            IsGoodWord("Channel 2", out1[1], out2[1], 2);
            IsGoodWord("Channel 3", out1[2], out2[2], 2);
        });
    }

    private static void CheckOne5D(Pipeline lut, ushort a1, ushort a2, ushort a3, ushort a4, ushort a5)
    {
        var @in = new ushort[5]
        {
            a1, a2, a3, a4, a5
        };
        var out1 = new ushort[3];
        var out2 = new ushort[3];

        // This is the interpolated value
        lut.Eval(@in, out1);

        // This is the real value
        Sampler5D(@in, out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            IsGoodWord("Channel 1", out1[0], out2[0], 2);
            IsGoodWord("Channel 2", out1[1], out2[1], 2);
            IsGoodWord("Channel 3", out1[2], out2[2], 2);
        });
    }

    private static void CheckOne6D(Pipeline lut, ushort a1, ushort a2, ushort a3, ushort a4, ushort a5, ushort a6)
    {
        var @in = new ushort[6]
        {
            a1, a2, a3, a4, a5, a6
        };
        var out1 = new ushort[3];
        var out2 = new ushort[3];

        // This is the interpolated value
        lut.Eval(@in, out1);

        // This is the real value
        Sampler6D(@in, out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            IsGoodWord("Channel 1", out1[0], out2[0], 2);
            IsGoodWord("Channel 2", out1[1], out2[1], 2);
            IsGoodWord("Channel 3", out1[2], out2[2], 2);
        });
    }

    private static void CheckOne7D(Pipeline lut, ushort a1, ushort a2, ushort a3, ushort a4, ushort a5, ushort a6, ushort a7)
    {
        var @in = new ushort[7]
        {
            a1, a2, a3, a4, a5, a6, a7
        };
        var out1 = new ushort[3];
        var out2 = new ushort[3];

        // This is the interpolated value
        lut.Eval(@in, out1);

        // This is the real value
        Sampler7D(@in, out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            IsGoodWord("Channel 1", out1[0], out2[0], 2);
            IsGoodWord("Channel 2", out1[1], out2[1], 2);
            IsGoodWord("Channel 3", out1[2], out2[2], 2);
        });
    }

    private static void CheckOne8D(Pipeline lut, ushort a1, ushort a2, ushort a3, ushort a4, ushort a5, ushort a6, ushort a7, ushort a8)
    {
        var @in = new ushort[8]
        {
            a1, a2, a3, a4, a5, a6, a7, a8
        };
        var out1 = new ushort[3];
        var out2 = new ushort[3];

        // This is the interpolated value
        lut.Eval(@in, out1);

        // This is the real value
        Sampler8D(@in, out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            IsGoodWord("Channel 1", out1[0], out2[0], 2);
            IsGoodWord("Channel 2", out1[1], out2[1], 2);
            IsGoodWord("Channel 3", out1[2], out2[2], 2);
        });
    }

    private static bool Sampler3D(in ushort[] @in, ushort[]? @out, in object? cargo)
    {
        @out![0] = Fn8D1(@in[0], @in[1], @in[2], 0, 0, 0, 0, 0, 3);
        @out![1] = Fn8D2(@in[0], @in[1], @in[2], 0, 0, 0, 0, 0, 3);
        @out![2] = Fn8D3(@in[0], @in[1], @in[2], 0, 0, 0, 0, 0, 3);

        return true;
    }

    private static bool Sampler4D(in ushort[] @in, ushort[]? @out, in object? cargo)
    {
        @out![0] = Fn8D1(@in[0], @in[1], @in[2], @in[3], 0, 0, 0, 0, 4);
        @out![1] = Fn8D2(@in[0], @in[1], @in[2], @in[3], 0, 0, 0, 0, 4);
        @out![2] = Fn8D3(@in[0], @in[1], @in[2], @in[3], 0, 0, 0, 0, 4);

        return true;
    }

    private static bool Sampler5D(in ushort[] @in, ushort[]? @out, in object? cargo)
    {
        @out![0] = Fn8D1(@in[0], @in[1], @in[2], @in[3], @in[4], 0, 0, 0, 5);
        @out![1] = Fn8D2(@in[0], @in[1], @in[2], @in[3], @in[4], 0, 0, 0, 5);
        @out![2] = Fn8D3(@in[0], @in[1], @in[2], @in[3], @in[4], 0, 0, 0, 5);

        return true;
    }

    private static bool Sampler6D(in ushort[] @in, ushort[]? @out, in object? cargo)
    {
        @out![0] = Fn8D1(@in[0], @in[1], @in[2], @in[3], @in[4], @in[5], 0, 0, 6);
        @out![1] = Fn8D2(@in[0], @in[1], @in[2], @in[3], @in[4], @in[5], 0, 0, 6);
        @out![2] = Fn8D3(@in[0], @in[1], @in[2], @in[3], @in[4], @in[5], 0, 0, 6);

        return true;
    }

    private static bool Sampler7D(in ushort[] @in, ushort[]? @out, in object? cargo)
    {
        @out![0] = Fn8D1(@in[0], @in[1], @in[2], @in[3], @in[4], @in[5], @in[6], 0, 7);
        @out![1] = Fn8D2(@in[0], @in[1], @in[2], @in[3], @in[4], @in[5], @in[6], 0, 7);
        @out![2] = Fn8D3(@in[0], @in[1], @in[2], @in[3], @in[4], @in[5], @in[6], 0, 7);

        return true;
    }

    private static bool Sampler8D(in ushort[] @in, ushort[]? @out, in object? cargo)
    {
        @out![0] = Fn8D1(@in[0], @in[1], @in[2], @in[3], @in[4], @in[5], @in[6], @in[7], 8);
        @out![1] = Fn8D2(@in[0], @in[1], @in[2], @in[3], @in[4], @in[5], @in[6], @in[7], 8);
        @out![2] = Fn8D3(@in[0], @in[1], @in[2], @in[3], @in[4], @in[5], @in[6], @in[7], 8);

        return true;
    }

    private static ushort Fn8D1(ushort a1, ushort a2, ushort a3, ushort a4, ushort a5, ushort a6, ushort a7, ushort a8, uint m) =>
        (ushort)(((uint)a1 + a2 + a3 + a4 + a5 + a6 + a7 + a8) / m);

    private static ushort Fn8D2(ushort a1, ushort a2, ushort a3, ushort a4, ushort a5, ushort a6, ushort a7, ushort a8, uint m) =>
        (ushort)(((uint)a1 + (3 * a2) + (3 * a3) + a4 + a5 + a6 + a7 + a8) / (m + 4));

    private static ushort Fn8D3(ushort a1, ushort a2, ushort a3, ushort a4, ushort a5, ushort a6, ushort a7, ushort a8, uint m) =>
        (ushort)(((3 * a1) + (2 * a2) + (3 * a3) + a4 + a5 + a6 + a7 + a8) / (m + 5));
}
