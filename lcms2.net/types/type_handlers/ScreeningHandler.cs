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

public class ScreeningHandler : TagTypeHandler
{
    #region Public Constructors

    public ScreeningHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public ScreeningHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        (value as Screening)?.Clone();

    public override void Free(object value)
    { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        if (!io.ReadUInt32Number(out var flag)) return null;
        if (!io.ReadUInt32Number(out var count)) return null;

        Screening sc = new(flag, (int)count);
        if (sc.NumChannels > maxChannels - 1)
            sc.NumChannels = maxChannels - 1;

        for (var i = 0; i < sc.NumChannels; i++)
        {
            if (!io.Read15Fixed16Number(out sc.Channels[i].Frequency)) return null;
            if (!io.Read15Fixed16Number(out sc.Channels[i].ScreenAngle)) return null;
            if (!io.ReadUInt32Number(out var shape)) return null;
            sc.Channels[i].SpotShape = (SpotShape)shape;
        }

        numItems = 1;
        return sc;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var sc = (Screening)value;

        if (!io.Write(sc.Flags)) return false;
        if (!io.Write(sc.NumChannels)) return false;

        for (var i = 0; i < sc.NumChannels; i++)
        {
            if (!io.Write(sc.Channels[i].Frequency)) return false;
            if (!io.Write(sc.Channels[i].ScreenAngle)) return false;
            if (!io.Write((uint)sc.Channels[i].SpotShape)) return false;
        }

        return true;
    }

    #endregion Public Methods
}
