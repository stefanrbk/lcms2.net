using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;

public class ColorantOrderHandler: TagTypeHandler
{
    public ColorantOrderHandler(Signature sig, Context? context = null)
        : base(sig, context, 0) { }

    public ColorantOrderHandler(Context? context = null)
        : this(default, context) { }

    public override object? Duplicate(object value, int num) =>
        ((byte[])value).Clone();

    public override void Free(object value)
    { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
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

    public override bool Write(Stream io, object value, int numItems)
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
