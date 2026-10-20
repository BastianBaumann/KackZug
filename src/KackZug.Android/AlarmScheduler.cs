using Android.App;
using Android.Content;
using KackZug.Core;
using KackZug.Core.Checking;
using KackZug.Core.Planning;
using KackZug.Core.Storage;

namespace KackZug.Android;

/// <summary>Stellt fuer jede geplante Pruefung einen exakten Alarm und raeumt alte Alarme weg</summary>
public static class AlarmScheduler
{
    private const string CodesFileName = "alarms.txt";

    private static readonly object FileLock = new object();

    public static void ScheduleFromFile(Context context)
    {
        var store = new AppDataStore(AndroidPaths.DataDirectory(context));
        var checks = BackgroundCheck.PlanFromFile(store, new SystemClock());
        ScheduleAll(context, checks);
    }

    public static void ScheduleAll(Context context, IReadOnlyList<PlannedCheck> checks)
    {
        var alarm = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (alarm is null)
        {
            return;
        }

        lock (FileLock)
        {
            foreach (var oldCode in ReadCodes(context))
            {
                var oldIntent = new Intent(context, typeof(CheckAlarmReceiver));
                oldIntent.SetAction(CheckIntent.Action);
                var oldPending = PendingIntent.GetBroadcast(context, oldCode, oldIntent, PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);
                if (oldPending is not null)
                {
                    alarm.Cancel(oldPending);
                    oldPending.Cancel();
                }
            }

            var codes = new List<int>();
            foreach (var check in checks)
            {
                SetAlarm(context, alarm, check, check.RequestCode, check.At, false);
                codes.Add(check.RequestCode);
            }

            WriteCodes(context, codes);
        }
    }

    public static void ScheduleRetry(Context context, PlannedCheck check)
    {
        var alarm = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (alarm is null)
        {
            return;
        }

        var retryCode = StableHash.Of(check.Key + "|retry");
        var at = DateTimeOffset.Now + BackgroundCheck.RetryDelay;
        SetAlarm(context, alarm, check, retryCode, at, true);
    }

    public static bool CanScheduleExact(Context context)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            return true;
        }

        var alarm = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        return alarm is not null && alarm.CanScheduleExactAlarms();
    }

    private static void SetAlarm(Context context, AlarmManager alarm, PlannedCheck check, int requestCode, DateTimeOffset at, bool isRetry)
    {
        var intent = CheckIntent.Create(context, check, isRetry);
        var flags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable;
        var pending = PendingIntent.GetBroadcast(context, requestCode, intent, flags)!;
        var triggerMilliseconds = at.ToUnixTimeMilliseconds();
        if (CanScheduleExact(context))
        {
            alarm.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerMilliseconds, pending);
        }
        else
        {
            alarm.SetAndAllowWhileIdle(AlarmType.RtcWakeup, triggerMilliseconds, pending);
        }
    }

    private static List<int> ReadCodes(Context context)
    {
        var path = System.IO.Path.Combine(AndroidPaths.DataDirectory(context), CodesFileName);
        var codes = new List<int>();
        if (!File.Exists(path))
        {
            return codes;
        }

        foreach (var line in File.ReadAllLines(path))
        {
            if (int.TryParse(line, out var code))
            {
                codes.Add(code);
            }
        }

        return codes;
    }

    private static void WriteCodes(Context context, List<int> codes)
    {
        var directory = AndroidPaths.DataDirectory(context);
        Directory.CreateDirectory(directory);
        var path = System.IO.Path.Combine(directory, CodesFileName);
        File.WriteAllLines(path, codes.Select(code => code.ToString()));
    }
}
