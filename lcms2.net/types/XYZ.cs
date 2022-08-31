namespace lcms2.types;

public struct XYZ: ICloneable
{
    public double X;
    public double Y;
    public double Z;

    public XYZ(double x, double y, double z) =>
        (X, Y, Z) = (x, y, z);

    public static implicit operator XYZ((double, double, double) v) =>
        new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
           new XYZ(X, Y, Z);

    public Lab ToLab(XYZ? whitePoint = null)
    {
        whitePoint ??= WhitePoint.D50XYZ;

        var fx = F(X / whitePoint.Value.X);
        var fy = F(Y / whitePoint.Value.Y);
        var fz = F(Z / whitePoint.Value.Z);

        var L = (116.0 * fy) - 16.0;
        var a = 500.0 * (fx - fy);
        var b = 200.0 * (fy - fz);

        return (L, a, b);
    }

    public xyY ToxyY()
    {
        var iSum = 1.0/(X + Y + Z);

        return new(X * iSum, Y * iSum, Y);
    }

    public static Lab operator %(XYZ xyz, XYZ whitepoint) =>
        xyz.ToLab(whitepoint);

    public static explicit operator Lab(XYZ xyz) =>
        xyz.ToLab();

    public static explicit operator xyY(XYZ xyz) =>
        xyz.ToxyY();
}
