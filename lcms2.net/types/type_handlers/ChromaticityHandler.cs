using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class ChromaticityHandler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion => 0;

    public object? Duplicate(ITagTypeHandler handler, object value, int num) => throw new NotImplementedException();
    public void Free(ITagTypeHandler handler, object value) => throw new NotImplementedException();
    public (object Value, int Count)? Read(ITagTypeHandler handler, Stream io, int sizeOfTag)
    {
        var chrm = new xyYTripple();

        var numChans = io.ReadUInt16Number();
        if (numChans is null) return null;

        // Let's recover from a bug introduced in early versions of lcms1
        if (numChans == 0 && sizeOfTag == 32)
        {
            if (io.ReadUInt16Number() is null) return null;
            numChans = io.ReadUInt16Number();
            if (numChans is null) return null;
        }

        if (numChans != 3) return null;

        var table = io.ReadUInt16Number();
        if (table is null) return null;

        var value = io.Read15Fixed16Number();
        if (value is null) return null;
        chrm.Red.x = (double)value;
        value = io.Read15Fixed16Number();
        if (value is null) return null;
        chrm.Red.y = (double)value;

        chrm.Red.Y = 1.0;

        value = io.Read15Fixed16Number();
        if (value is null) return null;
        chrm.Green.x = (double)value;
        value = io.Read15Fixed16Number();
        if (value is null) return null;
        chrm.Green.y = (double)value;

        chrm.Green.Y = 1.0;

        value = io.Read15Fixed16Number();
        if (value is null) return null;
        chrm.Blue.x = (double)value;
        value = io.Read15Fixed16Number();
        if (value is null) return null;
        chrm.Blue.y = (double)value;

        chrm.Blue.Y = 1.0;

        return (chrm, 1);
    }

    private static bool SaveOne(double x, double y, Stream io)
    {
        if (!io.Write(IOHandler.DoubleToS15Fixed16(x))) return false;
        if (!io.Write(IOHandler.DoubleToS15Fixed16(y))) return false;

        return true;
    }
    public bool Write(ITagTypeHandler handler, Stream io, object value, int numItems)
    {
        xyYTripple chrm = (xyYTripple)value;

        if (!io.Write((uint)3)) return false; // numChannels
        if (!io.Write((uint)0)) return false; // Table

        if (!SaveOne(chrm.Red.x, chrm.Red.y, io)) return false;
        if (!SaveOne(chrm.Green.x, chrm.Green.y, io)) return false;
        if (!SaveOne(chrm.Blue.x, chrm.Blue.y, io)) return false;

        return true;
    }
}
