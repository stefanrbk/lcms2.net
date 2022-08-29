namespace lcms2.types;

public partial struct Signature
{
    public static class Tag
    {
        public static readonly Signature ArgyllArts = new("arts");
        public static readonly Signature AToB0 = new("A2B0");
        public static readonly Signature AToB1 = new("A2B1");
        public static readonly Signature AToB2 = new("A2B2");
        public static readonly Signature BlueColorant = new("bXYZ");
        public static readonly Signature BlueMatrixColumn = new("bXYZ");
        public static readonly Signature BlueTRC = new("bTRC");
        public static readonly Signature BToA0 = new("B2A0");
        public static readonly Signature BToA1 = new("B2A1");
        public static readonly Signature BToA2 = new("B2A2");
        public static readonly Signature BToD0 = new("B2D0");
        public static readonly Signature BToD1 = new("B2D1");
        public static readonly Signature BToD2 = new("B2D2");
        public static readonly Signature BToD3 = new("B2D3");
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
        public static readonly Signature DToB3 = new("D2B3");
        public static readonly Signature Gamut = new("gamt");
        public static readonly Signature GrayTRC = new("kTRC");
        public static readonly Signature GreenColorant = new("gXYZ");
        public static readonly Signature GreenMatrixColumn = new("gXYZ");
        public static readonly Signature GreenTRC = new("gTRC");
        public static readonly Signature Luminance = new("lumi");
        public static readonly Signature Measurement = new("meas");
        public static readonly Signature MediaBlackPoint = new("bkpt");
        public static readonly Signature MediaWhitePoint = new("wtpt");
        public static readonly Signature Meta = new("meta");

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
        public static readonly Signature Screening = new("scrn");
        public static readonly Signature ScreeningDesc = new("scrd");
        public static readonly Signature Technology = new("tech");
        public static readonly Signature UcrBg = new("bfd ");
        public static readonly Signature Vcgt = new("vcgt");
        public static readonly Signature ViewingCondDesc = new("vued");
        public static readonly Signature ViewingConditions = new("view");
    }
}
