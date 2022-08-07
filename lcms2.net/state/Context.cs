using lcms2.plugins;
using lcms2.state.chunks;

namespace lcms2.state;

public sealed class Context
{
    #region Fields

    private Context? next = null;
    internal object?[] chunks = new object[(int)Chunks.Max];

    #endregion Fields

    #region Constructors

    private Context()
    { }

    #endregion Constructors

    #region Methods

    public void Delete()
    {
        // TODO Unregister plugins

        // Maintain list
        lock (poolHeadMutex)
        {
            if (poolHead == this)
            {
                poolHead = next;
            }
            else
            {
                // Search for previous
                for (var prev = poolHead; prev is not null; prev = prev.next)
                {
                    if (prev.next == this)
                    {
                        prev.next = this.next;
                        break;
                    }
                }
            }
        }
    }

    public Context? Duplicate(object? newUserData)
    {
        var ctx = new Context();

        lock (poolHeadMutex)
        {
            ctx.next = poolHead;
            poolHead = ctx;
        }

        ctx.chunks[(int)Chunks.UserPtr] = newUserData ?? chunks[(int)Chunks.UserPtr];

        LogErrorHandler.Alloc(ref ctx, this);
        AlarmCodes.Alloc(ref ctx, this);
        AdaptationState.Alloc(ref ctx, this);
        InterpPlugin.Alloc(ref ctx, this);
        CurvesPlugin.Alloc(ref ctx, this);
        FormattersPlugin.Alloc(ref ctx, this);
        TagTypePlugin.TagType.Alloc(ref ctx, this);
        TagPlugin.Alloc(ref ctx, this);
        IntentsPlugin.Alloc(ref ctx, this);
        TagTypePlugin.MPE.Alloc(ref ctx, this);
        OptimizationPlugin.Alloc(ref ctx, this);
        TransformPlugin.Alloc(ref ctx, this);
        MutexPlugin.Alloc(ref ctx, this);

        // Make sure no one failed
        for (var i = Chunks.Logger; i < Chunks.Max; i++)
        {
            if (ctx.chunks[(int)i] is null)
            {
                ctx.Delete();
                return null;
            }
        }

        return ctx;
    }

    #endregion Methods

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
            InterpPlugin.global,
            CurvesPlugin.global,
            FormattersPlugin.global,
            TagTypePlugin.TagType.global,
            TagPlugin.global,
            IntentsPlugin.global,
            TagTypePlugin.MPE.global,
            OptimizationPlugin.global,
            TransformPlugin.global,
            MutexPlugin.global,
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
        InterpPlugin.Alloc(ref ctx, null);
        CurvesPlugin.Alloc(ref ctx, null);
        FormattersPlugin.Alloc(ref ctx, null);
        TagTypePlugin.TagType.Alloc(ref ctx, null);
        TagTypePlugin.MPE.Alloc(ref ctx, null);
        TagPlugin.Alloc(ref ctx, null);
        IntentsPlugin.Alloc(ref ctx, null);
        OptimizationPlugin.Alloc(ref ctx, null);
        TransformPlugin.Alloc(ref ctx, null);
        MutexPlugin.Alloc(ref ctx, null);

        // TODO add plugin support
        return !Plugin.Register(ctx, plugin)
            ? null
            : ctx;
    }

    public static object? GetUserData(Context? context) =>
        GetClientChunk(context, Chunks.UserPtr);

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
