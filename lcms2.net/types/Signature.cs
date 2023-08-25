//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
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

using System.Diagnostics;

namespace lcms2.types;

public readonly partial struct Signature : ICloneable
{
    #region Fields

    //public static readonly Signature LcmsSignature = new("lcms"u8);
    //public static readonly Signature MagicNumber = new("ascp"u8);

    private readonly uint _value;

    #endregion Fields

    #region Public Constructors
    
    [DebuggerStepThrough]
    public Signature(uint value) =>
        _value = value;

    public Signature(ReadOnlySpan<byte> value)
    {
        Span<byte> bytes = stackalloc byte[] { 0x20, 0x20, 0x20, 0x20 };
        if (value.Length > 4)
            value = value[..4];
        value.CopyTo(bytes);
        _value = BitConverter.ToUInt32(bytes);
    }

    #endregion Public Constructors

    #region Public Methods

    public static implicit operator uint(Signature v) =>
        v._value;

    public static implicit operator Signature(uint v) =>
        new(v);

    public object Clone() =>
        new Signature(_value);

    public override string ToString()
    {
        _cmsTagSignature2String(this);
        return _cmsTagSignature2String(this);
    }

    #endregion Public Methods
}
