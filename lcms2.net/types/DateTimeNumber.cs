using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace lcms2.types;
public record DateTimeNumber
{
    public ushort Year;
    public ushort Month;
    public ushort Day;
    public ushort Hours;
    public ushort Minutes;
    public ushort Seconds;

    public static implicit operator DateTime(DateTimeNumber value) =>
        new(value.Year, value.Month, value.Day, value.Hours, value.Minutes, value.Seconds);
    public static explicit operator DateTimeNumber(DateTime value) =>
        new()
        {
            Year = (ushort)value.Year,
            Month = (ushort)value.Month,
            Day = (ushort)value.Day,
            Hours = (ushort)value.Hour,
            Minutes = (ushort)value.Minute,
            Seconds = (ushort)value.Second
        };
}
