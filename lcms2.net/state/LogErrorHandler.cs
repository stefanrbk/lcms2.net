namespace lcms2.state;

public delegate void LogErrorHandlerFunction(Context? context, ErrorCode errorCode, string text);

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
    internal LogErrorHandlerFunction? handler;

    // Set to null for global fallback

    private static readonly LogErrorHandler _logErrorChunk = new() { handler = DefaultLogErrorHandlerFunction };

    private LogErrorHandler()
    { }

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        LogErrorHandler from = (LogErrorHandler?)src?.chunks[(int)Chunks.Logger] ?? _logErrorChunk;

        ctx.chunks[(int)Chunks.Logger] = from;
    }

    internal static void DefaultLogErrorHandlerFunction(Context? _context, ErrorCode _errorCode, string _text)
    { }
}
