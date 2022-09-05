namespace lcms2.types;

public class UcrBg: ICloneable, IDisposable
{
    public ToneCurve Bg;
    public Mlu Description;
    public ToneCurve Ucr;

    private bool _disposed;

    public UcrBg(ToneCurve ucr, ToneCurve bg, Mlu description)
    {
        Ucr = ucr;
        Bg = bg;
        Description = description;

        _disposed = false;
    }

    public object Clone() =>
        new UcrBg((ToneCurve)Ucr.Clone(), (ToneCurve)Bg.Clone(), (Mlu)Description.Clone());

    public void Dispose()
    {
        if (!_disposed)
        {
            Ucr?.Dispose();
            Bg?.Dispose();
            Description?.Dispose();

            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
