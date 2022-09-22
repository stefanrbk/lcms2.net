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
using lcms2.state;

namespace lcms2.tests;

public static class Globals
{
    #region Fields

    public const string FixedTest = "Fixed Test";
    public const double FloatPrecision = 1E-5;
    public const string RandomTest = "Random Test";
    public const double S15Fixed16Precision = 1.0 / 65535.0;
    public const double U8Fixed8Precision = 1.0 / 255.0;
    public static readonly object? TestState;

    #endregion Fields

    #region Public Constructors

    static Globals()
    {
        TestState = State.CreateStateContainer();
        State.SetLogErrorHandler(
            TestState,
            (_, ec, text) =>
                Console.Error.WriteLine($"({nameof(ErrorCode)}.{Enum.GetName(typeof(ErrorCode), ec)}): {text}"));
    }

    #endregion Public Constructors

    #region Public Methods

    public static void IsGoodDouble(string message, double actual, double expected, double delta) =>
            Assert.That(actual, Is.EqualTo(expected).Within(delta), message);

    public static void IsGoodFixed15_16(string message, double @in, double @out, object @lock, ref double maxErr) =>
        IsGoodVal(message, @in, @out, S15Fixed16Precision, @lock, ref maxErr);

    public static void IsGoodFixed15_16(string message, double @in, double @out) =>
        IsGoodVal(message, @in, @out, S15Fixed16Precision);

    public static void IsGoodFixed8_8(string message, double @in, double @out, object @lock, ref double maxErr) =>
        IsGoodVal(message, @in, @out, U8Fixed8Precision, @lock, ref maxErr);

    public static void IsGoodVal(string message, double @in, double @out, double max, object @lock, ref double maxErr)
    {
        var err = Math.Abs(@in - @out);

        lock (@lock)
            if (err > maxErr) maxErr = err;

        Assert.That(@in, Is.EqualTo(@out).Within(max), message);
    }

    public static void IsGoodVal(string message, double @in, double @out, double max)
    {
        var err = Math.Abs(@in - @out);

        Assert.That(@in, Is.EqualTo(@out).Within(max), message);
    }

    public static void IsGoodWord(string message, ushort @in, ushort @out, ushort maxErr = 0) =>
        Assert.That(@in, Is.EqualTo(@out).Within(maxErr), message);

    public static double Sqr(double v) =>
        v * v;

    #endregion Public Methods
}
