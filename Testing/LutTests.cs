﻿//---------------------------------------------------------------------------------
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
using lcms2.types;

namespace lcms2.testbed;

public static class LutTests
{
    #region Public Methods

    public static bool Check1StageLut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        return CheckFullLut(ref lut, 1);
    }

    public static bool Check2Stage16Lut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        AddIdentityClut16(ref lut);

        return CheckFullLut(ref lut, 2);
    }

    public static bool Check2StageLut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        AddIdentityClutFloat(ref lut);

        return CheckFullLut(ref lut, 2);
    }

    public static bool Check3Stage16Lut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        AddIdentityClut16(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        return CheckFullLut(ref lut, 3);
    }

    public static bool Check3StageLut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        AddIdentityClutFloat(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        return CheckFullLut(ref lut, 3);
    }

    public static bool Check4Stage16Lut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        AddIdentityClut16(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);

        return CheckFullLut(ref lut, 4);
    }

    public static bool Check4StageLut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        AddIdentityClutFloat(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);

        return CheckFullLut(ref lut, 4);
    }

    public static bool Check5Stage16Lut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        AddIdentityClut16(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        return CheckFullLut(ref lut, 5);
    }

    public static bool Check5StageLut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        AddIdentityClutFloat(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        return CheckFullLut(ref lut, 5);
    }

    public static bool Check6Stage16Lut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityClut16(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        return CheckFullLut(ref lut, 6);
    }

    public static bool Check6StageLut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityClutFloat(ref lut);
        Add3GammaCurves(ref lut, 1.0);
        AddIdentityMatrix(ref lut);
        Add3GammaCurves(ref lut, 1.0);

        return CheckFullLut(ref lut, 6);
    }

    public static bool CheckLab2LabLut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        lut.InsertStage(StageLoc.AtEnd, Stage.AllocLab2XYZ(null));
        lut.InsertStage(StageLoc.AtEnd, Stage.AllocXyz2Lab(null));

        return CheckFloatLut(ref lut) && CheckStagesLut(ref lut, 2);
    }

    public static bool CheckLab2LabMatLut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        lut.InsertStage(StageLoc.AtEnd, Stage.AllocLab2XYZ(null));
        AddIdentityMatrix(ref lut);
        lut.InsertStage(StageLoc.AtEnd, Stage.AllocXyz2Lab(null));

        return CheckFloatLut(ref lut) && CheckStagesLut(ref lut, 3);
    }

    public static bool CheckLutCreation()
    {
        var lut = Pipeline.Alloc(null, 1, 1);
        if (lut is null) return false;

        var n1 = lut.StageCount;

        var lut2 = (Pipeline?)lut.Clone();
        if (lut2 is null) return false;

        var n2 = lut2.StageCount;

        return (n1 is 0) && (n2 is 0);
    }

    public static bool CheckXyz2XyzLut()
    {
        var lut = Pipeline.Alloc(null, 3, 3);
        if (lut is null) return false;

        lut.InsertStage(StageLoc.AtEnd, Stage.AllocXyz2Lab(null));
        lut.InsertStage(StageLoc.AtEnd, Stage.AllocLab2XYZ(null));

        return CheckFloatLut(ref lut) && CheckStagesLut(ref lut, 2);
    }

    #endregion Public Methods

    #region Private Methods

    private static void Add3GammaCurves(ref Pipeline lut, double curve)
    {
        using var id = ToneCurve.BuildGamma(null, curve);
        if (id is null)
        {
            lut = null!;
            return;
        }

        var id3 = new[] { id, id, id };

        lut.InsertStage(
            StageLoc.AtEnd,
            Stage.AllocToneCurves(null, 3, id3));
    }

    private static void AddIdentityClut16(ref Pipeline lut) =>
            lut.InsertStage(
            StageLoc.AtEnd,
            Stage.AllocCLut16bit(
                null,
                2,
                3,
                3,
                new ushort[]
                {
                    0,      0,      0,
                    0,      0,      0xFFFF,

                    0,      0xFFFF, 0,
                    0,      0xFFFF, 0xFFFF,

                    0xFFFF, 0,      0,
                    0xFFFF, 0,      0xFFFF,

                    0xFFFF, 0xFFFF, 0,
                    0xFFFF, 0xFFFF, 0xFFFF
                }));

    private static void AddIdentityClutFloat(ref Pipeline lut) =>
        lut.InsertStage(
            StageLoc.AtEnd,
            Stage.AllocCLutFloat(
                null,
                2,
                3,
                3,
                new float[]
                {
                    0, 0, 0,
                    0, 0, 1,

                    0, 1, 0,
                    0, 1, 1,

                    1, 0, 0,
                    1, 0, 1,

                    1, 1, 0,
                    1, 1, 1
                }));

    private static void AddIdentityMatrix(ref Pipeline lut) =>
                lut.InsertStage(
            StageLoc.AtEnd,
            Stage.AllocMatrix(
                null,
                3,
                3,
                new double[]
                {
                    1, 0, 0,
                    0, 1, 0,
                    0, 0, 1,
                    0, 0, 0
                },
                default));

    private static bool Check16Lut(ref Pipeline lut)
    {
        var n2 = 0;

        for (var j = 0; j < 65535; j++)
        {
            var inw = Enumerable.Repeat((ushort)j, 3).ToArray();
            var outw = new ushort[3];
            lut.Eval(inw, outw);

            n2 += outw.Count(i => i != j);
        }

        return n2 is 0;
    }

    private static bool CheckFloatLut(ref Pipeline lut)
    {
        var n1 = 0;

        for (var j = 0; j < 65535; j++)
        {
            var inf = Enumerable.Repeat(j / 65535f, 3).ToArray();
            var outf = new float[3];
            lut.Eval(inf, outf);

            var af = outf.Select(f => (int)Math.Floor((f * 65535.0) + 0.5));

            n1 += af.Count(i => i != j);
        }

        return n1 is 0;
    }

    private static bool CheckFullLut(ref Pipeline lut, int expectedStages) =>
        CheckStagesLut(ref lut, expectedStages) &&
        Check16Lut(ref lut) &&
        CheckFloatLut(ref lut);

    private static bool CheckStagesLut(ref Pipeline lut, int expectedStages) =>
            (lut.InputChannels is 3) &&
        (lut.OutputChannels is 3) &&
        (lut.StageCount == expectedStages);

    #endregion Private Methods
}
