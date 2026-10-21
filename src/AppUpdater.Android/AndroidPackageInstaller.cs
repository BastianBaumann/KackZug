using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Provider;

[assembly: UsesPermission("android.permission.REQUEST_INSTALL_PACKAGES")]

namespace AppUpdater.Android;

/// <summary>
/// Installs a downloaded APK as an update of the running app through the PackageInstaller session API.
/// The result arrives later through <see cref="InstallStatusReceiver.StatusReceived"/>.
/// </summary>
public sealed class AndroidPackageInstaller : IUpdateInstaller
{
    private readonly Context _context;

    public AndroidPackageInstaller(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context.ApplicationContext ?? context;
    }

    public bool CanInstall
    {
        get { return true; }
    }

    /// <summary>True if the user allowed this app to install packages ("Install unknown apps").</summary>
    public bool HasInstallPermission
    {
        get { return _context.PackageManager?.CanRequestPackageInstalls() ?? false; }
    }

    /// <summary>Opens the system screen where the user allows this app to install unknown apps.</summary>
    public void OpenInstallPermissionSettings()
    {
        var intent = new Intent(Settings.ActionManageUnknownAppSources, global::Android.Net.Uri.Parse("package:" + _context.PackageName));
        intent.AddFlags(ActivityFlags.NewTask);
        _context.StartActivity(intent);
    }

    public Task<InstallStartResult> InstallAsync(string filePath, CancellationToken cancellationToken)
    {
        return Task.Run(() => Install(filePath, cancellationToken), cancellationToken);
    }

    private InstallStartResult Install(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return InstallStartResult.Failed("Update file not found: " + filePath);
        }

        var packageManager = _context.PackageManager;
        var packageName = _context.PackageName;
        if (packageManager is null || packageName is null)
        {
            return InstallStartResult.Failed("Package manager not available");
        }

        var check = CheckArchive(packageManager, packageName, filePath);
        if (check is not null)
        {
            return InstallStartResult.Failed(check);
        }

        if (!packageManager.CanRequestPackageInstalls())
        {
            return InstallStartResult.NeedsPermission();
        }

        cancellationToken.ThrowIfCancellationRequested();
        var installer = packageManager.PackageInstaller;
        var sessionParams = new PackageInstaller.SessionParams(PackageInstallMode.FullInstall);
        sessionParams.SetAppPackageName(packageName);
        var length = new FileInfo(filePath).Length;
        sessionParams.SetSize(length);
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            sessionParams.SetRequireUserAction((int)PackageInstallUserAction.NotRequired);
        }

        var sessionId = installer.CreateSession(sessionParams);
        try
        {
            using var session = installer.OpenSession(sessionId);
            using (var output = session.OpenWrite("update.apk", 0, length))
            using (var input = File.OpenRead(filePath))
            {
                input.CopyTo(output);
                output.Flush();
                session.Fsync(output);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var intent = new Intent(_context, typeof(InstallStatusReceiver));
            intent.SetAction(InstallStatusReceiver.GetAction(_context));
            intent.SetPackage(packageName);
            var flags = PendingIntentFlags.UpdateCurrent;
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                flags |= PendingIntentFlags.Mutable;
            }

            var pendingIntent = PendingIntent.GetBroadcast(_context, sessionId, intent, flags);
            if (pendingIntent?.IntentSender is null)
            {
                session.Abandon();
                return InstallStartResult.Failed("Could not create the status callback");
            }

            session.Commit(pendingIntent.IntentSender);
            return InstallStartResult.Started();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            TryAbandon(installer, sessionId);
            return InstallStartResult.Failed(exception.Message);
        }
        catch (OperationCanceledException)
        {
            TryAbandon(installer, sessionId);
            throw;
        }
    }

    private static string? CheckArchive(PackageManager packageManager, string packageName, string filePath)
    {
        var archive = GetArchiveInfo(packageManager, filePath);
        if (archive is null)
        {
            return "Update file is not a valid APK";
        }

        if (archive.PackageName != packageName)
        {
            return "Update is for package " + archive.PackageName + ", not " + packageName;
        }

        var installed = GetInstalledInfo(packageManager, packageName);
        if (installed is null)
        {
            return "Installed package info not available";
        }

        var archiveCode = GetVersionCode(archive);
        var installedCode = GetVersionCode(installed);
        if (archiveCode <= installedCode)
        {
            return "Update versionCode " + archiveCode + " is not higher than installed " + installedCode;
        }

        return null;
    }

    private static PackageInfo? GetArchiveInfo(PackageManager packageManager, string filePath)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            return packageManager.GetPackageArchiveInfo(filePath, PackageManager.PackageInfoFlags.Of(0L));
        }

#pragma warning disable CA1422
        return packageManager.GetPackageArchiveInfo(filePath, 0);
#pragma warning restore CA1422
    }

    private static PackageInfo? GetInstalledInfo(PackageManager packageManager, string packageName)
    {
        try
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                return packageManager.GetPackageInfo(packageName, PackageManager.PackageInfoFlags.Of(0L));
            }

#pragma warning disable CA1422
            return packageManager.GetPackageInfo(packageName, 0);
#pragma warning restore CA1422
        }
        catch (PackageManager.NameNotFoundException)
        {
            return null;
        }
    }

    private static long GetVersionCode(PackageInfo info)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            return info.LongVersionCode;
        }

#pragma warning disable CA1422
        return info.VersionCode;
#pragma warning restore CA1422
    }

    private static void TryAbandon(PackageInstaller installer, int sessionId)
    {
        try
        {
            using var session = installer.OpenSession(sessionId);
            session.Abandon();
        }
        catch (Exception)
        {
        }
    }
}
