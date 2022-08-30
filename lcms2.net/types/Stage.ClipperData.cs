namespace lcms2.types;

public sealed partial class Stage
{
    internal class ClipperData: StageData
    {
        internal override StageData? Duplicate(Stage parent) =>
            new ClipperData();

        internal override void Evaluate(in float[] @in, float[] @out, Stage parent) =>
            @in
                .Take((int)parent.InputChannels)
                .Select(n => n < 0 ? 0 : n)
                .ToArray()
                .CopyTo(@out.AsSpan());

        internal override void Free() { }
    }
}
