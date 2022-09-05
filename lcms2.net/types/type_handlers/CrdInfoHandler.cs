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
using lcms2.plugins;

namespace lcms2.types.type_handlers;

public class CrdInfoHandler : TagTypeHandler
{
    #region Public Constructors

    public CrdInfoHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public CrdInfoHandler(object? state = null)
        : base(default, state, 0) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        (value as Mlu)?.Clone();

    public override void Free(object value) =>
        (value as Mlu)?.Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        Mlu mlu = new(StateContainer);

        numItems = 0;

        if (!ReadCountAndString(io, mlu, ref sizeOfTag, "nm")) goto Error;
        if (!ReadCountAndString(io, mlu, ref sizeOfTag, "#0")) goto Error;
        if (!ReadCountAndString(io, mlu, ref sizeOfTag, "#1")) goto Error;
        if (!ReadCountAndString(io, mlu, ref sizeOfTag, "#2")) goto Error;
        if (!ReadCountAndString(io, mlu, ref sizeOfTag, "#3")) goto Error;

        numItems = 1;
        return mlu;

    Error:
        mlu?.Dispose();

        return null;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var mlu = (Mlu)value;

        if (!WriteCountAndString(io, mlu, "nm")) return false;
        if (!WriteCountAndString(io, mlu, "#0")) return false;
        if (!WriteCountAndString(io, mlu, "#1")) return false;
        if (!WriteCountAndString(io, mlu, "#2")) return false;
        if (!WriteCountAndString(io, mlu, "#3")) return false;

        return true;
    }

    #endregion Public Methods
}
