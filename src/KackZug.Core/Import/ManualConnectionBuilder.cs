using KackZug.Core.Localization;
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
            return ManualBuildResult.Failure(Strings.Manual_ErrorNoLegs);
        }

        var connection = new SavedConnection { ImportedAt = now };
        var dayOffset = 0;
        TimeOnly? previousTime = null;
        Leg? previousRide = null;

        for (var index = 0; index < inputs.Count; index++)
        {
            var input = inputs[index];
            var number = index + 1;
            if (input.From is null)
            {
                return ManualBuildResult.Failure(LegError(number, Strings.Manual_ErrorNoStart));
            }

            if (input.To is null)
            {
                return ManualBuildResult.Failure(LegError(number, Strings.Manual_ErrorNoDestination));
            }

            if (input.From.EvaNumber == input.To.EvaNumber)
            {
                return ManualBuildResult.Failure(LegError(number, Strings.Manual_ErrorSameStations));
            }

            var departure = ClockText.Parse(input.DepartureText);
            if (departure is null)
            {
                return ManualBuildResult.Failure(LegError(number, Strings.Manual_ErrorDepartureTime));
            }

            var arrival = ClockText.Parse(input.ArrivalText);
            if (arrival is null)
            {
                return ManualBuildResult.Failure(LegError(number, Strings.Manual_ErrorArrivalTime));
            }

            var line = input.LineText.Trim();
            if (line.Length == 0)
            {
                return ManualBuildResult.Failure(LegError(number, Strings.Manual_ErrorNoLine));
            }

            var trainNumber = input.TrainNumberText.Trim();
            if (trainNumber.Length > 0 && !trainNumber.All(char.IsAsciiDigit))
            {
                return ManualBuildResult.Failure(LegError(number, Strings.Manual_ErrorTrainNumberDigits));
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
                    Name = Strings.Import_WalkName
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

    private static string LegError(int number, string message)
    {
        return string.Format(Language.Culture, Strings.Manual_ErrorLegPrefix, number, message);
    }
}
