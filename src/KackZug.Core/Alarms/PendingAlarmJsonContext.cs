using System.Text.Json.Serialization;

namespace KackZug.Core.Alarms;

/// <summary>Vorab erzeugte JSON-Beschreibung fuer offene Alarme, sicher nach dem Trimmen</summary>
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(List<PendingAlarm>))]
public partial class PendingAlarmJsonContext : JsonSerializerContext
{
}
