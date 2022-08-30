namespace lcms2.types;

public partial class Stage
{
    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>Implements the <c>_cmsStageMatrixData</c> struct.</remarks>
    public class MatrixData: StageData
    {
        public double[] Double;
        public double[]? Offset;

        internal MatrixData(double[] @double, double[]? offset)
        {
            Double = @double;
            Offset = offset;
        }

        internal override StageData? Duplicate(Stage _) =>
            new MatrixData((double[])Double.Clone(), (double[]?)Offset?.Clone());
        internal override void Evaluate(in float[] @in, float[] @out, Stage parent)
        {
            for (var i = 0; i < parent.OutputChannels; i++)
            {
                var tmp = 0.0;
                for (var j = 0; j < parent.InputChannels; j++)
                    tmp += @in[j] * Double[(i * parent.InputChannels) + j];

                if (Offset is not null)
                    tmp += Offset[i];

                @out[i] = (float)tmp;
            }
        }
        internal override void Free() { }
    }
}