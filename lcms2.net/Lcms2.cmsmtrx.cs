//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright ©️ 1998-2024 Marti Maria Saguer
//              2022-2024 Stefan Kewatt
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

//using lcms2.types;

//using System.Diagnostics;
//using System.Runtime.CompilerServices;

//namespace lcms2;

//public static partial class Lcms2
//{
//    [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static void DSWAP<T>(ref T x, ref T y) =>
//        (y, x) = (x, y);

//    [DebuggerStepThrough]
//    internal static VEC3 _cmsVEC3init(double x, double y, double z) =>
//        // Moved to VEC3.ctor
//        new(x, y, z);

//    [DebuggerStepThrough]
//    internal static VEC3 _cmsVEC3minus(VEC3 a, VEC3 b) =>
//        // Moved to VEC3.op_Subtraction
//        a - b;

//    [DebuggerStepThrough]
//    internal static VEC3 _cmsVEC3cross(VEC3 u, VEC3 v) =>
//        // Moved to VEC3.Cross
//        u.Cross(v);

//    [DebuggerStepThrough]
//    internal static double _cmsVEC3dot(VEC3 u, VEC3 v) =>
//        // Moved to VEC3.Dot
//        u.Dot(v);

//    [DebuggerStepThrough]
//    // Euclidean length
//    internal static double _cmsVEC3length(VEC3 a) =>
//        // Moved to VEC3.Length
//        a.Length;

//    [DebuggerStepThrough]
//    // Euclidean distance
//    internal static double _cmsVEC3distance(VEC3 a, VEC3 b) =>
//        // Moved to VEC3.Distance
//        a.Distance(b);

//    [DebuggerStepThrough]
//    // 3x3 Identity
//    internal static MAT3 _cmsMAT3identity() =>
//        // Moved to MAT3.Identity
//        MAT3.Identity;

//    [DebuggerStepThrough]
//    internal static bool _cmsMAT3isIdentity(MAT3 a) =>
//        // Moved to MAT3.IsIdentity
//        a.IsIdentity;

//    [DebuggerStepThrough]
//    // Multiply two matrices
//    internal static MAT3 _cmsMAT3per(MAT3 a, MAT3 b) =>
//        // Moved to MAT3.op_Multiplication
//        a * b;

//    [DebuggerStepThrough]
//    // Inverse of a matrix b = a^(-1)
//    internal static MAT3 _cmsMAT3inverse(MAT3 a) =>
//        // Moved to MAT3.Inverse
//        a.Inverse;

//    [DebuggerStepThrough]
//    // Solve a system in the form Ax = b
//    internal static VEC3 _cmsMAT3solve(MAT3 a, VEC3 b) =>
//        // Moved to MAT3.Solve
//        a.Solve(b);

//    [DebuggerStepThrough]
//    // Evaluate a vector across a matrix
//    internal static VEC3 _cmsMAT3eval(MAT3 a, VEC3 v) =>
//        // Moved to MAT3.Eval
//        a.Eval(v);
//}
