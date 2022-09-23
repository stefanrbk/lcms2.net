//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
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
using lcms2.types;

namespace lcms2.tests.types;

[TestFixture(TestOf = typeof(ToneCurve))]
public class ToneCurveTests
{
    #region Fields

    ToneCurve LinGamma;

    #endregion Fields

    #region Public Methods

    [Test, Category(RandomTest)]
    public void CheckGammaCreation16(
        [Random(0, 0xFFFF, 20)]
            int i)
    {
        var @in = (ushort)i;
        var @out = LinGamma.Eval(@in);
        Assert.That(@out, Is.EqualTo(@in));
    }

    [Test, Category(RandomTest)]
    public void CheckGammaCreationFloat(
        [Random(0, 0xFFFF, 20)]
            int i)
    {
        var @in = i / 65535f;
        var @out = LinGamma.Eval(@in);
        Assert.That(@out, Is.EqualTo(@in).Within(1 / 65535f));
    }

    [Test, Category(FixedTest)]
    public void CheckGammaEstimation10()
    {
        var est = LinGamma.EstimateGamma(1E-3);
        Assert.That(est, Is.EqualTo(1.0).Within(1E-3));
    }

    [SetUp]
    public void SetUp()
    {
        var linGamma = ToneCurve.BuildGamma(null, 1.0);
        Assert.That(linGamma, Is.Not.Null);
        LinGamma = linGamma;
    }

    [TearDown]
    public void TearDown() =>
        LinGamma.Dispose();

    #endregion Public Methods
}
