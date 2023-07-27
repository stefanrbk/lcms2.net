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

namespace lcms2.state;

internal unsafe class TransformPluginChunkType : IDup
{
    public TransformCollection* TransformCollection;

    public object? Dup(Context ctx)
    {
        var head = this;
        TransformCollection* Anterior = null, entry;
        TransformPluginChunkType newHead = new();

        _cmsAssert(ctx);
        _cmsAssert(head);

        // Walk the list copying all nodes
        for (entry = head.TransformCollection;
             entry is not null;
             entry = entry->Next)
        {
            var newEntry = _cmsSubAllocDup<TransformCollection>(ctx.MemPool, entry);

            if (newEntry is null)
                return null;

            // We want to keep the linked list order, so this is a little bit tricky
            newEntry->Next = null;
            if (Anterior is not null)
                Anterior->Next = newEntry;

            Anterior = newEntry;

            if (newHead.TransformCollection is null)
                newHead.TransformCollection = newEntry;
        }

        return newHead;
    }
}
