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

    internal class ClipperData : StageData
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
