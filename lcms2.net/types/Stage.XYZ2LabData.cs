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

    internal class XYZ2LabData : StageData
    {
        #region Internal Methods

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

        #endregion Internal Methods
    }

    #endregion Classes
}
