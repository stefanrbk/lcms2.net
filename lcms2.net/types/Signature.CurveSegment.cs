namespace lcms2.types;
public partial struct Signature
{
    public static class CurveSegment
    {
        public static readonly Signature Formula = new("parf");
        public static readonly Signature Sampled = new("samf");
        public static readonly Signature Segmented = new("curf");
    }
}
