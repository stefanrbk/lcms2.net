using lcms2.io;
using lcms2.plugins;

namespace lcms2.types.type_handlers;

public class ChromaticityHandler: TagTypeHandler
{
    public ChromaticityHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public ChromaticityHandler(object? state = null)
        : this(default, state) { }

    public override object? Duplicate(object value, int num) =>
        ((xyYTripple)value).Clone();

    public override void Free(object value)
    { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        var chrm = new xyYTripple();

        if (!io.ReadUInt16Number(out var numChans)) return null;

        // Let's recover from a bug introduced in early versions of lcms1
        if (numChans == 0 && sizeOfTag == 32)
        {
            if (!io.ReadUInt16Number(out _)) return null;
            if (!io.ReadUInt16Number(out numChans)) return null;
        }

        if (numChans != 3) return null;

        if (!io.ReadUInt16Number(out _)) return null; // Table

        if (!io.Read15Fixed16Number(out chrm.Red.x)) return null;
        if (!io.Read15Fixed16Number(out chrm.Red.y)) return null;

        chrm.Red.Y = 1.0;

        if (!io.Read15Fixed16Number(out chrm.Green.x)) return null;
        if (!io.Read15Fixed16Number(out chrm.Green.y)) return null;

        chrm.Green.Y = 1.0;

        if (!io.Read15Fixed16Number(out chrm.Blue.x)) return null;
        if (!io.Read15Fixed16Number(out chrm.Blue.y)) return null;

        chrm.Blue.Y = 1.0;

        numItems = 1;
        return chrm;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        xyYTripple chrm = (xyYTripple)value;

        if (!io.Write((uint)3)) return false; // numChannels
        if (!io.Write((uint)0)) return false; // Table

        if (!SaveOne(chrm.Red.x, chrm.Red.y, io)) return false;
        if (!SaveOne(chrm.Green.x, chrm.Green.y, io)) return false;
        if (!SaveOne(chrm.Blue.x, chrm.Blue.y, io)) return false;

        return true;
    }

    private static bool SaveOne(double x, double y, Stream io)
    {
        if (!io.Write(IOHandler.DoubleToS15Fixed16(x))) return false;
        if (!io.Write(IOHandler.DoubleToS15Fixed16(y))) return false;

        return true;
    }
}
