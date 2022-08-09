using lcms2.state;

namespace lcms2.types;

/// <summary>
///     Pipeline evaluator (in 16 bits)
/// </summary>
/// <remarks>
///     Implements the <c>_cmsPipelineEval16Fn</c> typedef.</remarks>
public delegate void PipelineEval16Fn(in ushort[] @in, ushort[] @out, in object? data);

/// <summary>
///     Pipeline evaluator (in floating point)
/// </summary>
/// <remarks>
///     Implements the <c>_cmsPipelineEvalFloatFn</c> typedef.</remarks>
public delegate void PipelineEvalFloatFn(in float[] @in, float[] @out, in object? data);

public class Pipeline
{
    internal Stage? Elements;
    internal int InputChannels, OutputChannels;

    internal object? Data;

    internal PipelineEval16Fn? Eval16Fn;
    internal PipelineEvalFloatFn? EvalFloatFn;
    internal FreeUserDataFn? FreeDataFn;
    internal DupUserDataFn? DupDataFn;

    internal Context Context;

    internal bool SaveAs8Bits;

    /// <summary>
    ///     This function may be used to set the optional evaluator and a block of private data. If private data is being used, an optional
    ///     duplicator and free functions should also be specified in order to duplicate the LUT construct. Use <see langword="null"/> to
    ///     inhibit such functionality.
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsPipelineSetOptimizationParameters</c> function.</remarks>
    public void SetOptimizationParameters(PipelineEval16Fn? eval16, object? privateData, FreeUserDataFn? freePrivateDataFn, DupUserDataFn? dupPrivateDataFn)
    {
        Eval16Fn = eval16;
        EvalFloatFn = null;
        Data = privateData;
        FreeDataFn = freePrivateDataFn;
        DupDataFn = dupPrivateDataFn;
    }

    /// <summary>
    ///     This function may be used to set the optional evaluator and a block of private data. If private data is being used, an optional
    ///     duplicator and free functions should also be specified in order to duplicate the LUT construct. Use <see langword="null"/> to
    ///     inhibit such functionality.
    /// </summary>
    public void SetOptimizationParameters(PipelineEvalFloatFn? evalFloat, object? privateData, FreeUserDataFn? freePrivateDataFn, DupUserDataFn? dupPrivateDataFn)
    {
        Eval16Fn = null;
        EvalFloatFn = evalFloat;
        Data = privateData;
        FreeDataFn = freePrivateDataFn;
        DupDataFn = dupPrivateDataFn;
    }
}
