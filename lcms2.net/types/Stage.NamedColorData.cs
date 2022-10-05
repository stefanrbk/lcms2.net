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

namespace lcms2.types;

public partial class Stage
{
    #region Classes

    public class NamedColorData : StageData
    {
        #region Fields

        public NamedColorList List;

        private readonly EvalFn eval;

        #endregion Fields

        #region Internal Constructors

        internal NamedColorData(NamedColorList ncl, bool usePCS)
        {
            List = ncl;
            eval = usePCS
                ? EvalNamedColorPcs
                : EvalNamedColor;
        }

        #endregion Internal Constructors

        #region Delegates

        private delegate void EvalFn(ReadOnlySpan<float> @in, Span<float> @out, Stage parent);

        #endregion Delegates

        #region Internal Methods

        internal override StageData? Duplicate(Stage _) =>
            /** Original Code (cmsnamed.c line: 696)
             **
             ** static
             ** void* DupNamedColorList(cmsStage* mpe)
             ** {
             **     cmsNAMEDCOLORLIST* List = (cmsNAMEDCOLORLIST*) mpe ->Data;
             **     return cmsDupNamedColorList(List);
             ** }
             **/

            new NamedColorData((NamedColorList)List.Clone(), eval == EvalNamedColorPcs);

        internal override void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage parent) =>
            eval(@in, @out, parent);

        #endregion Internal Methods

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            /** Original Code (cmsnamed.c line: 689)
             **
             ** static
             ** void FreeNamedColorList(cmsStage* mpe)
             ** {
             **     cmsNAMEDCOLORLIST* List = (cmsNAMEDCOLORLIST*) mpe ->Data;
             **     cmsFreeNamedColorList(List);
             ** }
             **/

            List.Dispose();
            base.Dispose(disposing);
        }

        #endregion Protected Methods

        #region Private Methods

        private void EvalNamedColor(ReadOnlySpan<float> @in, Span<float> @out, Stage _)
        {
            /** Original Code (cmsnamed.c line: 722)
             **
             ** static
             ** void EvalNamedColor(const cmsFloat32Number In[], cmsFloat32Number Out[], const cmsStage *mpe)
             ** {
             **     cmsNAMEDCOLORLIST* NamedColorList = (cmsNAMEDCOLORLIST*) mpe ->Data;
             **     cmsUInt16Number index = (cmsUInt16Number) _cmsQuickSaturateWord(In[0] * 65535.0);
             **     cmsUInt32Number j;
             **
             **     if (index >= NamedColorList-> nColors) {
             **         cmsSignalError(NamedColorList ->ContextID, cmsERROR_RANGE, "Color %d out of range", index);
             **         for (j = 0; j < NamedColorList->ColorantCount; j++)
             **             Out[j] = 0.0f;
             **
             **     }
             **     else {
             **         for (j=0; j < NamedColorList ->ColorantCount; j++)
             **             Out[j] = (cmsFloat32Number) (NamedColorList->List[index].DeviceColorant[j] / 65535.0);
             **     }
             ** }
             **/

            var ncl = List;
            var index = QuickSaturateWord(@in[0] * 65535.0);

            if (index >= ncl.list.Count)
            {
                State.SignalError(ncl.state, ErrorCode.Range, "Color {0} out of range", index);
                @out[0] = @out[1] = @out[2] = 0f;
            }
            else
            {
                // Named color always uses Lab
                @out[0] = ncl.list[index].pcs[0] / 65535f;
                @out[1] = ncl.list[index].pcs[1] / 65535f;
                @out[2] = ncl.list[index].pcs[2] / 65535f;
            }
        }

        private void EvalNamedColorPcs(ReadOnlySpan<float> @in, Span<float> @out, Stage _)
        {
            /** Original Code (cmsnamed.c line: 703)
             **
             ** static
             ** void EvalNamedColorPCS(const cmsFloat32Number In[], cmsFloat32Number Out[], const cmsStage *mpe)
             ** {
             **     cmsNAMEDCOLORLIST* NamedColorList = (cmsNAMEDCOLORLIST*) mpe ->Data;
             **     cmsUInt16Number index = (cmsUInt16Number) _cmsQuickSaturateWord(In[0] * 65535.0);
             **
             **     if (index >= NamedColorList-> nColors) {
             **         cmsSignalError(NamedColorList ->ContextID, cmsERROR_RANGE, "Color %d out of range", index);
             **         Out[0] = Out[1] = Out[2] = 0.0f;
             **     }
             **     else {
             **
             **             // Named color always uses Lab
             **             Out[0] = (cmsFloat32Number) (NamedColorList->List[index].PCS[0] / 65535.0);
             **             Out[1] = (cmsFloat32Number) (NamedColorList->List[index].PCS[1] / 65535.0);
             **             Out[2] = (cmsFloat32Number) (NamedColorList->List[index].PCS[2] / 65535.0);
             **     }
             ** }
             **/

            var ncl = List;
            var index = QuickSaturateWord(@in[0] * 65535.0);

            if (index >= ncl.list.Count)
            {
                State.SignalError(ncl.state, ErrorCode.Range, "Color {0} out of range", index);
                for (var j = 0; j < ncl.colorantCount; j++)
                    @out[j] = 0f;
            }
            else
            {
                for (var j = 0; j < ncl.colorantCount; j++)
                    @out[j] = ncl.list[index].deviceColorant[j] / 65535f;
            }
        }

        #endregion Private Methods
    }

    #endregion Classes
}
