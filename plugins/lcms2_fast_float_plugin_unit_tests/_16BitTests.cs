using lcms2.state;
using lcms2.types;

using System.Resources;

namespace lcms2.FastFloatPlugin.tests;

//[Parallelizable(ParallelScope.All)]
public class _16BitTests
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

    [TestCase(nameof(TestProfiles.test5), nameof(TestProfiles.test3), INTENT_PERCEPTUAL, Description = "CLUT accuracy")]
    public void TestConversionsOnAll16BitValues(string profileInName, string profileOutName, int Intent)
    {
        var resources = new ResourceManager("lcms2.FastFloatPlugin.tests.TestProfiles", typeof(_16BitTests).Assembly);

        var profileInData = (byte[])resources.GetObject(profileInName)!;
        var profileOutData = (byte[])resources.GetObject(profileOutName)!;

        var profileIn = cmsOpenProfileFromMemTHR(_pluginCtx, profileInData)!;
        var profileOut = cmsOpenProfileFromMemTHR(_pluginCtx, profileOutData)!;

        var xformRaw = cmsCreateTransformTHR(_rawCtx, profileIn, TYPE_RGBA_16, profileOut, TYPE_RGBA_16, (uint)Intent, cmsFLAGS_NOCACHE | cmsFLAGS_COPY_ALPHA);
        var xformPlugin = cmsCreateTransformTHR(_pluginCtx, profileIn, TYPE_RGBA_16, profileOut, TYPE_RGBA_16, (uint)Intent, cmsFLAGS_NOCACHE | cmsFLAGS_COPY_ALPHA);

        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        Assert.Multiple(() =>
        {
            Assert.That(xformRaw, Is.Not.Null);
            Assert.That(xformPlugin, Is.Not.Null);
        });

        const int npixels = 256 * 256 * 256;  // All RGB cube in 8 bits

        var bufferIn = new Scanline_rgba16bits[npixels];
        var bufferRawOut = new Scanline_rgba16bits[npixels];
        var bufferPluginOut = new Scanline_rgba16bits[npixels];

        // Fill input values for 8 and 15 bits
        var j = 0;
        for (var r = 0; r < 256; r++)
        {
            for (var g = 0; g < 256; g++)
            {
                for (var b = 0; b < 256; b++)
                {
                    bufferIn[j].r = FROM_8_TO_16((byte)r);
                    bufferIn[j].g = FROM_8_TO_16((byte)g);
                    bufferIn[j].b = FROM_8_TO_16((byte)b);
                    bufferIn[j].a = 0xffff;

                    j++;
                }
            }
        }

        cmsDoTransform(xformRaw, bufferIn, bufferRawOut, npixels);
        cmsDoTransform(xformPlugin, bufferIn, bufferPluginOut, npixels);

        Assert.Multiple(() =>
        {
            for (var j = 0; j < npixels; j++)
            {
                // Check the results
                Assert.That(bufferPluginOut[j], Is.EqualTo(bufferRawOut[j]));
            }
        });

    }
}
