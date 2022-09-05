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

public class ColorantTableHandler : TagTypeHandler
{
    #region Public Constructors

    public ColorantTableHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public ColorantTableHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        (value as NamedColorList)?.Clone();

    public override void Free(object value) =>
        (value as NamedColorList)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        byte[] name = new byte[33];

        if (!io.ReadUInt32Number(out var count)) return null;

        if (count > maxChannels)
        {
            state.State.SignalError(StateContainer, ErrorCode.Range, "Too many colorants '{0}'", count);
            return null;
        }

        NamedColorList list = new(StateContainer, count, "", "");

        for (var i = 0; i < count; i++)
        {
            if (io.Read(name, 0, 32) != 32) goto Error;

            if (!io.ReadUInt16Array(3, out var pcs)) goto Error;

            if (!list.Append(new string(name.Select(c => (char)c).ToArray()), pcs, null)) goto Error;
        }

        numItems = 1;
        return list;

    Error:
        numItems = 0;
        list.Dispose();
        return null;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var namedColorList = (NamedColorList)value;

        var numColors = namedColorList.numColors;

        if (!io.Write(numColors)) return false;

        for (var i = 0u; i < numColors; i++)
        {
            if (!namedColorList.Info(i, out var root, out _, out _, out var pcs, out _)) return false;

            for (var j = 0; j < root.Length; j++)
            {
                if (!io.Write((byte)root[j])) return false;
            }
            if (!io.Write(3, pcs)) return false;
        }

        return true;
    }

    #endregion Public Methods
}
