using KackZug.Core.Models;

namespace KackZug.Core.Planning;

/// <summary>Ermittelt, welche Fahrten an einem Tag laut Wochenplan anstehen</summary>
public static class TripCalendar
{
    public static List<TripPlan> TripsOn(AppData data, DateOnly date)
    {
        var trips = new List<TripPlan>();
        var dayPlan = DayPlanFor(data, date);
        if (dayPlan is null)
        {
            return trips;
        }

        var shift = data.FindShift(dayPlan.ShiftId);
        if (shift is null)
        {
            return trips;
        }

        foreach (var direction in new[] { TripDirection.Outbound, TripDirection.Return })
        {
            var connection = shift.GetConnection(direction);
            if (connection is null || connection.FirstRide is null)
            {
                continue;
            }

            if (!dayPlan.IsTripEnabled(direction))
            {
                continue;
            }

            trips.Add(new TripPlan(date, shift, direction, connection));
        }

        return trips;
    }

    private static DayPlan? DayPlanFor(AppData data, DateOnly date)
    {
        var week = data.CurrentWeek;
        if (week is null)
        {
            return null;
        }

        if (week.Contains(date))
        {
            return week.GetDay(date.DayOfWeek);
        }

        if (data.NextWeek is not null && data.NextWeek.Contains(date))
        {
            return data.NextWeek.GetDay(date.DayOfWeek);
        }

        var weekEnd = week.WeekStart.AddDays(7);
        var isNextWeek = date >= weekEnd && date < weekEnd.AddDays(7);
        if (isNextWeek && data.Settings.NewWeekMode == NewWeekMode.CopyPrevious)
        {
            return week.GetDay(date.DayOfWeek);
        }

        return null;
    }
}
