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

namespace lcms2.types;

public class InterpParams<T> : ICloneable, IDisposable
{
    public Context? ContextID;
    public uint dwFlags;
    public uint nInputs;
    public uint nOutputs;
    public uint[] nSamples;
    public uint[] Domain;
    public uint[] opta;
    public Memory<T> Table;
    public InterpFunction? Interpolation;
    private bool disposedValue;

    public InterpParams(Context? context)
    {
        ContextID = context;

        var pool = Context.GetPool<uint>(context);

        nSamples = pool.Rent(MAX_INPUT_DIMENSIONS);
        Domain = pool.Rent(MAX_INPUT_DIMENSIONS);
        opta = pool.Rent(MAX_INPUT_DIMENSIONS);

        Array.Clear(nSamples);
        Array.Clear(Domain);
        Array.Clear(opta);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                var pool = Context.GetPool<uint>(ContextID);

                ReturnArray(pool, nSamples); nSamples = null!;
                ReturnArray(pool, Domain); Domain = null!;
                ReturnArray(pool, opta); opta = null!;
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public object Clone()
    {
        var result = new InterpParams<T>(ContextID)
        {
            dwFlags = dwFlags,
            nInputs = nInputs,
            nOutputs = nOutputs,
            Table = Table,
            Interpolation = Interpolation,
        };

        nSamples.AsSpan(..MAX_INPUT_DIMENSIONS).CopyTo(result.nSamples);
        Domain.AsSpan(..MAX_INPUT_DIMENSIONS).CopyTo(result.Domain);
        opta.AsSpan(..MAX_INPUT_DIMENSIONS).CopyTo(result.opta);

        return result;
    }
}
