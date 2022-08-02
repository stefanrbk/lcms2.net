namespace lcms2.types;

public class ICCData
{
    internal uint Length;
    internal uint Flag;

    internal byte[] Data { get; }

    internal ICCData(uint length, uint flag, byte[] data)
    {
        Length = length;
        Flag = flag;
        Data = data;
    }
}
