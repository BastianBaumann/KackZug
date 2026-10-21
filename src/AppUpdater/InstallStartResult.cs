namespace AppUpdater;

/// <summary>Result of handing a downloaded file to the platform installer.</summary>
public sealed class InstallStartResult
{
    private InstallStartResult(InstallStartStatus status, string? message)
    {
        Status = status;
        Message = message;
    }

    public InstallStartStatus Status { get; }

    /// <summary>Technical detail when <see cref="Status"/> is Failed.</summary>
    public string? Message { get; }

    public static InstallStartResult Started()
    {
        return new InstallStartResult(InstallStartStatus.Started, null);
    }

    public static InstallStartResult NeedsPermission()
    {
        return new InstallStartResult(InstallStartStatus.NeedsPermission, null);
    }

    public static InstallStartResult Failed(string message)
    {
        return new InstallStartResult(InstallStartStatus.Failed, message);
    }
}
