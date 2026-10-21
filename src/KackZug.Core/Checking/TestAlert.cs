using KackZug.Core.Localization;

namespace KackZug.Core.Checking;

/// <summary>Beispielmeldung fuer den Test-Alarm in den Einstellungen</summary>
public static class TestAlert
{
    public static TripAlert Create(DateTimeOffset now)
    {
        return new TripAlert
        {
            Title = Strings.Test_Title,
            Text = Strings.Test_Text,
            Severity = AlertSeverity.Problem,
            Status = LegStatus.Cancelled,
            StatusHash = "test",
            TravelDate = DateOnly.FromDateTime(now.LocalDateTime),
            ShiftId = Guid.Empty,
            Kicker = Strings.Test_Kicker,
            Headline = Strings.Alert_HeadlineCancelled,
            TripLine = Strings.Test_TripLine,
            RepeatUntil = now.AddMinutes(11)
        };
    }
}
