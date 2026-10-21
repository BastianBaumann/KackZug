using KackZug.Core.Models;
using KackZug.Core.Planning;

namespace KackZug.Core.Storage;

/// <summary>Haelt die Daten aktuell: legt die neue Woche an, raeumt vorbei gefahrene Einzelfahrten und alte Pruefergebnisse weg</summary>
public static class DataMaintenance
{
    public static bool Apply(AppData data, DateTimeOffset now, TimeZoneInfo zone)
    {
        var today = DateOnly.FromDateTime(BerlinTime.ToWallClock(now, zone));
        var changed = false;

        if (RollOverWeek(data, today))
        {
            changed = true;
        }

        if (FreeDaysOfMissingShifts(data))
        {
            changed = true;
        }

        if (RemoveFinishedTrips(data, now, zone))
        {
            changed = true;
        }

        if (RemoveOldCheckStates(data, today))
        {
            changed = true;
        }

        return changed;
    }

    private static bool RemoveFinishedTrips(AppData data, DateTimeOffset now, TimeZoneInfo zone)
    {
        var finished = data.Trips.Where(trip => OneOffTripDate.IsOver(trip.Connection, trip.TravelDate, now, zone)).ToList();
        foreach (var trip in finished)
        {
            data.RemoveTrip(trip.Id);
        }

        return finished.Count > 0;
    }

    private static bool RollOverWeek(AppData data, DateOnly today)
    {
        var monday = WeekPlan.MondayOf(today);
        var previous = data.CurrentWeek;
        var changed = false;

        if (previous is not null && previous.WeekStart == monday)
        {
            changed = CompleteDays(previous);
        }
        else if (data.NextWeek is not null && data.NextWeek.WeekStart == monday)
        {
            data.CurrentWeek = data.NextWeek;
            data.NextWeek = null;
            CompleteDays(data.CurrentWeek);
            changed = true;
        }
        else
        {
            var copyPrevious = previous is not null && data.Settings.NewWeekMode == NewWeekMode.CopyPrevious;
            if (copyPrevious)
            {
                data.CurrentWeek = WeekPlan.CreateFrom(previous!, monday, true);
            }
            else
            {
                data.CurrentWeek = WeekPlan.CreateEmpty(monday);
            }

            changed = true;
        }

        if (data.NextWeek is not null && data.NextWeek.WeekStart != monday.AddDays(7))
        {
            data.NextWeek = null;
            changed = true;
        }

        if (data.NextWeek is not null && CompleteDays(data.NextWeek))
        {
            changed = true;
        }

        return changed;
    }

    private static bool CompleteDays(WeekPlan week)
    {
        var dayCountBefore = week.Days.Count;
        foreach (var day in WeekPlan.OrderedDays)
        {
            week.GetDay(day);
        }

        return week.Days.Count != dayCountBefore;
    }

    private static bool FreeDaysOfMissingShifts(AppData data)
    {
        var changed = false;
        foreach (var week in new[] { data.CurrentWeek, data.NextWeek })
        {
            if (week is null)
            {
                continue;
            }

            foreach (var day in week.Days)
            {
                if (day.ShiftId is not null && data.FindShift(day.ShiftId) is null)
                {
                    day.ShiftId = null;
                    changed = true;
                }
            }
        }

        return changed;
    }

    private static bool RemoveOldCheckStates(AppData data, DateOnly today)
    {
        var yesterday = today.AddDays(-1);
        var removed = data.CheckStates.RemoveAll(state => state.Date < yesterday);
        return removed > 0;
    }
}
