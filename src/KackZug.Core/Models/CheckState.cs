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

    public string LastBadgeText { get; set; } = "";

    /// <remarks>Sprache, in der LastBadgeText geschrieben wurde; leer bei alten Daten, die noch deutsch sind.</remarks>
    public string LastBadgeLanguage { get; set; } = "";

    public DateTimeOffset LastCheckedAt { get; set; }

    public DateTimeOffset? LastNotifiedAt { get; set; }
}
