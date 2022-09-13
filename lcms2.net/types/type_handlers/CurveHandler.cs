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

public class CurveHandler : TagTypeHandler
{
    #region Public Constructors

    public CurveHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public CurveHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        (value as ToneCurve)?.Clone();

    public override void Free(object value) =>
        (value as ToneCurve)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        double singleGamma;
        ToneCurve? newGamma;

        numItems = 0;
        if (!io.ReadUInt32Number(out var count)) return null;

        switch (count)
        {
            case 0: // Linear
                singleGamma = 1.0;

                newGamma = ToneCurve.BuildParametric(StateContainer, 1, singleGamma);
                if (newGamma is null) return null;
                numItems = 1;
                return newGamma;

            case 1: // Specified as the exponent of gamma function
                if (!io.ReadUInt16Number(out var singleGammaFixed)) return null;
                singleGamma = U8Fixed8toDouble(singleGammaFixed);

                numItems = 1;
                return ToneCurve.BuildParametric(StateContainer, 1, singleGamma);

            default: // Curve
                if (count > 0x7FFF)
                    return null; // This is to prevent bad guys for doing bad things.

                newGamma = ToneCurve.BuildEmptyTabulated16(StateContainer, count);
                if (newGamma is null) return null;

                if (!io.ReadUInt16Array((int)count, out newGamma.table16))
                {
                    newGamma.Dispose();
                    return null;
                }

                numItems = 1;
                return newGamma;
        }
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var curve = (ToneCurve)value;

        if (curve.NumSegments == 1 && curve.segments[0].Type == 1)
        {
            // Single gamma, preserve number
            var singleGammaFixed = DoubleToU8Fixed8(curve.segments[0].Params[0]);

            if (!io.Write((uint)1)) return false;
            if (!io.Write(singleGammaFixed)) return false;
            return true;
        }

        if (!io.Write(curve.NumEntries)) return false;
        return io.Write((int)curve.NumEntries, curve.table16);
    }

    #endregion Public Methods
}
