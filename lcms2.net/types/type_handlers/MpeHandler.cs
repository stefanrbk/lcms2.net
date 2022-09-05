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

namespace lcms2.types.type_handlers;

public class MpeHandler : TagTypeHandler
{
    #region Public Constructors

    public MpeHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public MpeHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        (value as Pipeline)?.Clone();

    public override void Free(object value) =>
        (value as Pipeline)?.Dispose();

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        // Get actual position as a basis for element offset
        var baseOffset = (uint)(io.Tell() - sizeof(TagBase));

        // Read channels and element count
        if (!io.ReadUInt16Number(out var inputChans)) return null;
        if (!io.ReadUInt16Number(out var outputChans)) return null;

        if ((inputChans is 0 or >= maxChannels) ||
            (outputChans is 0 or >= maxChannels)) return null;

        // Allocate an empty LUT
        object? newLut = Pipeline.Alloc(StateContainer, inputChans, outputChans);
        if (newLut is null) return null;

        if (!io.ReadUInt32Number(out var elementCount)) goto Error;
        if (!ReadPositionTable(io, (int)elementCount, baseOffset, ref newLut, ReadMpeElem)) goto Error;

        // Check channel count
        if (inputChans != ((Pipeline)newLut).InputChannels ||
            outputChans != ((Pipeline)newLut).OutputChannels) goto Error;

        // Success
        numItems = 1;
        return newLut;

    Error:

        ((Pipeline)newLut)?.Dispose();
        return null;
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        var lut = (Pipeline)value;
        var elem = lut.elements;
        var mpeChunk = State.GetMultiProcessElementPlugin(StateContainer);

        var baseOffset = (uint)(io.Tell() - sizeof(TagBase));

        var inputChan = lut.InputChannels;
        var outputChan = lut.OutputChannels;
        var elemCount = lut.StageCount;

        var elementOffsets = new uint[elemCount];
        var elementSizes = new uint[elemCount];

        // Write the head
        if (!io.Write((ushort)inputChan)) return false;
        if (!io.Write((ushort)outputChan)) return false;
        if (!io.Write(elemCount)) return false;

        var dirPos = io.Tell();

        // Write a face directory to be filled later on
        for (var i = 0; i < elemCount; i++)
        {
            if (!io.Write((uint)0)) return false;   // Offset
            if (!io.Write((uint)0)) return false;   // Size
        }

        // Write each single tag. Keep track of the size as well.
        for (var i = 0; i < elemCount; i++)
        {
            elementOffsets[i] = (uint)io.Tell() - baseOffset;

            var elementSig = elem!.Type;

            var typeHandler = GetHandler(elementSig, mpeChunk.tagTypes);
            if (typeHandler is null)
            {
                State.SignalError(StateContainer, ErrorCode.UnknownExtension, "Found unknown MPE type '{0}'", elementSig);
                return false;
            }

            if (!io.Write((uint)elementSig)) return false;
            if (!io.Write((uint)0)) return false;
            var before = (uint)io.Tell();
            if (!typeHandler.Write(io, elem, 1)) return false;
            if (!io.WriteAlignment()) return false;

            elementSizes[i] = (uint)io.Tell() - before;

            elem = elem.Next;
        }

        // Write the directory
        var curPos = (uint)io.Tell();

        if (io.Seek(dirPos, SeekOrigin.Begin) != dirPos) return false;

        for (var i = 0; i < elemCount; i++)
        {
            if (!io.Write(elementOffsets[i])) return false;
            if (!io.Write(elementSizes[i])) return false;
        }

        if (io.Seek(curPos, SeekOrigin.Begin) != curPos) return false;

        return true;
    }

    #endregion Public Methods
}
