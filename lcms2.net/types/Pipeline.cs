using lcms2.state;

namespace lcms2.types;

/// <summary>
///     Pipeline evaluator (in 16 bits)
/// </summary>
/// <remarks>Implements the <c>_cmsPipelineEval16Fn</c> typedef.</remarks>
public delegate void PipelineEval16Fn(in ushort[] @in, ushort[] @out, in object? data);

/// <summary>
///     Pipeline evaluator (in floating point)
/// </summary>
/// <remarks>Implements the <c>_cmsPipelineEvalFloatFn</c> typedef.</remarks>
public delegate void PipelineEvalFloatFn(in float[] @in, float[] @out, in object? data);

public class Pipeline: ICloneable, IDisposable
{
    internal Context? context;

    internal object? data;

    internal DupUserDataFn? dupDataFn;

    internal Stage? elements;

    internal PipelineEval16Fn? eval16Fn;

    internal PipelineEvalFloatFn? evalFloatFn;

    internal FreeUserDataFn? freeDataFn;

    internal uint inputChannels, outputChannels;

    internal bool saveAs8Bits;

    internal Pipeline(Stage? elements, uint inputChannels, uint outputChannels, object? data, PipelineEval16Fn? eval16Fn, PipelineEvalFloatFn? evalFloatFn, FreeUserDataFn? freeDataFn, DupUserDataFn? dupDataFn, Context? context, bool saveAs8Bits)
    {
        this.elements = elements;
        this.inputChannels = inputChannels;
        this.outputChannels = outputChannels;
        this.data = data;
        this.eval16Fn = eval16Fn;
        this.evalFloatFn = evalFloatFn;
        this.freeDataFn = freeDataFn;
        this.dupDataFn = dupDataFn;
        this.context = context;
        this.saveAs8Bits = saveAs8Bits;
    }

    public uint StageCount
    {
        get
        {
            Stage? mpe;
            uint n;

            for (n = 0, mpe = elements; mpe is not null; mpe = mpe.next)
                n++;

            return n;
        }
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
        newLut.data = newLut;

        if (!newLut.BlessLut())
        {
            newLut.Dispose();
            return null;
        }

        return newLut;
    }

    public bool CheckAndRetreiveStagesAtoB(out Stage? a, out Stage? clut, out Stage? m, out Stage? matrix, out Stage? b)
    {
        a = null;
        clut = null;
        m = null;
        matrix = null;
        b = null;

        var stages = new List<Stage>(5);

        for (var mpe = elements; mpe is not null; mpe = mpe.next)
            stages.Add(mpe);

        switch (stages.Count)
        {
            case 1:

                if (stages[0].type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    return true;
                } else
                {
                    return false;
                }

            case 3:

                if (stages[0].type == Signature.Stage.CurveSetElem &&
                    stages[1].type == Signature.Stage.MatrixElem &&
                    stages[2].type == Signature.Stage.CurveSetElem)
                {
                    m = stages[0];
                    matrix = stages[1];
                    b = stages[2];

                    return true;
                } else if (stages[0].type == Signature.Stage.CurveSetElem &&
                    stages[1].type == Signature.Stage.CLutElem &&
                    stages[2].type == Signature.Stage.CurveSetElem)
                {
                    a = stages[0];
                    clut = stages[1];
                    b = stages[2];

                    return true;
                } else
                {
                    return false;
                }

            case 5:

                if (stages[0].type == Signature.Stage.CurveSetElem &&
                    stages[1].type == Signature.Stage.CLutElem &&
                    stages[2].type == Signature.Stage.CurveSetElem &&
                    stages[3].type == Signature.Stage.MatrixElem &&
                    stages[4].type == Signature.Stage.CurveSetElem)
                {
                    a = stages[0];
                    clut = stages[1];
                    m = stages[2];
                    matrix = stages[3];
                    b = stages[4];

                    return true;
                } else
                {
                    return false;
                }

            default:

                return false;
        }
    }

    public bool CheckAndRetrieveStagesBtoA(out Stage? b, out Stage? matrix, out Stage? m, out Stage? clut, out Stage? a)
    {
        a = null;
        clut = null;
        m = null;
        matrix = null;
        b = null;

        var stages = new List<Stage>(5);

        for (var mpe = elements; mpe is not null; mpe = mpe.next)
            stages.Add(mpe);

        switch (stages.Count)
        {
            case 1:

                if (stages[0].type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    return true;
                } else
                {
                    return false;
                }

            case 3:

                if (stages[0].type == Signature.Stage.CurveSetElem &&
                    stages[1].type == Signature.Stage.MatrixElem &&
                    stages[2].type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    matrix = stages[1];
                    m = stages[2];

                    return true;
                } else if (stages[0].type == Signature.Stage.CurveSetElem &&
                    stages[1].type == Signature.Stage.CLutElem &&
                    stages[2].type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    clut = stages[1];
                    a = stages[2];

                    return true;
                } else
                {
                    return false;
                }

            case 5:

                if (stages[0].type == Signature.Stage.CurveSetElem &&
                    stages[1].type == Signature.Stage.CLutElem &&
                    stages[2].type == Signature.Stage.CurveSetElem &&
                    stages[3].type == Signature.Stage.MatrixElem &&
                    stages[4].type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    matrix = stages[1];
                    m = stages[2];
                    clut = stages[3];
                    a = stages[4];

                    return true;
                } else
                {
                    return false;
                }

            default:

                return false;
        }
    }

    public object Clone() => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();

    public bool InsertStage(StageLoc loc, Stage? mpe)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     This function may be used to set the optional evaluator and a block of private data. If
    ///     private data is being used, an optional duplicator and free functions should also be
    ///     specified in order to duplicate the LUT construct. Use <see langword="null"/> to inhibit
    ///     such functionality.
    /// </summary>
    /// <remarks>Implements the <c>_cmsPipelineSetOptimizationParameters</c> function.</remarks>
    public void SetOptimizationParameters(PipelineEval16Fn? eval16, object? privateData, FreeUserDataFn? freePrivateDataFn, DupUserDataFn? dupPrivateDataFn)
    {
        eval16Fn = eval16;
        evalFloatFn = null;
        data = privateData;
        freeDataFn = freePrivateDataFn;
        dupDataFn = dupPrivateDataFn;
    }

    /// <summary>
    ///     This function may be used to set the optional evaluator and a block of private data. If
    ///     private data is being used, an optional duplicator and free functions should also be
    ///     specified in order to duplicate the LUT construct. Use <see langword="null"/> to inhibit
    ///     such functionality.
    /// </summary>
    public void SetOptimizationParameters(PipelineEvalFloatFn? evalFloat, object? privateData, FreeUserDataFn? freePrivateDataFn, DupUserDataFn? dupPrivateDataFn)
    {
        eval16Fn = null;
        evalFloatFn = evalFloat;
        data = privateData;
        freeDataFn = freePrivateDataFn;
        dupDataFn = dupPrivateDataFn;
    }

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
