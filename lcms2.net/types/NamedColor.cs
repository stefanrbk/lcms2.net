namespace lcms2.types;

internal class NamedColor
{
    internal ushort[] deviceColorant = new ushort[maxChannels];
    internal string name;
    internal ushort[] pcs = new ushort[3];
}
