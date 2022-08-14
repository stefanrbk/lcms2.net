namespace lcms2.types;

/// <summary>
/// Represents a 3x3 matrix of double-precision floating-point values.
/// </summary>
/// <remarks>Implements the <c>cmsMAT3</c> struct.</remarks>
public struct Mat3
{
    public Vec3 X, Y, Z;

    internal const double MatrixDetTolerance = 0.0001;

    public Mat3(Vec3 x, Vec3 y, Vec3 z) =>
        (X, Y, Z) = (x, y, z);

    public Vec3 this[int index]
    {
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
            switch (index) {
                case 0: X = value; break;
                case 1: Y = value; break;
                case 2: Z = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(index), "Valid indexes are between 0 and 2 inclusively.");
            }
        }
    }

    /// <summary>
    /// 3x3 identity matrix
    /// </summary>
    /// <remarks>Implements the <c>_cmsMAT3identity</c> function.</remarks>
    public static Mat3 Identity =>
        new(
            new(1, 0, 0),
            new(0, 1, 0),
            new(0, 0, 1));
    private static bool CloseEnough(double a, double b) =>
        Math.Abs(b - a) < (1.0 / 65535.0);

    /// <summary>
    /// Checks to see if this matrix is within 1e-4 of the identity matrix.
    /// </summary>
    /// <remarks>Implements the <c>_cmsMAT3isIdentity</c> function.</remarks>
    public bool IsIdentity
    {
        get
        {
            for (var i = 0; i < 3; i++) {
                for (var j = 0; j < 3; j++) {
                    if (!CloseEnough(this[i][j], Identity[i][j]))
                        return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Inverse of a matrix b = a^(-1)
    /// </summary>
    /// <remarks>Implements the <c>_cmsMAT3inverse</c> function.</remarks>
    public Mat3? Inverse()
    {
        var c0 = (this[1][1] * this[2][2]) - (this[1][2] * this[2][1]);
        var c1 = (this[1][0] * this[2][2]) + (this[1][2] * this[2][0]);
        var c2 = (this[1][0] * this[2][1]) - (this[1][1] * this[2][0]);

        var det = (this[0][0] * c0) + (this[0][1] * c1) + (this[0][2] * c2);

        return Math.Abs(det) < MatrixDetTolerance
            ? null
            : (new(
            new(
                c0 / det,
                ((this[0][2] * this[2][1]) - (this[0][1] * this[2][2])) / det,
                ((this[0][2] * this[1][2]) - (this[0][2] * this[1][1])) / det),
            new(
                c1 / det,
                ((this[0][0] * this[2][2]) - (this[0][2] * this[2][0])) / det,
                ((this[0][2] * this[1][0]) - (this[0][0] * this[1][2])) / det),
            new(
                c2 / det,
                ((this[0][1] * this[2][0]) - (this[0][0] * this[2][1])) / det,
                ((this[0][0] * this[1][1]) - (this[0][1] * this[1][0])) / det)));
    }

    /// <summary>
    /// Solve a system in the form Ax = b
    /// </summary>
    /// <remarks>Implements the <c>_cmsMAT3solve</c> function.</remarks>
    public Vec3? Solve(Vec3 vec)
    {
        var inv = Inverse();
        return inv?.Eval(vec);
    }

    /// <summary>
    /// Evaluate a vector across a matrix
    /// </summary>
    /// <remarks>Implements the <c>_cmsMAT3eval</c> function.</remarks>
    public Vec3 Eval(Vec3 vec) =>
        new(
            (this[0].X * vec.X) + (this[0].Y * vec.Y) + (this[0].Z * vec.Z),
            (this[1].X * vec.X) + (this[1].Y * vec.Y) + (this[1].Z * vec.Z),
            (this[2].X * vec.X) + (this[2].Y * vec.Y) + (this[2].Z * vec.Z));

    /// <summary>
    /// Multiply two matrices
    /// </summary>
    /// <remarks>Implements the <c>_cmsMAT3per</c> function.</remarks>
    public static Mat3 Multiply(Mat3 a, Mat3 b)
    {
        double RowCol(int i, int j) =>
            (a[i][0] * b[0][j]) + (a[i][1] * b[1][j]) + (a[i][2] * b[2][j]);

        return new(
            new(RowCol(0, 0), RowCol(0, 1), RowCol(0, 2)),
            new(RowCol(1, 0), RowCol(1, 1), RowCol(1, 2)),
            new(RowCol(2, 0), RowCol(2, 1), RowCol(2, 2)));
    }

    public static Mat3 operator *(Mat3 left, Mat3 right) =>
        Multiply(left, right);

    public static explicit operator double[](Mat3 mat) =>
        new double[]
        {
            mat.X.X,
            mat.X.Y,
            mat.X.Z,
            mat.Y.X,
            mat.Y.Y,
            mat.Y.Z,
            mat.Z.X,
            mat.Z.Y,
            mat.Z.Z,
        };

    public static explicit operator Mat3(double[] d) =>
        new(
            new(d[0], d[1], d[2]),
            new(d[3], d[4], d[5]),
            new(d[6], d[7], d[8]));
}
