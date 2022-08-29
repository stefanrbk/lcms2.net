using lcms2.state;
using lcms2.testing;

var exhaustive = true;
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

if (doCheckTests)
{
    var interp = new InterpolationTests();
    interp.Setup();

    Check("1D interpolation in 2pt tables", () => CheckSimpleTest(() => interp.Check1DTest(2, false, 0)));
    Check("1D interpolation in 3pt tables", () => CheckSimpleTest(() => interp.Check1DTest(3, false, 1)));
    Check("1D interpolation in 4pt tables", () => CheckSimpleTest(() => interp.Check1DTest(4, false, 0)));
    Check("1D interpolation in 6pt tables", () => CheckSimpleTest(() => interp.Check1DTest(6, false, 0)));
    Check("1D interpolation in 18pt tables", () => CheckSimpleTest(() => interp.Check1DTest(18, false, 0)));
    Check("1D interpolation in descending 2pt tables", () => CheckSimpleTest(() => interp.Check1DTest(2, true, 0)));
    Check("1D interpolation in descending 3pt tables", () => CheckSimpleTest(() => interp.Check1DTest(3, true, 1)));
    Check("1D interpolation in descending 6pt tables", () => CheckSimpleTest(() => interp.Check1DTest(6, true, 0)));
    Check("1D interpolation in descending 18pt tables", () => CheckSimpleTest(() => interp.Check1DTest(18, true, 0)));

    if (exhaustive)
    {
        Check("1D interpolation in n tables", () => CheckInterp1D(interp.ExhaustiveCheck1DTest, false));
        Check("1D interpolation in descending tables", () => CheckInterp1D(interp.ExhaustiveCheck1DTest, true));
    }

    interp.Teardown();
}

if (doPluginTests)
{
    var state = new StateTests();
    Check("Simple context functionality", () => CheckSimpleTest(state.TestSimpleState));
    Check("Alarm codes context", () => CheckSimpleTest(state.TestAlarmCodes));
    Check("Adaptation state context", () => CheckSimpleTest(state.TestAdaptationStateState));
}