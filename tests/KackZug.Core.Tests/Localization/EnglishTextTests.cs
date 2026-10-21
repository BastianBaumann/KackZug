using KackZug.Core.Checking;
using KackZug.Core.Import;
using KackZug.Core.Localization;
using KackZug.Core.Models;
using KackZug.Core.Tests.Checking;

namespace KackZug.Core.Tests.Localization;

/// <summary>Prueft Meldungen auf Englisch; stellt danach wieder Deutsch ein</summary>
[Collection(LanguageCollection.Name)]
public sealed class EnglishTextTests : IDisposable
{
    private static readonly DateOnly Day = new DateOnly(2026, 10, 21);
    private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 10, 21, 5, 0, 0, TimeSpan.FromHours(2));

    public EnglishTextTests()
    {
        Language.Apply(AppLanguage.English);
    }

    public void Dispose()
    {
        Language.Apply(AppLanguage.German);
    }

    [Fact]
    public async Task CancellationIsReportedInEnglish()
    {
        var data = new AppData();
        var shift = SampleData.CreateShift("Early shift");
        data.Shifts.Add(shift);
        var source = new FakeTripStatusSource();
        source.CancelledNames.Add("RE7");
        var checker = new TripChecker(source, new FixedClock(Now));

        var outcome = await checker.CheckAsync(data, Day, shift.Id, TripDirection.Outbound, CheckContext.LeadTime, CancellationToken.None);

        Assert.Equal("EARLY SHIFT OUT: RE7 06:42 CANCELLED", outcome!.Alert.Title);
        Assert.Equal("Cancelled", outcome.Alert.BadgeText);
        Assert.Equal("CANCELLED", outcome.Alert.Headline);
        Assert.Equal("EARLY SHIFT · OUTBOUND · WED 21/10", outcome.Alert.Kicker);
        Assert.Contains("Musterstadt Hbf → Beispielhausen: cancelled", outcome.Alert.Text);
        Assert.Contains("DB Navigator", outcome.Alert.Text);
    }

    [Fact]
    public void ManualErrorIsInEnglish()
    {
        var result = ManualConnectionBuilder.Build(new List<ManualLegInput> { new ManualLegInput() }, Now);

        Assert.Equal("Leg 1: Please search for the departure station and tap it.", result.Error);
    }
}
