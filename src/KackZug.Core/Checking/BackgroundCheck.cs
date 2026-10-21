using KackZug.Core.Localization;
using KackZug.Core.Models;
using KackZug.Core.Planning;
using KackZug.Core.Storage;

namespace KackZug.Core.Checking;

/// <summary>Fuehrt eine geplante Pruefung ohne Oberflaeche aus, so wie sie der Alarm auf dem Handy startet</summary>
public sealed class BackgroundCheck
{
    public static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(2);

    private readonly AppDataStore _store;
    private readonly ITripStatusSource _source;
    private readonly IClock _clock;

    public BackgroundCheck(AppDataStore store, ITripStatusSource source, IClock clock)
    {
        _store = store;
        _source = source;
        _clock = clock;
    }

    public async Task<BackgroundCheckResult> RunAsync(PlannedCheck check, bool isRetry, CancellationToken cancellationToken)
    {
        var result = new BackgroundCheckResult();
        var data = _store.Load();
        Language.Apply(data.Settings.Language);
        DataMaintenance.Apply(data, _clock.Now, BerlinTime.Zone);

        if (check.Kind != CheckKind.Maintenance && IsStillPlanned(data, check))
        {
            var checker = new TripChecker(_source, _clock);
            var context = ContextFor(check, data.Settings);
            var outcome = await checker.CheckAsync(data, check.TravelDate, check.ShiftId, check.Direction, context, cancellationToken);
            if (outcome is not null)
            {
                if (outcome.ShouldNotify)
                {
                    result.AlertToShow = outcome.Alert;
                }

                var couldNotCheck = outcome.Status.Overall == LegStatus.Unknown;
                result.RetrySoon = couldNotCheck && !isRetry;
            }
        }

        _store.Save(data);
        result.NextChecks = CheckPlanner.Plan(data, _clock.Now, BerlinTime.Zone);
        return result;
    }

    public static List<PlannedCheck> PlanFromFile(AppDataStore store, IClock clock)
    {
        var data = store.Load();
        return CheckPlanner.Plan(data, clock.Now, BerlinTime.Zone);
    }

    private static bool IsStillPlanned(AppData data, PlannedCheck check)
    {
        var trips = TripCalendar.TripsOn(data, check.TravelDate);
        return trips.Any(trip => trip.Id == check.ShiftId && trip.Direction == check.Direction);
    }

    private static CheckContext ContextFor(PlannedCheck check, Settings settings)
    {
        if (check.Kind == CheckKind.Evening)
        {
            return CheckContext.Evening;
        }

        var positiveLeadTimes = settings.LeadTimesMinutes.Where(minutes => minutes > 0).ToList();
        if (positiveLeadTimes.Count > 0 && check.LeadMinutes == positiveLeadTimes.Min())
        {
            return CheckContext.LastLeadTime;
        }

        return CheckContext.LeadTime;
    }
}
