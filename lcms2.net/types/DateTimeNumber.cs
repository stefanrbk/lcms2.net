//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
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
using System.Runtime.InteropServices;

namespace lcms2.types;

[StructLayout(LayoutKind.Explicit)]
public struct DateTimeNumber : ICloneable
{
    #region Fields

    [FieldOffset(4)] public ushort Day;

    [FieldOffset(6)] public ushort Hours;

    [FieldOffset(8)] public ushort Minutes;

    [FieldOffset(2)] public ushort Month;

    [FieldOffset(10)] public ushort Seconds;

    [FieldOffset(0)] public ushort Year;

    #endregion Fields

    #region Public Constructors

    public DateTimeNumber(ushort year, ushort month, ushort day, ushort hours, ushort minutes, ushort seconds)
    {
        Year = year;
        Month = month;
        Day = day;
        Hours = hours;
        Minutes = minutes;
        Seconds = seconds;
    }

    #endregion Public Constructors

    #region Public Methods

    /// <summary>
    ///     Converts a <see cref="DateTime"/> into a <see cref="DateTimeNumber"/>.
    /// </summary>
    /// <remarks>Implements the <c>_cmsEncodeDateTimeNumber</c> function.</remarks>
    public static explicit operator DateTimeNumber(DateTime value) =>
        new((ushort)value.Year,
            (ushort)value.Month,
            (ushort)value.Day,
            (ushort)value.Hour,
            (ushort)value.Minute,
            (ushort)value.Second);

    /// <summary>
    ///     Converts a <see cref="DateTimeNumber"/> into a <see cref="DateTime"/>.
    /// </summary>
    /// <remarks>Implements the <c>_cmsDecodeDateTimeNumber</c> function.</remarks>
    public static implicit operator DateTime(DateTimeNumber value) =>
        new(value.Year, value.Month, value.Day, value.Hours, value.Minutes, value.Seconds);

    public object Clone() =>
               new DateTimeNumber(Year, Month, Day, Hours, Minutes, Seconds);

    #endregion Public Methods
}
