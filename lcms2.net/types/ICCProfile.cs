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
using lcms2.io;
using lcms2.plugins;
using lcms2.state;

using System.Runtime.CompilerServices;

namespace lcms2.types;

public unsafe struct IccProfile : ICloneable
{
    #region Fields

    private IRawTag?[] tagPtrs = new IRawTag?[maxTableTag];

    private TagTypeHandler?[] tagTypeHandlers = new TagTypeHandler?[maxTableTag];

    private bool isWrite;
    private fixed uint tagLinked[maxTableTag];
    private fixed uint tagNames[maxTableTag];
    private fixed uint tagOffsets[maxTableTag];
    private fixed bool tagSaveAsRaw[maxTableTag];
    private fixed uint tagSizes[maxTableTag];
    private object? usrMutex;

    #endregion Fields

    #region Public Constructors

    public IccProfile(object? state)
    {
        ContextId = state;

        EncodedVersion = 0x02100000;

        Created = DateTime.UtcNow;

        usrMutex = State.GetMutexPlugin(state).create(state);
    }

    #endregion Public Constructors

    #region Properties

    public ulong Attributes { get; set; }

    public Signature ColorSpace { get; set; }

    public object? ContextId { get; }
    public DateTime Created { get; private set; }
    public uint Creator { get; private set; }
    public Signature DeviceClass { get; set; }
    public uint EncodedVersion { get; set; }
    public uint Flags { get; set; }
    public Stream? IOhandler { get; internal set; }
    public uint Manufacturer { get; set; }
    public uint Model { get; set; }
    public Signature PCS { get; set; }
    public ProfileID ProfileID { get; set; }
    public uint RenderingIntent { get; set; }
    public uint TagCount { get; private set; }

    public double Version
    {
        get =>
            BaseToBase(EncodedVersion >> 16, 16, 10) / 100.0;
        set =>
            EncodedVersion = BaseToBase((uint)Math.Floor((value * 100.0) + 0.5), 10, 16) << 16;
    }

    #endregion Properties

    #region Public Methods

    /// <summary>
    ///     Assuming io points to an ICC profile, compute and store MD5 checksum In the header,
    ///     rendering intent, attributes and ID should be set to zero before computing MD5 checksum
    ///     (per 6.1.13 in ICC spec)
    /// </summary>
    /// <remarks>Implements the <c>cmsMD5computeID</c> function.</remarks>
    public bool ComputeId()
    {
        // TODO
        throw new NotImplementedException();
    }

    public Signature GetTagName(int n)
    {
        if (n is < 0 or >= maxTableTag || n > TagCount)
            return default;
        return new(tagNames[n]);
    }

    /// <summary>
    ///     Check existence
    /// </summary>
    public bool IsTag(Signature sig) =>
        SearchTag(sig, false) >= 0;

    public uint SaveToStream(Stream? io)
    {
        ref var Icc = ref this;

        if (!State.GetMutexPlugin(ContextId).@lock(ContextId, ref Icc.usrMutex)) return 0;

        var Keep = (IccProfile?)Clone();

        var PrevIO = IOhandler = cmsOpenIOhandlerFromNULL();

        // Pass #1 does compute offsets

        if (!WriteHeader(0)) goto Error;
        if (!SaveTags(ref Keep)) goto Error;

        var UsedSpace = (uint)PrevIO.Length;

        // Pass #2 does save to iohandler

        if (io is not null)
        {
            Icc.IOhandler = io;
            if (!SetLinks()) goto Error;
            if (!WriteHeader(UsedSpace)) goto Error;
            if (!SaveTags(ref Keep)) goto Error;
        }

        this = Keep!.Value;
        PrevIO.Dispose();
        State.GetMutexPlugin(ContextId).unlock(ContextId, ref Icc.usrMutex);

        return UsedSpace;

    Error:
        PrevIO.Dispose();
        this = Keep!.Value;
        State.GetMutexPlugin(ContextId).unlock(ContextId, ref Icc.usrMutex);

        return 0;
    }

    #endregion Public Methods

    #region Internal Methods

    /// <summary>
    ///     Enforces that the profile version is per. spec.
    /// </summary>
    /// <remarks>
    ///     Operates on the big endian bytes from the profile.
    ///     Called before converting to platform endianness.
    ///     Byte 0 is BCD major version, so max 9.
    ///     Byte 1 is 2 BCD digits, one per nibble.
    ///     Reserved bytes 2 & 3 must be 0.
    /// </remarks>
    internal static uint ValidatedVersion(uint dword)
    {
        var pByte = BitConverter.GetBytes(dword);
        byte temp1, temp2;

        if (pByte[0] > 0x09) pByte[0] = 0x09;
        temp1 = (byte)(pByte[1] & 0xF0);
        temp2 = (byte)(pByte[1] & 0x0F);
        if (temp1 > 0x90) temp1 = 0x90;
        if (temp2 > 0x09) temp2 = 0x09;
        pByte[1] = (byte)(temp1 | temp2);
        pByte[2] = pByte[3] = 0;

        return BitConverter.ToUInt32(pByte);
    }

    internal void DeleteTagByPos(int i)
    {
        var ptr = tagPtrs[i];

        if (ptr is not null)
        {
            // Free previous version
            if (tagSaveAsRaw[i])
            {
                // TODO: Dispose??
            }
            else
            {
                var typeHandler = tagTypeHandlers[i];

                if (typeHandler is not null)
                {
                    typeHandler.StateContainer = ContextId;
                    typeHandler.ICCVersion = EncodedVersion;
                    typeHandler.Free(ptr);

                    tagPtrs[i] = null;
                }
            }
        }
    }

    internal int NewTag(Signature sig)
    {
        // Search for the tag
        var i = SearchTag(sig, false);

        if (i >= 0)
        {
            // Already exists? delete it
            DeleteTagByPos(i);
            return i;
        }
        else
        {
            // No, make a new one
            if (TagCount >= maxTableTag)
            {
                State.SignalError(ContextId, ErrorCode.Range, $"Too many tags ({maxTableTag})");
                return -1;
            }

            return (int)++TagCount;
        }
    }

    /// <summary>
    ///     Read profile header and validate it
    /// </summary>
    internal bool ReadHeader()
    {
        TagEntry tag;

        var io = IOhandler;
        var buffer = stackalloc byte[Unsafe.SizeOf<IccHeader>()];

        if (io is null) return false;

        // Read the header
        try
        {
            io.ReadExactly(new Span<byte>(buffer, Unsafe.SizeOf<IccHeader>()));
        }
        catch
        {
            return false;
        }
        var header = Unsafe.Read<IccHeader>(buffer);

        // Validate file as an ICC profile
        if (IOHandler.AdjustEndianness(header.magic) != MagicNumber)
        {
            State.SignalError(ContextId, ErrorCode.BadSignature, "Not an ICC profile, invalid signature");
            return false;
        }

        // Adjust endianness of the used parameters
        DeviceClass = new(IOHandler.AdjustEndianness(header.renderingIntent));
        ColorSpace = new(IOHandler.AdjustEndianness(header.colorSpace));
        PCS = new(IOHandler.AdjustEndianness(header.pcs));

        RenderingIntent = IOHandler.AdjustEndianness(header.renderingIntent);
        Flags = IOHandler.AdjustEndianness(header.flags);
        Manufacturer = IOHandler.AdjustEndianness(header.manufacturer);
        Model = IOHandler.AdjustEndianness(header.model);
        Creator = IOHandler.AdjustEndianness(header.creator);

        Attributes = IOHandler.AdjustEndianness(header.attributes);
        EncodedVersion = IOHandler.AdjustEndianness(header.version);

        // Get size as reported in header
        var headerSize = IOHandler.AdjustEndianness(header.size);

        // Make sure headerSize is lower than profile size
        var length = (uint)io.Length;
        if (headerSize >= length)
            headerSize = length;

        // Get creation date/time
        Created = IOHandler.AdjustEndianness(header.date);

        ProfileID = header.profileID;

        // Read tag directory
        if (!io.ReadUInt32Number(out var tagCount))
            return false;
        if (tagCount > maxTableTag)
        {
            State.SignalError(ContextId, ErrorCode.Range, $"Too many tags ({tagCount})");
            return false;
        }

        // Read tag directory
        TagCount = 0;
        for (var i = 0; i < tagCount; i++)
        {
            if (!io.ReadUInt32Number(out tag.sig)) return false;
            if (!io.ReadUInt32Number(out tag.offset)) return false;
            if (!io.ReadUInt32Number(out tag.size)) return false;

            // Perform some sanity check. Offset + size should fall inside file.
            if (tag.offset + tag.size > headerSize ||
                tag.offset + tag.size < tag.offset)
            {
                continue;
            }

            tagNames[TagCount] = tag.sig;
            tagOffsets[TagCount] = tag.offset;
            tagSizes[TagCount] = tag.size;

            // Search for links
            for (var j = 0; j < TagCount; j++)
            {
                if ((tagOffsets[j] == tag.offset) &&
                    (tagSizes[j] == tag.size))
                {
                    tagLinked[TagCount] = tagNames[j];
                }
            }

            TagCount++;
        }

        return true;
    }

    internal int SearchTag(Signature sig, bool followLinks)
    {
        int n;
        Signature linkedSig;

        do
        {
            // Search for given tag in ICC profile directory
            n = SearchOneTag(sig);
            if (n < 0)
                return -1;      // Not found

            if (!followLinks)
                return n;       // Found, don't follow links

            // Is this a linked tag?
            linkedSig = new(tagLinked[n]);

            // Yes, follow link
            if (linkedSig != default)
                sig = linkedSig;
        } while (linkedSig != default);

        return n;
    }

    /// <summary>
    ///     Saves profile header
    /// </summary>
    internal bool WriteHeader(uint usedSpace)
    {
        IccHeader Header;
        TagEntry Tag;

        var io = IOhandler;
        var buffer = stackalloc byte[Unsafe.SizeOf<IccHeader>()];

        if (io is null) return false;

        Header.size = IOHandler.AdjustEndianness(usedSpace);
        Header.cmmId = IOHandler.AdjustEndianness(LcmsSignature);
        Header.version = IOHandler.AdjustEndianness(EncodedVersion);

        Header.deviceClass = IOHandler.AdjustEndianness(DeviceClass);
        Header.colorSpace = IOHandler.AdjustEndianness(ColorSpace);
        Header.pcs = IOHandler.AdjustEndianness(PCS);

        // NOTE: in v4 timestamp must by in UTC rather than in local time
        Header.date = (DateTimeNumber)Created;

        Header.magic = IOHandler.AdjustEndianness(MagicNumber);

        Header.platform = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => IOHandler.AdjustEndianness(Signature.Platform.Microsoft),
            PlatformID.Unix => IOHandler.AdjustEndianness(Signature.Platform.Unices),
            _ => IOHandler.AdjustEndianness(Signature.Platform.Macintosh),
        };

        Header.flags = IOHandler.AdjustEndianness(Flags);
        Header.manufacturer = IOHandler.AdjustEndianness(Manufacturer);
        Header.model = IOHandler.AdjustEndianness(Model);

        Header.attributes = IOHandler.AdjustEndianness(Attributes);

        // Rendering intent in the header (for embedded profiles
        Header.renderingIntent = IOHandler.AdjustEndianness(RenderingIntent);

        // Illuminant is always D50
        Header.illuminant.X = (int)IOHandler.AdjustEndianness((uint)DoubleToS15Fixed16(D50.X));
        Header.illuminant.Y = (int)IOHandler.AdjustEndianness((uint)DoubleToS15Fixed16(D50.Y));
        Header.illuminant.Z = (int)IOHandler.AdjustEndianness((uint)DoubleToS15Fixed16(D50.Z));

        // Created by LittleCMS (that's me!)
        Header.creator = IOHandler.AdjustEndianness(LcmsSignature);

        Unsafe.InitBlock(Header.reserved, 0, 28);

        Header.profileID = ProfileID;

        // Dump the header
        Unsafe.Write(buffer, Header);
        try
        {
            io.Write(new(buffer, Unsafe.SizeOf<IccHeader>()));
        }
        catch
        {
            return false;
        }

        // Save Tag directory

        // Get true count
        var count = 0;
        for (var i = 0; i < TagCount; i++)
        {
            if (tagNames[i] != 0)
                count++;
        }

        // Store number of tags
        if (!io.Write(count)) return false;

        for (var i = 0; i < TagCount; i++)
        {
            if (tagNames[i] == 0) continue; // It is just a placeholder

            Tag.sig = IOHandler.AdjustEndianness(tagNames[i]);
            Tag.offset = IOHandler.AdjustEndianness(tagOffsets[i]);
            Tag.size = IOHandler.AdjustEndianness(tagSizes[i]);

            Unsafe.Write(buffer, Tag);
            try
            {
                io.Write(new(buffer, Unsafe.SizeOf<TagEntry>()));
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    #endregion Internal Methods

    #region Private Methods

    private static uint BaseToBase(uint @in, int baseIn, int baseOut)
    {
        int len, i;
        var buff = stackalloc byte[100];
        uint val;

        for (len = 0; @in > 0 && len < 100; len++)
        {
            buff[len] = (byte)(@in % baseIn);
            @in = (uint)(@in / baseIn);
        }

        for (i = len - 1, val = 0; i >= 0; --i)
        {
            val = (uint)((val * baseOut) + buff[i]);
        }

        return val;
    }

    private bool SetLinks()
    {
        for (var i = 0; i < TagCount; i++)
        {
            if (tagLinked[i] != 0)
            {
                var lnk = new Signature(tagLinked[i]);

                var j = SearchTag(lnk, false);
                if (j >= 0)
                {
                    tagOffsets[i] = tagOffsets[j];
                    tagSizes[i] = tagSizes[j];
                }
            }
        }

        return true;
    }

    private bool SaveTags(ref IccProfile? fileOrig)
    {
        ref var icc = ref this;
        var io = IOhandler;
        if (io is null) return false;
        var version = Version;

        for (var i = 0; i < TagCount; i++)
        {
            if (tagNames[i] == 0) continue;

            var tagName = new Signature(tagNames[i]);

            // Linked tags are not written
            if (tagLinked[i] != 0) continue;

            var begin = tagOffsets[i] = (uint)io.Length;

            var data = tagPtrs[i];

            if (data is null)
            {
                // Reach here if we are copying a tag from a disk-based ICC profile which has not been modified by user.
                // In this case a blind copy of the block data is performed
                if (fileOrig is not null && tagOffsets[i] != 0)
                {
                    var orig = fileOrig.Value;
                    if (orig.IOhandler is not null)
                    {
                        var tagSize = orig.tagSizes[i];
                        var tagOffset = orig.tagOffsets[i];

                        if (!orig.IOhandler.Seek(tagOffset)) return false;

                        var mem = new byte[tagSize];

                        try
                        {
                            orig.IOhandler.ReadExactly(mem);
                            io.Write(mem);
                        }
                        catch
                        {
                            return false;
                        }

                        icc.tagSizes[i] = (uint)(io.Length - begin);

                        // Align to 32 bit boundary.
                        if (!io.WriteAlignment()) return false;
                    }
                }

                continue;
            }

            // Should this tag be saved as RAW? If so, tagsizes should be specified in advance (no further cooking is done)
            if (icc.tagSaveAsRaw[i])
            {
                if (!data.WriteRaw(io)) return false;
            }
            else
            {
                // Search for support on this tag
                var tagDescriptor = State.GetTagDescriptor(ContextId, tagName);
                if (tagDescriptor is null) continue;        // Unsupported, ignore it

                var dataObj = (object)data;

                var type = tagDescriptor.DecideType is not null
                    ? tagDescriptor.DecideType(version, ref dataObj)
                    : tagDescriptor.SupportedTypes[0];

                var typeHandler = TagTypeHandler.GetHandler(ContextId, type);

                if (typeHandler is null)
                {
                    State.SignalError(ContextId, ErrorCode.Internal, $"(Internal) no handler for tag {tagName}");
                    continue;
                }

                var tagBase = new TagBase(typeHandler.Signature);
                if (!io.Write(tagBase)) return false;

                typeHandler.StateContainer = ContextId;
                typeHandler.ICCVersion = EncodedVersion;
                if (!typeHandler.Write(io, data, tagDescriptor.ElementCount))
                {
                    State.SignalError(ContextId, ErrorCode.Write, $"Couldn't write type '{tagBase.Signature}'");
                    return false;
                }
            }

            icc.tagSizes[i] = (uint)(io.Length - begin);

            // Align to 32 bit boundary
            if (!io.WriteAlignment()) return false;
        }

        return true;
    }

    private int SearchOneTag(Signature sig)
    {
        for (var i = 0; i < TagCount; i++)
        {
            if (sig == tagNames[i])
                return i;
        }

        return -1;
    }

    public object Clone()
    {
        var result = (IccProfile)MemberwiseClone();

        result.tagTypeHandlers = ((TagTypeHandler?[])tagTypeHandlers.Clone());
        result.tagPtrs = (IRawTag?[])tagPtrs.Clone();

        return result;
    }

    #endregion Private Methods
}
