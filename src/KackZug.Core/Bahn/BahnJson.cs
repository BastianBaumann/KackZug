using System.Globalization;
using System.Text.Json;

namespace KackZug.Core.Bahn;

/// <summary>Kleine Helfer zum Lesen der JSON-Antworten von bahn.de</summary>
public static class BahnJson
{
    public static string? GetString(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!element.TryGetProperty(name, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        if (value.ValueKind == JsonValueKind.Number)
        {
            return value.GetRawText();
        }

        return null;
    }

    public static bool GetBool(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!element.TryGetProperty(name, out var value))
        {
            return false;
        }

        return value.ValueKind == JsonValueKind.True;
    }

    public static JsonElement GetObject(JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value))
        {
            return value;
        }

        return default;
    }

    public static DateTime? ReadPlannedTime(JsonElement element, string name)
    {
        return ReadTime(GetObject(element, name), "sollzeit");
    }

    public static DateTime? ReadLiveTime(JsonElement element, string name)
    {
        return ReadTime(GetObject(element, name), "echtzeit");
    }

    public static DateTime? ParseTime(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var parsed = DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time);
        if (!parsed)
        {
            return null;
        }

        return time;
    }

    private static DateTime? ReadTime(JsonElement element, string name)
    {
        return ParseTime(GetString(element, name));
    }
}
