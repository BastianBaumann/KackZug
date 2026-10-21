namespace AppUpdater;

/// <summary>Why a check for updates failed.</summary>
public enum UpdateFailureReason
{
    Network,
    RateLimited,
    NoMatchingAsset,
    InvalidResponse
}
