using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;

public class MpeStubHandler: TagTypeHandler
{
    public MpeStubHandler(Signature signature, Context? context = null)
        : base(signature, context, 0) { }

    public override object? Duplicate(object value, int num) =>
        null;

    public override void Free(object value)
    { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        return null;
    }

    public override bool Write(Stream io, object value, int numItems) =>
        true;
}
