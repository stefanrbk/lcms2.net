//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
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
using System.Runtime.CompilerServices;

namespace lcms2;

internal static class Helpers
{
    #region Fields

    internal const int maxChannels = 16;
    internal const int maxInputDimensions = 15;
    internal const ushort maxNodesInCurve = 4097;
    internal const float minusInf = -1e22f;
    internal const float plusInf = 1e22f;
    internal static Lazy<long> alignPtr = new(new Func<long>(() => { unsafe { return sizeof(nuint); } }), LazyThreadSafetyMode.ExecutionAndPublication);

    #endregion Fields

    #region Internal Methods

    internal static ushort ab2Fix2(double ab) =>
        _cmsQuickSaturateWord((ab + 128.0) * 256.0);

    internal static ushort ab2Fix4(double ab) =>
        _cmsQuickSaturateWord((ab + 128.0) * 257.0);

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

    internal static double ClampabDoubleV2(double ab) =>
                ab switch
                {
                    < minEncodableAb2 => minEncodableAb2,
                    > maxEncodableAb2 => maxEncodableAb2,
                    _ => ab
                };

    internal static double ClampabDoubleV4(double ab) =>
        ab switch
        {
            < minEncodableAb4 => minEncodableAb4,
            > maxEncodableAb4 => maxEncodableAb4,
            _ => ab
        };

    internal static double ClampLDoubleV2(double l)
    {
        const double lMax = 0xFFFF * 100.0 / 0xFF00;

        return l switch
        {
            < 0 => 0,
            > lMax => lMax,
            _ => l
        };
    }

    internal static double ClampLDoubleV4(double l) =>
        l switch
        {
            < 0 => 0,
            > 100.0 => 100.0,
            _ => l
        };

    internal static uint CubeSize(ReadOnlySpan<uint> dims, int b)
    {
        /**  Original Code (cmslut.c line: 459)
         **
         **  // Given an hypercube of b dimensions, with Dims[] number of nodes by dimension, calculate the total amount of nodes
         **  static
         **  cmsUInt32Number CubeSize(const cmsUInt32Number Dims[], cmsUInt32Number b)
         **  {
         **      cmsUInt32Number rv, dim;
         **
         **      _cmsAssert(Dims != NULL);
         **
         **      for (rv = 1; b > 0; b--) {
         **
         **          dim = Dims[b-1];
         **          if (dim == 0) return 0;  // Error
         **
         **          rv *= dim;
         **
         **          // Check for overflow
         **          if (rv > UINT_MAX / dim) return 0;
         **      }
         **
         **      return rv;
         **  }
         **/

        uint rv;
        for (rv = 1; b > 0; b--)
        {
            var dim = dims[b - 1];
            if (dim is 0) return 0;     // Error

            rv *= dim;

            // Check for overflow
            if (rv > UInt32.MaxValue / dim) return 0;
        }

        return rv;
    }

    internal static double Deg2Rad(double deg) =>
        deg * Math.PI / 180.0;

    internal static double F(double t)
    {
        const double limit = 24.0 / 116.0 * (24.0 / 116.0) * (24.0 / 116.0);

        if (t is <= limit)
            return (841.0 / 108.0 * t) + (16.0 / 116.0);

        return Math.Pow(t, 1.0 / 3.0);
    }

    internal static double F1(double t)
    {
        const double limit = 24.0 / 116.0;

        if (t is <= limit)
            return 108.0 / 841.0 * (t - (16.0 / 116.0));

        return t * t * t;
    }

    internal static void From16ToFloat(ReadOnlySpan<ushort> @in, Span<float> @out, int n)
    {
        /**  Original Code (cmslut.c line: 92)
         **
         **  // From 16 bits to floating point
         **  static
         **  void From16ToFloat(const cmsUInt16Number In[], cmsFloat32Number Out[], cmsUInt32Number n)
         **  {
         **      cmsUInt32Number i;
         **
         **      for (i=0; i < n; i++) {
         **          Out[i] = (cmsFloat32Number) In[i] / 65535.0F;
         **      }
         **  }
         **/
        for (var i = 0; i < n; i++)
            @out[i] = @in[i] / 65535f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FromFixedDomain(int a) =>
        a - ((a + 0x7FFF) >> 16);

    internal static void FromFloatTo16(ReadOnlySpan<float> @in, Span<ushort> @out, int n)
    {
        /** Original Code (cmslut.c line: 81)
         **
         ** // Conversion functions. From floating point to 16 bits
         ** static
         ** void FromFloatTo16(const cmsFloat32Number In[], cmsUInt16Number Out[], cmsUInt32Number n)
         ** {
         **     cmsUInt32Number i;
         **
         **     for (i=0; i < n; i++) {
         **         Out[i] = _cmsQuickSaturateWord(In[i] * 65535.0);
         **     }
         ** }
         **/
        for (var i = 0; i < n; i++)
            @out[i] = _cmsQuickSaturateWord(@in[i] * 65535);
    }

    internal static ushort L2Fix2(double l) =>
        _cmsQuickSaturateWord(l * 652.8);

    internal static ushort L2Fix4(double l) =>
        _cmsQuickSaturateWord(l * 655.35);

    internal static ushort QuantizeValue(double i, uint maxSamples) =>
        /**  Original Code (cmslut.c line: 732)
         **
         **  // Quantize a value 0 <= i < MaxSamples to 0..0xffff
         **  cmsUInt16Number CMSEXPORT _cmsQuantizeVal(cmsFloat64Number i, cmsUInt32Number MaxSamples)
         **  {
         **      cmsFloat64Number x;
         **
         **      x = ((cmsFloat64Number) i * 65535.) / (cmsFloat64Number) (MaxSamples - 1);
         **      return _cmsQuickSaturateWord(x);
         **  }
         **/

        _cmsQuickSaturateWord(i * 65535.0 / (maxSamples - 1));

    internal static double Sqr(double v) =>
        v * v;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ToFixedDomain(int a) =>
        a + ((a + 0x7FFF) / 0xFFFF);

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

    internal static ushort XYZ2Fix(double d) =>
        _cmsQuickSaturateWord(d * 32768.0);

    internal static double XYZ2Float(ushort v) =>
        _cms15Fixed16toDouble(v << 1);

    #endregion Internal Methods
}
