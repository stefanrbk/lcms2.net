namespace lcms2.types;
public record XYZ(double X, double Y, double Z)
{
    public static implicit operator XYZ((double, double, double) v)
    {
        return new XYZ(v.Item1, v.Item2, v.Item3);
    }
}
