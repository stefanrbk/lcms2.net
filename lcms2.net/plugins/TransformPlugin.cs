using System.Diagnostics;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Transform plugin
/// </summary>
/// <remarks>Implements the <c>cmsPluginTransform</c> typedef.</remarks>
public sealed class TransformPlugin: Plugin
{
    public Transform.Factory Factories;

    public TransformPlugin(Signature magic, uint expectedVersion, Signature type, Transform.Factory factories)
        : base(magic, expectedVersion, type) =>
        Factories = factories;

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
}

internal class TransformCollection
{
    internal Transform.Factory factory;

    internal TransformCollection? next;

    internal bool oldXform;

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
}

internal sealed class TransformPluginChunk
{
    internal static TransformPluginChunk global = new();
    internal TransformCollection? transformCollection;

    internal static TransformPluginChunk Default => new();

    private TransformPluginChunk()
    { }
}
