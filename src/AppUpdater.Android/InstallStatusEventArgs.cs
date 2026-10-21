namespace AppUpdater.Android;

/// <summary>Status of an install session as reported by the Android package installer.</summary>
public sealed class InstallStatusEventArgs : EventArgs
{
    public InstallStatusEventArgs(AndroidInstallStatus status, int statusCode, string? message)
    {
        Status = status;
        StatusCode = statusCode;
        Message = message;
    }

    public AndroidInstallStatus Status { get; }

    /// <summary>The raw PackageInstaller.STATUS_* value.</summary>
    public int StatusCode { get; }

    /// <summary>The installer's EXTRA_STATUS_MESSAGE, technical and not localized.</summary>
    public string? Message { get; }
}
