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

public sealed partial class Stage
{
    #region Classes

    internal class Lab2XYZData : StageData
    {
        #region Internal Methods

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

        #endregion Internal Methods
    }

    #endregion Classes
}
