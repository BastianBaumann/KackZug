using System.Text.Json;
using KackZug.Core.Bahn;
using KackZug.Core.Import;
using KackZug.Core.Models;

namespace KackZug.Core.Sharing;

/// <summary>Findet in den Suchergebnissen von bahn.de genau die gespeicherte Verbindung wieder</summary>
/// <remarks>Gleich heisst: dieselben Fahrten ab denselben Halten zur selben Zeit, bei bekannter Zugnummer auch dieselbe Nummer.</remarks>
public static class ConnectionPicker
{
    public static string? PickRecon(JsonElement root, SavedConnection saved)
    {
        if (!root.TryGetProperty("verbindungen", out var connections) || connections.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var savedRides = saved.Legs.Where(leg => !leg.IsWalk).ToList();
        foreach (var candidate in connections.EnumerateArray())
        {
            var recon = BahnJson.GetString(candidate, "ctxRecon");
            if (string.IsNullOrEmpty(recon))
            {
                continue;
            }

            SavedConnection found;
            try
            {
                found = ReconResponseMapper.MapConnection(candidate, DateTimeOffset.MinValue).Connection;
            }
            catch (FormatException)
            {
                continue;
            }

            var foundRides = found.Legs.Where(leg => !leg.IsWalk).ToList();
            if (SameRides(savedRides, foundRides))
            {
                return recon;
            }
        }

        return null;
    }

    private static bool SameRides(List<Leg> saved, List<Leg> found)
    {
        if (saved.Count != found.Count)
        {
            return false;
        }

        for (var index = 0; index < saved.Count; index++)
        {
            var mine = saved[index];
            var theirs = found[index];
            if (mine.From.EvaNumber != theirs.From.EvaNumber || mine.PlannedDeparture != theirs.PlannedDeparture)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(mine.TrainNumber) && !string.IsNullOrEmpty(theirs.TrainNumber) && mine.TrainNumber != theirs.TrainNumber)
            {
                return false;
            }
        }

        return true;
    }
}
