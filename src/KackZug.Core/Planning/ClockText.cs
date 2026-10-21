namespace KackZug.Core.Planning;

/// <summary>Liest eine getippte Uhrzeit wie 19:30, 19.30, 1930 oder 19 und schreibt sie als HH:mm</summary>
public static class ClockText
{
    public static TimeOnly? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var cleaned = text.Trim().ToLowerInvariant().Replace("uhr", "").Trim();
        foreach (var separator in new[] { '.', ',', ' ', 'h' })
        {
            cleaned = cleaned.Replace(separator, ':');
        }

        string hourText;
        string minuteText;
        var colon = cleaned.IndexOf(':');
        if (colon >= 0)
        {
            hourText = cleaned.Substring(0, colon);
            minuteText = cleaned.Substring(colon + 1);
            if (minuteText.Length == 0)
            {
                minuteText = "0";
            }
        }
        else if (cleaned.Length <= 2)
        {
            hourText = cleaned;
            minuteText = "0";
        }
        else if (cleaned.Length <= 4)
        {
            hourText = cleaned.Substring(0, cleaned.Length - 2);
            minuteText = cleaned.Substring(cleaned.Length - 2);
        }
        else
        {
            return null;
        }

        if (hourText.Length == 0 || hourText.Length > 2 || minuteText.Length > 2
            || !hourText.All(char.IsAsciiDigit) || !minuteText.All(char.IsAsciiDigit))
        {
            return null;
        }

        var hour = int.Parse(hourText);
        var minute = int.Parse(minuteText);
        if (hour > 23 || minute > 59)
        {
            return null;
        }

        return new TimeOnly(hour, minute);
    }

    /// <summary>Setzt beim Tippen den Doppelpunkt selbst: 2230 wird 22:30, 930 wird 9:30</summary>
    /// <remarks>Der Doppelpunkt kommt erst mit der ersten Minutenziffer, sonst liesse er sich nicht wieder loeschen.
    /// Beginnt die Zahl mit 3 bis 9, ist die Stunde einstellig; 25 bleibt 25, damit der Hinweis auf eine falsche Zeit kommt.</remarks>
    public static string FormatTyping(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        var digits = new string(text.Where(char.IsAsciiDigit).Take(4).ToArray());
        if (digits.Length == 0)
        {
            return "";
        }

        var hourLength = 2;
        if (digits[0] >= '3')
        {
            hourLength = 1;
        }

        if (digits.Length <= hourLength)
        {
            return digits;
        }

        var minutes = digits.Substring(hourLength);
        if (minutes.Length > 2)
        {
            minutes = minutes.Substring(0, 2);
        }

        return digits.Substring(0, hourLength) + ":" + minutes;
    }

    public static string Format(TimeOnly time)
    {
        return time.ToString("HH:mm");
    }
}
