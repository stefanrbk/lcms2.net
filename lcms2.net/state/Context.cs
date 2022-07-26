using lcms2.state.chunks;

namespace lcms2.state;

public unsafe sealed class Context
{
    private Context? next = null;
    internal object?[] chunks = new object[(int)Chunks.Max];
    private readonly object mutex = new();

    internal static Context Create(object? plugin, object? userData)
    {
        var ctx = new Context();

        lock (ctx.mutex)
        {
            lock (globalContext)
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
            TagPlugin.Alloc(ref ctx, null);
            IntentsPlugin.Alloc(ref ctx, null);
            TagTypePlugin.MPE.Alloc(ref ctx, null);
            OptimizationPlugin.Alloc(ref ctx, null);
            TransformPlugin.Alloc(ref ctx, null);
            MutexPlugin.Alloc(ref ctx, null);
        }

        return ctx;
    }

    private Context() { }

    private static Context globalContext = new()
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
}
internal enum Chunks
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
