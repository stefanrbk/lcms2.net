using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.it8;
using lcms2.state;
using lcms2.types;

using Newtonsoft.Json.Linq;

using NUnit.Framework.Internal;

namespace lcms2.testing;
public static class Utils
{
    public static int totalTests = 0;
    public static int totalFail = 0;
    public static int simultaneousErrors = 0;
    public static string subTest = String.Empty;
    public static string reasonToFail = String.Empty;
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

    public static void IsGoodDouble(string title, double actual, double expected, double delta) =>
        Assert.That(actual, Is.EqualTo(expected).Within(delta), $"({title}): Must be {actual}, But is {expected}");

    [DoesNotReturn]
    public static void Die(string reason, params object[] args)
    {
        WriteRed(() => Console.Error.WriteLine(String.Format(reason, args)));
        Environment.Exit(-1);
    }

    public static void Check(string title, Func<bool> test)
    {
        Console.Write("Checking {0} ... ", title);

        simultaneousErrors = 0;
        totalTests++;

        if (!test())
        {
            WriteRed(() =>
            {
                Console.WriteLine("FAIL!");
                if (!String.IsNullOrEmpty(subTest))
                    Console.WriteLine("{0}: [{1}]\n\t{2}", title, subTest, reasonToFail);
                else
                    Console.WriteLine("{0}:\n\t{1}", title, reasonToFail);

                if (simultaneousErrors > 1)
                    Console.WriteLine("\tMore than one ({0}) errors were reported", simultaneousErrors);

                totalFail++;
            });
            reasonToFail = String.Empty;
        } else
        {
            WriteLineGreen("Ok.");
        }
    }

    public static bool CheckSimpleTest(Action fn)
    {
        try
        {
            fn();
            return true;
        } catch (Exception ex)
        {
            reasonToFail = ex.Message;
            return false;
        }
    }

    public static bool CheckInterp1D(Func<int, int, bool> fn, bool down)
    {
        var result = true;
        for (int i = 0, j = 1; i < 16; i++, j++)
            if (!CheckSimpleTest(() => fn(i is 0 ? 10 : 256 * i, 256 * j)))
                result = false;

        Console.Write("The result is ");
        return result;
    }

    public static void WriteLineGreen(string value) =>
        WriteGreen(() => Console.WriteLine(value));

    public static void WriteGreen(Action fn)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        fn();
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void WriteRed(Action fn)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        fn();
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void ClearAssert() =>
        TestExecutionContext.CurrentContext.CurrentResult.AssertionResults.Clear();

    public static void ProgressBar(int start, int stop, int width, int j)
    {
        if ((j % 8) == 0)
        {
            var percent = (double)(j - start) / (stop - start);
            var filled = (int)Math.Round((double)percent * width);
            var empty = width - filled;
            Console.Write($"{new string(' ', 8)}{start} {new string('█', filled)}{new string('░', empty)} {stop}\r");
        }
    }
}
