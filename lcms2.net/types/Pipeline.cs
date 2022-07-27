using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;

namespace lcms2.types;
#if PLUGIN
    public
#else
internal
#endif
    delegate void PipelineEval16Fn(in ushort[] @in, ushort[] @out, in object? data);
#if PLUGIN
    public
#else
internal
#endif
    delegate void PipelineEvalFloatFn(in float[] @in, float[] @out, in object? data);

public class Pipeline
{
    internal Stage? Elements;
    internal int InputChannels, OutputChannels;

    internal object? Data;

    internal PipelineEval16Fn Eval16Fn;
    internal PipelineEvalFloatFn EvalFloatFn;
    internal FreeUserDataFn FreeDataFn;
    internal DupUserDataFn DupDataFn;

    internal Context Context;

    internal bool SaveAs8Bits;
}
