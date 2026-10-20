using Android.App;
using Android.Content;
using AndroidX.Core.App;
using KackZug.Core.Alarms;
using KackZug.Core.Planning;

namespace KackZug.Android;

/// <summary>Ablauf einer Stoerung: Vollbild-Alarm, Wiederkehren nach dem Wegwischen, Wiederholung bis "Gesehen"</summary>
public static class AlarmFlow
{
    public const string ExtraNotificationId = "notification";

    public static PendingAlarmStore Store(Context context)
    {
        return new PendingAlarmStore(AndroidPaths.DataDirectory(context));
    }

    public static void Start(Context context, PendingAlarm alarm, bool insistent)
    {
        Store(context).Add(alarm);
        Post(context, alarm, insistent);
        ScheduleRepeat(context, alarm);
    }

    public static void Post(Context context, PendingAlarm alarm, bool insistent)
    {
        AlertNotifier.EnsureChannels(context);
        var id = alarm.NotificationId;
        var builder = new NotificationCompat.Builder(context, AlertNotifier.ProblemChannelId);
        builder.SetSmallIcon(Resource.Drawable.ic_notification);
        builder.SetContentTitle(alarm.Title);
        builder.SetContentText(alarm.Text);
        builder.SetStyle(new NotificationCompat.BigTextStyle().BigText(alarm.Text));
        builder.SetPriority(NotificationCompat.PriorityMax);
        builder.SetCategory(NotificationCompat.CategoryAlarm);
        builder.SetVisibility(NotificationCompat.VisibilityPublic);
        builder.SetOngoing(true);
        builder.SetAutoCancel(false);
        builder.SetColor(unchecked((int)0xFFD1001A));
        builder.SetContentIntent(AlarmScreenIntent(context, id));
        builder.SetFullScreenIntent(AlarmScreenIntent(context, id), true);
        builder.SetDeleteIntent(ActionIntent(context, NotificationActionReceiver.ActionRepost, id));
        builder.AddAction(0, "Gesehen", ActionIntent(context, NotificationActionReceiver.ActionSeen, id));
        builder.AddAction(0, "DB Navigator öffnen", ActionIntent(context, NotificationActionReceiver.ActionNavigator, id));

        var notification = builder.Build()!;
        if (insistent)
        {
            notification.Flags |= NotificationFlags.Insistent;
        }

        NotificationManagerCompat.From(context)!.Notify(id, notification);
    }

    public static void Acknowledge(Context context, int notificationId)
    {
        Store(context).Acknowledge(notificationId);
        NotificationManagerCompat.From(context)!.Cancel(notificationId);
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        var pending = PendingIntent.GetBroadcast(context, RepeatCode(notificationId), RepeatIntent(context, notificationId), PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);
        if (alarmManager is not null && pending is not null)
        {
            alarmManager.Cancel(pending);
            pending.Cancel();
        }

        AlarmActivity.CloseIfShowing(notificationId);
    }

    public static void Repost(Context context, int notificationId)
    {
        var alarm = Store(context).Find(notificationId);
        if (alarm is null)
        {
            return;
        }

        if (alarm.RepeatUntil <= DateTimeOffset.Now)
        {
            Store(context).Acknowledge(notificationId);
            return;
        }

        Post(context, alarm, Insistent(context));
    }

    public static void Repeat(Context context, int notificationId)
    {
        var store = Store(context);
        var alarm = store.Find(notificationId);
        if (alarm is null)
        {
            return;
        }

        var now = DateTimeOffset.Now;
        if (alarm.RepeatUntil <= now)
        {
            store.Acknowledge(notificationId);
            return;
        }

        NotificationManagerCompat.From(context)!.Cancel(notificationId);
        Post(context, alarm, Insistent(context));
        store.MarkShown(notificationId, now);
        ScheduleRepeat(context, alarm);
    }

    public static int NotificationIdFor(Core.Checking.TripAlert alert)
    {
        return StableHash.Of(alert.TravelDate.ToString("yyyy-MM-dd") + "|" + alert.ShiftId + "|" + alert.Direction);
    }

    private static void ScheduleRepeat(Context context, PendingAlarm alarm)
    {
        var next = PendingAlarmStore.NextRepeat(alarm, DateTimeOffset.Now);
        if (next is null)
        {
            return;
        }

        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (alarmManager is null)
        {
            return;
        }

        var pending = PendingIntent.GetBroadcast(context, RepeatCode(alarm.NotificationId), RepeatIntent(context, alarm.NotificationId), PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
        var milliseconds = next.Value.ToUnixTimeMilliseconds();
        if (AlarmScheduler.CanScheduleExact(context))
        {
            alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, milliseconds, pending);
        }
        else
        {
            alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, milliseconds, pending);
        }
    }

    private static bool Insistent(Context context)
    {
        var data = new Core.Storage.AppDataStore(AndroidPaths.DataDirectory(context)).Load();
        return data.Settings.InsistentAlerts;
    }

    private static int RepeatCode(int notificationId)
    {
        return StableHash.Of("repeat|" + notificationId);
    }

    private static Intent RepeatIntent(Context context, int notificationId)
    {
        var intent = new Intent(context, typeof(NotificationActionReceiver));
        intent.SetAction(NotificationActionReceiver.ActionRepeat);
        intent.PutExtra(ExtraNotificationId, notificationId);
        return intent;
    }

    private static PendingIntent AlarmScreenIntent(Context context, int notificationId)
    {
        var intent = new Intent(context, typeof(AlarmActivity));
        intent.PutExtra(ExtraNotificationId, notificationId);
        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.NoUserAction);
        return PendingIntent.GetActivity(context, StableHash.Of("screen|" + notificationId), intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    private static PendingIntent ActionIntent(Context context, string action, int notificationId)
    {
        var intent = new Intent(context, typeof(NotificationActionReceiver));
        intent.SetAction(action);
        intent.PutExtra(ExtraNotificationId, notificationId);
        return PendingIntent.GetBroadcast(context, StableHash.Of(action + "|" + notificationId), intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    public static void OpenNavigator(Context context)
    {
        var launch = context.PackageManager?.GetLaunchIntentForPackage(AlertNotifier.DbNavigatorPackage);
        if (launch is null)
        {
            launch = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("https://www.bahn.de"));
        }

        launch.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(launch);
    }
}
