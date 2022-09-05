//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//---------------------------------------------------------------------------------
//
namespace lcms2.it8;

public class IT8
{
    #region Fields

    internal const int maxId = 128;
    internal const int maxInclude = 20;
    internal const double maxInum = +2147483647.0;
    internal const int maxStr = 1024;
    internal readonly List<KeyValue> availableProperties = new();
    internal readonly List<KeyValue> availableSampleId = new();
    internal readonly List<Table> tables = new();
    internal int currentTable;
    internal string doubleFormat = "D";

    #endregion Fields

    #region Public Constructors

    public IT8()
    {
        foreach (var p in Property.PredefinedProperties)
            availableProperties.Add(p.Id, null, null, p.As);

        foreach (var s in SampleId.PredefinedSampleIds)
            availableSampleId.Add(s, null, null, WriteMode.Uncooked);
    }

    #endregion Public Constructors

    #region Properties

    public string? SheetType { get; set; } = "CGATS.17";

    public int TableCount =>
        tables.Count;

    internal Table Table
    {
        get
        {
            if (currentTable > tables.Count)
                throw new IT8Exception($"Table {currentTable} out of sequence");
            else if (currentTable == tables.Count)
                tables.Add(new Table());
            return tables[currentTable];
        }
    }

    #endregion Properties

    #region Public Methods

    public static IT8? LoadFromFile(string filename) =>
        new Reader().LoadFromFile(filename);

    public static IT8? LoadFromMemory(byte[] buffer) =>
        new Reader().LoadFromMemory(buffer);

    public int CountProperties() =>
        Table.header.Count;

    public int CountProperties(string property) =>
        Table.header.Count(p => p.Key == property && p.Subkey is not null);

    public void DefineDoubleFormat(string? format) =>
        doubleFormat = format ?? "D";

    public IEnumerable<string> EnumDataFormat() =>
        Table.dataFormat ?? Array.Empty<string>();

    public IEnumerable<string> EnumProperties() =>
        Table.header.Select(p => p.Key);

    public IEnumerable<string> EnumProperties(string property) =>
        Table.header.Where(p => p.Key == property && p.Subkey is not null).Select(p => p.Subkey!);

    public int FindDataFormat(string sample) =>
        Table.LocateSample(sample);

    public string? GetData(string patch, string sample) =>
        Table.GetData(patch, sample);

    public double GetDataDouble(string patch, string sample) =>
        Double.TryParse(GetData(patch, sample), out var val)
            ? val
            : 0.0;

    public string? GetDataRowCol(int row, int col) =>
        Table.GetData(row, col);

    public double GetDataRowColDouble(int row, int col) =>
        Double.Parse(Table.GetData(row, col) ?? "0");

    public int GetPatchByName(string patch) =>
        Table.GetPatchByName(patch);

    public string? GetPatchName(int patch) =>
        Table.GetPatchName(patch);

    public string? GetProperty(string property) =>
        GetProperty(property, null);

    public string? GetProperty(string key, string? subkey) =>
        Table.header.Find(kv => kv.Key == key && ((subkey is null) || kv.Subkey == subkey))?.Value;

    public double GetPropertyDouble(string property) =>
        Double.TryParse(GetProperty(property), out var result)
            ? result
            : 0;

    public void SaveToFile(string filename) =>
        new Writer(this).SaveToFile(filename);

    public long SaveToMemory(byte[] buffer) =>
        new Writer(this).SaveToMemory(buffer);

    public byte[] SaveToMemory() =>
        new Writer(this).SaveToMemory();

    public void SetComment(string value)
    {
        if (!string.IsNullOrEmpty(value))
            Table.header.Add("#", null, value, WriteMode.Uncooked);
    }

    public void SetData(string patch, string sample, string value) =>
        Table.SetData(patch, sample, value, tables);

    public void SetData(string patch, string sample, double value) =>
        SetData(patch, sample, value.ToString(doubleFormat));

    public void SetDataFormat(int n, string sample) =>
        Table.SetDataFormat(n, sample);

    public void SetDataRowCol(int row, int col, string value) =>
        Table.SetData(row, col, value);

    public void SetDataRowCol(int row, int col, double value) =>
        Table.SetData(row, col, value.ToString(doubleFormat));

    public void SetIndexColumn(string sample)
    {
        var pos = Table.LocateSample(sample);
        if (pos == -1) return;

        Table.sampleId = pos;
    }

    public void SetProperty(string key, string value)
    {
        if (!String.IsNullOrEmpty(value))
            Table.header.Add(key, null, value, WriteMode.Stringify);
    }

    public void SetProperty(string key, string? subkey, string? value) =>
        Table.header.Add(key, subkey, value, WriteMode.Stringify);

    public void SetPropertyDouble(string key, double value) =>
        SetProperty(key, value.ToString(doubleFormat));

    public void SetPropertyHex(string key, int value) =>
        Table.header.Add(key, null, value.ToString(), WriteMode.Hexadecimal);

    public void SetPropertyUncooked(string key, string? value) =>
        Table.header.Add(key, null, value, WriteMode.Uncooked);

    public void SetTable(int n)
    {
        if (n > TableCount)
            throw new IT8Exception($"Table {n} is out of sequence");
        else if (n == TableCount)
            tables.Add(new Table());
        currentTable = n;
    }

    public void SetTableByLabel(string set, string field, string? expectedType)
    {
        if (string.IsNullOrEmpty(field)) field = "LABEL";

        var labelFld = GetData(set, field);
        if (string.IsNullOrEmpty(labelFld)) return;

        var scan = labelFld.Split(' ');
        if (scan.Length < 3) return;
        if (!UInt32.TryParse(scan[1], out var numTable)) return;
        var type = scan[2];

        if (!string.IsNullOrEmpty(expectedType) &&
            string.Compare(type, expectedType) != 0)
        {
            return;
        }

        SetTable((int)numTable);
    }

    #endregion Public Methods

    #region Internal Methods

    internal static Symbol FindKey(string id) =>
        Keywords.List.GetValueOrDefault(id, Symbol.Undefined);

    internal static string StringToBinary(string? v)
    {
        if (v is null) return "0";
        var value = StringToInt(v);

        if (value == 0) return "0";

        var chars = new List<char>((int)Math.Floor(Math.Log2(value)));

        for (; value > 0; value /= 2) chars.Add((char)('0' + (value % 2)));

        return new string(chars.AsEnumerable().Reverse().ToArray());
    }

    internal static int StringToInt(string? b)
    {
        try
        {
            if (b is null) return 0;
            return Int32.Parse(b);
        }
        catch
        {
            return 0;
        }
    }

    internal static double XPow10(int n) =>
                Math.Pow(10, n);

    #endregion Internal Methods
}
