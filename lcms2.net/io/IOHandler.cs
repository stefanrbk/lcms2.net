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
namespace lcms2.io;

internal unsafe delegate uint ReadFn(IOHandler io, void* buffer, uint size, uint count);

internal unsafe delegate bool WriteFn(IOHandler io, uint size, in void* buffer);

internal delegate bool SeekFn(IOHandler io, uint offset);

internal delegate bool CloseFn(IOHandler io);

internal delegate uint TellFn(IOHandler io);

public class IOHandler
{
    object? stream;
    Context? contextID;
    uint usedSpace;
    uint reportedSize;
    string? physicalFile;

    ReadFn read;
    SeekFn seek;
    CloseFn close;
    TellFn tell;
    WriteFn write;

    internal IOHandler(ReadFn read, SeekFn seek, CloseFn close, TellFn tell, WriteFn write)
    {
        this.read = read;
        this.seek = seek;
        this.close = close;
        this.tell = tell;
        this.write = write;
    }

    public unsafe uint Read(IOHandler io, void* buffer, uint size, uint count) =>
        read(io, buffer, size, count);

    public bool Seek(IOHandler io, uint offset) =>
        seek(io, offset);

    public bool Close(IOHandler io) =>
        close(io);

    public uint Tell(IOHandler io) =>
        tell(io);

    public unsafe bool Write(IOHandler io, uint size, in void* buffer) =>
        write(io, size, buffer);
}
