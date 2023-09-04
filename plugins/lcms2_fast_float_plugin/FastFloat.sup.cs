//---------------------------------------------------------------------------------
//
//  Little Color Management System, fast floating point extensions
//  Copyright (c) 1998-2022 Marti Maria Saguer, all rights reserved
//                     2023 Stefan Kewatt, all rights reserved
//
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//---------------------------------------------------------------------------------
using lcms2.types;

using static lcms2.Plugin;

namespace lcms2.FastFloatPlugin;
public static partial class FastFloat
{
    private static bool Floating_Point_Transforms_Dispatcher(out Transform2Fn TransformFn,
                                                             out object? UserData,
                                                             out FreeUserDataFn? FreeUserData,
                                                             ref Pipeline Lut,
                                                             ref uint InputFormat,
                                                             ref uint OutputFormat,
                                                             ref uint dwFlags)
    {
        TransformFn = null!;
        UserData = null;
        FreeUserData = null;

        // Softproofing & gamut check does not use plugin, both are activated via following flag.
        if ((dwFlags & cmsFLAGS_SOFTPROOFING) is not 0)
            return false;

        // Try to optimize as a set of curves plus a matrix plus a set of curves
        if (OptimizeMatrixShaper15(out TransformFn, out UserData, out FreeUserData, ref Lut, ref InputFormat, ref OutputFormat, ref dwFlags))
            return true;

        // Try to optimize by joining curves
        if (Optimize8ByJoiningCurves(out TransformFn, out UserData, out FreeUserData, ref Lut, ref InputFormat, ref OutputFormat, ref dwFlags))
            return true;

        // Try to use SSE2 to optimize as a set of curves plus a matrix plus a set of curves
        if (Optimize8MatrixShaperSSE(out TransformFn, out UserData, out FreeUserData, ref Lut, ref InputFormat, ref OutputFormat, ref dwFlags))
            return true;

        // Try to optimize as a set of curves plus a matrix plus a set of curves
        if (Optimize8MatrixShaper(out TransformFn, out UserData, out FreeUserData, ref Lut, ref InputFormat, ref OutputFormat, ref dwFlags))
            return true;

        // Try to optimize by joining curves
        if (OptimizeFloatByJoiningCurves(out TransformFn, out UserData, out FreeUserData, ref Lut, ref InputFormat, ref OutputFormat, ref dwFlags))
            return true;

        // Try to optimize as a set of curves plus a matrix plus a set of curves
        if (OptimizeFloatMatrixShaper(out TransformFn, out UserData, out FreeUserData, ref Lut, ref InputFormat, ref OutputFormat, ref dwFlags))
            return true;

        // Try to optimize using prelinearization plus tetrahedral on 8 bit RGB
        if (Optimize8BitRGBTransform(out TransformFn, out UserData, out FreeUserData, ref Lut, ref InputFormat, ref OutputFormat, ref dwFlags))
            return true;

        return false;
    }
}
