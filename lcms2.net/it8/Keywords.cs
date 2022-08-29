namespace lcms2.it8;

internal static class Keywords
{
    public static readonly Dictionary<string, Symbol> List = new(new KeyValuePair<string, Symbol>[]
        {
        new("$INCLUDE", Symbol.Include),    // This is an extension!
        new(".INCLUDE", Symbol.Include),    // This is an extension!

        new("BEGIN_DATA", Symbol.BeginData),
        new("BEGIN_DATA_FORMAT", Symbol.BeginDataFormat),
        new("DATA_FORMAT_IDENTIFIER", Symbol.DataFormatId),
        new("END_DATA", Symbol.SendData),
        new("END_DATA_FORMAT", Symbol.SendDataFormat),
        new("KEYWORD", Symbol.Keyword),
    });

    public static int Count =>
        List.Count;
}
