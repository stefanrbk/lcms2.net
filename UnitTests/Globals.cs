using lcms2.state;

namespace lcms2.tests;
public static class Globals
{
    public const double S15Fixed16Precision = 1.0 / 65535.0;
    public const double U8Fixed8Precision = 1.0 / 255.0;
    public const double FloatPrecision = 1E-5;

    public const string FixedTest = "Fixed Test";
    public const string RandomTest = "Random Test";

    public static readonly object? TestState;

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

    static Globals()
    {
        TestState = State.CreateStateContainer();
        State.SetLogErrorHandler(
            TestState,
            (_, ec, text) =>
                Console.Error.WriteLine($"({nameof(ErrorCode)}.{Enum.GetName(typeof(ErrorCode), ec)}): {text}"));
    }
}
