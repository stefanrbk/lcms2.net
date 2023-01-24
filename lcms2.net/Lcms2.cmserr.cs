//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
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
using lcms2.state;
using lcms2.types;

namespace lcms2;

public static partial class Lcms2
{
    private const int MaxErrorMessageLen = 1024;

    public static int cmsGetEncodedCMMversion() =>
        Version;

    public static int cmsstrcasecmp(string? s1, string? s2) =>
        String.Compare(s1, s2, ignoreCase: true);

    public static long cmsfilelength(FileStream f)
    {
        try
        {
            var p = f.Position; // register current file position
            var n = f.Seek(0, SeekOrigin.End);
            f.Seek(p, SeekOrigin.Begin);

            return n;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    ///     The default error logger does nothing.
    /// </summary>
    private static readonly LogErrorHandler defaultLogErrorHandler =
#if DEBUG
        (_, e, t) => Console.Error.WriteLine($"[lcms ErrorCode.{Enum.GetName(e)}]: {t}");

#else
        (_, __, ___) => { };
#endif

    private static LogErrorChunkType defaultLogErrorChunk =>
        new()
        {
            LogErrorHandler = defaultLogErrorHandler,
        };

    /// <summary>
    ///     Global context storage
    /// </summary>
    private static readonly LogErrorChunkType globalLogErrorChunk = defaultLogErrorChunk;

    /// <summary>
    ///     "Allocates" and inits error logger container for a given context.
    /// </summary>
    /// <remarks>
    ///     If src is null, only initiallizes to the default.
    ///     Otherwise, it duplicates the value from the other context.
    /// </remarks>
    internal static void _cmsAllocLogErrorChunk(Context ctx, Context? src = null)
    {
        ctx.chunks[(int)Chunks.Logger] = new LogErrorChunkType()
        {
            LogErrorHandler = ((LogErrorChunkType?)src?.chunks[(int)Chunks.Logger])?.LogErrorHandler ?? defaultLogErrorHandler,
        };
    }

    /// <summary>
    ///     Change error logger, context based
    /// </summary>
    public static void cmsSetLogErrorHandlerTHR(Context? context, LogErrorHandler? fn)
    {
        ref var lhg = ref _cmsContextGetClientChunk(context, Chunks.Logger);

        lhg = fn ?? defaultLogErrorHandler;
    }

    /// <summary>
    ///     Change error logger, legacy
    /// </summary>
    public static void cmsSetLogErrorHandler(LogErrorHandler? fn) =>
        cmsSetLogErrorHandlerTHR(null, fn);

    /// <summary>
    ///     Log an error
    /// </summary>
    /// <param name="text">English description of the error in String.Format format</param>
    public static void cmsSignalError(Context? context, ErrorCode errorCode, string text, params object?[] args)
    {
        // Check for the context, if specified go there. If not, go for the global
        var lhg = (LogErrorHandler?)_cmsContextGetClientChunk(context, Chunks.Logger);
        if (lhg is not null)
            lhg(context, errorCode, String.Format(text, args));
    }

    /// <summary>
    ///     Utility function to print signatures
    /// </summary>
    internal static void _cmsTagSignature2String(out string str, Signature sig)
    {
        var buf = new char[4];
        var be = BitConverter.GetBytes(_cmsAdjustEndianess32((uint)sig));

        for (var i = 0; i < 4; i++)
            buf[i] = (char)be[i];

        str = new(buf);
    }

    private static IMutex defMtxCreate(Context? id) =>
        DefaultMutex.Create(id);

    private static void defMtxDestroy(Context? id, IMutex mtx) =>
        mtx.Destroy(id);

    private static bool defMtxLock(Context? id, IMutex mtx) =>
        mtx.Lock(id);

    private static void defMtxUnlock(Context? id, IMutex mtx) =>
        mtx.Unlock(id);

    private static readonly MutexPluginChunkType globalMutexPluginChunk = defaultMutexChunk;

    private static MutexPluginChunkType defaultMutexChunk => new(defMtxCreate, defMtxDestroy, defMtxLock, defMtxUnlock);

    /// <summary>
    ///     "Allocates" and inits mutex container.
    /// </summary>
    /// <remarks>
    ///     If src is null, only initiallizes to the default.
    ///     Otherwise, it duplicates the value from the other context.
    /// </remarks>
    internal static void _cmsAllocMutexPluginChunk(Context ctx, Context? src = null)
    {
        ctx.chunks[(int)Chunks.MutexPlugin] =
            src?.chunks[(int)Chunks.MutexPlugin] is MutexPluginChunkType chunk
            ? new MutexPluginChunkType(
                chunk.CreateFn,
                chunk.DestroyFn,
                chunk.LockFn,
                chunk.UnlockFn)
            : defaultMutexChunk;
    }

    internal static bool _cmsRegisterMutexPlugin(Context? context, PluginBase? data)
    {
        var ctx = (MutexPluginChunkType)_cmsContextGetClientChunk(context, Chunks.MutexPlugin)!;

        if (data is null)
        {
            // Mo lock routines
            ctx.CreateFn = c => NullMutex.Create(c);
            ctx.DestroyFn = defMtxDestroy;
            ctx.LockFn = defMtxLock;
            ctx.UnlockFn = defMtxUnlock;

            return true;
        }

        if (data is not PluginMutex plugin)
            return false;

        ctx.CreateFn = plugin.CreateMutex;
        ctx.DestroyFn = plugin.DestroyMutex;
        ctx.LockFn = plugin.LockMutex;
        ctx.UnlockFn = plugin.UnlockMutex;

        return true;
    }

    internal static IMutex _cmsCreateMutex(Context? context)
    {
        var ptr = (MutexPluginChunkType)_cmsContextGetClientChunk(context, Chunks.MutexPlugin)!;

        return ptr.CreateFn(context);
    }

    internal static void _cmsDestroyMutex(Context? context, IMutex mutex)
    {
        var ptr = (MutexPluginChunkType)_cmsContextGetClientChunk(context, Chunks.MutexPlugin)!;

        ptr.DestroyFn(context, mutex);
    }

    internal static void _cmsLockMutex(Context? context, IMutex mutex)
    {
        var ptr = (MutexPluginChunkType)_cmsContextGetClientChunk(context, Chunks.MutexPlugin)!;

        ptr.LockFn(context, mutex);
    }

    internal static void _cmsUnlockMutex(Context? context, IMutex mutex)
    {
        var ptr = (MutexPluginChunkType)_cmsContextGetClientChunk(context, Chunks.MutexPlugin)!;

        ptr.UnlockFn(context, mutex);
    }
}

public delegate void LogErrorHandler(Context? context, ErrorCode errorCode, string text);
