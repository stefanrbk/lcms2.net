using lcms2.state;

namespace lcms2.types;
public sealed class Dictionary : ICloneable, IDisposable
{
    public DictionaryEntry? Head { get; internal set; }
    public Context? Context { get; internal set; }

    public Dictionary(Context? context = null)
    {
        Head = null;
        Context = context;
    }

    public void AddEntry(string name, string value, in Mlu? displayName, in Mlu? displayValue) =>
        Head = new DictionaryEntry(name, value)
        {
            DisplayName = displayName,
            DisplayValue = displayValue,

            Next = Head,
        };

    public object Clone() => throw new NotImplementedException();
    public void Dispose() => throw new NotImplementedException();
}
