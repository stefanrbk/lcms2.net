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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace lcms2.types;

public struct CIEXYZ(double x, double y, double z) : IEquatable<CIEXYZ>
{
    public double X = x;
    public double Y = y;
    public double Z = z;

    public readonly VEC3 AsVec =>
        new(X, Y, Z);

    public static CIEXYZ NaN =>
        VEC3.NaN.AsXYZ;

    public readonly bool IsNaN =>
        AsVec.IsNaN;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(CIEXYZ other)
    {
        if (Vector256.IsHardwareAccelerated)
        {
            var vec1 = (X, Y, Z, 0.0);
            var vec2 = (other.X, other.Y, other.Z, 0.0);
            var v1 = Unsafe.As<(double, double, double, double), Vector256<double>>(ref vec1);
            var v2 = Unsafe.As<(double, double, double, double), Vector256<double>>(ref vec2);
            return v1.Equals(v2);
        }
        return
            X == other.X &&
            Y == other.Y &&
            Z == other.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(CIEXYZ other, double tolerance)
    {
        if (Vector256.IsHardwareAccelerated)
        {
            var vec1 = (X, Y, Z, 0.0);
            var vec2 = (other.X, other.Y, other.Z, 0.0);
            var v1 = Unsafe.As<(double, double, double, double), Vector256<double>>(ref vec1);
            var v2 = Unsafe.As<(double, double, double, double), Vector256<double>>(ref vec2);
            var v3 = Vector256.Create(tolerance);
            var v4 = v1 - v2;
            var v5 = Vector256.Abs(v4);
            return Vector256.LessThanOrEqualAll(v5, v3);
        }
        return
            Math.Abs(X - other.X) <= tolerance &&
            Math.Abs(Y - other.Y) <= tolerance &&
            Math.Abs(Z - other.Z) <= tolerance;
    }
}
