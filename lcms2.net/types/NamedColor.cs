namespace lcms2.types;
internal class NamedColor
{
    internal string Name;
    internal ushort[] PCS = new ushort[3];
    internal ushort[] DeviceColorant = new ushort[Lcms2.MaxChannels];
}
