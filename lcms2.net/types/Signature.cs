using System.Text;

namespace lcms2.types;
public record Signature
{
    private readonly uint value;

    public Signature(uint value) =>
        this.value = value;
    public Signature(string value)
    {
        byte[] bytes = { 0x20, 0x20, 0x20, 0x20 };
        var s = Encoding.ASCII.GetBytes(value);
        switch (s.Length)
        {
            case 0:
                break;
            case 1:
                bytes[0] = s[0];
                break;
            case 2:
                bytes[1] = s[1];
                goto case 1;
            case 3:
                bytes[2] = s[2];
                goto case 2;
            default:
                bytes[3] = s[3];
                goto case 3;
        };
        this.value = ((uint)bytes[0] << 24) + ((uint)bytes[1] << 16) + ((uint)bytes[2] << 8) + bytes[3];
    }
    public static implicit operator uint(Signature v)
    {
        return v.value;
    }

    public readonly static Signature MagicNumber = new("ascp");
    public readonly static Signature LcmsSignature = new("lcms");

    public static class TagType
    {
        public readonly static Signature Chromaticity = new("chrm");
        public readonly static Signature ColorantOrder = new("clro");
        public readonly static Signature ColorantTable = new("clrt");
        public readonly static Signature CrdInfo = new("crdi");
        public readonly static Signature Curve = new("curv");
        public readonly static Signature Data = new("data");
        public readonly static Signature Dict = new("dict");
        public readonly static Signature DateTime = new("dtim");
        public readonly static Signature DeviceSettings = new("devs");
        public readonly static Signature Lut16 = new("mft2");
        public readonly static Signature Lut8 = new("mft1");
        public readonly static Signature LutAtoB = new("mAB ");
        public readonly static Signature LutBtoA = new("mBA ");
        public readonly static Signature Measurement = new("meas");
        public readonly static Signature MultiLocalizedUnicode = new("mluc");
        public readonly static Signature MultiProcessElement = new("mpet");
        [Obsolete("Use NamedColor2")]
        public readonly static Signature NamedColor = new("ncol");
        public readonly static Signature NamedColor2 = new("ncl2");
        public readonly static Signature ParametricCurve = new("para");
        public readonly static Signature ProfileSequenceDesc = new("pseq");
        public readonly static Signature ProfileSequenceId = new("psid");
        public readonly static Signature ResponseCurveSet16 = new("rcs2");
        public readonly static Signature S15Fixed16Array = new("sf32");
        public readonly static Signature Screening = new("scrn");
        public readonly static Signature Signature = new("sig ");
        public readonly static Signature Text = new("text");
        public readonly static Signature TextDescription = new("desc");
        public readonly static Signature U16Fixed16Array = new("uf32");
        public readonly static Signature UcrBg = new("bfd ");
        public readonly static Signature UInt16Array = new("ui16");
        public readonly static Signature UInt32Array = new("ui32");
        public readonly static Signature UInt64Array = new("ui64");
        public readonly static Signature UInt8Array = new("ui08");
        public readonly static Signature Vcgt = new("vcgt");
        public readonly static Signature ViewingConditions = new("view");
        public readonly static Signature XYZ = new("XYZ ");
    }
    public static class Tag
    {
        public readonly static Signature AToB0 = new("A2B0");
        public readonly static Signature AToB1 = new("A2B1");
        public readonly static Signature AToB2 = new("A2B2");
        public readonly static Signature BlueColorant = new("bXYZ");
        public readonly static Signature BlueMatrixColumn = new("bXYZ");
        public readonly static Signature BlueTRC = new("bTRC");
        public readonly static Signature BToA0 = new("B2A0");
        public readonly static Signature BToA1 = new("B2A1");
        public readonly static Signature BToA2 = new("B2A2");
        public readonly static Signature CalibrationDateTime = new("calt");
        public readonly static Signature CharTarget = new("targ");
        public readonly static Signature ChromaticAdaptation = new("chad");
        public readonly static Signature Chromaticity = new("chrm");
        public readonly static Signature ColorantOrder = new("clro");
        public readonly static Signature ColorantTable = new("clrt");
        public readonly static Signature ColorantTableOut = new("clot");
        public readonly static Signature ColorimetricIntentImageState = new("ciis");
        public readonly static Signature Copyright = new("cprt");
        public readonly static Signature CrdInfo = new("crdi");
        public readonly static Signature Data = new("data");
        public readonly static Signature DateTime = new("dtim");
        public readonly static Signature DeviceMfgDesc = new("dmnd");
        public readonly static Signature DeviceModelDesc = new("dmdd");
        public readonly static Signature DeviceSettings = new("devs");
        public readonly static Signature DToB0 = new("D2B0");
        public readonly static Signature DToB1 = new("D2B1");
        public readonly static Signature DToB2 = new("D2B2");
        public readonly static Signature BToD0 = new("B2D0");
        public readonly static Signature BToD1 = new("B2D1");
        public readonly static Signature BToD2 = new("B2D2");
        public readonly static Signature Gamut = new("gamt");
        public readonly static Signature GrayTRC = new("kTRC");
        public readonly static Signature GreenColorant = new("gXYZ");
        public readonly static Signature GreenMatrixColumn = new("gXYZ");
        public readonly static Signature GreenTRC = new("gTRC");
        public readonly static Signature Luminance = new("lumi");
        public readonly static Signature Measurement = new("meas");
        public readonly static Signature MediaBlackPoint = new("bkpt");
        public readonly static Signature MediaWhitePoint = new("wtpt");
        [Obsolete("Use NamedColor2")]
        public readonly static Signature NamedColor = new("ncol");
        public readonly static Signature NamedColor2 = new("ncl2");
        public readonly static Signature OutputResponse = new("resp");
        public readonly static Signature PerceptualRenderingIntentGamut = new("rig0");
        public readonly static Signature Preview0 = new("pre0");
        public readonly static Signature Preview1 = new("pre1");
        public readonly static Signature Preview2 = new("pre2");
        public readonly static Signature ProfileDescription = new("desc");
        public readonly static Signature ProfileDescriptionML = new("decm");
        public readonly static Signature ProfileSequenceDesc = new("pseq");
        public readonly static Signature ProfileSequenceId = new("psid");
        public readonly static Signature Ps2CRD0 = new("psd0");
        public readonly static Signature Ps2CRD1 = new("psd1");
        public readonly static Signature Ps2CRD2 = new("psd2");
        public readonly static Signature Ps2CRD3 = new("psd3");
        public readonly static Signature Ps2CSA = new("ps2s");
        public readonly static Signature Ps2RenderingIntent = new("ps2i");
        public readonly static Signature RedColorant = new("rXYZ");
        public readonly static Signature RedMatrixColumn = new("rXYZ");
        public readonly static Signature RedTRC = new("rTRC");
        public readonly static Signature SaturationRenderingIntentGamut = new("rig2");
        public readonly static Signature ScreeningDesc = new("scrd");
        public readonly static Signature Screening = new("scrn");
        public readonly static Signature Technology = new("tech");
        public readonly static Signature UcrBg = new("bfd ");
        public readonly static Signature Vcgt = new("vcgt");
        public readonly static Signature ViewingCondDesc = new("vued");
        public readonly static Signature ViewingConditions = new("view");
        public readonly static Signature Meta = new("meta");
        public readonly static Signature ArgyllArts = new("arts");
    }
}
