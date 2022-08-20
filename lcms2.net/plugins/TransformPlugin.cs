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

    internal static bool RegisterPlugin(Context? context, TransformPlugin? plugin)
    {
        var ctx = Context.GetTransformPlugin(context);

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

    private static readonly TransformPluginChunk _transformChunk = new();

    private TransformPluginChunk()
    { }

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupTransformList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.TransformPlugin] = _transformChunk;
    }

    private static void DupTransformList(ref Context ctx, in Context src)
    {
        TransformPluginChunk newHead = new();
        TransformCollection? anterior = null;
        var head = (TransformPluginChunk?)src.chunks[(int)Chunks.TransformPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.transformCollection; entry is not null; entry = entry.next)
        {
            // We want to keep the linked list order, so this is a little bit tricky
            TransformCollection newEntry = new(entry);

            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.transformCollection is null)
                newHead.transformCollection = newEntry;
        }

        ctx.chunks[(int)Chunks.TransformPlugin] = newHead;
    }
}
