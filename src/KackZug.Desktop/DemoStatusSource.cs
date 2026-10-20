#if DEBUG
using KackZug.Core.Checking;
using KackZug.Core.Models;

namespace KackZug.Desktop;

/// <summary>Erfundene Live-Daten fuer die Screenshot-Tour: der RE7 ist 8 Minuten spaet</summary>
internal sealed class DemoStatusSource : ITripStatusSource
{
    public Task<LegObservation> ObserveAsync(Leg leg, DateOnly travelDate, CancellationToken cancellationToken)
    {
        var plannedDeparture = leg.DepartureOn(travelDate);
        var plannedArrival = leg.ArrivalOn(travelDate);
        var delay = 0;
        if (leg.Name == "RE7")
        {
            delay = 8;
        }

        var observation = new LegObservation
        {
            Result = ObservationResult.Found,
            PlannedDeparture = plannedDeparture,
            PlannedArrival = plannedArrival,
            LiveDeparture = plannedDeparture.AddMinutes(delay),
            LiveArrival = plannedArrival.AddMinutes(delay),
            ArrivalKnown = true,
            CurrentPlatform = leg.PlannedPlatform
        };
        return Task.FromResult(observation);
    }
}
#endif
