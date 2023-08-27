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

using S1Fixed15Number = System.Int32;

namespace lcms2.FastFloatPlugin;
public static partial class FastFloat
{
    private static S1Fixed15Number DOUBLE_TO_1FIXED15(double x) =>
        (S1Fixed15Number)(x * 0x8000 + 0.5);

    private static void FreeMatShaper(Context? _, object? Data)
    {
        if (Data is IDisposable p)
            p.Dispose();
    }

    private static void FillShaper(Span<ushort> Table, ToneCurve Curve)
    {
        for (var i = 0; i < MAX_NODES_IN_CURVE; i++)
        {
            var R = (float)i / (MAX_NODES_IN_CURVE - 1);
            var y = cmsEvalToneCurveFloat(Curve, R);

            Table[i] = (ushort)DOUBLE_TO_1FIXED15(y);
        }
    }

    private static XMatShaperData? SetMatShaper(Context? ContextID,
                                                ReadOnlySpan<ToneCurve> Curve1,
                                                MAT3 Mat,
                                                VEC3? Off,
                                                ReadOnlySpan<ToneCurve> Curve2,
                                                bool IdentityMat)
    {
        var p = new XMatShaperData(ContextID)
        {
            IdentityMat = IdentityMat,
        };

        p.Shaper1R.Clear();
        p.Shaper1G.Clear();
        p.Shaper1B.Clear();

        p.Shaper2R.Clear();
        p.Shaper2G.Clear();
        p.Shaper2B.Clear();

        p.Mat.Clear();
        p.Off.Clear();

        // Precompute tables
        FillShaper(p.Shaper1R, Curve1[0]);
        FillShaper(p.Shaper1G, Curve1[1]);
        FillShaper(p.Shaper1B, Curve1[2]);

        FillShaper(p.Shaper2R, Curve2[0]);
        FillShaper(p.Shaper2G, Curve2[1]);
        FillShaper(p.Shaper2B, Curve2[2]);

        // Convert matrix to nFixed14. Note that those values may take more than 16 bits if negative
        p.Mat[0] = DOUBLE_TO_1FIXED15(Mat.X.X);
        p.Mat[1] = DOUBLE_TO_1FIXED15(Mat.X.Y);
        p.Mat[2] = DOUBLE_TO_1FIXED15(Mat.X.Z);

        p.Mat[3] = DOUBLE_TO_1FIXED15(Mat.Y.X);
        p.Mat[4] = DOUBLE_TO_1FIXED15(Mat.Y.Y);
        p.Mat[5] = DOUBLE_TO_1FIXED15(Mat.Y.Z);

        p.Mat[6] = DOUBLE_TO_1FIXED15(Mat.Z.X);
        p.Mat[7] = DOUBLE_TO_1FIXED15(Mat.Z.Y);
        p.Mat[8] = DOUBLE_TO_1FIXED15(Mat.Z.Z);

        if (Off is null)
        {
            p.Off[0] = 0x4000;
            p.Off[1] = 0x4000;
            p.Off[2] = 0x4000;
        }
        else
        {
            p.Off[0] = DOUBLE_TO_1FIXED15(Off.Value.X) + 0x4000;
            p.Off[1] = DOUBLE_TO_1FIXED15(Off.Value.Y) + 0x4000;
            p.Off[2] = DOUBLE_TO_1FIXED15(Off.Value.Z) + 0x4000;
        }

        return p;
    }

    private static void MatShaperXform(Transform CMMcargo,
                                       ReadOnlySpan<byte> Input,
                                       Span<byte> Output,
                                       uint PixelsPerLine,
                                       uint LineCount,
                                       Stride Stride)
    {
        if (_cmsGetTransformUserData(CMMcargo) is not XMatShaperData p)
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
                // Across first shaper, which also converts to 1.15 fixed point.
                var r = p.Shaper1R[rin[0]];
                var g = p.Shaper1G[gin[0]];
                var b = p.Shaper1B[bin[0]];

                var (l1, l2, l3) =
                    p.IdentityMat
                        ? (r, g, b)
                        : (     // Evaluate the matrix in 1.14 fixed point
                            ((p.Mat[(0 * 3) + 0] * r) + (p.Mat[(0 * 3) + 1] * g) + (p.Mat[(0 * 3) + 2] * b) + p.Off[0]) >> 15,
                            ((p.Mat[(1 * 3) + 0] * r) + (p.Mat[(1 * 3) + 1] * g) + (p.Mat[(1 * 3) + 2] * b) + p.Off[1]) >> 15,
                            ((p.Mat[(2 * 3) + 0] * r) + (p.Mat[(2 * 3) + 1] * g) + (p.Mat[(2 * 3) + 2] * b) + p.Off[2]) >> 15);

                // Now we have to clip to 0..1.0 range
                var ri = (uint)Math.Max(0, Math.Min(l1, 0x8000));
                var gi = (uint)Math.Max(0, Math.Min(l1, 0x8000));
                var bi = (uint)Math.Max(0, Math.Min(l1, 0x8000));

                // And across second shaper
                BitConverter.TryWriteBytes(rout, p.Shaper2R[(int)ri]);
                BitConverter.TryWriteBytes(gout, p.Shaper2G[(int)gi]);
                BitConverter.TryWriteBytes(bout, p.Shaper2B[(int)bi]);

                // Handle alpha
                if (!ain.IsEmpty)
                    ain[..2].CopyTo(aout);

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

    private static bool OptimizeMatrixShaper15(out Transform2Fn TransformFn,
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

        // Only works on RGB to RGB and gray
        if (!(T_CHANNELS(InputFormat) is 3 && T_CHANNELS(OutputFormat) is 3))
            return false;

        // Only works of 15 bit to 15 bit
        if (T_BYTES(InputFormat) is not 2 || T_BYTES(OutputFormat) is not 2 ||
            T_BIT15(InputFormat) is 0 || T_BIT15(OutputFormat) is 0)
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
        cmsPipelineInsertStage(Dest, StageLoc.AtEnd, cmsStageDup(Curve1));

        if (!IdentityMat)
        {
            cmsPipelineInsertStage(Dest, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 3, 3, res, Data2.Offset));
        }

        cmsPipelineInsertStage(Dest, StageLoc.AtEnd, cmsStageDup(Curve2));

        {
            if (cmsStageData(Curve1) is not StageToneCurvesData mpeC1 || cmsStageData(Curve2) is not StageToneCurvesData mpeC2)
                return false;

            // In this particular optimization, cache does not help as it takes more time to deal with
            // the cache than with the pixel handling
            dwFlags |= cmsFLAGS_NOCACHE;

            // Setup the optimization routines
            UserData = SetMatShaper(ContextID, mpeC1.TheCurves, res, new VEC3(Data2.Offset), mpeC2.TheCurves, IdentityMat);
            FreeUserData = FreeMatShaper;

            TransformFn = MatShaperXform;
        }

        cmsPipelineFree(Src);
        dwFlags &= ~cmsFLAGS_CAN_CHANGE_FORMATTER;
        Lut = Dest;
        return true;
    }
}
