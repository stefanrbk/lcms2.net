namespace lcms2.state;

public delegate void LogErrorHandlerFunction(object? context, ErrorCode errorCode, string text);

public enum ErrorCode
{
    Undefined,
    File,
    Range,
    Internal,
    Null,
    Read,
    Seek,
    Write,
    UnknownExtension,
    ColorspaceCheck,
    AlreadyDefined,
    BadSignature,
    CorruptionDetected,
    NotSuitable,
}

internal sealed class LogErrorHandler
{
    internal static LogErrorHandler global = new() { handler = DefaultLogErrorHandlerFunction };

    // Set to null for global fallback
    internal LogErrorHandlerFunction? handler;

    internal static LogErrorHandler Default => new() { handler = DefaultLogErrorHandlerFunction };

    private LogErrorHandler()
    { }

    internal static void DefaultLogErrorHandlerFunction(object? _context, ErrorCode _errorCode, string _text)
    { }
}
