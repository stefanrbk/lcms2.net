using lcms2.types;

namespace lcms2.testing;
public class ColorspaceTests: ITest
{
    [Test, Sequential]
    public void CheckLab2LCh([Range(-128, 120, 8)] int bFrom,
                             [Range(-120, 128, 8)] int bTo) =>
        Assert.Multiple(() =>
        {
            ClearAssert();

            for (var l = 0; l < 100; l += 10)
            {
                for (var a = -128; a < 128; a += 8)
                {
                    for (var b = bFrom; b < bTo; b += 8)
                    {
                        var lab = new Lab(l, a, b);
                        var lch = (LCh)lab;
                        var lab2 = (Lab)lch;

                        var dist = DeltaE(lab, lab2);
                        Assert.That(dist, Is.LessThan(1E-12));
                    }
                }
            }
        });

    [Test, Sequential]
    public void CheckLab2XYZ([Range(-128, 120, 8)] int bFrom,
                             [Range(-120, 128, 8)] int bTo) =>
        Assert.Multiple(() =>
        {
            ClearAssert();

            for (var l = 0; l < 100; l += 10)
            {
                for (var a = -128; a < 128; a += 8)
                {
                    for (var b = bFrom; b < bTo; b += 8)
                    {
                        var lab = new Lab(l, a, b);
                        var xyz = (XYZ)lab;
                        var lab2 = (Lab)xyz;

                        var dist = DeltaE(lab, lab2);
                        Assert.That(dist, Is.LessThan(1E-12));
                    }
                }
            }
        });

    [Test, Sequential]
    public void CheckLab2xyY([Range(-128, 120, 8)] int bFrom,
                             [Range(-120, 128, 8)] int bTo) =>
        Assert.Multiple(() =>
        {
            ClearAssert();

            for (var l = 0; l < 100; l += 10)
            {
                for (var a = -128; a < 128; a += 8)
                {
                    for (var b = bFrom; b < bTo; b += 8)
                    {
                        var lab = new Lab(l, a, b);
                        var xyz = (XYZ)lab;
                        var xyy = (xyY)xyz;
                        xyz = (XYZ)xyy;
                        var lab2 = (Lab)xyz;

                        var dist = DeltaE(lab, lab2);
                        Assert.That(dist, Is.LessThan(1E-12));
                    }
                }
            }
        });

    [Test, Sequential]
    public void CheckLabV2EncodingTest([Range(0, 64512, 1024)] int from,
                                       [Range(1024, 65536, 1024)] int to)
    {
        for (var j = from; j < to; j++)
        {
            var aw1 = new LabEncodedV2((ushort)j, (ushort)j, (ushort)j);
            var lab = (Lab)aw1;
            var aw2 = (LabEncodedV2)lab;

            Assert.That(aw2, Is.EqualTo(aw1));
        }
    }

    [Test, Sequential]
    public void CheckLabV4EncodingTest([Range(0, 64512, 1024)] int from,
                                       [Range(1024, 65536, 1024)] int to)
    {
        for (var j = from; j < to; j++)
        {
            var aw1 = new LabEncoded((ushort)j, (ushort)j, (ushort)j);
            var lab = (Lab)aw1;
            var aw2 = (LabEncoded)lab;

            Assert.That(aw2, Is.EqualTo(aw1));
        }
    }

    void ITest.Setup() { }
    void ITest.Teardown() { }
}
