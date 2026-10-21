using Android.App;
using Android.Content;
using Android.Content.PM;

namespace AppUpdater.Android;

/// <summary>
/// Receives the result of an install session. Starts the system confirmation screen when the user
/// must approve, and raises <see cref="StatusReceived"/> for every status.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class InstallStatusReceiver : BroadcastReceiver
{
    private const string ActionSuffix = ".appupdater.INSTALL_STATUS";

    /// <summary>Raised on the main thread. Subscribe early (for example in the Application or main Activity).</summary>
    public static event EventHandler<InstallStatusEventArgs>? StatusReceived;

    /// <summary>The app-unique broadcast action used for install results.</summary>
    public static string GetAction(Context context)
    {
        return context.PackageName + ActionSuffix;
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null || intent.Action != GetAction(context))
        {
            return;
        }

        var statusCode = intent.GetIntExtra(PackageInstaller.ExtraStatus, (int)PackageInstallStatus.Failure);
        var message = intent.GetStringExtra(PackageInstaller.ExtraStatusMessage);
        var status = MapStatus(statusCode);
        if (status == AndroidInstallStatus.WaitingForUser)
        {
            var confirm = GetConfirmIntent(intent);
            if (confirm is null)
            {
                status = AndroidInstallStatus.Failed;
                message = "Installer asked for confirmation without a confirmation intent";
            }
            else
            {
                confirm.AddFlags(ActivityFlags.NewTask);
                try
                {
                    context.StartActivity(confirm);
                }
                catch (ActivityNotFoundException exception)
                {
                    status = AndroidInstallStatus.Failed;
                    message = exception.Message;
                }
            }
        }

        StatusReceived?.Invoke(this, new InstallStatusEventArgs(status, statusCode, message));
    }

    private static AndroidInstallStatus MapStatus(int statusCode)
    {
        if (statusCode == (int)PackageInstallStatus.PendingUserAction)
        {
            return AndroidInstallStatus.WaitingForUser;
        }

        if (statusCode == (int)PackageInstallStatus.Success)
        {
            return AndroidInstallStatus.Succeeded;
        }

        if (statusCode == (int)PackageInstallStatus.FailureAborted)
        {
            return AndroidInstallStatus.Aborted;
        }

        return AndroidInstallStatus.Failed;
    }

    private static Intent? GetConfirmIntent(Intent intent)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            return intent.GetParcelableExtra(Intent.ExtraIntent, Java.Lang.Class.FromType(typeof(Intent))) as Intent;
        }

#pragma warning disable CA1422
        return intent.GetParcelableExtra(Intent.ExtraIntent) as Intent;
#pragma warning restore CA1422
    }
}
