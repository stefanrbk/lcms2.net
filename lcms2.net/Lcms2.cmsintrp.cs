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
using lcms2.state;
using lcms2.types;

using System.Runtime.CompilerServices;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static readonly InterpPluginChunkType InterpPluginChunk = new();
    private static readonly InterpPluginChunkType* globalInterpPluginChunk;

    internal static void _cmsAllocInterpPluginChunk(Context ctx, in Context src)
    {
        fixed (InterpPluginChunkType* @default = &InterpPluginChunk)
            AllocPluginChunk(ctx, src, Chunks.InterpPlugin, @default);
    }

    internal static bool _cmsRegisterInterpPlugin(Context ctx, PluginBase* Data)
    {
        var Plugin = (PluginInterpolation*)Data;
        var ptr = _cmsContextGetClientChunk<InterpPluginChunkType>(ctx, Chunks.InterpPlugin);

        if (Data is not null)
        {
            // Set replacement functions
            ptr->Interpolators = Plugin->InterpolatorsFactory;
            return true;
        }
        else
        {
            ptr->Interpolators = null;
            return true;
        }
    }

    internal static bool _cmsSetInterpolationRoutine(Context ctx, InterpParams* p)
    {
        var ptr = _cmsContextGetClientChunk<InterpPluginChunkType>(ctx, Chunks.InterpPlugin);

        p->Interpolation.Lerp16 = null;

        // Invoke factory, possibly in the Plug-in
        if (ptr->Interpolators is not null)
            p->Interpolation = ptr->Interpolators(p->nInputs, p->nOutputs, p->dwFlags);

        // If unsupported by the plug-in, go for the LittleCMS default.
        // If happens only if an extern plug-in is being used
        if (p->Interpolation.Lerp16 is null)
            p->Interpolation = DefaultInterpolatorsFactory(p->nInputs, p->nOutputs, (LerpFlag)p->dwFlags);

        // Check for valid interpolator (we just check one member of the union)
        return p->Interpolation.Lerp16 is not null;
    }

    internal static InterpParams* _cmsComputeInterpParamsEx(
        Context ContextID, in uint* nSamples, uint InputChan, uint OutputChan, void* Table, LerpFlag flags)
    {
        var dwFlags = (uint)flags;

        // Check for maximum inputs
        if (InputChan > MAX_INPUT_DIMENSIONS)
        {
            cmsSignalError(ContextID, ErrorCode.Range, $"Too many input channels ({InputChan} channels, max={MAX_INPUT_DIMENSIONS})");
            return null;
        }

        // Creates an empty object
        var p = _cmsMallocZero<InterpParams>(ContextID);
        if (p is null) return null;

        // Keep original parameters
        p->dwFlags = dwFlags;
        p->nInputs = InputChan;
        p->nOutputs = OutputChan;
        p->Table = Table;
        p->ContextID = ContextID;

        // Fill samples per input direction and domain (which is number of nodes minus one)
        for (var i = 0; i < InputChan; i++)
        {
            p->nSamples[i] = nSamples[i];
            p->Domain[i] = nSamples[i] - 1;
        }

        // Compute factors to apply to each component to index the grid array
        p->opta[0] = p->nOutputs;
        for (var i = 1; i < InputChan; i++)
            p->opta[i] = p->opta[i - 1] * nSamples[(int)InputChan - i];

        if (!_cmsSetInterpolationRoutine(ContextID, p))
        {
            cmsSignalError(ContextID, ErrorCode.UnknownExtension, $"Unsupported interpolation ({InputChan}->{OutputChan} channels)");
            _cmsFree(ContextID, p);
            return null;
        }

        // All seems ok
        return p;
    }

    internal static InterpParams* _cmsComputeInterpParams(
        Context ContextID, uint nSamples, uint InputChan, uint OutputChan, void* Table, LerpFlag flags)
    {
        var Samples = stackalloc uint[MAX_INPUT_DIMENSIONS];

        // Fill the auxiliary array
        for (var i = 0; i < MAX_INPUT_DIMENSIONS; i++)
            Samples[i] = nSamples;

        // Call the extended function
        return _cmsComputeInterpParamsEx(ContextID, Samples, InputChan, OutputChan, Table, flags);
    }

    internal static void _cmsFreeInterpParams(InterpParams* p)
    {
        if (p is not null) _cmsFree(p->ContextID, p);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort LinearInterp(int a, int l, int h)
    {
        uint dif = (uint)((h - l) * a) + 0x8000u;
        dif = (dif >> 16) + (uint)l;
        return (ushort)dif;
    }

    private static void LinLerp1D(in ushort* Value, ushort* Output, in InterpParams* p)
    {
        var lutTable = (ushort*)p->Table;

        // if last value or just one point
        if (Value[0] == 0xFFFF || p->Domain[0] == 0)
        {
            Output[0] = lutTable[p->Domain[0]];
        }
        else
        {
            var val3 = (int)(p->Domain[0] * Value[0]);
            val3 = _cmsToFixedDomain(val3);

            var cell0 = FIXED_TO_INT(val3);
            var rest = FIXED_REST_TO_INT(val3);

            var y0 = lutTable[cell0];
            var y1 = lutTable[cell0 + 1];

            Output[0] = LinearInterp(rest, y0, y1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Fclamp(float v) =>
        (v < 1.0e-9f) || Single.IsNaN(v)
            ? 0.0f
            : (v > 1.0f ? 1.0f : v);

    private static void LinLerp1Dfloat(in float* value, float* output, in InterpParams* p)
    {
        var lutTable = (float*)p->Table;

        var val2 = Fclamp(value[0]);

        // if last value...
        if (val2 == 1.0 || p->Domain[0] == 0)
        {
            output[0] = lutTable[p->Domain[0]];
        }
        else
        {
            val2 *= p->Domain[0];

            var cell0 = (int)MathF.Floor(val2);
            var cell1 = (int)MathF.Ceiling(val2);

            // rest is 16 LSB bits
            var rest = val2 - cell0;

            var y0 = lutTable[cell0];
            var y1 = lutTable[cell1];

            output[0] = y0 + ((y1 - y0) * rest);
        }
    }

    private static void Eval1Input(in ushort* input, ushort* output, in InterpParams* p16)
    {
        var lutTable = (ushort*)p16->Table;

        // if last value...
        if (input[0] == 0xFFFF || p16->Domain[0] == 0)
        {
            var y0 = p16->Domain[0] * p16->opta[0];

            for (var outChan = 0; outChan < p16->nOutputs; outChan++)
                output[outChan] = lutTable[y0 + outChan];
        }
        else
        {
            var v = input[0] * (int)p16->Domain[0];
            var fk = _cmsToFixedDomain(v);

            var k0 = FIXED_TO_INT(fk);
            var rk = (ushort)FIXED_REST_TO_INT(fk);

            var k1 = k0 + (input[0] != 0xFFFF ? 1 : 0);

            k0 *= (int)p16->opta[0];
            k1 *= (int)p16->opta[0];

            for (var outChan = 0; outChan < p16->nOutputs; outChan++)
                output[outChan] = LinearInterp(rk, lutTable[k0 + outChan], lutTable[k1 + outChan]);
        }
    }

    private static void Eval1InputFloat(in float* value, float* output, in InterpParams* p)
    {
        var lutTable = (float*)p->Table;

        var val2 = Fclamp(value[0]);

        if (val2 == 1.0 || p->Domain[0] == 0)
        {
            var start = p->Domain[0] * p->opta[0];

            for (var outChan = 0; outChan < p->nOutputs; outChan++)
                output[outChan] = lutTable[start + outChan];
        }
        else
        {
            val2 *= p->Domain[0];

            var cell0 = (int)MathF.Floor(val2);
            var cell1 = (int)MathF.Ceiling(val2);

            var rest = val2 - cell0;

            cell0 *= (int)p->opta[0];
            cell1 *= (int)p->opta[0];

            for (var outChan = 0; outChan < p->nOutputs; outChan++)
            {
                var y0 = lutTable[cell0 + outChan];
                var y1 = lutTable[cell1 + outChan];

                output[outChan] = y0 + ((y1 - y0) * rest);
            }
        }
    }

    private static void BilinearInterpFloat(in float* input, float* output, in InterpParams* p)
    {
        var lutTable = (float*)p->Table;

        var totalOut = p->nOutputs;
        var px = Fclamp(input[0]) * p->Domain[0];
        var py = Fclamp(input[1]) * p->Domain[1];

        var x0 = _cmsQuickFloor(px); var fx = px - x0;
        var y0 = _cmsQuickFloor(py); var fy = py - y0;

        x0 *= (int)p->opta[1];
        var x1 = x0 + (Fclamp(input[0]) >= 1.0 ? 0 : (int)p->opta[1]);

        y0 *= (int)p->opta[0];
        var y1 = y0 + (Fclamp(input[1]) >= 1.0 ? 0 : (int)p->opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var d00 = Dens(lutTable, x0, y0, outChan);
            var d01 = Dens(lutTable, x0, y1, outChan);
            var d10 = Dens(lutTable, x1, y0, outChan);
            var d11 = Dens(lutTable, x1, y1, outChan);

            var dx0 = Lerp(fx, d00, d10);
            var dx1 = Lerp(fx, d01, d11);

            output[outChan] = Lerp(fy, dx0, dx1);
        }
    }

    private static void BilinearInterp16(in ushort* input, ushort* output, in InterpParams* p)
    {
        var lutTable = (ushort*)p->Table;

        var totalOut = p->nOutputs;

        var fx = _cmsToFixedDomain(input[0] * (int)p->Domain[0]);
        var x0 = FIXED_TO_INT(fx);
        var rx = FIXED_REST_TO_INT(fx);

        var fy = _cmsToFixedDomain(input[1] * (int)p->Domain[1]);
        var y0 = FIXED_TO_INT(fy);
        var ry = FIXED_REST_TO_INT(fy);

        x0 *= (int)p->opta[1];
        var x1 = x0 + (input[0] == 0xFFFF ? 0 : (int)p->opta[1]);

        y0 *= (int)p->opta[0];
        var y1 = y0 + (input[1] == 0xFFFF ? 0 : (int)p->opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var d00 = Dens(lutTable, x0, y0, outChan);
            var d01 = Dens(lutTable, x0, y1, outChan);
            var d10 = Dens(lutTable, x1, y0, outChan);
            var d11 = Dens(lutTable, x1, y1, outChan);

            var dx0 = Lerp(rx, d00, d10);
            var dx1 = Lerp(rx, d01, d11);

            output[outChan] = Lerp(ry, dx0, dx1);
        }
    }

    private static void TrilinearInterpFloat(in float* input, float* output, in InterpParams* p)
    {
        var lutTable = (float*)p->Table;

        var totalOut = p->nOutputs;

        var px = Fclamp(input[0]) * p->Domain[0];
        var py = Fclamp(input[1]) * p->Domain[1];
        var pz = Fclamp(input[2]) * p->Domain[2];

        // We need full floor functionality here
        var x0 = (int)MathF.Floor(px); var fx = px - x0;
        var y0 = (int)MathF.Floor(py); var fy = py - y0;
        var z0 = (int)MathF.Floor(pz); var fz = pz - z0;

        x0 *= (int)p->opta[2];
        var x1 = x0 + (Fclamp(input[0]) >= 1.0 ? 0 : (int)p->opta[2]);

        y0 *= (int)p->opta[1];
        var y1 = y0 + (Fclamp(input[1]) >= 1.0 ? 0 : (int)p->opta[1]);

        z0 *= (int)p->opta[0];
        var z1 = z0 + (Fclamp(input[2]) >= 1.0 ? 0 : (int)p->opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var d000 = Dens(lutTable, x0, y0, z0, outChan);
            var d001 = Dens(lutTable, x0, y0, z1, outChan);
            var d010 = Dens(lutTable, x0, y1, z0, outChan);
            var d011 = Dens(lutTable, x0, y1, z1, outChan);

            var d100 = Dens(lutTable, x1, y0, z0, outChan);
            var d101 = Dens(lutTable, x1, y0, z1, outChan);
            var d110 = Dens(lutTable, x1, y1, z0, outChan);
            var d111 = Dens(lutTable, x1, y1, z1, outChan);

            var dx00 = Lerp(fx, d000, d100);
            var dx01 = Lerp(fx, d001, d101);
            var dx10 = Lerp(fx, d010, d110);
            var dx11 = Lerp(fx, d011, d111);

            var dxy0 = Lerp(fy, dx00, dx10);
            var dxy1 = Lerp(fy, dx01, dx11);

            output[outChan] = Lerp(fz, dxy0, dxy1);
        }
    }

    private static void TrilinearInterp16(in ushort* input, ushort* output, in InterpParams* p)
    {
        var lutTable = (ushort*)p->Table;

        var totalOut = p->nOutputs;

        var fx = _cmsToFixedDomain(input[0]) * (int)p->Domain[0];
        var x0 = FIXED_TO_INT(fx);
        var rx = FIXED_REST_TO_INT(fx);

        var fy = _cmsToFixedDomain(input[1]) * (int)p->Domain[1];
        var y0 = FIXED_TO_INT(fy);
        var ry = FIXED_REST_TO_INT(fy);

        var fz = _cmsToFixedDomain(input[2]) * (int)p->Domain[2];
        var z0 = FIXED_TO_INT(fz);
        var rz = FIXED_REST_TO_INT(fz);

        x0 *= (int)p->opta[2];
        var x1 = x0 + (input[0] == 0xFFFF ? 0 : (int)p->opta[2]);

        y0 *= (int)p->opta[1];
        var y1 = y0 + (input[1] == 0xFFFF ? 0 : (int)p->opta[1]);

        z0 *= (int)p->opta[0];
        var z1 = z0 + (input[2] == 0xFFFF ? 0 : (int)p->opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var d000 = Dens(lutTable, x0, y0, z0, outChan);
            var d001 = Dens(lutTable, x0, y0, z1, outChan);
            var d010 = Dens(lutTable, x0, y1, z0, outChan);
            var d011 = Dens(lutTable, x0, y1, z1, outChan);

            var d100 = Dens(lutTable, x1, y0, z0, outChan);
            var d101 = Dens(lutTable, x1, y0, z1, outChan);
            var d110 = Dens(lutTable, x1, y1, z0, outChan);
            var d111 = Dens(lutTable, x1, y1, z1, outChan);

            var dx00 = Lerp(rx, d000, d100);
            var dx01 = Lerp(rx, d001, d101);
            var dx10 = Lerp(rx, d010, d110);
            var dx11 = Lerp(rx, d011, d111);

            var dxy0 = Lerp(ry, dx00, dx10);
            var dxy1 = Lerp(ry, dx01, dx11);

            output[outChan] = Lerp(rz, dxy0, dxy1);
        }
    }

    // Tetrahedral interpolation, using Sakamoto algorithm.
    private static void TetrahedralInterpFloat(in float* input, float* output, in InterpParams* p)
    {
        var lutTable = (float*)p->Table;
        float c1 = 0, c2 = 0, c3 = 0;

        var totalOut = p->nOutputs;

        var px = Fclamp(input[0]) * p->Domain[0];
        var py = Fclamp(input[1]) * p->Domain[1];
        var pz = Fclamp(input[2]) * p->Domain[2];

        // We need full floor functionality here
        var x0 = (int)MathF.Floor(px); var rx = px - x0;
        var y0 = (int)MathF.Floor(py); var ry = py - y0;
        var z0 = (int)MathF.Floor(pz); var rz = pz - z0;

        x0 *= (int)p->opta[2];
        var x1 = x0 + (Fclamp(input[0]) >= 1.0 ? 0 : (int)p->opta[2]);

        y0 *= (int)p->opta[1];
        var y1 = y0 + (Fclamp(input[1]) >= 1.0 ? 0 : (int)p->opta[1]);

        z0 *= (int)p->opta[0];
        var z1 = z0 + (Fclamp(input[2]) >= 1.0 ? 0 : (int)p->opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var c0 = Dens(lutTable, x0, y0, z0, outChan);

            if (rx >= ry && ry >= rz)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x1, y0, z0, outChan);
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            }
            else if (rx >= rz && rz >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x1, y0, z0, outChan);
            }
            else if (rz >= rx && rx >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            }
            else if (ry >= rx && rx >= rz)
            {
                c1 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x0, y1, z0, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            }
            else if (ry >= rz && rz >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y1, z0, outChan);
            }
            else if (rz >= ry && ry >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            }
            else
            {
                c1 = c2 = c3 = 0;
            }

            output[outChan] = c0 + (c1 * rx) + (c2 * ry) + (c3 * rz);
        }
    }

    private static void TetrahedralInterp16(in ushort* input, ushort* output, in InterpParams* p)
    {
        var lutTable = (ushort*)p->Table;
        int rest, c0, c1 = 0, c2 = 0, c3 = 0;

        var totalOut = p->nOutputs;

        var fx = _cmsToFixedDomain(input[0] * (int)p->Domain[0]);
        var fy = _cmsToFixedDomain(input[1] * (int)p->Domain[1]);
        var fz = _cmsToFixedDomain(input[2] * (int)p->Domain[2]);

        // We need full floor functionality here
        var x0 = FIXED_TO_INT(fx);
        var y0 = FIXED_TO_INT(fy);
        var z0 = FIXED_TO_INT(fz);

        var rx = FIXED_REST_TO_INT(fx);
        var ry = FIXED_REST_TO_INT(fy);
        var rz = FIXED_REST_TO_INT(fz);

        x0 *= (int)p->opta[2];
        var x1 = input[0] == 0xFFFF ? 0 : (int)p->opta[2];

        y0 *= (int)p->opta[1];
        var y1 = input[1] == 0xFFFF ? 0 : (int)p->opta[1];

        z0 *= (int)p->opta[0];
        var z1 = input[2] == 0xFFFF ? 0 : (int)p->opta[0];

        lutTable += x0 + y0 + z0;
        if (rx >= ry)
        {
            if (ry >= rz)
            {
                y1 += x1;
                z1 += y1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = *lutTable++;
                    c3 -= c2;
                    c2 -= c1;
                    c1 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    *output++ = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                }
            }
            else if (rz >= rx)
            {
                x1 += z1;
                y1 += x1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = *lutTable++;
                    c2 -= c1;
                    c1 -= c3;
                    c3 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    *output++ = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                }
            }
            else
            {
                z1 += x1;
                y1 += z1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = *lutTable++;
                    c2 -= c3;
                    c3 -= c1;
                    c1 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    *output++ = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                }
            }
        }
        else
        {
            if (rx >= rz)
            {
                x1 += y1;
                z1 += x1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = *lutTable++;
                    c3 -= c1;
                    c1 -= c2;
                    c2 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    *output++ = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                }
            }
            else if (ry >= rz)
            {
                z1 += y1;
                x1 += z1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = *lutTable++;
                    c1 -= c3;
                    c3 -= c2;
                    c2 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    *output++ = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                }
            }
            else
            {
                y1 += z1;
                x1 += y1;
                for (; totalOut > 0; totalOut--)
                {
                    c1 = lutTable[x1];
                    c2 = lutTable[y1];
                    c3 = lutTable[z1];
                    c0 = *lutTable++;
                    c1 -= c2;
                    c2 -= c3;
                    c3 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    *output++ = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Dens(float* table, int i, int j, int outChan) =>
        table[i + j + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort Dens(ushort* table, int i, int j, int outChan) =>
        table[i + j + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Dens(float* table, int i, int j, int k, int outChan) =>
        table[i + j + k + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort Dens(ushort* table, int i, int j, int k, int outChan) =>
        table[i + j + k + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float a, float l, float h) =>
        l + ((h - l) * a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort Lerp(int a, ushort l, ushort h) =>
        (ushort)(l + ROUND_FIXED_TO_INT((h - l) * a));

    private static void Eval4Inputs(in ushort* input, ushort* output, in InterpParams* p16)
    {
        var tmp1 = stackalloc ushort[MAX_STAGE_CHANNELS];
        var tmp2 = stackalloc ushort[MAX_STAGE_CHANNELS];

        int c0, rest, c1 = 0, c2 = 0, c3 = 0;

        var fk = _cmsToFixedDomain(input[0] * (int)p16->Domain[0]);
        var fx = _cmsToFixedDomain(input[1] * (int)p16->Domain[1]);
        var fy = _cmsToFixedDomain(input[2] * (int)p16->Domain[2]);
        var fz = _cmsToFixedDomain(input[3] * (int)p16->Domain[3]);

        var k0 = FIXED_TO_INT(fk);
        var x0 = FIXED_TO_INT(fx);
        var y0 = FIXED_TO_INT(fy);
        var z0 = FIXED_TO_INT(fz);

        var rk = FIXED_REST_TO_INT(fk);
        var rx = FIXED_REST_TO_INT(fx);
        var ry = FIXED_REST_TO_INT(fy);
        var rz = FIXED_REST_TO_INT(fz);

        k0 *= (int)p16->opta[3];
        var k1 = k0 + (input[0] == 0xFFFF ? 0 : (int)p16->opta[3]);

        x0 *= (int)p16->opta[2];
        var x1 = x0 + (input[1] == 0xFFFF ? 0 : (int)p16->opta[2]);

        y0 *= (int)p16->opta[1];
        var y1 = y0 + (input[2] == 0xFFFF ? 0 : (int)p16->opta[1]);

        z0 *= (int)p16->opta[0];
        var z1 = z0 + (input[3] == 0xFFFF ? 0 : (int)p16->opta[0]);

        var lutTable = (ushort*)p16->Table;
        lutTable += k0;

        for (var outChan = 0; outChan < p16->nOutputs; outChan++)
        {
            c0 = Dens(lutTable, x0, y0, z0, outChan);

            if (rx >= ry && ry >= rz)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x1, y0, z0, outChan);
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            }
            else if (rx >= rz && rz >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x1, y0, z0, outChan);
            }
            else if (rz >= rx && rx >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            }
            else if (ry >= rx && rx >= rz)
            {
                c1 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x0, y1, z0, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            }
            else if (ry >= rz && rz >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y1, z0, outChan);
            }
            else if (rz >= ry && ry >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            }
            else
            {
                c1 = c2 = c3 = 0;
            }

            rest = (c1 * rx) + (c2 * ry) + (c3 * rz);
            tmp1[outChan] = (ushort)(c0 + ROUND_FIXED_TO_INT(_cmsToFixedDomain(rest)));
        }

        lutTable = (ushort*)p16->Table;
        lutTable += k1;

        for (var outChan = 0; outChan < p16->nOutputs; outChan++)
        {
            c0 = Dens(lutTable, x0, y0, z0, outChan);

            if (rx >= ry && ry >= rz)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x1, y0, z0, outChan);
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            }
            else if (rx >= rz && rz >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z0, outChan) - c0;
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x1, y0, z0, outChan);
            }
            else if (rz >= rx && rx >= ry)
            {
                c1 = Dens(lutTable, x1, y0, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c2 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            }
            else if (ry >= rx && rx >= rz)
            {
                c1 = Dens(lutTable, x1, y1, z0, outChan) - Dens(lutTable, x0, y1, z0, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x1, y1, z0, outChan);
            }
            else if (ry >= rz && rz >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z0, outChan) - c0;
                c3 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y1, z0, outChan);
            }
            else if (rz >= ry && ry >= rx)
            {
                c1 = Dens(lutTable, x1, y1, z1, outChan) - Dens(lutTable, x0, y1, z1, outChan);
                c2 = Dens(lutTable, x0, y1, z1, outChan) - Dens(lutTable, x0, y0, z1, outChan);
                c3 = Dens(lutTable, x0, y0, z1, outChan) - c0;
            }
            else
            {
                c1 = c2 = c3 = 0;
            }

            rest = (c1 * rx) + (c2 * ry) + (c3 * rz);
            tmp2[outChan] = (ushort)(c0 + ROUND_FIXED_TO_INT(_cmsToFixedDomain(rest)));
        }

        for (var i = 0; i < p16->nOutputs; i++)
            output[i] = LinearInterp(rk, tmp1[i], tmp2[i]);
    }

    private static void Eval4InputsFloat(in float* input, float* output, in InterpParams* p)
    {
        var lutTable = (float*)p->Table;
        var tmp1 = stackalloc float[MAX_STAGE_CHANNELS];
        var tmp2 = stackalloc float[MAX_STAGE_CHANNELS];

        var pk = Fclamp(input[0]) * p->Domain[0];
        var k0 = _cmsQuickFloor(pk);
        var rest = pk - k0;

        k0 *= (int)p->opta[3];
        var k1 = k0 + (Fclamp(input[0]) >= 1.0 ? 0 : (int)p->opta[3]);

        var p1 = *p;
        memcpy(&p1.Domain[0], &p->Domain[1], 3 * _sizeof<uint>());

        var t = lutTable + k0;
        p1.Table = t;

        TetrahedralInterpFloat(input + 1, tmp1, &p1);

        t = lutTable + k1;
        p1.Table = t;

        TetrahedralInterpFloat(input + 1, tmp2, &p1);

        for (var i = 0; i < p->nOutputs; i++)
        {
            var y0 = tmp1[i];
            var y1 = tmp2[i];

            output[i] = Lerp(rest, y0, y1);
        }
    }

    private static void EvalXInputs(int N, in ushort* input, ushort* output, in InterpParams* p16)
    {
        var NM = N - 1;

        var lutTable = (ushort*)p16->Table;

        var tmp1 = stackalloc ushort[MAX_STAGE_CHANNELS];
        var tmp2 = stackalloc ushort[MAX_STAGE_CHANNELS];

        var fk = _cmsToFixedDomain(input[0] * (int)p16->Domain[0]);
        var k0 = FIXED_TO_INT(fk);
        var rk = FIXED_REST_TO_INT(fk);

        var K0 = (int)p16->opta[NM] * k0;
        var K1 = (int)p16->opta[NM] * (k0 + (input[0] != 0xFFFF ? 1 : 0));

        var p1 = *p16;
        Buffer.MemoryCopy(&p16->Domain[1], &p1.Domain[0], MAX_INPUT_DIMENSIONS * _sizeof<uint>(), NM * _sizeof<uint>());

        var t = lutTable + K0;
        p1.Table = t;

        if (NM is 4)
            Eval4Inputs(input + 1, tmp1, &p1);
        else
            EvalXInputs(NM, input + 1, tmp1, &p1);

        t = lutTable + K1;
        p1.Table = t;

        if (NM is 4)
            Eval4Inputs(input + 1, tmp2, &p1);
        else
            EvalXInputs(NM, input + 1, tmp2, &p1);

        for (var j = 0; j < p16->nOutputs; j++)
            output[j] = LinearInterp(rk, tmp1[j], tmp2[j]);
    }

    private static void EvalXInputsFloat(int N, in float* input, float* output, in InterpParams* p)
    {
        var NM = N - 1;

        var lutTable = (float*)p->Table;
        var tmp1 = stackalloc float[MAX_STAGE_CHANNELS];
        var tmp2 = stackalloc float[MAX_STAGE_CHANNELS];

        var pk = Fclamp(input[0]) * p->Domain[0];
        var k0 = _cmsQuickFloor(pk);
        var rest = pk - k0;

        var K0 = (int)p->opta[NM] * k0;
        var K1 = K0 + (Fclamp(input[0]) >= 1.0 ? 0 : (int)p->opta[NM]);

        var p1 = *p;
        Buffer.MemoryCopy(&p->Domain[1], &p1.Domain[0], MAX_INPUT_DIMENSIONS * _sizeof<uint>(), NM * _sizeof<uint>());

        var t = lutTable + K0;
        p1.Table = t;

        if (NM is 4)
            Eval4InputsFloat(input + 1, tmp1, &p1);
        else
            EvalXInputsFloat(NM, input + 1, tmp1, &p1);

        t = lutTable + K1;
        p1.Table = t;

        if (NM is 4)
            Eval4InputsFloat(input + 1, tmp2, &p1);
        else
            EvalXInputsFloat(NM, input + 1, tmp2, &p1);

        for (var j = 0; j < p->nOutputs; j++)
        {
            var y0 = tmp1[j];
            var y1 = tmp2[j];

            output[j] = y0 + ((y1 - y0) * rest);
        }
    }

    private static void Eval5Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(5, input, output, p16);

    private static void Eval6Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(6, input, output, p16);

    private static void Eval7Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(7, input, output, p16);

    private static void Eval8Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(8, input, output, p16);

    private static void Eval9Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(9, input, output, p16);

    private static void Eval10Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(10, input, output, p16);

    private static void Eval11Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(11, input, output, p16);

    private static void Eval12Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(12, input, output, p16);

    private static void Eval13Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(13, input, output, p16);

    private static void Eval14Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(14, input, output, p16);

    private static void Eval15Inputs(in ushort* input, ushort* output, in InterpParams* p16) =>
        EvalXInputs(15, input, output, p16);

    private static void Eval5InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(5, input, output, p);

    private static void Eval6InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(6, input, output, p);

    private static void Eval7InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(7, input, output, p);

    private static void Eval8InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(8, input, output, p);

    private static void Eval9InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(9, input, output, p);

    private static void Eval10InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(10, input, output, p);

    private static void Eval11InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(11, input, output, p);

    private static void Eval12InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(12, input, output, p);

    private static void Eval13InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(13, input, output, p);

    private static void Eval14InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(14, input, output, p);

    private static void Eval15InputsFloat(in float* input, float* output, in InterpParams* p) =>
        EvalXInputsFloat(15, input, output, p);

    private static InterpFunction DefaultInterpolatorsFactory(uint nInputChannels, uint nOutputChannels, LerpFlag dwFlags)
    {
        InterpFunction interpolation = default;
        var isFloat = (dwFlags & LerpFlag.Float) != 0;
        var isTriliniar = (dwFlags & LerpFlag.Trilinear) != 0;

        memset(&interpolation, 0);

        // Safety check
        if (nInputChannels >= 4 && nOutputChannels >= MAX_STAGE_CHANNELS)
            return default;

        switch (nInputChannels)
        {
            case 1: // Gray Lut / linear

                if (nOutputChannels is 1)
                {
                    if (isFloat)
                        interpolation.LerpFloat = LinLerp1Dfloat;
                    else
                        interpolation.Lerp16 = LinLerp1D;
                }
                else
                {
                    if (isFloat)
                        interpolation.LerpFloat = Eval1InputFloat;
                    else
                        interpolation.Lerp16 = Eval1Input;
                }

                break;

            case 2: // Duotone

                if (isFloat)
                    interpolation.LerpFloat = BilinearInterpFloat;
                else
                    interpolation.Lerp16 = BilinearInterp16;

                break;

            case 3: // RGB et al

                if (isTriliniar)
                {
                    if (isFloat)
                        interpolation.LerpFloat = TrilinearInterpFloat;
                    else
                        interpolation.Lerp16 = TrilinearInterp16;
                }
                else
                {
                    if (isFloat)
                        interpolation.LerpFloat = TetrahedralInterpFloat;
                    else
                        interpolation.Lerp16 = TetrahedralInterp16;
                }
                break;

            case 4: // CMYK lut

                if (isFloat)
                    interpolation.LerpFloat = Eval4InputsFloat;
                else
                    interpolation.Lerp16 = Eval4Inputs;

                break;

            case 5:

                if (isFloat)
                    interpolation.LerpFloat = Eval5InputsFloat;
                else
                    interpolation.Lerp16 = Eval5Inputs;

                break;

            case 6:

                if (isFloat)
                    interpolation.LerpFloat = Eval6InputsFloat;
                else
                    interpolation.Lerp16 = Eval6Inputs;

                break;

            case 7:

                if (isFloat)
                    interpolation.LerpFloat = Eval7InputsFloat;
                else
                    interpolation.Lerp16 = Eval7Inputs;

                break;

            case 8:

                if (isFloat)
                    interpolation.LerpFloat = Eval8InputsFloat;
                else
                    interpolation.Lerp16 = Eval8Inputs;

                break;

            case 9:

                if (isFloat)
                    interpolation.LerpFloat = Eval9InputsFloat;
                else
                    interpolation.Lerp16 = Eval9Inputs;

                break;

            case 10:

                if (isFloat)
                    interpolation.LerpFloat = Eval10InputsFloat;
                else
                    interpolation.Lerp16 = Eval10Inputs;

                break;

            case 11:

                if (isFloat)
                    interpolation.LerpFloat = Eval11InputsFloat;
                else
                    interpolation.Lerp16 = Eval11Inputs;

                break;

            case 12:

                if (isFloat)
                    interpolation.LerpFloat = Eval12InputsFloat;
                else
                    interpolation.Lerp16 = Eval12Inputs;

                break;

            case 13:

                if (isFloat)
                    interpolation.LerpFloat = Eval13InputsFloat;
                else
                    interpolation.Lerp16 = Eval13Inputs;

                break;

            case 14:

                if (isFloat)
                    interpolation.LerpFloat = Eval14InputsFloat;
                else
                    interpolation.Lerp16 = Eval14Inputs;

                break;

            case 15:

                if (isFloat)
                    interpolation.LerpFloat = Eval15InputsFloat;
                else
                    interpolation.Lerp16 = Eval15Inputs;

                break;
        }
        return interpolation;
    }
}
