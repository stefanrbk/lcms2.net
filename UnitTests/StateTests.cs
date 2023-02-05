//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
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
//
using lcms2.state;

namespace lcms2.tests;

[TestFixture(TestOf = typeof(Context))]
public class StateTests : TestBase
{
    #region Public Methods

    [Test]
    public void SetLogErrorHandlerSetsLogger()
    {
        var logger = Substitute.For<LogErrorHandler>();

        cmsSignalError(null, ErrorCode.Undefined, "This isn't logged.");
        cmsSetLogErrorHandler(logger);
        cmsSignalError(null, ErrorCode.Undefined, "This does get logged.");

        Assert.That(logger.ReceivedCalls().Count(), Is.EqualTo(1));
    }

    [Test]
    public void SetLogErrorHandlerSetsLoggerForProvidedState()
    {
        var state1 = cmsCreateContext(null, null);
        var state2 = cmsCreateContext(null, null);

        var logger1 = Substitute.For<LogErrorHandler>();
        var logger2 = Substitute.For<LogErrorHandler>();

        cmsSetLogErrorHandlerTHR(state1, logger1);
        cmsSetLogErrorHandlerTHR(state2, logger2);

        cmsSignalError(state1, ErrorCode.Undefined, "This is logger1.");
        cmsSignalError(state2, ErrorCode.Undefined, "This is logger2.");

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

        cmsDeleteContext(state1);
        cmsDeleteContext(state2);
    }

    #endregion Public Methods
}
