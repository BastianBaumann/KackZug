using AppUpdater;

namespace KackZug.Services;

/// <summary>Platzhalter fuer Plattformen, auf denen die App sich nicht selbst installieren kann</summary>
public sealed class NoUpdateInstaller : IUpdateInstaller
{
    public bool CanInstall
    {
        get { return false; }
    }

    public Task<InstallStartResult> InstallAsync(string filePath, CancellationToken cancellationToken)
    {
        return Task.FromResult(InstallStartResult.Failed("Installing updates is not supported on this platform."));
    }
}
