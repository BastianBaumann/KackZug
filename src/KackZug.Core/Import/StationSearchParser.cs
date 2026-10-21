using System.Text.Json;
using KackZug.Core.Bahn;
using KackZug.Core.Models;

namespace KackZug.Core.Import;

/// <summary>Liest die Ortssuche von bahn.de; Adressen und Orte ohne Nummer fallen weg</summary>
public static class StationSearchParser
{
    public static List<Station> Parse(JsonElement root)
    {
        var stations = new List<Station>();
        if (root.ValueKind != JsonValueKind.Array)
        {
            return stations;
        }

        foreach (var item in root.EnumerateArray())
        {
            var name = BahnJson.GetString(item, "name");
            var number = BahnJson.GetString(item, "extId");
            var type = BahnJson.GetString(item, "type");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(number) || type != "ST")
            {
                continue;
            }

            if (stations.Any(station => station.EvaNumber == number))
            {
                continue;
            }

            stations.Add(new Station { Name = name, EvaNumber = number });
        }

        return stations;
    }
}
