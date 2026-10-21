using System.Text.Json.Serialization;

namespace KackZug.Core.Models;

/// <summary>Eine Verbindung, die nur an einem festen Tag gefahren wird; danach raeumt die App sie selbst weg</summary>
public sealed class OneOffTrip
{
    public static readonly TimeSpan KeepAfterArrival = TimeSpan.FromMinutes(60);

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";

    public DateOnly TravelDate { get; set; }

    public SavedConnection Connection { get; set; } = new SavedConnection();

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Geplante Ankunft der letzten Fahrt als Ortszeit, Tagesversatz eingerechnet</summary>
    [JsonIgnore]
    public DateTime PlannedArrival
    {
        get { return ArrivalOf(Connection, TravelDate); }
    }

    public static DateTime ArrivalOf(SavedConnection connection, DateOnly travelDate)
    {
        var lastRide = connection.LastRide;
        if (lastRide is not null)
        {
            return lastRide.ArrivalOn(travelDate);
        }

        var lastLeg = connection.Legs.LastOrDefault();
        if (lastLeg is not null)
        {
            return lastLeg.ArrivalOn(travelDate);
        }

        return travelDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
    }

    /// <summary>Name, wenn keiner eingegeben wurde: Start und Ziel der Verbindung</summary>
    public static string DefaultName(SavedConnection connection)
    {
        if (connection.Legs.Count == 0)
        {
            return "";
        }

        return connection.Legs[0].From.Name + " → " + connection.Legs[connection.Legs.Count - 1].To.Name;
    }
}
