namespace lcms2.types;

public sealed partial class Stage
{
    internal class Lab2XYZData: StageData
    {
        internal override StageData? Duplicate(Stage parent) =>
            new Lab2XYZData();

        /*  Original Code (cmslut.c line: 937)
         *  
         *  static
         *  void EvaluateLab2XYZ(const cmsFloat32Number In[],
         *                       cmsFloat32Number Out[],
         *                       const cmsStage *mpe)
         *  {
         *      cmsCIELab Lab;
         *      cmsCIEXYZ XYZ;
         *      const cmsFloat64Number XYZadj = MAX_ENCODEABLE_XYZ;
         *
         *      // V4 rules
         *      Lab.L = In[0] * 100.0;
         *      Lab.a = In[1] * 255.0 - 128.0;
         *      Lab.b = In[2] * 255.0 - 128.0;
         *
         *      cmsLab2XYZ(NULL, &XYZ, &Lab);
         *
         *      // From XYZ, range 0..19997 to 0..1.0, note that 1.99997 comes from 0xffff
         *      // encoded as 1.15 fixed point, so 1 + (32767.0 / 32768.0)
         *
         *      Out[0] = (cmsFloat32Number) ((cmsFloat64Number) XYZ.X / XYZadj);
         *      Out[1] = (cmsFloat32Number) ((cmsFloat64Number) XYZ.Y / XYZadj);
         *      Out[2] = (cmsFloat32Number) ((cmsFloat64Number) XYZ.Z / XYZadj);
         *      return;
         *
         *      cmsUNUSED_PARAMETER(mpe);
         *  }
         */
        internal override void Evaluate(ReadOnlySpan<float> @in, Span<float> @out, Stage _)
        {
            const double xyzAdj = maxEncodableXYZ;

            // V4 rules
            var lab = new Lab(
                @in[0] * 100.0,
               (@in[1] * 255.0) - 128.0,
               (@in[2] * 255.0) - 128.0);

            var xyz = lab.ToXYZ();

            // From XYZ, range 0..19997 to 0..1.0, note that 1.99997 comes from 0xffff
            // encoded as 1.15 fixed point, so 1 + (32767.0 / 32768.0)
            @out[0] = (float)(xyz.X / xyzAdj);
            @out[1] = (float)(xyz.Y / xyzAdj);
            @out[2] = (float)(xyz.Z / xyzAdj);
        }
    }
}
