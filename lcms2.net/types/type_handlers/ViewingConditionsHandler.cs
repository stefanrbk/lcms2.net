using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;

public class ViewingConditionsHandler: TagTypeHandler
{
    public ViewingConditionsHandler(Signature sig, Context? context = null)
        : base(sig, context, 0) { }

    public ViewingConditionsHandler(Context? context = null)
        : this(default, context) { }

    public override object? Duplicate(object value, int num) =>
        (value as IccViewingConditions)?.Clone();

    public override void Free(object value)
    { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        if (!io.ReadXYZNumber(out var illum)) return null;
        if (!io.ReadXYZNumber(out var surro)) return null;
        if (!io.ReadUInt32Number(out var type)) return null;

        numItems = 1;
        return new IccViewingConditions(illum, surro, (IlluminantType)type);
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var sc = (IccViewingConditions)value;

        if (!io.Write(sc.IlluminantXyz)) return false;
        if (!io.Write(sc.SurroundXyz)) return false;
        if (!io.Write((uint)sc.IlluminantType)) return false;

        return true;
    }
}
