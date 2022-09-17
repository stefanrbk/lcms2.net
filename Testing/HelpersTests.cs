namespace lcms2.testbed;

public static class HelpersTests
{
    #region Public Methods

    private static bool TestSingleFixed15_16(double value)
    {
        var f = DoubleToS15Fixed16(value);
        var roundTrip = S15Fixed16toDouble(f);
        var error = Math.Abs(value - roundTrip);

        return error <= FixedPrecision15_16;
    }

    public static bool TestSingleFixed8_8(double value)
    {
        var f = DoubleToU8Fixed8(value);
        var roundTrip = U8Fixed8toDouble(f);
        var error = Math.Abs(value - roundTrip);

        return error <= FixedPrecision8_8;
    }

    #endregion Public Methods

    #region Internal Methods

    internal static bool CheckFixedPoint15_16() =>
        TestSingleFixed15_16(1.0) &&
        TestSingleFixed15_16(2.0) &&
        TestSingleFixed15_16(1.23456) &&
        TestSingleFixed15_16(0.99999) &&
        TestSingleFixed15_16(0.1234567890123456789099999) &&
        TestSingleFixed15_16(-1.0) &&
        TestSingleFixed15_16(-2.0) &&
        TestSingleFixed15_16(-1.123456) &&
        TestSingleFixed15_16(-1.1234567890123456789099999) &&
        TestSingleFixed15_16(32767.1234567890123456789099999) &&
        TestSingleFixed15_16(-32767.1234567890123456789099999);

    internal static bool CheckFixedPoint8_8() =>
        TestSingleFixed8_8(1.0) &&
        TestSingleFixed8_8(2.0) &&
        TestSingleFixed8_8(1.23456) &&
        TestSingleFixed8_8(0.99999) &&
        TestSingleFixed8_8(0.1234567890123456789099999) &&
        TestSingleFixed8_8(255.1234567890123456789099999);

    internal static bool CheckQuickFloor()
    {
        if (QuickFloor(1.234) is not 1 ||
            QuickFloor(32767.234) is not 32767 ||
            QuickFloor(-1.234) is not -2 ||
            QuickFloor(-32767.1) is not -32768)
        {
            Die("\nOOOPPSS! Helpers.QuickFloor() does not work as expected in your machine!\n\n" +
                "Please use the \"(No Fast Floor)\" configuration toggles.\n");
            return false;
        }
        return true;
    }

    internal static bool CheckQuickFloorWord()
    {

        for (var i = 0; i < UInt16.MaxValue; i++)
        {
            if (QuickFloorWord(i + 0.1234) != i)
            {
                Die("\nOOOPPSS! Helpers.QuickFloorWord() does not work as expected in your machine!\n\nPlease use the \"(No Fast Floor)\" configuration toggles.\n");
                return false;
            }
        }
        return true;
    }

    #endregion Internal Methods
}
