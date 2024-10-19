//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright ©️ 1998-2024 Marti Maria Saguer
//              2022-2024 Stefan Kewatt
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

using lcms2.state;

namespace lcms2.types;

public class InterpParams<T> : ICloneable, IDisposable
{
    public Context? ContextID;
    public uint dwFlags;
    public uint nInputs;
    public uint nOutputs;
    public uint[] nSamples = new uint[MAX_INPUT_DIMENSIONS];
    public uint[] Domain = new uint[MAX_INPUT_DIMENSIONS];
    public uint[] opta = new uint[MAX_INPUT_DIMENSIONS];
    public Memory<T> Table;
    public InterpFunction? Interpolation;
    private bool disposedValue;

    private InterpParams(Context? context, uint inputs, uint outputs, uint flags)
    {
        ContextID = context;

        nInputs = inputs;
        nOutputs = outputs;
        dwFlags = flags;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    object ICloneable.Clone() =>
        Clone();

    public InterpParams<T> Clone()
    {
        var result = new InterpParams<T>(ContextID, nInputs, nOutputs, dwFlags)
        {
            Table = Table,
            Interpolation = Interpolation,
        };

        nSamples.AsSpan().CopyTo(result.nSamples);
        Domain.AsSpan().CopyTo(result.Domain);
        opta.AsSpan().CopyTo(result.opta);

        return result;
    }
}
