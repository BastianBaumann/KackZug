using System.Text.Json;
using KackZug.Core.Bahn;
using KackZug.Core.Localization;
using KackZug.Core.Models;

namespace KackZug.Core.Checking;

/// <summary>Liest aus den Fahrtdetails von bahn.de den Ab- und den Zielhalt einer Teilstrecke</summary>
public static class TripReader
{
    public static void Apply(JsonElement trip, Leg leg, LegObservation observation)
    {
        if (BahnJson.GetBool(trip, "cancelled"))
        {
            observation.DepartureCancelled = true;
            observation.ArrivalCancelled = true;
        }

        if (!trip.TryGetProperty("halte", out var stops) || stops.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var stopList = stops.EnumerateArray().ToList();
        var departureIndex = stopList.FindIndex(stop => IsStop(stop, leg.From));
        if (departureIndex < 0)
        {
            return;
        }

        var departureStop = stopList[departureIndex];
        var liveDeparture = BahnJson.ReadLiveTime(departureStop, "abfahrt");
        if (liveDeparture is not null)
        {
            observation.LiveDeparture = liveDeparture;
            observation.HasLiveData = true;
        }

        var platform = CurrentPlatform(departureStop);
        if (platform is not null)
        {
            observation.CurrentPlatform = platform;
        }

        if (CancellationNotes.ContainsCancellation(departureStop))
        {
            observation.DepartureCancelled = true;
        }

        var arrivalIndex = -1;
        for (var index = departureIndex + 1; index < stopList.Count; index++)
        {
            if (IsStop(stopList[index], leg.To))
            {
                arrivalIndex = index;
                break;
            }
        }

        if (arrivalIndex < 0)
        {
            observation.ArrivalMissing = true;
            observation.Problem = string.Format(Language.Culture, Strings.Check_DestinationMissing, leg.To.Name);
            return;
        }

        var arrivalStop = stopList[arrivalIndex];
        observation.ArrivalKnown = true;
        observation.LiveArrival = BahnJson.ReadLiveTime(arrivalStop, "ankunft");
        if (observation.LiveArrival is not null)
        {
            observation.HasLiveData = true;
        }
        if (CancellationNotes.ContainsCancellation(arrivalStop))
        {
            observation.ArrivalCancelled = true;
        }
    }

    /// <remarks>Meist passt die Haltnummer; manche Halte fuehrt bahn.de in den Fahrtdaten unter einer anderen Nummer, dann zaehlt der Name.</remarks>
    private static bool IsStop(JsonElement stop, Station station)
    {
        if (BahnJson.GetString(stop, "extId") == station.EvaNumber)
        {
            return true;
        }

        var name = BahnJson.GetString(stop, "name");
        return !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(station.Name)
            && string.Equals(Compact(name), Compact(station.Name), StringComparison.OrdinalIgnoreCase);
    }

    private static string Compact(string text)
    {
        return string.Concat(text.Where(character => !char.IsWhiteSpace(character)));
    }

    private static string? CurrentPlatform(JsonElement stop)
    {
        var changed = BahnJson.GetString(stop, "ezGleis");
        if (!string.IsNullOrEmpty(changed))
        {
            return changed;
        }

        return BahnJson.GetString(stop, "gleis");
    }
}
