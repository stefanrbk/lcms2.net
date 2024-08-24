namespace lcms2.FastFloatPlugin.tests;

public class IncrementTests
{
    [TestCaseSource(nameof(TestComputeIncrementsMatchExpectedValuesCases))]
    public void TestComputeIncrementsMatchExpectedValues(string frm, int planeStride, int ExpectedChannels, int ExpectedAlpha, (int, int)[] args)
    {
        var field = typeof(FastFloat).GetProperty(frm) ?? typeof(Lcms2).GetProperty(frm);
        var Format = (uint)field!.GetValue(null)!;

        var ComponentStartingOrder = new uint[cmsMAXCHANNELS];
        var ComponentPointerIncrements = new uint[cmsMAXCHANNELS];

        _cmsComputeComponentIncrements(Format, (uint)planeStride, out var nChannels, out var nAlpha, ComponentStartingOrder, ComponentPointerIncrements);

        Assert.Multiple(() =>
        {
            Assert.That(nChannels, Is.EqualTo(ExpectedChannels));
            Assert.That(nAlpha, Is.EqualTo(ExpectedAlpha));

            var nTotal = nAlpha + nChannels;

            for (var i = 0; i < nTotal; i++)
            {
                var (so, pi) = args[i];
                Assert.That(so, Is.EqualTo(ComponentStartingOrder[i]));
                Assert.That(pi, Is.EqualTo(ComponentPointerIncrements[i]));
            }
        });
    }

    internal static object[] TestComputeIncrementsMatchExpectedValuesCases =
    [
        new object[] { nameof(TYPE_GRAY_8), 0, 1, 0, new (int, int)[] { (0, 1) } },
        new object[] { nameof(TYPE_GRAYA_8), 0, 1, 1, new (int, int)[] { (0, 2), (1, 2) } },
        new object[] { nameof(TYPE_AGRAY_8), 0, 1, 1, new (int, int)[] { (1, 2), (0, 2) } },
        new object[] { nameof(TYPE_GRAY_16), 0, 1, 0, new (int, int)[] { (0, 2) } },
        new object[] { nameof(TYPE_GRAYA_16), 0, 1, 1, new (int, int)[] { (0, 4), (2, 4) } },
        new object[] { nameof(TYPE_AGRAY_16), 0, 1, 1, new (int, int)[] { (2, 4), (0, 4) } },

        new object[] { nameof(TYPE_GRAY_FLT), 0, 1, 0, new (int, int)[] { (0, 4) } },
        new object[] { nameof(TYPE_GRAYA_FLT), 0, 1, 1, new (int, int)[] { (0, 8), (4, 8) }},
        new object[] { nameof(TYPE_AGRAY_FLT), 0, 1, 1, new (int, int)[] { (4, 8), (0, 8) }},

        new object[] { nameof(TYPE_GRAY_DBL), 0, 1, 0, new (int, int)[] { (0, 8) } },
        new object[] { nameof(TYPE_AGRAY_DBL), 0, 1, 1, new (int, int)[] { (8, 16), (0, 16) } },

        new object[] { nameof(TYPE_RGB_8), 0, 3, 0, new (int, int)[] { (0, 3), (1, 3), (2, 3) } },
        new object[] { nameof(TYPE_RGBA_8), 0, 3, 1, new (int, int)[] { (0, 4), (1, 4), (2, 4), (3, 4) } },
        new object[] { nameof(TYPE_ARGB_8), 0, 3, 1, new (int, int)[] { (1, 4), (2, 4), (3, 4), (0, 4) } },

        new object[] { nameof(TYPE_RGB_16), 0, 3, 0, new (int, int)[] { (0, 6), (2, 6), (4, 6) } },
        new object[] { nameof(TYPE_RGBA_16), 0, 3, 1, new (int, int)[] { (0, 8), (2, 8), (4, 8), (6, 8) } },
        new object[] { nameof(TYPE_ARGB_16), 0, 3, 1, new (int, int)[] { (2, 8), (4, 8), (6, 8), (0, 8) } },

        new object[] { nameof(TYPE_RGB_FLT), 0, 3, 0, new (int, int)[] { (0, 12), (4, 12), (8, 12) } },
        new object[] { nameof(TYPE_RGBA_FLT), 0, 3, 1, new (int, int)[] { (0, 16), (4, 16), (8, 16), (12, 16) } },
        new object[] { nameof(TYPE_ARGB_FLT), 0, 3, 1, new (int, int)[] { (4, 16), (8, 16), (12, 16), (0, 16) } },

        new object[] { nameof(TYPE_BGR_8), 0, 3, 0, new (int, int)[] { (2, 3), (1, 3), (0, 3) } },
        new object[] { nameof(TYPE_BGRA_8), 0, 3, 1, new (int, int)[] { (2, 4), (1, 4), (0, 4), (3, 4) } },
        new object[] { nameof(TYPE_ABGR_8), 0, 3, 1, new (int, int)[] { (3, 4), (2, 4), (1, 4), (0, 4) } },

        new object[] { nameof(TYPE_BGR_16), 0, 3, 0, new (int, int)[] { (4, 6), (2, 6), (0, 6) } },
        new object[] { nameof(TYPE_BGRA_16), 0, 3, 1, new (int, int)[] { (4, 8), (2, 8), (0, 8), (6, 8) } },
        new object[] { nameof(TYPE_ABGR_16), 0, 3, 1, new (int, int)[] { (6, 8), (4, 8), (2, 8), (0, 8) } },

        new object[] { nameof(TYPE_BGR_FLT), 0, 3, 0, new (int, int)[] { (8, 12), (4, 12), (0, 12) } },
        new object[] { nameof(TYPE_BGRA_FLT), 0, 3, 1, new (int, int)[] { (8, 16), (4, 16), (0, 16), (12, 16) } },
        new object[] { nameof(TYPE_ABGR_FLT), 0, 3, 1, new (int, int)[] { (12, 16), (8, 16), (4, 16), (0, 16) } },

        new object[] { nameof(TYPE_CMYK_8), 0, 4, 0, new (int, int)[] { (0, 4), (1, 4), (2, 4), (3, 4) } },
        new object[] { nameof(TYPE_CMYKA_8), 0, 4, 1, new (int, int)[] { (0, 5), (1, 5), (2, 5), (3, 5), (4, 5) } },
        new object[] { nameof(TYPE_ACMYK_8), 0, 4, 1, new (int, int)[] { (1, 5), (2, 5), (3, 5), (4, 5), (0, 5) } },

        new object[] { nameof(TYPE_KYMC_8), 0, 4, 0, new (int, int)[] { (3, 4), (2, 4), (1, 4), (0, 4) } },
        new object[] { nameof(TYPE_KYMCA_8), 0, 4, 1, new (int, int)[] { (3, 5), (2, 5), (1, 5), (0, 5), (4, 5) } },
        new object[] { nameof(TYPE_AKYMC_8), 0, 4, 1, new (int, int)[] { (4, 5), (3, 5), (2, 5), (1, 5), (0, 5) } },

        new object[] { nameof(TYPE_KCMY_8), 0, 4, 0, new (int, int)[] { (1, 4), (2, 4), (3, 4), (0, 4) } },

        new object[] { nameof(TYPE_CMYK_16), 0, 4, 0, new (int, int)[] { (0, 8), (2, 8), (4, 8), (6, 8) } },
        new object[] { nameof(TYPE_CMYKA_16), 0, 4, 1, new (int, int)[] { (0, 10), (2, 10), (4, 10), (6, 10), (8, 10) } },
        new object[] { nameof(TYPE_ACMYK_16), 0, 4, 1, new (int, int)[] { (2, 10), (4, 10), (6, 10), (8, 10), (0, 10) } },

        new object[] { nameof(TYPE_KYMC_16), 0, 4, 0, new (int, int)[] { (6, 8), (4, 8), (2, 8), (0, 8) } },
        new object[] { nameof(TYPE_KYMCA_16), 0, 4, 1, new (int, int)[] { (6, 10), (4, 10), (2, 10), (0, 10), (8, 10) } },
        new object[] { nameof(TYPE_AKYMC_16), 0, 4, 1, new (int, int)[] { (8, 10), (6, 10), (4, 10), (2, 10), (0, 10) } },

        new object[] { nameof(TYPE_KCMY_16), 0, 4, 0, new (int, int)[] { (2, 8), (4, 8), (6, 8), (0, 8) } },

        // Planar

        new object[] { nameof(TYPE_GRAYA_8_PLANAR), 100, 1, 1, new (int, int)[] { (0, 1), (100, 1) } },
        new object[] { nameof(TYPE_AGRAY_8_PLANAR), 100, 1, 1, new (int, int)[] { (100, 1), (0, 1) } },

        new object[] { nameof(TYPE_GRAYA_16_PLANAR), 100, 1, 1, new (int, int)[] { (0, 2), (100, 2) } },
        new object[] { nameof(TYPE_AGRAY_16_PLANAR), 100, 1, 1, new (int, int)[] { (100, 2), (0, 2) } },

        new object[] { nameof(TYPE_GRAYA_FLT_PLANAR), 100, 1, 1, new (int, int)[] { (0, 4), (100, 4) } },
        new object[] { nameof(TYPE_AGRAY_FLT_PLANAR), 100, 1, 1, new (int, int)[] { (100, 4), (0, 4) } },

        new object[] { nameof(TYPE_GRAYA_DBL_PLANAR), 100, 1, 1, new (int, int)[] { (0, 8), (100, 8) } },
        new object[] { nameof(TYPE_AGRAY_DBL_PLANAR), 100, 1, 1, new (int, int)[] { (100, 8), (0, 8) } },

        new object[] { nameof(TYPE_RGB_8_PLANAR), 100, 3, 0, new (int, int)[] { (0, 1), (100, 1), (200, 1) } },
        new object[] { nameof(TYPE_RGBA_8_PLANAR), 100, 3, 1, new (int, int)[] { (0, 1), (100, 1), (200, 1), (300, 1) } },
        new object[] { nameof(TYPE_ARGB_8_PLANAR), 100, 3, 1, new (int, int)[] { (100, 1), (200, 1), (300, 1), (0, 1) } },

        new object[] { nameof(TYPE_BGR_8_PLANAR), 100, 3, 0, new (int, int)[] { (200, 1), (100, 1), (0, 1) } },
        new object[] { nameof(TYPE_BGRA_8_PLANAR), 100, 3, 1, new (int, int)[] { (200, 1), (100, 1), (0, 1), (300, 1) } },
        new object[] { nameof(TYPE_ABGR_8_PLANAR), 100, 3, 1, new (int, int)[] { (300, 1), (200, 1), (100, 1), (0, 1) } },

        new object[] { nameof(TYPE_RGB_16_PLANAR), 100, 3, 0, new (int, int)[] { (0, 2), (100, 2), (200, 2) } },
        new object[] { nameof(TYPE_RGBA_16_PLANAR), 100, 3, 1, new (int, int)[] { (0, 2), (100, 2), (200, 2), (300, 2) } },
        new object[] { nameof(TYPE_ARGB_16_PLANAR), 100, 3, 1, new (int, int)[] { (100, 2), (200, 2), (300, 2), (0, 2) } },

        new object[] { nameof(TYPE_BGR_16_PLANAR), 100, 3, 0, new (int, int)[] { (200, 2), (100, 2), (0, 2) } },
        new object[] { nameof(TYPE_BGRA_16_PLANAR), 100, 3, 1, new (int, int)[] { (200, 2), (100, 2), (0, 2), (300, 2) } },
        new object[] { nameof(TYPE_ABGR_16_PLANAR), 100, 3, 1, new (int, int)[] { (300, 2), (200, 2), (100, 2), (0, 2) } },
    ];
}
