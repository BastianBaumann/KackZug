namespace AppUpdater;

/// <summary>Where the <see cref="UpdateService"/> currently is.</summary>
public enum UpdateState
{
    Idle,
    Checking,
    UpdateAvailable,
    Downloading,
    ReadyToInstall,
    Installing,
    Failed
}
