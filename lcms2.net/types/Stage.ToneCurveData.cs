namespace lcms2.types;

public partial class Stage
{
    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>Implements the <c>_cmsStageToneCurvesData</c> struct.</remarks>
    public class ToneCurveData: StageData
    {
        public ToneCurve[] TheCurves;

        internal ToneCurveData(ToneCurve[]? theCurves = null) =>
            TheCurves = theCurves ?? Array.Empty<ToneCurve>();

        public int NumCurves =>
            TheCurves.Length;

        /*  Original Code (cmslut.c line: 166)
         *  
         *  static
         *  void EvaluateCurves(const cmsFloat32Number In[],
         *                      cmsFloat32Number Out[],
         *                      const cmsStage *mpe)
         *  {
         *      _cmsStageToneCurvesData* Data;
         *      cmsUInt32Number i;
         *
         *      _cmsAssert(mpe != NULL);
         *
         *      Data = (_cmsStageToneCurvesData*) mpe ->Data;
         *      if (Data == NULL) return;
         *
         *      if (Data ->TheCurves == NULL) return;
         *
         *      for (i=0; i < Data ->nCurves; i++) {
         *          Out[i] = cmsEvalToneCurveFloat(Data ->TheCurves[i], In[i]);
         *      }
         *  }
         */
        internal override void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage _)
        {
            for (var i = 0; i < NumCurves; i++)
                @out[i] = TheCurves[i].Eval(@in[i]);
        }

        /*  Original Code (cmslut.c line: 186)
         *  
         *  static
         *  void CurveSetElemTypeFree(cmsStage* mpe)
         *  {
         *      _cmsStageToneCurvesData* Data;
         *      cmsUInt32Number i;
         *
         *      _cmsAssert(mpe != NULL);
         *
         *      Data = (_cmsStageToneCurvesData*) mpe ->Data;
         *      if (Data == NULL) return;
         *
         *      if (Data ->TheCurves != NULL) {
         *          for (i=0; i < Data ->nCurves; i++) {
         *              if (Data ->TheCurves[i] != NULL)
         *                  cmsFreeToneCurve(Data ->TheCurves[i]);
         *          }
         *      }
         *      _cmsFree(mpe ->ContextID, Data ->TheCurves);
         *      _cmsFree(mpe ->ContextID, Data);
         *  }
         */
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                TheCurves.All(t =>
                {
                    t.Dispose();
                    return true;
                });
            }
            base.Dispose(isDisposing);
        }

        /*  Original Code (cmslut.c line: 208)
         *  
         *  static
         *  void* CurveSetDup(cmsStage* mpe)
         *  {
         *      _cmsStageToneCurvesData* Data = (_cmsStageToneCurvesData*) mpe ->Data;
         *      _cmsStageToneCurvesData* NewElem;
         *      cmsUInt32Number i;
         *
         *      NewElem = (_cmsStageToneCurvesData*) _cmsMallocZero(mpe ->ContextID, sizeof(_cmsStageToneCurvesData));
         *      if (NewElem == NULL) return NULL;
         *
         *      NewElem ->nCurves   = Data ->nCurves;
         *      NewElem ->TheCurves = (cmsToneCurve**) _cmsCalloc(mpe ->ContextID, NewElem ->nCurves, sizeof(cmsToneCurve*));
         *
         *      if (NewElem ->TheCurves == NULL) goto Error;
         *
         *      for (i=0; i < NewElem ->nCurves; i++) {
         *
         *          // Duplicate each curve. It may fail.
         *          NewElem ->TheCurves[i] = cmsDupToneCurve(Data ->TheCurves[i]);
         *          if (NewElem ->TheCurves[i] == NULL) goto Error;
         *
         *
         *      }
         *      return (void*) NewElem;
         *
         *  Error:
         *
         *      if (NewElem ->TheCurves != NULL) {
         *          for (i=0; i < NewElem ->nCurves; i++) {
         *              if (NewElem ->TheCurves[i])
         *                  cmsFreeToneCurve(NewElem ->TheCurves[i]);
         *          }
         *      }
         *      _cmsFree(mpe ->ContextID, NewElem ->TheCurves);
         *      _cmsFree(mpe ->ContextID, NewElem);
         *      return NULL;
         *  }
         */
        internal override StageData? Duplicate(Stage _) =>
            new ToneCurveData(TheCurves.Select(t => (ToneCurve)t.Clone())
                                       .ToArray());
    }
}