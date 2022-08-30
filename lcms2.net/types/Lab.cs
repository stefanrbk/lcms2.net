namespace lcms2.types;

public struct Lab: ICloneable
{
    public double L;
    public double a;
    public double b;

    public Lab(double l, double a, double b) =>
        (L, this.a, this.b) = (l, a, b);

    public static implicit operator Lab((double, double, double) v) =>
        new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
           new Lab(L, a, b);

    public XYZ ToXYZ(XYZ? whitePoint = null)
    {
        if (whitePoint is null)
            whitePoint = WhitePoint.D50XYZ;

        var y = (L + 16.0) / 116.0;
        var x = y + (0.002 * a);
        var z = y - (0.005 * b);

        return (F1(x) * whitePoint.Value.X, F1(y) * whitePoint.Value.Y, F1(z) * whitePoint.Value.Z);
    }
}
