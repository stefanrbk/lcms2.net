using lcms2.types;

namespace lcms2;
public static class Lcms2
{
    public const int Version = 2131;

    public const int MaxPath = 256;

    public readonly static XYZ D50 = (0.9642, 1.0, 0.8249);

    public readonly static XYZ PerceptualBlack = (0.00336, 0.0034731, 0.00287);
}