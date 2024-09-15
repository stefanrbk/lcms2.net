//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright ©️ 1998-2024 Marti Maria Saguer
//              2022-2024 Stefan Kewatt
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

//namespace lcms2.tests;

//[TestFixture]
//public unsafe class HelperTests : TestBase
//{
//    private static readonly object[][] _doubleToS15Fixed16Values = new[]
//    {
//        new object[] {                           0.0,  0x00000000 },
//        new object[] {                           1.0,  0x00010000 },
//        new object[] { 32767.0 + (65535.0 / 65536.0),  0x7FFFFFFF },
//        new object[] { 32767.0 + (65535.0 / 65536.0),  0x7FFFFFFF },
//        new object[] {                          -1.0, -0x00010000 }
//    };

//    private static readonly double[] _quickFloorValues = new[]
//    {
//        1.234,
//        32767.234,
//        -1.234,
//        -32767.234
//    };

//    private static readonly double[] _s15Fixed16RoundTripValues = new[]
//    {
//        1.0,
//        2.0,
//        1.23456,
//        0.99999,
//        0.1234567890123456789099999,
//        -1.0,
//        -2.0,
//        -1.23456,
//        -1.1234567890123456789099999,
//        32767.1234567890123456789099999,
//        -32767.1234567890123456789099999
//    };

//    private static readonly double[] _u8Fixed8RoundTripValues = new[]
//    {
//        1.0,
//        2.0,
//        1.23456,
//        0.99999,
//        0.1234567890123456789099999,
//        255.1234567890123456789099999,
//    };

//    [Test, Category(FixedTest)]
//    public void SanityTest()
//    {
//        Assert.Multiple(() =>
//        {
//            Assert.That(sizeof(byte), Is.EqualTo(1), "byte");
//            Assert.That(sizeof(sbyte), Is.EqualTo(1), "sbyte");
//            Assert.That(sizeof(ushort), Is.EqualTo(2), "ushort");
//            Assert.That(sizeof(short), Is.EqualTo(2), "short");
//            Assert.That(sizeof(uint), Is.EqualTo(4), "uint");
//            Assert.That(sizeof(int), Is.EqualTo(4), "int");
//            Assert.That(sizeof(ulong), Is.EqualTo(8), "ulong");
//            Assert.That(sizeof(long), Is.EqualTo(8), "long");
//            Assert.That(sizeof(float), Is.EqualTo(4), "float");
//            Assert.That(sizeof(double), Is.EqualTo(8), "double");
//            Assert.That(sizeof(Signature), Is.EqualTo(4), "Signature");
//        });
//    }

//    [Test, Category(FixedTest)]
//    public void D50ConstRoundtripTest()
//    {
//        const double d50x2 = 0.96420288;
//        const double d50y2 = 1.0;
//        const double d50z2 = 0.82490540;

//        var xe = _cmsDoubleTo15Fixed16(cmsD50X);
//        var ye = _cmsDoubleTo15Fixed16(cmsD50Y);
//        var ze = _cmsDoubleTo15Fixed16(cmsD50Z);

//        var x = _cms15Fixed16toDouble(xe);
//        var y = _cms15Fixed16toDouble(ye);
//        var z = _cms15Fixed16toDouble(ze);

//        var dx = Math.Abs(cmsD50X - x);
//        var dy = Math.Abs(cmsD50Y - y);
//        var dz = Math.Abs(cmsD50Z - z);

//        var euc = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));

//        Assert.That(euc, Is.LessThan(1E-5).Within(0));

//        xe = _cmsDoubleTo15Fixed16(d50x2);
//        ye = _cmsDoubleTo15Fixed16(d50y2);
//        ze = _cmsDoubleTo15Fixed16(d50z2);

//        x = _cms15Fixed16toDouble(xe);
//        y = _cms15Fixed16toDouble(ye);
//        z = _cms15Fixed16toDouble(ze);

//        dx = Math.Abs(d50x2 - x);
//        dy = Math.Abs(d50y2 - y);
//        dz = Math.Abs(d50z2 - z);

//        euc = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));

//        Assert.That(euc, Is.LessThan(1E-5).Within(0));
//    }

//    [TestCaseSource(nameof(_doubleToS15Fixed16Values), Category = FixedTest)]
//    public void DoubleToS15Fixed16ReturnsProperFixedPointValueTest(double value, int expected) =>
//        Assert.That(_cmsDoubleTo15Fixed16(value), Is.EqualTo(expected));

//    [TestCaseSource(nameof(_quickFloorValues), Category = FixedTest)]
//    public void QuickFloorReturnsFlooredValueTest(double value) =>
//        Assert.That(_cmsQuickFloor(value), Is.EqualTo(Math.Floor(value)));

//    [Test, Category(RandomTest)]
//    public void QuickFloorWordReturnsFlooredValueTest(
//        [Random((ushort)0, (ushort)65535, 10, Distinct = true)]
//            ushort value) =>
//        Assert.That(_cmsQuickFloorWord(value + 0.1234), Is.EqualTo(value));

//    [TestCaseSource(nameof(_s15Fixed16RoundTripValues), Category = FixedTest)]
//    [TestCaseSource(nameof(S15Fixed16TestRandomGenerator), Category = RandomTest)]
//    public void S15Fixed16RoundTripTest(double value) =>
//        Assert.That(_cms15Fixed16toDouble(_cmsDoubleTo15Fixed16(value)), Is.EqualTo(value).Within(S15Fixed16Precision));

//    [TestCaseSource(nameof(_doubleToS15Fixed16Values), Category = FixedTest)]
//    public void S15Fixed16ToDoubleReturnsProperDoubleValueTest(double expected, int value) =>
//        Assert.That(_cms15Fixed16toDouble(value), Is.EqualTo(expected));

//    [TestCaseSource(nameof(_u8Fixed8RoundTripValues), Category = FixedTest)]
//    [TestCaseSource(nameof(U8Fixed8TestRandomGenerator), Category = RandomTest)]
//    public void U8Fixed8RoundTripTest(double value) =>
//        Assert.That(_cms8Fixed8toDouble(_cmsDoubleTo8Fixed8(value)), Is.EqualTo(value).Within(U8Fixed8Precision));

//    private static IEnumerable<double> S15Fixed16TestRandomGenerator()
//    {
//        var whole = new List<int>(16);
//        var frac = new List<int>(16);
//        var rand = TestContext.CurrentContext.Random;

//        for (int i = 0; i < 16; i++)
//        {
//            int w, f;
//            do
//            {
//                w = rand.Next(-32767, 32768);
//            } while (whole.Contains(w));

//            do
//            {
//                f = rand.Next(0, 65536);
//            } while (frac.Contains(f));

//            whole.Add(w);
//            frac.Add(f);

//            yield return w + (f / 65536.0);
//        }
//    }

//    private static IEnumerable<double> U8Fixed8TestRandomGenerator()
//    {
//        var whole = new List<int>(16);
//        var frac = new List<int>(16);
//        var rand = TestContext.CurrentContext.Random;

//        for (int i = 0; i < 16; i++)
//        {
//            int w, f;
//            do
//            {
//                w = rand.Next(0, 256);
//            } while (whole.Contains(w));

//            do
//            {
//                f = rand.Next(0, 256);
//            } while (frac.Contains(f));

//            whole.Add(w);
//            frac.Add(f);

//            yield return w + (f / 256.0);
//        }
//    }
//}
