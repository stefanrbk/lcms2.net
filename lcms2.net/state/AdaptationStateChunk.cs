namespace lcms2.state;

internal class AdaptationStateChunk
{
    private double adaptationState;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null ? (AdaptationStateChunk?)src.chunks[(int)Chunks.AdaptationStateContext] : adaptationStateChunk;

        ctx.chunks[(int)Chunks.Logger] = from;
    }

    private AdaptationStateChunk(double value) =>
        adaptationState = value;

    internal static AdaptationStateChunk global = new(DefaultAdaptationState);
    private readonly static AdaptationStateChunk adaptationStateChunk = new(DefaultAdaptationState);

    internal static readonly double DefaultAdaptationState = 1.0;
}
