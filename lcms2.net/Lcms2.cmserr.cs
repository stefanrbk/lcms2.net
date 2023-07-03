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
using lcms2.io;
using lcms2.state;
using lcms2.types;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private const int MaxErrorMessageLen = 1024;
    private const uint MaxMemoryForAlloc = 1024u * 1024u * 512u;

    internal static MemPluginChunkType* globalMemPluginChunk;

    public static int cmsGetEncodedCMMversion() =>
        LCMS_VERSION;

    public static int cmsstrcasecmp(in byte* s1, in byte* s2)
    {
        var ss1 = s1;
        var ss2 = s2;

        var us1 = (char)*s1;
        var us2 = (char)*s2;

        while (Char.ToUpper(us1) == Char.ToUpper(us2))
        {
            us1 = (char)*++ss1;
            us2 = (char)*++ss2;

            if (us1 is '\0')
                return 0;
        }

        return Char.ToUpper((char)*--ss1) - Char.ToUpper((char)*--ss2);
    }

    public static long cmsfilelength(FILE* f)
    {
        try
        {
            var p = ftell(f); // register current file position
            if (p is -1) return -1;

            if (fseek(f, 0, SEEK_END) is not 0)
                return -1;

            var n = ftell(f);
            fseek(f, p, SEEK_SET);  // file position restored

            return n;
        }
        catch
        {
            return -1;
        }
    }

    internal static void* _cmsMallocDefaultFn(Context _, uint size)
    {
        if (size > MaxMemoryForAlloc) return null;

        try
        {
            return alloc(size);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static void* _cmsMallocZeroDefaultFn(Context ContextID, uint size)
    {
        var pt = _cmsMalloc(ContextID, size);
        if (pt is null) return null;

        NativeMemory.Fill(pt, size, 0);

        return pt;
    }

    internal static void _cmsFreeDefaultFn(Context _, void* Ptr)
    {
        if (Ptr is not null) free(Ptr);
    }

    internal static void* _cmsReallocDefaultFn(Context _, void* Ptr, uint size)
    {
        if (size > MaxMemoryForAlloc) return null;

        try
        {
            return NativeMemory.Realloc(Ptr, size);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static void* _cmsCallocDefaultFn(Context ContextID, uint num, uint size)
    {
        var Total = num * size;

        // Preserve calloc behavior
        if (Total == 0) return null;

        // Safe check for overflow.
        if (num >= UInt32.MaxValue / size) return null;

        // Check for overflow
        if (Total < num || Total < size) return null;

        if (Total > MaxMemoryForAlloc) return null;

        return _cmsMallocZero(ContextID, Total);
    }

    internal static void* _cmsDupDefaultFn(Context ContextID, in void* Org, uint size)
    {
        if (size > MaxMemoryForAlloc) return null;

        var mem = _cmsMalloc(ContextID, size);
        if (mem is not null && Org is not null)
            NativeMemory.Copy(Org, mem, size);

        return mem;
    }

    internal static void _cmsAllocMemPluginChunk(Context ctx, in Context src)
    {
        _cmsAssert(ctx);

        if (src is not null)
        {
            // Duplicate
            ctx->chunks[Chunks.MemPlugin] = _cmsSubAllocDup<MemPluginChunkType>(ctx->MemPool, src->chunks[Chunks.MemPlugin]);
        }
        else
        {
            // To reset it, we use the default allocators, which cannot be overridden
            ctx->chunks[Chunks.MemPlugin] = &ctx->DefaultMemoryManager;
        }
    }

    internal static void _cmsInstallAllocFunctions(PluginMemHandler* Plugin, MemPluginChunkType* ptr)
    {
        if (Plugin is null)
        {
            memcpy(ptr, globalMemPluginChunk);
        }
        else
        {
            ptr->MallocPtr = Plugin->MallocPtr;
            ptr->FreePtr = Plugin->FreePtr;
            ptr->ReallocPtr = Plugin->ReallocPtr;

            // Make sure we revert to defaults
            ptr->MallocZeroPtr = _cmsMallocZeroDefaultFn;
            ptr->CallocPtr = _cmsCallocDefaultFn;
            ptr->DupPtr = _cmsDupDefaultFn;

            if (Plugin->MallocZeroPtr is not null) ptr->MallocZeroPtr = Plugin->MallocZeroPtr;
            if (Plugin->CallocPtr is not null) ptr->CallocPtr = Plugin->CallocPtr;
            if (Plugin->DupPtr is not null) ptr->DupPtr = Plugin->DupPtr;
        }
    }

    internal static bool _cmsRegisterMemHandlerPlugin(Context ContextID, PluginBase* Data)
    {
        var Plugin = (PluginMemHandler*)Data;

        // NULL forces to reset to defaults. In this special case, the defaults are stored in the context structure.
        // Remaining plug-ins does NOT have any copy in the context structure, but this is somehow special as the
        // context internal data should be malloce'd by using those functions.
        if (Data is null)
        {
            var ctx = *&ContextID;

            // Return to the default allocators
            if (ctx is not null)
                ctx->chunks[Chunks.MemPlugin] = &ctx->DefaultMemoryManager;
            return true;
        }

        // Check for required callbacks
        if (Plugin->MallocPtr is null ||
            Plugin->FreePtr is null ||
            Plugin->ReallocPtr is null) return false;

        // Set replacement functions
        var ptr = _cmsContextGetClientChunk<MemPluginChunkType>(ContextID, Chunks.MemPlugin);
        if (ptr == null) return false;

        _cmsInstallAllocFunctions(Plugin, ptr);
        return true;
    }
    [DebuggerStepThrough]
    internal static void* _cmsMalloc(Context ContextID, uint size)
    {
        var ptr = _cmsContextGetClientChunk<MemPluginChunkType>(ContextID, Chunks.MemPlugin);
        return ptr->MallocPtr(ContextID, size);
    }
    [DebuggerStepThrough]
    internal static T* _cmsMalloc<T>(Context ContextID, uint size) where T : struct =>
        (T*)_cmsMalloc(ContextID, size);
    [DebuggerStepThrough]
    internal static T* _cmsMalloc<T>(Context ContextID) where T : struct =>
        (T*)_cmsMalloc(ContextID, _sizeof<T>());
    [DebuggerStepThrough]
    internal static T** _cmsMalloc2<T>(Context ContextID) where T : struct =>
        (T**)_cmsMalloc<nint>(ContextID);
    [DebuggerStepThrough]
    internal static void* _cmsMallocZero(Context ContextID, uint size)
    {
        var ptr = _cmsContextGetClientChunk<MemPluginChunkType>(ContextID, Chunks.MemPlugin);
        return ptr->MallocZeroPtr(ContextID, size);
    }
    [DebuggerStepThrough]
    internal static T* _cmsMallocZero<T>(Context ContextID, uint count) where T : struct =>
        (T*)_cmsMallocZero(ContextID, count * _sizeof<T>());
    [DebuggerStepThrough]
    internal static T* _cmsMallocZero<T>(Context ContextID) where T : struct =>
        (T*)_cmsMallocZero(ContextID, _sizeof<T>());
    [DebuggerStepThrough]
    internal static T** _cmsMallocZero2<T>(Context ContextID) where T : struct =>
        (T**)_cmsMallocZero<nint>(ContextID);
    [DebuggerStepThrough]
    internal static void* _cmsCalloc(Context ContextID, uint num, uint size)
    {
        var ptr = _cmsContextGetClientChunk<MemPluginChunkType>(ContextID, Chunks.MemPlugin);
        return ptr->CallocPtr(ContextID, num, size);
    }
    [DebuggerStepThrough]
    internal static T* _cmsCalloc<T>(Context ContextID, uint num, uint size) where T : struct =>
        (T*)_cmsCalloc(ContextID, num, size);
    [DebuggerStepThrough]
    internal static T* _cmsCalloc<T>(Context ContextID, uint num) where T : struct =>
        (T*)_cmsCalloc(ContextID, num, _sizeof<T>());
    [DebuggerStepThrough]
    internal static T** _cmsCalloc2<T>(Context ContextID, uint num) =>
        (T**)_cmsCalloc<nint>(ContextID, num);
    [DebuggerStepThrough]
    internal static void* _cmsRealloc(Context ContextID, void* Ptr, uint size)
    {
        var ptr = _cmsContextGetClientChunk<MemPluginChunkType>(ContextID, Chunks.MemPlugin);
        return ptr->ReallocPtr(ContextID, Ptr, size);
    }
    [DebuggerStepThrough]
    internal static T* _cmsRealloc<T>(Context ContextID, void* Ptr, uint size) where T : struct =>
        (T*)_cmsRealloc(ContextID, Ptr, size);
    [DebuggerStepThrough]
    internal static void _cmsFree(Context ContextID, void* Ptr)
    {
        if (Ptr is not null)
        {
            var ptr = _cmsContextGetClientChunk<MemPluginChunkType>(ContextID, Chunks.MemPlugin);
            ptr->FreePtr(ContextID, Ptr);
        }
    }
    [DebuggerStepThrough]
    internal static void* _cmsDupMem(Context ContextID, in void* Org, uint size)
    {
        var ptr = _cmsContextGetClientChunk<MemPluginChunkType>(ContextID, Chunks.MemPlugin);
        return ptr->DupPtr(ContextID, Org, size);
    }
    [DebuggerStepThrough]
    internal static T* _cmsDupMem<T>(Context ContextID, in void* Org, uint num) where T : struct =>
        (T*)_cmsDupMem(ContextID, Org, num * _sizeof<T>());
    [DebuggerStepThrough]
    internal static T* _cmsDupMem<T>(Context ContextID, in void* Org) where T : struct =>
        (T*)_cmsDupMem(ContextID, Org, _sizeof<T>());
    [DebuggerStepThrough]
    internal static T** _cmsDupMem2<T>(Context ContextID, in void* Org, uint num) where T : struct =>
        (T**)_cmsDupMem<nint>(ContextID, Org, num * _sizeof<nint>());
    [DebuggerStepThrough]
    internal static T** _cmsDupMem2<T>(Context ContextID, in void* Org) where T : struct =>
        (T**)_cmsDupMem<nint>(ContextID, Org);

    internal static SubAllocator.Chunk* _cmsCreateSubAllocChunk(Context ContextID, uint Initial)
    {
        // 20K by default
        if (Initial is 0)
            Initial = 20 * 1024;

        // Create the container
        var chunk = _cmsMallocZero<SubAllocator.Chunk>(ContextID);
        if (chunk is null) return null;

        // Initialize values
        chunk->Block = (byte*)_cmsMalloc(ContextID, Initial);
        if (chunk->Block is null)
        {
            // Something went wrong
            _cmsFree(ContextID, chunk);
            return null;
        }

        chunk->BlockSize = Initial;
        chunk->Used = 0;
        chunk->next = null;

        return chunk;
    }

    internal static SubAllocator* _cmsCreateSubAlloc(Context ContextID, uint Initial)
    {
        // Create the container
        var sub = _cmsMallocZero<SubAllocator>(ContextID);
        if (sub is null) return null;

        sub->ContextID = ContextID;

        sub->h = _cmsCreateSubAllocChunk(ContextID, Initial);
        if (sub->h is null)
        {
            _cmsFree(ContextID, sub);
            return null;
        }

        return sub;
    }

    internal static void _cmsSubAllocDestroy(SubAllocator* sub)
    {
        SubAllocator.Chunk* n;

        for (var chunk = sub->h; chunk is not null; chunk = n)
        {
            n = chunk->next;
            if (chunk->Block is not null) _cmsFree(sub->ContextID, chunk->Block);
            _cmsFree(sub->ContextID, chunk);
        }

        // Free the header
        _cmsFree(sub->ContextID, sub);
    }

    internal static T* _cmsSubAlloc<T>(SubAllocator* sub) where T : struct =>
        (T*)_cmsSubAlloc(sub, _sizeof<T>());

    internal static T** _cmsSubAlloc2<T>(SubAllocator* sub) =>
        (T**)_cmsSubAlloc<nint>(sub);

    internal static void* _cmsSubAlloc(SubAllocator* sub, int size) =>
        _cmsSubAlloc(sub, (uint)size);

    internal static void* _cmsSubAlloc(SubAllocator* sub, uint size)
    {
        var Free = sub->h->BlockSize - sub->h->Used;

        size = _cmsALIGNMEM(size);

        // Check for memory. If there is no room, allocate a new chunk of double memory size.
        if (size > Free)
        {
            var newSize = sub->h->BlockSize * 2;
            if (newSize < size) newSize = size;

            var chunk = _cmsCreateSubAllocChunk(sub->ContextID, newSize);
            if (chunk is null) return null;

            // Link list
            chunk->next = sub->h;
            sub->h = chunk;
        }

        var ptr = sub->h->Block + sub->h->Used;
        sub->h->Used += size;

        return ptr;
    }

    internal static T* _cmsSubAllocDup<T>(SubAllocator* s, in void* ptr) where T : struct =>
        (T*)_cmsSubAllocDup(s, ptr, _sizeof<T>());

    internal static T** _cmsSubAllocDup2<T>(SubAllocator* s, in void* ptr) where T : struct =>
        (T**)_cmsSubAllocDup<nint>(s, ptr);

    internal static void* _cmsSubAllocDup(SubAllocator* s, in void* ptr, int size) =>
        _cmsSubAllocDup(s, ptr, (uint)size);

    internal static void* _cmsSubAllocDup(SubAllocator* s, in void* ptr, uint size)
    {
        // Dup of null pointer is also NULL
        if (ptr is null)
            return null;

        var NewPtr = _cmsSubAlloc(s, size);

        if (ptr is not null && NewPtr is not null)
        {
            memcpy(NewPtr, ptr, size);
        }

        return NewPtr;
    }

    private static readonly LogErrorChunkType LogErrorChunk = new() { LogErrorHandler = DefaultLogErrorHandlerFunction };

    /// <summary>
    ///     Global context storage
    /// </summary>
    private static readonly LogErrorChunkType* globalLogErrorChunk;

    /// <summary>
    ///     "Allocates" and inits error logger container for a given context.
    /// </summary>
    /// <remarks>
    ///     If src is null, only initiallizes to the default.
    ///     Otherwise, it duplicates the value from the other context.
    /// </remarks>
    internal static void _cmsAllocLogErrorChunk(Context ctx, in Context src)
    {
        fixed (LogErrorChunkType* @default = &LogErrorChunk)
            AllocPluginChunk(ctx, src, Chunks.Logger, @default);
    }

    private static void DefaultLogErrorHandlerFunction(Context _, ErrorCode ErrorCode, string Text)
    {
#if DEBUG
        Console.Error.WriteLine($"[lcms ErrorCode.{Enum.GetName(ErrorCode)}]: {Text}");
#endif
    }

    /// <summary>
    ///     Change error logger, context based
    /// </summary>
    public static void cmsSetLogErrorHandlerTHR(Context context, LogErrorHandlerFunction Fn)
    {
        var lhg = _cmsContextGetClientChunk<LogErrorChunkType>(context, Chunks.Logger);

        if (lhg is not null)
        {
            lhg->LogErrorHandler = Fn is null ? DefaultLogErrorHandlerFunction : Fn;
        }
    }

    /// <summary>
    ///     Change error logger, legacy
    /// </summary>
    public static void cmsSetLogErrorHandler(LogErrorHandlerFunction Fn) =>
        cmsSetLogErrorHandlerTHR(null, Fn);

    /// <summary>
    ///     Log an error
    /// </summary>
    /// <param name="text">English description of the error in String.Format format</param>
    public static void cmsSignalError(Context ContextID, ErrorCode errorCode, string text, params object?[] args)
    {
        // Check for the context, if specified go there. If not, go for the global
        var lhg = (LogErrorChunkType*)_cmsContextGetClientChunk(ContextID, Chunks.Logger);
        text = String.Format(text, args);
        if (text.Length > MaxErrorMessageLen)
            text = text.Remove(MaxErrorMessageLen);
        if (lhg->LogErrorHandler is not null)
            lhg->LogErrorHandler(ContextID, errorCode, text);
    }

    /// <summary>
    ///     Utility function to print signatures
    /// </summary>
    internal static void _cmsTagSignature2String(byte* str, Signature sig)
    {
        // Convert to big endian
        var be = _cmsAdjustEndianess32((uint)sig);

        // Move characters
        memcpy(str, &be, 4);

        // Make sure of terminator
        str[4] = 0;
    }

    private static void* defMtxCreate(Context id)
    {
        var ptr_mutex = _cmsMalloc<MUTEX>(id);
        ptr_mutex->Mutex = new Mutex(false);
        return ptr_mutex;
    }

    private static void defMtxDestroy(Context id, void* mtx)
    {
        ((MUTEX*)mtx)->Mutex.Dispose();
        ((MUTEX*)mtx)->Mutex = null!;
        _cmsFree(id, mtx);
    }

    private static bool defMtxLock(Context _, void* mtx) =>
        ((MUTEX*)mtx)->Mutex.WaitOne();

    private static void defMtxUnlock(Context id, void* mtx) =>
        ((MUTEX*)mtx)->Mutex.ReleaseMutex();

    private static readonly MutexPluginChunkType* globalMutexPluginChunk;

    private static readonly MutexPluginChunkType MutexChunk = new()
    {
        CreateFn = defMtxCreate,
        DestroyFn = defMtxDestroy,
        LockFn = defMtxLock,
        UnlockFn = defMtxUnlock,
    };

    /// <summary>
    ///     "Allocates" and inits mutex container.
    /// </summary>
    /// <remarks>
    ///     If src is null, only initiallizes to the default.
    ///     Otherwise, it duplicates the value from the other context.
    /// </remarks>
    internal static void _cmsAllocMutexPluginChunk(Context ctx, in Context src)
    {
        fixed (MutexPluginChunkType* @default = &MutexChunk)
            AllocPluginChunk(ctx, src, Chunks.MutexPlugin, @default);
    }

    internal static bool _cmsRegisterMutexPlugin(Context context, PluginBase* data)
    {
        var Plugin = (PluginMutex*)data;
        var ctx = _cmsContextGetClientChunk<MutexPluginChunkType>(context, Chunks.MutexPlugin);

        if (data is null)
        {
            // Mo lock routines
            ctx->CreateFn = null;
            ctx->DestroyFn = null;
            ctx->LockFn = null;
            ctx->UnlockFn = null;

            return true;
        }

        // Factory callback is required
        if (Plugin->CreateMutexPtr is null || Plugin->DestroyMutexPtr is null || Plugin->LockMutexPtr is null || Plugin->UnlockMutexPtr is null) return false;

        ctx->CreateFn = Plugin->CreateMutexPtr;
        ctx->DestroyFn = Plugin->DestroyMutexPtr;
        ctx->LockFn = Plugin->LockMutexPtr;
        ctx->UnlockFn = Plugin->UnlockMutexPtr;

        return true;
    }

    internal static void* _cmsCreateMutex(Context context)
    {
        var ptr = _cmsContextGetClientChunk<MutexPluginChunkType>(context, Chunks.MutexPlugin);

        if (ptr->CreateFn is null) return null;

        return ptr->CreateFn(context);
    }

    internal static void _cmsDestroyMutex(Context context, void* mutex)
    {
        var ptr = _cmsContextGetClientChunk<MutexPluginChunkType>(context, Chunks.MutexPlugin);

        if (ptr->DestroyFn is not null)
        {
            ptr->DestroyFn(context, mutex);
        }
    }

    internal static bool _cmsLockMutex(Context context, void* mutex)
    {
        var ptr = _cmsContextGetClientChunk<MutexPluginChunkType>(context, Chunks.MutexPlugin);

        if (ptr->CreateFn is null) return true;

        return ptr->LockFn(context, mutex);
    }

    internal static void _cmsUnlockMutex(Context context, void* mutex)
    {
        var ptr = _cmsContextGetClientChunk<MutexPluginChunkType>(context, Chunks.MutexPlugin);

        if (ptr->UnlockFn is not null)
        {
            ptr->UnlockFn(context, mutex);
        }
    }
}
