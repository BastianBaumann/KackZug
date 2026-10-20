using Android.App;
using Android.Content;

namespace KackZug.Android;

/// <summary>Reagiert auf Gesehen, Wegwischen, DB Navigator und die Wiederholung eines offenen Alarms</summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class NotificationActionReceiver : BroadcastReceiver
{
    public const string ActionSeen = "io.github.bastianbaumann.kackzug.SEEN";
    public const string ActionRepost = "io.github.bastianbaumann.kackzug.REPOST";
    public const string ActionRepeat = "io.github.bastianbaumann.kackzug.REPEAT";
    public const string ActionNavigator = "io.github.bastianbaumann.kackzug.NAVIGATOR";
    public const string ActionTest = "io.github.bastianbaumann.kackzug.TEST";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null)
        {
            return;
        }

        var notificationId = intent.GetIntExtra(AlarmFlow.ExtraNotificationId, 0);
        switch (intent.Action)
        {
            case ActionSeen:
                AlarmFlow.Acknowledge(context, notificationId);
                break;
            case ActionNavigator:
                AlarmFlow.Acknowledge(context, notificationId);
                AlarmFlow.OpenNavigator(context);
                break;
            case ActionRepost:
                AlarmFlow.Repost(context, notificationId);
                break;
            case ActionRepeat:
                AlarmFlow.Repeat(context, notificationId);
                break;
            case ActionTest:
                var settings = new Core.Storage.AppDataStore(AndroidPaths.DataDirectory(context)).Load().Settings;
                AlertNotifier.Show(context, Core.Checking.TestAlert.Create(DateTimeOffset.Now), settings.InsistentAlerts);
                break;
        }
    }
}
