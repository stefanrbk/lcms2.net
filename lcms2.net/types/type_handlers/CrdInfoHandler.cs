using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;

public class CrdInfoHandler: TagTypeHandler
{
    public CrdInfoHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public CrdInfoHandler(object? state = null)
        : base(default, state, 0) { }

    public override object? Duplicate(object value, int num) =>
        (value as Mlu)?.Clone();

    public override void Free(object value) =>
        (value as Mlu)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        Mlu mlu = new(StateContainer);

        numItems = 0;

        if (!ReadCountAndString(io, mlu, ref sizeOfTag, "nm")) goto Error;
        if (!ReadCountAndString(io, mlu, ref sizeOfTag, "#0")) goto Error;
        if (!ReadCountAndString(io, mlu, ref sizeOfTag, "#1")) goto Error;
        if (!ReadCountAndString(io, mlu, ref sizeOfTag, "#2")) goto Error;
        if (!ReadCountAndString(io, mlu, ref sizeOfTag, "#3")) goto Error;

        numItems = 1;
        return mlu;

    Error:
        mlu?.Dispose();

        return null;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var mlu = (Mlu)value;

        if (!WriteCountAndString(io, mlu, "nm")) return false;
        if (!WriteCountAndString(io, mlu, "#0")) return false;
        if (!WriteCountAndString(io, mlu, "#1")) return false;
        if (!WriteCountAndString(io, mlu, "#2")) return false;
        if (!WriteCountAndString(io, mlu, "#3")) return false;

        return true;
    }
}
