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

public class UcrBg : ICloneable, IDisposable
{
    #region Fields

    public ToneCurve Bg;
    public Mlu Description;
    public ToneCurve Ucr;

    private bool _disposed;

    #endregion Fields

    #region Public Constructors

    public UcrBg(ToneCurve ucr, ToneCurve bg, Mlu description)
    {
        Ucr = ucr;
        Bg = bg;
        Description = description;

        _disposed = false;
    }

    #endregion Public Constructors

    #region Public Methods

    public object Clone() =>
        new UcrBg((ToneCurve)Ucr.Clone(), (ToneCurve)Bg.Clone(), (Mlu)Description.Clone());

    public void Dispose()
    {
        if (!_disposed)
        {
            Ucr?.Dispose();
            Bg?.Dispose();
            Description?.Dispose();

            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    #endregion Public Methods
}
