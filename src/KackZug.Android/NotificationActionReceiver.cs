using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace KackZug.Android;

/// <summary>Reagiert auf "Gesehen" in der Benachrichtigung und stoppt den Alarmton</summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class NotificationActionReceiver : BroadcastReceiver
{
    public const string ActionSeen = "io.github.bastianbaumann.kackzug.SEEN";
    public const string ExtraNotificationId = "notification";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null || intent.Action != ActionSeen)
        {
            return;
        }

        var notificationId = intent.GetIntExtra(ExtraNotificationId, 0);
        NotificationManagerCompat.From(context)!.Cancel(notificationId);
    }
}
