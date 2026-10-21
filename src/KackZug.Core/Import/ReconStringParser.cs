using System.Globalization;
using KackZug.Core.Localization;
using KackZug.Core.Models;

namespace KackZug.Core.Import;

/// <summary>Liest Bahnhoefe, Zeiten und Zug direkt aus dem Verbindungsschluessel von bahn.de, ohne Gleise</summary>
/// <remarks>Aufbau laut eigener Analyse und dem Projekt dbFREE: Abschnitte getrennt durch §, Felder durch $.</remarks>
public static class ReconStringParser
{
    private const string TimeFormat = "yyyyMMddHHmm";

    public static ResolvedConnection Parse(string recon, DateTimeOffset importedAt)
    {
        var body = recon.Replace("¶HKI¶", "");
        var endMarker = body.IndexOf('¶');
        if (endMarker >= 0)
        {
            body = body.Substring(0, endMarker);
        }

        var connection = new SavedConnection { ImportedAt = importedAt };
        DateTime? startDate = null;

        foreach (var sectionText in body.Split('§', StringSplitOptions.RemoveEmptyEntries))
        {
            var fields = sectionText.Split('$');
            if (fields.Length < 6)
            {
                throw new ImportException(Strings.Import_ErrorUnreadable);
            }

            var departure = ParseTime(fields[3]);
            var arrival = ParseTime(fields[4]);
            if (startDate is null)
            {
                startDate = departure.Date;
            }

            var leg = new Leg
            {
                From = ParseStation(fields[1]),
                To = ParseStation(fields[2]),
                PlannedDeparture = TimeOnly.FromDateTime(departure),
                PlannedArrival = TimeOnly.FromDateTime(arrival),
                DepartureDayOffset = (departure.Date - startDate.Value).Days,
                ArrivalDayOffset = (arrival.Date - startDate.Value).Days
            };

            if (fields[0] == "W")
            {
                leg.Kind = LegKind.Walk;
                leg.Name = Strings.Import_WalkName;
            }
            else
            {
                FillTrain(leg, fields[5]);
            }

            connection.Legs.Add(leg);
        }

        if (startDate is null || connection.FirstRide is null)
        {
            throw new ImportException(Strings.Import_ErrorUnreadable);
        }

        return new ResolvedConnection(connection, DateOnly.FromDateTime(startDate.Value), false);
    }

    private static DateTime ParseTime(string text)
    {
        var parsed = DateTime.TryParseExact(text, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time);
        if (!parsed)
        {
            throw new ImportException(Strings.Import_ErrorUnreadable);
        }

        return time;
    }

    private static Station ParseStation(string text)
    {
        var station = new Station();
        foreach (var part in text.Split('@'))
        {
            if (part.StartsWith("O=", StringComparison.Ordinal))
            {
                station.Name = part.Substring(2);
            }
            else if (part.StartsWith("L=", StringComparison.Ordinal))
            {
                station.EvaNumber = part.Substring(2);
            }
        }

        return station;
    }

    private static void FillTrain(Leg leg, string trainText)
    {
        var words = trainText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            leg.Name = Strings.Import_TrainName;
            return;
        }

        var number = words[words.Length - 1];
        leg.TrainNumber = number;
        if (words.Length > 1)
        {
            leg.Category = words[0];
            leg.Name = words[0] + " " + number;
        }
        else
        {
            leg.Name = string.Format(Language.Culture, Strings.Import_TrainNameWithNumber, number);
        }
    }
}
