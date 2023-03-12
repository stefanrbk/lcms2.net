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

namespace lcms2.testbed;

public static unsafe class InterpolationTests
{
    private static readonly ushort[][] _checkXDData =
    {
        new ushort[] { 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000 },
        new ushort[] { 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF },
        new ushort[] { 0x8080, 0x8080, 0x8080, 0x8080, 0x1234, 0x1122, 0x0056, 0x0011 },
        new ushort[] { 0x0000, 0xFE00, 0x80FF, 0x8888, 0x8878, 0x2233, 0x0088, 0x2020 },
        new ushort[] { 0x1111, 0x2222, 0x3333, 0x4444, 0x1455, 0x3344, 0x1987, 0x4532 },
        new ushort[] { 0x0000, 0x0012, 0x0013, 0x0014, 0x2333, 0x4455, 0x9988, 0x1200 },
        new ushort[] { 0x3141, 0x1415, 0x1592, 0x9261, 0x4567, 0x5566, 0xFE56, 0x6666 },
        new ushort[] { 0xFF00, 0xFF01, 0xFF12, 0xFF13, 0xF344, 0x6677, 0xBABE, 0xFACE },
    };

    private static readonly uint[][] _checkXDDims =
    {
        new uint[] { 7, 8, 9 },
        new uint[] { 9, 8, 7, 6 },
        new uint[] { 3, 2, 2, 2, 2 },
        new uint[] { 4, 3, 3, 2, 2, 2 },
        new uint[] { 4, 3, 3, 2, 2, 2, 2 },
        new uint[] { 4, 3, 3, 2, 2, 2, 2, 2 }
    };
}
