namespace lcms2;
internal static class Helpers
{
    internal static double atan2deg(double a, double b)
    {
        var h = a is 0 && b is 0
            ? 0
            : Math.Atan2(a, b);

        h *= 180 / Math.PI;

        while (h > 360)
            h -= 360;
        while (h < 0)
            h += 360;

        return h;
    }

    internal static double Sqr(double v) =>
        v * v;

    internal static double f(double t)
    {
        const double Limit = 24.0 / 116 * (24.0 / 116) * (24.0 / 116);

        return (t <= Limit)
            ? (841.0 / 108 * t) + (16.0 / 116)
            : Math.Pow(t, 1.0 / 3);
    }

    internal static double f_1(double t)
    {
        const double Limit = 24.0 / 116;

        return (t <= Limit)
            ? 108.0 / 841 * (t - (16.0 / 116))
            : t * t * t;
    }

    internal static double XYZ2float(ushort v) =>
        _cms15Fixed16toDouble(v << 1);
}
