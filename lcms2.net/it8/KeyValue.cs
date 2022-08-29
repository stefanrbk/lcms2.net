namespace lcms2.it8;
internal class KeyValue
{
    public readonly string Key;
    public readonly string? Subkey;
    public readonly string? Value;
    public readonly WriteMode Mode;
    public readonly List<KeyValue> Subkeys = new();

    public KeyValue(string key, WriteMode mode) =>
        (Key, Mode) = (key, mode);

    public KeyValue(string key, string value, WriteMode mode) =>
        (Key, Value, Mode) = (key, value, mode);

    public KeyValue(string key, double value, WriteMode mode) =>
        (Key, Value, Mode) = (key, value.ToString(), mode);

    public KeyValue(string key, string subkey, string value, WriteMode mode) =>
        (Key, Subkey, Value, Mode) = (key, subkey, value, mode);

    public KeyValue(string key, string subkey, double value, WriteMode mode) =>
        (Key, Subkey, Value, Mode) = (key, subkey, value.ToString(), mode);
}
