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

public unsafe struct TagTypeHandler
{
    public Signature Signature;
    public Context? ContextID;

    public delegate object? ReadFn(TagTypeHandler* self, IOHandler io, uint* nItems, uint SizeOfTag);
    public delegate bool WriteFn(TagTypeHandler* self, IOHandler io, object? Ptr, uint nItems);
    public delegate object? DupFn(TagTypeHandler* self, object? Ptr, uint nItems);
    public delegate void FreeFn(TagTypeHandler* self, object? Ptr);

    public ReadFn ReadPtr;
    public WriteFn WritePtr;
    public DupFn DupPtr;
    public FreeFn FreePtr;

    public uint ICCVersion;

    public TagTypeHandler(
        Signature signature,
        ReadFn readPtr,
        WriteFn writePtr,
        DupFn dupPtr,
        FreeFn freePtr,
        Context? contextID,
        uint iCCVersion)
    {
        Signature = signature;
        ContextID = contextID;
        ReadPtr = readPtr;
        WritePtr = writePtr;
        DupPtr = dupPtr;
        FreePtr = freePtr;
        ICCVersion = iCCVersion;
    }
}
