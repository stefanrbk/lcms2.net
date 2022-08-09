using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class U16Fixed16Handler : ITagTypeHandler
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
        {
            if (!io.ReadUInt32Number(out var v)) return null;

            // Convert to double
            array_double[i] = v / 65536.0;
        }

        numItems = num;
        return array_double;
    }

    public bool Write(Stream io, object ptr, int numItems)
    {
        var value = (double[])ptr;

        for (var i = 0; i < numItems; i++)
        {
            var v = (uint)Math.Floor((value[i] * 65536.0) + 0.5);

            if (!io.Write(v)) return false;
        }

        return true;
    }
}
