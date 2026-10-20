using System.Text.Json;
using KackZug.Core.Bahn;
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
        var departureIndex = stopList.FindIndex(stop => BahnJson.GetString(stop, "extId") == leg.From.EvaNumber);
        if (departureIndex < 0)
        {
            return;
        }

        var departureStop = stopList[departureIndex];
        var liveDeparture = BahnJson.ReadLiveTime(departureStop, "abfahrt");
        if (liveDeparture is not null)
        {
            observation.LiveDeparture = liveDeparture;
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
            if (BahnJson.GetString(stopList[index], "extId") == leg.To.EvaNumber)
            {
                arrivalIndex = index;
                break;
            }
        }

        if (arrivalIndex < 0)
        {
            observation.ArrivalCancelled = true;
            return;
        }

        var arrivalStop = stopList[arrivalIndex];
        observation.ArrivalKnown = true;
        observation.LiveArrival = BahnJson.ReadLiveTime(arrivalStop, "ankunft");
        if (CancellationNotes.ContainsCancellation(arrivalStop))
        {
            observation.ArrivalCancelled = true;
        }
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
