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

    public static readonly Signature MagicNumber = new("ascp");
    public static readonly Signature LcmsSignature = new("lcms");

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
    public static class Tag
    {
        public static readonly Signature AToB0 = new("A2B0");
        public static readonly Signature AToB1 = new("A2B1");
        public static readonly Signature AToB2 = new("A2B2");
        public static readonly Signature BlueColorant = new("bXYZ");
        public static readonly Signature BlueMatrixColumn = new("bXYZ");
        public static readonly Signature BlueTRC = new("bTRC");
        public static readonly Signature BToA0 = new("B2A0");
        public static readonly Signature BToA1 = new("B2A1");
        public static readonly Signature BToA2 = new("B2A2");
        public static readonly Signature CalibrationDateTime = new("calt");
        public static readonly Signature CharTarget = new("targ");
        public static readonly Signature ChromaticAdaptation = new("chad");
        public static readonly Signature Chromaticity = new("chrm");
        public static readonly Signature ColorantOrder = new("clro");
        public static readonly Signature ColorantTable = new("clrt");
        public static readonly Signature ColorantTableOut = new("clot");
        public static readonly Signature ColorimetricIntentImageState = new("ciis");
        public static readonly Signature Copyright = new("cprt");
        public static readonly Signature CrdInfo = new("crdi");
        public static readonly Signature Data = new("data");
        public static readonly Signature DateTime = new("dtim");
        public static readonly Signature DeviceMfgDesc = new("dmnd");
        public static readonly Signature DeviceModelDesc = new("dmdd");
        public static readonly Signature DeviceSettings = new("devs");
        public static readonly Signature DToB0 = new("D2B0");
        public static readonly Signature DToB1 = new("D2B1");
        public static readonly Signature DToB2 = new("D2B2");
        public static readonly Signature BToD0 = new("B2D0");
        public static readonly Signature BToD1 = new("B2D1");
        public static readonly Signature BToD2 = new("B2D2");
        public static readonly Signature Gamut = new("gamt");
        public static readonly Signature GrayTRC = new("kTRC");
        public static readonly Signature GreenColorant = new("gXYZ");
        public static readonly Signature GreenMatrixColumn = new("gXYZ");
        public static readonly Signature GreenTRC = new("gTRC");
        public static readonly Signature Luminance = new("lumi");
        public static readonly Signature Measurement = new("meas");
        public static readonly Signature MediaBlackPoint = new("bkpt");
        public static readonly Signature MediaWhitePoint = new("wtpt");
        [Obsolete("Use NamedColor2")]
        public static readonly Signature NamedColor = new("ncol");
        public static readonly Signature NamedColor2 = new("ncl2");
        public static readonly Signature OutputResponse = new("resp");
        public static readonly Signature PerceptualRenderingIntentGamut = new("rig0");
        public static readonly Signature Preview0 = new("pre0");
        public static readonly Signature Preview1 = new("pre1");
        public static readonly Signature Preview2 = new("pre2");
        public static readonly Signature ProfileDescription = new("desc");
        public static readonly Signature ProfileDescriptionML = new("decm");
        public static readonly Signature ProfileSequenceDesc = new("pseq");
        public static readonly Signature ProfileSequenceId = new("psid");
        public static readonly Signature Ps2CRD0 = new("psd0");
        public static readonly Signature Ps2CRD1 = new("psd1");
        public static readonly Signature Ps2CRD2 = new("psd2");
        public static readonly Signature Ps2CRD3 = new("psd3");
        public static readonly Signature Ps2CSA = new("ps2s");
        public static readonly Signature Ps2RenderingIntent = new("ps2i");
        public static readonly Signature RedColorant = new("rXYZ");
        public static readonly Signature RedMatrixColumn = new("rXYZ");
        public static readonly Signature RedTRC = new("rTRC");
        public static readonly Signature SaturationRenderingIntentGamut = new("rig2");
        public static readonly Signature ScreeningDesc = new("scrd");
        public static readonly Signature Screening = new("scrn");
        public static readonly Signature Technology = new("tech");
        public static readonly Signature UcrBg = new("bfd ");
        public static readonly Signature Vcgt = new("vcgt");
        public static readonly Signature ViewingCondDesc = new("vued");
        public static readonly Signature ViewingConditions = new("view");
        public static readonly Signature Meta = new("meta");
        public static readonly Signature ArgyllArts = new("arts");
    }
#if PLUGIN
    public
#else
    internal
#endif
    static class Plugin
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
