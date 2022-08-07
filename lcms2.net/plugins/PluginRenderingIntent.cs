using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     This function should join all profiles specified in the array into a single LUT.
/// </summary>
/// <remarks>
///     Implements the <c>cmsIntentFn</c> typedef.</remarks>
public delegate Pipeline IntentFn(Context? context, int numProfiles, int[] intents, object[] profiles, bool[] bpc, double[] adaptationStates, uint flags);

/// <summary>
///     Custom intent plugin
/// </summary>
/// <remarks>
///     Each plugin defines a single intent number.<br />
///     Implements the <c>cmsPluginTag</c> struct.</remarks>
public sealed class PluginRenderingIntent : Plugin
{
    public Signature Intent;
    public IntentFn Link;
    public string Description;

    public PluginRenderingIntent(Signature magic, uint expectedVersion, Signature type, Signature intent, IntentFn link, string description)
        : base(magic, expectedVersion, type)
    {
        Intent = intent;
        Link = link;
        Description = description;
    }

    internal static bool RegisterPlugin(Context? context, PluginRenderingIntent? plugin)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     The default ICC intents (perceptual, saturation, rel.col, and abs.col)
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsDefaultICCintents</c> function.</remarks>
    public static Pipeline DefaultIccIntents(Context? context, int[] intents, object[] profiles, bool[] bpc, double[] adaptationStates, uint flags)
    {
        throw new NotImplementedException();
    }
}
public class IntentsList
{
    internal Signature intent;
    internal string description;

    internal IntentsList? next = null;
}