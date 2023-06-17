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

using lcms2.state;

using static lcms2.cgats.CGATS;

namespace lcms2.cgats;

internal unsafe struct IT8
{
    public uint TablesCount;
    public uint nTable;

    public TABLE* Tab;

    // Memory management
    public OWNEDMEM* MemorySink;
    public SUBALLOCATOR Allocator;

    // Parser state machine
    public SYMBOL sy;
    public int ch;

    public int inum;
    public double dnum;

    public @string* id;
    public @string* str;

    // Allowed keywords & datasets. They have visibility on whole stream
    public KEYVALUE* ValidKeywords;
    public KEYVALUE* ValidSampleID;

    public byte* Source;
    public int lineno;

    public FILECTX** FileStack;
    public int IncludeSP;

    public byte* MemoryBlock;

    public fixed byte DoubleFormatter[MAXID];

    public Context? ContextID;
}