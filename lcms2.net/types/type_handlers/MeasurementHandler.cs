using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;

public class MeasurementHandler: TagTypeHandler
{
    public MeasurementHandler(Signature sig, Context? context = null)
        : base(sig, context, 0) { }

    public MeasurementHandler(Context? context = null)
        : this(default, context) { }

    public override object? Duplicate(object value, int num) =>
        (value as IccMeasurementConditions)?.Clone();

    public override void Free(object value)
    { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        var mc = new IccMeasurementConditions();

        if (!io.ReadUInt32Number(out mc.Observer)) return null;
        if (!io.ReadXYZNumber(out mc.Backing)) return null;
        if (!io.ReadUInt32Number(out mc.Geometry)) return null;
        if (!io.Read15Fixed16Number(out mc.Flare)) return null;
        if (!io.ReadUInt32Number(out var it)) return null;
        mc.IlluminantType = (IlluminantType)it;

        numItems = 1;
        return mc;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var mc = (IccMeasurementConditions)value;

        if (!io.Write(mc.Observer)) return false;
        if (!io.Write(mc.Backing)) return false;
        if (!io.Write(mc.Geometry)) return false;
        if (!io.Write(mc.Flare)) return false;
        if (!io.Write((uint)mc.IlluminantType)) return false;

        return true;
    }
}
