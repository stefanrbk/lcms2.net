namespace lcms2.it8_template;
public struct Keyword
{
    public string Id;
    public Symbol Symbol;

    public Keyword(string id, Symbol symbol) =>
        (Id, Symbol) = (id, symbol);

    public static readonly Keyword[] TabKeys = new Keyword[]
    {
        new("$INCLUDE", Symbol.Include),    // This is an extension!
        new(".INCLUDE", Symbol.Include),    // This is an extension!

        new("BEGIN_DATA", Symbol.BeginData),
        new("BEGIN_DATA_FORMAT", Symbol.BeginDataFormat),
        new("DATA_FORMAT_IDENTIFIER", Symbol.DataFormatId),
        new("END_DATA", Symbol.SendData),
        new("END_DATA_FORMAT", Symbol.SendDataFormat),
        new("KEYWORD", Symbol.Keyword),
    };

    public static int NumKeys =>
        TabKeys.Length;
}

public enum Symbol
{
    Undefined,
    Inum,               // Integer
    Dnum,               // Real
    Ident,              // Identifier
    String,             // string
    Comment,            // comment
    EOL,                // End of line
    EOF,                // End of stream
    SynError,           // Syntax error found on stream

    // Keywords

    BeginData,
    BeginDataFormat,
    SendData,
    SendDataFormat,
    Keyword,
    DataFormatId,
    Include,
}
