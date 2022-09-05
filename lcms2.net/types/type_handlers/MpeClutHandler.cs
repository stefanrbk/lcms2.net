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

public class MpeClutHandler : TagTypeHandler
{
    #region Public Constructors

    public MpeClutHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public MpeClutHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        (value as Stage)?.Clone();

    public override void Free(object value) =>
        (value as Stage)?.Dispose();

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        var dimensions8 = new byte[16];

        numItems = 0;

        if (!io.ReadUInt16Number(out var inputChans)) return null;
        if (!io.ReadUInt16Number(out var outputChans)) return null;

        if (inputChans == 0 || outputChans == 0) return null;

        if (io.Read(dimensions8) != 16) return null;

        // Copy MaxInputDimensions at most. Expand to uint
        var numMaxGrids = (uint)Math.Min((ushort)maxInputDimensions, inputChans);
        var gridPoints = new uint[numMaxGrids];

        for (var i = 0; i < numMaxGrids; i++)
        {
            if (dimensions8[i] == 1) return null; // Impossible value, 0 for no CLUT or at least 2
            gridPoints[i] = dimensions8[i];
        }

        // Allocate the true CLUT
        var mpe = Stage.AllocCLutFloat(StateContainer, gridPoints, inputChans, outputChans, null);
        if (mpe is null) goto Error;

        // Read and sanitize the data
        var clut = (Stage.CLutFloatData)mpe.Data;
        for (var i = 0; i < clut.NumEntries; i++)
            if (!io.ReadFloat32Number(out clut.Table[i])) goto Error;

        numItems = 1;
        return mpe;

    Error:

        mpe?.Dispose();
        return null;
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        var dimensions8 = new byte[16];

        // Only floats are supported in MPE
        var mpe = (Stage)value;
        if (mpe.Data is not Stage.CLutFloatData clut)
            return false;

        // Check for maximum number of channels supported by lcms
        if (mpe.InputChannels > maxInputDimensions) return false;

        if (!io.Write((ushort)mpe.InputChannels)) return false;
        if (!io.Write((ushort)mpe.OutputChannels)) return false;

        for (var i = 0; i < mpe.InputChannels; i++)
            dimensions8[i] = (byte)clut.Params.NumSamples[i];

        io.Write(dimensions8);

        for (var i = 0; i < clut.NumEntries; i++)
            if (!io.Write(clut.Table[i])) return false;

        return true;
    }

    #endregion Public Methods
}
