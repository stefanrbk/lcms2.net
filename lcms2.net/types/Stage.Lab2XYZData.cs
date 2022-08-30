namespace lcms2.types;

public sealed partial class Stage
{
    internal class Lab2XYZData: StageData
    {
        internal override StageData? Duplicate(Stage parent) =>
            new Lab2XYZData();

        internal override void Evaluate(in float[] @in, float[] @out, Stage _)
        {
            const double xyzAdj = maxEncodableXYZ;

            // V4 rules
            var lab = new Lab(
                @in[0] * 100.0,
               (@in[1] * 255.0) - 128.0,
               (@in[2] * 255.0) - 128.0);

            var xyz = lab.ToXYZ();

            // From XYZ, range 0..19997 to 0..1.0, note that 1.99997 comes from 0xffff
            // encoded as 1.15 fixed point, so 1 + (32767.0 / 32768.0)
            @out[0] = (float)(xyz.X / xyzAdj);
            @out[1] = (float)(xyz.Y / xyzAdj);
            @out[2] = (float)(xyz.Z / xyzAdj);
        }
        internal override void Free() { }
    }
}
