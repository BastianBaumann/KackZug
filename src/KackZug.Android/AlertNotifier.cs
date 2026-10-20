using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using KackZug.Core.Checking;
using KackZug.Core.Planning;

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
        var isProblem = alert.Severity == AlertSeverity.Problem;
        var notificationId = StableHash.Of(alert.TravelDate.ToString("yyyy-MM-dd") + "|" + alert.ShiftId + "|" + alert.Direction);

        var channel = InfoChannelId;
        if (isProblem)
        {
            channel = ProblemChannelId;
        }

        var builder = new NotificationCompat.Builder(context, channel);
        builder.SetSmallIcon(Resource.Drawable.ic_notification);
        builder.SetContentTitle(alert.Title);
        builder.SetContentText(alert.Text);
        builder.SetStyle(new NotificationCompat.BigTextStyle().BigText(alert.Text));
        builder.SetAutoCancel(true);
        builder.SetContentIntent(OpenAppIntent(context, notificationId));

        if (isProblem)
        {
            builder.SetPriority(NotificationCompat.PriorityMax);
            builder.SetCategory(NotificationCompat.CategoryAlarm);
            builder.SetVibrate(VibrationPattern);
            builder.SetColor(unchecked((int)0xFFD93A3A));
            builder.AddAction(0, "Gesehen", SeenIntent(context, notificationId));
            builder.AddAction(0, "DB Navigator öffnen", NavigatorIntent(context, notificationId));
        }

        var notification = builder.Build()!;
        if (isProblem && insistent)
        {
            notification.Flags |= NotificationFlags.Insistent;
        }

        NotificationManagerCompat.From(context)!.Notify(notificationId, notification);
    }

    private static PendingIntent OpenAppIntent(Context context, int notificationId)
    {
        var open = new Intent(context, typeof(MainActivity));
        open.AddFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
        return PendingIntent.GetActivity(context, notificationId, open, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    private static PendingIntent SeenIntent(Context context, int notificationId)
    {
        var seen = new Intent(context, typeof(NotificationActionReceiver));
        seen.SetAction(NotificationActionReceiver.ActionSeen);
        seen.PutExtra(NotificationActionReceiver.ExtraNotificationId, notificationId);
        return PendingIntent.GetBroadcast(context, notificationId, seen, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    private static PendingIntent NavigatorIntent(Context context, int notificationId)
    {
        var launch = context.PackageManager?.GetLaunchIntentForPackage(DbNavigatorPackage);
        if (launch is null)
        {
            launch = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("https://www.bahn.de"));
        }

        launch.AddFlags(ActivityFlags.NewTask);
        return PendingIntent.GetActivity(context, notificationId + 1, launch, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }
}
