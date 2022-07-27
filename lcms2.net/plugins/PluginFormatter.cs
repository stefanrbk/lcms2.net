using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using lcms2.types;

namespace lcms2.plugins;

#if PLUGIN
    public
#else
internal
#endif
    sealed class PluginFormatter : PluginBase
{
    public FormatterFactory FormattersFactory;

    public PluginFormatter(Signature magic, uint expectedVersion, Signature type, FormatterFactory formatterFactory)
        : base(magic, expectedVersion, type) =>
        FormattersFactory = formatterFactory;
}

#if PLUGIN
    public
#else
internal
#endif
    delegate byte[] Formatter16(ref Transform cmmCargo, ushort[] values, out byte[] buffer, int stride);
#if PLUGIN
    public
#else
internal
#endif
    delegate byte[] FormatterFloat(ref Transform cmmCargo, float[] values, out byte[] buffer, int stride);

[Flags]

#if PLUGIN
    public
#else
internal
#endif
    enum PackFlag
{
    Ushort = 0,
    Float = 1,
}

#if PLUGIN
    public
#else
internal
#endif
    enum FormatterDirection
{
    Input,
    Output,
}


#if PLUGIN
    public
#else
internal
#endif
    delegate Formatter FormatterFactory(Signature type, FormatterDirection dir, PackFlag flags);

[StructLayout(LayoutKind.Explicit)]

#if PLUGIN
    public
#else
internal
#endif
    struct Formatter
{
    [FieldOffset(0)]
    public Formatter16 Fmt16;
    [FieldOffset(0)]
    public FormatterFloat FmtFloat;
}


#if PLUGIN
    public
#else
internal
#endif
    class FormattersFactoryList
{
    internal FormatterFactory? factory;

    internal FormattersFactoryList? next = null;
}
