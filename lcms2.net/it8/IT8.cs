using Stri = System.String;
using lcms2.state;

namespace lcms2.it8;
public class IT8 : IDisposable
{
    public const int MaxId = 128;
    public const int MaxStr = 1024;
    public const int MaxTables = 255;
    public const int MaxInclude = 20;

    /// <summary>
    /// How many tables in this stream
    /// </summary>
    public int TablesCount;

    public int NumTable;

    public Table[] Tables = new Table[MaxTables];

    // Parser state machine
    public Symbol Sy;
    public int Ch;

    public int Inum;
    public double Dnum;

    public String Id;
    public String Str;

    // Allowed keywords & datasets. They have visibility on whole stream
    public KeyValue? ValidKeywords;
    public KeyValue? ValidSampleId;

    public Memory<char> Source;
    public int LineNo;

    public FileContext[] FileStack = new FileContext[MaxInclude];
    public int IncludeStackPointer;

    public byte[]? MemoryBlock;

    public byte[] DoubleFormatter = new byte[MaxId];

    public Context? Context;

    public Table Table
    {
        get
        {
            if (NumTable >= TablesCount) {
                SynError($"Table {NumTable} out of sequence");
                return Tables[0];
            }
            return Tables[NumTable];
        }
    }

    public string? SheetType
    {
        get =>
            Table.SheetType;
        set =>
            Table.SheetType = value;
    }

    public void AllocTable() =>
        Tables[TablesCount++] = new Table();

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
        "CHI_SQD_PAR"};   // The average of the standard deviations of L*, a* and b*. It is
                          // used to derive an estimate of the chi-squared parameter which is
                          // recommended as the predictor of the variability of dE

    public static int NumPredefinedSampleId =>
        PredefinedSampleId.Length;

    public bool SynError(ReadOnlySpan<char> txt)
    {
        var errMsg = $"{FileStack[IncludeStackPointer].FileName}: Line {LineNo}, {new string(txt)}";
        Context.SignalError(Context, ErrorCode.CorruptionDetected, errMsg);
        return false;
    }

    public bool Check(Symbol sy, ReadOnlySpan<char> err) =>
        Sy == sy || SynError(String.NoMeta(err));

    public void NextChar()
    {
        var stream = FileStack[IncludeStackPointer].Stream;

        if (stream is not null) {
            Ch = stream.Read();
            if (Ch < 0) {
                if (IncludeStackPointer > 0) {
                    stream.Dispose();
                    IncludeStackPointer--;
                    Ch = ' ';                   // Whitespace to be ignored
                } else
                    Ch = 0;
            }
        } else {
            var span = Source.Span;
            Ch = span[0];
            Source = Source[1..];
        }
    }

    public static Symbol BinSrchKey(ReadOnlySpan<char> id)
    {
        var l = 1;
        var r = Keyword.NumKeys;

        while (r >= l) {

            var x = (l + r) / 2;
            var res = Stri.Compare(new string(id), Keyword.TabKeys[x - 1].Id);
            if (res == 0) return Keyword.TabKeys[x - 1].Symbol;
            if (res < 0) r = x - 1;
            else l = x + 1;
        }

        return Symbol.Undefined;
    }

    public static double XPow10(int n) =>
        Math.Pow(10, n);

    public void ReadReal(int inum)
    {
        Dnum = inum;

        while (Char.IsDigit((char)Ch)) {

            Dnum = (Dnum * 10) + (Ch - '0');
            NextChar();
        }

        if (Ch == '.') {         // Decimal point

            var frac = 0d;     // Fraction
            var prec = 0;        // Precision

            NextChar();          // Eats dec. point

            while (Char.IsDigit((char)Ch)) {

                frac = (frac * 10) + (Ch - '0');
                prec++;
                NextChar();
            }

            Dnum += frac / XPow10(prec);
        }

        // Exponent, example 34.00E+20
        if (Char.ToUpper((char)Ch) == 'E') {

            NextChar();

            var sgn = 1;
            if (Ch == '-') {
                sgn = -1;
                NextChar();
            } else if (Ch == '+') {
                sgn = +1;
                NextChar();
            }

            var e = 0;
            while (Char.IsDigit((char)Ch)) {

                var digit = Ch - '0';

                if ((e * 10.0) + digit < +2147483647.0)
                    e = (e * 10) + digit;

                NextChar();
            }
            e *= sgn;
            Dnum *= XPow10(e);
        }
    }

    public static double ParseFloatNumber(ReadOnlySpan<char> buffer)
    {
        var dnum = 0d;
        var sign = 1;

        // keep safe
        if (buffer.Length == 0) return 0.0;

        if (buffer[0] is '-' or '+') {

            sign = buffer[0] == '-' ? -1 : 1;
            buffer = buffer[1..];
        }

        while (buffer.Length > 0 && buffer[0] != 0 && Char.IsDigit((char)buffer[0])) {

            dnum = (dnum * 10.0) + (buffer[0] - '0');

            buffer = buffer[1..];
        }

        if (buffer.Length > 0 && buffer[0] == '.') {

            var frac = 0d;
            var prec = 0;

            buffer = buffer[1..];

            while (buffer.Length > 0 && Char.IsDigit(buffer[0])) {

                frac = (frac * 10.0) + (buffer[0] - '0');
                prec++;

                buffer = buffer[1..];
            }

            dnum += frac / XPow10(prec);
        }

        if (buffer.Length > 0 && Char.ToUpper(buffer[0]) == 'E') {

            buffer = buffer[1..];
            int sgn = 1;

            if (buffer.Length > 0 && buffer[0] == '-') {
                sgn = -1;
                buffer = buffer[1..];
            } else if (buffer.Length > 0 && buffer[0] == '+') {
                sgn = +1;
                buffer = buffer[1..];
            }

            var e = 0;
            while (buffer.Length > 0 && Char.IsDigit(buffer[0])) {

                var digit = buffer[0] - '0';

                if ((e * 10.0) + digit < +2147483647.0) {
                    e = (e * 10) + digit;
                }
                buffer = buffer[1..];
            }

            e *= sgn;
            dnum *= XPow10(e);
        }

        return sign * dnum;
    }

    public void InStringSymbol()
    {
        while (String.IsSeparator(Ch))
            NextChar();

        if (Ch is '\'' or '\"') {

            var sng = Ch;
            Str.Clear();

            NextChar();

            while (Ch != sng) {

                if (Ch is '\n' or '\r' or 0) break;
                else {
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
        do {

            while (String.IsSeparator(Ch)) {
                NextChar();
            }

            if (String.IsFirstIdChar(Ch)) {                                     // Identifier?
                Id.Clear();

                do {

                    Id.Append((char)Ch);

                    NextChar();

                } while (String.IsIdChar(Ch));

                var key = BinSrchKey(Id.Begin.ToString());
                Sy = key == Symbol.Undefined
                    ? Symbol.Ident
                    : key;
            } else if (Char.IsDigit((char)Ch) || Ch is '.' or '-' or '+') {     // Is a number?

                var sign = 1;

                if (Ch == '-') {
                    sign = -1;
                    NextChar();
                }

                Inum = 0;
                Sy = Symbol.Inum;

                if (Ch == '0') {                // 0xnnnn (Hex) or 0bnnnn (Bin)

                    NextChar();
                    if (Char.ToUpper((char)Ch) == 'X') {

                        NextChar();
                        while (Char.IsDigit((char)Ch) || Char.ToUpper((char)Ch) is >= 'A' and <= 'F') {

                            Ch = Char.ToUpper((char)Ch);
                            var j = Ch is >= 'A' and <= 'F'
                                ? Ch - 'A' + 10
                                : Ch - '0';

                            if ((Inum * 16.0) + j > +2147483647.0) {
                                SynError("Invalid hexadecimal number");
                                return;
                            }

                            Inum = (Inum * 16) + j;
                            NextChar();
                        }

                        return;
                    }

                    if (Char.ToUpper((char)Ch) == 'B') {

                        NextChar();
                        while (Ch is '0' or '1') {

                            var j = Ch - '0';

                            if ((Inum * 2.0) + j > +2147483647.0) {
                                SynError("Invalid binary number");
                                return;
                            }

                            Inum = (Inum * 2) + j;
                            NextChar();
                        }

                        return;
                    }
                }

                while (Char.IsDigit((char)Ch)) {

                    var digit = Ch - '0';

                    if ((Inum * 10.0) + digit > +2147483647.0) {

                        ReadReal(Inum);
                        Sy = Symbol.Dnum;
                        Dnum *= sign;
                        return;
                    }

                    Inum = (Inum * 10) + digit;
                    NextChar();
                }

                if (Ch is '.') {

                    ReadReal(Inum);
                    Sy = Symbol.Dnum;
                    Dnum *= sign;
                    return;
                }

                Inum *= sign;

                // Special case. Numbers followed by letters are taken as identifiers

                if (String.IsIdChar(Ch)) {

                    var buf = Sy == Symbol.Inum
                        ? Inum.ToString()
                        : Dnum.ToString();

                    Id.Concat(buf);

                    do {
                        Id.Append((char)Ch);

                        NextChar();
                    } while (String.IsIdChar(Ch));

                    Sy = Symbol.Ident;
                }
                return;
            } else {
                switch (Ch) {
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
        if (Sy == Symbol.Include) {

            if (IncludeStackPointer >= MaxInclude - 1) {
                SynError("Too many recursion levels");

                return;
            }

            InStringSymbol();
            if (!Check(Symbol.String, "Filename expected")) return;

            ref var fileNest = ref FileStack[IncludeStackPointer + 1];
            if (fileNest is null) {
                fileNest = new();
            }

            var buffer = new char[255];
            if (!String.BuildAbsolutePath(Str.Begin.ToString(), FileStack[IncludeStackPointer].FileName, buffer.AsSpan(), 255)) {
                SynError("File path too long");
                return;
            }
            unsafe {
                fixed (char* ptr = buffer) {
                    fileNest.FileName = new Stri(ptr);
                }
            }

            try {
                fileNest.Stream = File.OpenText(fileNest.FileName);
            } catch {
                SynError($"An error occured while trying to open {fileNest.FileName}");
                return;
            }
            IncludeStackPointer++;

            Ch = ' ';
            InSymbol();
        }
    }

    public bool CheckEOL()
    {
        if (!Check(Symbol.EOL, "Expected separator")) return false;
        while (Sy == Symbol.EOL) {
            InSymbol();
        }
        return true;
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

    public bool GetValue(out Span<char> buffer, int max, ReadOnlySpan<char> errorTitle)
    {
        switch (Sy) {
            case Symbol.EOL:
                buffer = Array.Empty<char>();
                break;

            case Symbol.Ident:
                buffer = Id.Begin.ToString().ToCharArray();
                break;

            case Symbol.Inum:
                buffer = $"{Inum}".ToCharArray();
                break;

            case Symbol.Dnum:
                buffer = $"{Dnum}".ToCharArray();
                break;

            case Symbol.String:
                buffer = Str.Begin.ToString().ToCharArray();
                break;

            default:
                buffer = Array.Empty<char>();
                return SynError(errorTitle);
        }

        return true;
    }

    private bool disposed = false;
    public void Dispose()
    {
        if (!disposed) {
            Tables = null!;
            Id.IT8 = null!;
            Str.IT8 = null!;
            
            for (var i = 0; i < MaxInclude; i++) {
                FileStack?[i].Stream?.Dispose();
            }
            GC.SuppressFinalize(this);
            disposed = true;
        }
    }

    public KeyValue AddAvailableProperty(string key, WriteMode @as) =>
        KeyValue.AddToList(ref ValidKeywords, key, "", "", @as);

    public KeyValue AddAvailableSampleId(string key) =>
        KeyValue.AddToList(ref ValidSampleId, key, "", "", WriteMode.Uncooked);

    public int SetTable(int numTable)
    {
        if (numTable >= TablesCount) {
            if (numTable == TablesCount) {
                if (numTable < MaxTables)
                    Tables[TablesCount++] = new Table();
            } else {
                SynError($"Table {NumTable} is out of sequence");
                return -1;
            }
        }

        NumTable = numTable;
        return numTable;
    }

    public bool SetComment(string val)
    {
        if (string.IsNullOrEmpty(val)) return false;

        return KeyValue.AddToList(ref Table.HeaderList, "# ", "", val, WriteMode.Uncooked) is not null;
    }

    public bool SetPropertyString(string key, string val)
    {
        if (string.IsNullOrEmpty(val)) return false;

        return KeyValue.AddToList(ref Table.HeaderList, key, "", val, WriteMode.Stringify) is not null;
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
}

public enum WriteMode
{
    Uncooked,
    Stringify,
    Hexadecimal,
    Binary,
    Pair,
}