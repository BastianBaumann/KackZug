namespace AppUpdater;

/// <summary>A published release that carries an installable asset for this app.</summary>
public sealed class UpdateRelease
{
    public required AppVersion Version { get; init; }

    public required string Tag { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public bool IsPrerelease { get; init; }

    public string PageUrl { get; init; } = string.Empty;

    public required string AssetName { get; init; }

    public required string AssetUrl { get; init; }

    /// <summary>Size of the asset in bytes, or 0 if the source does not know it.</summary>
    public long AssetSize { get; init; }

    /// <summary>Expected SHA-256 of the asset as lowercase hex, or null if the source has none.</summary>
    public string? Sha256 { get; init; }
}
