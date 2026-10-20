using Android;
using Android.Content;

using Android.OS;
using AndroidX.Core.App;
using KackZug.Core.Checking;
using KackZug.Core.Planning;
using KackZug.Services;

namespace KackZug.Android;

/// <summary>Plattform fuer Android: Datenordner, exakte Alarme, Benachrichtigungen und Berechtigungen</summary>
public sealed class AndroidPlatform : IPlatform
{
    public const string PermissionNotifications = "notifications";
    public const string PermissionExactAlarms = "exact";
    public const string PermissionBattery = "battery";

    private readonly Context _context;

    public AndroidPlatform(Context context)
    {
        _context = context;
        DataDirectory = AndroidPaths.DataDirectory(context);
    }

    public string DataDirectory { get; }

    public bool IsMobile
    {
        get { return true; }
    }

    public void ScheduleChecks(IReadOnlyList<PlannedCheck> checks)
    {
        AlarmScheduler.ScheduleAll(_context, checks);
    }

    public void ShowAlert(TripAlert alert)
    {
        var insistent = true;
        if (App.Services is not null)
        {
            insistent = App.Services.Data.Settings.InsistentAlerts;
        }

        AlertNotifier.Show(_context, alert, insistent);
    }

    public void OpenUrl(string url)
    {
        var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url));
        intent.AddFlags(ActivityFlags.NewTask);
        _context.StartActivity(intent);
    }

    public IReadOnlyList<PermissionInfo> GetPermissions()
    {
        var notificationsAllowed = NotificationManagerCompat.From(_context)!.AreNotificationsEnabled();
        var exactAllowed = AlarmScheduler.CanScheduleExact(_context);
        var power = (PowerManager?)_context.GetSystemService(Context.PowerService);
        var batteryFree = power is not null && power.IsIgnoringBatteryOptimizations(_context.PackageName);

        return new List<PermissionInfo>
        {
            new PermissionInfo(PermissionNotifications, "Benachrichtigungen", "Ohne sie kann KackZug keine Störung melden.", notificationsAllowed),
            new PermissionInfo(PermissionExactAlarms, "Pünktliche Alarme", "Damit die Prüfung genau zur eingestellten Zeit läuft.", exactAllowed),
            new PermissionInfo(PermissionBattery, "Akku-Optimierung aus", "Sonst kann Android die Prüfung verzögern oder ganz auslassen.", batteryFree)
        };
    }

    public void OpenPermissionSettings(string key)
    {
        Intent intent;
        var packageUri = global::Android.Net.Uri.Parse("package:" + _context.PackageName);
        if (key == PermissionNotifications)
        {
            intent = new Intent(global::Android.Provider.Settings.ActionAppNotificationSettings);
            intent.PutExtra(global::Android.Provider.Settings.ExtraAppPackage, _context.PackageName);
        }
        else if (key == PermissionExactAlarms && OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            intent = new Intent(global::Android.Provider.Settings.ActionRequestScheduleExactAlarm, packageUri);
        }
        else if (key == PermissionBattery)
        {
            intent = new Intent(global::Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations, packageUri);
        }
        else
        {
            intent = new Intent(global::Android.Provider.Settings.ActionApplicationDetailsSettings, packageUri);
        }

        intent.AddFlags(ActivityFlags.NewTask);
        _context.StartActivity(intent);
    }
}
