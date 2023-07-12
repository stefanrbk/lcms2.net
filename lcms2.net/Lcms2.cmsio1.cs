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

namespace lcms2;

public static unsafe partial class Lcms2
{
    internal static readonly Signature* Device2PCS16;
    internal static readonly Signature* Device2PCSFloat;
    internal static readonly Signature* PCS2Device16;
    internal static readonly Signature* PCS2DeviceFloat;

    internal static readonly double[] GrayInputMatrix;
    internal static readonly double[] OneToThreeInputMatrix;
    internal static readonly double[] PickYMatrix;
    internal static readonly double[] PickLstarMatrix;

    internal const double InpAdj = 1 / MAX_ENCODEABLE_XYZ;
    internal const double OutpAdj = MAX_ENCODEABLE_XYZ;

    internal static bool _cmsReadMediaWhitePoint(CIEXYZ* Dest, Profile Profile)
    {
        _cmsAssert(Dest);

        // If no wp, take D50
        if (cmsReadTag(Profile, cmsSigMediaWhitePointTag) is not BoxPtr<CIEXYZ> Tag)
        {
            *Dest = *cmsD50_XYZ();
            return true;
        }

        // V2 display profiles should give D50
        if (cmsGetEncodedICCVersion(Profile) < 0x04000000)
        {
            if (cmsGetDeviceClass(Profile) == cmsSigDisplayClass)
            {
                *Dest = *cmsD50_XYZ();
                return true;
            }
        }

        // All seems ok
        *Dest = *Tag.Ptr;
        return true;
    }

    internal static bool _cmsReadCHAD(MAT3* Dest, Profile Profile)
    {
        _cmsAssert(Dest);

        if (cmsReadTag(Profile, cmsSigChromaticAdaptationTag) is BoxPtr<MAT3> Tag)
        {
            *Dest = *Tag.Ptr;
            return true;
        }

        // No CHAD available, default it to identity
        _cmsMAT3identity(out *Dest);

        // V2 display profiles should give D50
        if (cmsGetEncodedICCVersion(Profile) < 0x04000000)
        {
            if ((uint)cmsGetDeviceClass(Profile) is cmsSigDisplayClass)
            {
                if (cmsReadTag(Profile, cmsSigMediaWhitePointTag) is not BoxPtr<CIEXYZ> White)
                {
                    _cmsMAT3identity(out *Dest);
                    return true;
                }
                return _cmsAdaptationMatrix(Dest, null, White, cmsD50_XYZ());
            }
        }

        return true;
    }

    private static bool ReadIccMatrixRGB2XYZ(MAT3* r, Profile Profile)
    {
        _cmsAssert(r);

        if (cmsReadTag(Profile, cmsSigRedColorantTag) is not BoxPtr<CIEXYZ> PtrRed ||
            cmsReadTag(Profile, cmsSigGreenColorantTag) is not BoxPtr<CIEXYZ> PtrGreen ||
            cmsReadTag(Profile, cmsSigBlueColorantTag) is not BoxPtr<CIEXYZ> PtrBlue)
        {
            return false;
        }

        _cmsVEC3init(out r->X, PtrRed.Ptr->X, PtrGreen.Ptr->X, PtrBlue.Ptr->X);
        _cmsVEC3init(out r->Y, PtrRed.Ptr->Y, PtrGreen.Ptr->Y, PtrBlue.Ptr->Y);
        _cmsVEC3init(out r->Z, PtrRed.Ptr->Z, PtrGreen.Ptr->Z, PtrBlue.Ptr->Z);

        return true;
    }

    private static Pipeline? BuildGrayInputMatrixPipeline(Profile Profile)
    {
        ToneCurve[]? LabCurves = null;
        var ContextID = cmsGetProfileContextID(Profile);
        var pool = Context.GetPool<ToneCurve>(ContextID);

        if (cmsReadTag(Profile, cmsSigGrayTRCTag) is not ToneCurve GrayTRC)
            return null;

        var Lut = cmsPipelineAlloc(ContextID, 1, 3);
        if (Lut is null) goto Error;

        LabCurves = pool.Rent(3);
        LabCurves[0] = GrayTRC;

        if ((uint)cmsGetPCS(Profile) is cmsSigLabData)
        {
            // In this case we implement the profile as an identity matrix plus 3 tone curves
            var Zero = stackalloc ushort[] { 0x8080, 0x8080 };

            var EmptyTab = cmsBuildTabulatedToneCurve16(ContextID, 2, Zero);

            if (EmptyTab is null) goto Error;

            LabCurves[1] = EmptyTab;
            LabCurves[2] = EmptyTab;

            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 3, 1, OneToThreeInputMatrix, null)) ||
                !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, 3, LabCurves)))
            {
                cmsFreeToneCurve(EmptyTab);
                goto Error;
            }

            cmsFreeToneCurve(EmptyTab);
        }
        else
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, 1, LabCurves)) ||
                !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 3, 1, GrayInputMatrix, null)))
            {
                goto Error;
            }
        }

        pool.Return(LabCurves);
        return Lut;

    Error:
        if (LabCurves is not null)
            pool.Return(LabCurves);

        cmsPipelineFree(Lut);
        return null;
    }

    private static Pipeline? BuildRGBInputMatrixShaper(Profile Profile)
    {
        //VEC3* Matv = &Mat.X;
        MAT3 Mat;
        Pipeline? Lut = null;
        double[]? MatArray = null;

        var ContextID = cmsGetProfileContextID(Profile);
        var tcPool = Context.GetPool<ToneCurve>(ContextID);
        var dPool = Context.GetPool<double>(ContextID);

        ToneCurve[] Shapes = tcPool.Rent(3);

        if (!ReadIccMatrixRGB2XYZ(&Mat, Profile))
            goto Error;

        // XYZ PCS in encoded in 1.15 format, and the matrix output comes in 0..0xffff range, so
        // we need to adjust the output by a factor of (0x10000/0xffff) to put data in
        // a 1.16 range, and then a >> 1 to obtain 1.15. The total factor is (65536.0)/(65535.0*2)

        //for (var i = 0; i < 3; i++)
        //    for (var j = 0; j < 3; j++)
        //        (&Matv[i].X)[j] *= InpAdj;
        Mat.X *= InpAdj;
        Mat.Y *= InpAdj;
        Mat.Z *= InpAdj;

        Shapes[0] = (cmsReadTag(Profile, cmsSigRedTRCTag) as ToneCurve)!;
        Shapes[1] = (cmsReadTag(Profile, cmsSigGreenTRCTag) as ToneCurve)!;
        Shapes[2] = (cmsReadTag(Profile, cmsSigBlueTRCTag) as ToneCurve)!;

        if (Shapes[0] is null || Shapes[1] is null || Shapes[2] is null)
            goto Error;

        Lut = cmsPipelineAlloc(ContextID, 3, 3);
        if (Lut is null)
            goto Error;

        MatArray = Mat.AsArray(dPool);
        if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, 3, Shapes)) ||
            !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 3, 3, MatArray, null)))
        {
            goto Error;
        }

        // Note that it is certainly possible a single profile would have a LUT based
        // tag for output working in lab and a matrix-shaper for the fallback cases.
        // This is not allowed by the spec, but this code is tolerant to those cases
        if ((uint)cmsGetPCS(Profile) is cmsSigLabData &&
            !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocXYZ2Lab(ContextID)))
        {
            goto Error;
        }

        tcPool.Return(Shapes);
        dPool.Return(MatArray);
        return Lut;

    Error:
        if (Shapes is not null)
            tcPool.Return(Shapes);
        if (MatArray is not null)
            dPool.Return(MatArray);
        cmsPipelineFree(Lut);
        return null;
    }

    internal static Pipeline? _cmsReadFloatInputTag(Profile Profile, Signature tagFloat)
    {
        var ContextID = cmsGetProfileContextID(Profile);
        var Lut = cmsPipelineDup(cmsReadTag(Profile, tagFloat) as Pipeline);
        var spc = cmsGetColorSpace(Profile);
        var PCS = cmsGetPCS(Profile);

        if (Lut is null) return null;

        // input and output of transform are in lcms 0..1 encoding.  If XYZ or Lab spaces are used,
        // these need to be normalized into the appropriate ranges (Lab = 100,0,0, XYZ=1.0,1.0,1.0)
        if ((uint)spc is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageNormalizeToLabFloat(ContextID)))
                goto Error;
        }
        else if ((uint)spc is cmsSigXYZData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageNormalizeToXYZFloat(ContextID)))
                goto Error;
        }

        if ((uint)PCS is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageNormalizeFromLabFloat(ContextID)))
                goto Error;
        }
        else if ((uint)PCS is cmsSigXYZData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageNormalizeFromXyzFloat(ContextID)))
                goto Error;
        }

        return Lut;

    Error:
        cmsPipelineFree(Lut);
        return null;
    }

    internal static Pipeline? _cmsReadInputLUT(Profile Profile, uint Intent)
    {
        var ContextID = cmsGetProfileContextID(Profile);

        // On named color, take the appropriate tag
        if ((uint)cmsGetDeviceClass(Profile) is cmsSigNamedColorClass)
        {
            if (cmsReadTag(Profile, cmsSigNamedColor2Tag) is not BoxPtr<NamedColorList> nc)
                return null;

            var Lut = cmsPipelineAlloc(ContextID, 0, 0);
            if (Lut is null)
            {
                cmsFreeNamedColorList(nc);
                return null;
            }

            if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageAllocNamedColor(nc, true)) ||
                !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocLabV2ToV4(ContextID)))
            {
                cmsPipelineFree(Lut);
                return null;
            }

            return Lut;
        }

        // This is an attempt to reuse this function to retrieve the matrix-shaper as pipeline no
        // matter other LUT are present and have precedence. Intent = 0xffffffff can be used for that.
        if (Intent <= INTENT_ABSOLUTE_COLORIMETRIC)
        {
            var tag16 = Device2PCS16[Intent];
            var tagFloat = Device2PCSFloat[Intent];

            if (cmsIsTag(Profile, tagFloat))   // Float tag takes precedence
            {
                // Floating point LUT are always V4, but the encoding range is no
                // longer 0..1.0, so we need to add an stage depending on the color space
                return _cmsReadFloatInputTag(Profile, tagFloat);
            }

            // Revert to perceptual if no tag is found
            if (!cmsIsTag(Profile, tag16))
                tag16 = Device2PCS16[0];

            if (cmsIsTag(Profile, tag16)) // Is there any LUT-Based table?
            {
                // Check profile version and LUT type. Do the necessaary adjustments if needed

                // First read the tag
                if (cmsReadTag(Profile, tag16) is not Pipeline Lut)
                    return null;

                // After reading it, we have the info about the original type
                var OriginalType = _cmsGetTagTrueType(Profile, tag16);

                // The profile owns the Lut, so we need to copy it
                Lut = cmsPipelineDup(Lut);

                // We need to adjust data only for Lab16 on output
                if ((uint)OriginalType is not cmsSigLut16Type || (uint)cmsGetPCS(Profile) is not cmsSigLabData)
                    return Lut;

                // If the input is Lab, add also a conversion at the begin
                if ((uint)cmsGetColorSpace(Profile) is cmsSigLabData &&
                    !cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageAllocLabV4ToV2(ContextID)))
                    goto Error;

                // Add a matrix for conversion V2 to V4 Lab PCS
                if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocLabV2ToV4(ContextID)))
                    goto Error;

                return Lut;

            Error:
                cmsPipelineFree(Lut);
                return null;
            }
        }

        // Lut was not found, try to create a matrix-shaper

        // Check if this is a grayscale profile.
        if ((uint)cmsGetColorSpace(Profile) is cmsSigGrayData)
            // if so, build appropriate conversion tables.
            // The tables are the PCS iluminant, scaled across GrayTRC
            return BuildGrayInputMatrixPipeline(Profile);

        // Not gray, create a normal matrix-shaper
        return BuildRGBInputMatrixShaper(Profile);
    }

    private static Pipeline? BuildGrayOutputPipeline(Profile Profile)
    {
        var ContextID = cmsGetProfileContextID(Profile);

        if (cmsReadTag(Profile, cmsSigGrayTRCTag) is not ToneCurve GrayTRC)
            return null;

        var RevGrayTRC = cmsReverseToneCurve(GrayTRC);
        if (RevGrayTRC is null) return null;

        var Lut = cmsPipelineAlloc(ContextID, 3, 1);
        if (Lut is null) goto Error1;

        var pool = Context.GetPool<ToneCurve>(ContextID);
        var rev = pool.Rent(1);

        if ((uint)cmsGetPCS(Profile) is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 1, 3, PickLstarMatrix, null)))
                goto Error2;
        }
        else
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 1, 3, PickYMatrix, null)))
                goto Error2;
        }
        rev[0] = RevGrayTRC;
        if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, 1, rev)))
            goto Error2;

        pool.Return(rev);
        cmsFreeToneCurve(RevGrayTRC);
        return Lut;

    Error2:
        pool.Return(rev);
        cmsPipelineFree(Lut);
    Error1:
        cmsFreeToneCurve(RevGrayTRC);
        return null;
    }

    private static Pipeline? BuildRGBOutputMatrixShaper(Profile Profile)
    {
        var Shapes = new ToneCurve?[3];
        var InvShapes = new ToneCurve?[3];
        MAT3 Mat;
        //VEC3* Invv = &Inv.X;

        var ContextID = cmsGetProfileContextID(Profile);

        if (!ReadIccMatrixRGB2XYZ(&Mat, Profile))
            return null;

        if (!_cmsMAT3inverse(Mat, out MAT3 Inv))
            return null;

        // XYZ PCS in encoded in 1.15 format, and the matrix input should come in 0..0xffff range, so
        // we need to adjust the input by a << 1 to obtain a 1.16 fixed and then by a factor of
        // (0xffff/0x10000) to put data in 0..0xffff range. Total factor is (2.0*65535.0)/65536.0;

        //for (var i = 0; i < 3; i++)
        //    for (var j = 0; j < 3; j++)
        //        (&Invv[i].X)[j] *= OutpAdj;
        Inv.X *= OutpAdj;
        Inv.Y *= OutpAdj;
        Inv.Z *= OutpAdj;

        Shapes[0] = cmsReadTag(Profile, cmsSigRedTRCTag) as ToneCurve;
        Shapes[1] = cmsReadTag(Profile, cmsSigGreenTRCTag) as ToneCurve;
        Shapes[2] = cmsReadTag(Profile, cmsSigBlueTRCTag) as ToneCurve;

        if (Shapes[0] is null || Shapes[1] is null || Shapes[2] is null)
            return null;

        InvShapes[0] = cmsReverseToneCurve(Shapes[0]!);
        InvShapes[1] = cmsReverseToneCurve(Shapes[1]!);
        InvShapes[2] = cmsReverseToneCurve(Shapes[2]!);

        if (InvShapes[0] is null || InvShapes[1] is null || InvShapes[2] is null)
            return null;

        ToneCurve[] InvShapesTriple = new ToneCurve[3]
        {
            InvShapes[0]!,
            InvShapes[1]!,
            InvShapes[2]!,
        };

        var Lut = cmsPipelineAlloc(ContextID, 3, 3);
        if (Lut is null) goto Error1;

        // Note that it is certainly possible a single profile would have a LUT based
        // tag for output working in lab and a matrix-shaper for the fallback cases.
        // This is not allowed by the spec, but this code is tolerant to those cases

        if ((uint)cmsGetPCS(Profile) is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocLab2XYZ(ContextID)))
                goto Error2;
        }

        var pool = _cmsGetContext(Lut.ContextID).GetBufferPool<double>();
        var InvArray = Inv.AsArray(pool);
        if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 3, 3, InvArray, null)) ||
            !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, 3, InvShapesTriple)))
        {
            pool.Return(InvArray);
            goto Error2;
        }
        pool.Return(InvArray);

        cmsFreeToneCurveTriple(InvShapesTriple);
        return Lut;

    Error2:
        cmsPipelineFree(Lut);
    Error1:
        cmsFreeToneCurveTriple(InvShapesTriple);
        return null;
    }

    private static void ChangeInterpolationToTrilinear(Pipeline? Lut)
    {
        for (var Stage = cmsPipelineGetPtrToFirstStage(Lut);
             Stage is not null;
             Stage = cmsStageNext(Stage))
        {
            if ((uint)cmsStageType(Stage) is cmsSigCLutElemType)
            {
                var CLUT = (StageCLutData)Stage.Data!;

                CLUT.Params.dwFlags |= (uint)LerpFlag.Trilinear;
                _cmsSetInterpolationRoutine(Lut?.ContextID, CLUT.Params);
            }
        }
    }

    internal static Pipeline? _cmsReadFloatOutputTag(Profile Profile, Signature tagFloat)
    {
        var ContextID = cmsGetProfileContextID(Profile);
        var Lut = cmsPipelineDup(cmsReadTag(Profile, tagFloat) as Pipeline);
        var PCS = cmsGetPCS(Profile);
        var dataSpace = cmsGetColorSpace(Profile);

        if (Lut is null) return null;

        // If PCS is Lab or XYZ, the floating point tag is accepting data in the space encoding,
        // and since the formatter has already accommodated to 0..1.0, we should undo this change
        if ((uint)PCS is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageNormalizeToLabFloat(ContextID)))
                goto Error;
        }
        else
        {
            if ((uint)PCS is cmsSigXYZData)
            {
                if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageNormalizeToXYZFloat(ContextID)))
                    goto Error;
            }
        }

        // The output can be Lab or XYZ, in which case normalization is needed on the end of the pipeline
        if ((uint)dataSpace is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageNormalizeFromLabFloat(ContextID)))
                goto Error;
        }
        else if ((uint)dataSpace is cmsSigXYZData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageNormalizeFromXyzFloat(ContextID)))
                goto Error;
        }

        return Lut;

    Error:
        cmsPipelineFree(Lut);
        return null;
    }

    internal static Pipeline? _cmsReadOutputLUT(Profile Profile, uint Intent)
    {
        var ContextID = cmsGetProfileContextID(Profile);

        if (Intent <= INTENT_ABSOLUTE_COLORIMETRIC)
        {
            var tag16 = PCS2Device16[Intent];
            var tagFloat = PCS2DeviceFloat[Intent];

            if (cmsIsTag(Profile, tagFloat))   // Float tag takes precedence
                // Floating point LUT are always V4
                return _cmsReadFloatOutputTag(Profile, tagFloat);

            // Revert to perceptual if no tag is found
            if (!cmsIsTag(Profile, tag16))
                tag16 = PCS2Device16[0];

            if (cmsIsTag(Profile, tag16))      // Is there any LUT-Based table?
            {
                // Check profile version and LUT type. Do the necessary adjustments if needed

                // First read the tag
                if (cmsReadTag(Profile, tag16) is not Pipeline Lut)
                    return null;

                // After reading it, we have info about the original type
                var OriginalType = _cmsGetTagTrueType(Profile, tag16);

                // The profile owns the Lut, so we need to copy it
                Lut = cmsPipelineDup(Lut);
                if (Lut is null) return null;

                // Now it is time for controversial stuff. I found that for 3D LUTS using
                // Lab used as indexer space, trilinear interpolation should be used
                if ((uint)cmsGetPCS(Profile) is cmsSigLabData)
                    ChangeInterpolationToTrilinear(Lut);

                // We need to adjust data only for Lab and Lut16 type
                if ((uint)OriginalType is not cmsSigLut16Type || (uint)cmsGetPCS(Profile) is not cmsSigLabData)
                    return Lut;

                // Add a matrix for conversion V4 to V3 Lab PCS
                if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageAllocLabV4ToV2(ContextID)))
                    goto Error;

                // If the output is Lab, add also a conversion at the end
                if ((uint)cmsGetColorSpace(Profile) is cmsSigLabData &&
                    !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocLabV2ToV4(ContextID)))
                {
                    goto Error;
                }

                return Lut;
            Error:
                cmsPipelineFree(Lut);
                return null;
            }
        }

        // Lut not found, try to create a matrix-shaper

        // Check if this is a grayscale profile.
        if ((uint)cmsGetColorSpace(Profile) is cmsSigGrayData)
            // if so, build appropriate conversion tables.
            // The tables are the PCS iluminant, scaled across GrayTRC
            return BuildGrayOutputPipeline(Profile);

        // Not gray, create a normal matrix-shaper, which only operates in XYZ space
        return BuildRGBOutputMatrixShaper(Profile);
    }

    internal static Pipeline? _cmsReadFloatDevicelinkTag(Profile Profile, Signature tagFloat)
    {
        var ContextID = cmsGetProfileContextID(Profile);
        var Lut = cmsPipelineDup(cmsReadTag(Profile, tagFloat) as Pipeline);
        var PCS = cmsGetPCS(Profile);
        var spc = cmsGetColorSpace(Profile);

        if (Lut is null) return null;

        if ((uint)spc is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageNormalizeToLabFloat(ContextID)))
                goto Error;
        }
        else if ((uint)spc is cmsSigXYZData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageNormalizeToXYZFloat(ContextID)))
                goto Error;
        }

        if ((uint)PCS is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageNormalizeFromLabFloat(ContextID)))
                goto Error;
        }
        else
        {
            if ((uint)PCS is cmsSigXYZData)
            {
                if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageNormalizeFromXyzFloat(ContextID)))
                    goto Error;
            }
        }

        return Lut;

    Error:
        cmsPipelineFree(Lut);
        return null;
    }

    internal static Pipeline? _cmsReadDevicelinkLUT(Profile Profile, uint Intent)
    {
        Pipeline? Lut;
        var ContextID = cmsGetProfileContextID(Profile);

        if (Intent > INTENT_ABSOLUTE_COLORIMETRIC)
            return null;

        var tag16 = Device2PCS16[Intent];
        var tagFloat = Device2PCSFloat[Intent];

        // On named color, take the appropriate tag
        if ((uint)cmsGetDeviceClass(Profile) is cmsSigNamedColorClass)
        {
            if (cmsReadTag(Profile, cmsSigNamedColor2Tag) is not BoxPtr<NamedColorList> nc) return null;

            Lut = cmsPipelineAlloc(ContextID, 0, 0);
            //if (Lut is null) goto Error;

            if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageAllocNamedColor(nc, false)) ||
                ((uint)cmsGetColorSpace(Profile) is cmsSigLabData &&
                 !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocLabV2ToV4(ContextID))))
            {
                goto Error;
            }

            return Lut;

        Error:
            cmsPipelineFree(Lut);
            cmsFreeNamedColorList(nc);
            return null;
        }

        if (cmsIsTag(Profile, tagFloat))   // Float tag takes precedence
        {
            // Floating point LUT are always V4
            return _cmsReadFloatDevicelinkTag(Profile, tagFloat);
        }

        tagFloat = Device2PCSFloat[0];
        if (cmsIsTag(Profile, tagFloat))
            return cmsPipelineDup(cmsReadTag(Profile, tagFloat) as Pipeline);

        if (!cmsIsTag(Profile, tag16))      // Is there any LUT-Based table?
        {
            tag16 = Device2PCS16[0];
            if (!cmsIsTag(Profile, tag16))
                return null;
        }

        // Check profile version and LUT type. Do the necessary adjustments if needed

        // Read the tag
        Lut = cmsReadTag(Profile, tag16) as Pipeline;
        if (Lut is null) return null;

        // The profile owns the Lut, so we need to copy it
        Lut = cmsPipelineDup(Lut);
        if (Lut is null) return null;

        // Now it is time for controversial stuff. I found that for 3D LUTS using
        // Lab used as indexer space, trilinear interpolation should be used
        if ((uint)cmsGetPCS(Profile) is cmsSigLabData)
            ChangeInterpolationToTrilinear(Lut);

        // After reading it, we have info about the original type
        var OriginalType = _cmsGetTagTrueType(Profile, tag16);

        // We need to adjust data only for Lab16 on output
        if ((uint)OriginalType is not cmsSigLut16Type)
            return Lut;
        // Here it is possible to get Lab on both sides
        if (((uint)cmsGetColorSpace(Profile) is cmsSigLabData &&
             !cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageAllocLabV4ToV2(ContextID))) ||
            ((uint)cmsGetPCS(Profile) is cmsSigLabData &&
             !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocLabV2ToV4(ContextID))))
        {
            goto Error2;
        }

        return Lut;
    Error2:
        cmsPipelineFree(Lut);
        return null;
    }

    public static bool cmsIsMatrixShaper(Profile Profile) =>
        (uint)cmsGetColorSpace(Profile) switch
        {
            cmsSigGrayData => cmsIsTag(Profile, cmsSigGrayTRCTag),
            cmsSigRgbData => cmsIsTag(Profile, cmsSigRedColorantTag) &&
                             cmsIsTag(Profile, cmsSigGreenColorantTag) &&
                             cmsIsTag(Profile, cmsSigBlueColorantTag) &&
                             cmsIsTag(Profile, cmsSigRedTRCTag) &&
                             cmsIsTag(Profile, cmsSigGreenTRCTag) &&
                             cmsIsTag(Profile, cmsSigBlueTRCTag),
            _ => false,
        };

    public static bool cmsIsCLUT(Profile Profile, uint Intent, uint UsedDirection)
    {
        Signature* TagTable;

        // For devicelinks, the supported intent is that one stated in the header
        if ((uint)cmsGetDeviceClass(Profile) is cmsSigLinkClass)
            return cmsGetHeaderRenderingIntent(Profile) == Intent;

        switch (UsedDirection)
        {
            case LCMS_USED_AS_INPUT: TagTable = Device2PCS16; break;
            case LCMS_USED_AS_OUTPUT: TagTable = PCS2Device16; break;

            // For proofing, we need rel. colorimetric in output. Let's do some recursion
            case LCMS_USED_AS_PROOF:
                return cmsIsIntentSupported(Profile, Intent, LCMS_USED_AS_INPUT) &&
                       cmsIsIntentSupported(Profile, INTENT_RELATIVE_COLORIMETRIC, LCMS_USED_AS_OUTPUT);

            default:
                cmsSignalError(cmsGetProfileContextID(Profile), cmsERROR_RANGE, $"Unexpected direction ({UsedDirection})");
                return false;
        }

        return cmsIsTag(Profile, TagTable[Intent]);
    }

    public static bool cmsIsIntentSupported(Profile Profile, uint Intent, uint UsedDirection) =>
        cmsIsCLUT(Profile, Intent, UsedDirection) ||
        // Is there any matrix-shaper? If so, the intent is supported. This is a bit odd, since V2 matrix shaper
        // does not fully support relative colorimetric because they cannot deal with non-zero black points, but
        // many profiles claims that, and this is certainly not true for V4 profiles. Lets answer "yes" no matter
        // the accuracy would be less than optimal in rel.col and v2 case.
        cmsIsMatrixShaper(Profile);

    internal static Sequence* _cmsReadProfileSequence(Profile Profile)
    {
        // Take profile sequence description first
        var ProfileSeq = cmsReadTag(Profile, cmsSigProfileSequenceDescTag) as BoxPtr<Sequence>;

        // Take profile sequence ID
        var ProfileID = cmsReadTag(Profile, cmsSigProfileSequenceIdTag) as BoxPtr<Sequence>;

        if (ProfileSeq is null && ProfileID is null) return null;

        if (ProfileSeq is null) return cmsDupProfileSequenceDescription(ProfileID!);
        if (ProfileID is null) return cmsDupProfileSequenceDescription(ProfileSeq);

        // We have to mix both together. For that they agree
        if (ProfileSeq.Ptr->n != ProfileID.Ptr->n) return cmsDupProfileSequenceDescription(ProfileSeq);

        var NewSeq = cmsDupProfileSequenceDescription(ProfileSeq);

        // Ok, proceed to the mixing
        if (NewSeq is not null)
        {
            for (var i = 0; i < ProfileSeq.Ptr->n; i++)
            {
                memmove(&NewSeq->seq[i].ProfileID, &ProfileID.Ptr->seq[i].ProfileID, _sizeof<ProfileID>());
                NewSeq->seq[i].Description = cmsMLUdup(ProfileID.Ptr->seq[i].Description);
            }
        }

        return NewSeq;
    }

    internal static bool _cmsWriteProfileSequence(Profile Profile, in Sequence* seq)
    {
        if (!cmsWriteTag(Profile, cmsSigProfileSequenceDescTag, new BoxPtr<Sequence>(seq))) return false;

        if (cmsGetEncodedICCVersion(Profile) >= 0x04000000)
            if (!cmsWriteTag(Profile, cmsSigProfileSequenceIdTag, new BoxPtr<Sequence>(seq))) return false;

        return true;
    }

    private static Mlu? GetMLUFromProfile(Profile h, Signature sig) => 
        (cmsReadTag(h, sig) is Mlu mlu)
            ? cmsMLUdup(mlu)
            : null;

    internal static Sequence* _cmsCompileProfileSequence(Context? ContextID, uint nProfiles, Profile[] Profiles)
    {
        var seq = cmsAllocProfileSequenceDescription(ContextID, nProfiles);

        if (seq is null) return null;

        for (var i = 0; i < nProfiles; i++)
        {
            var ps = &seq->seq[i];
            var h = Profiles[i];

            cmsGetHeaderAttributes(h, &ps->attributes);
            cmsGetHeaderProfileID(h, ps->ProfileID.id8);
            ps->deviceMfg = cmsGetHeaderManufacturer(h);
            ps->deviceModel = cmsGetHeaderModel(h);

            var techpt = cmsReadTag(h, cmsSigTechnologyTag) as BoxPtr<Signature>;
            ps->technology =
                techpt is not null
                    ? *techpt.Ptr
                    : 0;

            ps->Manufacturer = GetMLUFromProfile(h, cmsSigDeviceMfgDescTag);
            ps->Model = GetMLUFromProfile(h, cmsSigDeviceModelDescTag);
            ps->Description = GetMLUFromProfile(h, cmsSigProfileDescriptionTag);
        }

        return seq;
    }

    private static Mlu? GetInfo(Profile Profile, InfoType Info)
    {
        Signature sig = Info switch
        {
            InfoType.Description => cmsSigProfileDescriptionTag,
            InfoType.Manufacturer => cmsSigDeviceMfgDescTag,
            InfoType.Model => cmsSigDeviceModelDescTag,
            InfoType.Copyright => cmsSigCopyrightTag,
            _ => 0
        };
        return ((uint)sig is not 0)
            ? cmsReadTag(Profile, sig) as Mlu
            : null;
    }

    public static uint cmsGetProfileInfo(Profile Profile, InfoType Info, ReadOnlySpan<byte> LanguageCode, ReadOnlySpan<byte> CountryCode, Span<char> Buffer)
    {
        var mlu = GetInfo(Profile, Info);
        if (mlu is null) return 0;

        return cmsMLUgetWide(mlu, LanguageCode, CountryCode, Buffer);
    }

    public static uint cmsGetProfileInfoASCII(Profile Profile, InfoType Info, ReadOnlySpan<byte> LanguageCode, ReadOnlySpan<byte> CountryCode, Span<byte> Buffer)
    {
        var mlu = GetInfo(Profile, Info);
        if (mlu is null) return 0;

        return cmsMLUgetASCII(mlu, LanguageCode, CountryCode, Buffer);
    }
}
