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

using System.Buffers;

namespace lcms2.types;

public unsafe class StageMatrixData : IDisposable
{
    public double[] Double
    {
        get =>
            d;
        private set
        {
            ArgumentNullException.ThrowIfNull(value);
            d = value;
        }
    }
    private double[] d;
    public double[]? Offset;
    private bool disposedValue;
    private readonly ArrayPool<double>? pool;

    public StageMatrixData(ReadOnlySpan<double> d, ReadOnlySpan<double> o = default, ArrayPool<double>? p = null)
    {
        Double = p is null
            ? new double[d.Length]
            : p.Rent(d.Length);

        Offset = o.Length < 0
            ? p is null
                ? new double[o.Length]
                : p.Rent(o.Length)
            : null;

        disposedValue = false;
        pool = p;

        if (Double is null)
            throw new InvalidOperationException();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (pool is not null)
                {
                    pool.Return(Double);
                    d = null!;
                    if (Offset is not null)
                        pool.Return(Offset);
                    Offset = null!;
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~StageMatrixData()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
