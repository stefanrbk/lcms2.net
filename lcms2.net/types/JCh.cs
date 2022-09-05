namespace lcms2.types;

public struct JCh: ICloneable
{
    public double J;
    public double C;
    public double h;

    public JCh(double j, double c, double h) =>
        (J, C, this.h) = (j, c, h);

    public static implicit operator JCh((double, double, double) v) =>
        new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
           new JCh(J, C, h);
}
