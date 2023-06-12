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

using lcms2.types;

using System.Runtime.CompilerServices;

namespace lcms2;

public static unsafe partial class Lcms2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DSWAP<T>(ref T x, ref T y) =>
        (y, x) = (x, y);

    internal static void _cmsVEC3init(out VEC3 r, double x, double y, double z) =>
        r = new(x, y, z);

    internal static void _cmsVEC3minus(out VEC3 r, VEC3 a, VEC3 b) =>
        r = new(
            a.X - b.X,
            a.Y - b.Y,
            a.Z - b.Z);

    internal static void _cmsVEC3cross(out VEC3 r, VEC3 u, VEC3 v) =>
        r = new(
            (u.Y * v.Z) - (v.Y * u.Z),
            (u.Z * v.X) - (v.Z * u.X),
            (u.X * v.Y) - (v.X * u.Y));

    internal static double _cmsVEC3dot(VEC3 u, VEC3 v) =>
        (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);

    // Euclidean length
    internal static double _cmsVEC3length(VEC3 a) =>
        Math.Sqrt(
            (a.X * a.X) +
            (a.Y * a.Y) +
            (a.Z * a.Z));

    // Euclidean distance
    internal static double _cmsVEC3distance(VEC3 a, VEC3 b)
    {
        _cmsVEC3minus(out var r, a, b);

        return _cmsVEC3length(r);
    }

    // 3x3 Identity
    internal static void _cmsMAT3identity(out MAT3 a) =>
        a = new(1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0);

    private static bool CloseEnough(double a, double b)
    {
        return Math.Abs(b - a) < (1.0 / 65535.0);
    }

    internal static bool _cmsMAT3isIdentity(MAT3 a)
    {
        _cmsMAT3identity(out var Identity);

        return
            CloseEnough(a.X.X, Identity.X.X) &&
            CloseEnough(a.X.Y, Identity.X.Y) &&
            CloseEnough(a.X.Z, Identity.X.Z) &&
            CloseEnough(a.Y.X, Identity.Y.X) &&
            CloseEnough(a.Y.Y, Identity.Y.Y) &&
            CloseEnough(a.Y.Z, Identity.Y.Z) &&
            CloseEnough(a.Z.X, Identity.Z.X) &&
            CloseEnough(a.Z.Y, Identity.Z.Y) &&
            CloseEnough(a.Z.Z, Identity.Z.Z);
    }

    // Multiply two matrices
    internal static void _cmsMAT3per(out MAT3 r, MAT3 a, MAT3 b)
    {
        double[,] av = new double[3, 3];
        double[,] bv = new double[3, 3];
        av[0, 0] = a.X.X; bv[0, 0] = b.X.X;
        av[0, 1] = a.X.Y; bv[0, 1] = b.X.Y;
        av[0, 2] = a.X.Z; bv[0, 2] = b.X.Z;
        av[1, 0] = a.Y.X; bv[1, 0] = b.Y.X;
        av[1, 1] = a.Y.Y; bv[1, 1] = b.Y.Y;
        av[1, 2] = a.Y.Z; bv[1, 2] = b.Y.Z;
        av[2, 0] = a.Z.X; bv[2, 0] = b.Z.X;
        av[2, 1] = a.Z.Y; bv[2, 1] = b.Z.Y;
        av[2, 2] = a.Z.Z; bv[2, 2] = b.Z.Z;

        double ROWCOL(int i, int j)
        {
            return (av[i, 0] * bv[0, j]) + (av[i, 1] * bv[1, j]) + (av[i, 2] * bv[2, j]);
        }

        r = new(
            ROWCOL(0, 0), ROWCOL(0, 1), ROWCOL(0, 2),
            ROWCOL(1, 0), ROWCOL(1, 1), ROWCOL(1, 2),
            ROWCOL(2, 0), ROWCOL(2, 1), ROWCOL(2, 2));
    }

    // Inverse of a matrix b = a^(-1)
    internal static bool _cmsMAT3inverse(MAT3 a, out MAT3 b)
    {
        double det, c0, c1, c2;

        c0 = (a.Y.Y * a.Z.Z) - (a.Y.Z * a.Z.Y);
        c1 = (-a.Y.X * a.Z.Z) + (a.Y.Z * a.Z.X);
        c2 = (a.Y.X * a.Z.Y) - (a.Y.Y * a.Z.X);

        det = (a.X.X * c0) + (a.X.Y * c1) + (a.X.Z * c2);

        if (Math.Abs(det) < MATRIX_DET_TOLERANCE)
        {
            b = new MAT3();
            return false;  // singular matrix; can't invert
        }

        b = new(
            c0 / det,
            ((a.X.Z * a.Z.Y) - (a.X.Y * a.Z.Z)) / det,
            ((a.X.Y * a.Y.Z) - (a.X.Z * a.Y.Y)) / det,
            c1 / det,
            ((a.X.X * a.Z.Z) - (a.X.Z * a.Z.X)) / det,
            ((a.X.Z * a.Y.X) - (a.X.X * a.Y.Z)) / det,
            c2 / det,
            ((a.X.Y * a.Z.X) - (a.X.X * a.Z.Y)) / det,
            ((a.X.X * a.Y.Y) - (a.X.Y * a.Y.X)) / det);

        return true;
    }

    // Solve a system in the form Ax = b
    internal static bool _cmsMAT3solve(out VEC3 x, MAT3 a, VEC3 b)
    {
        x = new();
        if (!_cmsMAT3inverse(a, out var a_1)) return false;  // Singular matrix

        _cmsMAT3eval(out x, a_1, b);
        return true;
    }

    // Evaluate a vector across a matrix
    internal static void _cmsMAT3eval(out VEC3 r, MAT3 a, VEC3 v) =>
        r = new VEC3(
            (a.X.X * v.X) + (a.X.Y * v.Y) + (a.X.Z * v.Z),
            (a.Y.X * v.X) + (a.Y.Y * v.Y) + (a.Y.Z * v.Z),
            (a.Z.X * v.X) + (a.Z.Y * v.Y) + (a.Z.Z * v.Z));
}
