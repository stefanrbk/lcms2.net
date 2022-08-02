using System.Runtime.InteropServices;

using lcms2.types;

namespace lcms2.plugins;

#if PLUGIN
public sealed class PluginFormatter
#else
internal sealed class PluginFormatter
#endif
    : PluginBase
{
    public FormatterFactory FormattersFactory;

    public PluginFormatter(Signature magic, uint expectedVersion, Signature type, FormatterFactory formatterFactory)
        : base(magic, expectedVersion, type) =>
        FormattersFactory = formatterFactory;
}

#if PLUGIN
public delegate byte[] Formatter16(
#else
internal delegate byte[] Formatter16(
#endif
    ref Transform cmmCargo, ushort[] values, out byte[] buffer, int stride);

#if PLUGIN
public delegate byte[] FormatterFloat(
#else
internal delegate byte[] FormatterFloat(
#endif
    ref Transform cmmCargo, float[] values, out byte[] buffer, int stride);

#if PLUGIN
[Flags]
public enum PackFlag
#else
[Flags]
internal enum PackFlag
#endif
{
    Ushort = 0,
    Float = 1,
}

#if PLUGIN
public enum FormatterDirection
#else
internal enum FormatterDirection
#endif
{
    Input,
    Output,
}

#if PLUGIN
public delegate Formatter FormatterFactory(
#else
internal delegate Formatter FormatterFactory(
#endif
    Signature type, FormatterDirection dir, PackFlag flags);

#if PLUGIN
[StructLayout(LayoutKind.Explicit)]
public struct Formatter
#else
[StructLayout(LayoutKind.Explicit)]
internal struct Formatter
#endif
{
    [FieldOffset(0)]
    public Formatter16 Fmt16;
    [FieldOffset(0)]
    public FormatterFloat FmtFloat;
}

#if PLUGIN
public class FormattersFactoryList
#else
internal class FormattersFactoryList
#endif
{
    internal FormatterFactory? factory;

    internal FormattersFactoryList? next = null;
}
