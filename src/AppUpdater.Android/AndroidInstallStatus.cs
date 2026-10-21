namespace AppUpdater.Android;

/// <summary>What the Android package installer reported for an install session.</summary>
public enum AndroidInstallStatus
{
    WaitingForUser,
    Succeeded,
    Failed,
    Aborted
}
