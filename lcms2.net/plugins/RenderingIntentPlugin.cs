using System.Diagnostics;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     This function should join all profiles specified in the array into a single LUT.
/// </summary>
/// <remarks>Implements the <c>cmsIntentFn</c> typedef.</remarks>
public delegate Pipeline IntentFn(Context? context, int numProfiles, int[] intents, object[] profiles, bool[] bpc, double[] adaptationStates, uint flags);

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
    public static Pipeline DefaultIccIntents(Context? context, int[] intents, object[] profiles, bool[] bpc, double[] adaptationStates, uint flags)
    {
        throw new NotImplementedException();
    }

    internal static bool RegisterPlugin(Context? context, RenderingIntentPlugin? plugin)
    {
        var ctx = Context.GetRenderingIntentsPlugin(context);

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

    private static readonly RenderingIntentsPluginChunk _intentsPluginChunk = new();

    private RenderingIntentsPluginChunk()
    { }

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupIntentsList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.IntentPlugin] = _intentsPluginChunk;
    }

    private static void DupIntentsList(ref Context ctx, in Context src)
    {
        RenderingIntentsPluginChunk newHead = new();
        IntentsList? anterior = null;
        var head = (RenderingIntentsPluginChunk?)src.chunks[(int)Chunks.IntentPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.intents; entry is not null; entry = entry.next)
        {
            // We want to keep the linked list order, so this is a little bit tricky
            IntentsList newEntry = new(entry.intent, entry.description, entry.link, null);

            if (anterior is not null)
                anterior.next = newEntry;

            anterior = newEntry;

            if (newHead.intents is null)
                newHead.intents = newEntry;
        }

        ctx.chunks[(int)Chunks.IntentPlugin] = newHead;
    }
}
