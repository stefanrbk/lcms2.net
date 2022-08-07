using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Pipelines, Multi Process Elements
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginMultiProcessElement</c> struct.</remarks>
public sealed class PluginMultiProcessElement : Plugin
{
    public ITagTypeHandler Handler;

    public PluginMultiProcessElement(Signature magic, uint expectedVersion, Signature type, ITagTypeHandler handler)
        : base(magic, expectedVersion, type)
    {
        Handler = handler;
    }

    internal static bool RegisterPlugin(Context? context, PluginMultiProcessElement? plugin)
    {
        throw new NotImplementedException();
    }
}
