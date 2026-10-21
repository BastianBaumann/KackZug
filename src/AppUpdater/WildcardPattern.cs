namespace AppUpdater;

/// <summary>Matches names against a simple pattern with "*" (any text) and "?" (one character), ignoring case.</summary>
public static class WildcardPattern
{
    public static bool IsMatch(string name, string pattern)
    {
        var n = 0;
        var p = 0;
        var starPattern = -1;
        var starName = 0;
        while (n < name.Length)
        {
            if (p < pattern.Length && (pattern[p] == '?' || char.ToLowerInvariant(pattern[p]) == char.ToLowerInvariant(name[n])))
            {
                n++;
                p++;
            }
            else if (p < pattern.Length && pattern[p] == '*')
            {
                starPattern = p;
                starName = n;
                p++;
            }
            else if (starPattern >= 0)
            {
                p = starPattern + 1;
                starName++;
                n = starName;
            }
            else
            {
                return false;
            }
        }

        while (p < pattern.Length && pattern[p] == '*')
        {
            p++;
        }

        return p == pattern.Length;
    }
}
