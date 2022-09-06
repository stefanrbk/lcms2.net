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
using lcms2.state;

using System.Runtime.CompilerServices;

namespace lcms2.types;

/** Original Code (lcms2_plugin.h line: 261)
 **
 ** // 16 bits forward interpolation. This function performs precision-limited linear interpolation
 ** // and is supposed to be quite fast. Implementation may be tetrahedral or trilinear, and plug-ins may
 ** // choose to implement any other interpolation algorithm.
 ** typedef void (* _cmsInterpFn16)(CMSREGISTER const cmsUInt16Number Input[],
 **                                 CMSREGISTER cmsUInt16Number Output[],
 **                                 CMSREGISTER const struct _cms_interp_struc* p);
 **/

public delegate void InterpFn16(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p);

/** Original Code (lcms2_plugin.h line: 311)
 **
 ** // Interpolators factory
 ** typedef cmsInterpFunction (* cmsInterpFnFactory)(cmsUInt32Number nInputChannels, cmsUInt32Number nOutputChannels, cmsUInt32Number dwFlags);
 **/

public delegate InterpFunction? InterpFnFactory(uint numInputChannels, uint numOutputChannels, LerpFlag flags);

/** Original Code (lcms2_plugin.h line: 268)
 **
 ** // Floating point forward interpolation. Full precision interpolation using floats. This is not a
 ** // time critical function. Implementation may be tetrahedral or trilinear, and plug-ins may
 ** // choose to implement any other interpolation algorithm.
 ** typedef void (* _cmsInterpFnFloat)(cmsFloat32Number const Input[],
 **                                    cmsFloat32Number Output[],
 **                                    const struct _cms_interp_struc* p);
 **/

public delegate void InterpFnFloat(ReadOnlySpan<float> input, Span<float> output, InterpParams p);

[Flags]
public enum LerpFlag
{
    /** Original Code (lcms2_plugin.h line: 283)
     **
     ** // Flags for interpolator selection
     ** #define CMS_LERP_FLAGS_16BITS             0x0000        // The default
     ** #define CMS_LERP_FLAGS_FLOAT              0x0001        // Requires different implementation
     ** #define CMS_LERP_FLAGS_TRILINEAR          0x0100        // Hint only
     **/

    Ushort = 0,
    Float = 1,
    Trilinear = 4
}

public class InterpParams : ICloneable
{
    /** Original Code (lcms2_plugin.h line: 291)
     **
     ** typedef struct _cms_interp_struc {  // Used on all interpolations. Supplied by lcms2 when calling the interpolation function
     **
     **     cmsContext ContextID;     // The calling thread
     **
     **     cmsUInt32Number dwFlags;  // Keep original flags
     **     cmsUInt32Number nInputs;  // != 1 only in 3D interpolation
     **     cmsUInt32Number nOutputs; // != 1 only in 3D interpolation
     **
     **     cmsUInt32Number nSamples[MAX_INPUT_DIMENSIONS];  // Valid on all kinds of tables
     **     cmsUInt32Number Domain[MAX_INPUT_DIMENSIONS];    // Domain = nSamples - 1
     **
     **     cmsUInt32Number opta[MAX_INPUT_DIMENSIONS];     // Optimization for 3D CLUT. This is the number of nodes premultiplied for each
     **                                                     // dimension. For example, in 7 nodes, 7, 7^2 , 7^3, 7^4, etc. On non-regular
     **                                                     // Samplings may vary according of the number of nodes for each dimension.
     **
     **     const void *Table;                // Points to the actual interpolation table
     **     cmsInterpFunction Interpolation;  // Points to the function to do the interpolation
     **
     **  } cmsInterpParams;
     **/

    #region Fields

    public const int MaxInputDimensions = 15;

    public int[] Domain = new int[MaxInputDimensions];

    public LerpFlag Flags;

    public InterpFunction? Interpolation;

    public uint NumInputs;

    public uint NumOutputs;

    public uint[] NumSamples = new uint[MaxInputDimensions];

    public int[] Opta = new int[MaxInputDimensions];

    public object? StateContainer;

    public object Table;

    #endregion Fields

    #region Public Constructors

    public InterpParams(object? state, LerpFlag flags, uint numInputs, uint numOutputs, object table)
    {
        StateContainer = state;
        Flags = flags;
        NumInputs = numInputs;
        NumOutputs = numOutputs;
        Table = table;
        Interpolation = default;
    }

    #endregion Public Constructors

    #region Properties

    public ushort[] Table16 =>
        (Flags & LerpFlag.Float) == 0
            ? (ushort[])Table
            : throw new InvalidOperationException();

    public float[] TableFloat =>
        (Flags & LerpFlag.Float) != 0
            ? (float[])Table
            : throw new InvalidOperationException();

    #endregion Properties

    #region Public Methods

    public object Clone() =>
        new InterpParams(StateContainer, Flags, NumInputs, NumOutputs, Table)
        {
            NumSamples = (uint[])NumSamples.Clone(),
            Domain = (int[])Domain.Clone(),
            Opta = (int[])Opta.Clone(),
            Interpolation = (InterpFunction?)Interpolation?.Clone(),
        };

    public void Lerp(ReadOnlySpan<ushort> input, Span<ushort> output)
    {
        if ((Flags & LerpFlag.Float) == 0)
            Interpolation?.Lerp(input, output, this);
        else
            throw new InvalidOperationException();
    }

    public void Lerp(ReadOnlySpan<float> input, Span<float> output)
    {
        if ((Flags & LerpFlag.Float) != 0)
            Interpolation?.Lerp(input, output, this);
        else
            throw new InvalidOperationException();
    }

    #endregion Public Methods

    #region Internal Methods

    internal static InterpParams? Compute(object? state, in uint[] numSamples, uint inputChan, uint outputChan, object table, LerpFlag flags)
    {
        /** Original Code (cmsintrp.c line: 109)
         **
         ** // This function precalculates as many parameters as possible to speed up the interpolation.
         ** cmsInterpParams* _cmsComputeInterpParamsEx(cmsContext ContextID,
         **                                            const cmsUInt32Number nSamples[],
         **                                            cmsUInt32Number InputChan, cmsUInt32Number OutputChan,
         **                                            const void *Table,
         **                                            cmsUInt32Number dwFlags)
         ** {
         **     cmsInterpParams* p;
         **     cmsUInt32Number i;
         **
         **     // Check for maximum inputs
         **     if (InputChan > MAX_INPUT_DIMENSIONS) {
         **              cmsSignalError(ContextID, cmsERROR_RANGE, "Too many input channels (%d channels, max=%d)", InputChan, MAX_INPUT_DIMENSIONS);
         **             return NULL;
         **     }
         **
         **     // Creates an empty object
         **     p = (cmsInterpParams*) _cmsMallocZero(ContextID, sizeof(cmsInterpParams));
         **     if (p == NULL) return NULL;
         **
         **     // Keep original parameters
         **     p -> dwFlags  = dwFlags;
         **     p -> nInputs  = InputChan;
         **     p -> nOutputs = OutputChan;
         **     p ->Table     = Table;
         **     p ->ContextID  = ContextID;
         **
         **     // Fill samples per input direction and domain (which is number of nodes minus one)
         **     for (i=0; i < InputChan; i++) {
         **
         **         p -> nSamples[i] = nSamples[i];
         **         p -> Domain[i]   = nSamples[i] - 1;
         **     }
         **
         **     // Compute factors to apply to each component to index the grid array
         **     p -> opta[0] = p -> nOutputs;
         **     for (i=1; i < InputChan; i++)
         **         p ->opta[i] = p ->opta[i-1] * nSamples[InputChan-i];
         **
         **
         **     if (!_cmsSetInterpolationRoutine(ContextID, p)) {
         **          cmsSignalError(ContextID, cmsERROR_UNKNOWN_EXTENSION, "Unsupported interpolation (%d->%d channels)", InputChan, OutputChan);
         **         _cmsFree(ContextID, p);
         **         return NULL;
         **     }
         **
         **     // All seems ok
         **     return p;
         ** }
         **/

        // Check for maximum inputs
        if (inputChan > MaxInputDimensions)
        {
            State.SignalError(state, ErrorCode.Range, "Too many input channels ({0} channels, max={1})", inputChan, MaxInputDimensions);
            return null;
        }

        // Creates an empty object and keep original parameters
        var p = new InterpParams(state, flags, inputChan, outputChan, table);

        // Fill samples per input direction and domain (which is number of nodes minus one)
        for (var i = 0; i < inputChan; i++)
        {
            p.NumSamples[i] = numSamples[i];
            p.Domain[i] = (int)numSamples[i] - 1;
        }

        // Compute factors to apply to each component to index the grid array
        p.Opta[0] = (int)p.NumOutputs;
        for (var i = 1; i < inputChan; i++)
            p.Opta[i] = (int)(p.Opta[i - 1] * numSamples[inputChan - i]);

        if (!p.SetInterpolationRoutine(state))
        {
            State.SignalError(state, ErrorCode.UnknownExtension, "Unsupported interpolation ({0}->{1} channels)", inputChan, outputChan);
            return null;
        }

        // All seems ok
        return p;
    }

    internal static InterpParams? Compute(object? state, uint numSamples, uint inputChan, uint outputChan, object table, LerpFlag flags)
    {
        /**  Original Code (cmsintrp.c line: 160)
         **
         **  // This one is a wrapper on the anterior, but assuming all directions have same number of nodes
         **  cmsInterpParams* CMSEXPORT _cmsComputeInterpParams(cmsContext ContextID, cmsUInt32Number nSamples,
         **                                                     cmsUInt32Number InputChan, cmsUInt32Number OutputChan, const void* Table, cmsUInt32Number dwFlags)
         **  {
         **      int i;
         **      cmsUInt32Number Samples[MAX_INPUT_DIMENSIONS];
         **
         **      // Fill the auxiliary array
         **      for (i=0; i < MAX_INPUT_DIMENSIONS; i++)
         **          Samples[i] = nSamples;
         **
         **      // Call the extended function
         **      return _cmsComputeInterpParamsEx(ContextID, Samples, InputChan, OutputChan, Table, dwFlags);
         **  }
         **/

        var samples = new uint[MaxInputDimensions];

        for (var i = 0; i < MaxInputDimensions; i++)
            samples[i] = numSamples;

        return Compute(state, samples, inputChan, outputChan, table, flags);
    }

    internal static InterpFunction? DefaultInterpolatorsFactory(uint numInputChannels, uint numOutputChannels, LerpFlag flags)
    {
        /**  Original Code (cmsintrp.c line: 1177)
         **
         **  // The default factory
         **  static
         **  cmsInterpFunction DefaultInterpolatorsFactory(cmsUInt32Number nInputChannels, cmsUInt32Number nOutputChannels, cmsUInt32Number dwFlags)
         **  {
         **
         **      cmsInterpFunction Interpolation;
         **      cmsBool  IsFloat     = (dwFlags & CMS_LERP_FLAGS_FLOAT);
         **      cmsBool  IsTrilinear = (dwFlags & CMS_LERP_FLAGS_TRILINEAR);
         **
         **      memset(&Interpolation, 0, sizeof(Interpolation));
         **
         **      // Safety check
         **      if (nInputChannels >= 4 && nOutputChannels >= MAX_STAGE_CHANNELS)
         **          return Interpolation;
         **
         **      switch (nInputChannels) {
         **
         **             case 1: // Gray LUT / linear
         **
         **                 if (nOutputChannels == 1) {
         **
         **                     if (IsFloat)
         **                         Interpolation.LerpFloat = LinLerp1Dfloat;
         **                     else
         **                         Interpolation.Lerp16 = LinLerp1D;
         **
         **                 }
         **                 else {
         **
         **                     if (IsFloat)
         **                         Interpolation.LerpFloat = Eval1InputFloat;
         **                     else
         **                         Interpolation.Lerp16 = Eval1Input;
         **                 }
         **                 break;
         **
         **             case 2: // Duotone
         **                 if (IsFloat)
         **                        Interpolation.LerpFloat =  BilinearInterpFloat;
         **                 else
         **                        Interpolation.Lerp16    =  BilinearInterp16;
         **                 break;
         **
         **             case 3:  // RGB et al
         **
         **                 if (IsTrilinear) {
         **
         **                     if (IsFloat)
         **                         Interpolation.LerpFloat = TrilinearInterpFloat;
         **                     else
         **                         Interpolation.Lerp16 = TrilinearInterp16;
         **                 }
         **                 else {
         **
         **                     if (IsFloat)
         **                         Interpolation.LerpFloat = TetrahedralInterpFloat;
         **                     else {
         **
         **                         Interpolation.Lerp16 = TetrahedralInterp16;
         **                     }
         **                 }
         **                 break;
         **
         **             case 4:  // CMYK lut
         **
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat =  Eval4InputsFloat;
         **                 else
         **                     Interpolation.Lerp16    =  Eval4Inputs;
         **                 break;
         **
         **             case 5: // 5 Inks
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat =  Eval5InputsFloat;
         **                 else
         **                     Interpolation.Lerp16    =  Eval5Inputs;
         **                 break;
         **
         **             case 6: // 6 Inks
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat =  Eval6InputsFloat;
         **                 else
         **                     Interpolation.Lerp16    =  Eval6Inputs;
         **                 break;
         **
         **             case 7: // 7 inks
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat =  Eval7InputsFloat;
         **                 else
         **                     Interpolation.Lerp16    =  Eval7Inputs;
         **                 break;
         **
         **             case 8: // 8 inks
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat =  Eval8InputsFloat;
         **                 else
         **                     Interpolation.Lerp16    =  Eval8Inputs;
         **                 break;
         **
         **             case 9:
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat = Eval9InputsFloat;
         **                 else
         **                     Interpolation.Lerp16 = Eval9Inputs;
         **                 break;
         **
         **             case 10:
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat = Eval10InputsFloat;
         **                 else
         **                     Interpolation.Lerp16 = Eval10Inputs;
         **                 break;
         **
         **             case 11:
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat = Eval11InputsFloat;
         **                 else
         **                     Interpolation.Lerp16 = Eval11Inputs;
         **                 break;
         **
         **             case 12:
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat = Eval12InputsFloat;
         **                 else
         **                     Interpolation.Lerp16 = Eval12Inputs;
         **                 break;
         **
         **             case 13:
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat = Eval13InputsFloat;
         **                 else
         **                     Interpolation.Lerp16 = Eval13Inputs;
         **                 break;
         **
         **             case 14:
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat = Eval14InputsFloat;
         **                 else
         **                     Interpolation.Lerp16 = Eval14Inputs;
         **                 break;
         **
         **             case 15:
         **                 if (IsFloat)
         **                     Interpolation.LerpFloat = Eval15InputsFloat;
         **                 else
         **                     Interpolation.Lerp16 = Eval15Inputs;
         **                 break;
         **
         **             default:
         **                 Interpolation.Lerp16 = NULL;
         **      }
         **
         **      return Interpolation;
         **  }
         **/

        InterpFunction? interpolation = null;
        var isFloat = (flags & LerpFlag.Float) != 0;
        var isTriliniar = (flags & LerpFlag.Trilinear) != 0;

        // Safety check
        if (numInputChannels >= 4 && numOutputChannels >= maxStageChannels)
            return default;

        switch (numInputChannels)
        {
            case 1: // Gray Lut / linear

                interpolation = numOutputChannels == 1
                    ? isFloat
                        ? new InterpFunction(LinLerp1DFloat)
                        : new InterpFunction(LinLerp1D16)
                    : isFloat
                        ? new InterpFunction(Eval1InputFloat)
                        : new InterpFunction(Eval1Input16);
                break;

            case 2: // Duotone

                interpolation = isFloat
                    ? new InterpFunction(BilinearInterpFloat)
                    : new InterpFunction(BilinearInterp16);

                break;

            case 3: // RGB et al

                interpolation = isTriliniar
                    ? isFloat
                        ? new InterpFunction(TrilinearInterpFloat)
                        : new InterpFunction(TrilinearInterp16)
                    : isFloat
                        ? new InterpFunction(TetrahedralInterpFloat)
                        : new InterpFunction(TetrahedralInterp16);
                break;

            case 4: // CMYK lut

                interpolation = isFloat
                    ? new InterpFunction(Eval4InputsFloat)
                    : new InterpFunction(Eval4Inputs16);

                break;

            case 5:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(5))
                    : new InterpFunction(EvalXInputs16(5));

                break;

            case 6:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(6))
                    : new InterpFunction(EvalXInputs16(6));

                break;

            case 7:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(7))
                    : new InterpFunction(EvalXInputs16(7));

                break;

            case 8:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(8))
                    : new InterpFunction(EvalXInputs16(8));

                break;

            case 9:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(9))
                    : new InterpFunction(EvalXInputs16(9));

                break;

            case 10:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(10))
                    : new InterpFunction(EvalXInputs16(10));

                break;

            case 11:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(11))
                    : new InterpFunction(EvalXInputs16(11));

                break;

            case 12:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(12))
                    : new InterpFunction(EvalXInputs16(12));

                break;

            case 13:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(13))
                    : new InterpFunction(EvalXInputs16(13));

                break;

            case 14:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(14))
                    : new InterpFunction(EvalXInputs16(14));

                break;

            case 15:

                interpolation = isFloat
                    ? new InterpFunction(EvalXInputsFloat(15))
                    : new InterpFunction(EvalXInputs16(15));

                break;
        }
        return interpolation;
    }

    internal bool SetInterpolationRoutine(object? state)
    {
        /**  Original Code (cmsintrp.c line: 84)
         **
         **  // Set the interpolation method
         **  cmsBool _cmsSetInterpolationRoutine(cmsContext ContextID, cmsInterpParams* p)
         **  {
         **      _cmsInterpPluginChunkType* ptr = (_cmsInterpPluginChunkType*) _cmsContextGetClientChunk(ContextID, InterpPlugin);
         **
         **      p ->Interpolation.Lerp16 = NULL;
         **
         **     // Invoke factory, possibly in the Plug-in
         **      if (ptr ->Interpolators != NULL)
         **          p ->Interpolation = ptr->Interpolators(p -> nInputs, p ->nOutputs, p ->dwFlags);
         **
         **      // If unsupported by the plug-in, go for the LittleCMS default.
         **      // If happens only if an extern plug-in is being used
         **      if (p ->Interpolation.Lerp16 == NULL)
         **          p ->Interpolation = DefaultInterpolatorsFactory(p ->nInputs, p ->nOutputs, p ->dwFlags);
         **
         **      // Check for valid interpolator (we just check one member of the union)
         **      if (p ->Interpolation.Lerp16 == NULL) {
         **              return FALSE;
         **      }
         **
         **      return TRUE;
         **  }
         **/

        var ptr = State.GetInterpolationPlugin(state);

        Interpolation = null;

        // Invoke factory, possibly in the Plugin
        if (ptr.interpolators is not null)
            Interpolation = ptr.interpolators(NumInputs, NumOutputs, Flags);

        // If unsupported by the plugin, go for the default. It happens only if an extern plugin is
        // being used
        Interpolation ??= DefaultInterpolatorsFactory(NumInputs, NumOutputs, Flags);

        // Check for valid interpolator (we just check one member of the union
        return Interpolation is not null;
    }

    #endregion Internal Methods

    #region Private Methods

    private static void BilinearInterp16(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line: 409)
         **
         **  // Bilinear interpolation (16 bits) - optimized version
         **  static CMS_NO_SANITIZE
         **  void BilinearInterp16(CMSREGISTER const cmsUInt16Number Input[],
         **                        CMSREGISTER cmsUInt16Number Output[],
         **                        CMSREGISTER const cmsInterpParams* p)
         **
         **  {
         **  #define DENS(i,j) (LutTable[(i)+(j)+OutChan])
         **  #define LERP(a,l,h)     (cmsUInt16Number) (l + ROUND_FIXED_TO_INT(((h-l)*a)))
         **
         **             const cmsUInt16Number* LutTable = (cmsUInt16Number*) p ->Table;
         **             int        OutChan, TotalOut;
         **             cmsS15Fixed16Number    fx, fy;
         **             CMSREGISTER int        rx, ry;
         **             int                    x0, y0;
         **             CMSREGISTER int        X0, X1, Y0, Y1;
         **
         **             int                    d00, d01, d10, d11,
         **                                    dx0, dx1,
         **                                    dxy;
         **
         **      TotalOut   = p -> nOutputs;
         **
         **      fx = _cmsToFixedDomain((int) Input[0] * p -> Domain[0]);
         **      x0  = FIXED_TO_INT(fx);
         **      rx  = FIXED_REST_TO_INT(fx);    // Rest in 0..1.0 domain
         **
         **
         **      fy = _cmsToFixedDomain((int) Input[1] * p -> Domain[1]);
         **      y0  = FIXED_TO_INT(fy);
         **      ry  = FIXED_REST_TO_INT(fy);
         **
         **
         **      X0 = p -> opta[1] * x0;
         **      X1 = X0 + (Input[0] == 0xFFFFU ? 0 : p->opta[1]);
         **
         **      Y0 = p -> opta[0] * y0;
         **      Y1 = Y0 + (Input[1] == 0xFFFFU ? 0 : p->opta[0]);
         **
         **      for (OutChan = 0; OutChan < TotalOut; OutChan++) {
         **
         **          d00 = DENS(X0, Y0);
         **          d01 = DENS(X0, Y1);
         **          d10 = DENS(X1, Y0);
         **          d11 = DENS(X1, Y1);
         **
         **          dx0 = LERP(rx, d00, d10);
         **          dx1 = LERP(rx, d01, d11);
         **
         **          dxy = LERP(ry, dx0, dx1);
         **
         **          Output[OutChan] = (cmsUInt16Number) dxy;
         **      }
         **
         **
         **  #   undef LERP
         **  #   undef DENS
         **  }
         **/

        var lutTable = p.Table16;

        var totalOut = p.NumOutputs;

        var fx = ToFixedDomain(input[0] * p.Domain[0]);
        var x0 = FixedToInt(fx);
        var rx = FixedRestToInt(fx);

        var fy = ToFixedDomain(input[1] * p.Domain[1]);
        var y0 = FixedToInt(fy);
        var ry = FixedRestToInt(fy);

        x0 *= p.Opta[1];
        var x1 = x0 + (input[0] == 0xFFFF ? 0 : p.Opta[1]);

        y0 *= p.Opta[0];
        var y1 = y0 + (input[1] == 0xFFFF ? 0 : p.Opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var d00 = Dens(lutTable, x0, y0, outChan);
            var d01 = Dens(lutTable, x0, y1, outChan);
            var d10 = Dens(lutTable, x1, y0, outChan, outChan);
            var d11 = Dens(lutTable, x1, y1, outChan, outChan);

            var dx0 = Lerp(rx, d00, d10);
            var dx1 = Lerp(rx, d01, d11);

            output[outChan] = Lerp(ry, dx0, dx1);
        }
    }

    private static void BilinearInterpFloat(ReadOnlySpan<float> input, Span<float> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line: 356)
         **
         **  // Bilinear interpolation (16 bits) - cmsFloat32Number version
         **  static
         **  void BilinearInterpFloat(const cmsFloat32Number Input[],
         **                           cmsFloat32Number Output[],
         **                           const cmsInterpParams* p)
         **
         **  {
         **  #   define LERP(a,l,h)    (cmsFloat32Number) ((l)+(((h)-(l))*(a)))
         **  #   define DENS(i,j)      (LutTable[(i)+(j)+OutChan])
         **
         **      const cmsFloat32Number* LutTable = (cmsFloat32Number*) p ->Table;
         **      cmsFloat32Number      px, py;
         **      int        x0, y0,
         **                 X0, Y0, X1, Y1;
         **      int        TotalOut, OutChan;
         **      cmsFloat32Number      fx, fy,
         **          d00, d01, d10, d11,
         **          dx0, dx1,
         **          dxy;
         **
         **      TotalOut   = p -> nOutputs;
         **      px = fclamp(Input[0]) * p->Domain[0];
         **      py = fclamp(Input[1]) * p->Domain[1];
         **
         **      x0 = (int) _cmsQuickFloor(px); fx = px - (cmsFloat32Number) x0;
         **      y0 = (int) _cmsQuickFloor(py); fy = py - (cmsFloat32Number) y0;
         **
         **      X0 = p -> opta[1] * x0;
         **      X1 = X0 + (fclamp(Input[0]) >= 1.0 ? 0 : p->opta[1]);
         **
         **      Y0 = p -> opta[0] * y0;
         **      Y1 = Y0 + (fclamp(Input[1]) >= 1.0 ? 0 : p->opta[0]);
         **
         **      for (OutChan = 0; OutChan < TotalOut; OutChan++) {
         **
         **          d00 = DENS(X0, Y0);
         **          d01 = DENS(X0, Y1);
         **          d10 = DENS(X1, Y0);
         **          d11 = DENS(X1, Y1);
         **
         **          dx0 = LERP(fx, d00, d10);
         **          dx1 = LERP(fx, d01, d11);
         **
         **          dxy = LERP(fy, dx0, dx1);
         **
         **          Output[OutChan] = dxy;
         **      }
         **
         **
         **  #   undef LERP
         **  #   undef DENS
         **  }
         **/

        var lutTable = p.TableFloat;

        var totalOut = p.NumOutputs;
        var px = Fclamp(input[0]) * p.Domain[0];
        var py = Fclamp(input[1]) * p.Domain[1];

        var x0 = QuickFloor(px); var fx = px - x0;
        var y0 = QuickFloor(py); var fy = py - y0;

        x0 *= p.Opta[1];
        var x1 = x0 + (Fclamp(input[0]) >= 1.0 ? 0 : p.Opta[1]);

        y0 *= p.Opta[0];
        var y1 = y0 + (Fclamp(input[1]) >= 1.0 ? 0 : p.Opta[0]);

        for (var outChan = 0; outChan < totalOut; outChan++)
        {
            var d00 = Dens(lutTable, x0, y0, outChan);
            var d01 = Dens(lutTable, x0, y1, outChan);
            var d10 = Dens(lutTable, x1, y0, outChan, outChan);
            var d11 = Dens(lutTable, x1, y1, outChan, outChan);

            var dx0 = Lerp(fx, d00, d10);
            var dx1 = Lerp(fx, d01, d11);

            output[outChan] = Lerp(fy, dx0, dx1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Dens(Span<float> table, int i, int j, int outChan) =>
        table[i + j + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort Dens(Span<ushort> table, int i, int j, int outChan) =>
        table[i + j + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Dens(Span<float> table, int i, int j, int k, int outChan) =>
        table[i + j + k + outChan];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort Dens(Span<ushort> table, int i, int j, int k, int outChan) =>
        table[i + j + k + outChan];

    private static void Eval1Input16(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p16)
    {
        /**  Original Code (cmsintrp.c line: 265)
         **
         **  // Eval gray LUT having only one input channel
         **  static CMS_NO_SANITIZE
         **  void Eval1Input(CMSREGISTER const cmsUInt16Number Input[],
         **                  CMSREGISTER cmsUInt16Number Output[],
         **                  CMSREGISTER const cmsInterpParams* p16)
         **  {
         **         cmsS15Fixed16Number fk;
         **         cmsS15Fixed16Number k0, k1, rk, K0, K1;
         **         int v;
         **         cmsUInt32Number OutChan;
         **         const cmsUInt16Number* LutTable = (cmsUInt16Number*) p16 -> Table;
         **
         **
         **         // if last value...
         **         if (Input[0] == 0xffff || p16->Domain[0] == 0) {
         **
         **             cmsUInt32Number y0 = p16->Domain[0] * p16->opta[0];
         **
         **             for (OutChan = 0; OutChan < p16->nOutputs; OutChan++) {
         **                 Output[OutChan] = LutTable[y0 + OutChan];
         **             }
         **         }
         **         else
         **         {
         **
         **             v = Input[0] * p16->Domain[0];
         **             fk = _cmsToFixedDomain(v);
         **
         **             k0 = FIXED_TO_INT(fk);
         **             rk = (cmsUInt16Number)FIXED_REST_TO_INT(fk);
         **
         **             k1 = k0 + (Input[0] != 0xFFFFU ? 1 : 0);
         **
         **             K0 = p16->opta[0] * k0;
         **             K1 = p16->opta[0] * k1;
         **
         **             for (OutChan = 0; OutChan < p16->nOutputs; OutChan++) {
         **
         **                 Output[OutChan] = LinearInterp(rk, LutTable[K0 + OutChan], LutTable[K1 + OutChan]);
         **             }
         **         }
         **  }
         **/

        var lutTable = p16.Table16;

        if (input[0] == 0xFFFF || p16.Domain[0] == 0)
        {
            var y0 = p16.Domain[0] * p16.Opta[0];

            for (var outChan = 0; outChan < p16.NumOutputs; outChan++)
                output[outChan] = lutTable[y0 + outChan];
        }
        else
        {
            var v = input[0] * p16.Domain[0];
            var fk = ToFixedDomain(v);

            var k0 = FixedToInt(fk);
            var rk = (ushort)FixedRestToInt(fk);

            var k1 = k0 + (input[0] != 0xFFFF ? 1 : 0);

            k0 *= p16.Opta[0];
            k1 *= p16.Opta[0];

            for (var outChan = 0; outChan < p16.NumOutputs; outChan++)
                output[outChan] = LinearInterp(rk, lutTable[k0 + outChan], lutTable[k1 + outChan]);
        }
    }

    private static void Eval1InputFloat(ReadOnlySpan<float> value, Span<float> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line: 310)
         **
         **  // Eval gray LUT having only one input channel
         **  static
         **  void Eval1InputFloat(const cmsFloat32Number Value[],
         **                       cmsFloat32Number Output[],
         **                       const cmsInterpParams* p)
         **  {
         **      cmsFloat32Number y1, y0;
         **      cmsFloat32Number val2, rest;
         **      int cell0, cell1;
         **      cmsUInt32Number OutChan;
         **      const cmsFloat32Number* LutTable = (cmsFloat32Number*)p->Table;
         **
         **      val2 = fclamp(Value[0]);
         **
         **      // if last value...
         **      if (val2 == 1.0 || p->Domain[0] == 0) {
         **
         **          cmsUInt32Number start = p->Domain[0] * p->opta[0];
         **
         **          for (OutChan = 0; OutChan<p->nOutputs; OutChan++) {
         **              Output[OutChan] = LutTable[start + OutChan];
         **          }
         **      }
         **      else
         **      {
         **          val2 *= p->Domain[0];
         **
         **          cell0 = (int)floor(val2);
         **          cell1 = (int)ceil(val2);
         **
         **          // Rest is 16 LSB bits
         **          rest = val2 - cell0;
         **
         **          cell0 *= p->opta[0];
         **          cell1 *= p->opta[0];
         **
         **          for (OutChan = 0; OutChan < p->nOutputs; OutChan++)
         **          {
         **
         **              y0 = LutTable[cell0 + OutChan];
         **              y1 = LutTable[cell1 + OutChan];
         **
         **              Output[OutChan] = y0 + (y1 - y0) * rest;
         **          }
         **      }
         **  }
         **/

        var lutTable = p.TableFloat;

        var val2 = Fclamp(value[0]);

        if (val2 == 1.0 || p.Domain[0] == 0)
        {
            var start = (uint)(p.Domain[0] * p.Opta[0]);

            for (var outChan = 0; outChan < p.NumOutputs; outChan++)
                output[outChan] = lutTable[start + outChan];
        }
        else
        {
            val2 *= p.Domain[0];

            var cell0 = (int)MathF.Floor(val2);
            var cell1 = (int)MathF.Ceiling(val2);

            var rest = val2 - cell0;

            cell0 *= p.Opta[0];
            cell1 *= p.Opta[0];

            for (var outChan = 0; outChan < p.NumOutputs; outChan++)
            {
                var y0 = lutTable[cell0 + outChan];
                var y1 = lutTable[cell1 + outChan];

                output[outChan] = y0 + ((y1 - y0) * rest);
            }
        }
    }

    private static void Eval4Inputs16(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line: 853)
         **
         **  #define DENS(i,j,k) (LutTable[(i)+(j)+(k)+OutChan])
         **  static CMS_NO_SANITIZE
         **  void Eval4Inputs(CMSREGISTER const cmsUInt16Number Input[],
         **                       CMSREGISTER cmsUInt16Number Output[],
         **                       CMSREGISTER const cmsInterpParams* p16)
         **  {
         **      const cmsUInt16Number* LutTable;
         **      cmsS15Fixed16Number fk;
         **      cmsS15Fixed16Number k0, rk;
         **      int K0, K1;
         **      cmsS15Fixed16Number    fx, fy, fz;
         **      cmsS15Fixed16Number    rx, ry, rz;
         **      int                    x0, y0, z0;
         **      cmsS15Fixed16Number    X0, X1, Y0, Y1, Z0, Z1;
         **      cmsUInt32Number i;
         **      cmsS15Fixed16Number    c0, c1, c2, c3, Rest;
         **      cmsUInt32Number        OutChan;
         **      cmsUInt16Number        Tmp1[MAX_STAGE_CHANNELS], Tmp2[MAX_STAGE_CHANNELS];
         **
         **
         **      fk  = _cmsToFixedDomain((int) Input[0] * p16 -> Domain[0]);
         **      fx  = _cmsToFixedDomain((int) Input[1] * p16 -> Domain[1]);
         **      fy  = _cmsToFixedDomain((int) Input[2] * p16 -> Domain[2]);
         **      fz  = _cmsToFixedDomain((int) Input[3] * p16 -> Domain[3]);
         **
         **      k0  = FIXED_TO_INT(fk);
         **      x0  = FIXED_TO_INT(fx);
         **      y0  = FIXED_TO_INT(fy);
         **      z0  = FIXED_TO_INT(fz);
         **
         **      rk  = FIXED_REST_TO_INT(fk);
         **      rx  = FIXED_REST_TO_INT(fx);
         **      ry  = FIXED_REST_TO_INT(fy);
         **      rz  = FIXED_REST_TO_INT(fz);
         **
         **      K0 = p16 -> opta[3] * k0;
         **      K1 = K0 + (Input[0] == 0xFFFFU ? 0 : p16->opta[3]);
         **
         **      X0 = p16 -> opta[2] * x0;
         **      X1 = X0 + (Input[1] == 0xFFFFU ? 0 : p16->opta[2]);
         **
         **      Y0 = p16 -> opta[1] * y0;
         **      Y1 = Y0 + (Input[2] == 0xFFFFU ? 0 : p16->opta[1]);
         **
         **      Z0 = p16 -> opta[0] * z0;
         **      Z1 = Z0 + (Input[3] == 0xFFFFU ? 0 : p16->opta[0]);
         **
         **      LutTable = (cmsUInt16Number*) p16 -> Table;
         **      LutTable += K0;
         **
         **      for (OutChan=0; OutChan < p16 -> nOutputs; OutChan++) {
         **
         **          c0 = DENS(X0, Y0, Z0);
         **
         **          if (rx >= ry && ry >= rz) {
         **
         **              c1 = DENS(X1, Y0, Z0) - c0;
         **              c2 = DENS(X1, Y1, Z0) - DENS(X1, Y0, Z0);
         **              c3 = DENS(X1, Y1, Z1) - DENS(X1, Y1, Z0);
         **
         **          }
         **          else
         **              if (rx >= rz && rz >= ry) {
         **
         **                  c1 = DENS(X1, Y0, Z0) - c0;
         **                  c2 = DENS(X1, Y1, Z1) - DENS(X1, Y0, Z1);
         **                  c3 = DENS(X1, Y0, Z1) - DENS(X1, Y0, Z0);
         **
         **              }
         **              else
         **                  if (rz >= rx && rx >= ry) {
         **
         **                      c1 = DENS(X1, Y0, Z1) - DENS(X0, Y0, Z1);
         **                      c2 = DENS(X1, Y1, Z1) - DENS(X1, Y0, Z1);
         **                      c3 = DENS(X0, Y0, Z1) - c0;
         **
         **                  }
         **                  else
         **                      if (ry >= rx && rx >= rz) {
         **
         **                          c1 = DENS(X1, Y1, Z0) - DENS(X0, Y1, Z0);
         **                          c2 = DENS(X0, Y1, Z0) - c0;
         **                          c3 = DENS(X1, Y1, Z1) - DENS(X1, Y1, Z0);
         **
         **                      }
         **                      else
         **                          if (ry >= rz && rz >= rx) {
         **
         **                              c1 = DENS(X1, Y1, Z1) - DENS(X0, Y1, Z1);
         **                              c2 = DENS(X0, Y1, Z0) - c0;
         **                              c3 = DENS(X0, Y1, Z1) - DENS(X0, Y1, Z0);
         **
         **                          }
         **                          else
         **                              if (rz >= ry && ry >= rx) {
         **
         **                                  c1 = DENS(X1, Y1, Z1) - DENS(X0, Y1, Z1);
         **                                  c2 = DENS(X0, Y1, Z1) - DENS(X0, Y0, Z1);
         **                                  c3 = DENS(X0, Y0, Z1) - c0;
         **
         **                              }
         **                              else {
         **                                  c1 = c2 = c3 = 0;
         **                              }
         **
         **          Rest = c1 * rx + c2 * ry + c3 * rz;
         **
         **          Tmp1[OutChan] = (cmsUInt16Number)(c0 + ROUND_FIXED_TO_INT(_cmsToFixedDomain(Rest)));
         **      }
         **
         **
         **      LutTable = (cmsUInt16Number*) p16 -> Table;
         **      LutTable += K1;
         **
         **      for (OutChan=0; OutChan < p16 -> nOutputs; OutChan++) {
         **
         **          c0 = DENS(X0, Y0, Z0);
         **
         **          if (rx >= ry && ry >= rz) {
         **
         **              c1 = DENS(X1, Y0, Z0) - c0;
         **              c2 = DENS(X1, Y1, Z0) - DENS(X1, Y0, Z0);
         **              c3 = DENS(X1, Y1, Z1) - DENS(X1, Y1, Z0);
         **
         **          }
         **          else
         **              if (rx >= rz && rz >= ry) {
         **
         **                  c1 = DENS(X1, Y0, Z0) - c0;
         **                  c2 = DENS(X1, Y1, Z1) - DENS(X1, Y0, Z1);
         **                  c3 = DENS(X1, Y0, Z1) - DENS(X1, Y0, Z0);
         **
         **              }
         **              else
         **                  if (rz >= rx && rx >= ry) {
         **
         **                      c1 = DENS(X1, Y0, Z1) - DENS(X0, Y0, Z1);
         **                      c2 = DENS(X1, Y1, Z1) - DENS(X1, Y0, Z1);
         **                      c3 = DENS(X0, Y0, Z1) - c0;
         **
         **                  }
         **                  else
         **                      if (ry >= rx && rx >= rz) {
         **
         **                          c1 = DENS(X1, Y1, Z0) - DENS(X0, Y1, Z0);
         **                          c2 = DENS(X0, Y1, Z0) - c0;
         **                          c3 = DENS(X1, Y1, Z1) - DENS(X1, Y1, Z0);
         **
         **                      }
         **                      else
         **                          if (ry >= rz && rz >= rx) {
         **
         **                              c1 = DENS(X1, Y1, Z1) - DENS(X0, Y1, Z1);
         **                              c2 = DENS(X0, Y1, Z0) - c0;
         **                              c3 = DENS(X0, Y1, Z1) - DENS(X0, Y1, Z0);
         **
         **                          }
         **                          else
         **                              if (rz >= ry && ry >= rx) {
         **
         **                                  c1 = DENS(X1, Y1, Z1) - DENS(X0, Y1, Z1);
         **                                  c2 = DENS(X0, Y1, Z1) - DENS(X0, Y0, Z1);
         **                                  c3 = DENS(X0, Y0, Z1) - c0;
         **
         **                              }
         **                              else  {
         **                                  c1 = c2 = c3 = 0;
         **                              }
         **
         **          Rest = c1 * rx + c2 * ry + c3 * rz;
         **
         **          Tmp2[OutChan] = (cmsUInt16Number) (c0 + ROUND_FIXED_TO_INT(_cmsToFixedDomain(Rest)));
         **      }
         **
         **
         **
         **      for (i=0; i < p16 -> nOutputs; i++) {
         **          Output[i] = LinearInterp(rk, Tmp1[i], Tmp2[i]);
         **      }
         **  }
         **  #undef DENS
         **/

        var tmp1 = new ushort[maxStageChannels];
        var tmp2 = new ushort[maxStageChannels];

        int c0, rest, c1 = 0, c2 = 0, c3 = 0;

        var totalOut = p.NumOutputs;

        var fk = ToFixedDomain(input[0] * p.Domain[0]);
        var fx = ToFixedDomain(input[1] * p.Domain[1]);
        var fy = ToFixedDomain(input[2] * p.Domain[2]);
        var fz = ToFixedDomain(input[3] * p.Domain[3]);

        var k0 = FixedToInt(fk);
        var x0 = FixedToInt(fx);
        var y0 = FixedToInt(fy);
        var z0 = FixedToInt(fz);

        var rk = FixedRestToInt(fk);
        var rx = FixedRestToInt(fx);
        var ry = FixedRestToInt(fy);
        var rz = FixedRestToInt(fz);

        k0 *= p.Opta[3];
        var k1 = k0 + (input[0] == 0xFFFF ? 0 : p.Opta[3]);

        x0 *= p.Opta[2];
        var x1 = x0 + (input[1] == 0xFFFF ? 0 : p.Opta[2]);

        y0 *= p.Opta[1];
        var y1 = y0 + (input[2] == 0xFFFF ? 0 : p.Opta[1]);

        z0 *= p.Opta[0];
        var z1 = z0 + (input[3] == 0xFFFF ? 0 : p.Opta[0]);

        var lutTable = p.Table16.AsSpan()[k0..];

        for (var outChan = 0; outChan < totalOut; outChan++)
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
            tmp1[outChan] = (ushort)(c0 + RoundFixedToInt(ToFixedDomain(rest)));
        }

        lutTable = p.Table16.AsSpan()[k1..];

        for (var outChan = 0; outChan < totalOut; outChan++)
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
            tmp2[outChan] = (ushort)(c0 + RoundFixedToInt(ToFixedDomain(rest)));
        }

        for (var i = 0; i < p.NumOutputs; i++)
            output[i] = LinearInterp(rk, tmp1[i], tmp2[i]);
    }

    private static void Eval4InputsFloat(ReadOnlySpan<float> input, Span<float> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line: 1036)
         **
         **  // For more that 3 inputs (i.e., CMYK)
         **  // evaluate two 3-dimensional interpolations and then linearly interpolate between them.
         **  static
         **  void Eval4InputsFloat(const cmsFloat32Number Input[],
         **                        cmsFloat32Number Output[],
         **                        const cmsInterpParams* p)
         **  {
         **         const cmsFloat32Number* LutTable = (cmsFloat32Number*) p -> Table;
         **         cmsFloat32Number rest;
         **         cmsFloat32Number pk;
         **         int k0, K0, K1;
         **         const cmsFloat32Number* T;
         **         cmsUInt32Number i;
         **         cmsFloat32Number Tmp1[MAX_STAGE_CHANNELS], Tmp2[MAX_STAGE_CHANNELS];
         **         cmsInterpParams p1;
         **
         **         pk = fclamp(Input[0]) * p->Domain[0];
         **         k0 = _cmsQuickFloor(pk);
         **         rest = pk - (cmsFloat32Number) k0;
         **
         **         K0 = p -> opta[3] * k0;
         **         K1 = K0 + (fclamp(Input[0]) >= 1.0 ? 0 : p->opta[3]);
         **
         **         p1 = *p;
         **         memmove(&p1.Domain[0], &p ->Domain[1], 3*sizeof(cmsUInt32Number));
         **
         **         T = LutTable + K0;
         **         p1.Table = T;
         **
         **         TetrahedralInterpFloat(Input + 1,  Tmp1, &p1);
         **
         **         T = LutTable + K1;
         **         p1.Table = T;
         **         TetrahedralInterpFloat(Input + 1,  Tmp2, &p1);
         **
         **         for (i=0; i < p -> nOutputs; i++)
         **         {
         **                cmsFloat32Number y0 = Tmp1[i];
         **                cmsFloat32Number y1 = Tmp2[i];
         **
         **                Output[i] = y0 + (y1 - y0) * rest;
         **         }
         **  }
         **/

        var lutTable = p.TableFloat;
        var tmp1 = new float[maxStageChannels];
        var tmp2 = new float[maxStageChannels];

        var pk = Fclamp(input[0]) * p.Domain[0];
        var k0 = QuickFloor(pk);
        var rest = pk - k0;

        k0 *= p.Opta[3];
        var k1 = k0 + (Fclamp(input[0]) >= 1.0 ? 0 : p.Opta[3]);

        var t = lutTable[k0..];
        var p1 = new InterpParams(p.StateContainer, p.Flags, p.NumInputs, p.NumOutputs, t);

        p.Domain[1..3].CopyTo(p1.Domain.AsSpan());

        var inp = input[1..];
        TetrahedralInterpFloat(inp, tmp1, p1);

        t = lutTable[k1..];
        p1.Table = t;

        TetrahedralInterpFloat(inp, tmp2, p1);

        for (var i = 0; i < p.NumOutputs; i++)
        {
            var y0 = tmp1[i];
            var y1 = tmp2[i];

            output[i] = Lerp(rest, y0, y1);
        }
    }

    private static InterpFn16 EvalXInputs16(int x) =>
        /**  Original Code (cmsintrp.c line: 1080)
         **
         **  #define EVAL_FNS(N,NM) static CMS_NO_SANITIZE \
         **  void Eval##N##Inputs(CMSREGISTER const cmsUInt16Number Input[], CMSREGISTER cmsUInt16Number Output[], CMSREGISTER const cmsInterpParams* p16)\
         **  {\
         **         const cmsUInt16Number* LutTable = (cmsUInt16Number*) p16 -> Table;\
         **         cmsS15Fixed16Number fk;\
         **         cmsS15Fixed16Number k0, rk;\
         **         int K0, K1;\
         **         const cmsUInt16Number* T;\
         **         cmsUInt32Number i;\
         **         cmsUInt16Number Tmp1[MAX_STAGE_CHANNELS], Tmp2[MAX_STAGE_CHANNELS];\
         **         cmsInterpParams p1;\
         **  \
         **         fk = _cmsToFixedDomain((cmsS15Fixed16Number) Input[0] * p16 -> Domain[0]);\
         **         k0 = FIXED_TO_INT(fk);\
         **         rk = FIXED_REST_TO_INT(fk);\
         **  \
         **         K0 = p16 -> opta[NM] * k0;\
         **         K1 = p16 -> opta[NM] * (k0 + (Input[0] != 0xFFFFU ? 1 : 0));\
         **  \
         **         p1 = *p16;\
         **         memmove(&p1.Domain[0], &p16 ->Domain[1], NM*sizeof(cmsUInt32Number));\
         **  \
         **         T = LutTable + K0;\
         **         p1.Table = T;\
         **  \
         **         Eval##NM##Inputs(Input + 1, Tmp1, &p1);\
         **  \
         **         T = LutTable + K1;\
         **         p1.Table = T;\
         **  \
         **         Eval##NM##Inputs(Input + 1, Tmp2, &p1);\
         **  \
         **         for (i=0; i < p16 -> nOutputs; i++) {\
         **  \
         **                Output[i] = LinearInterp(rk, Tmp1[i], Tmp2[i]);\
         **         }\
         **  }\
         **
         **  ...
         **
         **  /**
         **  * Thanks to Carles Llopis for the templating idea
         **  *\
         **  EVAL_FNS(5, 4)
         **  EVAL_FNS(6, 5)
         **  EVAL_FNS(7, 6)
         **  EVAL_FNS(8, 7)
         **  EVAL_FNS(9, 8)
         **  EVAL_FNS(10, 9)
         **  EVAL_FNS(11, 10)
         **  EVAL_FNS(12, 11)
         **  EVAL_FNS(13, 12)
         **  EVAL_FNS(14, 13)
         **  EVAL_FNS(15, 14)
         **/

        (i, o, p) =>
        {
            var lutTable = p.Table16;

            var tmp1 = new ushort[maxStageChannels];
            var tmp2 = new ushort[maxStageChannels];

            var fk = ToFixedDomain(i[0] * p.Domain[0]);
            var k0 = FixedToInt(fk);
            var rk = FixedRestToInt(fk);

            var K0 = p.Opta[--x] * k0;
            var K1 = p.Opta[x] * (k0 + (i[0] != 0xFFFF ? 1 : 0));

            var p1 = (InterpParams)p.Clone();
            p.Domain[1..x].CopyTo(p1.Domain.AsSpan());

            var t = lutTable[K0..];
            p1.Table = t;

            var inp = i[1..];
            if (x is 4)
                Eval4Inputs16(inp, tmp1, p1);
            else
                EvalXInputs16(x)(inp, tmp1, p1);

            t = lutTable[K1..];
            p1.Table = t;

            if (x is 4)
                Eval4Inputs16(inp, tmp2, p1);
            else
                EvalXInputs16(x)(inp, tmp2, p1);

            for (var j = 0; j < p.NumOutputs; j++)
                o[j] = LinearInterp(rk, tmp1[j], tmp2[j]);
        };

    private static InterpFnFloat EvalXInputsFloat(int x) =>
        /**  Original Code (cmsintrp.c line: 1118)
         **
         **  static void Eval##N##InputsFloat(const cmsFloat32Number Input[], \
         **                                   cmsFloat32Number Output[],\
         **                                   const cmsInterpParams * p)\
         **  {\
         **         const cmsFloat32Number* LutTable = (cmsFloat32Number*) p -> Table;\
         **         cmsFloat32Number rest;\
         **         cmsFloat32Number pk;\
         **         int k0, K0, K1;\
         **         const cmsFloat32Number* T;\
         **         cmsUInt32Number i;\
         **         cmsFloat32Number Tmp1[MAX_STAGE_CHANNELS], Tmp2[MAX_STAGE_CHANNELS];\
         **         cmsInterpParams p1;\
         **  \
         **         pk = fclamp(Input[0]) * p->Domain[0];\
         **         k0 = _cmsQuickFloor(pk);\
         **         rest = pk - (cmsFloat32Number) k0;\
         **  \
         **         K0 = p -> opta[NM] * k0;\
         **         K1 = K0 + (fclamp(Input[0]) >= 1.0 ? 0 : p->opta[NM]);\
         **  \
         **         p1 = *p;\
         **         memmove(&p1.Domain[0], &p ->Domain[1], NM*sizeof(cmsUInt32Number));\
         **  \
         **         T = LutTable + K0;\
         **         p1.Table = T;\
         **  \
         **         Eval##NM##InputsFloat(Input + 1, Tmp1, &p1);\
         **  \
         **         T = LutTable + K1;\
         **         p1.Table = T;\
         **  \
         **         Eval##NM##InputsFloat(Input + 1, Tmp2, &p1);\
         **  \
         **         for (i=0; i < p -> nOutputs; i++) {\
         **  \
         **                cmsFloat32Number y0 = Tmp1[i];\
         **                cmsFloat32Number y1 = Tmp2[i];\
         **  \
         **                Output[i] = y0 + (y1 - y0) * rest;\
         **         }\
         **  }
         **
         **
         **  /**
         **  * Thanks to Carles Llopis for the templating idea
         **  *\
         **  EVAL_FNS(5, 4)
         **  EVAL_FNS(6, 5)
         **  EVAL_FNS(7, 6)
         **  EVAL_FNS(8, 7)
         **  EVAL_FNS(9, 8)
         **  EVAL_FNS(10, 9)
         **  EVAL_FNS(11, 10)
         **  EVAL_FNS(12, 11)
         **  EVAL_FNS(13, 12)
         **  EVAL_FNS(14, 13)
         **  EVAL_FNS(15, 14)
         **/

        (i, o, p) =>
    {
        var lutTable = p.TableFloat;
        var tmp1 = new float[maxStageChannels];
        var tmp2 = new float[maxStageChannels];

        var pk = Fclamp(i[0]) * p.Domain[0];
        var k0 = QuickFloor(pk);
        var rest = pk - k0;

        var K0 = p.Opta[--x] * k0;
        var K1 = K0 + (Fclamp(i[0]) >= 1.0 ? 0 : p.Opta[x]);

        var t = lutTable[K0..];
        var p1 = new InterpParams(p.StateContainer, p.Flags, p.NumInputs, p.NumOutputs, t);

        p.Domain[1..x].CopyTo(p1.Domain.AsSpan());

        var inp = i[1..];
        if (x is 4)
            Eval4InputsFloat(inp, tmp1, p1);
        else
            EvalXInputsFloat(x)(inp, tmp1, p1);

        t = lutTable[K1..];
        p1.Table = t;
        if (x is 4)
            Eval4InputsFloat(inp, tmp2, p1);
        else
            EvalXInputsFloat(x)(inp, tmp2, p1);

        for (var j = 0; j < p.NumOutputs; j++)
        {
            var y0 = tmp1[j];
            var y1 = tmp2[j];

            o[j] = Lerp(rest, y0, y1);
        }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Fclamp(float v) =>
        /**  Original Code (cmsintrp.c line:224)
         **
         **  // To prevent out of bounds indexing
         **  cmsINLINE cmsFloat32Number fclamp(cmsFloat32Number v)
         **  {
         **      return ((v < 1.0e-9f) || isnan(v)) ? 0.0f : (v > 1.0f ? 1.0f : v);
         **  }
         **/

        (v < 1.0e-9f) || Single.IsNaN(v)
            ? 0.0f
            : (v > 1.0f ? 1.0f : v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float a, float l, float h) =>
        l + ((h - l) * a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort Lerp(int a, ushort l, ushort h) =>
        (ushort)(l + RoundFixedToInt((h - l) * a));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort LinearInterp(int a, int l, int h)
    {
        /**  Original Code (cmsintrp.c line:183)
         **
         **  // Inline fixed point interpolation
         **  cmsINLINE CMS_NO_SANITIZE cmsUInt16Number LinearInterp(cmsS15Fixed16Number a, cmsS15Fixed16Number l, cmsS15Fixed16Number h)
         **  {
         **      cmsUInt32Number dif = (cmsUInt32Number) (h - l) * a + 0x8000;
         **      dif = (dif >> 16) + l;
         **      return (cmsUInt16Number) (dif);
         **  }
         **/

        uint dif = ((uint)(h - l) * (uint)a) + 0x8000u;
        dif = (dif >> 16) + (uint)l;
        return (ushort)dif;
    }

    private static void LinLerp1D16(ReadOnlySpan<ushort> value, Span<ushort> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line:192)
         **
         **  //  Linear interpolation (Fixed-point optimized)
         **  static
         **  void LinLerp1D(CMSREGISTER const cmsUInt16Number Value[],
         **                 CMSREGISTER cmsUInt16Number Output[],
         **                 CMSREGISTER const cmsInterpParams* p)
         **  {
         **      cmsUInt16Number y1, y0;
         **      int cell0, rest;
         **      int val3;
         **      const cmsUInt16Number* LutTable = (cmsUInt16Number*) p ->Table;
         **
         **      // if last value or just one point
         **      if (Value[0] == 0xffff || p->Domain[0] == 0) {
         **
         **          Output[0] = LutTable[p -> Domain[0]];
         **      }
         **      else
         **      {
         **          val3 = p->Domain[0] * Value[0];
         **          val3 = _cmsToFixedDomain(val3);    // To fixed 15.16
         **
         **          cell0 = FIXED_TO_INT(val3);             // Cell is 16 MSB bits
         **          rest = FIXED_REST_TO_INT(val3);        // Rest is 16 LSB bits
         **
         **          y0 = LutTable[cell0];
         **          y1 = LutTable[cell0 + 1];
         **
         **          Output[0] = LinearInterp(rest, y0, y1);
         **      }
         **  }
         **/

        var lutTable = p.Table16;

        // if last value or just one point
        if (value[0] == 0xFFFF || p.Domain[0] == 0)
        {
            output[0] = lutTable[p.Domain[0]];
        }
        else
        {
            var val3 = p.Domain[0] * value[0];
            val3 = ToFixedDomain(val3);

            var cell0 = FixedToInt(val3);
            var rest = FixedRestToInt(val3);

            var y0 = lutTable[cell0];
            var y1 = lutTable[cell0 + 1];

            output[0] = LinearInterp(rest, y0, y1);
        }
    }

    private static void LinLerp1DFloat(ReadOnlySpan<float> value, Span<float> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line:229)
         **
         **  / Floating-point version of 1D interpolation
         **  static
         **  void LinLerp1Dfloat(const cmsFloat32Number Value[],
         **                      cmsFloat32Number Output[],
         **                      const cmsInterpParams* p)
         **  {
         **         cmsFloat32Number y1, y0;
         **         cmsFloat32Number val2, rest;
         **         int cell0, cell1;
         **         const cmsFloat32Number* LutTable = (cmsFloat32Number*) p ->Table;
         **
         **         val2 = fclamp(Value[0]);
         **
         **         // if last value...
         **         if (val2 == 1.0 || p->Domain[0] == 0) {
         **             Output[0] = LutTable[p -> Domain[0]];
         **         }
         **         else
         **         {
         **             val2 *= p->Domain[0];
         **
         **             cell0 = (int)floor(val2);
         **             cell1 = (int)ceil(val2);
         **
         **             // Rest is 16 LSB bits
         **             rest = val2 - cell0;
         **
         **             y0 = LutTable[cell0];
         **             y1 = LutTable[cell1];
         **
         **             Output[0] = y0 + (y1 - y0) * rest;
         **         }
         **  }
         **/

        var lutTable = p.TableFloat;

        var val2 = Fclamp(value[0]);

        if (val2 == 1.0 || p.Domain[0] == 0)
        {
            output[0] = lutTable[p.Domain[0]];
        }
        else
        {
            val2 *= p.Domain[0];

            var cell0 = (int)MathF.Floor(val2);
            var cell1 = (int)MathF.Ceiling(val2);

            // rest is 16 LSB bits
            var rest = val2 - cell0;

            var y0 = lutTable[cell0];
            var y1 = lutTable[cell1];

            output[0] = y0 + ((y1 - y0) * rest);
        }
    }

    private static void TetrahedralInterp16(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line: 720)
         **
         **  static CMS_NO_SANITIZE
         **  void TetrahedralInterp16(CMSREGISTER const cmsUInt16Number Input[],
         **                           CMSREGISTER cmsUInt16Number Output[],
         **                           CMSREGISTER const cmsInterpParams* p)
         **  {
         **      const cmsUInt16Number* LutTable = (cmsUInt16Number*) p -> Table;
         **      cmsS15Fixed16Number fx, fy, fz;
         **      cmsS15Fixed16Number rx, ry, rz;
         **      int x0, y0, z0;
         **      cmsS15Fixed16Number c0, c1, c2, c3, Rest;
         **      cmsUInt32Number X0, X1, Y0, Y1, Z0, Z1;
         **      cmsUInt32Number TotalOut = p -> nOutputs;
         **
         **      fx = _cmsToFixedDomain((int) Input[0] * p -> Domain[0]);
         **      fy = _cmsToFixedDomain((int) Input[1] * p -> Domain[1]);
         **      fz = _cmsToFixedDomain((int) Input[2] * p -> Domain[2]);
         **
         **      x0 = FIXED_TO_INT(fx);
         **      y0 = FIXED_TO_INT(fy);
         **      z0 = FIXED_TO_INT(fz);
         **
         **      rx = FIXED_REST_TO_INT(fx);
         **      ry = FIXED_REST_TO_INT(fy);
         **      rz = FIXED_REST_TO_INT(fz);
         **
         **      X0 = p -> opta[2] * x0;
         **      X1 = (Input[0] == 0xFFFFU ? 0 : p->opta[2]);
         **
         **      Y0 = p -> opta[1] * y0;
         **      Y1 = (Input[1] == 0xFFFFU ? 0 : p->opta[1]);
         **
         **      Z0 = p -> opta[0] * z0;
         **      Z1 = (Input[2] == 0xFFFFU ? 0 : p->opta[0]);
         **
         **      LutTable += X0+Y0+Z0;
         **
         **      // Output should be computed as x = ROUND_FIXED_TO_INT(_cmsToFixedDomain(Rest))
         **      // which expands as: x = (Rest + ((Rest+0x7fff)/0xFFFF) + 0x8000)>>16
         **      // This can be replaced by: t = Rest+0x8001, x = (t + (t>>16))>>16
         **      // at the cost of being off by one at 7fff and 17ffe.
         **
         **      if (rx >= ry) {
         **          if (ry >= rz) {
         **              Y1 += X1;
         **              Z1 += Y1;
         **              for (; TotalOut; TotalOut--) {
         **                  c1 = LutTable[X1];
         **                  c2 = LutTable[Y1];
         **                  c3 = LutTable[Z1];
         **                  c0 = *LutTable++;
         **                  c3 -= c2;
         **                  c2 -= c1;
         **                  c1 -= c0;
         **                  Rest = c1 * rx + c2 * ry + c3 * rz + 0x8001;
         **                  *Output++ = (cmsUInt16Number) c0 + ((Rest + (Rest>>16))>>16);
         **              }
         **          } else if (rz >= rx) {
         **              X1 += Z1;
         **              Y1 += X1;
         **              for (; TotalOut; TotalOut--) {
         **                  c1 = LutTable[X1];
         **                  c2 = LutTable[Y1];
         **                  c3 = LutTable[Z1];
         **                  c0 = *LutTable++;
         **                  c2 -= c1;
         **                  c1 -= c3;
         **                  c3 -= c0;
         **                  Rest = c1 * rx + c2 * ry + c3 * rz + 0x8001;
         **                  *Output++ = (cmsUInt16Number) c0 + ((Rest + (Rest>>16))>>16);
         **              }
         **          } else {
         **              Z1 += X1;
         **              Y1 += Z1;
         **              for (; TotalOut; TotalOut--) {
         **                  c1 = LutTable[X1];
         **                  c2 = LutTable[Y1];
         **                  c3 = LutTable[Z1];
         **                  c0 = *LutTable++;
         **                  c2 -= c3;
         **                  c3 -= c1;
         **                  c1 -= c0;
         **                  Rest = c1 * rx + c2 * ry + c3 * rz + 0x8001;
         **                  *Output++ = (cmsUInt16Number) c0 + ((Rest + (Rest>>16))>>16);
         **              }
         **          }
         **      } else {
         **          if (rx >= rz) {
         **              X1 += Y1;
         **              Z1 += X1;
         **              for (; TotalOut; TotalOut--) {
         **                  c1 = LutTable[X1];
         **                  c2 = LutTable[Y1];
         **                  c3 = LutTable[Z1];
         **                  c0 = *LutTable++;
         **                  c3 -= c1;
         **                  c1 -= c2;
         **                  c2 -= c0;
         **                  Rest = c1 * rx + c2 * ry + c3 * rz + 0x8001;
         **                  *Output++ = (cmsUInt16Number) c0 + ((Rest + (Rest>>16))>>16);
         **              }
         **          } else if (ry >= rz) {
         **              Z1 += Y1;
         **              X1 += Z1;
         **              for (; TotalOut; TotalOut--) {
         **                  c1 = LutTable[X1];
         **                  c2 = LutTable[Y1];
         **                  c3 = LutTable[Z1];
         **                  c0 = *LutTable++;
         **                  c1 -= c3;
         **                  c3 -= c2;
         **                  c2 -= c0;
         **                  Rest = c1 * rx + c2 * ry + c3 * rz + 0x8001;
         **                  *Output++ = (cmsUInt16Number) c0 + ((Rest + (Rest>>16))>>16);
         **              }
         **          } else {
         **              Y1 += Z1;
         **              X1 += Y1;
         **              for (; TotalOut; TotalOut--) {
         **                  c1 = LutTable[X1];
         **                  c2 = LutTable[Y1];
         **                  c3 = LutTable[Z1];
         **                  c0 = *LutTable++;
         **                  c1 -= c2;
         **                  c2 -= c3;
         **                  c3 -= c0;
         **                  Rest = c1 * rx + c2 * ry + c3 * rz + 0x8001;
         **                  *Output++ = (cmsUInt16Number) c0 + ((Rest + (Rest>>16))>>16);
         **              }
         **          }
         **      }
         **  }
         **/

        var lutTable = p.Table16;
        int rest, c0, c1 = 0, c2 = 0, c3 = 0;
        var o = output;

        var totalOut = p.NumOutputs;

        var fx = ToFixedDomain(input[0] * p.Domain[0]);
        var fy = ToFixedDomain(input[1] * p.Domain[1]);
        var fz = ToFixedDomain(input[2] * p.Domain[2]);

        // We need full floor functionality here
        var x0 = FixedToInt(fx);
        var y0 = FixedToInt(fy);
        var z0 = FixedToInt(fz);

        var rx = FixedRestToInt(fx);
        var ry = FixedRestToInt(fy);
        var rz = FixedRestToInt(fz);

        x0 *= p.Opta[2];
        var x1 = input[0] == 0xFFFF ? 0 : p.Opta[2];

        y0 *= p.Opta[1];
        var y1 = input[1] == 0xFFFF ? 0 : p.Opta[1];

        z0 *= p.Opta[0];
        var z1 = input[2] == 0xFFFF ? 0 : p.Opta[0];

        lutTable = lutTable[(x0 + y0 + z0)..];
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
                    c0 = lutTable[0];
                    c3 -= c2;
                    c2 -= c1;
                    c1 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
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
                    c0 = lutTable[0];
                    c2 -= c1;
                    c1 -= c3;
                    c3 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
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
                    c0 = lutTable[0];
                    c2 -= c3;
                    c3 -= c1;
                    c1 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
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
                    c0 = lutTable[0];
                    c3 -= c1;
                    c1 -= c2;
                    c2 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
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
                    c0 = lutTable[0];
                    c1 -= c3;
                    c3 -= c2;
                    c2 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
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
                    c0 = lutTable[0];
                    c1 -= c2;
                    c2 -= c3;
                    c3 -= c0;
                    rest = (c1 * rx) + (c2 * ry) + (c3 * rz) + 0x8001;
                    o[0] = (ushort)(c0 + ((rest + (rest >> 16)) >> 16));
                    lutTable = lutTable[1..];
                    o = o[1..];
                }
            }
        }
    }

    // Tetrahedral interpolation, using Sakamoto algorithm.
    private static void TetrahedralInterpFloat(ReadOnlySpan<float> input, Span<float> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line: 620)
         **
         **  // Tetrahedral interpolation, using Sakamoto algorithm.
         **  #define DENS(i,j,k) (LutTable[(i)+(j)+(k)+OutChan])
         **  static
         **  void TetrahedralInterpFloat(const cmsFloat32Number Input[],
         **                              cmsFloat32Number Output[],
         **                              const cmsInterpParams* p)
         **  {
         **      const cmsFloat32Number* LutTable = (cmsFloat32Number*) p -> Table;
         **      cmsFloat32Number     px, py, pz;
         **      int                  x0, y0, z0,
         **                           X0, Y0, Z0, X1, Y1, Z1;
         **      cmsFloat32Number     rx, ry, rz;
         **      cmsFloat32Number     c0, c1=0, c2=0, c3=0;
         **      int                  OutChan, TotalOut;
         **
         **      TotalOut   = p -> nOutputs;
         **
         **      // We need some clipping here
         **      px = fclamp(Input[0]) * p->Domain[0];
         **      py = fclamp(Input[1]) * p->Domain[1];
         **      pz = fclamp(Input[2]) * p->Domain[2];
         **
         **      x0 = (int) floor(px); rx = (px - (cmsFloat32Number) x0);  // We need full floor functionality here
         **      y0 = (int) floor(py); ry = (py - (cmsFloat32Number) y0);
         **      z0 = (int) floor(pz); rz = (pz - (cmsFloat32Number) z0);
         **
         **
         **      X0 = p -> opta[2] * x0;
         **      X1 = X0 + (fclamp(Input[0]) >= 1.0 ? 0 : p->opta[2]);
         **
         **      Y0 = p -> opta[1] * y0;
         **      Y1 = Y0 + (fclamp(Input[1]) >= 1.0 ? 0 : p->opta[1]);
         **
         **      Z0 = p -> opta[0] * z0;
         **      Z1 = Z0 + (fclamp(Input[2]) >= 1.0 ? 0 : p->opta[0]);
         **
         **      for (OutChan=0; OutChan < TotalOut; OutChan++) {
         **
         **         // These are the 6 Tetrahedral
         **
         **          c0 = DENS(X0, Y0, Z0);
         **
         **          if (rx >= ry && ry >= rz) {
         **
         **              c1 = DENS(X1, Y0, Z0) - c0;
         **              c2 = DENS(X1, Y1, Z0) - DENS(X1, Y0, Z0);
         **              c3 = DENS(X1, Y1, Z1) - DENS(X1, Y1, Z0);
         **
         **          }
         **          else
         **              if (rx >= rz && rz >= ry) {
         **
         **                  c1 = DENS(X1, Y0, Z0) - c0;
         **                  c2 = DENS(X1, Y1, Z1) - DENS(X1, Y0, Z1);
         **                  c3 = DENS(X1, Y0, Z1) - DENS(X1, Y0, Z0);
         **
         **              }
         **              else
         **                  if (rz >= rx && rx >= ry) {
         **
         **                      c1 = DENS(X1, Y0, Z1) - DENS(X0, Y0, Z1);
         **                      c2 = DENS(X1, Y1, Z1) - DENS(X1, Y0, Z1);
         **                      c3 = DENS(X0, Y0, Z1) - c0;
         **
         **                  }
         **                  else
         **                      if (ry >= rx && rx >= rz) {
         **
         **                          c1 = DENS(X1, Y1, Z0) - DENS(X0, Y1, Z0);
         **                          c2 = DENS(X0, Y1, Z0) - c0;
         **                          c3 = DENS(X1, Y1, Z1) - DENS(X1, Y1, Z0);
         **
         **                      }
         **                      else
         **                          if (ry >= rz && rz >= rx) {
         **
         **                              c1 = DENS(X1, Y1, Z1) - DENS(X0, Y1, Z1);
         **                              c2 = DENS(X0, Y1, Z0) - c0;
         **                              c3 = DENS(X0, Y1, Z1) - DENS(X0, Y1, Z0);
         **
         **                          }
         **                          else
         **                              if (rz >= ry && ry >= rx) {
         **
         **                                  c1 = DENS(X1, Y1, Z1) - DENS(X0, Y1, Z1);
         **                                  c2 = DENS(X0, Y1, Z1) - DENS(X0, Y0, Z1);
         **                                  c3 = DENS(X0, Y0, Z1) - c0;
         **
         **                              }
         **                              else  {
         **                                  c1 = c2 = c3 = 0;
         **                              }
         **
         **         Output[OutChan] = c0 + c1 * rx + c2 * ry + c3 * rz;
         **         }
         **
         **  }
         **
         **  #undef DENS
         **/

        var lutTable = p.TableFloat;
        float c1 = 0, c2 = 0, c3 = 0;

        var totalOut = p.NumOutputs;

        var px = Fclamp(input[0]) * p.Domain[0];
        var py = Fclamp(input[1]) * p.Domain[1];
        var pz = Fclamp(input[2]) * p.Domain[2];

        // We need full floor functionality here
        var x0 = (int)MathF.Floor(px); var rx = px - x0;
        var y0 = (int)MathF.Floor(py); var ry = py - y0;
        var z0 = (int)MathF.Floor(pz); var rz = pz - z0;

        x0 *= p.Opta[2];
        var x1 = x0 + (Fclamp(input[0]) >= 1.0 ? 0 : p.Opta[2]);

        y0 *= p.Opta[1];
        var y1 = y0 + (Fclamp(input[1]) >= 1.0 ? 0 : p.Opta[1]);

        z0 *= p.Opta[0];
        var z1 = z0 + (Fclamp(input[2]) >= 1.0 ? 0 : p.Opta[0]);

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

    private static void TrilinearInterp16(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line: 542)
         **
         **  // Trilinear interpolation (16 bits) - optimized version
         **  static CMS_NO_SANITIZE
         **  void TrilinearInterp16(CMSREGISTER const cmsUInt16Number Input[],
         **                         CMSREGISTER cmsUInt16Number Output[],
         **                         CMSREGISTER const cmsInterpParams* p)
         **
         **  {
         **  #define DENS(i,j,k) (LutTable[(i)+(j)+(k)+OutChan])
         **  #define LERP(a,l,h)     (cmsUInt16Number) (l + ROUND_FIXED_TO_INT(((h-l)*a)))
         **
         **             const cmsUInt16Number* LutTable = (cmsUInt16Number*) p ->Table;
         **             int        OutChan, TotalOut;
         **             cmsS15Fixed16Number    fx, fy, fz;
         **             CMSREGISTER int        rx, ry, rz;
         **             int                    x0, y0, z0;
         **             CMSREGISTER int        X0, X1, Y0, Y1, Z0, Z1;
         **             int                    d000, d001, d010, d011,
         **                                    d100, d101, d110, d111,
         **                                    dx00, dx01, dx10, dx11,
         **                                    dxy0, dxy1, dxyz;
         **
         **      TotalOut   = p -> nOutputs;
         **
         **      fx = _cmsToFixedDomain((int) Input[0] * p -> Domain[0]);
         **      x0  = FIXED_TO_INT(fx);
         **      rx  = FIXED_REST_TO_INT(fx);    // Rest in 0..1.0 domain
         **
         **
         **      fy = _cmsToFixedDomain((int) Input[1] * p -> Domain[1]);
         **      y0  = FIXED_TO_INT(fy);
         **      ry  = FIXED_REST_TO_INT(fy);
         **
         **      fz = _cmsToFixedDomain((int) Input[2] * p -> Domain[2]);
         **      z0 = FIXED_TO_INT(fz);
         **      rz = FIXED_REST_TO_INT(fz);
         **
         **
         **      X0 = p -> opta[2] * x0;
         **      X1 = X0 + (Input[0] == 0xFFFFU ? 0 : p->opta[2]);
         **
         **      Y0 = p -> opta[1] * y0;
         **      Y1 = Y0 + (Input[1] == 0xFFFFU ? 0 : p->opta[1]);
         **
         **      Z0 = p -> opta[0] * z0;
         **      Z1 = Z0 + (Input[2] == 0xFFFFU ? 0 : p->opta[0]);
         **
         **      for (OutChan = 0; OutChan < TotalOut; OutChan++) {
         **
         **          d000 = DENS(X0, Y0, Z0);
         **          d001 = DENS(X0, Y0, Z1);
         **          d010 = DENS(X0, Y1, Z0);
         **          d011 = DENS(X0, Y1, Z1);
         **
         **          d100 = DENS(X1, Y0, Z0);
         **          d101 = DENS(X1, Y0, Z1);
         **          d110 = DENS(X1, Y1, Z0);
         **          d111 = DENS(X1, Y1, Z1);
         **
         **
         **          dx00 = LERP(rx, d000, d100);
         **          dx01 = LERP(rx, d001, d101);
         **          dx10 = LERP(rx, d010, d110);
         **          dx11 = LERP(rx, d011, d111);
         **
         **          dxy0 = LERP(ry, dx00, dx10);
         **          dxy1 = LERP(ry, dx01, dx11);
         **
         **          dxyz = LERP(rz, dxy0, dxy1);
         **
         **          Output[OutChan] = (cmsUInt16Number) dxyz;
         **      }
         **
         **
         **  #   undef LERP
         **  #   undef DENS
         **  }
         **/

        var lutTable = p.Table16;

        var totalOut = p.NumOutputs;

        var fx = ToFixedDomain(input[0]) * p.Domain[0];
        var x0 = FixedToInt(fx);
        var rx = FixedRestToInt(fx);

        var fy = ToFixedDomain(input[1]) * p.Domain[1];
        var y0 = FixedToInt(fy);
        var ry = FixedRestToInt(fy);

        var fz = ToFixedDomain(input[2]) * p.Domain[2];
        var z0 = FixedToInt(fz);
        var rz = FixedRestToInt(fz);

        x0 *= p.Opta[2];
        var x1 = x0 + (input[0] == 0xFFFF ? 0 : p.Opta[2]);

        y0 *= p.Opta[1];
        var y1 = y0 + (input[1] == 0xFFFF ? 0 : p.Opta[1]);

        z0 *= p.Opta[0];
        var z1 = z0 + (input[2] == 0xFFFF ? 0 : p.Opta[0]);

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

    private static void TrilinearInterpFloat(ReadOnlySpan<float> input, Span<float> output, InterpParams p)
    {
        /**  Original Code (cmsintrp.c line: 469)
         **
         **  // Trilinear interpolation (16 bits) - cmsFloat32Number version
         **  static
         **  void TrilinearInterpFloat(const cmsFloat32Number Input[],
         **                            cmsFloat32Number Output[],
         **                            const cmsInterpParams* p)
         **
         **  {
         **  #   define LERP(a,l,h)      (cmsFloat32Number) ((l)+(((h)-(l))*(a)))
         **  #   define DENS(i,j,k)      (LutTable[(i)+(j)+(k)+OutChan])
         **
         **      const cmsFloat32Number* LutTable = (cmsFloat32Number*) p ->Table;
         **      cmsFloat32Number      px, py, pz;
         **      int        x0, y0, z0,
         **                 X0, Y0, Z0, X1, Y1, Z1;
         **      int        TotalOut, OutChan;
         **
         **      cmsFloat32Number      fx, fy, fz,
         **                            d000, d001, d010, d011,
         **                            d100, d101, d110, d111,
         **                            dx00, dx01, dx10, dx11,
         **                            dxy0, dxy1, dxyz;
         **
         **      TotalOut   = p -> nOutputs;
         **
         **      // We need some clipping here
         **      px = fclamp(Input[0]) * p->Domain[0];
         **      py = fclamp(Input[1]) * p->Domain[1];
         **      pz = fclamp(Input[2]) * p->Domain[2];
         **
         **      x0 = (int) floor(px); fx = px - (cmsFloat32Number) x0;  // We need full floor funcionality here
         **      y0 = (int) floor(py); fy = py - (cmsFloat32Number) y0;
         **      z0 = (int) floor(pz); fz = pz - (cmsFloat32Number) z0;
         **
         **      X0 = p -> opta[2] * x0;
         **      X1 = X0 + (fclamp(Input[0]) >= 1.0 ? 0 : p->opta[2]);
         **
         **      Y0 = p -> opta[1] * y0;
         **      Y1 = Y0 + (fclamp(Input[1]) >= 1.0 ? 0 : p->opta[1]);
         **
         **      Z0 = p -> opta[0] * z0;
         **      Z1 = Z0 + (fclamp(Input[2]) >= 1.0 ? 0 : p->opta[0]);
         **
         **      for (OutChan = 0; OutChan < TotalOut; OutChan++) {
         **
         **          d000 = DENS(X0, Y0, Z0);
         **          d001 = DENS(X0, Y0, Z1);
         **          d010 = DENS(X0, Y1, Z0);
         **          d011 = DENS(X0, Y1, Z1);
         **
         **          d100 = DENS(X1, Y0, Z0);
         **          d101 = DENS(X1, Y0, Z1);
         **          d110 = DENS(X1, Y1, Z0);
         **          d111 = DENS(X1, Y1, Z1);
         **
         **
         **          dx00 = LERP(fx, d000, d100);
         **          dx01 = LERP(fx, d001, d101);
         **          dx10 = LERP(fx, d010, d110);
         **          dx11 = LERP(fx, d011, d111);
         **
         **          dxy0 = LERP(fy, dx00, dx10);
         **          dxy1 = LERP(fy, dx01, dx11);
         **
         **          dxyz = LERP(fz, dxy0, dxy1);
         **
         **          Output[OutChan] = dxyz;
         **      }
         **
         **
         **  #   undef LERP
         **  #   undef DENS
         **  }
         **/

        var lutTable = p.TableFloat;

        var totalOut = p.NumOutputs;

        var px = Fclamp(input[0]) * p.Domain[0];
        var py = Fclamp(input[1]) * p.Domain[1];
        var pz = Fclamp(input[2]) * p.Domain[2];

        // We need full floor functionality here
        var x0 = (int)MathF.Floor(px); var fx = px - x0;
        var y0 = (int)MathF.Floor(py); var fy = py - y0;
        var z0 = (int)MathF.Floor(pz); var fz = pz - z0;

        x0 *= p.Opta[2];
        var x1 = x0 + (Fclamp(input[0]) >= 1.0 ? 0 : p.Opta[2]);

        y0 *= p.Opta[1];
        var y1 = y0 + (Fclamp(input[1]) >= 1.0 ? 0 : p.Opta[1]);

        z0 *= p.Opta[0];
        var z1 = z0 + (Fclamp(input[2]) >= 1.0 ? 0 : p.Opta[0]);

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

    #endregion Private Methods
}

public class InterpFunction : ICloneable
{
    /** Original Code (lcms2_plugin.h line: 277)
     **
     ** // This type holds a pointer to an interpolator that can be either 16 bits or float
     ** typedef union {
     **     _cmsInterpFn16       Lerp16;            // Forward interpolation in 16 bits
     **     _cmsInterpFnFloat    LerpFloat;         // Forward interpolation in floating point
     ** } cmsInterpFunction;
     **/

    #region Fields

    private readonly InterpFn16? _lerp16;

    private readonly InterpFnFloat? _lerpFloat;

    #endregion Fields

    #region Public Constructors

    public InterpFunction(InterpFn16 fn16) =>
        _lerp16 = fn16;

    public InterpFunction(InterpFnFloat fnFloat) =>
        _lerpFloat = fnFloat;

    #endregion Public Constructors

    #region Private Constructors

    private InterpFunction()
    { }

    #endregion Private Constructors

    #region Public Methods

    public object Clone() =>
        _lerp16 is not null
            ? new InterpFunction(_lerp16)
            : _lerpFloat is not null
                ? new InterpFunction(_lerpFloat)
                : null!;

    public void Lerp(ReadOnlySpan<ushort> input, Span<ushort> output, InterpParams p) =>
            _lerp16?.Invoke(input, output, p);

    public void Lerp(ReadOnlySpan<float> input, Span<float> output, InterpParams p) =>
        _lerpFloat?.Invoke(input, output, p);

    #endregion Public Methods
}
