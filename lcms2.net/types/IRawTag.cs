namespace lcms2.types;
public interface IRawTag : ICloneable, IDisposable
{
    bool WriteRaw(Stream io);

    abstract static IRawTag ReadRaw(Stream io);
}
