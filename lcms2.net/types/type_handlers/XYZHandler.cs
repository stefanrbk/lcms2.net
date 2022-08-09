using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class XYZHandler : ITagTypeHandler
{
    public Signature Signature =>
        Signature.TagType.XYZ;
    public Context? Context { get; }
    public uint ICCVersion => 0;

    public XYZHandler(Context? context = null) =>
        Context = Context.Get(context);

    public object? Duplicate(object value, int num) =>
        ((XYZ)value).Clone();

    public void Free(object value) { }

    public object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        if (!io.ReadXYZNumber(out var xyz)) return null;

        numItems = 1;
        return xyz;
    }

    public bool Write(Stream io, object value, int numItems) =>
        io.Write((XYZ)value);
}
