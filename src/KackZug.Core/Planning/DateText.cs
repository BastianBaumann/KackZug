using System.Globalization;

namespace KackZug.Core.Planning;

/// <summary>Liest ein getipptes Datum wie 23.10.2026, 23.10.26 oder 23102026 und schreibt es als TT.MM.JJJJ</summary>
public static class DateText
{
    public static DateOnly? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var cleaned = text.Trim();
        foreach (var separator in new[] { '/', '-', ',', ' ' })
        {
            cleaned = cleaned.Replace(separator, '.');
        }

        string[] parts;
        if (cleaned.Contains('.'))
        {
            parts = cleaned.Split('.');
        }
        else if (cleaned.Length == 6 || cleaned.Length == 8)
        {
            parts = new[] { cleaned.Substring(0, 2), cleaned.Substring(2, 2), cleaned.Substring(4) };
        }
        else
        {
            return null;
        }

        if (parts.Length != 3 || parts.Any(part => part.Length == 0 || !part.All(char.IsAsciiDigit)))
        {
            return null;
        }

        if (parts[0].Length > 2 || parts[1].Length > 2 || (parts[2].Length != 2 && parts[2].Length != 4))
        {
            return null;
        }

        var day = int.Parse(parts[0], CultureInfo.InvariantCulture);
        var month = int.Parse(parts[1], CultureInfo.InvariantCulture);
        var year = int.Parse(parts[2], CultureInfo.InvariantCulture);
        if (parts[2].Length == 2)
        {
            year += 2000;
        }

        if (month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
        {
            return null;
        }

        return new DateOnly(year, month, day);
    }

    /// <summary>Setzt beim Tippen die Punkte selbst: 23102026 wird 23.10.2026, 1.2.2027 bleibt wie getippt</summary>
    /// <remarks>Ein Punkt kommt von selbst erst mit der naechsten Ziffer, sonst liesse er sich nicht wieder loeschen.</remarks>
    public static string FormatTyping(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        var parts = new List<string> { "" };
        foreach (var character in text)
        {
            var last = parts[parts.Count - 1];
            if (char.IsAsciiDigit(character))
            {
                var limit = 2;
                if (parts.Count == 3)
                {
                    limit = 4;
                }

                if (last.Length < limit)
                {
                    parts[parts.Count - 1] = last + character;
                }
                else if (parts.Count < 3)
                {
                    parts.Add(character.ToString());
                }
            }
            else if (".,/- ".Contains(character) && last.Length > 0 && parts.Count < 3)
            {
                parts.Add("");
            }
        }

        return string.Join(".", parts);
    }

    public static string Format(DateOnly date)
    {
        return date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
    }
}
