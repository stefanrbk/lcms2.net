namespace lcms2.types;

public sealed partial class Stage
{
    internal class XYZ2LabData: StageData
    {
        internal override StageData? Duplicate(Stage parent) =>
            new XYZ2LabData();

        internal override void Evaluate(in float[] @in, float[] @out, Stage _)
        {
            const double xyzAdj = maxEncodableXYZ;

            // From 0..1.0 to XYZ
            var xyz = new XYZ(
                @in[0] * xyzAdj,
                @in[1] * xyzAdj,
                @in[2] * xyzAdj);

            var lab = xyz.ToLab();

            // From V4 Lab to 0..1.0
            @out[0] = (float)(lab.L / 100.0);
            @out[1] = (float)((lab.a + 128.0) / 255.0);
            @out[2] = (float)((lab.b + 128.0) / 255.0);
        }
        internal override void Free() { }
    }
}
