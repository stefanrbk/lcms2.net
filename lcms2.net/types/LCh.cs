namespace lcms2.types;

public struct LCh: ICloneable
{
    public double L;
    public double C;
    public double h;

    public LCh(double l, double c, double h) =>
        (L, C, this.h) = (l, c, h);

    public static implicit operator LCh((double, double, double) v) =>
        new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
           new LCh(L, C, h);

    public Lab ToLab() =>
        new(L, C * Math.Cos(h * Math.PI / 180.0), C * Math.Sin(h * Math.PI / 180.0));

    public static explicit operator Lab(LCh lch) =>
        lch.ToLab();
}
