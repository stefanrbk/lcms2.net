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
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Pipelines, Multi Process Elements
/// </summary>
/// <remarks>Implements the <c>cmsPluginMultiProcessElement</c> struct.</remarks>
public sealed class MultiProcessElementPlugin : Plugin
{
    #region Fields

    public TagTypeHandler Handler;

    #endregion Fields

    #region Public Constructors

    public MultiProcessElementPlugin(Signature magic, uint expectedVersion, Signature type, TagTypeHandler handler)
        : base(magic, expectedVersion, type) =>

        Handler = handler;

    #endregion Public Constructors

    #region Internal Methods

    internal static bool RegisterPlugin(object? state, MultiProcessElementPlugin? plugin) =>
        TagTypePluginChunk.MPE.RegisterPlugin(state, plugin);

    #endregion Internal Methods
}
