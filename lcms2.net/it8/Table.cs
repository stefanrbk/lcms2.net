using System.Text;

namespace lcms2.it8;
internal class Table
{
    internal string[]? data;
    internal string[]? dataFormat;
    internal readonly List<KeyValue> header = new();
    internal int sampleId;

    internal int NumSamples =>
        Int32.Parse(GetProperty("NUMBER_OF_FIELDS"));

    internal int NumPatches =>
        Int32.Parse(GetProperty("NUMBER_OF_SETS"));

    internal void AllocateDataFormat()
    {
        if (dataFormat is not null) return;     // Already allocated

        var samples = (int)GetPropertyDouble("NUMBER_OF_FIELDS");

        if (samples is <= 0) samples = 10;

        dataFormat = new string[samples + 1];
    }

    internal void AllocateDataSet()
    {
        if (data is not null) return;     // Already allocated

        var numSamples = IT8.StringToInt(GetProperty("NUMBER_OF_FIELDS"));
        var numPatches = IT8.StringToInt(GetProperty("NUMBER_OF_SETS"));

        if (numSamples is < 0 or > 0x7FFE || numPatches is < 0 or > 0x7FFE)
            throw new IT8Exception("AllocateDataSet: too much data");

        data = new string[(numSamples + 1) * (numPatches + 2)];
    }

    internal string[] EnumProperties() =>
        header.Select(k => k.Key).ToArray();

    internal string[] EnumProperties(string property) =>
        header.FindAll(k => k.Key == property && k.Subkey is not null)
              .Select(k => k.Subkey)
              .Cast<string>()
              .ToArray();

    internal int FindDataFormat(string sample) =>
        LocateSample(sample);

    internal string? GetData(int set, int field)
    {
        if (set >= NumPatches || field >= NumSamples) return null;

        if (data is null) return null;

        return data[(set * NumSamples) + field];
    }

    internal string? GetData(string patch, string sample)
    {
        var field = LocateSample(sample);
        if (field < 0) return null;

        var set = LocatePatch(patch);
        if (set < 0) return null;

        return GetData(set, field);
    }

    internal string? GetDataFormat(int n) =>
        dataFormat?[n];

    internal int GetPatchByName(string patch) =>
        LocatePatch(patch);

    internal string? GetPatchName(int patch) =>
        GetData(patch, sampleId);

    internal string GetProperty(string key) =>
        header.Find(k => k.Key == key)?.Value ?? String.Empty;

    internal double GetPropertyDouble(string key) =>
        Double.Parse(header.Find(k => k.Key == key)?.Value ?? "0");

    internal string GetProperty(string key, string subkey) =>
        header.Find(k => k.Key == key && k.Subkey == subkey)?.Value ?? String.Empty;

    internal int LocateSample(string sample) =>
        dataFormat?.FindIndex(i => String.Compare(i, sample) == 0) ?? -1;

    internal int LocatePatch(string patch) =>
        data?.Chunk(NumSamples)
             .Select(s => s[sampleId])
             .ToList()
             .FindIndex(k => String.Compare(k, patch) == 0) ?? -1;

    internal int LocateEmptyPatch() =>
        data?.Chunk(NumSamples)
             .Select(s => s[sampleId])
             .ToList()
             .FindIndex(k => String.IsNullOrEmpty(k)) ?? -1;

    internal void SetDataFormat(int n, string label, Stack<StreamReader>? s = null, int? lineNo = null)
    {
        if (dataFormat is null)
            AllocateDataFormat();

        if (n > NumSamples)
        {
            if (s is not null && lineNo is not null)
                throw new IT8Exception(s, lineNo.Value, "More than NUMBER_OF_FIELDS fields.");
            else
                throw new IT8Exception("More than NUMBER_OF_FIELDS fields.");
        }

        if (dataFormat is not null)
            dataFormat[n] = label;
    }

    internal void SetData(int setIndex, int fieldIndex, string value, Stack<StreamReader>? s = null, int? lineNo = null)
    {
        if (data is null) return;

        if (setIndex > NumPatches || setIndex < 0)
        {
            if (s is not null && lineNo is not null)
                throw new IT8Exception(s, lineNo.Value, $"Patch {setIndex} out of range, there are {NumPatches} patches");
            else
                throw new IT8Exception($"Patch {setIndex} out of range, there are {NumPatches} patches");
        }

        if (fieldIndex > NumSamples || fieldIndex < 0)
        {
            if (s is not null && lineNo is not null)
                throw new IT8Exception(s, lineNo.Value, $"Sample {fieldIndex} out of range, there are {NumSamples} samples");
            else
                throw new IT8Exception($"Sample {fieldIndex} out of range, there are {NumSamples} samples");
        }

        data[(setIndex * NumSamples) + fieldIndex] = value;
    }

    internal void SetData(string patch, string sample, string value, List<Table>? tables = null, Stack<StreamReader>? s = null, int? lineNo = null)
    {
        var field = LocateSample(sample);

        if (field < 0) return;

        if (NumPatches is 0)
        {
            AllocateDataFormat();
            AllocateDataSet();
            if (tables is not null)
                tables.Cook(s, lineNo);
        }

        int set;
        if (String.Compare(sample, "SAMPLE_ID") == 0)
        {
            set = LocateEmptyPatch();
            if (set < 0)
            {
                if (s is not null && lineNo is not null)
                    throw new IT8Exception(s, lineNo.Value, $"Couldn't add more patches '{patch}'");
                else
                    throw new IT8Exception($"Couldn't add more patches '{patch}'");
            }
            field = sampleId;
        } else {
            set = LocatePatch(patch);
            if (set < 0) return;
        }

        SetData(set, field, value, s, lineNo);
    }
}
