using System.Runtime.InteropServices;

namespace lcms2.types;

public partial class Stage
{
    public bool Sample(CLutFloatData.Sampler sampler, in object? cargo, SamplerFlags flags) =>
        (Data as CLutFloatData)?.Sample(sampler, cargo, flags) ?? throw new InvalidOperationException();

    public class CLutFloatData: StageData
    {
        public delegate bool Sampler(ReadOnlySpan<float> @in, Span<float> @out, in object? cargo);

        public uint NumEntries;
        public InterpParams Params;
        public float[] Table;

        /*  Original Code (cmslut.c line: 809)
         *  
         *  // Same as anterior, but for floating point
         *  cmsBool CMSEXPORT cmsStageSampleCLutFloat(cmsStage* mpe, cmsSAMPLERFLOAT Sampler, void * Cargo, cmsUInt32Number dwFlags)
         *  {
         *      int i, t, index, rest;
         *      cmsUInt32Number nTotalPoints;
         *      cmsUInt32Number nInputs, nOutputs;
         *      cmsUInt32Number* nSamples;
         *      cmsFloat32Number In[MAX_INPUT_DIMENSIONS+1], Out[MAX_STAGE_CHANNELS];
         *      _cmsStageCLutData* clut = (_cmsStageCLutData*) mpe->Data;
         *
         *      nSamples = clut->Params ->nSamples;
         *      nInputs  = clut->Params ->nInputs;
         *      nOutputs = clut->Params ->nOutputs;
         *
         *      if (nInputs <= 0) return FALSE;
         *      if (nOutputs <= 0) return FALSE;
         *      if (nInputs  > MAX_INPUT_DIMENSIONS) return FALSE;
         *      if (nOutputs >= MAX_STAGE_CHANNELS) return FALSE;
         *
         *      nTotalPoints = CubeSize(nSamples, nInputs);
         *      if (nTotalPoints == 0) return FALSE;
         *
         *      index = 0;
         *      for (i = 0; i < (int)nTotalPoints; i++) {
         *
         *          rest = i;
         *          for (t = (int) nInputs-1; t >=0; --t) {
         *
         *              cmsUInt32Number  Colorant = rest % nSamples[t];
         *
         *              rest /= nSamples[t];
         *
         *              In[t] =  (cmsFloat32Number) (_cmsQuantizeVal(Colorant, nSamples[t]) / 65535.0);
         *          }
         *
         *          if (clut ->Tab.TFloat != NULL) {
         *              for (t=0; t < (int) nOutputs; t++)
         *                  Out[t] = clut->Tab.TFloat[index + t];
         *          }
         *
         *          if (!Sampler(In, Out, Cargo))
         *              return FALSE;
         *
         *          if (!(dwFlags & SAMPLER_INSPECT)) {
         *
         *              if (clut ->Tab.TFloat != NULL) {
         *                  for (t=0; t < (int) nOutputs; t++)
         *                      clut->Tab.TFloat[index + t] = Out[t];
         *              }
         *          }
         *
         *          index += nOutputs;
         *      }
         *
         *      return TRUE;
         *  }
         */
        public bool Sample(Sampler sampler, in object? cargo, SamplerFlags flags)
        {
            var @in = new float[maxInputDimensions + 1];
            var @out = new float[maxInputDimensions];

            var numSamples = Params.NumSamples;
            var numInputs = Params.NumInputs;
            var numOutputs = Params.NumOutputs;

            if ((numInputs is <= 0 or > maxInputDimensions) ||
                (numOutputs is <= 0 or > maxStageChannels))
            {
                return false;
            }

            var numTotalPoints = CubeSize(numSamples, (int)numInputs);
            if (numTotalPoints is 0) return false;

            var index = 0u;
            for (var i = 0; i < numTotalPoints; i++)
            {
                var rest = i;
                for (var t = (int)numInputs - 1; t >= 0; --t)
                {
                    var colorant = rest % numSamples[t];

                    rest /= (int)numSamples[i];

                    @in[i] = QuantizeValue(colorant, numSamples[t]) / 65535f;
                }

                for (var t = 0; t < numOutputs; t++)
                    @out[i] = Table[index + t];

                if (!sampler(@in, @out, in cargo))
                    return false;

                if (flags is not SamplerFlags.Inspect)
                {
                    for (var t = 0; t < numOutputs; t++)
                        Table[index + t] = @out[t];
                }

                index += numOutputs;
            }

            return true;
        }

        /*  Original Code (cmslut.c line: 901)
         *  
         *  cmsInt32Number CMSEXPORT cmsSliceSpaceFloat(cmsUInt32Number nInputs, const cmsUInt32Number clutPoints[],
         *                                              cmsSAMPLERFLOAT Sampler, void * Cargo)
         *  {
         *      int i, t, rest;
         *      cmsUInt32Number nTotalPoints;
         *      cmsFloat32Number In[cmsMAXCHANNELS];
         *
         *      if (nInputs >= cmsMAXCHANNELS) return FALSE;
         *
         *      nTotalPoints = CubeSize(clutPoints, nInputs);
         *      if (nTotalPoints == 0) return FALSE;
         *
         *      for (i = 0; i < (int) nTotalPoints; i++) {
         *
         *          rest = i;
         *          for (t = (int) nInputs-1; t >=0; --t) {
         *
         *              cmsUInt32Number  Colorant = rest % clutPoints[t];
         *
         *              rest /= clutPoints[t];
         *              In[t] =  (cmsFloat32Number) (_cmsQuantizeVal(Colorant, clutPoints[t]) / 65535.0);
         *
         *          }
         *
         *          if (!Sampler(In, NULL, Cargo))
         *              return FALSE;
         *      }
         *
         *      return TRUE;
         *  }
         */
        public static bool SliceSpace(uint numInputs, in uint[] clutPoints, Sampler sampler, in object cargo)
        {
            var @in = new float[maxChannels];

            if (numInputs >= maxChannels) return false;

            var numTotalPoints = CubeSize(clutPoints, (int)numInputs);
            if (numTotalPoints is 0) return false;

            for (var i = 0; i < numTotalPoints; i++)
            {
                var rest = i;
                for (var t = (int)numInputs - 1; t >= 0; --t)
                {
                    var colorant = rest % clutPoints[t];

                    rest /= (int)clutPoints[t];
                    @in[t] = QuantizeValue(colorant, clutPoints[t]) / 65535f;
                }

                if (!sampler(@in, null, in cargo)) return false;
            }

            return true;
        }

        internal CLutFloatData(float[] table, InterpParams @params, uint numEntries)
        {
            Table = table;
            Params = @params;
            NumEntries = numEntries;
        }

        /*  Original Code (cmslut.c line: 481)
         *  
         *  static
         *  void* CLUTElemDup(cmsStage* mpe)
         *  {
         *      _cmsStageCLutData* Data = (_cmsStageCLutData*) mpe ->Data;
         *      _cmsStageCLutData* NewElem;
         *
         *
         *      NewElem = (_cmsStageCLutData*) _cmsMallocZero(mpe ->ContextID, sizeof(_cmsStageCLutData));
         *      if (NewElem == NULL) return NULL;
         *
         *      NewElem ->nEntries       = Data ->nEntries;
         *      NewElem ->HasFloatValues = Data ->HasFloatValues;
         *
         *      if (Data ->Tab.T) {
         *
         *          if (Data ->HasFloatValues) {
         *              NewElem ->Tab.TFloat = (cmsFloat32Number*) _cmsDupMem(mpe ->ContextID, Data ->Tab.TFloat, Data ->nEntries * sizeof *(cmsFloat32Number));
         *              if (NewElem ->Tab.TFloat == NULL)
         *                  goto Error;
         *          } else {
         *              NewElem ->Tab.T = (cmsUInt16Number*) _cmsDupMem(mpe ->ContextID, Data ->Tab.T, Data ->nEntries * sizeof *(cmsUInt16Number));
         *              if (NewElem ->Tab.T == NULL)
         *                  goto Error;
         *          }
         *      }
         *
         *      NewElem ->Params   = _cmsComputeInterpParamsEx(mpe ->ContextID,
         *                                                     Data ->Params ->nSamples,
         *                                                     Data ->Params ->nInputs,
         *                                                     Data ->Params ->nOutputs,
         *                                                     NewElem ->Tab.T,
         *                                                     Data ->Params ->dwFlags);
         *      if (NewElem->Params != NULL)
         *          return (void*) NewElem;
         *   Error:
         *      if (NewElem->Tab.T)
         *          // This works for both types
         *          _cmsFree(mpe ->ContextID, NewElem -> Tab.T);
         *      _cmsFree(mpe ->ContextID, NewElem);
         *      return NULL;
         *  }
         */
        internal override StageData? Duplicate(Stage parent)
        {
            var p =
                InterpParams.Compute(
                    parent.StateContainer,
                    Params.NumSamples,
                    Params.NumInputs,
                    Params.NumOutputs,
                    Table,
                    Params.Flags);

            if (p is null) return null;

            return
                new CLutFloatData(
                    (float[])Table.Clone(),
                    p,
                    NumEntries);
        }

        /*  Original Code (cmslut.c line: 433)
         *  
         *  // Evaluate in true floating point
         *  static
         *  void EvaluateCLUTfloat(const cmsFloat32Number In[], cmsFloat32Number Out[], const cmsStage *mpe)
         *  {
         *      _cmsStageCLutData* Data = (_cmsStageCLutData*) mpe ->Data;
         *
         *      Data -> Params ->Interpolation.LerpFloat(In, Out, Data->Params);
         *  }
         */
        internal override void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage _) =>
            Params.LerpFloat(@in, @out);

        /*  Original Code (cmslut.c line: 524)
         *  
         *  static
         *  void CLutElemTypeFree(cmsStage* mpe)
         *  {
         *
         *      _cmsStageCLutData* Data = (_cmsStageCLutData*) mpe ->Data;
         *
         *      // Already empty
         *      if (Data == NULL) return;
         *
         *      // This works for both types
         *      if (Data -> Tab.T)
         *          _cmsFree(mpe ->ContextID, Data -> Tab.T);
         *
         *      _cmsFreeInterpParams(Data ->Params);
         *      _cmsFree(mpe ->ContextID, mpe ->Data);
         *  }
         */
        // Free is lifted up to base class as Dispose
    }
}

public enum SamplerFlags
{
    None = 0,
    Inspect = 0x01000000,
}