using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
/// The plugin representing an interpolation
/// </summary>
/// <remarks>Implements the <c>cmsPluginInterpolation</c> struct.</remarks>
public sealed class InterpolationPlugin
    : Plugin
{
    public InterpFnFactory? InterpolatorsFactory;

    public InterpolationPlugin(Signature magic, uint expectedVersion, Signature type, InterpFnFactory? interpolatorsFactory)
        : base(magic, expectedVersion, type) =>
        InterpolatorsFactory = interpolatorsFactory;

    internal static bool RegisterPlugin(Context? context, InterpolationPlugin? plugin)
    {
        var ptr = (InterpolationPluginChunk)Context.GetClientChunk(context, Chunks.InterpPlugin)!;

        if (plugin is null) {
            ptr.interpolators = null;
            return true;
        }

        // Set replacement functions
        ptr.interpolators = plugin.InterpolatorsFactory;
        return true;
    }
}

internal sealed class InterpolationPluginChunk
{
    internal InterpFnFactory? interpolators;

    internal static void Alloc(ref Context ctx, in Context? src) =>
        ctx.chunks[(int)Chunks.InterpPlugin] =
            (InterpolationPluginChunk?)src?.chunks[(int)Chunks.InterpPlugin] ?? new InterpolationPluginChunk();

    private InterpolationPluginChunk()
    { }

    internal static InterpolationPluginChunk global = new() { interpolators = null };
}