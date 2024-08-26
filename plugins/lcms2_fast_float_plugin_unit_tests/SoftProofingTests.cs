using lcms2.state;

using NUnit.Framework.Internal;

namespace lcms2.FastFloatPlugin.tests;
public class SoftProofingTests
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

    [TestCaseSource(nameof(TestSoftProofingTransformParityCaseGenerator))]
    public void TestSoftProofingTransformParity(object oIn)
    {
        var hRGB1 = cmsOpenProfileFromMemTHR(_pluginCtx, TestProfiles.test5)!;
        var hRGB2 = cmsOpenProfileFromMemTHR(_pluginCtx, TestProfiles.test3)!;

        var xformNoPlugin = cmsCreateProofingTransformTHR(_rawCtx, hRGB1, TYPE_RGB_FLT, hRGB1, TYPE_RGB_FLT, hRGB2, INTENT_RELATIVE_COLORIMETRIC, INTENT_RELATIVE_COLORIMETRIC, cmsFLAGS_GAMUTCHECK | cmsFLAGS_SOFTPROOFING)!;
        var xformPlugin = cmsCreateProofingTransformTHR(_pluginCtx, hRGB1, TYPE_RGB_FLT, hRGB1, TYPE_RGB_FLT, hRGB2, INTENT_RELATIVE_COLORIMETRIC, INTENT_RELATIVE_COLORIMETRIC, cmsFLAGS_GAMUTCHECK | cmsFLAGS_SOFTPROOFING)!;

        cmsCloseProfile(hRGB1);
        cmsCloseProfile(hRGB2);

        uint nPixels = 256 * 256 * 4;

        var In = (Scanline_rgbFloat[])oIn;
        var Out1 = new Scanline_rgbFloat[nPixels];
        var Out2 = new Scanline_rgbFloat[nPixels];

        cmsDoTransform(xformNoPlugin, In, Out1, nPixels);
        cmsDoTransform(xformPlugin, In, Out2, nPixels);

        Assert.Multiple(() =>
        {
            for (var j = 0; j < nPixels; j++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(ValidFloat(Out2[j].r, Out1[j].r));
                    Assert.That(ValidFloat(Out2[j].g, Out1[j].g));
                    Assert.That(ValidFloat(Out2[j].b, Out1[j].b));
                });
            }
        });

        cmsDeleteTransform(xformNoPlugin);
        cmsDeleteTransform(xformPlugin);
    }

    private static bool ValidFloat(float a, float b) =>
        MathF.Abs(a - b) < EPSILON_FLOAT_TESTS;

    internal static IEnumerable<object> TestSoftProofingTransformParityCaseGenerator()
    {
        var rand = TestContext.CurrentContext.Random;

        var values = new Scanline_rgbFloat[256 * 256 * 4];

        for (var i = 0; i < 16; i++)
        {
            for (var j = 0; j < values.Length; j++)
                values[j] = new(rand.NextFloat(), rand.NextFloat(), rand.NextFloat());
            yield return values;
        }

        rand = new Randomizer();

        for (var i = 0; i < 16; i++)
        {
            for (var j = 0; j < values.Length; j++)
                values[j] = new(rand.NextFloat(), rand.NextFloat(), rand.NextFloat());
            yield return values;
        }
    }
}
