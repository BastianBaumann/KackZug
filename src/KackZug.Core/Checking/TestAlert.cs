namespace KackZug.Core.Checking;

/// <summary>Beispielmeldung fuer den Test-Alarm in den Einstellungen</summary>
public static class TestAlert
{
    public static TripAlert Create(DateTimeOffset now)
    {
        return new TripAlert
        {
            Title = "TEST HIN: RE7 06:42 FÄLLT AUS",
            Text = "So sieht eine Störung aus. Alles gut, das war nur ein Test.",
            Severity = AlertSeverity.Problem,
            Status = LegStatus.Cancelled,
            StatusHash = "test",
            TravelDate = DateOnly.FromDateTime(now.LocalDateTime),
            ShiftId = Guid.Empty,
            Kicker = "TEST · HINFAHRT",
            Headline = "FÄLLT AUS",
            TripLine = "RE7 · 06:42 Musterstadt Hbf → Beispielhausen",
            RepeatUntil = now.AddMinutes(11)
        };
    }
}
