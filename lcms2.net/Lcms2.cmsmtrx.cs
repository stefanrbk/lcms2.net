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

public static unsafe partial class Lcms2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DSWAP<T>(ref T x, ref T y) =>
        (y, x) = (x, y);

    internal static void _cmsVEC3init(VEC3* r, double x, double y, double z)
    {
        r->n[VX] = x;
        r->n[VY] = y;
        r->n[VZ] = z;
    }

    internal static void _cmsVEC3minus(VEC3* r, in VEC3* a, in VEC3* b)
    {
        r->n[VX] = a->n[VX] - b->n[VX];
        r->n[VY] = a->n[VY] - b->n[VY];
        r->n[VZ] = a->n[VZ] - b->n[VZ];
    }

    internal static void _cmsVEC3cross(VEC3* r, in VEC3* u, in VEC3* v)
    {
        r->n[VX] = (u->n[VY] * v->n[VZ]) - (v->n[VY] * u->n[VZ]);
        r->n[VY] = (u->n[VZ] * v->n[VX]) - (v->n[VZ] * u->n[VX]);
        r->n[VZ] = (u->n[VX] * v->n[VY]) - (v->n[VX] * u->n[VY]);
    }

    internal static double _cmsVEC3dot(in VEC3* u, in VEC3* v) =>
        (u->n[VX] * v->n[VX]) + (u->n[VY] * v->n[VY]) + (u->n[VZ] * v->n[VZ]);

    // Euclidean length
    internal static double _cmsVEC3length(in VEC3* a)
    {
        return Math.Sqrt(
            (a->n[VX] * a->n[VX]) +
            (a->n[VY] * a->n[VY]) +
            (a->n[VZ] * a->n[VZ]));
    }

    // Euclidean distance
    internal static double _cmsVEC3distance(in VEC3* a, in VEC3* b)
    {
        double d1 = a->n[VX] - b->n[VX];
        double d2 = a->n[VY] - b->n[VY];
        double d3 = a->n[VZ] - b->n[VZ];

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
        var av = (VEC3*)a;
        MAT3 Identity;
        var Identityv = (VEC3*)&Identity;
        int i, j;

        _cmsMAT3identity(&Identity);

        for (i = 0; i < 3; i++)
        {
            for (j = 0; j < 3; j++)
            {
                if (!CloseEnough(av[i].n[j], Identityv[i].n[j])) return false;
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
            return (av[i].n[0] * bv[0].n[j]) + (av[i].n[1] * bv[1].n[j]) + (av[i].n[2] * bv[2].n[j]);
        }

        _cmsVEC3init(&rv[0], ROWCOL(0, 0), ROWCOL(0, 1), ROWCOL(0, 2));
        _cmsVEC3init(&rv[1], ROWCOL(1, 0), ROWCOL(1, 1), ROWCOL(1, 2));
        _cmsVEC3init(&rv[2], ROWCOL(2, 0), ROWCOL(2, 1), ROWCOL(2, 2));
    }

    // Inverse of a matrix b = a^(-1)
    internal static bool _MAT3inverse(in MAT3* a, MAT3* b)
    {
        var av = (VEC3*)a;
        var bv = (VEC3*)b;
        double det, c0, c1, c2;

        c0 = (av[1].n[1] * av[2].n[2]) - (av[1].n[2] * av[2].n[1]);
        c1 = (-av[1].n[0] * av[2].n[2]) + (av[1].n[2] * av[2].n[0]);
        c2 = (av[1].n[0] * av[2].n[1]) - (av[1].n[1] * av[2].n[0]);

        det = (av[0].n[0] * c0) + (av[0].n[1] * c1) + (av[0].n[2] * c2);

        if (Math.Abs(det) < MATRIX_DET_TOLERANCE) return false;  // singular matrix; can't invert

        bv[0].n[0] = c0 / det;
        bv[0].n[1] = ((av[0].n[2] * av[2].n[1]) - (av[0].n[1] * av[2].n[2])) / det;
        bv[0].n[2] = ((av[0].n[1] * av[1].n[2]) - (av[0].n[2] * av[1].n[1])) / det;
        bv[1].n[0] = c1 / det;
        bv[1].n[1] = ((av[0].n[0] * av[2].n[2]) - (av[0].n[2] * av[2].n[0])) / det;
        bv[1].n[2] = ((av[0].n[2] * av[1].n[0]) - (av[0].n[0] * av[1].n[2])) / det;
        bv[2].n[0] = c2 / det;
        bv[2].n[1] = ((av[0].n[1] * av[2].n[0]) - (av[0].n[0] * av[2].n[1])) / det;
        bv[2].n[2] = ((av[0].n[0] * av[1].n[1]) - (av[0].n[1] * av[1].n[0])) / det;

        return true;
    }

    // Solve a system in the form Ax = b
    internal static bool _cmsMAT3solve(VEC3* x, MAT3* a, VEC3* b)
    {
        MAT3 m, a_1;

        memmove(&m, a);

        if (!_MAT3inverse(&m, &a_1)) return false;  // Singular matrix

        _cmsMAT3eval(x, &a_1, b);
        return true;
    }

    // Evaluate a vector across a matrix
    internal static void _cmsMAT3eval(VEC3* r, in MAT3* a, in VEC3* v)
    {
        var av = (VEC3*)a;

        r->n[VX] = (av[0].n[VX] * v->n[VX]) + (av[0].n[VY] * v->n[VY]) + (av[0].n[VZ] * v->n[VZ]);
        r->n[VY] = (av[1].n[VX] * v->n[VX]) + (av[1].n[VY] * v->n[VY]) + (av[1].n[VZ] * v->n[VZ]);
        r->n[VZ] = (av[2].n[VX] * v->n[VX]) + (av[2].n[VY] * v->n[VY]) + (av[2].n[VZ] * v->n[VZ]);
    }
}
