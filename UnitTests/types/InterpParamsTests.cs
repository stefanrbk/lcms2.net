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
using lcms2.types;

namespace lcms2.tests.types;

public class InterpParamsTests
{
    [TestCase(2, false, 0, TestName = "1D interpolation in 2pt tables")]
    [TestCase(3, false, 1, TestName = "1D interpolation in 3pt tables")]
    [TestCase(4, false, 0, TestName = "1D interpolation in 4pt tables")]
    [TestCase(6, false, 0, TestName = "1D interpolation in 6pt tables")]
    [TestCase(18, false, 0, TestName = "1D interpolation in 18pt tables")]
    [TestCase(2, true, 0, TestName = "1D interpolation in descending 2pt tables")]
    [TestCase(3, true, 1, TestName = "1D interpolation in descending 3pt tables")]
    [TestCase(4, true, 0, TestName = "1D interpolation in descending 4pt tables")]
    [TestCase(6, true, 0, TestName = "1D interpolation in descending 6pt tables")]
    [TestCase(18, true, 0, TestName = "1D interpolation in descending 18pt tables")]
    public void Test1DLerp(int nodes, bool down, int maxErr)
    {
        Span<ushort> @in = stackalloc ushort[1];
        Span<ushort> @out = stackalloc ushort[1];

        var tab = new ushort[nodes];

        var p = _cmsComputeInterpParams(null, (uint)nodes, 1, 1, tab, LerpFlag.Ushort);
        Assert.That(p?.Interpolation?.Lerp16, Is.Not.Null);

        BuildTable(nodes, tab, down);

        for (var i = 0; i <= 0xffff; i++)
        {
            @in[0] = (ushort)i;
            @out[0] = 0;

            p.Interpolation.Lerp16(@in, @out, p);

            if (down)
                @out[0] = (ushort)(0xFFFF - @out[0]);

            Assert.That(@in[0], Is.EqualTo(@out[0]).Within(maxErr));
        }

        _cmsFreeInterpParams(p);
    }

    [TestCase(TestName = "1D interpolation in n tables"), Explicit]
    public void Test1DLerpExhaustive()
    {
        Assert.Multiple(() =>
        {
            for (var j = 10; j <= 4096; j++)
                Test1DLerp(j, false, 1);
        });
    }

    [TestCase(TestName = "1D interpolation in descending tables"), Explicit]
    public void Test1DLerpDownExhaustive()
    {
        Assert.Multiple(() =>
        {
            for (var j = 10; j <= 4096; j++)
                Test1DLerp(j, true, 1);
        });
    }

    [TestCase(TestName = "3D interpolation Tetrahedral (float)")]
    public void Test3DInterpFloatTetrahedral()
    {
        var @in = new float[3];
        var @out = new float[3];
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

        var p = _cmsComputeInterpParams(null, 2, 3, 3, floatTable, LerpFlag.Float);
        Assert.That(p?.Interpolation?.LerpFloat, Is.Not.Null);

        Assert.Multiple(() =>
        {
            for (var i = 0; i < 0xFFFF; i++)
            {
                @in[0] = @in[1] = @in[2] = i / 65535f;
                Array.Clear(@out);

                p.Interpolation.LerpFloat(@in, @out, p);

                CheckFixed15_16("Channel 1", @out[0], @in[0]);
                CheckFixed15_16("Channel 2", @out[1], @in[1] / 2);
                CheckFixed15_16("Channel 3", @out[2], @in[2] / 4);
            }
        });
    }

    [TestCase(TestName = "3D interpolation Trilinear (float)")]
    public void Test3DInterpFloatTrilinear()
    {
        var @in = new float[3];
        var @out = new float[3];
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

        var p = _cmsComputeInterpParams(null, 2, 3, 3, floatTable, LerpFlag.Float | LerpFlag.Trilinear);
        Assert.That(p?.Interpolation?.LerpFloat, Is.Not.Null);

        Assert.Multiple(() =>
        {
            for (var r = 0; r < 0xff; r++)
                for (var g = 0; g < 0xff; g++)
                    for (var b = 0; b < 0xff; b++)
                    {
                        @in[0] = r / 255f; 
                        @in[1] = g / 255f; 
                        @in[2] = b / 255f;

                        Array.Clear(@out);

                        p.Interpolation.LerpFloat(@in, @out, p);

                        CheckFixed15_16("Channel 1", @out[0], @in[0]);
                        CheckFixed15_16("Channel 2", @out[1], @in[1] / 2);
                        CheckFixed15_16("Channel 3", @out[2], @in[2] / 4);
                    }
        });
    }

    [TestCase(TestName = "3D interpolation Tetrahedral (16)")]
    public void Test3DInterp16Tetrahedral()
    {
        var @in = new ushort[3];
        var @out = new ushort[3];
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

        var p = _cmsComputeInterpParams(null, 2, 3, 3, table, LerpFlag.Ushort);
        Assert.That(p?.Interpolation?.Lerp16, Is.Not.Null);

        Assert.Multiple(() =>
        {
            for (var i = 0; i < 0xFFFF; i++)
            {
                @in[0] = @in[1] = @in[2] = (ushort)i;
                Array.Clear(@out);

                p.Interpolation.Lerp16(@in, @out, p);

                CheckWord("Channel 1", @out[0], @in[0]);
                CheckWord("Channel 2", @out[1], @in[1]);
                CheckWord("Channel 3", @out[2], @in[2]);
            }
        });
    }

    [TestCase(TestName = "3D interpolation Trilinear (16)")]
    public void Test3DInterp16Trilinear()
    {
        var @in = new ushort[3];
        var @out = new ushort[3];
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

        var p = _cmsComputeInterpParams(null, 2, 3, 3, table, LerpFlag.Ushort | LerpFlag.Trilinear);
        Assert.That(p?.Interpolation?.Lerp16, Is.Not.Null);

        Assert.Multiple(() =>
        {
            for (var i = 0; i < 0xFFFF; i++)
            {
                @in[0] = @in[1] = @in[2] = (ushort)i;
                Array.Clear(@out);

                p.Interpolation.Lerp16(@in, @out, p);

                CheckWord("Channel 1", @out[0], @in[0]);
                CheckWord("Channel 2", @out[1], @in[1]);
                CheckWord("Channel 3", @out[2], @in[2]);
            }
        });
    }

    [TestCase(TestName = "Reverse interpolation 3 -> 3")]
    public void TestReverseInterpolation3x3()
    {
        var target = new float[4];
        var result = new float[4];
        var hint = new float[4];

        Span<ushort> table = stackalloc ushort[]
        {
            0,
            0,
            0,                // B=0, G=0, R=0
            0,
            0,
            0xFFFF,           // B=1, G=0, R=0

            0,
            0xFFFF,
            0,           // B=0, G=1, R=0
            0,
            0xFFFF,
            0xFFFF,      // B=1, G=1, R=0

            0xFFFF,
            0,
            0,           // B=0, G=0, R=1
            0xFFFF,
            0,
            0xFFFF,      // B=1, G=0, R=1

            0xFFFF,
            0xFFFF,
            0,      // B=0, G=1, R=1
            0xFFFF,
            0xFFFF,
            0xFFFF, // B=1, G=1, R=1
        };

        var lut = cmsPipelineAlloc(null, 3, 3)!;

        var clut = cmsStageAllocCLut16bit(null, 2, 3, 3, table);
        cmsPipelineInsertStage(lut, StageLoc.AtBegin, clut);

        target[0] = target[1] = target[2] = 0;
        hint[0] = hint[1] = hint[2] = 0;
        cmsPipelineEvalReverseFloat(target, result, null, lut);

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(0));
            Assert.That(result[1], Is.EqualTo(0));
            Assert.That(result[2], Is.EqualTo(0));
        });

        // Transverse identity
        var max = 0f;
        for (var i = 0; i <= 100; i++)
        {
            var @in = i / 100f;

            target[0] = @in; target[1] = 0; target[2] = 0;
            cmsPipelineEvalReverseFloat(target, result, hint, lut);

            var err = MathF.Abs(@in - result[0]);
            if (err > max) max = err;

            result.CopyTo(hint.AsSpan());
        }

        cmsPipelineFree(lut);

        Assert.That(max, Is.LessThanOrEqualTo(1e-5));
    }

    [TestCase(TestName = "Reverse interpolation 4 -> 3")]
    public void TestReverseInterpolation4x3()
    {
        var target = new float[4];
        var result = new float[4];
        var hint = new float[4];

        Span<ushort> table = stackalloc ushort[]
        {
            0,
            0,
            0,                // 0 0 0 0 = ( 0, 0, 0)
            0,
            0,
            0,                // 0 0 0 1 = ( 0, 0, 0)

            0,
            0,
            0xFFFF,           // 0 0 1 0 = ( 0, 0, 1)
            0,
            0,
            0xFFFF,           // 0 0 1 1 = ( 0, 0, 1)

            0,
            0xFFFF,
            0,           // 0 1 0 0 = ( 0, 1, 0)
            0,
            0xFFFF,
            0,           // 0 1 0 1 = ( 0, 1, 0)

            0,
            0xFFFF,
            0xFFFF,      // 0 1 1 0 = ( 0, 1, 1)
            0,
            0xFFFF,
            0xFFFF,      // 0 1 1 1 = ( 0, 1, 1)

            0xFFFF,
            0,
            0,           // 1 0 0 0 = ( 1, 0, 0)
            0xFFFF,
            0,
            0,           // 1 0 0 1 = ( 1, 0, 0)

            0xFFFF,
            0,
            0xFFFF,      // 1 0 1 0 = ( 1, 0, 1)
            0xFFFF,
            0,
            0xFFFF,      // 1 0 1 1 = ( 1, 0, 1)

            0xFFFF,
            0xFFFF,
            0,      // 1 1 0 0 = ( 1, 1, 0)
            0xFFFF,
            0xFFFF,
            0,      // 1 1 0 1 = ( 1, 1, 0)

            0xFFFF,
            0xFFFF,
            0xFFFF, // 1 1 1 0 = ( 1, 1, 1)
            0xFFFF,
            0xFFFF,
            0xFFFF, // 1 1 1 1 = ( 1, 1, 1)
        };

        var lut = cmsPipelineAlloc(null, 4, 3)!;

        var clut = cmsStageAllocCLut16bit(null, 2, 4, 3, table);
        cmsPipelineInsertStage(lut, StageLoc.AtBegin, clut);

        // Check if the LUT is behaving as expected
        for (var i = 0; i <= 100; i++)
        {
            target[0] = i / 100f;
            target[1] = target[0];
            target[2] = 0;
            target[3] = 12;

            cmsPipelineEvalFloat(target, result, lut);

            Assert.Multiple(() =>
            {
                CheckFixed15_16("4->3 feasibility 0", target[0], result[0]);
                CheckFixed15_16("4->3 feasibility 1", target[1], result[1]);
                CheckFixed15_16("4->3 feasibility 2", target[2], result[2]);
            });
        }

        target[0] = target[1] = target[2] = 0;

        // This one holds the fixed k
        target[3] = 0;

        // This is our hint (which is a big lie in this case)
        hint[0] = hint[1] = hint[2] = 0.1f;

        cmsPipelineEvalReverseFloat(target, result, hint, lut);

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(0));
            Assert.That(result[1], Is.EqualTo(0));
            Assert.That(result[2], Is.EqualTo(0));
        });

        var max = 0f;
        for (var i = 0; i <= 100; i++)
        {
            var @in = i / 100f;

            target[0] = @in; target[1] = 0; target[2] = 0;
            cmsPipelineEvalReverseFloat(target, result, hint, lut);

            var err = MathF.Abs(@in - result[0]);
            if (err > max) max = err;

            result.CopyTo(hint.AsSpan());
        }

        cmsPipelineFree(lut);

        Assert.That(max, Is.LessThanOrEqualTo(1e-5));
    }

    [TestCase(0, 0, 0, TestName = "3D interpolation (0, 0, 0)")]
    [TestCase(0xffff, 0xffff, 0xffff, TestName = "3D interpolation (0xffff, 0xffff, 0xffff)")]
    [TestCase(0x8080, 0x8080, 0x8080, TestName = "3D interpolation (0x8080, 0x8080, 0x8080)")]
    [TestCase(0x0000, 0xfe00, 0x80ff, TestName = "3D interpolation (0x0000, 0xfe00, 0x80ff)")]
    [TestCase(0x1111, 0x2222, 0x3333, TestName = "3D interpolation (0x1111, 0x2222, 0x3333)")]
    [TestCase(0x0000, 0x0012, 0x0013, TestName = "3D interpolation (0x0000, 0x0012, 0x0013)")]
    [TestCase(0x3141, 0x1415, 0x1592, TestName = "3D interpolation (0x3141, 0x1415, 0x1592)")]
    [TestCase(0xff00, 0xff01, 0xff12, TestName = "3D interpolation (0xff00, 0xff01, 0xff12)")]
    public void Test3DLerp(int a1, int a2, int a3)
    {
        var lut = cmsPipelineAlloc(null, 3, 3)!;
        var mpe = cmsStageAllocCLut16bit(null, 9, 3, 3, null);
        cmsStageSampleCLut16bit(mpe, Sampler3D, null, SamplerFlag.None);
        cmsPipelineInsertStage(lut, StageLoc.AtBegin, mpe);

        var In = new ushort[3];
        var Out1 = new ushort[3];
        var Out2 = new ushort[3];

        In[0] = (ushort)a1; In[1] = (ushort)a2; In[2] = (ushort)a3;

        // This is the interpolated value
        cmsPipelineEval16(In, Out1, lut);

        // This is the real value
        Sampler3D(In, Out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            CheckWord("Channel 1", Out1[0], Out2[0], 2);
            CheckWord("Channel 2", Out1[1], Out2[1], 2);
            CheckWord("Channel 3", Out1[2], Out2[2], 2);
        });

        cmsPipelineFree(lut);
    }

    [TestCase(0, 0, 0, TestName = "3D interpolation with granularity (0, 0, 0)")]
    [TestCase(0xffff, 0xffff, 0xffff, TestName = "3D interpolation with granularity (0xffff, 0xffff, 0xffff)")]
    [TestCase(0x8080, 0x8080, 0x8080, TestName = "3D interpolation with granularity (0x8080, 0x8080, 0x8080)")]
    [TestCase(0x0000, 0xfe00, 0x80ff, TestName = "3D interpolation with granularity (0x0000, 0xfe00, 0x80ff)")]
    [TestCase(0x1111, 0x2222, 0x3333, TestName = "3D interpolation with granularity (0x1111, 0x2222, 0x3333)")]
    [TestCase(0x0000, 0x0012, 0x0013, TestName = "3D interpolation with granularity (0x0000, 0x0012, 0x0013)")]
    [TestCase(0x3141, 0x1415, 0x1592, TestName = "3D interpolation with granularity (0x3141, 0x1415, 0x1592)")]
    [TestCase(0xff00, 0xff01, 0xff12, TestName = "3D interpolation with granularity (0xff00, 0xff01, 0xff12)")]
    public void Test3DLerpGranular(int a1, int a2, int a3)
    {
        Span<uint> Dimensions = stackalloc uint[3] { 7, 8, 9 };

        var lut = cmsPipelineAlloc(null, 3, 3)!;
        var mpe = cmsStageAllocCLut16bitGranular(null, Dimensions, 3, 3, null);
        cmsStageSampleCLut16bit(mpe, Sampler3D, null, SamplerFlag.None);
        cmsPipelineInsertStage(lut, StageLoc.AtBegin, mpe);

        var In = new ushort[3];
        var Out1 = new ushort[3];
        var Out2 = new ushort[3];

        In[0] = (ushort)a1; In[1] = (ushort)a2; In[2] = (ushort)a3;

        // This is the interpolated value
        cmsPipelineEval16(In, Out1, lut);

        // This is the real value
        Sampler3D(In, Out2, null);

        // Let's see the difference

        Assert.Multiple(() =>
        {
            CheckWord("Channel 1", Out1[0], Out2[0], 2);
            CheckWord("Channel 2", Out1[1], Out2[1], 2);
            CheckWord("Channel 3", Out1[2], Out2[2], 2);
        });

        cmsPipelineFree(lut);
    }

    private static void BuildTable(int n, Span<ushort> tab, bool descending)
    {
        for (var i = 0; i < n; i++)
        {
            var v = 65535.0 * i / (n - 1);

            tab[descending ? n - i - 1 : i] = (ushort)Math.Floor(v + 0.5);
        }
    }

    private static ushort Fn8D1(uint m, ushort a1, ushort a2, ushort a3, ushort a4 = 0, ushort a5 = 0, ushort a6 = 0, ushort a7 = 0, ushort a8 = 0) =>
        (ushort)((a1 + a2 + a3 + a4 + a5 + a6 + a7 + a8) / m);

    private static ushort Fn8D2(uint m, ushort a1, ushort a2, ushort a3, ushort a4 = 0, ushort a5 = 0, ushort a6 = 0, ushort a7 = 0, ushort a8 = 0) =>
        (ushort)((a1 + (3 * a2) + (3 * a3) + a4 + a5 + a6 + a7 + a8) / (m + 4));

    private static ushort Fn8D3(uint m, ushort a1, ushort a2, ushort a3, ushort a4 = 0, ushort a5 = 0, ushort a6 = 0, ushort a7 = 0, ushort a8 = 0) =>
        (ushort)(((3 * a1) + (2 * a2) + (3 * a3) + a4 + a5 + a6 + a7 + a8) / (m + 5));

    private static bool Sampler3D(ReadOnlySpan<ushort> In, Span<ushort> Out, object? _)
    {
        Out[0] = Fn8D1(3, In[0], In[1], In[2]);
        Out[1] = Fn8D2(3, In[0], In[1], In[2]);
        Out[2] = Fn8D3(3, In[0], In[1], In[2]);

        return true;
    }

    private static bool Sampler4D(ReadOnlySpan<ushort> In, Span<ushort> Out, object? _)
    {
        Out[0] = Fn8D1(4, In[0], In[1], In[2], In[3]);
        Out[1] = Fn8D2(4, In[0], In[1], In[2], In[3]);
        Out[2] = Fn8D3(4, In[0], In[1], In[2], In[3]);

        return true;
    }

    private static bool Sampler5D(ReadOnlySpan<ushort> In, Span<ushort> Out, object? _)
    {
        Out[0] = Fn8D1(5, In[0], In[1], In[2], In[3], In[4]);
        Out[1] = Fn8D2(5, In[0], In[1], In[2], In[3], In[4]);
        Out[2] = Fn8D3(5, In[0], In[1], In[2], In[3], In[4]);
        //Con.WriteLine("\n");
        //Con.WriteLine($"In = \t{In[0]:x}\t{In[1]:x}\t{In[2]:x}\t{In[3]:x}\t{In[4]:x}");
        //Con.WriteLine($"Out = \t{Out[0]:x}\t{Out[1]:x}\t{Out[2]:x}");

        return true;
    }

    private static bool Sampler6D(ReadOnlySpan<ushort> In, Span<ushort> Out, object? _)
    {
        Out[0] = Fn8D1(6, In[0], In[1], In[2], In[3], In[4], In[5]);
        Out[1] = Fn8D2(6, In[0], In[1], In[2], In[3], In[4], In[5]);
        Out[2] = Fn8D3(6, In[0], In[1], In[2], In[3], In[4], In[5]);

        return true;
    }

    private static bool Sampler7D(ReadOnlySpan<ushort> In, Span<ushort> Out, object? _)
    {
        Out[0] = Fn8D1(7, In[0], In[1], In[2], In[3], In[4], In[5], In[6]);
        Out[1] = Fn8D2(7, In[0], In[1], In[2], In[3], In[4], In[5], In[6]);
        Out[2] = Fn8D3(7, In[0], In[1], In[2], In[3], In[4], In[5], In[6]);

        return true;
    }

    private static bool Sampler8D(ReadOnlySpan<ushort> In, Span<ushort> Out, object? _)
    {
        Out[0] = Fn8D1(8, In[0], In[1], In[2], In[3], In[4], In[5], In[6], In[7]);
        Out[1] = Fn8D2(8, In[0], In[1], In[2], In[3], In[4], In[5], In[6], In[7]);
        Out[2] = Fn8D3(8, In[0], In[1], In[2], In[3], In[4], In[5], In[6], In[7]);

        return true;
    }


    //    #region Fields

    //    private static readonly ushort[][] _checkXDData =
    //    {
    //        new ushort[] { 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000 },
    //        new ushort[] { 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF },
    //        new ushort[] { 0x8080, 0x8080, 0x8080, 0x8080, 0x1234, 0x1122, 0x0056, 0x0011 },
    //        new ushort[] { 0x0000, 0xFE00, 0x80FF, 0x8888, 0x8878, 0x2233, 0x0088, 0x2020 },
    //        new ushort[] { 0x1111, 0x2222, 0x3333, 0x4444, 0x1455, 0x3344, 0x1987, 0x4532 },
    //        new ushort[] { 0x0000, 0x0012, 0x0013, 0x0014, 0x2333, 0x4455, 0x9988, 0x1200 },
    //        new ushort[] { 0x3141, 0x1415, 0x1592, 0x9261, 0x4567, 0x5566, 0xFE56, 0x6666 },
    //        new ushort[] { 0xFF00, 0xFF01, 0xFF12, 0xFF13, 0xF344, 0x6677, 0xBABE, 0xFACE },
    //    };

    //    private static readonly uint[][] _checkXDDims =
    //    {
    //        new uint[] { 7, 8, 9 },
    //        new uint[] { 9, 8, 7, 6 },
    //        new uint[] { 3, 2, 2, 2, 2 },
    //        new uint[] { 4, 3, 3, 2, 2, 2 },
    //        new uint[] { 4, 3, 3, 2, 2, 2, 2 },
    //        new uint[] { 4, 3, 3, 2, 2, 2, 2, 2 }
    //    };

    //    private static readonly object[][] _interp1DValues = new[]
    //    {
    //        new object[] { 2u, false, 0 },
    //        new object[] { 3u, false, 1 },
    //        new object[] { 4u, false, 0 },
    //        new object[] { 6u, false, 0 },
    //        new object[] { 18u, false, 0 },
    //        new object[] { 2u, true, 0 },
    //        new object[] { 3u, true, 1 },
    //        new object[] { 4u, true, 0 },
    //        new object[] { 6u, true, 0 },
    //        new object[] { 18u, true, 0 },
    //    };

    //    #endregion Fields

    //    #region Public Methods

    //    [Test, Category(FixedTest)]
    //    public void CheckReverseInterpolation3x3()
    //    {
    //        var target = new float[4];
    //        var result = new float[4];
    //        var hint = new float[4];

    //        var table = new ushort[]
    //        {
    //            0, 0, 0,                // B=0, G=0, R=0
    //            0, 0, 0xFFFF,           // B=1, G=0, R=0

    //            0, 0xFFFF, 0,           // B=0, G=1, R=0
    //            0, 0xFFFF, 0xFFFF,      // B=1, G=1, R=0

    //            0xFFFF, 0, 0,           // B=0, G=0, R=1
    //            0xFFFF, 0, 0xFFFF,      // B=1, G=0, R=1

    //            0xFFFF, 0xFFFF, 0,      // B=0, G=1, R=1
    //            0xFFFF, 0xFFFF, 0xFFFF, // B=1, G=1, R=1
    //        };

    //        var lut = Pipeline.Alloc(null, 3, 3);
    //        Assert.That(lut, Is.Not.Null);

    //        var clut = old.types.Stage.AllocCLut16bit(null, 2, 3, 3, table);
    //        lut.InsertStage(StageLoc.AtBegin, clut);

    //        lut.EvalReverse(target, result, null);
    //        if (!result.Take(3).Contains(0))
    //            Assert.Fail("Reverse interpolation didn't find zero");

    //        // Transverse identity
    //        var max = 0f;
    //        for (var i = 0; i <= 100; i++)
    //        {
    //            var @in = i / 100f;

    //            target[0] = @in; target[1] = 0; target[2] = 0;
    //            lut.EvalReverse(target, result, hint);

    //            var err = MathF.Abs(@in - result[0]);
    //            if (err > max) max = err;

    //            result.CopyTo(hint, 0);
    //        }

    //        lut.Dispose();
    //        Assert.That(max, Is.LessThanOrEqualTo(FloatPrecision));
    //    }

    //    [Test, Category(FixedTest)]
    //    public void CheckReverseInterpolation4x3()
    //    {
    //        var target = new float[4];
    //        var result = new float[4];
    //        var hint = new float[4];

    //        var table = new ushort[]
    //        {
    //            0, 0, 0,                // 0 0 0 0 = ( 0, 0, 0)
    //            0, 0, 0,                // 0 0 0 1 = ( 0, 0, 0)

    //            0, 0, 0xFFFF,           // 0 0 1 0 = ( 0, 0, 1)
    //            0, 0, 0xFFFF,           // 0 0 1 1 = ( 0, 0, 1)

    //            0, 0xFFFF, 0,           // 0 1 0 0 = ( 0, 1, 0)
    //            0, 0xFFFF, 0,           // 0 1 0 1 = ( 0, 1, 0)

    //            0, 0xFFFF, 0xFFFF,      // 0 1 1 0 = ( 0, 1, 1)
    //            0, 0xFFFF, 0xFFFF,      // 0 1 1 1 = ( 0, 1, 1)

    //            0xFFFF, 0, 0,           // 1 0 0 0 = ( 1, 0, 0)
    //            0xFFFF, 0, 0,           // 1 0 0 1 = ( 1, 0, 0)

    //            0xFFFF, 0, 0xFFFF,      // 1 0 1 0 = ( 1, 0, 1)
    //            0xFFFF, 0, 0xFFFF,      // 1 0 1 1 = ( 1, 0, 1)

    //            0xFFFF, 0xFFFF, 0,      // 1 1 0 0 = ( 1, 1, 0)
    //            0xFFFF, 0xFFFF, 0,      // 1 1 0 1 = ( 1, 1, 0)

    //            0xFFFF, 0xFFFF, 0xFFFF, // 1 1 1 0 = ( 1, 1, 1)
    //            0xFFFF, 0xFFFF, 0xFFFF, // 1 1 1 1 = ( 1, 1, 1)
    //        };

    //        var lut = Pipeline.Alloc(null, 4, 3);
    //        Assert.That(lut, Is.Not.Null);

    //        var clut = old.types.Stage.AllocCLut16bit(null, 2, 4, 3, table);
    //        lut.InsertStage(StageLoc.AtBegin, clut);

    //        // Check if the LUT is behaving as expected
    //        for (var i = 0; i <= 100; i++)
    //        {
    //            target[0] = i / 100f;
    //            target[1] = target[0];
    //            target[2] = 0;
    //            target[3] = 12;

    //            lut.Eval(target, result);

    //            Assert.Multiple(() =>
    //            {
    //                IsGoodFixed15_16($"4->3 feasibility\n({i}): 0", target[0], result[0]);
    //                IsGoodFixed15_16($"4->3 feasibility\n({i}): 1", target[1], result[1]);
    //                IsGoodFixed15_16($"4->3 feasibility\n({i}): 2", target[2], result[2]);
    //            });
    //        }

    //        target[0] = target[1] = target[2] = 0;

    //        // This one holds the fixed k
    //        target[3] = 0;

    //        // This is our hint (which is a big lie in this case)
    //        Enumerable.Repeat(0.1f, 3).ToArray().CopyTo(hint, 0);

    //        lut.EvalReverse(target, result, hint);

    //        if (!result.Contains(0))
    //            Assert.Fail("4->3 zero\nReverse interpolation didn't find zero");

    //        var max = 0f;
    //        for (var i = 0; i <= 100; i++)
    //        {
    //            var @in = i / 100f;

    //            target[0] = @in; target[1] = 0; target[2] = 0;
    //            lut.EvalReverse(target, result, hint);

    //            var err = MathF.Abs(@in - result[0]);
    //            if (err > max) max = err;

    //            result.CopyTo(hint, 0);
    //        }

    //        lut.Dispose();
    //        Assert.That(max, Is.LessThanOrEqualTo(FloatPrecision), "4->3 find CMY");
    //    }

    //    [Test, Category(FixedTest)]
    //    public void CheckXDInterpGranular([Range(3u, 8u)] uint inputChans)
    //    {
    //        var dims = _checkXDDims[inputChans - 3][..(int)inputChans];

    //        var lut = Pipeline.Alloc(null, inputChans, 3);
    //        var mpe = old.types.Stage.AllocCLut16bit(null, dims, inputChans, 3, null);

    //        Assert.Multiple(() =>
    //        {
    //            Assert.That(lut, Is.Not.Null);
    //            Assert.That(mpe, Is.Not.Null);
    //        });

    //        mpe!.Sample(SamplerXD, null, SamplerFlags.None);
    //        lut!.InsertStage(StageLoc.AtBegin, mpe);

    //        // Check accuracy
    //        Assert.Multiple(() =>
    //        {
    //            foreach (var test in _checkXDData)
    //                CheckOneXD(lut, test[..(int)inputChans]);
    //        });
    //    }

    //    [TestCaseSource(nameof(_interp1DValues), Category = FixedTest)]
    //    public void Interp1DTest(uint numNodesToCheck, bool down, int maxErr)
    //    {
    //        var tab = new ushort[numNodesToCheck];

    //        var p = InterpParams.Compute(TestState, numNodesToCheck, 1, 1, tab, LerpFlag.Ushort);
    //        Assert.That(p, Is.Not.Null);
    //        Assert.That(p.Interpolation, Is.Not.Null);

    //        BuildTable(numNodesToCheck, ref tab, down);

    //        for (var i = 0; i <= 0xFFFF; i++)
    //        {
    //            var @in = new ushort[] { (ushort)i };
    //            var @out = new ushort[1];

    //            p.Interpolation.Lerp(@in, @out, p);

    //            if (down) @out[0] = (ushort)(0xFFFF - @out[0]);

    //            Assert.That(@out[0], Is.EqualTo(@in[0]).Within(maxErr));
    //        }
    //    }

    //    [Test, Category(FixedTest)]
    //    public void Interp3DTrilinear16Test()
    //    {
    //        var table = new ushort[]
    //        {
    //            0, 0, 0,                // B=0, G=0, R=0
    //            0, 0, 0xFFFF,           // B=1, G=0, R=0

    //            0, 0xFFFF, 0,           // B=0, G=1, R=0
    //            0, 0xFFFF, 0xFFFF,      // B=1, G=1, R=0

    //            0xFFFF, 0, 0,           // B=0, G=0, R=1
    //            0xFFFF, 0, 0xFFFF,      // B=1, G=0, R=1

    //            0xFFFF, 0xFFFF, 0,      // B=0, G=1, R=1
    //            0xFFFF, 0xFFFF, 0xFFFF, // B=1, G=1, R=1
    //        };

    //        var p = InterpParams.Compute(TestState, 2, 3, 3, table, LerpFlag.Ushort | LerpFlag.Trilinear);
    //        Assert.That(p, Is.Not.Null);
    //        Assert.That(p.Interpolation, Is.Not.Null);

    //        for (var i = 0; i < 0xFFFF; i++)
    //        {
    //            var @in = new ushort[] { (ushort)i, (ushort)i, (ushort)i };
    //            var @out = new ushort[3];

    //            p.Interpolation.Lerp(@in, @out, p);

    //            Assert.Multiple(() =>
    //            {
    //                IsGoodWord($"{i}: Channel 1", @out[0], @in[0]);
    //                IsGoodWord($"{i}: Channel 2", @out[1], @in[1]);
    //                IsGoodWord($"{i}: Channel 3", @out[2], @in[2]);
    //            });
    //        }
    //    }

    //    [Test, Category(FixedTest)]
    //    public void Interp3DTrilinearFloatTest()
    //    {
    //        var floatTable = new float[]
    //        {
    //            0, 0, 0,            // B=0, G=0, R=0
    //            0, 0, .25f,         // B=1, G=0, R=0

    //            0, .5f, 0,          // B=0, G=1, R=0
    //            0, .5f, .25f,       // B=1, G=1, R=0

    //            1, 0, 0,            // B=0, G=0, R=1
    //            1, 0, .25f,         // B=1, G=0, R=1

    //            1, .5f, 0,          // B=0, G=1, R=1
    //            1, .5f, .25f,       // B=1, G=1, R=1
    //        };

    //        var p = InterpParams.Compute(TestState, 2, 3, 3, floatTable, LerpFlag.Float | LerpFlag.Trilinear);
    //        Assert.That(p, Is.Not.Null);
    //        Assert.That(p.Interpolation, Is.Not.Null);

    //        var MaxErr = 0.0;
    //        var mutex = new object();

    //        for (var i = 0; i < 0xFFFF; i++)
    //        {
    //            var @in = new float[] { i / 65535f, i / 65535f, i / 65535f };
    //            var @out = new float[3];

    //            p.Interpolation.Lerp(@in, @out, p);

    //            Assert.Multiple(() =>
    //            {
    //                IsGoodFixed15_16($"{i}: Channel 1", @out[0], @in[0], mutex, ref MaxErr);
    //                IsGoodFixed15_16($"{i}: Channel 2", @out[1], @in[1] / 2f, mutex, ref MaxErr);
    //                IsGoodFixed15_16($"{i}: Channel 3", @out[2], @in[2] / 4f, mutex, ref MaxErr);
    //            });
    //        }

    //        if (MaxErr > 0)
    //            Console.WriteLine($"|Err|<{MaxErr}");
    //    }

    //    [Test, Category(FixedTest)]
    //    public void XDInterpTest([Range(3u, 4u)] uint inputChans)
    //    {
    //        var lut = Pipeline.Alloc(null, inputChans, 3);
    //        var mpe = old.types.Stage.AllocCLut16bit(null, 9, inputChans, 3, null);

    //        Assert.Multiple(() =>
    //        {
    //            Assert.That(lut, Is.Not.Null);
    //            Assert.That(mpe, Is.Not.Null);
    //        });

    //        mpe!.Sample(SamplerXD, null, SamplerFlags.None);
    //        lut!.InsertStage(StageLoc.AtBegin, mpe);

    //        // Check accuracy
    //        Assert.Multiple(() =>
    //        {
    //            foreach (var test in _checkXDData)
    //                CheckOneXD(lut, test[..(int)inputChans]);
    //        });
    //    }

    //    #endregion Public Methods

    //    #region Private Methods

    //    private static void BuildTable(uint n, ref ushort[] tab, bool descending)
    //    {
    //        for (var i = 0; i < n; i++)
    //        {
    //            var v = 65535.0 * i / (n - 1);

    //            tab[descending ? n - i - 1 : i] = (ushort)Math.Floor(v + 0.5);
    //        }
    //    }

    //    private static void CheckOneXD(Pipeline lut, params ushort[] a)
    //    {
    //        var out1 = new ushort[3];
    //        var out2 = new ushort[3];

    //        // This is the interpolated value
    //        lut.Eval(a, out1);

    //        // This is the real value
    //        SamplerXD(a.Concat(Enumerable.Repeat<ushort>(0, 5)).ToArray(), out2, null);

    //        // Let's see the difference

    //        Assert.Multiple(() =>
    //        {
    //            IsGoodWord($"({a}): Channel 1", out1[0], out2[0], 2);
    //            IsGoodWord($"({a}): Channel 2", out1[0], out2[0], 2);
    //            IsGoodWord($"({a}): Channel 3", out1[0], out2[0], 2);
    //        });
    //    }

    //    private static ushort Fn8D1(ReadOnlySpan<ushort> a) =>
    //            (ushort)a.ToArray()
    //                 .Average(i => i);

    //    private static ushort Fn8D2(ReadOnlySpan<ushort> a) =>
    //        (ushort)a.ToArray()
    //                 .Concat(Enumerable.Repeat<ushort>(0, 4))
    //                 .Select((i, j) => j is 2 or 3 ? 3 * i : i)
    //                 .Average(i => i);

    //    private static ushort Fn8D3(ReadOnlySpan<ushort> a) =>
    //        (ushort)a.ToArray()
    //                 .Concat(Enumerable.Repeat<ushort>(0, 5))
    //                 .Select((i, j) => j is 1 or 3 ? 3 * i : j is 2 ? 2 * i : i)
    //                 .Average(i => i);

    //    private static bool SamplerXD(ReadOnlySpan<ushort> @in, Span<ushort> @out, in object? cargo)
    //    {
    //        @out![0] = Fn8D1(@in[..8]);
    //        @out![1] = Fn8D2(@in[..8]);
    //        @out![2] = Fn8D3(@in[..8]);

    //        return true;
    //    }

    //    #endregion Private Methods

}