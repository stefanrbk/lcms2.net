using lcms2.state;
using lcms2.testbed;

using interp = lcms2.testbed.InterpolationTests;
using cs = lcms2.testbed.ColorspaceTests;

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
    Console.WriteLine("\nForward 1D interpolation");
    Check("1D interpolation in 2pt tables", interp.Check1DLerp2);
    Check("1D interpolation in 3pt tables", interp.Check1DLerp3);
    Check("1D interpolation in 4pt tables", interp.Check1DLerp4);
    Check("1D interpolation in 6pt tables", interp.Check1DLerp6);
    Check("1D interpolation in 18pt tables", interp.Check1DLerp18);
    Check("1D interpolation in descending 2pt tables", interp.Check1DLerp2Down);
    Check("1D interpolation in descending 3pt tables", interp.Check1DLerp3Down);
    Check("1D interpolation in descending 4pt tables", interp.Check1DLerp4Down);
    Check("1D interpolation in descending 6pt tables", interp.Check1DLerp6Down);
    Check("1D interpolation in descending 18pt tables", interp.Check1DLerp18Down);

    if (exhaustive)
    {
        Check("1D interpolation in n tables", interp.ExhaustiveCheck1DLerp);
        Check("1D interpolation in descending tables", interp.ExhaustiveCheck1DLerpDown);
    }

    Console.WriteLine("\nForward 3D interpolation");
    Check("3D interpolation Tetrahedral (float)", interp.Check3DInterpolationFloatTetrahedral);
    Check("3D interpolation Trilinear (float)", interp.Check3DInterpolationFloatTrilinear);
    Check("3D interpolation Tetrahedral (16)", interp.Check3DInterpolationTetrahedral16);
    Check("3D interpolation Trilinear (16)", interp.Check3DInterpolationTrilinear16);

    if (exhaustive)
    {
        Check("Exhaustive 3D interpolation Tetrahedral (float)", interp.ExhaustiveCheck3DInterpolationTetrahedralFloat);
        Check("Exhaustive 3D interpolation Trilinear (float)", interp.ExhaustiveCheck3DInterpolationTrilinearFloat);
        Check("Exhaustive 3D interpolation Tetrahedral (16)", interp.ExhaustiveCheck3DInterpolationTetrahedral16);
        Check("Exhaustive 3D interpolation Trilinear (16)", interp.ExhaustiveCheck3DInterpolationTrilinear16);
    }

    Check("Reverse interpolation 3 -> 3", interp.CheckReverseInterpolation3x3);
    Check("Reverse interpolation 4 -> 3", interp.CheckReverseInterpolation4x3);

    Console.WriteLine("\nHigh dimensionality interpolation");
    Check("3D interpolation", () => interp.CheckXDInterp(3));
    Check("3D interpolation with granularity", () => interp.CheckXDInterpGranular(3));
    Check("4D interpolation", () => interp.CheckXDInterp(4));
    Check("4D interpolation with granularity", () => interp.CheckXDInterpGranular(4));
    Check("5D interpolation with granularity", () => interp.CheckXDInterpGranular(5));
    Check("6D interpolation with granularity", () => interp.CheckXDInterpGranular(6));
    Check("7D interpolation with granularity", () => interp.CheckXDInterpGranular(7));
    Check("8D interpolation with granularity", () => interp.CheckXDInterpGranular(8));

    Console.WriteLine("\nEncoding of colorspaces");

    Check("Lab to LCh and back (float only)", cs.CheckLab2LCh);
    Check("Lab to XYZ and back (float only)", cs.CheckLab2XYZ);
    Check("Lab to xyY and back (float only)", cs.CheckLab2xyY);
    Check("Lab V2 encoding", cs.CheckLabV2EncodingTest);
    Check("Lab V4 encoding", cs.CheckLabV4EncodingTest);
}

if (doPluginTests)
{
    var state = new StateTests();

    Console.WriteLine("\nPlugin tests");
    Check("Simple context functionality", state.TestSimpleState);
    Check("Alarm codes context", state.TestAlarmCodes);
    Check("Adaptation state context", state.TestAdaptationStateState);
}

return totalFail;