using lcms2.state;

namespace lcms2.FastFloatPlugin.tests;

//[Parallelizable(ParallelScope.All)]
public class PremultipliedAlphaTests
{
    private readonly Context _pluginCtx = cmsCreateContext()!;
    private readonly Context _rawCtx = cmsCreateContext()!;

    [SetUp]
    public void Setup() =>
        cmsPluginTHR(_pluginCtx, cmsFastFloatExtensions());

    [TearDown]
    public void Cleanup()
    {
        cmsDeleteContext(_rawCtx);
        cmsDeleteContext(_pluginCtx);
    }

    [Test]
    public void TestPremultipliedAlphaParity()
    {
        ReadOnlySpan<byte> BGRA8 = [255, 192, 160, 128];
        var bgrA8_1 = new byte[4];
        var bgrA8_2 = new byte[4];

        var srgb1 = cmsCreate_sRGBProfile();
        var srgb2 = cmsCreate_sRGBProfile();

        var xform1 = cmsCreateTransformTHR(_rawCtx, srgb1, TYPE_BGRA_8, srgb2, TYPE_BGRA_8_PREMUL, INTENT_PERCEPTUAL, cmsFLAGS_COPY_ALPHA);
        var xform2 = cmsCreateTransformTHR(_pluginCtx, srgb1, TYPE_BGRA_8, srgb2, TYPE_BGRA_8_PREMUL, INTENT_PERCEPTUAL, cmsFLAGS_COPY_ALPHA);

        cmsCloseProfile(srgb1);
        cmsCloseProfile(srgb2);

        cmsDoTransform(xform1, BGRA8, bgrA8_1, 1);
        cmsDoTransform(xform2, BGRA8, bgrA8_2, 1);

        cmsDeleteTransform(xform1);
        cmsDeleteTransform(xform2);

        Assert.That(bgrA8_1, Is.EquivalentTo(bgrA8_2));
    }
}
