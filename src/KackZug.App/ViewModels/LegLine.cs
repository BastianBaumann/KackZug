using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Eine Teilstrecke fuer die Anzeige im Stil der Connection-Komponente</summary>
public sealed class LegLine
{
    public LegLine(Leg leg)
    {
        Times = ConnectionText.Times(leg);
        Route = ConnectionText.Route(leg);
        Details = ConnectionText.Details(leg);
        IsWalk = leg.IsWalk;
        DepartureTime = leg.PlannedDeparture.ToString("HH:mm");
        ArrivalTime = leg.PlannedArrival.ToString("HH:mm");
        FromName = leg.From.Name;
        ToName = leg.To.Name;
        LineName = leg.Name;
        Meta = ConnectionText.Meta(leg);
    }

    public string Times { get; }

    public string Route { get; }

    public string Details { get; }

    public bool IsWalk { get; }

    public string DepartureTime { get; }

    public string ArrivalTime { get; }

    public string FromName { get; }

    public string ToName { get; }

    public string LineName { get; }

    public string Meta { get; }
}
