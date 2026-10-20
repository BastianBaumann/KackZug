using System.Text.Json.Serialization;
using KackZug.Core.Models;

namespace KackZug.Core.Storage;

/// <summary>Vorab erzeugte JSON-Beschreibung der Daten, damit das Speichern auch nach dem Trimmen auf Android klappt</summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(AppData))]
public partial class AppJsonContext : JsonSerializerContext
{
}
