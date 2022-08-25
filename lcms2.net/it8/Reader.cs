using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static lcms2.it8.IT8;

namespace lcms2.it8;
internal class Reader: IDisposable
{
    private int _ch;
    private readonly Stack<StreamReader> _fileStack = new();
    private readonly StringBuilder _id = new(128);
    private readonly StringBuilder _str = new(1024);
    private Symbol _sy = Symbol.Undefined;
    private int _inum;
    private double _dnum;
    private int _lineNo;
    private readonly List<KeyValue> _availableProperties = new();
    private readonly List<KeyValue> _availableSampleIds = new();
    private readonly List<Table> _tables = new();
    private int _currentTable;
    private string? _sheetType;
    private bool _disposedValue;

    private Table Table
    {
        get
        {
            if (_currentTable >= _tables.Count)
                throw new IT8Exception(_fileStack, _lineNo, $"Table {_currentTable} out of sequence");
            return _tables[_currentTable];
        }
    }

    /// <summary>
    ///     Checks to see if <paramref name="stream"/> is a compatible stream to read from.
    ///     Returns the stream position to the original location and keeps it open.
    /// </summary>
    /// <param name="stream"></param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="stream"/> does not support reading.
    /// </exception>
    /// <exception cref="IOException">
    ///     An I/O error occurs.
    /// </exception>
    /// <exception cref="NotSupportedException">
    ///     The stream does not support seeking.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///     The stream was closed.
    /// </exception>
    public static bool IsMyFormat(Stream stream)
    {
        using var reader = new StreamReader(stream, encoding: Encoding.ASCII, leaveOpen: true);

        var buffer = new char[132];
        var pos = reader.BaseStream.Seek(0, SeekOrigin.Current);
        var size = reader.Read(buffer);

        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        reader.Close();

        int words = 1, space = 0;
        var quot = false;
        var span = new ReadOnlySpan<char>(buffer, 0, size);

        if (span.Length < 10) return false;

        for (var i = 0; i < span.Length; i++)
        {
            switch (span[i])
            {
                case '\n':
                case '\r':
                    return !(quot || (words > 2));

                case '\t':
                case ' ':
                    if (!quot && space == 0)
                        space = 1;
                    break;

                case '\"':
                    quot = !quot;
                    break;

                default:
                    if (span[i] is < (char)32 or > (char)127) return false;
                    words += space;
                    space = 0;
                    break;
            }
        }

        return false;
    }

    private void GetValue(out string value, int maxStringLength, string err)
    {
        switch (_sy)
        {
            case Symbol.EOL:
                value = "";
                return;

            case Symbol.Ident:
                value = _id.ToString();
                if (value.Length > maxStringLength) value = value[..maxStringLength];

                return;

            case Symbol.Inum:
                value = _inum.ToString();
                if (value.Length > maxStringLength) value = value[..maxStringLength];

                return;

            case Symbol.Dnum:
                value = _dnum.ToString();
                if (value.Length > maxStringLength) value = value[..maxStringLength];

                return;

            case Symbol.String:
                value = _str.ToString();
                if (value.Length > maxStringLength) value = value[..maxStringLength];

                return;

            default:
                value = "";

                throw new IT8Exception(_fileStack, _lineNo, err);
        }
    }

    private void ReadType(out string sheetType)
    {
        var count = 0;

        // First line is a very special case.
        var sb = new StringBuilder(100);
        while (IsSeparator(_ch))
            NextChar();

        while (_ch is not '\r' and not '\n' and not '\t' and not 0)
        {
            if (count++ < maxStr)
                sb.Append(_ch);
            NextChar();
        }

        sheetType = sb.ToString();
    }

    private void ReadDataFormatSection()
    {
        var t = _tables[_currentTable];

        InSymbol();     // Eats "BEGIN_DATA_FORMAT"
        CheckEol();

        var field = 0;
        while (_sy is not Symbol.SendDataFormat and not Symbol.EOL and not Symbol.EOF and not Symbol.SynError)
        {
            if (_sy is not Symbol.Ident)
                throw new IT8Exception(_fileStack, _lineNo, "Sample type expected");

            t.SetDataFormat(field++, _id.ToString(), _fileStack, _lineNo);

            InSymbol();
            SkipEol();
        }

        SkipEol();
        Skip(Symbol.SendDataFormat);
        SkipEol();

        if (field != t.NumSamples)
            throw new IT8Exception(_fileStack, _lineNo, $"Count mismatch. NUMBER_OF_FIELDS was {t.NumSamples}, found {field}");
    }

    private void ReadDataSection()
    {
        var t = _tables[_currentTable];

        InSymbol();     // Eats "BEGIN_DATA"
        CheckEol();

        var field = 0;
        var set = 0;

        if (t.data is null)
            t.AllocateDataSet();

        while (_sy is not Symbol.SendData and not Symbol.EOF)
        {
            if (field >= t.NumSamples)
            {
                field = 0;
                set++;
            }
            switch (_sy)
            {
                case Symbol.Ident:
                    t.SetData(set, field, _id.ToString());
                    break;

                case Symbol.String:
                    t.SetData(set, field, _str.ToString());
                    break;

                default:
                    GetValue(out var buffer, 255, "Sample data expected");
                    t.SetData(set, field, buffer);
                    break;
            }

            field++;

            InSymbol();
            SkipEol();
        }

        SkipEol();
        Skip(Symbol.SendDataFormat);
        SkipEol();

        if (field != t.NumSamples)
            throw new IT8Exception(_fileStack, _lineNo, $"Count mismatch. NUMBER_OF_FIELDS was {t.NumSamples}, found {field}");
    }

    private void ReadHeaderSection()
    {
        string buffer;

        while (_sy is not Symbol.EOF and not Symbol.SynError and not Symbol.BeginDataFormat and not Symbol.BeginData)
        {
            switch (_sy)
            {
                case Symbol.Keyword:

                    InSymbol();
                    GetValue(out buffer, maxStr - 1, "Keyword expected");
                    _availableProperties.Add(new(buffer, WriteMode.Uncooked));
                    InSymbol();
                    break;

                case Symbol.DataFormatId:

                    InSymbol();
                    GetValue(out buffer, maxStr - 1, "Keyword expected");
                    _availableSampleIds.Add(new(buffer, WriteMode.Uncooked));
                    InSymbol();
                    break;

                case Symbol.Ident:

                    var varName = _id.ToString();
                    KeyValue? key;
                    if ((key = _availableProperties.First(i => i.Key == varName)) is null)
                    {
                        key = new(varName, WriteMode.Uncooked);
                        _availableProperties.Add(key);
                    }

                    InSymbol();
                    GetValue(out buffer, maxStr - 1, "Property data expected");

                    if (key.Mode is not WriteMode.Pair)
                        Table.header.Add(new(varName, buffer, (_sy is Symbol.String) ? WriteMode.Stringify : WriteMode.Uncooked));
                    else
                    {
                        if (_sy is not Symbol.String)
                            throw new IT8Exception(_fileStack, _lineNo, $"Invalid value '{buffer}' for property '{varName}'.");

                        // chop the string as a list of "subkey, value" pairs, using ';' as a
                        // separator for each pair, split the subkey and the value
                        var keys = new string(buffer).Split(';')
                                                              .Select(k => k.Split(',', StringSplitOptions.TrimEntries))
                                                              .ToArray();
                        for (var i = 0; i < keys.Length; i++)
                        {
                            if (keys[i].Length < 2)
                                throw new IT8Exception(_fileStack, _lineNo, $"Invalid value for property '{varName}'");

                            var (subkey, value) = (keys[i][0], keys[i][1]);

                            if (String.IsNullOrEmpty(subkey) || String.IsNullOrEmpty(value))
                                throw new IT8Exception(_fileStack, _lineNo, $"Invalid value for property '{varName}'");

                            Table.header.Add(new (varName, subkey, value, WriteMode.Pair));
                        }
                    }

                    InSymbol();

                    break;

                default:
                    throw new IT8Exception(_fileStack, _lineNo, "Expected keyword or identifier");
            }

            SkipEol();
        }
    }

    private void ReadReal(int inum)
    {
        _dnum = inum;

        while (IsDigit(_ch))
        {
            _dnum = (_dnum * 10) + (_ch - '0');
            NextChar();
        }

        if (_ch is '.')     // Decimal point
        {
            var frac = 0d;
            var prec = 0;

            NextChar();

            while (IsDigit(_ch))
            {
                frac = (frac * 10) + (_ch - '0');
                prec++;
                NextChar();
            }

            _dnum += frac / XPow10(prec);
        }

        // Exponent, example 34.00E+20
        if (ToUpper(_ch) is 'E')
        {
            NextChar();

            var eSign = 1;
            if (_ch is '-')
            {
                eSign = -1;
                NextChar();
            } else if (_ch is '+')
                NextChar();

            var e = 0;
            while (IsDigit(_ch))
            {
                var digit = _ch - '0';

                if ((e * 10.0) + digit < maxInum)
                    e = (e * 10) + digit;

                NextChar();
            }
            e *= eSign;
            _dnum *= XPow10(e);
        }
    }

    private static bool IsSeparator(int c) =>
        c is ' ' or '\t';

    private static bool IsMiddle(int c) =>
        !IsSeparator(c) && c is not '#' and not '\"' and not '\'' and > 32 and < 127;

    private static bool IsId(int c) =>
        IsDigit(c) || IsMiddle(c);

    private static bool IsFirstId(int c) =>
        !IsDigit(c) && IsMiddle(c);

    private static bool IsDigit(int c) =>
        Char.IsDigit((char)c);

    private static char ToUpper(int c) =>
        Char.ToUpper((char)c);

    private void NextChar()
    {
        var stream = _fileStack.Peek();

        _ch = stream.Read();
        if (_ch < 0 && _fileStack.Count > 0)
        {
            stream.Dispose();
            _fileStack.Pop();
            _ch = ' ';
        }
    }

    private void Check(Symbol sy, string err)
    {
        if (_sy != sy)
            throw new IT8Exception(_fileStack, _lineNo, err);
    }

    private void CheckEol()
    {
        Check(Symbol.EOL, "Expected separator");

        while (_sy is Symbol.EOL)
            InSymbol();
    }

    private void InStringSymbol()
    {
        while (IsSeparator(_ch))
            NextChar();

        if (_ch is '\'' or '\"')
        {
            var quot = _ch;
            _str.Clear();

            NextChar();

            while (_ch != quot)
            {
                if (_ch is '\n' or '\r' or <= 0) break;
                else
                {
                    _str.Append((char)_ch);
                    NextChar();
                }
            }

            _sy = Symbol.String;
            NextChar();
        } else
            throw new IT8Exception(_fileStack, _lineNo, "String expected");
    }

    private void InSymbol()
    {
        do
        {
            while (IsSeparator(_ch))
                NextChar();

            if (IsFirstId(_ch))
            {
                _id.Clear();

                do
                {
                    _id.Append((char)_ch);

                    NextChar();
                } while (IsId(_ch));

                var key = FindKey(_id.ToString());
                _sy = key is Symbol.Undefined ? Symbol.Ident : key;
            } else if (IsDigit((char)_ch) || _ch is '.' or '-' or '+')      // Is a number?
            {
                var sign = 1;

                if (_ch is '-')
                {
                    sign = -1;
                    NextChar();
                }

                _inum = 0;
                _sy = Symbol.Inum;

                if (_ch is '0')     // 0xnnnn (Hex) or 0bnnnn (Bin)
                {
                    NextChar();
                    switch (ToUpper(_ch))
                    {
                        case 'X':
                            NextChar();
                            while (IsDigit(_ch) || ToUpper(_ch) is >= 'A' and <= 'F')
                            {
                                _ch = ToUpper(_ch);
                                var j = _ch is >= 'A' and <= 'F'
                                    ? _ch - 'A' + 10
                                    : _ch - '0';

                                if ((_inum * 16.0) + j > maxInum)
                                    throw new IT8Exception(_fileStack, _lineNo, "Invalid hexadecimal number");

                                _inum = (_inum * 16) + j;
                                NextChar();
                            }

                            return;

                        case 'B':
                            NextChar();
                            while (_ch is '0' or '1')
                            {
                                var j = _ch - '0';

                                if ((_inum * 2.0) + j > maxInum)
                                    throw new IT8Exception(_fileStack, _lineNo, "Invalid binary number");

                                _inum = (_inum * 2) + j;
                                NextChar();
                            }

                            return;
                    }
                }

                while (IsDigit(_ch))
                {
                    var digit = _ch - '0';

                    if ((_inum * 10.0) + digit > maxInum)
                    {
                        ReadReal(_inum);
                        _sy = Symbol.Dnum;
                        _dnum *= sign;
                        return;
                    }

                    _inum = (_inum * 10) + digit;
                    NextChar();
                }

                if (_ch is '.')
                {
                    ReadReal(_inum);
                    _sy = Symbol.Dnum;
                    _dnum *= sign;
                    return;
                }

                _inum *= sign;

                // Special case. Numbers followed by letters are taken as identifiers
                if (IsId(_ch))
                {
                    var buf = _sy is Symbol.Inum
                        ? _inum.ToString()
                        : _dnum.ToString();

                    _id.Append(buf);

                    do
                    {
                        _id.Append((char)_ch);

                        NextChar();
                    } while (IsId(_ch));

                    _sy = Symbol.Ident;
                }
                return;
            }
            else
            {
                switch (_ch)
                {
                    // Eof marker -- ignore it
                    case '\x1a':
                        NextChar();

                        break;

                    // Eof stream markers
                    case 0:
                    case -1:
                        _sy = Symbol.EOF;

                        break;

                    // Next line
                    case '\r':
                        NextChar();
                        if (_ch is '\n')
                            NextChar();
                        _sy = Symbol.EOL;
                        _lineNo++;

                        break;

                    case '\n':
                        NextChar();
                        _sy = Symbol.EOL;
                        _lineNo++;

                        break;

                    // Comment
                    case '#':
                        NextChar();
                        while (_ch is not '\n' and not '\r' and > 0)
                            NextChar();

                        _sy = Symbol.Comment;
                        break;

                    // String
                    case '\'':
                    case '\"':
                        InStringSymbol();

                        break;

                    default:
                        throw new IT8Exception(_fileStack, _lineNo, $"Unrecognized character: 0x{_ch:x}");
                }
            }
        } while (_sy is Symbol.Comment);

        // Handle the include special token
        if (_sy is Symbol.Include)
        {
            if (_fileStack.Count > maxInclude - 1)
                throw new IT8Exception(_fileStack, _lineNo, "Too many recursion levels");

            InStringSymbol();
            Check(Symbol.String, "Filename expected");

            var file = File.OpenText(_str.ToString());
            _fileStack.Push(file);

            _ch = ' ';
            InSymbol();
        }
    }

    private void Skip(Symbol sy)
    {
        if (_sy == sy && _sy is not Symbol.EOF)
            InSymbol();
    }

    private void SkipEol()
    {
        while (_sy is Symbol.EOL)
            InSymbol();
    }

    private void Parse(bool nosheet)
    {
        if (nosheet)
            ReadType(out _sheetType);

        InSymbol();
        SkipEol();

        while (_sy is not Symbol.EOF and not Symbol.SynError)
        {
            switch (_sy)
            {
                case Symbol.BeginDataFormat:
                    ReadDataFormatSection();
                    break;

                case Symbol.BeginData:
                    ReadDataSection();

                    if (_sy is not Symbol.EOF)
                    {
                        _tables.Add(new());
                        _currentTable = _tables.Count - 1;

                        // Read sheet type if present. We only support identifier and string.
                        // <ident> <eol> is a type string anything else, is not a type string
                        if (!nosheet)
                        {
                            if (_sy is Symbol.Ident)
                            {
                                // May be a type sheet or may be a prop value statement. We cannot
                                // use InSymbol in this special case...
                                while (IsSeparator(_ch))
                                    NextChar();

                                // If a newline is found, then this is a type string
                                if (_ch is '\n' or '\r')
                                {
                                    _sheetType = _id.ToString();
                                    InSymbol();
                                } else
                                {
                                    _sheetType = "";
                                }
                            } else if (_sy is Symbol.String)
                            {
                                // Validate quoted strings
                                _sheetType = _str.ToString();
                                InSymbol();
                            }
                        }

                    }

                    break;

                case Symbol.EOL:
                    SkipEol();
                    break;

                default:
                    ReadHeaderSection();
                    break;
            }
        }
    }

    internal IT8? LoadFromFile(string filename)
    {
        using var fs = File.OpenRead(filename);
        return Load(fs);
    }

    internal IT8? LoadFromMemory(byte[] buffer)
    {
        using var fs = new MemoryStream(buffer);
        return Load(fs);
    }

    private IT8? Load(Stream fs)
    {
        var type = IsMyFormat(fs);
        if (!type) return null;

        var it8 = new IT8();

        _fileStack.Push(new StreamReader(fs, Encoding.ASCII));

        Parse(!type);
        it8.SheetType = _sheetType;
        _tables.Cook();

        it8.tables.AddRange(_tables);
        it8.SetTable(0);
        it8.availableProperties.AddRange(_availableProperties);
        it8.availableSampleId.AddRange(_availableSampleIds);

        return it8;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                while (_fileStack.Count > 0)
                    _fileStack.Pop().Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
