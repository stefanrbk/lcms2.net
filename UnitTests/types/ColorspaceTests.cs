//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
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
//namespace lcms2.tests.types;

//[TestFixture]
//public class ColorspaceTests
//{
//    #region Public Methods

//    [Test, TestOf(typeof(LCh)), TestOf(typeof(Lab)), Category(RandomTest)]
//    public void Lab2LChTest(
//        [Random(0, 100, 4)]
//            int l,
//        [Random(-128, 128, 4)]
//            int a,
//        [Random(-128, 128, 4)]
//            int b)
//    {
//        var lab = new Lab(l, a, b);
//        var lch = (LCh)lab;
//        var lab2 = (Lab)lch;

//        Assert.That(lab, Is.EqualTo(lab2).Using<Lab>(DeltaE), $"({l},{a},{b}): Difference outside tolerence.");
//    }

//    [Test, TestOf(typeof(XYZ)), TestOf(typeof(xyY)), TestOf(typeof(Lab)), Category(RandomTest)]
//    public void Lab2xyYTest(
//        [Random(0, 100, 4, Distinct = true)]
//            int l,
//        [Random(-128, 128, 4, Distinct = true)]
//            int a,
//        [Random(-128, 128, 4, Distinct = true)]
//            int b)
//    {
//        var lab = new Lab(l, a, b);
//        var xyz = (XYZ)lab;
//        var xyy = (xyY)xyz;
//        var xyz2 = (XYZ)xyy;
//        var lab2 = (Lab)xyz2;

//        Assert.That(lab, Is.EqualTo(lab2).Using<Lab>(DeltaE), $"({l},{a},{b}): Difference outside tolerence.");
//    }

//    [Test, TestOf(typeof(XYZ)), TestOf(typeof(Lab)), Category(RandomTest)]
//    public void Lab2XYZTest(
//        [Random(0, 100, 4)]
//            int l,
//        [Random(-128, 128, 4)]
//            int a,
//        [Random(-128, 128, 4)]
//            int b)
//    {
//        var lab = new Lab(l, a, b);
//        var xyz = (XYZ)lab;
//        var lab2 = (Lab)xyz;

//        Assert.That(lab, Is.EqualTo(lab2).Using<Lab>(DeltaE), $"({l},{a},{b}): Difference outside tolerence.");
//    }

//    [Test, TestOf(typeof(Lab)), TestOf(typeof(LabEncodedV2)), Category(RandomTest)]
//    public void LabV2EncodingTest(
//        [Random(0, 65535, 64, Distinct = true)]
//            int j)
//    {
//        var aw1 = new LabEncodedV2((ushort)j, (ushort)j, (ushort)j);
//        var lab = (Lab)aw1;
//        var aw2 = (LabEncodedV2)lab;

//        Assert.That(aw1, Is.EqualTo(aw2));
//    }

//    [Test, TestOf(typeof(Lab)), TestOf(typeof(LabEncoded)), Category(RandomTest)]
//    public void LabV4EncodingTest(
//        [Random(0, 65535, 64, Distinct = true)]
//            int j)
//    {
//        var aw1 = new LabEncoded((ushort)j, (ushort)j, (ushort)j);
//        var lab = (Lab)aw1;
//        var aw2 = (LabEncoded)lab;

//        Assert.That(aw1, Is.EqualTo(aw2));
//    }

//    #endregion Public Methods

//    #region Private Methods

//    private static bool DeltaE(Lab lab1, Lab lab2)
//    {
//        var dL = Math.Abs(lab1.L - lab2.L);
//        var da = Math.Abs(lab1.a - lab2.a);
//        var db = Math.Abs(lab1.b - lab2.b);

//        return Math.Pow(Sqr(dL) + Sqr(da) + Sqr(db), 0.5) < 1E-12;
//    }

//    #endregion Private Methods
//}
