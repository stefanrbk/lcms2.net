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
namespace lcms2.types;

public class NamedColorList : ICloneable, IDisposable
{
    #region Fields

    internal uint colorantCount;

    internal List<NamedColor> list;
    internal uint numColors;
    internal string prefix;
    internal object? state;
    internal string suffix;

    #endregion Fields

    #region Public Constructors

    public NamedColorList(object? state, uint colorantCount, string prefix, string suffix)
    {
        this.state = state;
        numColors = 0;
        list = new List<NamedColor>();

        this.prefix = prefix;
        this.suffix = suffix;

        this.colorantCount = colorantCount;
    }

    #endregion Public Constructors

    #region Public Methods

    public bool Append(string name, ushort[] pcs, ushort[]? colorant)
    {
        throw new NotImplementedException();
    }

    public object Clone() => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();

    public bool Info(uint numColor, out string name, out string prefix, out string suffix, out ushort[] pcs, out ushort[] colorant)
    {
        name = prefix = suffix = String.Empty;
        pcs = colorant = Array.Empty<ushort>();

        if (numColor >= numColors) return false;

        name = list[(int)numColor].name;
        prefix = this.prefix;
        suffix = this.suffix;
        pcs = (ushort[])list[(int)numColor].pcs.Clone();
        colorant = (ushort[])list[(int)numColor].deviceColorant.Clone();

        return true;
    }

    #endregion Public Methods
}
