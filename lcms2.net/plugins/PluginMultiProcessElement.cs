using lcms2.types;

namespace lcms2.plugins;

public sealed class PluginMultiProcessElement : Plugin
{
    public ITagTypeHandler Handler;

    public PluginMultiProcessElement(Signature magic, uint expectedVersion, Signature type, ITagTypeHandler handler)
        : base(magic, expectedVersion, type)
    {
        Handler = handler;
    }
}
