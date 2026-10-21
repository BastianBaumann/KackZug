using KackZug.Core.Checking;
using KackZug.Core.Models;
using KackZug.Core.Planning;
using KackZug.Core.Storage;

namespace KackZug.Core.Tests.Checking;

/// <summary>Prueft die Pruefung im Hintergrund, wie sie der Alarm ausloest</summary>
public sealed class BackgroundCheckTests : IDisposable
{
    private static readonly DateOnly Wednesday = new DateOnly(2026, 10, 21);
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "kackzug-bg-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    [Fact]
    public async Task CancelledTrainIsReportedAndSaved()
    {
        var (store, shift) = CreateStore();
        var source = new FakeTripStatusSource();
        source.CancelledNames.Add("RE7");
        var check = new BackgroundCheck(store, source, Clock(5, 0));

        var result = await check.RunAsync(LeadCheck(shift, 90), false, CancellationToken.None);

        Assert.NotNull(result.AlertToShow);
        Assert.Equal(LegStatus.Cancelled, result.AlertToShow.Status);
        Assert.False(result.RetrySoon);
        Assert.Single(store.Load().CheckStates);
        Assert.Contains(result.NextChecks, item => item.Kind == CheckKind.LeadTime && item.LeadMinutes == 45);
    }

    [Fact]
    public async Task OfflineAsksForOneRetry()
    {
        var (store, shift) = CreateStore();
        var source = new FakeTripStatusSource { Offline = true };

        var first = await new BackgroundCheck(store, source, Clock(5, 0)).RunAsync(LeadCheck(shift, 90), false, CancellationToken.None);
        var retry = await new BackgroundCheck(store, source, Clock(5, 2)).RunAsync(LeadCheck(shift, 90), true, CancellationToken.None);

        Assert.True(first.RetrySoon);
        Assert.Null(first.AlertToShow);
        Assert.False(retry.RetrySoon);
    }

    [Fact]
    public async Task LastLeadTimeReportsThatCheckingFailed()
    {
        var (store, shift) = CreateStore();
        var source = new FakeTripStatusSource { Offline = true };

        var result = await new BackgroundCheck(store, source, Clock(6, 27)).RunAsync(LeadCheck(shift, 15), false, CancellationToken.None);

        Assert.NotNull(result.AlertToShow);
        Assert.Equal(LegStatus.Unknown, result.AlertToShow.Status);
    }

    [Fact]
    public async Task TripThatWasSwitchedOffIsNotChecked()
    {
        var (store, shift) = CreateStore();
        var data = store.Load();
        data.CurrentWeek!.GetDay(DayOfWeek.Wednesday).OutboundEnabled = false;
        store.Save(data);
        var source = new FakeTripStatusSource();

        var result = await new BackgroundCheck(store, source, Clock(5, 0)).RunAsync(LeadCheck(shift, 90), false, CancellationToken.None);

        Assert.Equal(0, source.Calls);
        Assert.Null(result.AlertToShow);
    }

    [Fact]
    public async Task MaintenanceOnlyPlansAgain()
    {
        var (store, _) = CreateStore();
        var source = new FakeTripStatusSource();
        var maintenance = new PlannedCheck { Kind = CheckKind.Maintenance, TravelDate = Wednesday };

        var result = await new BackgroundCheck(store, source, Clock(0, 10)).RunAsync(maintenance, false, CancellationToken.None);

        Assert.Equal(0, source.Calls);
        Assert.NotEmpty(result.NextChecks);
    }

    private (AppDataStore Store, Shift Shift) CreateStore()
    {
        var store = new AppDataStore(_directory);
        var data = new AppData();
        data.Settings.Language = AppLanguage.German;
        var shift = SampleData.CreateShift("Frühdienst");
        data.Shifts.Add(shift);
        data.CurrentWeek = WeekPlan.CreateEmpty(new DateOnly(2026, 10, 19));
        data.CurrentWeek.GetDay(DayOfWeek.Wednesday).ShiftId = shift.Id;
        store.Save(data);
        return (store, shift);
    }

    private static PlannedCheck LeadCheck(Shift shift, int leadMinutes)
    {
        return new PlannedCheck
        {
            Kind = CheckKind.LeadTime,
            TravelDate = Wednesday,
            ShiftId = shift.Id,
            Direction = TripDirection.Outbound,
            LeadMinutes = leadMinutes
        };
    }

    private static FixedClock Clock(int hour, int minute)
    {
        return new FixedClock(BerlinTime.ToInstant(Wednesday.ToDateTime(new TimeOnly(hour, minute)), BerlinTime.Zone));
    }
}
