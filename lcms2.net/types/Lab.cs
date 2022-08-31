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
        whitePoint ??= WhitePoint.D50XYZ;

        var y = (L + 16.0) / 116.0;
        var x = y + (0.002 * a);
        var z = y - (0.005 * b);

        return (F1(x) * whitePoint.Value.X, F1(y) * whitePoint.Value.Y, F1(z) * whitePoint.Value.Z);
    }

    public LCh ToLCh() =>
        new(L, Math.Pow(Sqr(a) + Sqr(b), 0.5), Atan2Deg(b, a));

    public static explicit operator LabEncoded(Lab lab) =>
        lab.ToLabEncoded();

    public LabEncoded ToLabEncoded()
    {
        var dl = ClampLDoubleV4(L);
        var da = ClampabDoubleV4(a);
        var db = ClampabDoubleV4(b);

        var fl = L2Fix4(dl);
        var fa = ab2Fix4(da);
        var fb = ab2Fix4(db);

        return (fl, fa, fb);
    }

    public static explicit operator LabEncodedV2(Lab lab) =>
        lab.ToLabEncodedV2();

    public LabEncodedV2 ToLabEncodedV2()
    {
        var dl = ClampLDoubleV2(L);
        var da = ClampabDoubleV2(a);
        var db = ClampabDoubleV2(b);

        var fl = L2Fix2(dl);
        var fa = ab2Fix2(da);
        var fb = ab2Fix2(db);

        return (fl, fa, fb);
    }

    public static explicit operator XYZ(Lab lab) =>
        lab.ToXYZ();

    public static XYZ operator %(Lab lab, XYZ whitepoint) =>
        lab.ToXYZ(whitepoint);

    public static explicit operator LCh(Lab lab) =>
        lab.ToLCh();

    public static ushort[] Float2LabEncodedV2(Lab lab)
    {
        lab.L = ClampLDoubleV2(lab.L);
        lab.a = ClampabDoubleV2(lab.a);
        lab.b = ClampabDoubleV2(lab.b);

        var l = L2Fix2(lab.L);
        var a = ab2Fix2(lab.a);
        var b = ab2Fix2(lab.b);

        return new ushort[] { l, a, b };
    }

    private static double ClampLDoubleV2(double l)
    {
        const double lMax = 0xFFFF * 100.0 / 0xFF00;

        return l switch
        {
            < 0 => 0,
            > lMax => lMax,
            _ => l
        };
    }

    private static double ClampabDoubleV2(double ab) =>
        ab switch
        {
            < minEncodableAb2 => minEncodableAb2,
            > maxEncodableAb2 => maxEncodableAb2,
            _ => ab
        };

    private static double ClampLDoubleV4(double l) =>
        l switch
        {
            < 0 => 0,
            > 100.0 => 100.0,
            _ => l
        };

    private static double ClampabDoubleV4(double ab) =>
        ab switch
        {
            < minEncodableAb4 => minEncodableAb4,
            > maxEncodableAb4 => maxEncodableAb4,
            _ => ab
        };

    private static ushort L2Fix2(double l) =>
        QuickSaturateWord(l * 652.8);

    private static ushort ab2Fix2(double ab) =>
        QuickSaturateWord((ab + 128.0) * 256.0);

    private static ushort L2Fix4(double l) =>
        QuickSaturateWord(l * 655.35);

    private static ushort ab2Fix4(double ab) =>
        QuickSaturateWord((ab + 128.0) * 257.0);
}
