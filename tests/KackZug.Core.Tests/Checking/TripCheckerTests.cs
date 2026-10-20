using KackZug.Core.Checking;
using KackZug.Core.Models;

namespace KackZug.Core.Tests.Checking;

/// <summary>Prueft Meldungstexte und dass dieselbe Stoerung nicht doppelt gemeldet wird</summary>
public class TripCheckerTests
{
    private static readonly DateOnly Day = new DateOnly(2026, 10, 21);
    private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 10, 21, 5, 0, 0, TimeSpan.FromHours(2));

    [Fact]
    public async Task CancellationIsReportedOnceWithClearText()
    {
        var (data, shift) = CreateData();
        var source = new FakeTripStatusSource();
        source.CancelledNames.Add("RE7");
        var checker = new TripChecker(source, new FixedClock(Now));

        var first = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);
        var second = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);

        Assert.True(first!.ShouldNotify);
        Assert.Equal(AlertSeverity.Problem, first.Alert.Severity);
        Assert.Equal("Frühdienst Hinfahrt: RE7 06:42 fällt aus", first.Alert.Title);
        Assert.Contains("Musterstadt Hbf → Beispielhausen: fällt aus", first.Alert.Text);
        Assert.Contains("DB Navigator", first.Alert.Text);
        Assert.Equal("FÄLLT AUS", first.Alert.Headline);
        Assert.Equal("FRÜHDIENST · HINFAHRT · MI 21.10.", first.Alert.Kicker);
        Assert.Equal("RE7 · 06:42 Musterstadt Hbf → Beispielhausen", first.Alert.TripLine);
        Assert.Equal(new DateTimeOffset(2026, 10, 21, 6, 42, 0, TimeSpan.FromHours(2)), first.Alert.RepeatUntil);
        Assert.False(second!.ShouldNotify);
        var state = Assert.Single(data.CheckStates);
        Assert.Equal(LegStatus.Cancelled, state.LastStatus);
    }

    [Fact]
    public async Task GrowingDelayIsReportedAgain()
    {
        var (data, shift) = CreateData();
        var source = new FakeTripStatusSource();
        source.DelayByName["RE7"] = 6;
        source.DelayByName["STR 2"] = 6;
        var checker = new TripChecker(source, new FixedClock(Now));

        var first = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);
        source.DelayByName["RE7"] = 8;
        source.DelayByName["STR 2"] = 8;
        var sameBucket = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);
        source.DelayByName["RE7"] = 12;
        source.DelayByName["STR 2"] = 12;
        var larger = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);

        Assert.True(first!.ShouldNotify);
        Assert.Equal("Frühdienst Hinfahrt: RE7 06:42 +6 Min.", first.Alert.Title);
        Assert.False(sameBucket!.ShouldNotify);
        Assert.True(larger!.ShouldNotify);
    }

    [Fact]
    public async Task MissedTransferNamesTheStation()
    {
        var (data, shift) = CreateData();
        var source = new FakeTripStatusSource();
        source.DelayByName["RE7"] = 8;
        var checker = new TripChecker(source, new FixedClock(Now));

        var outcome = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);

        Assert.Equal(LegStatus.TransferAtRisk, outcome!.Status.Overall);
        Assert.Equal("Frühdienst Hinfahrt: Anschluss in Beispielhausen gefährdet", outcome.Alert.Title);
        Assert.Equal("ANSCHLUSS GEFÄHRDET", outcome.Alert.Headline);
        Assert.Contains("Umstieg in Beispielhausen: nur noch 0 Min. Puffer", outcome.Alert.Text);
    }

    [Fact]
    public async Task OnTimeInfoOnlyOnEveningCheck()
    {
        var (data, shift) = CreateData();
        var checker = new TripChecker(new FakeTripStatusSource(), new FixedClock(Now));

        var lead = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);
        var evening = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.Evening, CancellationToken.None);

        Assert.False(lead!.ShouldNotify);
        Assert.True(evening!.ShouldNotify);
        Assert.Equal(AlertSeverity.Info, evening.Alert.Severity);
        Assert.Equal("Frühdienst Hinfahrt: RE7 06:42 fährt planmäßig", evening.Alert.Title);
    }

    [Fact]
    public async Task WithoutRealtimeEveningSaysOnlyPlanned()
    {
        var (data, shift) = CreateData();
        data.Settings.EveningOkNotification = false;
        var source = new FakeTripStatusSource { NoLiveData = true };
        var checker = new TripChecker(source, new FixedClock(Now));

        var lead = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);
        var evening = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.Evening, CancellationToken.None);

        Assert.Equal(LegStatus.PlannedOnly, evening!.Status.Overall);
        Assert.False(lead!.ShouldNotify);
        Assert.True(evening.ShouldNotify);
        Assert.Equal(AlertSeverity.Info, evening.Alert.Severity);
        Assert.Equal("Frühdienst Hinfahrt: RE7 06:42 fährt laut Fahrplan", evening.Alert.Title);
        Assert.Contains("Echtzeitdaten gibt es erst kurz vor der Abfahrt", evening.Alert.Text);
        Assert.DoesNotContain("pünktlich", evening.Alert.Text);
    }

    [Fact]
    public async Task ProblemThatIsSolvedGivesAllClear()
    {
        var (data, shift) = CreateData();
        var source = new FakeTripStatusSource();
        source.CancelledNames.Add("RE7");
        var checker = new TripChecker(source, new FixedClock(Now));
        await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);
        source.CancelledNames.Clear();

        var outcome = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);

        Assert.True(outcome!.ShouldNotify);
        Assert.Equal(AlertSeverity.Info, outcome.Alert.Severity);
    }

    [Fact]
    public async Task UnknownIsOnlyReportedOnLastCheck()
    {
        var (data, shift) = CreateData();
        var source = new FakeTripStatusSource { Offline = true };
        var checker = new TripChecker(source, new FixedClock(Now));

        var early = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);
        data.CheckStates.Clear();
        var last = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LastLeadTime, CancellationToken.None);

        Assert.False(early!.ShouldNotify);
        Assert.True(last!.ShouldNotify);
        Assert.Contains("konnte nicht geprüft werden", last.Alert.Title);
    }

    [Fact]
    public async Task MissingConnectionGivesNoOutcome()
    {
        var (data, shift) = CreateData();
        var source = new FakeTripStatusSource();
        var checker = new TripChecker(source, new FixedClock(Now));

        var outcome = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Return, CheckContext.LeadTime, CancellationToken.None);

        Assert.Null(outcome);
        Assert.Equal(0, source.Calls);
    }

    private static (AppData Data, Shift Shift) CreateData()
    {
        var data = new AppData();
        var shift = SampleData.CreateShift("Frühdienst");
        data.Shifts.Add(shift);
        return (data, shift);
    }
}
