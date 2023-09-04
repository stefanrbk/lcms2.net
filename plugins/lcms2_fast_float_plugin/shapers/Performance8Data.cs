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
using lcms2.types;

namespace lcms2.FastFloatPlugin.shapers;

public class Performance8Data : IDisposable
{
    public readonly Context? ContextID;
    public readonly InterpParams<ushort> p;     // Tetrahedrical interpolation parameters

    private readonly ushort[] _rx;
    private readonly ushort[] _ry;
    private readonly ushort[] _rz;

    private readonly uint[] _X0;  // Precomputed nodes and offsets for 8-bit input data
    private readonly uint[] _Y0;
    private readonly uint[] _Z0;

    private bool disposedValue;

    public Span<ushort> rx => _rx.AsSpan(..256);
    public Span<ushort> ry => _ry.AsSpan(..256);
    public Span<ushort> rz => _rz.AsSpan(..256);

    public Span<uint> X0 => _X0.AsSpan(..0x4001);
    public Span<uint> Y0 => _Y0.AsSpan(..0x4001);
    public Span<uint> Z0 => _Z0.AsSpan(..0x4001);

    public Performance8Data(Context? context, InterpParams<ushort> p)
    {
        ContextID = context;
        this.p = p;

        var ipool = Context.GetPool<uint>(context);
        var spool = Context.GetPool<ushort>(context);

        _rx = spool.Rent(256);
        _ry = spool.Rent(256);
        _rz = spool.Rent(256);

        _X0 = ipool.Rent(256);
        _Y0 = ipool.Rent(256);
        _Z0 = ipool.Rent(256);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                var ipool = Context.GetPool<uint>(ContextID);
                var spool = Context.GetPool<ushort>(ContextID);

                spool.Return(_rx);
                spool.Return(_ry);
                spool.Return(_rz);

                ipool.Return(_X0);
                ipool.Return(_Y0);
                ipool.Return(_Z0);
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
