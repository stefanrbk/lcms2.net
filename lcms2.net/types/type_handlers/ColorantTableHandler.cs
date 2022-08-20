using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;

public class ColorantTableHandler: TagTypeHandler
{
    public ColorantTableHandler(Signature sig, Context? context = null)
        : base(sig, context, 0) { }

    public ColorantTableHandler(Context? context = null)
        : this(default, context) { }

    public override object? Duplicate(object value, int num) =>
        (value as NamedColorList)?.Clone();

    public override void Free(object value) =>
        (value as NamedColorList)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        byte[] name = new byte[33];

        if (!io.ReadUInt32Number(out var count)) return null;

        if (count > Lcms2.MaxChannels)
        {
            Context.SignalError(Context, ErrorCode.Range, "Too many colorants '{0}'", count);
            return null;
        }

        NamedColorList list = new(Context, count, "", "");

        for (var i = 0; i < count; i++)
        {
            if (io.Read(name, 0, 32) != 32) goto Error;

            if (!io.ReadUInt16Array(3, out var pcs)) goto Error;

            if (!list.Append(new string(name.Select(c => (char)c).ToArray()), pcs, null)) goto Error;
        }

        numItems = 1;
        return list;

    Error:
        numItems = 0;
        list.Dispose();
        return null;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var namedColorList = (NamedColorList)value;

        var numColors = namedColorList.numColors;

        if (!io.Write(numColors)) return false;

        for (var i = 0u; i < numColors; i++)
        {
            if (!namedColorList.Info(i, out var root, out _, out _, out var pcs, out _)) return false;

            for (var j = 0; j < root.Length; j++)
            {
                if (!io.Write((byte)root[j])) return false;
            }
            if (!io.Write(3, pcs)) return false;
        }

        return true;
    }
}
