using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class TextHandler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion { get; }

    public object? Duplicate(ITagTypeHandler handler, object value, int num) =>
        ((Mlu)value).Clone();

    public void Free(ITagTypeHandler handler, object value) =>
        ((Mlu)value).Dispose();

    public object? Read(ITagTypeHandler handler, Stream io, int sizeOfTag, out int numItems)
    {
        // Create a container
        Mlu mlu = new(Context.Get(Context));
        numItems = 0;

        // We need to store the "\0" at the end, so +1
        if (sizeOfTag == Int32.MaxValue) goto Error;

        var text = new byte[sizeOfTag + 1];

        if (io.Read(text, 0, sizeOfTag) != sizeOfTag) goto Error;

        numItems = 1;

        // Keep the result
        if (!mlu.SetAscii(Mlu.NoLanguage, Mlu.NoCountry, text)) goto Error;
        return mlu;

    Error:
        mlu.Dispose();
        return null;
    }

    public bool Write(ITagTypeHandler handler, Stream io, object value, int numItems)
    {
        var mlu = (Mlu)value;

        byte[]? buffer = null;
        // Get the size of the string. Note there is an extra "\0" at the end
        var size = mlu.GetAscii(Mlu.NoLanguage, Mlu.NoCountry, ref buffer);
        if (size == 0) return false; // Cannot be zero!

        // Create memory
        buffer = new byte[size];
        mlu.GetAscii(Mlu.NoLanguage, Mlu.NoCountry, ref buffer);

        // Write it, including separator
        io.Write(buffer!, 0, (int)size);

        return true;
    }
}
