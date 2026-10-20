using System.Text.Json;
using KackZug.Core.Bahn;

namespace KackZug.Core.Checking;

/// <summary>Liest die Abfahrtstafel von bahn.de</summary>
/// <remarks>Auf der Tafel heisst das Feld "verkehrmittel", ohne s.</remarks>
public static class BoardParser
{
    public static List<BoardEntry> Parse(JsonElement root)
    {
        var entries = new List<BoardEntry>();
        if (!root.TryGetProperty("entries", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return entries;
        }

        foreach (var item in items.EnumerateArray())
        {
            var planned = BahnJson.ParseTime(BahnJson.GetString(item, "zeit"));
            var journeyId = BahnJson.GetString(item, "journeyId");
            if (planned is null || journeyId is null)
            {
                continue;
            }

            var means = BahnJson.GetObject(item, "verkehrmittel");
            if (means.ValueKind != JsonValueKind.Object)
            {
                means = BahnJson.GetObject(item, "verkehrsmittel");
            }

            entries.Add(new BoardEntry
            {
                PlannedTime = planned.Value,
                LiveTime = BahnJson.ParseTime(BahnJson.GetString(item, "ezZeit")),
                Platform = BahnJson.GetString(item, "gleis"),
                LivePlatform = BahnJson.GetString(item, "ezGleis"),
                JourneyId = journeyId,
                Name = BahnJson.GetString(means, "mittelText"),
                LineNumber = BahnJson.GetString(means, "linienNummer"),
                Category = JourneyIdReader.Read(journeyId, "CA"),
                Number = JourneyIdReader.Read(journeyId, "ZE"),
                Cancelled = CancellationNotes.ContainsCancellation(item)
            });
        }

        return entries;
    }
}
