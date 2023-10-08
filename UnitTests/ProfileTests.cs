using Castle.Components.DictionaryAdapter.Xml;

using lcms2.types;

namespace lcms2.tests;
public class ProfileTests
{
    [TestCase(TestName = "Validate sRGB Profile")]
    public void Validate_sRGB_Profile()
    {
        var now = DateTime.UtcNow;
        now = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        Assert.That(sRGBProfile, Is.Not.Null);

        var h = cmsOpenProfileFromMem(sRGBProfile);

        Assert.That(h, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(h.Created, Is.GreaterThanOrEqualTo(TestStart).And.LessThanOrEqualTo(now));
            Assert.That(cmsGetProfileVersion(h), Is.EqualTo(4.3));
            Assert.That(cmsGetDeviceClass(h), Is.EqualTo((Signature)cmsSigDisplayClass));
            Assert.That(cmsGetColorSpace(h), Is.EqualTo((Signature)cmsSigRgbData));
            Assert.That(cmsGetPCS(h), Is.EqualTo((Signature)cmsSigXYZData));
            Assert.That(cmsGetHeaderRenderingIntent(h), Is.EqualTo(INTENT_PERCEPTUAL));
        });
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                var desc = cmsReadTag(h, cmsSigProfileDescriptionTag) as Mlu;
                var buf = new char[50];

                Assert.That(desc, Is.Not.Null);
                cmsMLUgetWide(desc, "en"u8, "US"u8, buf);
                var strDesc = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strDesc, Does.Contain("sRGB"));
            });

            Assert.Multiple(() =>
            {
                var copy = cmsReadTag(h, cmsSigCopyrightTag) as Mlu;
                var buf = new char[50];

                Assert.That(copy, Is.Not.Null);
                cmsMLUgetWide(copy, "en"u8, "US"u8, buf);
                var strCopy = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strCopy, Does.Contain("No copyright"));
            });

            Assert.Multiple(() =>
            {
                var wp = cmsReadTag(h, cmsSigMediaWhitePointTag) as Box<CIEXYZ>;
                Assert.That(wp, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(wp!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3feedac000000000)));
                    Assert.That(wp!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));
                    Assert.That(wp!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fea65a000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var chad = cmsReadTag(h, cmsSigChromaticAdaptationTag) as double[];
                Assert.That(chad, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(chad![0], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0c42000000000)));
                    Assert.That(chad![1], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3f97780000000000)));
                    Assert.That(chad![2], Is.EqualTo(BitConverter.UInt64BitsToDouble(0xbfa9b60000000000)));
                    Assert.That(chad![3], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3f9e4c0000000000)));
                    Assert.That(chad![4], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fefb20000000000)));
                    Assert.That(chad![5], Is.EqualTo(BitConverter.UInt64BitsToDouble(0xbf917c0000000000)));
                    Assert.That(chad![6], Is.EqualTo(BitConverter.UInt64BitsToDouble(0xbf82f00000000000)));
                    Assert.That(chad![7], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3f8ee00000000000)));
                    Assert.That(chad![8], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe80dc000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var red = cmsReadTag(h, cmsSigRedColorantTag) as Box<CIEXYZ>;
                Assert.That(red, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(red!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fdbe80000000000)));
                    Assert.That(red!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fcc7a8000000000)));
                    Assert.That(red!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3f8c800000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var green = cmsReadTag(h, cmsSigGreenColorantTag) as Box<CIEXYZ>;
                Assert.That(green, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(green!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fd8a5c000000000)));
                    Assert.That(green!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe6f0e000000000)));
                    Assert.That(green!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fb8d90000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var blue = cmsReadTag(h, cmsSigBlueColorantTag) as Box<CIEXYZ>;
                Assert.That(blue, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(blue!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fc24f8000000000)));
                    Assert.That(blue!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3faf080000000000)));
                    Assert.That(blue!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe6d86000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var trc = cmsReadTag(h, cmsSigRedTRCTag) as ToneCurve;
                Assert.That(trc, Is.Not.Null);

                Assert.That(cmsReadTag(h, cmsSigGreenTRCTag), Is.EqualTo(trc));
                Assert.That(cmsReadTag(h, cmsSigBlueTRCTag), Is.EqualTo(trc));

                Assert.Multiple(() =>
                {
                    Assert.That(trc!.nSegments, Is.EqualTo(1));
                    Assert.That(trc!.nEntries, Is.EqualTo(4096));
                    Assert.That(trc!.Segments![0].x0, Is.EqualTo(BitConverter.UInt64BitsToDouble(0xc480f0cf00000000)));
                    Assert.That(trc!.Segments![0].x1, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4480f0cf00000000)));
                    Assert.That(trc!.Segments![0].Type, Is.EqualTo(4));
                    Assert.That(trc!.Segments![0].Params[0], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4003333000000000)));
                    Assert.That(trc!.Segments![0].Params[1], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fee54e000000000)));
                    Assert.That(trc!.Segments![0].Params[2], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3faab20000000000)));
                    Assert.That(trc!.Segments![0].Params[3], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fb3d00000000000)));
                    Assert.That(trc!.Segments![0].Params[4], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fa4b60000000000)));
                    Assert.That(trc!.Table16![0], Is.EqualTo(0x0000));
                    Assert.That(trc!.Table16![898], Is.EqualTo(0x0a19));
                    Assert.That(trc!.Table16![1981], Is.EqualTo(0x3306));
                    Assert.That(trc!.Table16![2048], Is.EqualTo(0x36d3));
                    Assert.That(trc!.Table16![2720], Is.EqualTo(0x6612));
                    Assert.That(trc!.Table16![3951], Is.EqualTo(0xebfe));
                    Assert.That(trc!.Table16![4095], Is.EqualTo(0xffff));
                });
            });

            Assert.Multiple(() =>
            {
                var chroma = cmsReadTag(h, cmsSigChromaticityTag) as Box<CIExyYTRIPLE>;
                Assert.That(chroma, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(chroma!.Value.Red.x, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe47ae000000000)));
                    Assert.That(chroma!.Value.Red.y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fd51ec000000000)));
                    Assert.That(chroma!.Value.Red.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));

                    Assert.That(chroma!.Value.Green.x, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fd3334000000000)));
                    Assert.That(chroma!.Value.Green.y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe3334000000000)));
                    Assert.That(chroma!.Value.Green.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));

                    Assert.That(chroma!.Value.Blue.x, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fc3330000000000)));
                    Assert.That(chroma!.Value.Blue.y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3faeb80000000000)));
                    Assert.That(chroma!.Value.Blue.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));
                });
            });
        });

    }

    [TestCase(TestName = "Validate aRGB Profile")]
    public void Validate_aRGB_Profile()
    {
        var now = DateTime.UtcNow;
        now = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        Assert.That(aRGBProfile, Is.Not.Null);

        var h = cmsOpenProfileFromMem(aRGBProfile);

        Assert.That(h, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(h.Created, Is.GreaterThanOrEqualTo(TestStart).And.LessThanOrEqualTo(now));
            Assert.That(cmsGetProfileVersion(h), Is.EqualTo(4.3));
            Assert.That(cmsGetDeviceClass(h), Is.EqualTo((Signature)cmsSigDisplayClass));
            Assert.That(cmsGetColorSpace(h), Is.EqualTo((Signature)cmsSigRgbData));
            Assert.That(cmsGetPCS(h), Is.EqualTo((Signature)cmsSigXYZData));
            Assert.That(cmsGetHeaderRenderingIntent(h), Is.EqualTo(INTENT_PERCEPTUAL));
        });
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                var desc = cmsReadTag(h, cmsSigProfileDescriptionTag) as Mlu;
                var buf = new char[50];

                Assert.That(desc, Is.Not.Null);
                cmsMLUgetWide(desc, "en"u8, "US"u8, buf);
                var strDesc = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strDesc, Does.Contain("RGB"));
            });

            Assert.Multiple(() =>
            {
                var copy = cmsReadTag(h, cmsSigCopyrightTag) as Mlu;
                var buf = new char[50];

                Assert.That(copy, Is.Not.Null);
                cmsMLUgetWide(copy, "en"u8, "US"u8, buf);
                var strCopy = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strCopy, Does.Contain("No copyright"));
            });

            Assert.Multiple(() =>
            {
                var wp = cmsReadTag(h, cmsSigMediaWhitePointTag) as Box<CIEXYZ>;
                Assert.That(wp, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(wp!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3feedac000000000)));
                    Assert.That(wp!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));
                    Assert.That(wp!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fea65a000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var chad = cmsReadTag(h, cmsSigChromaticAdaptationTag) as double[];
                Assert.That(chad, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(chad![0], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0c4a000000000)));
                    Assert.That(chad![1], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3f978c0000000000)));
                    Assert.That(chad![2], Is.EqualTo(BitConverter.UInt64BitsToDouble(0xbfa9ac0000000000)));
                    Assert.That(chad![3], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3f9e6c0000000000)));
                    Assert.That(chad![4], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fefb0e000000000)));
                    Assert.That(chad![5], Is.EqualTo(BitConverter.UInt64BitsToDouble(0xbf91780000000000)));
                    Assert.That(chad![6], Is.EqualTo(BitConverter.UInt64BitsToDouble(0xbf82e80000000000)));
                    Assert.That(chad![7], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3f8ec00000000000)));
                    Assert.That(chad![8], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe8128000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var red = cmsReadTag(h, cmsSigRedColorantTag) as Box<CIEXYZ>;
                Assert.That(red, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(red!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe3822000000000)));
                    Assert.That(red!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fd3e80000000000)));
                    Assert.That(red!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3f93f00000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var green = cmsReadTag(h, cmsSigGreenColorantTag) as Box<CIEXYZ>;
                Assert.That(green, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(green!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fca4a8000000000)));
                    Assert.That(green!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe4064000000000)));
                    Assert.That(green!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3faf2e0000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var blue = cmsReadTag(h, cmsSigBlueColorantTag) as Box<CIEXYZ>;
                Assert.That(blue, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(blue!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fc3180000000000)));
                    Assert.That(blue!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fb02d0000000000)));
                    Assert.That(blue!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe7d32000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var trc = cmsReadTag(h, cmsSigRedTRCTag) as ToneCurve;
                Assert.That(trc, Is.Not.Null);

                Assert.That(cmsReadTag(h, cmsSigGreenTRCTag), Is.EqualTo(trc));
                Assert.That(cmsReadTag(h, cmsSigBlueTRCTag), Is.EqualTo(trc));

                Assert.Multiple(() =>
                {
                    Assert.That(trc!.nSegments, Is.EqualTo(1));
                    Assert.That(trc!.nEntries, Is.EqualTo(4096));
                    Assert.That(trc!.Segments![0].x0, Is.EqualTo(BitConverter.UInt64BitsToDouble(0xc480f0cf00000000)));
                    Assert.That(trc!.Segments![0].x1, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4480f0cf00000000)));
                    Assert.That(trc!.Segments![0].Type, Is.EqualTo(1));
                    Assert.That(trc!.Segments![0].Params[0], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4001980000000000)));
                    Assert.That(trc!.Table16![0], Is.EqualTo(0x0000));
                    Assert.That(trc!.Table16![898], Is.EqualTo(0x0919));
                    Assert.That(trc!.Table16![1981], Is.EqualTo(0x33d7));
                    Assert.That(trc!.Table16![2048], Is.EqualTo(0x37c6));
                    Assert.That(trc!.Table16![2720], Is.EqualTo(0x681a));
                    Assert.That(trc!.Table16![3951], Is.EqualTo(0xec9d));
                    Assert.That(trc!.Table16![4095], Is.EqualTo(0xffff));
                });
            });

            Assert.Multiple(() =>
            {
                var chroma = cmsReadTag(h, cmsSigChromaticityTag) as Box<CIExyYTRIPLE>;
                Assert.That(chroma, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(chroma!.Value.Red.x, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe47ae000000000)));
                    Assert.That(chroma!.Value.Red.y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fd51ec000000000)));
                    Assert.That(chroma!.Value.Red.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));

                    Assert.That(chroma!.Value.Green.x, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fcae18000000000)));
                    Assert.That(chroma!.Value.Green.y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fe6b86000000000)));
                    Assert.That(chroma!.Value.Green.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));

                    Assert.That(chroma!.Value.Blue.x, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fc3330000000000)));
                    Assert.That(chroma!.Value.Blue.y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3faeb80000000000)));
                    Assert.That(chroma!.Value.Blue.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));
                });
            });
        });

    }

    [TestCase(TestName = "Validate Gray Profile (2.2 Gamma)")]
    public void ValidateGrayProfile()
    {
        var now = DateTime.UtcNow;
        now = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        Assert.That(GrayProfile, Is.Not.Null);

        var h = cmsOpenProfileFromMem(GrayProfile);

        Assert.That(h, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(h.Created, Is.GreaterThanOrEqualTo(TestStart).And.LessThanOrEqualTo(now));
            Assert.That(cmsGetProfileVersion(h), Is.EqualTo(4.3));
            Assert.That(cmsGetDeviceClass(h), Is.EqualTo((Signature)cmsSigDisplayClass));
            Assert.That(cmsGetColorSpace(h), Is.EqualTo((Signature)cmsSigGrayData));
            Assert.That(cmsGetPCS(h), Is.EqualTo((Signature)cmsSigXYZData));
            Assert.That(cmsGetHeaderRenderingIntent(h), Is.EqualTo(INTENT_PERCEPTUAL));
        });
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                var desc = cmsReadTag(h, cmsSigProfileDescriptionTag) as Mlu;
                var buf = new char[50];

                Assert.That(desc, Is.Not.Null);
                cmsMLUgetWide(desc, "en"u8, "US"u8, buf);
                var strDesc = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strDesc, Does.Contain("gray"));
            });

            Assert.Multiple(() =>
            {
                var copy = cmsReadTag(h, cmsSigCopyrightTag) as Mlu;
                var buf = new char[50];

                Assert.That(copy, Is.Not.Null);
                cmsMLUgetWide(copy, "en"u8, "US"u8, buf);
                var strCopy = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strCopy, Does.Contain("No copyright"));
            });

            Assert.Multiple(() =>
            {
                var wp = cmsReadTag(h, cmsSigMediaWhitePointTag) as Box<CIEXYZ>;
                Assert.That(wp, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(wp!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3feedac000000000)));
                    Assert.That(wp!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));
                    Assert.That(wp!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fea65a000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var trc = cmsReadTag(h, cmsSigGrayTRCTag) as ToneCurve;
                Assert.That(trc, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(trc!.nSegments, Is.EqualTo(1));
                    Assert.That(trc!.nEntries, Is.EqualTo(4096));
                    Assert.That(trc!.Segments![0].x0, Is.EqualTo(BitConverter.UInt64BitsToDouble(0xc480f0cf00000000)));
                    Assert.That(trc!.Segments![0].x1, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4480f0cf00000000)));
                    Assert.That(trc!.Segments![0].Type, Is.EqualTo(1));
                    Assert.That(trc!.Segments![0].Params[0], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4001999800000000)));
                    Assert.That(trc!.Table16![0], Is.EqualTo(0x0000));
                    Assert.That(trc!.Table16![898], Is.EqualTo(0x0917));
                    Assert.That(trc!.Table16![1981], Is.EqualTo(0x33d0));
                    Assert.That(trc!.Table16![2048], Is.EqualTo(0x37bf));
                    Assert.That(trc!.Table16![2720], Is.EqualTo(0x6812));
                    Assert.That(trc!.Table16![3951], Is.EqualTo(0xec9c));
                    Assert.That(trc!.Table16![4095], Is.EqualTo(0xffff));
                });
            });
        });

    }

    [TestCase(TestName = "Validate Gray Profile (3.0 Gamma)")]
    public void ValidateGray3Profile()
    {
        var now = DateTime.UtcNow;
        now = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        Assert.That(Gray3Profile, Is.Not.Null);

        var h = cmsOpenProfileFromMem(Gray3Profile);

        Assert.That(h, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(h.Created, Is.GreaterThanOrEqualTo(TestStart).And.LessThanOrEqualTo(now));
            Assert.That(cmsGetProfileVersion(h), Is.EqualTo(4.3));
            Assert.That(cmsGetDeviceClass(h), Is.EqualTo((Signature)cmsSigDisplayClass));
            Assert.That(cmsGetColorSpace(h), Is.EqualTo((Signature)cmsSigGrayData));
            Assert.That(cmsGetPCS(h), Is.EqualTo((Signature)cmsSigXYZData));
            Assert.That(cmsGetHeaderRenderingIntent(h), Is.EqualTo(INTENT_PERCEPTUAL));
        });
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                var desc = cmsReadTag(h, cmsSigProfileDescriptionTag) as Mlu;
                var buf = new char[50];

                Assert.That(desc, Is.Not.Null);
                cmsMLUgetWide(desc, "en"u8, "US"u8, buf);
                var strDesc = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strDesc, Does.Contain("gray"));
            });

            Assert.Multiple(() =>
            {
                var copy = cmsReadTag(h, cmsSigCopyrightTag) as Mlu;
                var buf = new char[50];

                Assert.That(copy, Is.Not.Null);
                cmsMLUgetWide(copy, "en"u8, "US"u8, buf);
                var strCopy = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strCopy, Does.Contain("No copyright"));
            });

            Assert.Multiple(() =>
            {
                var wp = cmsReadTag(h, cmsSigMediaWhitePointTag) as Box<CIEXYZ>;
                Assert.That(wp, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(wp!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3feedac000000000)));
                    Assert.That(wp!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));
                    Assert.That(wp!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fea65a000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var trc = cmsReadTag(h, cmsSigGrayTRCTag) as ToneCurve;
                Assert.That(trc, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(trc!.nSegments, Is.EqualTo(1));
                    Assert.That(trc!.nEntries, Is.EqualTo(4096));
                    Assert.That(trc!.Segments![0].x0, Is.EqualTo(BitConverter.UInt64BitsToDouble(0xc480f0cf00000000)));
                    Assert.That(trc!.Segments![0].x1, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4480f0cf00000000)));
                    Assert.That(trc!.Segments![0].Type, Is.EqualTo(1));
                    Assert.That(trc!.Segments![0].Params[0], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4008000000000000)));
                    Assert.That(trc!.Table16![0], Is.EqualTo(0x0000));
                    Assert.That(trc!.Table16![898], Is.EqualTo(0x02b3));
                    Assert.That(trc!.Table16![1981], Is.EqualTo(0x1cfb));
                    Assert.That(trc!.Table16![2048], Is.EqualTo(0x2006));
                    Assert.That(trc!.Table16![2720], Is.EqualTo(0x4b05));
                    Assert.That(trc!.Table16![3951], Is.EqualTo(0xe5ee));
                    Assert.That(trc!.Table16![4095], Is.EqualTo(0xffff));
                });
            });
        });
    }

    [TestCase(TestName = "Validate Gray Profile (Lab PCS)")]
    public void ValidateGrayLabProfile()
    {
        var now = DateTime.UtcNow;
        now = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        Assert.That(GrayProfile, Is.Not.Null);

        var h = cmsOpenProfileFromMem(GrayLabProfile);

        Assert.That(h, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(h.Created, Is.GreaterThanOrEqualTo(TestStart).And.LessThanOrEqualTo(now));
            Assert.That(cmsGetProfileVersion(h), Is.EqualTo(4.3));
            Assert.That(cmsGetDeviceClass(h), Is.EqualTo((Signature)cmsSigDisplayClass));
            Assert.That(cmsGetColorSpace(h), Is.EqualTo((Signature)cmsSigGrayData));
            Assert.That(cmsGetPCS(h), Is.EqualTo((Signature)cmsSigLabData));
            Assert.That(cmsGetHeaderRenderingIntent(h), Is.EqualTo(INTENT_PERCEPTUAL));
        });
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                var desc = cmsReadTag(h, cmsSigProfileDescriptionTag) as Mlu;
                var buf = new char[50];

                Assert.That(desc, Is.Not.Null);
                cmsMLUgetWide(desc, "en"u8, "US"u8, buf);
                var strDesc = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strDesc, Does.Contain("gray"));
            });

            Assert.Multiple(() =>
            {
                var copy = cmsReadTag(h, cmsSigCopyrightTag) as Mlu;
                var buf = new char[50];

                Assert.That(copy, Is.Not.Null);
                cmsMLUgetWide(copy, "en"u8, "US"u8, buf);
                var strCopy = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strCopy, Does.Contain("No copyright"));
            });

            Assert.Multiple(() =>
            {
                var wp = cmsReadTag(h, cmsSigMediaWhitePointTag) as Box<CIEXYZ>;
                Assert.That(wp, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(wp!.Value.X, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3feedac000000000)));
                    Assert.That(wp!.Value.Y, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));
                    Assert.That(wp!.Value.Z, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3fea65a000000000)));
                });
            });

            Assert.Multiple(() =>
            {
                var trc = cmsReadTag(h, cmsSigGrayTRCTag) as ToneCurve;
                Assert.That(trc, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(trc!.nSegments, Is.EqualTo(1));
                    Assert.That(trc!.nEntries, Is.EqualTo(2));
                    Assert.That(trc!.Segments![0].x0, Is.EqualTo(BitConverter.UInt64BitsToDouble(0xc480f0cf00000000)));
                    Assert.That(trc!.Segments![0].x1, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4480f0cf00000000)));
                    Assert.That(trc!.Segments![0].Type, Is.EqualTo(1));
                    Assert.That(trc!.Segments![0].Params[0], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x3ff0000000000000)));
                    Assert.That(trc!.Table16![0], Is.EqualTo(0x0000));
                    Assert.That(trc!.Table16![1], Is.EqualTo(0xffff));
                });
            });
        });
    }
    [TestCase(TestName = "Validate Linearization Device Link Profile")]
    public void ValidateLinearizationDeviceLinkProfile()
    {
        var now = DateTime.UtcNow;
        now = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        Assert.That(LinProfile, Is.Not.Null);

        var h = cmsOpenProfileFromMem(LinProfile);

        Assert.That(h, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(h.Created, Is.GreaterThanOrEqualTo(TestStart).And.LessThanOrEqualTo(now));
            Assert.That(cmsGetProfileVersion(h), Is.EqualTo(4.3));
            Assert.That(cmsGetDeviceClass(h), Is.EqualTo((Signature)cmsSigLinkClass));
            Assert.That(cmsGetColorSpace(h), Is.EqualTo((Signature)cmsSigCmykData));
            Assert.That(cmsGetPCS(h), Is.EqualTo((Signature)cmsSigCmykData));
            Assert.That(cmsGetHeaderRenderingIntent(h), Is.EqualTo(INTENT_PERCEPTUAL));
        });
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                var desc = cmsReadTag(h, cmsSigProfileDescriptionTag) as Mlu;
                var buf = new char[50];

                Assert.That(desc, Is.Not.Null);
                cmsMLUgetWide(desc, "en"u8, "US"u8, buf);
                var strDesc = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strDesc, Does.Contain("Linearization"));
            });

            Assert.Multiple(() =>
            {
                var copy = cmsReadTag(h, cmsSigCopyrightTag) as Mlu;
                var buf = new char[50];

                Assert.That(copy, Is.Not.Null);
                cmsMLUgetWide(copy, "en"u8, "US"u8, buf);
                var strCopy = new string(buf.AsSpan(..Array.FindIndex(buf, i => i == '\0')));
                Assert.That(strCopy, Does.Contain("No copyright"));
            });

            Assert.Multiple(() =>
            {
                var pipeline = cmsReadTag(h, cmsSigAToB0Tag) as Pipeline;
                Assert.That(pipeline, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(pipeline!.InputChannels, Is.EqualTo(4));
                    Assert.That(pipeline!.OutputChannels, Is.EqualTo(4));

                    var element = pipeline!.Elements!;
                    Assert.That(element, Is.Not.Null);

                    var curve = ((StageToneCurvesData)element!.Data!).TheCurves[0];
                    Assert.That(curve.nSegments, Is.EqualTo(1));
                    Assert.That(curve.Segments![0].x0, Is.EqualTo(BitConverter.UInt64BitsToDouble(0xc480f0cf00000000)));
                    Assert.That(curve.Segments![0].x1, Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4480f0cf00000000)));
                    Assert.That(curve.Segments![0].Type, Is.EqualTo(1));
                    Assert.That(curve.Segments![0].Params[0], Is.EqualTo(BitConverter.UInt64BitsToDouble(0x4008000000000000)));
                    Assert.That(curve.Table16![0], Is.EqualTo(0x0000));
                    Assert.That(curve.Table16![898], Is.EqualTo(0x02b3));
                    Assert.That(curve.Table16![1981], Is.EqualTo(0x1cfb));
                    Assert.That(curve.Table16![2048], Is.EqualTo(0x2006));
                    Assert.That(curve.Table16![2720], Is.EqualTo(0x4b05));
                    Assert.That(curve.Table16![3951], Is.EqualTo(0xe5ee));
                    Assert.That(curve.Table16![4095], Is.EqualTo(0xffff));

                    Assert.That(element.Next, Is.Null);
                });
            });

            Assert.Multiple(() =>
            {
                var seq = cmsReadTag(h, cmsSigProfileSequenceDescTag) as Sequence;
                Assert.That(seq, Is.Not.Null);

                Assert.That(seq!.n, Is.EqualTo(1));

                Assert.Multiple(() =>
                {
                });
            });

            Assert.Multiple(() =>
            {
                var id = cmsReadTag(h, cmsSigProfileSequenceIdTag) as Sequence;
                Assert.That(id, Is.Not.Null);

                Assert.Multiple(() =>
                {
                });
            });
        });

    }
}
