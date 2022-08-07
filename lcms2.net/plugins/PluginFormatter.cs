using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     This plugin adds new handlers, replacing them if they already exist.
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginFormatters</c> typedef.</remarks>
public sealed class PluginFormatter : Plugin
{
    public FormatterFactory FormattersFactory;

    public PluginFormatter(Signature magic, uint expectedVersion, Signature type, FormatterFactory formatterFactory)
        : base(magic, expectedVersion, type) =>
        FormattersFactory = formatterFactory;

    internal static bool RegisterPlugin(Context?context, PluginFormatter? plugin)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///     Formatter for <see cref="ushort"/> values.
/// </summary>
/// <remarks>
///     Implements the <c>cmsFormatter16</c> typedef.</remarks>
public delegate byte[] Formatter16(ref Transform cmmCargo, ushort[] values, out byte[] buffer, int stride);

/// <summary>
///     Formatter for <see cref="float"/> values.
/// </summary>
/// <remarks>
///     Implements the <c>cmsFormatterFloat</c> typedef.</remarks>
public delegate byte[] FormatterFloat(ref Transform cmmCargo, float[] values, out byte[] buffer, int stride);

[Flags]
public enum PackFlag
{
    Ushort = 0,
    Float = 1,
}

/// <summary>
///     The requested direction of the <see cref="Formatter"/>.
/// </summary>
/// <remarks>
///     Implements the <c>cmsFormatterDirection</c> enum.</remarks>
public enum FormatterDirection
{
    Input,
    Output,
}

/// <summary>
///     The factory to build a <see cref="Formatter"/> of a specified type.
/// </summary>
/// <remarks>
///     Implements the <c>cmsFormatterFactory</c> typedef.</remarks>
public delegate Formatter FormatterFactory(Signature type, FormatterDirection dir, PackFlag flags);

/// <summary>
///     This type holds a pointer to a formatter that can be either 16 bits or cmsFloat32Number
/// </summary>
/// <remarks>
///     Implements the <c>cmsFormatter</c> union.</remarks>
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
