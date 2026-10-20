using KackZug.Core.Checking;

namespace KackZug.Core.Alarms;

/// <summary>Eine Stoerungsmeldung, die noch nicht mit "Gesehen" bestaetigt wurde</summary>
public sealed class PendingAlarm
{
    public int NotificationId { get; set; }

    public string Title { get; set; } = "";

    public string Text { get; set; } = "";

    public string Kicker { get; set; } = "";

    public string Headline { get; set; } = "";

    public string TripLine { get; set; } = "";

    public LegStatus Status { get; set; }

    public DateTimeOffset FirstShownAt { get; set; }

    public DateTimeOffset LastShownAt { get; set; }

    public DateTimeOffset RepeatUntil { get; set; }

    public static PendingAlarm From(TripAlert alert, int notificationId, DateTimeOffset now)
    {
        return new PendingAlarm
        {
            NotificationId = notificationId,
            Title = alert.Title,
            Text = alert.Text,
            Kicker = alert.Kicker,
            Headline = alert.Headline,
            TripLine = alert.TripLine,
            Status = alert.Status,
            FirstShownAt = now,
            LastShownAt = now,
            RepeatUntil = alert.RepeatUntil
        };
    }
}
