using lcms2.types;

namespace lcms2.plugins;

public abstract class Plugin
{
    public Signature Magic;
    public uint ExpectedVersion;
    public Signature Type;
    public Plugin? Next = null;

    protected internal Plugin(Signature magic, uint expectedVersion, Signature type)
    {
        Magic = magic;
        ExpectedVersion = expectedVersion;
        Type = type;
    }
}
