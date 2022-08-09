namespace lcms2.types;

public partial struct Signature
{
    public static class TagType
    {
        public static readonly Signature Chromaticity = new("chrm");
        public static readonly Signature ColorantOrder = new("clro");
        public static readonly Signature ColorantTable = new("clrt");
        public static readonly Signature CrdInfo = new("crdi");
        public static readonly Signature Curve = new("curv");
        public static readonly Signature Data = new("data");
        public static readonly Signature Dict = new("dict");
        public static readonly Signature DateTime = new("dtim");
        public static readonly Signature DeviceSettings = new("devs");
        public static readonly Signature Lut16 = new("mft2");
        public static readonly Signature Lut8 = new("mft1");
        public static readonly Signature LutAtoB = new("mAB ");
        public static readonly Signature LutBtoA = new("mBA ");
        public static readonly Signature Measurement = new("meas");
        public static readonly Signature MultiLocalizedUnicode = new("mluc");
        public static readonly Signature MultiProcessElement = new("mpet");
        [Obsolete("Use NamedColor2")]
        public static readonly Signature NamedColor = new("ncol");
        public static readonly Signature NamedColor2 = new("ncl2");
        public static readonly Signature ParametricCurve = new("para");
        public static readonly Signature ProfileSequenceDesc = new("pseq");
        public static readonly Signature ProfileSequenceId = new("psid");
        public static readonly Signature ResponseCurveSet16 = new("rcs2");
        public static readonly Signature S15Fixed16Array = new("sf32");
        public static readonly Signature Screening = new("scrn");
        public static readonly Signature Signature = new("sig ");
        public static readonly Signature Text = new("text");
        public static readonly Signature TextDescription = new("desc");
        public static readonly Signature U16Fixed16Array = new("uf32");
        public static readonly Signature UcrBg = new("bfd ");
        public static readonly Signature UInt16Array = new("ui16");
        public static readonly Signature UInt32Array = new("ui32");
        public static readonly Signature UInt64Array = new("ui64");
        public static readonly Signature UInt8Array = new("ui08");
        public static readonly Signature Vcgt = new("vcgt");
        public static readonly Signature ViewingConditions = new("view");
        public static readonly Signature XYZ = new("XYZ ");
    }
}
