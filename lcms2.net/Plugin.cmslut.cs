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
using lcms2.state;
using lcms2.types;

namespace lcms2;
public static partial class Plugin
{
    public static Stage _cmsStageAllocPlaceholder(
        Context? ContextID,
        Signature Type,
        uint InputChannels,
        uint OutputChannels,
        StageEvalFn EvalPtr,
        StageDupElemFn? DupElemPtr,
        StageFreeElemFn? FreePtr,
        object? Data)
    {
        var ph = new Stage();
        //if (ph is null) return null;

        ph.ContextID = ContextID;

        ph.Type = Type;
        ph.Implements = Type;  // By default, no clue on what is implementing

        ph.InputChannels = InputChannels;
        ph.OutputChannels = OutputChannels;
        ph.EvalPtr = EvalPtr;
        ph.DupElemPtr = DupElemPtr;
        ph.FreePtr = FreePtr;
        ph.Data = Data;

        return ph;
    }

    // This function may be used to set the optional evaluator and a block of private data. If private data is being used, an optional
    // duplicator and free functions should also be specified in order to duplicate the LUT construct. Use NULL to inhibit such functionality.
    public static void _cmsPipelineSetOptimizationParameters(
        Pipeline Lut,
        PipelineEval16Fn Eval16,
        object? PrivateData,
        FreeUserDataFn? FreePrivateDataFn,
        DupUserDataFn? DupPrivateDataFn)
    {
        Lut.Eval16Fn = Eval16;
        Lut.DupDataFn = DupPrivateDataFn;
        Lut.FreeDataFn = FreePrivateDataFn;
        Lut.Data = PrivateData;
    }
}
