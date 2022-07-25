using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public delegate byte[] Formatter16(ref Transform cmmCargo, ushort[] values, out byte[] buffer, int stride);
public delegate byte[] FormatterFloat(ref Transform cmmCargo, float[] values, out byte[] buffer, int stride);

[Flags]
public enum PackFlag
{
    Ushort = 0,
    Float = 1,
}
public enum FormatterDirection
{
    Input,
    Output,
}

public delegate Formatter FormatterFactory(Signature type, FormatterDirection dir, PackFlag flags);

[StructLayout(LayoutKind.Explicit)]
public struct Formatter
{
    [FieldOffset(0)]
    public Formatter16 Fmt16;
    [FieldOffset(0)]
    public FormatterFloat FmtFloat;
}

public class FormattersFactoryList
{
    internal FormatterFactory? factory;

    internal FormattersFactoryList? next = null;
}
