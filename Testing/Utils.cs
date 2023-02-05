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
using lcms2.it8;
using lcms2.state;
using lcms2.types;

using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace lcms2.testing;

public static class Utils
{
    #region Fields

    public const double FixedPrecision15_16 = 1.0 / 65535;
    public const double FixedPrecision8_8 = 1.0 / 255;
    public const double FloatPrecision = 0.00001;

    public static bool HasConsole = !Console.IsInputRedirected;
    public static double MaxErr = 0;
    public static string reasonToFail = String.Empty;
    public static int simultaneousErrors = 0;
    public static string subTest = String.Empty;
    public static int totalFail = 0;
    public static int totalTests = 0;
    public static bool TrappedError;
    private static object maxErrLock = new();

    #endregion Fields

    #region Public Methods

    public static void Check(string title, Action test)
    {
        if (HasConsole)
            Console.Write("\tChecking {0} ... ", title);

        simultaneousErrors = 0;
        totalTests++;

        try
        {
            test();
            WriteLineGreen("Ok.");
        }
        catch (Exception ex)
        {
            reasonToFail = ex.Message;
            WriteRed(() =>
            {
                Console.Error.WriteLine("FAIL!");
                if (!String.IsNullOrEmpty(subTest))
                    Console.Error.WriteLine("{0}: [{1}]\n\t\t{2}", title, subTest, reasonToFail);
                else
                    Console.Error.WriteLine("{0}:\n\t\t{1}", title, reasonToFail);

                if (simultaneousErrors > 1)
                    Console.Error.WriteLine("\t\tMore than one ({0}) errors were reported", simultaneousErrors);

                totalFail++;
            });
            reasonToFail = String.Empty;
            subTest = String.Empty;
        }
    }

    public static void Check(string title, Func<bool> test)
    {
        if (HasConsole)
            Console.Write("\tChecking {0} ...", title);

        simultaneousErrors = 0;
        reasonToFail = String.Empty;
        subTest = String.Empty;
        totalTests++;

        if (test() && !TrappedError)
        {
            WriteLineGreen("Ok.");
        }
        else
        {
            WriteRed(() =>
            {
                Console.Error.WriteLine("FAIL!");
                if (!String.IsNullOrEmpty(subTest))
                    Console.Error.WriteLine("{0}: [{1}]\n\t\t{2}", title, subTest, reasonToFail);
                else
                    Console.Error.WriteLine("{0}:\n\t\t{1}", title, reasonToFail);

                if (simultaneousErrors > 1)
                    Console.Error.WriteLine("\t\tMore than one ({0}) errors were reported", simultaneousErrors);

                totalFail++;
            });
        }
    }

    public static bool CheckExhaustive()
    {
        Console.Write("Run exhaustive tests? (y/N) (N in 5 sec) ");
        var key = WaitForKey(5000);
        if (key.HasValue)
        {
            if (key.Value.Key is ConsoleKey.Enter or ConsoleKey.N)
            {
                if (key.Value.Key is ConsoleKey.Enter)
                    Console.WriteLine("N");
                else
                    Console.WriteLine(key.Value.KeyChar);
                return false;
            }
            else if (key.Value.Key is ConsoleKey.Y)
            {
                Console.WriteLine(key.Value.KeyChar);
                return true;
            }
        }
        else
        {
            Console.WriteLine();
        }

        return false;
    }

    public static bool CheckGammaEstimation(ToneCurve c, double g)
    {
        var est = c.EstimateGamma(1E-3);

        SubTest("Gamma estimation");
        return Math.Abs(est - g) <= 1E-3;
    }

    public static bool CheckInterp1D(Action<uint, uint> fn)
    {
        if (HasConsole)
            Console.Write("10 - 4096");
        var result = true;
        for (uint i = 0, j = 1; i < 16; i++, j++)
        {
            try
            {
                fn(i is 0 ? 10 : 256 * i, 256 * j);
            }
            catch
            {
                result = false;
            }
        }

        if (HasConsole)
            Console.Write("\nThe result is ");
        return result;
    }

    public static bool CheckInterp3D(Action<uint, uint> fn)
    {
        if (HasConsole)
            Console.Write("0 - 256");
        var result = true;
        for (uint i = 0, j = 1; i < 16; i++, j++)
        {
            try
            {
                fn(16 * i, 16 * j);
            }
            catch
            {
                result = false;
            }
        }

        if (HasConsole)
            Console.Write("\nThe result is ");
        return result;
    }

    public static double DeltaE(Lab lab1, Lab lab2)
    {
        var dL = Math.Abs(lab1.L - lab2.L);
        var da = Math.Abs(lab1.a - lab2.a);
        var db = Math.Abs(lab1.b - lab2.b);

        return Math.Pow(Sqr(dL) + Sqr(da) + Sqr(db), 0.5);
    }

    [DoesNotReturn]
    public static void Die(string reason, params object[] args)
    {
        WriteRed(() => Console.Error.WriteLine(String.Format($"\n{reason}", args)));
        Environment.Exit(-1);
    }

    [DoesNotReturn]
    public static void Die(string reason)
    {
        WriteRed(() => Console.Error.WriteLine($"\n{reason}"));
        Environment.Exit(1);
    }

    public static void Dot() =>
        Console.Write(".");

    public static void DumpToneCurve(ToneCurve gamma, string filename)
    {
        var it8 = new IT8();

        it8.SetPropertyDouble("NUMBER_OF_FIELDS", 2);
        it8.SetPropertyDouble("NUMBER_OF_SETS", gamma.NumEntries);

        it8.SetDataFormat(0, "SAMPLE_ID");
        it8.SetDataFormat(1, "VALUE");

        for (var i = 0; i < gamma.NumEntries; i++)
        {
            it8.SetDataRowCol(i, 0, i);
            it8.SetDataRowCol(i, 1, gamma.EstimatedTable[i]);
        }

        it8.SaveToFile(filename);
    }

    public static bool Fail(string text)
    {
        lock (reasonToFail)
            reasonToFail = text;

        return false;
    }

    public static void FatalErrorQuit(object? _1, ErrorCode _2, string text) =>
                                        Die(text);

    public static bool IsGoodDouble(string title, double actual, double expected, double delta)
    {
        var err = Math.Abs(actual - expected);

        if (err > delta)
            return Fail($"({title}): Must be {expected}, but was {actual}");

        return true;
    }

    public static bool IsGoodFixed15_16(string title, double @in, double @out) =>
        IsGoodVal(title, @in, @out, FixedPrecision15_16);

    public static bool IsGoodFixed8_8(string title, double @in, double @out) =>
        IsGoodVal(title, @in, @out, FixedPrecision8_8);

    public static bool IsGoodVal(string title, double @in, double @out, double max)
    {
        var err = Math.Abs(@in - @out);

        lock (maxErrLock)
            if (err > MaxErr) MaxErr = err;

        if (err > max)
        {
            Fail($"({title}): Must be {@in}, but was {@out} ");
            return false;
        }
        return true;
    }

    public static bool IsGoodWord(string title, ushort @in, ushort @out) =>
        IsGoodWord(title, @in, @out, 0);

    public static bool IsGoodWord(string title, ushort @in, ushort @out, ushort maxErr)
    {
        if (Math.Abs(@in - @out) > maxErr)
        {
            Fail($"({title}): Must be {@in}, but was {@out} ");
            return false;
        }

        return true;
    }

    public static unsafe void PrintSupportedIntents()
    {
        var codes = stackalloc uint[200];
        var descriptions = new string?[200];
        var intents = cmsGetSupportedIntents(200, codes, descriptions);

        Console.WriteLine("Supported intents:");
        for (var i = 0; i < 200; i++)
        {
            if (codes[i] is not 0)
            {
                Console.WriteLine($"\t{codes[i]} - {descriptions[i]}");
            }
        }

        Console.WriteLine();
    }

    public static void ProgressBar(uint start, uint stop, int widthSubtract, uint i)
    {
        if (!HasConsole) return;

        var width = Console.BufferWidth - widthSubtract;
        var percent = (double)(i - start) / (stop - start);
        var filled = (int)Math.Round((double)percent * width);
        var empty = width - filled;
        Console.Write($"{new string(' ', 8)}{start} {new string('█', filled)}{new string('▓', empty)} {stop}\r");
    }

    public static void SubTest(string frm)
    {
        Dot();
        subTest = frm;
    }

    public static ConsoleKeyInfo? WaitForKey(int ms)
    {
        var cancel = new CancellationTokenSource(ms);
        var token = cancel.Token;
        var task = Task.Run(() => Console.ReadKey(true), token);
        try
        {
            task.Wait(token);
            var read = task.IsCompletedSuccessfully;
            if (read) return task.Result;
        }
        catch
        {
        }
        return null;
    }

    public static void WriteGreen(Action fn)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        fn();
        Console.ResetColor();
    }

    public static void WriteLineGreen(string value) =>
            WriteGreen(() => Console.WriteLine(value));

    public static void WriteLineRed(string value) =>
        WriteRed(() => Console.WriteLine(value));

    public static void WriteRed(Action fn)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        fn();
        Console.ResetColor();
    }

    #endregion Public Methods
}
