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
using lcms2.plugins;
using lcms2.types;

using System.Diagnostics;

namespace lcms2.state;

public static class State
{
    #region Enums

    private enum Chunks
    {
        UserPtr,
        Logger,
        AlarmCodesContext,
        AdaptationStateContext,
        InterpPlugin,
        CurvesPlugin,
        FormattersPlugin,
        TagTypePlugin,
        TagPlugin,
        IntentPlugin,
        MPEPlugin,
        OptimizationPlugin,
        TransformPlugin,
        MutexPlugin,

        Max
    }

    #endregion Enums

    #region Public Methods

    public static object? CreateStateContainer(Plugin? plugin = null, object? userData = null) =>
            Context.Create(plugin, userData);

    public static void DeleteStateContainer(object? state) =>
        Context.Delete(state);

    public static object? DuplicateStateContainer(object? state, object? newUserData = null) =>
        Context.Duplicate(state, newUserData);

    public static ushort[] GetAlarmCodes() =>
        GetAlarmCodes(null);

    public static ushort[] GetAlarmCodes(object? state) =>
        ((AlarmCodes)Context.GetClientChunk(state, Chunks.AlarmCodesContext)!).alarmCodes;

    public static object? GetStateContainerUserData(object? state) =>
                Context.GetClientChunk(state, Chunks.UserPtr);

    public static object? GetUserData(object? state) =>
        Context.GetClientChunk(state, Chunks.UserPtr);

    public static double SetAdaptationState(double d) =>
        SetAdaptationState(null, d);

    public static double SetAdaptationState(object? state, double d)
    {
        var p = (AdaptationState)Context.GetClientChunk(state, Chunks.AdaptationStateContext)!;

        // Get previous value for return
        var prev = p.adaptationState;

        // Set the value if d is positive or zero
        if (d is >= 0)
            p.adaptationState = d;

        // Always return previous value
        return prev;
    }

    public static void SetAlarmCodes(ushort[] codes) =>
        SetAlarmCodes(null, codes);

    public static void SetAlarmCodes(object? state, ushort[] codes)
    {
        if (codes.Length is not 16) Errors.InvalidAlarmCode(state);

        var alarmCodes = (AlarmCodes)Context.GetClientChunk(state, Chunks.AlarmCodesContext)!;
        alarmCodes.alarmCodes = codes;
    }

    public static void SetLogErrorHandler(LogErrorHandlerFunction fn) =>
        SetLogErrorHandler(null, fn);

    public static void SetLogErrorHandler(object? state, LogErrorHandlerFunction? fn) =>
        ((LogErrorHandler)Context.GetClientChunk(state, Chunks.Logger)!).handler = fn ?? LogErrorHandler.DefaultLogErrorHandlerFunction;

    #endregion Public Methods

    #region Internal Methods

    internal static ParametricCurvesPluginChunk GetCurvesPlugin(object? state) =>
            (ParametricCurvesPluginChunk)Context.GetClientChunk(state, Chunks.CurvesPlugin)!;

    internal static FormattersPluginChunk GetFormattersPlugin(object? state) =>
            (FormattersPluginChunk)Context.GetClientChunk(state, Chunks.FormattersPlugin)!;

    internal static InterpolationPluginChunk GetInterpolationPlugin(object? state) =>
            (InterpolationPluginChunk)Context.GetClientChunk(state, Chunks.InterpPlugin)!;

    internal static TagTypePluginChunk GetMultiProcessElementPlugin(object? state) =>
            (TagTypePluginChunk)Context.GetClientChunk(state, Chunks.MPEPlugin)!;

    internal static MutexPluginChunk GetMutexPlugin(object? state) =>
            (MutexPluginChunk)Context.GetClientChunk(state, Chunks.MutexPlugin)!;

    internal static OptimizationPluginChunk GetOptimizationPlugin(object? state) =>
            (OptimizationPluginChunk)Context.GetClientChunk(state, Chunks.OptimizationPlugin)!;

    internal static RenderingIntentsPluginChunk GetRenderingIntentsPlugin(object? state) =>
            (RenderingIntentsPluginChunk)Context.GetClientChunk(state, Chunks.IntentPlugin)!;

    internal static TagPluginChunk GetTagPlugin(object? state) =>
            (TagPluginChunk)Context.GetClientChunk(state, Chunks.TagPlugin)!;

    internal static TagTypePluginChunk GetTagTypePlugin(object? state) =>
            (TagTypePluginChunk)Context.GetClientChunk(state, Chunks.TagTypePlugin)!;

    internal static TransformPluginChunk GetTransformPlugin(object? state) =>
            (TransformPluginChunk)Context.GetClientChunk(state, Chunks.TransformPlugin)!;

    /// <summary>
    ///     Log an error.
    /// </summary>
    /// <param name="errorText">English description of the error.</param>
    /// <remarks>Implements the <c>cmsSignalError</c> function.</remarks>
    internal static void SignalError(object? state, ErrorCode errorCode, string errorText, params object?[] args)
    {
        // Check for the context, if specified go there. If not, go for the global
        LogErrorHandler lhg = (LogErrorHandler)Context.GetClientChunk(state, Chunks.Logger)!;
        if (lhg.handler is not null)
            lhg.handler(state, errorCode, String.Format(errorText, args));
    }

    #endregion Internal Methods

    #region Classes

    private sealed class Context
    {
        #region Fields

        private static readonly Context _globalContext = new()
        {
            /**  Original Code (cmsplugin.c line: 642)
             **  
             **  // The Global storage for system context. This is the one and only global variable
             **  // pointers structure. All global vars are referenced here.
             **  static struct _cmsContext_struct globalContext = {
             **
             **      NULL,                              // Not in the linked list
             **      NULL,                              // No suballocator
             **      {
             **          NULL,                          //  UserPtr,            
             **          &_cmsLogErrorChunk,            //  Logger,
             **          &_cmsAlarmCodesChunk,          //  AlarmCodes,
             **          &_cmsAdaptationStateChunk,     //  AdaptationState, 
             **          &_cmsMemPluginChunk,           //  MemPlugin,
             **          &_cmsInterpPluginChunk,        //  InterpPlugin,
             **          &_cmsCurvesPluginChunk,        //  CurvesPlugin,
             **          &_cmsFormattersPluginChunk,    //  FormattersPlugin,
             **          &_cmsTagTypePluginChunk,       //  TagTypePlugin,
             **          &_cmsTagPluginChunk,           //  TagPlugin,
             **          &_cmsIntentsPluginChunk,       //  IntentPlugin,
             **          &_cmsMPETypePluginChunk,       //  MPEPlugin,
             **          &_cmsOptimizationPluginChunk,  //  OptimizationPlugin,
             **          &_cmsTransformPluginChunk,     //  TransformPlugin,
             **          &_cmsMutexPluginChunk          //  MutexPlugin
             **      },
             **
             **      { NULL, NULL, NULL, NULL, NULL, NULL } // The default memory allocator is not used for context 0
             **  };
             **/

            _chunks = new object?[(int)Chunks.Max]
        {
            null,
            LogErrorHandler.global,
            AlarmCodes.global,
            AdaptationState.global,
            InterpolationPluginChunk.global,
            ParametricCurvesPluginChunk.global,
            FormattersPluginChunk.global,
            TagTypePluginChunk.TagType.global,
            TagPluginChunk.global,
            RenderingIntentsPluginChunk.global,
            TagTypePluginChunk.MPE.global,
            OptimizationPluginChunk.global,
            TransformPluginChunk.global,
            MutexPluginChunk.global,
        }
        };

        private static readonly object _poolHeadMutex = new();
        private static Context? _poolHead = null;
        private object?[] _chunks = new object[(int)Chunks.Max];
        private Context? _next;

        #endregion Fields

        #region Private Constructors

        private Context()
        { }

        #endregion Private Constructors

        #region Internal Methods

        internal static object? Create(Plugin? plugin, object? userData)
        {
            var ctx = new Context();

            lock (_poolHeadMutex)
            {
                ctx._next = _poolHead;
                _poolHead = ctx;
            }

            ctx._chunks[(int)Chunks.UserPtr] = userData;

            AllocLogErrorHandler(ctx);
            AllocAlarmCodes(ctx);
            AllocAdaptationState(ctx);
            AllocInterpolationPluginChunk(ctx);
            AllocParametricCurvesPluginChunk(ctx);
            AllocFormattersPluginChunk(ctx);
            AllocTagTypePluginChunk(ctx);
            AllocMPEPluginChunk(ctx);
            AllocTagPluginChunk(ctx);
            AllocRenderingIntentsPluginChunk(ctx);
            AllocOptimizationPluginChunk(ctx);
            AllocTransformPluginChunk(ctx);
            AllocMutexPluginChunk(ctx);

            // TODO add plugin support
            return !Plugin.Register(ctx, plugin)
                ? null
                : ctx;
        }

        internal static void Delete(object? state)
        {
            var ctx = Get(state);

            Plugin.UnregisterAll(ctx);

            // Maintain list
            lock (_poolHeadMutex)
            {
                if (_poolHead == ctx)
                {
                    _poolHead = ctx._next;
                }
                else
                {
                    // Search for previous
                    for (var prev = _poolHead; prev is not null; prev = prev._next)
                    {
                        if (prev._next == ctx)
                        {
                            prev._next = ctx._next;
                            break;
                        }
                    }
                }
            }
        }

        internal static object? Duplicate(object? state, object? newUserData = null)
        {
            var src = Get(state);

            var userData = newUserData ?? src._chunks[(int)Chunks.UserPtr];

            var ctx = new Context();

            lock (_poolHeadMutex)
            {
                ctx._next = _poolHead;
                _poolHead = ctx;
            }

            ctx._chunks[(int)Chunks.UserPtr] = userData;

            AllocLogErrorHandler(ctx, src);
            AllocAlarmCodes(ctx, src);
            AllocAdaptationState(ctx, src);
            AllocInterpolationPluginChunk(ctx, src);
            AllocParametricCurvesPluginChunk(ctx, src);
            AllocFormattersPluginChunk(ctx, src);
            AllocTagTypePluginChunk(ctx, src);
            AllocTagPluginChunk(ctx, src);
            AllocRenderingIntentsPluginChunk(ctx, src);
            AllocMPEPluginChunk(ctx, src);
            AllocOptimizationPluginChunk(ctx, src);
            AllocTransformPluginChunk(ctx, src);
            AllocMutexPluginChunk(ctx, src);

            // Make sure no one failed
            for (var i = Chunks.Logger; i < Chunks.Max; i++)
            {
                if (ctx._chunks[(int)i] is null)
                {
                    Delete(ctx);
                    return null;
                }
            }

            return ctx;
        }

        internal static object? GetClientChunk(object? state, Chunks chunk)
        {
            // Bounds checking not needed as State handles all calls to this function

            var ctx = Get(state);
            var ptr = ctx._chunks[(int)chunk];

            if (ptr is not null)
                return ptr;

            // A null ptr means no special settings for that context, and this reverts to globals
            return _globalContext._chunks[(int)chunk];
        }

        #endregion Internal Methods

        #region Private Methods

        private static void AllocAdaptationState(Context state, Context? src = null) =>
            state._chunks[(int)Chunks.AdaptationStateContext] =
                (AdaptationState?)src?._chunks[(int)Chunks.AdaptationStateContext] ?? AdaptationState.Default;

        private static void AllocAlarmCodes(Context state, Context? src = null) =>
            state._chunks[(int)Chunks.AlarmCodesContext] =
                (AlarmCodes?)src?._chunks[(int)Chunks.AlarmCodesContext] ?? AlarmCodes.Default;

        private static void AllocFormattersPluginChunk(Context state, Context? src = null)
        {
            if (src is not null)
                DupFormatterFactoryList(ref state, src);
            else
                state._chunks[(int)Chunks.FormattersPlugin] = FormattersPluginChunk.Default;
        }

        private static void AllocInterpolationPluginChunk(Context state, Context? src = null) =>
            /**  Original Code (cmsintrp.c line: 45)
             **  
             **  // The interpolation plug-in memory chunk allocator/dup
             **  void _cmsAllocInterpPluginChunk(struct _cmsContext_struct* ctx, const struct _cmsContext_struct* src)
             **  {
             **      void* from;
             **
             **      _cmsAssert(ctx != NULL);
             **
             **      if (src != NULL) {
             **          from = src ->chunks[InterpPlugin];       
             **      }
             **      else { 
             **          static _cmsInterpPluginChunkType InterpPluginChunk = { NULL };
             **
             **          from = &InterpPluginChunk;
             **      }
             **
             **      _cmsAssert(from != NULL);
             **      ctx ->chunks[InterpPlugin] = _cmsSubAllocDup(ctx ->MemPool, from, sizeof(_cmsInterpPluginChunkType));
             **  }
             **/

            state._chunks[(int)Chunks.InterpPlugin] =
                (InterpolationPluginChunk?)src?._chunks[(int)Chunks.InterpPlugin] ?? InterpolationPluginChunk.Default;

        private static void AllocLogErrorHandler(Context state, Context? src = null) =>
            state._chunks[(int)Chunks.Logger] =
                (LogErrorHandler?)src?._chunks[(int)Chunks.Logger] ?? LogErrorHandler.Default;

        private static void AllocMPEPluginChunk(Context state, Context? src = null)
        {
            if (src is not null)
                DupTagTypeList(state, src, Chunks.MPEPlugin);
            else
                state._chunks[(int)Chunks.MPEPlugin] = TagTypePluginChunk.Default;
        }

        private static void AllocMutexPluginChunk(Context state, Context? src = null) =>
            state._chunks[(int)Chunks.MutexPlugin] =
                (MutexPluginChunk?)src?._chunks[(int)Chunks.MutexPlugin] ?? MutexPluginChunk.Default;

        private static void AllocOptimizationPluginChunk(Context state, Context? src = null)
        {
            if (src is not null)
                DupOptimizationList(ref state, src);
            else
                state._chunks[(int)Chunks.OptimizationPlugin] = OptimizationPluginChunk.Default;
        }

        private static void AllocParametricCurvesPluginChunk(Context state, Context? src = null)
        {
            if (src is not null)
                DupPluginCurvesList(ref state, src);
            else
                state._chunks[(int)Chunks.CurvesPlugin] = ParametricCurvesPluginChunk.Default;
        }

        private static void AllocRenderingIntentsPluginChunk(Context state, Context? src = null)
        {
            if (src is not null)
                DupIntentsList(ref state, src);
            else
                state._chunks[(int)Chunks.IntentPlugin] = RenderingIntentsPluginChunk.Default;
        }

        private static void AllocTagPluginChunk(Context state, Context? src = null)
        {
            if (src is not null)
                DupTagList(ref state, src);
            else
                state._chunks[(int)Chunks.TagPlugin] = TagPluginChunk.Default;
        }

        private static void AllocTagTypePluginChunk(Context state, Context? src = null)
        {
            if (src is not null)
                DupTagTypeList(state, src, Chunks.TagTypePlugin);
            else
                state._chunks[(int)Chunks.TagTypePlugin] = TagTypePluginChunk.Default;
        }

        private static void AllocTransformPluginChunk(Context state, Context? src = null)
        {
            if (src is not null)
                DupTransformList(ref state, src);
            else
                state._chunks[(int)Chunks.TransformPlugin] = TransformPluginChunk.Default;
        }

        private static void DupFormatterFactoryList(ref Context state, in Context src)
        {
            FormattersPluginChunk newHead = FormattersPluginChunk.Default;
            FormattersFactoryList? anterior = null;
            var head = (FormattersPluginChunk?)src._chunks[(int)Chunks.FormattersPlugin];

            Debug.Assert(head is not null);

            // Walk the list copying all nodes
            for (var entry = head.factoryList; entry is not null; entry = entry.next)
            {
                // We want to keep the linked list order, so this is a little bit tricky
                FormattersFactoryList newEntry = new(entry.factory, null);

                if (anterior is not null)
                    anterior.next = newEntry;

                anterior = newEntry;

                if (newHead.factoryList is null)
                    newHead.factoryList = newEntry;
            }

            state._chunks[(int)Chunks.FormattersPlugin] = newHead;
        }

        private static void DupIntentsList(ref Context state, in Context src)
        {
            RenderingIntentsPluginChunk newHead = RenderingIntentsPluginChunk.Default;
            IntentsList? anterior = null;
            var head = (RenderingIntentsPluginChunk?)src._chunks[(int)Chunks.IntentPlugin];

            Debug.Assert(head is not null);

            // Walk the list copying all nodes
            for (var entry = head.intents; entry is not null; entry = entry.next)
            {
                // We want to keep the linked list order, so this is a little bit tricky
                IntentsList newEntry = new(entry.intent, entry.description, entry.link, null);

                if (anterior is not null)
                    anterior.next = newEntry;

                anterior = newEntry;

                if (newHead.intents is null)
                    newHead.intents = newEntry;
            }

            state._chunks[(int)Chunks.IntentPlugin] = newHead;
        }

        private static void DupOptimizationList(ref Context state, in Context src)
        {
            OptimizationPluginChunk newHead = OptimizationPluginChunk.Default;
            OptimizationCollection? anterior = null;
            var head = (OptimizationPluginChunk?)src._chunks[(int)Chunks.OptimizationPlugin];

            Debug.Assert(head is not null);

            // Walk the list copying all nodes
            for (var entry = head.optimizationCollection; entry is not null; entry = entry.next)
            {
                // We want to keep the linked list order, so this is a little bit tricky
                OptimizationCollection newEntry = new(entry);

                if (anterior is not null)
                    anterior.next = newEntry;

                anterior = newEntry;

                if (newHead.optimizationCollection is null)
                    newHead.optimizationCollection = newEntry;
            }

            state._chunks[(int)Chunks.OptimizationPlugin] = newHead;
        }

        private static void DupPluginCurvesList(ref Context state, in Context src)
        {
            ParametricCurvesPluginChunk newHead = ParametricCurvesPluginChunk.Default;
            ParametricCurvesCollection? anterior = null;
            var head = (ParametricCurvesPluginChunk?)src._chunks[(int)Chunks.CurvesPlugin];

            Debug.Assert(head is not null);

            // Walk the list copying all nodes
            for (var entry = head.parametricCurves; entry is not null; entry = entry.next)
            {
                // We want to keep the linked list order, so this is a little bit tricky
                ParametricCurvesCollection newEntry = new(entry);

                if (anterior is not null)
                    anterior.next = newEntry;

                anterior = newEntry;

                if (newHead.parametricCurves is null)
                    newHead.parametricCurves = newEntry;
            }

            state._chunks[(int)Chunks.CurvesPlugin] = newHead;
        }

        private static void DupTagList(ref Context ctx, in Context src)
        {
            TagPluginChunk newHead = TagPluginChunk.Default;
            TagLinkedList? anterior = null;
            var head = (TagPluginChunk?)src._chunks[(int)Chunks.TagPlugin];

            Debug.Assert(head is not null);

            // Walk the list copying all nodes
            for (var entry = head.tags; entry is not null; entry = entry.Next)
            {
                // We want to keep the linked list order, so this is a little bit tricky
                TagLinkedList newEntry = new(entry.Signature, entry.Descriptor, null);

                if (anterior is not null)
                    anterior.Next = newEntry;

                anterior = newEntry;

                if (newHead.tags is null)
                    newHead.tags = newEntry;
            }

            ctx._chunks[(int)Chunks.TagPlugin] = newHead;
        }

        private static void DupTagTypeList(Context state, in Context src, Chunks loc)
        {
            TagTypePluginChunk newHead = TagTypePluginChunk.Default;
            TagTypeLinkedList? anterior = null;
            var head = (TagTypePluginChunk?)src._chunks[(int)loc];

            Debug.Assert(head is not null);

            // Walk the list copying all nodes
            for (var entry = head.tagTypes; entry is not null; entry = entry.Next)
            {
                TagTypeLinkedList newEntry = new(entry.Handler, null);

                if (anterior is not null)
                    anterior.Next = newEntry;

                anterior = newEntry;

                if (newHead.tagTypes is null)
                    newHead.tagTypes = newEntry;
            }

            state._chunks[(int)loc] = newHead;
        }

        private static void DupTransformList(ref Context ctx, in Context src)
        {
            TransformPluginChunk newHead = TransformPluginChunk.Default;
            TransformCollection? anterior = null;
            var head = (TransformPluginChunk?)src._chunks[(int)Chunks.TransformPlugin];

            Debug.Assert(head is not null);

            // Walk the list copying all nodes
            for (var entry = head.transformCollection; entry is not null; entry = entry.next)
            {
                // We want to keep the linked list order, so this is a little bit tricky
                TransformCollection newEntry = new(entry);

                if (anterior is not null)
                    anterior.next = newEntry;

                anterior = newEntry;

                if (newHead.transformCollection is null)
                    newHead.transformCollection = newEntry;
            }

            ctx._chunks[(int)Chunks.TransformPlugin] = newHead;
        }

        // Helps prevent deleted contexts from being used by searching for the context in the list of
        // active contexts and returning the global context if not found.
        private static Context Get(object? state)
        {
            // On null, use global settings
            if (state is not Context context)
                return _globalContext;

            // Search
            lock (_poolHeadMutex)
            {
                for (var ctx = _poolHead; ctx is not null; ctx = ctx._next)
                {
                    // Found it?
                    if (context == ctx)
                    {
                        return ctx;
                    }
                }
            }
            return _globalContext;
        }

        #endregion Private Methods
    }

    #endregion Classes
}
