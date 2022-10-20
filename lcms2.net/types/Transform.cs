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
///     Transform factory
/// </summary>
/// <remarks>Implements the <c>_cmsTransform2Factory</c> typedef.</remarks>
public delegate void Transform2Factory(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

/// <summary>
///     Transform function
/// </summary>
/// <remarks>Implements the <c>_cmsTransform2Fn</c> typedef.</remarks>
public delegate void Transform2Fn(Transform cargo, object inputBuffer, object outputBuffer, int pixelsPerLine, int lineCount, Stride stride);

/// <summary>
///     Transform factory
/// </summary>
/// <remarks>Implements the <c>_cmsTransformFactory</c> typedef.</remarks>
public delegate void TransformFactory(TransformFn xform, object? userData, object inputBuffer, object outputBuffer, int size, int stride);

/// <summary>
///     Legacy function, handles just ONE scanline.
/// </summary>
/// <remarks>Implements the <c>_cmsTransformFn</c> typedef.</remarks>
public delegate void TransformFn(Transform cargo, object inputBuffer, object outputBuffer, int size, int stride);

public unsafe struct Cache
{
    #region Fields

    public fixed ushort CacheIn[maxChannels];
    public fixed ushort CacheOut[maxChannels];

    #endregion Fields
}

/// <summary>
///     Stride info for a transform.
/// </summary>
/// <remarks>Implements the <c>cmsStride</c> struct.</remarks>
public struct Stride
{
    #region Fields

    public int BytesPerLineIn;
    public int BytesPerLineOut;
    public int BytesPerPlaneIn;
    public int BytesPerPlaneOut;

    #endregion Fields
}

public class Transform
{
    #region Fields

    internal double adaptationState;

    internal Cache cache;

    internal Signature entryColorSpace;
    internal XYZ entryWhitePoint;
    internal Signature exitColorSpace;
    internal XYZ exitWhitePoint;
    internal FreeUserDataFn? freeUserData;
    internal Formatter16? fromInput;
    internal FormatterFloat? fromInputFloat;
    internal Pipeline gamutCheck;
    internal NamedColorList inputColorant;
    internal PixelFormat inputFormat, outputFormat;
    internal Pipeline lut;
    internal TransformFn? oldXform;
    internal NamedColorList outputColorant;
    internal Signature renderingIntent;
    internal Sequence sequence;
    internal object? state;
    internal Formatter16? toOutput;

    internal FormatterFloat? toOutputFloat;

    internal Transform2Fn? xform;

    #endregion Fields

    #region Properties

    /// <summary>
    ///     Retrieve original flags
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsGetTransformFlags</c> and <c>_cmsGetTransformUserData</c> functions.
    /// </remarks>
    public uint Flags { get; internal set; }

    /// <summary>
    ///     Retrieve 16 bit formatters
    /// </summary>
    /// <remarks>Implements the <c>_cmsGetTransformFormatters16</c> function.</remarks>
    public (Formatter16? FromInput, Formatter16? ToOutput) Formatters16 =>
        (fromInput, toOutput);

    /// <summary>
    ///     Retrieve float formatters
    /// </summary>
    /// <remarks>Implements the <c>_cmsGetTransformFormattersFloat</c> function.</remarks>
    public (FormatterFloat? FromInput, FormatterFloat? ToOutput) FormattersFloat =>
        (fromInputFloat, toOutputFloat);

    public NamedColorList? NamedColorList =>
        /** Original Code (cmsnamed.c line: 756)
         **
         ** // Retrieve the named color list from a transform. Should be first element in the LUT
         ** cmsNAMEDCOLORLIST* CMSEXPORT cmsGetNamedColorList(cmsHTRANSFORM xform)
         ** {
         **     _cmsTRANSFORM* v = (_cmsTRANSFORM*) xform;
         **     cmsStage* mpe  = v ->Lut->Elements;
         **
         **     if (mpe ->Type != cmsSigNamedColorElemType) return NULL;
         **     return (cmsNAMEDCOLORLIST*) mpe ->Data;
         ** }
         **/
        (lut.elements?.Data as Stage.NamedColorData)?.List;

    /// <summary>
    ///     User data as specified by the factory
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsSetTransformUserData</c> and <c>_cmsGetTransformUserData</c> functions.
    /// </remarks>
    public object? UserData { get; set; }

    #endregion Properties

    #region Structs

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Factory
    {
        [FieldOffset(0)]
        public TransformFactory LegacyXform;

        [FieldOffset(0)]
        public Transform2Factory Xform;
    }

    #endregion Structs
}
