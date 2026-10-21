namespace AppUpdater;

/// <summary>
/// Runs the whole update flow: check the source, download and verify the asset, hand it to the installer.
/// Holds the current <see cref="UpdateState"/> so an app can bind its own UI to it.
/// </summary>
public sealed class UpdateService
{
    private readonly IUpdateSource _source;
    private readonly UpdateDownloader _downloader;
    private readonly IUpdateInstaller _installer;
    private readonly string _downloadDirectory;
    private readonly object _gate = new object();
    private UpdateState _state = UpdateState.Idle;

    /// <summary>
    /// <paramref name="downloadDirectory"/> must be used only for update files: older files with the same extension are removed.
    /// </summary>
    public UpdateService(IUpdateSource source, UpdateDownloader downloader, IUpdateInstaller installer, string downloadDirectory)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(downloader);
        ArgumentNullException.ThrowIfNull(installer);
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadDirectory);
        _source = source;
        _downloader = downloader;
        _installer = installer;
        _downloadDirectory = downloadDirectory;
    }

    /// <summary>Raised after <see cref="State"/> changed, on the thread that changed it.</summary>
    public event EventHandler<UpdateState>? StateChanged;

    public UpdateState State
    {
        get
        {
            lock (_gate)
            {
                return _state;
            }
        }
    }

    /// <summary>The newer release found by the last successful check.</summary>
    public UpdateRelease? AvailableRelease { get; private set; }

    /// <summary>Path of the verified download, set in state ReadyToInstall and later.</summary>
    public string? DownloadedFilePath { get; private set; }

    /// <summary>Technical detail of the last failure, for logs.</summary>
    public string? LastError { get; private set; }

    public bool CanInstall
    {
        get { return _installer.CanInstall; }
    }

    /// <summary>True if no check happened yet or the last one is at least <paramref name="interval"/> ago.</summary>
    public static bool IsCheckDue(DateTimeOffset? lastCheck, DateTimeOffset now, TimeSpan interval)
    {
        if (lastCheck is null)
        {
            return true;
        }

        if (lastCheck.Value > now)
        {
            return true;
        }

        return now - lastCheck.Value >= interval;
    }

    public async Task<UpdateCheckResult> CheckAsync(AppVersion currentVersion, bool includePrereleases, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(currentVersion);
        var previous = EnterBusy(UpdateState.Checking);
        try
        {
            var latest = await _source.GetLatestAsync(includePrereleases, cancellationToken);
            if (latest is null || latest.Version <= currentVersion)
            {
                AvailableRelease = null;
                DownloadedFilePath = null;
                LastError = null;
                SetState(UpdateState.Idle);
                return UpdateCheckResult.UpToDate(latest?.Version);
            }

            if (AvailableRelease is null || AvailableRelease.Version != latest.Version || AvailableRelease.AssetName != latest.AssetName)
            {
                DownloadedFilePath = null;
            }

            AvailableRelease = latest;
            LastError = null;
            SetState(DownloadedFilePath is not null && File.Exists(DownloadedFilePath) ? UpdateState.ReadyToInstall : UpdateState.UpdateAvailable);
            return UpdateCheckResult.Available(latest);
        }
        catch (UpdateSourceException exception)
        {
            LastError = exception.Message;
            SetState(UpdateState.Failed);
            return UpdateCheckResult.Failed(exception.Reason, exception.Message);
        }
        catch (OperationCanceledException)
        {
            SetState(previous == UpdateState.Failed ? UpdateState.Idle : previous);
            throw;
        }
    }

    /// <summary>
    /// Downloads and verifies the release found by <see cref="CheckAsync"/>. Returns the file path.
    /// On failure the state becomes Failed and the exception is rethrown; on cancellation the state goes back to UpdateAvailable.
    /// </summary>
    public async Task<string> DownloadAsync(IProgress<UpdateProgress>? progress, CancellationToken cancellationToken)
    {
        var release = AvailableRelease ?? throw new InvalidOperationException("No update available; call CheckAsync first.");
        EnterBusy(UpdateState.Downloading);
        try
        {
            var path = await _downloader.DownloadAsync(release, _downloadDirectory, progress, cancellationToken);
            DownloadedFilePath = path;
            LastError = null;
            SetState(UpdateState.ReadyToInstall);
            return path;
        }
        catch (OperationCanceledException)
        {
            SetState(UpdateState.UpdateAvailable);
            throw;
        }
        catch (Exception exception)
        {
            LastError = exception.Message;
            SetState(UpdateState.Failed);
            throw;
        }
    }

    /// <summary>
    /// Hands the downloaded file to the installer. Started keeps the state Installing until
    /// <see cref="ReportInstallFinished"/> is called; NeedsPermission goes back to ReadyToInstall.
    /// </summary>
    public async Task<InstallStartResult> InstallAsync(CancellationToken cancellationToken)
    {
        var path = DownloadedFilePath ?? throw new InvalidOperationException("Nothing downloaded; call DownloadAsync first.");
        if (!_installer.CanInstall)
        {
            return InstallStartResult.Failed("Installing updates is not supported on this platform.");
        }

        EnterBusy(UpdateState.Installing);
        InstallStartResult result;
        try
        {
            result = await _installer.InstallAsync(path, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            SetState(UpdateState.ReadyToInstall);
            throw;
        }
        catch (Exception exception)
        {
            result = InstallStartResult.Failed(exception.Message);
        }

        if (result.Status == InstallStartStatus.NeedsPermission)
        {
            SetState(UpdateState.ReadyToInstall);
        }
        else if (result.Status == InstallStartStatus.Failed)
        {
            LastError = result.Message;
            SetState(UpdateState.Failed);
        }

        return result;
    }

    /// <summary>Called by the app when the platform reports the end of an installation it did not finish by restarting the app.</summary>
    public void ReportInstallFinished(bool succeeded, string? message)
    {
        if (succeeded)
        {
            AvailableRelease = null;
            DownloadedFilePath = null;
            LastError = null;
            SetState(UpdateState.Idle);
            return;
        }

        LastError = message;
        SetState(DownloadedFilePath is not null ? UpdateState.ReadyToInstall : UpdateState.Failed);
    }

    private UpdateState EnterBusy(UpdateState busyState)
    {
        UpdateState previous;
        lock (_gate)
        {
            if (_state == UpdateState.Checking || _state == UpdateState.Downloading)
            {
                throw new InvalidOperationException("Update service is busy: " + _state);
            }

            previous = _state;
            _state = busyState;
        }

        StateChanged?.Invoke(this, busyState);
        return previous;
    }

    private void SetState(UpdateState state)
    {
        lock (_gate)
        {
            if (_state == state)
            {
                return;
            }

            _state = state;
        }

        StateChanged?.Invoke(this, state);
    }
}
