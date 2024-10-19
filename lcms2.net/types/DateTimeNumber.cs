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

using System.Runtime.InteropServices;

namespace lcms2.types;

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct DateTimeNumber(ushort year, ushort month, ushort day, ushort hours, ushort minutes, ushort seconds)
{
    [FieldOffset(4)] public ushort Day = day;

    [FieldOffset(6)] public ushort Hours = hours;

    [FieldOffset(8)] public ushort Minutes = minutes;

    [FieldOffset(2)] public ushort Month = month;

    [FieldOffset(10)] public ushort Seconds = seconds;

    [FieldOffset(0)] public ushort Year = year;

    public static void Encode(out DateTimeNumber dest, DateTime source)   // _cmsEncodeDateTimeNumber
    {
        dest = new()
        {
            Seconds = AdjustEndianess((ushort)source.Second),
            Minutes = AdjustEndianess((ushort)source.Minute),
            Hours = AdjustEndianess((ushort)source.Hour),
            Day = AdjustEndianess((ushort)source.Day),
            Month = AdjustEndianess((ushort)source.Month),
            Year = AdjustEndianess((ushort)source.Year)
        };
    }

    public readonly void Decode(out DateTime Dest)   // _cmsDecodeDateTimeNumber
    {
        var sec = AdjustEndianess(Seconds);
        var min = AdjustEndianess(Minutes);
        var hour = AdjustEndianess(Hours);
        var day = AdjustEndianess(Day);
        var mon = AdjustEndianess(Month);
        var year = AdjustEndianess(Year);

        try
        {
            Dest = new(year, mon, day, hour, min, sec);
        }
        catch (ArgumentOutOfRangeException)
        {
            Dest = default;
        }
    }
}
