using lcms2.state;
using lcms2.testing;

var exhaustive = false;
var doSpeedTests = true;
var doCheckTests = true;
var doPluginTests = true;
var doZooTests = false;

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
WriteLineGreen("done");

Check("quick floor", new HelpersTests().CheckQuickFloor);
Check("quick floor word", new HelpersTests().CheckQuickFloorWord);
Check("Fixed point 15.16 representation", new HelpersTests().CheckFixedPoint15_16);
Check("Fixed point 8.8 representation", new HelpersTests().CheckFixedPoint8_8);

if (doPluginTests)
{
    Check("Simple context functionality", new StateTests().CheckSimpleContext);
    Check("Alarm codes context", new StateTests().CheckAlarmCodes);
    Check("Adaptation state context", new StateTests().CheckAdatationStateContext);
}