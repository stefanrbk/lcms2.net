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
using System.Text;

namespace lcms2.types;

public partial struct Signature : ICloneable
{
    #region Fields

    public static readonly Signature LcmsSignature = new("lcms");
    public static readonly Signature MagicNumber = new("ascp");

    private readonly uint _value;

    #endregion Fields

    #region Public Constructors

    public Signature(uint value) =>
           _value = value;

    public Signature(string value)
    {
        byte[] bytes = { 0x20, 0x20, 0x20, 0x20 };
        var s = Encoding.ASCII.GetBytes(value);
        switch (s.Length)
        {
            case 0:
                break;

            case 1:
                bytes[0] = s[0];
                break;

            case 2:
                bytes[1] = s[1];
                goto case 1;
            case 3:
                bytes[2] = s[2];
                goto case 2;
            default:
                bytes[3] = s[3];
                goto case 3;
        };
        this._value = ((uint)bytes[0] << 24) + ((uint)bytes[1] << 16) + ((uint)bytes[2] << 8) + bytes[3];
    }

    #endregion Public Constructors

    #region Public Methods

    public static implicit operator uint(Signature v) =>
        v._value;

    public object Clone() =>
        new Signature(_value);

    public override string ToString()
    {
        var str = new char[4];

        str[0] = (char)((_value >> 24) & 0xFF);
        str[1] = (char)((_value >> 16) & 0xFF);
        str[2] = (char)((_value >> 8) & 0xFF);
        str[3] = (char)(_value & 0xFF);

        return new string(str);
    }

    #endregion Public Methods
}
