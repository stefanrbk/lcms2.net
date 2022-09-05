using System.Diagnostics;

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
public sealed class RenderingIntentPlugin: Plugin
{
    public string Description;
    public Signature Intent;
    public IntentFn Link;

    public RenderingIntentPlugin(Signature magic, uint expectedVersion, Signature type, Signature intent, IntentFn link, string description)
        : base(magic, expectedVersion, type)
    {
        Intent = intent;
        Link = link;
        Description = description;
    }

    /// <summary>
    ///     The default ICC intents (perceptual, saturation, rel.col, and abs.col)
    /// </summary>
    /// <remarks>Implements the <c>_cmsDefaultICCintents</c> function.</remarks>
    public static Pipeline DefaultIccIntents(object? context, int[] intents, object[] profiles, bool[] bpc, double[] adaptationStates, uint flags)
    {
        throw new NotImplementedException();
    }

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
}

internal class IntentsList
{
    internal string description;

    internal Signature intent;

    internal IntentFn link;

    internal IntentsList? next;

    public IntentsList(Signature intent, string description, IntentFn link, IntentsList? next)
    {
        this.intent = intent;
        this.description = description;
        this.link = link;
        this.next = next;
    }
}

internal sealed class RenderingIntentsPluginChunk
{
    internal static RenderingIntentsPluginChunk global = new();
    internal IntentsList? intents;

    internal static RenderingIntentsPluginChunk Default => new();

    private RenderingIntentsPluginChunk()
    { }
}
