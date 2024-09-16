//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright ©️ 1998-2024 Marti Maria Saguer
//              2022-2024 Stefan Kewatt
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

using lcms2.types;

using System.Diagnostics;

namespace lcms2.state;

[DebuggerStepThrough]
public class Context(object? UserData = null) : ICloneable
{
    private object? userData = UserData;
    internal LogErrorChunkType ErrorLogger = new();
    internal AlarmCodesChunkType AlarmCodes = new();
    internal AdaptationStateChunkType AdaptationState = new(DEFAULT_OBSERVER_ADAPTATION_STATE);
    internal InterpPluginChunkType InterpPlugin = new();
    internal CurvesPluginChunkType CurvesPlugin = new();
    internal FormattersPluginChunkType FormattersPlugin = new();
    internal TagTypePluginChunkType TagTypePlugin = new();
    internal TagPluginChunkType TagPlugin = new();
    internal IntentsPluginChunkType IntentsPlugin = new();
    internal TagTypePluginChunkType MPEPlugin = new();
    internal OptimizationPluginChunkType OptimizationPlugin = new();
    internal TransformPluginChunkType TransformPlugin = new();
    internal MutexPluginChunkType MutexPlugin = new();
    internal ParallelizationPluginChunkType ParallelizationPlugin = new(0, 0, null);

    public ref object? UserData => ref userData;

    public static Context Default => new();
    public static readonly Context Shared = new();

    public Context(IEnumerable<PluginBase> plugins, object? UserData = null) : this(UserData)
    {
        foreach (var plugin in plugins)
        {
            cmsPluginTHR(this, plugin);
        }
    }

    public Context Clone(object? NewUserData = null) =>
        new()
        {
            UserData = NewUserData ?? UserData,
            ErrorLogger = ErrorLogger.Clone(),
            AlarmCodes = AlarmCodes.Clone(),
            AdaptationState = AdaptationState.Clone(),
            InterpPlugin = InterpPlugin.Clone(),
            CurvesPlugin = CurvesPlugin.Clone(),
            FormattersPlugin = FormattersPlugin.Clone(),
            TagTypePlugin = TagTypePlugin.Clone(),
            TagPlugin = TagPlugin.Clone(),
            IntentsPlugin = IntentsPlugin.Clone(),
            MPEPlugin = MPEPlugin.Clone(),
            OptimizationPlugin = OptimizationPlugin.Clone(),
            TransformPlugin = TransformPlugin.Clone(),
            MutexPlugin = MutexPlugin.Clone(),
            ParallelizationPlugin = ParallelizationPlugin.Clone(),
        };

    public void RegisterPlugin(PluginBase plugin)
    {
        if (plugin.Magic != cmsPluginMagicNumber)
        {
            cmsSignalError(this, ErrorCodes.UnknownExtension, "Unrecognized plugin");
        }

        if (plugin.ExpectedVersion > LCMS_VERSION)
        {
            cmsSignalError(this, ErrorCodes.UnknownExtension, $"plugin needs Little CMS {plugin.ExpectedVersion}, current version is {LCMS_VERSION}");
        }

        switch ((uint)plugin.Type)
        {
            case cmsPluginInterpolationSig:
                _cmsRegisterInterpPlugin(this, plugin);
                break;

            case cmsPluginTagTypeSig:
                _cmsRegisterTagTypePlugin(this, plugin);
                break;

            case cmsPluginTagSig:
                _cmsRegisterTagPlugin(this, plugin);
                break;

            case cmsPluginFormattersSig:
                _cmsRegisterFormattersPlugin(this, plugin);
                break;

            case cmsPluginRenderingIntentSig:
                _cmsRegisterRenderingIntentPlugin(this, plugin);
                break;

            case cmsPluginParametricCurveSig:
                _cmsRegisterParametricCurvesPlugin(this, plugin);
                break;

            case cmsPluginMultiProcessElementSig:
                _cmsRegisterMultiProcessElementPlugin(this, plugin);
                break;

            case cmsPluginOptimizationSig:
                _cmsRegisterOptimizationPlugin(this, plugin);
                break;

            case cmsPluginTransformSig:
                _cmsRegisterTransformPlugin(this, plugin);
                break;

            case cmsPluginMutexSig:
                _cmsRegisterMutexPlugin(this, plugin);
                break;

            case cmsPluginParalellizationSig:
                _cmsRegisterParallelizationPlugin(this, plugin);
                break;

            default:
                cmsSignalError(this, ErrorCodes.UnknownExtension, $"Unrecognized plugin type '{plugin.Type}'");
                break;
        }
    }

    public void RegisterPlugin(IEnumerable<PluginBase> plugins)
    {
        foreach (var plugin in plugins)
            RegisterPlugin(plugin);
    }

    object ICloneable.Clone() =>
        Clone();

    //private readonly List<object> BufferPools = new();
    //internal MemPluginChunkType DefaultMemoryManager;
    //internal MemPluginChunkType MemPlugin;

    //public ArrayPool<T> GetBufferPool<T>()
    //{
    //    lock (BufferPools)
    //    {
    //        foreach (var pool in BufferPools)
    //        {
    //            if (pool is ArrayPool<T> foundPool)
    //                return foundPool;
    //        }

    //        var newPool = ArrayPool<T>.Create();

    //        BufferPools.Add(newPool);

    //        return newPool;
    //    }
    //}

    //public static ArrayPool<T> GetPool<T>(Context? context) =>
    //    _cmsGetContext(context).GetBufferPool<T>();

    private const double DEFAULT_OBSERVER_ADAPTATION_STATE = 1.0;
}
