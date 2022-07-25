namespace lcms2.state;

internal class AdaptationState
{
    private double adaptationState;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null ? (AdaptationState?)src.chunks[(int)Chunks.AdaptationStateContext] : adaptationStateChunk;

        ctx.chunks[(int)Chunks.Logger] = from;
    }

    private AdaptationState(double value) =>
        adaptationState = value;

    internal static AdaptationState global = new(DefaultAdaptationState);
    private readonly static AdaptationState adaptationStateChunk = new(DefaultAdaptationState);

    internal static readonly double DefaultAdaptationState = 1.0;
}
