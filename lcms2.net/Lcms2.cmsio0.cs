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
using lcms2.types;

using System.Runtime.CompilerServices;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static uint NULLRead(IOHandler iohandler, void* _, uint size, uint count)
    {
        if (iohandler.stream is not FILENULL ResData)
            return 0;

        var len = size * count;
        ResData.Pointer += len;
        return count;
    }

    private static bool NULLSeek(IOHandler iohandler, uint offset)
    {
        if (iohandler.stream is not FILENULL ResData)
            return false;

        ResData.Pointer = offset;
        return true;
    }

    private static uint NULLTell(IOHandler iohandler)
    {
        if (iohandler.stream is not FILENULL ResData)
            return uint.MaxValue;

        return ResData.Pointer;
    }

    private static bool NULLWrite(IOHandler iohandler, uint size, in void* _)
    {
        if (iohandler.stream is not FILENULL ResData)
            return false;

        ResData.Pointer += size;
        if (ResData.Pointer > iohandler.UsedSpace)
            iohandler.UsedSpace = ResData.Pointer;

        return true;
    }

    private static bool NULLClose(IOHandler iohandler)
    {
        if (iohandler.stream is not FILENULL ResData)
            return false;

        //_cmsFree(iohandler.ContextID, ResData);
        //_cmsFree(iohandler.ContextID, iohandler);
        return true;
    }

    public static IOHandler? cmsOpenIOhandlerFromNULL(Context? ContextID)
    {
        var iohandler = new IOHandler();
        if (iohandler is null) return null;

        var fm = new FILENULL { Pointer = 0 };
        //if (fm is null) return null;

        iohandler.ContextID = ContextID;
        iohandler.stream = fm;
        iohandler.UsedSpace = 0;
        iohandler.reportedSize = 0;
        iohandler.physicalFile = String.Empty;

        iohandler.Read = NULLRead;
        iohandler.Seek = NULLSeek;
        iohandler.Close = NULLClose;
        iohandler.Tell = NULLTell;
        iohandler.Write = NULLWrite;

        return iohandler;
    }

    private static uint MemoryRead(IOHandler iohandler, void* Buffer, uint size, uint count)
    {
        if (iohandler.stream is not FILEMEM ResData)
            return 0;

        var len = size * count;

        if (ResData.Pointer + len > ResData.Size)
        {
            len = ResData.Size - ResData.Pointer;
            cmsSignalError(iohandler.ContextID, ErrorCode.Read, $"Read from memory error. Got {len} bytes, block should be of {count * size} bytes");
            return 0;
        }

        var Ptr = ResData.Block.Span[(int)ResData.Pointer..];
        new ReadOnlySpan<byte>(Buffer, (int)len).CopyTo(Ptr);
        //memmove(Buffer, Ptr, len);
        ResData.Pointer += len;

        return count;
    }

    private static bool MemorySeek(IOHandler iohandler, uint offset)
    {
        if (iohandler.stream is not FILEMEM ResData)
            return false;

        if (offset > ResData.Size)
        {
            cmsSignalError(iohandler.ContextID, ErrorCode.Seek, "Too few data; probably corrupted profile");
            return false;
        }

        ResData.Pointer = offset;
        return true;
    }

    private static uint MemoryTell(IOHandler iohandler)
    {
        if (iohandler.stream is not FILEMEM ResData)
            return 0;

        return ResData.Pointer;
    }

    private static bool MemoryWrite(IOHandler iohandler, uint size, in void* Ptr)
    {
        if (iohandler.stream is not FILEMEM ResData)
            return false;

        // Check for available space. Clip.
        if (ResData.Pointer + size > ResData.Size)
            size = ResData.Size - ResData.Pointer;

        if (size is 0) return true;     // Write zero bytes is ok, but does nothing

        new Span<byte>(Ptr, (int)size).CopyTo(ResData.Block.Span[(int)ResData.Pointer..]);
        //memmove(ResData.Block + ResData.Pointer, Ptr, size);
        ResData.Pointer += size;

        if (ResData.Pointer > iohandler.UsedSpace)
            iohandler.UsedSpace = ResData.Pointer;

        return true;
    }

    private static bool MemoryClose(IOHandler iohandler)
    {
        if (iohandler.stream is not FILEMEM ResData)
            return false;

        if (ResData.FreeBlockOnClose && ResData.Array is not null) _cmsFree(iohandler.ContextID, ResData.Array);

        //_cmsFree(iohandler.ContextID, ResData);
        //_cmsFree(iohandler.ContextID, iohandler);

        return true;
    }

    public static IOHandler? cmsOpenIOhandlerFromMem(Context? ContextID, Memory<byte> Buffer, uint size, string AccessMode)
    {
        FILEMEM? fm = null;

        _cmsAssert(AccessMode);

        var iohandler = new IOHandler();
        if (iohandler is null) return null;

        switch (AccessMode)
        {
            case "r":
                //fm = _cmsMallocZero<FILEMEM>(ContextID);
                //if (fm is null) goto Error;

                if (Buffer.IsEmpty)
                {
                    cmsSignalError(ContextID, ErrorCode.Read, "Couldn't read profile from NULL pointer");
                    goto Error;
                }

                //fm->Block = _cmsMalloc<byte>(ContextID, size);
                //if (fm->Block is null)
                //{
                //    cmsSignalError(ContextID, ErrorCode.Read, $"Couldn't allocate {size} bytes for profile");
                //    goto Error;
                //}

                //memmove(fm->Block, Buffer, size);
                //fm->FreeBlockOnClose = true;
                //fm->Size = size;
                //fm->Pointer = 0;

                fm = new FILEMEM(_cmsGetContext(ContextID).GetBufferPool<byte>().Rent((int)size), size, 0);
                (Buffer.Span[..(int)size]).CopyTo(fm.Block.Span);
                iohandler.reportedSize = size;

                break;

            case "w":
                //fm = _cmsMallocZero<FILEMEM>(ContextID);
                //if (fm is null) goto Error;

                //fm->Block = (byte*)Buffer;
                //fm->FreeBlockOnClose = false;
                //fm->Size = size;
                //fm->Pointer = 0;

                fm = new FILEMEM(Buffer, size, 0);
                iohandler.reportedSize = 0;

                break;

            default:
                cmsSignalError(ContextID, ErrorCode.UnknownExtension, $"Unknown access mode '{AccessMode}'");
                return null;
        }

        iohandler.ContextID = ContextID;
        iohandler.stream = fm;
        iohandler.UsedSpace = 0;

        iohandler.Read = MemoryRead;
        iohandler.Seek = MemorySeek;
        iohandler.Close = MemoryClose;
        iohandler.Tell = MemoryTell;
        iohandler.Write = MemoryWrite;

        return iohandler;

    Error:
        //if (fm is not null) _cmsFree(ContextID, fm);
        //if (iohandler is not null) _cmsFree(ContextID, iohandler);
        return null;
    }

    private static uint FileRead(IOHandler iohandler, void* Buffer, uint size, uint count)
    {
        if (iohandler.stream is not FILE file)
            return 0;

        var nReaded = (uint)fread(Buffer, size, count, file);

        if (nReaded != count)
        {
            cmsSignalError(iohandler.ContextID, ErrorCode.File, $"Read error. Got {nReaded * size} bytes, block should be of {count * size} bytes");
            return 0;
        }

        return nReaded;
    }

    private static bool FileSeek(IOHandler iohandler, uint offset)
    {
        if (iohandler.stream is not FILE file)
            return false;

        if (fseek(file, offset, SEEK_SET) is not 0)
        {
            cmsSignalError(iohandler.ContextID, ErrorCode.File, "Seek error; probably corrupted file");
            return false;
        }

        return true;
    }

    private static uint FileTell(IOHandler iohandler)
    {
        if (iohandler.stream is not FILE file)
            return uint.MaxValue;

        var t = ftell(file);
        if (t is -1)
        {
            cmsSignalError(iohandler.ContextID, ErrorCode.File, "Tell error; probably corrupted file");
            return 0;
        }

        return (uint)t;
    }

    private static bool FileWrite(IOHandler iohandler, uint size, in void* Buffer)
    {
        if (size is 0) return true;     // We allow to write 0 bytes, but nothing is written

        iohandler.UsedSpace += size;
        
        if (iohandler.stream is not FILE file)
            return false;

        return fwrite(Buffer, size, 1, file) is 1;
    }

    private static bool FileClose(IOHandler iohandler)
    {
        //_cmsFree(iohandler.ContextID, iohandler);
        return iohandler.stream is FILE file && fclose(file) is 0;
    }

    public static IOHandler? cmsOpenIOhandlerFromFile(Context? ContextID, string FileName, string AccessMode)
    {
        FILE? fm = null;
        int fileLen;

        _cmsAssert(FileName);
        _cmsAssert(AccessMode);

        var iohandler = new IOHandler();
        if (iohandler is null) return null;

        switch (AccessMode[0])
        {
            case 'r':
                fm = fopen(FileName, "rb");
                if (fm is null)
                {
                    //_cmsFree(ContextID, iohandler);
                    cmsSignalError(ContextID, ErrorCode.File, $"File '{FileName}' not found");
                    return null;
                }
                fileLen = (int)cmsfilelength(fm);
                if (fileLen < 0)
                {
                    fclose(fm);
                    //_cmsFree(ContextID, iohandler);
                    cmsSignalError(ContextID, ErrorCode.File, $"Cannot get size of file '{FileName}'");
                    return null;
                }

                iohandler.reportedSize = (uint)fileLen;
                break;

            case 'w':
                fm = fopen(FileName, "wb");
                if (fm is null)
                {
                    //_cmsFree(ContextID, iohandler);
                    cmsSignalError(ContextID, ErrorCode.File, $"Couldn't create '{FileName}'");
                    return null;
                }

                iohandler.reportedSize = 0;
                break;

            default:
                //_cmsFree(ContextID, iohandler);
                cmsSignalError(ContextID, ErrorCode.File, $"Unknown access mode '{AccessMode}'");
                return null;
        }

        iohandler.ContextID = ContextID;
        iohandler.stream = fm;
        iohandler.UsedSpace = 0;

        // Keep track of the original file
        iohandler.physicalFile = FileName;

        iohandler.Read = FileRead;
        iohandler.Seek = FileSeek;
        iohandler.Close = FileClose;
        iohandler.Tell = FileTell;
        iohandler.Write = FileWrite;

        return iohandler;
    }

    public static IOHandler? cmsOpenIOhandlerFromStream(Context? ContextID, Stream Stream)
    {
        IOHandler? iohandler = null;

        var file = new FILE(Stream);
        //file.Stream = Stream;

        var fileSize = cmsfilelength(file);
        if (fileSize < 0)
        {
            cmsSignalError(ContextID, ErrorCode.File, "Cannot get size of stream");
            return null;
            //goto Error;
        }

        iohandler = new IOHandler
        {
            ContextID = ContextID,
            stream = file,
            UsedSpace = 0,
            reportedSize = (uint)fileSize,
            physicalFile = String.Empty,

            Read = FileRead,
            Seek = FileSeek,
            Close = FileClose,
            Tell = FileTell,
            Write = FileWrite
        };
        //if (iohandler is null) goto Error;

        return iohandler;
        //Error:
        //if (file is not null) free(file);
        //if (iohandler is not null) _cmsFree(ContextID, iohandler);
        //return null;
    }

    public static bool cmsCloseIOhandler(IOHandler io) =>
        io.Close(io);

    public static IOHandler? cmsGetProfileIOhandler(Profile? Icc) =>
        Icc?.IOHandler;

    public static Profile? cmsCreateProfilePlaceholder(Context? ContextID)
    {
        var Icc = new Profile();
        if (Icc is null) return null;

        Icc.ContextID = ContextID;

        // Set it to empty
        Icc.TagCount = 0;

        // Set default version
        Icc.Version = 0x02100000;

        // Set creation date/time
        fixed(DateTime* dt = &Icc.Created)
            if (!_cmsGetTime(dt)) return null;

        // Create a mutex if the user provided proper plugin. NULL otherwise
        Icc.UserMutex = _cmsCreateMutex(ContextID);

        // Return the handle
        return Icc;
    //Error:
    //    _cmsFree(ContextID, Icc);
    //    return null;
    }

    public static Context? cmsGetProfileContextID(Profile? Icc) =>
        Icc?.ContextID;

    public static int cmsGetTagCount(Profile? Icc) =>
        (int)(Icc?.TagCount ?? uint.MaxValue);

    public static Signature cmsGetTagSignature(Profile Profile, uint n)
    {
        var Icc = Profile;

        if (n > Icc.TagCount || n >= MAX_TABLE_TAG)
            return default;

        return Icc.TagNames[n];
    }

    private static int SearchOneTag(Profile Profile, Signature sig)
    {
        for (var i = 0; i < (int)Profile.TagCount; i++)
        {
            if (sig == Profile.TagNames[i])
                return i;
        }

        return -1;
    }

    internal static int _cmsSearchTag(Profile Icc, Signature sig, bool lFollowLinks)
    {
        int n;
        Signature LinkedSig;
        do
        {
            // Search for givven tag in ICC profile directory
            n = SearchOneTag(Icc, sig);
            if (n < 0)
                return -1;          // Not found

            if (!lFollowLinks)
                return n;           // Found, don't follow links

            // Is this a linked tag?
            LinkedSig = Icc.TagLinked[n];

            // Yes, follow links
            if (LinkedSig != default)
                sig = LinkedSig;
        } while (LinkedSig != default);

        return n;
    }

    internal static void _cmsDeleteTagByPos(Profile Icc, int i)
    {
        _cmsAssert(Icc);
        _cmsAssert(i >= 0);

        if (Icc.TagPtrs[i] is not null)
        {
            // Free previous version
            if (Icc.TagSaveAsRaw[i])
            {
                //_cmsFree(Icc.ContextID, Icc.TagPtrs[i]);
            }
            else
            {
                var TypeHandler = Icc.TagTypeHandlers[i];

                if (TypeHandler is not null)
                {
                    var LocalTypeHandler = *TypeHandler;
                    LocalTypeHandler.ContextID = Icc.ContextID;    // As an aditional parameter
                    LocalTypeHandler.ICCVersion = Icc.Version;
                    LocalTypeHandler.FreePtr(&LocalTypeHandler, Icc.TagPtrs[i]);
                    Icc.TagPtrs[i] = null;
                }
            }
        }
    }

    internal static bool _cmsNewTag(Profile Icc, Signature sig, int* NewPos)
    {
        // Search for the tag
        var i = _cmsSearchTag(Icc, sig, false);
        if (i >= 0)
        {
            // Already exists? delete it
            _cmsDeleteTagByPos(Icc, i);
            *NewPos = i;
        }
        else
        {
            // No, make a new one
            if (Icc.TagCount >= MAX_TABLE_TAG)
            {
                cmsSignalError(Icc.ContextID, ErrorCode.Range, $"Too many tags ({MAX_TABLE_TAG})");
                return false;
            }

            *NewPos = (int)Icc.TagCount;
            Icc.TagCount++;
        }

        return true;
    }

    public static bool cmsIsTag(Profile Icc, Signature sig) =>
        _cmsSearchTag(Icc, sig, false) >= 0;

    private static uint _validatedVersion(uint DWord)
    {
        var pByte = (byte*)&DWord;

        if (*pByte > 0x09) *pByte = 0x09;
        var temp1 = (byte)(*(pByte + 1) & 0xF0);
        var temp2 = (byte)(*(pByte + 1) & 0x0F);
        if (temp1 > 0x90) temp1 = 0x90;
        if (temp2 > 0x09) temp2 = 0x09;
        *(pByte + 1) = (byte)(temp1 | temp2);
        *(pByte + 2) = 0;
        *(pByte + 3) = 0;

        return DWord;
    }

    internal static bool _cmsReadHeader(Profile Icc)
    {
        Profile.Header Header;
        TagEntry Tag;
        uint TagCount;

        var io = Icc.IOHandler;

        if (io is null) return false;

        // Read the header
        if (io.Read(io, &Header, _sizeof<Profile.Header>(), 1) is not 1)
            return false;

        // Validate file as an ICC profile
        if (_cmsAdjustEndianess32(Header.magic) != cmsMagicNumber)
        {
            cmsSignalError(Icc.ContextID, ErrorCode.BadSignature, "not an Icc profile, invalid signature");
            return false;
        }

        // Adjust endianness of the used parameters
        Icc.DeviceClass = new(_cmsAdjustEndianess32(Header.deviceClass));
        Icc.ColorSpace = new(_cmsAdjustEndianess32(Header.colorSpace));
        Icc.PCS = new(_cmsAdjustEndianess32(Header.pcs));

        Icc.RenderingIntent = _cmsAdjustEndianess32(Header.renderingIntent);
        Icc.flags = _cmsAdjustEndianess32(_cmsAdjustEndianess32(Header.flags)); ;
        Icc.manufacturer = _cmsAdjustEndianess32(Header.manufacturer);
        Icc.model = _cmsAdjustEndianess32(Header.model);
        Icc.creator = _cmsAdjustEndianess32(Header.creator);

        fixed(ulong* attributes = &Icc.attributes)
            _cmsAdjustEndianess64(attributes, &Header.attributes);
        Icc.Version = _cmsAdjustEndianess32(_validatedVersion(Header.version));

        // Get size as reported in header
        var HeaderSize = _cmsAdjustEndianess32(Header.size);

        // Make sure HeaderSize is lower than profile size
        if (HeaderSize >= io.reportedSize)
            HeaderSize = io.reportedSize;

        // Get creation date/time
        fixed(DateTime* Created = &Icc.Created)
            _cmsDecodeDateTimeNumber(&Header.date, Created);

        // The profile ID are 32 raw bytes
        fixed(ProfileID* ProfileID = &Icc.ProfileID)
            memmove(ProfileID->id32, Header.profileID.id32, 16);

        // Read tag directory
        if (!_cmsReadUInt32Number(io, &TagCount)) return false;
        if (TagCount > MAX_TABLE_TAG)
        {
            cmsSignalError(Icc.ContextID, ErrorCode.Range, $"Too many tags({TagCount})");
            return false;
        }

        // Read tag directory
        Icc.TagCount = 0;
        for (var i = 0; i < TagCount; i++)
        {
            if (!_cmsReadUInt32Number(io, (uint*)&Tag.sig)) return false;
            if (!_cmsReadUInt32Number(io, &Tag.offset)) return false;
            if (!_cmsReadUInt32Number(io, &Tag.size)) return false;

            // Perform some sanity check. Offset + size should fall inside file.
            if (Tag.offset + Tag.size > HeaderSize ||
                Tag.offset + Tag.size < Tag.offset)
            {
                continue;
            }

            Icc.TagNames[Icc.TagCount] = Tag.sig;
            Icc.TagOffsets[Icc.TagCount] = Tag.offset;
            Icc.TagSizes[Icc.TagCount] = Tag.size;

            // Search for links
            for (var j = 0; j < Icc.TagCount; j++)
            {
                if ((Icc.TagOffsets[j] == Tag.offset) &&
                    (Icc.TagSizes[j] == Tag.size))
                {
                    Icc.TagLinked[Icc.TagCount] = Icc.TagNames[j];
                }
            }

            Icc.TagCount++;
        }

        return true;
    }

    internal static bool _cmsWriteHeader(Profile Icc, uint UsedSpace)
    {
        Profile.Header Header;
        TagEntry Tag;

        Header.size = _cmsAdjustEndianess32(UsedSpace);
        Header.cmmId = new(_cmsAdjustEndianess32(lcmsSignature));
        Header.version = _cmsAdjustEndianess32(Icc.Version);

        Header.deviceClass = new(_cmsAdjustEndianess32(Icc.DeviceClass));
        Header.colorSpace = new(_cmsAdjustEndianess32(Icc.ColorSpace));
        Header.pcs = new(_cmsAdjustEndianess32(Icc.PCS));

        // NOTE: in v4 Timestamp must be in UTC rather than in local time
        fixed(DateTime* Created = &Icc.Created)
            _cmsEncodeDateTimeNumber(&Header.date, Created);

        Header.magic = new(_cmsAdjustEndianess32(cmsMagicNumber));
        Header.manufacturer = new(_cmsAdjustEndianess32(Environment.OSVersion.Platform is PlatformID.Win32NT ? cmsSigMicrosoft : cmsSigMacintosh));

        Header.flags = _cmsAdjustEndianess32(Icc.flags);
        Header.manufacturer = new(_cmsAdjustEndianess32(Icc.manufacturer));
        Header.model = _cmsAdjustEndianess32(Icc.model);

        fixed(ulong* attributes = &Icc.attributes)
            _cmsAdjustEndianess64(&Header.attributes, attributes);

        // Rendering intent in the header (for embedded profiles)
        Header.renderingIntent = _cmsAdjustEndianess32(Icc.RenderingIntent);

        // Illuminant is always D50
        Header.illuminant.X = (int)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(cmsD50_XYZ()->X));
        Header.illuminant.Y = (int)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(cmsD50_XYZ()->Y));
        Header.illuminant.Z = (int)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(cmsD50_XYZ()->Z));

        // Created by LittleCMS (that's me!)
        Header.creator = new(_cmsAdjustEndianess32(lcmsSignature));

        memset(&Header.reserved, 0, 28);

        // Set profile ID. Endianness is always big endian
        fixed(ProfileID* ProfileID = &Icc.ProfileID)
            memmove(&Header.profileID, ProfileID, 16);

        // Dump the header
        if (Icc.IOHandler is null || !Icc.IOHandler.Write(Icc.IOHandler, _sizeof<Profile.Header>(), &Header)) return false;

        // Saves Tag directory

        // Get true count
        var Count = 0u;
        for (var i = 0; i < Icc.TagCount; i++)
        {
            if (Icc.TagNames[i] != default)
                Count++;
        }

        // Store number of tags
        if (!_cmsWriteUInt32Number(Icc.IOHandler, Count)) return false;

        for (var i = 0; i < Icc.TagCount; i++)
        {
            if (Icc.TagNames[i] == default) continue;      // It is just a placeholder

            Tag.sig = new(_cmsAdjustEndianess32(Icc.TagNames[i]));
            Tag.offset = _cmsAdjustEndianess32(Icc.TagOffsets[i]);
            Tag.size = _cmsAdjustEndianess32(Icc.TagSizes[i]);

            if (!Icc.IOHandler.Write(Icc.IOHandler, _sizeof<TagEntry>(), &Tag)) return false;
        }

        return true;
    }

    public static uint cmsGetHeaderRenderingIntent(Profile Icc) =>
        Icc.RenderingIntent;

    public static void cmsSetHeaderRenderingIntent(Profile Icc, uint RenderingIntent) =>
        Icc.RenderingIntent = RenderingIntent;

    public static uint cmsGetHeaderFlags(Profile Icc) =>
        Icc.flags;

    public static void cmsSetHeaderFlags(Profile Icc, uint Flags) =>
        Icc.flags = Flags;

    public static uint cmsGetHeaderManufacturer(Profile Icc) =>
        Icc.manufacturer;

    public static void cmsSetHeaderManufacturer(Profile Icc, uint manufacturer) =>
        Icc.manufacturer = manufacturer;

    public static uint cmsGetHeaderCreator(Profile Icc) =>
        Icc.creator;

    public static uint cmsGetHeaderModel(Profile Icc) =>
        Icc.model;

    public static void cmsSetHeaderModel(Profile Icc, uint model) =>
        Icc.model = model;

    public static void cmsGetHeaderAttributes(Profile Icc, ulong* Flags) =>
        *Flags = Icc.attributes;

    public static void cmsSetHeaderAttributes(Profile Icc, ulong Flags) =>
        Icc.attributes = Flags;

    public static void cmsGetHeaderProfileID(Profile Icc, byte* ProfileID)
    {
        fixed (ProfileID* profileID = &Icc.ProfileID)
                memmove(ProfileID, profileID->id8, 16);
    }

    public static void cmsSetHeaderProfileID(Profile Icc, byte* ProfileID)
    {
        fixed(ProfileID* profileID = &Icc.ProfileID)
            memmove(profileID->id8, ProfileID, 16);
    }

    public static bool cmsGetHeaderCreationDateTime(Profile Icc, DateTime* Dest)
    {
        fixed(DateTime* Created = &Icc.Created)
            memmove(Dest, Created, _sizeof<DateTime>());
        return true;
    }

    public static Signature cmsGetPCS(Profile Icc) =>
        Icc.PCS;

    public static void cmsSetPCS(Profile Icc, Signature pcs) =>
        Icc.PCS = pcs;

    public static Signature cmsGetColorSpace(Profile Icc) =>
        Icc.ColorSpace;

    public static void cmsSetColorSpace(Profile Icc, Signature sig) =>
        Icc.ColorSpace = sig;

    public static Signature cmsGetDeviceClass(Profile Icc) =>
        Icc.DeviceClass;

    public static void cmsSetDeviceClass(Profile Icc, Signature sig) =>
        Icc.DeviceClass = sig;

    public static uint cmsGetEncodedICCVersion(Profile Icc) =>
        Icc.Version;

    public static void cmsSetEncodedICCVersion(Profile Icc, uint Version) =>
        Icc.Version = Version;

    private static uint BaseToBase(uint @in, int BaseIn, int BaseOut)
    {
        var Buff = stackalloc byte[100];
        var @out = 0u;
        int len = 0, i;

        for (; @in > 0 && len < 100; len++)
        {
            Buff[len] = (byte)(@in % BaseIn);
            @in = (uint)(@in / BaseIn);
        }

        for (i = len - 1, @out = 0; i >= 0; --i)
        {
            @out = (uint)((@out * BaseOut) + Buff[i]);
        }

        return @out;
    }

    public static void cmsSetProfileVersion(Profile Icc, double Version) =>
        // 4.2 -> 0x04200000
        Icc.Version = BaseToBase((uint)Math.Floor((Version * 100.0) + 0.5), 10, 16) << 16;

    public static double cmsGetProfileVersion(Profile Icc) =>
        // 0x04200000 -> 4.2
        BaseToBase(Icc.Version >> 16, 16, 10) / 100.0;

    public static Profile? cmsOpenProfileFromIOHandlerTHR(Context? ContextID, IOHandler io)
    {
        var hEmpty = cmsCreateProfilePlaceholder(ContextID);
        if (hEmpty is null) return null;

        var NewIcc = hEmpty;

        NewIcc.IOHandler = io;
        if (!_cmsReadHeader(NewIcc)) goto Error;
        return NewIcc;
    Error:
        cmsCloseProfile(NewIcc);
        return null;
    }

    public static Profile? cmsOpenProfileFromIOHandler2THR(Context? ContextID, IOHandler io, bool write)
    {
        var hEmpty = cmsCreateProfilePlaceholder(ContextID);
        if (hEmpty is null) return null;

        var NewIcc = hEmpty;

        NewIcc.IOHandler = io;
        if (write)
        {
            NewIcc.IsWrite = true;
            return NewIcc;
        }

        if (!_cmsReadHeader(NewIcc)) goto Error;
        return NewIcc;
    Error:
        cmsCloseProfile(NewIcc);
        return null;
    }

    public static Profile? cmsOpenProfileFromFileTHR(Context? ContextID, string FileName, string Access)
    {
        var hEmpty = cmsCreateProfilePlaceholder(ContextID);
        if (hEmpty is null) return null;

        var NewIcc = hEmpty;

        NewIcc.IOHandler = cmsOpenIOhandlerFromFile(ContextID, FileName, Access);
        if (NewIcc.IOHandler is null) goto Error;

        if (Access.ToLower().Contains('w'))
        {
            NewIcc.IsWrite = true;
            return NewIcc;
        }

        if (!_cmsReadHeader(NewIcc)) goto Error;
        return NewIcc;

    Error:
        cmsCloseProfile(NewIcc);
        return null;
    }

    public static Profile? cmsOpenProfileFromFile(string FileName, string Access) =>
        cmsOpenProfileFromFileTHR(null, FileName, Access);

    public static Profile? cmsOpenProfileFromStreamTHR(Context? ContextID, Stream ICCProfile, string Access)
    {
        var hEmpty = cmsCreateProfilePlaceholder(ContextID);
        if (hEmpty is null) return null;

        var NewIcc = hEmpty;

        NewIcc.IOHandler = cmsOpenIOhandlerFromStream(ContextID, ICCProfile);
        if (NewIcc.IOHandler is null) goto Error;

        if (Access.ToLower().Contains('w'))
        {
            NewIcc.IsWrite = true;
            return NewIcc;
        }

        if (!_cmsReadHeader(NewIcc)) goto Error;
        return NewIcc;

    Error:
        cmsCloseProfile(NewIcc);
        return null;
    }

    public static Profile? cmsOpenProfileFromStream(Stream ICCProfile, string Access) =>
        cmsOpenProfileFromStreamTHR(null, ICCProfile, Access);

    public static Profile? cmsOpenProfileFromMemTHR(Context? ContextID, Memory<byte> MemPtr, uint Size)
    {
        var hEmpty = cmsCreateProfilePlaceholder(ContextID);
        if (hEmpty is null) return null;

        var NewIcc = hEmpty;

        NewIcc.IOHandler = cmsOpenIOhandlerFromMem(ContextID, MemPtr, Size, "r");
        if (NewIcc.IOHandler is null) goto Error;

        if (!_cmsReadHeader(NewIcc)) goto Error;
        return NewIcc;

    Error:
        cmsCloseProfile(NewIcc);
        return null;
    }

    public static Profile? cmsOpenProfileFromMem(Memory<byte> MemPtr, uint Size) =>
        cmsOpenProfileFromMemTHR(null, MemPtr, Size);

    private static bool SaveTags(Profile Icc, Profile? FileOrig)
    {
        var io = Icc.IOHandler;
        if (io is null) return false;

        var Version = cmsGetProfileVersion(Icc);

        for (var i = 0; i < Icc.TagCount; i++)
        {
            if ((uint)Icc.TagNames[i] is 0) continue;

            // Linked tags are not written
            if ((uint)Icc.TagLinked[i] is not 0) continue;

            var Begin = Icc.TagOffsets[i] = io.UsedSpace;

            var Data = Icc.TagPtrs[i];
            if (Data is null)
            {
                void* Mem;
                // Reach here if we are copying a tag from a disk-based ICC profile which has not been modified by user.
                // In this case a blind copy of the block data is performed
                if (FileOrig is not null &&
                    Icc.TagOffsets[i] is not 0 &&
                    FileOrig.IOHandler is not null)
                {
                    var TagSize = FileOrig.TagSizes[i];
                    var TagOffset = FileOrig.TagOffsets[i];

                    if (!FileOrig.IOHandler.Seek(FileOrig.IOHandler, TagOffset)) return false;

                    Mem = _cmsMalloc(Icc.ContextID, TagSize);
                    if (Mem is null) return false;

                    if (FileOrig.IOHandler.Read(FileOrig.IOHandler, Mem, TagSize, 1) is not 1) goto Error;
                    if (!io.Write(io, TagSize, Mem)) goto Error;
                    _cmsFree(Icc.ContextID, Mem);

                    Icc.TagSizes[i] = io.UsedSpace - Begin;

                    // Align to 32 bit boundary.
                    if (!_cmsWriteAlignment(io))
                        return false;
                }

                continue;
            Error:
                _cmsFree(Icc.ContextID, Mem);
                return false;
            }

            // Should this tag be saved as RAW? If so, tagsizes should be specified in advance (no further cooking is done)
            if (Icc.TagSaveAsRaw[i] && Data is BoxPtr<byte> buffer)
            {
                if (!io.Write(io, Icc.TagSizes[i], buffer)) return false;
            }
            else
            {
                // Search for support on this tag
                var TagDescriptor = _cmsGetTagDescriptor(Icc.ContextID, Icc.TagNames[i]);
                if (TagDescriptor is null) continue;        // Unsupported, ignore it

                var Type = (TagDescriptor->DecideType is not null)
                    ? TagDescriptor->DecideType(Version, Data)
                    : (Signature)TagDescriptor->SupportedTypes[0];

                var TypeHandler = _cmsGetTagTypeHandler(Icc.ContextID, Type);

                if (TypeHandler is null)
                {
                    cmsSignalError(Icc.ContextID, cmsERROR_INTERNAL, $"(Internal) no handler for tag {Icc.TagNames[i]:x}");
                    continue;
                }

                var TypeBase = TypeHandler->Signature;
                if (!_cmsWriteTypeBase(io, TypeBase))
                    return false;

                var LocalTypeHandler = *TypeHandler;
                LocalTypeHandler.ContextID = Icc.ContextID;
                LocalTypeHandler.ICCVersion = Icc.Version;
                if (!LocalTypeHandler.WritePtr(&LocalTypeHandler, io, Data, TagDescriptor->ElemCount))
                {
                    var str = stackalloc sbyte[5];

                    _cmsTagSignature2String((byte*)str, TypeBase);
                    cmsSignalError(Icc.ContextID, cmsERROR_WRITE, $"Couldn't write type '{new string(str)}'");
                    return false;
                }
            }

            Icc.TagSizes[i] = io.UsedSpace - Begin;

            // Align to 32 bit boundary
            if (!_cmsWriteAlignment(io))
                return false;
        }

        return true;
    }

    private static bool SetLinks(Profile Icc)
    {
        for (var i = 0; i < Icc.TagCount; i++)
        {
            var lnk = Icc.TagLinked[i];
            if ((uint)lnk is not 0)
            {
                var j = _cmsSearchTag(Icc, lnk, false);
                if (j >= 0)
                {
                    Icc.TagOffsets[i] = Icc.TagOffsets[j];
                    Icc.TagSizes[i] = Icc.TagSizes[j];
                }
            }
        }

        return true;
    }

    public static uint cmsSaveProfileToIOhandler(Profile Profile, IOHandler? io)
    {
        Profile Keep;

        _cmsAssert(Profile);

        Keep = Profile;

        if (!_cmsLockMutex(Keep.ContextID, Keep.UserMutex)) return 0;
        var Icc = (Profile)Keep.Clone();
        //memmove(&Keep, Icc, _sizeof<Profile>());

        var ContextID = cmsGetProfileContextID(Icc);
        var PrevIO = Icc.IOHandler = cmsOpenIOhandlerFromNULL(ContextID);
        if (PrevIO is null)
        {
            _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
            return 0;
        }

        // Pass #1 does computer offsets
        if (!_cmsWriteHeader(Icc, 0)) goto Error;
        if (!SaveTags(Icc, Keep)) goto Error;

        var UsedSpace = PrevIO.UsedSpace;

        // Pass #2 does save to iohandler

        if (io is not null)
        {
            Icc.IOHandler = io;
            if (!SetLinks(Icc)) goto Error;
            if (!_cmsWriteHeader(Icc, UsedSpace)) goto Error;
            if (!SaveTags(Icc, Keep)) goto Error;
        }


        //memmove(Icc, &Keep, _sizeof<Profile>());
        if (!cmsCloseIOhandler(PrevIO))
            UsedSpace = 0; // As an error marker

        _cmsUnlockMutex(Keep.ContextID, Keep.UserMutex);

        return UsedSpace;

    Error:
        cmsCloseIOhandler(PrevIO);
        //memmove(Icc, &Keep, _sizeof<Profile>());
        _cmsUnlockMutex(Keep.ContextID, Keep.UserMutex);

        return 0;
    }

    public static bool cmsSaveProfileToFile(Profile Profile, string FileName)
    {
        var ContextID = cmsGetProfileContextID(Profile);
        var io = cmsOpenIOhandlerFromFile(ContextID, FileName, "w");

        if (io is null) return false;

        var rc = cmsSaveProfileToIOhandler(Profile, io) is not 0;
        rc &= cmsCloseIOhandler(io);

        if (!rc)
        {
            try
            {
                File.Delete(FileName);
            }
            catch { }
        }
        return rc;
    }

    public static bool cmsSaveProfileToStream(Profile Profile, Stream Stream)
    {
        var ContextID = cmsGetProfileContextID(Profile);
        var io = cmsOpenIOhandlerFromStream(ContextID, Stream);

        if (io is null) return false;

        var rc = cmsSaveProfileToIOhandler(Profile, io) is not 0;
        rc &= cmsCloseIOhandler(io);

        return rc;
    }

    public static bool cmsSaveProfileToMem(Profile Profile, Memory<byte> MemPtr, uint* BytesNeeded)
    {
        var ContextID = cmsGetProfileContextID(Profile);

        _cmsAssert(BytesNeeded);

        // Should we just calculate the needed space?
        if (MemPtr.IsEmpty)
        {
            *BytesNeeded = cmsSaveProfileToIOhandler(Profile, null);
            return *BytesNeeded is not 0;
        }

        // That is a read write operation
        var io = cmsOpenIOhandlerFromMem(ContextID, MemPtr, *BytesNeeded, "w");
        if (io is null) return false;

        var rc = cmsSaveProfileToIOhandler(Profile, io) is not 0;
        rc &= cmsCloseIOhandler(io);

        return rc;
    }

    private static void freeOneTag(Profile Icc, uint i)
    {
        if (Icc.TagPtrs[i] is not null)
        {
            var TypeHandler = Icc.TagTypeHandlers[i];

            if (TypeHandler is not null)
            {
                var LocalTypeHandler = *TypeHandler;

                LocalTypeHandler.ContextID = Icc.ContextID;
                LocalTypeHandler.ICCVersion = Icc.Version;
                LocalTypeHandler.FreePtr(&LocalTypeHandler, Icc.TagPtrs[i]);
            }
            else
            {
                //_cmsFree(Icc.ContextID, Icc.TagPtrs[i]);
            }
        }
    }

    public static bool cmsCloseProfile(Profile Profile)
    {
        var Icc = Profile;
        var rc = true;

        if (Icc is null) return false;

        // Was open in write mode?
        if (Icc.IsWrite)
        {
            Icc.IsWrite = false;   // Assure no further writing
            if (Icc.IOHandler is null || Icc.IOHandler.physicalFile is null)
                return false;
            rc &= cmsSaveProfileToFile(Icc, Icc.IOHandler.physicalFile!);
        }

        for (var i = 0u; i < Icc.TagCount; i++)
            freeOneTag(Icc, i);

        if (Icc.IOHandler is not null)
            rc &= cmsCloseIOhandler(Icc.IOHandler);

        _cmsDestroyMutex(Icc.ContextID, Icc.UserMutex);

        //_cmsFree(Icc.ContextID, Icc);  // Free placeholder memory

        return rc;
    }

    private static bool IsTypeSupported(TagDescriptor* TagDescriptor, Signature Type)
    {
        var nMaxTypes = TagDescriptor->nSupportedTypes;
        if (nMaxTypes >= MAX_TYPES_IN_LCMS_PLUGIN)
            nMaxTypes = MAX_TYPES_IN_LCMS_PLUGIN;

        for (var i = 0; i < nMaxTypes; i++)
            if (Type == TagDescriptor->SupportedTypes[i]) return true;

        return false;
    }

    public static object? cmsReadTag(Profile Profile, Signature sig)
    {
        TagDescriptor* TagDescriptor;
        Signature BaseType;
        uint ElemCount;
        var str = stackalloc sbyte[5];

        var Icc = Profile;

        if (!_cmsLockMutex(Icc.ContextID, Icc.UserMutex)) return null;

        var n = _cmsSearchTag(Icc, sig, true);
        if (n < 0)
        {
            // Not found, return null
            _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
            return null;
        }

        // If the element is already in memory, return the pointer
        if (Icc.TagPtrs[n] is not null)
        {
            if (Icc.TagTypeHandlers[n] is null) goto Error;

            // Sanity check
            BaseType = Icc.TagTypeHandlers[n]->Signature;
            if ((uint)BaseType is 0) goto Error;

            TagDescriptor = _cmsGetTagDescriptor(Icc.ContextID, sig);
            if (TagDescriptor is null) goto Error;

            if (!IsTypeSupported(TagDescriptor, BaseType)) goto Error;

            if (Icc.TagSaveAsRaw[n]) goto Error;   // We don't support read raw tags as cooked

            _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
            return Icc.TagPtrs[n];
        }

        // We need to read it. Get the offset and size to the file
        var Offset = Icc.TagOffsets[n];
        var TagSize = Icc.TagSizes[n];

        if (TagSize < 8) goto Error;

        var io = Icc.IOHandler;
        // Seek to its location
        if (io is null || !io.Seek(io, Offset))
            goto Error;

        // Search for support on this tag
        TagDescriptor = _cmsGetTagDescriptor(Icc.ContextID, sig);
        if (TagDescriptor is null)
        {
            _cmsTagSignature2String((byte*)str, sig);

            // An unknown element was found.
            cmsSignalError(Icc.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Unknown tag type '{new string(str)}' found.");
            goto Error;     // Unsupported
        }

        // if supported, get type and check if in list
        BaseType = _cmsReadTypeBase(io);
        if ((uint)BaseType is 0) goto Error;

        if (!IsTypeSupported(TagDescriptor, BaseType)) goto Error;

        TagSize -= 8;   // Already read by the type base logic

        // Get type handler
        var TypeHandler = _cmsGetTagTypeHandler(Icc.ContextID, BaseType);
        if (TypeHandler is null) goto Error;
        var LocalTypeHandler = *TypeHandler;

        // Read the tag
        Icc.TagTypeHandlers[n] = TypeHandler;
        LocalTypeHandler.ContextID = Icc.ContextID;
        LocalTypeHandler.ICCVersion = Icc.Version;
        Icc.TagPtrs[n] = LocalTypeHandler.ReadPtr(&LocalTypeHandler, io, &ElemCount, TagSize);

        // The tag type is supported, but something wrong happened and we cannot read the tag.
        // let the user know about this (although it is just a warning)
        if (Icc.TagPtrs[n] is null)
        {
            _cmsTagSignature2String((byte*)str, sig);
            cmsSignalError(Icc.ContextID, cmsERROR_CORRUPTION_DETECTED, $"Corrupted tag '{new string(str)}'");
            goto Error;
        }

        // This is a weird error that may be a symptom of something more serious, the number of
        // stored items is actuall less than the number of required elements.
        if (ElemCount < TagDescriptor->ElemCount)
        {
            _cmsTagSignature2String((byte*)str, sig);
            cmsSignalError(Icc.ContextID, cmsERROR_CORRUPTION_DETECTED,
                $"'{new string(str)}' Inconsistent number of items: expected {TagDescriptor->ElemCount}, got {ElemCount}");
            goto Error;
        }

        // Return the data
        _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
        return Icc.TagPtrs[n];

    Error:
        freeOneTag(Icc, (uint)n);
        Icc.TagPtrs[n] = null;

        _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
        return null;
    }

    internal static Signature _cmsGetTagTrueType(Profile Profile, Signature sig)
    {
        var Icc = Profile;

        // Search for given tag in Icc profile directory
        var n = _cmsSearchTag(Icc, sig, true);
        if (n < 0) return 0;                        // Not found, return null

        // Get the handler. The true type is there
        var TypeHandler = Icc.TagTypeHandlers[n];
        return TypeHandler->Signature;
    }

    public static bool cmsWriteTag(Profile Profile, Signature sig, object? data)
    {
        var Icc = Profile;

        var TypeString = stackalloc sbyte[5];
        var SigString = stackalloc sbyte[5];
        Signature Type;
        int i;

        if (!_cmsLockMutex(Icc.ContextID, Icc.UserMutex)) return false;

        // To delete tags
        if (data is null)
        {
            // Delete the tag
            i = _cmsSearchTag(Icc, sig, false);
            if (i >= 0)
            {
                // Use zero as a mark of deleted
                _cmsDeleteTagByPos(Icc, i);
                Icc.TagNames[i] = 0;
                _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
                return true;
            }
            // Didn't find the tag
            goto Error;
        }

        if (!_cmsNewTag(Icc, sig, &i)) goto Error;

        // This is not raw
        Icc.TagSaveAsRaw[i] = false;

        // This is not a link
        Icc.TagLinked[i] = 0;

        // Get information about the TAG
        var TagDescriptor = _cmsGetTagDescriptor(Icc.ContextID, sig);
        if (TagDescriptor is null)
        {
            _cmsTagSignature2String((byte*)SigString, sig);
            cmsSignalError(Icc.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Unsupported tag '{new string(SigString)}'");
            goto Error;
        }

        // Now we need to know which type to use. It depends on the version.
        var Version = cmsGetProfileVersion(Icc);

        if (TagDescriptor->DecideType is not null)
        {
            // Let the tag descriptor to decide the type base on depending on
            // the data. This is useful for example on parametric curves, where
            // curves specified by a table cannot be saved as parametric and needs
            // to be casted to single v2-curves, even on v4 profiles.

            Type = TagDescriptor->DecideType(Version, data);
        }
        else
        {
            Type = TagDescriptor->SupportedTypes[0];
        }

        // Does the tag support this type?
        if (!IsTypeSupported(TagDescriptor, Type))
        {
            _cmsTagSignature2String((byte*)TypeString, Type);
            _cmsTagSignature2String((byte*)SigString, sig);

            cmsSignalError(Icc.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Unsupported type '{new string(TypeString)}' for tag '{new string(SigString)}'");
            goto Error;
        }

        // Do we have a handler for this type?
        var TypeHandler = _cmsGetTagTypeHandler(Icc.ContextID, Type);
        if (TypeHandler is null)
        {
            _cmsTagSignature2String((byte*)TypeString, Type);
            _cmsTagSignature2String((byte*)SigString, sig);

            cmsSignalError(Icc.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Unsupported type '{new string(TypeString)}' for tag '{new string(SigString)}'");
            goto Error;     // Should never happen
        }

        // Fill fields on icc structure
        Icc.TagTypeHandlers[i] = TypeHandler;
        Icc.TagNames[i] = sig;
        Icc.TagSizes[i] = 0;
        Icc.TagOffsets[i] = 0;

        var LocalTagTypeHandler = *TypeHandler;
        LocalTagTypeHandler.ContextID = Icc.ContextID;
        LocalTagTypeHandler.ICCVersion = Icc.Version;
        Icc.TagPtrs[i] = LocalTagTypeHandler.DupPtr(&LocalTagTypeHandler, data, TagDescriptor->ElemCount);

        if (Icc.TagPtrs[i] is null)
        {
            _cmsTagSignature2String((byte*)TypeString, Type);
            _cmsTagSignature2String((byte*)SigString, sig);

            cmsSignalError(Icc.ContextID, cmsERROR_UNKNOWN_EXTENSION, $"Malformed struct in type '{new string(TypeString)}' for tag '{new string(SigString)}'");
            goto Error;
        }

        _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
        return true;
    Error:
        _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
        return false;
    }

    public static uint cmsReadRawTag(Profile Profile, Signature sig, Memory<byte> data, uint BufferSize)
    {
        var Icc = Profile;

        if (!_cmsLockMutex(Icc.ContextID, Icc.UserMutex)) return 0;

        // Search for given tag in ICC profile directory
        var i = _cmsSearchTag(Icc, sig, true);
        if (i < 0) goto Error;  // Not found

        // It is already read?
        if (Icc.TagPtrs[i] is null)
        {
            // Not yet, get original position
            var Offset = Icc.TagOffsets[i];
            var TagSize = Icc.TagSizes[i];

            // read the data directly, don't keep copy
            if (!data.IsEmpty)
            {
                if (BufferSize < TagSize)
                    TagSize = BufferSize;

                if (Icc.IOHandler?.Seek(Icc.IOHandler, Offset) != true) goto Error;
                if (Icc.IOHandler.Read(Icc.IOHandler, Unsafe.AsPointer(ref data.Span.GetPinnableReference()), 1, TagSize) is 0) goto Error;

                _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
                return TagSize;
            }

            _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
            return Icc.TagSizes[i];
        }

        // The data has been already read, or written. But wait! maybe the user chose to save as
        // raw data. In this case, return the raw data directly
        if (Icc.TagSaveAsRaw[i])
        {
            if (!data.IsEmpty)
            {
                var TagSize = Icc.TagSizes[i];
                if (BufferSize < TagSize)
                    TagSize = BufferSize;

                new ReadOnlySpan<byte>((BoxPtrVoid)Icc.TagPtrs[i], (int)TagSize).CopyTo(data.Span);
                //memmove(data, (BoxPtrVoid)Icc.TagPtrs[i], TagSize);

                _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
                return TagSize;
            }

            _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
            return Icc.TagSizes[i];
        }

        // Already read, or previously set by cmsWriteTag(). We need to serialize that
        // data to raw in order to maintain consistency.

        _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
        var Object = cmsReadTag(Icc, sig);
        if (!_cmsLockMutex(Icc.ContextID, Icc.UserMutex)) return 0;

        if (Object is null) goto Error;

        // Now we need to serialize to a memory block: just use a memory iohandler

        var MemIO = data.IsEmpty
            ? cmsOpenIOhandlerFromNULL(cmsGetProfileContextID(Icc))
            : cmsOpenIOhandlerFromMem(cmsGetProfileContextID(Icc), data, BufferSize, "w");
        if (MemIO is null) goto Error;

        // Obtain type handling for the tag
        var TypeHandler = Icc.TagTypeHandlers[i];
        var TagDescriptor = _cmsGetTagDescriptor(Icc.ContextID, sig);
        if (TagDescriptor is null)
        {
            cmsCloseIOhandler(MemIO);
            goto Error;
        }

        if (TypeHandler is null) goto Error;

        // Serialize
        var LocalTypeHandler = *TypeHandler;
        LocalTypeHandler.ContextID = Icc.ContextID;
        LocalTypeHandler.ICCVersion = Icc.Version;

        if (!_cmsWriteTypeBase(MemIO, TypeHandler->Signature))
        {
            cmsCloseIOhandler(MemIO);
            goto Error;
        }

        if (!LocalTypeHandler.WritePtr(&LocalTypeHandler, MemIO, Object, TagDescriptor->ElemCount))
        {
            cmsCloseIOhandler(MemIO);
            goto Error;
        }

        // Get Size and close
        var rc = MemIO.Tell(MemIO);
        cmsCloseIOhandler(MemIO);       // Ignore return code this time

        _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
        return rc;

    Error:
        _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
        return 0;
    }

    public static bool cmsWriteRawTag(Profile Profile, Signature sig, in void* data, uint Size)
    {
        int i;

        var Icc = Profile;

        if (!_cmsLockMutex(Icc.ContextID, Icc.UserMutex)) return false;

        if (!_cmsNewTag(Icc, sig, &i))
        {
            _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
            return false;
        }

        // Mark the tag as being written as RAW
        Icc.TagSaveAsRaw[i] = true;
        Icc.TagNames[i] = sig;
        Icc.TagLinked[i] = 0;

        // Keep a copy of the block
        Icc.TagPtrs[i] = new BoxPtrVoid(_cmsDupMem(Icc.ContextID, data, Size));
        Icc.TagSizes[i] = Size;

        _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);

        if (Icc.TagPtrs[i] is null)
        {
            Icc.TagNames[i] = 0;
            return false;
        }

        return true;
    }

    public static bool cmsLinkTag(Profile Profile, Signature sig, Signature dest)
    {
        int i;

        var Icc = Profile;

        if (!_cmsLockMutex(Icc.ContextID, Icc.UserMutex)) return false;

        if (!_cmsNewTag(Icc, sig, &i))
        {
            _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
            return false;
        }

        // Keep necessary information
        Icc.TagSaveAsRaw[i] = false;
        Icc.TagNames[i] = sig;
        Icc.TagLinked[i] = dest;

        Icc.TagPtrs[i] = null;
        Icc.TagSizes[i] = 0;
        Icc.TagOffsets[i] = 0;

        _cmsUnlockMutex(Icc.ContextID, Icc.UserMutex);
        return true;
    }

    public static Signature cmsTagLinkedTo(Profile Profile, Signature sig)
    {
        var Icc = Profile;

        // Search for given tag in ICC profile directory
        var i = _cmsSearchTag(Icc, sig, false);
        if (i < 0) return 0;        // Not found, return 0

        return Icc.TagLinked[i];
    }
}
