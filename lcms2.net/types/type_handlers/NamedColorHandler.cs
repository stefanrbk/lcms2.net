using lcms2.io;
using lcms2.plugins;
using lcms2.state;

using static lcms2.Lcms2;

namespace lcms2.types.type_handlers;

public class NamedColorHandler: TagTypeHandler
{
    public NamedColorHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public NamedColorHandler(object? state = null)
        : this(default, state) { }

    public override object? Duplicate(object value, int num) =>
        (value as NamedColorList)?.Clone();

    public override void Free(object value) =>
        (value as NamedColorList)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        if (!io.ReadUInt32Number(out /*vendorFlag*/_)) return null;         // Bottom 16 bits for ICC use
        if (!io.ReadUInt32Number(out var count)) return null;           // Count of named colors
        if (!io.ReadUInt32Number(out var numDeviceCoords)) return null; // Num of device coordinates

        if (!io.ReadAsciiString(32, out var prefix)) return null;   // Prefix for each color name
        if (!io.ReadAsciiString(32, out var suffix)) return null;   // Suffix for each color name

        var v = new NamedColorList(StateContainer, count, prefix, suffix);
        if (v is null)
        {
            State.SignalError(StateContainer, ErrorCode.Range, "Too many named colors '{0}'", count);
            return null;
        }

        if (numDeviceCoords > MaxChannels)
        {
            State.SignalError(StateContainer, ErrorCode.Range, "Too many device coordinates '{0}'", numDeviceCoords);
            goto Error;
        }

        for (var i = 0; i < count; i++)
        {
            if (!io.ReadAsciiString(32, out var root)) goto Error;

            if (!io.ReadUInt16Array(3, out var pcs)) goto Error;
            if (!io.ReadUInt16Array((int)numDeviceCoords, out var colorant)) goto Error;

            if (!v.Append(root, pcs, colorant)) goto Error;
        }

        return v;

    Error:
        v?.Dispose();
        return null;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var namedColorList = (NamedColorList)value;

        var numColors = namedColorList.numColors;

        if (!io.Write((uint)0)) return false;
        if (!io.Write(numColors)) return false;
        if (!io.Write(namedColorList.colorantCount)) return false;

        if (!io.WriteAsciiString(namedColorList.prefix, 32)) return false;
        if (!io.WriteAsciiString(namedColorList.suffix, 32)) return false;

        for (var i = 0; i < numColors; i++)
        {
            if (!namedColorList.Info((uint)i, out var root, out _, out _, out var pcs, out var colorant)) return false;

            if (!io.WriteAsciiString(root, 32)) return false;
            if (!io.Write(3, pcs)) return false;
            if (!io.Write((int)namedColorList.colorantCount, colorant)) return false;
        }

        return true;
    }
}
