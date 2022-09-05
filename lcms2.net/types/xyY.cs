namespace lcms2.types;

public struct xyY: ICloneable
{
    public double x;
    public double y;
    public double Y;

    public xyY(double x, double y, double Y) =>
        (this.x, this.y, this.Y) = (x, y, Y);

    public static implicit operator xyY((double, double, double) v) =>
        new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
           new xyY(x, y, Y);

    public XYZ ToXYZ()
    {
        var dx = x / y * Y;
        var dz = (1 - x - y) / y * Y;

        return (dx, Y, dz);
    }

    public static explicit operator XYZ(xyY xyy) =>
        xyy.ToXYZ();
}

public struct xyYTripple: ICloneable
{
    public xyY Blue;
    public xyY Green;
    public xyY Red;

    public xyYTripple(xyY red, xyY green, xyY blue) =>
        (Red, Green, Blue) = (red, green, blue);

    public static implicit operator xyYTripple((xyY, xyY, xyY) v) =>
        new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
        new xyYTripple(Red, Green, Blue);
}
