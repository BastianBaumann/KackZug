using KackZug.Core.Models;
using KackZug.Core.Storage;

namespace KackZug.Core.Tests.Storage;

/// <summary>Prueft Laden und Speichern der Datendatei</summary>
public sealed class AppDataStoreTests : IDisposable
{
    private readonly string _directory;

    public AppDataStoreTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "kackzug-tests-" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    [Fact]
    public void LoadWithoutFileReturnsDefaults()
    {
        var store = new AppDataStore(_directory);

        var data = store.Load();

        Assert.Empty(data.Shifts);
        Assert.Equal(new List<int> { 90, 45, 15 }, data.Settings.LeadTimesMinutes);
    }

    [Fact]
    public void SavedDataComesBackUnchanged()
    {
        var store = new AppDataStore(_directory);
        var shift = SampleData.CreateShift("Fruehdienst");
        var data = new AppData();
        data.Shifts.Add(shift);
        data.CurrentWeek = WeekPlan.CreateEmpty(new DateOnly(2026, 10, 19));
        data.CurrentWeek.GetDay(DayOfWeek.Tuesday).ShiftId = shift.Id;
        data.CurrentWeek.GetDay(DayOfWeek.Tuesday).ReturnEnabled = false;
        data.Settings.ThemeMode = ThemeMode.Dark;

        store.Save(data);
        var loaded = new AppDataStore(_directory).Load();

        var loadedShift = Assert.Single(loaded.Shifts);
        Assert.Equal("Fruehdienst", loadedShift.Name);
        Assert.Equal(3, loadedShift.Outbound!.Legs.Count);
        Assert.Equal(new TimeOnly(6, 42), loadedShift.Outbound.Legs[0].PlannedDeparture);
        Assert.Equal(LegKind.Walk, loadedShift.Outbound.Legs[1].Kind);
        var tuesday = loaded.CurrentWeek!.GetDay(DayOfWeek.Tuesday);
        Assert.Equal(shift.Id, tuesday.ShiftId);
        Assert.False(tuesday.ReturnEnabled);
        Assert.Equal(ThemeMode.Dark, loaded.Settings.ThemeMode);
    }

    [Fact]
    public void FileUsesReadableEnumNames()
    {
        var store = new AppDataStore(_directory);
        var data = new AppData();
        data.Shifts.Add(SampleData.CreateShift("Spaetdienst"));

        store.Save(data);
        var json = File.ReadAllText(store.FilePath);

        Assert.Contains("\"Walk\"", json);
        Assert.Contains("\"CopyPrevious\"", json);
        Assert.DoesNotContain("isWalk", json);
    }

    [Fact]
    public void BrokenFileIsKeptAndDefaultsAreUsed()
    {
        var store = new AppDataStore(_directory);
        File.WriteAllText(store.FilePath, "{ kaputt");

        var data = store.Load();

        Assert.Empty(data.Shifts);
        Assert.False(File.Exists(store.FilePath));
        Assert.Single(Directory.GetFiles(_directory, AppDataStore.FileName + ".broken-*"));
    }

    [Fact]
    public void SaveLeavesNoTemporaryFile()
    {
        var store = new AppDataStore(_directory);

        store.Save(new AppData());
        store.Save(new AppData());

        Assert.Single(Directory.GetFiles(_directory));
    }
}
