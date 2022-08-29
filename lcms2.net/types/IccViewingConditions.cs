﻿namespace lcms2.types;

public class IccViewingConditions: ICloneable
{
    public IlluminantType IlluminantType;
    public XYZ IlluminantXyz;
    public XYZ SurroundXyz;

    public IccViewingConditions(XYZ illuminantXyz, XYZ surroundXyz, IlluminantType illuminantType)
    {
        IlluminantXyz = illuminantXyz;
        SurroundXyz = surroundXyz;
        IlluminantType = illuminantType;
    }

    public object Clone() =>
        new IccViewingConditions(IlluminantXyz, SurroundXyz, IlluminantType);
}
