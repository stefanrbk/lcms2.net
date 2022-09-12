//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
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
namespace lcms2.types;

/// <summary>
///     Represents a vector with three double-precision floating-point values.
/// </summary>
/// <remarks>Implements the <c>cmsVEC3</c> struct.</remarks>
public struct Vec3
{
    #region Fields

    public double X, Y, Z;

    #endregion Fields

    #region Public Constructors

    /// <summary>
    ///     Initiate a vector
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3init</c> function.</remarks>
    public Vec3(double x, double y, double z) =>
        (X, Y, Z) = (x, y, z);

    #endregion Public Constructors

    #region Properties

    public static Vec3 NaN =>
        new(Double.NaN, Double.NaN, Double.NaN);

    public bool IsNaN =>
        Double.IsNaN(X) || Double.IsNaN(Y) || Double.IsNaN(Z);

    #endregion Properties

    #region Indexers

    public double this[int index]
    {
        get =>
            index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Valid indexes are between 0 and 2 inclusively."),
            };
        set
        {
            switch (index)
            {
                case 0: X = value; break;
                case 1: Y = value; break;
                case 2: Z = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(index), "Valid indexes are between 0 and 2 inclusively.");
            }
        }
    }

    #endregion Indexers

    #region Public Methods

    /// <summary>
    ///     Vector cross product
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3cross</c> function.</remarks>
    public static Vec3 Cross(Vec3 left, Vec3 right) =>
        new(
            (left.Y * right.Z) - (right.Y * left.Z),
            (left.Z * right.X) - (right.Z * left.X),
            (left.X * right.Y) - (right.X * left.Y));

    /// <summary>
    ///     Euclidean distance
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3distance</c> function.</remarks>
    public static double Distance(Vec3 a, Vec3 b)
    {
        var d1 = a.X - b.X;
        var d2 = a.Y - b.Y;
        var d3 = a.Z - b.Z;

        return Math.Sqrt((d1 * d1) + (d2 * d2) + (d3 * d3));
    }

    /// <summary>
    ///     Vector cross product
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3dot</c> function.</remarks>
    public static double Dot(Vec3 u, Vec3 v) =>
        (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);

    public static Vec3 operator -(Vec3 left, Vec3 right) =>
           Subtract(left, right);

    /// <summary>
    ///     Vector subtraction
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3minus</c> function.</remarks>
    public static Vec3 Subtract(Vec3 left, Vec3 right) =>
        new(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z);

    /// <summary>
    ///     Euclidean length
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3length</c> function.</remarks>
    public double Length() =>
        Math.Sqrt((X * X) + (Y * Y) + (Z * Z));

    #endregion Public Methods
}
