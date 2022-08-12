using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class SignatureHandler : TagTypeHandler
{
    public SignatureHandler(Context? context = null)
        : base(default, context, 0) { }

    public override object? Duplicate(object value, int num) =>
        ((Signature)value).Clone();

    public override void Free(object value) { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        if (!io.ReadUInt32Number(out var value)) return null;

        numItems = 1;
        return new Signature(value);
    }

    public override bool Write(Stream io, object value, int numItems) =>
        io.Write((Signature)value);
}
