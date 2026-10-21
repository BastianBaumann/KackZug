namespace AppUpdater;

/// <summary>Hands a downloaded, verified update file to the platform installer.</summary>
public interface IUpdateInstaller
{
    /// <summary>False on platforms where this installer cannot work.</summary>
    bool CanInstall { get; }

    /// <summary>Starts the installation. The final outcome is reported by the platform, not by this task.</summary>
    Task<InstallStartResult> InstallAsync(string filePath, CancellationToken cancellationToken);
}
