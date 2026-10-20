using KackZug.Core.Alarms;
using KackZug.Core.Checking;

namespace KackZug.Core.Tests.Alarms;

/// <summary>Prueft das Merken, Bestaetigen und Wiederholen offener Alarme</summary>
public sealed class PendingAlarmStoreTests : IDisposable
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 10, 21, 6, 0, 0, TimeSpan.FromHours(2));
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "kackzug-alarm-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    [Fact]
    public void AlarmStaysUntilAcknowledged()
    {
        var store = new PendingAlarmStore(_directory);
        store.Add(CreateAlarm(1, Now.AddMinutes(42)));
        store.Add(CreateAlarm(2, Now.AddMinutes(42)));

        store.Acknowledge(1);

        var remaining = new PendingAlarmStore(_directory).Load();
        var alarm = Assert.Single(remaining);
        Assert.Equal(2, alarm.NotificationId);
        Assert.Equal("FÄLLT AUS", alarm.Headline);
    }

    [Fact]
    public void SameNotificationReplacesOlderEntry()
    {
        var store = new PendingAlarmStore(_directory);
        store.Add(CreateAlarm(1, Now.AddMinutes(10)));
        store.Add(CreateAlarm(1, Now.AddMinutes(42)));

        Assert.Equal(Now.AddMinutes(42), Assert.Single(store.Load()).RepeatUntil);
    }

    [Fact]
    public void RepeatsEveryFiveMinutesUntilDeparture()
    {
        var alarm = CreateAlarm(1, Now.AddMinutes(12));

        Assert.Equal(Now.AddMinutes(5), PendingAlarmStore.NextRepeat(alarm, Now));
        Assert.Equal(Now.AddMinutes(10), PendingAlarmStore.NextRepeat(alarm, Now.AddMinutes(5)));
        Assert.Null(PendingAlarmStore.NextRepeat(alarm, Now.AddMinutes(10)));
    }

    [Fact]
    public void ExpiredAlarmsAreRemoved()
    {
        var store = new PendingAlarmStore(_directory);
        store.Add(CreateAlarm(1, Now.AddMinutes(-1)));
        store.Add(CreateAlarm(2, Now.AddMinutes(30)));

        var expired = store.RemoveExpired(Now);

        Assert.Equal(1, Assert.Single(expired).NotificationId);
        Assert.Equal(2, Assert.Single(store.Load()).NotificationId);
    }

    private static PendingAlarm CreateAlarm(int notificationId, DateTimeOffset until)
    {
        var alert = new TripAlert
        {
            Title = "Frühdienst Hinfahrt: RE7 06:42 fällt aus",
            Headline = "FÄLLT AUS",
            Kicker = "FRÜHDIENST · HINFAHRT · MI 21.10.",
            TripLine = "RE7 · 06:42 Musterstadt Hbf → Beispielhausen",
            Status = LegStatus.Cancelled,
            RepeatUntil = until
        };
        return PendingAlarm.From(alert, notificationId, Now);
    }
}
