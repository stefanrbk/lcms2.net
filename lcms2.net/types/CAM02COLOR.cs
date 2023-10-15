//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2023 Marti Maria Saguer
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

using System.Buffers;

namespace lcms2.types;
public class CAM02COLOR : IDisposable
{
    private readonly ArrayPool<double> pool;
    private readonly double[] _XYZ;
    private readonly double[] _RGB;
    private readonly double[] _RGBc;
    private readonly double[] _RGBp;
    private readonly double[] _RGBpa;
    public double a, b, h, e, H, A, J, Q, s, t, C, M;
    private readonly double[] _AbC;
    private readonly double[] _Abs;
    private readonly double[] _AbM;
    private bool disposedValue;

    public Span<double> XYZ => _XYZ.AsSpan(..3);
    public Span<double> RGB => _RGB.AsSpan(..3);
    public Span<double> RGBc => _RGBc.AsSpan(..3);
    public Span<double> RGBp => _RGBp.AsSpan(..3);
    public Span<double> RGBpa => _RGBpa.AsSpan(..3);
    public Span<double> AbC => _AbC.AsSpan(..2);
    public Span<double> Abs => _Abs.AsSpan(..2);
    public Span<double> AbM => _AbM.AsSpan(..2);

    internal CAM02COLOR(Context? context)
    {
        pool = Context.GetPool<double>(context);

        _XYZ = pool.Rent(3);
        _RGB = pool.Rent(3);
        _RGBc = pool.Rent(3);
        _RGBp = pool.Rent(3);
        _RGBpa = pool.Rent(3);
        _AbC = pool.Rent(2);
        _Abs = pool.Rent(2);
        _AbM = pool.Rent(2);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ReturnArray(pool, _XYZ);
                ReturnArray(pool, _RGB);
                ReturnArray(pool, _RGBc);
                ReturnArray(pool, _RGBp);
                ReturnArray(pool, _RGBpa);
                ReturnArray(pool, _AbC);
                ReturnArray(pool, _Abs);
                ReturnArray(pool, _AbM);
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
