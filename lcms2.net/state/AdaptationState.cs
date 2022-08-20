namespace lcms2.state;

internal sealed class AdaptationState
{
    internal static readonly double defaultAdaptationState = 1.0;
    internal static AdaptationState global = new(defaultAdaptationState);
    internal double adaptationState;

    private static readonly AdaptationState _adaptationStateChunk = new(defaultAdaptationState);

    private AdaptationState(double value) =>
            adaptationState = value;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        AdaptationState from = (AdaptationState?)src?.chunks[(int)Chunks.AdaptationStateContext] ?? _adaptationStateChunk;

        ctx.chunks[(int)Chunks.AdaptationStateContext] = from;
    }
}
