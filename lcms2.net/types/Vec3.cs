namespace lcms2.types;

/// <summary>
/// Represents a vector with three double-precision floating-point values.
/// </summary>
/// <remarks>Implements the <c>cmsVEC3</c> struct.</remarks>
public struct Vec3
{
    public double X, Y, Z;

    /// <summary>
    /// Initiate a vector
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3init</c> function.</remarks>
    public Vec3(double x, double y, double z) =>
        (X, Y, Z) = (x, y, z);

    public double this[int index] {
        get
        {
            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Valid indexes are between 0 and 2 inclusively."),
            };
        }
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

    /// <summary>
    /// Euclidean length
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3length</c> function.</remarks>
    public double Length() =>
        Math.Sqrt((X * X) + (Y * Y) + (Z * Z));

    /// <summary>
    /// Vector subtraction
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3minus</c> function.</remarks>
    public static Vec3 Subtract(Vec3 left, Vec3 right) =>
        new(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z);

    /// <summary>
    /// Vector cross product
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3cross</c> function.</remarks>
    public static Vec3 Cross(Vec3 left, Vec3 right) =>
        new(
            (left.Y * right.Z) - (right.Y * left.Z),
            (left.Z * right.X) - (right.Z * left.X),
            (left.X * right.Y) - (right.X * left.Y));

    /// <summary>
    /// Vector cross product
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3dot</c> function.</remarks>
    public static double Dot(Vec3 u, Vec3 v) =>
        (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);

    /// <summary>
    /// Euclidean distance
    /// </summary>
    /// <remarks>Implements the <c>_cmsVEC3distance</c> function.</remarks>
    public static double Distance(Vec3 a, Vec3 b)
    {
        var d1 = a.X - b.X;
        var d2 = a.Y - b.Y;
        var d3 = a.Z - b.Z;

        return Math.Sqrt((d1 * d1) + (d2 * d2) + (d3 * d3));
    }

    public static Vec3 operator -(Vec3 left, Vec3 right) =>
        Subtract(left, right);
}
