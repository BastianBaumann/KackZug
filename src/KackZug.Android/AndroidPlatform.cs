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
    public const string PermissionFullScreen = "fullscreen";
    public const string PermissionDoNotDisturb = "dnd";

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

    public void ScheduleTestAlarm(TimeSpan delay)
    {
        var alarmManager = (global::Android.App.AlarmManager?)_context.GetSystemService(Context.AlarmService);
        if (alarmManager is null)
        {
            return;
        }

        var intent = new Intent(_context, typeof(NotificationActionReceiver));
        intent.SetAction(NotificationActionReceiver.ActionTest);
        var pending = global::Android.App.PendingIntent.GetBroadcast(_context, StableHash.Of("test-alarm"), intent,
            global::Android.App.PendingIntentFlags.UpdateCurrent | global::Android.App.PendingIntentFlags.Immutable)!;
        var at = DateTimeOffset.Now + delay;
        if (AlarmScheduler.CanScheduleExact(_context))
        {
            alarmManager.SetExactAndAllowWhileIdle(global::Android.App.AlarmType.RtcWakeup, at.ToUnixTimeMilliseconds(), pending);
        }
        else
        {
            alarmManager.SetAndAllowWhileIdle(global::Android.App.AlarmType.RtcWakeup, at.ToUnixTimeMilliseconds(), pending);
        }
    }

    public void OpenUrl(string url)
    {
        var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url));
        intent.AddFlags(ActivityFlags.NewTask);
        _context.StartActivity(intent);
    }

    public bool OpenInNavigator(string? connectionUrl)
    {
        Intent? intent = null;
        var inNavigator = true;
        if (connectionUrl is not null)
        {
            var uri = global::Android.Net.Uri.Parse(connectionUrl);
            intent = new Intent(Intent.ActionView, uri);
            intent.SetPackage(AlertNotifier.DbNavigatorPackage);
            if (_context.PackageManager?.ResolveActivity(intent, 0) is null)
            {
                intent = new Intent(Intent.ActionView, uri);
                inNavigator = false;
            }
        }
        else
        {
            intent = _context.PackageManager?.GetLaunchIntentForPackage(AlertNotifier.DbNavigatorPackage);
            if (intent is null)
            {
                intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("https://www.bahn.de"));
                inNavigator = false;
            }
        }

        intent.AddFlags(ActivityFlags.NewTask);
        try
        {
            _context.StartActivity(intent);
        }
        catch (ActivityNotFoundException)
        {
            return false;
        }

        return inNavigator;
    }

    public IReadOnlyList<PermissionInfo> GetPermissions()
    {
        var notificationsAllowed = NotificationManagerCompat.From(_context)!.AreNotificationsEnabled();
        var exactAllowed = AlarmScheduler.CanScheduleExact(_context);
        var power = (PowerManager?)_context.GetSystemService(Context.PowerService);
        var batteryFree = power is not null && power.IsIgnoringBatteryOptimizations(_context.PackageName);
        var fullScreenAllowed = true;
        if (OperatingSystem.IsAndroidVersionAtLeast(34))
        {
            var manager = (global::Android.App.NotificationManager?)_context.GetSystemService(Context.NotificationService);
            fullScreenAllowed = manager is not null && manager.CanUseFullScreenIntent();
        }

        var permissions = new List<PermissionInfo>
        {
            new PermissionInfo(PermissionNotifications, "Benachrichtigungen", "Ohne sie kann KackZug keine Störung melden.", notificationsAllowed),
            new PermissionInfo(PermissionExactAlarms, "Pünktliche Alarme", "Damit die Prüfung genau zur eingestellten Zeit läuft.", exactAllowed),
            new PermissionInfo(PermissionBattery, "Akku-Optimierung aus", "Sonst kann Android die Prüfung verzögern oder ganz auslassen.", batteryFree),
            new PermissionInfo(PermissionFullScreen, "Vollbild-Alarm", "Damit eine Störung auch auf dem Sperrbildschirm groß erscheint.", fullScreenAllowed)
        };

        if (App.Services is not null && App.Services.Data.Settings.BreakThroughDoNotDisturb)
        {
            permissions.Add(new PermissionInfo(PermissionDoNotDisturb, "Bitte nicht stören", "Damit eine Störung auch im Ruhemodus klingelt.", AlertNotifier.HasDoNotDisturbAccess(_context)));
        }

        return permissions;
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
        else if (key == PermissionFullScreen && OperatingSystem.IsAndroidVersionAtLeast(34))
        {
            intent = new Intent(global::Android.Provider.Settings.ActionManageAppUseFullScreenIntent, packageUri);
        }
        else if (key == PermissionDoNotDisturb)
        {
            intent = new Intent(global::Android.Provider.Settings.ActionNotificationPolicyAccessSettings);
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
