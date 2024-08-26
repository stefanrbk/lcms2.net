using lcms2.state;

namespace lcms2.FastFloatPlugin.tests;
public class FloatConversionTests
{
    private static readonly Context _pluginCtx = cmsCreateContext()!;
    private static readonly Context _rawCtx = cmsCreateContext()!;

    [OneTimeSetUp]
    public void Setup() =>
        cmsPluginTHR(_pluginCtx, cmsFastFloatExtensions());

    [OneTimeTearDown]
    public void Cleanup()
    {
        cmsDeleteContext(_rawCtx);
        cmsDeleteContext(_pluginCtx);
    }

    [Test]
    public void TestTransformCreationFailureWhenUsingMismatchedChannelsAndCopyAlpha()
    {
        cmsSetLogErrorHandlerTHR(_pluginCtx, BuildNullLogger());

        var hsRGB = cmsCreate_sRGBProfileTHR(_pluginCtx)!;

        var xform = cmsCreateTransformTHR(_pluginCtx, hsRGB, TYPE_RGB_FLT, hsRGB, TYPE_RGBA_FLT, INTENT_PERCEPTUAL, cmsFLAGS_COPY_ALPHA);
        cmsCloseProfile(hsRGB);

        Assert.That(xform, Is.Null);
    }
}
