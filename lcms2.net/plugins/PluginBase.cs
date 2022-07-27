using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.types;

namespace lcms2.plugins;

#if PLUGIN
    public
#else
internal
#endif
    abstract class PluginBase
{
    public Signature Magic;
    public uint ExpectedVersion;
    public Signature Type;
    public PluginBase? Next = null;

    protected internal PluginBase(Signature magic, uint expectedVersion, Signature type)
    {
        Magic = magic;
        ExpectedVersion = expectedVersion;
        Type = type;
    }
}
