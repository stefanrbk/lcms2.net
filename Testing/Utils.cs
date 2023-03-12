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
//using lcms2.types;

//using System.Diagnostics.CodeAnalysis;

//[assembly: ExcludeFromCodeCoverage]

//namespace lcms2.testbed;

//public static unsafe class Utils
//{
//    private static object maxErrLock = new();

//    public static bool CheckGammaEstimation(ToneCurve c, double g)
//    {
//        var est = c.EstimateGamma(1E-3);

//        SubTest("Gamma estimation");
//        return Math.Abs(est - g) <= 1E-3;
//    }

//    public static bool CheckInterp1D(Action<uint, uint> fn)
//    {
//        if (HasConsole)
//            Console.Write("10 - 4096");
//        var result = true;
//        for (uint i = 0, j = 1; i < 16; i++, j++)
//        {
//            try
//            {
//                fn(i is 0 ? 10 : 256 * i, 256 * j);
//            }
//            catch
//            {
//                result = false;
//            }
//        }

//        if (HasConsole)
//            Console.Write("\nThe result is ");
//        return result;
//    }

//    public static bool CheckInterp3D(Action<uint, uint> fn)
//    {
//        if (HasConsole)
//            Console.Write("0 - 256");
//        var result = true;
//        for (uint i = 0, j = 1; i < 16; i++, j++)
//        {
//            try
//            {
//                fn(16 * i, 16 * j);
//            }
//            catch
//            {
//                result = false;
//            }
//        }

//        if (HasConsole)
//            Console.Write("\nThe result is ");
//        return result;
//    }

//    public static double DeltaE(Lab lab1, Lab lab2)
//    {
//        var dL = Math.Abs(lab1.L - lab2.L);
//        var da = Math.Abs(lab1.a - lab2.a);
//        var db = Math.Abs(lab1.b - lab2.b);

//        return Math.Pow(Sqr(dL) + Sqr(da) + Sqr(db), 0.5);
//    }

//    public static void DumpToneCurve(ToneCurve gamma, string filename)
//    {
//        var it8 = new IT8();

//        it8.SetPropertyDouble("NUMBER_OF_FIELDS", 2);
//        it8.SetPropertyDouble("NUMBER_OF_SETS", gamma.NumEntries);

//        it8.SetDataFormat(0, "SAMPLE_ID");
//        it8.SetDataFormat(1, "VALUE");

//        for (var i = 0; i < gamma.NumEntries; i++)
//        {
//            it8.SetDataRowCol(i, 0, i);
//            it8.SetDataRowCol(i, 1, gamma.EstimatedTable[i]);
//        }

//        it8.SaveToFile(filename);
//    }

//    public static bool IsGoodDouble(string title, double actual, double expected, double delta)
//    {
//        var err = Math.Abs(actual - expected);

//        if (err > delta)
//            return Fail($"({title}): Must be {expected}, but was {actual}");

//        return true;
//    }

//    public static void ProgressBar(uint start, uint stop, int widthSubtract, uint i)
//    {
//        if (!HasConsole) return;

//        var width = Console.BufferWidth - widthSubtract;
//        var percent = (double)(i - start) / (stop - start);
//        var filled = (int)Math.Round((double)percent * width);
//        var empty = width - filled;
//        Console.Write($"{new string(' ', 8)}{start} {new string('█', filled)}{new string('▓', empty)} {stop}\r");
//    }
//}

public static class Con
{
    public static void Write(params object[] args) =>
        _write(Console.Out, args);

    public static void WriteLine(params object[] args)
    {
        Write(args);
        Console.WriteLine();
    }
}

public static class Err
{
    public static void Write(params object[] args) =>
        _write(Console.Error, args);

    public static void WriteLine(params object[] args)
    {
        Write(args);
        Console.Error.WriteLine();
    }
}

public class Text
{
    internal readonly ConsoleColor? fColor, bColor;
    internal readonly string value;

    public Text(object text, ConsoleColor? foreground, ConsoleColor? background)
    {
        value = text is string str ? str : text.ToString()!;
        fColor = foreground;
        bColor = background;
    }
}

public class TextRed : Text
{
    public TextRed(object text)
        : base(text, ConsoleColor.Red, null) { }
}

public class TextGreen : Text
{
    public TextGreen(object text)
        : base(text, ConsoleColor.Green, null) { }
}
