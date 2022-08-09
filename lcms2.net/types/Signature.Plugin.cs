namespace lcms2.types;

public partial struct Signature
{
    public static class Plugin
    {
        public static readonly Signature MagicNumber = new("acpp");
        public static readonly Signature Interpolation = new("inpH");
        public static readonly Signature ParametricCurve = new("parH");
        public static readonly Signature Formatters = new("frmH");
        public static readonly Signature TagType = new("typH");
        public static readonly Signature Tag = new("tagH");
        public static readonly Signature RenderingIntent = new("intH");
        public static readonly Signature MultiProcessElement = new("mpeH");
        public static readonly Signature Optimization = new("optH");
        public static readonly Signature Translform = new("xfmH");
        public static readonly Signature Mutex = new("mtxH");
    }
}
