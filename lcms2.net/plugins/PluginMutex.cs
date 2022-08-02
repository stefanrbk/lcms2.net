using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public delegate object? CreateMutexFunction(ref Context context);
public delegate void DestroyMutexFunction(ref Context context, ref object mtx);
public delegate bool LockMutexFunction(ref Context context, ref object mtx);
public delegate void UnlockMutexFunction(ref Context context, ref object mtx);

public sealed class PluginMutex : Plugin
{
    public CreateMutexFunction CreateMutex;
    public DestroyMutexFunction DestroyMutex;
    public LockMutexFunction LockMutex;
    public UnlockMutexFunction UnlockMutex;

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