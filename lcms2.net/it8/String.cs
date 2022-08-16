using System.Text;

namespace lcms2.it8;
public class String
{
    public IT8 IT8;
    public int Max =>
        Begin.MaxCapacity;
    public int Len =>
        Begin.Length;
    public StringBuilder Begin;

    public String(IT8 it8, int max)
    {
        IT8 = it8;
        Begin = new StringBuilder(max, max);
    }

    public void Clear() =>
        Begin.Clear();

    public void Append(char c)
    {
        if (Len + 1 >= Max) {
            var max = Max * 10;
            var newSb = new StringBuilder(max, max);
            newSb.Append(Begin);
            Begin = newSb;
        }

        Begin.Append(c);
    }

    public void Concat(ReadOnlySpan<char> c) =>
        Begin.Append(c);

    public static bool IsSeparator(int c) =>
        c is ' ' or '\t';

    public static bool IsMiddle(int c) =>
        !IsSeparator(c) && c is not '#' and not '\"' and not '\'' and > 32 and < 127;

    public static bool IsIdChar(int c) =>
        Char.IsDigit((char)c) || IsMiddle(c);

    public static bool IsFirstIdChar(int c) =>
        !Char.IsDigit((char)c) && IsMiddle(c);

    public static bool IsAbsolutePath(ReadOnlySpan<char> path)
    {
        if (path.Length == 0)
            return false;

        if (path[0] == 0)
            return false;

        if (path[0] == Path.PathSeparator)
            return true;

        return Environment.OSVersion.Platform == PlatformID.Win32NT &&
            Char.IsLetter(path[0]) && path[1] == ':';
    }

    public static bool BuildAbsolutePath(ReadOnlySpan<char> relPath, ReadOnlySpan<char> basePath, Span<char> buffer, int maxLen)
    {
        // Already absolute?
        if (IsAbsolutePath(relPath)) {
            relPath.CopyTo(buffer);
            return true;
        }

        // No, search for last
        basePath.CopyTo(buffer);

        var tail = buffer.LastIndexOf(Path.PathSeparator);
        if (tail == -1)
            return false;   // Is not absolute and has no separators??

        var len = tail;
        if (len >= maxLen)
            return false;

        relPath.CopyTo(buffer[(tail + 1)..]);

        return true;
    }

    public static ReadOnlySpan<char> NoMeta(ReadOnlySpan<char> str) =>
        str.Contains('%')
            ? "*** CORRUPTED FORMAT STRING ***"
            : str;
}
