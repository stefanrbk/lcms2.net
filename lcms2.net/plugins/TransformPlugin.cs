using System.Diagnostics;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Transform plugin
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginTransform</c> typedef.</remarks>
public sealed class TransformPlugin : Plugin
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
    internal Transform.Factory Factory;
    internal bool OldXform;

    internal TransformCollection? Next;

    public TransformCollection(Transform.Factory factory, bool oldXform, TransformCollection? next)
    {
        Factory = factory;
        OldXform = oldXform;
        Next = next;
    }

    public TransformCollection(TransformCollection other, TransformCollection? next = null)
    {
        Factory = other.Factory;
        OldXform = other.OldXform;
        Next = next;
    }
}

internal sealed class TransformPluginChunk
{
    internal TransformCollection? transformCollection;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupTransformList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.TransformPlugin] = transformChunk;
    }

    private TransformPluginChunk()
    { }

    internal static TransformPluginChunk global = new();
    private static readonly TransformPluginChunk transformChunk = new();

    private static void DupTransformList(ref Context ctx, in Context src)
    {
        TransformPluginChunk newHead = new();
        TransformCollection? anterior = null;
        var head = (TransformPluginChunk?)src.chunks[(int)Chunks.TransformPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.transformCollection; entry is not null; entry = entry.Next)
        {
            // We want to keep the linked list order, so this is a little bit tricky
            TransformCollection newEntry = new(entry);

            if (anterior is not null)
                anterior.Next = newEntry;

            anterior = newEntry;

            if (newHead.transformCollection is null)
                newHead.transformCollection = newEntry;
        }

        ctx.chunks[(int)Chunks.TransformPlugin] = newHead;
    }
}
