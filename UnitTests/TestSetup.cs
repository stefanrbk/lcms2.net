using lcms2.types;

[SetUpFixture]
public static class TestSetup
{
    [OneTimeSetUp]
    public static void Setup()
    {
        var now = DateTime.UtcNow;
        TestStart = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        OneVirtual(cmsCreate_sRGBProfile(), ref sRGBProfile);
        OneVirtual(Create_AboveRGB(), ref aRGBProfile);
        OneVirtual(Create_Gray22(), ref GrayProfile);
        OneVirtual(Create_Gray30(), ref Gray3Profile);
        OneVirtual(Create_GrayLab(), ref GrayLabProfile);
        OneVirtual(Create_CMYK_DeviceLink(), ref LinProfile);
        OneVirtual(cmsCreateInkLimitingDeviceLink(cmsSigCmyData, 150), ref LinProfile);
        OneVirtual(cmsCreateLab2Profile(null), ref Labv2Profile);
        OneVirtual(cmsCreateLab4Profile(null), ref Labv4Profile);
        OneVirtual(cmsCreateXYZProfile(), ref XYZProfile);
        OneVirtual(cmsCreateNULLProfile(), ref nullProfile);
        OneVirtual(cmsCreateBCHSWabstractProfile(17, 0, 0, 0, 0, 5000, 6000), ref BCHSProfile);
        OneVirtual(CreateFakeCMYK(300, false), ref FakeCMYKProfile);
        OneVirtual(cmsCreateBCHSWabstractProfile(17, 0, 1.2, 0, 3, 5000, 5000), ref BrightnessProfile);
    }

    private static void OneVirtual(Profile? h, ref byte[]? mem)
    {
        if (h is null)
            return;

        cmsSaveProfileToMem(h, null, out var bytes);
        mem = new byte[bytes];
        if (!cmsSaveProfileToMem(h, mem, out _))
            mem = null;

        cmsCloseProfile(h);
    }
}
