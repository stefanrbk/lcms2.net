using lcms2.state;
using lcms2.testbed;

Console.WriteLine("LittleCMS.net {0} test bed {1} {2}", Lcms2.Version / 1000.0, DateTime.Now.Day, DateTime.Now.TimeOfDay);
Console.WriteLine();

var cliResult = CommandLine.Parser.Default.ParseArguments<CliOptions>(args);

var exhaustive = cliResult.Value.DoExhaustive;
var doSpeedTests = cliResult.Value.DoSpeed;
var doCheckTests = cliResult.Value.DoChecks;
var doPluginTests = cliResult.Value.DoPlugins;
var doZooTests = cliResult.Value.DoZoo;

if (args.Length is 0 && HasConsole)
    exhaustive = CheckExhaustive();

if (exhaustive)
{
    Console.WriteLine("Running exhaustive tests (will take a while...)");
    Console.WriteLine();
}

Console.WriteLine();
Console.Write("\tInstalling error logger ... ");
State.SetLogErrorHandler(FatalErrorQuit);
WriteLineGreen("done");

PrintSupportedIntents();

Check("quick floor", HelpersTests.CheckQuickFloor);
Check("quick floor word", HelpersTests.CheckQuickFloorWord);
Check("Fixed point 15.16 representation", HelpersTests.CheckFixedPoint15_16);
Check("Fixed point 8.8 representation", HelpersTests.CheckFixedPoint8_8);


if (doCheckTests)
{
    var interp = new InterpolationTests();
    interp.Setup();

    Console.WriteLine("\nForward 1D interpolation");
    Check("1D interpolation in 2pt tables", () => interp.Check1DTest(2, false, 0));
    Check("1D interpolation in 3pt tables", () => interp.Check1DTest(3, false, 1));
    Check("1D interpolation in 4pt tables", () => interp.Check1DTest(4, false, 0));
    Check("1D interpolation in 6pt tables", () => interp.Check1DTest(6, false, 0));
    Check("1D interpolation in 18pt tables", () => interp.Check1DTest(18, false, 0));
    Check("1D interpolation in descending 2pt tables", () => interp.Check1DTest(2, true, 0));
    Check("1D interpolation in descending 3pt tables", () => interp.Check1DTest(3, true, 1));
    Check("1D interpolation in descending 6pt tables", () => interp.Check1DTest(6, true, 0));
    Check("1D interpolation in descending 18pt tables", () => interp.Check1DTest(18, true, 0));

    if (exhaustive)
    {
        Check("1D interpolation in n tables", () => CheckInterp1D(interp.ExhaustiveCheck1DTest));
        Check("1D interpolation in descending tables", () => CheckInterp1D(interp.ExhaustiveCheck1DDownTest));
    }

    Console.WriteLine("\nForward 3D interpolation");
    Check("3D interpolation Tetrahedral (float)", interp.Check3DInterpolationFloatTetrahedralTest);
    Check("3D interpolation Trilinear (float)", interp.Check3DInterpolationFloatTrilinearTest);
    Check("3D interpolation Tetrahedral (16)", interp.Check3DInterpolation16TetrahedralTest);
    Check("3D interpolation Trilinear (16)", interp.Check3DInterpolation16TrilinearTest);

    if (exhaustive)
    {
        Check("Exhaustive 3D interpolation Tetrahedral (float)", () => CheckInterp3D(interp.ExhaustiveCheck3DInterpolationFloatTetrahedralTest));
        Check("Exhaustive 3D interpolation Trilinear (float)", () => CheckInterp3D(interp.ExhaustiveCheck3DInterpolationFloatTrilinearTest));
        Check("Exhaustive 3D interpolation Tetrahedral (16)", () => CheckInterp3D(interp.ExhaustiveCheck3DInterpolation16TetrahedralTest));
        Check("Exhaustive 3D interpolation Trilinear (16)", () => CheckInterp3D(interp.ExhaustiveCheck3DInterpolation16TrilinearTest));
    }

    Check("Reverse interpolation 3 -> 3", interp.CheckReverseInterpolation3x3Test);
    Check("Reverse interpolation 4 -> 3", interp.CheckReverseInterpolation4x3Test);

    Console.WriteLine("\nHigh dimensionality interpolation");
    Check("3D interpolation", () => Assert.Multiple(() =>
    {
        foreach (var i in TestDataGenerator.CheckXD(3))
            interp.CheckXDInterpTest((uint)i[0], (ushort[])i[1]);
    }));
    Check("3D interpolation with granularity", () => Assert.Multiple(() =>
    {
        foreach (var i in TestDataGenerator.CheckXDGranular(3))
            interp.CheckXDInterpGranularTest((uint[])i[0], (uint)i[1], (ushort[])i[2]);
    }));
    Check("4D interpolation", () => Assert.Multiple(() =>
    {
        foreach (var i in TestDataGenerator.CheckXD(4))
            interp.CheckXDInterpTest((uint)i[0], (ushort[])i[1]);
    }));
    Check("4D interpolation with granularity", () => Assert.Multiple(() =>
    {
        foreach (var i in TestDataGenerator.CheckXDGranular(4))
            interp.CheckXDInterpGranularTest((uint[])i[0], (uint)i[1], (ushort[])i[2]);
    }));
    Check("5D interpolation with granularity", () => Assert.Multiple(() =>
    {
        foreach (var i in TestDataGenerator.CheckXDGranular(5))
            interp.CheckXDInterpGranularTest((uint[])i[0], (uint)i[1], (ushort[])i[2]);
    }));
    Check("6D interpolation with granularity", () => Assert.Multiple(() =>
    {
        foreach (var i in TestDataGenerator.CheckXDGranular(6))
            interp.CheckXDInterpGranularTest((uint[])i[0], (uint)i[1], (ushort[])i[2]);
    }));
    Check("7D interpolation with granularity", () => Assert.Multiple(() =>
    {
        foreach (var i in TestDataGenerator.CheckXDGranular(7))
            interp.CheckXDInterpGranularTest((uint[])i[0], (uint)i[1], (ushort[])i[2]);
    }));
    Check("8D interpolation with granularity", () => Assert.Multiple(() =>
    {
        foreach (var i in TestDataGenerator.CheckXDGranular(8))
            interp.CheckXDInterpGranularTest((uint[])i[0], (uint)i[1], (ushort[])i[2]);
    }));

    interp.Teardown();

    var cs = new ColorspaceTests();

    Console.WriteLine("\nEncoding of colorspaces");
    ((ITest)cs).Setup();

    Check("Lab to LCh and back (float only)", () =>
        Assert.Multiple(() =>
        {
            for (var b = -16; b <= 16; b++)
                cs.CheckLab2LCh(b * 8, (b + 1) * 8);
        }));
    Check("Lab to XYZ and back (float only)", () =>
        Assert.Multiple(() =>
        {
            for (var b = -16; b <= 16; b++)
                cs.CheckLab2XYZ(b * 8, (b + 1) * 8);
        }));
    Check("Lab to xyY and back (float only)", () =>
        Assert.Multiple(() =>
        {
            for (var b = -16; b <= 16; b++)
                cs.CheckLab2xyY(b * 8, (b + 1) * 8);
        }));
    Check("Lab V2 encoding", () =>
        Assert.Multiple(() =>
        {
            for (var i = 0; i < 64; i++)
                cs.CheckLabV2EncodingTest(i * 1024, (i + 1) * 1024);
        }));
    Check("Lab V4 encoding", () =>
        Assert.Multiple(() =>
        {
            for (var i = 0; i < 64; i++)
                cs.CheckLabV4EncodingTest(i * 1024, (i + 1) * 1024);
        }));

    ((ITest)cs).Teardown();
}

if (doPluginTests)
{
    var state = new StateTests();

    Console.WriteLine("\nPlugin tests");
    Check("Simple context functionality", state.TestSimpleState);
    Check("Alarm codes context", state.TestAlarmCodes);
    Check("Adaptation state context", state.TestAdaptationStateState);
}