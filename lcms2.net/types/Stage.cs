using System.Runtime.InteropServices;

using lcms2.state;

namespace lcms2.types;

/// <summary>
///     Stage element duplication function
/// </summary>
/// <remarks>Implements the <c>_cmsStageDupElemFn</c> typedef.</remarks>
public delegate Stage? StageDupElemFn(ref Stage mpe);

/// <summary>
///     Stage evaluation function
/// </summary>
/// <remarks>Implements the <c>_cmsStageEvalFn</c> typedef.</remarks>
public delegate void StageEvalFn(in float[] @in, float[] @out, in Stage mpe);

/// <summary>
///     Stage element free function
/// </summary>
/// <remarks>Implements the <c>_cmsStageFreeElemFn</c> typedef.</remarks>
public delegate void StageFreeElemFn(ref Stage mpe);

public class Stage: ICloneable, IDisposable
{
    internal object? state;

    internal object data;

    internal StageDupElemFn dupElemPtr;

    internal StageEvalFn evalPtr;

    internal StageFreeElemFn freeElemPtr;

    internal Signature implements;

    internal int inputChannels;

    internal Stage? next;

    internal int outputChannels;

    internal Signature type;

    private Stage(object? state, Signature type, Signature implements, int inputChannels, int outputChannels, StageEvalFn evalPtr, StageDupElemFn dupElemPtr, StageFreeElemFn freeElemPtr, object data)
    {
        this.state = state;
        this.type = type;
        this.implements = implements;
        this.inputChannels = inputChannels;
        this.outputChannels = outputChannels;
        this.evalPtr = evalPtr;
        this.dupElemPtr = dupElemPtr;
        this.freeElemPtr = freeElemPtr;
        this.data = data;
    }

    internal ToneCurve[] CurveSet =>
        (data as ToneCurveData)?.TheCurves ?? throw new InvalidOperationException();

    public static Stage AllocCLut16bit(object? state, uint numGridPoints, uint inputChan, uint outputChan, in ushort[]? table)
    {
        throw new NotImplementedException();
    }

    public static Stage AllocCLut16bitGranular(object? state, uint[] clutPoints, uint inputChan, uint outputChan, in ushort[]? table)
    {
        throw new NotImplementedException();
    }

    public static Stage AllocCLutFloatGranular(object? state, uint[] clutPoints, uint inputChan, uint outputChan, in float[]? table)
    {
        throw new NotImplementedException();
    }

    public static Stage AllocMatrix(object? state, uint rows, uint cols, in double[] matrix, double[]? offset)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Allocates an empty multi profile element
    /// </summary>
    /// <param name="evalPtr">
    ///     Points to a function that evaluates the element (always in floating point)
    /// </param>
    /// <param name="dupElemPtr">Points to a function that duplicates the stage</param>
    /// <param name="freePtr">Points to a function that sets the element free</param>
    /// <param name="data">A generic pointer to whatever memory needed by the element</param>
    /// <remarks>Implements the <c>_cmsStageAllocPlaceholder</c> function.</remarks>
    public static Stage AllocPlaceholder(object? state, Signature type, int inputChannels, int outputChannels, StageEvalFn evalPtr, StageDupElemFn dupElemPtr, StageFreeElemFn freePtr, object data) =>
        new(state, type, type, inputChannels, outputChannels, evalPtr, dupElemPtr, freePtr, data);

    public static Stage AllocToneCurves(object? state, uint numChannels, ToneCurve?[] curves)
    {
        throw new NotImplementedException();
    }

    public object Clone() => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();

    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>Implements the <c>_cmsStageCLutData</c> struct.</remarks>
    public class CLutData
    {
        public bool HasFloatValues;
        public int NumEntries;
        public InterpParams[] Params;
        public Tab Table;

        internal CLutData(Tab table, InterpParams[] @params, int numEntries, bool hasFloatValues)
        {
            Table = table;
            Params = @params;
            NumEntries = numEntries;
            HasFloatValues = hasFloatValues;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Tab
        {
            [FieldOffset(0)]
            public ushort[] T;

            [FieldOffset(0)]
            public float[] TFloat;
        }
    }

    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>Implements the <c>_cmsStageMatrixData</c> struct.</remarks>
    public class MatrixData
    {
        public double[] Double;
        public double[]? Offset;

        internal MatrixData(double[] @double, double[]? offset)
        {
            Double = @double;
            Offset = offset;
        }
    }

    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>Implements the <c>_cmsStageToneCurvesData</c> struct.</remarks>
    public class ToneCurveData
    {
        public ToneCurve[] TheCurves;

        internal ToneCurveData(ToneCurve[] theCurves)
        {
            this.TheCurves = theCurves;
        }

        public int NumCurves => TheCurves.Length;
    }
}

public enum StageLoc
{
    AtBegin,
    AtEnd,
}
