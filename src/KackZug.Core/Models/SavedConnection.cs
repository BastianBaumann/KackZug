using System.Text.Json.Serialization;

namespace KackZug.Core.Models;

/// <summary>Eine gespeicherte Verbindung aus einer oder mehreren Teilstrecken</summary>
public sealed class SavedConnection
{
    public List<Leg> Legs { get; set; } = new List<Leg>();

    public DateTimeOffset ImportedAt { get; set; }

    [JsonIgnore]
    public Leg? FirstRide
    {
        get { return Legs.FirstOrDefault(leg => !leg.IsWalk); }
    }

    [JsonIgnore]
    public Leg? LastRide
    {
        get { return Legs.LastOrDefault(leg => !leg.IsWalk); }
    }
}
