using System.Globalization;
using System.Text.Json;
using KackZug.Core.Bahn;
using KackZug.Core.Localization;
using KackZug.Core.Models;

namespace KackZug.Core.Import;

/// <summary>Uebersetzt die Antwort von bahn.de auf eine Verbindungsanfrage in Teilstrecken</summary>
public static class ReconResponseMapper
{
    public static ResolvedConnection Map(JsonElement root, DateTimeOffset importedAt)
    {
        return MapConnection(FirstConnection(root), importedAt);
    }

    public static ResolvedConnection MapConnection(JsonElement connectionElement, DateTimeOffset importedAt)
    {
        if (!connectionElement.TryGetProperty("verbindungsAbschnitte", out var sections))
        {
            throw new FormatException("Antwort enthaelt keine Teilstrecken.");
        }

        var connection = new SavedConnection { ImportedAt = importedAt };
        DateTime? firstDeparture = null;
        var hasPlatforms = false;

        foreach (var section in sections.EnumerateArray())
        {
            var departure = BahnJson.ReadPlannedTime(section, "abfahrt");
            var arrival = BahnJson.ReadPlannedTime(section, "ankunft");
            if (departure is null || arrival is null)
            {
                throw new FormatException("Teilstrecke ohne Zeiten.");
            }

            if (firstDeparture is null)
            {
                firstDeparture = departure.Value;
            }

            var leg = CreateLeg(section, departure.Value, arrival.Value, firstDeparture.Value.Date);
            if (leg.PlannedPlatform is not null)
            {
                hasPlatforms = true;
            }

            connection.Legs.Add(leg);
        }

        if (connection.FirstRide is null)
        {
            throw new FormatException("Verbindung enthaelt keine Fahrt.");
        }

        var travelDate = DateOnly.FromDateTime(firstDeparture!.Value);
        return new ResolvedConnection(connection, travelDate, hasPlatforms);
    }

    private static JsonElement FirstConnection(JsonElement root)
    {
        if (!root.TryGetProperty("verbindungen", out var connections) || connections.GetArrayLength() == 0)
        {
            throw new FormatException("Antwort enthaelt keine Verbindung.");
        }

        return connections[0];
    }

    private static Leg CreateLeg(JsonElement section, DateTime departure, DateTime arrival, DateTime startDate)
    {
        var means = BahnJson.GetObject(section, "verkehrsmittel");
        var type = BahnJson.GetString(means, "typ");
        var leg = new Leg
        {
            From = new Station
            {
                Name = BahnJson.GetString(section, "abfahrtsOrt") ?? "",
                EvaNumber = BahnJson.GetString(section, "abfahrtsOrtExtId") ?? ""
            },
            To = new Station
            {
                Name = BahnJson.GetString(section, "ankunftsOrt") ?? "",
                EvaNumber = BahnJson.GetString(section, "ankunftsOrtExtId") ?? ""
            },
            PlannedDeparture = TimeOnly.FromDateTime(departure),
            PlannedArrival = TimeOnly.FromDateTime(arrival),
            DepartureDayOffset = (departure.Date - startDate).Days,
            ArrivalDayOffset = (arrival.Date - startDate).Days
        };

        if (type == "WALK")
        {
            leg.Kind = LegKind.Walk;
            leg.Name = Strings.Import_WalkName;
            return leg;
        }

        leg.Kind = LegKind.PublicTransport;
        leg.Name = BahnJson.GetString(means, "mittelText") ?? BahnJson.GetString(means, "name") ?? "";
        leg.Category = BahnJson.GetString(means, "kategorie") ?? "";
        leg.ProductClass = BahnJson.GetString(means, "produktGattung");
        leg.Line = BahnJson.GetString(means, "linienNummer");
        leg.TrainNumber = BahnJson.GetString(means, "nummer");
        leg.Direction = BahnJson.GetString(means, "richtung");
        leg.PlannedPlatform = FirstStopPlatform(section);
        return leg;
    }

    private static string? FirstStopPlatform(JsonElement section)
    {
        if (!section.TryGetProperty("halte", out var stops) || stops.GetArrayLength() == 0)
        {
            return null;
        }

        return BahnJson.GetString(stops[0], "gleis");
    }
}
