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
using lcms2.io;
using lcms2.state;

namespace lcms2.types;

public unsafe class Profile : ICloneable
{
    public IOHandler? IOHandler;
    public Context? ContextID;

    public DateTime Created;

    public uint Version;
    public Signature DeviceClass;
    public Signature ColorSpace;
    public Signature PCS;
    public uint RenderingIntent;

    public uint flags;
    public uint manufacturer, model;
    public ulong attributes;
    public uint creator;

    public ProfileID ProfileID;

    public uint TagCount;
    public readonly Signature[] TagNames = new Signature[MAX_TABLE_TAG];
    public readonly Signature[] TagLinked = new Signature[MAX_TABLE_TAG];
    public readonly uint[] TagSizes = new uint[MAX_TABLE_TAG];
    public readonly uint[] TagOffsets = new uint[MAX_TABLE_TAG];
    public readonly bool[] TagSaveAsRaw = new bool[MAX_TABLE_TAG];
    public readonly object?[] TagPtrs = new object?[MAX_TABLE_TAG];
    public readonly TagTypeHandler*[] TagTypeHandlers = new TagTypeHandler*[MAX_TABLE_TAG];

    public bool IsWrite;

    public object? UserMutex;

    public struct Header
    {
        public uint size;
        public Signature cmmId;
        public uint version;
        public Signature deviceClass;
        public Signature colorSpace;
        public Signature pcs;
        public DateTimeNumber date;
        public Signature magic;
        public Signature platform;
        public uint flags;
        public Signature manufacturer;
        public uint model;
        public ulong attributes;
        public uint renderingIntent;
        public EncodedXYZNumber illuminant;
        public Signature creator;
        public ProfileID profileID;
        public fixed sbyte reserved[28];
    }

    public object Clone()
    {
        var result = new Profile()
        {
            attributes = attributes,
            ColorSpace = ColorSpace,
            ContextID = ContextID,
            Created = Created,
            creator = creator,
            DeviceClass = DeviceClass,
            flags = flags,
            IOHandler = IOHandler,
            IsWrite = IsWrite,
            UserMutex = UserMutex,
            manufacturer = manufacturer,
            model = model,
            PCS = PCS,
            ProfileID = ProfileID,
            RenderingIntent = RenderingIntent,
            TagCount = TagCount,
            Version = Version,
        };
        TagNames.CopyTo(result.TagNames, 0);
        TagLinked.CopyTo(result.TagLinked, 0);
        TagSizes.CopyTo(result.TagSizes, 0);
        TagOffsets.CopyTo(result.TagOffsets, 0);
        TagSaveAsRaw.CopyTo(result.TagSaveAsRaw, 0);
        TagPtrs.CopyTo(result.TagPtrs, 0);
        TagTypeHandlers.CopyTo(result.TagTypeHandlers, 0);

        return result;
    }
}
