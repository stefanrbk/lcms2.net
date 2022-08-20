using lcms2.plugins;

namespace lcms2.state;

public sealed class Context
{
    internal object?[] chunks = new object[(int)Chunks.Max];

    private static readonly Context _globalContext = new()
    {
        chunks = new object?[(int)Chunks.Max]
        {
            null,
            LogErrorHandler.global,
            AlarmCodes.global,
            AdaptationState.global,
            InterpolationPluginChunk.global,
            ParametricCurvesPluginChunk.global,
            FormattersPluginChunk.global,
            TagTypePluginChunk.TagType.global,
            TagPluginChunk.global,
            RenderingIntentsPluginChunk.global,
            TagTypePluginChunk.MPE.global,
            OptimizationPluginChunk.global,
            TransformPluginChunk.global,
            MutexPluginChunk.global,
        }
    };

    private static readonly object _poolHeadMutex = new();

    private static Context? _poolHead = null;

    private Context? _next;

    private Context()
    { }

    public static Context? Create(Plugin? plugin, object? userData)
    {
        var ctx = new Context();

        lock (_poolHeadMutex)
        {
            ctx._next = _poolHead;
            _poolHead = ctx;
        }

        ctx.chunks[(int)Chunks.UserPtr] = userData;

        LogErrorHandler.Alloc(ref ctx, null);
        AlarmCodes.Alloc(ref ctx, null);
        AdaptationState.Alloc(ref ctx, null);
        InterpolationPluginChunk.Alloc(ref ctx, null);
        ParametricCurvesPluginChunk.Alloc(ref ctx, null);
        FormattersPluginChunk.Alloc(ref ctx, null);
        TagTypePluginChunk.TagType.Alloc(ctx, null);
        TagTypePluginChunk.MPE.Alloc(ctx, null);
        TagPluginChunk.Alloc(ref ctx, null);
        RenderingIntentsPluginChunk.Alloc(ref ctx, null);
        OptimizationPluginChunk.Alloc(ref ctx, null);
        TransformPluginChunk.Alloc(ref ctx, null);
        MutexPluginChunk.Alloc(ref ctx, null);

        // TODO add plugin support
        return !Plugin.Register(ctx, plugin)
            ? null
            : ctx;
    }

    public static void Delete(Context? ctx)
    {
        if (ctx is not null)
        {
            Plugin.UnregisterAll(ctx);

            // Maintain list
            lock (_poolHeadMutex)
            {
                if (_poolHead == ctx)
                {
                    _poolHead = ctx._next;
                } else
                {
                    // Search for previous
                    for (var prev = _poolHead; prev is not null; prev = prev._next)
                    {
                        if (prev._next == ctx)
                        {
                            prev._next = ctx._next;
                            break;
                        }
                    }
                }
            }
        }
    }

    public static Context? Duplicate(Context? context, object? newUserData)
    {
        var src = Get(context);

        var userData = newUserData ?? src.chunks[(int)Chunks.UserPtr];

        var ctx = new Context();

        lock (_poolHeadMutex)
        {
            ctx._next = _poolHead;
            _poolHead = ctx;
        }

        ctx.chunks[(int)Chunks.UserPtr] = userData;

        LogErrorHandler.Alloc(ref ctx, src);
        AlarmCodes.Alloc(ref ctx, src);
        AdaptationState.Alloc(ref ctx, src);
        InterpolationPluginChunk.Alloc(ref ctx, src);
        ParametricCurvesPluginChunk.Alloc(ref ctx, src);
        FormattersPluginChunk.Alloc(ref ctx, src);
        TagTypePluginChunk.TagType.Alloc(ctx, src);
        TagPluginChunk.Alloc(ref ctx, src);
        RenderingIntentsPluginChunk.Alloc(ref ctx, src);
        TagTypePluginChunk.MPE.Alloc(ctx, src);
        OptimizationPluginChunk.Alloc(ref ctx, src);
        TransformPluginChunk.Alloc(ref ctx, src);
        MutexPluginChunk.Alloc(ref ctx, src);

        // Make sure no one failed
        for (var i = Chunks.Logger; i < Chunks.Max; i++)
        {
            if (ctx.chunks[(int)i] is null)
            {
                Delete(ctx);
                return null;
            }
        }

        return ctx;
    }

    public static object? GetClientChunk(Context? context, Chunks chunk)
    {
        if (chunk is < 0 or >= Chunks.Max)
        {
            SignalError(context, ErrorCode.Internal, "Bad context chunk -- possible corruption");

            return _globalContext.chunks[(int)Chunks.UserPtr]!;
        }

        var ctx = Get(context);
        var ptr = ctx.chunks[(int)chunk];

        if (ptr is not null)
            return ptr;

        // A null ptr means no special settings for that context, and this reverts to globals
        return _globalContext.chunks[(int)chunk];
    }

    public static object? GetUserData(Context? context) =>
        GetClientChunk(context, Chunks.UserPtr);

    /// <summary>
    ///     Log an error.
    /// </summary>
    /// <param name="errorText">English description of the error.</param>
    /// <remarks>Implements the <c>cmsSignalError</c> function.</remarks>
    public static void SignalError(Context? context, ErrorCode errorCode, string errorText, params object?[] args) =>
        SignalError(context, errorCode, String.Format(errorText, args));

    /// <summary>
    ///     Log an error.
    /// </summary>
    /// <param name="errorText">English description of the error.</param>
    public static void SignalError(Context? context, ErrorCode errorCode, string errorText)
    {
        // Check for the context, if specified go there. If not, go for the global
        LogErrorHandler lhg = (LogErrorHandler)GetClientChunk(context, Chunks.Logger)!;
        if (lhg.handler is not null)
            lhg.handler(context, errorCode, errorText);
    }

    // Helps prevent deleted contexts from being used by searching for the context in the list of
    // active contexts and returning the global context if not found.
    internal static Context Get(Context? context)
    {
        // On null, use global settings
        if (context is null)
            return _globalContext;

        // Search
        lock (_poolHeadMutex)
        {
            for (var ctx = _poolHead; ctx is not null; ctx = ctx._next)
            {
                // Found it?
                if (context == ctx)
                {
                    return ctx;
                }
            }
        }
        return _globalContext;
    }

    internal static ParametricCurvesPluginChunk GetCurvesPlugin(Context? context) =>
        (ParametricCurvesPluginChunk)GetClientChunk(context, Chunks.CurvesPlugin)!;

    internal static FormattersPluginChunk GetFormattersPlugin(Context? context) =>
        (FormattersPluginChunk)GetClientChunk(context, Chunks.FormattersPlugin)!;

    internal static InterpolationPluginChunk GetInterpolationPlugin(Context? context) =>
        (InterpolationPluginChunk)GetClientChunk(context, Chunks.InterpPlugin)!;

    internal static TagTypePluginChunk GetMultiProcessElementPlugin(Context? context) =>
        (TagTypePluginChunk)GetClientChunk(context, Chunks.MPEPlugin)!;

    internal static MutexPluginChunk GetMutexPlugin(Context? context) =>
        (MutexPluginChunk)GetClientChunk(context, Chunks.MutexPlugin)!;

    internal static OptimizationPluginChunk GetOptimizationPlugin(Context? context) =>
        (OptimizationPluginChunk)GetClientChunk(context, Chunks.OptimizationPlugin)!;

    internal static RenderingIntentsPluginChunk GetRenderingIntentsPlugin(Context? context) =>
        (RenderingIntentsPluginChunk)GetClientChunk(context, Chunks.IntentPlugin)!;

    internal static TagPluginChunk GetTagPlugin(Context? context) =>
        (TagPluginChunk)GetClientChunk(context, Chunks.TagPlugin)!;

    internal static TagTypePluginChunk GetTagTypePlugin(Context? context) =>
        (TagTypePluginChunk)GetClientChunk(context, Chunks.TagTypePlugin)!;

    internal static TransformPluginChunk GetTransformPlugin(Context? context) =>
        (TransformPluginChunk)GetClientChunk(context, Chunks.TransformPlugin)!;
}

public enum Chunks
{
    UserPtr,
    Logger,
    AlarmCodesContext,
    AdaptationStateContext,
    InterpPlugin,
    CurvesPlugin,
    FormattersPlugin,
    TagTypePlugin,
    TagPlugin,
    IntentPlugin,
    MPEPlugin,
    OptimizationPlugin,
    TransformPlugin,
    MutexPlugin,

    Max
}
