namespace lcms2.types;
public record DateTimeNumber
{
    public ushort Year;
    public ushort Month;
    public ushort Day;
    public ushort Hours;
    public ushort Minutes;
    public ushort Seconds;

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
