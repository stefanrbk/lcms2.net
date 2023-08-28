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
using lcms2.FastFloatPlugin.shapers;
using lcms2.state;
using lcms2.types;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

using S1Fixed14Number = System.Int32;

namespace lcms2.FastFloatPlugin;
public unsafe static partial class FastFloat
{
    [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static S1Fixed14Number DOUBLE_TO_1FIXED14(double x) =>
        (S1Fixed14Number)Math.Floor((x * 16384.0) + 0.5);

    private static void FillFirstShaper8Mat(Span<S1Fixed14Number>Table, ToneCurve Curve)
    {
        for (var i = 0; i < 256; i++)
        {
            var R = (float)(i / 255.0);
            Table[i] = DOUBLE_TO_1FIXED14(cmsEvalToneCurveFloat(Curve, R));
        }
    }

    private static void FillSecondShaper8Mat(Span<byte> Table, ToneCurve Curve)
    {
        for (var i = 0; i < 0x4001; i++)
        {
            var R = i / 16384.0f;
            var Val = cmsEvalToneCurveFloat(Curve, R);
            var w = (int)((Val * 255.0f) + 0.5f);

            w = Math.Clamp(w, 0, 255);

            Table[i] = (byte)w;
        }
    }

    private static XMatShaper8Data? Set8MatShaper(Context? ContextID,
                                                  ReadOnlySpan<ToneCurve> Curve1,
                                                  MAT3 Mat,
                                                  VEC3? Off,
                                                  ReadOnlySpan<ToneCurve> Curve2)
    {
        // Allocate a big chunk of memory to store precomputed tables
        var p = new XMatShaper8Data(ContextID);

        // Precompute tables
        FillFirstShaper8Mat(p.Shaper1R, Curve1[0]);
        FillFirstShaper8Mat(p.Shaper1G, Curve1[1]);
        FillFirstShaper8Mat(p.Shaper1B, Curve1[2]);

        FillSecondShaper8Mat(p.Shaper2R, Curve2[0]);
        FillSecondShaper8Mat(p.Shaper2G, Curve2[1]);
        FillSecondShaper8Mat(p.Shaper2B, Curve2[2]);

        // Convert matrix to nFixed14. Note that those values may take more than 16 bits
        p.Mat(0, 0) = DOUBLE_TO_1FIXED14(Mat.X.X);
        p.Mat(0, 1) = DOUBLE_TO_1FIXED14(Mat.X.Y);
        p.Mat(0, 2) = DOUBLE_TO_1FIXED14(Mat.X.Z);

        p.Mat(1, 0) = DOUBLE_TO_1FIXED14(Mat.Y.X);
        p.Mat(1, 1) = DOUBLE_TO_1FIXED14(Mat.Y.Y);
        p.Mat(1, 2) = DOUBLE_TO_1FIXED14(Mat.Y.Z);

        p.Mat(2, 0) = DOUBLE_TO_1FIXED14(Mat.Z.X);
        p.Mat(2, 1) = DOUBLE_TO_1FIXED14(Mat.Z.Y);
        p.Mat(2, 2) = DOUBLE_TO_1FIXED14(Mat.Z.Z);

        if (Off is null)
        {
            p.Mat(3, 0) = DOUBLE_TO_1FIXED14(0.5f);
            p.Mat(3, 1) = DOUBLE_TO_1FIXED14(0.5f);
            p.Mat(3, 2) = DOUBLE_TO_1FIXED14(0.5f);
        }
        else
        {
            p.Mat(3, 0) = DOUBLE_TO_1FIXED14(Off.Value.X + 0.5);
            p.Mat(3, 1) = DOUBLE_TO_1FIXED14(Off.Value.Y + 0.5);
            p.Mat(3, 2) = DOUBLE_TO_1FIXED14(Off.Value.Z + 0.5);
        }

        return p;
    }

    private static void MatShaperXform8(Transform CMMcargo,
                                        ReadOnlySpan<byte> Input,
                                        Span<byte> Output,
                                        uint PixelsPerLine,
                                        uint LineCount,
                                        Stride Stride)
    {
        if (_cmsGetTransformUserData(CMMcargo) is not XMatShaper8Data p)
            return;

        Span<uint> SourceStartingOrder = stackalloc uint[cmsMAXCHANNELS];
        Span<uint> SourceIncrements = stackalloc uint[cmsMAXCHANNELS];
        Span<uint> DestStartingOrder = stackalloc uint[cmsMAXCHANNELS];
        Span<uint> DestIncrements = stackalloc uint[cmsMAXCHANNELS];

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

            for (var ii = 0; ii < PixelsPerLine; ii++)
            {
                // Across first shaper, which also converts to 1.14 fixed point. 16 bits guaranteed.
                var r = p.Shaper1R[rin[0]];
                var g = p.Shaper1G[gin[0]];
                var b = p.Shaper1B[bin[0]];

                // Evaluate the matrix in 1.14 fixed point
                var l1 = ((p.Mat(0, 0) * r) + (p.Mat(1, 0) * g) + (p.Mat(2, 0) * b) + p.Mat(3, 0)) >> 14;
                var l2 = ((p.Mat(0, 1) * r) + (p.Mat(1, 1) * g) + (p.Mat(2, 1) * b) + p.Mat(3, 1)) >> 14;
                var l3 = ((p.Mat(0, 2) * r) + (p.Mat(1, 2) * g) + (p.Mat(2, 2) * b) + p.Mat(3, 2)) >> 14;

                // Now we have to clip to 0..1.0 range
                var ri = Math.Clamp(l1, 0, 0x4000);
                var gi = Math.Clamp(l2, 0, 0x4000);
                var bi = Math.Clamp(l3, 0, 0x4000);

                // And across second shaper

                rout[0] = p.Shaper2R[ri];
                gout[0] = p.Shaper2G[gi];
                bout[0] = p.Shaper2B[bi];

                // Handle alpha
                if (!ain.IsEmpty)
                    aout[0] = ain[0];

                rin = rin[(int)SourceIncrements[0]..];
                gin = gin[(int)SourceIncrements[1]..];
                bin = bin[(int)SourceIncrements[2]..];
                if (!ain.IsEmpty) ain = ain[(int)SourceIncrements[3]..];

                rout = rout[(int)DestIncrements[0]..];
                gout = gout[(int)DestIncrements[1]..];
                bout = bout[(int)DestIncrements[2]..];
                if (!aout.IsEmpty) aout = aout[(int)DestIncrements[3]..];
            }

            strideIn += Stride.BytesPerLineIn;
            strideOut += Stride.BytesPerLineOut;
        }
    }

    private static bool Optimize8MatrixShaper(out Transform2Fn TransformFn,
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

        // Only works on RGB to RGB and gray to gray
        if (!((T_CHANNELS(InputFormat) is 3 && T_CHANNELS(OutputFormat) is 3) ||
              (T_CHANNELS(InputFormat) is 1 && T_CHANNELS(OutputFormat) is 1) ))
        {
            return false;
        }

        // Only works on 8 bit input
        if (T_BYTES(InputFormat) is not 1 || T_BYTES(OutputFormat) is not 1)
        {
            return false;
        }

        // Seems suitable, proceed

        var Src = Lut;
        Span<double> factor = stackalloc double[1] { 1.0 };
        var IdentityMat = false;
        MAT3 res = default;

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

        if (cmsStageInputChannels(Matrix1) is 1 && cmsStageInputChannels(Matrix2) is 1)
        {
            // This is a gray to gray. Just multiply
            factor[0] = (Data1.Double[0] * Data2.Double[0]) +
                        (Data1.Double[1] * Data2.Double[1]) +
                        (Data1.Double[2] * Data2.Double[2]);

            if (Math.Abs(1 - factor[0]) < (1.0 / 65535.0))
                IdentityMat = true;
        }
        else
        {
            // Multiply both matrices to get the result
            res = new MAT3(Data2.Double) * new MAT3(Data1.Double);

            // Now the result is in res + Data2.Offset. Maybe is a plain identity?
            IdentityMat = res.IsIdentity && Data2.Offset is null;   // We can get rid of full matrix
        }

        // Allocate an empty LUT
        var Dest = cmsPipelineAlloc(ContextID, nChans, nChans);
        if (Dest is null) return false;

        // Assemble the new LUT
        cmsPipelineInsertStage(Dest, StageLoc.AtBegin, cmsStageDup(Curve1));

        if (!IdentityMat)
        {
            if (nChans is 1)
            {
                cmsPipelineInsertStage(Dest, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 1, 1, factor, Data2.Offset));
            }
            else
            {
                cmsPipelineInsertStage(Dest, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 3, 3, res, Data2.Offset));
            }
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
                UserData = Set8MatShaper(ContextID, mpeC1.TheCurves, res, new VEC3(Data2.Offset), mpeC2.TheCurves);
                FreeUserData = FreeDisposable;

                TransformFn = MatShaperXform8;
            }
        }

        cmsPipelineFree(Src);
        dwFlags &= ~cmsFLAGS_CAN_CHANGE_FORMATTER;
        Lut = Dest;
        return true;
    }
}
