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

namespace lcms2.types.type_handlers;

public class ProfileSequenceIdHandler : TagTypeHandler
{
    #region Public Constructors

    public ProfileSequenceIdHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public ProfileSequenceIdHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        (value as Sequence)?.Clone();

    public override void Free(object value) =>
        (value as Sequence)?.Dispose();

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        // Get actual position as a basis for element offsets
        var baseOffset = io.Tell() - sizeof(TagBase);

        // Get table count
        if (!io.ReadUInt32Number(out var count)) return null;
        sizeOfTag -= sizeof(uint);

        // Allocate an empty structure
        object? outSeq = Sequence.Alloc(StateContainer, (int)count);
        if (outSeq is null) return null;

        // Read the position table
        if (!ReadPositionTable(io, (int)count, (uint)baseOffset, ref outSeq, ReadSequenceId))
        {
            ((IDisposable)outSeq)?.Dispose();
            return null;
        }

        // Success
        numItems = 1;
        return outSeq;
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        var seq = (Sequence)value;

        // Keep the base offset
        var baseOffset = io.Tell() - sizeof(TagBase);

        // This is the table count
        if (!io.Write(seq.SeqCount)) return false;

        // This is the position table and content
        return WritePositionTable(io, 0, (uint)seq.SeqCount, (uint)baseOffset, value, WriteSequenceId);
    }

    #endregion Public Methods
}
