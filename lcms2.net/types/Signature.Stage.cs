namespace lcms2.types;

public partial struct Signature
{
    public static class Stage
    {
        public static readonly Signature CurveSetElem = new("cvst");
        public static readonly Signature MatrixElem = new("matf");
        public static readonly Signature CLutElem = new("clut");
    }
}
