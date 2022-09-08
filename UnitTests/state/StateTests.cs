using lcms2.state;

using static lcms2.state.State;

namespace lcms2.tests.state;

[TestFixture(TestOf = typeof(State))]
public class StateTests
{
    [Test]
    public void SetLogErrorHandlerSetsLogger()
    {
        var logger = Substitute.For<LogErrorHandlerFunction>();

        SignalError(null, ErrorCode.Undefined, "This isn't logged.");
        SetLogErrorHandler(logger);
        SignalError(null, ErrorCode.Undefined, "This does get logged.");

        Assert.That(logger.ReceivedCalls().Count(), Is.EqualTo(1));
    }
    [Test]
    public void SetLogErrorHandlerSetsLoggerForProvidedState()
    {
        var state1 = CreateStateContainer();
        var state2 = CreateStateContainer();

        var logger1 = Substitute.For<LogErrorHandlerFunction>();
        var logger2 = Substitute.For<LogErrorHandlerFunction>();

        SetLogErrorHandler(state1, logger1);
        SetLogErrorHandler(state2, logger2);

        SignalError(state1, ErrorCode.Undefined, "This is logger1.");
        SignalError(state2, ErrorCode.Undefined, "This is logger2.");

        Assert.Multiple(() =>
        {
            Assert.That(logger1.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(logger2.ReceivedCalls().Count(), Is.EqualTo(1));
        });

        Assert.Multiple(() =>
        {
            Assert.That((string?)logger1.ReceivedCalls().First().GetArguments()[2], Does.Contain("logger1"));
            Assert.That((string?)logger2.ReceivedCalls().First().GetArguments()[2], Does.Contain("logger2"));
        });

        DeleteStateContainer(state1);
        DeleteStateContainer(state2);
    }
}
