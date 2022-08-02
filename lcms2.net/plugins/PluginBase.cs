using lcms2.types;

namespace lcms2.plugins;

#if PLUGIN
public abstract class PluginBase
#else
internal abstract class PluginBase
#endif

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
