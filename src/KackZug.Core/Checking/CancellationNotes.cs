using System.Text.Json;
using KackZug.Core.Bahn;

namespace KackZug.Core.Checking;

/// <summary>Erkennt Ausfaelle an den Hinweisen von bahn.de, die es dort nur als Text gibt</summary>
/// <remarks>Erkennungsmerkmale wie im Projekt db-vendo-client: Schluessel, Typ HALT_AUSFALL oder Texte wie "Halt entfällt".</remarks>
public static class CancellationNotes
{
    private static readonly string[] NoteArrays = { "meldungen", "risNotizen", "priorisierteMeldungen" };

    public static bool ContainsCancellation(JsonElement element)
    {
        if (BahnJson.GetBool(element, "cancelled") || BahnJson.GetBool(element, "canceled"))
        {
            return true;
        }

        foreach (var arrayName in NoteArrays)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(arrayName, out var notes))
            {
                continue;
            }

            if (notes.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var note in notes.EnumerateArray())
            {
                if (IsCancellationNote(note))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsCancellationNote(JsonElement note)
    {
        var key = BahnJson.GetString(note, "key");
        if (key == "text.realtime.stop.cancelled" || key == "text.realtime.journey.cancelled")
        {
            return true;
        }

        var type = BahnJson.GetString(note, "type");
        if (type == "HALT_AUSFALL")
        {
            return true;
        }

        var text = BahnJson.GetString(note, "text") ?? BahnJson.GetString(note, "value") ?? "";
        if (text.Contains("Halt entfällt", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return text.Contains("fällt aus", StringComparison.OrdinalIgnoreCase);
    }
}
