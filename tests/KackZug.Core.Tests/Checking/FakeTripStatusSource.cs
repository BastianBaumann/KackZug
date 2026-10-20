using KackZug.Core.Checking;
using KackZug.Core.Models;

namespace KackZug.Core.Tests.Checking;

/// <summary>Liefert vorbereitete Beobachtungen, Verspaetung pro Zugname einstellbar</summary>
public sealed class FakeTripStatusSource : ITripStatusSource
{
    public Dictionary<string, int> DelayByName { get; } = new Dictionary<string, int>();

    public HashSet<string> CancelledNames { get; } = new HashSet<string>();

    public bool Offline { get; set; }

    public int Calls { get; private set; }

    public Task<LegObservation> ObserveAsync(Leg leg, DateOnly travelDate, CancellationToken cancellationToken)
    {
        Calls++;
        var plannedDeparture = leg.DepartureOn(travelDate);
        var plannedArrival = leg.ArrivalOn(travelDate);
        if (Offline)
        {
            return Task.FromResult(LegObservation.Unknown(plannedDeparture, plannedArrival, "offline"));
        }

        var delay = 0;
        if (DelayByName.TryGetValue(leg.Name, out var configuredDelay))
        {
            delay = configuredDelay;
        }

        var observation = new LegObservation
        {
            Result = ObservationResult.Found,
            PlannedDeparture = plannedDeparture,
            PlannedArrival = plannedArrival,
            LiveDeparture = plannedDeparture.AddMinutes(delay),
            LiveArrival = plannedArrival.AddMinutes(delay),
            ArrivalKnown = true,
            CurrentPlatform = leg.PlannedPlatform,
            DepartureCancelled = CancelledNames.Contains(leg.Name)
        };
        return Task.FromResult(observation);
    }
}
