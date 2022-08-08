namespace lcms2.state;

internal sealed class AdaptationState
{
    internal double adaptationState;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null ? (AdaptationState?)src.chunks[(int)Chunks.AdaptationStateContext] : adaptationStateChunk;

        ctx.chunks[(int)Chunks.AdaptationStateContext] = from;
    }

    private AdaptationState(double value) =>
        adaptationState = value;

    internal static AdaptationState global = new(DefaultAdaptationState);
    private static readonly AdaptationState adaptationStateChunk = new(DefaultAdaptationState);

    internal static readonly double DefaultAdaptationState = 1.0;
}
