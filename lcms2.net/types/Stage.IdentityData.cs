namespace lcms2.types;

public partial class Stage
{
    public class IdentityData: StageData
    {
        internal override StageData? Duplicate(Stage _) => null;
        internal override void Evaluate(in float[] @in, float[] @out, Stage _) =>
            @in.CopyTo(@out.AsSpan());
        internal override void Free() { }
    }
}