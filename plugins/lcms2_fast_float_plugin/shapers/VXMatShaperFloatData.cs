//---------------------------------------------------------------------------------
//
//  Little Color Management System, fast floating point extensions
//  Copyright (c) 1998-2022 Marti Maria Saguer, all rights reserved
//                     2023 Stefan Kewatt, all rights reserved
//
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//---------------------------------------------------------------------------------
using lcms2.state;

using S1Fixed14Number = System.Int32;

namespace lcms2.FastFloatPlugin.shapers;

public class VXMatShaperFloatData : IDisposable
{
    private readonly float[] _mat;
    private readonly float[] _off;

    private readonly float[] _shaper1R;
    private readonly float[] _shaper1G;
    private readonly float[] _shaper1B;

    private readonly float[] _shaper2R;
    private readonly float[] _shaper2G;
    private readonly float[] _shaper2B;

    public readonly Context? ContextID;
    private bool disposedValue;

    public ref float Mat(int a, int b) =>
        ref _mat[(a * 3) + b];

    public Span<float> Off => _off.AsSpan(..3);

    public Span<float> Shaper1R => _shaper1R.AsSpan(..MAX_NODES_IN_CURVE);
    public Span<float> Shaper1G => _shaper1G.AsSpan(..MAX_NODES_IN_CURVE);
    public Span<float> Shaper1B => _shaper1B.AsSpan(..MAX_NODES_IN_CURVE);

    public Span<float> Shaper2R => _shaper2R.AsSpan(..MAX_NODES_IN_CURVE);
    public Span<float> Shaper2G => _shaper2G.AsSpan(..MAX_NODES_IN_CURVE);
    public Span<float> Shaper2B => _shaper2B.AsSpan(..MAX_NODES_IN_CURVE);

    public bool UseOff;

    public VXMatShaperFloatData(Context? context)
    {
        ContextID = context;

        var pool = Context.GetPool<float>(context);

        _mat = pool.Rent(9);
        _off = pool.Rent(3);

        _shaper1R = pool.Rent(MAX_NODES_IN_CURVE);
        _shaper1G = pool.Rent(MAX_NODES_IN_CURVE);
        _shaper1B = pool.Rent(MAX_NODES_IN_CURVE);

        _shaper2R = pool.Rent(MAX_NODES_IN_CURVE);
        _shaper2G = pool.Rent(MAX_NODES_IN_CURVE);
        _shaper2B = pool.Rent(MAX_NODES_IN_CURVE);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                var pool = Context.GetPool<float>(ContextID);

                pool.Return(_mat);
                pool.Return(_off);

                pool.Return(_shaper1R);
                pool.Return(_shaper1G);
                pool.Return(_shaper1B);

                pool.Return(_shaper2R);
                pool.Return(_shaper2G);
                pool.Return(_shaper2B);
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
