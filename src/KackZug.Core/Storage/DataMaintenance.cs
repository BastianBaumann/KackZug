using KackZug.Core.Models;

namespace KackZug.Core.Storage;

/// <summary>Haelt die Daten aktuell: legt die neue Woche an und raeumt alte Pruefergebnisse weg</summary>
public static class DataMaintenance
{
    public static bool Apply(AppData data, DateOnly today)
    {
        var changed = false;

        if (RollOverWeek(data, today))
        {
            changed = true;
        }

        if (FreeDaysOfMissingShifts(data))
        {
            changed = true;
        }

        if (RemoveOldCheckStates(data, today))
        {
            changed = true;
        }

        return changed;
    }

    private static bool RollOverWeek(AppData data, DateOnly today)
    {
        var monday = WeekPlan.MondayOf(today);
        var previous = data.CurrentWeek;

        if (previous is not null && previous.WeekStart == monday)
        {
            return CompleteDays(previous);
        }

        var newWeek = WeekPlan.CreateEmpty(monday);
        var copyPrevious = previous is not null && data.Settings.NewWeekMode == NewWeekMode.CopyPrevious;
        if (copyPrevious)
        {
            foreach (var day in WeekPlan.OrderedDays)
            {
                var copy = previous!.GetDay(day).Copy();
                var target = newWeek.GetDay(day);
                target.ShiftId = copy.ShiftId;
                target.OutboundEnabled = copy.OutboundEnabled;
                target.ReturnEnabled = copy.ReturnEnabled;
            }
        }

        data.CurrentWeek = newWeek;
        return true;
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
        if (data.CurrentWeek is null)
        {
            return false;
        }

        var changed = false;
        foreach (var day in data.CurrentWeek.Days)
        {
            if (day.ShiftId is not null && data.FindShift(day.ShiftId) is null)
            {
                day.ShiftId = null;
                changed = true;
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
