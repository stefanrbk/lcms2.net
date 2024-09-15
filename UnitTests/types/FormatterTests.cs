//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright ©️ 1998-2024 Marti Maria Saguer
//              2022-2024 Stefan Kewatt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//---------------------------------------------------------------------------------

//namespace lcms2.tests.types;

//[TestFixture(TestOf = typeof(Formatter))]
//public class FormatterTests : TestBase
//{
//    #region Fields

//    private static readonly Dictionary<PixelFormat, string> _formats = new()
//    {
//            { GRAY_8, nameof(GRAY_8) },
//            { GRAY_8_REV, nameof(GRAY_8_REV) },
//            { GRAY_16, nameof(GRAY_16) },
//            { GRAY_16_REV, nameof(GRAY_16_REV) },
//            { GRAY_16_SE, nameof(GRAY_16_SE) },
//            { GRAYA_8, nameof(GRAYA_8) },
//            { GRAYA_16, nameof(GRAYA_16) },
//            { GRAYA_16_SE, nameof(GRAYA_16_SE) },
//            { GRAYA_8_PLANAR, nameof(GRAYA_8_PLANAR) },
//            { GRAYA_16_PLANAR, nameof(GRAYA_16_PLANAR) },
//            { RGB_8, nameof(RGB_8) },
//            { RGB_8_PLANAR, nameof(RGB_8_PLANAR) },
//            { BGR_8, nameof(BGR_8) },
//            { BGR_8_PLANAR, nameof(BGR_8_PLANAR) },
//            { RGB_16, nameof(RGB_16) },
//            { RGB_16_PLANAR, nameof(RGB_16_PLANAR) },
//            { RGB_16_SE, nameof(RGB_16_SE) },
//            { BGR_16, nameof(BGR_16) },
//            { BGR_16_PLANAR, nameof(BGR_16_PLANAR) },
//            { BGR_16_SE, nameof(BGR_16_SE) },
//            { RGBA_8, nameof(RGBA_8) },
//            { RGBA_8_PLANAR, nameof(RGBA_8_PLANAR) },
//            { RGBA_16, nameof(RGBA_16) },
//            { RGBA_16_PLANAR, nameof(RGBA_16_PLANAR) },
//            { RGBA_16_SE, nameof(RGBA_16_SE) },
//            { ARGB_8, nameof(ARGB_8) },
//            { ARGB_8_PLANAR, nameof(ARGB_8_PLANAR) },
//            { ARGB_16, nameof(ARGB_16) },
//            { ABGR_8, nameof(ABGR_8) },
//            { ABGR_8_PLANAR, nameof(ABGR_8_PLANAR) },
//            { ABGR_16, nameof(ABGR_16) },
//            { ABGR_16_PLANAR, nameof(ABGR_16_PLANAR) },
//            { ABGR_16_SE, nameof(ABGR_16_SE) },
//            { BGRA_8, nameof(BGRA_8) },
//            { BGRA_8_PLANAR, nameof(BGRA_8_PLANAR) },
//            { BGRA_16, nameof(BGRA_16) },
//            { BGRA_16_SE, nameof(BGRA_16_SE) },
//            { CMY_8, nameof(CMY_8) },
//            { CMY_8_PLANAR, nameof(CMY_8_PLANAR) },
//            { CMY_16, nameof(CMY_16) },
//            { CMY_16_PLANAR, nameof(CMY_16_PLANAR) },
//            { CMY_16_SE, nameof(CMY_16_SE) },
//            { CMYK_8, nameof(CMYK_8) },
//            { CMYKA_8, nameof(CMYKA_8) },
//            { CMYK_8_REV, nameof(CMYK_8_REV) },
//            { CMYK_8_PLANAR, nameof(CMYK_8_PLANAR) },
//            { CMYK_16, nameof(CMYK_16) },
//            { CMYK_16_REV, nameof(CMYK_16_REV) },
//            { CMYK_16_PLANAR, nameof(CMYK_16_PLANAR) },
//            { CMYK_16_SE, nameof(CMYK_16_SE) },
//            { KYMC_8, nameof(KYMC_8) },
//            { KYMC_16, nameof(KYMC_16) },
//            { KYMC_16_SE, nameof(KYMC_16_SE) },
//            { KCMY_8, nameof(KCMY_8) },
//            { KCMY_8_REV, nameof(KCMY_8_REV) },
//            { KCMY_16, nameof(KCMY_16) },
//            { KCMY_16_REV, nameof(KCMY_16_REV) },
//            { KCMY_16_SE, nameof(KCMY_16_SE) },
//            { CMYK5_8, nameof(CMYK5_8) },
//            { CMYK5_16, nameof(CMYK5_16) },
//            { CMYK5_16_SE, nameof(CMYK5_16_SE) },
//            { KYMC5_8, nameof(KYMC5_8) },
//            { KYMC5_16, nameof(KYMC5_16) },
//            { KYMC5_16_SE, nameof(KYMC5_16_SE) },
//            { CMYK6_8, nameof(CMYK6_8) },
//            { CMYK6_8_PLANAR, nameof(CMYK6_8_PLANAR) },
//            { CMYK6_16, nameof(CMYK6_16) },
//            { CMYK6_16_PLANAR, nameof(CMYK6_16_PLANAR) },
//            { CMYK6_16_SE, nameof(CMYK6_16_SE) },
//            { CMYK7_8, nameof(CMYK7_8) },
//            { CMYK7_16, nameof(CMYK7_16) },
//            { CMYK7_16_SE, nameof(CMYK7_16_SE) },
//            { KYMC7_8, nameof(KYMC7_8) },
//            { KYMC7_16, nameof(KYMC7_16) },
//            { KYMC7_16_SE, nameof(KYMC7_16_SE) },
//            { CMYK8_8, nameof(CMYK8_8) },
//            { CMYK8_16, nameof(CMYK8_16) },
//            { CMYK8_16_SE, nameof(CMYK8_16_SE) },
//            { KYMC8_8, nameof(KYMC8_8) },
//            { KYMC8_16, nameof(KYMC8_16) },
//            { KYMC8_16_SE, nameof(KYMC8_16_SE) },
//            { CMYK9_8, nameof(CMYK9_8) },
//            { CMYK9_16, nameof(CMYK9_16) },
//            { CMYK9_16_SE, nameof(CMYK9_16_SE) },
//            { KYMC9_8, nameof(KYMC9_8) },
//            { KYMC9_16, nameof(KYMC9_16) },
//            { KYMC9_16_SE, nameof(KYMC9_16_SE) },
//            { CMYK10_8, nameof(CMYK10_8) },
//            { CMYK10_16, nameof(CMYK10_16) },
//            { CMYK10_16_SE, nameof(CMYK10_16_SE) },
//            { KYMC10_8, nameof(KYMC10_8) },
//            { KYMC10_16, nameof(KYMC10_16) },
//            { KYMC10_16_SE, nameof(KYMC10_16_SE) },
//            { CMYK11_8, nameof(CMYK11_8) },
//            { CMYK11_16, nameof(CMYK11_16) },
//            { CMYK11_16_SE, nameof(CMYK11_16_SE) },
//            { KYMC11_8, nameof(KYMC11_8) },
//            { KYMC11_16, nameof(KYMC11_16) },
//            { KYMC11_16_SE, nameof(KYMC11_16_SE) },
//            { CMYK12_8, nameof(CMYK12_8) },
//            { CMYK12_16, nameof(CMYK12_16) },
//            { CMYK12_16_SE, nameof(CMYK12_16_SE) },
//            { KYMC12_8, nameof(KYMC12_8) },
//            { KYMC12_16, nameof(KYMC12_16) },
//            { KYMC12_16_SE, nameof(KYMC12_16_SE) },
//        { XYZ_16, nameof(XYZ_16) },
//        { Lab_8, nameof(Lab_8) },
//            { ALab_8, nameof(ALab_8) },
//            { Lab_16, nameof(Lab_16) },
//            { Yxy_16, nameof(Yxy_16) },
//            { YCbCr_8, nameof(YCbCr_8) },
//            { YCbCr_8_PLANAR, nameof(YCbCr_8_PLANAR) },
//            { YCbCr_16, nameof(YCbCr_16) },
//            { YCbCr_16_PLANAR, nameof(YCbCr_16_PLANAR) },
//            { YCbCr_16_SE, nameof(YCbCr_16_SE) },
//            { YUV_8_PLANAR, nameof(YUV_8_PLANAR) },
//            { YUV_16_PLANAR, nameof(YUV_16_PLANAR) },
//            { YUV_16_SE, nameof(YUV_16_SE) },
//            { HLS_8, nameof(HLS_8) },
//            { HLS_8_PLANAR, nameof(HLS_8_PLANAR) },
//            { HLS_16, nameof(HLS_16) },
//            { HLS_16_PLANAR, nameof(HLS_16_PLANAR) },
//            { HLS_16_SE, nameof(HLS_16_SE) },
//            { HSV_8, nameof(HSV_8) },
//            { HSV_8_PLANAR, nameof(HSV_8_PLANAR) },
//            { HSV_16, nameof(HSV_16) },
//            { HSV_16_PLANAR, nameof(HSV_16_PLANAR) },
//            { HSV_16_SE, nameof(HSV_16_SE) },

//            { XYZ_FLT, nameof(XYZ_FLT) },
//            { Lab_FLT, nameof(Lab_FLT) },
//            { GRAY_FLT, nameof(GRAY_FLT) },
//            { RGB_FLT, nameof(RGB_FLT) },
//            { BGR_FLT, nameof(BGR_FLT) },
//            { CMYK_FLT, nameof(CMYK_FLT) },
//            { LabA_FLT, nameof(LabA_FLT) },
//            { RGBA_FLT, nameof(RGBA_FLT) },
//            { ARGB_FLT, nameof(ARGB_FLT) },
//            { BGRA_FLT, nameof(BGRA_FLT) },
//            { ABGR_FLT, nameof(ABGR_FLT) },

//            { XYZ_DBL, nameof(XYZ_DBL) },
//            { Lab_DBL, nameof(Lab_DBL) },
//            { GRAY_DBL, nameof(GRAY_DBL) },
//            { RGB_DBL, nameof(RGB_DBL) },
//            { BGR_DBL, nameof(BGR_DBL) },
//            { CMYK_DBL, nameof(CMYK_DBL) },

//            { LabV2_8, nameof(LabV2_8) },
//            { ALabV2_8, nameof(ALabV2_8) },
//            { LabV2_16, nameof(LabV2_16) },

//            { GRAY_HALF_FLT, nameof(GRAY_HALF_FLT) },
//            { RGB_HALF_FLT, nameof(RGB_HALF_FLT) },
//            { CMYK_HALF_FLT, nameof(CMYK_HALF_FLT) },
//            { RGBA_HALF_FLT, nameof(RGBA_HALF_FLT) },

//            { ARGB_HALF_FLT, nameof(ARGB_HALF_FLT) },
//            { BGR_HALF_FLT, nameof(BGR_HALF_FLT) },
//            { BGRA_HALF_FLT, nameof(BGRA_HALF_FLT) },
//            { ABGR_HALF_FLT, nameof(ABGR_HALF_FLT) },
//    };

//    #endregion Fields

//    #region Public Methods

//    [TestCaseSource(nameof(ListFormats))]
//    public void FormatterCanUndoItselfTest(string _, PixelFormat type)
//    {
//        Span<byte> buffer = stackalloc byte[1024];

//        if (type.IsInt)
//        {
//            Span<ushort> values = stackalloc ushort[Helpers.maxChannels];

//            var info = new Transform();
//            info.outputFormat = info.inputFormat = type;

//            // Go back and forth
//            var f = State.GetFormattersPlugin(null).GetFormatter(type, FormatterDirection.Input, PackFlag.Ushort);
//            var b = State.GetFormattersPlugin(null).GetFormatter(type, FormatterDirection.Output, PackFlag.Ushort);

//            Assert.Multiple(() =>
//            {
//                Assert.That(f.Fmt16In, Is.Not.Null, "Missing in formatter");
//                Assert.That(f.Fmt16Out, Is.Not.Null, "Missing out formatter");
//            });

//            var nChannels = type.Channels;
//            var bytes = type.Bytes;

//            for (var j = 0; j < 5; j++)
//            {
//                for (var i = 0; i < nChannels; i++)
//                {
//                    values[i] = (ushort)(i + j);
//                    // For 8-bit
//                    if (bytes is 1)
//                        values[i] <<= 8;
//                }

//                b.Fmt16Out(info, values, buffer, 64);
//                for (var i = 0; i < values.Length; i++)
//                    values[i] = 0;
//                f.Fmt16In(info, values, buffer, 64);

//                for (var i = 0; i < nChannels; i++)
//                {
//                    if (bytes is 1)
//                        values[i] >>= 8;

//                    Assert.That(values[i], Is.EqualTo(i + j), $"i = \"{i}\"; j = \"{j}\"");
//                }
//            }
//        }
//        else if (type.IsFloat)
//        {
//            Span<float> values = stackalloc float[Helpers.maxChannels];

//            var info = new Transform();
//            info.outputFormat = info.inputFormat = type;

//            // Go back and forth
//            var f = State.GetFormattersPlugin(null).GetFormatter(type, FormatterDirection.Input, PackFlag.Float);
//            var b = State.GetFormattersPlugin(null).GetFormatter(type, FormatterDirection.Output, PackFlag.Float);

//            Assert.Multiple(() =>
//            {
//                Assert.That(f.FmtFloatIn, Is.Not.Null, "Missing in formatter");
//                Assert.That(f.FmtFloatOut, Is.Not.Null, "Missing out formatter");
//            });

//            var nChannels = type.Channels;

//            for (var j = 0; j < 5; j++)
//            {
//                for (var i = 0; i < nChannels; i++)
//                {
//                    values[i] = (ushort)(i + j);
//                }

//                b.FmtFloatOut(info, values, buffer, 64);
//                for (var i = 0; i < values.Length; i++)
//                    values[i] = 0;
//                f.FmtFloatIn(info, values, buffer, 64);

//                for (var i = 0; i < nChannels; i++)
//                    Assert.That(values[i], Is.EqualTo(i + j).Within(1e-9), $"i = \"{i}\"; j = \"{j}\"");
//            }
//        }
//        else
//        {
//            Assert.Fail("How did we get here???");
//        }
//    }

//    [Test]
//    public void ValidateHalfBehavior()
//    {
//        for (var i = 0; i < 0xFFFF; i++)
//        {
//            var f = (float)BitConverter.UInt16BitsToHalf((ushort)i);

//            if (Single.IsFinite(f))
//            {
//                var j = BitConverter.HalfToUInt16Bits((Half)f);
//                Assert.That(i, Is.EqualTo(j));
//            }
//        }
//    }

//    #endregion Public Methods

//    #region Private Methods

//    private static IEnumerable<object[]> ListFormats() =>
//        from i in _formats
//        select new object[] { i.Value, i.Key };

//    #endregion Private Methods
//}
