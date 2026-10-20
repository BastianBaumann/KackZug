using KackZug.Core.Models;

namespace KackZug.Core.Checking;

/// <summary>Fertige Meldung zu einer Fahrt, wie sie in der Benachrichtigung steht</summary>
public sealed class TripAlert
{
    public DateOnly TravelDate { get; set; }

    public Guid ShiftId { get; set; }

    public TripDirection Direction { get; set; }

    public LegStatus Status { get; set; }

    public AlertSeverity Severity { get; set; }

    public string Title { get; set; } = "";

    public string Text { get; set; } = "";

    public string StatusHash { get; set; } = "";
}
