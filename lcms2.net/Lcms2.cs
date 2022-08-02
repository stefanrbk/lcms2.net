using lcms2.state;
using lcms2.types;

namespace lcms2;
public static class Lcms2
{
    public const int Version = 2131;

    public const int MaxPath = 256;
    public const int MaxChannels = 16;
    public const int MaxTypesInPlugin = 20;

    public static readonly XYZ D50 = (0.9642, 1.0, 0.8249);

    public static readonly XYZ PerceptualBlack = (0.00336, 0.0034731, 0.00287);
    internal const int TypesInLcmsPlugin = 20;
}

#if PLUGIN
public delegate void FreeUserDataFn
#else
internal delegate void FreeUserDataFn
#endif
(Context? context, ref object data);
#if PLUGIN
public delegate object? DupUserDataFn
#else
internal delegate object? DupUserDataFn
#endif
(Context? context, in object? data);
