using lcms2.state;
using lcms2.testing;

var exhaustive = false;
var doSpeedTests = true;
var doCheckTests = true;
var doPluginTests = true;
var doZooTests = false;

Console.WriteLine("LittleCMS.net {0} test bed {1} {2}", Lcms2.Version / 1000.0, DateTime.Now.Day, DateTime.Now.TimeOfDay);
Console.WriteLine();

if (args.Length is 0)
{
    Console.Write("Run exhaustive tests? (y/N) (N in 5 sec) ");
    if (!Console.IsInputRedirected)
    {
        var key = WaitForKey(5000);
        if (key.HasValue)
        {
            if (key.Value.Key is ConsoleKey.Enter or ConsoleKey.N)
            {
                exhaustive = false;
                if (key.Value.Key is ConsoleKey.Enter)
                    Console.WriteLine("N");
                else
                    Console.WriteLine(key.Value.KeyChar);
            } else if (key.Value.Key is ConsoleKey.Y)
            {
                exhaustive = true;
                Console.WriteLine(key.Value.KeyChar);
                Console.WriteLine("Running exhaustive tests (will take a while...)");
            }
        } else
            Console.WriteLine();
    }
}
if (args.Contains("--exhaustive", StringComparer.OrdinalIgnoreCase))
{
    exhaustive = true;
    Console.WriteLine("Running exhaustive tests (will take a while...)");
    Console.WriteLine();
}

Console.WriteLine();
Console.Write("\tInstalling error logger ... ");
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

    Console.WriteLine("\nForward 1D interpolation");
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
        Check("1D interpolation in n tables", () => CheckInterp1D(interp.ExhaustiveCheck1DTest));
        Check("1D interpolation in descending tables", () => CheckInterp1D(interp.ExhaustiveCheck1DDownTest));
    }

    Console.WriteLine("\nForward 3D interpolation");
    Check("3D interpolation Tetrahedral (float)", () => CheckSimpleTest(interp.Check3DInterpolationFloatTetrahedralTest));
    Check("3D interpolation Trilinear (float)", () => CheckSimpleTest(interp.Check3DInterpolationFloatTrilinearTest));
    Check("3D interpolation Tetrahedral (16)", () => CheckSimpleTest(interp.Check3DInterpolation16TetrahedralTest));
    Check("3D interpolation Trilinear (16)", () => CheckSimpleTest(interp.Check3DInterpolation16TrilinearTest));

    if (exhaustive)
    {
        Check("Exhaustive 3D interpolation Tetrahedral (float)", () => CheckInterp3D(interp.ExhaustiveCheck3DInterpolationFloatTetrahedralTest));
        Check("Exhaustive 3D interpolation Trilinear (float)", () => CheckInterp3D(interp.ExhaustiveCheck3DInterpolationFloatTrilinearTest));
        Check("Exhaustive 3D interpolation Tetrahedral (16)", () => CheckInterp3D(interp.ExhaustiveCheck3DInterpolation16TetrahedralTest));
        Check("Exhaustive 3D interpolation Trilinear (16)", () => CheckInterp3D(interp.ExhaustiveCheck3DInterpolation16TrilinearTest));
    }

    Check("Reverse interpolation 3 -> 3", () => CheckSimpleTest(interp.CheckReverseInterpolation3x3Test));
    Check("Reverse interpolation 4 -> 3", () => CheckSimpleTest(interp.CheckReverseInterpolation4x3Test));

    Console.WriteLine("\nHigh dimensionality interpolation");
    Check("3D interpolation", () => CheckSimpleTest(interp.Check3DInterpTest));
    Check("3D interpolation with granularity", () => CheckSimpleTest(interp.Check3DInterpGranularTest));
    Check("4D interpolation", () => CheckSimpleTest(interp.Check4DInterpTest));
    Check("4D interpolation with granularity", () => CheckSimpleTest(interp.Check4DInterpGranularTest));
    Check("5D interpolation with granularity", () => CheckSimpleTest(interp.Check5DInterpGranularTest));
    Check("6D interpolation with granularity", () => CheckSimpleTest(interp.Check6DInterpGranularTest));
    Check("7D interpolation with granularity", () => CheckSimpleTest(interp.Check7DInterpGranularTest));
    Check("8D interpolation with granularity", () => CheckSimpleTest(interp.Check8DInterpGranularTest));

    interp.Teardown();

    var cs = new ColorspaceTests();

    Console.WriteLine("\nEncoding of colorspaces");
    ((ITest)cs).Setup();

    Check("Lab to LCh and back (float only)", () => CheckSimpleTest(() =>
        Assert.Multiple(() =>
        {
            for (var b = -16; b <= 16; b++)
                cs.CheckLab2LCh(b * 8, (b + 1) * 8);
        })));
    Check("Lab to XYZ and back (float only)", () => CheckSimpleTest(() =>
        Assert.Multiple(() =>
        {
            for (var b = -16; b <= 16; b++)
                cs.CheckLab2XYZ(b * 8, (b + 1) * 8);
        })));
    Check("Lab to xyY and back (float only)", () => CheckSimpleTest(() =>
        Assert.Multiple(() =>
        {
            for (var b = -16; b <= 16; b++)
                cs.CheckLab2xyY(b * 8, (b + 1) * 8);
        })));
    Check("Lab V2 encoding", () => CheckSimpleTest(() =>
        Assert.Multiple(() =>
        {
            for (var i = 0; i < 64; i++)
                cs.CheckLabV2EncodingTest(i * 1024, (i + 1) * 1024);
        })));
    Check("Lab V4 encoding", () => CheckSimpleTest(() =>
        Assert.Multiple(() =>
        {
            for (var i = 0; i < 64; i++)
                cs.CheckLabV4EncodingTest(i * 1024, (i + 1) * 1024);
        })));

    ((ITest)cs).Teardown();
}

if (doPluginTests)
{
    var state = new StateTests();

    Console.WriteLine("\nPlugin tests");
    Check("Simple context functionality", () => CheckSimpleTest(state.TestSimpleState));
    Check("Alarm codes context", () => CheckSimpleTest(state.TestAlarmCodes));
    Check("Adaptation state context", () => CheckSimpleTest(state.TestAdaptationStateState));
}