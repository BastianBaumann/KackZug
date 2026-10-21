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
    public const string ProblemChannelId = "stoerung_v2";
    public const string ProblemDoNotDisturbChannelId = "stoerung_dnd_v2";
    public const string InfoChannelId = "info_v1";
    public const string DbNavigatorPackage = "de.hafas.android.db";

    public const int ProblemColor = unchecked((int)0xFFD1001A);
    private const int OkColor = unchecked((int)0xFF1B7A35);
    private const int UnknownColor = unchecked((int)0xFF56606B);

    private static readonly long[] VibrationPattern = { 0, 800, 400, 800, 400, 800, 400, 1200 };

    /// <remarks>Ein Kanal behaelt seinen Ton fuer immer, deshalb bekommt jeder neue Ton neue Kanal-Kennungen.</remarks>
    private static readonly string[] OldChannelIds = { "stoerung_v1", "stoerung_dnd_v1" };

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

        foreach (var oldId in OldChannelIds)
        {
            if (manager.GetNotificationChannel(oldId) is not null)
            {
                manager.DeleteNotificationChannel(oldId);
            }
        }

        if (manager.GetNotificationChannel(ProblemChannelId) is null)
        {
            var problem = CreateProblemChannel(context, ProblemChannelId, "Störung");
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

    public static bool HasDoNotDisturbAccess(Context context)
    {
        var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        return manager is not null && manager.IsNotificationPolicyAccessGranted;
    }

    /// <remarks>Android uebernimmt "Bitte nicht stoeren durchbrechen" nur, wenn der Zugriff beim Anlegen des Kanals schon erlaubt ist.</remarks>
    public static string ProblemChannelFor(Context context, bool breakThroughDoNotDisturb)
    {
        EnsureChannels(context);
        if (!breakThroughDoNotDisturb || Build.VERSION.SdkInt < BuildVersionCodes.O || !HasDoNotDisturbAccess(context))
        {
            return ProblemChannelId;
        }

        var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        if (manager is null)
        {
            return ProblemChannelId;
        }

        if (manager.GetNotificationChannel(ProblemDoNotDisturbChannelId) is null)
        {
            var channel = CreateProblemChannel(context, ProblemDoNotDisturbChannelId, "Störung (auch bei Bitte nicht stören)");
            channel.SetBypassDnd(true);
            manager.CreateNotificationChannel(channel);
        }

        return ProblemDoNotDisturbChannelId;
    }

    private static NotificationChannel CreateProblemChannel(Context context, string id, string name)
    {
        var channel = new NotificationChannel(id, name, NotificationImportance.High)
        {
            Description = "Ausfall, Verspätung, Gleiswechsel oder gefährdeter Anschluss",
            LockscreenVisibility = NotificationVisibility.Public
        };
        channel.EnableVibration(true);
        channel.SetVibrationPattern(VibrationPattern);
        channel.EnableLights(true);
        var sound = global::Android.Net.Uri.Parse("android.resource://" + context.PackageName + "/" + Resource.Raw.kackzug_alarm);
        var audio = new AudioAttributes.Builder()!
            .SetUsage(AudioUsageKind.Alarm)!
            .SetContentType(AudioContentType.Sonification)!
            .Build();
        channel.SetSound(sound, audio);
        return channel;
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

        if (AlarmFlow.Store(context).Find(notificationId) is not null)
        {
            AlarmFlow.Acknowledge(context, notificationId);
        }

        var builder = new NotificationCompat.Builder(context, InfoChannelId);
        builder.SetSmallIcon(Resource.Drawable.ic_notification);
        builder.SetColor(InfoColorFor(alert.Status));
        builder.SetContentTitle(alert.Title);
        builder.SetContentText(alert.Text);
        builder.SetStyle(new NotificationCompat.BigTextStyle().BigText(alert.Text));
        builder.SetAutoCancel(true);
        builder.SetContentIntent(OpenAppIntent(context, notificationId));
        NotificationManagerCompat.From(context)!.Notify(notificationId, builder.Build()!);
    }

    private static int InfoColorFor(LegStatus status)
    {
        if (status == LegStatus.Unknown)
        {
            return UnknownColor;
        }

        return OkColor;
    }

    private static PendingIntent OpenAppIntent(Context context, int notificationId)
    {
        var open = new Intent(context, typeof(MainActivity));
        open.AddFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
        return PendingIntent.GetActivity(context, notificationId, open, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }
}
