using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class DataHandler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion => 0;

    public object? Duplicate(ITagTypeHandler handler, object value, int num) =>
        ((IccData)value).Clone();

    public void Free(ITagTypeHandler handler, object value) =>
        ((IccData)value).Dispose();

    public object? Read(ITagTypeHandler handler, Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        if (sizeOfTag < sizeof(uint)) return null;

        var lenOfData = sizeOfTag - sizeof(uint);
        if (lenOfData < 0) return null;

        if (!io.ReadUInt32Number(out var flag)) return null;
        
        var buf = new byte[lenOfData];
        if (io.Read(buf) != lenOfData) return null;

        numItems = 1;

        return new IccData((uint)lenOfData, flag, buf);
    }

    public bool Write(ITagTypeHandler handler, Stream io, object value, int numItems)
    {
        var binData = (IccData)value;

        if (!io.Write(binData.Flag)) return false;

        io.Write(binData.Data, 0, (int)binData.Length);

        return true;
    }
}
