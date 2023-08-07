using Microsoft.Extensions.Logging;

using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace lcms2.state;
internal sealed class AccountableArrayPool<T> : ArrayPool<T>, IAccountableArrayPoolInfo
{
    private enum BufferAllocatedReason
    {
        Pooled,
        OverMaximumSize,
        PoolExhausted
    }

    private enum BufferDroppedReason
    {
        Full,
        OverMaximumSize
    }

    private sealed class Bucket
    {
        private readonly static Dictionary<LogErrorChunkType, ILogger<Bucket>> _loggers = new();

        private ILogger<Bucket>? _logger;

        internal readonly int _bufferLength;

        private readonly T[][] _buffers;

        private readonly int _poolId;

        private SpinLock _lock;

        private int _index;

        private volatile int _totalRentCount;

        private volatile int _maxRentCount;

        private volatile int _totalAllocated;

        private volatile int _maxAllocated;

        internal int RentCount => _totalRentCount;
        internal int MaxRented => _maxRentCount;
        internal int AllocCount => _totalAllocated;
        internal int MaxAlloced => _maxAllocated;

        internal int Id => GetHashCode();

        internal Bucket(int bufferLength, int numberOfBuffers, int poolId)
        {
            _lock = new SpinLock(Debugger.IsAttached);
            _buffers = new T[numberOfBuffers][];
            _bufferLength = bufferLength;
            _poolId = poolId;
        }

        static Bucket()
        {
            _cmsGetContext(null).ErrorLogger.FactoryChanged += ErrorLogger_FactoryChanged;
        }

        internal T[] Rent()
        {
            T[][] buffers = _buffers;
            T[] array = null!;
            bool lockTaken = false;
            bool flag = false;
            _logger ??= GetLogger(null);
            try
            {
                _lock.Enter(ref lockTaken);
                if (_index < buffers.Length)
                {
                    array = buffers[_index];
                    buffers[_index++] = null!;
                    flag = array == null;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit(useMemoryBarrier: false);
                }
            }
            if (flag)
            {
                array = new T[_bufferLength];
                _totalAllocated++;
                _maxAllocated = Math.Max(_maxAllocated, _totalAllocated);
                BufferAllocated(_logger, array.GetHashCode(), _bufferLength, _poolId, Id, BufferAllocatedReason.Pooled);
            }

            _totalRentCount++;
            _maxRentCount = Math.Max(_maxRentCount, _totalRentCount);
            PrintCounts(_logger, _totalRentCount, _maxRentCount, _totalAllocated, _maxAllocated);
            return array;
        }

        internal void Return(T[] array)
        {
            if (array.Length != _bufferLength)
            {
                throw new ArgumentException("The buffer is not associated with this pool and may not be returned to it.", nameof(array));
            }
            bool lockTaken = false;
            bool flag;
            _logger ??= GetLogger(null);
            try
            {
                _lock.Enter(ref lockTaken);
                flag = _index != 0;
                if (flag)
                {
                    _buffers[--_index] = array;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit(useMemoryBarrier: false);
                }
            }
            if (!flag)
            {
                BufferDropped(_logger, array.GetHashCode(), _bufferLength, _poolId, Id, BufferDroppedReason.Full);
                _totalAllocated--;
            }
            _totalRentCount--;
            PrintCounts(_logger, _totalRentCount, _maxRentCount, _totalAllocated, _maxAllocated);
        }

        private static void ErrorLogger_FactoryChanged(object? sender, FactoryChangedEventArgs e)
        {
            if (sender is LogErrorChunkType logChunk)
            {
                if (_loggers.TryGetValue(logChunk, out _))
                {
                    _loggers[logChunk] = logChunk.Factory.CreateLogger<Bucket>();
                }
                //else
                //{
                //    var logger = logChunk.Factory.CreateLogger("Lcms2");
                //    loggers.Add(logChunk, logger);

                //}
            }
        }

        //[DebuggerStepThrough]
        internal static ILogger<Bucket> GetLogger(Context? context)
        {
            context = _cmsGetContext(context);

            if (_loggers.TryGetValue(context.ErrorLogger, out var logger))
            {
                return logger;
            }

            logger = context.ErrorLogger.Factory.CreateLogger<Bucket>();
            _loggers.Add(context.ErrorLogger, logger);
            return logger;
        }
    }

    private readonly Bucket[] _buckets;

    private readonly static Dictionary<LogErrorChunkType, ILogger<AccountableArrayPool<T>>> _loggers = new();

    private ILogger<AccountableArrayPool<T>>? _logger;

    private volatile int _totalRentCount;

    private volatile int _maxRentCount;

    private volatile int _totalAllocated;

    private volatile int _maxAllocated;

    private int Id => GetHashCode();

    static AccountableArrayPool()
    {
        _cmsGetContext(null).ErrorLogger.FactoryChanged += ErrorLogger_FactoryChanged;
    }

    internal AccountableArrayPool()
        : this(1048576, 50)
    {
    }

    internal AccountableArrayPool(int maxArrayLength, int maxArraysPerBucket)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxArrayLength, nameof(maxArrayLength));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxArraysPerBucket, nameof(maxArraysPerBucket));
        if (maxArrayLength > 1073741824)
        {
            maxArrayLength = 1073741824;
        }
        else if (maxArrayLength < 16)
        {
            maxArrayLength = 16;
        }
        int id = Id;
        int num = SelectBucketIndex(maxArrayLength);
        Bucket[] array = new Bucket[num + 1];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new Bucket(GetMaxSizeForBucket(i), maxArraysPerBucket, id);
        }
        _buckets = array;
    }

    public override T[] Rent(int minimumLength)
    {
        T[] array;
        ArgumentOutOfRangeException.ThrowIfNegative(minimumLength, nameof(minimumLength));
        _logger ??= GetLogger(null);
        if (minimumLength == 0)
        {
            array = Array.Empty<T>();
            _totalRentCount++;
            _maxRentCount = Math.Max(_maxRentCount, _totalRentCount);
            BufferRented(_logger, array.GetHashCode(), 0, Id, -1);
            PrintCounts(_logger, _totalRentCount, _maxRentCount, _totalAllocated, _maxAllocated);
            return array;
        }
        int num = SelectBucketIndex(minimumLength);
        if (num < _buckets.Length)
        {
            int num2 = num;
            do
            {
                array = _buckets[num2].Rent();
                if (array != null)
                {
                    _totalRentCount++;
                    _maxRentCount = Math.Max(_maxRentCount, _totalRentCount);
                    BufferRented(_logger, array.GetHashCode(), array.Length, Id, _buckets[num2].Id);
                    PrintCounts(_logger, _totalRentCount, _maxRentCount, _totalAllocated, _maxAllocated);
                    return array;
                }
            }
            while (++num2 < _buckets.Length && num2 != num + 2);
            array = new T[_buckets[num]._bufferLength];
        }
        else
        {
            array = new T[minimumLength];
        }

        int hashCode = array.GetHashCode();
        _totalRentCount++;
        _maxRentCount = Math.Max(_maxRentCount, _totalRentCount);
        _totalAllocated++;
        _maxAllocated = Math.Max(_maxAllocated, _totalAllocated);
        BufferRented(_logger, hashCode, array.Length, Id, -1);
        BufferAllocated(_logger, hashCode, array.Length, Id, -1, (num >= _buckets.Length) ? BufferAllocatedReason.OverMaximumSize : BufferAllocatedReason.PoolExhausted);
        PrintCounts(_logger, _totalRentCount, _maxRentCount, _totalAllocated, _maxAllocated);
        return array;
    }

    public override void Return(T[] array, bool clearArray = false)
    {
        ArgumentNullException.ThrowIfNull(array, nameof(array));
        _logger ??= GetLogger(null);
        if (array.Length == 0)
        {
            BufferReturned(_logger, array.GetHashCode(), array.Length, Id);
            _totalRentCount--;
            PrintCounts(_logger, _totalRentCount, _maxRentCount, _totalAllocated, _maxAllocated);
            return;
        }
        int num = SelectBucketIndex(array.Length);
        bool flag = num < _buckets.Length;
        if (flag)
        {
            if (clearArray || !typeof(T).IsValueType)
            {
                Array.Clear(array);
            }
            _buckets[num].Return(array);
        }
        int hashCode = array.GetHashCode();
        BufferReturned(_logger, hashCode, array.Length, Id);
        _totalRentCount--;
        if (!flag)
        {
            BufferDropped(_logger, hashCode, array.Length, Id, -1, BufferDroppedReason.Full);
            _totalAllocated--;
        }
        PrintCounts(_logger, _totalRentCount, _maxRentCount, _totalAllocated, _maxAllocated);
    }

    public IEnumerable<(int bufferSize, int rentCount, int maxRent, int allocCount, int maxAlloc)> GetTotals
    {
        get {
            foreach (var b in _buckets)
            {
                if (b is null) continue;
                if (b.MaxAlloced is 0) continue;

                yield return (b._bufferLength, b.RentCount, b.MaxRented, b.AllocCount, b.MaxAlloced);
            }
        }
    }

    public Type Type => typeof(T);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int SelectBucketIndex(int bufferSize) =>
        BitOperations.Log2((uint)(bufferSize - 1) | 0xFu) - 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetMaxSizeForBucket(int binIndex) =>
        16 << binIndex;

    private static void BufferAllocated(ILogger logger, int bufferId, int bufferSize, int poolId, int bucketId, BufferAllocatedReason reason)
    {
        logger ??= _cmsGetContext(null).ErrorLogger.Factory.CreateLogger<Bucket>();
        logger.LogDebug("Allocated Buffer\nBuffer ID: 0x{bufferId:X8}, Buffer Size: {bufferSize}, Pool ID: 0x{poolId:X8}, Bucket ID: 0x{bucketId:X8}, Reason: {reason}",
            bufferId, bufferSize, poolId, bucketId, Enum.GetName(reason));
    }

    private static void BufferRented(ILogger logger, int bufferId, int bufferSize, int poolId, int bucketId)
    {
        logger ??= _cmsGetContext(null).ErrorLogger.Factory.CreateLogger<Bucket>();
        logger.LogDebug("Allocated Buffer\nBuffer ID: 0x{bufferId:X8}, Buffer Size: {bufferSize}, Pool ID: 0x{poolId:X8}, Bucket ID: 0x{bucketId:X8}",
            bufferId, bufferSize, poolId, bucketId);
    }

    private static void BufferDropped(ILogger logger, int bufferId, int bufferSize, int poolId, int bucketId, BufferDroppedReason reason)
    {
        logger.LogDebug("Dropped Buffer\nBuffer ID: 0x{bufferId:X8}, Buffer Size: {bufferSize}, Pool ID: 0x{poolId:X8}, Bucket ID: 0x{bucketId:X8}, Reason: {reason}",
            bufferId, bufferSize, poolId, bucketId, Enum.GetName(reason));
    }

    private static void BufferReturned(ILogger logger, int bufferId, int bufferSize, int poolId)
    {
        logger.LogDebug("Dropped Buffer\nBuffer ID: 0x{bufferId:X8}, Buffer Size: {bufferSize}, Pool ID: 0x{poolId:X8}",
            bufferId, bufferSize, poolId);
    }

    private static void PrintCounts(ILogger logger, int totalRent, int maxRent, int totalAlloc, int maxAlloc)
    {
        logger.LogTrace("Counts\nTotal Rented: {totalRent}, Most Rented at Once: {maxRent}\nTotal Alloced: {totalAlloc}, Most Alloced at Once: {maxAlloc}",
            totalRent, maxRent, totalAlloc, maxAlloc);
    }

    private static void ErrorLogger_FactoryChanged(object? sender, FactoryChangedEventArgs e)
    {
        if (sender is LogErrorChunkType logChunk)
        {
            if (loggers.TryGetValue(logChunk, out _))
            {
                loggers[logChunk] = logChunk.Factory.CreateLogger<AccountableArrayPool<T>>();
            }
            //else
            //{
            //    var logger = logChunk.Factory.CreateLogger("Lcms2");
            //    loggers.Add(logChunk, logger);

            //}
        }
    }

    //[DebuggerStepThrough]
    internal static ILogger<AccountableArrayPool<T>> GetLogger(Context? context)
    {
        context = _cmsGetContext(context);

        if (_loggers.TryGetValue(context.ErrorLogger, out var logger))
        {
            return logger;
        }

        logger = context.ErrorLogger.Factory.CreateLogger<AccountableArrayPool<T>>();
        _loggers.Add(context.ErrorLogger, logger);
        return logger;
    }
}

internal interface IAccountableArrayPoolInfo
{
    abstract IEnumerable<(int bufferSize, int rentCount, int maxRent, int allocCount, int maxAlloc)> GetTotals { get; }
    abstract Type Type { get; }
}