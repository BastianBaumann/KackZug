using System.Text.RegularExpressions;

namespace KackZug.Core.Updates;

/// <summary>Macht aus den Release-Notizen von GitHub ein paar kurze Zeilen fuer den Update-Hinweis</summary>
public static partial class ReleaseNotesText
{
    public static string Shorten(string? notes, int maxLines)
    {
        if (string.IsNullOrWhiteSpace(notes) || maxLines <= 0)
        {
            return "";
        }

        var lines = new List<string>();
        var truncated = false;
        foreach (var rawLine in notes.Replace("\r", "").Split('\n'))
        {
            var line = CleanLine(rawLine);
            if (line.Length == 0)
            {
                continue;
            }

            if (lines.Count == maxLines)
            {
                truncated = true;
                break;
            }

            lines.Add(line);
        }

        if (truncated)
        {
            lines[lines.Count - 1] = lines[lines.Count - 1] + " …";
        }

        return string.Join("\n", lines);
    }

    private static string CleanLine(string rawLine)
    {
        var line = rawLine.Trim();
        if (line.StartsWith('#') || line.StartsWith("<!--", StringComparison.Ordinal))
        {
            return "";
        }

        if (line.Contains("Full Changelog", StringComparison.OrdinalIgnoreCase)
            || line.Contains("New Contributors", StringComparison.OrdinalIgnoreCase)
            || line.Contains("made their first contribution", StringComparison.OrdinalIgnoreCase))
        {
            return "";
        }

        if (line.StartsWith("* ", StringComparison.Ordinal) || line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("+ ", StringComparison.Ordinal))
        {
            line = line.Substring(2).Trim();
        }

        line = AuthorSuffix().Replace(line, "");
        line = line.Replace("**", "").Replace("`", "");
        return line.Trim();
    }

    [GeneratedRegex(@"\s+by @\S+ in \S+$")]
    private static partial Regex AuthorSuffix();
}
