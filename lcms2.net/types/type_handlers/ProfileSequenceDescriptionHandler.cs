using lcms2.io;
using lcms2.plugins;

namespace lcms2.types.type_handlers;

public class ProfileSequenceDescriptionHandler: TagTypeHandler
{
    public ProfileSequenceDescriptionHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public ProfileSequenceDescriptionHandler(object? state = null)
        : this(default, state) { }

    public override object? Duplicate(object value, int num) =>
        (value as Sequence)?.Clone();

    public override void Free(object value) =>
        (value as Sequence)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        if (!io.ReadUInt32Number(out var count)) return null;

        if (sizeOfTag < sizeof(uint)) return null;
        sizeOfTag -= sizeof(uint);

        Sequence outSeq = new(StateContainer, (int)count);

        // Get structures as well

        for (var i = 0; i < count; i++)
        {
            if (!io.ReadUInt32Number(out var deviceMfg)) goto Error;
            if (sizeOfTag < sizeof(uint)) goto Error;
            sizeOfTag -= sizeof(uint);

            if (!io.ReadUInt32Number(out var deviceModel)) goto Error;
            if (sizeOfTag < sizeof(uint)) goto Error;
            sizeOfTag -= sizeof(uint);

            if (!io.ReadUInt64Number(out var attributes)) goto Error;
            if (sizeOfTag < sizeof(ulong)) goto Error;
            sizeOfTag -= sizeof(ulong);

            if (!io.ReadUInt32Number(out var technology)) goto Error;
            if (sizeOfTag < sizeof(uint)) goto Error;
            sizeOfTag -= sizeof(uint);

            var sec = new ProfileSequenceDescription(StateContainer, new Signature(deviceMfg), new Signature(deviceModel), attributes, new Signature(technology), default);

            if (!ReadEmbeddedText(io, ref sec.Manufacturer, sizeOfTag)) goto Error;
            if (!ReadEmbeddedText(io, ref sec.Model, sizeOfTag)) goto Error;
        }

        numItems = 1;
        return outSeq;

    Error:

        outSeq?.Dispose();

        return null;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var seq = (Sequence)value;

        if (!io.Write(seq.SeqCount)) return false;

        for (var i = 0; i < seq.SeqCount; i++)
        {
            var sec = seq.Seq[i];

            if (!io.Write(sec.DeviceMfg)) return false;
            if (!io.Write(sec.DeviceModel)) return false;
            if (!io.Write(sec.Attributes)) return false;
            if (!io.Write(sec.Technology)) return false;

            if (!SaveDescription(io, sec.Manufacturer)) return false;
            if (!SaveDescription(io, sec.Model)) return false;
        }

        return true;
    }
}
