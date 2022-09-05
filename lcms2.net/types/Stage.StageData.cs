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

    public abstract class StageData : IDisposable
    {
        #region Fields

        private bool _disposed;

        #endregion Fields

        #region Public Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion Public Methods

        #region Internal Methods

        internal abstract StageData? Duplicate(Stage parent);

        internal abstract void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage parent);

        #endregion Internal Methods

        /*  Original Code (cmslut.c line: 1240)
         *
         *  // Duplicates an MPE
         *  cmsStage* CMSEXPORT cmsStageDup(cmsStage* mpe)
         *  {
         *      cmsStage* NewMPE;
         *
         *      if (mpe == NULL) return NULL;
         *      NewMPE = _cmsStageAllocPlaceholder(mpe ->ContextID,
         *                                       mpe ->Type,
         *                                       mpe ->InputChannels,
         *                                       mpe ->OutputChannels,
         *                                       mpe ->EvalPtr,
         *                                       mpe ->DupElemPtr,
         *                                       mpe ->FreePtr,
         *                                       NULL);
         *      if (NewMPE == NULL) return NULL;
         *
         *      NewMPE ->Implements = mpe ->Implements;
         *
         *      if (mpe ->DupElemPtr) {
         *
         *          NewMPE ->Data = mpe ->DupElemPtr(mpe);
         *
         *          if (NewMPE->Data == NULL) {
         *
         *              cmsStageFree(NewMPE);
         *              return NULL;
         *          }
         *
         *      } else {
         *
         *          NewMPE ->Data       = NULL;
         *      }
         *
         *      return NewMPE;
         *  }
         */
        /*  Original Code (cmslut.c line: 1199)
         *
         *  // Free a single MPE
         *  void CMSEXPORT cmsStageFree(cmsStage* mpe)
         *  {
         *      if (mpe ->FreePtr)
         *          mpe ->FreePtr(mpe);
         *
         *      _cmsFree(mpe ->ContextID, mpe);
         *  }
         */

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        #endregion Protected Methods
    }

    #endregion Classes
}
