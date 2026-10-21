using KackZug.Core.Localization;
using KackZug.Core.Models;
using KackZug.Core.Planning;

namespace KackZug.Core.Checking;

/// <summary>Prueft eine Fahrt, bewertet sie und entscheidet, ob der Nutzer benachrichtigt wird</summary>
public sealed class TripChecker
{
    private readonly ITripStatusSource _source;
    private readonly IClock _clock;

    public TripChecker(ITripStatusSource source, IClock clock)
    {
        _source = source;
        _clock = clock;
    }

    public async Task<TripCheckOutcome?> CheckAsync(AppData data, DateOnly travelDate, Guid shiftId, TripDirection direction, CheckContext context, CancellationToken cancellationToken)
    {
        var shift = data.FindShift(shiftId);
        if (shift is null)
        {
            return null;
        }

        var connection = shift.GetConnection(direction);
        if (connection is null || connection.FirstRide is null)
        {
            return null;
        }

        var observations = new List<LegObservation?>();
        foreach (var leg in connection.Legs)
        {
            if (leg.IsWalk)
            {
                observations.Add(null);
                continue;
            }

            var observation = await _source.ObserveAsync(leg, travelDate, cancellationToken);
            observations.Add(observation);
        }

        var status = StatusEvaluator.Evaluate(connection, observations, data.Settings);
        var alert = AlertComposer.Compose(shift, direction, travelDate, status);
        alert.RepeatUntil = BerlinTime.ToInstant(connection.FirstRide.DepartureOn(travelDate), BerlinTime.Zone);
        var state = FindOrCreateState(data, travelDate, shiftId, direction);
        var shouldNotify = ShouldNotify(state, alert, context, data.Settings);

        state.LastStatusHash = alert.StatusHash;
        state.LastStatus = status.Overall;
        state.LastSummary = alert.Title;
        state.LastBadgeText = alert.BadgeText;
        state.LastBadgeLanguage = Language.Culture.TwoLetterISOLanguageName;
        state.LastCheckedAt = _clock.Now;
        if (shouldNotify)
        {
            state.LastNotifiedAt = _clock.Now;
        }

        return new TripCheckOutcome(status, alert, shouldNotify);
    }

    private static bool ShouldNotify(CheckState state, TripAlert alert, CheckContext context, Settings settings)
    {
        var changed = state.LastStatusHash != alert.StatusHash;
        if (alert.Severity == AlertSeverity.Problem)
        {
            return changed;
        }

        if (alert.Status == LegStatus.Unknown)
        {
            return context == CheckContext.LastLeadTime && changed;
        }

        if (alert.Status == LegStatus.PlannedOnly)
        {
            return context == CheckContext.Evening || (context == CheckContext.LastLeadTime && changed);
        }

        var wasProblem = state.LastStatus is not null && AlertComposer.SeverityOf(state.LastStatus.Value) == AlertSeverity.Problem;
        if (wasProblem)
        {
            return true;
        }

        return context == CheckContext.Evening && settings.EveningOkNotification;
    }

    private static CheckState FindOrCreateState(AppData data, DateOnly travelDate, Guid shiftId, TripDirection direction)
    {
        var state = data.CheckStates.FirstOrDefault(item => item.Date == travelDate && item.ShiftId == shiftId && item.Direction == direction);
        if (state is not null)
        {
            return state;
        }

        state = new CheckState
        {
            Date = travelDate,
            ShiftId = shiftId,
            Direction = direction
        };
        data.CheckStates.Add(state);
        return state;
    }
}
