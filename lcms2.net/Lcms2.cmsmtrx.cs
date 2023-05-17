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

    internal static void _cmsVEC3init(VEC3* r, double x, double y, double z)
    {
        r->X = x;
        r->Y = y;
        r->Z = z;
    }

    internal static void _cmsVEC3minus(VEC3* r, in VEC3* a, in VEC3* b)
    {
        r->X = a->X - b->X;
        r->Y = a->Y - b->Y;
        r->Z = a->Z - b->Z;
    }

    internal static void _cmsVEC3cross(VEC3* r, in VEC3* u, in VEC3* v)
    {
        r->X = (u->Y * v->Z) - (v->Y * u->Z);
        r->Y = (u->Z * v->X) - (v->Z * u->X);
        r->Z = (u->X * v->Y) - (v->X * u->Y);
    }

    internal static double _cmsVEC3dot(in VEC3* u, in VEC3* v) =>
        (u->X * v->X) + (u->Y * v->Y) + (u->Z * v->Z);

    // Euclidean length
    internal static double _cmsVEC3length(in VEC3* a)
    {
        return Math.Sqrt(
            (a->X * a->X) +
            (a->Y * a->Y) +
            (a->Z * a->Z));
    }

    // Euclidean distance
    internal static double _cmsVEC3distance(in VEC3* a, in VEC3* b)
    {
        double d1 = a->X - b->X;
        double d2 = a->Y - b->Y;
        double d3 = a->Z - b->Z;

        return Math.Sqrt((d1 * d1) + (d2 * d2) + (d3 * d3));
    }

    // 3x3 Identity
    internal static void _cmsMAT3identity(MAT3* a)
    {
        var av = (VEC3*)a;
        _cmsVEC3init(&av[0], 1.0, 0.0, 0.0);
        _cmsVEC3init(&av[1], 0.0, 1.0, 0.0);
        _cmsVEC3init(&av[2], 0.0, 0.0, 1.0);
    }

    private static bool CloseEnough(double a, double b)
    {
        return Math.Abs(b - a) < (1.0 / 65535.0);
    }

    internal static bool _cmsMAT3isIdentity(in MAT3* a)
    {
        var av = &a->X;
        MAT3 Identity;
        var Identityv = &Identity.X;
        int i, j;

        _cmsMAT3identity(&Identity);

        for (i = 0; i < 3; i++)
        {
            for (j = 0; j < 3; j++)
            {
                if (!CloseEnough((&av[i].X)[j], (&Identityv[i].X)[j])) return false;
            }
        }

        return true;
    }

    // Multiply two matrices
    internal static void _cmsMAT3per(MAT3* r, in MAT3* a, in MAT3* b)
    {
        var av = (VEC3*)a;
        var bv = (VEC3*)b;
        var rv = (VEC3*)r;

        double ROWCOL(int i, int j)
        {
            return ((&av[i].X)[0] * (&bv[0].X)[j]) + ((&av[i].X)[1] * (&bv[1].X)[j]) + ((&av[i].X)[2] * (&bv[2].X)[j]);
        }

        _cmsVEC3init(&rv[0], ROWCOL(0, 0), ROWCOL(0, 1), ROWCOL(0, 2));
        _cmsVEC3init(&rv[1], ROWCOL(1, 0), ROWCOL(1, 1), ROWCOL(1, 2));
        _cmsVEC3init(&rv[2], ROWCOL(2, 0), ROWCOL(2, 1), ROWCOL(2, 2));
    }

    // Inverse of a matrix b = a^(-1)
    internal static bool _cmsMAT3inverse(in MAT3* a, MAT3* b)
    {
        double det, c0, c1, c2;

        c0 = a->Y.Y * a->Z.Z - a->Y.Z * a->Z.Y;
        c1 = -a->Y.X * a->Z.Z + a->Y.Z * a->Z.X;
        c2 = a->Y.X * a->Z.Y - a->Y.Y * a->Z.X;

        det = a->X.X * c0 + a->X.Y * c1 + a->X.Z * c2;

        if (Math.Abs(det) < MATRIX_DET_TOLERANCE) return false;  // singular matrix; can't invert

        b->X.X = c0 / det;
        b->X.Y = (a->X.Z * a->Z.Y - a->X.Y * a->Z.Z) / det;
        b->X.Z = (a->X.Y * a->Y.Z - a->X.Z * a->Y.Y) / det;
        b->Y.X = c1 / det;
        b->Y.Y = (a->X.X * a->Z.Z - a->X.Z * a->Z.X) / det;
        b->Y.Z = (a->X.Z * a->Y.X - a->X.X * a->Y.Z) / det;
        b->Z.X = c2 / det;
        b->Z.Y = (a->X.Y * a->Z.X - a->X.X * a->Z.Y) / det;
        b->Z.Z = (a->X.X * a->Y.Y - a->X.Y * a->Y.X) / det;

        return true;
    }

    // Solve a system in the form Ax = b
    internal static bool _cmsMAT3solve(VEC3* x, MAT3* a, VEC3* b)
    {
        MAT3 m, a_1;

        memmove(&m, a);

        if (!_cmsMAT3inverse(&m, &a_1)) return false;  // Singular matrix

        _cmsMAT3eval(x, &a_1, b);
        return true;
    }

    // Evaluate a vector across a matrix
    internal static void _cmsMAT3eval(VEC3* r, in MAT3* a, in VEC3* v)
    {
        var av = &a->X;
        var av0 = &av->X;
        var av1 = &av->Y;
        var av2 = &av->Z;

        r->X = a->X.X * v->X + a->X.Y * v->Y + a->X.Z * v->Z;
        r->Y = a->Y.X * v->X + a->Y.Y * v->Y + a->Y.Z * v->Z;
        r->Z = a->Z.X * v->X + a->Z.Y * v->Y + a->Z.Z * v->Z;
    }
}
