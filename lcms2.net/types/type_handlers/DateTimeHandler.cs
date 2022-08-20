using System.Runtime.InteropServices;

using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;

public class DateTimeHandler: TagTypeHandler
{
    public DateTimeHandler(Signature sig, Context? context = null)
        : base(sig, context, 0) { }

    public DateTimeHandler(Context? context = null)
        : this(default, context) { }

    public override object? Duplicate(object value, int num) =>
        (DateTime)value;

    public override void Free(object value)
    { }

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        try
        {
            var buf = new byte[sizeof(DateTimeNumber)];
            if (io.Read(buf) != sizeof(DateTimeNumber)) return null;
            var dt = MemoryMarshal.Read<DateTimeNumber>(buf);

            numItems = 1;
            return (DateTime)dt;
        } catch
        {
            return null;
        }
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        var dt = (DateTime)value;
        var timestamp = (DateTimeNumber)dt;
        try
        {
            var buf = new byte[sizeof(DateTimeNumber)];
            MemoryMarshal.Write(buf, ref timestamp);
            io.Write(buf);
        } catch
        {
            return false;
        }
        return true;
    }
}
