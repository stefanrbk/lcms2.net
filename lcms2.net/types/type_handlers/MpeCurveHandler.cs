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

public class MpeCurveHandler : TagTypeHandler
{
    #region Public Constructors

    public MpeCurveHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public MpeCurveHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        (value as Stage)?.Clone();

    public override void Free(object value) =>
        (value as Stage)?.Dispose();

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        // Get actual position as a basis for element offsets
        var baseOffset = (uint)(io.Tell() - sizeof(TagBase));

        if (!io.ReadUInt16Number(out var inputChans)) return null;
        if (!io.ReadUInt16Number(out var outputChans)) return null;

        if (inputChans != outputChans) return null;

        object gammaTables = new ToneCurve[inputChans];
        var mpe = ReadPositionTable(io, inputChans, baseOffset, ref gammaTables, ReadMpeCurve)
            ? Stage.AllocToneCurves(StateContainer, inputChans, (ToneCurve[])gammaTables)
            : null;

        for (var i = 0; i < inputChans; i++)
            ((ToneCurve[])gammaTables)[i]?.Dispose();

        numItems = mpe is not null ? 1 : 0;
        return mpe;
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        var mpe = (Stage)value;

        var baseOffset = (uint)(io.Tell() - sizeof(TagBase));

        // Write the header. Since those are curves, input and output channels are the same
        if (!io.Write((ushort)mpe.InputChannels)) return false;
        if (!io.Write((ushort)mpe.InputChannels)) return false;

        if (!WritePositionTable(io, 0, mpe.InputChannels, baseOffset, mpe.Data, WriteMpeCurve)) return false;

        return true;
    }

    #endregion Public Methods
}
