using lcms2.types;

namespace lcms2;

public delegate void FreeUserDataFn(object? state, ref object data);

public static class Lcms2
{
    public const int MaxPath = 256;
    public const int MaxTypesInPlugin = 20;
    public const int Version = 2131;
    public static readonly XYZ D50 = (0.9642, 1.0, 0.8249);

    public static readonly XYZ PerceptualBlack = (0.00336, 0.0034731, 0.00287);

    internal const int typesInLcmsPlugin = 20;
}

public delegate object? DupUserDataFn(object? state, in object? data);
