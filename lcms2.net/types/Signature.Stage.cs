namespace lcms2.types;

public partial struct Signature
{
    public static class Stage
    {
        public static readonly Signature BAcsElem = new("bACS");
        public static readonly Signature ClipNegativesElem = new("clp ");
        public static readonly Signature CLutElem = new("clut");
        public static readonly Signature CurveSetElem = new("cvst");
        public static readonly Signature EAcsElem = new("eACS");
        public static readonly Signature FloatPCS2Lab = new("l2d ");
        public static readonly Signature FloatPCS2XYZ = new("x2d ");

        // Identities
        public static readonly Signature IdentityElem = new("idn ");

        // Float to floatPCS
        public static readonly Signature Lab2FloatPCS = new("d2l ");

        public static readonly Signature Lab2XYZElem = new("x2l ");
        public static readonly Signature LabV2toV4Elem = new("2 4 ");
        public static readonly Signature LabV4toV2Elem = new("4 2 ");
        public static readonly Signature MatrixElem = new("matf");
        public static readonly Signature NamedColorElem = new("ncl ");

        public static readonly Signature XYZ2FloatPCS = new("d2x ");

        // Custom from here, not in the ICC Spec
        public static readonly Signature XYZ2LabElem = new("l2x ");
    }
}
