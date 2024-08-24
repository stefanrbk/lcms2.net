using lcms2.types;

namespace lcms2.FastFloatPlugin.tests;
public class _15BitTests
{
    [TestCaseSource(nameof(Test15BitFormattersReturnInputsAfterRoundTripCases))]
    public void Test15BitFormattersReturnInputsAfterRoundTrip(string a)
    {
        var field = typeof(FastFloat).GetProperty(a) ?? typeof(Lcms2).GetProperty(a);
        var Type = (uint)field!.GetValue(null)!;

        var ValuesIn = new ushort[cmsMAXCHANNELS];
        var ValuesOut = new ushort[cmsMAXCHANNELS];
        var Buffer = new byte[1024];

        var info = new _xform_head(Type, Type);

        // Get functions to go back and forth
        var f = Formatter_15Bit_Factory_In(Type, (uint)PackFlags.Ushort);
        var b = Formatter_15Bit_Factory_Out(Type, (uint)PackFlags.Ushort);

        Assert.Multiple(() =>
        {
            Assert.That(f.Fmt16, Is.Not.Null, $"In formatter missing for {a}");
            Assert.That(b.Fmt16, Is.Not.Null, $"Out formatter missing for {a}");
        });

        var nChannels = T_CHANNELS(Type);
        var bytes = T_BYTES(Type);

        Assert.Multiple(() =>
        {
            for (var j = 0; j < 5; j++)
            {
                for (var i = 0; i < nChannels; i++)
                {
                    ValuesIn[i] = (ushort)((i + j) << 1);
                }

                b.Fmt16((Transform)info, ValuesIn, Buffer, 1);
                f.Fmt16((Transform)info, ValuesOut, Buffer, 1);

                Assert.That(ValuesOut, Is.EquivalentTo(ValuesIn));
            }
        });
    }

    [Test]
    public void TestInternal15BitMacrosReturnProperValuesAfterRoundTrip()
    {
        Assert.Multiple(() =>
        {
            for (var i = 0; i < 256; i++)
            {
                var n = FROM_8_TO_15((byte)i);
                var m = FROM_15_TO_8(n);

                Assert.That(m, Is.EqualTo(i));
            }
        });
    }

    internal static object[] Test15BitFormattersReturnInputsAfterRoundTripCases =
    {
        nameof(TYPE_GRAY_15),
        nameof(TYPE_GRAY_15_REV),
        nameof(TYPE_GRAY_15_SE),
        nameof(TYPE_GRAYA_15),
        nameof(TYPE_GRAYA_15_SE),
        nameof(TYPE_GRAYA_15_PLANAR),
        nameof(TYPE_RGB_15),
        nameof(TYPE_RGB_15_PLANAR),
        nameof(TYPE_RGB_15_SE),
        nameof(TYPE_BGR_15),
        nameof(TYPE_BGR_15_PLANAR),
        nameof(TYPE_BGR_15_SE),
        nameof(TYPE_RGBA_15),
        nameof(TYPE_RGBA_15_PLANAR),
        nameof(TYPE_RGBA_15_SE),
        nameof(TYPE_ARGB_15),
        nameof(TYPE_ABGR_15),
        nameof(TYPE_ABGR_15_PLANAR),
        nameof(TYPE_ABGR_15_SE),
        nameof(TYPE_BGRA_15),
        nameof(TYPE_BGRA_15_SE),
        nameof(TYPE_YMC_15),
        nameof(TYPE_CMY_15),
        nameof(TYPE_CMY_15_PLANAR),
        nameof(TYPE_CMY_15_SE),
        nameof(TYPE_CMYK_15),
        nameof(TYPE_CMYK_15_REV),
        nameof(TYPE_CMYK_15_PLANAR),
        nameof(TYPE_CMYK_15_SE),
        nameof(TYPE_KYMC_15),
        nameof(TYPE_KYMC_15_SE),
        nameof(TYPE_KCMY_15),
        nameof(TYPE_KCMY_15_REV),
        nameof(TYPE_KCMY_15_SE),
    };
}
