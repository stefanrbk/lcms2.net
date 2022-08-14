namespace lcms2.types;
public sealed class DictionaryEntry
{
    public DictionaryEntry? Next;

    public Mlu? DisplayName;
    public Mlu? DisplayValue;
    public string Name;
    public string Value;

    public DictionaryEntry(string name, string value)
    {
        Name = name;
        Value = value;
        DisplayName = null;
        DisplayValue = null;
    }
}
