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
using lcms2.types;

namespace lcms2.tests.types;

[TestFixture, TestOf(typeof(Pipeline)), TestOf(typeof(Stage))]
public class LutTests : TestBase
{
    #region Fields

    private Pipeline lut;

    #endregion Fields

    #region Public Methods

    [Test, Category(FixedTest)]
    public void Lut1StageBasicsTest()
    {
        AddIdentityMatrix(ref lut);

        {
            Assert.That(lut.StageCount, Is.EqualTo(1), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Lut2Stage16BasicsTest()
    {
        AddIdentityMatrix(ref lut);
        AddIdentityClut16(ref lut);

        {
            Assert.That(lut.StageCount, Is.EqualTo(2), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Lut2StageBasicsTest()
    {
        AddIdentityMatrix(ref lut);
        AddIdentityClutFloat(ref lut);

        {
            Assert.That(lut.StageCount, Is.EqualTo(2), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Lut3Stage16BasicsTest()
    {
        AddIdentityMatrix(ref lut);
        AddIdentityClut16(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        {
            Assert.That(lut.StageCount, Is.EqualTo(3), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Lut3StageBasicsTest()
    {
        AddIdentityMatrix(ref lut);
        AddIdentityClutFloat(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        {
            Assert.That(lut.StageCount, Is.EqualTo(3), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Lut4Stage16BasicsTest()
    {
        AddIdentityMatrix(ref lut);
        AddIdentityClut16(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);

        {
            Assert.That(lut.StageCount, Is.EqualTo(4), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Lut4StageBasicsTest()
    {
        AddIdentityMatrix(ref lut);
        AddIdentityClutFloat(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);

        {
            Assert.That(lut.StageCount, Is.EqualTo(4), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Lut5Stage16BasicsTest()
    {
        AddIdentityMatrix(ref lut);
        AddIdentityClut16(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        {
            Assert.That(lut.StageCount, Is.EqualTo(5), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Lut5StageBasicsTest()
    {
        AddIdentityMatrix(ref lut);
        AddIdentityClutFloat(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        {
            Assert.That(lut.StageCount, Is.EqualTo(5), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Lut6Stage16BasicsTest()
    {
        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityClut16(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        {
            Assert.That(lut.StageCount, Is.EqualTo(6), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Lut6StageBasicsTest()
    {
        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityClutFloat(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        {
            Assert.That(lut.StageCount, Is.EqualTo(6), "Stage count check failed");

            // Check float
            var n1 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
                var outf = new float[3];
                lut.Eval(inf, outf);

                var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

                n1 += af.Count(i => i != j);
            }

            // Check ushort
            var n2 = 0;

            for (var j = 0; j < 65535; j++)
            {
                var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
                var outw = new ushort[3];
                lut.Eval(inw, outw);

                n2 += outw.Count(i => i != j);
            }

            Assert.Multiple(() =>
            {
                Assert.That(n1, Is.Zero, "Float check failed");
                Assert.That(n2, Is.Zero, "16 bit check failed");
            });
        }
    }

    [Test, Category(FixedTest)]
    public void NamedColorLutTest()
    {
        var nc = new NamedColorList(null, 256, 3, "pre", "post");

        for (var i = 0; i < 256; i++)
        {
            var pcs = Enumerable.Repeat((ushort)i, 3).ToArray();
            var colorant = Enumerable.Repeat((ushort)i, 4).ToArray();

            var name = $"#{i}";
            nc.Append(name, pcs, colorant);
        }
        lut.InsertStage(StageLoc.AtEnd, Stage.AllocNamedColor(nc, false));

        var inw = new ushort[3];
        var outw = new ushort[3];
        Assert.Multiple(() =>
        {
            for (var j = 0; j < 256; j++)
            {
                inw[0] = (ushort)j;

                lut.Eval(inw, outw);
                if (outw.Any(i => i != j))
                    Assert.Fail($"Failed eval pass {j}");
            }
        });
    }

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        var l = Pipeline.Alloc(null, 3, 3);
        Assert.That(l, Is.Not.Null);
        lut = l;
    }

    [TearDown]
    public void TearDown() =>
        lut?.Dispose();

    #endregion Public Methods

    #region Private Methods

    private static void Add3GammaCurves(ref Pipeline lut, double curve)
    {
        using var id = ToneCurve.BuildGamma(null, curve);
        if (id is null)
        {
            lut = null!;
            return;
        }

        var id3 = new[] { id, id, id };

        lut.InsertStage(
            StageLoc.AtEnd,
            Stage.AllocToneCurves(null, 3, id3));
    }

    private static void AddIdentityClut16(ref Pipeline lut) =>
            lut.InsertStage(
            StageLoc.AtEnd,
            Stage.AllocCLut16bit(
                null,
                2,
                3,
                3,
                new ushort[]
                {
                    0,      0,      0,
                    0,      0,      0xFFFF,

                    0,      0xFFFF, 0,
                    0,      0xFFFF, 0xFFFF,

                    0xFFFF, 0,      0,
                    0xFFFF, 0,      0xFFFF,

                    0xFFFF, 0xFFFF, 0,
                    0xFFFF, 0xFFFF, 0xFFFF
                }));

    private static void AddIdentityClutFloat(ref Pipeline lut) =>
        lut.InsertStage(
            StageLoc.AtEnd,
            Stage.AllocCLutFloat(
                null,
                2,
                3,
                3,
                new float[]
                {
                    0, 0, 0,
                    0, 0, 1,

                    0, 1, 0,
                    0, 1, 1,

                    1, 0, 0,
                    1, 0, 1,

                    1, 1, 0,
                    1, 1, 1
                }));

    private static void AddIdentityMatrix(ref Pipeline lut) =>
                lut.InsertStage(
            StageLoc.AtEnd,
            Stage.AllocMatrix(
                null,
                3,
                3,
                new double[]
                {
                    1, 0, 0,
                    0, 1, 0,
                    0, 0, 1,
                    0, 0, 0
                },
                default));

    #endregion Private Methods
}
