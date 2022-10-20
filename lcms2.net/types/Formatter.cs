//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//---------------------------------------------------------------------------------
//
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

internal struct Formatters16
{
    #region Fields

    public Formatter16 Frm;
    public uint Mask;
    public uint Type;

    #endregion Fields
}

internal struct FormattersFloat
{
    #region Fields

    public FormatterFloat Frm;
    public uint Mask;
    public uint Type;

    #endregion Fields
}
