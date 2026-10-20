using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace KackZug.Android;

/// <summary>Hauptaktivitaet der App</summary>
[Activity(
    Label = "KackZug",
    Theme = "@style/KackZugTheme",
    Icon = "@drawable/icon",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
