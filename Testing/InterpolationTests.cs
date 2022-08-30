using System.Reflection.Metadata.Ecma335;

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
}
