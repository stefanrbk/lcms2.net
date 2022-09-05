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
using System.Globalization;
using System.Text;

namespace lcms2.types;

public partial struct Signature
{
    #region Classes

    public class Formatter : IFormatProvider, ICustomFormatter
    {
        #region Public Methods

        public string Format(string? format, object? obj, IFormatProvider? provider)
        {
            if (obj is null)
                return string.Empty;

            if (obj is Signature value)
            {
                // Text output
                if (format?.ToUpper().StartsWith("T") ?? false)
                {
                    var sb = new StringBuilder(4);
                    sb.Append((char)((value._value >> 24) & 0xFF));
                    sb.Append((char)((value._value >> 16) & 0xFF));
                    sb.Append((char)((value._value >> 8) & 0xFF));
                    sb.Append((char)(value._value & 0xFF));
                    return sb.ToString();
                }
                // Hex output
                if (format?.ToUpper().StartsWith("X") ?? false)
                    return String.Format(provider, "{" + format + "}", (uint)obj);
            }

            // Use default for all other formatting
            return obj is IFormattable formattable
                ? formattable.ToString(format, CultureInfo.CurrentCulture)
                : obj.ToString() ?? String.Empty;
        }

        public object? GetFormat(Type? formatType) =>
            formatType == typeof(ICustomFormatter) ? this : (object?)null;

        #endregion Public Methods
    }

    #endregion Classes
}
