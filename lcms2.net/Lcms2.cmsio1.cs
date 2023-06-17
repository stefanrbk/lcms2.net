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

    internal static readonly double* GrayInputMatrix;
    internal static readonly double* OneToThreeInputMatrix;
    internal static readonly double* PickYMatrix;
    internal static readonly double* PickLstarMatrix;

    internal const double InpAdj = 1 / MAX_ENCODEABLE_XYZ;
    internal const double OutpAdj = MAX_ENCODEABLE_XYZ;

    internal static bool _cmsReadMediaWhitePoint(CIEXYZ* Dest, HPROFILE hProfile)
    {
        _cmsAssert(Dest);

        var Tag = (CIEXYZ*)cmsReadTag(hProfile, cmsSigMediaWhitePointTag);

        // If no wp, take D50
        if (Tag is null)
        {
            *Dest = *cmsD50_XYZ();
            return true;
        }

        // V2 display profiles should give D50
        if (cmsGetEncodedICCVersion(hProfile) < 0x04000000)
        {
            if (cmsGetDeviceClass(hProfile) == cmsSigDisplayClass)
            {
                *Dest = *cmsD50_XYZ();
                return true;
            }
        }

        // All seems ok
        *Dest = *Tag;
        return true;
    }

    internal static bool _cmsReadCHAD(MAT3* Dest, HPROFILE hProfile)
    {
        _cmsAssert(Dest);

        var Tag = (MAT3*)cmsReadTag(hProfile, cmsSigChromaticAdaptationTag);

        if (Tag is not null)
        {
            *Dest = *Tag;
            return true;
        }

        // No CHAD available, default it to identity
        _cmsMAT3identity(out *Dest);

        // V2 display profiles should give D50
        if (cmsGetEncodedICCVersion(hProfile) < 0x04000000)
        {
            if ((uint)cmsGetDeviceClass(hProfile) is cmsSigDisplayClass)
            {
                var White = (CIEXYZ*)cmsReadTag(hProfile, cmsSigMediaWhitePointTag);

                if (White is null)
                {
                    _cmsMAT3identity(out *Dest);
                    return true;
                }
                return _cmsAdaptationMatrix(Dest, null, White, cmsD50_XYZ());
            }
        }

        return true;
    }

    private static bool ReadIccMatrixRGB2XYZ(MAT3* r, HPROFILE hProfile)
    {
        _cmsAssert(r);

        var PtrRed = (CIEXYZ*)cmsReadTag(hProfile, cmsSigRedColorantTag);
        var PtrGreen = (CIEXYZ*)cmsReadTag(hProfile, cmsSigGreenColorantTag);
        var PtrBlue = (CIEXYZ*)cmsReadTag(hProfile, cmsSigBlueColorantTag);

        if (PtrRed is null || PtrGreen is null || PtrBlue is null)
            return false;

        _cmsVEC3init(out r->X, PtrRed->X, PtrGreen->X, PtrBlue->X);
        _cmsVEC3init(out r->Y, PtrRed->Y, PtrGreen->Y, PtrBlue->Y);
        _cmsVEC3init(out r->Z, PtrRed->Z, PtrGreen->Z, PtrBlue->Z);

        return true;
    }

    private static Pipeline* BuildGrayInputMatrixPipeline(HPROFILE hProfile)
    {
        var ContextID = cmsGetProfileContextID(hProfile);

        var GrayTRC = (ToneCurve*)cmsReadTag(hProfile, cmsSigGrayTRCTag);
        if (GrayTRC is null) return null;

        var Lut = cmsPipelineAlloc(ContextID, 1, 3);
        if (Lut is null) goto Error;

        if ((uint)cmsGetPCS(hProfile) is cmsSigLabData)
        {
            // In this case we implement the profile as an identity matrix plus 3 tone curves
            var Zero = stackalloc ushort[] { 0x8080, 0x8080 };
            var LabCurves = stackalloc ToneCurve*[3];

            var EmptyTab = cmsBuildTabulatedToneCurve16(ContextID, 2, Zero);

            if (EmptyTab is null) goto Error;

            LabCurves[0] = GrayTRC;
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
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, 1, &GrayTRC)) ||
                !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 3, 1, GrayInputMatrix, null)))
            {
                goto Error;
            }
        }

        return Lut;

    Error:
        cmsPipelineFree(Lut);
        return null;
    }

    private static Pipeline* BuildRGBInputMatrixShaper(HPROFILE hProfile)
    {
        var Shapes = stackalloc ToneCurve*[3];
        var ContextID = cmsGetProfileContextID(hProfile); ;
        MAT3 Mat;
        VEC3* Matv = &Mat.X;

        if (!ReadIccMatrixRGB2XYZ(&Mat, hProfile)) return null;

        // XYZ PCS in encoded in 1.15 format, and the matrix output comes in 0..0xffff range, so
        // we need to adjust the output by a factor of (0x10000/0xffff) to put data in
        // a 1.16 range, and then a >> 1 to obtain 1.15. The total factor is (65536.0)/(65535.0*2)

        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                (&Matv[i].X)[j] *= InpAdj;

        Shapes[0] = (ToneCurve*)cmsReadTag(hProfile, cmsSigRedTRCTag);
        Shapes[1] = (ToneCurve*)cmsReadTag(hProfile, cmsSigGreenTRCTag);
        Shapes[2] = (ToneCurve*)cmsReadTag(hProfile, cmsSigBlueTRCTag);

        if (Shapes[0] is null || Shapes[1] is null || Shapes[2] is null)
            return null;

        var Lut = cmsPipelineAlloc(ContextID, 3, 3);
        if (Lut is null) return null;

        if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, 3, Shapes)) ||
            !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 3, 3, (double*)&Mat, null)))
        {
            goto Error;
        }

        // Note that it is certainly possible a single profile would have a LUT based
        // tag for output working in lab and a matrix-shaper for the fallback cases.
        // This is not allowed by the spec, but this code is tolerant to those cases
        if ((uint)cmsGetPCS(hProfile) is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocXYZ2Lab(ContextID)))
                goto Error;
        }

        return Lut;

    Error:
        cmsPipelineFree(Lut);
        return null;
    }

    internal static Pipeline* _cmsReadFloatInputTag(HPROFILE hProfile, Signature tagFloat)
    {
        var ContextID = cmsGetProfileContextID(hProfile);
        var Lut = cmsPipelineDup((Pipeline*)cmsReadTag(hProfile, tagFloat));
        var spc = cmsGetColorSpace(hProfile);
        var PCS = cmsGetPCS(hProfile);

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

    internal static Pipeline* _cmsReadInputLUT(HPROFILE hProfile, uint Intent)
    {
        var ContextID = cmsGetProfileContextID(hProfile);

        // On named color, take the appropriate tag
        if ((uint)cmsGetDeviceClass(hProfile) is cmsSigNamedColorClass)
        {
            var nc = (NamedColorList*)cmsReadTag(hProfile, cmsSigNamedColor2Tag);

            if (nc is null) return null;

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

            if (cmsIsTag(hProfile, tagFloat))   // Float tag takes precedence
            {
                // Floating point LUT are always V4, but the encoding range is no
                // longer 0..1.0, so we need to add an stage depending on the color space
                return _cmsReadFloatInputTag(hProfile, tagFloat);
            }

            // Revert to perceptual if no tag is found
            if (!cmsIsTag(hProfile, tag16))
                tag16 = Device2PCS16[0];

            if (cmsIsTag(hProfile, tag16)) // Is there any LUT-Based table?
            {
                // Check profile version and LUT type. Do the necessaary adjustments if needed

                // First read the tag
                var Lut = (Pipeline*)cmsReadTag(hProfile, tag16);
                if (Lut is null) return null;

                // After reading it, we have the info about the original type
                var OriginalType = _cmsGetTagTrueType(hProfile, tag16);

                // The profile owns the Lut, so we need to copy it
                Lut = cmsPipelineDup(Lut);

                // We need to adjust data only for Lab16 on output
                if ((uint)OriginalType is not cmsSigLut16Type || (uint)cmsGetPCS(hProfile) is not cmsSigLabData)
                    return Lut;

                // If the input is Lab, add also a conversion at the begin
                if ((uint)cmsGetColorSpace(hProfile) is cmsSigLabData &&
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
        if ((uint)cmsGetColorSpace(hProfile) is cmsSigGrayData)
            // if so, build appropriate conversion tables.
            // The tables are the PCS iluminant, scaled across GrayTRC
            return BuildGrayInputMatrixPipeline(hProfile);

        // Not gray, create a normal matrix-shaper
        return BuildRGBInputMatrixShaper(hProfile);
    }

    private static Pipeline* BuildGrayOutputPipeline(HPROFILE hProfile)
    {
        var ContextID = cmsGetProfileContextID(hProfile);

        var GrayTRC = (ToneCurve*)cmsReadTag(hProfile, cmsSigGrayTRCTag);
        if (GrayTRC is null) return null;

        var RevGrayTRC = cmsReverseToneCurve(GrayTRC);
        if (RevGrayTRC is null) return null;

        var Lut = cmsPipelineAlloc(ContextID, 3, 1);
        if (Lut is null) goto Error1;

        if ((uint)cmsGetPCS(hProfile) is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 1, 3, PickLstarMatrix, null)))
                goto Error2;
        }
        else
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 1, 3, PickYMatrix, null)))
                goto Error2;
        }

        if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, 1, &RevGrayTRC)))
            goto Error2;

        cmsFreeToneCurve(RevGrayTRC);
        return Lut;

    Error2:
        cmsPipelineFree(Lut);
    Error1:
        cmsFreeToneCurve(RevGrayTRC);
        return null;
    }

    private static Pipeline* BuildRGBOutputMatrixShaper(HPROFILE hProfile)
    {
        var Shapes = stackalloc ToneCurve*[3];
        var InvShapes = stackalloc ToneCurve*[3];
        MAT3 Mat, Inv;
        VEC3* Invv = &Inv.X;

        var ContextID = cmsGetProfileContextID(hProfile);

        if (!ReadIccMatrixRGB2XYZ(&Mat, hProfile))
            return null;

        if (!_cmsMAT3inverse(Mat, out Inv))
            return null;

        // XYZ PCS in encoded in 1.15 format, and the matrix input should come in 0..0xffff range, so
        // we need to adjust the input by a << 1 to obtain a 1.16 fixed and then by a factor of
        // (0xffff/0x10000) to put data in 0..0xffff range. Total factor is (2.0*65535.0)/65536.0;

        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                (&Invv[i].X)[j] *= OutpAdj;

        Shapes[0] = (ToneCurve*)cmsReadTag(hProfile, cmsSigRedTRCTag);
        Shapes[1] = (ToneCurve*)cmsReadTag(hProfile, cmsSigGreenTRCTag);
        Shapes[2] = (ToneCurve*)cmsReadTag(hProfile, cmsSigBlueTRCTag);

        if (Shapes[0] is null || Shapes[1] is null || Shapes[2] is null)
            return null;

        InvShapes[0] = cmsReverseToneCurve(Shapes[0]);
        InvShapes[1] = cmsReverseToneCurve(Shapes[1]);
        InvShapes[2] = cmsReverseToneCurve(Shapes[2]);

        if (InvShapes[0] is null || InvShapes[1] is null || InvShapes[2] is null)
            return null;

        var Lut = cmsPipelineAlloc(ContextID, 3, 3);
        if (Lut is null) goto Error1;

        // Note that it is certainly possible a single profile would have a LUT based
        // tag for output working in lab and a matrix-shaper for the fallback cases.
        // This is not allowed by the spec, but this code is tolerant to those cases

        if ((uint)cmsGetPCS(hProfile) is cmsSigLabData)
        {
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocLab2XYZ(ContextID)))
                goto Error2;
        }

        if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocMatrix(ContextID, 3, 3, (double*)&Inv, null)) ||
            !cmsPipelineInsertStage(Lut, StageLoc.AtEnd, cmsStageAllocToneCurves(ContextID, 3, InvShapes)))
            goto Error2;

        cmsFreeToneCurveTriple(InvShapes);
        return Lut;

    Error2:
        cmsPipelineFree(Lut);
    Error1:
        cmsFreeToneCurveTriple(InvShapes);
        return null;
    }

    private static void ChangeInterpolationToTrilinear(Pipeline* Lut)
    {
        for (var Stage = cmsPipelineGetPtrToFirstStage(Lut);
             Stage is not null;
             Stage = cmsStageNext(Stage))
        {
            if ((uint)cmsStageType(Stage) is cmsSigCLutElemType)
            {
                var CLUT = (StageCLutData)Stage->Data!;

                CLUT.Params->dwFlags |= (uint)LerpFlag.Trilinear;
                _cmsSetInterpolationRoutine(Lut->ContextID, CLUT.Params);
            }
        }
    }

    internal static Pipeline* _cmsReadFloatOutputTag(HPROFILE hProfile, Signature tagFloat)
    {
        var ContextID = cmsGetProfileContextID(hProfile);
        var Lut = cmsPipelineDup((Pipeline*)cmsReadTag(hProfile, tagFloat));
        var PCS = cmsGetPCS(hProfile);
        var dataSpace = cmsGetColorSpace(hProfile);

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

    internal static Pipeline* _cmsReadOutputLUT(HPROFILE hProfile, uint Intent)
    {
        var ContextID = cmsGetProfileContextID(hProfile);

        if (Intent <= INTENT_ABSOLUTE_COLORIMETRIC)
        {
            var tag16 = PCS2Device16[Intent];
            var tagFloat = PCS2DeviceFloat[Intent];

            if (cmsIsTag(hProfile, tagFloat))   // Float tag takes precedence
                // Floating point LUT are always V4
                return _cmsReadFloatOutputTag(hProfile, tagFloat);

            // Revert to perceptual if no tag is found
            if (!cmsIsTag(hProfile, tag16))
                tag16 = PCS2Device16[0];

            if (cmsIsTag(hProfile, tag16))      // Is there any LUT-Based table?
            {
                // Check profile version and LUT type. Do the necessary adjustments if needed

                // First read the tag
                var Lut = (Pipeline*)cmsReadTag(hProfile, tag16);
                if (Lut is null) return null;

                // After reading it, we have info about the original type
                var OriginalType = _cmsGetTagTrueType(hProfile, tag16);

                // The profile owns the Lut, so we need to copy it
                Lut = cmsPipelineDup(Lut);
                if (Lut is null) return null;

                // Now it is time for controversial stuff. I found that for 3D LUTS using
                // Lab used as indexer space, trilinear interpolation should be used
                if ((uint)cmsGetPCS(hProfile) is cmsSigLabData)
                    ChangeInterpolationToTrilinear(Lut);

                // We need to adjust data only for Lab and Lut16 type
                if ((uint)OriginalType is not cmsSigLut16Type || (uint)cmsGetPCS(hProfile) is not cmsSigLabData)
                    return Lut;

                // Add a matrix for conversion V4 to V3 Lab PCS
                if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageAllocLabV4ToV2(ContextID)))
                    goto Error;

                // If the output is Lab, add also a conversion at the end
                if ((uint)cmsGetColorSpace(hProfile) is cmsSigLabData)
                    if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocLabV2ToV4(ContextID)))
                        goto Error;

                return Lut;
            Error:
                cmsPipelineFree(Lut);
                return null;
            }
        }

        // Lut not found, try to create a matrix-shaper

        // Check if this is a grayscale profile.
        if ((uint)cmsGetColorSpace(hProfile) is cmsSigGrayData)
            // if so, build appropriate conversion tables.
            // The tables are the PCS iluminant, scaled across GrayTRC
            return BuildGrayOutputPipeline(hProfile);

        // Not gray, create a normal matrix-shaper, which only operates in XYZ space
        return BuildRGBOutputMatrixShaper(hProfile);
    }

    internal static Pipeline* _cmsReadFloatDevicelinkTag(HPROFILE hProfile, Signature tagFloat)
    {
        var ContextID = cmsGetProfileContextID(hProfile);
        var Lut = cmsPipelineDup((Pipeline*)cmsReadTag(hProfile, tagFloat));
        var PCS = cmsGetPCS(hProfile);
        var spc = cmsGetColorSpace(hProfile);

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

    internal static Pipeline* _cmsReadDevicelinkLUT(HPROFILE hProfile, uint Intent)
    {
        Pipeline* Lut;
        var ContextID = cmsGetProfileContextID(hProfile);

        if (Intent > INTENT_ABSOLUTE_COLORIMETRIC)
            return null;

        var tag16 = Device2PCS16[Intent];
        var tagFloat = Device2PCSFloat[Intent];

        // On named color, take the appropriate tag
        if ((uint)cmsGetDeviceClass(hProfile) is cmsSigNamedColorClass)
        {
            var nc = (NamedColorList*)cmsReadTag(hProfile, cmsSigNamedColor2Tag);
            if (nc is null) return null;

            Lut = cmsPipelineAlloc(ContextID, 0, 0);
            if (Lut is null) goto Error;

            if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageAllocNamedColor(nc, false)))
                goto Error;

            if ((uint)cmsGetColorSpace(hProfile) is cmsSigLabData)
                if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocLabV2ToV4(ContextID)))
                    goto Error;

            return Lut;

        Error:
            cmsPipelineFree(Lut);
            cmsFreeNamedColorList(nc);
            return null;
        }

        if (cmsIsTag(hProfile, tagFloat))   // Float tag takes precedence
        {
            // Floating point LUT are always V4
            return _cmsReadFloatDevicelinkTag(hProfile, tagFloat);
        }

        tagFloat = Device2PCSFloat[0];
        if (cmsIsTag(hProfile, tagFloat))
            return cmsPipelineDup((Pipeline*)cmsReadTag(hProfile, tagFloat));

        if (!cmsIsTag(hProfile, tag16))      // Is there any LUT-Based table?
        {
            tag16 = Device2PCS16[0];
            if (!cmsIsTag(hProfile, tag16))
                return null;
        }

        // Check profile version and LUT type. Do the necessary adjustments if needed

        // Read the tag
        Lut = (Pipeline*)cmsReadTag(hProfile, tag16);
        if (Lut is null) return null;

        // The profile owns the Lut, so we need to copy it
        Lut = cmsPipelineDup(Lut);
        if (Lut is null) return null;

        // Now it is time for controversial stuff. I found that for 3D LUTS using
        // Lab used as indexer space, trilinear interpolation should be used
        if ((uint)cmsGetPCS(hProfile) is cmsSigLabData)
            ChangeInterpolationToTrilinear(Lut);

        // After reading it, we have info about the original type
        var OriginalType = _cmsGetTagTrueType(hProfile, tag16);

        // We need to adjust data only for Lab16 on output
        if ((uint)OriginalType is not cmsSigLut16Type)
            return Lut;
        // Here it is possible to get Lab on both sides
        if ((uint)cmsGetColorSpace(hProfile) is cmsSigLabData)
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtBegin, _cmsStageAllocLabV4ToV2(ContextID)))
                goto Error2;

        if ((uint)cmsGetPCS(hProfile) is cmsSigLabData)
            if (!cmsPipelineInsertStage(Lut, StageLoc.AtEnd, _cmsStageAllocLabV2ToV4(ContextID)))
                goto Error2;

        return Lut;
    Error2:
        cmsPipelineFree(Lut);
        return null;
    }

    public static bool cmsIsMatrixShaper(HPROFILE hProfile) =>
        (uint)cmsGetColorSpace(hProfile) switch
        {
            cmsSigGrayData => cmsIsTag(hProfile, cmsSigGrayTRCTag),
            cmsSigRgbData => cmsIsTag(hProfile, cmsSigRedColorantTag) &&
                             cmsIsTag(hProfile, cmsSigGreenColorantTag) &&
                             cmsIsTag(hProfile, cmsSigBlueColorantTag) &&
                             cmsIsTag(hProfile, cmsSigRedTRCTag) &&
                             cmsIsTag(hProfile, cmsSigGreenTRCTag) &&
                             cmsIsTag(hProfile, cmsSigBlueTRCTag),
            _ => false,
        };

    public static bool cmsIsCLUT(HPROFILE hProfile, uint Intent, uint UsedDirection)
    {
        Signature* TagTable;

        // For devicelinks, the supported intent is that one stated in the header
        if ((uint)cmsGetDeviceClass(hProfile) is cmsSigLinkClass)
            return cmsGetHeaderRenderingIntent(hProfile) == Intent;

        switch (UsedDirection)
        {
            case LCMS_USED_AS_INPUT: TagTable = Device2PCS16; break;
            case LCMS_USED_AS_OUTPUT: TagTable = PCS2Device16; break;

            // For proofing, we need rel. colorimetric in output. Let's do some recursion
            case LCMS_USED_AS_PROOF:
                return cmsIsIntentSupported(hProfile, Intent, LCMS_USED_AS_INPUT) &&
                       cmsIsIntentSupported(hProfile, INTENT_RELATIVE_COLORIMETRIC, LCMS_USED_AS_OUTPUT);

            default:
                cmsSignalError(cmsGetProfileContextID(hProfile), cmsERROR_RANGE, $"Unexpected direction ({UsedDirection})");
                return false;
        }

        return cmsIsTag(hProfile, TagTable[Intent]);
    }

    public static bool cmsIsIntentSupported(HPROFILE hProfile, uint Intent, uint UsedDirection) =>
        cmsIsCLUT(hProfile, Intent, UsedDirection) ||
        // Is there any matrix-shaper? If so, the intent is supported. This is a bit odd, since V2 matrix shaper
        // does not fully support relative colorimetric because they cannot deal with non-zero black points, but
        // many profiles claims that, and this is certainly not true for V4 profiles. Lets answer "yes" no matter
        // the accuracy would be less than optimal in rel.col and v2 case.
        cmsIsMatrixShaper(hProfile);

    internal static Sequence* _cmsReadProfileSequence(HPROFILE hProfile)
    {
        // Take profile sequence description first
        var ProfileSeq = (Sequence*)cmsReadTag(hProfile, cmsSigProfileSequenceDescTag);

        // Take profile sequence ID
        var ProfileID = (Sequence*)cmsReadTag(hProfile, cmsSigProfileSequenceIdTag);

        if (ProfileSeq is null && ProfileID is null) return null;

        if (ProfileSeq is null) return cmsDupProfileSequenceDescription(ProfileID);
        if (ProfileID is null) return cmsDupProfileSequenceDescription(ProfileSeq);

        // We have to mix both together. For that they agree
        if (ProfileSeq->n != ProfileID->n) return cmsDupProfileSequenceDescription(ProfileSeq);

        var NewSeq = cmsDupProfileSequenceDescription(ProfileSeq);

        // Ok, proceed to the mixing
        if (NewSeq is not null)
        {
            for (var i = 0; i < ProfileSeq->n; i++)
            {
                memmove(&NewSeq->seq[i].ProfileID, &ProfileID->seq[i].ProfileID, sizeof(ProfileID));
                NewSeq->seq[i].Description = cmsMLUdup(ProfileID->seq[i].Description);
            }
        }

        return NewSeq;
    }

    internal static bool _cmsWriteProfileSequence(HPROFILE hProfile, in Sequence* seq)
    {
        if (!cmsWriteTag(hProfile, cmsSigProfileSequenceDescTag, seq)) return false;

        if (cmsGetEncodedICCVersion(hProfile) >= 0x04000000)
            if (!cmsWriteTag(hProfile, cmsSigProfileSequenceIdTag, seq)) return false;

        return true;
    }

    private static Mlu* GetMLUFromProfile(HPROFILE h, Signature sig)
    {
        var mlu = (Mlu*)cmsReadTag(h, sig);
        if (mlu is null) return null;

        return cmsMLUdup(mlu);
    }

    internal static Sequence* _cmsCompileProfileSequence(Context? ContextID, uint nProfiles, HPROFILE* hProfiles)
    {
        var seq = cmsAllocProfileSequenceDescription(ContextID, nProfiles);

        if (seq is null) return null;

        for (var i = 0; i < nProfiles; i++)
        {
            var ps = &seq->seq[i];
            var h = hProfiles[i];

            cmsGetHeaderAttributes(h, &ps->attributes);
            cmsGetHeaderProfileID(h, ps->ProfileID.id8);
            ps->deviceMfg = cmsGetHeaderManufacturer(h);
            ps->deviceModel = cmsGetHeaderModel(h);

            var techpt = (Signature*)cmsReadTag(h, cmsSigTechnologyTag);
            ps->technology =
                techpt is not null
                    ? *techpt
                    : 0;

            ps->Manufacturer = GetMLUFromProfile(h, cmsSigDeviceMfgDescTag);
            ps->Model = GetMLUFromProfile(h, cmsSigDeviceModelDescTag);
            ps->Description = GetMLUFromProfile(h, cmsSigProfileDescriptionTag);
        }

        return seq;
    }

    private static Mlu* GetInfo(Profile* hProfile, InfoType Info)
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
            ? (Mlu*)cmsReadTag(hProfile, sig)
            : null;
    }

    public static uint cmsGetProfileInfo(Profile* hProfile, InfoType Info, in byte* LanguageCode, in byte* CountryCode, char* Buffer, uint BufferSize)
    {
        var mlu = GetInfo(hProfile, Info);
        if (mlu is null) return 0;

        return cmsMLUgetWide(mlu, LanguageCode, CountryCode, Buffer, BufferSize);
    }

    public static uint cmsGetProfileInfoASCII(Profile* hProfile, InfoType Info, in byte* LanguageCode, in byte* CountryCode, byte* Buffer, uint BufferSize)
    {
        var mlu = GetInfo(hProfile, Info);
        if (mlu is null) return 0;

        return cmsMLUgetASCII(mlu, LanguageCode, CountryCode, Buffer, BufferSize);
    }
}
