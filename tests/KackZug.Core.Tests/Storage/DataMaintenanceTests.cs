using KackZug.Core.Models;
using KackZug.Core.Storage;

namespace KackZug.Core.Tests.Storage;

/// <summary>Prueft Wochenwechsel und Aufraeumen</summary>
public class DataMaintenanceTests
{
    private static readonly DateOnly Wednesday = new DateOnly(2026, 10, 21);
    private static readonly DateOnly Monday = new DateOnly(2026, 10, 19);
    private static readonly DateOnly NextMonday = new DateOnly(2026, 10, 26);

    [Fact]
    public void CreatesCurrentWeekWhenMissing()
    {
        var data = new AppData();

        var changed = DataMaintenance.Apply(data, Wednesday);

        Assert.True(changed);
        Assert.Equal(Monday, data.CurrentWeek!.WeekStart);
        Assert.Equal(7, data.CurrentWeek.Days.Count);
    }

    [Fact]
    public void SameWeekStaysUntouched()
    {
        var data = new AppData();
        DataMaintenance.Apply(data, Monday);

        var changed = DataMaintenance.Apply(data, Wednesday);

        Assert.False(changed);
    }

    [Fact]
    public void NewWeekCopiesPreviousPlan()
    {
        var data = DataWithPlannedTuesday(NewWeekMode.CopyPrevious);

        DataMaintenance.Apply(data, NextMonday);

        Assert.Equal(NextMonday, data.CurrentWeek!.WeekStart);
        var tuesday = data.CurrentWeek.GetDay(DayOfWeek.Tuesday);
        Assert.Equal(data.Shifts[0].Id, tuesday.ShiftId);
        Assert.False(tuesday.ReturnEnabled);
    }

    [Fact]
    public void NewWeekStartsEmptyWhenWanted()
    {
        var data = DataWithPlannedTuesday(NewWeekMode.Empty);

        DataMaintenance.Apply(data, NextMonday);

        Assert.All(data.CurrentWeek!.Days, day => Assert.Null(day.ShiftId));
    }

    [Fact]
    public void SundayBelongsToTheWeekBefore()
    {
        var sunday = new DateOnly(2026, 10, 25);

        Assert.Equal(Monday, WeekPlan.MondayOf(sunday));
        Assert.Equal(sunday, WeekPlan.CreateEmpty(Monday).DateOf(DayOfWeek.Sunday));
    }

    [Fact]
    public void RemovesCheckStatesOlderThanYesterday()
    {
        var data = new AppData();
        data.CheckStates.Add(new CheckState { Date = Wednesday.AddDays(-2) });
        data.CheckStates.Add(new CheckState { Date = Wednesday.AddDays(-1) });
        data.CheckStates.Add(new CheckState { Date = Wednesday });

        DataMaintenance.Apply(data, Wednesday);

        Assert.Equal(2, data.CheckStates.Count);
    }

    [Fact]
    public void DaysOfDeletedShiftsBecomeFree()
    {
        var data = DataWithPlannedTuesday(NewWeekMode.CopyPrevious);
        data.Shifts.Clear();

        DataMaintenance.Apply(data, Monday.AddDays(1));

        Assert.Null(data.CurrentWeek!.GetDay(DayOfWeek.Tuesday).ShiftId);
    }

    [Fact]
    public void RemoveShiftFreesItsDays()
    {
        var data = DataWithPlannedTuesday(NewWeekMode.CopyPrevious);

        data.RemoveShift(data.Shifts[0].Id);

        Assert.Empty(data.Shifts);
        Assert.Null(data.CurrentWeek!.GetDay(DayOfWeek.Tuesday).ShiftId);
    }

    private static AppData DataWithPlannedTuesday(NewWeekMode mode)
    {
        var data = new AppData();
        data.Settings.NewWeekMode = mode;
        var shift = SampleData.CreateShift("Fruehdienst");
        data.Shifts.Add(shift);
        data.CurrentWeek = WeekPlan.CreateEmpty(Monday);
        var tuesday = data.CurrentWeek.GetDay(DayOfWeek.Tuesday);
        tuesday.ShiftId = shift.Id;
        tuesday.ReturnEnabled = false;
        return data;
    }
}
