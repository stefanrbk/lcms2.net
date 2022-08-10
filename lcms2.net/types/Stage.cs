using System.Runtime.InteropServices;

using lcms2.plugins;
using lcms2.state;

namespace lcms2.types;

/// <summary>
///     Stage evaluation function
/// </summary>
/// <remarks>
///     Implements the <c>_cmsStageEvalFn</c> typedef.</remarks>
public delegate void StageEvalFn(in float[] @in, float[] @out, in Stage mpe);

/// <summary>
///     Stage element duplication function
/// </summary>
/// <remarks>
///     Implements the <c>_cmsStageDupElemFn</c> typedef.</remarks>
public delegate Stage? StageDupElemFn(ref Stage mpe);

/// <summary>
///     Stage element free function
/// </summary>
/// <remarks>
///     Implements the <c>_cmsStageFreeElemFn</c> typedef.</remarks>
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

    private Stage(Context context, Signature type, Signature implements, int inputChannels, int outputChannels, StageEvalFn evalPtr, StageDupElemFn dupElemPtr, StageFreeElemFn freeElemPtr, object? data)
    {
        Context = context;
        Type = type;
        Implements = implements;
        InputChannels = inputChannels;
        OutputChannels = outputChannels;
        EvalPtr = evalPtr;
        DupElemPtr = dupElemPtr;
        FreeElemPtr = freeElemPtr;
        Data = data;
    }

    /// <summary>
    ///     Allocates an empty multi profile element
    /// </summary>
    /// <param name="evalPtr">
    ///     Points to a function that evaluates the element (always in floating point)</param>
    /// <param name="dupElemPtr">
    ///     Points to a function that duplicates the stage</param>
    /// <param name="freePtr">
    ///     Points to a function that sets the element free</param>
    /// <param name="data">
    ///     A generic pointer to whatever memory needed by the element</param>
    /// <remarks>
    ///     Implements the <c>_cmsStageAllocPlaceholder</c> function.</remarks>
    public static Stage AllocPlaceholder(Context? context, Signature type, int inputChannels, int outputChannels, StageEvalFn evalPtr, StageDupElemFn dupElemPtr, StageFreeElemFn freePtr, object? data) =>
        new(Context.Get(context), type, type, inputChannels, outputChannels, evalPtr, dupElemPtr, freePtr, data);

    public static Stage AllocToneCurves(Context? context, uint numChannels, ToneCurve[] curves)
    {
        throw new NotImplementedException();
    }

    public static Stage AllocMatrix(Context? context, uint rows, uint cols, in double[] matrix, double[]? offset)
    {
        throw new NotImplementedException();
    }

    public static Stage AllocCLut16bit(Context? context, uint numGridPoints, uint inputChan, uint outputChan, in ushort[] table)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsStageToneCurvesData</c> struct.</remarks>
    public class ToneCurveData
    {
        public ToneCurve[] TheCurves;
        public int NumCurves => TheCurves.Length;

        internal ToneCurveData(ToneCurve[] theCurves)
        {
            this.TheCurves = theCurves;
        }
    }

    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsStageMatrixData</c> struct.</remarks>
    public class MatrixData
    {
        public double[] Double;
        public double[] Offset;

        internal MatrixData(double[] @double, double[] offset)
        {
            Double = @double;
            Offset = offset;
        }
    }

    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsStageCLutData</c> struct.</remarks>
    public class CLutData
    {
        public Tab Table;

        public InterpParams[] Params;
        public int NumEntries;
        public bool HasFloatValues;

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
}

public enum StageLoc
{
    AtBegin,
    AtEnd,
}