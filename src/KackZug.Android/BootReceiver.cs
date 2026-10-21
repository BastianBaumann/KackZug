using Android.App;
using Android.Content;

namespace KackZug.Android;

/// <summary>Plant alle Pruefungen nach Neustart, App-Update oder geaenderter Uhrzeit neu</summary>
[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionMyPackageReplaced, Intent.ActionTimeChanged, Intent.ActionTimezoneChanged })]
public sealed class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null)
        {
            return;
        }

        AlarmFlow.RemoveExpired(context);
        AlarmScheduler.ScheduleFromFile(context);
    }
}
