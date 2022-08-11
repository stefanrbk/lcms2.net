using lcms2.state;

namespace lcms2.types;
public class NamedColorList : ICloneable, IDisposable
{
    internal uint NumColors;
    internal uint ColorantCount;

    internal string Prefix;
    internal string Suffix;

    internal List<NamedColor> List;

    internal Context? Context;

    public NamedColorList(Context? context, uint colorantCount, string prefix, string suffix)
    {
        Context = context;
        NumColors = 0;
        List = new List<NamedColor>();

        Prefix = prefix;
        Suffix = suffix;

        ColorantCount = colorantCount;
    }

    public bool Append(string name, ushort[] pcs, ushort[]? colorant)
    {
        throw new NotImplementedException();
    }

    public bool Info(uint numColor, out string name, out string prefix, out string suffix, out ushort[] pcs, out ushort[] colorant)
    {
        name = prefix = suffix = String.Empty;
        pcs = colorant = Array.Empty<ushort>();

        if (numColor >= NumColors) return false;

        name = List[(int)numColor].Name;
        prefix = Prefix;
        suffix = Suffix;
        pcs = (ushort[])List[(int)numColor].PCS.Clone();
        colorant = (ushort[])List[(int)numColor].DeviceColorant.Clone();

        return true;
    }

    public object Clone() => throw new NotImplementedException();
    public void Dispose() => throw new NotImplementedException();
}
