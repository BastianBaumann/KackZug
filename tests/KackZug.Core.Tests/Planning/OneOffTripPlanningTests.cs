using KackZug.Core.Models;
using KackZug.Core.Planning;

namespace KackZug.Core.Tests.Planning;

/// <summary>Prueft, dass Einzelfahrten nur an ihrem Tag eingeplant werden</summary>
public class OneOffTripPlanningTests
{
    private static readonly TimeZoneInfo Berlin = BerlinTime.Zone;
    private static readonly DateOnly Thursday = new DateOnly(2026, 10, 22);

    [Fact]
    public void CalendarShowsTripOnlyOnItsDate()
    {
        var data = new AppData();
        var trip = SampleData.CreateTrip("Konzert", Thursday);
        data.Trips.Add(trip);

        var onDate = TripCalendar.TripsOn(data, Thursday);
        var dayBefore = TripCalendar.TripsOn(data, Thursday.AddDays(-1));
        var dayAfter = TripCalendar.TripsOn(data, Thursday.AddDays(1));

        var plan = Assert.Single(onDate);
        Assert.Equal(trip.Id, plan.Id);
        Assert.Same(trip, plan.OneOffTrip);
        Assert.Null(plan.Shift);
        Assert.Equal("Konzert", plan.Name);
        Assert.Equal(TripDirection.Outbound, plan.Direction);
        Assert.Empty(dayBefore);
        Assert.Empty(dayAfter);
    }

    [Fact]
    public void CalendarShowsShiftAndTripOnTheSameDay()
    {
        var data = new AppData();
        var shift = SampleData.CreateShift("Fruehdienst");
        data.Shifts.Add(shift);
        data.CurrentWeek = WeekPlan.CreateEmpty(WeekPlan.MondayOf(Thursday));
        data.CurrentWeek.GetDay(DayOfWeek.Thursday).ShiftId = shift.Id;
        data.Trips.Add(SampleData.CreateTrip("Konzert", Thursday));

        var trips = TripCalendar.TripsOn(data, Thursday);

        Assert.Equal(2, trips.Count);
        Assert.Contains(trips, trip => trip.Shift is not null);
        Assert.Contains(trips, trip => trip.OneOffTrip is not null);
    }

    [Fact]
    public void PlannerPlansChecksOfTheTrip()
    {
        var data = new AppData();
        var trip = SampleData.CreateTrip("Konzert", Thursday);
        data.Trips.Add(trip);
        var now = At(Thursday.AddDays(-1), 12, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        var tripChecks = checks.Where(check => check.Kind != CheckKind.Maintenance).ToList();
        Assert.Equal(4, tripChecks.Count);
        Assert.All(tripChecks, check => Assert.Equal(trip.Id, check.ShiftId));
        Assert.All(tripChecks, check => Assert.Equal(Thursday, check.TravelDate));
        Assert.Equal(CheckKind.Evening, tripChecks[0].Kind);
        Assert.Equal(At(Thursday, 6, 27), tripChecks[3].At);
    }

    [Fact]
    public void PlannerSkipsTripOfAnotherDay()
    {
        var data = new AppData();
        data.Trips.Add(SampleData.CreateTrip("Konzert", Thursday.AddDays(5)));
        var now = At(Thursday, 12, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        Assert.DoesNotContain(checks, check => check.Kind != CheckKind.Maintenance);
    }

    [Fact]
    public void TripAndShiftGetDifferentRequestCodes()
    {
        var data = new AppData();
        var shift = SampleData.CreateShift("Fruehdienst");
        data.Shifts.Add(shift);
        data.CurrentWeek = WeekPlan.CreateEmpty(WeekPlan.MondayOf(Thursday));
        data.CurrentWeek.GetDay(DayOfWeek.Thursday).ShiftId = shift.Id;
        data.Trips.Add(SampleData.CreateTrip("Konzert", Thursday));
        var now = At(Thursday.AddDays(-1), 12, 0);

        var checks = CheckPlanner.Plan(data, now, Berlin);

        Assert.Equal(9, checks.Count);
        Assert.Equal(checks.Count, checks.Select(check => check.RequestCode).Distinct().Count());
    }

    [Fact]
    public void DateInThePastIsRefused()
    {
        var connection = SampleData.CreateTrip("Konzert", Thursday).Connection;
        var now = At(Thursday, 12, 0);

        Assert.Equal(TripDateProblem.Invalid, OneOffTripDate.Check(null, connection, now, Berlin));
        Assert.Equal(TripDateProblem.Past, OneOffTripDate.Check(Thursday.AddDays(-1), connection, now, Berlin));
        Assert.Equal(TripDateProblem.AlreadyOver, OneOffTripDate.Check(Thursday, connection, now, Berlin));
        Assert.Equal(TripDateProblem.None, OneOffTripDate.Check(Thursday.AddDays(1), connection, now, Berlin));
        Assert.Equal(TripDateProblem.None, OneOffTripDate.Check(Thursday, connection, At(Thursday, 6, 0), Berlin));
    }

    [Fact]
    public void DefaultNameIsStartAndDestination()
    {
        var connection = SampleData.CreateTrip("Konzert", Thursday).Connection;

        Assert.Equal("Musterstadt Hbf → Werkstor", OneOffTrip.DefaultName(connection));
    }

    private static DateTimeOffset At(DateOnly date, int hour, int minute)
    {
        return BerlinTime.ToInstant(date.ToDateTime(new TimeOnly(hour, minute)), Berlin);
    }
}
