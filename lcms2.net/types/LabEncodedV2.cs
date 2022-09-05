namespace lcms2.types;
public struct LabEncodedV2: ICloneable
{
    public ushort L;
    public ushort a;
    public ushort b;

    public LabEncodedV2(ushort l, ushort a, ushort b) =>
        (L, this.a, this.b) = (l, a, b);

    public static implicit operator LabEncodedV2((ushort, ushort, ushort) v) =>
        new(v.Item1, v.Item2, v.Item3);

    public object Clone() =>
           new LabEncodedV2(L, a, b);

    public static explicit operator Lab(LabEncodedV2 lab) =>
        lab.ToLab();

    public Lab ToLab()
    {
        var dl = L2Float(L);
        var da = ab2Float(a);
        var db = ab2Float(b);

        return (dl, da, db);
    }

    private static double L2Float(ushort v) =>
        v / 652.8;

    private static double ab2Float(ushort v) =>
        (v / 256.0) - 128.0;
}
