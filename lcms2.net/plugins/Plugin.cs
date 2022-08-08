using lcms2.state;
using lcms2.state.chunks;
using lcms2.types;

namespace lcms2.plugins;

public abstract class Plugin
{
    public Signature Magic;
    public uint ExpectedVersion;
    public Signature Type;
    public Plugin? Next;

    protected internal Plugin(Signature magic, uint expectedVersion, Signature type)
    {
        Magic = magic;
        ExpectedVersion = expectedVersion;
        Type = type;
    }

    public static bool Register(Plugin? plugin) =>
        Register(null, plugin);
    
    public static bool Register(Context? context, Plugin? plug_in)
    {
        for (var plugin = plug_in; plugin is not null; plugin = plugin.Next)
        {
            if (plugin.Magic != Signature.Plugin.MagicNumber)
            {
                Context.SignalError(context, ErrorCode.UnknownExtension, "Unrecognized plugin");
                return false;
            }

            if (plugin.ExpectedVersion > Lcms2.Version)
            {
                Context.SignalError(context, ErrorCode.UnknownExtension,
                    "plugin needs Little CMS {0}, current version is {1}", plugin.ExpectedVersion, Lcms2.Version);
                return false;
            }

            if (plugin.Type == Signature.Plugin.Interpolation)
            {
                return InterpolationPlugin.RegisterPlugin(context, plugin as InterpolationPlugin);
            }
            else if (plugin.Type == Signature.Plugin.TagType)
            {
                return TagTypePlugin.RegisterPlugin(context, plugin as TagTypePlugin);
            }
            else if (plugin.Type == Signature.Plugin.Tag)
            {
                return PluginTag.RegisterPlugin(context, plugin as PluginTag);
            }
            else if (plugin.Type == Signature.Plugin.Formatters)
            {
                return PluginFormatter.RegisterPlugin(context, plugin as PluginFormatter);
            }
            else if (plugin.Type == Signature.Plugin.RenderingIntent)
            {
                return PluginRenderingIntent.RegisterPlugin(context, plugin as PluginRenderingIntent);
            }
            else if (plugin.Type == Signature.Plugin.ParametricCurve)
            {
                return PluginParametricCurves.RegisterPlugin(context, plugin as PluginParametricCurves);
            }
            else if (plugin.Type == Signature.Plugin.MultiProcessElement)
            {
                return PluginMultiProcessElement.RegisterPlugin(context, plugin as PluginMultiProcessElement);
            }
            else if (plugin.Type == Signature.Plugin.Optimization)
            {
                return PluginOptimization.RegisterPlugin(context, plugin as PluginOptimization);
            }
            else if (plugin.Type == Signature.Plugin.Translform)
            {
                return PluginTransform.RegisterPlugin(context, plugin as PluginTransform);
            }
            else if (plugin.Type == Signature.Plugin.Mutex)
            {
                return PluginMutex.RegisterPlugin(context, plugin as PluginMutex);
            }
            else
            {
                Context.SignalError(context, ErrorCode.UnknownExtension, "Unrecognized plugin type {0:X8}", plugin.Type);
                return false;
            }
        }
        // plug_in was null somehow? I would expect this to be false, but it is true in the original...
        return true;
    }

    public static void UnregisterAll() =>
        UnregisterAll(null);

    public static void UnregisterAll(Context? context)
    {
        InterpolationPlugin.RegisterPlugin(context, null);
        TagTypePlugin.RegisterPlugin(context, null);
        PluginTag.RegisterPlugin(context, null);
        PluginFormatter.RegisterPlugin(context, null);
        PluginRenderingIntent.RegisterPlugin(context, null);
        PluginParametricCurves.RegisterPlugin(context, null);
        PluginMultiProcessElement.RegisterPlugin(context, null);
        PluginOptimization.RegisterPlugin(context, null);
        PluginTransform.RegisterPlugin(context, null);
        PluginMutex.RegisterPlugin(context, null);
    }
}
