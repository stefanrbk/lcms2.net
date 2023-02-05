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
using lcms2.types;

namespace lcms2.plugins;

public class PluginFormatters : PluginBase
{
    public CmsFormatterFactory FormattersFactory { get; internal set; }

    public PluginFormatters(
        uint expectedVersion,
        Signature magic,
        Signature type,
        CmsFormatterFactory formatterFactory)

        : base(expectedVersion, magic, type)
    {
        FormattersFactory = formatterFactory;
    }
}

public unsafe delegate byte* Formatter16(Transform CMMcargo, ushort* Values, byte* Buffer, uint Stride);

public unsafe delegate byte* FormatterFloat(Transform CMMcargo, float* Values, byte* Buffer, uint Stride);

public unsafe class CmsFormatter
{
    #region Fields

    private readonly Formatter16? _fmt16;

    private readonly FormatterFloat? _fmtFloat;

    #endregion Fields

    #region Public Constructors

    public CmsFormatter(Formatter16 fn16) =>
        _fmt16 = fn16;

    public CmsFormatter(FormatterFloat fnFloat) =>
        _fmtFloat = fnFloat;

    #endregion Public Constructors

    #region Private Constructors

    private CmsFormatter()
    { }

    #endregion Private Constructors

    #region Public Methods

    public void Format(Transform CMMcargo, ushort* Values, byte* Buffer, uint Stride) =>
            _fmt16?.Invoke(CMMcargo, Values, Buffer, Stride);

    public void Format(Transform CMMcargo, float* Values, byte* Buffer, uint Stride) =>
        _fmtFloat?.Invoke(CMMcargo, Values, Buffer, Stride);

    #endregion Public Methods
}

public delegate CmsFormatter CmsFormatterFactory(uint Type, FormatterDirection Dir, uint dwFlags);
