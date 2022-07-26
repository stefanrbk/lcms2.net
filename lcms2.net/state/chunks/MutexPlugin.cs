using System.Diagnostics;

using lcms2.plugins;

namespace lcms2.state.chunks;

internal class MutexPlugin
{
    private CreateMutexFunction create = DefaultMutexCreate;
    private DestroyMutexFunction destroy = DefaultMutexDestroy;
    private LockMutexFunction @lock = DefaultMutexLock;
    private UnlockMutexFunction unlock = DefaultMutexUnlock;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null
            ? src.chunks[(int)Chunks.MutexPlugin]
            : mutexPluginChunk;

        ctx.chunks[(int)Chunks.MutexPlugin] = from;
    }

    internal static MutexPlugin global = new();
    private readonly static MutexPlugin mutexPluginChunk = new();

    private static object? DefaultMutexCreate(ref Context context)
    {
        return new Mutex();
    }
    private static void DefaultMutexDestroy(ref Context _context, ref object mtx)
    {
        var mutex = (Mutex)mtx;
        mutex.Dispose();
    }
    private static bool DefaultMutexLock(ref Context _context, ref object mtx)
    {
        var mutex = (Mutex)mtx;
        return mutex.WaitOne();
    }
    private static void DefaultMutexUnlock(ref Context _context, ref object mtx)
    {
        var mutex = (Mutex)mtx;
        mutex.ReleaseMutex();
    }
}
