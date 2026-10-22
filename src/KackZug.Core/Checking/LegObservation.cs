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

    /// <remarks>Der Zielhalt fehlt in den Fahrtdaten von bahn.de, auch unter seinem Namen.</remarks>
    public bool ArrivalMissing { get; set; }

    /// <summary>Letzter Halt des Zuges, wenn er vor dem Ziel endet, etwa bei einem geaenderten Fahrplan</summary>
    public string? EndsAt { get; set; }

    /// <summary>Text von bahn.de zu abweichenden Fahrtagen, z. B. "22. Okt bis 12. Dez 2026"</summary>
    public string? ChangedDays { get; set; }

    /// <summary>Haltnummer und geplante Ankunft am letzten Halt, um dort nach der Weiterfahrt zu suchen</summary>
    public string? EndsAtEva { get; set; }

    public DateTime? EndsAtArrival { get; set; }

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
