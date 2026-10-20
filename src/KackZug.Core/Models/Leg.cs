using System.Text.Json.Serialization;

namespace KackZug.Core.Models;

/// <summary>Eine Teilstrecke einer Verbindung, gespeichert als Muster ohne Datum</summary>
public sealed class Leg
{
    public LegKind Kind { get; set; } = LegKind.PublicTransport;

    public Station From { get; set; } = new Station();

    public Station To { get; set; } = new Station();

    public TimeOnly PlannedDeparture { get; set; }

    public TimeOnly PlannedArrival { get; set; }

    public int DepartureDayOffset { get; set; }

    public int ArrivalDayOffset { get; set; }

    public string Name { get; set; } = "";

    public string Category { get; set; } = "";

    public string? ProductClass { get; set; }

    public string? Line { get; set; }

    public string? TrainNumber { get; set; }

    public string? Direction { get; set; }

    public string? PlannedPlatform { get; set; }

    public DateTime DepartureOn(DateOnly travelDate)
    {
        return travelDate.AddDays(DepartureDayOffset).ToDateTime(PlannedDeparture);
    }

    public DateTime ArrivalOn(DateOnly travelDate)
    {
        return travelDate.AddDays(ArrivalDayOffset).ToDateTime(PlannedArrival);
    }

    [JsonIgnore]
    public bool IsWalk
    {
        get { return Kind == LegKind.Walk; }
    }
}
