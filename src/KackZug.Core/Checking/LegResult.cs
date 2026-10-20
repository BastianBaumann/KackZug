using KackZug.Core.Models;

namespace KackZug.Core.Checking;

/// <summary>Bewertung einer einzelnen Teilstrecke</summary>
public sealed class LegResult
{
    public LegResult(Leg leg, LegObservation? observation)
    {
        Leg = leg;
        Observation = observation;
    }

    public Leg Leg { get; }

    public LegObservation? Observation { get; }

    public LegStatus Status { get; set; } = LegStatus.OnTime;

    public int DelayMinutes { get; set; }

    public string? NewPlatform { get; set; }
}
