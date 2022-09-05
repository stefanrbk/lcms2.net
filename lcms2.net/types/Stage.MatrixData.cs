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

    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>Implements the <c>_cmsStageMatrixData</c> struct.</remarks>
    public class MatrixData : StageData
    {
        #region Fields

        public double[] Double;
        public double[]? Offset;

        #endregion Fields

        #region Internal Constructors

        internal MatrixData(double[] @double, double[]? offset)
        {
            Double = @double;
            Offset = offset;
        }

        #endregion Internal Constructors

        /*  Original Code (cmslut.c line: 339)
         *
         *  // Duplicate a yet-existing matrix element
         *  static
         *  void* MatrixElemDup(cmsStage* mpe)
         *  {
         *      _cmsStageMatrixData* Data = (_cmsStageMatrixData*) mpe ->Data;
         *      _cmsStageMatrixData* NewElem;
         *      cmsUInt32Number sz;
         *
         *      NewElem = (_cmsStageMatrixData*) _cmsMallocZero(mpe ->ContextID, sizeof(_cmsStageMatrixData));
         *      if (NewElem == NULL) return NULL;
         *
         *      sz = mpe ->InputChannels * mpe ->OutputChannels;
         *
         *      NewElem ->Double = (cmsFloat64Number*) _cmsDupMem(mpe ->ContextID, Data ->Double, sz * sizeof(cmsFloat64Number)) ;
         *
         *      if (Data ->Offset)
         *          NewElem ->Offset = (cmsFloat64Number*) _cmsDupMem(mpe ->ContextID,
         *                                                  Data ->Offset, mpe -> OutputChannels * sizeof(cmsFloat64Number)) ;
         *
         *      return (void*) NewElem;
         *  }
         */

        #region Internal Methods

        internal override StageData? Duplicate(Stage _) =>
            new MatrixData((double[])Double.Clone(), (double[]?)Offset?.Clone());

        /*  Original Code (cmslut.c line: 310)
         *
         *  // Special care should be taken here because precision loss. A temporary cmsFloat64Number buffer is being used
         *  static
         *  void EvaluateMatrix(const cmsFloat32Number In[],
         *                      cmsFloat32Number Out[],
         *                      const cmsStage *mpe)
         *  {
         *      cmsUInt32Number i, j;
         *      _cmsStageMatrixData* Data = (_cmsStageMatrixData*) mpe ->Data;
         *      cmsFloat64Number Tmp;
         *
         *      // Input is already in 0..1.0 notation
         *      for (i=0; i < mpe ->OutputChannels; i++) {
         *
         *          Tmp = 0;
         *          for (j=0; j < mpe->InputChannels; j++) {
         *              Tmp += In[j] * Data->Double[i*mpe->InputChannels + j];
         *          }
         *
         *          if (Data ->Offset != NULL)
         *              Tmp += Data->Offset[i];
         *
         *          Out[i] = (cmsFloat32Number) Tmp;
         *      }
         *
         *
         *      // Output in 0..1.0 domain
         *  }
         */

        internal override void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage parent)
        {
            // Input is already in 0..1.0 notation
            for (var i = 0; i < parent.OutputChannels; i++)
            {
                var tmp = 0.0;
                for (var j = 0; j < parent.InputChannels; j++)
                    tmp += @in[j] * Double[(i * parent.InputChannels) + j];

                if (Offset is not null)
                    tmp += Offset[i];

                @out[i] = (float)tmp;
            }
            // Output in 0..1.0 domain
        }

        #endregion Internal Methods

        /*  Original Code (cmslut.c line: 362)
         *
         *  static
         *  void MatrixElemTypeFree(cmsStage* mpe)
         *  {
         *      _cmsStageMatrixData* Data = (_cmsStageMatrixData*) mpe ->Data;
         *      if (Data == NULL)
         *          return;
         *      if (Data ->Double)
         *          _cmsFree(mpe ->ContextID, Data ->Double);
         *
         *      if (Data ->Offset)
         *          _cmsFree(mpe ->ContextID, Data ->Offset);
         *
         *      _cmsFree(mpe ->ContextID, mpe ->Data);
         *  }
         */
        // Free is lifted up to base class as Dispose
    }

    #endregion Classes
}
