using lcms2.state;

namespace lcms2.types;

public partial class Stage
{
    private Stage(object? state, Signature type, Signature implements, uint inputChannels, uint outputChannels, StageData data)
    {
        StateContainer = state;
        Type = type;
        this.implements = implements;
        InputChannels = inputChannels;
        OutputChannels = outputChannels;
        Data = data;
    }

    public static Stage? AllocIdentity(object? state, uint numChannels) =>
        Alloc(state, Signature.Stage.IdentityElem, numChannels, numChannels, new IdentityData());

    private static Stage? AllocIdentityCurves(object? state, uint numChannels)
    {
        var mpe = AllocToneCurves(state, numChannels, null);
        if (mpe is null) return null;

        mpe.implements = Signature.Stage.IdentityElem;
        return mpe;
    }

    public static Stage? AllocCLut16bit(object? state, uint numGridPoints, uint inputChan, uint outputChan, in ushort[]? table) =>
        AllocCLut16bit(state, Enumerable.Repeat(numGridPoints, maxInputDimensions).ToArray(), inputChan, outputChan, table);

    public static Stage? AllocCLut16bit(object? state, uint[] clutPoints, uint inputChan, uint outputChan, in ushort[]? table)
    {
        if (inputChan > maxInputDimensions)
        {
            State.SignalError(state, ErrorCode.Range, $"Too many input channels ({inputChan} channels, max={maxInputDimensions})");
            return null;
        }

        uint n = outputChan * CubeSize(clutPoints, inputChan);
        var newTable = ((ushort[]?)table?.Clone()) ?? new ushort[n];

        var p =
            InterpParams.Compute(
                state,
                clutPoints,
                inputChan,
                outputChan,
                newTable,
                LerpFlag.Ushort);
        if (p is null) return null;

        var newElem =
            new CLutData(
                new CLutData.Tab() { T = newTable },
                p,
                n,
                false);

        if (n is 0) return null;

        return
            Alloc(
                state,
                Signature.Stage.CLutElem,
                inputChan,
                outputChan,
                newElem);
    }

    public static Stage? AllocCLutFloat(object? state, uint numGridPoints, uint inputChan, uint outputChan, in float[]? table) =>
        AllocCLutFloat(state, Enumerable.Repeat(numGridPoints, maxInputDimensions).ToArray(), inputChan, outputChan, table);

    public static Stage? AllocCLutFloat(object? state, uint[] clutPoints, uint inputChan, uint outputChan, in float[]? table)
    {
        if (inputChan > maxInputDimensions)
        {
            State.SignalError(state, ErrorCode.Range, $"Too many input channels ({inputChan} channels, max={maxInputDimensions})");
            return null;
        }

        var p =
            InterpParams.Compute(
                state,
                clutPoints,
                inputChan,
                outputChan,
                table ?? Array.Empty<float>(),
                LerpFlag.Ushort);
        if (p is null) return null;

        uint n = outputChan * CubeSize(clutPoints, inputChan);
        var newElem =
            new CLutData(
                new CLutData.Tab() { TFloat = ((float[]?)table?.Clone()) ?? new float[n]},
                p,
                n,
                true);

        if (n is 0) return null;

        return
            Alloc(
                state,
                Signature.Stage.CLutElem,
                inputChan,
                outputChan,
                newElem);
    }

    internal static Stage? AllocIdentityCLut(object? state, uint numChan)
    {
        var dims = Enumerable.Repeat(2u,maxInputDimensions).ToArray();

        var mpe = AllocCLut16bit(state, dims, numChan, numChan, null);
        if (mpe is null) return null;

        if (!((CLutData)mpe.Data).Sample(IdentitySampler, (int)numChan, SamplerFlags.None))
        {
            mpe.Dispose();
            return null;
        }

        mpe.implements = Signature.Stage.IdentityElem;
        return mpe;
    }

    internal static Stage? AllocLab2XYZ(object? state) =>
        Alloc(state, Signature.Stage.Lab2XYZElem, 3, 3, new Lab2XYZData());

    internal static Stage? AllocLabPrelin(object? state)
    {
        var labTable = new ToneCurve[3];
        var @params = new double[] {2.4};

        labTable[0] = ToneCurve.BuildGamma(state, 1.0)!;
        labTable[1] = ToneCurve.BuildParametric(state, 108, @params)!;
        labTable[2] = ToneCurve.BuildParametric(state, 108, @params)!;

        if (labTable.Contains(null)) return null;

        return AllocToneCurves(state, 3, labTable);
    }

    internal static Stage? AllocLabV2ToV4Curves(object? state)
    {
        var labTable = new ToneCurve[3];

        labTable[0] = ToneCurve.BuildTabulated16(state, 258, null)!;
        labTable[1] = ToneCurve.BuildTabulated16(state, 258, null)!;
        labTable[2] = ToneCurve.BuildTabulated16(state, 258, null)!;

        if (labTable.Contains(null))
        {
            ToneCurve.DisposeTriple(labTable!);
            return null;
        }

        Array.ForEach(
            labTable,
            l => Array.ForEach(
                Enumerable.Range(0, 257)
                          .ToArray(),
                i => l!.table16[i] = (ushort)(((i * 0xFFFF) + 0x80) >> 8)));
        var mpe = AllocToneCurves(state, 3, labTable);
        ToneCurve.DisposeTriple(labTable!);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.LabV2toV4Elem;
        return mpe;
    }

    internal static Stage? AllocLabV2ToV4(object? state)
    {
        var v2ToV4 = new double[]
        {
            65535.0 / 65280.0, 0, 0,
            0, 65535.0 / 65280.0, 0,
            0, 0, 65535.0 / 65280.0
        };

        var mpe = AllocMatrix(state, 3, 3, v2ToV4, null);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.LabV2toV4Elem;
        return mpe;
    }

    internal static Stage? AllocLabV4ToV2(object? state)
    {
        var v4ToV2 = new double[]
        {
            65280.0 / 65535.0, 0, 0,
            0, 65280.0 / 65535.0, 0,
            0, 0, 65280.0 / 65535.0
        };

        var mpe = AllocMatrix(state, 3, 3, v4ToV2, null);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.LabV4toV2Elem;
        return mpe;
    }

    internal static Stage? AllocXyz2Lab(object? state) =>
        Alloc(state, Signature.Stage.XYZ2LabElem, 3, 3, new XYZ2LabData());

    internal static Stage? NormalizeFromLabFloat(object? state)
    {
        var a1 = new double[]
        {
            1.0 / 100.0, 0, 0,
            0, 1.0 / 255.0, 0,
            0, 0, 1.0 / 255.0
        };

        var o1 = new double[]
        {
            0,
            128.0/255.0,
            128.0/255.0
        };

        var mpe = AllocMatrix(state, 3, 3, a1, o1);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.Lab2FloatPCS;
        return mpe;
    }

    internal static Stage? NormalizeFromXyzFloat(object? state)
    {
        const double n = 32768.0 / 65535.0;

        var a1 = new double[]
        {
            n, 0, 0,
            0, n, 0,
            0, 0, n
        };

        var mpe = AllocMatrix(state, 3, 3, a1, null);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.XYZ2FloatPCS;
        return mpe;
    }

    internal static Stage? NormalizeToLabFloat(object? state)
    {
        var a1 = new double[]
        {
            100.0, 0, 0,
            0, 255.0, 0,
            0, 0, 255.0
        };

        var o1 = new double[]
        {
            0,
            -128.0,
            -128.0
        };

        var mpe = AllocMatrix(state, 3, 3, a1, o1);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.FloatPCS2Lab;
        return mpe;
    }

    internal static Stage? NormalizeToXyzFloat(object? state)
    {
        const double n = 65535.0 / 32768.0;

        var a1 = new double[]
        {
            n, 0, 0,
            0, n, 0,
            0, 0, n
        };

        var mpe = AllocMatrix(state, 3, 3, a1, null);

        if (mpe is null) return null;
        mpe.implements = Signature.Stage.FloatPCS2XYZ;
        return mpe;
    }

    internal static Stage? ClipNegatives(object? state, uint numChannels) =>
        Alloc(state, Signature.Stage.ClipNegativesElem, numChannels, numChannels, new ClipperData());

    public static Stage? AllocMatrix(object? state, uint rows, uint cols, in double[] matrix, double[]? offset)
    {
        var n = rows * cols;

        // Check for overflow
        if (n is 0 || n < rows || n < cols ||
            n >= UInt32.MaxValue / cols ||
            n >= UInt32.MaxValue / rows)
        {
            return null;
        }

        var newElem = new MatrixData((double[])matrix.Clone(), (double[]?)offset?.Clone());
        return Alloc(state, Signature.Stage.MatrixElem, cols, rows, newElem);
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
    public static Stage? Alloc(object? state,
                               Signature type,
                               uint inputChannels,
                               uint outputChannels,
                               StageData? data)
    {
        if (inputChannels > maxStageChannels ||
            outputChannels > maxStageChannels)
        {
            return null;
        }

        return new(state, type, type, inputChannels, outputChannels, data);
    }


    public static Stage? AllocToneCurves(object? state, uint numChannels, ToneCurve[]? curves)
    {
        var newMpe = Alloc(state, Signature.Stage.CurveSetElem, numChannels, numChannels, new ToneCurveData());
        if (newMpe is null) return null;

        ToneCurveData newElem;
        if (curves is not null)
            newElem = new ToneCurveData(curves.Select(t => (ToneCurve)t.Clone()).ToArray());
        else
        {
            var array = new ToneCurve[numChannels];
            for (var i = 0; i < numChannels; i++)
            {
                var t = ToneCurve.BuildGamma(state, 1.0);
                if (t is null) return null;
                array[i] = t;
            }
            newElem = new ToneCurveData(array);
        }

        return newMpe;
    }

    private static uint CubeSize(in uint[] dims, uint b) =>
        dims.Take((int)b).Reverse().Aggregate(1u, (rv, dim) =>
        {
            if (rv is 0 || dim is 0) return 0;     // Error

            var result = rv *= dim;

            // Check for overflow
            if (result > UInt32.MaxValue / dim) return 0;

            return result;
        });

    private static bool IdentitySampler(in ushort[] @in, ushort[]? @out, in object? cargo)
    {
        if (cargo is null) return false;
        var numChan = (int)cargo;
        for (var i = 0; i < numChan; i++)
            @out![i] = @in[i];

        return true;
    }
}
