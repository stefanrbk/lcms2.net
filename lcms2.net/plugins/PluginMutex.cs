using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Function to create a mutex
/// </summary>
/// <remarks>
///     Implements the <c>_cmsCreateMutexFnPtrType</c> typedef.</remarks>
public delegate object? CreateMutexFunction(Context? context);

/// <summary>
///     Function to destroy a mutex
/// </summary>
/// <remarks>
///     Implements the <c>_cmsDestroyMutexFnPtrType</c> typedef.</remarks>
public delegate void DestroyMutexFunction(Context? context, ref object mtx);

/// <summary>
///     Function to lock a mutex
/// </summary>
/// <remarks>
///     Implements the <c>_cmsLockMutexFnPtrType</c> typedef.</remarks>
public delegate bool LockMutexFunction(Context? context, ref object mtx);

/// <summary>
///     Function to unlock a mutex
/// </summary>
/// <remarks>
///     Implements the <c>_cmsUnlockMutexFnPtrType</c> typedef.</remarks>
public delegate void UnlockMutexFunction(Context? context, ref object mtx);

/// <summary>
///     Mutex plugin
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginMutex</c> typedef.</remarks>
public sealed class PluginMutex : Plugin
{
    /// <summary>
    ///     Function to create a mutex
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsCreateMutex</c> typedef.</remarks>
    public CreateMutexFunction CreateMutex { get; internal set; }

    /// <summary>
    ///     Function to destroy a mutex
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsDestroyMutex</c> typedef.</remarks>
    public DestroyMutexFunction DestroyMutex { get; internal set; }

    /// <summary>
    ///     Function to lock a mutex
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsLockMutex</c> typedef.</remarks>
    public LockMutexFunction LockMutex { get; internal set; }

    /// <summary>
    ///     Function to unlock a mutex
    /// </summary>
    /// <remarks>
    ///     Implements the <c>_cmsUnlockMutex</c> typedef.</remarks>
    public UnlockMutexFunction UnlockMutex { get; internal set; }

    public PluginMutex(Signature magic, uint expectedVersion, Signature type, CreateMutexFunction create, DestroyMutexFunction destroy, LockMutexFunction @lock, UnlockMutexFunction unlock)
        : base(magic, expectedVersion, type)
    {
        CreateMutex = create;
        DestroyMutex = destroy;
        LockMutex = @lock;
        UnlockMutex = unlock;
    }

    internal static bool RegisterPlugin(Context? context, PluginMutex? plugin)
    {
        throw new NotImplementedException();
    }
}