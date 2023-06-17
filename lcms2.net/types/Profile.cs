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

public unsafe struct Profile
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
    public fixed uint TagNames[MAX_TABLE_TAG];
    public fixed uint TagLinked[MAX_TABLE_TAG];
    public fixed uint TagSizes[MAX_TABLE_TAG];
    public fixed uint TagOffsets[MAX_TABLE_TAG];
    public fixed bool TagSaveAsRaw[MAX_TABLE_TAG];
    public fixed long TagPtrs[MAX_TABLE_TAG];
    public fixed long TagTypeHandlers[MAX_TABLE_TAG];

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
}
