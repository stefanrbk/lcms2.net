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

[StructLayout(LayoutKind.Explicit)]
public unsafe struct IccHeader
{
    /// <summary>
    ///     Profile size in bytes
    /// </summary>
    [FieldOffset(0)] public uint size;

    /// <summary>
    ///     CMM for this profile
    /// </summary>
    [FieldOffset(4)] public uint cmmId;

    /// <summary>
    ///     Format version number
    /// </summary>
    [FieldOffset(8)] public uint version;

    /// <summary>
    ///     Type of profile
    /// </summary>
    [FieldOffset(12)] public uint deviceClass;

    /// <summary>
    ///     Color space of data
    /// </summary>
    [FieldOffset(16)] public uint colorSpace;

    /// <summary>
    ///     PCS, XYZ, or Lab only
    /// </summary>
    [FieldOffset(20)] public uint pcs;

    /// <summary>
    ///     Date profile was created
    /// </summary>
    [FieldOffset(24)] public DateTimeNumber date;

    /// <summary>
    ///     Magic Number to identify an ICC profile
    /// </summary>
    [FieldOffset(36)] public uint magic;

    /// <summary>
    ///     Primary platform
    /// </summary>
    [FieldOffset(40)] public uint platform;

    /// <summary>
    ///     Various bit settings
    /// </summary>
    [FieldOffset(44)] public uint flags;

    /// <summary>
    ///     Device manufacturer
    /// </summary>
    [FieldOffset(48)] public uint manufacturer;

    /// <summary>
    ///     Device model number
    /// </summary>
    [FieldOffset(52)] public uint model;

    /// <summary>
    ///     Device attributes
    /// </summary>
    [FieldOffset(56)] public ulong attributes;

    /// <summary>
    ///     Rendering intent
    /// </summary>
    [FieldOffset(64)] public uint renderingIntent;

    /// <summary>
    ///     Profile illuminant
    /// </summary>
    [FieldOffset(68)] public XYZEncoded illuminant;

    /// <summary>
    ///     Profile creator
    /// </summary>
    [FieldOffset(80)] public uint creator;

    /// <summary>
    ///     Profile ID using MD5
    /// </summary>
    [FieldOffset(84)] public ProfileID profileID;

    /// <summary>
    ///     Reserved for future use
    /// </summary>
    [FieldOffset(100)] public fixed byte reserved[28];
}
