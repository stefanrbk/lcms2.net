namespace lcms2.types;


public unsafe struct TagBase
{
    public Signature Signature;
    public fixed byte Reserved[4];

    public TagBase(Signature sig) =>
        Signature = sig;

    public static readonly int SizeOf = sizeof(TagBase);
}
