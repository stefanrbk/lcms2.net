using lcms2.state;

namespace lcms2.FastFloatPlugin.tests;
public class ChangeFormatTests
{
    private readonly Context _ctx = cmsCreateContext()!;

    [SetUp]
    public void Setup() =>
        cmsPluginTHR(_ctx, cmsFastFloatExtensions());

    [TearDown]
    public void Cleanup() =>
        cmsDeleteContext(_ctx);

    [Test]
    public void TestChangeFormatFunctionWorksWithKnownValues()
    {
        var rgb8 = new Scanline_rgb8bits(10, 120, 40);
        var rgb16 = new Scanline_rgb16bits(10 * 257, 120 * 257, 40 * 257);

        var hsRGB = cmsCreate_sRGBProfileTHR(_ctx)!;
        var hLab = cmsCreateLab4ProfileTHR(_ctx, null)!;

        var xform = cmsCreateTransformTHR(_ctx, hsRGB, TYPE_RGB_16, hLab, TYPE_Lab_16, INTENT_PERCEPTUAL, 0)!;

        cmsCloseProfile(hsRGB);
        cmsCloseProfile(hLab);

        cmsDoTransform(xform, rgb16, out Scanline_Lab16bits lab16_1, 1);

        cmsChangeBuffersFormat(xform, TYPE_RGB_8, TYPE_Lab_16);

        cmsDoTransform(xform, rgb8, out Scanline_Lab16bits lab16_2, 1);
        cmsDeleteTransform(xform);

        Assert.That(lab16_2, Is.EqualTo(lab16_1));
    }
}
