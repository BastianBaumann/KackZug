using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Eine Zeile mit einer Teilstrecke fuer die Anzeige</summary>
public sealed class LegLine
{
    public LegLine(Leg leg)
    {
        Times = ConnectionText.Times(leg);
        Route = ConnectionText.Route(leg);
        Details = ConnectionText.Details(leg);
        IsWalk = leg.IsWalk;
    }

    public string Times { get; }

    public string Route { get; }

    public string Details { get; }

    public bool IsWalk { get; }
}
