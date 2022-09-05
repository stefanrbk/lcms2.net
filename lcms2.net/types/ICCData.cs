namespace lcms2.types;

public class IccData: ICloneable, IDisposable
{
    internal byte[] data;

    internal uint flag;

    internal uint length;

    private bool disposed = false;

    internal IccData(uint length, uint flag, byte[] data)
    {
        this.length = length;
        this.flag = flag;
        this.data = data;
    }

    public object Clone() =>
                        new IccData(length, flag, (byte[])data.Clone());

    public void Dispose()
    {
        if (!disposed)
            data = null!;
        GC.SuppressFinalize(this);
    }
}
