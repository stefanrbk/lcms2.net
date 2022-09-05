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
using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     This plugin adds new handlers, replacing them if they already exist.
/// </summary>
/// <remarks>Implements the <c>cmsPluginFormatters</c> typedef.</remarks>
public sealed class FormattersPlugin : Plugin
{
    #region Fields

    public FormatterFactory FormattersFactory;

    #endregion Fields

    #region Public Constructors

    public FormattersPlugin(Signature magic, uint expectedVersion, Signature type, FormatterFactory formatterFactory)
        : base(magic, expectedVersion, type) =>
        FormattersFactory = formatterFactory;

    #endregion Public Constructors

    #region Internal Methods

    internal static bool RegisterPlugin(object? state, FormattersPlugin? plugin)
    {
        var ctx = State.GetFormattersPlugin(state);

        if (plugin is null)
        {
            ctx.factoryList = null;
            return true;
        }

        ctx.factoryList = new FormattersFactoryList(plugin.FormattersFactory, ctx.factoryList);

        return true;
    }

    #endregion Internal Methods
}

internal class FormattersFactoryList
{
    #region Fields

    internal FormatterFactory? factory;

    internal FormattersFactoryList? next;

    #endregion Fields

    #region Public Constructors

    public FormattersFactoryList(FormatterFactory? factory, FormattersFactoryList? next)
    {
        this.factory = factory;
        this.next = next;
    }

    #endregion Public Constructors
}

internal sealed class FormattersPluginChunk
{
    #region Fields

    internal static FormattersPluginChunk global = new();
    internal FormattersFactoryList? factoryList;

    #endregion Fields

    #region Private Constructors

    private FormattersPluginChunk()
    { }

    #endregion Private Constructors

    #region Properties

    internal static FormattersPluginChunk Default => new();

    #endregion Properties
}
