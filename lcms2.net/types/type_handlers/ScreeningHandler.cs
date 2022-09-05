using lcms2.io;
using lcms2.plugins;

using static lcms2.Lcms2;

namespace lcms2.types.type_handlers;

public class ScreeningHandler: TagTypeHandler
{
    public ScreeningHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public ScreeningHandler(object? state = null)
        : this(default, state) { }

    public override object? Duplicate(object value, int num) =>
        (value as Screening)?.Clone();

    public override void Free(object value)
    { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        if (!io.ReadUInt32Number(out var flag)) return null;
        if (!io.ReadUInt32Number(out var count)) return null;

        Screening sc = new(flag, (int)count);
        if (sc.NumChannels > maxChannels - 1)
            sc.NumChannels = maxChannels - 1;

        for (var i = 0; i < sc.NumChannels; i++)
        {
            if (!io.Read15Fixed16Number(out sc.Channels[i].Frequency)) return null;
            if (!io.Read15Fixed16Number(out sc.Channels[i].ScreenAngle)) return null;
            if (!io.ReadUInt32Number(out var shape)) return null;
            sc.Channels[i].SpotShape = (SpotShape)shape;
        }

        numItems = 1;
        return sc;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var sc = (Screening)value;

        if (!io.Write(sc.Flags)) return false;
        if (!io.Write(sc.NumChannels)) return false;

        for (var i = 0; i < sc.NumChannels; i++)
        {
            if (!io.Write(sc.Channels[i].Frequency)) return false;
            if (!io.Write(sc.Channels[i].ScreenAngle)) return false;
            if (!io.Write((uint)sc.Channels[i].SpotShape)) return false;
        }

        return true;
    }
}
