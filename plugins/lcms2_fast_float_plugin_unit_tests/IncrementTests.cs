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

            for (var i = 0; i < nTotal; i++)
            {
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
    ];
}
