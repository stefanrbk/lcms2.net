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
using lcms2.types;

namespace lcms2.plugins;

public class PluginTagType : PluginBase
{
    public CmsTagTypeHandler Handler { get; internal set; }

    public PluginTagType(
        uint expectedVersion,
        Signature magic,
        Signature type,
        CmsTagTypeHandler handler)

        : base(expectedVersion, magic, type)
    {
        Handler = handler;
    }
}

public abstract class CmsTagTypeHandler
{
    public Signature Signature;
    public Context ContextID;
    public uint ICCVersion;

    protected CmsTagTypeHandler(Signature signature, Context contextID, uint iCCVersion)
    {
        Signature = signature;
        ContextID = contextID;
        ICCVersion = iCCVersion;
    }

    public abstract unsafe ITag? Read(IOHandler io, uint* nItems, uint SizeOfTag);

    public abstract bool Write(IOHandler io, ITag Ptr, uint nItems);

    public abstract ITag? Duplicate(ITag Ptr, uint n);

    public abstract void Free(ITag Ptr);
}

public interface ITag : ICloneable, IDisposable
{
    bool WriteRaw(IOHandler io);

    abstract static ITag ReadRaw(IOHandler io);
}

public class CmsTagTypeLinkedList
{
    public CmsTagTypeHandler Handler;
    public CmsTagTypeLinkedList? Next;

    public CmsTagTypeLinkedList(CmsTagTypeHandler handler, CmsTagTypeLinkedList? next = null)
    {
        Handler = handler;
        Next = next;
    }
}
