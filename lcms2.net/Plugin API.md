# Lcms2 API

## Table of Contents

- [Plugin API](#plugin-api)
  - [Structs/Typedefs](#structs/typedefs)
    - [cmsInterpFnFactory ⇆ InterpFnFactory](#cmsinterpfnfactory-⇆-interpfnfactory)
    - [cmsInterpFunction ⇆ InterpFunction](#cmsinterpfunction-⇆-interpfunction)
- [Public API](#public-api)
  - [Enums](#enums)
    - [cmsColorSpaceSignature](#cmscolorspacesignature)
    - [cmsCurveSegSignature](#cmscurvesegsignature)
    - [cmsPlatformSignature](#cmsplatformsignature)
    - [cmsStageLoc](#cmsstageloc)
    - [cmsStageSignature](#cmsstagesignature)
    - [cmsTagSignature](#cmstagsignature)
    - [cmsTagTypeSignature](#cmstagtypesignature)
    - [cmsTechnologySignature](#cmstechnologysignature)
  - [Functions](#functions)
    - [cmsAdaptToIlluminant](#cmsadapttoilluminant)
    - [cmsBFDdeltaE](#cmsbfddeltae)
    - [cmsBuildGamma](#cmsbuildgamma)
    - [cmsBuildParametricToneCurve](#cmsbuildparametrictonecurve)
    - [cmsBuildSegmentedToneCurve](#cmsbuildsegmentedtonecurve)
    - [cmsBuildTabulatedToneCurveFloat](#cmsbuildtabulatedtonecurvefloat)
    - [cmsCIE2000DeltaE](#cmscie2000deltae)
    - [cmsCIE94DeltaE](#cmscie94deltae)
    - [cmsCIECAM02Done](#cmsciecam02done)
    - [cmsCIECAM02Forward](#cmsciecam02forward)
    - [cmsCIECAM02Init](#cmsciecam02init)
    - [cmsCIECAM02Reverse](#cmsciecam02reverse)
    - [cmsCMCdeltaE](#cmscmcdeltae)
    - [cmsCreateContext](#cmscreatecontext)
    - [cmsD50_xyY](#cmsd50_xyy)
    - [cmsD50_XYZ](#cmsd50_xyz)
    - [cmsDeleteContext](#cmsdeletecontext)
    - [cmsDeltaE](#cmsdeltae)
    - [cmsDupContext](#cmsdupcontext)
    - [cmsDupToneCurve](#cmsduptonecurve)
    - [cmsEstimateGamma](#cmsestimategamma)
    - [cmsEvalToneCurve16](#cmsevaltonecurve16)
    - [cmsEvalToneCurveFloat](#cmsevaltonecurvefloat)
    - [cmsfilelength](#cmsfilelength)
    - [cmsFloat2LabEncoded](#cmsfloat2labencoded)
    - [cmsFloat2LabEncodedV2](#cmsfloat2labencodedv2)
    - [cmsFloat2XYZEncoded](#cmsfloat2xyzencoded)
    - [cmsFreeToneCurve](#cmsfreetonecurve)
    - [cmsFreeToneCurveTriple](#cmsfreetonecurvetriple)
    - [cmsGetContextUserData](#cmsgetcontextuserdata)
    - [cmsGetEncodedCMMversion](#cmsgetencodedcmmversion)
    - [cmsGetPipelineContextID](#cmsgetpipelinecontextid)
    - [cmsGetStageContextID](#cmsgetstagecontextid)
    - [cmsGetToneCurveEstimatedTable](#cmsgettonecurveestimatedtable)
    - [cmsGetToneCurveEstimatedTableEntries](#cmsgettonecurveestimatedtableentries)
    - [cmsGetToneCurveParametricType](#cmsgettonecurveparametrictype)
    - [cmsGetToneCurveParams](#cmsgettonecurveparams)
    - [cmsIsToneCurveDescending](#cmsistonecurvedescending)
    - [cmsIsToneCurveLinear](#cmsistonecurvelinear)
    - [cmsIsToneCurveMonotonic](#cmsistonecurvemonotonic)
    - [cmsIsToneCurveMultisegment](#cmsistonecurvemultisegment)
    - [cmsJoinToneCurve](#cmsjointonecurve)
    - [cmsLab2LCh](#cmslab2lch)
    - [cmsLab2XYZ](#cmslab2xyz)
    - [cmsLabEncoded2Float](#cmslabencoded2float)
    - [cmsLabEncoded2FloatV2](#cmslabencoded2floatv2)
    - [cmsLCh2Lab](#cmslch2lab)
    - [cmsPipelineAlloc](#cmspipelinealloc)
    - [cmsPipelineCat](#cmspipelinecat)
    - [cmsPipelineCheckAndRetreiveStages](#cmspipelinecheckandretreivestages)
    - [cmsPipelineDup](#cmspipelinedup)
    - [cmsPipelineEval16](#cmspipelineeval16)
    - [cmsPipelineEvalFloat](#cmspipelineevalfloat)
    - [cmsPipelineEvalReverseFloat](#cmspipelineevalreversefloat)
    - [cmsPipelineFree](#cmspipelinefree)
    - [cmsPipelineGetPtrToFirstStage](#cmspipelinegetptrtofirststage)
    - [cmsPipelineGetPtrToLastStage](#cmspipelinegetptrtolaststage)
    - [cmsPipelineInputChannels](#cmspipelineinputchannels)
    - [cmsPipelineInsertStage](#cmspipelineinsertstage)
    - [cmsPipelineOutputChannels](#cmspipelineoutputchannels)
    - [cmsPipelineSetSaveAs8bitsFlag](#cmspipelinesetsaveas8bitsflag)
    - [cmsPipelineStageCount](#cmspipelinestagecount)
    - [cmsPipelineUnlinkStage](#cmspipelineunlinkstage)
    - [cmsPlugin](#cmsplugin)
    - [cmsPluginTHR](#cmspluginthr)
    - [cmsReverseToneCurve](#cmsreversetonecurve)
    - [cmsReverseToneCurveEx](#cmsreversetonecurveex)
    - [cmsSetLogErrorHandler](#cmssetlogerrorhandler)
    - [cmsSetLogErrorHandlerTHR](#cmssetlogerrorhandlerthr)
    - [cmsSliceSpace16](#cmsslicespace16)
    - [cmsSliceSpaceFloat](#cmsslicespacefloat)
    - [cmsSmoothToneCurve](#cmssmoothtonecurve)
    - [cmsStageAllocCLut16bit](#cmsstageallocclut16bit)
    - [cmsStageAllocCLut16bitGranular](#cmsstageallocclut16bitgranular)
    - [cmsStageAllocCLutFloat](#cmsstageallocclutfloat)
    - [cmsStageAllocCLutFloatGranular](#cmsstageallocclutfloatgranular)
    - [cmsStageAllocIdentity](#cmsstageallocidentity)
    - [cmsStageAllocMatrix](#cmsstageallocmatrix)
    - [cmsStageAllocToneCurves](#cmsstagealloctonecurves)
    - [cmsStageData](#cmsstagedata)
    - [cmsStageDup](#cmsstagedup)
    - [cmsStageFree](#cmsstagefree)
    - [cmsStageInputChannels](#cmsstageinputchannels)
    - [cmsStageNext](#cmsstagenext)
    - [cmsStageOutputChannels](#cmsstageoutputchannels)
    - [cmsStageSampleCLut16bit](#cmsstagesampleclut16bit)
    - [cmsStageSampleCLutFloat](#cmsstagesampleclutfloat)
    - [cmsStageType](#cmsstagetype)
    - [cmsstrcasecmp](#cmsstrcasecmp)
    - [cmsTempFromWhitePoint](#cmstempfromwhitepoint)
    - [cmsUnregisterPlugins](#cmsunregisterplugins)
    - [cmsUnregisterPluginsTHR](#cmsunregisterpluginsthr)
    - [cmsWhitePointFromTemp](#cmswhitepointfromtemp)
    - [cmsxyY2XYZ](#cmsxyy2xyz)
    - [cmsXYZ2xyY](#cmsxyz2xyy)
    - [cmsXYZEncoded2Float](#cmsxyzencoded2float)
  - [Structs/Typedefs](#structs/typedefs)
    - [_cms_curve_struct](#_cms_curve_struct)
    - [_cmsContext_struct](#_cmscontext_struct)
    - [_cmsPipeline_struct](#_cmspipeline_struct)
    - [_cmsStage_struct](#_cmsstage_struct)
    - [cmsCIELab](#cmscielab)
    - [cmsCIELCh](#cmscielch)
    - [cmsCIExyY](#cmsciexyy)
    - [cmsCIExyYTRIPLE](#cmsciexyytriple)
    - [cmsCIEXYZ](#cmsciexyz)
    - [cmsCIEXYZTRIPLE](#cmsciexyztriple)
    - [cmsContext](#cmscontext)
    - [cmsCurveSegment](#cmscurvesegment)
    - [cmsDateTimeNumber](#cmsdatetimenumber)
    - [cmsEncodedXYZNumber](#cmsencodedxyznumber)
    - [cmsICCData](#cmsiccdata)
    - [cmsICCHeader](#cmsiccheader)
    - [cmsICCMeasurementConditions](#cmsiccmeasurementconditions)
    - [cmsICCViewingConditions](#cmsiccviewingconditions)
    - [cmsJCh](#cmsjch)
    - [cmsLogErrorHandlerFunction](#cmslogerrorhandlerfunction)
    - [cmsPipeline](#cmspipeline)
    - [cmsProfileID](#cmsprofileid)
    - [cmsS15Fixed16Number](#cmss15fixed16number)
    - [cmsSAMPLER16](#cmssampler16)
    - [cmsSAMPLERFLOAT](#cmssamplerfloat)
    - [cmsSignature](#cmssignature)
    - [cmsStage](#cmsstage)
    - [cmsTagBase](#cmstagbase)
    - [cmsTagEntry](#cmstagentry)
    - [cmsToneCurve](#cmstonecurve)
    - [cmsU16Fixed16Number](#cmsu16fixed16number)
    - [cmsU8Fixed8Number](#cmsu8fixed8number)
    - [cmsViewingConditions](#cmsviewingconditions)

## Plugin API

### Structs/Typedefs

#### cmsInterpFnFactory ⇆ InterpFnFactory

```c
typedef cmsInterpFunction *cmsInterpFnFactory(cmsUInt32Number nInputChannels, cmsUInt32Number nOutputChannels, cmsUInt32Number dwFlags);
```
`namespace lcms2.plugins`
```csharp
public delegate InterpFunction InterpFnFactory(int numInputChannels, int numOutputChannels, LerpFlag flags);
```
---

#### cmsInterpFunction ⇆ InterpFunction

```c
typedef union {
    _cmsInterpFn16       Lerp16;
    _cmsInterpFnFloat    LerpFloat;
} cmsInterpFunction;
```
`namespace lcms2.plugins`
```csharp
[StructLayout(LayoutKind.Explicit)]
public struct InterpFunction
{
    [FieldOffset(0)]
    public InterpFn16 Lerp16;
    [FieldOffset(0)]
    public InterpFnFloat LerpFloat;
}
```
## Public API

### Enums

#### cmsColorSpaceSignature

```c
typedef enum {
    ...
} cmsColorSpaceSignature;
```
---

#### cmsCurveSegSignature

```c
typedef enum {
    ...
} cmsCurveSegSignature;
```
---

#### cmsPlatformSignature

```c
typedef enum {
    ...
} cmsPlatformSignature;
```
---

#### cmsStageLoc

```c
typedef enum { cmsAT_BEGIN, cmsAT_END } cmsStageLoc;
```
---

#### cmsStageSignature

```c
typedef enum {
    ...
} cmsStageSignature;
```
---

#### cmsTagSignature

```c
typedef enum {
    ...
} cmsTagSignature;
```
---

#### cmsTagTypeSignature

```c
typedef enum {
    ...
} cmsTagTypeSignature;
```
---

#### cmsTechnologySignature

```c
typedef enum {
    ...
} cmsTechnologySignature;
```
### Functions

#### cmsAdaptToIlluminant

```c
CMSAPI cmsBool CMSEXPORT cmsAdaptToIlluminant(cmsCIEXYZ* Result, const cmsCIEXYZ* SourceWhitePt, const cmsCIEXYZ* Illuminant, const cmsCIEXYZ* Value);
```
---

#### cmsBFDdeltaE

```c
CMSAPI cmsFloat64Number CMSEXPORT cmsBFDdeltaE(const cmsCIELab* Lab1, const cmsCIELab* Lab2);
```
---

#### cmsBuildGamma

```c
CMSAPI cmsToneCurve* CMSEXPORT cmsBuildGamma(cmsContext ContextID, cmsFloat64Number Gamma);
```
---

#### cmsBuildParametricToneCurve

```c
CMSAPI cmsToneCurve* CMSEXPORT cmsBuildParametricToneCurve(cmsContext ContextID, cmsInt32Number Type, const cmsFloat64Number Params[]);
```
---

#### cmsBuildSegmentedToneCurve

```c
CMSAPI cmsToneCurve* CMSEXPORT cmsBuildSegmentedToneCurve(cmsContext ContextID, cmsUInt32Number nSegments, const cmsCurveSegment Segments[]);
```
---

#### cmsBuildTabulatedToneCurveFloat

```c
CMSAPI cmsToneCurve* CMSEXPORT cmsBuildTabulatedToneCurveFloat(cmsContext ContextID, cmsUInt32Number nEntries, const cmsFloat32Number values[]);
```
---

#### cmsCIE2000DeltaE

```c
CMSAPI cmsFloat64Number CMSEXPORT cmsCIE2000DeltaE(const cmsCIELab* Lab1, const cmsCIELab* Lab2, cmsFloat64Number Kl, cmsFloat64Number Kc, cmsFloat64Number Kh);
```
---

#### cmsCIE94DeltaE

```c
CMSAPI cmsFloat64Number CMSEXPORT cmsCIE94DeltaE(const cmsCIELab* Lab1, const cmsCIELab* Lab2);
```
---

#### cmsCIECAM02Done

```c
CMSAPI void CMSEXPORT cmsCIECAM02Done(cmsHANDLE hModel);
```
---

#### cmsCIECAM02Forward

```c
CMSAPI void CMSEXPORT cmsCIECAM02Forward(cmsHANDLE hModel, const cmsCIEXYZ* pIn, cmsJCh* pOut);
```
---

#### cmsCIECAM02Init

```c
CMSAPI cmsHANDLE CMSEXPORT cmsCIECAM02Init(cmsContext ContextID, const cmsViewingConditions* pVC);
```
---

#### cmsCIECAM02Reverse

```c
CMSAPI void CMSEXPORT cmsCIECAM02Reverse(cmsHANDLE hModel, const cmsJCh* pIn, cmsCIEXYZ* pOut);
```
---

#### cmsCMCdeltaE

```c
CMSAPI cmsFloat64Number CMSEXPORT cmsCMCdeltaE(const cmsCIELab* Lab1, const cmsCIELab* Lab2, cmsFloat64Number l, cmsFloat64Number c);
```
---

#### cmsCreateContext

```c
CMSAPI cmsContext CMSEXPORT cmsCreateContext(void* Plugin, void* UserData);
```
---

#### cmsD50_xyY

```c
CMSAPI const cmsCIExyY* CMSEXPORT cmsD50_xyY(void);
```
---

#### cmsD50_XYZ

```c
CMSAPI const cmsCIEXYZ* CMSEXPORT cmsD50_XYZ(void);
```
---

#### cmsDeleteContext

```c
CMSAPI void CMSEXPORT cmsDeleteContext(cmsContext ContextID);
```
---

#### cmsDeltaE

```c
CMSAPI cmsFloat64Number CMSEXPORT cmsDeltaE(const cmsCIELab* Lab1, const cmsCIELab* Lab2);
```
---

#### cmsDupContext

```c
CMSAPI cmsContext CMSEXPORT cmsDupContext(cmsContext ContextID, void* NewUserData);
```
---

#### cmsDupToneCurve

```c
CMSAPI cmsToneCurve* CMSEXPORT cmsDupToneCurve(const cmsToneCurve* Src);
```
---

#### cmsEstimateGamma

```c
CMSAPI cmsFloat64Number CMSEXPORT cmsEstimateGamma(const cmsToneCurve* t, cmsFloat64Number Precision);
```
---

#### cmsEvalToneCurve16

```c
CMSAPI cmsUInt16Number CMSEXPORT cmsEvalToneCurve16(const cmsToneCurve* Curve, cmsUInt16Number v);
```
---

#### cmsEvalToneCurveFloat

```c
CMSAPI cmsFloat32Number CMSEXPORT cmsEvalToneCurveFloat(const cmsToneCurve* Curve, cmsFloat32Number v);
```
---

#### cmsfilelength

```c
CMSAPI long int CMSEXPORT cmsfilelength(FILE* f);
```
---

#### cmsFloat2LabEncoded

```c
CMSAPI void CMSEXPORT cmsFloat2LabEncoded(cmsUInt16Number wLab[3], const cmsCIELab* Lab);
```
---

#### cmsFloat2LabEncodedV2

```c
CMSAPI void CMSEXPORT cmsFloat2LabEncodedV2(cmsUInt16Number wLab[3], const cmsCIELab* Lab);
```
---

#### cmsFloat2XYZEncoded

```c
CMSAPI void CMSEXPORT cmsFloat2XYZEncoded(cmsUInt16Number XYZ[3], const cmsCIEXYZ* fXYZ);
```
---

#### cmsFreeToneCurve

```c
CMSAPI void CMSEXPORT cmsFreeToneCurve(cmsToneCurve* Curve);
```
---

#### cmsFreeToneCurveTriple

```c
CMSAPI void CMSEXPORT cmsFreeToneCurveTriple(cmsToneCurve* Curve[3]);
```
---

#### cmsGetContextUserData

```c
CMSAPI void* CMSEXPORT cmsGetContextUserData(cmsContext ContextID);
```
---

#### cmsGetEncodedCMMversion

```c
CMSAPI int CMSEXPORT cmsGetEncodedCMMversion(void);
```
---

#### cmsGetPipelineContextID

```c
CMSAPI cmsContext CMSEXPORT cmsGetPipelineContextID(const cmsPipeline* lut);
```
---

#### cmsGetStageContextID

```c
CMSAPI cmsContext CMSEXPORT cmsGetStageContextID(const cmsStage* mpe);
```
---

#### cmsGetToneCurveEstimatedTable

```c
CMSAPI const cmsUInt16Number* CMSEXPORT cmsGetToneCurveEstimatedTable(const cmsToneCurve* t);
```
---

#### cmsGetToneCurveEstimatedTableEntries

```c
CMSAPI cmsUInt32Number CMSEXPORT cmsGetToneCurveEstimatedTableEntries(const cmsToneCurve* t);
```
---

#### cmsGetToneCurveParametricType

```c
CMSAPI cmsInt32Number CMSEXPORT cmsGetToneCurveParametricType(const cmsToneCurve* t);
```
---

#### cmsGetToneCurveParams

```c
CMSAPI cmsFloat64Number* CMSEXPORT cmsGetToneCurveParams(const cmsToneCurve* t);
```
---

#### cmsIsToneCurveDescending

```c
CMSAPI cmsBool CMSEXPORT cmsIsToneCurveDescending(const cmsToneCurve* t);
```
---

#### cmsIsToneCurveLinear

```c
CMSAPI cmsBool CMSEXPORT cmsIsToneCurveLinear(const cmsToneCurve* Curve);
```
---

#### cmsIsToneCurveMonotonic

```c
CMSAPI cmsBool CMSEXPORT cmsIsToneCurveMonotonic(const cmsToneCurve* t);
```
---

#### cmsIsToneCurveMultisegment

```c
CMSAPI cmsBool CMSEXPORT cmsIsToneCurveMultisegment(const cmsToneCurve* InGamma);
```
---

#### cmsJoinToneCurve

```c
CMSAPI cmsToneCurve* CMSEXPORT cmsJoinToneCurve(cmsContext ContextID, const cmsToneCurve* X,  const cmsToneCurve* Y, cmsUInt32Number nPoints);
```
---

#### cmsLab2LCh

```c
CMSAPI void CMSEXPORT cmsLab2LCh(cmsCIELCh*LCh, const cmsCIELab* Lab);
```
---

#### cmsLab2XYZ

```c
CMSAPI void CMSEXPORT cmsLab2XYZ(const cmsCIEXYZ* WhitePoint, cmsCIEXYZ* xyz, const cmsCIELab* Lab);
```
---

#### cmsLabEncoded2Float

```c
CMSAPI void CMSEXPORT cmsLabEncoded2Float(cmsCIELab* Lab, const cmsUInt16Number wLab[3]);
```
---

#### cmsLabEncoded2FloatV2

```c
CMSAPI void CMSEXPORT cmsLabEncoded2FloatV2(cmsCIELab* Lab, const cmsUInt16Number wLab[3]);
```
---

#### cmsLCh2Lab

```c
CMSAPI void CMSEXPORT cmsLCh2Lab(cmsCIELab* Lab, const cmsCIELCh* LCh);
```
---

#### cmsPipelineAlloc

```c
CMSAPI cmsPipeline* CMSEXPORT cmsPipelineAlloc(cmsContext ContextID, cmsUInt32Number InputChannels, cmsUInt32Number OutputChannels);
```
---

#### cmsPipelineCat

```c
CMSAPI cmsBool CMSEXPORT cmsPipelineCat(cmsPipeline* l1, const cmsPipeline* l2);
```
---

#### cmsPipelineCheckAndRetreiveStages

```c
CMSAPI cmsBool CMSEXPORT cmsPipelineCheckAndRetreiveStages(const cmsPipeline* Lut, cmsUInt32Number n, ...);
```
---

#### cmsPipelineDup

```c
CMSAPI cmsPipeline* CMSEXPORT cmsPipelineDup(const cmsPipeline* Orig);
```
---

#### cmsPipelineEval16

```c
CMSAPI void CMSEXPORT cmsPipelineEval16(const cmsUInt16Number In[], cmsUInt16Number Out[], const cmsPipeline* lut);
```
---

#### cmsPipelineEvalFloat

```c
CMSAPI void CMSEXPORT cmsPipelineEvalFloat(const cmsFloat32Number In[], cmsFloat32Number Out[], const cmsPipeline* lut);
```
---

#### cmsPipelineEvalReverseFloat

```c
CMSAPI cmsBool CMSEXPORT cmsPipelineEvalReverseFloat(cmsFloat32Number Target[], cmsFloat32Number Result[], cmsFloat32Number Hint[], const cmsPipeline* lut);
```
---

#### cmsPipelineFree

```c
CMSAPI void CMSEXPORT cmsPipelineFree(cmsPipeline* lut);
```
---

#### cmsPipelineGetPtrToFirstStage

```c
CMSAPI cmsStage* CMSEXPORT cmsPipelineGetPtrToFirstStage(const cmsPipeline* lut);
```
---

#### cmsPipelineGetPtrToLastStage

```c
CMSAPI cmsStage* CMSEXPORT cmsPipelineGetPtrToLastStage(const cmsPipeline* lut);
```
---

#### cmsPipelineInputChannels

```c
CMSAPI cmsUInt32Number CMSEXPORT cmsPipelineInputChannels(const cmsPipeline* lut);
```
---

#### cmsPipelineInsertStage

```c
CMSAPI cmsBool CMSEXPORT cmsPipelineInsertStage(cmsPipeline* lut, cmsStageLoc loc, cmsStage* mpe);
```
---

#### cmsPipelineOutputChannels

```c
CMSAPI cmsUInt32Number CMSEXPORT cmsPipelineOutputChannels(const cmsPipeline* lut);
```
---

#### cmsPipelineSetSaveAs8bitsFlag

```c
CMSAPI cmsBool CMSEXPORT cmsPipelineSetSaveAs8bitsFlag(cmsPipeline* lut, cmsBool On);
```
---

#### cmsPipelineStageCount

```c
CMSAPI cmsUInt32Number CMSEXPORT cmsPipelineStageCount(const cmsPipeline* lut);
```
---

#### cmsPipelineUnlinkStage

```c
CMSAPI void CMSEXPORT cmsStageFree(cmsStage* mpe);
```
---

#### cmsPlugin

```c
CMSAPI cmsBool CMSEXPORT cmsPlugin(void* Plugin);
```
---

#### cmsPluginTHR

```c
CMSAPI cmsBool CMSEXPORT cmsPluginTHR(cmsContext ContextID, void* Plugin);
```
---

#### cmsReverseToneCurve

```c
CMSAPI cmsToneCurve* CMSEXPORT cmsReverseToneCurve(const cmsToneCurve* InGamma);
```
---

#### cmsReverseToneCurveEx

```c
CMSAPI cmsToneCurve* CMSEXPORT cmsReverseToneCurveEx(cmsUInt32Number nResultSamples, const cmsToneCurve* InGamma);
```
---

#### cmsSetLogErrorHandler

```c
CMSAPI void CMSEXPORT cmsSetLogErrorHandler(cmsLogErrorHandlerFunction Fn);
```
---

#### cmsSetLogErrorHandlerTHR

```c
CMSAPI void CMSEXPORT cmsSetLogErrorHandlerTHR(cmsContext ContextID, cmsLogErrorHandlerFunction Fn);
```
---

#### cmsSliceSpace16

```c
CMSAPI cmsBool CMSEXPORT cmsSliceSpace16(cmsUInt32Number nInputs, const cmsUInt32Number clutPoints[], cmsSAMPLER16 Sampler, void * Cargo);
```
---

#### cmsSliceSpaceFloat

```c
CMSAPI cmsBool CMSEXPORT cmsSliceSpaceFloat(cmsUInt32Number nInputs, const cmsUInt32Number clutPoints[], cmsSAMPLERFLOAT Sampler, void * Cargo);
```
---

#### cmsSmoothToneCurve

```c
CMSAPI cmsBool CMSEXPORT cmsSmoothToneCurve(cmsToneCurve* Tab, cmsFloat64Number lambda);
```
---

#### cmsStageAllocCLut16bit

```c
CMSAPI cmsStage* CMSEXPORT cmsStageAllocCLut16bit(cmsContext ContextID, cmsUInt32Number nGridPoints, cmsUInt32Number inputChan, cmsUInt32Number outputChan, const cmsUInt16Number* Table);
```
---

#### cmsStageAllocCLut16bitGranular

```c
CMSAPI cmsStage* CMSEXPORT cmsStageAllocCLut16bitGranular(cmsContext ContextID, const cmsUInt32Number clutPoints[], cmsUInt32Number inputChan, cmsUInt32Number outputChan, const cmsUInt16Number* Table);
```
---

#### cmsStageAllocCLutFloat

```c
CMSAPI cmsStage* CMSEXPORT cmsStageAllocCLutFloat(cmsContext ContextID, cmsUInt32Number nGridPoints, cmsUInt32Number inputChan, cmsUInt32Number outputChan, const cmsFloat32Number* Table);
```
---

#### cmsStageAllocCLutFloatGranular

```c
CMSAPI cmsStage* CMSEXPORT cmsStageAllocCLutFloatGranular(cmsContext ContextID, const cmsUInt32Number clutPoints[], cmsUInt32Number inputChan, cmsUInt32Number outputChan, const cmsFloat32Number* Table);
```
---

#### cmsStageAllocIdentity

```c
CMSAPI cmsStage* CMSEXPORT cmsStageAllocIdentity(cmsContext ContextID, cmsUInt32Number nChannels);
```
---

#### cmsStageAllocMatrix

```c
CMSAPI cmsStage* CMSEXPORT cmsStageAllocMatrix(cmsContext ContextID, cmsUInt32Number Rows, cmsUInt32Number Cols, const cmsFloat64Number* Matrix, const cmsFloat64Number* Offset);
```
---

#### cmsStageAllocToneCurves

```c
CMSAPI cmsStage* CMSEXPORT cmsStageAllocToneCurves(cmsContext ContextID, cmsUInt32Number nChannels, cmsToneCurve* const Curves[]);
```
---

#### cmsStageData

```c
CMSAPI void* CMSEXPORT cmsStageData(const cmsStage* mpe);
```
---

#### cmsStageDup

```c
CMSAPI cmsStage* CMSEXPORT cmsStageDup(cmsStage* mpe);
```
---

#### cmsStageFree

```c
CMSAPI void CMSEXPORT cmsStageFree(cmsStage* mpe);
```
---

#### cmsStageInputChannels

```c
CMSAPI cmsUInt32Number CMSEXPORT cmsStageInputChannels(const cmsStage* mpe);
```
---

#### cmsStageNext

```c
CMSAPI cmsStage* CMSEXPORT cmsStageNext(const cmsStage* mpe);
```
---

#### cmsStageOutputChannels

```c
CMSAPI cmsUInt32Number CMSEXPORT cmsStageOutputChannels(const cmsStage* mpe);
```
---

#### cmsStageSampleCLut16bit

```c
CMSAPI cmsBool CMSEXPORT cmsStageSampleCLut16bit(cmsStage* mpe, cmsSAMPLER16 Sampler, void* Cargo, cmsUInt32Number dwFlags);
```
---

#### cmsStageSampleCLutFloat

```c
CMSAPI cmsBool CMSEXPORT cmsStageSampleCLutFloat(cmsStage* mpe, cmsSAMPLERFLOAT Sampler, void* Cargo, cmsUInt32Number dwFlags);
```
---

#### cmsStageType

```c
CMSAPI cmsStageSignature CMSEXPORT cmsStageType(const cmsStage* mpe);
```
---

#### cmsstrcasecmp

```c
CMSAPI int CMSEXPORT cmsstrcasecmp(const char* s1, const char* s2);
```
---

#### cmsTempFromWhitePoint

```c
CMSAPI cmsBool CMSEXPORT cmsTempFromWhitePoint(cmsFloat64Number* TempK, const cmsCIExyY* WhitePoint);
```
---

#### cmsUnregisterPlugins

```c
CMSAPI void CMSEXPORT cmsUnregisterPlugins(void);
```
---

#### cmsUnregisterPluginsTHR

```c
CMSAPI void CMSEXPORT cmsUnregisterPluginsTHR(cmsContext ContextID);
```
---

#### cmsWhitePointFromTemp

```c
CMSAPI cmsBool CMSEXPORT cmsWhitePointFromTemp(cmsCIExyY* WhitePoint, cmsFloat64Number  TempK);
```
---

#### cmsxyY2XYZ

```c
CMSAPI void CMSEXPORT cmsxyY2XYZ(cmsCIEXYZ* Dest, const cmsCIExyY* Source);
```
---

#### cmsXYZ2xyY

```c
CMSAPI void CMSEXPORT cmsXYZ2xyY(cmsCIExyY* Dest, const cmsCIEXYZ* Source);
```
---

#### cmsXYZEncoded2Float

```c
CMSAPI void CMSEXPORT cmsXYZEncoded2Float(cmsCIEXYZ* fxyz, const cmsUInt16Number XYZ[3]);
```
### Structs/Typedefs

#### _cms_curve_struct

```c
typedef struct _cms_curve_struct cmsToneCurve;
```
---

#### _cmsContext_struct

```c
typedef struct _cmsContext_struct* cmsContext;
```
---

#### _cmsPipeline_struct

```c
typedef struct _cmsPipeline_struct cmsPipeline;
```
---

#### _cmsStage_struct

```c
typedef struct _cmsStage_struct cmsStage;
```
---

#### cmsCIELab

```c
typedef struct {
        cmsFloat64Number L;
        cmsFloat64Number a;
        cmsFloat64Number b;

    } cmsCIELab;
```
---

#### cmsCIELCh

```c
typedef struct {
        cmsFloat64Number L;
        cmsFloat64Number C;
        cmsFloat64Number h;

    } cmsCIELCh;
```
---

#### cmsCIExyY

```c
typedef struct {
        cmsFloat64Number x;
        cmsFloat64Number y;
        cmsFloat64Number Y;

    } cmsCIExyY;
```
---

#### cmsCIExyYTRIPLE

```c
typedef struct {
        cmsCIExyY  Red;
        cmsCIExyY  Green;
        cmsCIExyY  Blue;

    } cmsCIExyYTRIPLE;
```
---

#### cmsCIEXYZ

```c
typedef struct {
        cmsFloat64Number X;
        cmsFloat64Number Y;
        cmsFloat64Number Z;

    } cmsCIEXYZ;
```
---

#### cmsCIEXYZTRIPLE

```c
typedef struct {
        cmsCIEXYZ  Red;
        cmsCIEXYZ  Green;
        cmsCIEXYZ  Blue;

    } cmsCIEXYZTRIPLE;
```
---

#### cmsContext

```c
typedef struct _cmsContext_struct* cmsContext;
```
---

#### cmsCurveSegment

```c
typedef struct {
    cmsFloat32Number   x0, x1;
    cmsInt32Number     Type;
    cmsFloat64Number   Params[10];
    cmsUInt32Number    nGridPoints;
    cmsFloat32Number*  SampledPoints;

} cmsCurveSegment;
```
---

#### cmsDateTimeNumber

```c
typedef struct {
    cmsUInt16Number      year;
    cmsUInt16Number      month;
    cmsUInt16Number      day;
    cmsUInt16Number      hours;
    cmsUInt16Number      minutes;
    cmsUInt16Number      seconds;

} cmsDateTimeNumber;
```
---

#### cmsEncodedXYZNumber

```c
typedef struct {
    cmsS15Fixed16Number  X;
    cmsS15Fixed16Number  Y;
    cmsS15Fixed16Number  Z;

} cmsEncodedXYZNumber;
```
---

#### cmsICCData

```c
typedef struct {
    cmsUInt32Number len;
    cmsUInt32Number flag;
    cmsUInt8Number  data[1];

} cmsICCData;
```
---

#### cmsICCHeader

```c
typedef struct {
    cmsUInt32Number              size;
    cmsSignature                 cmmId;
    cmsUInt32Number              version;
    cmsProfileClassSignature     deviceClass;
    cmsColorSpaceSignature       colorSpace;
    cmsColorSpaceSignature       pcs;
    cmsDateTimeNumber            date;
    cmsSignature                 magic;
    cmsPlatformSignature         platform;
    cmsUInt32Number              flags;
    cmsSignature                 manufacturer;
    cmsUInt32Number              model;
    cmsUInt64Number              attributes;
    cmsUInt32Number              renderingIntent;
    cmsEncodedXYZNumber          illuminant;
    cmsSignature                 creator;
    cmsProfileID                 profileID;
    cmsInt8Number                reserved[28];

} cmsICCHeader;
```
---

#### cmsICCMeasurementConditions

```c
typedef struct {
        cmsUInt32Number  Observer;
        cmsCIEXYZ        Backing;
        cmsUInt32Number  Geometry;
        cmsFloat64Number Flare;
        cmsUInt32Number  IlluminantType;

    } cmsICCMeasurementConditions;
```
---

#### cmsICCViewingConditions

```c
typedef struct {
        cmsCIEXYZ       IlluminantXYZ;
        cmsCIEXYZ       SurroundXYZ;
        cmsUInt32Number IlluminantType;

    } cmsICCViewingConditions;
```
---

#### cmsJCh

```c
typedef struct {
        cmsFloat64Number J;
        cmsFloat64Number C;
        cmsFloat64Number h;

    } cmsJCh;
```
---

#### cmsLogErrorHandlerFunction

```c
typedef void* cmsLogErrorHandlerFunction(cmsContext ContextID, cmsUInt32Number ErrorCode, const char *Text);
```
---

#### cmsPipeline

```c
typedef struct _cmsPipeline_struct cmsPipeline;
```
---

#### cmsProfileID

```c
typedef union {
    cmsUInt8Number       ID8[16];
    cmsUInt16Number      ID16[8];
    cmsUInt32Number      ID32[4];

} cmsProfileID;
```
---

#### cmsS15Fixed16Number

```c
typedef cmsInt32Number cmsS15Fixed16Number;
```
---

#### cmsSAMPLER16

```c
typedef cmsInt32Number* cmsSAMPLER16(CMSREGISTER const cmsUInt16Number In[], CMSREGISTER cmsUInt16Number Out[], CMSREGISTER void * Cargo);
```
---

#### cmsSAMPLERFLOAT

```c
typedef cmsInt32Number* cmsSAMPLERFLOAT(CMSREGISTER const cmsFloat32Number In[], CMSREGISTER cmsFloat32Number Out[], CMSREGISTER void * Cargo);
```
---

#### cmsSignature

```c
typedef cmsUInt32Number cmsSignature;
```
---

#### cmsStage

```c
typedef struct _cmsStage_struct cmsStage;
```
---

#### cmsTagBase

```c
typedef struct {
    cmsTagTypeSignature  sig;
    cmsInt8Number        reserved[4];

} cmsTagBase;
```
---

#### cmsTagEntry

```c
typedef struct {
    cmsTagSignature      sig;
    cmsUInt32Number      offset;
    cmsUInt32Number      size;

} cmsTagEntry;
```
---

#### cmsToneCurve

```c
typedef struct _cms_curve_struct cmsToneCurve;
```
---

#### cmsU16Fixed16Number

```c
typedef cmsUInt32Number cmsU16Fixed16Number;
```
---

#### cmsU8Fixed8Number

```c
typedef cmsUInt16Number cmsU8Fixed8Number;
```
---

#### cmsViewingConditions

```c
typedef struct {
    cmsCIEXYZ        whitePoint;
    cmsFloat64Number Yb;
    cmsFloat64Number La;
    cmsUInt32Number  surround;
    cmsFloat64Number D_value;

    } cmsViewingConditions;
```
