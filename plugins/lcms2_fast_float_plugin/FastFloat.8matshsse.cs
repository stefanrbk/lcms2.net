//---------------------------------------------------------------------------------
//
//  Little Color Management System, fast floating point extensions
//  Copyright (c) 1998-2022 Marti Maria Saguer, all rights reserved
//                     2023 Stefan Kewatt, all rights reserved
//
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//---------------------------------------------------------------------------------
using lcms2.state;
using lcms2.types;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace lcms2.FastFloatPlugin;
public unsafe static partial class FastFloat
{
    private static void MatShaperXform8SSE(Transform CMMcargo,
                                           ReadOnlySpan<byte> Input,
                                           Span<byte> Output,
                                           uint PixelsPerLine,
                                           uint LineCount,
                                           Stride Stride)
    {
        if (_cmsGetTransformUserData(CMMcargo) is not XMatShaperSSEData p)
            return;

        Span<uint> SourceStartingOrder = stackalloc uint[cmsMAXCHANNELS];
        Span<uint> SourceIncrements = stackalloc uint[cmsMAXCHANNELS];
        Span<uint> DestStartingOrder = stackalloc uint[cmsMAXCHANNELS];
        Span<uint> DestIncrements = stackalloc uint[cmsMAXCHANNELS];

        var mat0 = Sse.LoadAlignedVector128(&p.Data->Mat[0 * 4]);
        var mat1 = Sse.LoadAlignedVector128(&p.Data->Mat[1 * 4]);
        var mat2 = Sse.LoadAlignedVector128(&p.Data->Mat[2 * 4]);
        var mat3 = Sse.LoadAlignedVector128(&p.Data->Mat[3 * 4]);

        var zero = Vector128<float>.Zero;
        var one = Vector128<float>.One;
        var scale = Vector128.Create((float)0x4000);

        var buffer = stackalloc byte[32];
        var output_index = (uint*)(((ulong)buffer + 16) & ~(ulong)0xf);

        _cmsComputeComponentIncrements(cmsGetTransformInputFormat(CMMcargo), Stride.BytesPerPlaneIn, out _, out var nalpha, SourceStartingOrder, SourceIncrements);
        _cmsComputeComponentIncrements(cmsGetTransformOutputFormat(CMMcargo), Stride.BytesPerPlaneOut, out _, out nalpha, DestStartingOrder, DestIncrements);

        if ((_cmsGetTransformFlags(CMMcargo) & cmsFLAGS_COPY_ALPHA) is 0)
            nalpha = 0;

        var strideIn = 0u;
        var strideOut = 0u;
        for (var i = 0; i < LineCount; i++)
        {
            var rin = Input[(int)(SourceStartingOrder[0] + strideIn)..];
            var gin = Input[(int)(SourceStartingOrder[1] + strideIn)..];
            var bin = Input[(int)(SourceStartingOrder[2] + strideIn)..];
            var ain =
                nalpha is not 0
                    ? Input[(int)(SourceStartingOrder[3] + strideIn)..]
                    : default;

            var rout = Output[(int)(DestStartingOrder[0] + strideOut)..];
            var gout = Output[(int)(DestStartingOrder[1] + strideOut)..];
            var bout = Output[(int)(DestStartingOrder[2] + strideOut)..];
            var aout =
                nalpha is not 0
                    ? Output[(int)(SourceStartingOrder[3] + strideOut)..]
                    : default;

            // Prefetch
            var rvector = Vector128.Create(p.Data->Shaper1R[rin[0]]);
            var gvector = Vector128.Create(p.Data->Shaper1G[gin[0]]);
            var bvector = Vector128.Create(p.Data->Shaper1B[bin[0]]);

            for (var ii = 0; ii < PixelsPerLine; ii++)
            {
                var el1 = Sse.Multiply(rvector, mat0);
                var el2 = Sse.Multiply(gvector, mat1);
                var el3 = Sse.Multiply(bvector, mat2);

                var sum = Sse.Add(el1, Sse.Add(el2, Sse.Add(el3, mat3)));

                var @out = Sse.Min(Sse.Max(sum, zero), one);

                @out = Sse.Multiply(@out, scale);

                // Rounding and converting to index.
                // Actually this is a costly instruction that may be blocking performance
                
                Vector128.ConvertToInt32(@out).StoreAligned((int*)output_index);

                // Handle alpha
                if (!ain.IsEmpty)
                    aout[0] = ain[0];

                rin = rin[(int)SourceIncrements[0]..];
                gin = gin[(int)SourceIncrements[1]..];
                bin = bin[(int)SourceIncrements[2]..];
                if (!ain.IsEmpty) ain = ain[(int)SourceIncrements[3]..];

                // Take next value whilst store is being performed
                if (ii < PixelsPerLine - 1)
                {
                    rvector = Vector128.Create(p.Data->Shaper1R[rin[0]]);
                    gvector = Vector128.Create(p.Data->Shaper1G[gin[0]]);
                    bvector = Vector128.Create(p.Data->Shaper1B[bin[0]]);
                }

                rout[0] = p.Data->Shaper2R[output_index[0]];
                gout[0] = p.Data->Shaper2G[output_index[1]];
                bout[0] = p.Data->Shaper2B[output_index[2]];

                rout = rout[(int)DestIncrements[0]..];
                gout = gout[(int)DestIncrements[1]..];
                bout = bout[(int)DestIncrements[2]..];
                if (!aout.IsEmpty) aout = aout[(int)DestIncrements[3]..];
            }

            strideIn += Stride.BytesPerLineIn;
            strideOut += Stride.BytesPerLineOut;
        }
    }

    private static bool IsSSE2Available() =>
        Sse2.IsSupported;

    private static bool Optimize8MatrixShaperSSE(out Transform2Fn TransformFn,
                                                 out object? UserData,
                                                 out FreeUserDataFn? FreeUserData,
                                                 ref Pipeline Lut,
                                                 ref uint InputFormat,
                                                 ref uint OutputFormat,
                                                 ref uint dwFlags)
    {
        FreeUserData = null;
        UserData = null;
        TransformFn = null!;

        // Check for SSE2 support
        if (!IsSSE2Available()) return false;

        // Only works on 3 to 3, probably RGB
        if (!(T_CHANNELS(InputFormat) is 3 && T_CHANNELS(OutputFormat) is 3))
            return false;

        // Only works on 8 bit input
        if (T_BYTES(InputFormat) is not 1 || T_BYTES(OutputFormat) is not 1)
        {
            return false;
        }

        // Seems suitable, proceed

        var Src = Lut;

        // Check for shaper-matrix-matrix-shaper structure, that is what this optimizer stands for
        if (!cmsPipelineCheckAndRetrieveStages(Src, cmsSigCurveSetElemType, out var Curve1,
                                                    cmsSigMatrixElemType, out var Matrix1,
                                                    cmsSigMatrixElemType, out var Matrix2,
                                                    cmsSigCurveSetElemType, out var Curve2))
        {
            return false;
        }

        var ContextID = cmsGetPipelineContextID(Src);
        var nChans = (uint)T_CHANNELS(InputFormat);

        // Get both matrices, which are 3x3
        if (cmsStageData(Matrix1) is not StageMatrixData Data1 || cmsStageData(Matrix2) is not StageMatrixData Data2)
            return false;

        // Input offset should be zero
        if (Data1.Offset is not null)
            return false;

        // Multiply both matrices to get the result
        var res = new MAT3(Data2.Double) * new MAT3(Data1.Double);

        // Now the result is in res + Data2.Offset. Maybe is a plain identity?
        var IdentityMat = res.IsIdentity && Data2.Offset is null;   // We can get rid of full matrix

        // Allocate an empty LUT
        var Dest = cmsPipelineAlloc(ContextID, nChans, nChans);
        if (Dest is null) return false;

        // Assemble the new LUT
        cmsPipelineInsertStage(Dest, StageLoc.AtBegin, cmsStageDup(Curve1));

        if (!IdentityMat)
        {
            cmsPipelineInsertStage(Dest, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 3, 3, res, Data2.Offset));
        }

        cmsPipelineInsertStage(Dest, StageLoc.AtEnd, cmsStageDup(Curve2));

        {
            // If identity on matrix, we can further optimize the curves, so call the join curves routine
            if (IdentityMat)
            {
                Optimize8ByJoiningCurves(out TransformFn, out UserData, out FreeUserData, ref Dest, ref InputFormat, ref OutputFormat, ref dwFlags);
            }
            else
            {
                if (cmsStageData(Curve1) is not StageToneCurvesData mpeC1 || cmsStageData(Curve2) is not StageToneCurvesData mpeC2)
                    return false;

                // In this particular optimization, cache does not help as it takes more time to deal with
                // the cache than with the pixel handling
                dwFlags |= cmsFLAGS_NOCACHE;

                // Setup the optimization routines
                UserData = XMatShaperSSEData.SetShaper(ContextID, mpeC1.TheCurves, res, new VEC3(Data2.Offset), mpeC2.TheCurves);
                FreeUserData = FreeDisposable;

                TransformFn = MatShaperXform8SSE;
            }
        }

        cmsPipelineFree(Src);
        dwFlags &= ~cmsFLAGS_CAN_CHANGE_FORMATTER;
        Lut = Dest;
        return true;
    }
}

file unsafe class XMatShaperSSEData : IDisposable
{
    public Inner* Data;
    public Context? ContextID;
    private bool disposedValue;

    public struct Inner
    {
        // This is for SSE, MUST be aligned at 16 bit boundary

        public fixed float Mat[16];     // n.14 to n.14 (needs a saturation after that)

        public void* real_ptr;

        public fixed float Shaper1R[256];   // from 0..255 to 1.14 (0.0...1.0)
        public fixed float Shaper1G[256];
        public fixed float Shaper1B[256];

        public fixed byte Shaper2R[0x4001];   // 1.14 to 0..255
        public fixed byte Shaper2G[0x4001];
        public fixed byte Shaper2B[0x4001];
    }

    public XMatShaperSSEData(Context? context)
    {
        ContextID = context;

        var real_ptr = (byte*)NativeMemory.AllocZeroed((nuint)sizeof(Inner) + 32);
        var aligned = (byte*)(((ulong)real_ptr + 16) & ~(ulong)0xf);
        var p = (Inner*)aligned;

        p->real_ptr = real_ptr;

        Data = p;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            NativeMemory.Free(Data->real_ptr);
            Data = null;
            disposedValue = true;
        }
    }

    ~XMatShaperSSEData() =>
        Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static void FillFirstShaper8MatSSE(Span<float> Table, ToneCurve Curve)
    {
        for (var i = 0; i < 256; i++)
        {
            var R = (float)(i / 255.0);
            Table[i] = cmsEvalToneCurveFloat(Curve, R);
        }
    }

    private static void FillSecondShaper8MatSSE(Span<float> Table, ToneCurve Curve)
    {
        for (var i = 0; i < 0x4001; i++)
        {
            var R = (float)(i / 16384.0);
            var Val = cmsEvalToneCurveFloat(Curve, R);
            var w = (int)((Val * 255.0f) + 0.5f);

            w = Math.Max(0, Math.Min(255, w));

            Table[i] = (byte)w;
        }
    }

    public static XMatShaperSSEData? SetShaper(Context? ContextID,
                                               ReadOnlySpan<ToneCurve> Curve1,
                                               MAT3 Mat,
                                               VEC3? Off,
                                               ReadOnlySpan<ToneCurve> Curve2)
    {
        // Allocate a big chunk of memory to store precomputed tables
        var p = new XMatShaperSSEData(ContextID);

        // Precompute tables
        FillFirstShaper8MatSSE(new(p.Data->Shaper1R, 256), Curve1[0]);
        FillFirstShaper8MatSSE(new(p.Data->Shaper1G, 256), Curve1[1]);
        FillFirstShaper8MatSSE(new(p.Data->Shaper1B, 256), Curve1[2]);

        FillSecondShaper8MatSSE(new(p.Data->Shaper2R, 0x4001), Curve2[0]);
        FillSecondShaper8MatSSE(new(p.Data->Shaper2G, 0x4001), Curve2[1]);
        FillSecondShaper8MatSSE(new(p.Data->Shaper2B, 0x4001), Curve2[2]);

        // Convert matrix to float
        p.Data->Mat[0] = (float)Mat.X.X;
        p.Data->Mat[1] = (float)Mat.X.Y;
        p.Data->Mat[2] = (float)Mat.X.Z;

        p.Data->Mat[3] = (float)Mat.Y.X;
        p.Data->Mat[4] = (float)Mat.Y.Y;
        p.Data->Mat[5] = (float)Mat.Y.Z;

        p.Data->Mat[6] = (float)Mat.Z.X;
        p.Data->Mat[7] = (float)Mat.Z.Y;
        p.Data->Mat[8] = (float)Mat.Z.Z;

        // Roundoff
        if (Off is null)
        {
            p.Data->Mat[(3 * 4) + 0] = 0f;
            p.Data->Mat[(3 * 4) + 1] = 0f;
            p.Data->Mat[(3 * 4) + 2] = 0f;
        }
        else
        {
            p.Data->Mat[(3 * 4) + 0] = (float)Off.Value.X;
            p.Data->Mat[(3 * 4) + 1] = (float)Off.Value.Y;
            p.Data->Mat[(3 * 4) + 2] = (float)Off.Value.Z;
        }

        return p;
    }
}