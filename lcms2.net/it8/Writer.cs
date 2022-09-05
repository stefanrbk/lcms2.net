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
using System.Text;

namespace lcms2.it8;

internal class Writer
{
    #region Fields

    private readonly IT8 _it8;

    #endregion Fields

    #region Public Constructors

    public Writer(IT8 it8) =>
        _it8 = it8;

    #endregion Public Constructors

    #region Public Methods

    public void SaveToFile(string filename)
    {
        using var fs = File.OpenWrite(filename);
        Save(fs);
    }

    public long SaveToMemory(byte[] buffer)
    {
        using var ms = new MemoryStream(buffer);
        Save(ms);
        return ms.Position - 1;
    }

    public byte[] SaveToMemory()
    {
        using var ms = new MemoryStream();
        Save(ms);
        return ms.ToArray();
    }

    #endregion Public Methods

    #region Internal Methods

    internal void WriteData(StreamWriter writer)
    {
        var t = _it8.Table;

        if (t.data is null) return;

        writer.WriteLine("BEGIN_DATA");

        var numPatches = Int32.Parse(t.GetProperty("NUMBER_OF_SETS"));

        for (var i = 0; i < numPatches; i++)
        {
            writer.Write(" ");

            for (var j = 0; j < t.NumSamples; j++)
            {
                var s = t.data[(i * t.NumSamples) + j];

                if (String.IsNullOrEmpty(s)) writer.Write("\"\"");
                else
                {
                    // If value contains whitespace, enclose within quote
                    if (s.Contains(' '))
                    {
                        writer.Write("\"");
                        writer.Write(s);
                        writer.Write("\"");
                    }
                    else
                        writer.Write(s);
                }

                writer.Write((j == (t.NumSamples - 1)) ? "\n" : "\t");
            }
        }
        writer.WriteLine("END_DATA");
    }

    internal void WriteDataFormat(StreamWriter writer)
    {
        var t = _it8.Table;
        if (t.dataFormat is null) return;

        writer.WriteLine("BEGIN_DATA_FORMAT");
        writer.Write(" ");
        var numSamples = Int32.Parse(t.GetProperty("NUMBER_OF_FIELDS"));

        for (var i = 0; i < numSamples; i++)
        {
            writer.Write(t.dataFormat[i]);
            writer.Write((i == (numSamples - 1)) ? "\n" : "\t");
        }
        writer.WriteLine("END_DATA_FORMAT");
    }

    #endregion Internal Methods

    #region Private Methods

    private void Save(Stream stream)
    {
        using var writer = new StreamWriter(stream, Encoding.ASCII);

        for (var i = 0; i < _it8.TableCount; i++)
        {
            _it8.SetTable(i);

            WriteHeader(writer);
            WriteDataFormat(writer);
            WriteData(writer);
        }
    }

    private void WriteHeader(StreamWriter writer)
    {
        writer.WriteLine(_it8.SheetType);

        foreach (var p in _it8.Table.header)
        {
            if (p.Key[0] is '#')
            {
                writer.WriteLine('#');
                writer.Write("# ");

                var pt = new StringBuilder(p.Value);
                writer.Write(pt.Replace("\n", "\n# ").ToString());

                writer.WriteLine();
                writer.WriteLine('#');
                continue;
            }

            writer.Write(p.Key);
            if (p.Value is not null)
            {
                switch (p.Mode)
                {
                    case WriteMode.Uncooked:
                        writer.WriteLine($"\t{p.Value}");
                        break;

                    case WriteMode.Stringify:
                        writer.WriteLine($"\t\"{p.Value}\"");
                        break;

                    case WriteMode.Hexadecimal:
                        writer.WriteLine($"\t0x{p.Value:x}");
                        break;

                    case WriteMode.Binary:
                        writer.WriteLine($"\t0b{IT8.StringToBinary(p.Value)}");
                        break;

                    case WriteMode.Pair:
                        writer.WriteLine($"\t\"{p.Subkey!},{p.Value}\"");
                        break;

                    default:
                        throw new IT8Exception($"Unknown write mode {p.Mode}");
                }
            }
        }
    }

    #endregion Private Methods
}
