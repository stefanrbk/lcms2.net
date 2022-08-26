using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     The plugin representing an interpolation
/// </summary>
/// <remarks>Implements the <c>cmsPluginInterpolation</c> struct.</remarks>
public sealed class InterpolationPlugin
    : Plugin
{
    public InterpFnFactory? InterpolatorsFactory;

    public InterpolationPlugin(Signature magic, uint expectedVersion, Signature type, InterpFnFactory? interpolatorsFactory)
        : base(magic, expectedVersion, type) =>
        InterpolatorsFactory = interpolatorsFactory;

    internal static bool RegisterPlugin(object? state, InterpolationPlugin? plugin)
    {
        var ptr = State.GetInterpolationPlugin(state);

        if (plugin is null)
        {
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
    internal static InterpolationPluginChunk global = new() { interpolators = null };
    internal InterpFnFactory? interpolators;
    internal static InterpolationPluginChunk Default => new() { interpolators = null };

    private InterpolationPluginChunk()
    { }
}
