using System.Runtime.InteropServices;

namespace lcms2.types;

public delegate bool Sampler16(in ushort[] @in, ushort[]? @out, in object? cargo);

public delegate bool SamplerFloat(in float[] @in, float[]? @out, in object? cargo);

public partial class Stage
{
    public bool Sample(Sampler16 sampler, in object? cargo, SamplerFlags flags) =>
        (Data as CLutData)?.Sample(sampler, cargo, flags) ?? throw new InvalidOperationException();

    public bool Sample(SamplerFloat sampler, in object? cargo, SamplerFlags flags) =>
        (Data as CLutData)?.Sample(sampler, cargo, flags) ?? throw new InvalidOperationException();

    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>Implements the <c>_cmsStageCLutData</c> struct.</remarks>
    public class CLutData: StageData
    {
        public bool HasFloatValues;
        public uint NumEntries;
        public InterpParams Params;
        public Tab Table;

        public ushort[] Table16 =>
            !HasFloatValues
                ? Table.T
                : throw new InvalidOperationException();

        public float[] TableFloat =>
            HasFloatValues
                ? Table.TFloat
                : throw new InvalidOperationException();

        public bool Sample(Sampler16 sampler, in object? cargo, SamplerFlags flags)
        {
            if (HasFloatValues) return false;

            var @in = new ushort[maxInputDimensions + 1];
            var @out = new ushort[maxStageChannels];

            var numSamples = Params.NumSamples;
            var numInputs = Params.NumInputs;
            var numOutputs = Params.NumOutputs;

            if ((numInputs is <= 0 or > maxInputDimensions) ||
                (numOutputs is <= 0 or > maxStageChannels))
            {
                return false;
            }

            var numTotalPoints = CubeSize(numSamples, numInputs);
            if (numTotalPoints is 0) return false;

            var index = 0u;
            for (var i = 0; i < numTotalPoints; i++)
            {
                var rest = i;
                for (var t = (int)numInputs - 1; t >= 0; --t)
                {
                    var colorant = rest % numSamples[t];

                    rest /= (int)numSamples[t];

                    @in[t] = QuantizeValue(colorant, numSamples[t]);
                }

                for (var t = 0; t < numOutputs; t++)
                    @out[t] = Table16[index + t];

                if (!sampler(in @in, @out, in cargo))
                    return false;

                if (flags is SamplerFlags.None)
                {
                    for (var t = 0; t < numOutputs; t++)
                        Table16[index + t] = @out[t];
                }

                index += numOutputs;
            }

            return true;
        }

        public bool Sample(SamplerFloat sampler, in object? cargo, SamplerFlags flags)
        {
            if (!HasFloatValues) return false;

            var @in = new float[maxInputDimensions + 1];
            var @out = new float[maxInputDimensions];

            var numSamples = Params.NumSamples;
            var numInputs = Params.NumInputs;
            var numOutputs = Params.NumOutputs;

            if ((numInputs is <= 0 or > maxInputDimensions) ||
                (numOutputs is <= 0 or > maxStageChannels))
            {
                return false;
            }

            var numTotalPoints = CubeSize(numSamples, numInputs);
            if (numTotalPoints is 0) return false;

            var index = 0u;
            for (var i = 0; i < numTotalPoints; i++)
            {
                var rest = i;
                for (var t = numInputs - 1; t >= 0; --t)
                {
                    var colorant = rest % numSamples[t];

                    rest /= (int)numSamples[i];

                    @in[i] = QuantizeValue(colorant, numSamples[t]) / 65535f;
                }

                for (var t = 0; t < numOutputs; t++)
                    @out[i] = TableFloat[index + t];

                if (!sampler(in @in, @out, in cargo))
                    return false;

                if (flags is SamplerFlags.None)
                {
                    for (var t = 0; t < numOutputs; t++)
                        TableFloat[index + t] = @out[t];
                }

                index += numOutputs;
            }

            return true;
        }

        public static bool SliceSpace(uint numInputs, in uint[] clutPoints, Sampler16 sampler, in object cargo)
        {
            var @in = new ushort[maxChannels];

            if (numInputs >= maxChannels) return false;

            var numTotalPoints = CubeSize(clutPoints, numInputs);
            if (numTotalPoints is 0) return false;

            for (var i = 0; i < numTotalPoints; i++)
            {
                var rest = i;
                for (var t = numInputs - 1; t >= 0; --t)
                {
                    var colorant = rest % clutPoints[t];

                    rest /= (int)clutPoints[t];
                    @in[t] = QuantizeValue(colorant, clutPoints[t]);
                }

                if (!sampler(in @in, null, in cargo)) return false;
            }

            return true;
        }

        public static bool SliceSpace(uint numInputs, in uint[] clutPoints, SamplerFloat sampler, in object cargo)
        {
            var @in = new float[maxChannels];

            if (numInputs >= maxChannels) return false;

            var numTotalPoints = CubeSize(clutPoints, numInputs);
            if (numTotalPoints is 0) return false;

            for (var i = 0; i < numTotalPoints; i++)
            {
                var rest = i;
                for (var t = numInputs - 1; t >= 0; --t)
                {
                    var colorant = rest % clutPoints[t];

                    rest /= (int)clutPoints[t];
                    @in[t] = QuantizeValue(colorant, clutPoints[t]) / 65535f;
                }

                if (!sampler(in @in, null, in cargo)) return false;
            }

            return true;
        }

        internal CLutData(Tab table, InterpParams @params, uint numEntries, bool hasFloatValues)
        {
            Table = table;
            Params = @params;
            NumEntries = numEntries;
            HasFloatValues = hasFloatValues;
        }

        internal override StageData? Duplicate(Stage parent)
        {
            var p =
                InterpParams.Compute(
                    parent.StateContainer,
                    Params.NumSamples,
                    Params.NumInputs,
                    Params.NumOutputs,
                    HasFloatValues
                        ? Table.TFloat
                        : Table.T,
                    Params.Flags);

            if (p is null) return null;

            return
                new CLutData(
                    HasFloatValues
                        ? new Tab() { TFloat = (float[])TableFloat.Clone() }
                        : new Tab() { T = (ushort[])Table16.Clone() },
                    p,
                    NumEntries,
                    HasFloatValues);
        }

        internal override void Evaluate(in float[] @in, float[] @out, Stage parent)
        {
            if (HasFloatValues)
                EvaluateFloat(in @in, @out, parent);
            else
                EvaluateIn16(in @in, @out, parent);
        }

        private void EvaluateFloat(in float[] @in, float[] @out, Stage _) =>
            Params.LerpFloat(in @in, ref @out);

        private void EvaluateIn16(in float[] @in, float[] @out, Stage parent)
        {
            var in16 = new ushort[parent.InputChannels];
            var out16 = new ushort[parent.OutputChannels];

            FromFloatTo16(in @in, in16);
            Params.Lerp16(in in16, ref out16);
            From16ToFloat(in out16, @out);
        }

        internal override void Free() { }

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

public enum SamplerFlags
{
    None = 0,
    Inspect = 0x01000000,
}