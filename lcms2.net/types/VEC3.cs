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

using System.Buffers;
using System.Runtime.InteropServices;

namespace lcms2;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct VEC3(double x, double y, double z)
{
    [FieldOffset(0)]
    public double X = x;

    [FieldOffset(8)]
    public double Y = y;

    [FieldOffset(16)]
    public double Z = z;

    public VEC3(ReadOnlySpan<double> d)
        : this(d[0], d[1], d[2])
    { }

    internal readonly double[] AsArray(ArrayPool<double>? pool = null)
    {
        var result = (pool is not null)
            ? pool.Rent(3)
            : new double[3];

        result[0] = X;
        result[1] = Y;
        result[2] = Z;

        return result;
    }

    public static VEC3 operator *(VEC3 lhs, double rhs) =>
        new(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);

    public static VEC3 operator *(double lhs, VEC3 rhs) =>
        new(rhs.X * lhs, rhs.Y * lhs, rhs.Z * lhs);
}
