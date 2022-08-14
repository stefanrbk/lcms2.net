namespace lcms2.types;
public class UcrBg : ICloneable, IDisposable
{
    public ToneCurve Ucr;
    public ToneCurve Bg;
    public Mlu Description;

    private bool disposed;

    public UcrBg(ToneCurve ucr, ToneCurve bg, Mlu description)
    {
        Ucr = ucr;
        Bg = bg;
        Description = description;

        disposed = false;
    }

    public object Clone() =>
        new UcrBg((ToneCurve)Ucr.Clone(), (ToneCurve)Bg.Clone(), (Mlu)Description.Clone());

    public void Dispose()
    {
        if (!disposed) {
            Ucr?.Dispose();
            Bg?.Dispose();
            Description?.Dispose();

            disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
