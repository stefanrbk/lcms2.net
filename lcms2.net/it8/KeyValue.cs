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
namespace lcms2.it8;

internal class KeyValue
{
    #region Fields

    public readonly string Key;
    public readonly WriteMode Mode;
    public readonly string? Subkey;
    public readonly List<KeyValue> Subkeys = new();
    public readonly string? Value;

    #endregion Fields

    #region Public Constructors

    public KeyValue(string key, WriteMode mode) =>
        (Key, Mode) = (key, mode);

    public KeyValue(string key, string value, WriteMode mode) =>
        (Key, Value, Mode) = (key, value, mode);

    public KeyValue(string key, double value, WriteMode mode) =>
        (Key, Value, Mode) = (key, value.ToString(), mode);

    public KeyValue(string key, string subkey, string value, WriteMode mode) =>
        (Key, Subkey, Value, Mode) = (key, subkey, value, mode);

    public KeyValue(string key, string subkey, double value, WriteMode mode) =>
        (Key, Subkey, Value, Mode) = (key, subkey, value.ToString(), mode);

    #endregion Public Constructors
}
