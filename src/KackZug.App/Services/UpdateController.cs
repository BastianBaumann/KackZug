using AppUpdater;
using CommunityToolkit.Mvvm.ComponentModel;
using KackZug.Core;
using KackZug.Core.Updates;
using KackZug.Localization;

namespace KackZug.Services;

/// <summary>Sucht auf GitHub nach neuen Versionen, laedt sie herunter und uebergibt sie dem Installer der Plattform</summary>
public sealed partial class UpdateController : ObservableObject
{
    public const string Owner = "BastianBaumann";
    public const string Repo = "KackZug";
    public const string AssetPattern = "KackZug-*.apk";
    public const int MaxNotesLines = 3;

    /// <summary>Einmal am Tag, auch wenn die App am naechsten Morgen etwas frueher geoeffnet wird</summary>
    public static readonly TimeSpan CheckInterval = TimeSpan.FromHours(20);

    private readonly AppServices _services;
    private UpdateService? _service;
    private CancellationTokenSource? _download;

    public UpdateController(AppServices services)
    {
        _services = services;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUpdate), nameof(BannerText), nameof(NotesText), nameof(HasNotes))]
    public partial UpdateRelease? Release { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUpdate))]
    public partial bool IsDismissed { get; private set; }

    [ObservableProperty]
    public partial bool IsChecking { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BannerText), nameof(HasNotes))]
    public partial bool IsDownloading { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BannerText))]
    public partial bool IsInstalling { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BannerText), nameof(ProgressValue), nameof(IsProgressUnknown))]
    public partial int? ProgressPercent { get; private set; }

    [ObservableProperty]
    public partial bool IsWaitingForPermission { get; private set; }

    public bool HasUpdate
    {
        get { return Release is not null && !IsDismissed; }
    }

    public bool CanInstall
    {
        get { return CurrentInstaller.CanInstall; }
    }

    public bool IsDownloaded
    {
        get { return _service?.DownloadedFilePath is string path && File.Exists(path); }
    }

    public string? DownloadedFilePath
    {
        get { return _service?.DownloadedFilePath; }
    }

    public string BannerText
    {
        get
        {
            if (Release is null)
            {
                return "";
            }

            if (IsDownloading)
            {
                if (ProgressPercent is null)
                {
                    return UiStrings.Update_Downloading;
                }

                return UiLanguage.Format(UiStrings.Update_DownloadingPercent, ProgressPercent.Value);
            }

            if (IsInstalling)
            {
                return UiLanguage.Format(UiStrings.Update_Installing, Release.Version);
            }

            return UiLanguage.Format(UiStrings.Update_Available, Release.Version);
        }
    }

    public string ActionText
    {
        get
        {
            if (CanInstall)
            {
                return UiStrings.Update_Now;
            }

            return UiStrings.Update_OpenPage;
        }
    }

    public string NotesText
    {
        get { return ReleaseNotesText.Shorten(Release?.Notes, MaxNotesLines); }
    }

    public bool HasNotes
    {
        get { return !IsDownloading && NotesText.Length > 0; }
    }

    public double ProgressValue
    {
        get { return ProgressPercent ?? 0; }
    }

    public bool IsProgressUnknown
    {
        get { return ProgressPercent is null; }
    }

    private IUpdateInstaller CurrentInstaller
    {
        get { return _services.UpdateInstaller ?? new NoUpdateInstaller(); }
    }

    public static AppVersion CurrentVersion
    {
        get
        {
            if (AppVersion.TryParse(AppInfo.FullVersion, out var version))
            {
                return version;
            }

            return AppVersion.Parse(AppInfo.Version);
        }
    }

    /// <summary>Automatische Suche beim Start, hoechstens einmal am Tag und nur, wenn sie eingeschaltet ist</summary>
    public async Task CheckIfDueAsync()
    {
        var data = _services.Data;
        if (!data.Settings.UpdateCheckEnabled || !UpdateService.IsCheckDue(data.LastUpdateCheck, _services.Clock.Now, CheckInterval))
        {
            return;
        }

        await CheckAsync();
    }

    /// <summary>Gibt null zurueck, wenn gerade schon gesucht oder geladen wird</summary>
    public async Task<UpdateCheckResult?> CheckAsync()
    {
        if (IsChecking || IsDownloading)
        {
            return null;
        }

        IsChecking = true;
        try
        {
            var service = CreateService();
            var data = _services.Data;
            var result = await service.CheckAsync(CurrentVersion, data.Settings.IncludePrereleases, CancellationToken.None);
            data.LastUpdateCheck = _services.Clock.Now;
            _services.Store.Save(data);
            if (result.Outcome == UpdateCheckOutcome.UpdateAvailable)
            {
                IsDismissed = false;
                if (Release is null || Release.Version != result.Release!.Version)
                {
                    IsInstalling = false;
                    IsWaitingForPermission = false;
                }

                _service = service;
                Release = result.Release;
                OnPropertyChanged(nameof(ActionText));
            }
            else if (result.Outcome == UpdateCheckOutcome.UpToDate)
            {
                _service = service;
                Release = null;
                IsInstalling = false;
                IsWaitingForPermission = false;
            }

            return result;
        }
        finally
        {
            IsChecking = false;
        }
    }

    /// <summary>Laedt die gefundene Version und prueft sie; wirft bei Netz-, Speicher- oder Pruefsummenfehlern</summary>
    public async Task<string> DownloadAsync()
    {
        var service = _service ?? throw new InvalidOperationException("No update found yet.");
        var download = new CancellationTokenSource();
        _download = download;
        ProgressPercent = 0;
        IsDownloading = true;
        try
        {
            var progress = new Progress<UpdateProgress>(ReportProgress);
            return await service.DownloadAsync(progress, download.Token);
        }
        catch (OperationCanceledException exception) when (!download.IsCancellationRequested)
        {
            throw new IOException("Download timed out", exception);
        }
        finally
        {
            IsDownloading = false;
            _download = null;
            download.Dispose();
        }
    }

    public async Task<InstallStartResult> InstallAsync()
    {
        var service = _service ?? throw new InvalidOperationException("No update found yet.");
        IsWaitingForPermission = false;
        var result = await service.InstallAsync(CancellationToken.None);
        IsInstalling = result.Status == InstallStartStatus.Started;
        return result;
    }

    /// <summary>Merkt sich, dass nach der Rueckkehr aus den Einstellungen weiter installiert werden soll</summary>
    public void OpenInstallPermissionSettings()
    {
        IsWaitingForPermission = true;
        _services.Platform.OpenInstallPermissionSettings();
    }

    /// <summary>True, wenn eine Installation auf die Erlaubnis gewartet hat; setzt das Warten zurueck</summary>
    public bool TakeWaitingForPermission()
    {
        var waiting = IsWaitingForPermission && _service is not null;
        IsWaitingForPermission = false;
        return waiting;
    }

    public void ReportInstallFinished(bool succeeded, string? message)
    {
        _service?.ReportInstallFinished(succeeded, message);
        IsInstalling = false;
        if (succeeded)
        {
            Release = null;
        }
    }

    /// <summary>Blendet den Hinweis aus und bricht einen laufenden Download ab</summary>
    public void Dismiss()
    {
        _download?.Cancel();
        IsDismissed = true;
    }

    private void ReportProgress(UpdateProgress progress)
    {
        if (!IsDownloading)
        {
            return;
        }

        var fraction = progress.Fraction;
        if (fraction is null)
        {
            ProgressPercent = null;
            return;
        }

        ProgressPercent = (int)Math.Floor(fraction.Value * 100);
    }

    /// <summary>Loescht beim Start heruntergeladene Updates, die schon installiert oder aelter sind</summary>
    public void RemoveOutdatedDownloads()
    {
        if (!AppVersion.TryParse(AppInfo.FullVersion, out var current))
        {
            return;
        }

        try
        {
            UpdateDownloader.RemoveOutdated(DownloadDirectory, current);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private string DownloadDirectory
    {
        get { return Path.Combine(_services.Platform.DataDirectory, "updates"); }
    }

    private UpdateService CreateService()
    {
        var http = _services.CreateUpdateHttpClient();
        var userAgent = "KackZug/" + AppInfo.Version;
        var source = new GitHubReleaseSource(http, Owner, Repo, AssetPattern, userAgent);
        var downloader = new UpdateDownloader(http, userAgent);
        return new UpdateService(source, downloader, CurrentInstaller, DownloadDirectory);
    }
}
