using LanguageExt;

using lcms2.types;

namespace lcms2;
public static class CHAD
{
    public static Option<CIEXYZ> AdaptToIlluminant(CIEXYZ SourceWhitePt, CIEXYZ Illuminant, CIEXYZ Value)
    {
        //_cmsAssert(Result);
        //_cmsAssert(SourceWhitePt);
        //_cmsAssert(Illuminant);
        //_cmsAssert(Value);

        var Bradford = AdaptationMatrix(null, SourceWhitePt, Illuminant);
        if (Bradford.IsNaN)
            return Option<CIEXYZ>.None;

        var In = new VEC3(Value.X, Value.Y, Value.Z);
        var Out = Bradford.Eval(In);

        return Option<CIEXYZ>.Some(Out.AsXYZ);
    }

    internal static MAT3 AdaptationMatrix(MAT3? ConeMatrix, CIEXYZ FromIll, CIEXYZ ToIll)
    {
        var LamRigg = new MAT3(0.8951, 0.2664, -0.1614, -0.7502, 1.7135, 0.0367, 0.0389, -0.0685, 1.0296);

        var _coneMatrix = ConeMatrix ?? LamRigg;

        return ComputeChromaticAdaptation(FromIll, ToIll, _coneMatrix);
    }

    private static MAT3 ComputeChromaticAdaptation(CIEXYZ SourceWhitePoint, CIEXYZ DestWhitePoint, MAT3 Chad)
    {
        var Chad_Inv = Chad.Inverse;
        if (Chad_Inv.IsNaN)
            return MAT3.NaN;

        var ConeSourceXYZ = new VEC3(SourceWhitePoint.X, SourceWhitePoint.Y, SourceWhitePoint.Z);

        var ConeDestXYZ = new VEC3(DestWhitePoint.X, DestWhitePoint.Y, DestWhitePoint.Z);

        var ConeSourceRGB = Chad.Eval(ConeSourceXYZ);
        var ConeDestRGB = Chad.Eval(ConeDestXYZ);

        if ((Math.Abs(ConeSourceRGB.X) < MATRIX_DET_TOLERANCE) ||
            (Math.Abs(ConeSourceRGB.Y) < MATRIX_DET_TOLERANCE) ||
            (Math.Abs(ConeSourceRGB.Z) < MATRIX_DET_TOLERANCE))
        {
            return MAT3.NaN;
        }

        // Build matrix
        var Cone = new MAT3(
            x: new(ConeDestRGB.X / ConeSourceRGB.X, 0.0, 0.0),
            y: new(0.0, ConeDestRGB.Y / ConeSourceRGB.Y, 0.0),
            z: new(0.0, 0.0, ConeDestRGB.Z / ConeSourceRGB.Z));

        // Normalize
        var Tmp = Cone * Chad;
        return Chad_Inv * Tmp;
    }
}
