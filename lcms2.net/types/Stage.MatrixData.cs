namespace lcms2.types;

public partial class Stage
{
    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>Implements the <c>_cmsStageMatrixData</c> struct.</remarks>
    public class MatrixData: StageData
    {
        public double[] Double;
        public double[]? Offset;

        internal MatrixData(double[] @double, double[]? offset)
        {
            Double = @double;
            Offset = offset;
        }

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
}