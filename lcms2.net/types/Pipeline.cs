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

public class Pipeline : ICloneable, IDisposable
{
    internal Stage? Elements;
    internal uint InputChannels, OutputChannels;

    internal object? Data;

    internal PipelineEval16Fn? Eval16Fn;
    internal PipelineEvalFloatFn? EvalFloatFn;
    internal FreeUserDataFn? FreeDataFn;
    internal DupUserDataFn? DupDataFn;

    internal Context? Context;

    internal bool SaveAs8Bits;

    internal Pipeline(Stage? elements, uint inputChannels, uint outputChannels, object? data, PipelineEval16Fn? eval16Fn, PipelineEvalFloatFn? evalFloatFn, FreeUserDataFn? freeDataFn, DupUserDataFn? dupDataFn, Context? context, bool saveAs8Bits)
    {
        Elements = elements;
        InputChannels = inputChannels;
        OutputChannels = outputChannels;
        Data = data;
        Eval16Fn = eval16Fn;
        EvalFloatFn = evalFloatFn;
        FreeDataFn = freeDataFn;
        DupDataFn = dupDataFn;
        Context = context;
        SaveAs8Bits = saveAs8Bits;
    }

    public static Pipeline? Alloc(Context? context, uint inputChannels, uint outputChannels)
    {
        // A value of zero in channels is allowed as placeholder
        if (inputChannels >= Lcms2.MaxChannels ||
            outputChannels >= Lcms2.MaxChannels) return null;

        var newLut = new Pipeline(
            null,
            inputChannels,
            outputChannels,
            null,
            LutEval16,
            LutEvalFloat,
            null,
            null,
            context,
            false);
        newLut.Data = newLut;

        if (!newLut.BlessLut()) {
            newLut.Dispose();
            return null;
        }

        return newLut;
    }

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

    public bool InsertStage(StageLoc loc, Stage? mpe)
    {
        throw new NotImplementedException();
    }

    public bool CheckAndRetreiveStages(out Stage? a, out Stage? clut, out Stage? m, out Stage? matrix, out Stage? b)
    {
        a = null;
        clut = null;
        m = null;
        matrix = null;
        b = null;

        var stages = new List<Stage>(5);

        for (var mpe = Elements; mpe is not null; mpe = mpe.Next)
            stages.Add(mpe);

        switch (stages.Count) {
            case 1:

                if (stages[0].Type == Signature.Stage.CurveSetElem) {

                    b = stages[0];
                    return true;
                } else {

                    return false;
                }

            case 3:

                if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.MatrixElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem) {

                    m = stages[0];
                    matrix = stages[1];
                    b = stages[2];

                    return true;

                } else if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.CLutElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem) {

                    a = stages[0];
                    clut = stages[1];
                    b = stages[2];

                    return true;

                } else {
                    return false;
                }

            case 5:

                if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.CLutElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem &&
                    stages[3].Type == Signature.Stage.MatrixElem &&
                    stages[4].Type == Signature.Stage.CurveSetElem) {

                    a = stages[0];
                    clut = stages[1];
                    m = stages[2];
                    matrix = stages[3];
                    b = stages[4];

                    return true;

                } else {
                    return false;
                }

            default:

                return false;
        }
    }

    public void Dispose() => throw new NotImplementedException();
    public object Clone() => throw new NotImplementedException();

    private static void LutEval16(in ushort[] @in, ushort[] @out, in object? d)
    {
        throw new NotImplementedException();
    }

    private static void LutEvalFloat(in float[] @in, float[] @out, in object? d)
    {
        throw new NotImplementedException();
    }

    private bool BlessLut()
    {
        throw new NotImplementedException();
    }
}
