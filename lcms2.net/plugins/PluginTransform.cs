//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
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
using lcms2.types;

namespace lcms2.plugins;

public class PluginTransform : PluginBase
{
    public CmsTransformFactory? legacy_xform { get; internal set; }
    public CmsTransform2Factory? xform { get; internal set; }

    public PluginTransform(
        uint expectedVersion,
        Signature magic,
        Signature type,
        CmsTransformFactory transform)

        : base(expectedVersion, magic, type)
    {
        legacy_xform = transform;
    }

    public PluginTransform(
        uint expectedVersion,
        Signature magic,
        Signature type,
        CmsTransform2Factory transform)

        : base(expectedVersion, magic, type)
    {
        xform = transform;
    }
}

public unsafe delegate void CmsTransformFn(
    Transform CMMcargo, in void* InputBuffer, void* OutputBuffer,
    uint Size, uint Stride);

public unsafe delegate void CmsTransform2Fn(
    Transform CMMcargo, in void* InputBuffer, void* OutputBuffer,
    uint PixelsPerLine, uint LineCount, in CmsStride Stride);

public unsafe delegate bool CmsTransformFactory(
    CmsTransformFn xform, ref object? UserData, FreeUserDataFn? FreePrivateDataFn,
    Pipeline Lut, uint* InputFormat, uint* OutputFormat, uint* dwFlags);

public unsafe delegate bool CmsTransform2Factory(
    CmsTransformFn xform, ref object? UserData, FreeUserDataFn? FreePrivateDataFn,
    Pipeline Lut, uint* InputFormat, uint* OutputFormat, uint* dwFlags);

public struct CmsStride
{
    public uint BytesPerLineIn, BytesPerLineOut, BytesPerPlaneIn, BytesPerPlaneOut;
}

public class CmsTransform
{
    public uint InputFormat, OutputFormat;

    public CmsTransform2Fn? xform;

    public Formatter16? FromInput;
    public Formatter16? ToOutput;

    public FormatterFloat? FromInputFloat;
    public FormatterFloat? ToInputFloat;

    public CmsCache Cache;

    public Pipeline? Lut;

    public Pipeline? GamutCheck;

    public NamedColorList? InputColorant;
    public NamedColorList? OutputColorant;

    public Signature EntryColorSpace;
    public Signature ExitColorSpace;

    public CIEXYZ EntryWhitePoint;
    public CIEXYZ ExitWhitePoint;

    public Sequence? Sequence;

    public uint dwOriginalFlags;
    public double AdaptationState;

    public uint RenderingIntent;

    public Context? ContextID;

    public object? UserData;
    public FreeUserDataFn? FreeUserData;

    public CmsTransformFn? OldXform;
}

public unsafe struct CmsCache
{
    public fixed ushort CacheIn[maxChannels];
    public fixed ushort CacheOut[maxChannels];
}

public class CmsTransformCollection
{
    public CmsTransformFactory? OldFactory;
    public CmsTransform2Factory? Factory;

    public bool OldXform;
    public CmsTransformCollection? Next;
}
