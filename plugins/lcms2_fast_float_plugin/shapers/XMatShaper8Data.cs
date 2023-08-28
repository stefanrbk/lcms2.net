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

using System.Runtime.InteropServices;

namespace lcms2.FastFloatPlugin.shapers;

public unsafe class XMatShaper8Data : IDisposable
{
    public Inner* Data;
    public Context? ContextID;
    private bool disposedValue;

    public struct Inner
    {
        // This is for SSE, MUST be aligned at 16 bit boundary

        public fixed float Mat[16];     // n.14 to n.14 (needs a saturation after that)

        public void* real_ptr;

        public fixed float Shaper1R[256];   // from 0..255 to 1.14 (0.0...1.0)
        public fixed float Shaper1G[256];
        public fixed float Shaper1B[256];

        public fixed byte Shaper2R[0x4001];   // 1.14 to 0..255
        public fixed byte Shaper2G[0x4001];
        public fixed byte Shaper2B[0x4001];
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            NativeMemory.Free(Data->real_ptr);
            Data = null;
            disposedValue = true;
        }
    }

    ~XMatShaper8Data() =>
        Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
