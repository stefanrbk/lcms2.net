using System.Text;

namespace lcms2.types;
public partial struct Signature : ICloneable
{
    private readonly uint value;

    public Signature(uint value) =>
        this.value = value;
    public Signature(string value)
    {
        byte[] bytes = { 0x20, 0x20, 0x20, 0x20 };
        var s = Encoding.ASCII.GetBytes(value);
        switch (s.Length) {
            case 0:
                break;

            case 1:
                bytes[0] = s[0];
                break;

            case 2:
                bytes[1] = s[1];
                goto case 1;
            case 3:
                bytes[2] = s[2];
                goto case 2;
            default:
                bytes[3] = s[3];
                goto case 3;
        };
        this.value = ((uint)bytes[0] << 24) + ((uint)bytes[1] << 16) + ((uint)bytes[2] << 8) + bytes[3];
    }
    public static implicit operator uint(Signature v)
    {
        return v.value;
    }

    public static readonly Signature MagicNumber = new("ascp");
    public static readonly Signature LcmsSignature = new("lcms");

    public object Clone() =>
        new Signature(value);

    public override string ToString()
    {
        var str = new char[4];

        str[0] = (char)((value >> 24) & 0xFF);
        str[1] = (char)((value >> 16) & 0xFF);
        str[2] = (char)((value >> 8) & 0xFF);
        str[3] = (char)(value & 0xFF);

        return new string(str);
    }
}
