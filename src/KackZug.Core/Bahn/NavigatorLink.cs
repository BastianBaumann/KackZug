using KackZug.Core.Models;

namespace KackZug.Core.Bahn;

/// <summary>Baut einen bahn.de-Suchlink fuer eine Verbindung an einem bestimmten Tag, den der DB Navigator oder der Browser oeffnet</summary>
public static class NavigatorLink
{
    private const string SearchPage = "https://www.bahn.de/buchung/fahrplan/suche";

    public static string? ForTrip(SavedConnection connection, DateOnly travelDate)
    {
        var firstRide = connection.FirstRide;
        var lastRide = connection.LastRide;
        if (firstRide is null || lastRide is null)
        {
            return null;
        }

        var departure = firstRide.DepartureOn(travelDate);
        var parts = new List<string>
        {
            "sts=true",
            "so=" + Uri.EscapeDataString(firstRide.From.Name),
            "zo=" + Uri.EscapeDataString(lastRide.To.Name),
        };

        if (!string.IsNullOrEmpty(firstRide.From.EvaNumber))
        {
            parts.Add("soid=" + Uri.EscapeDataString(StationId(firstRide.From)));
        }

        if (!string.IsNullOrEmpty(lastRide.To.EvaNumber))
        {
            parts.Add("zoid=" + Uri.EscapeDataString(StationId(lastRide.To)));
        }

        parts.Add("hd=" + Uri.EscapeDataString(departure.ToString("yyyy-MM-dd'T'HH:mm:ss")));
        parts.Add("hza=D");
        parts.Add("ar=false");
        parts.Add("s=true");
        parts.Add("d=false");
        parts.Add("hz=%5B%5D");
        parts.Add("fm=false");
        parts.Add("bp=false");
        return SearchPage + "#" + string.Join("&", parts);
    }

    private static string StationId(Station station)
    {
        return "A=1@O=" + station.Name + "@L=" + station.EvaNumber + "@";
    }
}
