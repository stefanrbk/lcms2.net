﻿namespace lcms2.types;
public class CurveSegment
{
    // Domain; for X0 < x <= X1
    public float X0, X1;
    // Parametric type, Type == 0 means sampled segment. Negative values are reserved
    public int Type;
    // Parameters if Type != 0;
    public double[] Params = new double[10];
    // Number of grid points if Type == 0
    public int NumGridPoints =>
        SampledPoints?.Length ?? 0;
    // Points to an array of floats if Type == 0;
    public float[]? SampledPoints;
}
