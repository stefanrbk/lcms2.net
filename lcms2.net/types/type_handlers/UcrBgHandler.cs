using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class UcrBgHandler : TagTypeHandler
{
    public UcrBgHandler(Signature sig, Context? context = null)
        : base(sig, context, 0) { }

    public UcrBgHandler(Context? context = null)
        : this(default, context) { }

    public override object? Duplicate(object value, int num) =>
        (value as UcrBg)?.Clone();

    public override void Free(object value) =>
        (value as UcrBg)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        ToneCurve? ucr, bg = null;
        Mlu? desc = null;

        numItems = 0;

        // First curve is Under color removal

        if (sizeOfTag < sizeof(uint)) return null;
        if (!io.ReadUInt32Number(out var countUcr)) return null;
        sizeOfTag -= sizeof(uint);

        ucr = ToneCurve.BuildTabulated16(Context, (int)countUcr, null);
        if (ucr is null) return null;

        if (sizeOfTag < (countUcr * sizeof(ushort))) goto Error;
        if (!io.ReadUInt16Array((int)countUcr, out ucr.Table16)) goto Error;
        sizeOfTag -= (int)countUcr * sizeof(ushort);

        // Second curve is Black generation

        if (sizeOfTag < sizeof(uint)) goto Error;
        if (!io.ReadUInt32Number(out var countBg)) goto Error;
        sizeOfTag -= sizeof(uint);

        bg = ToneCurve.BuildTabulated16(Context, (int)countBg, null);
        if (bg is null) goto Error;

        if (sizeOfTag < (countBg * sizeof(ushort))) goto Error;
        if (!io.ReadUInt16Array((int)countBg, out bg.Table16)) goto Error;
        sizeOfTag -= (int)countBg * sizeof(ushort);

        if (sizeOfTag is < 0 or > 32000) goto Error;

        // Now comes the text. The length is specified by the tag size
        desc = new Mlu(Context);

        var asciiString = new byte[sizeOfTag];
        if (io.Read(asciiString) != sizeOfTag) goto Error;

        if (!desc.SetAscii(Mlu.NoLanguage, Mlu.NoCountry, asciiString)) goto Error;

        UcrBg n = new(ucr, bg, desc);

        numItems = 1;
        return n;

    Error:

        ucr?.Dispose();
        bg?.Dispose();
        desc?.Dispose();

        return null;
    }

    public override bool Write(Stream io, object ptr, int numItems)
    {
        var value = (UcrBg)ptr;
        byte[]? nullBuffer = null;

        // First curve is Under color removal
        if (!io.Write(value.Ucr.NumEntries)) return false;
        if (!io.Write(value.Ucr.NumEntries, value.Ucr.Table16)) return false;

        // Then black generation
        if (!io.Write(value.Bg.NumEntries)) return false;
        if (!io.Write(value.Bg.NumEntries, value.Bg.Table16)) return false;

        // Now comes the text. The length is specified by the tab size
        var textSize = value.Description.GetAscii(Mlu.NoLanguage, Mlu.NoCountry, ref nullBuffer);
        var text = new byte[textSize];
        _ = value.Description.GetAscii(Mlu.NoLanguage, Mlu.NoCountry, ref text);

        io.Write(text);

        return true;
    }
}
