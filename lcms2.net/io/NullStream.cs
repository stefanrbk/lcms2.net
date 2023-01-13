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
namespace lcms2.io;

public class NullStream : Stream
{
    /** Original Code (cmsio0.c line: 36)
     **
     ** // NULL stream, for taking care of used space -------------------------------------
     **
     ** // NULL IOhandler basically does nothing but keep track on how many bytes have been
     ** // written. This is handy when creating profiles, where the file size is needed in the
     ** // header. Then, whole profile is serialized across NULL IOhandler and a second pass
     ** // writes the bytes to the pertinent IOhandler.
     **
     ** typedef struct {
     **     cmsUInt32Number Pointer;         // Points to current location
     ** } FILENULL;
     **/

    #region Fields

    private long _length;

    #endregion Fields

    #region Properties

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => _length;

    public override long Position { get; set; }

    #endregion Properties

    #region Public Methods

    public override void Flush()
    { }

    public override int Read(byte[] buffer, int offset, int count)
    {
        /** Original Code (cmsio0.cs line 47)
         **
         ** static
         ** cmsUInt32Number NULLRead(cmsIOHANDLER* iohandler, void *Buffer, cmsUInt32Number size, cmsUInt32Number count)
         ** {
         **     FILENULL* ResData = (FILENULL*) iohandler ->stream;
         **
         **     cmsUInt32Number len = size * count;
         **     ResData -> Pointer += len;
         **     return count;
         **
         **     cmsUNUSED_PARAMETER(Buffer);
         ** }
         **/

        _length = count;
        Position += _length;
        return count;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        /** Original Code (cmsio0.cs line 59)
         **
         ** static
         ** cmsBool  NULLSeek(cmsIOHANDLER* iohandler, cmsUInt32Number offset)
         ** {
         **     FILENULL* ResData = (FILENULL*) iohandler ->stream;
         **
         **     ResData ->Pointer = offset;
         **     return TRUE;
         ** }
         **/

        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;

            case SeekOrigin.Current:
                Position += offset;
                break;

            case SeekOrigin.End:
                Position = _length;
                Position -= offset;
                break;
        }
        return Position;
    }

    public override void SetLength(long value) =>
        _length = value;

    public override void Write(byte[] buffer, int offset, int count)
    {
        /** Original Code (cmsio0.cs line 75)
         **
         ** static
         ** cmsBool  NULLWrite(cmsIOHANDLER* iohandler, cmsUInt32Number size, const void *Ptr)
         ** {
         **     FILENULL* ResData = (FILENULL*) iohandler ->stream;
         **
         **     ResData ->Pointer += size;
         **     if (ResData ->Pointer > iohandler->UsedSpace)
         **         iohandler->UsedSpace = ResData ->Pointer;
         **
         **     return TRUE;
         **
         **     cmsUNUSED_PARAMETER(Ptr);
         ** }
         **/

        Position += count;
        if (Position > _length)
            _length = Position;
    }

    #endregion Public Methods
}
