namespace lcms2.types;

public sealed class DictionaryEntry
{
    public Mlu? DisplayName;
    public Mlu? DisplayValue;
    public string Name;
    public DictionaryEntry? Next;
    public string Value;

    public DictionaryEntry(string name, string value)
    {
        Name = name;
        Value = value;
        DisplayName = null;
        DisplayValue = null;
    }
}
