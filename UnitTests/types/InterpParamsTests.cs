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

public class InterpParamsTests
{
    #region Fields

    private static readonly ushort[][] _checkXDData =
    {
        new ushort[] { 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000 },
        new ushort[] { 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF },
        new ushort[] { 0x8080, 0x8080, 0x8080, 0x8080, 0x1234, 0x1122, 0x0056, 0x0011 },
        new ushort[] { 0x0000, 0xFE00, 0x80FF, 0x8888, 0x8878, 0x2233, 0x0088, 0x2020 },
        new ushort[] { 0x1111, 0x2222, 0x3333, 0x4444, 0x1455, 0x3344, 0x1987, 0x4532 },
        new ushort[] { 0x0000, 0x0012, 0x0013, 0x0014, 0x2333, 0x4455, 0x9988, 0x1200 },
        new ushort[] { 0x3141, 0x1415, 0x1592, 0x9261, 0x4567, 0x5566, 0xFE56, 0x6666 },
        new ushort[] { 0xFF00, 0xFF01, 0xFF12, 0xFF13, 0xF344, 0x6677, 0xBABE, 0xFACE },
    };

    private static readonly uint[][] _checkXDDims =
    {
        new uint[] { 7, 8, 9 },
        new uint[] { 9, 8, 7, 6 },
        new uint[] { 3, 2, 2, 2, 2 },
        new uint[] { 4, 3, 3, 2, 2, 2 },
        new uint[] { 4, 3, 3, 2, 2, 2, 2 },
        new uint[] { 4, 3, 3, 2, 2, 2, 2, 2 }
    };

    private static readonly object[][] _interp1DValues = new[]
    {
        new object[] { 2u, false, 0 },
        new object[] { 3u, false, 1 },
        new object[] { 4u, false, 0 },
        new object[] { 6u, false, 0 },
        new object[] { 18u, false, 0 },
        new object[] { 2u, true, 0 },
        new object[] { 3u, true, 1 },
        new object[] { 4u, true, 0 },
        new object[] { 6u, true, 0 },
        new object[] { 18u, true, 0 },
    };

    #endregion Fields

    #region Public Methods

    [Test, Category(FixedTest)]
    public void CheckReverseInterpolation3x3()
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

        var lut = Pipeline.Alloc(null, 3, 3);
        Assert.That(lut, Is.Not.Null);

        var clut = Stage.AllocCLut16bit(null, 2, 3, 3, table);
        lut.InsertStage(StageLoc.AtBegin, clut);

        lut.EvalReverse(target, result, null);
        if (!result.Take(3).Contains(0))
            Assert.Fail("Reverse interpolation didn't find zero");

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

    [Test, Category(FixedTest)]
    public void CheckReverseInterpolation4x3()
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

        var lut = Pipeline.Alloc(null, 4, 3);
        Assert.That(lut, Is.Not.Null);

        var clut = Stage.AllocCLut16bit(null, 2, 4, 3, table);
        lut.InsertStage(StageLoc.AtBegin, clut);

        // Check if the LUT is behaving as expected
        for (var i = 0; i <= 100; i++)
        {
            target[0] = i / 100f;
            target[1] = target[0];
            target[2] = 0;
            target[3] = 12;

            lut.Eval(target, result);

            Assert.Multiple(() =>
            {
                IsGoodFixed15_16($"4->3 feasibility\n({i}): 0", target[0], result[0]);
                IsGoodFixed15_16($"4->3 feasibility\n({i}): 1", target[1], result[1]);
                IsGoodFixed15_16($"4->3 feasibility\n({i}): 2", target[2], result[2]);
            });
        }

        target[0] = target[1] = target[2] = 0;

        // This one holds the fixed k
        target[3] = 0;

        // This is our hint (which is a big lie in this case)
        Enumerable.Repeat(0.1f, 3).ToArray().CopyTo(hint, 0);

        lut.EvalReverse(target, result, hint);

        if (!result.Contains(0))
            Assert.Fail("4->3 zero\nReverse interpolation didn't find zero");

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
        Assert.That(max, Is.LessThanOrEqualTo(FloatPrecision), "4->3 find CMY");
    }

    [Test, Category(FixedTest)]
    public void CheckXDInterpGranular([Range(3u, 8u)] uint inputChans)
    {
        var dims = _checkXDDims[inputChans - 3][..(int)inputChans];

        var lut = Pipeline.Alloc(null, inputChans, 3);
        var mpe = Stage.AllocCLut16bit(null, dims, inputChans, 3, null);

        Assert.Multiple(() =>
        {
            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(SamplerXD, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        Assert.Multiple(() =>
        {
            foreach (var test in _checkXDData)
                CheckOneXD(lut, test[..(int)inputChans]);
        });
    }

    [TestCaseSource(nameof(_interp1DValues), Category = FixedTest)]
    public void Interp1DTest(uint numNodesToCheck, bool down, int maxErr)
    {
        var tab = new ushort[numNodesToCheck];

        var p = InterpParams.Compute(TestState, numNodesToCheck, 1, 1, tab, LerpFlag.Ushort);
        Assert.That(p, Is.Not.Null);
        Assert.That(p.Interpolation, Is.Not.Null);

        BuildTable(numNodesToCheck, ref tab, down);

        for (var i = 0; i <= 0xFFFF; i++)
        {
            var @in = new ushort[] { (ushort)i };
            var @out = new ushort[1];

            p.Interpolation.Lerp(@in, @out, p);

            if (down) @out[0] = (ushort)(0xFFFF - @out[0]);

            Assert.That(@out[0], Is.EqualTo(@in[0]).Within(maxErr));
        }
    }

    [Test, Category(FixedTest)]
    public void Interp3DTetrahedral16Test()
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

        var p = InterpParams.Compute(TestState, 2, 3, 3, table, LerpFlag.Ushort);
        Assert.That(p, Is.Not.Null);
        Assert.That(p.Interpolation, Is.Not.Null);

        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new ushort[] { (ushort)i, (ushort)i, (ushort)i };
            var @out = new ushort[3];

            p.Interpolation.Lerp(@in, @out, p);

            Assert.Multiple(() =>
            {
                IsGoodWord($"{i}: Channel 1", @out[0], @in[0]);
                IsGoodWord($"{i}: Channel 2", @out[1], @in[1]);
                IsGoodWord($"{i}: Channel 3", @out[2], @in[2]);
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Interp3DTetrahedralFloatTest()
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

        var p = InterpParams.Compute(TestState, 2, 3, 3, floatTable, LerpFlag.Float);
        Assert.That(p, Is.Not.Null);
        Assert.That(p.Interpolation, Is.Not.Null);

        var MaxErr = 0.0;
        var mutex = new object();

        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new float[] { i / 65535f, i / 65535f, i / 65535f };
            var @out = new float[3];

            p.Interpolation.Lerp(@in, @out, p);

            Assert.Multiple(() =>
            {
                IsGoodFixed15_16($"{i}: Channel 1", @out[0], @in[0], mutex, ref MaxErr);
                IsGoodFixed15_16($"{i}: Channel 2", @out[1], @in[1] / 2f, mutex, ref MaxErr);
                IsGoodFixed15_16($"{i}: Channel 3", @out[2], @in[2] / 4f, mutex, ref MaxErr);
            });
        }

        if (MaxErr > 0)
            Console.WriteLine($"|Err|<{MaxErr}");
    }

    [Test, Category(FixedTest)]
    public void Interp3DTrilinear16Test()
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

        var p = InterpParams.Compute(TestState, 2, 3, 3, table, LerpFlag.Ushort | LerpFlag.Trilinear);
        Assert.That(p, Is.Not.Null);
        Assert.That(p.Interpolation, Is.Not.Null);

        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new ushort[] { (ushort)i, (ushort)i, (ushort)i };
            var @out = new ushort[3];

            p.Interpolation.Lerp(@in, @out, p);

            Assert.Multiple(() =>
            {
                IsGoodWord($"{i}: Channel 1", @out[0], @in[0]);
                IsGoodWord($"{i}: Channel 2", @out[1], @in[1]);
                IsGoodWord($"{i}: Channel 3", @out[2], @in[2]);
            });
        }
    }

    [Test, Category(FixedTest)]
    public void Interp3DTrilinearFloatTest()
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

        var p = InterpParams.Compute(TestState, 2, 3, 3, floatTable, LerpFlag.Float | LerpFlag.Trilinear);
        Assert.That(p, Is.Not.Null);
        Assert.That(p.Interpolation, Is.Not.Null);

        var MaxErr = 0.0;
        var mutex = new object();

        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = new float[] { i / 65535f, i / 65535f, i / 65535f };
            var @out = new float[3];

            p.Interpolation.Lerp(@in, @out, p);

            Assert.Multiple(() =>
            {
                IsGoodFixed15_16($"{i}: Channel 1", @out[0], @in[0], mutex, ref MaxErr);
                IsGoodFixed15_16($"{i}: Channel 2", @out[1], @in[1] / 2f, mutex, ref MaxErr);
                IsGoodFixed15_16($"{i}: Channel 3", @out[2], @in[2] / 4f, mutex, ref MaxErr);
            });
        }

        if (MaxErr > 0)
            Console.WriteLine($"|Err|<{MaxErr}");
    }

    [Test, Category(FixedTest)]
    public void XDInterpTest([Range(3u, 4u)] uint inputChans)
    {
        var lut = Pipeline.Alloc(null, inputChans, 3);
        var mpe = Stage.AllocCLut16bit(null, 9, inputChans, 3, null);

        Assert.Multiple(() =>
        {
            Assert.That(lut, Is.Not.Null);
            Assert.That(mpe, Is.Not.Null);
        });

        mpe!.Sample(SamplerXD, null, SamplerFlags.None);
        lut!.InsertStage(StageLoc.AtBegin, mpe);

        // Check accuracy
        Assert.Multiple(() =>
        {
            foreach (var test in _checkXDData)
                CheckOneXD(lut, test[..(int)inputChans]);
        });
    }

    #endregion Public Methods

    #region Private Methods

    private static void BuildTable(uint n, ref ushort[] tab, bool descending)
    {
        for (var i = 0; i < n; i++)
        {
            var v = 65535.0 * i / (n - 1);

            tab[descending ? n - i - 1 : i] = (ushort)Math.Floor(v + 0.5);
        }
    }

    private static void CheckOneXD(Pipeline lut, params ushort[] a)
    {
        var out1 = new ushort[3];
        var out2 = new ushort[3];

        // This is the interpolated value
        lut.Eval(a, out1);

        // This is the real value
        SamplerXD(a.Concat(Enumerable.Repeat<ushort>(0, 5)).ToArray(), out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            IsGoodWord($"({a}): Channel 1", out1[0], out2[0], 2);
            IsGoodWord($"({a}): Channel 2", out1[0], out2[0], 2);
            IsGoodWord($"({a}): Channel 3", out1[0], out2[0], 2);
        });
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
        @out![2] = Fn8D3(@in[..8]);

        return true;
    }

    #endregion Private Methods
}
