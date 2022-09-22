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

public class ParametricCurveHandler : TagTypeHandler
{
    #region Fields

    private static readonly int[] _readParamsByType = new int[] { 1, 3, 4, 5, 7 };

    private static readonly int[] _writeParamsByType = new int[] { 0, 1, 3, 4, 5, 7 };

    #endregion Fields

    #region Public Constructors

    public ParametricCurveHandler(Signature sig, object? state = null)
               : base(sig, state, 0) { }

    public ParametricCurveHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        (value as ToneCurve)?.Clone();

    public override void Free(object value) =>
        (value as ToneCurve)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        var @params = new double[10];

        numItems = 0;

        if (!io.ReadUInt16Number(out var type)) return null;
        if (!io.ReadUInt16Number(out _)) return null; // Reserved

        if (type > 4)
        {
            Errors.UnknownParametricCurveType(StateContainer, type);
            return null;
        }
        var n = _readParamsByType[type];

        for (var i = 0; i < n; i++)
            if (!io.Read15Fixed16Number(out @params[i])) return null;

        var newGamma = ToneCurve.BuildParametric(StateContainer, type + 1, @params);

        numItems = 1;
        return newGamma;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var curve = (ToneCurve)value;

        var typeN = curve.segments[0].Type;

        if (curve.NumSegments > 1 || typeN < 1)
            return Errors.ParametricCurveCannotWrite(StateContainer);

        if (typeN > 5)
            return Errors.UnsupportedParametricCurve(StateContainer);

        var numParams = _writeParamsByType[typeN];

        if (!io.Write((ushort)(curve.segments[0].Type - 1))) return false;
        if (!io.Write((ushort)0)) return false; // Reserved

        for (var i = 0; i < numParams; i++)
            if (!io.Write(curve.segments[0].Params![i])) return false;

        return true;
    }

    #endregion Public Methods
}
