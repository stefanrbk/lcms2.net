using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class S15Fixed16Handler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion => 0;

    public object? Duplicate(object value, int num) =>
        ((double[])value).Clone();

    public void Free(object value) { }

    public object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        var num = sizeOfTag / sizeof(uint);
        double[] array_double = new double[num];

        for (var i = 0; i < num; i++)
            if (!io.Read15Fixed16Number(out array_double[i])) return null;

        numItems = num;
        return array_double;
    }

    public bool Write(Stream io, object ptr, int numItems)
    {
        var value = (double[])ptr;

        for (var i = 0; i < numItems; i++)
            if (!io.Write(value[i])) return false;

        return true;
    }
}
