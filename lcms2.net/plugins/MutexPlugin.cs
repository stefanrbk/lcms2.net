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

/// <summary>
///     Function to create a mutex
/// </summary>
/// <remarks>Implements the <c>_cmsCreateMutexFnPtrType</c> typedef.</remarks>
public delegate object? CreateMutexFunction(object? state);

/// <summary>
///     Function to destroy a mutex
/// </summary>
/// <remarks>Implements the <c>_cmsDestroyMutexFnPtrType</c> typedef.</remarks>
public delegate void DestroyMutexFunction(object? state, ref object? mtx);

/// <summary>
///     Function to lock a mutex
/// </summary>
/// <remarks>Implements the <c>_cmsLockMutexFnPtrType</c> typedef.</remarks>
public delegate bool LockMutexFunction(object? state, ref object? mtx);

/// <summary>
///     Function to unlock a mutex
/// </summary>
/// <remarks>Implements the <c>_cmsUnlockMutexFnPtrType</c> typedef.</remarks>
public delegate void UnlockMutexFunction(object? state, ref object? mtx);

/// <summary>
///     Mutex plugin
/// </summary>
/// <remarks>Implements the <c>cmsPluginMutex</c> typedef.</remarks>
public sealed class MutexPlugin : Plugin
{
    #region Public Constructors

    public MutexPlugin(Signature magic, uint expectedVersion, Signature type, CreateMutexFunction create, DestroyMutexFunction destroy, LockMutexFunction @lock, UnlockMutexFunction unlock)
           : base(magic, expectedVersion, type)
    {
        CreateMutex = create;
        DestroyMutex = destroy;
        LockMutex = @lock;
        UnlockMutex = unlock;
    }

    #endregion Public Constructors

    #region Properties

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

    #endregion Properties

    #region Internal Methods

    internal static object? DefaultCreate(object? context)
    {
        return new Mutex();
    }

    internal static void DefaultDestroy(object? _context, ref object? mtx)
    {
        var mutex = (Mutex?)mtx;
        mutex?.Dispose();
    }

    internal static bool DefaultLock(object? _context, ref object? mtx)
    {
        var mutex = (Mutex?)mtx;
        return mutex?.WaitOne() ?? true;
    }

    internal static void DefaultUnlock(object? _context, ref object? mtx)
    {
        var mutex = (Mutex?)mtx;
        mutex?.ReleaseMutex();
    }

    internal static bool RegisterPlugin(object? context, MutexPlugin? plugin)
    {
        var ctx = State.GetMutexPlugin(context);

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

    #endregion Internal Methods
}

internal sealed class MutexPluginChunk
{
    #region Fields

    internal static MutexPluginChunk global = new();

    internal LockMutexFunction @lock = MutexPlugin.DefaultLock;
    internal CreateMutexFunction create = MutexPlugin.DefaultCreate;

    internal DestroyMutexFunction destroy = MutexPlugin.DefaultDestroy;
    internal UnlockMutexFunction unlock = MutexPlugin.DefaultUnlock;

    #endregion Fields

    #region Public Constructors

    public MutexPluginChunk()
    { }

    public MutexPluginChunk(CreateMutexFunction create, DestroyMutexFunction destroy, LockMutexFunction @lock, UnlockMutexFunction unlock)
    {
        this.create = create;
        this.destroy = destroy;
        this.@lock = @lock;
        this.unlock = unlock;
    }

    #endregion Public Constructors

    #region Properties

    internal static MutexPluginChunk Default => new();

    #endregion Properties
}
