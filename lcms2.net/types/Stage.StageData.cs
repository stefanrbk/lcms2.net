namespace lcms2.types;

public partial class Stage
{
    public abstract class StageData
    {
        internal abstract void Evaluate(in float[] @in, float[] @out, Stage parent);
        internal abstract void Free();
        internal abstract StageData? Duplicate(Stage parent);
    }
}