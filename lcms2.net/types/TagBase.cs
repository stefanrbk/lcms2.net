namespace lcms2.types;

public unsafe struct TagBase
{
    public static readonly int SizeOf = sizeof(TagBase);
    public fixed byte Reserved[4];
    public Signature Signature;

    public TagBase(Signature sig) =>
        Signature = sig;
}
