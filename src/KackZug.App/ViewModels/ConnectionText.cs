using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Kurze Texte zu Verbindungen fuer die Anzeige</summary>
public static class ConnectionText
{
    public static string Summary(SavedConnection? connection)
    {
        if (connection is null || connection.FirstRide is null)
        {
            return "noch keine Verbindung";
        }

        var first = connection.Legs[0];
        var last = connection.Legs[connection.Legs.Count - 1];
        var rides = connection.Legs.Where(leg => !leg.IsWalk).Select(leg => leg.Name);
        return first.PlannedDeparture.ToString("HH:mm") + " " + first.From.Name
            + " → " + last.To.Name + " " + last.PlannedArrival.ToString("HH:mm")
            + " (" + string.Join(", ", rides) + ")";
    }

    public static string Times(Leg leg)
    {
        return leg.PlannedDeparture.ToString("HH:mm") + " – " + leg.PlannedArrival.ToString("HH:mm");
    }

    public static string Route(Leg leg)
    {
        return leg.From.Name + " → " + leg.To.Name;
    }

    public static string Meta(Leg leg)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(leg.PlannedPlatform))
        {
            parts.Add("Gleis " + leg.PlannedPlatform);
        }

        if (!string.IsNullOrEmpty(leg.TrainNumber) && leg.TrainNumber != leg.Line && !leg.Name.Contains(leg.TrainNumber))
        {
            parts.Add("Zug " + leg.TrainNumber);
        }

        if (!string.IsNullOrEmpty(leg.Direction))
        {
            parts.Add("Richtung " + leg.Direction);
        }

        return string.Join(" · ", parts);
    }

    public static string Details(Leg leg)
    {
        if (leg.IsWalk)
        {
            var minutes = (int)(leg.PlannedArrival - leg.PlannedDeparture).TotalMinutes;
            return minutes + " Min. Fußweg";
        }

        var text = leg.Name;
        if (!string.IsNullOrEmpty(leg.TrainNumber) && leg.TrainNumber != leg.Line && !leg.Name.Contains(leg.TrainNumber))
        {
            text += " (" + leg.TrainNumber + ")";
        }

        if (!string.IsNullOrEmpty(leg.PlannedPlatform))
        {
            text += " · Gleis " + leg.PlannedPlatform;
        }

        return text;
    }
}
