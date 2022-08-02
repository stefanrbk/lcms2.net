using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public delegate Pipeline IntentFn(Context? context, int numProfiles, int[] intents, object[] profiles, bool[] bpc, double[] adaptationStates, uint flags);

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
}
public class IntentsList
{
    internal Signature intent;
    internal string description;

    internal IntentsList? next = null;
}