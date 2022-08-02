using lcms2.plugins;

namespace lcms2.state.chunks;

public class MutexPlugin
{
    internal CreateMutexFunction create = DefaultMutexCreate;
    internal DestroyMutexFunction destroy = DefaultMutexDestroy;
    internal LockMutexFunction @lock = DefaultMutexLock;
    internal UnlockMutexFunction unlock = DefaultMutexUnlock;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null
            ? src.chunks[(int)Chunks.MutexPlugin]
            : mutexPluginChunk;

        ctx.chunks[(int)Chunks.MutexPlugin] = from;
    }

    internal static MutexPlugin global = new();
    private static readonly MutexPlugin mutexPluginChunk = new();

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
