using System.Runtime.InteropServices;

namespace lcms2.types;

/// <summary>
///     Formatter for <see cref="ushort"/> values.
/// </summary>
/// <remarks>Implements the <c>cmsFormatter16</c> typedef.</remarks>
public delegate byte[] Formatter16(ref Transform cmmCargo, ushort[] values, out byte[] buffer, int stride);

/// <summary>
///     The factory to build a <see cref="Formatter"/> of a specified type.
/// </summary>
/// <remarks>Implements the <c>cmsFormatterFactory</c> typedef.</remarks>
public delegate Formatter FormatterFactory(Signature type, FormatterDirection dir, PackFlag flags);

/// <summary>
///     Formatter for <see cref="float"/> values.
/// </summary>
/// <remarks>Implements the <c>cmsFormatterFloat</c> typedef.</remarks>
public delegate byte[] FormatterFloat(ref Transform cmmCargo, float[] values, out byte[] buffer, int stride);

/// <summary>
///     The requested direction of the <see cref="Formatter"/>.
/// </summary>
/// <remarks>Implements the <c>cmsFormatterDirection</c> enum.</remarks>
public enum FormatterDirection
{
    Input,
    Output,
}

[Flags]
public enum PackFlag
{
    Ushort = 0,
    Float = 1,
}

/// <summary>
///     This type holds a pointer to a formatter that can be either 16 bits or cmsFloat32Number
/// </summary>
/// <remarks>Implements the <c>cmsFormatter</c> union.</remarks>
[StructLayout(LayoutKind.Explicit)]
public struct Formatter
{
    [FieldOffset(0)]
    public Formatter16 Fmt16;

    [FieldOffset(0)]
    public FormatterFloat FmtFloat;
}
