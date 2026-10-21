using KackZug.Core.Models;
using KackZug.Core.Planning;

namespace KackZug.Core.Tests.Planning;

/// <summary>Prueft die Berechnung der Pruefzeitpunkte inklusive Zeitumstellung</summary>
public class CheckPlannerTests
{
    private static readonly TimeZoneInfo Berlin = BerlinTime.Zone;

    [Fact]
    public void PlansEveningAndLeadTimeChecks()
    {
        var data = DataWithShiftOn(new DateOnly(2026, 10, 19), DayOfWeek.Wednesday);
        var now = At(2026, 10, 20, 12, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        var tripChecks = checks.Where(check => check.Kind != CheckKind.Maintenance).ToList();
        Assert.Equal(4, tripChecks.Count);
        Assert.Equal(At(2026, 10, 20, 20, 0), tripChecks[0].At);
        Assert.Equal(CheckKind.Evening, tripChecks[0].Kind);
        Assert.Equal(At(2026, 10, 21, 5, 12), tripChecks[1].At);
        Assert.Equal(At(2026, 10, 21, 5, 57), tripChecks[2].At);
        Assert.Equal(At(2026, 10, 21, 6, 27), tripChecks[3].At);
        Assert.All(tripChecks, check => Assert.Equal(new DateOnly(2026, 10, 21), check.TravelDate));
    }

    [Fact]
    public void PastChecksAreSkipped()
    {
        var data = DataWithShiftOn(new DateOnly(2026, 10, 19), DayOfWeek.Wednesday);
        var now = At(2026, 10, 21, 6, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        var tripChecks = checks.Where(check => check.Kind != CheckKind.Maintenance).ToList();
        Assert.Single(tripChecks);
        Assert.Equal(15, tripChecks[0].LeadMinutes);
    }

    [Fact]
    public void MaintenanceRunsAfterMidnight()
    {
        var data = new AppData();
        var now = At(2026, 10, 20, 23, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        var maintenance = Assert.Single(checks);
        Assert.Equal(CheckKind.Maintenance, maintenance.Kind);
        Assert.Equal(At(2026, 10, 21, 0, 10), maintenance.At);
    }

    [Fact]
    public void DisabledTripAndFreeDayGetNoChecks()
    {
        var data = DataWithShiftOn(new DateOnly(2026, 10, 19), DayOfWeek.Wednesday);
        data.CurrentWeek!.GetDay(DayOfWeek.Wednesday).OutboundEnabled = false;
        var now = At(2026, 10, 20, 12, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        Assert.Single(checks);
    }

    [Fact]
    public void MondayAfterWeekEndUsesCopiedPlan()
    {
        var data = DataWithShiftOn(new DateOnly(2026, 10, 19), DayOfWeek.Monday);
        var now = At(2026, 10, 25, 12, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        Assert.Contains(checks, check => check.TravelDate == new DateOnly(2026, 10, 26) && check.Kind == CheckKind.Evening);
    }

    [Fact]
    public void PlannedNextWeekWinsOverCopy()
    {
        var data = DataWithShiftOn(new DateOnly(2026, 10, 19), DayOfWeek.Monday);
        data.EnsureNextWeek().GetDay(DayOfWeek.Monday).ShiftId = null;
        var now = At(2026, 10, 25, 12, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        Assert.DoesNotContain(checks, check => check.TravelDate == new DateOnly(2026, 10, 26));
    }

    [Fact]
    public void MondayAfterWeekEndIsEmptyWhenWeeksStartEmpty()
    {
        var data = DataWithShiftOn(new DateOnly(2026, 10, 19), DayOfWeek.Monday);
        data.Settings.NewWeekMode = NewWeekMode.Empty;
        var now = At(2026, 10, 25, 12, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        Assert.DoesNotContain(checks, check => check.Kind != CheckKind.Maintenance);
    }

    [Fact]
    public void AutumnClockChangeKeepsWallClockTimes()
    {
        var data = DataWithShiftOn(new DateOnly(2026, 10, 19), DayOfWeek.Sunday);
        var now = At(2026, 10, 24, 21, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        var leadCheck = checks.Single(check => check.Kind == CheckKind.LeadTime && check.LeadMinutes == 90);
        Assert.Equal(new DateTimeOffset(2026, 10, 25, 5, 12, 0, TimeSpan.FromHours(1)), leadCheck.At);
    }

    [Fact]
    public void SpringClockChangeKeepsWallClockTimes()
    {
        var data = DataWithShiftOn(new DateOnly(2026, 3, 23), DayOfWeek.Sunday);
        data.CurrentWeek!.WeekStart = new DateOnly(2026, 3, 23);
        var now = new DateTimeOffset(2026, 3, 28, 21, 0, 0, TimeSpan.FromHours(1));

        var checks = CheckPlanner.Plan(data, now, Berlin);

        var leadCheck = checks.Single(check => check.Kind == CheckKind.LeadTime && check.LeadMinutes == 90);
        Assert.Equal(new DateTimeOffset(2026, 3, 29, 5, 12, 0, TimeSpan.FromHours(2)), leadCheck.At);
    }

    [Fact]
    public void RequestCodesAreStableAndUnique()
    {
        var data = DataWithShiftOn(new DateOnly(2026, 10, 19), DayOfWeek.Wednesday);
        var now = At(2026, 10, 20, 12, 0);

        var first = CheckPlanner.Plan(data, now, Berlin);
        var second = CheckPlanner.Plan(data, now.AddMinutes(5), Berlin);

        Assert.Equal(first.Select(check => check.RequestCode), second.Select(check => check.RequestCode));
        Assert.Equal(first.Count, first.Select(check => check.RequestCode).Distinct().Count());
    }

    [Fact]
    public void NightReturnAfterMidnightIsPlannedOnTheRightDay()
    {
        var data = DataWithShiftOn(new DateOnly(2026, 10, 19), DayOfWeek.Wednesday);
        var night = data.Shifts[0].Outbound!;
        night.Legs[0].PlannedDeparture = new TimeOnly(0, 20);
        night.Legs[0].DepartureDayOffset = 1;
        var now = At(2026, 10, 21, 12, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        var leadCheck = checks.Single(check => check.Kind == CheckKind.LeadTime && check.LeadMinutes == 15);
        Assert.Equal(At(2026, 10, 22, 0, 5), leadCheck.At);
    }

    private static AppData DataWithShiftOn(DateOnly weekStart, DayOfWeek day)
    {
        var data = new AppData();
        var shift = SampleData.CreateShift("Fruehdienst");
        data.Shifts.Add(shift);
        data.CurrentWeek = WeekPlan.CreateEmpty(weekStart);
        var dayPlan = data.CurrentWeek.GetDay(day);
        dayPlan.ShiftId = shift.Id;
        return data;
    }

    private static DateTimeOffset At(int year, int month, int day, int hour, int minute)
    {
        return BerlinTime.ToInstant(new DateTime(year, month, day, hour, minute, 0), Berlin);
    }
}
