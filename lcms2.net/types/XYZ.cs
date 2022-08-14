namespace lcms2.types;
public struct XYZ : ICloneable
{
    public double X;
    public double Y;
    public double Z;

    public XYZ(double x, double y, double z) =>
        (X, Y, Z) = (x, y, z);

    public object Clone() =>
        new XYZ(X, Y, Z);

    public static implicit operator XYZ((double, double, double) v)
    {
        return new XYZ(v.Item1, v.Item2, v.Item3);
    }
}
