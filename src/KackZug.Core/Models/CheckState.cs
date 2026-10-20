using KackZug.Core.Checking;

namespace KackZug.Core.Models;

/// <summary>Letztes Pruefergebnis einer Fahrt, damit dasselbe nicht doppelt gemeldet wird</summary>
public sealed class CheckState
{
    public DateOnly Date { get; set; }

    public Guid ShiftId { get; set; }

    public TripDirection Direction { get; set; }

    public string LastStatusHash { get; set; } = "";

    public LegStatus? LastStatus { get; set; }

    public string LastSummary { get; set; } = "";

    public DateTimeOffset LastCheckedAt { get; set; }

    public DateTimeOffset? LastNotifiedAt { get; set; }
}
