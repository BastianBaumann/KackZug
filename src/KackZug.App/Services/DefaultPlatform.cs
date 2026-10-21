using System.Diagnostics;
using AppUpdater;
using KackZug.Core.Checking;
using KackZug.Core.Planning;

namespace KackZug.Services;

/// <summary>Plattform fuer den Desktop zum Entwickeln: Alarme und Meldungen landen nur im Log</summary>
public sealed class DefaultPlatform : IPlatform
{
    public DefaultPlatform()
    {
        var overrideDirectory = Environment.GetEnvironmentVariable("KACKZUG_DATA_DIR");
        if (!string.IsNullOrEmpty(overrideDirectory))
        {
            DataDirectory = overrideDirectory;
        }
        else
        {
            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DataDirectory = Path.Combine(baseDirectory, "KackZug");
        }
    }

    public string DataDirectory { get; }

    public bool IsMobile
    {
        get { return false; }
    }

    public void ScheduleChecks(IReadOnlyList<PlannedCheck> checks)
    {
        foreach (var check in checks)
        {
            Trace.WriteLine("Pruefung geplant: " + check.At.ToString("yyyy-MM-dd HH:mm") + " " + check.Key);
        }
    }

    public void ShowAlert(TripAlert alert)
    {
        Trace.WriteLine("Meldung: " + alert.Title + " / " + alert.Text);
    }

    public void ScheduleTestAlarm(TimeSpan delay)
    {
        _ = ShowTestAlarmLaterAsync(delay);
    }

    private async Task ShowTestAlarmLaterAsync(TimeSpan delay)
    {
        await Task.Delay(delay);
        ShowAlert(TestAlert.Create(DateTimeOffset.Now));
    }

    public Action<string>? UrlOpener { get; set; }

    public void OpenUrl(string url)
    {
        if (UrlOpener is not null)
        {
            UrlOpener(url);
            return;
        }

        var start = new ProcessStartInfo(url) { UseShellExecute = true };
        Process.Start(start);
    }

    /// <remarks>Auf dem Desktop gibt es keinen Navigator, bahn.de im Browser ist hier der normale Weg.</remarks>
    public bool HasNavigator { get; set; } = true;

    public bool OpenInNavigator(string? connectionUrl)
    {
        OpenUrl(connectionUrl ?? "https://www.bahn.de");
        return HasNavigator;
    }

    public void RecreateMainView()
    {
    }

    public IReadOnlyList<PermissionInfo> GetPermissions()
    {
        return new List<PermissionInfo>();
    }

    public void OpenPermissionSettings(string key)
    {
    }

    public IUpdateInstaller? CreateUpdateInstaller()
    {
        return null;
    }

    public Action? InstallPermissionOpener { get; set; }

    public void OpenInstallPermissionSettings()
    {
        InstallPermissionOpener?.Invoke();
    }
}
