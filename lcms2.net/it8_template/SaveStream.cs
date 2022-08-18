using System.Text;

using lcms2.state;

namespace lcms2.it8_template;
public class SaveStream
{
    public StreamWriter? Stream;
    public Memory<byte>? Base;
    public Memory<byte>? Ptr;
    public int Used;
    public int Max;

    public void WriteString(string? str)
    {
        var f = this;

        if (str is null)
            str = " ";

        // Length to write
        var len = str.Length;
        f.Used += len;

        if (f.Stream is not null) {     // Should I write it to a file?
            try {
                f.Stream.Write(str);
            } catch {
                Context.SignalError(null, ErrorCode.Write, "Write to file error in CGATS parser");
                return;
            }
        } else {                        // Or to a memory block?

            if (f.Base is not null) {

                if (f.Used > f.Max) {
                    Context.SignalError(null, ErrorCode.Write, "Write to memory overflows in CGATS parser");
                    return;
                }

                var ascii = new ASCIIEncoding();

                if (f.Ptr is null)
                    f.Ptr = f.Base;

                ascii.GetBytes(str).CopyTo(f.Ptr.Value);
                f.Ptr = f.Ptr.Value[len..];
            }
        }
    }

    public void WriteFormatted(string? str, params object?[] args)
    {
        if (str is null) str = string.Empty;
        WriteString(string.Format(str, args));
    }
}
