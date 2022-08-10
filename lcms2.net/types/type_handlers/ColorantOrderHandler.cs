
using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class ColorantOrderHandler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion => 0;

    public object? Duplicate(object value, int num) =>
        ((byte[])value).Clone();
    public void Free(object value) { }
    public object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        if (!io.ReadUInt32Number(out var count)) return null;
        if (count > Lcms2.MaxChannels) return null;

        byte[] colorantOrder = new byte[Lcms2.MaxChannels];

        // We use FF as end marker
        for (var i = 0; i < Lcms2.MaxChannels; i++)
            colorantOrder[i] = 0xFF;

        if (io.Read(colorantOrder, 0, (int)count) != count) return null;

        numItems = 1;
        return colorantOrder;
    }
    public bool Write(Stream io, object value, int numItems)
    {
        var colorantOrder = (byte[])value;
        int count;

        // Get the length
        for (var i = count = 0; i < Lcms2.MaxChannels; i++)
            if (colorantOrder[i] != 0xFF) count++;

        if (!io.Write(count)) return false;

        var sz = count * sizeof(byte);
        io.Write(colorantOrder, 0, sz);

        return true;
    }
}
