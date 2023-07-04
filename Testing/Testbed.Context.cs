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
using lcms2.state;
using lcms2.types;

namespace lcms2.testbed;

internal static unsafe partial class Testbed
{
    private static Context DupContext(Context src, void* Data)
    {
        var cpy = cmsDupContext(src, Data);

        DebugMemDontCheckThis(cpy);

        return cpy;
    }

    private readonly static PluginInterpolation InterpPluginSample = new()
    {
        @base = new() { Magic = cmsPluginMagicNumber, ExpectedVersion = 2060, Type = cmsPluginInterpolationSig, Next = null },
        InterpolatorsFactory = my_Interpolators_Factory,
    };

    // This fake interpolation takes always the closest lower node in the interpolation table for 1D 
    private static void Fake1Dfloat(in float* Value,
                                    float* Output,
                                    in InterpParams* p)
    {
        float val2;
        int cell;
        float* LutTable = (float*) p ->Table;

           // Clip upper values
           if (Value[0] >= 1.0) {
               Output[0] = LutTable[p->Domain[0]]; 
               return; 
           }

        val2 = p -> Domain[0] * Value[0];
        cell = (int) Math.Floor(val2);
        Output[0] =  LutTable[cell] ;
    }

    // This fake interpolation just uses scrambled negated indexes for output
    private static void Fake3D16(in ushort* Input,
                                 ushort* Output,
                                 in InterpParams* _)
    {
        Output[0] = (ushort)(0xFFFF - Input[2]);
        Output[1] = (ushort)(0xFFFF - Input[1]);
        Output[2] = (ushort)(0xFFFF - Input[0]);
    }

    // The factory chooses interpolation routines on depending on certain conditions.
    private static InterpFunction my_Interpolators_Factory(uint nInputChannels,
                                                           uint nOutputChannels,
                                                           uint dwFlags)
    {
        InterpFunction Interpolation;
        var IsFloat = (dwFlags & (uint)LerpFlag.Float) is not 0;

        // Initialize the return to zero as a non-supported mark
        memset(&Interpolation, 0);

        // For 1D to 1D and floating point
        if (nInputChannels == 1 && nOutputChannels == 1 && IsFloat)
        {

            Interpolation.LerpFloat = Fake1Dfloat;
        }
        else
        if (nInputChannels == 3 && nOutputChannels == 3 && !IsFloat)
        {

            // For 3D to 3D and 16 bits
            Interpolation.Lerp16 = Fake3D16;
        }

        // Here is the interpolation 
        return Interpolation;
    }


public static bool CheckAllocContext()
    {
        fixed (void* handler = &DebugMemHandler)
        {
            var c1 = cmsCreateContext(null, null);      // This creates a context by using the normal malloc
            DebugMemDontCheckThis(c1);
            cmsDeleteContext(c1);

            var c2 = cmsCreateContext(handler, null); // This creates a context by using the debug malloc
            DebugMemDontCheckThis(c2);
            cmsDeleteContext(c2);

            c1 = cmsCreateContext(null, null);
            DebugMemDontCheckThis(c1);

            c2 = cmsCreateContext(handler, null);
            DebugMemDontCheckThis(c2);

            cmsPluginTHR(c1, handler);    // Now the context has custom allocators

            var c3 = DupContext(c1, null);
            var c4 = DupContext(c2, null);

            cmsDeleteContext(c1);   // Should be deleted by using normal malloc
            cmsDeleteContext(c2);   // Should be deleted by using debug malloc
            cmsDeleteContext(c3);   // Should be deleted by using normal malloc
            cmsDeleteContext(c4);   // Should be deleted by using debug malloc

            return true;
        }
    }

    public static bool CheckSimpleContext()
    {
        var a = 1;
        var b = 32;
        var rc = false;

        // This function creates a context with a special
        // memory manager that checks allocation
        var c1 = WatchDogContext(&a);
        cmsDeleteContext(c1);

        c1 = WatchDogContext(&a);

        // Let's check duplication
        var c2 = DupContext(c1, null);
        var c3 = DupContext(c2, null);

        // User data should have been propagated
        rc = (*(int*)cmsGetContextUserData(c3)) == 1;

        // Free resources
        cmsDeleteContext(c1);
        cmsDeleteContext(c2);
        cmsDeleteContext(c3);

        if (!rc)
            return Fail("Creation of user data failed");

        // Back to create 3 levels of inheritance
        c1 = cmsCreateContext(null, &a);
        DebugMemDontCheckThis(c1);

        c2 = DupContext(c1, null);
        c3 = DupContext(c2, &b);

        // New user data should be applied to c3
        rc = (*(int*)cmsGetContextUserData(c3)) == 32;

        cmsDeleteContext(c1);
        cmsDeleteContext(c2);
        cmsDeleteContext(c3);

        if (!rc)
            return Fail("Modification of user data failed");

        return true;
    }

    public static bool CheckAlarmColorsContext()
    {
        var codes = stackalloc ushort[16] { 0x0000, 0x1111, 0x2222, 0x3333, 0x4444, 0x5555, 0x6666, 0x7777, 0x8888, 0x9999, 0xaaaa, 0xbbbb, 0xcccc, 0xdddd, 0xeeee, 0xffff };
        var values = stackalloc ushort[16];

        var c1 = WatchDogContext(null);

        cmsSetAlarmCodesTHR(c1, codes);
        var c2 = DupContext(c1, null);
        var c3 = DupContext(c2, null);

        cmsGetAlarmCodesTHR(c3, values);

        var rc = true;

        for (var i = 0; i < 16; i++)
        {
            if (values[i] != codes[i])
            {
                Fail($"Bad alarm code #{i}: {values[i]} != {codes[i]}");
                rc = false;
                break;
            }
        }

        cmsDeleteContext(c1);
        cmsDeleteContext(c2);
        cmsDeleteContext(c3);

        return rc;
    }

    public static bool CheckAdaptationStateContext()
    {
        var rc = false;
        var old1 = cmsSetAdaptationStateTHR(null, -1);

        var c1 = WatchDogContext(null);

        cmsSetAdaptationStateTHR(c1, 0.7);

        var c2 = DupContext(c1, null);
        var c3 = DupContext(c2, null);

        rc = IsGoodVal("Adaption state", cmsSetAdaptationStateTHR(c3, -1), 0.7, 0.001);

        cmsDeleteContext(c1);
        cmsDeleteContext(c2);
        cmsDeleteContext(c3);

        var old2 = cmsSetAdaptationStateTHR(null, -1);

        if (old1 != old2)
            return Fail("Adaptation state has changed");

        return rc;
    }

    // This is the check code for 1D interpolation plug-in
    public static bool CheckInterp1DPlugin()
    {
        ToneCurve* Sampled1D = null;
        Context ctx = null;
        Context cpy = null;
        var tab = stackalloc float[] { 0.0f, 0.10f, 0.20f, 0.30f, 0.40f, 0.50f, 0.60f, 0.70f, 0.80f, 0.90f, 1.00f };  // A straight line

        // 1st level context
        ctx = WatchDogContext(null);
        if (ctx == null)
        {
            Fail("Cannot create context");
            goto Error;
        }

        fixed (PluginInterpolation* sample = &InterpPluginSample)
            cmsPluginTHR(ctx, sample);

        cpy = DupContext(ctx, null);
        if (cpy == null)
        {
            Fail("Cannot create context (2)");
            goto Error;
        }

        Sampled1D = cmsBuildTabulatedToneCurveFloat(cpy, 11, tab);
        if (Sampled1D == null)
        {
            Fail("Cannot create tone curve (1)");
            goto Error;
        }

        // Do some interpolations with the plugin
        if (!IsGoodVal("0.10", cmsEvalToneCurveFloat(Sampled1D, 0.10f), 0.10, 0.01)) goto Error;
        if (!IsGoodVal("0.13", cmsEvalToneCurveFloat(Sampled1D, 0.13f), 0.10, 0.01)) goto Error;
        if (!IsGoodVal("0.55", cmsEvalToneCurveFloat(Sampled1D, 0.55f), 0.50, 0.01)) goto Error;
        if (!IsGoodVal("0.9999", cmsEvalToneCurveFloat(Sampled1D, 0.9999f), 0.90, 0.01)) goto Error;

        cmsFreeToneCurve(Sampled1D);
        cmsDeleteContext(ctx);
        cmsDeleteContext(cpy);

        // Now in global context
        Sampled1D = cmsBuildTabulatedToneCurveFloat(null, 11, tab);
        if (Sampled1D == null)
        {
            Fail("Cannot create tone curve (2)");
            goto Error;
        }

        // Now without the plug-in
        if (!IsGoodVal("0.10", cmsEvalToneCurveFloat(Sampled1D, 0.10f), 0.10, 0.001)) goto Error;
        if (!IsGoodVal("0.13", cmsEvalToneCurveFloat(Sampled1D, 0.13f), 0.13, 0.001)) goto Error;
        if (!IsGoodVal("0.55", cmsEvalToneCurveFloat(Sampled1D, 0.55f), 0.55, 0.001)) goto Error;
        if (!IsGoodVal("0.9999", cmsEvalToneCurveFloat(Sampled1D, 0.9999f), 0.9999, 0.001)) goto Error;

        cmsFreeToneCurve(Sampled1D);
        return true;

    Error:
        if (ctx != null) cmsDeleteContext(ctx);
        if (cpy != null) cmsDeleteContext(ctx);
        if (Sampled1D != null) cmsFreeToneCurve(Sampled1D);
        return false;

    }

    // Checks the 3D interpolation
    public static bool CheckInterp3DPlugin()
    {

        Pipeline* p;
        Stage* clut;
        Context ctx;
        var In = stackalloc ushort[3];
        var Out = stackalloc ushort[3];
        var identity = stackalloc ushort[] {
            0,       0,       0,
            0,       0,       0xffff,
            0,       0xffff,  0,
            0,       0xffff,  0xffff,
            0xffff,  0,       0,
            0xffff,  0,       0xffff,
            0xffff,  0xffff,  0,
            0xffff,  0xffff,  0xffff
        };


        ctx = WatchDogContext(null);
        if (ctx == null)
        {
            Fail("Cannot create context");
            return false;
        }

        fixed(PluginInterpolation* sample = &InterpPluginSample)
        cmsPluginTHR(ctx, sample);


        p = cmsPipelineAlloc(ctx, 3, 3);
        clut = cmsStageAllocCLut16bit(ctx, 2, 3, 3, identity);
        cmsPipelineInsertStage(p, StageLoc.AtBegin, clut);

        // Do some interpolations with the plugin

        In[0] = 0; In[1] = 0; In[2] = 0;
        cmsPipelineEval16(In, Out, p);

        if (!IsGoodWord("0", Out[0], 0xFFFF - 0)) goto Error;
        if (!IsGoodWord("1", Out[1], 0xFFFF - 0)) goto Error;
        if (!IsGoodWord("2", Out[2], 0xFFFF - 0)) goto Error;

        In[0] = 0x1234; In[1] = 0x5678; In[2] = 0x9ABC;
        cmsPipelineEval16(In, Out, p);

        if (!IsGoodWord("0", 0xFFFF - 0x9ABC, Out[0])) goto Error;
        if (!IsGoodWord("1", 0xFFFF - 0x5678, Out[1])) goto Error;
        if (!IsGoodWord("2", 0xFFFF - 0x1234, Out[2])) goto Error;

        cmsPipelineFree(p);
        cmsDeleteContext(ctx);

        // Now without the plug-in

        p = cmsPipelineAlloc(null, 3, 3);
        clut = cmsStageAllocCLut16bit(null, 2, 3, 3, identity);
        cmsPipelineInsertStage(p, StageLoc.AtBegin, clut);

        In[0] = 0; In[1] = 0; In[2] = 0;
        cmsPipelineEval16(In, Out, p);

        if (!IsGoodWord("0", 0, Out[0])) goto Error;
        if (!IsGoodWord("1", 0, Out[1])) goto Error;
        if (!IsGoodWord("2", 0, Out[2])) goto Error;

        In[0] = 0x1234; In[1] = 0x5678; In[2] = 0x9ABC;
        cmsPipelineEval16(In, Out, p);

        if (!IsGoodWord("0", 0x1234, Out[0])) goto Error;
        if (!IsGoodWord("1", 0x5678, Out[1])) goto Error;
        if (!IsGoodWord("2", 0x9ABC, Out[2])) goto Error;

        cmsPipelineFree(p);
        return true;

    Error:
        cmsPipelineFree(p);
        return false;

    }
}
