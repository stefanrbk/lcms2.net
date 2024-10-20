﻿//---------------------------------------------------------------------------------
//
//  Little Color Management System, multithread extensions
//  Copyright ©️ 1998-2024 Marti Maria Saguer, all rights reserved
//              2022-2024 Stefan Kewatt, all rights reserved
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

using lcms2.types;

namespace lcms2.ThreadedPlugin;
public unsafe class WorkSlice : ICloneable
{
    public Transform CMMcargo;

    public byte* InputBuffer;
    public byte* OutputBuffer;

    public int InputBufferLength;
    public int OutputBufferLength;

    public uint PixelsPerLine;
    public uint LineCount;
    public Stride Stride;

    public object Clone() =>
        new WorkSlice()
        {
            CMMcargo = CMMcargo,
            InputBuffer = InputBuffer,
            OutputBuffer = OutputBuffer,
            PixelsPerLine = PixelsPerLine,
            LineCount = LineCount,
            Stride = Stride
        };
}
