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

using System.Runtime.InteropServices;

namespace lcms2.types.type_handlers;

public class DateTimeHandler : TagTypeHandler
{
    #region Public Constructors

    public DateTimeHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public DateTimeHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        (DateTime)value;

    public override void Free(object value)
    { }

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        try
        {
            var buf = new byte[sizeof(DateTimeNumber)];
            if (io.Read(buf) != sizeof(DateTimeNumber)) return null;
            var dt = MemoryMarshal.Read<DateTimeNumber>(buf);

            numItems = 1;
            return (DateTime)dt;
        }
        catch
        {
            return null;
        }
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        var dt = (DateTime)value;
        var timestamp = (DateTimeNumber)dt;
        try
        {
            var buf = new byte[sizeof(DateTimeNumber)];
            MemoryMarshal.Write(buf, ref timestamp);
            io.Write(buf);
        }
        catch
        {
            return false;
        }
        return true;
    }

    #endregion Public Methods
}
