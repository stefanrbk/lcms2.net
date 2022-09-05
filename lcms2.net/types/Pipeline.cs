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

public delegate void PipelineEval16Fn(ReadOnlySpan<ushort> @in, Span<ushort> @out, in object data);

public delegate void PipelineEvalFloatFn(ReadOnlySpan<float> @in, Span<float> @out, in object data);

public class Pipeline : ICloneable, IDisposable
{
    /*  Original Code (cmslut.c line: 1398)
     *
     *  cmsContext CMSEXPORT cmsGetPipelineContextID(const cmsPipeline* lut)
     *  {
     *      _cmsAssert(lut != NULL);
     *      return lut ->ContextID;
     *  }
     */

    #region Fields

    internal object? data;
    internal DupUserDataFn? dupDataFn;
    internal Stage? elements;
    internal PipelineEval16Fn? eval16Fn;
    internal PipelineEvalFloatFn? evalFloatFn;
    internal FreeUserDataFn? freeDataFn;
    private const int _inversionMaxIterations = 30;
    private const float _jacobianEpsilon = 0.001f;
    private bool _disposedValue;

    #endregion Fields

    #region Internal Constructors

    internal Pipeline(Stage? elements,
                      uint inputChannels,
                      uint outputChannels,
                      object? data,
                      PipelineEval16Fn? eval16Fn,
                      PipelineEvalFloatFn? evalFloatFn,
                      FreeUserDataFn? freeDataFn,
                      DupUserDataFn? dupDataFn,
                      object? state,
                      bool saveAs8Bits)
    {
        this.elements = elements;
        InputChannels = inputChannels;
        OutputChannels = outputChannels;
        this.data = data;
        this.eval16Fn = eval16Fn;
        this.evalFloatFn = evalFloatFn;
        this.freeDataFn = freeDataFn;
        this.dupDataFn = dupDataFn;
        StateContainer = state;
        SaveAs8Bits = saveAs8Bits;
    }

    #endregion Internal Constructors

    #region Properties

    public Stage? FirstStage =>
        elements;

    public uint InputChannels { get; internal set; }

    public Stage? LastStage
    {
        get
        {
            Stage? anterior = null;
            for (var mpe = elements; mpe is not null; mpe = mpe.Next)
                anterior = mpe;

            return anterior;
        }
    }

    public uint OutputChannels { get; internal set; }
    public bool SaveAs8Bits { get; internal set; }

    public uint StageCount
    {
        get
        {
            Stage? mpe;
            uint n;

            for (n = 0, mpe = elements; mpe is not null; mpe = mpe.Next)
                n++;

            return n;
        }
    }

    public object? StateContainer { get; internal set; }

    #endregion Properties

    /*  Original Code (cmslut.c line: 1404)
     *
     *  cmsUInt32Number CMSEXPORT cmsPipelineInputChannels(const cmsPipeline* lut)
     *  {
     *      _cmsAssert(lut != NULL);
     *      return lut ->InputChannels;
     *  }
     */
    /*  Original Code (cmalut.c line: 1410)
     *
     *  cmsUInt32Number CMSEXPORT cmsPipelineOutputChannels(const cmsPipeline* lut)
     *  {
     *      _cmsAssert(lut != NULL);
     *      return lut ->OutputChannels;
     *  }
     */
    /*  Original Code (cmslut.c line: 1628)
     *
     *  cmsBool CMSEXPORT cmsPipelineSetSaveAs8bitsFlag(cmsPipeline* lut, cmsBool On)
     *  {
     *      cmsBool Anterior = lut ->SaveAs8Bits;
     *
     *      lut ->SaveAs8Bits = On;
     *      return Anterior;
     *  }
     */
    /*  Original Code (cmslut.c line: 1701)
     *
     *  #define JACOBIAN_EPSILON            0.001f
     */
    /*  Original Code (cmslut.c line: 1702)
     *
     *  #define INVERSION_MAX_ITERATIONS    30
     */
    /*  Original Code (cmslut.c line: 1652)
     *
     *  cmsUInt32Number CMSEXPORT cmsPipelineStageCount(const cmsPipeline* lut)
     *  {
     *      cmsStage *mpe;
     *      cmsUInt32Number n;
     *
     *      for (n=0, mpe = lut ->Elements; mpe != NULL; mpe = mpe ->Next)
     *              n++;
     *
     *      return n;
     *  }
     */
    /*  Original Code (cmslut.c line: 1637)
     *
     *  cmsStage* CMSEXPORT cmsPipelineGetPtrToFirstStage(const cmsPipeline* lut)
     *  {
     *      return lut ->Elements;
     *  }
     */
    /*  Original Code (cmslut.c line: 1642)
     *
     *  cmsStage* CMSEXPORT cmsPipelineGetPtrToLastStage(const cmsPipeline* lut)
     *  {
     *      cmsStage *mpe, *Anterior = NULL;
     *
     *      for (mpe = lut ->Elements; mpe != NULL; mpe = mpe ->Next)
     *          Anterior = mpe;
     *
     *      return Anterior;
     *  }
     */
    /*  Original Code (cmslut.c line: 1367)
     *
     *  // LUT Creation & Destruction
     *  cmsPipeline* CMSEXPORT cmsPipelineAlloc(cmsContext ContextID, cmsUInt32Number InputChannels, cmsUInt32Number OutputChannels)
     *  {
     *         cmsPipeline* NewLUT;
     *
     *         // A value of zero in channels is allowed as placeholder
     *         if (InputChannels >= cmsMAXCHANNELS ||
     *             OutputChannels >= cmsMAXCHANNELS) return NULL;
     *
     *         NewLUT = (cmsPipeline*) _cmsMallocZero(ContextID, sizeof(cmsPipeline));
     *         if (NewLUT == NULL) return NULL;
     *
     *         NewLUT -> InputChannels  = InputChannels;
     *         NewLUT -> OutputChannels = OutputChannels;
     *
     *         NewLUT ->Eval16Fn    = _LUTeval16;
     *         NewLUT ->EvalFloatFn = _LUTevalFloat;
     *         NewLUT ->DupDataFn   = NULL;
     *         NewLUT ->FreeDataFn  = NULL;
     *         NewLUT ->Data        = NewLUT;
     *         NewLUT ->ContextID   = ContextID;
     *
     *         if (!BlessLUT(NewLUT))
     *         {
     *             _cmsFree(ContextID, NewLUT);
     *             return NULL;
     *         }
     *
     *         return NewLUT;
     *  }
     */

    #region Public Methods

    public static Pipeline? Alloc(object? state, uint inputChannels, uint outputChannels)
    {
        // A value of zero in channels is allowed as placeholder
        if (inputChannels >= maxChannels ||
            outputChannels >= maxChannels)
        {
            return null;
        }

        var newLut = new Pipeline(
            null,
            inputChannels,
            outputChannels,
            null,
            LutEval16,
            LutEvalFloat,
            null,
            null,
            state,
            false);
        newLut.data = newLut;

        if (!newLut.BlessLut())
        {
            newLut.Dispose();
            return null;
        }

        return newLut;
    }

    /*  Original Code (cmslut.c line: 1602)
     *
     *  // Concatenate two LUT into a new single one
     *  cmsBool  CMSEXPORT cmsPipelineCat(cmsPipeline* l1, const cmsPipeline* l2)
     *  {
     *      cmsStage* mpe;
     *
     *      // If both LUTS does not have elements, we need to inherit
     *      // the number of channels
     *      if (l1 ->Elements == NULL && l2 ->Elements == NULL) {
     *          l1 ->InputChannels  = l2 ->InputChannels;
     *          l1 ->OutputChannels = l2 ->OutputChannels;
     *      }
     *
     *      // Cat second
     *      for (mpe = l2 ->Elements;
     *           mpe != NULL;
     *           mpe = mpe ->Next) {
     *
     *              // We have to dup each element
     *              if (!cmsPipelineInsertStage(l1, cmsAT_END, cmsStageDup(mpe)))
     *                  return FALSE;
     *      }
     *
     *      return BlessLUT(l1);
     *  }
     */

    public bool CheckAndRetreiveStagesAtoB(out Stage? a, out Stage? clut, out Stage? m, out Stage? matrix, out Stage? b)
    {
        a = null;
        clut = null;
        m = null;
        matrix = null;
        b = null;

        var stages = new List<Stage>(5);

        for (var mpe = elements; mpe is not null; mpe = mpe.Next)
            stages.Add(mpe);

        switch (stages.Count)
        {
            case 1:

                if (stages[0].Type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    return true;
                }
                else
                {
                    return false;
                }

            case 3:

                if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.MatrixElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem)
                {
                    m = stages[0];
                    matrix = stages[1];
                    b = stages[2];

                    return true;
                }
                else if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.CLutElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem)
                {
                    a = stages[0];
                    clut = stages[1];
                    b = stages[2];

                    return true;
                }
                else
                {
                    return false;
                }

            case 5:

                if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.CLutElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem &&
                    stages[3].Type == Signature.Stage.MatrixElem &&
                    stages[4].Type == Signature.Stage.CurveSetElem)
                {
                    a = stages[0];
                    clut = stages[1];
                    m = stages[2];
                    matrix = stages[3];
                    b = stages[4];

                    return true;
                }
                else
                {
                    return false;
                }

            default:

                return false;
        }
    }

    public bool CheckAndRetrieveStagesBtoA(out Stage? b, out Stage? matrix, out Stage? m, out Stage? clut, out Stage? a)
    {
        a = null;
        clut = null;
        m = null;
        matrix = null;
        b = null;

        var stages = new List<Stage>(5);

        for (var mpe = elements; mpe is not null; mpe = mpe.Next)
            stages.Add(mpe);

        switch (stages.Count)
        {
            case 1:

                if (stages[0].Type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    return true;
                }
                else
                {
                    return false;
                }

            case 3:

                if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.MatrixElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    matrix = stages[1];
                    m = stages[2];

                    return true;
                }
                else if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.CLutElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    clut = stages[1];
                    a = stages[2];

                    return true;
                }
                else
                {
                    return false;
                }

            case 5:

                if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.CLutElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem &&
                    stages[3].Type == Signature.Stage.MatrixElem &&
                    stages[4].Type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    matrix = stages[1];
                    m = stages[2];
                    clut = stages[3];
                    a = stages[4];

                    return true;
                }
                else
                {
                    return false;
                }

            default:

                return false;
        }
    }

    public object Clone()
    {
        Stage? anterior = null;
        var first = true;

        var newLut = Alloc(StateContainer, InputChannels, OutputChannels);
        if (newLut is null) return null!;

        for (var mpe = elements; mpe is not null; mpe = mpe.Next)
        {
            var newMpe = (Stage)mpe.Clone();
            if (newMpe is null)
            {
                newLut.Dispose();
                return null!;
            }

            if (first)
            {
                newLut.elements = newMpe;
                first = false;
            }
            else if (anterior is not null)
            {
                anterior.Next = newMpe;
            }

            anterior = newMpe;
        }

        newLut.eval16Fn = eval16Fn;
        newLut.evalFloatFn = evalFloatFn;
        newLut.dupDataFn = dupDataFn;
        newLut.freeDataFn = freeDataFn;

        if (newLut.dupDataFn is not null)
            newLut.data = newLut.dupDataFn?.Invoke(StateContainer, data);

        newLut.SaveAs8Bits = SaveAs8Bits;

        if (!newLut.BlessLut())
        {
            newLut.Dispose();
            return null!;
        }

        return newLut;
    }

    public bool Concat(Pipeline l2)
    {
        // If both LUTS have no elements, we need to inherit
        // the number of channels
        if (elements is null && l2.elements is null)
        {
            InputChannels = l2.InputChannels;
            OutputChannels = l2.OutputChannels;
        }

        // Concat second
        for (var mpe = l2.elements;
             mpe is not null;
             mpe = mpe.Next)
        {
            // We have to dup each element
            if (!InsertStage(StageLoc.AtEnd, (Stage)mpe.Clone()))
                return false;
        }

        return BlessLut();
    }

    /*  Original Code (cmslut.c line: 104)
     *
     *  // This function is quite useful to analyze the structure of a LUT and retrieve the MPE elements
     *  // that conform the LUT. It should be called with the LUT, the number of expected elements and
     *  // then a list of expected types followed with a list of cmsFloat64Number pointers to MPE elements. If
     *  // the function founds a match with current pipeline, it fills the pointers and returns TRUE
     *  // if not, returns FALSE without touching anything. Setting pointers to NULL does bypass
     *  // the storage process.
     *  cmsBool  CMSEXPORT cmsPipelineCheckAndRetreiveStages(const cmsPipeline* Lut, cmsUInt32Number n, ...)
     *  {
     *      va_list args;
     *      cmsUInt32Number i;
     *      cmsStage* mpe;
     *      cmsStageSignature Type;
     *      void** ElemPtr;
     *
     *      // Make sure same number of elements
     *      if (cmsPipelineStageCount(Lut) != n) return FALSE;
     *
     *      va_start(args, n);
     *
     *      // Iterate across asked types
     *      mpe = Lut ->Elements;
     *      for (i=0; i < n; i++) {
     *
     *          // Get asked type. cmsStageSignature is promoted to int by compiler
     *          Type  = (cmsStageSignature)va_arg(args, int);
     *          if (mpe ->Type != Type) {
     *
     *              va_end(args);       // Mismatch. We are done.
     *              return FALSE;
     *          }
     *          mpe = mpe ->Next;
     *      }
     *
     *      // Found a combination, fill pointers if not NULL
     *      mpe = Lut ->Elements;
     *      for (i=0; i < n; i++) {
     *
     *          ElemPtr = va_arg(args, void**);
     *          if (ElemPtr != NULL)
     *              *ElemPtr = mpe;
     *
     *          mpe = mpe ->Next;
     *      }
     *
     *      va_end(args);
     *      return TRUE;
     *  }
     */
    /* NOTE:
     *  Not able to implement the same way, as C# doesn't support passing object refs in params
     *  This is the most elegant solution I could devise.
     */
    /*  Original Code (cmslut.c Line: 1454)
     *
     *  // Duplicates a LUT
     *  cmsPipeline* CMSEXPORT cmsPipelineDup(const cmsPipeline* lut)
     *  {
     *      cmsPipeline* NewLUT;
     *      cmsStage *NewMPE, *Anterior = NULL, *mpe;
     *      cmsBool  First = TRUE;
     *
     *      if (lut == NULL) return NULL;
     *
     *      NewLUT = cmsPipelineAlloc(lut ->ContextID, lut ->InputChannels, lut ->OutputChannels);
     *      if (NewLUT == NULL) return NULL;
     *
     *      for (mpe = lut ->Elements;
     *           mpe != NULL;
     *           mpe = mpe ->Next) {
     *
     *               NewMPE = cmsStageDup(mpe);
     *
     *               if (NewMPE == NULL) {
     *                   cmsPipelineFree(NewLUT);
     *                   return NULL;
     *               }
     *
     *               if (First) {
     *                   NewLUT ->Elements = NewMPE;
     *                   First = FALSE;
     *               }
     *               else {
     *                  if (Anterior != NULL)
     *                      Anterior ->Next = NewMPE;
     *               }
     *
     *              Anterior = NewMPE;
     *      }
     *
     *      NewLUT ->Eval16Fn    = lut ->Eval16Fn;
     *      NewLUT ->EvalFloatFn = lut ->EvalFloatFn;
     *      NewLUT ->DupDataFn   = lut ->DupDataFn;
     *      NewLUT ->FreeDataFn  = lut ->FreeDataFn;
     *
     *      if (NewLUT ->DupDataFn != NULL)
     *          NewLUT ->Data = NewLUT ->DupDataFn(lut ->ContextID, lut->Data);
     *
     *
     *      NewLUT ->SaveAs8Bits    = lut ->SaveAs8Bits;
     *
     *      if (!BlessLUT(NewLUT))
     *      {
     *          _cmsFree(lut->ContextID, NewLUT);
     *          return NULL;
     *      }
     *
     *      return NewLUT;
     *  }
     */
    /*  Original Code (cmslut.c line: 1437)
     *
     *  // Default to evaluate the LUT on 16 bit-basis.
     *  void CMSEXPORT cmsPipelineEval16(const cmsUInt16Number In[], cmsUInt16Number Out[],  const cmsPipeline* lut)
     *  {
     *      _cmsAssert(lut != NULL);
     *      lut ->Eval16Fn(In, Out, lut->Data);
     *  }
     */

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void Eval(ReadOnlySpan<ushort> @in, Span<ushort> @out) =>
            eval16Fn?.Invoke(@in, @out, this);

    /*  Original Code (cmslut.c line: 1445)
     *
     *  // Does evaluate the LUT on cmsFloat32Number-basis.
     *  void CMSEXPORT cmsPipelineEvalFloat(const cmsFloat32Number In[], cmsFloat32Number Out[], const cmsPipeline* lut)
     *  {
     *      _cmsAssert(lut != NULL);
     *      lut ->EvalFloatFn(In, Out, lut);
     *  }
     */

    public void Eval(ReadOnlySpan<float> @in, Span<float> @out) =>
        evalFloatFn?.Invoke(@in, @out, this);

    /*  Original Code (cmslut.c line: 1735
     *
     *  // Evaluate a LUT in reverse direction. It only searches on 3->3 LUT. Uses Newton method
     *  //
     *  // x1 <- x - [J(x)]^-1 * f(x)
     *  //
     *  // lut: The LUT on where to do the search
     *  // Target: LabK, 3 values of Lab plus destination K which is fixed
     *  // Result: The obtained CMYK
     *  // Hint:   Location where begin the search
     *
     *  cmsBool CMSEXPORT cmsPipelineEvalReverseFloat(cmsFloat32Number Target[],
     *                                                cmsFloat32Number Result[],
     *                                                cmsFloat32Number Hint[],
     *                                                const cmsPipeline* lut)
     *  {
     *      cmsUInt32Number  i, j;
     *      cmsFloat64Number  error, LastError = 1E20;
     *      cmsFloat32Number  fx[4], x[4], xd[4], fxd[4];
     *      cmsVEC3 tmp, tmp2;
     *      cmsMAT3 Jacobian;
     *
     *      // Only 3->3 and 4->3 are supported
     *      if (lut ->InputChannels != 3 && lut ->InputChannels != 4) return FALSE;
     *      if (lut ->OutputChannels != 3) return FALSE;
     *
     *      // Take the hint as starting point if specified
     *      if (Hint == NULL) {
     *
     *          // Begin at any point, we choose 1/3 of CMY axis
     *          x[0] = x[1] = x[2] = 0.3f;
     *      }
     *      else {
     *
     *          // Only copy 3 channels from hint...
     *          for (j=0; j < 3; j++)
     *              x[j] = Hint[j];
     *      }
     *
     *      // If Lut is 4-dimensions, then grab target[3], which is fixed
     *      if (lut ->InputChannels == 4) {
     *          x[3] = Target[3];
     *      }
     *      else x[3] = 0; // To keep lint happy
     *
     *
     *      // Iterate
     *      for (i = 0; i < INVERSION_MAX_ITERATIONS; i++) {
     *
     *          // Get beginning fx
     *          cmsPipelineEvalFloat(x, fx, lut);
     *
     *          // Compute error
     *          error = EuclideanDistance(fx, Target, 3);
     *
     *          // If not convergent, return last safe value
     *          if (error >= LastError)
     *              break;
     *
     *          // Keep latest values
     *          LastError     = error;
     *          for (j=0; j < lut ->InputChannels; j++)
     *                  Result[j] = x[j];
     *
     *          // Found an exact match?
     *          if (error <= 0)
     *              break;
     *
     *          // Obtain slope (the Jacobian)
     *          for (j = 0; j < 3; j++) {
     *
     *              xd[0] = x[0];
     *              xd[1] = x[1];
     *              xd[2] = x[2];
     *              xd[3] = x[3];  // Keep fixed channel
     *
     *              IncDelta(&xd[j]);
     *
     *              cmsPipelineEvalFloat(xd, fxd, lut);
     *
     *              Jacobian.v[0].n[j] = ((fxd[0] - fx[0]) / JACOBIAN_EPSILON);
     *              Jacobian.v[1].n[j] = ((fxd[1] - fx[1]) / JACOBIAN_EPSILON);
     *              Jacobian.v[2].n[j] = ((fxd[2] - fx[2]) / JACOBIAN_EPSILON);
     *          }
     *
     *          // Solve system
     *          tmp2.n[0] = fx[0] - Target[0];
     *          tmp2.n[1] = fx[1] - Target[1];
     *          tmp2.n[2] = fx[2] - Target[2];
     *
     *          if (!_cmsMAT3solve(&tmp, &Jacobian, &tmp2))
     *              return FALSE;
     *
     *          // Move our guess
     *          x[0] -= (cmsFloat32Number) tmp.n[0];
     *          x[1] -= (cmsFloat32Number) tmp.n[1];
     *          x[2] -= (cmsFloat32Number) tmp.n[2];
     *
     *          // Some clipping....
     *          for (j=0; j < 3; j++) {
     *              if (x[j] < 0) x[j] = 0;
     *              else
     *                  if (x[j] > 1.0) x[j] = 1.0;
     *          }
     *      }
     *
     *      return TRUE;
     *  }
     */

    public bool EvalReverse(ReadOnlySpan<float> target, Span<float> result, ReadOnlySpan<float> hint)
    {
        var lastError = 1e20;
        var fx = new float[4];
        var x = new float[4];
        var xd = new float[4];
        var fxd = new float[4];

        // Only 3->3 and 4->3 are supported
        if (InputChannels is not 3 and not 4 || OutputChannels is not 3)
            return false;

        // Take the hint as starting point if specified
        if (hint == default || hint.Length < 3)
        {
            // Begin at any point, we choose 1/3 of CMY axis
            x[0] = x[1] = x[2] = 0.3f;
        }
        else
        {
            // Only copy 3 channels from hint...
            for (var j = 0; j < 3; j++)
                x[j] = hint[j];
        }

        // If Lut is 4-dimensional, then grab target[3], which is fixed
        if (InputChannels is 4)
            x[3] = target[3];

        // Iterate
        for (var i = 0; i < _inversionMaxIterations; i++)
        {
            // Get beginning fx
            Eval(x, fx);

            // Compute error
            var error = (double)EuclideanDistance(fx, target, 3);

            // If not convergent, return last safe value
            if (error >= lastError) break;

            // Keep latest values
            lastError = error;
            for (var j = 0; j < InputChannels; j++)
                result[j] = x[j];

            // Found an exact match?
            if (error <= 0) break;

            var jacobian = new Mat3();

            // Obtain slope (the Jacobian)
            for (var j = 0; j < 3; j++)
            {
                x.CopyTo(xd.AsSpan());

                IncDelta(ref xd[j]);

                Eval(xd, fxd);

                jacobian.X[j] = (fxd[0] - fx[0]) / _jacobianEpsilon;
                jacobian.Y[j] = (fxd[1] - fx[1]) / _jacobianEpsilon;
                jacobian.Z[j] = (fxd[2] - fx[2]) / _jacobianEpsilon;
            }

            // Solve system
            var tmp =
                new Vec3(
                    fx[0] - target[0],
                    fx[1] - target[1],
                    fx[2] - target[2]);

            var tmp2 = jacobian.Solve(tmp);
            if (tmp2 is null) return false;

            // Move our guess
            x[0] -= (float)tmp2.Value[0];
            x[1] -= (float)tmp2.Value[1];
            x[2] -= (float)tmp2.Value[2];

            // Some clipping...
            for (var j = 0; j < 3; j++)
            {
                if (x[j] < 0)
                    x[j] = 0;
                else if (x[j] > 1)
                    x[j] = 1;
            }
        }

        return true;
    }

    /*  Original Code (cmslut.c line: 1510)
     *
     *  int CMSEXPORT cmsPipelineInsertStage(cmsPipeline* lut, cmsStageLoc loc, cmsStage* mpe)
     *  {
     *      cmsStage* Anterior = NULL, *pt;
     *
     *      if (lut == NULL || mpe == NULL)
     *          return FALSE;
     *
     *      switch (loc) {
     *
     *          case cmsAT_BEGIN:
     *              mpe ->Next = lut ->Elements;
     *              lut ->Elements = mpe;
     *              break;
     *
     *          case cmsAT_END:
     *
     *              if (lut ->Elements == NULL)
     *                  lut ->Elements = mpe;
     *              else {
     *
     *                  for (pt = lut ->Elements;
     *                       pt != NULL;
     *                       pt = pt -> Next) Anterior = pt;
     *
     *                  Anterior ->Next = mpe;
     *                  mpe ->Next = NULL;
     *              }
     *              break;
     *          default:;
     *              return FALSE;
     *      }
     *
     *      return BlessLUT(lut);
     *  }
     */

    public bool InsertStage(StageLoc loc, Stage? mpe)
    {
        Stage? anterior = null;

        if (mpe is null) return false;

        switch (loc)
        {
            case StageLoc.AtBegin:
                mpe.Next = elements;
                elements = mpe;
                break;

            case StageLoc.AtEnd:
                if (elements is null)
                    elements = mpe;
                else
                {
                    for (var pt = elements; pt is not null; pt = pt.Next)
                        anterior = pt;

                    anterior!.Next = mpe;
                    mpe.Next = null;
                }
                break;

            default:
                return false;
        }

        return BlessLut();
    }

    /*  Original Code (cmslut.c line: 1663)
     *
     *  // This function may be used to set the optional evaluator and a block of private data. If private data is being used, an optional
     *  // duplicator and free functions should also be specified in order to duplicate the LUT construct. Use NULL to inhibit such functionality.
     *  void CMSEXPORT _cmsPipelineSetOptimizationParameters(cmsPipeline* Lut,
     *                                          _cmsPipelineEval16Fn Eval16,
     *                                          void* PrivateData,
     *                                          _cmsFreeUserDataFn FreePrivateDataFn,
     *                                          _cmsDupUserDataFn  DupPrivateDataFn)
     *  {
     *
     *      Lut ->Eval16Fn = Eval16;
     *      Lut ->DupDataFn = DupPrivateDataFn;
     *      Lut ->FreeDataFn = FreePrivateDataFn;
     *      Lut ->Data = PrivateData;
     *  }
     */

    public void SetOptimizationParameters(PipelineEval16Fn? eval16,
                                          object? privateData,
                                          FreeUserDataFn? freePrivateDataFn,
                                          DupUserDataFn? dupPrivateDataFn)
    {
        eval16Fn = eval16;
        evalFloatFn = null;
        data = privateData;
        freeDataFn = freePrivateDataFn;
        dupDataFn = dupPrivateDataFn;
    }

    /*  Original Code (cmslut.c line: 1545)
     *
     *  // Unlink an element and return the pointer to it
     *  void CMSEXPORT cmsPipelineUnlinkStage(cmsPipeline* lut, cmsStageLoc loc, cmsStage** mpe)
     *  {
     *      cmsStage *Anterior, *pt, *Last;
     *      cmsStage *Unlinked = NULL;
     *
     *
     *      // If empty LUT, there is nothing to remove
     *      if (lut ->Elements == NULL) {
     *          if (mpe) *mpe = NULL;
     *          return;
     *      }
     *
     *      // On depending on the strategy...
     *      switch (loc) {
     *
     *          case cmsAT_BEGIN:
     *              {
     *                  cmsStage* elem = lut ->Elements;
     *
     *                  lut ->Elements = elem -> Next;
     *                  elem ->Next = NULL;
     *                  Unlinked = elem;
     *
     *              }
     *              break;
     *
     *          case cmsAT_END:
     *              Anterior = Last = NULL;
     *              for (pt = lut ->Elements;
     *                  pt != NULL;
     *                  pt = pt -> Next) {
     *                      Anterior = Last;
     *                      Last = pt;
     *              }
     *
     *              Unlinked = Last;  // Next already points to NULL
     *
     *              // Truncate the chain
     *              if (Anterior)
     *                  Anterior ->Next = NULL;
     *              else
     *                  lut ->Elements = NULL;
     *              break;
     *          default:;
     *      }
     *
     *      if (mpe)
     *          *mpe = Unlinked;
     *      else
     *          cmsStageFree(Unlinked);
     *
     *      // May fail, but we ignore it
     *      BlessLUT(lut);
     *  }
     */

    public Stage? UnlinkStage(StageLoc loc)
    {
        Stage? unlinked = null;

        // If empty LUT, there is nothing to remove
        if (elements is null)
            return null;

        // Depending on the strategy...
        switch (loc)
        {
            case StageLoc.AtBegin:
                var elem = elements;

                elements = elem.Next;
                elem.Next = null;
                unlinked = elem;
                break;

            case StageLoc.AtEnd:
                Stage? anterior = null, last = null;
                for (var pt = elements;
                     pt is not null;
                     pt = pt.Next)
                {
                    anterior = last;
                    last = pt;
                }

                unlinked = last;    // Next already points to null

                // Truncate the chain
                if (anterior is not null)
                    anterior.Next = null;
                else
                    elements = null;
                break;
        }

        BlessLut();

        return unlinked;
    }

    #endregion Public Methods

    /*  Original Code (cmslut.c line: 1317)
     *
     *  // Default to evaluate the LUT on 16 bit-basis. Precision is retained.
     *  static
     *  void _LUTeval16(CMSREGISTER const cmsUInt16Number In[], CMSREGISTER cmsUInt16Number Out[],  CMSREGISTER const void* D)
     *  {
     *      cmsPipeline* lut = (cmsPipeline*) D;
     *      cmsStage *mpe;
     *      cmsFloat32Number Storage[2][MAX_STAGE_CHANNELS];
     *      int Phase = 0, NextPhase;
     *
     *      From16ToFloat(In, &Storage[Phase][0], lut ->InputChannels);
     *
     *      for (mpe = lut ->Elements;
     *           mpe != NULL;
     *           mpe = mpe ->Next) {
     *
     *               NextPhase = Phase ^ 1;
     *               mpe ->EvalPtr(&Storage[Phase][0], &Storage[NextPhase][0], mpe);
     *               Phase = NextPhase;
     *      }
     *
     *
     *      FromFloatTo16(&Storage[Phase][0], Out, lut ->OutputChannels);
     *  }
     */

    #region Protected Methods

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                for (Stage? mpe = elements; mpe is not null; mpe = mpe.Next)
                    mpe.Dispose();

                if (freeDataFn is not null)
                    freeDataFn(StateContainer, ref data!);
            }
            _disposedValue = true;
        }
    }

    #endregion Protected Methods

    #region Private Methods

    private static float EuclideanDistance(ReadOnlySpan<float> a, ReadOnlySpan<float> b, int n)
    {
        var sum = 0f;
        for (var i = 0; i < n; i++)
        {
            var dif = b[i] - a[i];
            sum += dif * dif;
        }

        return MathF.Sqrt(sum);
    }

    private static void IncDelta(ref float val)
    {
        if (val < (1.0 - _jacobianEpsilon))
            val += _jacobianEpsilon;
        else
            val -= _jacobianEpsilon;
    }

    private static void LutEval16(ReadOnlySpan<ushort> @in, Span<ushort> @out, in object d)
    {
        var lut = (Pipeline)d;

        var storage = new float[][]
        {
            new float[maxStageChannels],
            new float[maxStageChannels]
        };

        var phase = 0;

        From16ToFloat(@in, storage[phase], (int)lut.InputChannels);

        for (var mpe = lut.elements; mpe is not null; mpe = mpe.Next)
        {
            var nextPhase = phase ^ 1;
            mpe.Data.Evaluate(storage[phase], storage[nextPhase], mpe);
            phase = nextPhase;
        }

        FromFloatTo16(storage[phase], @out, (int)lut.OutputChannels);
    }

    /*  Original Code (cmslut.c line: 1343)
     *
     *  // Does evaluate the LUT on cmsFloat32Number-basis.
     *  static
     *  void _LUTevalFloat(const cmsFloat32Number In[], cmsFloat32Number Out[], const void* D)
     *  {
     *      cmsPipeline* lut = (cmsPipeline*) D;
     *      cmsStage *mpe;
     *      cmsFloat32Number Storage[2][MAX_STAGE_CHANNELS];
     *      int Phase = 0, NextPhase;
     *
     *      memmove(&Storage[Phase][0], In, lut ->InputChannels  * sizeof(cmsFloat32Number));
     *
     *      for (mpe = lut ->Elements;
     *           mpe != NULL;
     *           mpe = mpe ->Next) {
     *
     *                NextPhase = Phase ^ 1;
     *                mpe ->EvalPtr(&Storage[Phase][0], &Storage[NextPhase][0], mpe);
     *                Phase = NextPhase;
     *      }
     *
     *      memmove(Out, &Storage[Phase][0], lut ->OutputChannels * sizeof(cmsFloat32Number));
     *  }
     */

    private static void LutEvalFloat(ReadOnlySpan<float> @in, Span<float> @out, in object d)
    {
        var lut = (Pipeline)d;

        var storage = new float[][]
        {
            new float[maxStageChannels],
            new float[maxStageChannels]
        };

        var phase = 0;

        @in[..(int)lut.InputChannels].CopyTo(storage[phase]);

        for (var mpe = lut.elements; mpe is not null; mpe = mpe.Next)
        {
            var nextPhase = phase ^ 1;
            mpe.Data.Evaluate(storage[phase], storage[nextPhase], mpe);
            phase = nextPhase;
        }
        storage[phase][..(int)lut.OutputChannels].CopyTo(@out);
    }

    /*  Original Code (cmslut.c line: 1279)
     *
     *  // This function sets up the channel count
     *  static
     *  cmsBool BlessLUT(cmsPipeline* lut)
     *  {
     *      // We can set the input/output channels only if we have elements.
     *      if (lut ->Elements != NULL) {
     *
     *          cmsStage* prev;
     *          cmsStage* next;
     *          cmsStage* First;
     *          cmsStage* Last;
     *
     *          First  = cmsPipelineGetPtrToFirstStage(lut);
     *          Last   = cmsPipelineGetPtrToLastStage(lut);
     *
     *          if (First == NULL || Last == NULL) return FALSE;
     *
     *          lut->InputChannels = First->InputChannels;
     *          lut->OutputChannels = Last->OutputChannels;
     *
     *          // Check chain consistency
     *          prev = First;
     *          next = prev->Next;
     *
     *          while (next != NULL)
     *          {
     *              if (next->InputChannels != prev->OutputChannels)
     *                  return FALSE;
     *
     *              next = next->Next;
     *              prev = prev->Next;
     *      }
     *  }
     *
     *      return TRUE;
     *  }
     */

    private bool BlessLut()
    {
        if (elements is not null)
        {
            var first = FirstStage;
            var last = LastStage;

            if (first is null || last is null) return false;

            InputChannels = first.InputChannels;
            OutputChannels = last.OutputChannels;

            // Check chain consistency
            var prev = first;
            var next = prev.Next;

            while (next is not null)
            {
                if (next.InputChannels != prev!.OutputChannels)
                    return false;

                next = next.Next;
                prev = prev.Next;
            }
        }
        return true;
    }

    #endregion Private Methods

    /*  Original Code (cmslut.c line: 1416)
     *
     *  // Free a profile elements LUT
     *  void CMSEXPORT cmsPipelineFree(cmsPipeline* lut)
     *  {
     *      cmsStage *mpe, *Next;
     *
     *      if (lut == NULL) return;
     *
     *      for (mpe = lut ->Elements;
     *          mpe != NULL;
     *          mpe = Next) {
     *
     *              Next = mpe ->Next;
     *              cmsStageFree(mpe);
     *      }
     *
     *      if (lut ->FreeDataFn) lut ->FreeDataFn(lut ->ContextID, lut ->Data);
     *
     *      _cmsFree(lut ->ContextID, lut);
     *  }
     */
    /*  Original Code (cmslut.c line: 1704)
     *
     *  // Increment with reflexion on boundary
     *  static
     *  void IncDelta(cmsFloat32Number *Val)
     *  {
     *      if (*Val < (1.0 - JACOBIAN_EPSILON))
     *
     *          *Val += JACOBIAN_EPSILON;
     *
     *      else
     *          *Val -= JACOBIAN_EPSILON;
     *
     *  }
     */
    /*  Original Code (cmslut.c line: 1719)
     *
     *  // Euclidean distance between two vectors of n elements each one
     *  static
     *  cmsFloat32Number EuclideanDistance(cmsFloat32Number a[], cmsFloat32Number b[], int n)
     *  {
     *      cmsFloat32Number sum = 0;
     *      int i;
     *
     *      for (i=0; i < n; i++) {
     *          cmsFloat32Number dif = b[i] - a[i];
     *          sum +=  dif * dif;
     *      }
     *
     *      return sqrtf(sum);
     *  }
     */
}
