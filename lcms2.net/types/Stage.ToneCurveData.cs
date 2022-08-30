namespace lcms2.types;

public partial class Stage
{
    /// <summary>
    ///     Data kept in "Element" member of <see cref="Stage"/>
    /// </summary>
    /// <remarks>Implements the <c>_cmsStageToneCurvesData</c> struct.</remarks>
    public class ToneCurveData: StageData
    {
        public ToneCurve[] TheCurves;

        internal ToneCurveData(ToneCurve[]? theCurves = null) =>
            TheCurves = theCurves ?? Array.Empty<ToneCurve>();

        public int NumCurves =>
            TheCurves.Length;

        internal override void Evaluate(in float[] @in, float[] @out, Stage _)
        {
            var _in = @in;

            TheCurves.Select((t, i) => t.Eval(_in[i])).ToArray().CopyTo(@out.AsSpan());
        }

        internal override void Free() =>
            _ = TheCurves.All(t =>
                {
                    t.Dispose();
                    return true;
                });

        internal override StageData? Duplicate(Stage _) =>
            new ToneCurveData(TheCurves.Select(t => (ToneCurve)t.Clone())
                                       .ToArray());
    }
}