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

using S1Fixed15Number = System.Int32;

namespace lcms2.FastFloatPlugin.shapers;
public class XMatShaperData(Context? context) : IDisposable
{
    private readonly S1Fixed15Number[] _mat = Context.GetPool<S1Fixed15Number>(context).Rent(9);
    private readonly S1Fixed15Number[] _off = Context.GetPool<S1Fixed15Number>(context).Rent(3);
    private readonly ushort[] _shapers = Context.GetPool<ushort>(context).Rent(MAX_NODES_IN_CURVE * 6);
    public Span<S1Fixed15Number> Mat => _mat.AsSpan(..9);
    public Span<S1Fixed15Number> Off => _off.AsSpan(..9);

    public Span<ushort> Shaper1R => _shapers.AsSpan(..MAX_NODES_IN_CURVE);
    public Span<ushort> Shaper1G => _shapers.AsSpan(MAX_NODES_IN_CURVE..(MAX_NODES_IN_CURVE * 2));
    public Span<ushort> Shaper1B => _shapers.AsSpan((MAX_NODES_IN_CURVE * 2)..(MAX_NODES_IN_CURVE * 3));

    public Span<ushort> Shaper2R => _shapers.AsSpan((MAX_NODES_IN_CURVE * 3)..(MAX_NODES_IN_CURVE * 4));
    public Span<ushort> Shaper2G => _shapers.AsSpan((MAX_NODES_IN_CURVE * 4)..(MAX_NODES_IN_CURVE * 5));
    public Span<ushort> Shaper2B => _shapers.AsSpan((MAX_NODES_IN_CURVE * 5)..(MAX_NODES_IN_CURVE * 6));

    public bool IdentityMat;

    public Context? ContextID = context;
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                var pool = Context.GetPool<S1Fixed15Number>(ContextID);

                pool.Return(_mat);
                pool.Return(_off);

                Context.GetPool<ushort>(ContextID).Return(_shapers);
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
