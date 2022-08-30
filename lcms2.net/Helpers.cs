using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace lcms2;

internal static class Helpers
{
    internal const float minusInf = -1e22f;
    internal const float plusInf = 1e22f;

    internal const ushort maxNodesInCurve = 4097;

    internal const int maxChannels = 16;
    internal const int maxInputDimensions = 15;
    internal const int maxStageChannels = 128;

    internal const double maxEncodableXYZ = 1 + (32767.0 /32768.0);
    internal const double minEncodableAb2 = -128.0;
    internal const double maxEncodableAb2 = (65535.0 / 256.0) - 128.0;
    internal const double minEncodableAb4 = -128.0;
    internal const double maxEncodableAb4 = 127.0;


    internal const double determinantTolerance = 0.0001;

    internal static uint Uipow(uint n, uint a, uint b)
    {
        var rv = (uint)1;

        if (a == 0) return 0;
        if (n == 0) return 0;

        for (; b > 0; b--)
        {
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

    internal static Lazy<long> alignPtr = new(new Func<long>(() => { unsafe { return sizeof(nuint); } }), LazyThreadSafetyMode.ExecutionAndPublication);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long AlignMem(long x) =>
        (x + (alignPtr.Value - 1)) & ~(alignPtr.Value - 1);

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
    [ExcludeFromCodeCoverage]
    internal static int QuickFloor(double val)
    {
#if DONT_USE_FAST_FLOOR
        return (int)Math.Floor(val);
#else
        const double magic = 68719476736.0 * 1.5;
        unsafe
        {
            val += magic;
            if (BitConverter.IsLittleEndian)
                return *(int*)&val >> 16; // take val, a double, and pretend the first half is an int and shift
            else
            {
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
        (d += 0.5) switch
        {
            <= 0 => 0,
            >= 65535.0 => 0xFFFF,
            _ => QuickFloorWord(d),
        };

    internal static ushort QuantizeValue(double i, uint maxSamples) =>
        QuickSaturateWord(i * 65535.0 / (maxSamples - 1));

    /// <summary>
    ///     Converts a double-precision floating-point number into a Q15.16 signed fixed-point number.
    /// </summary>
    /// <remarks>Implements the <c>_cmsDoubleTo15Fixed16</c> function.</remarks>
    public static int DoubleToS15Fixed16(double value) =>
        (int)Math.Floor((value * 65536.0) + 0.5);

    /// <summary>
    ///     Converts a double-precision floating-point number into a Q8.8 unsigned fixed-point number.
    /// </summary>
    /// <remarks>Implements the <c>_cmsDoubleTo8Fixed8</c> function.</remarks>
    public static ushort DoubleToU8Fixed8(double value) =>
        (ushort)((DoubleToS15Fixed16(value) >> 8) & 0xffff);

    /// <summary>
    ///     Converts a Q15.16 signed fixed-point number into a double-precision floating-point number.
    /// </summary>
    /// <remarks>Implements the <c>_cms15Fixed16toDouble</c> function.</remarks>
    /// 
    public static double S15Fixed16toDouble(int value)
    {
        var sign = value < 0 ? -1 : 1;
        value = Math.Abs(value);

        var whole = (ushort)((value >> 16) & 0xffff);
        var fracPart = (ushort)(value & 0xffff);

        var mid = fracPart / 65536.0;
        var floater = whole + mid;

        return sign * floater;
    }

    /// <summary>
    ///     Converts a Q8.8 unsigned fixed-point number into a double-precision floating-point number.
    /// </summary>
    /// <remarks>Implements the <c>_cms8Fixed8toDouble</c> function.</remarks>
    public static double U8Fixed8toDouble(ushort value)
    {
        var lsb = (byte)(value & 0xff);
        var msb = (byte)((value >> 8) & 0xff);

        return msb + (lsb / 256.0);
    }
    public static double F(double t)
    {
        const double limit = 24.0 / 116.0 * (24.0 / 116.0) * (24.0 / 116.0);

        if (t is <= limit)
            return (841.0 / 108.0 * t) + (16.0 / 116.0);

        return Math.Pow(t, 1.0 / 3.0);
    }
    public static double F1(double t)
    {
        const double limit = 24.0 / 116.0;

        if (t is <= limit)
            return 108.0 / 841.0 * (t - (16.0 / 116.0));

        return t * t * t;
    }
    public static void FromFloatTo16(in float[] @in, ushort[] @out) =>
        @in.Take(Math.Min(@in.Length, @out.Length))
           .Select(i => QuickSaturateWord(i * 65535.0))
           .ToArray()
           .CopyTo(@out.AsSpan());

    public static void From16ToFloat(in ushort[] @in, float[] @out) =>
        @in.Take(Math.Min(@in.Length, @out.Length))
           .Select(i => i / 65535f)
           .ToArray()
           .CopyTo(@out.AsSpan());
}
