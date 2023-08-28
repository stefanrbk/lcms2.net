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

public class XMatShaper8Data : IDisposable
{
    private readonly S1Fixed14Number[] _mat;     // n.14 to n.14 (needs a saturation after that)

    private readonly S1Fixed14Number[] _shaper1R;   // from 0..255 to 1.14 (0.0...1.0)
    private readonly S1Fixed14Number[] _shaper1G;
    private readonly S1Fixed14Number[] _shaper1B;

    private readonly byte[] _shaper2R;   // 1.14 to 0..255
    private readonly byte[] _shaper2G;
    private readonly byte[] _shaper2B;

    public readonly Context? ContextID;
    private bool disposedValue;

    public ref S1Fixed14Number Mat(int a, int b) =>
        ref _mat[(a * 4) + b];

    public Span<S1Fixed14Number> Shaper1R => _shaper1R.AsSpan(..256);
    public Span<S1Fixed14Number> Shaper1G => _shaper1G.AsSpan(..256);
    public Span<S1Fixed14Number> Shaper1B => _shaper1B.AsSpan(..256);

    public Span<byte> Shaper2R => _shaper2R.AsSpan(..0x4001);
    public Span<byte> Shaper2G => _shaper2G.AsSpan(..0x4001);
    public Span<byte> Shaper2B => _shaper2B.AsSpan(..0x4001);

    public XMatShaper8Data(Context? context)
    {
        ContextID = context;

        var ipool = Context.GetPool<S1Fixed14Number>(context);
        var bpool = Context.GetPool<byte>(context);

        _mat = ipool.Rent(16);

        _shaper1R = ipool.Rent(256);
        _shaper1G = ipool.Rent(256);
        _shaper1B = ipool.Rent(256);

        _shaper2R = bpool.Rent(0x4001);
        _shaper2G = bpool.Rent(0x4001);
        _shaper2B = bpool.Rent(0x4001);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                var ipool = Context.GetPool<S1Fixed14Number>(ContextID);
                var bpool = Context.GetPool<byte>(ContextID);

                ipool.Return(_mat);

                ipool.Return(_shaper1R);
                ipool.Return(_shaper1G);
                ipool.Return(_shaper1B);

                bpool.Return(_shaper2R);
                bpool.Return(_shaper2G);
                bpool.Return(_shaper2B);
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
