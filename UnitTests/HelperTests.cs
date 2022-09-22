using static lcms2.Helpers;

namespace lcms2.tests;

[TestFixture(TestOf = typeof(Helpers))]
public class HelperTests
{
    [TestCaseSource(nameof(_quickFloorValues), Category = FixedTest)]
    public void QuickFloorReturnsFlooredValueTest(double value) =>
        Assert.That(QuickFloor(value), Is.EqualTo(Math.Floor(value)));

    [Test, Category(RandomTest)]
    public void QuickFloorWordReturnsFlooredValueTest(
        [Random((ushort)0, (ushort)65534, 10, Distinct = true)]
            ushort value) =>
        Assert.That(QuickFloorWord(value + 0.1234), Is.EqualTo(value));

    [TestCaseSource(nameof(_doubleToS15Fixed16Values), Category = FixedTest)]
    public void DoubleToS15Fixed16ReturnsProperFixedPointValueTest(double value, int expected) =>
        Assert.That(DoubleToS15Fixed16(value), Is.EqualTo(expected));

    [TestCaseSource(nameof(_doubleToS15Fixed16Values), Category = FixedTest)]
    public void S15Fixed16ToDoubleReturnsProperDoubleValueTest(double expected, int value) =>
        Assert.That(S15Fixed16toDouble(value), Is.EqualTo(expected));

    [TestCaseSource(nameof(_s15Fixed16RoundTripValues), Category = FixedTest)]
    [TestCaseSource(nameof(S15Fixed16TestRandomGenerator), Category = RandomTest)]
    public void S15Fixed16RoundTripTest(double value) =>
        Assert.That(S15Fixed16toDouble(DoubleToS15Fixed16(value)), Is.EqualTo(value).Within(S15Fixed16Precision));

    [TestCaseSource(nameof(_u8Fixed8RoundTripValues), Category = FixedTest)]
    [TestCaseSource(nameof(U8Fixed8TestRandomGenerator), Category = RandomTest)]
    public void U8Fixed8RoundTripTest(double value) =>
        Assert.That(U8Fixed8toDouble(DoubleToU8Fixed8(value)), Is.EqualTo(value).Within(U8Fixed8Precision));

    [Test, Category(FixedTest)]
    public void D50ConstRoundtripTest()
    {
        const double d50x2 = 0.96420288;
        const double d50y2 = 1.0;
        const double d50z2 = 0.82490540;

        var xe = DoubleToS15Fixed16(Lcms2.D50.X);
        var ye = DoubleToS15Fixed16(Lcms2.D50.Y);
        var ze = DoubleToS15Fixed16(Lcms2.D50.Z);

        var x = S15Fixed16toDouble(xe);
        var y = S15Fixed16toDouble(ye);
        var z = S15Fixed16toDouble(ze);

        var dx = Math.Abs(Lcms2.D50.X - x);
        var dy = Math.Abs(Lcms2.D50.Y - y);
        var dz = Math.Abs(Lcms2.D50.Z - z);

        var euc = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));

        Assert.That(euc, Is.LessThan(1E-5).Within(0));

        xe = DoubleToS15Fixed16(d50x2);
        ye = DoubleToS15Fixed16(d50y2);
        ze = DoubleToS15Fixed16(d50z2);

        x = S15Fixed16toDouble(xe);
        y = S15Fixed16toDouble(ye);
        z = S15Fixed16toDouble(ze);

        dx = Math.Abs(d50x2 - x);
        dy = Math.Abs(d50y2 - y);
        dz = Math.Abs(d50z2 - z);

        euc = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));

        Assert.That(euc, Is.LessThan(1E-5).Within(0));
    }

    private static readonly double[] _quickFloorValues = new[]
    {
        1.234,
        32767.234,
        -1.234,
        -32767.234
    };
    private static readonly object[][] _doubleToS15Fixed16Values = new[]
    {
        new object[] {                           0.0,  0x00000000 },
        new object[] {                           1.0,  0x00010000 },
        new object[] { 32767.0 + (65535.0 / 65536.0),  0x7FFFFFFF },
        new object[] { 32767.0 + (65535.0 / 65536.0),  0x7FFFFFFF },
        new object[] {                          -1.0, -0x00010000 }
    };
    private static readonly double[] _s15Fixed16RoundTripValues = new[]
    {
        1.0,
        2.0,
        1.23456,
        0.99999,
        0.1234567890123456789099999,
        -1.0,
        -2.0,
        -1.23456,
        -1.1234567890123456789099999,
        32767.1234567890123456789099999,
        -32767.1234567890123456789099999
    };
    private static readonly double[] _u8Fixed8RoundTripValues = new[]
    {
        1.0,
        2.0,
        1.23456,
        0.99999,
        0.1234567890123456789099999,
        255.1234567890123456789099999,
    };
    private static IEnumerable<double> S15Fixed16TestRandomGenerator()
    {
        var whole = new List<int>();
        var frac = new List<int>();
        var rand = TestContext.CurrentContext.Random;

        for (int i = 0; i < 16; i++)
        {
            int w, f;
            do
            {
                w = rand.Next(-32767, 32768);
            } while (whole.Contains(w));

            do
            {
                f = rand.Next(0, 65536);
            } while (frac.Contains(f));

            whole.Add(w);
            frac.Add(f);

            yield return w + (f / 65536.0);
        }
    }
    private static IEnumerable<double> U8Fixed8TestRandomGenerator()
    {
        var whole = new List<int>();
        var frac = new List<int>();
        var rand = TestContext.CurrentContext.Random;

        for (int i = 0; i < 16; i++)
        {
            int w, f;
            do
            {
                w = rand.Next(0, 256);
            } while (whole.Contains(w));

            do
            {
                f = rand.Next(0, 256);
            } while (frac.Contains(f));

            whole.Add(w);
            frac.Add(f);

            yield return w + (f / 256.0);
        }
    }
}
