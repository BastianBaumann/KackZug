using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;

namespace KackZug.Android;

/// <summary>Hauptaktivitaet, nimmt auch geteilte Verbindungen aus dem DB Navigator entgegen</summary>
[Activity(
    Name = "io.github.bastianbaumann.kackzug.MainActivity",
    Label = "KackZug",
    Theme = "@style/KackZugTheme",
    Icon = "@mipmap/icon",
    MainLauncher = true,
    Exported = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
[IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = "text/plain")]
public class MainActivity : AvaloniaMainActivity
{
    private const int NotificationPermissionRequestCode = 1;

    public static MainActivity? Current { get; private set; }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Current = this;
        HoldSplash();
        AskForNotifications();
        HandleIntent(Intent);
    }

    protected override void OnResume()
    {
        base.OnResume();
        App.ReloadFromBackground();
        App.ContinueUpdateAfterResume();
    }

    /// <remarks>Zwischen Startbildschirm des Systems und erstem Bild der Oberflaeche waere sonst kurz alles schwarz.</remarks>
    private void HoldSplash()
    {
        SplashOverlay.Show(this);
    }

    protected override void OnDestroy()
    {
        if (Current == this)
        {
            Current = null;
        }

        base.OnDestroy();
    }

    private void AskForNotifications()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            return;
        }

        if (CheckSelfPermission(Manifest.Permission.PostNotifications) == Permission.Granted)
        {
            return;
        }

        RequestPermissions(new[] { Manifest.Permission.PostNotifications }, NotificationPermissionRequestCode);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        HandleIntent(intent);
    }

    private static void HandleIntent(Intent? intent)
    {
        if (intent is null || intent.Action != Intent.ActionSend)
        {
            return;
        }

        var text = intent.GetStringExtra(Intent.ExtraText);
        intent.SetAction(null);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        App.HandleSharedText(text);
    }
}
