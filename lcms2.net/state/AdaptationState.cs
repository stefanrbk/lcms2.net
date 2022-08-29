namespace lcms2.state;

internal sealed class AdaptationState
{
    internal static readonly double defaultAdaptationState = 1.0;
    internal static AdaptationState global = new(defaultAdaptationState);
    internal double adaptationState;
    internal static AdaptationState Default => new(defaultAdaptationState);

    private AdaptationState(double value) =>
            adaptationState = value;
}
