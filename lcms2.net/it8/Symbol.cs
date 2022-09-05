namespace lcms2.it8;
public enum Symbol
{
    Undefined,
    /// <summary>
    /// Integer
    /// </summary>
    Inum,
    /// <summary>
    /// Real
    /// </summary>
    Dnum,
    /// <summary>
    /// Identifier
    /// </summary>
    Ident,
    /// <summary>
    /// string
    /// </summary>
    String,
    /// <summary>
    /// comment
    /// </summary>
    Comment,
    /// <summary>
    /// End of line
    /// </summary>
    EOL,
    /// <summary>
    /// End of stream
    /// </summary>
    EOF,
    /// <summary>
    /// Syntax error found on stream
    /// </summary>
    SynError,

    // Keywords

    BeginData,
    BeginDataFormat,
    SendData,
    SendDataFormat,
    Keyword,
    DataFormatId,
    Include,
}
