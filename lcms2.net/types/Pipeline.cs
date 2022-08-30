namespace lcms2.types;

/// <summary>
///     Pipeline evaluator (in 16 bits)
/// </summary>
/// <remarks>Implements the <c>_cmsPipelineEval16Fn</c> typedef.</remarks>
public delegate void PipelineEval16Fn(in ushort[] @in, ushort[] @out, in object data);

/// <summary>
///     Pipeline evaluator (in floating point)
/// </summary>
/// <remarks>Implements the <c>_cmsPipelineEvalFloatFn</c> typedef.</remarks>
public delegate void PipelineEvalFloatFn(in float[] @in, float[] @out, in object data);

public class Pipeline: ICloneable, IDisposable
{
    public object? StateContainer { get; internal set; }

    internal object? data;

    internal DupUserDataFn? dupDataFn;

    internal Stage? elements;

    internal PipelineEval16Fn? eval16Fn;

    internal PipelineEvalFloatFn? evalFloatFn;

    internal FreeUserDataFn? freeDataFn;

    public uint InputChannels { get; internal set; }
    public uint OutputChannels { get; internal set; }

    public bool SaveAs8Bits { get; internal set; }

    private bool _disposedValue;

    private const float _jacobianEpsilon = 0.001f;
    private const int _inversionMaxIterations = 30;

    internal Pipeline(Stage? elements,
                      uint inputChannels,
                      uint outputChannels,
                      object? data,
                      PipelineEval16Fn? eval16Fn,
                      PipelineEvalFloatFn? evalFloatFn,
                      FreeUserDataFn? freeDataFn,
                      DupUserDataFn? dupDataFn,
                      object? state,
                      bool saveAs8Bits)
    {
        this.elements = elements;
        this.InputChannels = inputChannels;
        this.OutputChannels = outputChannels;
        this.data = data;
        this.eval16Fn = eval16Fn;
        this.evalFloatFn = evalFloatFn;
        this.freeDataFn = freeDataFn;
        this.dupDataFn = dupDataFn;
        this.StateContainer = state;
        this.SaveAs8Bits = saveAs8Bits;
    }

    public uint StageCount
    {
        get
        {
            Stage? mpe;
            uint n;

            for (n = 0, mpe = elements; mpe is not null; mpe = mpe.Next)
                n++;

            return n;
        }
    }

    public Stage? FirstStage =>
        elements;

    public Stage? LastStage
    {
        get
        {
            Stage? anterior = null;
            for (var mpe = elements; mpe is not null; mpe = mpe.Next)
                anterior = mpe;

            return anterior;
        }
    }

    public static Pipeline? Alloc(object? context, uint inputChannels, uint outputChannels)
    {
        // A value of zero in channels is allowed as placeholder
        if (inputChannels >= maxChannels ||
            outputChannels >= maxChannels) return null;

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

    public bool Concat(Pipeline l2)
    {
        // If both LUTS does not have elements, we need to inherit
        // the number of channels
        if (elements is null && l2.elements is null)
        {
            InputChannels = l2.InputChannels;
            OutputChannels = l2.OutputChannels;
        }

        // Concat second
        for (var mpe = l2.elements; mpe is not null; mpe = mpe.Next)
            // We have to dup each element
            if (!InsertStage(StageLoc.AtEnd, (Stage)mpe.Clone()))
                return false;

        return BlessLut();
    }

    public bool CheckAndRetreiveStagesAtoB(out Stage? a, out Stage? clut, out Stage? m, out Stage? matrix, out Stage? b)
    {
        a = null;
        clut = null;
        m = null;
        matrix = null;
        b = null;

        var stages = new List<Stage>(5);

        for (var mpe = elements; mpe is not null; mpe = mpe.Next)
            stages.Add(mpe);

        switch (stages.Count)
        {
            case 1:

                if (stages[0].Type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    return true;
                } else
                {
                    return false;
                }

            case 3:

                if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.MatrixElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem)
                {
                    m = stages[0];
                    matrix = stages[1];
                    b = stages[2];

                    return true;
                } else if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.CLutElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem)
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

                if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.CLutElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem &&
                    stages[3].Type == Signature.Stage.MatrixElem &&
                    stages[4].Type == Signature.Stage.CurveSetElem)
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

        for (var mpe = elements; mpe is not null; mpe = mpe.Next)
            stages.Add(mpe);

        switch (stages.Count)
        {
            case 1:

                if (stages[0].Type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    return true;
                } else
                {
                    return false;
                }

            case 3:

                if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.MatrixElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem)
                {
                    b = stages[0];
                    matrix = stages[1];
                    m = stages[2];

                    return true;
                } else if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.CLutElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem)
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

                if (stages[0].Type == Signature.Stage.CurveSetElem &&
                    stages[1].Type == Signature.Stage.CLutElem &&
                    stages[2].Type == Signature.Stage.CurveSetElem &&
                    stages[3].Type == Signature.Stage.MatrixElem &&
                    stages[4].Type == Signature.Stage.CurveSetElem)
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

    public object Clone()
    {
        Stage? anterior = null;
        var first = true;

        var newLut = Alloc(StateContainer, InputChannels, OutputChannels);
        if (newLut is null) return null!;

        for (var mpe = elements; mpe is not null; mpe = mpe.Next)
        {
            var newMpe = (Stage)mpe.Clone();
            if (newMpe is null)
            {
                newLut.Dispose();
                return null!;
            }

            if (first)
            {
                newLut.elements = newMpe;
                first = false;
            }
            else if (anterior is not null)
            {
                anterior.Next = newMpe;
            }

            anterior = newMpe;
        }

        newLut.eval16Fn = eval16Fn;
        newLut.evalFloatFn = evalFloatFn;
        newLut.dupDataFn = dupDataFn;
        newLut.freeDataFn = freeDataFn;

        if (newLut.dupDataFn is not null)
            newLut.data = newLut.dupDataFn?.Invoke(StateContainer, data);

        newLut.SaveAs8Bits = SaveAs8Bits;

        if (!newLut.BlessLut())
        {
            newLut.Dispose();
            return null!;
        }

        return newLut;
    }

    public void Eval(in ushort[] @in, ushort[] @out) =>
        eval16Fn?.Invoke(@in, @out, this);

    public void Eval(in float[] @in, float[] @out) =>
        evalFloatFn?.Invoke(@in, @out, this);

    public bool EvalReverse(float[] target, float[] result, float[]? hint)
    {
        var lastError = 1e20;
        var fx = new float[4];
        var x = new float[4];
        var xd = new float[4];
        var fxd = new float[4];

        // Only 3->3 and 4->3 are supported
        if (InputChannels is not 3 and not 4 || OutputChannels is not 3)
            return false;

        // Take the hint as starting point if specified
        if (hint is null)
        {
            // Begin at any point, we choose 1/3 of CMY axis
            x[0] = x[1] = x[2] = 0.3f;
        } else
        {
            // Only copy 3 channels from hint...
            for (var j = 0; j < 3; j++)
                x[j] = hint[j];
        }

        // If Lut is 4-dimensional, then grab target[3], which is fixed
        if (InputChannels is 4)
            x[3] = target[3];

        // Iterate
        for (var i = 0; i < _inversionMaxIterations; i++)
        {
            // Get beginning fx
            Eval(x, fx);

            // Compute error
            var error = EuclideanDistance(fx, target, 3);

            // If not convergent, return last safe value
            if (error >= lastError) break;

            // Keep latest values
            lastError = error;
            for (var j = 0; j < InputChannels; j++)
                result[j] = x[j];

            // Found an exact match?
            if (error <= 0) break;

            var jacobian = new Mat3();

            // Obtain slope (the Jacobian)
            for (var j = 0; j < 3; j++)
            {
                x.CopyTo(xd.AsSpan());

                IncDelta(ref xd[j]);

                Eval(xd, fxd);

                jacobian.X[0] = (fxd[0] - fx[0]) / _jacobianEpsilon;
                jacobian.X[1] = (fxd[1] - fx[1]) / _jacobianEpsilon;
                jacobian.X[2] = (fxd[2] - fx[2]) / _jacobianEpsilon;
            }

            // Solve system
            var tmp = new Vec3(fx[0] - target[0], fx[1] - target[1], fx[2] - target[2]);
            var tmp2 = jacobian.Solve(tmp);
            if (tmp2 is null) return false;

            // Move our guess
            x[0] -= (float)tmp2.Value[0];
            x[1] -= (float)tmp2.Value[1];
            x[2] -= (float)tmp2.Value[2];

            // Some clipping...
            for (var j = 0; j < 3; j++)
            {
                if (x[j] < 0) x[j] = 0;
                else if (x[j] > 1) x[j] = 1;
            }
        }

        return true;
    }

    public bool InsertStage(StageLoc loc, Stage? mpe)
    {
        Stage? anterior = null;

        if (mpe is null) return false;

        switch (loc)
        {
            case StageLoc.AtBegin:
                mpe.Next = elements;
                elements = mpe;
                break;
            case StageLoc.AtEnd:
                if (elements is null)
                    elements = mpe;
                else
                {
                    for (var pt = elements; pt is not null; pt = pt.Next)
                        anterior = pt;

                    anterior!.Next = mpe;
                    mpe.Next = null;
                }
                break;
            default:
                return false;
        }

        return BlessLut();
    }

    /// <summary>
    ///     This function may be used to set the optional evaluator and a block of private data. If
    ///     private data is being used, an optional duplicator and free functions should also be
    ///     specified in order to duplicate the LUT construct. Use <see langword="null"/> to inhibit
    ///     such functionality.
    /// </summary>
    /// <remarks>Implements the <c>_cmsPipelineSetOptimizationParameters</c> function.</remarks>
    public void SetOptimizationParameters(PipelineEval16Fn? eval16,
                                          object? privateData,
                                          FreeUserDataFn? freePrivateDataFn,
                                          DupUserDataFn? dupPrivateDataFn)
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
    public void SetOptimizationParameters(PipelineEvalFloatFn? evalFloat,
                                          object? privateData,
                                          FreeUserDataFn? freePrivateDataFn,
                                          DupUserDataFn? dupPrivateDataFn)
    {
        eval16Fn = null;
        evalFloatFn = evalFloat;
        data = privateData;
        freeDataFn = freePrivateDataFn;
        dupDataFn = dupPrivateDataFn;
    }

    public Stage? UnlinkStage(StageLoc loc)
    {
        Stage? unlinked = null;

        // If empty LUT, there is nothing to remove
        if (elements is null)
            return null;

        // On depending on the strategy...
        switch (loc)
        {
            case StageLoc.AtBegin:
                var elem = elements;

                elements = elem.Next;
                elem.Next = null;
                unlinked = elem;
                break;

            case StageLoc.AtEnd:
                Stage? anterior = null, last = null;
                for (var pt = elements; pt is not null; pt = pt.Next)
                {
                    anterior = last;
                    last = pt;
                }

                unlinked = last;    // Next already points to null

                // Truncate the chain
                if (anterior is not null)
                    anterior.Next = null;
                else
                    elements = null;
                break;
        }

        BlessLut();

        return unlinked;
    }

    private static void LutEval16(in ushort[] @in, ushort[] @out, in object d)
    {
        var lut = (Pipeline)d;

        var storage = new float[][]
        {
            new float[maxStageChannels],
            new float[maxStageChannels]
        };

        var phase = 0;

        From16ToFloat(@in.Take((int)lut.InputChannels).ToArray(), storage[phase]);

        for (var mpe = lut.elements; mpe is not null; mpe = mpe.Next)
        {
            var nextPhase = phase ^ 1;
            mpe.Data.Evaluate(storage[phase], storage[nextPhase], mpe);
            phase = nextPhase;
        }

        FromFloatTo16(storage[phase].Take((int)lut.InputChannels).ToArray(), @out);
    }

    private static void LutEvalFloat(in float[] @in, float[] @out, in object d)
    {
        var lut = (Pipeline)d;

        var storage = new float[][]
        {
            new float[maxStageChannels],
            new float[maxStageChannels]
        };

        var phase = 0;

        @in.Take((int)lut.InputChannels).ToArray().CopyTo(storage[phase].AsSpan());

        for (var mpe = lut.elements; mpe is not null; mpe = mpe.Next)
        {
            var nextPhase = phase ^ 1;
            mpe.Data.Evaluate(storage[phase], storage[nextPhase], mpe);
            phase = nextPhase;
        }
        storage[phase].Take((int)lut.OutputChannels).ToArray().CopyTo(@out.AsSpan());
    }

    private bool BlessLut()
    {
        if (elements is not null)
        {
            var first = FirstStage;
            var last = LastStage;

            if (first is null || last is null) return false;

            InputChannels = first.InputChannels;
            OutputChannels = last.OutputChannels;

            // Check chain consistency
            var prev = first;
            var next = prev.Next;

            while (next is not null)
            {
                if (next.InputChannels != prev.OutputChannels)
                    return false;

                next = next.Next;
                prev = prev.Next;
            }
        }
        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                for (Stage? mpe = elements, next = null; mpe is not null; mpe = mpe.Next)
                {
                    next = mpe.Next;
                    mpe.Dispose();
                }
                if (freeDataFn is not null)
                    freeDataFn(StateContainer, ref data!);
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static void IncDelta(ref float val)
    {
        if (val < (1.0 - _jacobianEpsilon))
            val += _jacobianEpsilon;
        else
            val -= _jacobianEpsilon;
    }

    private static float EuclideanDistance(float[] a, float[] b, int n)
    {
        var sum = 0f;
        for (var i = 0; i < n; i++)
        {
            var dif = b[i] - a[i];
            sum += dif * dif;
        }

        return MathF.Sqrt(sum);
    }
}
