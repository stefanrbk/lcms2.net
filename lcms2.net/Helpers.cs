using System.Runtime.CompilerServices;

namespace lcms2;
internal static class Helpers
{
    internal const float MinusInf = -1e22f;
    internal const float PlusInf = 1e22f;

    internal const int MaxInputDimensions = 15;

    internal static uint Uipow(uint n, uint a, uint b)
    {
        var rv = (uint)1;

        if (a == 0) return 0;
        if (n == 0) return 0;

        for (; b > 0; b--) {
            rv *= a;

            // Check for overflow
            if (rv > UInt32.MaxValue / a) return unchecked((uint)-1);
        }

        var rc = rv * n;

        if (rv != rc / n) return unchecked((uint)-1);
        return rc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long AlignLong(long x) =>
        (x + (sizeof(uint) - 1)) & ~(sizeof(uint) - 1);

    internal static Lazy<long> AlignPtr = new(new Func<long>(() => { unsafe { return sizeof(nuint); } }), LazyThreadSafetyMode.ExecutionAndPublication);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long AlignMem(long x) =>
        (x + (AlignPtr.Value - 1)) & ~(AlignPtr.Value - 1);

    internal static ushort From8to16(byte rgb) =>
        (ushort)((rgb << 8) | rgb);

    internal static byte From16to8(ushort rgb) =>
        (byte)((((rgb * (uint)65281) + 8388608) >> 24) & 0xFF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FixedToInt(int x) =>
        x >> 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FixedRestToInt(int x) =>
        x & 0xFFFF;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ToFixedDomain(int a) =>
        a + ((a + 0x7FFF) / 0xFFFF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FromFixedDomain(int a) =>
        a - ((a + 0x7FFF) >> 16);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int RoundFixedToInt(int x) =>
        (x + 0x8000) >> 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int QuickFloor(double val)
    {
#if DONT_USE_FAST_FLOOR
        return (int)Math.Floor(val);
#else
        const double magic = 68719476736.0 * 1.5;
        unsafe {
            val += magic;
            if (BitConverter.IsLittleEndian)
                return *(int*)&val >> 16; // take val, a double, and pretend the first half is an int and shift
            else {
                int* ptr = (int*)&val;
                return *++ptr >> 16;
            }
        }
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort QuickFloorWord(double d) =>
        (ushort)(QuickFloor(d - 32767.0) + 32767);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort QuickSaturateWord(double d) =>
        (d + 0.5) switch
        {
            <= 0 => 0,
            >= 65535.0 => 0xFFFF,
            _ => QuickFloorWord(d),
        };
}
