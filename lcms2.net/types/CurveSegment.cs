namespace lcms2.types;

public class CurveSegment
{
    // Parameters if Type != 0;
    public double[] Params = new double[10];

    // Points to an array of floats if Type == 0;
    public float[]? SampledPoints;

    // Parametric type, Type == 0 means sampled segment. Negative values are reserved
    public int Type;

    // Domain; for X0 < x <= X1
    public float X0, X1;

    // Number of grid points if Type == 0
    public uint NumGridPoints =>
        (uint?)SampledPoints?.Length ?? 0u;
}
