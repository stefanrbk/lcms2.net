using lcms2.state;

using System.Resources;

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

    [TestCase("test5", "test0", INTENT_PERCEPTUAL, false, Description = "Without Alpha Copy")]
    [TestCase("test5", "test0", INTENT_PERCEPTUAL, true, Description = "With Alpha Copy")]
    [TestCase("test0", "test0", INTENT_PERCEPTUAL, false, Description = "Without Alpha Copy")]
    [TestCase("test0", "test0", INTENT_PERCEPTUAL, true, Description = "With Alpha Copy")]
    public void TestAllFloatValuesWithAlphaTransformParity(string profileInName, string profileOutName, int Intent, bool copyAlpha)
    {
        var flags = cmsFLAGS_NOCACHE | (copyAlpha ? cmsFLAGS_COPY_ALPHA : 0);
        var resources = new ResourceManager("lcms2.FastFloatPlugin.tests.TestProfiles", typeof(FloatConversionTests).Assembly);

        var profileIn = cmsOpenProfileFromMemTHR(_pluginCtx, (byte[])resources.GetObject(profileInName)!)!;
        var profileOut = cmsOpenProfileFromMemTHR(_pluginCtx, (byte[])resources.GetObject(profileOutName)!)!;

        var xformRaw = cmsCreateTransformTHR(_rawCtx, profileIn, TYPE_RGBA_FLT, profileOut, TYPE_RGBA_FLT, (uint)Intent, flags)!;
        var xformPlugin = cmsCreateTransformTHR(_pluginCtx, profileIn, TYPE_RGBA_FLT, profileOut, TYPE_RGBA_FLT, (uint)Intent, flags)!;

        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        Assert.Multiple(() =>
        {
            Assert.That(xformRaw, Is.Not.Null);
            Assert.That(xformPlugin, Is.Not.Null);
        });

        const int nPixels = 256 * 256 * 256;

        var bufferIn = new Scanline_rgbaFloat[nPixels];
        var bufferRawOut = new Scanline_rgbaFloat[nPixels];
        var bufferPluginOut = new Scanline_rgbaFloat[nPixels];

        // Same input to both transforms
        var j = 0;
        for (var r = 0; r < 256; r++)
            for (var g = 0; g < 256; g++)
                for (var b = 0; b < 256; b++)
                {
                    bufferIn[j].r = r / 255.0f;
                    bufferIn[j].g = g / 255.0f;
                    bufferIn[j].b = b / 255.0f;
                    bufferIn[j].a = 1.0f;

                    j++;
                }

        // Different transforms, different output buffers
        cmsDoTransform(xformRaw, bufferIn, bufferRawOut, nPixels);
        cmsDoTransform(xformPlugin, bufferIn, bufferPluginOut, nPixels);

        // Let's compare results
        Assert.Multiple(() =>
        {
            for (j = 0; j < nPixels; j++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(bufferPluginOut[j].r, Is.EqualTo(bufferRawOut[j].r).Within(EPSILON_FLOAT_TESTS));
                    Assert.That(bufferPluginOut[j].g, Is.EqualTo(bufferRawOut[j].g).Within(EPSILON_FLOAT_TESTS));
                    Assert.That(bufferPluginOut[j].b, Is.EqualTo(bufferRawOut[j].b).Within(EPSILON_FLOAT_TESTS));
                    Assert.That(bufferPluginOut[j].a, Is.EqualTo(bufferRawOut[j].a).Within(EPSILON_FLOAT_TESTS));
                });
            }
        });
    }
}
