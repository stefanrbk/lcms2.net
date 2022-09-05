using lcms2.io;
using lcms2.plugins;

namespace lcms2.types.type_handlers;

public class S15Fixed16Handler: TagTypeHandler
{
    public S15Fixed16Handler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public S15Fixed16Handler(object? state = null)
        : this(default, state) { }

    public override object? Duplicate(object value, int num) =>
        ((double[])value).Clone();

    public override void Free(object value)
    { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        var num = sizeOfTag / sizeof(uint);
        double[] array_double = new double[num];

        for (var i = 0; i < num; i++)
            if (!io.Read15Fixed16Number(out array_double[i])) return null;

        numItems = num;
        return array_double;
    }

    public override bool Write(Stream io, object ptr, int numItems)
    {
        var value = (double[])ptr;

        for (var i = 0; i < numItems; i++)
            if (!io.Write(value[i])) return false;

        return true;
    }
}
