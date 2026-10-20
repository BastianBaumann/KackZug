namespace KackZug.Core.Planning;

/// <summary>Rechnet Uhrzeiten aus dem Fahrplan (deutsche Ortszeit) in echte Zeitpunkte um</summary>
public static class BerlinTime
{
    private static TimeZoneInfo? _zone;

    public static TimeZoneInfo Zone
    {
        get
        {
            if (_zone is null)
            {
                _zone = FindZone();
            }

            return _zone;
        }
    }

    public static DateTimeOffset ToInstant(DateTime wallClock, TimeZoneInfo zone)
    {
        var unspecified = DateTime.SpecifyKind(wallClock, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(unspecified))
        {
            unspecified = unspecified.AddHours(1);
        }

        var offset = zone.GetUtcOffset(unspecified);
        if (zone.IsAmbiguousTime(unspecified))
        {
            var offsets = zone.GetAmbiguousTimeOffsets(unspecified);
            offset = offsets.Max();
        }

        return new DateTimeOffset(unspecified, offset);
    }

    public static DateTime ToWallClock(DateTimeOffset instant, TimeZoneInfo zone)
    {
        return TimeZoneInfo.ConvertTime(instant, zone).DateTime;
    }

    private static TimeZoneInfo FindZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
    }
}
