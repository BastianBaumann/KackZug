namespace AppUpdater;

/// <summary>Whether the platform installer could be started.</summary>
public enum InstallStartStatus
{
    Started,
    NeedsPermission,
    Failed
}
