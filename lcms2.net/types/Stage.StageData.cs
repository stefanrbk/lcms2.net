namespace lcms2.types;

public partial class Stage
{
    public abstract class StageData: IDisposable
    {
        private bool _disposed;

        internal abstract void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage parent);

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
        internal abstract StageData? Duplicate(Stage parent);

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
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}