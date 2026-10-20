using KackZug.Core.Planning;

namespace KackZug.Core.Checking;

/// <summary>Ergebnis einer Pruefung im Hintergrund: Meldung, Wiederholung und neue Alarme</summary>
public sealed class BackgroundCheckResult
{
    public TripAlert? AlertToShow { get; set; }

    public bool RetrySoon { get; set; }

    public List<PlannedCheck> NextChecks { get; set; } = new List<PlannedCheck>();
}
