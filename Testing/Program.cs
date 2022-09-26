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
using lcms2.state;
using lcms2.testbed;

using cs = lcms2.testbed.ColorspaceTests;
using helper = lcms2.testbed.HelpersTests;
using interp = lcms2.testbed.InterpolationTests;
using state = lcms2.testbed.StateTests;
using tc = lcms2.testbed.ToneCurveTests;
using wp = lcms2.testbed.WhitePointTests;

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
WriteLineGreen("done\n");

PrintSupportedIntents();

Console.WriteLine("\nBasic operations");
Check("quick floor", helper.CheckQuickFloor);
Check("quick floor word", helper.CheckQuickFloorWord);
Check("Fixed point 15.16 representation", helper.CheckFixedPoint15_16);
Check("Fixed point 8.8 representation", helper.CheckFixedPoint8_8);
Check("D50 roundtrip", helper.CheckD50Roundtrip);

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

    Console.WriteLine("\nBlackBody");
    Check("Blackbody radiator", wp.CheckTemp2Chroma);

    Console.WriteLine("\nTone curves");
    Check("Linear gamma curves (16 bits)", tc.CheckGammaCreation16);
    Check("Linear gamma curves (float)", tc.CheckGammaCreationFloat);

    Check("Curve 1.8 (float)", tc.CheckGamma18);
    Check("Curve 2.2 (float)", tc.CheckGamma22);
    Check("Curve 3.0 (float)", tc.CheckGamma30);

    Check("Curve 1.8 (table)", tc.CheckGamma18Table);
    Check("Curve 2.2 (table)", tc.CheckGamma22Table);
    Check("Curve 3.0 (table)", tc.CheckGamma30Table);
}

if (doPluginTests)
{
    Console.WriteLine("\nPlugin tests");
    Check("Simple context functionality", state.TestSimpleState);
    Check("Alarm codes context", state.TestAlarmCodes);
    Check("Adaptation state context", state.TestAdaptationStateState);
}

return totalFail;
