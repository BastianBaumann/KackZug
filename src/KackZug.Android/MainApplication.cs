using Android.App;
using Android.Runtime;
using AppUpdater.Android;
using Avalonia.Android;

namespace KackZug.Android;

/// <summary>Android-Anwendung, setzt die Plattform und startet Avalonia mit der gemeinsamen App</summary>
[Application(Label = "KackZug", Icon = "@mipmap/icon")]
public class MainApplication : AvaloniaAndroidApplication<App>
{
    protected MainApplication(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    protected override Avalonia.AppBuilder CustomizeAppBuilder(Avalonia.AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder).WithAppFont();
    }

    public override void OnCreate()
    {
        App.Platform = new AndroidPlatform(this);
        AlertNotifier.EnsureChannels(this);
        InstallStatusReceiver.StatusReceived += OnInstallStatus;
        base.OnCreate();
    }

    private static void OnInstallStatus(object? sender, InstallStatusEventArgs e)
    {
        if (e.Status == AndroidInstallStatus.WaitingForUser)
        {
            return;
        }

        App.ReportUpdateInstallFinished(e.Status == AndroidInstallStatus.Succeeded, e.Status == AndroidInstallStatus.Aborted, e.Message);
    }
}
