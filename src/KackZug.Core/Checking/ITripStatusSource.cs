using KackZug.Core.Models;

namespace KackZug.Core.Checking;

/// <summary>Fragt den aktuellen Stand einer Teilstrecke an einem bestimmten Tag ab</summary>
public interface ITripStatusSource
{
    Task<LegObservation> ObserveAsync(Leg leg, DateOnly travelDate, CancellationToken cancellationToken);
}
