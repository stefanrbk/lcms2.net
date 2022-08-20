namespace lcms2.types;

public unsafe struct IccProfile
{
    /// <summary>
    ///     Assuming io points to an ICC profile, compute and store MD5 checksum In the header,
    ///     rendering intent, attributes and ID should be set to zero before computing MD5 checksum
    ///     (per 6.1.13 in ICC spec)
    /// </summary>
    /// <remarks>Implements the <c>cmsMD5computeID</c> function.</remarks>
    public bool ComputeId()
    {
        // TODO
        throw new NotImplementedException();
    }
}
