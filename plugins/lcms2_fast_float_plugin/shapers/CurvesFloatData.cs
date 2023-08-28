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

namespace lcms2.FastFloatPlugin.shapers;
public class CurvesFloatData : IDisposable
{
    private bool disposedValue;
    public Context? ContextID;
    private readonly float[] _curveR;
    private readonly float[] _curveG;
    private readonly float[] _curveB;

    public Span<float> CurveR => _curveR.AsSpan(..MAX_NODES_IN_CURVE);
    public Span<float> CurveG => _curveG.AsSpan(..MAX_NODES_IN_CURVE);
    public Span<float> CurveB => _curveB.AsSpan(..MAX_NODES_IN_CURVE);

    public CurvesFloatData(Context? context)
    {
        ContextID = context;
        var pool = Context.GetPool<float>(context);
        _curveR = pool.Rent(MAX_NODES_IN_CURVE);
        _curveG = pool.Rent(MAX_NODES_IN_CURVE);
        _curveB = pool.Rent(MAX_NODES_IN_CURVE);
        Array.Clear(_curveR);
        Array.Clear(_curveG);
        Array.Clear(_curveB);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                var pool = Context.GetPool<float>(ContextID);
                pool.Return(_curveR);
                pool.Return(_curveG);
                pool.Return(_curveB);
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
