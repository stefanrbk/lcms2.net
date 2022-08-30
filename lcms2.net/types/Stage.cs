namespace lcms2.types;

public sealed partial class Stage: ICloneable, IDisposable
{
    private uint _inputChan;
    private uint _outputChan;
    private bool _disposedValue;

    public object? StateContainer { get; internal set; }

    public StageData Data { get; internal set; }

    internal Signature implements;

    public uint InputChannels
    {
        get => _inputChan;
        internal set => _inputChan = value <= maxStageChannels ? value : _inputChan;
    }

    public Stage? Next;

    public uint OutputChannels
    {
        get => _outputChan;
        internal set => _outputChan = value <= maxStageChannels ? value : _outputChan;
    }

    public Signature Type { get; internal set; }

    internal ToneCurve[] CurveSet =>
        (Data as ToneCurveData)?.TheCurves ?? throw new InvalidOperationException();

    public object Clone()
    {
        var result = AllocPlaceholder(
            StateContainer,
            Type,
            InputChannels,
            OutputChannels,
            Data.Duplicate(this))!;
        result.implements = implements;

        return result;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
            }

            Data = null!;

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public enum StageLoc
{
    AtBegin,
    AtEnd,
}
