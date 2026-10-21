namespace KackZug.Core.Updates;

/// <summary>Ergebnis einer Suche nach Updates: neue Version, aktuell oder nicht pruefbar</summary>
public sealed class UpdateCheckResult
{
    public UpdateCheckResult(UpdateCheckOutcome outcome, UpdateInfo? update, string? latestVersion)
    {
        Outcome = outcome;
        Update = update;
        LatestVersion = latestVersion;
    }

    public UpdateCheckOutcome Outcome { get; }

    public UpdateInfo? Update { get; }

    public string? LatestVersion { get; }
}
