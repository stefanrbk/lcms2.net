using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Function to create a mutex
/// </summary>
/// <remarks>Implements the <c>_cmsCreateMutexFnPtrType</c> typedef.</remarks>
public delegate object? CreateMutexFunction(Context? context);

/// <summary>
///     Function to destroy a mutex
/// </summary>
/// <remarks>Implements the <c>_cmsDestroyMutexFnPtrType</c> typedef.</remarks>
public delegate void DestroyMutexFunction(Context? context, ref object mtx);

/// <summary>
///     Function to lock a mutex
/// </summary>
/// <remarks>Implements the <c>_cmsLockMutexFnPtrType</c> typedef.</remarks>
public delegate bool LockMutexFunction(Context? context, ref object mtx);

/// <summary>
///     Function to unlock a mutex
/// </summary>
/// <remarks>Implements the <c>_cmsUnlockMutexFnPtrType</c> typedef.</remarks>
public delegate void UnlockMutexFunction(Context? context, ref object mtx);

/// <summary>
///     Mutex plugin
/// </summary>
/// <remarks>Implements the <c>cmsPluginMutex</c> typedef.</remarks>
public sealed class MutexPlugin: Plugin
{
    public MutexPlugin(Signature magic, uint expectedVersion, Signature type, CreateMutexFunction create, DestroyMutexFunction destroy, LockMutexFunction @lock, UnlockMutexFunction unlock)
           : base(magic, expectedVersion, type)
    {
        CreateMutex = create;
        DestroyMutex = destroy;
        LockMutex = @lock;
        UnlockMutex = unlock;
    }

    /// <summary>
    ///     Function to create a mutex
    /// </summary>
    /// <remarks>Implements the <c>_cmsCreateMutex</c> typedef.</remarks>
    public CreateMutexFunction CreateMutex { get; internal set; }

    /// <summary>
    ///     Function to destroy a mutex
    /// </summary>
    /// <remarks>Implements the <c>_cmsDestroyMutex</c> typedef.</remarks>
    public DestroyMutexFunction DestroyMutex { get; internal set; }

    /// <summary>
    ///     Function to lock a mutex
    /// </summary>
    /// <remarks>Implements the <c>_cmsLockMutex</c> typedef.</remarks>
    public LockMutexFunction LockMutex { get; internal set; }

    /// <summary>
    ///     Function to unlock a mutex
    /// </summary>
    /// <remarks>Implements the <c>_cmsUnlockMutex</c> typedef.</remarks>
    public UnlockMutexFunction UnlockMutex { get; internal set; }

    internal static object? DefaultCreate(Context? context)
    {
        return new Mutex();
    }

    internal static void DefaultDestroy(Context? _context, ref object mtx)
    {
        var mutex = (Mutex)mtx;
        mutex.Dispose();
    }

    internal static bool DefaultLock(Context? _context, ref object mtx)
    {
        var mutex = (Mutex)mtx;
        return mutex.WaitOne();
    }

    internal static void DefaultUnlock(Context? _context, ref object mtx)
    {
        var mutex = (Mutex)mtx;
        mutex.ReleaseMutex();
    }

    internal static bool RegisterPlugin(Context? context, MutexPlugin? plugin)
    {
        var ctx = Context.GetMutexPlugin(context);

        if (plugin is null)
        {
            ctx.create = DefaultCreate;
            ctx.destroy = DefaultDestroy;
            ctx.@lock = DefaultLock;
            ctx.unlock = DefaultUnlock;
            return true;
        }

        ctx.create = plugin.CreateMutex;
        ctx.destroy = plugin.DestroyMutex;
        ctx.@lock = plugin.LockMutex;
        ctx.unlock = plugin.UnlockMutex;

        return true;
    }
}

internal sealed class MutexPluginChunk
{
    internal static MutexPluginChunk global = new();

    internal LockMutexFunction @lock = MutexPlugin.DefaultLock;
    internal CreateMutexFunction create = MutexPlugin.DefaultCreate;

    internal DestroyMutexFunction destroy = MutexPlugin.DefaultDestroy;
    internal UnlockMutexFunction unlock = MutexPlugin.DefaultUnlock;

    private static readonly MutexPluginChunk _mutexPluginChunk = new();

    public MutexPluginChunk()
    { }

    public MutexPluginChunk(CreateMutexFunction create, DestroyMutexFunction destroy, LockMutexFunction @lock, UnlockMutexFunction unlock)
    {
        this.create = create;
        this.destroy = destroy;
        this.@lock = @lock;
        this.unlock = unlock;
    }

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null
            ? src.chunks[(int)Chunks.MutexPlugin]
            : _mutexPluginChunk;

        ctx.chunks[(int)Chunks.MutexPlugin] = from;
    }
}
