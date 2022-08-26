namespace lcms2.types;

public class NamedColorList: ICloneable, IDisposable
{
    internal uint colorantCount;

    internal object? state;

    internal List<NamedColor> list;

    internal uint numColors;

    internal string prefix;

    internal string suffix;

    public NamedColorList(object? state, uint colorantCount, string prefix, string suffix)
    {
        this.state = state;
        numColors = 0;
        list = new List<NamedColor>();

        this.prefix = prefix;
        this.suffix = suffix;

        this.colorantCount = colorantCount;
    }

    public bool Append(string name, ushort[] pcs, ushort[]? colorant)
    {
        throw new NotImplementedException();
    }

    public object Clone() => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();

    public bool Info(uint numColor, out string name, out string prefix, out string suffix, out ushort[] pcs, out ushort[] colorant)
    {
        name = prefix = suffix = String.Empty;
        pcs = colorant = Array.Empty<ushort>();

        if (numColor >= numColors) return false;

        name = list[(int)numColor].name;
        prefix = this.prefix;
        suffix = this.suffix;
        pcs = (ushort[])list[(int)numColor].pcs.Clone();
        colorant = (ushort[])list[(int)numColor].deviceColorant.Clone();

        return true;
    }
}
