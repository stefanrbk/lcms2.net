namespace lcms2.types;

public class ICCData : ICloneable, IDisposable
{
    internal uint Length;
    internal uint Flag;
    internal byte[] Data;
    private bool disposed = false;

    internal ICCData(uint length, uint flag, byte[] data)
    {
        Length = length;
        Flag = flag;
        Data = data;
    }

    public object Clone() =>
        new ICCData(Length, Flag, (byte[])Data.Clone());

    public void Dispose()
    {
        if (!disposed)
            Data = null!;
        GC.SuppressFinalize(this);
    }
}
