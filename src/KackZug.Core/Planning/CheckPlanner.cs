using KackZug.Core.Models;

namespace KackZug.Core.Planning;

/// <summary>Berechnet alle Pruefzeitpunkte der naechsten 48 Stunden aus Wochenplan, Einzelfahrten und Einstellungen</summary>
public static class CheckPlanner
{
    public static readonly TimeSpan Window = TimeSpan.FromHours(48);
    public static readonly TimeOnly MaintenanceTime = new TimeOnly(0, 10);

    public static List<PlannedCheck> Plan(AppData data, DateTimeOffset now, TimeZoneInfo zone)
    {
        var checks = new List<PlannedCheck>();
        var end = now + Window;
        var today = DateOnly.FromDateTime(BerlinTime.ToWallClock(now, zone));

        checks.Add(CreateMaintenance(today, now, zone));

        for (var dayOffset = 0; dayOffset <= 3; dayOffset++)
        {
            var date = today.AddDays(dayOffset);
            foreach (var trip in TripCalendar.TripsOn(data, date))
            {
                AddTripChecks(checks, trip, data.Settings, now, end, zone);
            }
        }

        var unique = new Dictionary<string, PlannedCheck>();
        foreach (var check in checks)
        {
            check.RequestCode = StableHash.Of(check.Key);
            unique[check.Key] = check;
        }

        return unique.Values.OrderBy(check => check.At).ToList();
    }

    private static PlannedCheck CreateMaintenance(DateOnly today, DateTimeOffset now, TimeZoneInfo zone)
    {
        var at = BerlinTime.ToInstant(today.ToDateTime(MaintenanceTime), zone);
        if (at <= now)
        {
            at = BerlinTime.ToInstant(today.AddDays(1).ToDateTime(MaintenanceTime), zone);
        }

        return new PlannedCheck { Kind = CheckKind.Maintenance, At = at, TravelDate = today };
    }

    private static void AddTripChecks(List<PlannedCheck> checks, TripPlan trip, Settings settings, DateTimeOffset now, DateTimeOffset end, TimeZoneInfo zone)
    {
        var departure = BerlinTime.ToInstant(trip.FirstDeparture, zone);

        if (settings.EveningCheckTime is not null)
        {
            var eveningWallClock = trip.TravelDate.AddDays(-1).ToDateTime(settings.EveningCheckTime.Value);
            var evening = BerlinTime.ToInstant(eveningWallClock, zone);
            if (evening < departure)
            {
                AddIfInWindow(checks, CreateCheck(trip, CheckKind.Evening, evening, 0), now, end);
            }
        }

        foreach (var leadMinutes in settings.LeadTimesMinutes.Distinct())
        {
            if (leadMinutes <= 0)
            {
                continue;
            }

            var at = departure.AddMinutes(-leadMinutes);
            AddIfInWindow(checks, CreateCheck(trip, CheckKind.LeadTime, at, leadMinutes), now, end);
        }
    }

    private static PlannedCheck CreateCheck(TripPlan trip, CheckKind kind, DateTimeOffset at, int leadMinutes)
    {
        return new PlannedCheck
        {
            Kind = kind,
            At = at,
            TravelDate = trip.TravelDate,
            ShiftId = trip.Id,
            Direction = trip.Direction,
            LeadMinutes = leadMinutes
        };
    }

    private static void AddIfInWindow(List<PlannedCheck> checks, PlannedCheck check, DateTimeOffset now, DateTimeOffset end)
    {
        if (check.At > now && check.At <= end)
        {
            checks.Add(check);
        }
    }
}
