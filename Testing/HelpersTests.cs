namespace lcms2.testing;

[TestFixture(TestOf = typeof(Helpers))]
public class HelpersTests
{
    #region Public Methods

    [TestCase(1.0)]
    [TestCase(2.0)]
    [TestCase(1.23456)]
    [TestCase(0.99999)]
    [TestCase(0.1234567890123456789099999)]
    [TestCase(-1.0)]
    [TestCase(-2.0)]
    [TestCase(-1.123456)]
    [TestCase(-1.1234567890123456789099999)]
    [TestCase(32767.1234567890123456789099999)]
    [TestCase(-32767.1234567890123456789099999)]
    public void FixedPoint15_16Test(double value)
    {
        var f = DoubleToS15Fixed16(value);
        var roundTrip = S15Fixed16toDouble(f);

        Assert.That(roundTrip, Is.EqualTo(value).Within(1.0 / 65535.0));
    }

    [TestCase(1.0)]
    [TestCase(2.0)]
    [TestCase(1.23456)]
    [TestCase(0.99999)]
    [TestCase(0.1234567890123456789099999)]
    [TestCase(255.1234567890123456789099999)]
    public void FixedPoint8_8Test(double value)
    {
        var f = DoubleToU8Fixed8(value);
        var roundTrip = U8Fixed8toDouble(f);

        Assert.That(roundTrip, Is.EqualTo(value).Within(1.0 / 255.0));
    }

    [TestCase(1.234, (short)1)]
    [TestCase(32767.234, (short)32767)]
    [TestCase(-1.234, (short)-2)]
    [TestCase(-32767.1, (short)-32768)]
    public void QuickFloorTest(double value, short expected) =>
        Assert.That(QuickFloor(value), Is.EqualTo(expected));

    [Test]
    public void QuickFloorWordTest()
    {
        for (var i = 0; i < UInt16.MaxValue; i++)
            Assert.That(QuickFloorWord(i + 0.1234), Is.EqualTo(i));
    }

    #endregion Public Methods

    #region Internal Methods

    internal void CheckFixedPoint15_16()
    {
        try
        {
            FixedPoint15_16Test(1.0);
            FixedPoint15_16Test(2.0);
            FixedPoint15_16Test(1.23456);
            FixedPoint15_16Test(0.99999);
            FixedPoint15_16Test(0.1234567890123456789099999);
            FixedPoint15_16Test(-1.0);
            FixedPoint15_16Test(-2.0);
            FixedPoint15_16Test(-1.123456);
            FixedPoint15_16Test(-1.1234567890123456789099999);
            FixedPoint15_16Test(32767.1234567890123456789099999);
            FixedPoint15_16Test(-32767.1234567890123456789099999);
        }
        catch
        {
            Assert.Fail();
        }
    }

    internal void CheckFixedPoint8_8()
    {
        try
        {
            FixedPoint8_8Test(1.0);
            FixedPoint8_8Test(2.0);
            FixedPoint8_8Test(1.23456);
            FixedPoint8_8Test(0.99999);
            FixedPoint8_8Test(0.1234567890123456789099999);
            FixedPoint8_8Test(255.1234567890123456789099999);
        }
        catch
        {
            Assert.Fail();
        }
    }

    internal static bool CheckQuickFloor()
    {
        if (QuickFloor(1.234) is not 1 ||
            QuickFloor(32767.234) is not 32767 ||
            QuickFloor(-1.234) is not -2 ||
            QuickFloor(-32767.1) is not -32767)
        {
            Die("\nOOOPPSS! Helpers.QuickFloor() does not work as expected in your machine!\n\n" +
                "Please use the \"(No Fast Floor)\" configuration toggles.\n");
            return false;
        }
        return true;
    }

    internal void CheckQuickFloorWord()
    {
        try
        {
            QuickFloorWordTest();
        } catch
        {
            Die("\nOOOPPSS! Helpers.QuickFloorWord() does not work as expected in your machine!\n\nPlease use the \"(No Fast Floor)\" configuration toggles.\n");
        }
    }

    #endregion Internal Methods
}
