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

public class PluginTag : PluginBase
{
    public Signature Signature { get; internal set; }
    public CmsTagDescriptor Descriptor { get; internal set; }

    public PluginTag(
        uint expectedVersion,
        Signature magic,
        Signature type,
        Signature sig,
        CmsTagDescriptor desc)

        : base(expectedVersion, magic, type)
    {
        Signature = sig;
        Descriptor = desc;
    }
}

public unsafe struct CmsTagDescriptor
{
    public uint ElemCount;

    public uint nSupportedTypes;
    public fixed uint SupportedTypes[MaxTypesInPlugin];

    public TagDecideFn DecideType;
}

public unsafe delegate Signature TagDecideFn(double ICCVersion, in void* Data);

public class CmsTagLinkedList
{
    public Signature Signature;
    public CmsTagDescriptor Descriptor;
    public CmsTagLinkedList? Next;

    public CmsTagLinkedList(Signature sig, CmsTagDescriptor desc, CmsTagLinkedList? next = null)
    {
        Descriptor = desc;
        Signature = sig;
        Next = next;
    }
}
