namespace lcms2.types;

public sealed partial class Stage
{
    #region Classes

    internal class ClipperData: StageData
    {
        #region Internal Methods

        internal override StageData? Duplicate(Stage parent) =>
            new ClipperData();

        internal override void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage parent)
        {
            /** Original Code (cmslut.c line: 1129)
             ** 
             ** // Clips values smaller than zero
             ** static
             ** void Clipper(const cmsFloat32Number In[], cmsFloat32Number Out[], const cmsStage *mpe)
             ** {
             **        cmsUInt32Number i;
             **        for (i = 0; i < mpe->InputChannels; i++) {
             **
             **               cmsFloat32Number n = In[i];
             **               Out[i] = n < 0 ? 0 : n;
             **        }
             ** }
             **/

            for (var i = 0; i < parent.InputChannels; i++)
                @out[i] = Math.Max(@in[i], 0);
        }

        #endregion Internal Methods
    }

    #endregion Classes
}
