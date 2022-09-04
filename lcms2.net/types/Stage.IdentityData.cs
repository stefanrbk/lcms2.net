namespace lcms2.types;

public partial class Stage
{
    public class IdentityData: StageData
    {
        internal override StageData? Duplicate(Stage _) => null;

        /*  Original Code (cmslut.c line: 61)
         * 
         *  static
         *  void EvaluateIdentity(const cmsFloat32Number In[],
         *                              cmsFloat32Number Out[],
         *                        const cmsStage *mpe)
         *  {
         *      memmove(Out, In, mpe ->InputChannels * sizeof(cmsFloat32Number));
         *  }
         */
        internal override void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage _) =>
            @in.CopyTo(@out);
    }
}