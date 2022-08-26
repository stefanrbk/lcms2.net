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

    internal static FormattersPluginChunk Default => new();

    private FormattersPluginChunk()
    { }
}
