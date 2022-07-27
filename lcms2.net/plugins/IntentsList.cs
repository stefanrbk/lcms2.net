using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public class IntentsList
{
    internal Signature intent;
    internal string description;

    internal IntentsList? next = null;
}
