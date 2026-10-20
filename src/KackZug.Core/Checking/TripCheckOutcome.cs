namespace KackZug.Core.Checking;

/// <summary>Ergebnis einer Pruefung: Bewertung, Meldung und ob gemeldet werden soll</summary>
public sealed class TripCheckOutcome
{
    public TripCheckOutcome(TripStatus status, TripAlert alert, bool shouldNotify)
    {
        Status = status;
        Alert = alert;
        ShouldNotify = shouldNotify;
    }

    public TripStatus Status { get; }

    public TripAlert Alert { get; }

    public bool ShouldNotify { get; }
}
