using lcms2.state;

namespace lcms2.types;

public delegate void StageEvalFn(in float[] @in, float[] @out, in Stage mpe);
public delegate Stage? StageDupElemFn(ref Stage mpe);
public delegate void StageFreeElemFn(ref Stage mpe);

public class Stage
{
    internal Context Context;

    internal Signature Type;
    internal Signature Implements;

    internal int InputChannels;
    internal int OutputChannels;

    internal StageEvalFn EvalPtr;
    internal StageDupElemFn DupElemPtr;
    internal StageFreeElemFn FreeElemPtr;

    internal object? Data;

    internal Stage? Next;
}