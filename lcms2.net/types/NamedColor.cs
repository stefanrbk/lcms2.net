namespace lcms2.types;

internal class NamedColor
{
    internal ushort[] deviceColorant = new ushort[Lcms2.MaxChannels];
    internal string name;
    internal ushort[] pcs = new ushort[3];
}
