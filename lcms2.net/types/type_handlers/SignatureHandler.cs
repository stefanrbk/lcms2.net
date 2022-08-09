using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class SignatureHandler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion => 0;

    public object? Duplicate(ITagTypeHandler handler, object value, int num) =>
        ((Signature)value).Clone();

    public void Free(ITagTypeHandler handler, object value) { }

    public object? Read(ITagTypeHandler handler, Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        if (!io.ReadUInt32Number(out var value)) return null;

        numItems = 1;
        return new Signature(value);
    }

    public bool Write(ITagTypeHandler handler, Stream io, object value, int numItems) =>
        io.Write((Signature)value);
}
