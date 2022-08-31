using lcms2.types;

namespace lcms2.testing;

[TestFixture(TestOf = typeof(Stage))]
public class MpeTests
{
    private readonly object? _state;
    Stage _identity;

    public MpeTests() =>
        _state = null;

    public MpeTests(object? state)
    {
        _state = state;
    }

    [SetUp]
    public void Setup()
    {
        _identity = Stage.AllocIdentityCLut(_state, 3)!;
    }

    [TearDown]
    public void TearDown()
    {
        _identity.Dispose();
    }

    [Test]
    public void SetInputChannelsGreaterThanMaxStageChannelsIgnoresChange()
    {
        Assert.That(_identity.InputChannels, Is.EqualTo(3), "Invalid start condition!");

        _identity.InputChannels = 4;
        Assert.That(_identity.InputChannels, Is.EqualTo(4), "InputChannel was not assigned!");

        _identity.InputChannels = 129;
        Assert.That(_identity.InputChannels, Is.EqualTo(4), "InputChannel didn't ignore invalid value!");
    }

    [Test]
    public void SetOutputChannelsGreaterThanMaxStageChannelsIgnoresChange()
    {
        Assert.That(_identity.OutputChannels, Is.EqualTo(3), "Invalid start condition!");

        _identity.OutputChannels = 4;
        Assert.That(_identity.OutputChannels, Is.EqualTo(4), "OutputChannel was not assigned!");

        _identity.OutputChannels = 129;
        Assert.That(_identity.OutputChannels, Is.EqualTo(4), "OutputChannel didn't ignore invalid value!");
    }

    [Test]
    public void AccessingCurveSetOnNonToneCurveStageThrowsInvalidOperationException() =>
        Assert.Throws<InvalidOperationException>(() => _identity.CurveSet[0].evals = null!);
}
