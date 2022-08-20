using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;

public class DataHandler: TagTypeHandler
{
    public DataHandler(Signature sig, Context? context = null)
        : base(sig, context, 0) { }

    public DataHandler(Context? context = null)
        : this(default, context) { }

    public override object? Duplicate(object value, int num) =>
        ((IccData)value).Clone();

    public override void Free(object value) =>
        ((IccData)value).Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
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

    public override bool Write(Stream io, object value, int numItems)
    {
        var binData = (IccData)value;

        if (!io.Write(binData.flag)) return false;

        io.Write(binData.data, 0, (int)binData.length);

        return true;
    }
}
