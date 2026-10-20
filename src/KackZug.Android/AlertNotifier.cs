using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using KackZug.Core.Alarms;
using KackZug.Core.Checking;

namespace KackZug.Android;

/// <summary>Zeigt Meldungen: Stoerungen laut und auffaellig, Infos ruhig</summary>
public static class AlertNotifier
{
    public const string ProblemChannelId = "stoerung_v1";
    public const string InfoChannelId = "info_v1";
    public const string DbNavigatorPackage = "de.hafas.android.db";

    private static readonly long[] VibrationPattern = { 0, 800, 400, 800, 400, 800, 400, 1200 };

    public static void EnsureChannels(Context context)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        if (manager is null)
        {
            return;
        }

        if (manager.GetNotificationChannel(ProblemChannelId) is null)
        {
            var problem = new NotificationChannel(ProblemChannelId, "Störung", NotificationImportance.High)
            {
                Description = "Ausfall, Verspätung, Gleiswechsel oder gefährdeter Anschluss",
                LockscreenVisibility = NotificationVisibility.Public
            };
            problem.EnableVibration(true);
            problem.SetVibrationPattern(VibrationPattern);
            problem.EnableLights(true);
            var sound = RingtoneManager.GetDefaultUri(RingtoneType.Alarm) ?? RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            var audio = new AudioAttributes.Builder()!
                .SetUsage(AudioUsageKind.Alarm)!
                .SetContentType(AudioContentType.Sonification)!
                .Build();
            problem.SetSound(sound, audio);
            manager.CreateNotificationChannel(problem);
        }

        if (manager.GetNotificationChannel(InfoChannelId) is null)
        {
            var info = new NotificationChannel(InfoChannelId, "Info", NotificationImportance.Default)
            {
                Description = "Fahrt planmäßig oder konnte nicht geprüft werden"
            };
            manager.CreateNotificationChannel(info);
        }
    }

    public static void Show(Context context, TripAlert alert, bool insistent)
    {
        EnsureChannels(context);
        var notificationId = AlarmFlow.NotificationIdFor(alert);
        if (alert.Severity == AlertSeverity.Problem)
        {
            var alarm = PendingAlarm.From(alert, notificationId, DateTimeOffset.Now);
            AlarmFlow.Start(context, alarm, insistent);
            return;
        }

        var builder = new NotificationCompat.Builder(context, InfoChannelId);
        builder.SetSmallIcon(Resource.Drawable.ic_notification);
        builder.SetContentTitle(alert.Title);
        builder.SetContentText(alert.Text);
        builder.SetStyle(new NotificationCompat.BigTextStyle().BigText(alert.Text));
        builder.SetAutoCancel(true);
        builder.SetContentIntent(OpenAppIntent(context, notificationId));
        NotificationManagerCompat.From(context)!.Notify(notificationId, builder.Build()!);
    }

    private static PendingIntent OpenAppIntent(Context context, int notificationId)
    {
        var open = new Intent(context, typeof(MainActivity));
        open.AddFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
        return PendingIntent.GetActivity(context, notificationId, open, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }
}
