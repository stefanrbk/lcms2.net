using lcms2.plugins;

namespace lcms2.state;

public sealed class Context
{
    #region Fields

    private Context? next;
    internal object?[] chunks = new object[(int)Chunks.Max];

    #endregion Fields

    #region Constructors

    private Context()
    { }

    #endregion Constructors

    #region Statics

    #region Variables

    private static readonly Context globalContext = new()
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
    private static Context? poolHead = null;
    private static readonly object poolHeadMutex = new();

    #endregion Variables

    #region Static Methods

    public static Context? Create(Plugin? plugin, object? userData)
    {
        var ctx = new Context();

        lock (poolHeadMutex)
        {
            ctx.next = poolHead;
            poolHead = ctx;
        }

        ctx.chunks[(int)Chunks.UserPtr] = userData;

        LogErrorHandler.Alloc(ref ctx, null);
        AlarmCodes.Alloc(ref ctx, null);
        AdaptationState.Alloc(ref ctx, null);
        InterpolationPluginChunk.Alloc(ref ctx, null);
        ParametricCurvesPluginChunk.Alloc(ref ctx, null);
        FormattersPluginChunk.Alloc(ref ctx, null);
        TagTypePluginChunk.TagType.Alloc(ref ctx, null);
        TagTypePluginChunk.MPE.Alloc(ref ctx, null);
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
            lock (poolHeadMutex)
            {
                if (poolHead == ctx)
                {
                    poolHead = ctx.next;
                }
                else
                {
                    // Search for previous
                    for (var prev = poolHead; prev is not null; prev = prev.next)
                    {
                        if (prev.next == ctx)
                        {
                            prev.next = ctx.next;
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

        lock (poolHeadMutex)
        {
            ctx.next = poolHead;
            poolHead = ctx;
        }

        ctx.chunks[(int)Chunks.UserPtr] = userData;

        LogErrorHandler.Alloc(ref ctx, src);
        AlarmCodes.Alloc(ref ctx, src);
        AdaptationState.Alloc(ref ctx, src);
        InterpolationPluginChunk.Alloc(ref ctx, src);
        ParametricCurvesPluginChunk.Alloc(ref ctx, src);
        FormattersPluginChunk.Alloc(ref ctx, src);
        TagTypePluginChunk.TagType.Alloc(ref ctx, src);
        TagPluginChunk.Alloc(ref ctx, src);
        RenderingIntentsPluginChunk.Alloc(ref ctx, src);
        TagTypePluginChunk.MPE.Alloc(ref ctx, src);
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

    public static object? GetUserData(Context? context) =>
        GetClientChunk(context, Chunks.UserPtr);

    internal static InterpolationPluginChunk GetInterpolationPlugin(Context? context) =>
        (InterpolationPluginChunk)GetClientChunk(context, Chunks.InterpPlugin)!;

    internal static TagTypePluginChunk GetTagTypePlugin(Context? context) =>
        (TagTypePluginChunk)GetClientChunk(context, Chunks.TagTypePlugin)!;

    internal static TagPluginChunk GetTagPlugin(Context? context) =>
        (TagPluginChunk)GetClientChunk(context, Chunks.TagPlugin)!;

    internal static FormattersPluginChunk GetFormattersPlugin(Context? context) =>
        (FormattersPluginChunk)GetClientChunk(context, Chunks.FormattersPlugin)!;

    internal static RenderingIntentsPluginChunk GetRenderingIntentsPlugin(Context? context) =>
        (RenderingIntentsPluginChunk)GetClientChunk(context, Chunks.IntentPlugin)!;

    internal static ParametricCurvesPluginChunk GetCurvesPlugin(Context? context) =>
        (ParametricCurvesPluginChunk)GetClientChunk(context, Chunks.CurvesPlugin)!;

    internal static TagTypePluginChunk GetMultiProcessElementPlugin(Context? context) =>
        (TagTypePluginChunk)GetClientChunk(context, Chunks.MPEPlugin)!;

    internal static OptimizationPluginChunk GetOptimizationPlugin(Context? context) =>
        (OptimizationPluginChunk)GetClientChunk(context, Chunks.OptimizationPlugin)!;

    internal static TransformPluginChunk GetTransformPlugin(Context? context) =>
        (TransformPluginChunk)GetClientChunk(context, Chunks.TransformPlugin)!;

    internal static MutexPluginChunk GetMutexPlugin(Context? context) =>
        (MutexPluginChunk)GetClientChunk(context, Chunks.MutexPlugin)!;

    public static object? GetClientChunk(Context? context, Chunks chunk)
    {
        if (chunk is < 0 or >= Chunks.Max)
        {
            SignalError(context, ErrorCode.Internal, "Bad context chunk -- possible corruption");

            return globalContext.chunks[(int)Chunks.UserPtr]!;
        }

        var ctx = Get(context);
        var ptr = ctx.chunks[(int)chunk];

        if (ptr is not null)
            return ptr;

        // A null ptr means no special settings for that context, and this
        // reverts to globals
        return globalContext.chunks[(int)chunk];
    }

    /// <summary>
    /// Log an error.
    /// </summary>
    /// <param name="errorText">English description of the error.</param>
    /// <remarks>Implements the <c>cmsSignalError</c> function.</remarks>
    public static void SignalError(Context? context, ErrorCode errorCode, string errorText, params object?[] args) =>
        SignalError(context, errorCode, String.Format(errorText, args));

    /// <summary>
    /// Log an error.
    /// </summary>
    /// <param name="errorText">English description of the error.</param>
    public static void SignalError(Context? context, ErrorCode errorCode, string errorText)
    {
        // Check for the context, if specified go there. If not, go for the global
        LogErrorHandler lhg = (LogErrorHandler)GetClientChunk(context, Chunks.Logger)!;
        if (lhg.handler is not null)
            lhg.handler(context, errorCode, errorText);
    }

    // Helps prevent deleted contexts from being used by searching for the context in the list of active contexts and returning the global context if not found.
    internal static Context Get(Context? context)
    {
        // On null, use global settings
        if (context is null)
            return globalContext;

        // Search
        lock (poolHeadMutex)
        {
            for (var ctx = poolHead; ctx is not null; ctx = ctx.next)
            {
                // Found it?
                if (context == ctx)
                {
                    return ctx;
                }
            }
        }
        return globalContext;
    }

    #endregion Static Methods

    #endregion Statics
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
