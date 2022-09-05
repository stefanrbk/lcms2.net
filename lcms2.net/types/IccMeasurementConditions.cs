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

public class IccMeasurementConditions : ICloneable
{
    #region Fields

    // Value of backing
    public XYZ Backing;

    // 0..1.0
    public double Flare;

    // 0=unknown, 1=45/0, 0/45 2=0d, d/0
    public uint Geometry;

    public IlluminantType IlluminantType;

    // 0 = unknown, 1=CIE 1931, 2=CIE 1964
    public uint Observer;

    #endregion Fields

    #region Public Methods

    public object Clone() =>
        new IccMeasurementConditions()
        {
            Observer = Observer,
            Backing = Backing,
            Geometry = Geometry,
            Flare = Flare,
            IlluminantType = IlluminantType,
        };

    #endregion Public Methods
}
