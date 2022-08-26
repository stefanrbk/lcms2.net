using lcms2.state;
using lcms2.testing;

using static lcms2.testing.Utils;

var exhaustive = false;
var doSpeedTests = true;
var doCheckTests = true;
var doPluginTests = true;
var doZooTests = false;
var totalTests = 0;
var totalFail = 0;
var simultaneousErrors = 0;
var subTest = String.Empty;
var reasonToFail = String.Empty;

void Check(string title, Func<bool> test)
{
    Console.Write("Checking {0} ... ", title);

    simultaneousErrors = 0;
    totalTests++;

    if (!test())
    {
        Console.WriteLine("FAIL!");
        if (!String.IsNullOrEmpty(subTest))
            Console.WriteLine("{0}: [{1}]\n\t{2}", title, subTest, reasonToFail);
        else
            Console.WriteLine("{0}:\n\t{1}", title, reasonToFail);

        if (simultaneousErrors > 1)
            Console.WriteLine("\tMore than one ({0}) errors were reported", simultaneousErrors);

        totalFail++;
    } else
        Console.WriteLine("Ok.");
}

Console.WriteLine("LittleCMS.net {0} test bed {1} {2}", Lcms2.Version / 1000.0, DateTime.Now.Day, DateTime.Now.TimeOfDay);
Console.WriteLine();

if (args.Contains("--exhaustive", StringComparer.OrdinalIgnoreCase))
{
    exhaustive = true;
    Console.WriteLine("Running exhaustive tests (will take a while...)");
    Console.WriteLine();
}

Console.Write("Installing error logger ... ");
State.SetLogErrorHandler((_, __, t) => Die(t));
Console.WriteLine("done");

Check("quick floor", new HelpersTests().CheckQuickFloor);
Check("quick floor word", new HelpersTests().CheckQuickFloorWord);
Check("Fixed point 15.16 representation", new HelpersTests().CheckFixedPoint15_16);
Check("Fixed point 8.8 representation", new HelpersTests().CheckFixedPoint8_8);
