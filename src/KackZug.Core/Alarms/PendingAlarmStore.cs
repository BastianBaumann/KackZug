using System.Text.Json;

namespace KackZug.Core.Alarms;

/// <summary>Merkt sich offene Alarme in einer eigenen Datei, damit Wiederholungen auch ohne laufende App funktionieren</summary>
public sealed class PendingAlarmStore
{
    public const string FileName = "pending-alarms.json";

    public static readonly TimeSpan RepeatInterval = TimeSpan.FromMinutes(5);

    private static readonly object FileLock = new object();

    public PendingAlarmStore(string directory)
    {
        Directory.CreateDirectory(directory);
        FilePath = Path.Combine(directory, FileName);
    }

    public string FilePath { get; }

    public List<PendingAlarm> Load()
    {
        lock (FileLock)
        {
            return LoadUnlocked();
        }
    }

    public void Add(PendingAlarm alarm)
    {
        lock (FileLock)
        {
            var alarms = LoadUnlocked();
            alarms.RemoveAll(item => item.NotificationId == alarm.NotificationId);
            alarms.Add(alarm);
            SaveUnlocked(alarms);
        }
    }

    public PendingAlarm? Find(int notificationId)
    {
        return Load().FirstOrDefault(item => item.NotificationId == notificationId);
    }

    public void Acknowledge(int notificationId)
    {
        lock (FileLock)
        {
            var alarms = LoadUnlocked();
            alarms.RemoveAll(item => item.NotificationId == notificationId);
            SaveUnlocked(alarms);
        }
    }

    public void MarkShown(int notificationId, DateTimeOffset now)
    {
        lock (FileLock)
        {
            var alarms = LoadUnlocked();
            foreach (var alarm in alarms)
            {
                if (alarm.NotificationId == notificationId)
                {
                    alarm.LastShownAt = now;
                }
            }

            SaveUnlocked(alarms);
        }
    }

    public List<PendingAlarm> RemoveExpired(DateTimeOffset now)
    {
        lock (FileLock)
        {
            var alarms = LoadUnlocked();
            var expired = alarms.Where(alarm => alarm.RepeatUntil <= now).ToList();
            alarms.RemoveAll(alarm => alarm.RepeatUntil <= now);
            SaveUnlocked(alarms);
            return expired;
        }
    }

    public static DateTimeOffset? NextRepeat(PendingAlarm alarm, DateTimeOffset now)
    {
        var next = now + RepeatInterval;
        if (next > alarm.RepeatUntil)
        {
            return null;
        }

        return next;
    }

    private List<PendingAlarm> LoadUnlocked()
    {
        if (!File.Exists(FilePath))
        {
            return new List<PendingAlarm>();
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            var alarms = JsonSerializer.Deserialize(json, PendingAlarmJsonContext.Default.ListPendingAlarm);
            if (alarms is null)
            {
                return new List<PendingAlarm>();
            }

            return alarms;
        }
        catch (JsonException)
        {
            return new List<PendingAlarm>();
        }
    }

    private void SaveUnlocked(List<PendingAlarm> alarms)
    {
        var json = JsonSerializer.Serialize(alarms, PendingAlarmJsonContext.Default.ListPendingAlarm);
        var temporaryPath = FilePath + ".tmp";
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, FilePath, true);
    }
}
