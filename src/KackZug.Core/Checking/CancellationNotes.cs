using System.Text.Json;
using KackZug.Core.Bahn;

namespace KackZug.Core.Checking;

/// <summary>Erkennt Ausfaelle an den Hinweisen von bahn.de, die es dort nur als Text gibt</summary>
/// <remarks>Erkennungsmerkmale wie im Projekt db-vendo-client: Schluessel, Typ HALT_AUSFALL oder Texte wie "Halt entfällt".
/// Texte zaehlen nur in Hinweisen zu genau dieser Fahrt. Allgemeine Stoerungsmeldungen (HIM, priorisierteMeldungen) haengen
/// an jeder Fahrt der Strecke, solange die Stoerung gilt, und meldeten sonst auch morgen einen Ausfall.</remarks>
public static class CancellationNotes
{
    private const string GeneralMessagesArray = "priorisierteMeldungen";

    private static readonly string[] NoteArrays = { "meldungen", "risNotizen", GeneralMessagesArray };

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

            var readText = arrayName != GeneralMessagesArray;
            foreach (var note in notes.EnumerateArray())
            {
                if (IsCancellationNote(note, readText))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsCancellationNote(JsonElement note, bool readText)
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

        if (!readText || type == "HIM")
        {
            return false;
        }

        var text = BahnJson.GetString(note, "text") ?? BahnJson.GetString(note, "value") ?? "";
        if (text.Contains("Halt entfällt", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return text.Contains("fällt aus", StringComparison.OrdinalIgnoreCase);
    }
}
