namespace lcms2.types;
public struct DateTimeNumber : ICloneable
{
    public ushort Year;
    public ushort Month;
    public ushort Day;
    public ushort Hours;
    public ushort Minutes;
    public ushort Seconds;

    public DateTimeNumber(ushort year, ushort month, ushort day, ushort hours, ushort minutes, ushort seconds)
    {
        Year = year;
        Month = month;
        Day = day;
        Hours = hours;
        Minutes = minutes;
        Seconds = seconds;
    }

    public object Clone() =>
        new DateTimeNumber(Year, Month, Day, Hours, Minutes, Seconds);

    /// <summary>
    /// Converts a <see cref="DateTimeNumber"/> into a <see cref="DateTime"/>.
    /// </summary>
    /// <remarks>Implements the <c>_cmsDecodeDateTimeNumber</c> function.</remarks>
    public static implicit operator DateTime(DateTimeNumber value) =>
        new(value.Year, value.Month, value.Day, value.Hours, value.Minutes, value.Seconds);

    /// <summary>
    /// Converts a <see cref="DateTime"/> into a <see cref="DateTimeNumber"/>.
    /// </summary>
    /// <remarks>Implements the <c>_cmsEncodeDateTimeNumber</c> function.</remarks>
    public static explicit operator DateTimeNumber(DateTime value) =>
        new((ushort)value.Year,
            (ushort)value.Month,
            (ushort)value.Day,
            (ushort)value.Hour,
            (ushort)value.Minute,
            (ushort)value.Second);
}
