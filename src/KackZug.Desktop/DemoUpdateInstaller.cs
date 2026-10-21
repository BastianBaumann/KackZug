#if DEBUG
using System.Security.Cryptography;
using AppUpdater;

namespace KackZug.Desktop;

/// <summary>Installer fuer die Screenshot-Tour: verlangt zuerst die Erlaubnis und merkt sich danach die uebergebenen Dateien</summary>
internal sealed class DemoUpdateInstaller : IUpdateInstaller
{
    public int PermissionRequestsLeft { get; set; }

    public List<string> InstalledFiles { get; } = new List<string>();

    public string? LastFileSha256 { get; private set; }

    public bool CanInstall
    {
        get { return true; }
    }

    public Task<InstallStartResult> InstallAsync(string filePath, CancellationToken cancellationToken)
    {
        if (PermissionRequestsLeft > 0)
        {
            PermissionRequestsLeft--;
            return Task.FromResult(InstallStartResult.NeedsPermission());
        }

        InstalledFiles.Add(filePath);
        LastFileSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(filePath))).ToLowerInvariant();
        return Task.FromResult(InstallStartResult.Started());
    }
}
#endif
