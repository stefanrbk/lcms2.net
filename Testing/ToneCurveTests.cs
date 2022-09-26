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

namespace lcms2.testbed;

public static class ToneCurveTests
{
    #region Public Methods

    public static bool CheckGamma18() =>
        CheckGamma(1.8);

    public static bool CheckGamma18Table() =>
        CheckGammaFloatTable(1.8);

    public static bool CheckGamma18TableWord() =>
        CheckGammaWordTable(1.8);

    public static bool CheckGamma22() =>
            CheckGamma(2.2);

    public static bool CheckGamma22Table() =>
        CheckGammaFloatTable(2.2);

    public static bool CheckGamma22TableWord() =>
        CheckGammaWordTable(2.2);

    public static bool CheckGamma30() =>
            CheckGamma(3.0);

    public static bool CheckGamma30Table() =>
        CheckGammaFloatTable(3.0);

    public static bool CheckGamma30TableWord() =>
        CheckGammaWordTable(3.0);

    public static bool CheckGammaCreation16()
    {
        var linGamma = ToneCurve.BuildGamma(null, 1.0);
        if (linGamma is null) return false;

        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = (ushort)i;
            var @out = linGamma.Eval(@in);
            if (@in != @out)
            {
                Fail($"(lin gamma): Must be {@in}, but was {@out} : ");
                linGamma.Dispose();
                return false;
            }
        }

        if (!CheckGammaEstimation(linGamma, 1.0)) return false;

        linGamma.Dispose();
        return true;
    }

    public static bool CheckGammaCreationFloat()
    {
        var linGamma = ToneCurve.BuildGamma(null, 1.0);
        if (linGamma is null) return false;

        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = i / 65535f;
            var @out = linGamma.Eval(@in);
            if (Math.Abs(@in - @out) > (1 / 65535f))
            {
                Fail($"(lin gamma): Must be {@in}, but was {@out} : ");
                linGamma.Dispose();
                return false;
            }
        }

        if (!CheckGammaEstimation(linGamma, 1.0)) return false;

        linGamma.Dispose();
        return true;
    }

    #endregion Public Methods

    #region Private Methods

    private static bool CheckGamma(double g)
    {
        var curve = ToneCurve.BuildGamma(null, g);
        if (curve is null) return false;

        MaxErr = 0;
        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = i / 65535f;
            var @out = curve.Eval(@in);
            var val = Math.Pow(@in, g);

            var err = Math.Abs(val - @out);
            if (err > MaxErr) MaxErr = err;
        }

        if (MaxErr > 0) Console.Write($"|Err|<{MaxErr * 65535.0:F6} ");

        if (!CheckGammaEstimation(curve, g)) return false;

        curve.Dispose();
        return true;
    }

    private static bool CheckGammaFloatTable(double g)
    {
        var values = new float[1025];

        for (var i = 0; i <= 1024; i++)
        {
            var @in = i / 1024f;
            values[i] = MathF.Pow(@in, (float)g);
        }

        var curve = ToneCurve.BuildTabulated(null, values);
        if (curve is null) return false;

        MaxErr = 0;
        for (var i = 0; i < 0xFFFF; i++)
        {
            var @in = i / 65535f;
            var @out = curve.Eval(@in);
            var val = Math.Pow(@in, g);

            var err = Math.Abs(val - @out);
            if (err > MaxErr) MaxErr = err;
        }

        if (MaxErr > 0) Console.Write($"|Err|<{MaxErr * 65535.0:F6} ");

        if (!CheckGammaEstimation(curve, g)) return false;

        curve.Dispose();
        return true;
    }

    private static bool CheckGammaWordTable(double g)
    {
        var values = new ushort[1025];

        for (var i = 0; i <= 1024; i++)
        {
            var @in = i / 1024f;
            values[i] = (ushort)Math.Floor((Math.Pow(@in, g) * 65535.0) + 0.5);
        }

        var curve = ToneCurve.BuildTabulated(null, values);
        if (curve is null) return false;

        MaxErr = 0.0;
        for (var i = 0; i <= 0xFFFF; i++)
        {
            var @in = i / 65535f;
            var @out = curve.Eval(@in);
            var val = Math.Pow(@in, g);

            var err = Math.Abs(val - @out);
            if (err > MaxErr) MaxErr = err;
        }

        if (MaxErr > 0) Console.Write($"|Err|<{MaxErr * 65535.0:F6} ");

        if (!CheckGammaEstimation(curve, g)) return false;

        curve.Dispose();
        return true;
    }

    #endregion Private Methods
}
