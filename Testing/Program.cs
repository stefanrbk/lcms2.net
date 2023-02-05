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
using lcms2.testbed;

using Cs = lcms2.testbed.ColorspaceTests;
using Frm = lcms2.testbed.FormatterTests;
using Helper = lcms2.testbed.HelpersTests;
using Interp = lcms2.testbed.InterpolationTests;
using Lut = lcms2.testbed.LutTests;
using State = lcms2.testbed.StateTests;
using Tc = lcms2.testbed.ToneCurveTests;
using Wp = lcms2.testbed.WhitePointTests;

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
cmsSetLogErrorHandler(FatalErrorQuit);
WriteLineGreen("done\n");

Console.Write("\tInstalling old error logger ... ");
lcms2.state.State.SetLogErrorHandler(FatalErrorQuit);
WriteLineGreen("done\n");

PrintSupportedIntents();

Console.WriteLine("\nBasic operations");
Check("Sanity check", Helper.Sanity);
Check("quick floor", Helper.CheckQuickFloor);
Check("quick floor word", Helper.CheckQuickFloorWord);
Check("Fixed point 15.16 representation", Helper.CheckFixedPoint15_16);
Check("Fixed point 8.8 representation", Helper.CheckFixedPoint8_8);
Check("D50 roundtrip", Helper.CheckD50Roundtrip);

if (doCheckTests)
{
    Console.WriteLine("\nForward 1D interpolation");
    Check("1D interpolation in 2pt tables", Interp.Check1DLerp2);
    Check("1D interpolation in 3pt tables", Interp.Check1DLerp3);
    Check("1D interpolation in 4pt tables", Interp.Check1DLerp4);
    Check("1D interpolation in 6pt tables", Interp.Check1DLerp6);
    Check("1D interpolation in 18pt tables", Interp.Check1DLerp18);
    Check("1D interpolation in descending 2pt tables", Interp.Check1DLerp2Down);
    Check("1D interpolation in descending 3pt tables", Interp.Check1DLerp3Down);
    Check("1D interpolation in descending 4pt tables", Interp.Check1DLerp4Down);
    Check("1D interpolation in descending 6pt tables", Interp.Check1DLerp6Down);
    Check("1D interpolation in descending 18pt tables", Interp.Check1DLerp18Down);

    if (exhaustive)
    {
        Check("1D interpolation in n tables", Interp.ExhaustiveCheck1DLerp);
        Check("1D interpolation in descending tables", Interp.ExhaustiveCheck1DLerpDown);
    }

    Console.WriteLine("\nForward 3D interpolation");
    Check("3D interpolation Tetrahedral (float)", Interp.Check3DInterpolationFloatTetrahedral);
    Check("3D interpolation Trilinear (float)", Interp.Check3DInterpolationFloatTrilinear);
    Check("3D interpolation Tetrahedral (16)", Interp.Check3DInterpolationTetrahedral16);
    Check("3D interpolation Trilinear (16)", Interp.Check3DInterpolationTrilinear16);

    if (exhaustive)
    {
        Check("Exhaustive 3D interpolation Tetrahedral (float)", Interp.ExhaustiveCheck3DInterpolationTetrahedralFloat);
        Check("Exhaustive 3D interpolation Trilinear (float)", Interp.ExhaustiveCheck3DInterpolationTrilinearFloat);
        Check("Exhaustive 3D interpolation Tetrahedral (16)", Interp.ExhaustiveCheck3DInterpolationTetrahedral16);
        Check("Exhaustive 3D interpolation Trilinear (16)", Interp.ExhaustiveCheck3DInterpolationTrilinear16);
    }

    Check("Reverse interpolation 3 -> 3", Interp.CheckReverseInterpolation3x3);
    Check("Reverse interpolation 4 -> 3", Interp.CheckReverseInterpolation4x3);

    Console.WriteLine("\nHigh dimensionality interpolation");
    Check("3D interpolation", () => Interp.CheckXDInterp(3));
    Check("3D interpolation with granularity", () => Interp.CheckXDInterpGranular(3));
    Check("4D interpolation", () => Interp.CheckXDInterp(4));
    Check("4D interpolation with granularity", () => Interp.CheckXDInterpGranular(4));
    Check("5D interpolation with granularity", () => Interp.CheckXDInterpGranular(5));
    Check("6D interpolation with granularity", () => Interp.CheckXDInterpGranular(6));
    Check("7D interpolation with granularity", () => Interp.CheckXDInterpGranular(7));
    Check("8D interpolation with granularity", () => Interp.CheckXDInterpGranular(8));

    Console.WriteLine("\nEncoding of colorspaces");

    Check("Lab to LCh and back (float only)", Cs.CheckLab2LCh);
    Check("Lab to XYZ and back (float only)", Cs.CheckLab2XYZ);
    Check("Lab to xyY and back (float only)", Cs.CheckLab2xyY);
    Check("Lab V2 encoding", Cs.CheckLabV2EncodingTest);
    Check("Lab V4 encoding", Cs.CheckLabV4EncodingTest);

    Console.WriteLine("\nBlackBody");
    Check("Blackbody radiator", Wp.CheckTemp2Chroma);

    Console.WriteLine("\nTone curves");
    Check("Linear gamma curves (16 bits)", Tc.CheckGammaCreation16);
    Check("Linear gamma curves (float)", Tc.CheckGammaCreationFloat);

    Check("Curve 1.8 (float)", Tc.CheckGamma18);
    Check("Curve 2.2 (float)", Tc.CheckGamma22);
    Check("Curve 3.0 (float)", Tc.CheckGamma30);

    Check("Curve 1.8 (table)", Tc.CheckGamma18Table);
    Check("Curve 2.2 (table)", Tc.CheckGamma22Table);
    Check("Curve 3.0 (table)", Tc.CheckGamma30Table);

    Check("Curve 1.8 (word table)", Tc.CheckGamma18TableWord);
    Check("Curve 2.2 (word table)", Tc.CheckGamma22TableWord);
    Check("Curve 3.0 (word table)", Tc.CheckGamma30TableWord);

    Check("Parametric curves", Tc.CheckParametricToneCurves);

    Check("Join curves", Tc.CheckJointCurves);
    Check("Join curves descending", Tc.CheckJointCurvesDescending);
    Check("Join curves degenerated", Tc.CheckReverseDegenerated);
    Check("Join curves sRGB (Float)", Tc.CheckJointFloatCurvesSrgb);
    Check("Join curves sRGB (16 bits)", Tc.CheckJoint16CurvesSrgb);
    Check("Join curves sigmoidal", Tc.CheckJointCurvesSShaped);

    Console.WriteLine("\nLUT basics");
    Check("LUT creation & dup", Lut.CheckLutCreation);
    Check("1 Stage LUT ", Lut.Check1StageLut);
    Check("2 Stage LUT ", Lut.Check2StageLut);
    Check("2 Stage LUT (16 bits)", Lut.Check2Stage16Lut);
    Check("3 Stage LUT ", Lut.Check3StageLut);
    Check("3 Stage LUT (16 bits)", Lut.Check3Stage16Lut);
    Check("4 Stage LUT ", Lut.Check4StageLut);
    Check("4 Stage LUT (16 bits)", Lut.Check4Stage16Lut);
    Check("5 Stage LUT ", Lut.Check5StageLut);
    Check("5 Stage LUT (16 bits)", Lut.Check5Stage16Lut);
    Check("6 Stage LUT ", Lut.Check6StageLut);
    Check("6 Stage LUT (16 bits)", Lut.Check6Stage16Lut);

    Console.WriteLine("\nLUT operation");
    Check("Lab to Lab LUT (float only)", Lut.CheckLab2LabLut);
    Check("XYZ to XYZ LUT (float only)", Lut.CheckXyz2XyzLut);
    Check("Lab to Lab MAT LUT (float only)", Lut.CheckLab2LabMatLut);
    Check("Named Color LUT", Lut.CheckNamedColorLut);

    Console.WriteLine("\nFormatter basic operation");
    Check("Usual formatters", Frm.CheckFormatters16);
    Check("Floating point formatters", Frm.CheckFormattersFloat);
    Check("Half formatters", Frm.CheckFormattersHalf);

    Console.WriteLine("\nChange buffers format");
}

if (doPluginTests)
{
    Console.WriteLine("\nPlugin tests");
    Check("Simple context functionality", State.TestSimpleState);
    Check("Alarm codes context", State.TestAlarmCodes);
    Check("Adaptation state context", State.TestAdaptationStateState);
}

return totalFail;
