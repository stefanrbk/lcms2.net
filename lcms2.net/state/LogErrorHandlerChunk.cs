namespace lcms2.state;

public delegate void LogErrorHandlerFunction(ref Context context, ErrorCode errorCode, string text);

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

internal class LogErrorHandlerChunk
{
    private LogErrorHandlerFunction? handler = null; // Set to null for global fallback

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null ? (LogErrorHandlerChunk?)src.chunks[(int)Chunks.Logger] : logErrorChunk;

        ctx.chunks[(int)Chunks.Logger] = from;
    }

    private LogErrorHandlerChunk() { }

    internal static LogErrorHandlerChunk global = new() { handler = DefaultLogErrorHandlerFunction };
    private readonly static LogErrorHandlerChunk logErrorChunk = new() { handler = DefaultLogErrorHandlerFunction };

    internal static void DefaultLogErrorHandlerFunction(ref Context _context, ErrorCode _errorCode, string _text) { }
}
