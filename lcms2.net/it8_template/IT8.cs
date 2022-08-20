using System.Diagnostics;
using System.Text;

using lcms2.state;

using Stri = System.String;

namespace lcms2.it8_template;

public class IT8: IDisposable
{
    public const int MaxId = 128;
    public const int MaxInclude = 20;
    public const int MaxStr = 1024;
    public const int MaxTables = 255;

    public static readonly string[] PredefinedSampleId = new string[]
    {
        "SAMPLE_ID",      // Identifies sample that data represents
        "STRING",         // Identifies label, or other non-machine readable value.
                          // Value must begin and end with a " symbol

        "CMYK_C",         // Cyan component of CMYK data expressed as a percentage
        "CMYK_M",         // Magenta component of CMYK data expressed as a percentage
        "CMYK_Y",         // Yellow component of CMYK data expressed as a percentage
        "CMYK_K",         // Black component of CMYK data expressed as a percentage
        "D_RED",          // Red filter density
        "D_GREEN",        // Green filter density
        "D_BLUE",         // Blue filter density
        "D_VIS",          // Visual filter density
        "D_MAJOR_FILTER", // Major filter d ensity
        "RGB_R",          // Red component of RGB data
        "RGB_G",          // Green component of RGB data
        "RGB_B",          // Blue com ponent of RGB data
        "SPECTRAL_NM",    // Wavelength of measurement expressed in nanometers
        "SPECTRAL_PCT",   // Percentage reflectance/transmittance
        "SPECTRAL_DEC",   // Reflectance/transmittance
        "XYZ_X",          // X component of tristimulus data
        "XYZ_Y",          // Y component of tristimulus data
        "XYZ_Z",          // Z component of tristimulus data
        "XYY_X",          // x component of chromaticity data
        "XYY_Y",          // y component of chromaticity data
        "XYY_CAPY",       // Y component of tristimulus data
        "LAB_L",          // L* component of Lab data
        "LAB_A",          // a* component of Lab data
        "LAB_B",          // b* component of Lab data
        "LAB_C",          // C*ab component of Lab data
        "LAB_H",          // hab component of Lab data
        "LAB_DE",         // CIE dE
        "LAB_DE_94",      // CIE dE using CIE 94
        "LAB_DE_CMC",     // dE using CMC
        "LAB_DE_2000",    // CIE dE using CIE DE 2000
        "MEAN_DE",        // Mean Delta E (LAB_DE) of samples compared to batch average
                          // (Used for data files for ANSI IT8.7/1 and IT8.7/2 targets)
        "STDEV_X",        // Standard deviation of X (tristimulus data)
        "STDEV_Y",        // Standard deviation of Y (tristimulus data)
        "STDEV_Z",        // Standard deviation of Z (tristimulus data)
        "STDEV_L",        // Standard deviation of L*
        "STDEV_A",        // Standard deviation of a*
        "STDEV_B",        // Standard deviation of b*
        "STDEV_DE",       // Standard deviation of CIE dE
        "CHI_SQD_PAR"};

    public int Ch;

    public Context? Context;

    public double Dnum;

    public byte[] DoubleFormatter = new byte[MaxId];

    public FileContext[] FileStack = new FileContext[MaxInclude];

    public String Id;

    public int IncludeStackPointer;

    public int Inum;

    public int LineNo;

    public char[]? MemoryBlock;

    public int NumTable;

    public Memory<char> Source;

    public String Str;

    // Parser state machine
    public Symbol Sy;

    public Table[] Tables = new Table[MaxTables];

    /// <summary>
    ///     How many tables in this stream
    /// </summary>
    public int TablesCount;

    // Allowed keywords & datasets. They have visibility on whole stream
    public KeyValue? ValidKeywords;

    public KeyValue? ValidSampleId;

    private bool disposed = false;

    public static int NumPredefinedSampleId =>
           PredefinedSampleId.Length;

    public string? SheetType
    {
        get =>
            Table.SheetType;
        set =>
            Table.SheetType = value;
    }

    public Table Table
    {
        get
        {
            if (NumTable >= TablesCount)
            {
                SynError($"Table {NumTable} out of sequence");
                return Tables[0];
            }
            return Tables[NumTable];
        }
    }

    public static IT8 Alloc(Context? context)
    {
        var it8 = new IT8();

        it8.AllocTable();

        it8.Context = context;
        it8.Sy = Symbol.Undefined;
        it8.Ch = ' ';
        it8.LineNo = 1;

        it8.Id = new String(it8, MaxStr);
        it8.Str = new String(it8, MaxStr);

        it8.SheetType = "CGATS.17";

        // Initialize predefined properties & data

        for (var i = 0; i < Property.NumPredefinedProperties; i++)
            it8.AddAvailableProperty(Property.PredefinedProperties[i].Id, Property.PredefinedProperties[i].As);

        for (var i = 0; i < NumPredefinedSampleId; i++)
            it8.AddAvailableSampleId(PredefinedSampleId[i]);

        return it8;
    }

    public static Symbol BinSrchKey(ReadOnlySpan<char> id)
    {
        var l = 1;
        var r = Keyword.NumKeys;

        while (r >= l)
        {
            var x = (l + r) / 2;
            var res = Stri.Compare(new string(id), Keyword.TabKeys[x - 1].Id);
            if (res == 0) return Keyword.TabKeys[x - 1].Symbol;
            if (res < 0) r = x - 1;
            else l = x + 1;
        }

        return Symbol.Undefined;
    }

    public static int IsMyBlock(ReadOnlySpan<char> buffer)
    {
        int words = 1, space = 0;
        var quot = false;
        var n = buffer.Length;

        if (n < 10) return 0;       // Too small

        if (n > 132)
        {
            n = 132;
            buffer = buffer[..132];
        }

        for (var i = 0; i < n; i++)
        {
            switch (buffer[i])
            {
                case '\n':
                case '\r':
                    return (quot || (words > 2)) ? 0 : words;

                case '\t':
                case ' ':
                    if (!quot && space == 0)
                        space = 1;
                    break;

                case '\"':
                    quot = !quot;
                    break;

                default:
                    if (buffer[i] is < (char)32 or > (char)127) return 0;
                    words += space;
                    space = 0;
                    break;
            }
        }

        return 0;
    }

    public static bool IsMyFile(string filename)
    {
        try
        {
            using var fp = File.OpenText(filename);

            var buffer = new char[132];
            var size = fp.Read(buffer);

            fp.Close();

            return IsMyBlock(buffer) != 0;
        } catch (FileNotFoundException)
        {
            Context.SignalError(null, ErrorCode.File, $"File '{filename}' not found");
            return false;
        } catch
        {
            return false;
        }
    }

    public static IT8? LoadFromFile(string filename)
    {
        Debug.Assert(!string.IsNullOrEmpty(filename));

        var type = IsMyFile(filename);
        if (!type) return null;

        var it8 = new IT8();

        try
        {
            it8.FileStack[0] = new FileContext()
            {
                Stream = File.OpenText(filename),
                FileName = filename,
            };

            if (!it8.Parse(!type))
            {
                it8.FileStack[0].Stream!.Close();
                it8.Dispose();
                return null;
            }

            it8.CookPointers();
            it8.NumTable = 0;

            return it8;
        } catch
        {
            it8.Dispose();
            return null;
        }
    }

    public static IT8? LoadFromMemory(ReadOnlyMemory<char> ptr)
    {
        var len = ptr.Length;

        Debug.Assert(len != 0);

        var type = IsMyBlock(ptr.Span);
        if (type is 0) return null;

        var it8 = new IT8()
        {
            MemoryBlock = ptr.ToArray(),
        };
        it8.FileStack[0] = new FileContext()
        {
            FileName = ""
        };
        it8.Source = it8.MemoryBlock;

        if (!it8.Parse(type == 0))
        {
            it8.Dispose();
            return null;
        }

        it8.CookPointers();
        it8.NumTable = 0;

        it8.MemoryBlock = null;

        return it8;
    }

    public static double ParseFloatNumber(ReadOnlySpan<char> buffer)
    {
        var dnum = 0d;
        var sign = 1;

        // keep safe
        if (buffer.Length == 0) return 0.0;

        if (buffer[0] is '-' or '+')
        {
            sign = buffer[0] == '-' ? -1 : 1;
            buffer = buffer[1..];
        }

        while (buffer.Length > 0 && buffer[0] != 0 && Char.IsDigit(buffer[0]))
        {
            dnum = (dnum * 10.0) + (buffer[0] - '0');

            buffer = buffer[1..];
        }

        if (buffer.Length > 0 && buffer[0] == '.')
        {
            var frac = 0d;
            var prec = 0;

            buffer = buffer[1..];

            while (buffer.Length > 0 && Char.IsDigit(buffer[0]))
            {
                frac = (frac * 10.0) + (buffer[0] - '0');
                prec++;

                buffer = buffer[1..];
            }

            dnum += frac / XPow10(prec);
        }

        if (buffer.Length > 0 && Char.ToUpper(buffer[0]) == 'E')
        {
            buffer = buffer[1..];
            int sgn = 1;

            if (buffer.Length > 0 && buffer[0] == '-')
            {
                sgn = -1;
                buffer = buffer[1..];
            } else if (buffer.Length > 0 && buffer[0] == '+')
            {
                sgn = +1;
                buffer = buffer[1..];
            }

            var e = 0;
            while (buffer.Length > 0 && Char.IsDigit(buffer[0]))
            {
                var digit = buffer[0] - '0';

                if ((e * 10.0) + digit < +2147483647.0)
                {
                    e = (e * 10) + digit;
                }
                buffer = buffer[1..];
            }

            e *= sgn;
            dnum *= XPow10(e);
        }

        return sign * dnum;
    }

    public static string StringToBinary(string? v)
    {
        if (v is null) return "0";
        var value = StringToInt(v);

        if (value == 0) return "0";

        var chars = new List<char>((int)Math.Floor(Math.Log2(value)));

        for (; value > 0; value /= 2) chars.Add((char)('0' + (value % 2)));

        return new string(((IEnumerable<char>)chars).Reverse().ToArray());
    }

    public static int StringToInt(string? b)
    {
        try
        {
            if (b is null) return 0;
            return Int32.Parse(b);
        } catch
        {
            return 0;
        }
    }

    public static double XPow10(int n) =>
        Math.Pow(10, n);

    public KeyValue AddAvailableProperty(string key, WriteMode @as) =>
        KeyValue.AddToList(ref ValidKeywords, key, "", "", @as);

    public KeyValue AddAvailableSampleId(string key) =>
        KeyValue.AddToList(ref ValidSampleId, key, "", "", WriteMode.Uncooked);

    public void AllocateDataFormat()
    {
        var t = Table;

        if (t.DataFormat is not null) return;   // Already allocated

        t.NumSamples = (int)GetPropertyDouble("NUMBER_OF_FIELDS");

        if (t.NumSamples <= 0)
        {
            SynError("AllocateDataFormat: Unknown NUMBER_OF_FIELDS");
            t.NumSamples = 10;
        }

        t.DataFormat = new string[t.NumSamples + 1];
    }

    public void AllocateDataSet()
    {
        var t = Table;

        if (t.Data is not null) return;     // Already allocated

        t.NumSamples = StringToInt(GetProperty("NUMBER_OF_FIELDS"));
        t.NumPatches = StringToInt(GetProperty("NUMBER_OF_SETS"));

        if (t.NumSamples is < 0 or > 0x7FFE || t.NumPatches is < 0 or > 0x7FFE)
        {
            SynError("AllocateDataSet: too much data");
        } else
        {
            t.Data = new string[(t.NumSamples + 1) * (t.NumPatches + 1)];
        }
    }

    public void AllocTable() =>
                           Tables[TablesCount++] = new Table();

    public bool Check(Symbol sy, ReadOnlySpan<char> err) =>
           Sy == sy || SynError(String.NoMeta(err));

    public bool CheckEOL()
    {
        if (!Check(Symbol.EOL, "Expected separator")) return false;
        while (Sy == Symbol.EOL)
        {
            InSymbol();
        }
        return true;
    }

    public void CookPointers()
    {
        var numOldTable = NumTable;

        for (var j = 0; j < TablesCount; j++)
        {
            var t = Tables[j];

            t.SampleId = 0;
            NumTable = j;

            for (var idField = 0; idField < t.NumSamples; idField++)
            {
                if (t.DataFormat is null)
                {
                    SynError("Undefined DATA_FORMAT");
                    return;
                }

                var fld = t.DataFormat[idField];
                if (string.IsNullOrEmpty(fld)) continue;

                if (string.Compare(fld, "SAMPLE_ID") == 0)

                    t.SampleId = idField;

                // "LABEL" is an extension. It keeps references to forward tables

                if (string.Compare(fld, "LABEL") == 0 || fld[0] == '$')
                {
                    // Search for table references...
                    for (var i = 0; i < t.NumPatches; i++)
                    {
                        var label = GetData(i, idField);

                        if (!string.IsNullOrEmpty(label))
                        {
                            // This is the label, search for a table containing this property

                            for (var k = 0; k < TablesCount; k++)
                            {
                                var table = Tables[k];

                                if (KeyValue.IsAvailableOnList(table.HeaderList!, label, "", out var p))
                                {
                                    // Available, keep type and table
                                    var type = p.Value;
                                    var numTable = k;

                                    var s = $"{label} {numTable} {type}";
                                    if (s.Length > 255)
                                        s = s[..255];
                                    SetData(i, idField, s);
                                }
                            }
                        }
                    }
                }
            }
        }

        NumTable = numOldTable;
    }

    public bool DataFormatSection()
    {
        var t = Table;

        InSymbol();     // Eats "BEGIN_DATA_FORMAT"
        CheckEOL();

        var iField = 0;
        while (Sy is not Symbol.SendDataFormat and not Symbol.EOL and not Symbol.EOF and not Symbol.SynError)
        {
            if (Sy != Symbol.Ident)
            {
                return SynError("Sample type expected");
            }

            if (!SetDataFormat(iField, Id.Begin.ToString())) return false;
            iField++;

            InSymbol();
            SkipEOL();
        }

        SkipEOL();
        Skip(Symbol.SendDataFormat);
        SkipEOL();

        if (iField != t.NumSamples)
        {
            SynError($"Count mismatch. NUMBER_OF_FIELDS was {t.NumSamples}, found {iField}\n");
        }

        return true;
    }

    public bool DataSection()
    {
        var iField = 0;
        var iSet = 0;
        var t = Table;

        InSymbol();     // Eats "BEGIN_DATA"
        CheckEOL();

        if (t.Data is null)
            AllocateDataSet();

        while (Sy is not Symbol.SendData and not Symbol.EOF)
        {
            if (iField >= t.NumSamples)
            {
                iField = 0;
                iSet++;
            }

            switch (Sy)
            {
                case Symbol.Ident:
                    if (!SetData(iSet, iField, Id.Begin.ToString()))
                        return false;
                    break;

                case Symbol.String:
                    if (!SetData(iSet, iField, Str.Begin.ToString()))
                        return false;
                    break;

                default:

                    if (!GetValue(out var buffer, 255, "Sample data expected"))
                        return false;

                    if (!SetData(iSet, iField, new string(buffer)))
                        return false;
                    break;
            }

            iField++;

            InSymbol();
            SkipEOL();
        }

        SkipEOL();
        Skip(Symbol.SendData);
        SkipEOL();

        // Check for data completion.

        if ((iSet + 1) != t.NumPatches)
            return SynError($"Count mismatch. NUMBER_OF_SETS was {t.NumPatches}, found {iSet + 1}\n");

        return true;
    }

    public void Dispose()
    {
        if (!disposed)
        {
            Tables = null!;
            Id.IT8 = null!;
            Str.IT8 = null!;

            for (var i = 0; i < MaxInclude; i++)
            {
                FileStack?[i].Stream?.Dispose();
            }
            GC.SuppressFinalize(this);
            disposed = true;
        }
    }

    public int EnumDataFormat(out string[]? sampleNames)
    {
        var it8 = this;

        var t = it8.Table;

        sampleNames = t.DataFormat;
        return t.NumSamples;
    }

    public int EnumProperties(out string[]? propertyNames)
    {
        var t = Table;

        var props = new List<string>();
        for (var p = t.HeaderList; p is not null; p = p.Next)
        {
            props.Add(p.Keyword);
        }

        propertyNames = props.ToArray();
        return props.Count;
    }

    public int EnumPropertyMulti(string cProp, out string[]? subPropertyNames)
    {
        var t = Table;

        if (!KeyValue.IsAvailableOnList(t.HeaderList!, cProp, "", out var p))
        {
            subPropertyNames = null;
            return 0;
        }

        var props = new List<string>();
        for (var tmp = p; tmp is not null; tmp = tmp.NextSubkey)
        {
            if (tmp.Subkey is not null)
                props.Add(tmp.Subkey);
        }

        subPropertyNames = props.ToArray();
        return props.Count;
    }

    public int FindDataFormat(string sample) =>
           LocateSample(sample);

    public string? GetData(int setIndex, int fieldIndex)
    {
        var t = Table;
        var sam = t.NumSamples;
        var pat = t.NumPatches;

        if (setIndex >= pat || fieldIndex >= sam)
            return null;

        if (t.Data is null)
            return null;
        return t.Data[(setIndex * sam) + fieldIndex];
    }

    public string? GetData(string patch, string sample)
    {
        var iField = LocateSample(sample);
        if (iField < 0) return null;

        var iSet = LocatePatch(patch);
        if (iSet < 0) return null;

        return GetData(iSet, iField);
    }

    public double GetDataDouble(string patch, string sample) =>
           ParseFloatNumber(GetData(patch, sample));

    public string? GetDataFormat(int n) =>
           Table.DataFormat?[n];

    public string? GetDataRowCol(int row, int col) =>
           GetData(row, col);

    public double GetDataRowColDouble(int row, int col)
    {
        var value = GetDataRowCol(row, col);

        if (string.IsNullOrEmpty(value)) return 0;

        return ParseFloatNumber(value);
    }

    public int GetPatchByName(string patch) =>
           LocatePatch(patch);

    public string? GetPatchName(int patch)
    {
        var t = Table;
        var data = GetData(patch, t.SampleId);

        if (string.IsNullOrEmpty(data)) return null;

        return data;
    }

    public string GetProperty(string key) =>
           KeyValue.IsAvailableOnList(Table.HeaderList, key, "", out var p)
               ? (p!.Value ?? "")
               : "";

    public double GetPropertyDouble(string cProp) =>
           ParseFloatNumber(GetProperty(cProp));

    public string GetPropertyMulti(string key, string subkey) =>
           KeyValue.IsAvailableOnList(Table.HeaderList, key, subkey, out var p)
               ? (p!.Value ?? "")
               : "";

    public bool GetValue(out Span<char> buffer, int max, ReadOnlySpan<char> errorTitle)
    {
        string value;
        switch (Sy)
        {
            case Symbol.EOL:
                buffer = Array.Empty<char>();
                break;

            case Symbol.Ident:
                value = Id.Begin.ToString();
                if (value.Length > max) value = value[..max];
                buffer = value.ToCharArray();

                break;

            case Symbol.Inum:
                value = Inum.ToString();
                if (value.Length > max) value = value[..max];
                buffer = value.ToCharArray();

                break;

            case Symbol.Dnum:
                value = Dnum.ToString();
                if (value.Length > max) value = value[..max];
                buffer = value.ToCharArray();

                break;

            case Symbol.String:
                value = Str.Begin.ToString();
                if (value.Length > max) value = value[..max];
                buffer = value.ToCharArray();

                break;

            default:
                buffer = Array.Empty<char>();

                return SynError(errorTitle);
        }

        return true;
    }

    public bool HeaderSection()
    {
        Span<char> buffer;

        while (Sy is not Symbol.EOF and not Symbol.SynError and not Symbol.BeginDataFormat and not Symbol.BeginData)
        {
            switch (Sy)
            {
                case Symbol.Keyword:

                    InSymbol();
                    if (!GetValue(out buffer, MaxStr - 1, "Keyword expected")) return false;
                    AddAvailableProperty(new string(buffer), WriteMode.Uncooked);
                    InSymbol();

                    break;

                case Symbol.DataFormatId:

                    InSymbol();
                    if (!GetValue(out buffer, MaxStr - 1, "Keyword expected")) return false;
                    AddAvailableSampleId(new string(buffer));
                    InSymbol();

                    break;

                case Symbol.Ident:

                    var varName = Id.Begin.ToString();

                    if (!KeyValue.IsAvailableOnList(ValidKeywords!, varName, "", out var key))
                    {
                        key = AddAvailableProperty(varName, WriteMode.Uncooked);
                    }

                    InSymbol();
                    if (!GetValue(out buffer, MaxStr - 1, "Property data expected")) return false;

                    if (key.WriteAs != WriteMode.Pair)
                    {
                        KeyValue.AddToList(ref Table.HeaderList, varName, "", new string(buffer), (Sy == Symbol.String) ? WriteMode.Stringify : WriteMode.Uncooked);
                    } else
                    {
                        if (Sy != Symbol.String)
                            return SynError($"Invalid value '{new string(buffer)}' for property '{varName}'");

                        // chop the string as a list of "subkey, value" pairs, using ';' as a
                        // separator for each pair, split the subkey and the value
                        var keys = new string(buffer).Split(';').Select(k => k.Split(',', StringSplitOptions.TrimEntries)).ToArray();
                        for (var i = 0; i < keys.Length; i++)
                        {
                            if (keys[i].Length != 2)
                                return SynError($"Invalid value for property '{varName}'");

                            (var subkey, var value) = (keys[i][0], keys[i][1]);

                            if (string.IsNullOrEmpty(subkey) || string.IsNullOrEmpty(value))
                                return SynError($"Invalid value for property '{varName}'");

                            KeyValue.AddToList(ref Table.HeaderList, varName, subkey, value, WriteMode.Pair);
                        }
                    }

                    InSymbol();

                    break;

                case Symbol.EOL: break;

                default:
                    return SynError("Expected keyword or identifier");
            }

            SkipEOL();
        }

        return true;
    }

    public void InStringSymbol()
    {
        while (String.IsSeparator(Ch))
            NextChar();

        if (Ch is '\'' or '\"')
        {
            var sng = Ch;
            Str.Clear();

            NextChar();

            while (Ch != sng)
            {
                if (Ch is '\n' or '\r' or 0) break;
                else
                {
                    Str.Append((char)Ch);
                    NextChar();
                }
            }

            Sy = Symbol.String;
            NextChar();
        } else
            SynError("String expected");
    }

    public void InSymbol()
    {
        do
        {
            while (String.IsSeparator(Ch))
            {
                NextChar();
            }

            if (String.IsFirstIdChar(Ch))
            {                                     // Identifier?
                Id.Clear();

                do
                {
                    Id.Append((char)Ch);

                    NextChar();
                } while (String.IsIdChar(Ch));

                var key = BinSrchKey(Id.Begin.ToString());
                Sy = key == Symbol.Undefined
                    ? Symbol.Ident
                    : key;
            } else if (Char.IsDigit((char)Ch) || Ch is '.' or '-' or '+')
            {     // Is a number?
                var sign = 1;

                if (Ch == '-')
                {
                    sign = -1;
                    NextChar();
                }

                Inum = 0;
                Sy = Symbol.Inum;

                if (Ch == '0')
                {                // 0xnnnn (Hex) or 0bnnnn (Bin)
                    NextChar();
                    if (Char.ToUpper((char)Ch) == 'X')
                    {
                        NextChar();
                        while (Char.IsDigit((char)Ch) || Char.ToUpper((char)Ch) is >= 'A' and <= 'F')
                        {
                            Ch = Char.ToUpper((char)Ch);
                            var j = Ch is >= 'A' and <= 'F'
                                ? Ch - 'A' + 10
                                : Ch - '0';

                            if ((Inum * 16.0) + j > +2147483647.0)
                            {
                                SynError("Invalid hexadecimal number");
                                return;
                            }

                            Inum = (Inum * 16) + j;
                            NextChar();
                        }

                        return;
                    }

                    if (Char.ToUpper((char)Ch) == 'B')
                    {
                        NextChar();
                        while (Ch is '0' or '1')
                        {
                            var j = Ch - '0';

                            if ((Inum * 2.0) + j > +2147483647.0)
                            {
                                SynError("Invalid binary number");
                                return;
                            }

                            Inum = (Inum * 2) + j;
                            NextChar();
                        }

                        return;
                    }
                }

                while (Char.IsDigit((char)Ch))
                {
                    var digit = Ch - '0';

                    if ((Inum * 10.0) + digit > +2147483647.0)
                    {
                        ReadReal(Inum);
                        Sy = Symbol.Dnum;
                        Dnum *= sign;
                        return;
                    }

                    Inum = (Inum * 10) + digit;
                    NextChar();
                }

                if (Ch is '.')
                {
                    ReadReal(Inum);
                    Sy = Symbol.Dnum;
                    Dnum *= sign;
                    return;
                }

                Inum *= sign;

                // Special case. Numbers followed by letters are taken as identifiers

                if (String.IsIdChar(Ch))
                {
                    var buf = Sy == Symbol.Inum
                        ? Inum.ToString()
                        : Dnum.ToString();

                    Id.Concat(buf);

                    do
                    {
                        Id.Append((char)Ch);

                        NextChar();
                    } while (String.IsIdChar(Ch));

                    Sy = Symbol.Ident;
                }
                return;
            } else
            {
                switch (Ch)
                {
                    // Eof marker -- ignore it
                    case '\x1a':
                        NextChar();

                        break;

                    // Eof stream markers
                    case 0:
                    case -1:
                        Sy = Symbol.EOF;

                        break;

                    // Next line
                    case '\r':
                        NextChar();
                        if (Ch is '\n')
                            NextChar();
                        Sy = Symbol.EOL;
                        LineNo++;

                        break;

                    case '\n':
                        NextChar();
                        Sy = Symbol.EOL;
                        LineNo++;

                        break;

                    // Comment
                    case '#':
                        NextChar();
                        while (Ch is not 0 and not '\n' and not '\r')
                            NextChar();

                        Sy = Symbol.Comment;
                        break;

                    // String
                    case '\'':
                    case '\"':
                        InStringSymbol();

                        break;

                    default:
                        SynError($"Unrecognized character: 0x{Ch:x}");

                        return;
                }
            }
        } while (Sy == Symbol.Comment);

        // Handle the include special token
        if (Sy == Symbol.Include)
        {
            if (IncludeStackPointer >= MaxInclude - 1)
            {
                SynError("Too many recursion levels");

                return;
            }

            InStringSymbol();
            if (!Check(Symbol.String, "Filename expected")) return;

            ref var fileNest = ref FileStack[IncludeStackPointer + 1];
            if (fileNest is null)
            {
                fileNest = new();
            }

            var buffer = new char[255];
            if (!String.BuildAbsolutePath(Str.Begin.ToString(), FileStack[IncludeStackPointer].FileName, buffer.AsSpan(), 255))
            {
                SynError("File path too long");
                return;
            }
            unsafe
            {
                fixed (char* ptr = buffer)
                {
                    fileNest.FileName = new Stri(ptr);
                }
            }

            try
            {
                fileNest.Stream = File.OpenText(fileNest.FileName);
            } catch
            {
                SynError($"An error occured while trying to open {fileNest.FileName}");
                return;
            }
            IncludeStackPointer++;

            Ch = ' ';
            InSymbol();
        }
    }

    public int LocateEmptyPatch()
    {
        var t = Table;

        for (var i = 0; i < t.NumPatches; i++)
        {
            var data = GetData(i, t.SampleId);

            if (string.IsNullOrEmpty(data)) return i;
        }

        return -1;
    }

    public int LocatePatch(string patch)
    {
        var t = Table;

        for (var i = 0; i < t.NumPatches; i++)
        {
            var data = GetData(i, t.SampleId);

            if (data is not null && string.Compare(data, patch) == 0)
                return i;
        }

        return -1;
    }

    public int LocateSample(string sample)
    {
        var t = Table;
        for (var i = 0; i < t.NumSamples; i++)
        {
            var fld = GetDataFormat(i);
            if (!string.IsNullOrEmpty(fld))
                if (string.Compare(fld, sample) == 0)
                    return i;
        }

        return -1;
    }

    public void NextChar()
    {
        var stream = FileStack[IncludeStackPointer].Stream;

        if (stream is not null)
        {
            Ch = stream.Read();
            if (Ch < 0)
            {
                if (IncludeStackPointer > 0)
                {
                    stream.Dispose();
                    IncludeStackPointer--;
                    Ch = ' ';                   // Whitespace to be ignored
                } else
                    Ch = 0;
            }
        } else
        {
            var span = Source.Span;
            Ch = span[0];
            Source = Source[1..];
        }
    }

    public bool Parse(bool nosheet)
    {
        var sheetType = Tables[0].SheetType;

        if (nosheet)
            ReadType(out sheetType);

        InSymbol();
        SkipEOL();

        while (Sy is not Symbol.EOF and not Symbol.SynError)
        {
            switch (Sy)
            {
                case Symbol.BeginDataFormat:

                    if (!DataFormatSection()) return false;

                    break;

                case Symbol.BeginData:

                    if (!DataSection()) return false;

                    if (Sy is not Symbol.EOF)
                    {
                        AllocTable();
                        NumTable = TablesCount - 1;

                        // Read sheet type if present. We only support identifier and string.
                        // <ident> <eol> is a type string anything else, is not a type string
                        if (!nosheet)
                        {
                            if (Sy is Symbol.Ident)
                            {
                                // May be a type sheet or may be a prop value statement. We cannot
                                // use InSymbol in this special case...
                                while (String.IsSeparator(Ch))
                                    NextChar();

                                // If a newline is found, then this is a type string
                                if (Ch is '\n' or '\r')
                                {
                                    SheetType = Id.Begin.ToString();
                                    InSymbol();
                                } else
                                    // It is not. Just continue
                                    SheetType = "";
                            } else if (Sy is Symbol.String)
                            {   // Validate quoted strings
                                SheetType = Str.Begin.ToString();
                                InSymbol();
                            }
                        }
                    }

                    break;

                case Symbol.EOL:

                    SkipEOL();
                    break;

                default:
                    if (!HeaderSection()) return false;
                    break;
            }
        }

        return Sy is not Symbol.SynError;
    }

    public void ReadReal(int inum)
    {
        Dnum = inum;

        while (Char.IsDigit((char)Ch))
        {
            Dnum = (Dnum * 10) + (Ch - '0');
            NextChar();
        }

        if (Ch == '.')
        {         // Decimal point
            var frac = 0d;     // Fraction
            var prec = 0;        // Precision

            NextChar();          // Eats dec. point

            while (Char.IsDigit((char)Ch))
            {
                frac = (frac * 10) + (Ch - '0');
                prec++;
                NextChar();
            }

            Dnum += frac / XPow10(prec);
        }

        // Exponent, example 34.00E+20
        if (Char.ToUpper((char)Ch) == 'E')
        {
            NextChar();

            var sgn = 1;
            if (Ch == '-')
            {
                sgn = -1;
                NextChar();
            } else if (Ch == '+')
            {
                sgn = +1;
                NextChar();
            }

            var e = 0;
            while (Char.IsDigit((char)Ch))
            {
                var digit = Ch - '0';

                if ((e * 10.0) + digit < +2147483647.0)
                    e = (e * 10) + digit;

                NextChar();
            }
            e *= sgn;
            Dnum *= XPow10(e);
        }
    }

    public void ReadType(out string sheetType)
    {
        var count = 0;

        // First line is a very special case.
        var sb = new StringBuilder();
        while (String.IsSeparator(Ch))
            NextChar();

        while (Ch is not '\r' and not '\n' and not '\t' and not 0)
        {
            if (count++ < MaxStr)
                sb.Append(Ch);
            NextChar();
        }

        sheetType = sb.ToString();
    }

    public bool SaveToFile(string filename)
    {
        var sd = new SaveStream();

        try
        {
            using var writer = new StreamWriter(File.OpenWrite(filename));
            sd.Stream = writer;

            for (var i = 0; i < TablesCount; i++)
            {
                SetTable(i);
                WriteHeader(sd);
                WriteDataFormat(sd);
                WriteData(sd);
            }

            sd.Stream = null;

            return true;
        } catch
        {
            return false;
        }
    }

    public bool SaveToMem(Memory<byte>? memPtr, ref int bytesNeeded)
    {
        var sd = new SaveStream
        {
            Stream = null,
            Base = memPtr,
            Ptr = memPtr,

            Used = 0,
            Max = (memPtr is not null)
                ? bytesNeeded
                : 0
        };

        for (var i = 0; i < TablesCount; i++)
        {
            SetTable(i);

            WriteHeader(sd);
            WriteDataFormat(sd);
            WriteData(sd);
        }

        sd.Used++;  // The \0 at the very end

        if (sd.Ptr is not null)
            sd.Ptr.Value.Span[0] = 0;

        bytesNeeded = sd.Used;

        return true;
    }

    public bool SetComment(string val)
    {
        if (string.IsNullOrEmpty(val)) return false;

        return KeyValue.AddToList(ref Table.HeaderList, "# ", "", val, WriteMode.Uncooked) is not null;
    }

    public bool SetData(int setIndex, int fieldIndex, string value)
    {
        var t = Table;

        if (t.Data is null) return false;

        if (setIndex > t.NumPatches || setIndex < 0)
            return SynError($"Patch {setIndex} out of range, there are {t.NumPatches} patches");

        if (fieldIndex > t.NumSamples || fieldIndex < 0)
            return SynError($"Sample {fieldIndex} out of range, there are {t.NumSamples} samples");

        t.Data[(setIndex * t.NumSamples) + fieldIndex] = value;
        return true;
    }

    public bool SetData(string patch, string sample, string value)
    {
        var t = Table;

        var iField = LocateSample(sample);

        if (iField < 0) return false;

        if (t.NumPatches == 0)
        {
            AllocateDataFormat();
            AllocateDataSet();
            CookPointers();
        }

        int iSet;
        if (string.Compare(sample, "SAMPLE_ID") == 0)
        {
            iSet = LocateEmptyPatch();
            if (iSet < 0)
                return SynError($"Couldn't add more patches '{patch}'");

            iField = t.SampleId;
        } else
        {
            iSet = LocatePatch(patch);
            if (iSet < 0) return false;
        }

        return SetData(iSet, iField, value);
    }

    public bool SetData(string patch, string sample, double value) =>
           SetData(patch, sample, value.ToString());

    public bool SetDataFormat(int n, string label)
    {
        var t = Table;

        if (t is null)
            AllocateDataFormat();

        if (n > t!.NumSamples)
        {
            SynError("More than NUMBER_OF_FIELDS fields.");
            return false;
        }

        if (t.DataFormat is not null)
            t.DataFormat[n] = label;

        return true;
    }

    public bool SetDataRowCol(int row, int col, string value) =>
           SetData(row, col, value);

    public bool SetDataRowCol(int row, int col, double value) =>
           SetData(row, col, value.ToString());

    public bool SetIndexColumn(string sample)
    {
        var pos = LocateSample(sample);
        if (pos == -1) return false;

        Tables[NumTable].SampleId = pos;
        return true;
    }

    public bool SetPropertyString(string key, string val)
    {
        if (string.IsNullOrEmpty(val)) return false;

        return KeyValue.AddToList(ref Table.HeaderList, key, "", val, WriteMode.Stringify) is not null;
    }

    public int SetTable(int numTable)
    {
        if (numTable >= TablesCount)
        {
            if (numTable == TablesCount)
            {
                if (numTable < MaxTables)
                    Tables[TablesCount++] = new Table();
            } else
            {
                SynError($"Table {NumTable} is out of sequence");
                return -1;
            }
        }

        NumTable = numTable;
        return numTable;
    }

    public int SetTableByLabel(string set, string field, string? expectedType)
    {
        if (string.IsNullOrEmpty(field)) field = "LABEL";

        var labelFld = GetData(set, field);
        if (string.IsNullOrEmpty(labelFld)) return -1;

        var scan = labelFld.Split(' ');
        if (scan.Length != 3) return -1;
        if (!UInt32.TryParse(scan[1], out var numTable)) return -1;
        var type = scan[2];

        if (!string.IsNullOrEmpty(expectedType) &&
            string.Compare(type, expectedType) != 0)

            return -1;

        return SetTable((int)numTable);
    }

    public void Skip(Symbol sy)
    {
        if (Sy == sy && Sy != Symbol.EOF)
            InSymbol();
    }

    public void SkipEOL()
    {
        while (Sy == Symbol.EOL)
            InSymbol();
    }

    // The average of the standard deviations of L*, a* and b*. It is used to derive an estimate of
    // the chi-squared parameter which is recommended as the predictor of the variability of dE
    public bool SynError(ReadOnlySpan<char> txt)
    {
        var errMsg = $"{FileStack[IncludeStackPointer].FileName}: Line {LineNo}, {new string(txt)}";
        Context.SignalError(Context, ErrorCode.CorruptionDetected, errMsg);
        return false;
    }

    public void WriteData(SaveStream fp)
    {
        var t = Table;

        if (t.Data is null) return;

        fp.WriteString("BEGIN_DATA\n");

        t.NumPatches = StringToInt(GetProperty("NUMBER_OF_SETS"));

        for (var i = 0; i < t.NumPatches; i++)
        {
            fp.WriteString(" ");

            for (var j = 0; j < t.NumSamples; j++)
            {
                var ptr = t.Data[(i * t.NumSamples) + j];

                if (string.IsNullOrEmpty(ptr)) fp.WriteString("\"\"");
                else
                {
                    // If value contains whitespace, enclose within quote

                    if (ptr.Contains(' '))
                    {
                        fp.WriteString($"\"{ptr}\"");
                    } else
                        fp.WriteString(ptr);
                }

                fp.WriteString((j == (t.NumSamples - 1)) ? "\n" : "\t");
            }
        }

        fp.WriteString("END_DATA\n");
    }

    public void WriteDataFormat(SaveStream fp)
    {
        var t = Table;

        if (t.DataFormat is null) return;

        fp.WriteString("BEGIN_DATA_FORMAT\n ");
        var numSamples = StringToInt(GetProperty("NUMBER_OF_FIELDS"));

        for (var i = 0; i < numSamples; i++)
        {
            fp.WriteString(t.DataFormat[i]);
            fp.WriteString((i == (numSamples - 1)) ? "\n" : "\t");
        }

        fp.WriteString("END_DATA_FORMAT\n");
    }

    public void WriteHeader(SaveStream fp)
    {
        var t = Table;

        // Writes the type
        fp.WriteString(t.SheetType);
        fp.WriteString("\n");

        for (var p = t.HeaderList; p is not null; p = p.Next)
        {
            if (p.Keyword[0] == '#')
            {
                fp.WriteString("#\n# ");

                var pt = new StringBuilder(p.Value);
                fp.WriteString(pt.Replace("\n", "\n# ").ToString());

                fp.WriteString("\n#\n");
                continue;
            }

            if (!KeyValue.IsAvailableOnList(ValidKeywords, p.Keyword, "", out _))
            {
                AddAvailableProperty(p.Keyword, WriteMode.Uncooked);
            }

            fp.WriteString(p.Keyword);
            if (p.Value is not null)
            {
                switch (p.WriteAs)
                {
                    case WriteMode.Uncooked:
                        fp.WriteString($"\t{p.Value}");
                        break;

                    case WriteMode.Stringify:
                        fp.WriteString($"\t\"{p.Value}\"");
                        break;

                    case WriteMode.Hexadecimal:
                        fp.WriteString($"\t0x{StringToInt(p.Value):X}");
                        break;

                    case WriteMode.Binary:
                        fp.WriteString($"\t0b{StringToBinary(p.Value)}");
                        break;

                    case WriteMode.Pair:
                        fp.WriteString($"\t\"{p.Subkey},{p.Value}\"");
                        break;

                    default:
                        SynError($"Unknown write mode {p.WriteAs}");
                        return;
                }
            }

            fp.WriteString("\n");
        }
    }
}

public enum WriteMode
{
    Uncooked,
    Stringify,
    Hexadecimal,
    Binary,
    Pair,
}
