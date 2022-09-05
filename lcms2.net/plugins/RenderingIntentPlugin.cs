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
///     This function should join all profiles specified in the array into a single LUT.
/// </summary>
/// <remarks>Implements the <c>cmsIntentFn</c> typedef.</remarks>
public delegate Pipeline IntentFn(object? context, int numProfiles, int[] intents, object[] profiles, bool[] bpc, double[] adaptationStates, uint flags);

/// <summary>
///     Custom intent plugin
/// </summary>
/// <remarks>
///     Each plugin defines a single intent number. <br/> Implements the <c>cmsPluginTag</c> struct.
/// </remarks>
public sealed class RenderingIntentPlugin : Plugin
{
    #region Fields

    public string Description;
    public Signature Intent;
    public IntentFn Link;

    #endregion Fields

    #region Public Constructors

    public RenderingIntentPlugin(Signature magic, uint expectedVersion, Signature type, Signature intent, IntentFn link, string description)
        : base(magic, expectedVersion, type)
    {
        Intent = intent;
        Link = link;
        Description = description;
    }

    #endregion Public Constructors

    #region Public Methods

    /// <summary>
    ///     The default ICC intents (perceptual, saturation, rel.col, and abs.col)
    /// </summary>
    /// <remarks>Implements the <c>_cmsDefaultICCintents</c> function.</remarks>
    public static Pipeline DefaultIccIntents(object? context, int[] intents, object[] profiles, bool[] bpc, double[] adaptationStates, uint flags)
    {
        throw new NotImplementedException();
    }

    #endregion Public Methods

    #region Internal Methods

    internal static bool RegisterPlugin(object? context, RenderingIntentPlugin? plugin)
    {
        var ctx = State.GetRenderingIntentsPlugin(context);

        if (plugin is null)
        {
            ctx.intents = null;
            return true;
        }

        ctx.intents = new IntentsList(plugin.Intent, plugin.Description, plugin.Link, ctx.intents);

        return true;
    }

    #endregion Internal Methods
}

internal class IntentsList
{
    #region Fields

    internal string description;

    internal Signature intent;

    internal IntentFn link;

    internal IntentsList? next;

    #endregion Fields

    #region Public Constructors

    public IntentsList(Signature intent, string description, IntentFn link, IntentsList? next)
    {
        this.intent = intent;
        this.description = description;
        this.link = link;
        this.next = next;
    }

    #endregion Public Constructors
}

internal sealed class RenderingIntentsPluginChunk
{
    #region Fields

    internal static RenderingIntentsPluginChunk global = new();
    internal IntentsList? intents;

    #endregion Fields

    #region Private Constructors

    private RenderingIntentsPluginChunk()
    { }

    #endregion Private Constructors

    #region Properties

    internal static RenderingIntentsPluginChunk Default => new();

    #endregion Properties
}
