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
            ReadEndsEarly(trip, stopList, departureIndex, leg, observation);
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

    /// <remarks>Fehlt das Ziel, faehrt der Zug meist nur bis zu seinem letzten Halt (geaenderter Fahrplan oder Kurzwende).
    /// Ohne Halt nach der Abfahrt ist das nicht sicher, dann bleibt es bei "keine Daten".</remarks>
    private static void ReadEndsEarly(JsonElement trip, List<JsonElement> stopList, int departureIndex, Leg leg, LegObservation observation)
    {
        if (departureIndex >= stopList.Count - 1)
        {
            observation.Problem = string.Format(Language.Culture, Strings.Check_DestinationMissing, leg.To.Name);
            return;
        }

        var last = stopList[stopList.Count - 1];
        var lastStop = BahnJson.GetString(last, "name");
        if (string.IsNullOrEmpty(lastStop))
        {
            observation.Problem = string.Format(Language.Culture, Strings.Check_DestinationMissing, leg.To.Name);
            return;
        }

        observation.EndsAt = lastStop;
        observation.EndsAtEva = BahnJson.GetString(last, "extId");
        observation.EndsAtArrival = BahnJson.ReadPlannedTime(last, "ankunft");
        observation.ChangedDays = BahnJson.GetString(trip, "irregulaereVerkehrstage");
        var problem = string.Format(Language.Culture, Strings.Check_EndsEarly, lastStop, leg.To.Name);
        if (!string.IsNullOrEmpty(observation.ChangedDays))
        {
            problem += " " + string.Format(Language.Culture, Strings.Check_ChangedDays, observation.ChangedDays);
        }

        observation.Problem = problem;
    }

    /// <summary>Liest die Ankunft aus der Weiterfahrt des Zuges ab seinem bisherigen letzten Halt</summary>
    /// <remarks>bahn.de fuehrt manche Zuege in zwei Fahrten, etwa RE3 3962 bis Weimar und weiter als 18682 nach Erfurt.
    /// Gibt false zurueck, wenn das Ziel auch in der Weiterfahrt fehlt.</remarks>
    public static bool ApplyContinuation(JsonElement trip, Leg leg, LegObservation observation)
    {
        if (!trip.TryGetProperty("halte", out var stops) || stops.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var stopList = stops.EnumerateArray().ToList();
        var startIndex = stopList.FindIndex(stop => BahnJson.GetString(stop, "extId") == observation.EndsAtEva);
        for (var index = startIndex + 1; index < stopList.Count; index++)
        {
            var stop = stopList[index];
            if (!IsStop(stop, leg.To))
            {
                continue;
            }

            observation.ArrivalMissing = false;
            observation.EndsAt = null;
            observation.EndsAtEva = null;
            observation.EndsAtArrival = null;
            observation.ChangedDays = null;
            observation.Problem = null;
            observation.ArrivalKnown = true;
            observation.LiveArrival = BahnJson.ReadLiveTime(stop, "ankunft");
            if (observation.LiveArrival is not null)
            {
                observation.HasLiveData = true;
            }

            if (CancellationNotes.ContainsCancellation(stop))
            {
                observation.ArrivalCancelled = true;
            }

            return true;
        }

        return false;
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
