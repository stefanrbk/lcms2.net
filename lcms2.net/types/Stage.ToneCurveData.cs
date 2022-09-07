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
    #region Classes

    public class ToneCurveData : StageData
    {
        #region Fields

        public ToneCurve[] TheCurves;

        #endregion Fields

        #region Internal Constructors

        internal ToneCurveData(ToneCurve[]? theCurves = null) =>
            TheCurves = theCurves ?? Array.Empty<ToneCurve>();

        #endregion Internal Constructors

        #region Properties

        public int NumCurves =>
            TheCurves.Length;

        #endregion Properties

        #region Internal Methods

        internal override StageData? Duplicate(Stage _) =>
            /** Original Code (cmslut.c line: 208)
             **
             ** static
             ** void* CurveSetDup(cmsStage* mpe)
             ** {
             **     _cmsStageToneCurvesData* Data = (_cmsStageToneCurvesData*) mpe ->Data;
             **     _cmsStageToneCurvesData* NewElem;
             **     cmsUInt32Number i;
             **
             **     NewElem = (_cmsStageToneCurvesData*) _cmsMallocZero(mpe ->ContextID, sizeof(_cmsStageToneCurvesData));
             **     if (NewElem == NULL) return NULL;
             **
             **     NewElem ->nCurves   = Data ->nCurves;
             **     NewElem ->TheCurves = (cmsToneCurve**) _cmsCalloc(mpe ->ContextID, NewElem ->nCurves, sizeof(cmsToneCurve*));
             **
             **     if (NewElem ->TheCurves == NULL) goto Error;
             **
             **     for (i=0; i < NewElem ->nCurves; i++) {
             **
             **         // Duplicate each curve. It may fail.
             **         NewElem ->TheCurves[i] = cmsDupToneCurve(Data ->TheCurves[i]);
             **         if (NewElem ->TheCurves[i] == NULL) goto Error;
             **
             **
             **     }
             **     return (void*) NewElem;
             **
             ** Error:
             **
             **     if (NewElem ->TheCurves != NULL) {
             **         for (i=0; i < NewElem ->nCurves; i++) {
             **             if (NewElem ->TheCurves[i])
             **                 cmsFreeToneCurve(NewElem ->TheCurves[i]);
             **         }
             **     }
             **     _cmsFree(mpe ->ContextID, NewElem ->TheCurves);
             **     _cmsFree(mpe ->ContextID, NewElem);
             **     return NULL;
             ** }
             **/

            new ToneCurveData(TheCurves.Select(t => (ToneCurve)t.Clone())
                                       .ToArray());

        internal override void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage _)
        {
            /** Original Code (cmslut.c line: 166)
             **
             ** static
             ** void EvaluateCurves(const cmsFloat32Number In[],
             **                     cmsFloat32Number Out[],
             **                     const cmsStage *mpe)
             ** {
             **     _cmsStageToneCurvesData* Data;
             **     cmsUInt32Number i;
             **
             **     _cmsAssert(mpe != NULL);
             **
             **     Data = (_cmsStageToneCurvesData*) mpe ->Data;
             **     if (Data == NULL) return;
             **
             **     if (Data ->TheCurves == NULL) return;
             **
             **     for (i=0; i < Data ->nCurves; i++) {
             **         Out[i] = cmsEvalToneCurveFloat(Data ->TheCurves[i], In[i]);
             **     }
             ** }
             **/

            for (var i = 0; i < NumCurves; i++)
                @out[i] = TheCurves[i].Eval(@in[i]);
        }

        #endregion Internal Methods

        #region Protected Methods

        protected override void Dispose(bool isDisposing)
        {
            /** Original Code (cmslut.c line: 186)
             **
             ** static
             ** void CurveSetElemTypeFree(cmsStage* mpe)
             ** {
             **     _cmsStageToneCurvesData* Data;
             **     cmsUInt32Number i;
             **
             **     _cmsAssert(mpe != NULL);
             **
             **     Data = (_cmsStageToneCurvesData*) mpe ->Data;
             **     if (Data == NULL) return;
             **
             **     if (Data ->TheCurves != NULL) {
             **         for (i=0; i < Data ->nCurves; i++) {
             **             if (Data ->TheCurves[i] != NULL)
             **                 cmsFreeToneCurve(Data ->TheCurves[i]);
             **         }
             **     }
             **     _cmsFree(mpe ->ContextID, Data ->TheCurves);
             **     _cmsFree(mpe ->ContextID, Data);
             ** }
             **/

            if (isDisposing)
            {
                _ = TheCurves.All(t =>
                {
                    t.Dispose();
                    return true;
                });
            }
            base.Dispose(isDisposing);
        }

        #endregion Protected Methods
    }

    #endregion Classes
}
