using lcms2.state;

using static lcms2.testing.Utils;

namespace lcms2.testing;

public class ContextTests
{
    [Test]
    public void TestSimpleContext()
    {
        object a = 1;
        object b = 32;

        var c1 = Context.Create(null, a);

        // Let's check duplication
        var c2 = Context.Duplicate(c1, null);
        var c3 = Context.Duplicate(c2, null);

        // User data should have been propagated
        Assert.That(a, Is.EqualTo(Context.GetUserData(c3)), "Creation of user data failed");

        Context.Delete(c1);
        Context.Delete(c2);
        Context.Delete(c3);

        c1 = Context.Create(null, a);

        c2 = Context.Duplicate(c1, null);
        c3 = Context.Duplicate(c2, b);

        // New user data should be applied to c3
        Assert.Multiple(() =>
        {
            Assert.That(Context.GetUserData(c1), Is.EqualTo(a));
            Assert.That(Context.GetUserData(c2), Is.EqualTo(a));
            Assert.That(Context.GetUserData(c3), Is.EqualTo(b));
        });
    }

    [Test]
    public void TestAlarmCodes()
    {
        var codes = new ushort[] {0x0000, 0x1111, 0x2222, 0x3333, 0x4444, 0x5555, 0x6666, 0x7777, 0x8888, 0x9999, 0xaaaa, 0xbbbb, 0xcccc, 0xdddd, 0xeeee, 0xffff};

        var c1 = Context.Create(null, null);
        Context.SetAlarmCodes(c1, codes);
        var c2 = Context.Duplicate(c1, null);
        var c3 = Context.Duplicate(c2, null);

        var values = Context.GetAlarmCodes(c3);

        Assert.Multiple(() =>
        {
            for (var i = 0; i < 16; i++)
                Assert.That(values[i], Is.EqualTo(codes[i]));
        });
    }

    [Test]
    public void TestAdaptionStateContext()
    {
        var old1 = Context.SetAdaptionState(null, -1);

        var c1 = Context.Create(null, null);

        Context.SetAdaptionState(c1, 0.7);

        var c2 = Context.Duplicate(c1, null);
        var c3 = Context.Duplicate(c2, null);


        var old2 = Context.SetAdaptionState(null, -1);

        Assert.Multiple(() =>
        {
            IsGoodDouble("Adaption state", Context.SetAdaptionState(c3, -1), 0.7, 0.001);
            Assert.That(old1, Is.EqualTo(old2), "Adaptation state has changed");
        });
    }
}
