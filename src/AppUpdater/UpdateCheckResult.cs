namespace AppUpdater;

/// <summary>Result of a check for updates: a newer release, up to date, or failed with a reason.</summary>
public sealed class UpdateCheckResult
{
    private UpdateCheckResult(UpdateCheckOutcome outcome, UpdateRelease? release, AppVersion? latestVersion, UpdateFailureReason? failureReason, string? failureMessage)
    {
        Outcome = outcome;
        Release = release;
        LatestVersion = latestVersion;
        FailureReason = failureReason;
        FailureMessage = failureMessage;
    }

    public UpdateCheckOutcome Outcome { get; }

    /// <summary>The newer release; only set when <see cref="Outcome"/> is UpdateAvailable.</summary>
    public UpdateRelease? Release { get; }

    /// <summary>The highest version the feed offers, or null if unknown.</summary>
    public AppVersion? LatestVersion { get; }

    public UpdateFailureReason? FailureReason { get; }

    /// <summary>Technical detail for logs; not meant to be shown to users.</summary>
    public string? FailureMessage { get; }

    public static UpdateCheckResult Available(UpdateRelease release)
    {
        return new UpdateCheckResult(UpdateCheckOutcome.UpdateAvailable, release, release.Version, null, null);
    }

    public static UpdateCheckResult UpToDate(AppVersion? latestVersion)
    {
        return new UpdateCheckResult(UpdateCheckOutcome.UpToDate, null, latestVersion, null, null);
    }

    public static UpdateCheckResult Failed(UpdateFailureReason reason, string? message)
    {
        return new UpdateCheckResult(UpdateCheckOutcome.Failed, null, null, reason, message);
    }
}
