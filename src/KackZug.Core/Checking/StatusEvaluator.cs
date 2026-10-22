using KackZug.Core.Models;

namespace KackZug.Core.Checking;

/// <summary>Bewertet die Beobachtungen einer Fahrt nach den Einstellungen des Nutzers</summary>
public static class StatusEvaluator
{
    public static TripStatus Evaluate(SavedConnection connection, IReadOnlyList<LegObservation?> observations, Settings settings)
    {
        var status = new TripStatus();
        for (var index = 0; index < connection.Legs.Count; index++)
        {
            LegObservation? observation = null;
            if (index < observations.Count)
            {
                observation = observations[index];
            }

            var result = EvaluateLeg(connection.Legs[index], observation, settings);
            status.Legs.Add(result);
            status.Overall = Worse(status.Overall, result.Status);
        }

        status.HasLiveData = status.Legs.Any(result => result.Observation is not null
            && result.Observation.Result == ObservationResult.Found
            && result.Observation.HasLiveData);

        EvaluateTransfers(status, settings);
        foreach (var transfer in status.Transfers)
        {
            if (transfer.AtRisk)
            {
                status.Overall = Worse(status.Overall, LegStatus.TransferAtRisk);
            }
        }

        if (status.Overall == LegStatus.OnTime && !status.HasLiveData)
        {
            status.Overall = LegStatus.PlannedOnly;
        }

        return status;
    }

    private static LegResult EvaluateLeg(Leg leg, LegObservation? observation, Settings settings)
    {
        var result = new LegResult(leg, observation);
        if (leg.IsWalk || observation is null)
        {
            return result;
        }

        if (observation.Result == ObservationResult.Unknown)
        {
            result.Status = LegStatus.Unknown;
            return result;
        }

        if (observation.Result == ObservationResult.NotFound)
        {
            result.Status = LegStatus.NotFound;
            return result;
        }

        if (observation.DepartureCancelled)
        {
            result.Status = LegStatus.Cancelled;
            return result;
        }

        if (observation.ArrivalCancelled)
        {
            result.Status = LegStatus.PartlyCancelled;
            return result;
        }

        if (observation.ArrivalMissing && observation.EndsAt is not null)
        {
            result.Status = LegStatus.PartlyCancelled;
            return result;
        }

        if (observation.ArrivalMissing)
        {
            result.Status = LegStatus.Unknown;
            return result;
        }

        var departureDelay = MinutesLate(observation.PlannedDeparture, observation.LiveDeparture);
        var arrivalDelay = MinutesLate(observation.PlannedArrival, observation.LiveArrival);
        result.DelayMinutes = Math.Max(departureDelay, arrivalDelay);
        if (result.DelayMinutes >= settings.DelayThresholdMinutes)
        {
            result.Status = LegStatus.Delayed;
        }

        var platformChanged = !string.IsNullOrEmpty(leg.PlannedPlatform)
            && !string.IsNullOrEmpty(observation.CurrentPlatform)
            && leg.PlannedPlatform != observation.CurrentPlatform;
        if (platformChanged)
        {
            result.NewPlatform = observation.CurrentPlatform;
            result.Status = Worse(result.Status, LegStatus.PlatformChanged);
        }

        return result;
    }

    private static void EvaluateTransfers(TripStatus status, Settings settings)
    {
        var rideIndexes = new List<int>();
        for (var index = 0; index < status.Legs.Count; index++)
        {
            if (!status.Legs[index].Leg.IsWalk)
            {
                rideIndexes.Add(index);
            }
        }

        for (var position = 0; position < rideIndexes.Count - 1; position++)
        {
            var arriving = status.Legs[rideIndexes[position]];
            var departing = status.Legs[rideIndexes[position + 1]];
            if (!CanCompare(arriving) || !CanCompare(departing))
            {
                continue;
            }

            var walkMinutes = 0;
            for (var index = rideIndexes[position] + 1; index < rideIndexes[position + 1]; index++)
            {
                var walk = status.Legs[index].Leg;
                walkMinutes += MinutesBetween(walk.PlannedDeparture, walk.PlannedArrival);
            }

            var arrivingObservation = arriving.Observation!;
            var departingObservation = departing.Observation!;
            var plannedGap = (int)(departingObservation.PlannedDeparture - arrivingObservation.PlannedArrival).TotalMinutes;
            var plannedSpare = Math.Max(0, plannedGap - walkMinutes);
            var requiredGap = walkMinutes + Math.Min(settings.MinTransferMinutes, plannedSpare);

            var expectedArrival = ExpectedArrival(arrivingObservation);
            var expectedDeparture = departingObservation.PlannedDeparture;
            if (departingObservation.LiveDeparture is not null)
            {
                expectedDeparture = departingObservation.LiveDeparture.Value;
            }

            var liveGap = (int)(expectedDeparture - expectedArrival).TotalMinutes;
            status.Transfers.Add(new TransferResult
            {
                ArrivingLegIndex = rideIndexes[position],
                DepartingLegIndex = rideIndexes[position + 1],
                SpareMinutes = liveGap - walkMinutes,
                AtRisk = liveGap < requiredGap
            });
        }
    }

    private static DateTime ExpectedArrival(LegObservation observation)
    {
        if (observation.LiveArrival is not null)
        {
            return observation.LiveArrival.Value;
        }

        if (observation.ArrivalKnown)
        {
            return observation.PlannedArrival;
        }

        var departureDelay = MinutesLate(observation.PlannedDeparture, observation.LiveDeparture);
        return observation.PlannedArrival.AddMinutes(departureDelay);
    }

    private static bool CanCompare(LegResult result)
    {
        if (result.Observation is null || result.Observation.Result != ObservationResult.Found)
        {
            return false;
        }

        return result.Status != LegStatus.Cancelled && result.Status != LegStatus.PartlyCancelled;
    }

    private static int MinutesLate(DateTime planned, DateTime? live)
    {
        if (live is null)
        {
            return 0;
        }

        var minutes = (int)(live.Value - planned).TotalMinutes;
        return Math.Max(0, minutes);
    }

    private static int MinutesBetween(TimeOnly start, TimeOnly end)
    {
        var minutes = (int)(end - start).TotalMinutes;
        return Math.Max(0, minutes);
    }

    private static LegStatus Worse(LegStatus first, LegStatus second)
    {
        if ((int)second > (int)first)
        {
            return second;
        }

        return first;
    }
}
