using KackZug.Core.Models;
using KackZug.Core.Planning;

namespace KackZug.Core.Import;

/// <summary>Prueft die Eingabe von Hand und baut daraus eine Verbindung; zwischen verschiedenen Haltestellen entsteht ein Fussweg</summary>
public static class ManualConnectionBuilder
{
    public static ManualBuildResult Build(IReadOnlyList<ManualLegInput> inputs, DateTimeOffset now)
    {
        if (inputs.Count == 0)
        {
            return ManualBuildResult.Failure("Bitte mindestens eine Teilstrecke eingeben.");
        }

        var connection = new SavedConnection { ImportedAt = now };
        var dayOffset = 0;
        TimeOnly? previousTime = null;
        Leg? previousRide = null;

        for (var index = 0; index < inputs.Count; index++)
        {
            var input = inputs[index];
            var prefix = "Teilstrecke " + (index + 1) + ": ";
            if (input.From is null)
            {
                return ManualBuildResult.Failure(prefix + "Bitte den Start-Bahnhof suchen und antippen.");
            }

            if (input.To is null)
            {
                return ManualBuildResult.Failure(prefix + "Bitte das Ziel suchen und antippen.");
            }

            if (input.From.EvaNumber == input.To.EvaNumber)
            {
                return ManualBuildResult.Failure(prefix + "Start und Ziel sind gleich.");
            }

            var departure = ClockText.Parse(input.DepartureText);
            if (departure is null)
            {
                return ManualBuildResult.Failure(prefix + "Abfahrt bitte als HH:mm eingeben.");
            }

            var arrival = ClockText.Parse(input.ArrivalText);
            if (arrival is null)
            {
                return ManualBuildResult.Failure(prefix + "Ankunft bitte als HH:mm eingeben.");
            }

            var line = input.LineText.Trim();
            if (line.Length == 0)
            {
                return ManualBuildResult.Failure(prefix + "Bitte die Linie eingeben, z. B. RE 1 oder Bus 11.");
            }

            var trainNumber = input.TrainNumberText.Trim();
            if (trainNumber.Length > 0 && !trainNumber.All(char.IsAsciiDigit))
            {
                return ManualBuildResult.Failure(prefix + "Die Zugnummer besteht nur aus Ziffern.");
            }

            if (previousTime is not null && departure.Value < previousTime.Value)
            {
                dayOffset++;
            }

            if (previousRide is not null && previousRide.To.EvaNumber != input.From.EvaNumber)
            {
                connection.Legs.Add(new Leg
                {
                    Kind = LegKind.Walk,
                    From = previousRide.To,
                    To = input.From,
                    PlannedDeparture = previousRide.PlannedArrival,
                    DepartureDayOffset = previousRide.ArrivalDayOffset,
                    PlannedArrival = departure.Value,
                    ArrivalDayOffset = dayOffset,
                    Name = "Fussweg"
                });
            }

            var departureOffset = dayOffset;
            if (arrival.Value < departure.Value)
            {
                dayOffset++;
            }

            var ride = new Leg
            {
                From = input.From,
                To = input.To,
                PlannedDeparture = departure.Value,
                DepartureDayOffset = departureOffset,
                PlannedArrival = arrival.Value,
                ArrivalDayOffset = dayOffset,
                Name = line,
                Line = line
            };

            if (trainNumber.Length > 0)
            {
                ride.TrainNumber = trainNumber;
            }

            connection.Legs.Add(ride);
            previousRide = ride;
            previousTime = arrival.Value;
        }

        return ManualBuildResult.Success(connection);
    }
}
