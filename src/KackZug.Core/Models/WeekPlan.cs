namespace KackZug.Core.Models;

/// <summary>Plan fuer eine Woche von Montag bis Sonntag</summary>
public sealed class WeekPlan
{
    public static IReadOnlyList<DayOfWeek> OrderedDays { get; } = new[]
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    };

    public DateOnly WeekStart { get; set; }

    public List<DayPlan> Days { get; set; } = new List<DayPlan>();

    public DayPlan GetDay(DayOfWeek day)
    {
        var existing = Days.FirstOrDefault(plan => plan.Day == day);
        if (existing is not null)
        {
            return existing;
        }

        var created = new DayPlan { Day = day };
        Days.Add(created);
        return created;
    }

    public DateOnly DateOf(DayOfWeek day)
    {
        var offset = ((int)day + 6) % 7;
        return WeekStart.AddDays(offset);
    }

    public static DateOnly MondayOf(DateOnly date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }

    public static WeekPlan CreateFrom(WeekPlan template, DateOnly weekStart, bool copyDays)
    {
        var week = CreateEmpty(weekStart);
        if (!copyDays)
        {
            return week;
        }

        foreach (var day in OrderedDays)
        {
            var source = template.GetDay(day);
            var target = week.GetDay(day);
            target.ShiftId = source.ShiftId;
            target.OutboundEnabled = source.OutboundEnabled;
            target.ReturnEnabled = source.ReturnEnabled;
        }

        return week;
    }

    public bool Contains(DateOnly date)
    {
        return date >= WeekStart && date < WeekStart.AddDays(7);
    }

    public static WeekPlan CreateEmpty(DateOnly weekStart)
    {
        var week = new WeekPlan { WeekStart = weekStart };
        foreach (var day in OrderedDays)
        {
            week.Days.Add(new DayPlan { Day = day });
        }

        return week;
    }
}
