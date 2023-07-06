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
using lcms2.io;
using lcms2.state;
using lcms2.types;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace lcms2.testbed;

internal static unsafe partial class Testbed
{
    const ushort TYPE_SIN = 1000;
    const ushort TYPE_COS = 1010;
    const ushort TYPE_TAN = 1020;
    const ushort TYPE_709 = 709;

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
    private static double my_fns(int Type,
                                 in double* Params,
                                 double R) =>
        Type switch
        {
            TYPE_SIN => Params[0] * Math.Sin(R * M_PI),
            -TYPE_SIN => Math.Asin(R) / (M_PI * Params[0]),
            TYPE_COS => Params[0] * Math.Cos(R * M_PI),
            -TYPE_COS => Math.Acos(R) / (M_PI * Params[0]),
            _ => -1.0
        };

    private static double my_fns2(int Type, in double* Params, double R) =>
        Type switch
        {
            TYPE_TAN => Params[0] * Math.Tan(R * M_PI),
            -TYPE_TAN => Math.Asin(R) / (M_PI * Params[0]),
            _ => -1.0
        };

    private static double Rec709Math(int Type, in double* Params, double R) =>
        Type switch
        {
            709 =>
                (R <= (Params[3] * Params[4]))
                    ? R / Params[3]
                    : Math.Pow(((R - Params[2]) / Params[1]), Params[0]),
            -709 =>
                (R <= Params[4])
                    ? R * Params[3]
                    : Params[1] * Math.Pow(R, (1 / Params[0])) + Params[2],
            _ => 0
        };


    // Add nonstandard TRC curves -> Rec709

    private readonly static PluginParametricCurves Rec709Plugin = new() {
        @base = new() { Magic = cmsPluginMagicNumber, ExpectedVersion = 2060, Type = cmsPluginParametricCurveSig, Next = null },
        NumFunctions = 1,
        //{TYPE_709},
        //{5},
        Evaluator = Rec709Math
    };


    private readonly static PluginParametricCurves CurvePluginSample = new() {
        @base = new() { Magic = cmsPluginMagicNumber, ExpectedVersion = 2060, Type = cmsPluginParametricCurveSig, Next = null},
        NumFunctions = 2,
        //{ TYPE_SIN, TYPE_COS },
        //{ 1, 1 },
        Evaluator = my_fns
    };

    private readonly static PluginParametricCurves CurvePluginSample2 = new()
    {
        @base = new() { Magic = cmsPluginMagicNumber, ExpectedVersion = 2060, Type = cmsPluginParametricCurveSig, Next = null },
        NumFunctions = 1,
        //{ TYPE_TAN },
        //{ 1 },
        Evaluator = my_fns2
    };

    // --------------------------------------------------------------------------------------------------
    // In this test, the DupContext function will be checked as well                      
    // --------------------------------------------------------------------------------------------------
    public static bool CheckParametricCurvePlugin()
    {
        Context ctx = null;
        Context cpy = null;
        Context cpy2 = null;
        ToneCurve* sinus;
        ToneCurve* cosinus;
        ToneCurve* tangent;
        ToneCurve* reverse_sinus;
        ToneCurve* reverse_cosinus;
        var scale = 1.0;


        ctx = WatchDogContext(null);

        fixed (PluginParametricCurves* sample = &CurvePluginSample)
            cmsPluginTHR(ctx, sample);

        cpy = DupContext(ctx, null);

        fixed (PluginParametricCurves* sample = &CurvePluginSample2)
            cmsPluginTHR(cpy, sample);

        cpy2 = DupContext(cpy, null);

        fixed (PluginParametricCurves* sample = &Rec709Plugin)
            cmsPluginTHR(cpy2, sample);


        sinus = cmsBuildParametricToneCurve(cpy, TYPE_SIN, &scale);
        cosinus = cmsBuildParametricToneCurve(cpy, TYPE_COS, &scale);
        tangent = cmsBuildParametricToneCurve(cpy, TYPE_TAN, &scale);
        reverse_sinus = cmsReverseToneCurve(sinus);
        reverse_cosinus = cmsReverseToneCurve(cosinus);


        if (!IsGoodVal("0.10", cmsEvalToneCurveFloat(sinus, 0.10f), Math.Sin(0.10 * M_PI), 0.001)) goto Error;
        if (!IsGoodVal("0.60", cmsEvalToneCurveFloat(sinus, 0.60f), Math.Sin(0.60 * M_PI), 0.001)) goto Error;
        if (!IsGoodVal("0.90", cmsEvalToneCurveFloat(sinus, 0.90f), Math.Sin(0.90 * M_PI), 0.001)) goto Error;

        if (!IsGoodVal("0.10", cmsEvalToneCurveFloat(cosinus, 0.10f), Math.Cos(0.10 * M_PI), 0.001)) goto Error;
        if (!IsGoodVal("0.60", cmsEvalToneCurveFloat(cosinus, 0.60f), Math.Cos(0.60 * M_PI), 0.001)) goto Error;
        if (!IsGoodVal("0.90", cmsEvalToneCurveFloat(cosinus, 0.90f), Math.Cos(0.90 * M_PI), 0.001)) goto Error;

        if (!IsGoodVal("0.10", cmsEvalToneCurveFloat(tangent, 0.10f), Math.Tan(0.10 * M_PI), 0.001)) goto Error;
        if (!IsGoodVal("0.60", cmsEvalToneCurveFloat(tangent, 0.60f), Math.Tan(0.60 * M_PI), 0.001)) goto Error;
        if (!IsGoodVal("0.90", cmsEvalToneCurveFloat(tangent, 0.90f), Math.Tan(0.90 * M_PI), 0.001)) goto Error;


        if (!IsGoodVal("0.10", cmsEvalToneCurveFloat(reverse_sinus, 0.10f), Math.Asin(0.10) / M_PI, 0.001)) goto Error;
        if (!IsGoodVal("0.60", cmsEvalToneCurveFloat(reverse_sinus, 0.60f), Math.Asin(0.60) / M_PI, 0.001)) goto Error;
        if (!IsGoodVal("0.90", cmsEvalToneCurveFloat(reverse_sinus, 0.90f), Math.Asin(0.90) / M_PI, 0.001)) goto Error;

        if (!IsGoodVal("0.10", cmsEvalToneCurveFloat(reverse_cosinus, 0.10f), Math.Acos(0.10) / M_PI, 0.001)) goto Error;
        if (!IsGoodVal("0.60", cmsEvalToneCurveFloat(reverse_cosinus, 0.60f), Math.Acos(0.60) / M_PI, 0.001)) goto Error;
        if (!IsGoodVal("0.90", cmsEvalToneCurveFloat(reverse_cosinus, 0.90f), Math.Acos(0.90) / M_PI, 0.001)) goto Error;

        cmsFreeToneCurve(sinus);
        cmsFreeToneCurve(cosinus);
        cmsFreeToneCurve(tangent);
        cmsFreeToneCurve(reverse_sinus);
        cmsFreeToneCurve(reverse_cosinus);

        cmsDeleteContext(ctx);
        cmsDeleteContext(cpy);
        cmsDeleteContext(cpy2);

        return true;

    Error:

        cmsFreeToneCurve(sinus);
        cmsFreeToneCurve(reverse_sinus);
        cmsFreeToneCurve(cosinus);
        cmsFreeToneCurve(reverse_cosinus);

        if (ctx != null) cmsDeleteContext(ctx);
        if (cpy != null) cmsDeleteContext(cpy);
        if (cpy2 != null) cmsDeleteContext(cpy2);
        return false;
    }

    // We define this special type as 0 bytes not float, and set the upper bit 

    private readonly static uint TYPE_RGB_565 = (COLORSPACE_SH(PT_RGB) | CHANNELS_SH(3) | BYTES_SH(0) | (1 << 23));

    private static byte* my_Unroll565(Transform* _1,
                                      ushort* wIn,
                                      byte* accum,
                                      uint _2)
    {
        var pixel = *(ushort*)accum;  // Take whole pixel

        double r = Math.Floor(((pixel & 31) * 65535.0) / 31.0 + 0.5);
        double g = Math.Floor((((pixel >> 5) & 63) * 65535.0) / 63.0 + 0.5);
        double b = Math.Floor((((pixel >> 11) & 31) * 65535.0) / 31.0 + 0.5);

        wIn[2] = (ushort) r;
        wIn[1] = (ushort) g;
        wIn[0] = (ushort) b;
    
        return accum + 2;
    }

    private static byte* my_Pack565(Transform* _1,
                                    ushort* wOut,
                                    byte* output,
                                    uint _2)
    {

        ushort pixel;
        int r, g, b;

        r = (int)Math.Floor((wOut[2] * 31) / 65535.0 + 0.5);
        g = (int)Math.Floor((wOut[1] * 63) / 65535.0 + 0.5);
        b = (int)Math.Floor((wOut[0] * 31) / 65535.0 + 0.5);


        pixel = (ushort)((r & 31) | ((g & 63) << 5) | ((b & 31) << 11));


        *(ushort*)output = pixel;
        return output + 2;
    }

    private static Formatter my_FormatterFactory(uint Type,
                                                 FormatterDirection Dir,
                                                 uint dwFlags)
    {
        Formatter Result = default;

        if ((Type == TYPE_RGB_565) &&
            (dwFlags & (uint)PackFlags.Float) is 0 &&
            (Dir == FormatterDirection.Input))
        {
            Result.Fmt16 = my_Unroll565;
        }
        return Result;
    }

    private static Formatter my_FormatterFactory2(uint Type,
                                                  FormatterDirection Dir,
                                                  uint dwFlags)
    {
        Formatter Result = default;

        if ((Type == TYPE_RGB_565) &&
            (dwFlags & (uint)PackFlags.Float) is 0 &&
            (Dir == FormatterDirection.Output))
        {
            Result.Fmt16 = my_Pack565;
        }
        return Result;
    }

    private readonly static PluginFormatters FormattersPluginSample = new() 
    {
        @base = new() { Magic = cmsPluginMagicNumber, ExpectedVersion = 2060, Type = cmsPluginFormattersSig, Next = null },
        FormattersFactory = my_FormatterFactory
    };



    private readonly static PluginFormatters FormattersPluginSample2 = new() 
    {
        @base = new() { Magic = cmsPluginMagicNumber, ExpectedVersion = 2060, Type = cmsPluginFormattersSig, Next = null },
        FormattersFactory = my_FormatterFactory2
    };


    public static bool CheckFormattersPlugin()
    {
        Context ctx = WatchDogContext(null);
        Context cpy;
        Context cpy2;
        Transform* xform;
        var stream = stackalloc ushort[] { (ushort)0xffffU, (ushort)0x1234U, (ushort)0x0000U, (ushort)0x33ddU };
        var result = stackalloc ushort[4];
        int i;

        fixed (PluginFormatters* sample = &FormattersPluginSample)
            cmsPluginTHR(ctx, sample);

        cpy = DupContext(ctx, null);

        fixed (PluginFormatters* sample = &FormattersPluginSample2)
            cmsPluginTHR(cpy, sample);

        cpy2 = DupContext(cpy, null);

        xform = cmsCreateTransformTHR(cpy2, null, TYPE_RGB_565, null, TYPE_RGB_565, INTENT_PERCEPTUAL, cmsFLAGS_NULLTRANSFORM);

        cmsDoTransform(xform, stream, result, 4);

        cmsDeleteTransform(xform);
        cmsDeleteContext(ctx);
        cmsDeleteContext(cpy);
        cmsDeleteContext(cpy2);

        for (i = 0; i < 4; i++)
            if (stream[i] != result[i]) return false;

        return true;
    }

    const uint SigIntType = 0x74747448;   //   'tttH'
    const uint SigInt = 0x74747448;       //   'tttH'

    private static void* Type_int_Read(TagTypeHandler* self,IOHandler* io,uint* nItems,uint _)
    {
        var Ptr = _cmsMalloc<uint>(self->ContextID);
        if (Ptr == null) return null;
        if (!_cmsReadUInt32Number(io, Ptr)) return null;
        *nItems = 1;
        return Ptr;
    }

    private static bool Type_int_Write(TagTypeHandler* _1, IOHandler* io, void* Ptr, uint _2) =>
        _cmsWriteUInt32Number(io, *(uint*)Ptr);

    private static void* Type_int_Dup(TagTypeHandler* self, in void* Ptr, uint n) =>
        _cmsDupMem<uint>(self->ContextID, Ptr, n);

    private static void Type_int_Free(TagTypeHandler* self, void* Ptr) =>
        _cmsFree(self->ContextID, Ptr);


    private readonly static PluginTag HiddenTagPluginSample = new() {
        @base = new() { Magic = cmsPluginMagicNumber, ExpectedVersion = 2060, Type = cmsPluginTagSig, Next = null },
        Signature = SigInt,
        Descriptor = new () { ElemCount = 1, nSupportedTypes = 1, /*{ SigIntType },*/ DecideType = null }
    };

    private readonly static PluginTagType TagTypePluginSample = new() {
        @base = new() { Magic = cmsPluginMagicNumber, ExpectedVersion = 2060, Type = cmsPluginTagTypeSig, Next = /*(PluginBase*) &HiddenTagPluginSample*/ null },
        Handler = new(SigIntType, Type_int_Read, Type_int_Write, Type_int_Dup, Type_int_Free, null, 0)
    };


    public static bool CheckTagTypePlugin()
    {
        Context ctx = null;
        Context cpy = null;
        Context cpy2 = null;
        HPROFILE h = null;
        uint myTag = 1234;
        bool rc = false;
        byte* data = null;
        uint* ptr = null;
        uint clen = 0;


        ctx = WatchDogContext(null);
        fixed (PluginTagType* sample = &TagTypePluginSample)
            cmsPluginTHR(ctx, sample);
        fixed (PluginTag* sample = &HiddenTagPluginSample)
            cmsPluginTHR(ctx, sample);

        cpy = DupContext(ctx, null);
        cpy2 = DupContext(cpy, null);

        cmsDeleteContext(ctx);
        cmsDeleteContext(cpy);

        h = cmsCreateProfilePlaceholder(cpy2);
        //h = cmsCreateProfilePlaceholder(ctx);
        if (h == null)
        {
            Fail("Create placeholder failed");
            goto Error;
        }


        if (!cmsWriteTag(h, SigInt, &myTag))
        {
            Fail("Plug-in failed");
            goto Error;
        }

        rc = cmsSaveProfileToMem(h, null, &clen);
        if (!rc)
        {
            Fail("Fetch mem size failed");
            goto Error;
        }


        data = (byte*)alloc(clen);
        if (data == null)
        {
            Fail("malloc failed ?!?");
            goto Error;
        }


        rc = cmsSaveProfileToMem(h, data, &clen);
        if (!rc)
        {
            Fail("Save to mem failed");
            goto Error;
        }

        cmsCloseProfile(h);

        cmsSetLogErrorHandler((_1, _2, _3) => { });
        h = cmsOpenProfileFromMem(data, clen);
        if (h == null)
        {
            Fail("Open profile failed");
            goto Error;
        }

        ptr = (uint*)cmsReadTag(h, SigInt);
        if (ptr != null)
        {

            Fail("read tag/context switching failed");
            goto Error;
        }

        cmsCloseProfile(h);
        ResetFatalError();

        h = cmsOpenProfileFromMemTHR(cpy2, data, clen);
        //h = cmsOpenProfileFromMemTHR(ctx, data, clen);
        if (h == null)
        {
            Fail("Open profile from mem failed");
            goto Error;
        }

        // Get rid of data
        free(data); data = null;

        ptr = (uint*)cmsReadTag(h, SigInt);
        if (ptr == null)
        {
            Fail("Read tag/conext switching failed (2)");
            return false;
        }

        rc = (*ptr == 1234);

        cmsCloseProfile(h);

        cmsDeleteContext(cpy2);
        //cmsDeleteContext(ctx);

        return rc;

    Error:

        if (h != null) cmsCloseProfile(h);
        if (ctx != null) cmsDeleteContext(ctx);
        if (cpy != null) cmsDeleteContext(cpy);
        if (cpy2 != null) cmsDeleteContext(cpy2);
        if (data is not null) free(data);

        return false;
    }

    private const uint SigNegateType = 0x6E202020;

    private static void EvaluateNegate(in float* In, float* Out, in Stage* _)
    {
        Out[0] = 1.0f - In[0];
        Out[1] = 1.0f - In[1];
        Out[2] = 1.0f - In[2];
    }

    private static Stage* StageAllocNegate(Context ContextID)
    {
        return _cmsStageAllocPlaceholder(ContextID,
                     SigNegateType, 3, 3, EvaluateNegate,
                     null, null, null);
    }

    private static void* Type_negate_Read(TagTypeHandler* self, IOHandler* io, uint* nItems, uint _)
    {
        ushort Chans;
        if (!_cmsReadUInt16Number(io, &Chans)) return null;
        if (Chans != 3) return null;

        * nItems = 1;
        return StageAllocNegate(self -> ContextID);
    }

    private static bool Type_negate_Write(TagTypeHandler* _1, IOHandler* io, void* _2, uint _3)
    {
        return _cmsWriteUInt16Number(io, 3);
    }

    private readonly static PluginTagType MPEPluginSample = new() {
        @base = new() { Magic = cmsPluginMagicNumber, ExpectedVersion = 2060, Type = cmsPluginMultiProcessElementSig, Next = null },
        Handler = new(SigNegateType, Type_negate_Read, Type_negate_Write, null, null, null, 0)
    };


    public static bool CheckMPEPlugin()
    {
        Context ctx = null;
        Context cpy = null;
        Context cpy2 = null;
        HPROFILE h = null;
        uint myTag = 1234;
        bool rc = false;
        byte* data = null;
        uint clen = 0;
        var In = stackalloc float[3];
        var Out = stackalloc float[3];
        Pipeline* pipe;

        ctx = WatchDogContext(null);
        fixed (PluginTagType* sample = &MPEPluginSample)
            cmsPluginTHR(ctx, sample);

        cpy = DupContext(ctx, null);
        cpy2 = DupContext(cpy, null);

        cmsDeleteContext(ctx);
        cmsDeleteContext(cpy);

        h = cmsCreateProfilePlaceholder(cpy2);
        if (h == null)
        {
            Fail("Create placeholder failed");
            goto Error;
        }

        pipe = cmsPipelineAlloc(cpy2, 3, 3);
        cmsPipelineInsertStage(pipe, StageLoc.AtBegin, StageAllocNegate(cpy2));


        In[0] = 0.3f; In[1] = 0.2f; In[2] = 0.9f;
        cmsPipelineEvalFloat(In, Out, pipe);

        rc = IsGoodVal("0", Out[0], 1.0 - In[0], 0.001) &&
             IsGoodVal("1", Out[1], 1.0 - In[1], 0.001) &&
             IsGoodVal("2", Out[2], 1.0 - In[2], 0.001);

        if (!rc)
        {
            Fail("Pipeline failed");
            goto Error;
        }

        if (!cmsWriteTag(h, cmsSigDToB3Tag, pipe))
        {
            Fail("Plug-in failed");
            goto Error;
        }

        // This cleans the stage as well
        cmsPipelineFree(pipe);

        rc = cmsSaveProfileToMem(h, null, &clen);
        if (!rc)
        {
            Fail("Fetch mem size failed");
            goto Error;
        }


        data = (byte*)alloc(clen);
        if (data == null)
        {
            Fail("malloc failed ?!?");
            goto Error;
        }


        rc = cmsSaveProfileToMem(h, data, &clen);
        if (!rc)
        {
            Fail("Save to mem failed");
            goto Error;
        }

        cmsCloseProfile(h);


        cmsSetLogErrorHandler((_1, _2, _3) => { });
        h = cmsOpenProfileFromMem(data, clen);
        if (h == null)
        {
            Fail("Open profile failed");
            goto Error;
        }

        pipe = (Pipeline*)cmsReadTag(h, cmsSigDToB3Tag);
        if (pipe != null)
        {

            // Unsupported stage, should fail
            Fail("read tag/context switching failed");
            goto Error;
        }

        cmsCloseProfile(h);

        ResetFatalError();

        h = cmsOpenProfileFromMemTHR(cpy2, data, clen);
        if (h == null)
        {
            Fail("Open profile from mem failed");
            goto Error;
        }

        // Get rid of data
        free(data); data = null;

        pipe = (Pipeline*)cmsReadTag(h, cmsSigDToB3Tag);
        if (pipe == null)
        {
            Fail("Read tag/conext switching failed (2)");
            return false;
        }

        // Evaluate for negation
        In[0] = 0.3f; In[1] = 0.2f; In[2] = 0.9f;
        cmsPipelineEvalFloat(In, Out, pipe);

        rc = IsGoodVal("0", Out[0], 1.0 - In[0], 0.001) &&
             IsGoodVal("1", Out[1], 1.0 - In[1], 0.001) &&
             IsGoodVal("2", Out[2], 1.0 - In[2], 0.001);

        cmsCloseProfile(h);

        cmsDeleteContext(cpy2);

        return rc;

    Error:

        if (h != null) cmsCloseProfile(h);
        if (ctx != null) cmsDeleteContext(ctx);
        if (cpy != null) cmsDeleteContext(cpy);
        if (cpy2 != null) cmsDeleteContext(cpy2);
        if (data != null) free(data);

        return false;
    }

}
