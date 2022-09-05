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

public class MpeMatrixHandler : TagTypeHandler
{
    #region Public Constructors

    public MpeMatrixHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public MpeMatrixHandler(object? state = null)
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

        if (!io.ReadUInt16Number(out var inputChans)) return null;
        if (!io.ReadUInt16Number(out var outputChans)) return null;

        // Input and output channels may be ANY (up to 0xFFFF), but we choose to limit to 16
        // channels for now
        if (inputChans >= maxChannels || outputChans >= maxChannels) return null;

        var numElements = (uint)inputChans * outputChans;

        var matrix = new double[numElements];
        var offsets = new double[outputChans];

        for (var i = 0; i < numElements; i++)
        {
            if (!io.ReadFloat32Number(out var v)) return null;
            matrix[i] = v;
        }

        for (var i = 0; i < outputChans; i++)
        {
            if (!io.ReadFloat32Number(out var v)) return null;
            offsets[i] = v;
        }

        var mpe = Stage.AllocMatrix(StateContainer, outputChans, inputChans, matrix, offsets);

        numItems = 1;
        return mpe;
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        var mpe = (Stage)value;
        var matrix = (Stage.MatrixData)mpe.Data;

        if (!io.Write((ushort)mpe.InputChannels)) return false;
        if (!io.Write((ushort)mpe.OutputChannels)) return false;

        var numElements = mpe.InputChannels * mpe.OutputChannels;

        for (var i = 0; i < numElements; i++)
            if (!io.Write((float)matrix.Double[i])) return false;

        for (var i = 0; i < mpe.OutputChannels; i++)
        {
            if (!io.Write((float)(matrix.Offset?[i] ?? 0.0f))) return false;
        }

        return true;
    }

    #endregion Public Methods
}
