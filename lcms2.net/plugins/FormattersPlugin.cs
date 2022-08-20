using System.Diagnostics;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     This plugin adds new handlers, replacing them if they already exist.
/// </summary>
/// <remarks>Implements the <c>cmsPluginFormatters</c> typedef.</remarks>
public sealed class FormattersPlugin: Plugin
{
    public FormatterFactory FormattersFactory;

    public FormattersPlugin(Signature magic, uint expectedVersion, Signature type, FormatterFactory formatterFactory)
        : base(magic, expectedVersion, type) =>
        FormattersFactory = formatterFactory;

    internal static bool RegisterPlugin(Context? context, FormattersPlugin? plugin)
    {
        var ctx = Context.GetFormattersPlugin(context);

        if (plugin is null)
        {
            ctx.factoryList = null;
            return true;
        }

        ctx.factoryList = new FormattersFactoryList(plugin.FormattersFactory, ctx.factoryList);

        return true;
    }
}

internal class FormattersFactoryList
{
    internal FormatterFactory? factory;

    internal FormattersFactoryList? next;

    public FormattersFactoryList(FormatterFactory? factory, FormattersFactoryList? next)
    {
        this.factory = factory;
        this.next = next;
    }
}

internal sealed class FormattersPluginChunk
{
    internal static FormattersPluginChunk global = new();
    internal FormattersFactoryList? factoryList;

    private static readonly FormattersPluginChunk _curvesPluginChunk = new();

    private FormattersPluginChunk()
    { }

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupFormatterFactoryList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.FormattersPlugin] = _curvesPluginChunk;
    }

    private static void DupFormatterFactoryList(ref Context ctx, in Context src)
    {
        FormattersPluginChunk newHead = new();
        FormattersFactoryList? anterior = null;
        var head = (FormattersPluginChunk?)src.chunks[(int)Chunks.FormattersPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.factoryList; entry is not null; entry = entry.next)
        {
            // We want to keep the linked list order, so this is a little bit tricky
            FormattersFactoryList newEntry = new(entry.factory, null);

            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.factoryList is null)
                newHead.factoryList = newEntry;
        }

        ctx.chunks[(int)Chunks.FormattersPlugin] = newHead;
    }
}
