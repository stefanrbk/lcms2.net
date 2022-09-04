namespace lcms2.types;

public sealed partial class Stage
{
    internal class XYZ2LabData: StageData
    {
        internal override StageData? Duplicate(Stage parent) =>
            new XYZ2LabData();

        /*  Original Code (cmslut.c line: 1151)
         *  
         *  static
         *  void EvaluateXYZ2Lab(const cmsFloat32Number In[], cmsFloat32Number Out[], const cmsStage *mpe)
         *  {
         *      cmsCIELab Lab;
         *      cmsCIEXYZ XYZ;
         *      const cmsFloat64Number XYZadj = MAX_ENCODEABLE_XYZ;
         *
         *      // From 0..1.0 to XYZ
         *
         *      XYZ.X = In[0] * XYZadj;
         *      XYZ.Y = In[1] * XYZadj;
         *      XYZ.Z = In[2] * XYZadj;
         *
         *      cmsXYZ2Lab(NULL, &Lab, &XYZ);
         *
         *      // From V4 Lab to 0..1.0
         *
         *      Out[0] = (cmsFloat32Number) (Lab.L / 100.0);
         *      Out[1] = (cmsFloat32Number) ((Lab.a + 128.0) / 255.0);
         *      Out[2] = (cmsFloat32Number) ((Lab.b + 128.0) / 255.0);
         *      return;
         *
         *      cmsUNUSED_PARAMETER(mpe);
         *  }
         */
        internal override void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage _)
        {
            const double xyzAdj = maxEncodableXYZ;

            // From 0..1.0 to XYZ
            var xyz = new XYZ(
                @in[0] * xyzAdj,
                @in[1] * xyzAdj,
                @in[2] * xyzAdj);

            var lab = xyz.ToLab();

            // From V4 Lab to 0..1.0
            @out[0] = (float)(lab.L / 100.0);
            @out[1] = (float)((lab.a + 128.0) / 255.0);
            @out[2] = (float)((lab.b + 128.0) / 255.0);
        }
    }
}
