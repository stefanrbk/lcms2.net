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

namespace lcms2.types;

public partial class Stage
{
    #region Private Constructors

    private Stage(object? state,
                  Signature type,
                  Signature implements,
                  uint inputChannels,
                  uint outputChannels,
                  StageData data)
    {
        StateContainer = state;
        Type = type;
        this.implements = implements;
        InputChannels = inputChannels;
        OutputChannels = outputChannels;
        Data = data;
    }

    #endregion Private Constructors

    #region Public Methods

    public static Stage? Alloc(object? state,
                               Signature type,
                               uint inputChannels,
                               uint outputChannels,
                               StageData? data)
    {
        /**  Original Code (cmslut.c line: 30)
         **
         **  // Allocates an empty multi profile element
         **  cmsStage* CMSEXPORT _cmsStageAllocPlaceholder(cmsContext ContextID,
         **                                  cmsStageSignature Type,
         **                                  cmsUInt32Number InputChannels,
         **                                  cmsUInt32Number OutputChannels,
         **                                  _cmsStageEvalFn     EvalPtr,
         **                                  _cmsStageDupElemFn  DupElemPtr,
         **                                  _cmsStageFreeElemFn FreePtr,
         **                                  void*             Data)
         **  {
         **      cmsStage* ph = (cmsStage*) _cmsMallocZero(ContextID, sizeof(cmsStage));
         **
         **      if (ph == NULL) return NULL;
         **
         **
         **      ph ->ContextID = ContextID;
         **
         **      ph ->Type       = Type;
         **      ph ->Implements = Type;   // By default, no clue on what is implementing
         **
         **      ph ->InputChannels  = InputChannels;
         **      ph ->OutputChannels = OutputChannels;
         **      ph ->EvalPtr        = EvalPtr;
         **      ph ->DupElemPtr     = DupElemPtr;
         **      ph ->FreePtr        = FreePtr;
         **      ph ->Data           = Data;
         **
         **      return ph;
         **  }
         **/

        if (inputChannels > maxStageChannels ||
            outputChannels > maxStageChannels)
        {
            return null;
        }

        return new(state, type, type, inputChannels, outputChannels, data);
    }

    public static Stage? AllocCLut16bit(object? state,
                                        uint numGridPoints,
                                        uint inputChan,
                                        uint outputChan,
                                        in ushort[]? table) =>
        /**  Original Code (cmslut.c line: 604)
         **
         **  cmsStage* CMSEXPORT cmsStageAllocCLut16bit(cmsContext ContextID,
         **                                      cmsUInt32Number nGridPoints,
         **                                      cmsUInt32Number inputChan,
         **                                      cmsUInt32Number outputChan,
         **                                      const cmsUInt16Number* Table)
         **  {
         **      cmsUInt32Number Dimensions[MAX_INPUT_DIMENSIONS];
         **      int i;
         **
         **     // Our resulting LUT would be same gridpoints on all dimensions
         **      for (i=0; i < MAX_INPUT_DIMENSIONS; i++)
         **          Dimensions[i] = nGridPoints;
         **
         **      return cmsStageAllocCLut16bitGranular(ContextID, Dimensions, inputChan, outputChan, Table);
         **  }
         **/

        AllocCLut16bit(state, Enumerable.Repeat(numGridPoints, maxInputDimensions).ToArray(), inputChan, outputChan, table);

    public static Stage? AllocCLut16bit(object? state,
                                        uint[] clutPoints,
                                        uint inputChan,
                                        uint outputChan,
                                        in ushort[]? table)
    {
        /**  Original Code (cmslut.c line: 542)
         **
         **  // Allocates a 16-bit multidimensional CLUT. This is evaluated at 16-bit precision. Table may have different
         **  // granularity on each dimension.
         **  cmsStage* CMSEXPORT cmsStageAllocCLut16bitGranular(cmsContext ContextID,
         **                                           const cmsUInt32Number clutPoints[],
         **                                           cmsUInt32Number inputChan,
         **                                           cmsUInt32Number outputChan,
         **                                           const cmsUInt16Number* Table)
         **  {
         **      cmsUInt32Number i, n;
         **      _cmsStageCLutData* NewElem;
         **      cmsStage* NewMPE;
         **
         **      _cmsAssert(clutPoints != NULL);
         **
         **      if (inputChan > MAX_INPUT_DIMENSIONS) {
         **          cmsSignalError(ContextID, cmsERROR_RANGE, "Too many input channels (%d channels, max=%d)", inputChan, MAX_INPUT_DIMENSIONS);
         **          return NULL;
         **      }
         **
         **      NewMPE = _cmsStageAllocPlaceholder(ContextID, cmsSigCLutElemType, inputChan, outputChan,
         **                                       EvaluateCLUTfloatIn16, CLUTElemDup, CLutElemTypeFree, NULL );
         **
         **      if (NewMPE == NULL) return NULL;
         **
         **      NewElem = (_cmsStageCLutData*) _cmsMallocZero(ContextID, sizeof(_cmsStageCLutData));
         **      if (NewElem == NULL) {
         **          cmsStageFree(NewMPE);
         **          return NULL;
         **      }
         **
         **      NewMPE ->Data  = (void*) NewElem;
         **
         **      NewElem -> nEntries = n = outputChan * CubeSize(clutPoints, inputChan);
         **      NewElem -> HasFloatValues = FALSE;
         **
         **      if (n == 0) {
         **          cmsStageFree(NewMPE);
         **          return NULL;
         **      }
         **
         **
         **      NewElem ->Tab.T  = (cmsUInt16Number*) _cmsCalloc(ContextID, n, sizeof(cmsUInt16Number));
         **      if (NewElem ->Tab.T == NULL) {
         **          cmsStageFree(NewMPE);
         **          return NULL;
         **      }
         **
         **      if (Table != NULL) {
         **          for (i=0; i < n; i++) {
         **              NewElem ->Tab.T[i] = Table[i];
         **          }
         **      }
         **
         **      NewElem ->Params = _cmsComputeInterpParamsEx(ContextID, clutPoints, inputChan, outputChan, NewElem ->Tab.T, CMS_LERP_FLAGS_16BITS);
         **      if (NewElem ->Params == NULL) {
         **          cmsStageFree(NewMPE);
         **          return NULL;
         **      }
         **
         **      return NewMPE;
         **  }
         **/

        if (inputChan > maxInputDimensions)
        {
            Errors.TooManyInputChannels(state, inputChan, maxInputDimensions);
            return null;
        }

        uint n = outputChan * CubeSize(clutPoints, (int)inputChan);
        var newTable = ((ushort[]?)table?.Clone()) ?? new ushort[n];

        var p =
            InterpParams.Compute(
                state,
                clutPoints,
                inputChan,
                outputChan,
                newTable,
                LerpFlag.Ushort);
        if (p is null) return null;

        var newElem =
            new CLut16Data(
                newTable,
                p,
                n);

        if (n is 0) return null;

        return
            Alloc(
                state,
                Signature.Stage.CLutElem,
                inputChan,
                outputChan,
                newElem);
    }

    public static Stage? AllocCLutFloat(object? state,
                                        uint numGridPoints,
                                        uint inputChan,
                                        uint outputChan,
                                        in float[]? table) =>
        /**  Original Code (cmslut.c line: 621)
         **
         **  cmsStage* CMSEXPORT cmsStageAllocCLutFloat(cmsContext ContextID,
         **                                         cmsUInt32Number nGridPoints,
         **                                         cmsUInt32Number inputChan,
         **                                         cmsUInt32Number outputChan,
         **                                         const cmsFloat32Number* Table)
         **  {
         **     cmsUInt32Number Dimensions[MAX_INPUT_DIMENSIONS];
         **     int i;
         **
         **      // Our resulting LUT would be same gridpoints on all dimensions
         **      for (i=0; i < MAX_INPUT_DIMENSIONS; i++)
         **          Dimensions[i] = nGridPoints;
         **
         **      return cmsStageAllocCLutFloatGranular(ContextID, Dimensions, inputChan, outputChan, Table);
         **  }
         **/

        AllocCLutFloat(state, Enumerable.Repeat(numGridPoints, maxInputDimensions).ToArray(), inputChan, outputChan, table);

    public static Stage? AllocCLutFloat(object? state,
                                        uint[] clutPoints,
                                        uint inputChan,
                                        uint outputChan,
                                        in float[]? table)
    {
        /**  Original Code (cmslut.c line: 639)
         **
         **  cmsStage* CMSEXPORT cmsStageAllocCLutFloatGranular(cmsContext ContextID, const cmsUInt32Number clutPoints[], cmsUInt32Number inputChan, cmsUInt32Number outputChan, const cmsFloat32Number* Table)
         **  {
         **      cmsUInt32Number i, n;
         **      _cmsStageCLutData* NewElem;
         **      cmsStage* NewMPE;
         **
         **      _cmsAssert(clutPoints != NULL);
         **
         **      if (inputChan > MAX_INPUT_DIMENSIONS) {
         **          cmsSignalError(ContextID, cmsERROR_RANGE, "Too many input channels (%d channels, max=%d)", inputChan, MAX_INPUT_DIMENSIONS);
         **          return NULL;
         **      }
         **
         **      NewMPE = _cmsStageAllocPlaceholder(ContextID, cmsSigCLutElemType, inputChan, outputChan,
         **                                               EvaluateCLUTfloat, CLUTElemDup, CLutElemTypeFree, NULL);
         **      if (NewMPE == NULL) return NULL;
         **
         **
         **      NewElem = (_cmsStageCLutData*) _cmsMallocZero(ContextID, sizeof(_cmsStageCLutData));
         **      if (NewElem == NULL) {
         **          cmsStageFree(NewMPE);
         **          return NULL;
         **      }
         **
         **      NewMPE ->Data  = (void*) NewElem;
         **
         **      // There is a potential integer overflow on conputing n and nEntries.
         **      NewElem -> nEntries = n = outputChan * CubeSize(clutPoints, inputChan);
         **      NewElem -> HasFloatValues = TRUE;
         **
         **      if (n == 0) {
         **          cmsStageFree(NewMPE);
         **          return NULL;
         **      }
         **
         **      NewElem ->Tab.TFloat  = (cmsFloat32Number*) _cmsCalloc(ContextID, n, sizeof(cmsFloat32Number));
         **      if (NewElem ->Tab.TFloat == NULL) {
         **          cmsStageFree(NewMPE);
         **          return NULL;
         **      }
         **
         **      if (Table != NULL) {
         **          for (i=0; i < n; i++) {
         **              NewElem ->Tab.TFloat[i] = Table[i];
         **          }
         **      }
         **
         **      NewElem ->Params = _cmsComputeInterpParamsEx(ContextID, clutPoints,  inputChan, outputChan, NewElem ->Tab.TFloat, CMS_LERP_FLAGS_FLOAT);
         **      if (NewElem ->Params == NULL) {
         **          cmsStageFree(NewMPE);
         **          return NULL;
         **      }
         **
         **      return NewMPE;
         **  }
         **/

        if (inputChan > maxInputDimensions)
        {
            Errors.TooManyInputChannels(state, inputChan, maxInputDimensions);
            return null;
        }

        uint n = outputChan * CubeSize(clutPoints, (int)inputChan);
        var newTable = ((float[]?)table?.Clone()) ?? new float[n];

        var p =
            InterpParams.Compute(
                state,
                clutPoints,
                inputChan,
                outputChan,
                newTable,
                LerpFlag.Float);
        if (p is null) return null;

        var newElem =
            new CLutFloatData(
                newTable,
                p,
                n);

        if (n is 0) return null;

        return
            Alloc(
                state,
                Signature.Stage.CLutElem,
                inputChan,
                outputChan,
                newElem);
    }

    public static Stage? AllocIdentity(object? state, uint numChannels) =>
        /**  Original Code (cmslut.c line: 70)
        **
        **  cmsStage* CMSEXPORT cmsStageAllocIdentity(cmsContext ContextID, cmsUInt32Number nChannels)
        **  {
        **      return _cmsStageAllocPlaceholder(ContextID,
        **                                      cmsSigIdentityElemType,
        **                                      nChannels, nChannels,
        **                                      EvaluateIdentity,
        **                                      NULL,
        **                                      NULL,
        **                                      NULL);
        **  }
        **/

        Alloc(
            state,
            Signature.Stage.IdentityElem,
            numChannels,
            numChannels,
            new IdentityData());

    public static Stage? AllocMatrix(object? state,
                                     uint rows,
                                     uint cols,
                                     ReadOnlySpan<double> matrix,
                                     ReadOnlySpan<double> offset)
    {
        /**  Original Code (cmslut.c line: 379)
         **
         **  cmsStage*  CMSEXPORT cmsStageAllocMatrix(cmsContext ContextID, cmsUInt32Number Rows, cmsUInt32Number Cols,
         **                                       const cmsFloat64Number* Matrix, const cmsFloat64Number* Offset)
         **  {
         **      cmsUInt32Number i, n;
         **      _cmsStageMatrixData* NewElem;
         **      cmsStage* NewMPE;
         **
         **      n = Rows * Cols;
         **
         **      // Check for overflow
         **      if (n == 0) return NULL;
         **      if (n >= UINT_MAX / Cols) return NULL;
         **      if (n >= UINT_MAX / Rows) return NULL;
         **      if (n < Rows || n < Cols) return NULL;
         **
         **      NewMPE = _cmsStageAllocPlaceholder(ContextID, cmsSigMatrixElemType, Cols, Rows,
         **                                       EvaluateMatrix, MatrixElemDup, MatrixElemTypeFree, NULL );
         **      if (NewMPE == NULL) return NULL;
         **
         **
         **      NewElem = (_cmsStageMatrixData*) _cmsMallocZero(ContextID, sizeof(_cmsStageMatrixData));
         **      if (NewElem == NULL) goto Error;
         **      NewMPE->Data = (void*)NewElem;
         **
         **      NewElem ->Double = (cmsFloat64Number*) _cmsCalloc(ContextID, n, sizeof(cmsFloat64Number));
         **      if (NewElem->Double == NULL) goto Error;
         **
         **      for (i=0; i < n; i++) {
         **          NewElem ->Double[i] = Matrix[i];
         **      }
         **
         **      if (Offset != NULL) {
         **
         **          NewElem ->Offset = (cmsFloat64Number*) _cmsCalloc(ContextID, Rows, sizeof(cmsFloat64Number));
         **          if (NewElem->Offset == NULL) goto Error;
         **
         **          for (i=0; i < Rows; i++) {
         **                  NewElem ->Offset[i] = Offset[i];
         **          }
         **      }
         **
         **      return NewMPE;
         **
         **  Error:
         **      cmsStageFree(NewMPE);
         **      return NULL;
         **  }
         **/

        var n = rows * cols;

        // Check for overflow
        if (n is 0 || n < rows || n < cols ||
            n >= UInt32.MaxValue / cols ||
            n >= UInt32.MaxValue / rows)
        {
            return null;
        }

        var newElem = new MatrixData(
            matrix[..(int)n].ToArray(),
            offset != default
                ? offset[..(int)rows].ToArray()
                : null);

        return Alloc(
            state,
            Signature.Stage.MatrixElem,
            cols,
            rows,
            newElem);
    }

    public static Stage? AllocNamedColor(NamedColorList ncl, bool usePCS) =>
        Alloc(
            ncl.state,
            Signature.Stage.NamedColorElem,
            1,
            usePCS ? 3 : ncl.colorantCount,
            new NamedColorData(ncl, usePCS));

    public static Stage? AllocToneCurves(object? state,
                                         uint numChannels,
                                         ToneCurve[]? curves)
    {
        /**  Original Code (cmslut.c line: 247)
         **
         **  // Curves == NULL forces identity curves
         **  cmsStage* CMSEXPORT cmsStageAllocToneCurves(cmsContext ContextID, cmsUInt32Number nChannels, cmsToneCurve* const Curves[])
         **  {
         **      cmsUInt32Number i;
         **      _cmsStageToneCurvesData* NewElem;
         **      cmsStage* NewMPE;
         **
         **
         **      NewMPE = _cmsStageAllocPlaceholder(ContextID, cmsSigCurveSetElemType, nChannels, nChannels,
         **                                       EvaluateCurves, CurveSetDup, CurveSetElemTypeFree, NULL );
         **      if (NewMPE == NULL) return NULL;
         **
         **      NewElem = (_cmsStageToneCurvesData*) _cmsMallocZero(ContextID, sizeof(_cmsStageToneCurvesData));
         **      if (NewElem == NULL) {
         **          cmsStageFree(NewMPE);
         **          return NULL;
         **      }
         **
         **      NewMPE ->Data  = (void*) NewElem;
         **
         **      NewElem ->nCurves   = nChannels;
         **      NewElem ->TheCurves = (cmsToneCurve**) _cmsCalloc(ContextID, nChannels, sizeof(cmsToneCurve*));
         **      if (NewElem ->TheCurves == NULL) {
         **          cmsStageFree(NewMPE);
         **          return NULL;
         **      }
         **
         **      for (i=0; i < nChannels; i++) {
         **
         **          if (Curves == NULL) {
         **              NewElem ->TheCurves[i] = cmsBuildGamma(ContextID, 1.0);
         **          }
         **          else {
         **              NewElem ->TheCurves[i] = cmsDupToneCurve(Curves[i]);
         **          }
         **
         **          if (NewElem ->TheCurves[i] == NULL) {
         **              cmsStageFree(NewMPE);
         **              return NULL;
         **          }
         **
         **      }
         **
         **     return NewMPE;
         **  }
         **/

        var array = new ToneCurve[numChannels];
        for (var i = 0; i < numChannels; i++)
        {
            if (curves is null)
            {
                var t = ToneCurve.BuildGamma(state, 1.0);
                if (t is null) return null;
                array[i] = t;
            }
            else
                array[i] = (ToneCurve)curves[i].Clone();
        }
        var newElem = new ToneCurveData(array);
        if (newElem is null) return null;

        var newMpe =
            Alloc(
                state,
                Signature.Stage.CurveSetElem,
                numChannels,
                numChannels,
                newElem);
        if (newMpe is null) return null;

        return newMpe;
    }

    #endregion Public Methods

    #region Internal Methods

    internal static Stage? AllocIdentityCLut(object? state, uint numChan)
    {
        /**  Original Code (cmslut.c line: 708)
         **
         **  // Creates an MPE that just copies input to output
         **  cmsStage* CMSEXPORT _cmsStageAllocIdentityCLut(cmsContext ContextID, cmsUInt32Number nChan)
         **  {
         **      cmsUInt32Number Dimensions[MAX_INPUT_DIMENSIONS];
         **      cmsStage* mpe ;
         **      int i;
         **
         **      for (i=0; i < MAX_INPUT_DIMENSIONS; i++)
         **          Dimensions[i] = 2;
         **
         **      mpe = cmsStageAllocCLut16bitGranular(ContextID, Dimensions, nChan, nChan, NULL);
         **      if (mpe == NULL) return NULL;
         **
         **      if (!cmsStageSampleCLut16bit(mpe, IdentitySampler, &nChan, 0)) {
         **          cmsStageFree(mpe);
         **          return NULL;
         **      }
         **
         **      mpe ->Implements = cmsSigIdentityElemType;
         **      return mpe;
         **  }
         **/

        var dims = Enumerable.Repeat(2u, maxInputDimensions).ToArray();

        var mpe = AllocCLut16bit(state, dims, numChan, numChan, null);
        if (mpe is null) return null;

        if (!mpe.Sample(IdentitySampler, (int)numChan, SamplerFlags.None))
        {
            mpe.Dispose();
            return null;
        }

        mpe.implements = Signature.Stage.IdentityElem;
        return mpe;
    }

    internal static Stage? AllocLab2XYZ(object? state) =>
        /**  Original Code (cmslut.c line: 966)
         **
         **  cmsStage* CMSEXPORT _cmsStageAllocLab2XYZ(cmsContext ContextID)
         **  {
         **      return _cmsStageAllocPlaceholder(ContextID, cmsSigLab2XYZElemType, 3, 3, EvaluateLab2XYZ, NULL, NULL, NULL);
         **  }
         **/

        Alloc(state, Signature.Stage.Lab2XYZElem, 3, 3, new Lab2XYZData());

    internal static Stage? AllocLabPrelin(object? state)
    {
        /**  Original Code (cmslut.c line: 1184)
         **
         **  // For v4, S-Shaped curves are placed in a/b axis to increase resolution near gray
         **
         **  cmsStage* _cmsStageAllocLabPrelin(cmsContext ContextID)
         **  {
         **      cmsToneCurve* LabTable[3];
         **      cmsFloat64Number Params[1] =  {2.4} ;
         **
         **      LabTable[0] = cmsBuildGamma(ContextID, 1.0);
         **      LabTable[1] = cmsBuildParametricToneCurve(ContextID, 108, Params);
         **      LabTable[2] = cmsBuildParametricToneCurve(ContextID, 108, Params);
         **
         **      return cmsStageAllocToneCurves(ContextID, 3, LabTable);
         **  }
         **/

        var @params = new double[] { 2.4 };

        var temp = new[]
        {
            ToneCurve.BuildGamma(state, 1.0),
            ToneCurve.BuildParametric(state, 108, @params),
            ToneCurve.BuildParametric(state, 108, @params),
        };

        if (temp.Contains(null)) return null;
        var labTable = temp.Cast<ToneCurve>().ToArray();

        return AllocToneCurves(state, 3, labTable);
    }

    internal static Stage? AllocLabV2ToV4(object? state)
    {
        /**  Original Code (cmslut.c line: 1016)
         **
         **  // Matrix-based conversion, which is more accurate, but slower and cannot properly be saved in devicelink profiles
         **  cmsStage* CMSEXPORT _cmsStageAllocLabV2ToV4(cmsContext ContextID)
         **  {
         **      static const cmsFloat64Number V2ToV4[] = { 65535.0/65280.0, 0, 0,
         **                                       0, 65535.0/65280.0, 0,
         **                                       0, 0, 65535.0/65280.0
         **                                       };
         **
         **      cmsStage *mpe = cmsStageAllocMatrix(ContextID, 3, 3, V2ToV4, NULL);
         **
         **      if (mpe == NULL) return mpe;
         **      mpe ->Implements = cmsSigLabV2toV4;
         **      return mpe;
         **  }
         **/

        var v2ToV4 = new double[]
        {
            65535.0 / 65280.0, 0, 0,
            0, 65535.0 / 65280.0, 0,
            0, 0, 65535.0 / 65280.0
        };

        var mpe = AllocMatrix(state, 3, 3, v2ToV4, null);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.LabV2toV4Elem;
        return mpe;
    }

    internal static Stage? AllocLabV2ToV4Curves(object? state)
    {
        /**  Original Code (cmslut.c line: 973)
         **
         **  // v2 L=100 is supposed to be placed on 0xFF00. There is no reasonable
         **  // number of gridpoints that would make exact match. However, a prelinearization
         **  // of 258 entries, would map 0xFF00 exactly on entry 257, and this is good to avoid scum dot.
         **  // Almost all what we need but unfortunately, the rest of entries should be scaled by
         **  // (255*257/256) and this is not exact.
         **
         **  cmsStage* _cmsStageAllocLabV2ToV4curves(cmsContext ContextID)
         **  {
         **      cmsStage* mpe;
         **      cmsToneCurve* LabTable[3];
         **      int i, j;
         **
         **      LabTable[0] = cmsBuildTabulatedToneCurve16(ContextID, 258, NULL);
         **      LabTable[1] = cmsBuildTabulatedToneCurve16(ContextID, 258, NULL);
         **      LabTable[2] = cmsBuildTabulatedToneCurve16(ContextID, 258, NULL);
         **
         **      for (j=0; j < 3; j++) {
         **
         **          if (LabTable[j] == NULL) {
         **              cmsFreeToneCurveTriple(LabTable);
         **              return NULL;
         **          }
         **
         **          // We need to map * (0xffff / 0xff00), that's same as (257 / 256)
         **          // So we can use 258-entry tables to do the trick (i / 257) * (255 * 257) * (257 / 256);
         **          for (i=0; i < 257; i++)  {
         **
         **              LabTable[j]->Table16[i] = (cmsUInt16Number) ((i * 0xffff + 0x80) >> 8);
         **          }
         **
         **          LabTable[j] ->Table16[257] = 0xffff;
         **      }
         **
         **      mpe = cmsStageAllocToneCurves(ContextID, 3, LabTable);
         **      cmsFreeToneCurveTriple(LabTable);
         **
         **      if (mpe == NULL) return NULL;
         **      mpe ->Implements = cmsSigLabV2toV4;
         **      return mpe;
         **  }
         **/

        var temp = new[]
        {
            ToneCurve.BuildEmptyTabulated16(state, 258),
            ToneCurve.BuildEmptyTabulated16(state, 258),
            ToneCurve.BuildEmptyTabulated16(state, 258),
        };

        if (temp.Contains(null))
        {
            ToneCurve.DisposeTriple(temp!);
            return null;
        }
        var labTable = temp.Cast<ToneCurve>().ToArray();

        for (var j = 0; j < 3; j++)
        {
            for (var i = 0; i < 257; i++)
                labTable[j].table16[i] = (ushort)(((i * 0xFFFF) + 0x80) >> 8);

            labTable[j].table16[257] = 0xFFFF;
        }

        var mpe = AllocToneCurves(state, 3, labTable);
        ToneCurve.DisposeTriple(labTable!);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.LabV2toV4Elem;
        return mpe;
    }

    internal static Stage? AllocLabV4ToV2(object? state)
    {
        /**  Original Code (cmslut.c line: 1032)
         **
         **  // Reverse direction
         **  cmsStage* CMSEXPORT _cmsStageAllocLabV4ToV2(cmsContext ContextID)
         **  {
         **      static const cmsFloat64Number V4ToV2[] = { 65280.0/65535.0, 0, 0,
         **                                       0, 65280.0/65535.0, 0,
         **                                       0, 0, 65280.0/65535.0
         **                                       };
         **
         **       cmsStage *mpe = cmsStageAllocMatrix(ContextID, 3, 3, V4ToV2, NULL);
         **
         **      if (mpe == NULL) return mpe;
         **      mpe ->Implements = cmsSigLabV4toV2;
         **      return mpe;
         **  }
         **/

        var v4ToV2 = new double[]
        {
            65280.0 / 65535.0, 0, 0,
            0, 65280.0 / 65535.0, 0,
            0, 0, 65280.0 / 65535.0
        };

        var mpe = AllocMatrix(state, 3, 3, v4ToV2, null);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.LabV4toV2Elem;
        return mpe;
    }

    internal static Stage? AllocXyz2Lab(object? state) =>
        /**  Original Code (cmslut.c line: 1176)
         **
         **  cmsStage* CMSEXPORT _cmsStageAllocXYZ2Lab(cmsContext ContextID)
         **  {
         **      return _cmsStageAllocPlaceholder(ContextID, cmsSigXYZ2LabElemType, 3, 3, EvaluateXYZ2Lab, NULL, NULL, NULL);
         **
         **  }
         **/

        Alloc(state, Signature.Stage.XYZ2LabElem, 3, 3, new XYZ2LabData());

    internal static Stage? ClipNegatives(object? state, uint numChannels) =>
        /**  Original Code (cmslut.c line: 1141)
         **
         **  cmsStage*  _cmsStageClipNegatives(cmsContext ContextID, cmsUInt32Number nChannels)
         **  {
         **         return _cmsStageAllocPlaceholder(ContextID, cmsSigClipNegativesElemType,
         **                nChannels, nChannels, Clipper, NULL, NULL, NULL);
         **  }
         **/

        Alloc(state, Signature.Stage.ClipNegativesElem, numChannels, numChannels, new ClipperData());

    internal static Stage? NormalizeFromLabFloat(object? state)
    {
        /**  Original Code (cmslut.c line: 1048)
         **
         **  // To Lab to float. Note that the MPE gives numbers in normal Lab range
         **  // and we need 0..1.0 range for the formatters
         **  // L* : 0...100 => 0...1.0  (L* / 100)
         **  // ab* : -128..+127 to 0..1  ((ab* + 128) / 255)
         **
         **  cmsStage* _cmsStageNormalizeFromLabFloat(cmsContext ContextID)
         **  {
         **      static const cmsFloat64Number a1[] = {
         **          1.0/100.0, 0, 0,
         **          0, 1.0/255.0, 0,
         **          0, 0, 1.0/255.0
         **      };
         **
         **      static const cmsFloat64Number o1[] = {
         **          0,
         **          128.0/255.0,
         **          128.0/255.0
         **      };
         **
         **      cmsStage *mpe = cmsStageAllocMatrix(ContextID, 3, 3, a1, o1);
         **
         **      if (mpe == NULL) return mpe;
         **      mpe ->Implements = cmsSigLab2FloatPCS;
         **      return mpe;
         **  }
         **/

        var a1 = new double[]
        {
            1.0 / 100.0, 0, 0,
            0, 1.0 / 255.0, 0,
            0, 0, 1.0 / 255.0
        };

        var o1 = new double[]
        {
            0,
            128.0/255.0,
            128.0/255.0
        };

        var mpe = AllocMatrix(state, 3, 3, a1, o1);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.Lab2FloatPCS;
        return mpe;
    }

    internal static Stage? NormalizeFromXyzFloat(object? state)
    {
        /**  Original Code (cmslut.c line: 1074)
         **
         **  // Fom XYZ to floating point PCS
         **  cmsStage* _cmsStageNormalizeFromXyzFloat(cmsContext ContextID)
         **  {
         **  #define n (32768.0/65535.0)
         **      static const cmsFloat64Number a1[] = {
         **          n, 0, 0,
         **          0, n, 0,
         **          0, 0, n
         **      };
         **  #undef n
         **
         **      cmsStage *mpe =  cmsStageAllocMatrix(ContextID, 3, 3, a1, NULL);
         **
         **      if (mpe == NULL) return mpe;
         **      mpe ->Implements = cmsSigXYZ2FloatPCS;
         **      return mpe;
         **  }
         **/

        const double n = 32768.0 / 65535.0;

        var a1 = new double[]
        {
            n, 0, 0,
            0, n, 0,
            0, 0, n
        };

        var mpe = AllocMatrix(state, 3, 3, a1, null);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.XYZ2FloatPCS;
        return mpe;
    }

    internal static Stage? NormalizeToLabFloat(object? state)
    {
        /**  Original Code (cmslut.c line: 1092)
         **
         **  cmsStage* _cmsStageNormalizeToLabFloat(cmsContext ContextID)
         **  {
         **      static const cmsFloat64Number a1[] = {
         **          100.0, 0, 0,
         **          0, 255.0, 0,
         **          0, 0, 255.0
         **      };
         **
         **      static const cmsFloat64Number o1[] = {
         **          0,
         **          -128.0,
         **          -128.0
         **      };
         **
         **      cmsStage *mpe =  cmsStageAllocMatrix(ContextID, 3, 3, a1, o1);
         **      if (mpe == NULL) return mpe;
         **      mpe ->Implements = cmsSigFloatPCS2Lab;
         **      return mpe;
         **  }
         **/

        var a1 = new double[]
        {
            100.0, 0, 0,
            0, 255.0, 0,
            0, 0, 255.0
        };

        var o1 = new double[]
        {
            0,
            -128.0,
            -128.0
        };

        var mpe = AllocMatrix(state, 3, 3, a1, o1);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.FloatPCS2Lab;
        return mpe;
    }

    internal static Stage? NormalizeToXyzFloat(object? state)
    {
        /**  Original Code (cmslut.c line: 1112)
         **
         **  cmsStage* _cmsStageNormalizeToXyzFloat(cmsContext ContextID)
         **  {
         **  #define n (65535.0/32768.0)
         **
         **      static const cmsFloat64Number a1[] = {
         **          n, 0, 0,
         **          0, n, 0,
         **          0, 0, n
         **      };
         **  #undef n
         **
         **      cmsStage *mpe = cmsStageAllocMatrix(ContextID, 3, 3, a1, NULL);
         **      if (mpe == NULL) return mpe;
         **      mpe ->Implements = cmsSigFloatPCS2XYZ;
         **      return mpe;
         **  }
         **/

        const double n = 65535.0 / 32768.0;

        var a1 = new double[]
        {
            n, 0, 0,
            0, n, 0,
            0, 0, n
        };

        var mpe = AllocMatrix(state, 3, 3, a1, null);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.FloatPCS2XYZ;
        return mpe;
    }

    #endregion Internal Methods

    #region Private Methods

    private static Stage? AllocIdentityCurves(object? state, uint numChannels)
    {
        /** Original Code (cmslut.c line: 294)
         **
         ** // Create a bunch of identity curves
         ** cmsStage* CMSEXPORT _cmsStageAllocIdentityCurves(cmsContext ContextID, cmsUInt32Number nChannels)
         ** {
         **     cmsStage* mpe = cmsStageAllocToneCurves(ContextID, nChannels, NULL);
         **
         **     if (mpe == NULL) return NULL;
         **     mpe ->Implements = cmsSigIdentityElemType;
         **     return mpe;
         ** }
         **/

        var mpe = AllocToneCurves(state, numChannels, null);
        if (mpe is null) return null;

        mpe.implements = Signature.Stage.IdentityElem;
        return mpe;
    }

    private static bool IdentitySampler(ReadOnlySpan<ushort> @in, Span<ushort> @out, in object? cargo)
    {
        /**  Original Code (cmslut.c line: 696)
         **
         **  static
         **  int IdentitySampler(CMSREGISTER const cmsUInt16Number In[], CMSREGISTER cmsUInt16Number Out[], CMSREGISTER void * Cargo)
         **  {
         **      int nChan = *(int*) Cargo;
         **      int i;
         **
         **      for (i=0; i < nChan; i++)
         **          Out[i] = In[i];
         **
         **      return 1;
         **  }
         **/

        if (cargo is not int numChan ||
            @in.Length < numChan ||
            @out.Length < numChan)
        {
            return false;
        }

        for (var i = 0; i < numChan; i++)
            @out[i] = @in[i];

        return true;
    }

    #endregion Private Methods
}
