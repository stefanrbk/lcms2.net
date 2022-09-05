//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//---------------------------------------------------------------------------------
//
using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public abstract class Plugin
{
    #region Fields

    public uint ExpectedVersion;
    public Signature Magic;
    public Plugin? Next;
    public Signature Type;

    #endregion Fields

    #region Protected Internal Constructors

    protected internal Plugin(Signature magic, uint expectedVersion, Signature type)
    {
        Magic = magic;
        ExpectedVersion = expectedVersion;
        Type = type;
    }

    #endregion Protected Internal Constructors

    #region Public Methods

    public static bool Register(Plugin? plugin = null) =>
           Register(null, plugin);

    public static bool Register(object? state = null, Plugin? plug_in = null)
    {
        for (var plugin = plug_in; plugin is not null; plugin = plugin.Next)
        {
            if (plugin.Magic != Signature.Plugin.MagicNumber)
            {
                State.SignalError(state, ErrorCode.UnknownExtension, "Unrecognized plugin");
                return false;
            }

            if (plugin.ExpectedVersion > Lcms2.Version)
            {
                State.SignalError(state, ErrorCode.UnknownExtension,
                    "plugin needs Little CMS {0}, current version is {1}", plugin.ExpectedVersion, Lcms2.Version);
                return false;
            }

            if (plugin.Type == Signature.Plugin.Interpolation)
            {
                return InterpolationPlugin.RegisterPlugin(state, plugin as InterpolationPlugin);
            }
            else if (plugin.Type == Signature.Plugin.TagType)
            {
                return TagTypePlugin.RegisterPlugin(state, plugin as TagTypePlugin);
            }
            else if (plugin.Type == Signature.Plugin.Tag)
            {
                return TagPlugin.RegisterPlugin(state, plugin as TagPlugin);
            }
            else if (plugin.Type == Signature.Plugin.Formatters)
            {
                return FormattersPlugin.RegisterPlugin(state, plugin as FormattersPlugin);
            }
            else if (plugin.Type == Signature.Plugin.RenderingIntent)
            {
                return RenderingIntentPlugin.RegisterPlugin(state, plugin as RenderingIntentPlugin);
            }
            else if (plugin.Type == Signature.Plugin.ParametricCurve)
            {
                return ParametricCurvesPlugin.RegisterPlugin(state, plugin as ParametricCurvesPlugin);
            }
            else if (plugin.Type == Signature.Plugin.MultiProcessElement)
            {
                return MultiProcessElementPlugin.RegisterPlugin(state, plugin as MultiProcessElementPlugin);
            }
            else if (plugin.Type == Signature.Plugin.Optimization)
            {
                return OptimizationPlugin.RegisterPlugin(state, plugin as OptimizationPlugin);
            }
            else if (plugin.Type == Signature.Plugin.Translform)
            {
                return TransformPlugin.RegisterPlugin(state, plugin as TransformPlugin);
            }
            else if (plugin.Type == Signature.Plugin.Mutex)
            {
                return MutexPlugin.RegisterPlugin(state, plugin as MutexPlugin);
            }
            else
            {
                State.SignalError(state, ErrorCode.UnknownExtension, "Unrecognized plugin type {0:X8}", plugin.Type);
                return false;
            }
        }
        // plug_in was null somehow? I would expect this to be false, but it is true in the original...
        return true;
    }

    public static void UnregisterAll() =>
        UnregisterAll(null);

    public static void UnregisterAll(object? state)
    {
        InterpolationPlugin.RegisterPlugin(state, null);
        TagTypePlugin.RegisterPlugin(state, null);
        TagPlugin.RegisterPlugin(state, null);
        FormattersPlugin.RegisterPlugin(state, null);
        RenderingIntentPlugin.RegisterPlugin(state, null);
        ParametricCurvesPlugin.RegisterPlugin(state, null);
        MultiProcessElementPlugin.RegisterPlugin(state, null);
        OptimizationPlugin.RegisterPlugin(state, null);
        TransformPlugin.RegisterPlugin(state, null);
        MutexPlugin.RegisterPlugin(state, null);
    }

    #endregion Public Methods
}
