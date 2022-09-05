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

public class ChromaticityHandler : TagTypeHandler
{
    #region Public Constructors

    public ChromaticityHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public ChromaticityHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        ((xyYTripple)value).Clone();

    public override void Free(object value)
    { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        var chrm = new xyYTripple();

        if (!io.ReadUInt16Number(out var numChans)) return null;

        // Let's recover from a bug introduced in early versions of lcms1
        if (numChans == 0 && sizeOfTag == 32)
        {
            if (!io.ReadUInt16Number(out _)) return null;
            if (!io.ReadUInt16Number(out numChans)) return null;
        }

        if (numChans != 3) return null;

        if (!io.ReadUInt16Number(out _)) return null; // Table

        if (!io.Read15Fixed16Number(out chrm.Red.x)) return null;
        if (!io.Read15Fixed16Number(out chrm.Red.y)) return null;

        chrm.Red.Y = 1.0;

        if (!io.Read15Fixed16Number(out chrm.Green.x)) return null;
        if (!io.Read15Fixed16Number(out chrm.Green.y)) return null;

        chrm.Green.Y = 1.0;

        if (!io.Read15Fixed16Number(out chrm.Blue.x)) return null;
        if (!io.Read15Fixed16Number(out chrm.Blue.y)) return null;

        chrm.Blue.Y = 1.0;

        numItems = 1;
        return chrm;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        xyYTripple chrm = (xyYTripple)value;

        if (!io.Write((uint)3)) return false; // numChannels
        if (!io.Write((uint)0)) return false; // Table

        if (!SaveOne(chrm.Red.x, chrm.Red.y, io)) return false;
        if (!SaveOne(chrm.Green.x, chrm.Green.y, io)) return false;
        if (!SaveOne(chrm.Blue.x, chrm.Blue.y, io)) return false;

        return true;
    }

    #endregion Public Methods

    #region Private Methods

    private static bool SaveOne(double x, double y, Stream io)
    {
        if (!io.Write(DoubleToS15Fixed16(x))) return false;
        if (!io.Write(DoubleToS15Fixed16(y))) return false;

        return true;
    }

    #endregion Private Methods
}
