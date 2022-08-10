using System.Runtime.CompilerServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.io;

public unsafe struct MD5
{
    private fixed uint buf[4];
    private fixed uint bits[2];
    private fixed byte @in[64];
    private Context? context;

    private delegate uint F_Func(uint x, uint y, uint z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ByteReverse(byte* buf, uint longs)
    {
        if (!BitConverter.IsLittleEndian) {
            do {
                var t = IOHandler.AdjustEndianness(*(uint*)buf);
                *(uint*)buf = t;
                buf += sizeof(uint);
            } while (--longs > 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint F1(uint x, uint y, uint z) =>
        z ^ (x & (y ^ z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint F2(uint x, uint y, uint z) =>
        F1(z, x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint F3(uint x, uint y, uint z) =>
        x ^ y ^ z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint F4(uint x, uint y, uint z) =>
        y ^ (x | ~z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Step(F_Func f, ref uint w, uint x, uint y, uint z, uint data, int s)
    {
        w += f(x, y, z) + data;
        w = (w << s) | (w >> (32 - s));
        w += x;
    }

    private static void Transform(uint* buf, uint* @in)
    {
        var a = buf[0];
        var b = buf[1];
        var c = buf[2];
        var d = buf[3];

        Step(F1, ref a, b, c, d, @in[0] + 0xd76aa478, 7);
        Step(F1, ref d, a, b, c, @in[1] + 0xe8c7b756, 12);
        Step(F1, ref c, d, a, b, @in[2] + 0x242070db, 17);
        Step(F1, ref b, c, d, a, @in[3] + 0xc1bdceee, 22);
        Step(F1, ref a, b, c, d, @in[4] + 0xf57c0faf, 7);
        Step(F1, ref d, a, b, c, @in[5] + 0x4787c62a, 12);
        Step(F1, ref c, d, a, b, @in[6] + 0xa8304613, 17);
        Step(F1, ref b, c, d, a, @in[7] + 0xfd469501, 22);
        Step(F1, ref a, b, c, d, @in[8] + 0x698098d8, 7);
        Step(F1, ref d, a, b, c, @in[9] + 0x8b44f7af, 12);
        Step(F1, ref c, d, a, b, @in[10] + 0xffff5bb1, 17);
        Step(F1, ref b, c, d, a, @in[11] + 0x895cd7be, 22);
        Step(F1, ref a, b, c, d, @in[12] + 0x6b901122, 7);
        Step(F1, ref d, a, b, c, @in[13] + 0xfd987193, 12);
        Step(F1, ref c, d, a, b, @in[14] + 0xa679438e, 17);
        Step(F1, ref b, c, d, a, @in[15] + 0x49b40821, 22);

        Step(F2, ref a, b, c, d, @in[1] + 0xf61e2562, 5);
        Step(F2, ref d, a, b, c, @in[6] + 0xc040b340, 9);
        Step(F2, ref c, d, a, b, @in[11] + 0x265e5a51, 14);
        Step(F2, ref b, c, d, a, @in[0] + 0xe9b6c7aa, 20);
        Step(F2, ref a, b, c, d, @in[5] + 0xd62f105d, 5);
        Step(F2, ref d, a, b, c, @in[10] + 0x02441453, 9);
        Step(F2, ref c, d, a, b, @in[15] + 0xd8a1e681, 14);
        Step(F2, ref b, c, d, a, @in[4] + 0xe7d3fbc8, 20);
        Step(F2, ref a, b, c, d, @in[9] + 0x21e1cde6, 5);
        Step(F2, ref d, a, b, c, @in[14] + 0xc33707d6, 9);
        Step(F2, ref c, d, a, b, @in[3] + 0xf4d50d87, 14);
        Step(F2, ref b, c, d, a, @in[8] + 0x455a14ed, 20);
        Step(F2, ref a, b, c, d, @in[13] + 0xa9e3e905, 5);
        Step(F2, ref d, a, b, c, @in[2] + 0xfcefa3f8, 9);
        Step(F2, ref c, d, a, b, @in[7] + 0x676f02d9, 14);
        Step(F2, ref b, c, d, a, @in[12] + 0x8d2a4c8a, 20);

        Step(F3, ref a, b, c, d, @in[5] + 0xfffa3942, 4);
        Step(F3, ref d, a, b, c, @in[8] + 0x8771f681, 11);
        Step(F3, ref c, d, a, b, @in[11] + 0x6d9d6122, 16);
        Step(F3, ref b, c, d, a, @in[14] + 0xfde5380c, 23);
        Step(F3, ref a, b, c, d, @in[1] + 0xa4beea44, 4);
        Step(F3, ref d, a, b, c, @in[4] + 0x4bdecfa9, 11);
        Step(F3, ref c, d, a, b, @in[7] + 0xf6bb4b60, 16);
        Step(F3, ref b, c, d, a, @in[10] + 0xbebfbc70, 23);
        Step(F3, ref a, b, c, d, @in[13] + 0x289b7ec6, 4);
        Step(F3, ref d, a, b, c, @in[0] + 0xeaa127fa, 11);
        Step(F3, ref c, d, a, b, @in[3] + 0xd4ef3085, 16);
        Step(F3, ref b, c, d, a, @in[6] + 0x04881d05, 23);
        Step(F3, ref a, b, c, d, @in[9] + 0xd9d4d039, 4);
        Step(F3, ref d, a, b, c, @in[12] + 0xe6db99e5, 11);
        Step(F3, ref c, d, a, b, @in[15] + 0x1fa27cf8, 16);
        Step(F3, ref b, c, d, a, @in[2] + 0xc4ac5665, 23);

        Step(F4, ref a, b, c, d, @in[0] + 0xf4292244, 6);
        Step(F4, ref d, a, b, c, @in[7] + 0x432aff97, 10);
        Step(F4, ref c, d, a, b, @in[14] + 0xab9423a7, 15);
        Step(F4, ref b, c, d, a, @in[5] + 0xfc93a039, 21);
        Step(F4, ref a, b, c, d, @in[12] + 0x655b59c3, 6);
        Step(F4, ref d, a, b, c, @in[3] + 0x8f0ccc92, 10);
        Step(F4, ref c, d, a, b, @in[10] + 0xffeff47d, 15);
        Step(F4, ref b, c, d, a, @in[1] + 0x85845dd1, 21);
        Step(F4, ref a, b, c, d, @in[8] + 0x6fa87e4f, 6);
        Step(F4, ref d, a, b, c, @in[15] + 0xfe2ce6e0, 10);
        Step(F4, ref c, d, a, b, @in[6] + 0xa3014314, 15);
        Step(F4, ref b, c, d, a, @in[13] + 0x4e0811a1, 21);
        Step(F4, ref a, b, c, d, @in[4] + 0xf7537e82, 6);
        Step(F4, ref d, a, b, c, @in[11] + 0xbd3af235, 10);
        Step(F4, ref c, d, a, b, @in[2] + 0x2ad7d2bb, 15);
        Step(F4, ref b, c, d, a, @in[9] + 0xeb86d391, 21);

        buf[0] += a;
        buf[1] += b;
        buf[2] += c;
        buf[3] += d;
    }

    /// <summary>
    /// Creates a MD5 object.
    /// </summary>
    /// <remarks>Implements the <c>cmsMD5alloc</c> function.</remarks>
    public static MD5 Create(Context? context)
    {
        var ctx = new MD5
        {
            context = context
        };

        ctx.buf[0] = 0x67452301;
        ctx.buf[1] = 0xefcdab89;
        ctx.buf[2] = 0x98badcfe;
        ctx.buf[3] = 0x10325476;

        ctx.bits[0] = 0;
        ctx.bits[1] = 0;

        return ctx;
    }

    /// <summary>
    /// Adds data to the computation.
    /// </summary>
    /// <remarks>Implements the <c>cmsMD5add</c> function.</remarks>
    public void Add(byte* buf, uint len)
    {
        ref var ctx = ref this;

        var t = ctx.bits[0];
        if ((ctx.bits[0] = t + (len << 3)) < t)
            ctx.bits[1]++;

        ctx.bits[1] += len >> 29;

        t = (t >> 3) & 0x3f;

        if (t != 0) {
            fixed (byte* ptr = ctx.@in) {
                var p = ptr + t;

                t = 64 - t;
                if (len < t) {
                    Buffer.MemoryCopy(buf, p, len, len);
                    return;
                }

                Buffer.MemoryCopy(buf, p, t, t);
                ByteReverse(ptr, 16);

                fixed (uint* ctxbuf = ctx.buf) {
                    Transform(ctxbuf, (uint*)ptr);
                }
                buf += t;
                len -= t;
            }
        }

        fixed (byte* ctxin = ctx.@in) {
            fixed (uint* ctxbuf = ctx.buf) {
                while (len >= 64) {
                    Buffer.MemoryCopy(buf, ctxin, 64, 64);
                    ByteReverse(ctxin, 16);
                    Transform(ctxbuf, (uint*)ctxin);
                    buf += 64;
                    len -= 64;
                }

                Buffer.MemoryCopy(buf, ctxin, len, len);
            }
        }
    }

    /// <summary>
    /// Return the MD5 checksum.
    /// </summary>
    /// <remarks>Implements the <c>cmsMD5finish</c> function.</remarks>
    public ProfileID Finish()
    {
        ref var ctx = ref this;

        fixed (uint* ctxbits = ctx.bits, ctxbuf = ctx.buf) {
            fixed (byte* ctxin = ctx.@in) {
                var count = (ctxbits[0] >> 3) & 0x3f;

                var p = ctxin + count;
                *p++ = 0x80;

                count = 64 - 1 - count;

                if (count < 8) {
                    for (var i = p; i < p + count; i++)
                        *i = 0;
                    ByteReverse(ctxin, 16);
                    Transform(ctxbuf, (uint*)ctxin);

                    for (var i = ctxin; i < ctxin + 56; i++)
                        *i = 0;
                } else {
                    for (var i = p; i < p + (count - 8); i++)
                        *i = 0;
                }
                ByteReverse(ctxin, 14);

                ((uint*)ctxin)[14] = ctxbits[0];
                ((uint*)ctxin)[15] = ctxbits[1];

                Transform(ctxbuf, (uint*)ctxin);

                ByteReverse((byte*)ctxbuf, 4);

                var result = new ProfileID();
                Buffer.MemoryCopy(ctxbuf, result.ID8, 16, 16);

                return result;
            }
        }
    }
}