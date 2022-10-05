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

public class TextHandler : TagTypeHandler
{
    #region Public Constructors

    public TextHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public TextHandler(object? state = null)
        : this(default, state) { }

    #endregion Public Constructors

    #region Public Methods

    public override object? Duplicate(object value, int num) =>
        ((Mlu)value).Clone();

    public override void Free(object value) =>
        ((Mlu)value).Dispose();

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        // Create a container
        Mlu mlu = new(StateContainer);
        numItems = 0;

        // We need to store the "\0" at the end, so +1
        if (sizeOfTag == Int32.MaxValue) goto Error;

        var text = new byte[sizeOfTag + 1];

        if (io.Read(text, 0, sizeOfTag) != sizeOfTag) goto Error;

        numItems = 1;

        // Keep the result
        if (!mlu.SetAscii(Mlu.noLanguage, Mlu.noCountry, text)) goto Error;
        return mlu;

    Error:
        mlu.Dispose();
        return null;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var mlu = (Mlu)value;

        // Get the size of the string. Note there is an extra "\0" at the end
        var size = mlu.GetAscii(Mlu.noLanguage, Mlu.noCountry, default);
        if (size == 0) return false; // Cannot be zero!

        // Create memory
        var buffer = new byte[size];
        mlu.GetAscii(Mlu.noLanguage, Mlu.noCountry, buffer);

        // Write it, including separator
        io.Write(buffer, 0, (int)size);

        return true;
    }

    #endregion Public Methods
}
