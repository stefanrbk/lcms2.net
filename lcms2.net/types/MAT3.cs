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

namespace lcms2.types;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct MAT3(VEC3 x, VEC3 y, VEC3 z)
{
    [FieldOffset(0)]
    public VEC3 X = x;

    [FieldOffset(24)]
    public VEC3 Y = y;

    [FieldOffset(48)]
    public VEC3 Z = z;

    public MAT3(double xx, double xy, double xz, double yx, double yy, double yz, double zx, double zy, double zz)
        : this(
            new(xx, xy, xz),
            new(yx, yy, yz),
            new(zx, zy, zz))
    { }

    public MAT3(ReadOnlySpan<double> d)
        : this(
            new(d[0], d[1], d[2]),
            new(d[3], d[4], d[5]),
            new(d[6], d[7], d[8]))
    { }

    public readonly double[] AsArray(ArrayPool<double>? pool = null)
    {
        var result = (pool is null)
            ? new double[9]
            : pool.Rent(9);

        result[0] = X.X;
        result[1] = X.Y;
        result[2] = X.Z;
        result[3] = Y.X;
        result[4] = Y.Y;
        result[5] = Y.Z;
        result[6] = Z.X;
        result[7] = Z.Y;
        result[8] = Z.Z;

        return result;
    }
}
