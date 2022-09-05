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
///     Transform plugin
/// </summary>
/// <remarks>Implements the <c>cmsPluginTransform</c> typedef.</remarks>
public sealed class TransformPlugin : Plugin
{
    #region Fields

    public Transform.Factory Factories;

    #endregion Fields

    #region Public Constructors

    public TransformPlugin(Signature magic, uint expectedVersion, Signature type, Transform.Factory factories)
        : base(magic, expectedVersion, type) =>
        Factories = factories;

    #endregion Public Constructors

    #region Internal Methods

    internal static bool RegisterPlugin(object? state, TransformPlugin? plugin)
    {
        var ctx = State.GetTransformPlugin(state);

        if (plugin is null)
        {
            ctx.transformCollection = null;
            return true;
        }

        // Check for full xform plugins previous to 2.8, we would need an adapter in that case
        var old = plugin.ExpectedVersion < 2080;

        ctx.transformCollection = new(plugin.Factories, old, ctx.transformCollection);

        return true;
    }

    #endregion Internal Methods
}

internal class TransformCollection
{
    #region Fields

    internal Transform.Factory factory;

    internal TransformCollection? next;

    internal bool oldXform;

    #endregion Fields

    #region Public Constructors

    public TransformCollection(Transform.Factory factory, bool oldXform, TransformCollection? next)
    {
        this.factory = factory;
        this.oldXform = oldXform;
        this.next = next;
    }

    public TransformCollection(TransformCollection other, TransformCollection? next = null)
    {
        factory = other.factory;
        oldXform = other.oldXform;
        this.next = next;
    }

    #endregion Public Constructors
}

internal sealed class TransformPluginChunk
{
    #region Fields

    internal static TransformPluginChunk global = new();
    internal TransformCollection? transformCollection;

    #endregion Fields

    #region Private Constructors

    private TransformPluginChunk()
    { }

    #endregion Private Constructors

    #region Properties

    internal static TransformPluginChunk Default => new();

    #endregion Properties
}
