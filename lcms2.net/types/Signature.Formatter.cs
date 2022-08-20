using System.Globalization;
using System.Text;

namespace lcms2.types;

public partial struct Signature
{
    public class Formatter: IFormatProvider, ICustomFormatter
    {
        public string Format(string? format, object? obj, IFormatProvider? provider)
        {
            if (obj is null)
                return string.Empty;

            if (obj is Signature value)
            {
                // Text output
                if (format?.ToUpper().StartsWith("T") ?? false)
                {
                    var sb = new StringBuilder(4);
                    sb.Append((char)((value._value >> 24) & 0xFF));
                    sb.Append((char)((value._value >> 16) & 0xFF));
                    sb.Append((char)((value._value >> 8) & 0xFF));
                    sb.Append((char)(value._value & 0xFF));
                    return sb.ToString();
                }
                // Hex output
                if (format?.ToUpper().StartsWith("X") ?? false)
                    return String.Format(provider, "{" + format + "}", (uint)obj);
            }

            // Use default for all other formatting
            return obj is IFormattable formattable
                ? formattable.ToString(format, CultureInfo.CurrentCulture)
                : obj.ToString() ?? String.Empty;
        }

        public object? GetFormat(Type? formatType) =>
            formatType == typeof(ICustomFormatter) ? this : (object?)null;
    }
}
