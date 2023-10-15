//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2023 Marti Maria Saguer
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
using lcms2.types;

namespace lcms2.testbed;

internal static partial class Testbed
{
    public static bool CheckLUTcreation()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 1, 1);
        var n1 = cmsPipelineStageCount(lut);
        var lut2 = cmsPipelineDup(lut);
        var n2 = cmsPipelineStageCount(lut2);

        cmsPipelineFree(lut);
        cmsPipelineFree(lut2);

        return (n1 is 0) && (n2 is 0);
    }

    private static void AddIdentityMatrix(Pipeline? lut)
    {
        ReadOnlySpan<double> Identity = stackalloc double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };

        cmsPipelineInsertStage(lut, StageLoc.AtEnd, cmsStageAllocMatrix(DbgThread(), 3, 3, Identity, null));
    }

    private static void AddIdentityCLUTfloat(Pipeline? lut)
    {
        ReadOnlySpan<float> Table = stackalloc float[] { 0, 0, 0, 0, 0, 1.0f, 0, 1.0f, 0, 0, 1.0f, 1.0f, 1.0f, 0, 0, 1.0f, 0, 1.0f, 1.0f, 1.0f, 0, 1.0f, 1.0f, 1.0f };

        cmsPipelineInsertStage(lut, StageLoc.AtEnd, cmsStageAllocCLutFloat(DbgThread(), 2, 3, 3, Table));
    }

    private static void AddIdentityCLUT16(Pipeline? lut)
    {
        ReadOnlySpan<ushort> Table = stackalloc ushort[] { 0, 0, 0, 0, 0, 0xffff, 0, 0xffff, 0, 0, 0xffff, 0xffff, 0xffff, 0, 0, 0xffff, 0, 0xffff, 0xffff, 0xffff, 0, 0xffff, 0xffff, 0xffff };

        cmsPipelineInsertStage(lut, StageLoc.AtEnd, cmsStageAllocCLut16bit(DbgThread(), 2, 3, 3, Table));
    }

    private static void Add3GammaCurves(Pipeline? lut, double Curve)
    {
        var id = cmsBuildGamma(DbgThread(), Curve);
        var id3 = new ToneCurve[3] { id, id, id };

        cmsPipelineInsertStage(lut, StageLoc.AtEnd, cmsStageAllocToneCurves(DbgThread(), 3, id3));

        cmsFreeToneCurve(id);
    }

    private static bool CheckFloatLUT(Pipeline lut)
    {
        Span<float> Inf = stackalloc float[3];
        Span<float> Outf = stackalloc float[3];
        Span<int> af = stackalloc int[3];

        var n1 = 0;

        for (var j = 0; j < 65535; j++)
        {
            Inf[0] = Inf[1] = Inf[2] = j / 65535f;
            cmsPipelineEvalFloat(Inf, Outf, lut);

            af[0] = (int)Math.Floor((Outf[0] * 65535.0) + 0.5);
            af[1] = (int)Math.Floor((Outf[1] * 65535.0) + 0.5);
            af[2] = (int)Math.Floor((Outf[2] * 65535.0) + 0.5);

            for (var i = 0; i < 3; i++)
            {
                if (af[i] != j)
                    n1++;
            }
        }

        return n1 is 0;
    }

    private static bool Check16LUT(Pipeline lut)
    {
        Span<ushort> Inf = stackalloc ushort[3];
        Span<ushort> Outf = stackalloc ushort[3];
        Span<int> af = stackalloc int[3];

        var n2 = 0;

        for (var j = 0; j < 65535; j++)
        {
            Inf[0] = Inf[1] = Inf[2] = (ushort)j;
            cmsPipelineEval16(Inf, Outf, lut);

            af[0] = Outf[0];
            af[1] = Outf[1];
            af[2] = Outf[2];

            for (var i = 0; i < 3; i++)
            {
                if (af[i] != j)
                    n2++;
            }
        }

        return n2 is 0;
    }

    private static bool CheckStagesLUT(Pipeline lut, int ExpectedStages)
    {
        var nInpChans = cmsPipelineInputChannels(lut);
        var nOutpChans = cmsPipelineOutputChannels(lut);
        var nStages = cmsPipelineStageCount(lut);

        return (nInpChans is 3) && (nOutpChans is 3) && (nStages == ExpectedStages);
    }

    private static bool CheckFullLUT(Pipeline? lut, int ExpectedStages)
    {
        var rc = CheckStagesLUT(lut, ExpectedStages) && Check16LUT(lut) && CheckFloatLUT(lut);

        cmsPipelineFree(lut);
        return rc;
    }

    public static bool Check1StageLUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        return CheckFullLUT(lut, 1);
    }

    public static bool Check2StageLUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        AddIdentityCLUTfloat(lut);

        return CheckFullLUT(lut, 2);
    }

    public static bool Check2Stage16LUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        AddIdentityCLUT16(lut);

        return CheckFullLUT(lut, 2);
    }

    public static bool Check3StageLUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        AddIdentityCLUTfloat(lut);
        Add3GammaCurves(lut, 1.0);

        return CheckFullLUT(lut, 3);
    }

    public static bool Check3Stage16LUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        AddIdentityCLUT16(lut);
        Add3GammaCurves(lut, 1.0);

        return CheckFullLUT(lut, 3);
    }

    public static bool Check4StageLUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        AddIdentityCLUTfloat(lut);
        Add3GammaCurves(lut, 1.0);
        AddIdentityMatrix(lut);

        return CheckFullLUT(lut, 4);
    }

    public static bool Check4Stage16LUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        AddIdentityCLUT16(lut);
        Add3GammaCurves(lut, 1.0);
        AddIdentityMatrix(lut);

        return CheckFullLUT(lut, 4);
    }

    public static bool Check5StageLUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        AddIdentityCLUTfloat(lut);
        Add3GammaCurves(lut, 1.0);
        AddIdentityMatrix(lut);
        Add3GammaCurves(lut, 1.0);

        return CheckFullLUT(lut, 5);
    }

    public static bool Check5Stage16LUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        AddIdentityCLUT16(lut);
        Add3GammaCurves(lut, 1.0);
        AddIdentityMatrix(lut);
        Add3GammaCurves(lut, 1.0);

        return CheckFullLUT(lut, 5);
    }

    public static bool Check6StageLUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        Add3GammaCurves(lut, 1.0);
        AddIdentityCLUTfloat(lut);
        Add3GammaCurves(lut, 1.0);
        AddIdentityMatrix(lut);
        Add3GammaCurves(lut, 1.0);

        return CheckFullLUT(lut, 6);
    }

    public static bool Check6Stage16LUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        AddIdentityMatrix(lut);
        Add3GammaCurves(lut, 1.0);
        AddIdentityCLUT16(lut);
        Add3GammaCurves(lut, 1.0);
        AddIdentityMatrix(lut);
        Add3GammaCurves(lut, 1.0);

        return CheckFullLUT(lut, 6);
    }

    public static bool CheckLab2LabLUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        cmsPipelineInsertStage(lut, StageLoc.AtEnd, _cmsStageAllocLab2XYZ(DbgThread()));
        cmsPipelineInsertStage(lut, StageLoc.AtEnd, _cmsStageAllocXYZ2Lab(DbgThread()));

        var rc = CheckFloatLUT(lut) && CheckStagesLUT(lut, 2);

        cmsPipelineFree(lut);

        return rc;
    }

    public static bool CheckXYZ2XYZLUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        cmsPipelineInsertStage(lut, StageLoc.AtEnd, _cmsStageAllocXYZ2Lab(DbgThread()));
        cmsPipelineInsertStage(lut, StageLoc.AtEnd, _cmsStageAllocLab2XYZ(DbgThread()));

        var rc = CheckFloatLUT(lut) && CheckStagesLUT(lut, 2);

        cmsPipelineFree(lut);

        return rc;
    }

    public static bool CheckLab2LabMatLUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);

        cmsPipelineInsertStage(lut, StageLoc.AtEnd, _cmsStageAllocLab2XYZ(DbgThread()));
        AddIdentityMatrix(lut);
        cmsPipelineInsertStage(lut, StageLoc.AtEnd, _cmsStageAllocXYZ2Lab(DbgThread()));

        var rc = CheckFloatLUT(lut) && CheckStagesLUT(lut, 3);

        cmsPipelineFree(lut);

        return rc;
    }

    public static bool CheckNamedColorLUT()
    {
        var lut = cmsPipelineAlloc(DbgThread(), 3, 3);
        var rc = true;
        Span<ushort> PCS = stackalloc ushort[3];
        Span<ushort> Colorant = stackalloc ushort[cmsMAXCHANNELS];
        Span<byte> Name = stackalloc byte[255];
        Span<ushort> Inw = stackalloc ushort[3];
        Span<ushort> Outw = stackalloc ushort[3];
        var pre = "pre"u8;
        var post = "post"u8;

        var nc = cmsAllocNamedColorList(DbgThread(), 256, 3, pre, post);
        if (nc is null) return false;

        for (var i = 0; i < 256; i++)
        {
            PCS[0] = PCS[1] = PCS[2] = (ushort)i;
            Colorant[0] = Colorant[1] = Colorant[2] = Colorant[3] = (ushort)i;

            sprintf(Name, $"#{i}");
            if (!cmsAppendNamedColor(nc, Name, PCS, Colorant)) { rc = false; break; }
        }

        cmsPipelineInsertStage(lut, StageLoc.AtEnd, _cmsStageAllocNamedColor(nc, false));

        cmsFreeNamedColorList(nc);
        if (!rc) return false;

        var n2 = 0;

        for (var j = 0; j < 256; j++)
        {
            Inw[0] = (ushort)j;

            cmsPipelineEval16(Inw, Outw, lut);
            for (var i = 0; i < 3; i++)
            {
                if (Outw[i] != j) n2++;
            }
        }

        cmsPipelineFree(lut);
        return n2 is 0;
    }
}
