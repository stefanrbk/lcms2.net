﻿//---------------------------------------------------------------------------------
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
namespace lcms2.testbed;

public static class HelpersTests
{
    #region Public Methods

    public static bool TestSingleFixed8_8(double value)
    {
        var f = DoubleToU8Fixed8(value);
        var roundTrip = U8Fixed8toDouble(f);
        var error = Math.Abs(value - roundTrip);

        return error <= FixedPrecision8_8;
    }

    #endregion Public Methods

    #region Internal Methods

    internal static bool CheckD50Roundtrip()
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

        if (euc > 1E-5)
            return Fail($"D50 roundtrip |err| > ({euc}) ");

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

        if (euc > 1E-5)
            return Fail($"D50 roundtrip |err| > ({euc}) ");

        return true;
    }

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

    #region Private Methods

    private static bool TestSingleFixed15_16(double value)
    {
        var f = DoubleToS15Fixed16(value);
        var roundTrip = S15Fixed16toDouble(f);
        var error = Math.Abs(value - roundTrip);

        return error <= FixedPrecision15_16;
    }

    #endregion Private Methods
}
