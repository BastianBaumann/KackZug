namespace KackZug.Core.Checking;

/// <summary>Was bahn.de aktuell ueber eine Teilstrecke an einem bestimmten Tag meldet</summary>
public sealed class LegObservation
{
    public ObservationResult Result { get; set; } = ObservationResult.Unknown;

    public DateTime PlannedDeparture { get; set; }

    public DateTime PlannedArrival { get; set; }

    public DateTime? LiveDeparture { get; set; }

    public DateTime? LiveArrival { get; set; }

    public bool ArrivalKnown { get; set; }

    public bool HasLiveData { get; set; }

    public bool DepartureCancelled { get; set; }

    public bool ArrivalCancelled { get; set; }

    /// <remarks>Der Zielhalt fehlt in den Fahrtdaten von bahn.de. Ein ausgefallener Halt bleibt dort mit Hinweis stehen,
    /// ein fehlender heisst meist nur, dass die Daten unvollstaendig sind oder der Halt anders heisst.</remarks>
    public bool ArrivalMissing { get; set; }

    public string? CurrentPlatform { get; set; }

    public string? Problem { get; set; }

    public static LegObservation Unknown(DateTime plannedDeparture, DateTime plannedArrival, string problem)
    {
        return new LegObservation
        {
            Result = ObservationResult.Unknown,
            PlannedDeparture = plannedDeparture,
            PlannedArrival = plannedArrival,
            Problem = problem
        };
    }
}
