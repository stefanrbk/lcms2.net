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

    /*  Original Code (cmslut.c line: 732)
     *  
     *  // Quantize a value 0 <= i < MaxSamples to 0..0xffff
     *  cmsUInt16Number CMSEXPORT _cmsQuantizeVal(cmsFloat64Number i, cmsUInt32Number MaxSamples)
     *  {
     *      cmsFloat64Number x;
     *
     *      x = ((cmsFloat64Number) i * 65535.) / (cmsFloat64Number) (MaxSamples - 1);
     *      return _cmsQuickSaturateWord(x);
     *  }
     */
    internal static ushort QuantizeValue(double i, uint maxSamples) =>
        QuickSaturateWord(i * 65535.0 / (maxSamples - 1));

    internal static double Sqr(double v) =>
        v * v;

    internal static double Atan2Deg(double a, double b)
    {
        var h = (a is 0 && b is 0)
            ? 0
            : Math.Atan2(a, b);

        h *= 180 / Math.PI;

        while (h > 360.0)
            h -= 360.0;

        while (h < 0)
            h += 360.0;

        return h;
    }

    internal static double Deg2Rad(double deg) =>
        deg * Math.PI / 180.0;

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

    /*  Original Code (cmslut.c line: 81)
     * 
     *  // Conversion functions. From floating point to 16 bits
     *  static
     *  void FromFloatTo16(const cmsFloat32Number In[], cmsUInt16Number Out[], cmsUInt32Number n)
     *  {
     *      cmsUInt32Number i;
     *
     *      for (i=0; i < n; i++) {
     *          Out[i] = _cmsQuickSaturateWord(In[i] * 65535.0);
     *      }
     *  }
     */
    public static void FromFloatTo16(ReadOnlySpan<float> @in, Span<ushort> @out, int n)
    {
        for (var i = 0; i < n; i++)
            @out[i] = QuickSaturateWord(@in[i] * 65535);
    }

    /*  Original Code (cmslut.c line: 92)
     * 
     *  // From 16 bits to floating point
     *  static
     *  void From16ToFloat(const cmsUInt16Number In[], cmsFloat32Number Out[], cmsUInt32Number n)
     *  {
     *      cmsUInt32Number i;
     *
     *      for (i=0; i < n; i++) {
     *          Out[i] = (cmsFloat32Number) In[i] / 65535.0F;
     *      }
     *  }
     */
    public static void From16ToFloat(ReadOnlySpan<ushort> @in, Span<float> @out, int n)
    {
        for (var i = 0; i < n; i++)
            @out[i] = @in[i] / 65535;
    }

    /*  Original Code (cmslut.c line: 459)
     *  
     *  // Given an hypercube of b dimensions, with Dims[] number of nodes by dimension, calculate the total amount of nodes
     *  static
     *  cmsUInt32Number CubeSize(const cmsUInt32Number Dims[], cmsUInt32Number b)
     *  {
     *      cmsUInt32Number rv, dim;
     *
     *      _cmsAssert(Dims != NULL);
     *
     *      for (rv = 1; b > 0; b--) {
     *
     *          dim = Dims[b-1];
     *          if (dim == 0) return 0;  // Error
     *
     *          rv *= dim;
     *
     *          // Check for overflow
     *          if (rv > UINT_MAX / dim) return 0;
     *      }
     *
     *      return rv;
     *  }
     */
    public static uint CubeSize(ReadOnlySpan<uint> dims, int b)
    {
        uint rv;
        for (rv = 1; b > 0; b--)
        {
            var dim = dims[b-1];
            if (dim is 0) return 0;     // Error

            rv *= dim;

            // Check for overflow
            if (rv > UInt32.MaxValue / dim) return 0;
        }

        return rv;
    }
}
