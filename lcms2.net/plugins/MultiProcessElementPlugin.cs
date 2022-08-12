using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Pipelines, Multi Process Elements
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginMultiProcessElement</c> struct.</remarks>
public sealed class MultiProcessElementPlugin : Plugin
{
    public TagTypeHandler Handler;

    public MultiProcessElementPlugin(Signature magic, uint expectedVersion, Signature type, TagTypeHandler handler)
        : base(magic, expectedVersion, type)
    {
        Handler = handler;
    }

    internal static bool RegisterPlugin(Context? context, MultiProcessElementPlugin? plugin) =>
        TagTypePluginChunk.MPE.RegisterPlugin(context, plugin);
}
